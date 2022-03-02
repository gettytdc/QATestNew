using BluePrism.AutomateAppCore;
using BluePrism.AutomateProcessCore;
using BluePrism.BPCoreLib.Data;
using NUnit.Framework;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System;
using BluePrism.Server.Domain.Models;

namespace AutomateAppCore.UnitTests.Archive
{
    [TestFixture]
    public class ArchiveTests
    {
        [Test]
        public void TestExport()
        {
            const string fileNameFormat = @"C:\temp\test-archive-export-{0:#000}.xml";
            string fileName;
            var index = 0;
            do
            {
                index += 1;
                fileName = string.Format(fileNameFormat, index);
            }
            while ((File.Exists(fileName) || File.Exists(fileName + ".gz")));

            var fileInfo = new FileInfo(fileName);
            var map = new Hashtable
            {
                {"SessionId", Guid.NewGuid()},
                {"SessionNumber", 1},
                {"StartDateTime", DateTime.Now.AddMilliseconds(-3273489)},
                {"EndDateTime", DateTime.Now.AddMilliseconds(-5000)},
                {"ProcessId", Guid.NewGuid()},
                {"StarterResourceId", Guid.NewGuid()},
                {"StarterUserId", Guid.NewGuid()},
                {"RunningResourceId", Guid.NewGuid()},
                {"RunningOSUserName", "woodst"},
                {"StatusId", SessionStatus.Completed}
            };

            var log = new FakeSessionLog(new DictionaryDataProvider(map));
            log.Add(1, Guid.NewGuid(), "Go Go GO!", StageTypes.Start, "Some process, this", "My page or yours?", null, null, null, null, DateTime.Now.AddSeconds(-25678), null, null);
            log.Add(3, Guid.NewGuid(), "Call Something", StageTypes.Note, "Some process, this", "My page or yours?", "An Object", "An action", null, null,
                DateTime.Now.AddSeconds(-25670), DateTime.Now.AddSeconds(-25660),
                "<parameters><inputs><input name=\"SkippedWeight\" type=\"number\" value=\"\" />" +
                "<input name=\"CompletedWeight\" type=\"number\" value=\"\" />" +
                "<input name=\"ExceptionedWeight\" type=\"number\" value=\"\" />" +
                "<input name=\"DeferredWeight\" type=\"number\" value=\"\" />" +
                "<input name=\"Start Number\" type=\"number\" value=\"\" />" +
                "<input name=\"End Number\" type=\"number\" value=\"\" />" +
                "<input name=\"Process Items\" type=\"flag\" value=\"\" />" +
                "</inputs><outputs /></parameters>");
            log.Add(4, Guid.NewGuid(), "Onward", StageTypes.Calculation, null, null, "An Object", "An action", "True",
                DataType.flag, DateTime.Now.AddSeconds(-25668), null, null);
            log.Add(5, Guid.NewGuid(), "Backward", StageTypes.End, null, null, "An Object", "An action", null, null, DateTime.Now.AddSeconds(-25661), null,
                "<parameters><inputs /><outputs><output name=\"Count\" type=\"number\" value=\"8\" /></outputs></parameters>");
            log.Add(6, Guid.NewGuid(), "Further Onward", StageTypes.Note, "Some process, this", "My page or yours?", null,
                null, null, null, DateTime.Now.AddSeconds(-25655), null, null);
            log.Add(7, Guid.NewGuid(), "More and more", StageTypes.Calculation, "Some process, this", "My page or yours?",
                null, null, "5", DataType.number, DateTime.Now.AddSeconds(-25650), null, null);
            log.Add(8, Guid.NewGuid(), "Call a sheet", StageTypes.SubSheet, "Some process, this", "My page or yours?",
                null, null, null, null, DateTime.Now.AddSeconds(-25647), null, null);
            log.Add(9, Guid.NewGuid(), "Start a sheet", StageTypes.Start, "Some process, this", "Yours, then", null, null,
                null, null, DateTime.Now.AddSeconds(-25642), null, null);
            log.Add(10, Guid.NewGuid(), "End a sheet", StageTypes.End, "Some process, this", "Yours, then", null, null,
                null, null, DateTime.Now.AddSeconds(-25640), null, null);
            log.Add(11, Guid.NewGuid(), "Final Demand", StageTypes.End, "Some process, this", "My page or yours?", null,
                null, null, null, DateTime.Now.AddSeconds(-25635), null, null);

            var memoryStream = new MemoryStream();
            log.ExportTo(memoryStream);
            log.ExportTo(fileInfo, false);
        }

        private class FakeSessionLog : clsSessionLog
        {
            // The list of entries held in this session log.
            private IList<clsSessionLogEntry> mEntries;

            /// <summary>
            ///         ''' Creates a new session log with data from the given provider.
            ///         ''' </summary>
            ///         ''' <param name="prov"></param>
            public FakeSessionLog(IDataProvider prov) : base(prov)
            {
                mEntries = new List<clsSessionLogEntry>();
            }

            /// <summary>
            /// Adds the given entry to this object.
            /// </summary>
            /// <param name="logId"></param>
            /// <param name="stageId"></param>
            /// <param name="stageName"></param>
            /// <param name="stageType"></param>
            /// <param name="procName"></param>
            /// <param name="pageName"></param>
            /// <param name="objName"></param>
            /// <param name="actionName"></param>
            /// <param name="result"></param>
            /// <param name="resultType"></param>
            /// <param name="startDate"></param>
            /// <param name="endDate"></param>
            /// <param name="attrXml"></param>
            public void Add(long logId, Guid stageId, string stageName, StageTypes stageType, string procName, string pageName, string objName, string actionName, string result, DataType? resultType, DateTime? startDate, DateTime? endDate, string attrXml)
            {
                startDate = startDate ?? DateTime.MinValue;
                endDate = endDate ?? DateTime.MinValue;
                resultType = resultType ?? DataType.unknown;

                var map = new Hashtable();
                map.Add("SessionNumber", mSessionNumber);
                map.Add("Logid", logId);
                map.Add("StageId", stageId);
                map.Add("StageName", stageName);
                map.Add("StageType", stageType);
                map.Add("ProcessName", procName);
                map.Add("PageName", pageName);
                map.Add("ObjectName", objName);
                map.Add("ActionName", actionName);
                map.Add("Result", result);
                map.Add("ResultType", resultType);
                map.Add("StartDateTime", startDate);
                map.Add("EndDateTime", endDate);
                map.Add("AttributeXML", attrXml);
                mEntries.Add(new clsSessionLogEntry(new DictionaryDataProvider(map)));
            }

            /// <summary>
            /// Gets an enumerator over the entries held in this object.
            /// </summary>
            /// <returns></returns>
            public override IEnumerator<clsSessionLogEntry> GetEnumerator()
            {
                return mEntries.GetEnumerator();
            }
        }
    }
}
