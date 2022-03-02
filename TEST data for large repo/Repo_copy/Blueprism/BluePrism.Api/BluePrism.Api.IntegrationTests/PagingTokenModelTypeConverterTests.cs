namespace BluePrism.Api.IntegrationTests
{
    using BluePrism.Api.Models;
    using FluentAssertions;
    using Newtonsoft.Json;
    using NUnit.Framework;

    [TestFixture]
    public class PagingTokenModelTypeConverterTests
    {
        [Test]
        public void ConvertFrom_ReturnPagingTokenModel_WhenValidObjectProvided()
        {
            var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(PagingTokenModel<long>));
            var token = new PagingTokenModel<long>
            {
                DataType = "Int32",
                PreviousSortColumnValue = "1",
                PreviousIdValue = 23,
                ParametersHashCode = "123456789"
            };

            var tokenString = converter.ConvertTo(token, typeof(string)) as string;
            var newToken = converter.ConvertFrom(tokenString) as PagingTokenModel<long>;

            newToken.DataType.Should().Be(token.DataType);
            newToken.PreviousSortColumnValue.Should().Be(token.PreviousSortColumnValue);
            newToken.PreviousIdValue.Should().Be(token.PreviousIdValue);
            newToken.ParametersHashCode.Should().Be(token.ParametersHashCode);
        }

        [Test]
        public void ConvertTo_ReturnString_WhenValidObjectProvided()
        {
            var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(PagingTokenModel<long>));
            var token = new PagingTokenModel<long>
            {
                DataType = "Int32",
                PreviousSortColumnValue = "1",
                PreviousIdValue = 23,
                ParametersHashCode = "123456789"
            };

            var tokenString = converter.ConvertTo(token, typeof(string)) as string;
            tokenString.Should().NotBeNull();
            tokenString.Should().NotContain("PagingTokenModel");
        }

        [Test]
        public void ConvertTo_DoesNotCauseError_WhenValueIsNull()
        {
            var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(PagingTokenModel<long>));
            PagingTokenModel<long> token = null;
            var tokenString = converter.ConvertTo(token, typeof(string));

            tokenString.Should().BeNull();
        }

        [Test]
        public void ConvertFrom_DoesNotCauseError_WhenValueIsNull()
        {
            var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(PagingTokenModel<long>));
            var token = converter.ConvertFrom(default(string));

            token.Should().BeNull();
        }
    }
}
