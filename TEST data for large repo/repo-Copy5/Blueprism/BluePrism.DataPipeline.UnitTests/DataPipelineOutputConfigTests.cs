using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BluePrism.DataPipeline.DataPipelineOutput;
using NUnit.Framework;

namespace BluePrism.DataPipeline.UnitTests
{
    [TestFixture]
    public class DataPipelineOutputConfigTests
    {
        #region "File Output Config Tests"
        [Test]
        public void DataPipelineOutputConfig_GetLogstashConfig_OneOutputType_ConfigValid()
        {
            string expected =
                @"if [event][EventType] == 1 {
                    file { 
                    ZXC => ""QWE""
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
                Name = "TestLog"
            };

            var result = config.GetLogstashConfig();
            Assert.AreEqual(RemoveWhiteSpace(expected), RemoveWhiteSpace(result));
        }

        [Test]
        public void DataPipelineOutputConfig_GetLogstashConfig_SessionLogFilteredOutputType_ConfigValid()
        {
            string expected =
                @"if ([event][EventType] == 1) and [type] == ""TestLog"" {
                    file { 
                    ZXC => ""QWE""
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
                Name = "TestLog",
                SelectedSessionLogFields = new List<string>() { "ProcessName", "StartDate"}
            };

            var result = config.GetLogstashConfig();
            Assert.AreEqual(RemoveWhiteSpace(expected), RemoveWhiteSpace(result));
        }


        [Test]
        public void DataPipelineOutputConfig_GetLogstashConfig_TwoOutputTypes_ConfigValid()
        {
            string expected =
                @"if [event][EventType] == 1 or ([event][EventType] == 2 and ([event][EventData][Source] == ""DB1"")) {
                    file { 
                    ZXC => ""QWE""
                            }
                        }";

            var config = new DataPipelineOutputConfig
            {
                IsSessions = true,
                IsDashboards = true,
                IsWqaSnapshotData = false,
                IsCustomObjectData = false,
                OutputType = new OutputType("File", "file"),
                OutputOptions = new List<OutputOption> { new OutputOption("ZXC", "QWE") },
                SelectedDashboards = new List<string> { "DB1" },
                Name = "TestLog"
            };

            var result = config.GetLogstashConfig();
            Assert.AreEqual(RemoveWhiteSpace(expected), RemoveWhiteSpace(result));
        }

        [Test]
        public void DataPipelineOutputConfig_GetLogstashConfig_ThreeOutputTypes_ConfigValid()
        {
            string expected =
                @"if [event][EventType] == 1 or ([event][EventType] == 2 and ([event][EventData][Source] == ""DB1"")) or [event][EventType] == 3 {
                    file { 
                    ZXC => ""QWE""
                            }
                        }";

            var config = new DataPipelineOutputConfig
            {
                IsDashboards = true,
                IsSessions = true,
                IsWqaSnapshotData = false,
                IsCustomObjectData = true,
                OutputType = new OutputType("File", "file"),
                OutputOptions = new List<OutputOption> { new OutputOption("ZXC", "QWE") },
                SelectedDashboards = new List<string> {"DB1"},
                Name = "TestLog"
            };

            var result = config.GetLogstashConfig();
            Assert.AreEqual(RemoveWhiteSpace(expected), RemoveWhiteSpace(result));
        }

        [Test]
        public void DataPipelineOutputConfig_GetLogstashConfig_MultipleDashboards_ConfigValid()
        {
            string expected =
                @"if [event][EventType] == 1 or ([event][EventType] == 2 and ([event][EventData][Source] == ""DB1"" or [event][EventData][Source] == ""DB2"" or [event][EventData][Source] == ""DB3"")) {
                    file { 
                    ZXC => ""QWE""
                            }
                        }";

            var config = new DataPipelineOutputConfig
            {
                IsDashboards = true,
                IsSessions = true,
                IsWqaSnapshotData = false,
                IsCustomObjectData = false,
                OutputType = new OutputType("File", "file"),
                OutputOptions = new List<OutputOption> { new OutputOption("ZXC", "QWE") },
                SelectedDashboards = new List<string> { "DB1", "DB2", "DB3" },
                Name = "TestLog"
            };

            var result = config.GetLogstashConfig();
            Assert.AreEqual(RemoveWhiteSpace(expected), RemoveWhiteSpace(result));
        }

        [Test]
        public void DataPipelineOutputConfig_GetLogstashConfig_FourOutputTypes_ConfigValid()
        {
            string expected =
                @"if [event][EventType] == 1 or ([event][EventType] == 2 and ([event][EventData][Source] == ""DB1"")) or [event][EventType] == 4 or [event][EventType] == 3  {
                    file { 
                    ZXC => ""QWE""
                            }
                        }";

            var config = new DataPipelineOutputConfig
            {
                IsDashboards = true,
                IsSessions = true,
                IsWqaSnapshotData = true,
                IsCustomObjectData = true,
                OutputType = new OutputType("File", "file"),
                OutputOptions = new List<OutputOption> { new OutputOption("ZXC", "QWE") },
                SelectedDashboards = new List<string> { "DB1" },
                Name = "TestLog"
            };

            var result = config.GetLogstashConfig();
            Assert.AreEqual(RemoveWhiteSpace(expected), RemoveWhiteSpace(result));
        }

        [Test]
        public void DataPipelineOutputConfig_GetLogstashConfig_NoOutputType_ExceptionThrown()
        {
            var config = new DataPipelineOutputConfig
            {
                IsDashboards = false,
                IsSessions = false,
                OutputType = new OutputType("File", "file"),
                OutputOptions = new List<OutputOption> { new OutputOption("ZXC", "QWE") }
            };

            Assert.Throws<InvalidOperationException>(() => config.GetLogstashConfig());
        }

        [Test]
        public void DataPipelineOutputConfig_GetLogstashConfig_MultiplePublishedDashboardSelected_ConfigValid()
        {
            string expected =
                @"if([event][EventType] == 2 and ([event][EventData][Source] == ""DB1"" or [event][EventData][Source] == ""DB2"")) {
                    file { 
                    ZXC => ""QWE""
                            }
                        }";

            var config = new DataPipelineOutputConfig
            {
                IsSessions = false,
                IsDashboards = true,
                IsWqaSnapshotData = false,
                IsCustomObjectData = false,
                OutputType = new OutputType("File", "file"),
                OutputOptions = new List<OutputOption> { new OutputOption("ZXC", "QWE") },
                SelectedDashboards = new List<string> { "DB1", "DB2" }
            };

            var result = config.GetLogstashConfig();
            Assert.AreEqual(RemoveWhiteSpace(expected), RemoveWhiteSpace(result));
        }

        private string RemoveWhiteSpace(string s)
        {
            return Regex.Replace(s, @"\s+", "");
        }
        #endregion

        #region "Database Output Config Tests"
        [Test]
        public void DataPipelineOutputConfig_Database_Valid_Integrated()
        {
            string expected =
                @"if [event][EventType] == 1 or ([event][EventType] == 2 and ([event][EventData][Source] == ""DB1"")) or [event][EventType] == 4 {
                    bpjdbc {
    connection_string => ""jdbc:sqlserver://DB:1433;databaseName=DBNAME;integratedSecurity=true;""
    driver_jar_path => ""..\sqljdbc_4.2\enu\jre8\sqljdbc42.jar""
    driver_class => ""com.microsoft.sqlserver.jdbc.SQLServerDriver""
    statement => [""insert into TestTable(EventType, EventData) values(?, ?)"", ""[event][EventType]"", ""[event][EventData]""]}}";


            var options = new List<OutputOption>();
            options.Add(new OutputOption("security", "integratedSecurity"));
            options.Add(new OutputOption("server", "DB:1433"));
            options.Add(new OutputOption("tableName", "TestTable"));
            options.Add(new OutputOption("databaseName", "DBNAME"));

            var config = new DataPipelineOutputConfig
            {
                IsDashboards = true,
                IsSessions = true,
                IsWqaSnapshotData = true,
                IsAdvanced = false,
                OutputType = new DatabaseOutput("dbName", "database"),
                OutputOptions = options,
                SelectedDashboards = new List<string> { "DB1" },
                Name = "TestLog"
            };

            var result = config.GetLogstashConfig();
            Assert.AreEqual(RemoveWhiteSpace(expected), RemoveWhiteSpace(result));
        }

        [Test]
        public void DataPipelineOutputConfig_Database_Valid_Credential()
        {
            string expected =
                @"if [event][EventType] == 1 or ([event][EventType] == 2 and ([event][EventData][Source] == ""DB1"")) or [event][EventType] == 4 {
                    bpjdbc {
    connection_string => ""jdbc:sqlserver://DB:1433;databaseName=DBNAME;user=<%blah.username%>;password=<%blah.password%>;""
    driver_jar_path => ""..\sqljdbc_4.2\enu\jre8\sqljdbc42.jar""
    driver_class => ""com.microsoft.sqlserver.jdbc.SQLServerDriver""
    statement => [""insert into TestTable(EventType, EventData) values(?, ?)"", ""[event][EventType]"", ""[event][EventData]""]}}";

            var options = new List<OutputOption>();
            options.Add(new OutputOption("security", "credentialSecurity"));
            options.Add(new OutputOption("credential", "blah"));
            options.Add(new OutputOption("server", "DB:1433"));
            options.Add(new OutputOption("tableName", "TestTable"));
            options.Add(new OutputOption("databaseName", "DBNAME"));

            var config = new DataPipelineOutputConfig
            {
                IsDashboards = true,
                IsSessions = true,
                IsWqaSnapshotData = true,
                IsAdvanced = false,
                OutputType = new DatabaseOutput("dbName", "database"),
                OutputOptions = options,
                SelectedDashboards = new List<string> { "DB1" },
                Name = "TestLog"
            };

            var result = config.GetLogstashConfig();
            Assert.AreEqual(RemoveWhiteSpace(expected), RemoveWhiteSpace(result));
        }

        #endregion

        #region "Splunk Output Config Tests"
        [Test]
        public void DataPipelineOutputConfig_Splunk_Valid()
        {
            string expected = @"if [event][EventType] == 1 or ([event][EventType] == 2 and ([event][EventData][Source] == ""DB1"")) or [event][EventType] == 4 {
            bphttp      {
            http_method => ""post""
            url => ""http:\\www.splunk.com\demo""
            headers => [""Authorization"", ""Splunk abcdefg1234567890HJKLMNOP""]
            mapping => { ""event"" => ""%{event}""}}}";


            var options = new List<OutputOption>();
            options.Add(new OutputOption("url", @"http:\\www.splunk.com\demo"));
            options.Add(new OutputOption("token", @"abcdefg1234567890HJKLMNOP"));

            var config = new DataPipelineOutputConfig
            {
                IsDashboards = true,
                IsSessions = true,
                IsWqaSnapshotData = true,
                IsAdvanced = false,
                OutputType = new SplunkOutput("splunk", "splunk"),
                OutputOptions = options,
                SelectedDashboards = new List<string> { "DB1" },
                Name = "TestLog"
            };

            var result = config.GetLogstashConfig();
            Assert.AreEqual(RemoveWhiteSpace(expected), RemoveWhiteSpace(result));
        }


        #endregion

        [Test]
        public void AuthorizationOutputOption_GetOutputOptionConfig_ValidHeader()
        {
            string expected = "headers => {\"Authorization\" => \"Basic<base64> <% credentialName.username %>:<% credentialName.password %></ base64 > \"}";
            var authorizationOutputOption = new AuthorizationOutputOption("credentialId", "credentialName");

            var actual = authorizationOutputOption.GetConfig();

            Assert.AreEqual(RemoveWhiteSpace(expected), RemoveWhiteSpace(actual));
        }
    }
}
