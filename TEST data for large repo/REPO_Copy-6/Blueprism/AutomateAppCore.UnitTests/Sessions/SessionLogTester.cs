using BluePrism.AutomateAppCore;
using BluePrism.BPCoreLib.Data;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AutomateAppCore.UnitTests.Sessions
{
    /// <summary>
    /// Test class for session variable parsing
    /// </summary>
    [TestFixture]
    public class SessionLogTester
    {

        /// <summary>
        /// Tests to see if an underscore is added to process names with spaces at the end
        /// Process names with spaces at the end interferes with archiving logs from that process' sessions
        /// </summary>
        [Test]
        public void ExportFilePath_ProcessNameEndsWithSpace_ShouldAppendNameWithUnderscore()
        {
            var testDateTime = DateTime.Now;
            var testGuid = Guid.NewGuid();
            var argProcessName = "Process with space ";
            var dataProviderMock = GetDataProviderMock(ref testDateTime, ref testGuid, ref argProcessName);
            var sessionLog = new clsSessionLog(dataProviderMock.Object);
            Assert.That(sessionLog.ExportFilePath, Is.EqualTo(GetExpectedFilePath(testDateTime, testGuid, "Process with space _")));
        }

        /// <summary>
        /// Tests to see if an underscore is added to process names with spaces at the end
        /// Process names with spaces at the end interferes with archiving logs from that process' sessions
        /// </summary>
        [Test]
        public void ExportFilePath_ProcessNameStartsWithSpace_ShouldAppendNameWithUnderscore()
        {
            var testDateTime = DateTime.Now;
            var testGuid = Guid.NewGuid();
            var argProcessName = " Process with space";
            var dataProviderMock = GetDataProviderMock(ref testDateTime, ref testGuid, ref argProcessName);
            var sessionLog = new clsSessionLog(dataProviderMock.Object);
            Assert.That(sessionLog.ExportFilePath, Is.EqualTo(GetExpectedFilePath(testDateTime, testGuid, "_ Process with space")));
        }

        /// <summary>
        /// Tests to see if any characters that could interfere with file paths are replaced with an underscore
        /// </summary>
        [TestCaseSource(nameof(GetProcessNames))]
        public void ExportFilePath_ProcessNameHasIllegalCharacters_ShouldReplaceWithUnderscore(string processName)
        {
            var testDateTime = DateTime.Now;
            var testGuid = Guid.NewGuid();
            var dataProviderMock = GetDataProviderMock(ref testDateTime, ref testGuid, ref processName);
            var sessionLog = new clsSessionLog(dataProviderMock.Object);
            Assert.That(sessionLog.ExportFilePath, Is.EqualTo(GetExpectedFilePath(testDateTime, testGuid, "processname_")));
        }

        protected static IEnumerable<string> GetProcessNames()
        {
            yield return "processname*";
            yield return "processname\"";
            yield return @"processname\";
            yield return "processname/";
            yield return "processname:";
            yield return "processname|";
            yield return "processname<";
            yield return "processname>";
            yield return "processname?";
        }

        private Mock<IDataProvider> GetDataProviderMock(ref DateTime testDateTime, ref Guid testGuid, ref string processName)
        {
            var dataProviderMock = new Mock<IDataProvider>();
            dataProviderMock.Setup(x => x.GetValue("ProcessName", It.IsAny<string>())).Returns(processName);
            dataProviderMock.Setup(x => x.GetValue("StartDateTime", It.IsAny<DateTime>())).Returns(testDateTime);
            dataProviderMock.Setup(x => x.GetValue("RunningResourceName", It.IsAny<string>())).Returns("a");
            dataProviderMock.Setup(x => x.GetValue("SessionId", It.IsAny<Guid>())).Returns(testGuid);
            return dataProviderMock;
        }

        private static string GetExpectedFilePath(DateTime testDateTime, Guid testGuid, string expectedProcessName)
        {
            return CombinePaths(testDateTime.Year.ToString(), testDateTime.ToString("MM MMMM"), testDateTime.ToString("dd dddd"), expectedProcessName, "a", testDateTime.ToString("yyyyMMdd HHmmss ") + testGuid.ToString() + ".bpl");
        }

        private static string CombinePaths(params string[] pathElements)
        {
            return pathElements.Aggregate(string.Empty, Path.Combine);
        }
    }
}
