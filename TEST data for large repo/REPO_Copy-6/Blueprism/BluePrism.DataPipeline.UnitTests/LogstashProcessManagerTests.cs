using BluePrism.AutomateAppCore;
using BluePrism.BPCoreLib;
using BluePrism.Common.Security;
using BluePrism.Core.Utility;
using BluePrism.Datapipeline.Logstash;
using BluePrism.Datapipeline.Logstash.Wrappers;
using Moq;
using NUnit.Framework;
using System;
using System.Timers;

namespace BluePrism.DataPipeline.UnitTests
{
    [TestFixture]
    public class LogstashProcessManagerTests
    {
        private int _dataGatewayProcessId = 1;
        private Mock<IServerDataPipeline> _appServerMock;
        private Mock<ILogstashProcess> _logstashProcessMock;
        private Mock<ITimer> _timerMock;

        private LogstashProcessManager _classUnderTest;

        private string _config = "input { stdin{} } output { stdout{} }";
        [SetUp]
        public void Setup()
        {

            _appServerMock = new Mock<IServerDataPipeline>();
            _appServerMock.Setup(x => x.RegisterDataPipelineProcess(It.IsAny<string>(), It.IsAny<string>())).Returns(_dataGatewayProcessId);
            _appServerMock.Setup(x => x.GetConfigForDataPipelineProcess(_dataGatewayProcessId)).Returns(_config);


            var eventLoggerMock = new Mock<IEventLogger>();
            var tcpCommandListenerMock = new Mock<ITCPCommandListener>();
            var logstashProcessFactoryMock = new Mock<ILogstashProcessFactory>();
            _logstashProcessMock = new Mock<ILogstashProcess>();

            logstashProcessFactoryMock.Setup(x => x.CreateProcess()).Returns(_logstashProcessMock.Object);

            _timerMock = new Mock<ITimer>();
            Func<ITimer> timerFactory = () => _timerMock.Object;


            var configService = new Mock<ILogstashConfigurationService>();

			_classUnderTest = new LogstashProcessManager(_appServerMock.Object, "appserver", eventLoggerMock.Object,
				 tcpCommandListenerMock.Object, logstashProcessFactoryMock.Object, configService.Object, timerFactory, 5, new LogstashProcessSettings());
        }

        /// <summary>
        /// Changes to the Logstash process state should prompt a status update to be sent to the app server.
        /// </summary>
        [TestCase(LogstashProcessState.Error, DataGatewayProcessState.Error)]
        [TestCase(LogstashProcessState.Running, DataGatewayProcessState.Running)]
        [TestCase(LogstashProcessState.Stalled, DataGatewayProcessState.Error)]
        [TestCase(LogstashProcessState.UnrecoverableError, DataGatewayProcessState.UnrecoverableError)]
        [TestCase(LogstashProcessState.Stopped, DataGatewayProcessState.Online)]
        [TestCase(LogstashProcessState.Starting, DataGatewayProcessState.Starting)]
        public void LogstashProcessStateChanged_ErrorState_ErrorStateSentToAppServer(LogstashProcessState processState, DataGatewayProcessState stateSentToAppServer)
        {
            _classUnderTest.StartProcess();
            _appServerMock.Invocations.Clear();
            _logstashProcessMock.Raise(x => x.StateChanged += (sender, state) => { }, null, (processState, ""));
            _appServerMock.Verify(x => x.UpdateDataPipelineProcessStatus(_dataGatewayProcessId, stateSentToAppServer, It.IsAny<string>()));
        }

        /// <summary>
        /// Check that if the Logstash process goes into an error state, it is automatically restarted.
        /// </summary>
        [Test]
        public void LogstashProcessState_ChangedToErrorState_RestartProcess()
        {
            _timerMock.Setup(x => x.Start()).Raises(x => x.Elapsed += null, new EventArgs() as ElapsedEventArgs);
            _classUnderTest.StartProcess();
            _logstashProcessMock.Invocations.Clear();
            _logstashProcessMock.Raise(x => x.StateChanged += (sender, state) => { }, null, (LogstashProcessState.Error, ""));
            _logstashProcessMock.Verify(x => x.Start(It.IsAny<LogstashProcessSettings>(), It.IsAny<SafeString>()));
        }

        /// <summary>
        /// Checks that if the Logstash process goes into the Unrecoverable Error state, it is not restarted.
        /// </summary>
        [Test]
        public void LogstashProcessState_ChangedToUnrecoverableErrorState_DoesNotRestartProcess()
        {
            _timerMock.Setup(x => x.Start()).Raises(x => x.Elapsed += null, new EventArgs() as ElapsedEventArgs);
            _classUnderTest.StartProcess();
            _logstashProcessMock.Invocations.Clear();
            _logstashProcessMock.Raise(x => x.StateChanged += (sender, state) => { }, null, (LogstashProcessState.UnrecoverableError, ""));
            _logstashProcessMock.Verify(x => x.Start(It.IsAny<LogstashProcessSettings>(), It.IsAny<SafeString>()), Times.Never);
        }
    }
}
