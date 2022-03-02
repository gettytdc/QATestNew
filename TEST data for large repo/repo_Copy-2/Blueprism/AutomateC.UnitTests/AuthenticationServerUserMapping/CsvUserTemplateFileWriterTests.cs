using System.IO;
using System.Linq;
using System.Collections.Generic;
using BluePrism.Utilities.Testing;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AutomateC.UnitTests.AuthenticationServerUserMapping
{
    [TestFixture]
    public class CsvUserTemplateFileWriterTests : UnitTestBase<CsvUserTemplateFileWriter>
    {
        private const string CsvLineTerminator = "\r\n";

        [Test]
        public void Write_ValidCsvTemplateWithUsers_ShouldBeOk()
        {
            var csvHeading = "BluePrism User name,Authentication Server user ID,FirstName,LastName,Email";
            var writeBuffer = new List<string>();
            var streamMock = new Mock<StreamWriter>("output.csv");
            GetMock<IStreamWriterFactory>().Setup(x => x.Create(It.IsAny<string>())).Returns(streamMock.Object);

            streamMock.Setup(s => s.Write(It.IsAny<char[]>(), It.IsAny<int>(), It.IsAny<int>()))
                      .Callback((char[] m, int i, int c) => writeBuffer.Add(new string(m, i, c)));

            var userRecords = new List<UserMappingCsvRecord> {
                    new UserMappingCsvRecord() { BluePrismUsername = "test1" },
                    new UserMappingCsvRecord() { BluePrismUsername = "test2" },
                    new UserMappingCsvRecord() { BluePrismUsername = "test3,with,comma" } };

            ClassUnderTest.Write(new CsvUserTemplate("output.csv", csvHeading, userRecords));

            writeBuffer.Count().Should().Be(5);
            writeBuffer[0].Should().Be(csvHeading + CsvLineTerminator);
            writeBuffer[1].Should().Be(userRecords[0].BluePrismUsername + ",,,," + CsvLineTerminator);
            writeBuffer[2].Should().Be(userRecords[1].BluePrismUsername + ",,,," + CsvLineTerminator);
            writeBuffer[3].Should().Be("\"" + userRecords[2].BluePrismUsername + "\",,,," + CsvLineTerminator);
            writeBuffer[4].Should().Be(string.Empty);
        }

        [Test]
        public void Write_ValidCsvTemplateWithNoUsers_ShouldBeOk()
        {
            var csvHeading = "BluePrism User name,Authentication Server user ID,FirstName,LastName,Email";
            var writeBuffer = new List<string>();
            var streamMock = new Mock<StreamWriter>("tmp.csv");
            GetMock<IStreamWriterFactory>().Setup(x => x.Create(It.IsAny<string>())).Returns(streamMock.Object);

            streamMock.Setup(s => s.Write(It.IsAny<char[]>(), It.IsAny<int>(), It.IsAny<int>()))
                      .Callback((char[] m, int i, int c) => writeBuffer.Add(new string(m, i, c)));

            var userRecords = new List<UserMappingCsvRecord>();

            ClassUnderTest.Write(new CsvUserTemplate("tmp.csv", csvHeading, userRecords));

            writeBuffer.Count().Should().Be(2);
            writeBuffer[0].Should().Be(csvHeading + CsvLineTerminator);
            writeBuffer[1].Should().Be(string.Empty);
        }
    }
}
