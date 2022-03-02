using System;
using System.Linq;
using System.ServiceModel;
using BluePrism.ClientServerResources.Core.Data;
using BluePrism.ClientServerResources.Core.Events;

namespace BluePrism.ClientServerResources.Wcf.Endpoints
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class NotificationServiceCallBack : INotificationServiceCallBack
    {
        public event EventHandler Message;
        public event EventHandler<ResourcesChangedEventArgs> ResourceStatus;
        public event EventHandler<SessionCreateEventArgs> SessionCreated;
        public event EventHandler<SessionDeleteEventArgs> SessionDeleted;
        public event EventHandler<SessionEndEventArgs> SessionEnd;
        public event EventHandler<SessionStartEventArgs> SessionStarted;
        public event EventHandler<SessionStopEventArgs> SessionStop;
        public event EventHandler<SessionVariableUpdatedEventArgs> SessionVariableUpdated;

        public void OnMessage()
        {
            Message?.Invoke(this, new EventArgs());
        }

        public void OnResourceStatus(ResourcesChangedData resourcesChangedData)
        {
            if (resourcesChangedData == null) { throw new ArgumentException(nameof(resourcesChangedData));}
            ResourceStatus?.Invoke(this, new ResourcesChangedEventArgs(resourcesChangedData.OverallChange, resourcesChangedData.Changes));
        }

        public void OnSessionCreated(SessionCreatedData sessionCreatedData)
        {
            if (sessionCreatedData == null) { throw new ArgumentException(nameof(sessionCreatedData)); }

            var sessionInfo = sessionCreatedData.Data?.ToDictionary
                (x => Guid.Parse(x.Key),
                y => y.Value);

            SessionCreated?.Invoke(this, new SessionCreateEventArgs(
                createState: sessionCreatedData.State,
                processId: sessionCreatedData.ProcessId,
                resourceId: sessionCreatedData.ResourceId,
                sessionId: sessionCreatedData.SessionId,
                schedSessId: sessionCreatedData.ScheduledSessionId,
                errMsg: sessionCreatedData.ErrorMessage,
                data: sessionInfo,
                userId: sessionCreatedData.UserId,
                tag: sessionCreatedData.Tag));
        }

        public void OnSessionDeleted(SessionDeletedData sessionDeletedData)
        {
            if (sessionDeletedData == null) { throw new ArgumentException(nameof(sessionDeletedData)); }
            SessionDeleted?.Invoke(this, new SessionDeleteEventArgs(sessionDeletedData.SessionId, sessionDeletedData.ErrorMessage, sessionDeletedData.UserId, sessionDeletedData.ScheduleId));
        }

        public void OnSessionEnd(SessionEndData sessionEndData)
        {
            if (sessionEndData == null) { throw new ArgumentException(nameof(sessionEndData)); }
            SessionEnd?.Invoke(this, new SessionEndEventArgs(sessionEndData.SessionId, sessionEndData.Status));
        }

        public void OnSessionStarted(SessionStartedData sessionStartedData)
        {
            if (sessionStartedData == null) { throw new ArgumentException(nameof(sessionStartedData)); }
            SessionStarted?.Invoke(this, new SessionStartEventArgs(sessid: sessionStartedData.SessionId, errmsg: sessionStartedData.ErrorMessage, userid: sessionStartedData.UserId, usermsg: sessionStartedData.UserMessage, schedId: sessionStartedData.SchedulerID));
        }

        public void OnSessionStop(SessionStopData sessionStopData)
        {
            if (sessionStopData == null)
            { throw new ArgumentException(nameof(sessionStopData)); }
            SessionStop?.Invoke(this, new SessionStopEventArgs(sessionStopData.SessionId, sessionStopData.ErrorMessage, sessionStopData.ScheduleId));
        }

        public void OnSessionVariableUpdated(SessionVariablesUpdatedData sessionVariablesUpdatedData)
        {
            if (sessionVariablesUpdatedData == null)
            { throw new ArgumentException(nameof(sessionVariablesUpdatedData)); }
            SessionVariableUpdated?.Invoke(this, new SessionVariableUpdatedEventArgs(sessionVariablesUpdatedData.JSONData, sessionVariablesUpdatedData.ErrorMessage));
        }
    }
}
