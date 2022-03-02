namespace BluePrism.Api.UnitTests.Mappers
{
    using Api.Mappers;
    using Domain.PagingTokens;
    using FluentAssertions;
    using Func;
    using Models;
    using NUnit.Framework;

    [TestFixture]
    public class PagingTokenMapperTests
    {
        [Test]
        public void ToDomainPagingToken_ShouldReturnOptionNone_WhenPagingTokenIsNull()
        {
            PagingTokenModel<int> testToken = null;
            var domainTokenOption = testToken.ToDomainPagingToken();
            domainTokenOption.Should().BeAssignableTo<None>();
        }

        [Test]
        public void ToDomainPagingToken_ShouldReturnOptionNone_WhenPagingTokenIsEmptyString()
        {
            PagingTokenModel<int> testToken = "";
            var domainTokenOption = testToken.ToDomainPagingToken();
            domainTokenOption.Should().BeAssignableTo<None>();
        }

        [Test]
        public void ToDomainPagingToken_ShouldReturnOptionNone_WhenPagingTokenIsMalformed()
        {
            PagingTokenModel<int> testToken = "someoldstring";
            var domainTokenOption = testToken.ToDomainPagingToken();
            domainTokenOption.Should().BeAssignableTo<None>();
        }

        [Test]
        public void ToDomainPagingToken_ShouldReturnOptionSome_WhenPagingTokenIsValid()
        {
            var testToken = new PagingTokenModel<int>
            {
                DataType = "string",
                ParametersHashCode = "1234567890",
                PreviousIdValue = 0,
                PreviousSortColumnValue = "somecolumn",
            };
            var domainTokenOption = testToken.ToDomainPagingToken();
            (domainTokenOption is Some).Should().BeTrue();
        }

        [Test]
        public void ToDomainPagingToken_ShouldReturnOptionSomeWithMatchingData_WhenPagingTokenIsValid()
        {
            var testToken = new PagingTokenModel<int>
            {
                DataType = "string",
                ParametersHashCode = "",
                PreviousIdValue = 0,
                PreviousSortColumnValue = "somecolumn",
            };

            var domainToken = testToken.ToDomainPagingToken();
            var resultToken = ((Some<PagingToken<int>>)domainToken).Value;

            resultToken.DataType.Should().Be("string");
            resultToken.ParametersHashCode.Should().Be("");
            resultToken.PreviousIdValue.Should().Be(0);
            resultToken.PreviousSortColumnValue.Should().Be("somecolumn");
        }
    }
}
