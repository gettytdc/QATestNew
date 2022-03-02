namespace BluePrism.Api.Services.UnitTests
{
    using FluentAssertions;
    using NUnit.Framework;
    using Utilities.Testing;

    [TestFixture(Category = "Unit Test")]
    public class CallTokenAccessorTests : UnitTestBase<CallTokenAccessor>
    {
        [Test]
        public void SetToken_ShouldSetTokenWithoutBearerString()
        {
            ClassUnderTest.SetToken("Bearer token");
            ClassUnderTest.TokenString.Should().Be("token");
        }

        [Test]
        public void SetToken_ShouldSetTokenValue()
        {
            ClassUnderTest.SetToken(null);
            ClassUnderTest.TokenString.Should().Be(string.Empty);
        }
    }
}
