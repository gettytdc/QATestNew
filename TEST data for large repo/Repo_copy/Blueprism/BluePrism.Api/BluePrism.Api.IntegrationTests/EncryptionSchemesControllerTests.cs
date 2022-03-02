namespace BluePrism.Api.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Autofac;
    using AutomateAppCore;
    using AutomateAppCore.Auth;
    using BpLibAdapters;
    using ControllerClients;
    using FluentAssertions;
    using Models;
    using Moq;
    using NUnit.Framework;
    using EncryptionAlgorithm = Models.EncryptionAlgorithm;
    using EncryptionKeyLocation = Models.EncryptionKeyLocation;
    using BpLibEncryptionAlgorithm = AutomateAppCore.EncryptionAlgorithm;
    using BpLibEncryptionKeyLocation = AutomateAppCore.EncryptionKeyLocation;

    [TestFixture]
    public class EncryptionSchemesControllerTests : ControllerTestBase<EncryptionSchemesControllerClient>
    {
        [SetUp]
        public override void Setup() =>
            Setup(() =>
            {
                GetMock<IServer>()
                    .Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReloginTokenRequest>()))
                    .Returns(new LoginResultWithReloginToken(LoginResultCode.Success));

                GetMock<IBluePrismServerFactory>()
                    .Setup(m => m.ClientInit())
                    .Returns(() => GetMock<IServer>().Object);

                RegisterMocks(builder =>
                {
                    builder.Register(_ => GetMock<IBluePrismServerFactory>().Object);
                    return builder;
                });
            });

        [Test]
        public async Task GetEncryptionSchemes_ShouldReturnDefinedValues_WhenSuccessful()
        {
            EncryptionSchemeModel GetModelWithName(string name) =>
                new EncryptionSchemeModel
                {
                    Name = name, Algorithm = EncryptionAlgorithm.None, KeyLocation = EncryptionKeyLocation.Database, IsAvailable = true,
                };

            clsEncryptionScheme GetClsSchemeWithName(string name) =>
                new clsEncryptionScheme
                {
                    Name = name, Algorithm = BpLibEncryptionAlgorithm.None, KeyLocation = BpLibEncryptionKeyLocation.Database, IsAvailable = true,
                };

            GetMock<IServer>()
                .Setup(m => m.GetEncryptionSchemes())
                .Returns(new[] { GetClsSchemeWithName("one"), GetClsSchemeWithName("two"), GetClsSchemeWithName("three") });

            var resultSchemes = new[] { GetModelWithName("one"), GetModelWithName("two"), GetModelWithName("three") };

            var httpResponse = await Subject.GetEncryptionSchemes();
            var result = await httpResponse.Content.ReadAsAsync<List<EncryptionSchemeModel>>();

            result.Should().BeEquivalentTo<EncryptionSchemeModel>(resultSchemes);
        }

        [Test]
        public async Task GetEncryptionSchemes_ShouldReturnHttpStatusCodeUnauthorized_WhenBluePrismLoginFailed()
        {
            GetMock<IServer>()
                .Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReloginTokenRequest>()))
                .Returns(new LoginResultWithReloginToken(LoginResultCode.UnableToFindUser));

            var result = await Subject.GetEncryptionSchemes();

            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetEncryptionSchemes_ShouldReturnHttpStatusOk_WhenSuccessful()
        {
            GetMock<IServer>()
                .Setup(m => m.GetEncryptionSchemes())
                .Returns(new List<clsEncryptionScheme>());

            var result = await Subject.GetEncryptionSchemes();

            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetEncryptionSchemes_ShouldReturnHttpStatusInternalServerError_WhenFailed()
        {
            GetMock<IServer>()
                .Setup(x => x.GetEncryptionSchemesExcludingKey())
                .Throws(new InvalidOperationException("ServerError"));

            var result = await Subject.GetEncryptionSchemes();

            result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }
    }
}
