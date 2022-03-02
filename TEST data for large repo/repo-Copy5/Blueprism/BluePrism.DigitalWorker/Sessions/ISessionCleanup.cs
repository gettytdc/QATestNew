using BluePrism.AutomateProcessCore;

namespace BluePrism.DigitalWorker.Sessions
{
    public interface ISessionCleanup
    {
        void OnSessionEnded(SessionIdentifier identifier);
    }
}