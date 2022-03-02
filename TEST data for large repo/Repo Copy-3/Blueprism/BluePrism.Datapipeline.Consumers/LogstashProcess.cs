using BluePrism.BPCoreLib;
using BluePrism.Common.Security;
using BluePrism.Core.Utility;
using BluePrism.Datapipeline.Logstash.Wrappers;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using NLog;

namespace BluePrism.Datapipeline.Logstash
{
    /// <summary>
    /// Launches a Logstash process and monitors it by parsing the text from the standard output stream.
    /// </summary>
    public class LogstashProcess : ILogstashProcess, IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly IProcessFactory _processFactory;
        private readonly IJavaProcessHelper _javaProcessHelper;
        private readonly IEventLogger _eventLogger;
        
        private IProcess _logstashCmdProcess;
        private IProcess _logstashJvmProcess;

        private bool _stopping;

        // Every line from the Logstash process' standard output stream gets written into _stdoutProcessingBuffer,
        // where they are then parsed on the _stdoutProcessingThread for relevant messages.
        private Thread _stdoutProcessingThread;
        private bool _stopStdoutProcessingThread = false;
        private ConcurrentQueue<string> _stdoutProcessingBuffer = new ConcurrentQueue<string>();

        public event EventHandler<(LogstashProcessState state, string stateMessage)> StateChanged;

        // This is the state we have determined the Logstash process to be in. This is determined by parsing the standard output stream for
        // relevant messages.
        private (LogstashProcessState state, string stateMessage) _state = (LogstashProcessState.Stopped, "");
        public (LogstashProcessState state, string stateMessage) State
        {
            get => _state;
            set
            {
                if (!(_state.state == value.state))
                {
                    _state = value;
                    StateChanged?.Invoke(this, _state);
                }
            }
        }

        public LogstashProcess(IProcessFactory processFactory, IJavaProcessHelper javaProcessHelper, IEventLogger eventLogger)
        {
            _processFactory = processFactory;
            _javaProcessHelper = javaProcessHelper;
            _eventLogger = eventLogger;
            _stdoutProcessingThread = new Thread(ProcessStdoutputText);
        }

        private ProcessStartInfo ConfigureLogstashProcess(LogstashProcessSettings logstashConfiguration, SafeString secretStorePassword)
        {
            var processConfiguration = new ProcessStartInfo
            {
                FileName = $"\"{logstashConfiguration.LogstashPath}\"",
                Arguments = $" -f \"{logstashConfiguration.ConfigurationPath}\"",
                WorkingDirectory = logstashConfiguration.LogstashDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                LoadUserProfile = false
            };

            if (logstashConfiguration.TraceLogging) processConfiguration.Arguments += " --log.level=trace ";

            if (!string.IsNullOrEmpty(logstashConfiguration.UserName))
            {
                ProcessSecurity.GrantAccessToWindowStationAndDesktop(logstashConfiguration.UserName);
                processConfiguration.Domain = logstashConfiguration.Domain;
                processConfiguration.UserName = logstashConfiguration.UserName;
                processConfiguration.Password = logstashConfiguration.Password;
            }
           
            // The password for the Logstash key store needs to be passed to Logstash by setting the LOGSTASH_KEYSTORE_PASS environment
            //variable.
            processConfiguration.EnvironmentVariables.Add("LOGSTASH_KEYSTORE_PASS", secretStorePassword.SecureString.MakeInsecure());

            return processConfiguration;
        }

        public void Start(LogstashProcessSettings processConfiguration, SafeString secretStorePassword)
        {
            _stopping = false;
            State = (LogstashProcessState.Starting, Properties.Resources.StartingProcess);
            if (_logstashCmdProcess != null) _eventLogger.Warn(Properties.Resources.ProcessAlreadyRunning, Log);

            // There shouldn't be a Logstash process running at this point. If there is, kill it.
            var logstashProcessId = GetLogstashJVMProcessId();
            if (logstashProcessId != -1) _processFactory.GetProcessById(logstashProcessId).Kill();

            var process = _processFactory.CreateProcess();
            process.StartInfo = ConfigureLogstashProcess(processConfiguration, secretStorePassword);
            process.OutputDataReceived += Process_OutputDataReceived;
            process.Exited += Process_Exited;

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            _logstashCmdProcess = process;
            _stdoutProcessingThread.Start();
        }

        public void Stop()
        {
            Stop(false);
        }


        private void Stop(bool suppressStateChange)
        {
            _stopping = true;
            _stopStdoutProcessingThread = true;

            if (_logstashCmdProcess != null && !_logstashCmdProcess.HasExited)
            {
                _logstashCmdProcess.Terminate();
            }

            var pid = GetLogstashJVMProcessId();
            if (pid != -1)
            {
                var logstashJavaProcess = _processFactory.GetProcessById(pid);
                if (!logstashJavaProcess.WaitForExit(TimeSpan.FromSeconds(10)))
                {
                    logstashJavaProcess.Kill();
                }
            }

            if (_logstashCmdProcess != null && !_logstashCmdProcess.HasExited)
            {
                _logstashCmdProcess.Kill();
            }


            _logstashCmdProcess = null;

            if (!suppressStateChange)
            {
                State = (LogstashProcessState.Stopped, "");
            }
        }

        private void Process_OutputDataReceived(object sender, StandardOutputEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Text))
                return;

            _stdoutProcessingBuffer.Enqueue(e.Text);
        }


        private void ProcessStdoutputText()
        {
           
            // This method loops continuously, pulling text from the queue which contains lines from the standard output stream of the logstash process, and parsing them for information about the state of the process.
            // This is a bit messy - the error logs which Logstash writes to the standard output stream are formatted differently depending on which part of the system that came from, so extracting useful messages from them is a bit hit and miss.
            while (!_stopStdoutProcessingThread)
            {
                try
                {
                    bool nextLineContainsErrorMessage = false;

                    while (_stdoutProcessingBuffer.Count > 0 && !_stopStdoutProcessingThread)
                    {
                        if (_stdoutProcessingBuffer.TryDequeue(out string text))
                        {
                            if (nextLineContainsErrorMessage)
                            {
                                OnError();
                                State = (LogstashProcessState.Error, text);
                                nextLineContainsErrorMessage = false;
                                continue;
                            }

                            // debugging logging.
                            Console.WriteLine(text);
                            // check if the Logstash process has started up ok - (config loaded and the pipeline has started processing events).
                            if (text.Contains("Successfully started Logstash API endpoint"))
                            {
                                // Now that the actual Logstash JRE process has been launched, we want to monitor if that closes unexpectedly.
                                var pid = GetLogstashJVMProcessId();
                                _logstashJvmProcess = _processFactory.GetProcessById(pid);
                                _logstashJvmProcess.Exited += Process_Exited;
                                State = (LogstashProcessState.Running, "");
                                continue;
                            }

                            // Check if the Logstash process has encountered an error processing the configuration file.
                            if (text.Contains("LogStash::ConfigurationError"))
                            {
                                // there is a configuration error.
                                // extract the error message from the line of text, this should give the user a better idea of where in the
                                // config the problem is.
                                var messageMatch = Regex.Match(text, ":message=>\"(?<message>[\\s\\S]+?)\",");
                                string message = "";
                                if (messageMatch.Success)
                                {
                                    message = messageMatch.Groups["message"].Value;
                                }

                                OnError();
                                State = (LogstashProcessState.UnrecoverableError, $"{Properties.Resources.ConfigurationInvalid}: {message}");
                                continue;
                            }

                            // The JDBC output plugin splits its error messages over 2 lines, so we need to account for this and use the next line
                            // as the error message.
                            if (text.Contains("[com.zaxxer.hikari.pool.HikariPool] HikariPool-1 - Exception during pool initialization."))
                            {
                                nextLineContainsErrorMessage = true;
                                continue;
                            }

                            // All error messages written to stdout has either an [ERROR] or [FATAL] tag, so we can check for these to see if the process has errorred.
                            if ((text.Contains("[ERROR]") || text.Contains("[FATAL]")) && !text.Contains("The TOP or OFFSET clause is not allowed when the FROM clause contains a nested INSERT, UPDATE, DELETE, or MERGE statement."))
                            {
                                // "[2019-12-12T09:35:10,861][ERROR][logstash.inputs.jdbc     ] Java::ComMicrosoftSqlserverJdbc::SQLServerException: The TOP or OFFSET clause is not allowed when the FROM clause contains a nested INSERT, UPDATE, DELETE, or MERGE statement.: SELECT TOP (1) count(*) AS [COUNT] FROM (delete top(3000)from BPADataPipelineInput with (rowlock, readpast) output deleted.eventdata) AS [T1]"'
                                // Ignore the above error


                                // try to extract an error message from the standard output text.
                                // they come in a couple of formats depending on the origin of the message.
                                var messageMatch = Regex.Match(text, ":error=>\"(?<message>[\\s\\S]+?)\",");
                                string message = "";
                                if (messageMatch.Success)
                                {
                                    message = messageMatch.Groups["message"].Value;
                                }

                                messageMatch = Regex.Match(text, ":error=>#<(?<message>[\\s\\S]+?)>,");
                                if (messageMatch.Success)
                                {
                                    message = messageMatch.Groups["message"].Value;
                                }

                                // if unable to extract any error message from the line of text, then just use the whole text line as the error message.
                                if (string.IsNullOrEmpty(message))
                                {
                                    message = text;
                                }

                                OnError();
                                State = (LogstashProcessState.Error, message);
                                continue;
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    _eventLogger.Error(e.Message, Log);
                }


                Thread.Sleep(100);
            }
        }

        private void OnError()
        {
            // stops processing the standard output stream and stop the Logstash process
            // but suppress the state change - we want to remain in the Error state rather than changing to the Stopped state.
            _stopStdoutProcessingThread = true;
            Stop(true);
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            HandleProcessExited();
        }

        private void HandleProcessExited()
        {
            // if the process exiting was expected, then that's fine.
            if (_stopping)
            {
                return;
            }

            // process any existing text lines from stdout. they may contain an error message which
            // puts us into an error state.
            while (!_stdoutProcessingBuffer.IsEmpty)
            {
                Thread.Sleep(100);
            }

            // After all the stdout lines are processed, if we are not in an error state then add our own error to the stdout text list.
            // this will be processed and put us into an error state with a relevant error message that the process exited unexpectedly.
            if (State.state != LogstashProcessState.Error && State.state != LogstashProcessState.UnrecoverableError)
            {
                _stdoutProcessingBuffer.Enqueue("[ERROR] The process has exited unexpectedly.");
            }
        }

        private int GetLogstashJVMProcessId()
        {
            try
            {
                return _javaProcessHelper.GetProcessIdWthStartupParamsContaining("logstash");
            }
            catch (Exception e)
            {
                _eventLogger.Error(string.Format(Properties.Resources.UnableToGetLogstashProcessId, e.Message), Log);
                return -1;
            }
        }





        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                _stopStdoutProcessingThread = true;
                disposedValue = true;
            }
        }


        ~LogstashProcess()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }



}
