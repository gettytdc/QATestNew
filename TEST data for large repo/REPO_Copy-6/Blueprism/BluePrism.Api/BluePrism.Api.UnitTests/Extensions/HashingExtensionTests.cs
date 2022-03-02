namespace BluePrism.Api.UnitTests.Extensions
{
    using Domain;
    using Domain.Filters;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class HashingExtensionTests 
    {
        [Test]
        public void ToHmacSha256_ShouldReturnSameHash_WhenProvidedTheSameModel()
        {
            var workQueueItemParameters = new WorkQueueItemParameters();

            var result1 = workQueueItemParameters.GetHashCodeForValidation();
            var result2 = workQueueItemParameters.GetHashCodeForValidation();

            result1.Should().Match(result2);
        }

        [Test]
        public void ToHmacSha256_ShouldReturnDifferentHash_WhenProvidedDifferentModels()
        {
            var workQueueItemParameters1 = new WorkQueueItemParameters()
            {
                Status = new EqualsFilter<string>("testing")
            };
            var workQueueItemParameters2 = new WorkQueueItemParameters()
            {
                ExceptionReason = new EqualsFilter<string>("Broken")
            };

            var result1 = workQueueItemParameters1.GetHashCodeForValidation();
            var result2 = workQueueItemParameters2.GetHashCodeForValidation();

            result1.Should().NotMatch(result2);
        }
    }
}
