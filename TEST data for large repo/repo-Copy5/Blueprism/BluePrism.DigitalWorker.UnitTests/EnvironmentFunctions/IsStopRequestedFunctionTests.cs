using BluePrism.AutomateProcessCore;
using BluePrism.DigitalWorker.EnvironmentFunctions;
using BluePrism.DigitalWorker.Sessions;
using NUnit.Framework;
using FluentAssertions;

namespace BluePrism.DigitalWorker.UnitTests.EnvironmentFunctions
{
    [TestFixture]
    public class IsStopRequestedFunctionTests : EnvironmentFunctionsTestBase<IsStopRequestedFunction>
    {
        protected override IsStopRequestedFunction TestClassConstructor()
            => new IsStopRequestedFunction(TestSessionRegistry);

        [Test]
        public void Evaluate_WhenNoStopRequested_ShouldReturnFalse()
        {
            var isStopRequestedValue = ClassUnderTest.Evaluate(null, Process);

            isStopRequestedValue.Should().Be(new clsProcessValue(false));
        }

        [Test]
        public void Evaluate_WhenStopRequested_ShouldReturnTrue()
        {
            TestDigitalWorkerRunnerRecord.StopRequested = true;

            GetMock<IRunningSessionRegistry>().Setup(_ => _.Get(SessionId)).Returns(TestDigitalWorkerRunnerRecord);

            var isStopRequestedValue = ClassUnderTest.Evaluate(null, Process);

            isStopRequestedValue.Should().Be(new clsProcessValue(true));
        }

        [Test]
        public void Evaluate_WithNoRunnerRecord_ShouldReturnFalse()
        {
            GetMock<IRunningSessionRegistry>().Setup(_ => _.Get(SessionId)).Returns(default(IDigitalWorkerRunnerRecord));

            var isStopRequestedValue = ClassUnderTest.Evaluate(null, Process);

            isStopRequestedValue.Should().Be(new clsProcessValue(false));
        }

        [Test]
        public void Evaluate_SessionIsNull_ShouldReturnFalse()
        {
            Process.Session = null;

            var isStopRequestedValue = ClassUnderTest.Evaluate(null, Process);

            isStopRequestedValue.Should().Be(new clsProcessValue(false));
        }
    }
}