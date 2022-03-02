namespace BluePrism.Api.IntegrationTests
{
    using Domain;
    using FluentAssertions;
    using Newtonsoft.Json;
    using NUnit.Framework;

    [TestFixture]
    public class ItemsPerPageConverterTests  
    {
        [Test]
        public void Serialize_ShouldWriteItemsPerPageValue_WhenGivenItemsPerPageWithValue()
        {
            var expectedItemsPerPageValue = 5;
            ItemsPerPage itemsPerPage = expectedItemsPerPageValue;

            var scheduleParameters = new ScheduleParameters {ItemsPerPage = itemsPerPage};

            var result = JsonConvert.SerializeObject(scheduleParameters);

            var expectedJson = $"\"ItemsPerPage\":{expectedItemsPerPageValue}";
            result.Should().Contain(expectedJson);
        }
    }
}

