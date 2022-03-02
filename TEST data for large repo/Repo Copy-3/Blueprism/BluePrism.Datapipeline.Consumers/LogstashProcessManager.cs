using BluePrism.AutomateAppCore;
using BluePrism.BPCoreLib;
using BluePrism.Common.Security;
using BluePrism.DataPipeline;
using BluePrism.Server.Domain.Models;
using System;
using System.Timers;
using NLog;
using BluePrism.Core.Utility;

namespace BluePrism.Datapipeline.Logstash
{
    /// <summary>
    /// Manages a Logstash process, which will process items from the data pipeline.
    /// </summary>
    public class LogstashProcessManager : IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private string _hostname;

        private int _logStashId;

        private IServerDataPipeline _appServer;

        private readonly IEventLogger _eventLogger;

        private ITCPCommandListener _tcpCommandListener;

        private ILogstashProcessFactory _logstashProcessFactory;

        private Func<ITimer> _timerFactory;

        private ILogstashProcess _process;

        private ILogstashConfigurationService _configurationService;

        private ITimer _autoRestartTimer;
        private bool _isAutoRestartTimerActivated = false;

        private object _syncLock = new object();
         
        private LogstashProcessSettings _processConfiguration;

        public LogstashProcessManager(
            IServerDataPipeline appServer,
            string hostname,
            IEventLogger eventLogger, 
            ITCPCommandListener tcpCommandListener, 
            ILogstashProcessFactory logstashProcessFactory, 
            ILogstashConfigurationService configurationService,
            Func<ITimer> timerFactory,
            int monitorFrequency,
            LogstashProcessSettings processConfiguration)
        {
            _appServer = appServer;
            _hostname = hostname;
            _tcpCommandListener = tcpCommandListener;
            _timerFactory = timerFactory;
            _eventLogger = eventLogger;
            _logstashProcessFactory = logstashProcessFactory;
            _configurationService = configurationService;
            _processConfiguration = processConfiguration;
            _tcpCommandListener.CommandReceived += CommandReceived;
            _tcpCommandListener.Start();

            try
            {
                _logStashId = _appServer.RegisterDataPipelineProcess(_hostname, $"{_hostname}:{tcpCommandListener.ListenPort}");
                _eventLogger.Info($"assigned id: {_logStashId}", Log);
                _appServer.UpdateDataPipelineProcessStatus(_logStashId, DataGatewayProcessState.Online, "");

            }
            catch(Exception ex)
            {
                _tcpCommandListener.Stop();
                _eventLogger.Error(string.Format("Error registering data gateways process: {0}", ex.Message), Log);
                throw;
            }

            _autoRestartTimer = _timerFactory();
            _autoRestartTimer.AutoReset = false;
            _autoRestartTimer.Elapsed += _restartTimer_Elapsed;
            _autoRestartTimer.Interval = 30000;
        }

        private void _restartTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            StartProcess();
        }

        public void StartProcess()
        {
            lock (_syncLock)
            {
                try
                {
                    if(_isAutoRestartTimerActivated)
                    {
                        _autoRestartTimer.Stop();
                        _isAutoRestartTimerActivated = false;
                    }

                    _eventLogger.Info(Properties.Resources.StartingLogstashProcess, Log);

                    (string config, SafeString secretStorePassword) = _configurationService.GetConfiguration(_logStashId);

                    if(_process != null)
                    {
                        _process.StateChanged -= ProcessStateChanged;
                    }

                    _process = _logstashProcessFactory.CreateProcess();
                    _configurationService.SaveConfigToFile(config);
                    _process.StateChanged += ProcessStateChanged;

                    _process.Start(_processConfiguration, secretStorePassword);
                }
                catch (Exception e)
                {
                    HandleError(string.Format(Properties.Resources.ErrorStartingProcess, e.Message));

                }
            }
        }


        private void ProcessStateChanged(object sender, (LogstashProcessState state, string stateMessage) state)
        {

            switch (state.state)
            {
                case LogstashProcessState.Running:
                    _appServer.UpdateDataPipelineProcessStatus(_logStashId, DataGatewayProcessState.Running, state.stateMessage);
                    _configurationService.DeleteConfigurationFileOnDisk();
                    break;

                case LogstashProcessState.Error:
                    HandleError(state.stateMessage);
                    break;

                case LogstashProcessState.Stalled:
                    _appServer.UpdateDataPipelineProcessStatus(_logStashId, DataGatewayProcessState.Error, state.stateMessage);
                    break;

                case LogstashProcessState.Starting:
                    _appServer.UpdateDataPipelineProcessStatus(_logStashId, DataGatewayProcessState.Starting, state.stateMessage);
                    _eventLogger.Info(Properties.Resources.ProcessStarted, Log);
                    break;

                case LogstashProcessState.Stopped:
                    _appServer.UpdateDataPipelineProcessStatus(_logStashId, DataGatewayProcessState.Online, state.stateMessage);
                    _eventLogger.Info(Properties.Resources.ProcessStopped, Log);
                    break;

                case LogstashProcessState.UnrecoverableError:
                    HandleError(state.stateMessage, false);
                    break;
            }
        }


        public void StopProcess()
        {
            lock (_syncLock)
            {
                try
                {

                    if (_isAutoRestartTimerActivated)
                    {
                        _autoRestartTimer.Stop();
                        _isAutoRestartTimerActivated = false;
                    }

                    if (_process == null)
                        return;

                    _eventLogger.Info(Properties.Resources.StoppingLogstashProcess, Log);
                    _process.Stop();
                }
                catch (Exception e)
                {
                    _eventLogger.Error(e.Message, Log);
                }
            }
        }


        private void CommandReceived(object sender, CommandEventArgs e)
        {
            try
            {
                DoCommand(e.Command);

            }catch(Exception ex)
            {
                _eventLogger.Error(string.Format(Properties.Resources.ErrorPerformingCommand, e.Command.ToString(), ex.Message), Log);
            }
        }

        private void DoCommand(DataPipelineProcessCommand command)
        {
            switch (command)
            {
                case DataPipelineProcessCommand.StartProcess:
                    _eventLogger.Info("Recevied Start Process Command", Log);
                    StartProcess();
                    break;

                case DataPipelineProcessCommand.StopProcess:
                    _eventLogger.Info("Recevied Stop Process Command", Log);
                    StopProcess();
                    break;

                default:
                    throw new InvalidArgumentException(string.Format(Properties.Resources.UnexpectedCommand, command.ToString()));
            }
        }

        private void HandleError(string error, bool recoverable = true)
        {
            _eventLogger.Error(error, Log);
            _appServer.UpdateDataPipelineProcessStatus(_logStashId, recoverable ? DataGatewayProcessState.Error : DataGatewayProcessState.UnrecoverableError, error);
            
            if (recoverable)
            {
                _autoRestartTimer.Start();
                _isAutoRestartTimerActivated = true;
                _eventLogger.Error($"Failed to start data gateways process - '{error}' recovering...", Log);
            }
            else
            {
                _eventLogger.Error($"Failed to start data gateways process - '{error}' unable to recover.", Log);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tcpCommandListener.Stop();
                StopProcess();
                _appServer.UpdateDataPipelineProcessStatus(_logStashId,
                    DataGatewayProcessState.Offline, "");
            }
        }

    }
}
