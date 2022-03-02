namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers
{
    using BpLibAdapters.Mappers;
    using FluentAssertions;
    using NUnit.Framework;
    using ScheduledTask = Domain.ScheduledTask;

    [TestFixture(Category = "Unit Test")]
    public class ScheduleTaskDataMapperTests
    {
        [Test]
        public void ToDomainModel_ShouldReturnCorrectlyMappedData()
        {
            var serverModel = new Server.Domain.Models.ScheduledTask
            {
                Name = "name",
                Description = "desc",
                DelayAfterEnd = 1,
                FailFastOnError = true,
                OnSuccessTaskName = "name",
                OnSuccessTaskId = 1,
                OnFailureTaskName = "name",
                OnFailureTaskId = 1

            };
            var domainModel = new ScheduledTask
            {
                Name = "name",
                Description = "desc",
                DelayAfterEnd = 1,
                FailFastOnError = true,
                OnSuccessTaskName = "name",
                OnSuccessTaskId = 1,
                OnFailureTaskName = "name",
                OnFailureTaskId = 1
            };

            var result = serverModel.ToDomainModel();
            result.Name.Should().Be(domainModel.Name);
            result.Description.Should().Be(domainModel.Description);
            result.DelayAfterEnd.Should().Be(domainModel.DelayAfterEnd);
            result.FailFastOnError.Should().Be(domainModel.FailFastOnError);
            result.OnSuccessTaskName.Should().Be(domainModel.OnSuccessTaskName);
            result.OnSuccessTaskId.Should().Be(domainModel.OnSuccessTaskId);
            result.OnFailureTaskName.Should().Be(domainModel.OnFailureTaskName);
            result.OnFailureTaskId.Should().Be(domainModel.OnFailureTaskId);
        }
    }
}
