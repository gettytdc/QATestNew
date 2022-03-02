#if UNITTESTS
using System;
using BluePrism.AutomateAppCore;
using BluePrism.BPCoreLib;
using NUnit.Framework;
using BluePrism.Server.Domain.Models;

namespace AutomateAppCore.UnitTests.Sessions
{
    /// <summary>
    /// Test class for session status dealings
    /// </summary>
    [TestFixture]
    public class SessionStatusTester
    {
        [Test]
        public void TestParse()
        {
            Assert.That(clsEnum<SessionStatus>.Parse("All"), Is.EqualTo(SessionStatus.All));
            Assert.That(clsEnum<SessionStatus>.Parse("Pending"), Is.EqualTo(SessionStatus.Pending));
            Assert.That(clsEnum<SessionStatus>.Parse("Running"), Is.EqualTo(SessionStatus.Running));
            Assert.That(clsEnum<SessionStatus>.Parse("Terminated"), Is.EqualTo(SessionStatus.Terminated));
            Assert.That(clsEnum<SessionStatus>.Parse("Stopped"), Is.EqualTo(SessionStatus.Stopped));
            Assert.That(clsEnum<SessionStatus>.Parse("Completed"), Is.EqualTo(SessionStatus.Completed));
            Assert.That(clsEnum<SessionStatus>.Parse("Debugging"), Is.EqualTo(SessionStatus.Debugging));
            Assert.That(clsEnum<SessionStatus>.Parse("Failed"), Is.EqualTo(SessionStatus.Failed));
        }

        [Test]
        public void TestTerminatedIsPrimaryFailureText()
        {
            var failOne = SessionStatus.Terminated;
            failOne = (SessionStatus)Convert.ToInt32(failOne);
            Assert.That(failOne, Is.EqualTo(SessionStatus.Terminated));
            Assert.That(failOne.ToString(), Is.EqualTo("Terminated"));
            var failTwo = SessionStatus.Failed;
            failTwo = (SessionStatus)Convert.ToInt32(failTwo);
            Assert.That(failTwo, Is.EqualTo(SessionStatus.Terminated));
            Assert.That(failTwo.ToString(), Is.EqualTo("Terminated"));
        }
    }
}
#endif
