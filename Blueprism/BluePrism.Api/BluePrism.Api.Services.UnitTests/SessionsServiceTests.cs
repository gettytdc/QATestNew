namespace BluePrism.Api.Services.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Autofac;
    using BpLibAdapters;
    using Domain;
    using Domain.Errors;
    using Utilities.Testing;
    using FluentAssertions;
    using Func;
    using Moq;
    using NUnit.Framework;

    using static Func.ResultHelper;

    [TestFixture]
    public class SessionsServiceTests : UnitTestBase<SessionsService>
    {
        public override void Setup() =>
            base.Setup(builder =>
            {
                builder.RegisterGeneric(typeof(MockAdapterAuthenticatedMethodRunner<>)).As(typeof(IAdapterAuthenticatedMethodRunner<>));
            });

        [Test]
        public async Task GetSessions_ShouldReturnSuccess_WhenSuccessful()
        {
            var sessions = new ItemsPage<Session>
            {
                Items = new List<Session> { new Session { SessionNumber = 1 } }
            };

            GetMock<ISessionServerAdapter>()
                .Setup(x => x.GetSessions(It.IsAny<SessionParameters>()))
                .ReturnsAsync(Succeed<ItemsPage<Session>>(sessions));

            var result = await ClassUnderTest.GetSessions(new SessionParameters());

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task GetSessions_ShouldReturnSuccess_WhenNoSessions()
        {
            GetMock<ISessionServerAdapter>()
                .Setup(x => x.GetSessions(It.IsAny<SessionParameters>()))
                .ReturnsAsync(Succeed<ItemsPage<Session>>(new ItemsPage<Session>()));

            var result = await ClassUnderTest.GetSessions(new SessionParameters());

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task GetSessions_ShouldReturnPermissionsError_WhenUserDoesNotHavePermissions()
        {
            GetMock<ISessionServerAdapter>()
                .Setup(x => x.GetSessions(It.IsAny<SessionParameters>()))
                .ReturnsAsync(ResultHelper<ItemsPage<Session>>.Fail(new PermissionError("")));

            var result = await ClassUnderTest.GetSessions(new SessionParameters());

            (result is Failure<PermissionError>).Should().BeTrue();
        }

        [Test]
        public async Task GetSessionById_ShouldReturnSession_WhenSuccessful()
        {
            var sessionId = Guid.NewGuid();

            GetMock<ISessionServerAdapter>()
                .Setup(x => x.GetActualSessionById(sessionId))
                .ReturnsAsync(Succeed(new Session { SessionId = sessionId }));

            var result = await ClassUnderTest.GetSessionById(sessionId);

            ((Success<Session>)result).Value.SessionId.Should().Be(sessionId);
        }

        [Test]
        public async Task GetSessionById_ShouldReturnPermissionsError_WhenUserDoesNotHavePermissions()
        {
            GetMock<ISessionServerAdapter>()
                .Setup(x => x.GetActualSessionById(It.IsAny<Guid>()))
                .ReturnsAsync(ResultHelper<Session>.Fail(new PermissionError("")));

            var result = await ClassUnderTest.GetSessionById(Guid.Empty);

            (result is Failure<PermissionError>).Should().BeTrue();
        }

        [Test]
        public async Task GetSessionById_ShouldReturnSessionNotFoundError_WhenSessionNotFound()
        {
            var sessionId = Guid.NewGuid();

            GetMock<ISessionServerAdapter>()
                .Setup(x => x.GetActualSessionById(sessionId))
                .ReturnsAsync(ResultHelper<Session>.Fail(new SessionNotFoundError()));

            var result = await ClassUnderTest.GetSessionById(sessionId);

            (result is Failure<SessionNotFoundError>).Should().BeTrue();
        }
    }
}
