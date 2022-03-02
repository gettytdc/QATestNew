namespace BluePrism.Api.UnitTests.Mappers
{
    using Api.Mappers;
    using Domain;
    using FluentAssertions;
    using Models;
    using NUnit.Framework;

    [TestFixture]
    public class ScheduledTaskMapperTests
    {
        [Test]
        public void ToModelObject_ShouldReturnCorrectlyMappedModel_WhenCalled()
        {
            var scheduledTask = new ScheduledTask
            {
                Id = 1,
                Name = "name",
                Description = "desc",
                DelayAfterEnd = 1,
                FailFastOnError = true,
                OnSuccessTaskId = 1,
                OnSuccessTaskName = "name",
                OnFailureTaskId = 1,
                OnFailureTaskName = "name"
            };

            var result = scheduledTask.ToModelObject();
            var expectedModel = new ScheduledTaskModel
            {
                Id = 1,
                Name = "name",
                Description = "desc",
                DelayAfterEnd = 1,
                FailFastOnError = true,
                OnSuccessTaskId = 1,
                OnSuccessTaskName = "name",
                OnFailureTaskId = 1,
                OnFailureTaskName = "name"
            };
            result.ShouldBeEquivalentTo(expectedModel);
        }
    }
}
