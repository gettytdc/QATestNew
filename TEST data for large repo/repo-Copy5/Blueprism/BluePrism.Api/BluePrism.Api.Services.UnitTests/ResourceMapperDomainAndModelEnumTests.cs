namespace BluePrism.Api.Services.UnitTests
{
    using System;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class ResourceMapperDomainAndModelEnumTests
    {
        [TestCase(typeof(Domain.ResourceAttribute), typeof(Models.ResourceAttribute))]
        [TestCase(typeof(Domain.ResourceDbStatus), typeof(Models.ResourceDbStatus))]
        [TestCase(typeof(Domain.ResourceDisplayStatus), typeof(Models.ResourceDisplayStatus))]
        public void ResourceEnums_ModelTypes_ShouldBeEquivalentToDomainTypes(Type domainEnumType, Type modelEnumType)
        {
            var (modelEnumNames, domainEnumNames) = (Enum.GetNames(domainEnumType), Enum.GetNames(modelEnumType));

            modelEnumNames.ShouldAllBeEquivalentTo(domainEnumNames);
        }
    }
}
