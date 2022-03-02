using BluePrism.BPCoreLib;
using BluePrism.Datapipeline.Logstash;
using BluePrism.Datapipeline.Logstash.Wrappers;
using Moq;
using NUnit.Framework;
using BluePrism.Common.Security;

namespace BluePrism.DataPipeline.UnitTests
{
    [TestFixture]
    public class LogstashProcessTests
    {

        private Mock<IProcess> _mockProcess;
        private Mock<IProcessFactory> _mockProcessFactory;
        private Mock<IJavaProcessHelper> _mockJavaProcessHelper;
        private Mock<IEventLogger> _mockEventLogger;

        private LogstashProcess _classUnderTest;

        [SetUp]
        public void Setup()
        {

            _mockProcess = new Mock<IProcess>();
            _mockProcess.Setup(x => x.StartInfo).Returns(new System.Diagnostics.ProcessStartInfo());

            _mockProcessFactory = new Mock<IProcessFactory>();
            _mockProcessFactory.Setup(x => x.CreateProcess()).Returns(_mockProcess.Object);
            _mockProcessFactory.Setup(x => x.GetProcessById(It.IsAny<int>())).Returns(new Mock<IProcess>().Object);

            _mockJavaProcessHelper = new Mock<IJavaProcessHelper>();
            _mockJavaProcessHelper.Setup(x => x.GetProcessIdWthStartupParamsContaining(It.IsAny<string>())).Returns(1);

            _mockEventLogger = new Mock<IEventLogger>();

            //"path/to/logstash.bat", "path/to/logstash/dir"

            _classUnderTest = new LogstashProcess(_mockProcessFactory.Object, _mockJavaProcessHelper.Object, _mockEventLogger.Object);

        }

		[TestCase(new string[] { "[2019-04-05T17:52:35,144] [ERROR] [logstash.pipeline] Error registering plugin {:pipeline_id=>\"main\", :plugin=>\"#<LogStash::OutputDelegator:0x7e7d43>\", :error=>\"JDBC - Can not target a Blue Prism database with this JDBC output plugin.\", :thread=>\"#<Thread:0x19df7d7 run>\"}"}, LogstashProcessState.Error, "JDBC - Can not target a Blue Prism database with this JDBC output plugin.")]
		[TestCase(new string[] { "[2019-04-05T17:57:15,360][FATAL][logstash.runner          ] An unexpected error occurred! {:error=>#<Errno::ENOENT: No such file or directory - G:/sessionlogs.txt>, :backtrace=>[\"org/jruby/RubyFile.java:366:in `initialize'\", \"org/jruby/RubyIO.java:875:in `new'\", \"D:/Logstash/logstash/vendor/bundle/jruby/2.3.0/gems/logstash-output-file-4.2.5/lib/logstash/outputs/file.rb:285:in `open'", "D:/Logstash/logstash/vendor/bundle/jruby/2.3.0/gems/logstash-output-file-4.2.5/lib/logstash" }, LogstashProcessState.Error, "Errno::ENOENT: No such file or directory - G:/sessionlogs.txt")]	
		[TestCase(new string[] { "[2019-04-08T11:13:49,327][ERROR][com.zaxxer.hikari.pool.HikariPool] HikariPool-1 - Exception during pool initialization.", "com.microsoft.sqlserver.jdbc.SQLServerException: The TCP/IP connection to the host dgfdg, port 1433 has failed. Error: \"dgfdg.Verify the connection properties.Make sure that an instance of SQL Server is running on the host and accepting TCP / IP connections at the port.Make sure that TCP connections to the port are not blocked by a firewall.\"" }, LogstashProcessState.Error, "com.microsoft.sqlserver.jdbc.SQLServerException: The TCP/IP connection to the host dgfdg, port 1433 has failed. Error: \"dgfdg.Verify the connection properties.Make sure that an instance of SQL Server is running on the host and accepting TCP / IP connections at the port.Make sure that TCP connections to the port are not blocked by a firewall.\"")]
		[TestCase(new string[] { "[2019-04-08T11:13:49,548][INFO ][logstash.agent           ] Successfully started Logstash API endpoint {:port=>9600}" }, LogstashProcessState.Running, "")]
        public void TestParseLogstashOutput(string[] stdoutputLines, LogstashProcessState expectedState, string expectedStateMessage)
        {

            _classUnderTest.Start(new LogstashProcessSettings(), new SafeString("password"));

            foreach (var line in stdoutputLines)
            {
                _mockProcess.Raise(x => x.OutputDataReceived += null, new StandardOutputEventArgs(line));
            }

            Assert.That(() => _classUnderTest.State.state, Is.EqualTo(expectedState).After(1000, 50));
            Assert.That(() => _classUnderTest.State.stateMessage, Is.EqualTo(expectedStateMessage).After(1000, 50));
        }
        
        /// <summary>
        /// Split this out as a test case as it breaks 'run all' in the nunit tests console which is really annoying.
        /// </summary>
        [Test]
        public void TestParseLogstashOutputCaseThatDoesntWorkAsTestCaseInput()
        {
            string[] stdOutputLines = { "[2019-04-07T19:09:33,634][ERROR][logstash.agent           ] Failed to execute action {:action=>LogStash::PipelineAction::Create/pipeline_id:main, :exception=>\"LogStash::ConfigurationError\", :message=>\"Expected one of #, { at line 23, column 1 (byte 545) after output {\nif [event][EventType] == 1  {\nfile  \n\", :backtrace=>[\"D:/Logstash/logstash/logstash-core/lib/logstash/compiler.rb:41:in `compile_imperative'\", \"D:/Logstash/logstash/logstash-core/lib/logstash/compiler.rb:49:in" };
            var expectedState = LogstashProcessState.UnrecoverableError;
            var expectedStateMessage = "The configuration is invalid. Please fix the configuration and restart the process.: Expected one of #, { at line 23, column 1 (byte 545) after output {\nif [event][EventType] == 1  {\nfile  \n";

            _classUnderTest.Start(new LogstashProcessSettings(), new SafeString("password"));

            foreach (var line in stdOutputLines)
            {
                _mockProcess.Raise(x => x.OutputDataReceived += null, new StandardOutputEventArgs(line));
            }

            Assert.That(() => _classUnderTest.State.state, Is.EqualTo(expectedState).After(1000, 50));
            Assert.That(() => _classUnderTest.State.stateMessage, Is.EqualTo(expectedStateMessage).After(1000, 50));
        }
    }
}
