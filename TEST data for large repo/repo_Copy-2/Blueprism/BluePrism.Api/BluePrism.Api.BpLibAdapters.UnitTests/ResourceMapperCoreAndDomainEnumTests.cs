namespace BluePrism.Api.BpLibAdapters.UnitTests
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class ResourceMapperCoreAndDomainEnumTests
    {
        [TestCase(typeof(AutomateAppCore.Resources.ResourceMachine.ResourceDBStatus), typeof(Domain.ResourceDbStatus))]
        [TestCase(typeof(BluePrism.Server.Domain.Models.WorkQueueSortByProperty), typeof(Domain.WorkQueueSortByProperty))]
        [TestCase(typeof(BluePrism.Server.Domain.Models.WorkQueueItemSortByProperty), typeof(Domain.WorkQueueItemSortByProperty))]
        [TestCase(typeof(BluePrism.Server.Domain.Models.ItemStatus), typeof(Domain.ScheduleLogStatus))]
        public void ResourceEnums_BluePrismCoreTypes_ShouldBeEquivalentToDomainTypes(Type bluePrismCoreEnumType, Type domainEnumType)
        {
            var (bluePrismCoreEnumNames, domainEnumNames) = (Enum.GetNames(bluePrismCoreEnumType), Enum.GetNames(domainEnumType));

            bluePrismCoreEnumNames.ShouldAllBeEquivalentTo(domainEnumNames);
        }

        [Test]
        public void ResourceAttributeEnum_BluePrismCoreTypes_ShouldBeEquivalentToDomainTypes()
        {
            var (bluePrismCoreEnumNames, domainEnumNames) = (Enum.GetNames(typeof(Core.Resources.ResourceAttribute)), Enum.GetNames(typeof(Domain.ResourceAttribute)));

            bluePrismCoreEnumNames.Where(x => x != "Pool" && x != "Debug").ShouldAllBeEquivalentTo(domainEnumNames);
        }

        [Test]
        public void ResourceStatusEnum_BluePrismCoreTypes_ShouldBeEquivalentToDomainTypes()
        {
            var (bluePrismCoreEnumNames, domainEnumNames) = (Enum.GetNames(typeof(AutomateAppCore.ResourceStatus)), Enum.GetNames(typeof(Domain.ResourceDisplayStatus)));

            bluePrismCoreEnumNames.Where(x => x != "Pool").ShouldAllBeEquivalentTo(domainEnumNames);
        }

    }
}
