using System;
using System.Data.SqlClient;
using BluePrism.Data;
using BluePrism.Server.Domain.Models;
using BluePrism.UnitTesting;
using BluePrism.AutomateAppCore;
using Moq;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.DynamicSQL
{
    /// <summary>
    /// Tests the methods used to validate dynamic parts of sql that are not parameterized
    /// </summary>
    [TestFixture()]
    public class DynamicSqlValidationTests
    {
        [SetUp]
        public void Setup()
        {
            LegacyUnitTestHelper.SetupDependencyResolver();
        }
        #region "Test Validate Table Name"

        [Test]
        [Description("Test that an invalid table name throws an exception")]
        public void TestInValidTables()
        {
            var mockConn = new Mock<IDatabaseConnection>();
            mockConn.Setup(x => x.ExecuteReturnScalar(It.IsAny<SqlCommand>())).Returns("");
            var s = new clsServer();
            Assert.Throws<BluePrismException>(() => s.ValidateTableName(mockConn.Object, "MyTable"));
        }

        [Test()]
        [NUnit.Framework.Description("Test that a valid table is returned ok")]
        public void TestValidTables()
        {
            const string tableName = "MyTable";
            var mockConn = new Mock<IDatabaseConnection>();
            mockConn.Setup(x => x.ExecuteReturnScalar(It.IsAny<SqlCommand>())).Returns(tableName);
            var s = new clsServer();
            var result = s.ValidateTableName(mockConn.Object, tableName);
            Assert.AreEqual(result, tableName, "Check that the table was a valid table in the database");
        }

        [Test()]
        [NUnit.Framework.Description("Test that a valid table name is cached")]
        public void TestValidTablesCache()
        {
            const string tableName = "MyTable";
            var mockConn = new Mock<IDatabaseConnection>();
            mockConn.Setup(x => x.ExecuteReturnScalar(It.IsAny<SqlCommand>())).Returns(tableName);
            var s = new clsServer();
            var result = s.ValidateTableName(mockConn.Object, tableName);
            Assert.AreEqual(result, tableName, "Check that the table was a valid table in the database");
            mockConn.Setup(x => x.ExecuteReturnScalar(It.IsAny<SqlCommand>())).Throws(new Exception());

            // This time it should be cached, so we shouldn't get the exception
            s.ValidateTableName(mockConn.Object, tableName);
        }

        [Test()]
        [NUnit.Framework.Description("Test that a table name with invalid characters throws and exception")]
        public void TestInValidTableChars()
        {
            const string tableName = "; DROP TABLE USERS; --";
            var mockConn = new Mock<IDatabaseConnection>();
            var s = new clsServer();
            Assert.Throws<BluePrismException>(() => s.ValidateTableName(mockConn.Object, tableName));

            // Should fail
            mockConn.Verify(x => x.ExecuteReturnScalar(It.IsAny<SqlCommand>()), Times.Never());
        }

        [Test()]
        [NUnit.Framework.Description("Test that a table name with all valid characters in is ok")]
        public void TestValidTableChars()
        {
            const string tableName = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var mockConn = new Mock<IDatabaseConnection>();
            mockConn.Setup(x => x.ExecuteReturnScalar(It.IsAny<SqlCommand>())).Returns(tableName);
            var s = new clsServer();
            var result = s.ValidateTableName(mockConn.Object, tableName);
            Assert.AreEqual(result, tableName, "Check that the table was a valid table in the database");
        }

        [Test()]
        [NUnit.Framework.Description("Test that a table name with all valid characters in is ok")]
        public void TestValidTableCharsUnderscoreAndDot()
        {
            const string tableName = "test.test_test1";
            var mockConn = new Mock<IDatabaseConnection>();
            mockConn.Setup(x => x.ExecuteReturnScalar(It.IsAny<SqlCommand>())).Returns(tableName);
            var s = new clsServer();
            var result = s.ValidateTableName(mockConn.Object, tableName);
            Assert.AreEqual(result, tableName, "Check that the table was a valid table in the database");
        }
        #endregion

        #region "Test Validate Field Name"
        [Test()]
        [NUnit.Framework.Description("Test that a table name with all valid characters in is ok")]
        public void TestValidFieldName()
        {
            const string tableName = "test.test_test1";
            const string fieldName = "myField";
            var mockConn = new Mock<IDatabaseConnection>();
            mockConn.SetupSequence(x => x.ExecuteReturnScalar(It.IsAny<SqlCommand>())).Returns(tableName).Returns(fieldName);
            var s = new clsServer();
            var result = s.ValidateFieldName(mockConn.Object, tableName, fieldName);
            Assert.AreEqual(result, fieldName, "Check that the Field was a valid table in the database");
        }

        [Test()]
        [NUnit.Framework.Description("Test Field name Cache")]
        public void TestValidFieldNameCached()
        {
            const string tableName = "test.test_test1";
            const string fieldName = "myField";
            var mockConn = new Mock<IDatabaseConnection>();
            mockConn.SetupSequence(x => x.ExecuteReturnScalar(It.IsAny<SqlCommand>())).Returns(tableName).Returns(fieldName).Throws(new Exception());
            var s = new clsServer();
            var result = s.ValidateFieldName(mockConn.Object, tableName, fieldName);

            // Now should read cached value
            result = s.ValidateFieldName(mockConn.Object, tableName, fieldName);
            Assert.AreEqual(result, fieldName, "Check that the Field was a valid table in the database");
        }

        [Test()]
        [NUnit.Framework.Description("Test that a field name unknown to the database errors")]
        public void TestValidFieldInvalidName()
        {
            const string tableName = "test.test_test1";
            const string fieldName = "myField";
            var mockConn = new Mock<IDatabaseConnection>();
            mockConn.SetupSequence(x => x.ExecuteReturnScalar(It.IsAny<SqlCommand>())).Returns(tableName).Returns("");
            var s = new clsServer();
            Assert.Throws<BluePrismException>(() => s.ValidateFieldName(mockConn.Object, tableName, fieldName));
        }

        [Test()]
        [NUnit.Framework.Description("Test that a field name with all valid chars works ok")]
        public void TestValidFieldValidChars()
        {
            const string tableName = "test.test_test1";
            const string fieldName = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var mockConn = new Mock<IDatabaseConnection>();
            mockConn.SetupSequence(x => x.ExecuteReturnScalar(It.IsAny<SqlCommand>())).Returns(tableName).Returns(fieldName);
            var s = new clsServer();
            var result = s.ValidateFieldName(mockConn.Object, tableName, fieldName);
            Assert.AreEqual(result, fieldName, "Check that the Field was a valid table in the database");
        }

        [Test()]
        [NUnit.Framework.Description("Test a field with and invalid table name chars errors")]
        public void TestValidFieldWithInvalidTableName()
        {
            const string tableName = "test.test_test1; drop table test; --";
            const string fieldName = "myField";
            var mockConn = new Mock<IDatabaseConnection>();
            var s = new clsServer();
            Assert.Throws<BluePrismException>(() => s.ValidateFieldName(mockConn.Object, tableName, fieldName));
        }

        [Test()]
        [NUnit.Framework.Description("Test a field with and invalid table name in db errors")]
        public void TestValidFieldWithInvalidTableInDb()
        {
            const string tableName = "test.test_test1; drop table test; --";
            const string fieldName = "myField";
            var mockConn = new Mock<IDatabaseConnection>();
            mockConn.SetupSequence(x => x.ExecuteReturnScalar(It.IsAny<SqlCommand>())).Returns(tableName);
            var s = new clsServer();
            Assert.Throws<BluePrismException>(() => s.ValidateFieldName(mockConn.Object, tableName, fieldName));
        }
        #endregion

        #region "Logging class selector tests"
        [Test()]
        [NUnit.Framework.Description("Test that the unicode table name is returned when it should be")]
        public void TestLoggingUnicode()
        {
            const int sessionNo = 15;
            const string expectedResult = "BPASessionLog_Unicode";
            var mockConn = new Mock<IDatabaseConnection>();
            mockConn.Setup(x => x.ExecuteReturnScalar(It.IsAny<SqlCommand>())).Returns(expectedResult);
            var s = new clsServer();
            var result = s.GetSessionLogTableName(mockConn.Object, sessionNo);
            Assert.AreEqual(result, expectedResult, "Check that the table returned was the one we expected");
        }

        [Test()]
        [NUnit.Framework.Description("Test that the non unicode table name is returned when it should be")]
        public void TestLoggingNonUnicode()
        {
            const int sessionNo = 15;
            const string expectedResult = "BPASessionLog_NonUnicode";
            var mockConn = new Mock<IDatabaseConnection>();
            mockConn.Setup(x => x.ExecuteReturnScalar(It.IsAny<SqlCommand>())).Returns(expectedResult);
            var s = new clsServer();
            var result = s.GetSessionLogTableName(mockConn.Object, sessionNo);
            Assert.AreEqual(result, expectedResult, "Check that the table returned was the one we expected");
        }

        [Test()]
        [NUnit.Framework.Description("Test that if the session is not found an error is thrown")]
        public void TestLoggingError()
        {
            const int sessionNo = 15;
            var mockConn = new Mock<IDatabaseConnection>();
            mockConn.Setup(x => x.ExecuteReturnScalar(It.IsAny<SqlCommand>())).Returns("");
            var s = new clsServer();
            Assert.Throws<NoSuchSessionException>(() => s.GetSessionLogTableName(mockConn.Object, sessionNo));

            // Should fail
            mockConn.Verify(x => x.ExecuteReturnScalar(It.IsAny<SqlCommand>()), Times.Once());
        }

        [Test()]
        public void TestLogging_CheckTableExists_pre65_exists()
        {
            const string tableName = "BPASessionLog_NonUnicode_pre65";
            var mockConn = new Mock<IDatabaseConnection>();
            mockConn.Setup(x => x.ExecuteReturnScalar(It.IsAny<SqlCommand>())).Returns(tableName);
            var s = new clsServer();
            var result = s.CheckTableExists_pre65(mockConn.Object, tableName);
            Assert.IsTrue(result, "Table should exist");
            mockConn.Verify(x => x.ExecuteReturnScalar(It.IsAny<SqlCommand>()), Times.Once());
        }

        [Test()]
        public void TestLogging_CheckTableExists_pre65_not_exists()
        {
            const string tableName = "BPASessionLog_NonUnicode_pre65";
            var mockConn = new Mock<IDatabaseConnection>();
            mockConn.Setup(x => x.ExecuteReturnScalar(It.IsAny<SqlCommand>())).Returns("");
            var s = new clsServer();
            var result = s.CheckTableExists_pre65(mockConn.Object, tableName);
            Assert.IsFalse(result, "Table should not exist");
            mockConn.Verify(x => x.ExecuteReturnScalar(It.IsAny<SqlCommand>()), Times.Once());
        }

        [Test()]
        public void TestLogging_GetSessionLogTableName_pre65_sessionNo_exists()
        {
            const string tableName = "BPASessionLog_NonUnicode_pre65";
            const int sessionNo = 15;
            var mockConn = new Mock<IDatabaseConnection>();
            mockConn.Setup(x => x.ExecuteReturnScalar(It.IsAny<SqlCommand>())).Returns(tableName);
            var mockServer = new Mock<clsServer>
            {
                CallBase = true
            };
            mockServer.Setup<bool>(x => x.CheckTableExists_pre65(mockConn.Object, "BPASessionLog_Unicode_pre65")).Returns(true);
            var result = mockServer.Object.GetSessionLogTableName_pre65(mockConn.Object, sessionNo);
            Assert.AreEqual(result, tableName, "Table returned was not expected");
            mockConn.Verify(x => x.ExecuteReturnScalar(It.IsAny<SqlCommand>()), Times.Once());
        }

        [Test()]
        public void TestLogging_GetSessionLogTableName_pre65_sessionNo_notexists0()
        {
            const string tableName = "BPASessionLog_NonUnicode_pre65";
            const int sessionNo = 15;
            var mockConn = new Mock<IDatabaseConnection>();
            mockConn.Setup(x => x.ExecuteReturnScalar(It.IsAny<SqlCommand>())).Returns(tableName);
            var mockServer = new Mock<clsServer>
            {
                CallBase = true
            };
            mockServer.Setup(x => x.CheckTableExists_pre65(mockConn.Object, "BPASessionLog_Unicode_pre65")).Returns(false);
            var result = mockServer.Object.GetSessionLogTableName_pre65(mockConn.Object, sessionNo);
            Assert.IsTrue(string.IsNullOrEmpty(result), "Table should not exist");
            mockConn.Verify(x => x.ExecuteReturnScalar(It.IsAny<SqlCommand>()), Times.Never());
        }

        [Test()]
        public void TestLogging_GetSessionLogTableName_pre65_sessionNo_notexists1()
        {
            const int sessionNo = 15;
            var mockConn = new Mock<IDatabaseConnection>();
            mockConn.Setup(x => x.ExecuteReturnScalar(It.IsAny<SqlCommand>())).Returns("");
            var mockServer = new Mock<clsServer>
            {
                CallBase = true
            };
            mockServer.Setup<bool>(x => x.CheckTableExists_pre65(mockConn.Object, "BPASessionLog_Unicode_pre65")).Returns(true);
            var result = mockServer.Object.GetSessionLogTableName_pre65(mockConn.Object, sessionNo);
            Assert.IsTrue(string.IsNullOrEmpty(result), "Table should not exist");
            mockConn.Verify(x => x.ExecuteReturnScalar(It.IsAny<SqlCommand>()), Times.Once());
        }

        #endregion
    }
}
