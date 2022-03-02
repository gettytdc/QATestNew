using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Resources;
using BluePrism.Core.Utility;
using BluePrism.DigitalWorker.Notifications;
using System;

namespace BluePrism.DigitalWorker.Sessions
{
    public class RunningSessionMonitor : ISessionNotifier
    {
        private readonly INotificationHandler _notificationHandler;
        private readonly ISystemClock _systemClock;

        public RunningSessionMonitor(INotificationHandler notificationHandler, ISystemClock systemClock)
        {
            _notificationHandler = notificationHandler ?? throw new ArgumentNullException(nameof(notificationHandler));
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        }
        
        private void RaiseNotify(ResourceNotificationLevel level, string message)
            => _notificationHandler.HandleNotification(new ResourceNotification(level, message, _systemClock.Now.LocalDateTime));

        public void HandleSessionStatusFailure(RunnerRecord record, string errmsg)
            => RaiseError($"Session failed ({(record as DigitalWorkerRunnerRecord)?.Context.SessionId}): {errmsg}");

        public void RaiseError(string message)
            => RaiseNotify(ResourceNotificationLevel.Error, message);

        public void RaiseError(string message, params object[] args)
            => RaiseError(string.Format(message, args));

        public void RaiseInfo(string message)
            => RaiseNotify(ResourceNotificationLevel.Comment, message);

        public void RaiseInfo(string message, params object[] args)
            => RaiseInfo(string.Format(message, args));

        public void RaiseWarn(string message)
            => RaiseNotify(ResourceNotificationLevel.Warning, message);

        public void RaiseWarn(string message, params object[] args)
            => RaiseWarn(string.Format(message, args));

        public void AddNotification(string message)
            => RaiseInfo(message);

        public void AddNotification(string message, params object[] args)
            => RaiseInfo(string.Format(message, args));

        public void NotifyStatus()
            => RaiseNotify(ResourceNotificationLevel.Verbose, "Status update");

        public void VarChanged(clsSessionVariable var)
            => RaiseNotify(ResourceNotificationLevel.Verbose, "Var changed");
    }
}
