using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BluePrism.Core.Analytics;
using FluentAssertions;
using NUnit.Framework;

namespace BluePrism.Core.UnitTests.Logging
{
    [TestFixture]
    public class PerformanceLoggerTests
    {
        private List<string> TestSingleTypeCaptureReport = new List<string>()
        {
            "GetTreeResponse,2,11305,50000,30652.5,19347.5,158,200,179,21,358,0,100,50,True"
        };


        private List<string> TestTwoTypesReport = new List<string>()
        {
            "GetTreeResponse,2,11305,50000,30652.5,19347.5,158,200,179,21,358,0,50,27.2036474164134,True",
            "GetProcessHistoryLogResponse,2,2000,10000,6000,4000,100,200,150,50,300,0,50,22.7963525835866,True"
        };

        [Test]
        public void PerformanceLogger_TestSingleTypeCaptureReport()
        {
            IMessageEventLogger messageEventLogger = new RecordOverviewAnalyser();

            DateTime utcDateTimeFixed = new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc);

            messageEventLogger.RecordCallEvent("GetTreeResponse", 11305, 158, utcDateTimeFixed);
            messageEventLogger.RecordCallEvent("GetTreeResponse", 50000, 200, utcDateTimeFixed);
            
            messageEventLogger.Analyse();

            var results = messageEventLogger.CreateReport(true).ToList();
            results.RemoveAt(0);

            Assert.AreEqual(TestSingleTypeCaptureReport, results.ToList());
        }

        [Test]
        public void PerformanceLogger_TestTwoTypesCaptureReport()
        {
            IMessageEventLogger messageEventLogger = new RecordOverviewAnalyser();

            DateTime utcDateTimeFixed = new DateTime(2020, 1, 1, 8, 0, 0, DateTimeKind.Utc);

            messageEventLogger.RecordCallEvent("GetTreeResponse", 11305, 158, utcDateTimeFixed);
            messageEventLogger.RecordCallEvent("GetTreeResponse", 50000, 200, utcDateTimeFixed);
            messageEventLogger.RecordCallEvent("GetProcessHistoryLogResponse", 2000, 100, utcDateTimeFixed);
            messageEventLogger.RecordCallEvent("GetProcessHistoryLogResponse", 10000, 200, utcDateTimeFixed);

            messageEventLogger.Analyse();

            var results = messageEventLogger.CreateReport(true).ToList();
            results.RemoveAt(0);

            Assert.AreEqual(TestTwoTypesReport, results);
        }
    }
}
