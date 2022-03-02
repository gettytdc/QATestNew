namespace BluePrism.Api.UnitTests.Mappers
{
    using NUnit.Framework;
    using Models;
    using BluePrism.Api.Mappers;
    using FluentAssertions;

    [TestFixture]
    public class ResourceMapperTests
    {
        [TestCase(ResourceAttribute.None, Domain.ResourceAttribute.None)]
        [TestCase(ResourceAttribute.Retired, Domain.ResourceAttribute.Retired)]
        [TestCase(ResourceAttribute.DefaultInstance, Domain.ResourceAttribute.DefaultInstance)]
        [TestCase(ResourceAttribute.Local, Domain.ResourceAttribute.Local)]
        [TestCase(ResourceAttribute.LoginAgent, Domain.ResourceAttribute.LoginAgent)]
        [TestCase(ResourceAttribute.Private, Domain.ResourceAttribute.Private)]
        public void UpdateResourceModel_ToDomain_ShouldReturnCorrectlyMappedResourceAttributes(ResourceAttribute sourceResourceAttribute, Domain.ResourceAttribute expectedResourceAttribute)
        {
            var model = new UpdateResourceModel { Attributes = new[] {sourceResourceAttribute} };

            model.ToDomain().Attributes.Should().Be(expectedResourceAttribute);
        }
    }
}
