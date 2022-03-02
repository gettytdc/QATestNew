using BluePrism.BPServer.Enums;
using BluePrism.BPServer.Utility;
using NUnit.Framework;

namespace BPServer.UnitTests
{
    [TestFixture]
    public class ExtensionTests
    {

        [TestCase(LoggingLevel.Trace, ExpectedResult = "Trace")]
        [TestCase(LoggingLevel.Debug, ExpectedResult = "Debug")]
        [TestCase(LoggingLevel.Information, ExpectedResult = "Info")]
        [TestCase(LoggingLevel.Warning, ExpectedResult = "Warn")]
        [TestCase(LoggingLevel.Error, ExpectedResult = "Error")]
        [TestCase(LoggingLevel.Fatal, ExpectedResult = "Fatal")]
        [TestCase(LoggingLevel.Analytics, ExpectedResult = "Warn")]
        [TestCase(LoggingLevel.Verbose, ExpectedResult = "Warn")]
        public string LoggingLevelMapsToCorrectNLogLevel(LoggingLevel level)
        {
            var nLogLevel = level.ToNLogLevel();
            return nLogLevel?.Name;
        }


        
    }
}
