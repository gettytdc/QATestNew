using System;
using System.Linq;
using BluePrism.ClientServerResources.Core.Data;
using BluePrism.ClientServerResources.Core.Enums;
using BluePrism.ClientServerResources.Core.Events;
using InstructionalConnection;

namespace BluePrism.ClientServerResources.Grpc
{
    /// <summary>
    /// Class converts objects in BluePrism.ClientServerResources.Core.Data into gRPC complaint files
    /// </summary>
    public static class MessageConverterClass
    {
        public static SessionCreatedDataMessage To(this SessionCreatedData sessionCreatedData)
        {
            if (sessionCreatedData == null)
            {
                throw new ArgumentNullException(nameof(sessionCreatedData));
            }

            var sessionData = new SessionCreatedDataMessage()
            {
                State = (int)sessionCreatedData.State,
                SessionId = sessionCreatedData.SessionId.ToString(),
                ResourceId = sessionCreatedData.ResourceId.ToString(),
                ProcessId = sessionCreatedData.ProcessId.ToString(),
                ErrorMessage = sessionCreatedData.ErrorMessage ?? string.Empty,     //grpc does not like null.
                ScheduledSessionId = sessionCreatedData.ScheduledSessionId,
                UserId = sessionCreatedData.UserId.ToString(),
                Tag = sessionCreatedData.Tag?.ToString() ?? string.Empty
            };

            if(!(sessionCreatedData.Data is null))
            {
                sessionData.Data.AddRange(sessionCreatedData.Data.Select(r => new DictionaryElm()
                {
                    Key = r.Key,
                    ResourceStatus = (int)r.Value
                }));
            }
            return sessionData;
        }

        public static SessionCreateEventArgs ToArgs(this SessionCreatedDataMessage message)
        {
            if (message == null)
            { throw new ArgumentNullException(nameof(message)); }

            var runnerStatusForSessions = message.Data?.ToDictionary(t => t.Key, t => (RunnerStatus)t.ResourceStatus);

            return new SessionCreateEventArgs(
                       createState: (SessionCreateState)message.State,
                       resourceId:  Guid.Parse(message.ResourceId),
                       processId:   Guid.Parse(message.ProcessId),
                       sessionId:   Guid.Parse(message.SessionId),
                       userId:      Guid.Parse(message.UserId),
                       schedSessId: message.ScheduledSessionId,
                       errMsg:      message.ErrorMessage,
                       data:        runnerStatusForSessions?.ToDictionary(
                                       x => Guid.Parse(x.Key),
                                       y => y.Value),
                       tag:         message.Tag
                       );
        }

        public static ResourcesChangedDataMessage To(this ResourcesChangedData resourcesChangedData)
        {
            if (resourcesChangedData == null)
            {
                throw new ArgumentNullException(nameof(resourcesChangedData));
            }
            var response = new ResourcesChangedDataMessage()
            {
                OverallChange = (int)resourcesChangedData.OverallChange,
            };
            response.Changes.AddRange(resourcesChangedData.Changes.Select(r => new DictionaryElm()
            {
                Key = r.Key,
                ResourceStatus = (int)r.Value
            }));
            return response;
        }

        public static ResourcesChangedEventArgs ToArgs(this ResourcesChangedDataMessage resourcesChangedDataMessage)
        {
            var changes = resourcesChangedDataMessage.Changes.ToDictionary(t => t.Key, t => (ResourceStatusChange)t.ResourceStatus);
            return new ResourcesChangedEventArgs((ResourceStatusChange)resourcesChangedDataMessage.OverallChange, changes);
        }

        public static SessionDeletedDataMessage To(this SessionDeletedData sessionDeletedData)
        {
            if (sessionDeletedData == null)
            {
                throw new ArgumentNullException(nameof(sessionDeletedData));
            }
            return new SessionDeletedDataMessage()
            {
                UserId = sessionDeletedData.UserId.ToString(),
                SessId = sessionDeletedData.SessionId.ToString(),
                ErrMsg = sessionDeletedData.ErrorMessage ?? string.Empty,
                ScheduledSessionId = sessionDeletedData.ScheduleId
            };
        }

        public static SessionDeleteEventArgs ToArgs(this SessionDeletedDataMessage sessionDeletedDataMessage)
        {
            return new SessionDeleteEventArgs(Guid.Parse(sessionDeletedDataMessage.SessId), sessionDeletedDataMessage.ErrMsg, Guid.Parse(sessionDeletedDataMessage.UserId), sessionDeletedDataMessage.ScheduledSessionId);
        }

        public static SessionEndDataMessage To(this SessionEndData sessionEndData)
        {
            if (sessionEndData == null)
            {
                throw new ArgumentNullException(nameof(sessionEndData));
            }

            return new SessionEndDataMessage()
            {
                SessId = sessionEndData.SessionId.ToString(),
                Status = sessionEndData.Status ?? string.Empty,
                
                
            };
        }

        public static SessionEndEventArgs ToArgs(this SessionEndDataMessage sessionEndDataMessage)
        {
            return new SessionEndEventArgs(Guid.Parse(sessionEndDataMessage.SessId), sessionEndDataMessage.Status);
        }

        public static SessionStartedDataMessage To(this SessionStartedData sessionStartedDataMessage)
        {
            if (sessionStartedDataMessage == null)
            {
                throw new ArgumentNullException(nameof(sessionStartedDataMessage));
            }

            return new SessionStartedDataMessage()
            {
                UserId = sessionStartedDataMessage.UserId.ToString(),
                ErrMsg = sessionStartedDataMessage.ErrorMessage ?? string.Empty,
                SessId = sessionStartedDataMessage.SessionId.ToString(),
                UserMsg = sessionStartedDataMessage.UserMessage ?? string.Empty,
                ScheduledSessionId = sessionStartedDataMessage.SchedulerID
            };
        }

        public static SessionStartEventArgs ToArgs(this SessionStartedDataMessage sessionStartedDataMessage)
        {
            if (sessionStartedDataMessage == null)
            {
                throw new ArgumentNullException(nameof(sessionStartedDataMessage));
            }
            return new SessionStartEventArgs(
                sessid: Guid.Parse(sessionStartedDataMessage.SessId),
                errmsg: sessionStartedDataMessage.ErrMsg,
                userid: Guid.Parse(sessionStartedDataMessage.UserId),
                usermsg: sessionStartedDataMessage.UserMsg,
                schedId: sessionStartedDataMessage.ScheduledSessionId);
        }

        public static SessionStopDataMessage To(this SessionStopData sessionStopDataMessage)
        {
            if (sessionStopDataMessage == null)
            {
                throw new ArgumentNullException(nameof(sessionStopDataMessage));
            }

            return new SessionStopDataMessage()
            {
                ErrMsg = sessionStopDataMessage.ErrorMessage ?? string.Empty,
                SessId = sessionStopDataMessage.SessionId.ToString(),
                ScheduledSessionId = sessionStopDataMessage.ScheduleId
            };
        }        

        public static SessionStopEventArgs ToArgs(this SessionStopDataMessage sessionStopDataMessage)
        {
            if (sessionStopDataMessage == null)
            {
                throw new ArgumentNullException(nameof(sessionStopDataMessage));
            }
            return new SessionStopEventArgs(Guid.Parse(sessionStopDataMessage.SessId), sessionStopDataMessage.ErrMsg, sessionStopDataMessage.ScheduledSessionId);
        }

        public static SessionVariablesUpdatedDataMessage To(this SessionVariablesUpdatedData sessionVariablesUpdatedMessage)
        {
            if (sessionVariablesUpdatedMessage == null)
            {
                throw new ArgumentNullException(nameof(sessionVariablesUpdatedMessage));
            }

            return new SessionVariablesUpdatedDataMessage()
            {
                SessVar = sessionVariablesUpdatedMessage.JSONData ?? string.Empty,
                ErrMsg = sessionVariablesUpdatedMessage.ErrorMessage ?? string.Empty

            };
        }

        public static SessionVariableUpdatedEventArgs ToArgs(this SessionVariablesUpdatedDataMessage sessionVariablesUpdatedMessage)
        {
            if (sessionVariablesUpdatedMessage == null)
            { 
                throw new ArgumentNullException(nameof(sessionVariablesUpdatedMessage)); 
            }
            return new SessionVariableUpdatedEventArgs(sessionVariablesUpdatedMessage.SessVar, sessionVariablesUpdatedMessage.ErrMsg);
        }

        public static FailedOperationMessage CreateFailedOperationMessage(string errMsg, string msg, int statusCode)
        {
            return new FailedOperationMessage
            {
                ErrMsg = errMsg ?? string.Empty,
                Message = msg ?? string.Empty,
                StatusCode = statusCode
            };
        }
    }
}
