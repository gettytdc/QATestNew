using BluePrism.AutomateAppCore;
using NUnit.Framework;
using BluePrism.Server.Domain.Models;

namespace AutomateAppCore.UnitTests.Process
{
    [TestFixture]
    public class ProcessSessionTests
    {
        [Test]
        public void StatusText_Pending_ReturnsPending()
        {
            var processSession = new clsProcessSession();
            processSession.Status = SessionStatus.Pending;
            var statusText = processSession.StatusText;
            Assert.That(statusText, Is.EqualTo("Pending"));
        }

        [Test]
        public void StatusText_Stalled_ReturnsWarning()
        {
            var processSession = new clsProcessSession();
            processSession.Status = SessionStatus.Stalled;
            var statusText = processSession.StatusText;
            Assert.That(statusText, Is.EqualTo("Warning"));
        }

        [Test]
        public void StatusText_StopRequested_ReturnsStopping()
        {
            var processSession = new clsProcessSession();
            processSession.Status = SessionStatus.StopRequested;
            var statusText = processSession.StatusText;
            Assert.That(statusText, Is.EqualTo("Stopping"));
        }
    }
}
