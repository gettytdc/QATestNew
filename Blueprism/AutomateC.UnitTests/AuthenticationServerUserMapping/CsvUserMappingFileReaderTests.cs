using System;
using System.IO;
using System.Text;
using BluePrism.AutomateAppCore.clsServerPartialClasses.AuthenticationServerUserMapping;
using BluePrism.Utilities.Testing;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AutomateC.UnitTests.AuthenticationServerUserMapping
{
    [TestFixture]
    public class CsvUserMappingFileReaderTests : UnitTestBase<CsvUserMappingFileReader>
    {
        [Test]
        public void Read_ValidRowsCsv_ShouldReturnCorrectMappingRecords()
        {
            var csv = new TestCsvBuilder()
                            .AddImportHeader()
                            .AddLine("", "", "Kevin", "Costner", "kevin.costner@blueprism.com")
                            .AddLine("gford", "d6dfe98f-0c2b-43be-a30c-33e26f990eea", "Gerald", "Ford", "gerald.ford@blueprism.com")
                            .AddLine("ssmith", "18700975-800c-446a-ad05-70240adfe7b8", "Steve", "Smith", "steve.smith@blueprism.com")
                            .Build();
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv)))
            {
                GetMock<IStreamReaderFactory>()
                    .Setup(x => x.Create(It.IsAny<string>()))
                    .Returns(new StreamReader(stream));

                var result = ClassUnderTest.Read("any path");

                var expectedResult = new[] {
                    new UserMappingRecord(string.Empty, null, "Kevin", "Costner",  "kevin.costner@blueprism.com"),
                    new UserMappingRecord("gford", new Guid("d6dfe98f-0c2b-43be-a30c-33e26f990eea"), "Gerald", "Ford",  "gerald.ford@blueprism.com"),
                    new UserMappingRecord("ssmith", new Guid("18700975-800c-446a-ad05-70240adfe7b8"), "Steve", "Smith",  "steve.smith@blueprism.com")
                };

                result.ShouldBeEquivalentTo(expectedResult);
            }           
        }

        [Test]
        public void Read_NameContainingCommaWrappedInQuotes_ShouldReturnCorrectMappingRecords()
        {
            var csv = new TestCsvBuilder()
                            .AddImportHeader()
                            .AddLine("\"g,ford\"", "d6dfe98f-0c2b-43be-a30c-33e26f990eea", "Gerald", "Ford", "gerald.ford@blueprism.com")                            
                            .Build();
            using (new MemoryStream(Encoding.UTF8.GetBytes(csv)))
            {
                GetMock<IStreamReaderFactory>()
                    .Setup(x => x.Create(It.IsAny<string>()))
                    .Returns(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(csv))));

                var result = ClassUnderTest.Read("any path");

                var expectedResult = new[]
                {
                    new UserMappingRecord("g,ford", new Guid("d6dfe98f-0c2b-43be-a30c-33e26f990eea"), "Gerald", "Ford", "gerald.ford@blueprism.com"),
                };

                result.ShouldBeEquivalentTo(expectedResult);
            }
        }

        [Test]
        public void Read_NameContainingEscapedQuotes_ShouldReturnCorrectMappingRecords()
        {
            var csv = new TestCsvBuilder()
                            .AddImportHeader()
                            .AddLine("\"g\"\"ford\"", "d6dfe98f-0c2b-43be-a30c-33e26f990eea", "Gerald", "Ford", "gerald.ford@blueprism.com")
                            .Build();

            using (new MemoryStream(Encoding.UTF8.GetBytes(csv)))
            {
                GetMock<IStreamReaderFactory>()
                    .Setup(x => x.Create(It.IsAny<string>()))
                    .Returns(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(csv))));

                var result = ClassUnderTest.Read("any path");

                var expectedResult = new[]
                {
                    new UserMappingRecord("g\"ford", new Guid("d6dfe98f-0c2b-43be-a30c-33e26f990eea"), "Gerald", "Ford", "gerald.ford@blueprism.com"),
                };

                result.ShouldBeEquivalentTo(expectedResult);
            }
        }
    }
}
