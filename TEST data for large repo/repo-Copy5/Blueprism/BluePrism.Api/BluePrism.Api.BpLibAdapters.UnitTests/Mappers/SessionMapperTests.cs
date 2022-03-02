namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers
{
    using System.Linq;
    using BpLibAdapters.Mappers;
    using CommonTestClasses;
    using FluentAssertions;
    using Func;
    using NUnit.Framework;

    [TestFixture(Category = "Unit Test")]
    public class SessionMapperTests
    {
        [Test]
        public void ToDomainObject_WithTestClsProcessSession_ReturnsCorrectlyMappedResult()
        {
            var clsProcessSession = SessionsHelper.GetTestBluePrismClsProcessSession().First();
            var domainSession = clsProcessSession.ToDomainObject();

            SessionsHelper.ValidateModelsAreEqual(clsProcessSession, domainSession);
        }

        [Test]
        public void ToDomainObject_WhenOptionPropertiesOriginalValueIsNull_ShouldMapDataCorrectly()
        {
            var clsProcessSession = SessionsHelper.GetTestBluePrismClsProcessSession().First();
            clsProcessSession.ExceptionType = null;

            var result = clsProcessSession.ToDomainObject();
            result.ExceptionType.Should().BeAssignableTo<None>();
        }
    }
}
