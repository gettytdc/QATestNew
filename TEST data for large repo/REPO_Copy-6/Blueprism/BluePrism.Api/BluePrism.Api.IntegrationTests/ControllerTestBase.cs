namespace BluePrism.Api.IntegrationTests
{
    using System;
    using System.Net.Http;
    using Autofac;
    using AutomateAppCore;
    using AutomateAppCore.Auth;
    using BPCoreLib.DependencyInjection;
    using CommonTestClasses;
    using ControllerClients;
    using Domain;
    using Func.AspNet;
    using Logging;
    using Microsoft.Owin.Testing;
    using Moq;
    using NLog;
    using NLog.Config;
    using NLog.Targets;
    using UnitTesting.TestSupport;
    using Utilities.Testing;

    public abstract class ControllerTestBase<TControllerClient> : IntegrationTestBase
        where TControllerClient : ControllerClientBase
    {
        protected const string TestIssuer = "https://issuer.com";
        protected const string TestAudience = "some-audience";

        private TestServer _testWebServer;
        private HttpClient _client;

        private Lazy<TControllerClient> _subject;
        protected TControllerClient Subject => _subject.Value;

        public void Setup(Action setupMocks)
        {
            DependencyResolver.SetContainer(null);
            base.Setup();

            setupMocks();

            LogManager.Configuration = new LoggingConfiguration
            {
                LoggingRules =
                {
                    new LoggingRule("test", NLog.LogLevel.Trace, new DebuggerTarget())
                },
            };

            ReflectionHelper.SetPrivateField(typeof(Options), "mMachineConfig", Options.Instance, new MachineConfig(GetMock<IConfigLocator>().Object));

            GetMock<IServer>()
                .Setup(m => m.GetUser(It.IsAny<Guid>()))
                .Returns(new User(Server.Domain.Models.AuthMode.Native, Guid.NewGuid(), "TestUser"));
            GetMock<IServer>()
                .Setup(m => m.GetAuthenticationServerUrl())
                .Returns(TestIssuer);

            _subject = new Lazy<TControllerClient>(() =>
                (TControllerClient)Activator.CreateInstance(
                    typeof(TControllerClient),
                    StartTestServerAndReturnClient(),
                    new OpenIdConfiguration{Authority = TestIssuer, Audience = TestAudience}));
        }

        public override void TearDown()
        {
            _client?.Dispose();
            _testWebServer?.Dispose();
        }

        protected void RegisterMocks(Func<ContainerBuilder, ContainerBuilder> registerMocks) =>
            ContainerConfigurator.AdditionalConfiguration = builder =>
            {
                registerMocks(builder);
                builder.RegisterInstance(Mock);
                builder.RegisterGeneric(typeof(MockLogger<>)).As(typeof(ILogger<>)).SingleInstance();

                builder.RegisterType<ApiExceptionResponseConverter>().As<IExceptionResponseConverter>();
                builder.RegisterInstance(new MockSigningKeyResolver()).As<ISigningKeyResolver>();
                builder.RegisterInstance(new OpenIdConfiguration
                {
                    Audience = TestAudience,
                    Authority = TestIssuer
                }).AsSelf();
                return builder;
            };

        private HttpClient StartTestServerAndReturnClient()
        {
            _testWebServer = TestServer.Create<Startup>();
            return _client = new HttpClient(_testWebServer.Handler) { BaseAddress = new Uri("http://testserver") };
        }
    }
}
