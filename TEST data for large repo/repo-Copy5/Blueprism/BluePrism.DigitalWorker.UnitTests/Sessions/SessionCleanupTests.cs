using System;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateProcessCore;
using BluePrism.DigitalWorker.Sessions;
using BluePrism.Utilities.Testing;
using NUnit.Framework;

namespace BluePrism.DigitalWorker.UnitTests.Sessions
{
    public class SessionCleanupTests : UnitTestBase<SessionCleanup>
    {
        [Test]
        public void OnSessionEnded_ShouldCleanEnvLocks()
        {
            var identifier = new DigitalWorkerSessionIdentifier(Guid.NewGuid());
            ClassUnderTest.OnSessionEnded(identifier);

            GetMock<IServer>().Verify(x => x.ReleaseEnvLocksForSession(identifier));
        }
    }
}