namespace BluePrism.Api.BpLibAdapters.UnitTests
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Func;
    using NUnit.Framework;

    using static ServerResultTask;
    using static Func.ResultHelper;

    [TestFixture]
    public class ServerResultTests
    {
        [Test]
        public async Task ServerResult_OnConversion_ExecutesMethod()
        {
            var hasRun = false;
            await (Task<Result>)RunOnServer(() => { hasRun = true; });

            hasRun.Should().BeTrue();
        }

        [Test]
        public async Task ServerResult_WithNoException_ReturnsSuccess()
        {
            var result = await (Task<Result>)RunOnServer(() => { });

            (result is Success).Should().BeTrue();
        }

        [Test]
        public void ServerResult_WithUncaughtException_ThrowsException()
        {
            Func<Task<Result>> test = () => RunOnServer(() => throw new InvalidOperationException());

            test.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public async Task ServerResult_WithCaughtException_ReturnsCatchResult()
        {
            var result = await (Task<Result>)
                RunOnServer(() => throw new InvalidOperationException())
                .Catch<InvalidOperationException>(_ => Fail<TestError>());

            (result is Failure<TestError>).Should().BeTrue();
        }

        [Test]
        public async Task GenericServerResult_OnConversion_ExecutesMethod()
        {
            var hasRun = false;
            await (Task<Result<bool>>)RunOnServer(() => hasRun = true);

            hasRun.Should().BeTrue();
        }

        [Test]
        public async Task GenericServerResult_WithNoException_ReturnsSuccess()
        {
            var result = await (Task<Result<int>>)RunOnServer(() => 123);

            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task GenericServerResult_WithNoException_ReturnsValue()
        {
            var result = await (Task<Result<int>>)RunOnServer(() => 123);

            ((Success<int>)result).Value.Should().Be(123);
        }

        [Test]
        public void GenericServerResult_WithUncaughtException_ThrowsException()
        {
            Func<Task<Result<int>>> test = () => RunOnServer<int>(() => throw new InvalidOperationException());

            test.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public async Task GenericServerResult_WithNullValueAndNoNullHandler_ReturnsValue()
        {
            var result = await (Task<Result<string>>)RunOnServer<string>(() => null);

            ((Success<string>)result).Value.Should().BeNull();
        }

        [Test]
        public async Task GenericServerResult_WithNullValueAndNullHandler_ReturnsValueFromHandler()
        {
            var result = await (Task<Result<string>>)
                RunOnServer<string>(() => null)
                .OnNull(() => Succeed("Null"));

            ((Success<string>)result).Value.Should().Be("Null");
        }

        [Test]
        public async Task GenericServerResult_WithCaughtException_ReturnsCatchResult()
        {
            var result = await (Task<Result<int>>)
                RunOnServer<int>(() => throw new InvalidOperationException())
                .Catch<InvalidOperationException>(_ => ResultHelper<int>.Fail<TestError>());

            (result is Failure<TestError>).Should().BeTrue();
        }

        private class TestError : ResultError { }
    }
}
