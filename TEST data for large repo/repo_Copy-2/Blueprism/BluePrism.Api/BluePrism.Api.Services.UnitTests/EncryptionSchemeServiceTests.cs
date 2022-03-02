namespace BluePrism.Api.Services.UnitTests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Autofac;
    using BpLibAdapters;
    using Domain;
    using Domain.Errors;
    using FluentAssertions;
    using Func;
    using Moq;
    using NUnit.Framework;
    using Utilities.Testing;

    using static Func.ResultHelper;

    [TestFixture]
    public class EncryptionSchemeServiceTests : UnitTestBase<EncryptionSchemeService>
    {
        public override void Setup()
        {
            base.Setup(builder =>
            {
                builder.RegisterGeneric(typeof(MockAdapterAuthenticatedMethodRunner<>)).As(typeof(IAdapterAuthenticatedMethodRunner<>));
            });
        }

        [Test]
        public async Task GetEncryptionSchemes_ShouldReturnPredefinedValues_WhenSuccessful()
        {
            var schemes = new[] { new EncryptionScheme {Name = "test1" }, new EncryptionScheme { Name = "test2" } };

            GetMock<IEncryptionSchemeServerAdapter>()
                .Setup(x => x.EncryptionSchemesGetSchemes())
                .ReturnsAsync(Succeed<IEnumerable<EncryptionScheme>>(schemes));

            var result = await ClassUnderTest.GetEncryptionSchemes();

            result.Should().BeAssignableTo<Success>();
            result.OnSuccess(x => x.Should().BeEquivalentTo<EncryptionScheme>(schemes));
        }

        [Test]
        public async Task GetEncryptionSchemes_ShouldReturnFailedResult_WhenFailed()
        {
            GetMock<IEncryptionSchemeServerAdapter>()
                .Setup(x => x.EncryptionSchemesGetSchemes())
                .ReturnsAsync(ResultHelper<IEnumerable<EncryptionScheme>>.Fail(new NotFoundError("")));

            var result = await ClassUnderTest.GetEncryptionSchemes();

            result.Should().BeAssignableTo<Failure>();
        }
    }
}
