using System;
using BluePrism.AutomateAppCore;
using BluePrism.Core.Utility;
using BluePrism.DigitalWorker.Messages.Events;
using BluePrism.DigitalWorker.Messages.Events.Factory;
using BluePrism.DigitalWorker.Messaging;

namespace BluePrism.DigitalWorker.Sessions
{
    public class SessionStatusPublisher : ISessionStatusPublisher
    {
        private readonly IMessageBusWrapper _bus;
        private readonly ISystemClock _clock;
        private readonly Guid _sessionId;

        public SessionStatusPublisher(IMessageBusWrapper bus, ISystemClock clock, SessionContext sessionContext)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _sessionId = sessionContext.SessionId;
        }

        public void SetPendingSessionRunning(DateTimeOffset sessionStarted)
        {
            _bus.Publish(DigitalWorkerEvents.ProcessStarted(_sessionId, sessionStarted)).Wait();
        }

        public void SetSessionTerminated(SessionExceptionDetail sessionExceptionDetail)
        {
            _bus.Publish(DigitalWorkerEvents.ProcessTerminated(_sessionId, _clock.Now, (TerminationReason)Enum.Parse(typeof(TerminationReason),
                sessionExceptionDetail?.TerminationReason.ToString()),
                sessionExceptionDetail?.ExceptionType,
                sessionExceptionDetail?.ExceptionMessage)).Wait();
        }


        public void SetSessionStopped()
        {
            _bus.Publish(DigitalWorkerEvents.ProcessStopped(_sessionId, _clock.Now)).Wait();
        }

        public void SetSessionCompleted()
        {
            _bus.Publish(DigitalWorkerEvents.ProcessCompleted(_sessionId, _clock.Now)).Wait();
        }
    }
}
