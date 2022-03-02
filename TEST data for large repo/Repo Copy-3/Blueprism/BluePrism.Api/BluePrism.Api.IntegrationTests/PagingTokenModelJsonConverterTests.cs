namespace BluePrism.Api.IntegrationTests
{
    using System.IO;
    using FluentAssertions;
    using Models;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using Utilities.Testing;

    [TestFixture]
    public class PagingTokenModelJsonConverterTests : UnitTestBase<PagingTokenModelJsonConverter<long>>
    {
        [Test]
        public void ReadJson_ShouldReturnTokenObject_WhenGivenTokenJson()
        {
            var testToken = new PagingTokenModel<long>
            {
                DataType = "Int32",
                ParametersHashCode = "123456789",
                PreviousIdValue = 1,
                PreviousSortColumnValue = "test"
            };

            var json = JsonConvert.SerializeObject(testToken, new PagingTokenModelJsonConverter<long>());
            using (var stringReader = new StringReader(json))
            using (var reader = new JsonTextReader(stringReader))
            {
                while (reader.TokenType == JsonToken.None)
                {
                    if (!reader.Read())
                    {
                        break;
                    }
                }

                var result = (PagingTokenModel<long>)ClassUnderTest.ReadJson(reader, typeof(PagingTokenModel<long>), null, JsonSerializer.CreateDefault());
                result.DataType.Should().Be(testToken.DataType);
                result.ParametersHashCode.Should().Be(testToken.ParametersHashCode);
                result.PreviousIdValue.Should().Be(testToken.PreviousIdValue);
                result.PreviousSortColumnValue.Should().Be(testToken.PreviousSortColumnValue);
            }
        }

        [Test]
        public void ReadJson_ShouldReturn_TokenJsonString_WhenGivenToken()
        {
            var testToken = new PagingTokenModel<long>
            {
                DataType = "Int32",
                ParametersHashCode = "123456789",
                PreviousIdValue = 1,
                PreviousSortColumnValue = "test"
            };

            var equivalentToken = new Domain.PagingTokens.PagingToken<long>
            {
                DataType = "Int32",
                ParametersHashCode = "123456789",
                PreviousIdValue = 1,
                PreviousSortColumnValue = "test"
            };

            var expectedJson = JsonConvert.SerializeObject(equivalentToken);

            using (var stringWriter = new StringWriter())
            using (var writer = new JsonTextWriter(stringWriter))
            {
                writer.Formatting = Formatting.None;

                ClassUnderTest.WriteJson(writer, testToken, JsonSerializer.CreateDefault());
                stringWriter.ToString().ShouldBeEquivalentTo(expectedJson);
            }
        }
    }
}
