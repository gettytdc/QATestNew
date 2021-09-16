#if UNITTESTS
namespace BluePrism.Core.UnitTests.Logging
{
    using BluePrism.Core.Logging;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class LogOutputHelperTests
    {
        [Test]
        [TestCase("This message is longer than 12 characters.", "This message", 12)]
        [TestCase("This message is shorter than 200 characters.", "This message is shorter than 200 characters.", 200)]
        [TestCase("MCjdds4jhtqttqmTQgnRFaQUSGmPgHbiyn1Le9uaGUEO5WSIhmN01Jydph3aZtAuXy3pq8FxjcrLhr9iuzl6Rzhsu412t6qa6943AyRoWgCmxO4lYxcLiW2wssjrFFCZ54GNzyajWsBnnpbQwLqLgW86MELs6lsbp7CfCPpZiB6PD4tnZHFDvoBa9ZOKFGcae9naz39kdKlRmLcg3rCpoQRKt3Y0D8mE0VfTdA9HTymoaDzo3FcDDNkahyjnqglUacqcFpPkwaQeDHFxc6sWXvjtoNhajNBOqa94hDZ5gHuXZmfEw9yUX2kvmxe7wEdIH9IWh01yFiGhkIXAL7uIJLLGCe95kiY8zYuRAVAfMlJVxnVAuAioAoDECYcTB5Ej6UHTKJrOWf0IMVzgS",
            "MCjdds4jhtqttqmTQgnRFaQUSGmPgHbiyn1Le9uaGUEO5WSIhmN01Jydph3aZtAuXy3pq8FxjcrLhr9iuzl6Rzhsu412t6qa6943AyRoWgCmxO4lYxcLiW2wssjrFFCZ54GNzyajWsBnnpbQwLqLgW86MELs6lsbp7CfCPpZiB6PD4tnZHFDvoBa9ZOKFGcae9naz39kdKlRmLcg3rCpoQRKt3Y0D8mE0VfTdA9HTymoaDzo3FcDDNkahyjnqglUacqcFpPkwaQeDHFxc6sWXvjtoNhajNBOqa94hDZ5gHuXZmfEw9yUX2kvmxe7wEdIH9IWh01yFiGhkIXAL7uIJLLGCe95kiY8zYuRAVAfMlJVxnVAuAioAoDECYcTB5Ej6UHTKJrOWf0IMVzg", 400)]
        public void Sanitize_WithLengthLimit_ShouldLimitLength(string messageToSanitize, string expectedResult, int messageLimit)
        {
            var result = LogOutputHelper.Sanitize(messageToSanitize, messageLimit);
            result.ToString().Should().Be(expectedResult);
        }

        [Test]
        [TestCase("This message contains\r\n line feeds", "This message contains line feeds")]
        [TestCase("This message contains\r\n line\r feeds\n", "This message contains line feeds")]
        public void Sanitize_ShouldStripWhitespaces(string messageToSanitize, string expectedResult)
        {
            var objectResult = LogOutputHelper.Sanitize(messageToSanitize);
            var stringResult = objectResult.ToString();

            stringResult.Should().Be(expectedResult);
        }
    }
}

#endif