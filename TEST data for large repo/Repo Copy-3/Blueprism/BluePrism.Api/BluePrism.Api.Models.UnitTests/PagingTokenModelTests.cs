namespace BluePrism.Api.Models.UnitTests
{
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class PagingTokenModelTests
    {
        [Test]
        public void PagingTokenModel_ShouldReturnValidToken_WhenImplicitlyConvertedFromValidStringToken()
        {
            var startingToken = new PagingTokenModel<int>
            {
                PreviousIdValue = 123
            };

            PagingTokenModel<int> testToken = startingToken.ToString();

            testToken.ShouldBeEquivalentTo(startingToken);
            testToken.GetPagingTokenState().Should().Be(PagingTokenState.Valid);
        }

        [Test]
        public void PagingTokenModel_ShouldReturnInvalidToken_WhenImplicitlyConvertedFromInvalidStringToken()
        {
            const string invalidTokenString = "78d9asduashdkjasblkd";
            PagingTokenModel<int> testToken = invalidTokenString;

            testToken.GetPagingTokenState().Should().Be(PagingTokenState.Malformed);
        }

        [Test]
        public void PagingTokenModel_ShouldReturnEmptyToken_WhenImplicitlyConvertedFromEmptyStringToken()
        {
            PagingTokenModel<int> testToken = string.Empty;
            testToken.GetPagingTokenState().Should().Be(PagingTokenState.Empty);
        }

        [Test]
        public void PagingTokenModel_ShouldReturnEmptyToken_WhenImplicitlyConvertedFromNullStringToken()
        {
            PagingTokenModel<int> testToken = default(string);
            testToken.GetPagingTokenState().Should().Be(PagingTokenState.Empty);
        }
    }
}
