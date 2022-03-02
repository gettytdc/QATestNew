using BluePrism.AutomateAppCore;
using BluePrism.AutomateProcessCore;

namespace BluePrism.DigitalWorker.Sessions
{
    public class SessionCleanup : ISessionCleanup
    {
        private readonly IServer _server;

        public SessionCleanup(IServer server)
        {
            _server = server;
        }

        public void OnSessionEnded(SessionIdentifier identifier)
        {
            ReleaseEnvLocksForSession(identifier);
        }

        private void ReleaseEnvLocksForSession(SessionIdentifier identifier) 
            => _server.ReleaseEnvLocksForSession(identifier);
    }
}