using System;
using System.IO;
using System.Linq;
using System.Text;
using AutomateC.AuthenticationServerUserMapping;
using BluePrism.AutomateAppCore.clsServerPartialClasses.AuthenticationServerUserMapping;
using BluePrism.Utilities.Testing;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AutomateC.UnitTests.AuthenticationServerUserMapping
{
    [TestFixture]
    public class CsvUserMappingErrorLoggerTests : UnitTestBase<CsvUserMappingErrorLogger>
    {
        [Test]
        public void LogErrors_HasErrors_ShouldWriteErrors()
        {
            using (var stream = new MemoryStream())
            {
                GetMock<IStreamWriterFactory>()
                .Setup(x => x.Create(It.IsAny<string>()))
                .Returns(new StreamWriter(stream));

                var gfordAuthServerId = Guid.NewGuid();
                var ssmithAuthServerId = Guid.NewGuid();

                var errors = new[] {
                    new
                    {
                        mappingRecord = new UserMappingRecord(string.Empty, null, "Kevin", "Costner",  "kevin.costner@blueprism.com"),
                        errorCode = UserMappingResultCode.BluePrismUserNotFound
                    },
                    new
                    {
                        mappingRecord = new UserMappingRecord("gford", gfordAuthServerId, "Gerald", "Ford",  "gerald.ford@blueprism.com"),
                        errorCode = UserMappingResultCode.BluePrismUserHasAlreadyBeenMapped
                    },
                    new
                    {
                        mappingRecord = new UserMappingRecord("ssmith", ssmithAuthServerId, "Steve", "Smith",  "steve.smith@blueprism.com"),
                        errorCode = UserMappingResultCode.BluePrismUsersAuthTypeDoesNotSupportMapping
                    }
                };

                var results = errors.Select(e => UserMappingResult.Failed(e.mappingRecord, e.errorCode)).ToList();

                ClassUnderTest.LogErrors("anything", results);                

                var errorFile = Encoding.UTF8.GetString(stream.ToArray());

                var expectedCsv = new TestCsvBuilder()
                                        .AddOutputLine("", "", "Kevin", "Costner", "kevin.costner@blueprism.com", UserMappingResultCode.BluePrismUserNotFound.ToLocalizedDescription())
                                        .AddOutputLine("gford", gfordAuthServerId.ToString(), "Gerald", "Ford", "gerald.ford@blueprism.com", UserMappingResultCode.BluePrismUserHasAlreadyBeenMapped.ToLocalizedDescription())
                                        .AddOutputLine("ssmith", ssmithAuthServerId.ToString(), "Steve", "Smith", "steve.smith@blueprism.com", UserMappingResultCode.BluePrismUsersAuthTypeDoesNotSupportMapping.ToLocalizedDescription())
                                        .Build();

                errorFile.Should().Be(expectedCsv);

            }
        }
    }
}
