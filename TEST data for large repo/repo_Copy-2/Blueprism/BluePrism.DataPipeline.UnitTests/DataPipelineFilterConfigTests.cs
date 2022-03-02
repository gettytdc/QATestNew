using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BluePrism.DataPipeline.DataPipelineOutput;
using NUnit.Framework;

namespace BluePrism.DataPipeline.UnitTests
{
    [TestFixture]
    public class DataPipelineFilterConfigTests
    {
        [Test]
        public void DataPipelineOutputConfig_GetLogstashFilterConfig_SessionLogs_ConfigValid()
        {
            string expected =
                @"if [event][EventType] == 1 and [type] == ""TestLog"" {
                    ruby {
                        code => ""
                        event.get('[event][EventData]').to_hash.keys.each {|k,v|
                        if (!['StartDate','ProcessName'].include?(k))
                            event.remove('[event][EventData]' + '[' + k + ']')
                        end
                        }""
                    }
                }";

            var config = new DataPipelineOutputConfig
            {
                IsSessions = true,
                IsDashboards = false,
                IsWqaSnapshotData = false,
                IsCustomObjectData = false,
                OutputType = new OutputType("File", "file"),
                OutputOptions = new List<OutputOption> { new OutputOption("ZXC", "QWE") },
                SelectedSessionLogFields = new List<string>() { "StartDate", "ProcessName" } ,
                Name = "TestLog"
            };

            var result = config.GetSessionFilter();
            Assert.AreEqual(RemoveWhiteSpace(expected), RemoveWhiteSpace(result));
        }

        private string RemoveWhiteSpace(string s)
        {
            return Regex.Replace(s, @"\s+", "");
        }
    }
}
