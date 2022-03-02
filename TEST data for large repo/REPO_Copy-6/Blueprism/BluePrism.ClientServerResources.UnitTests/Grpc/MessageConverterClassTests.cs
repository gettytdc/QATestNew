using System;
using System.Collections.Generic;
using System.Linq;
using BluePrism.ClientServerResources.Core.Data;
using BluePrism.ClientServerResources.Core.Enums;
using BluePrism.ClientServerResources.Core.Events;
using BluePrism.ClientServerResources.Grpc;
using NUnit.Framework;

namespace BluePrism.ClientServerResources.UnitTests.Grpc
{
    public class MessageConverterClassTests
    {
        [Test]
        public void To_Extension_ConvertSessionCreatedDataToSessionCreatedDataMessage()
        {
            var userId = Guid.NewGuid();
            var errorMessage = "Error message";
            var processId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();
            var schedID = 25352345;
            var sessionRunnerStatus = new Dictionary<string, RunnerStatus>();
            var tag = Guid.NewGuid();
            sessionRunnerStatus.Add(sessionId.ToString(), RunnerStatus.PENDING);

            var sessionCreatedData = new SessionCreatedData(SessionCreateState.Created, resourceId, processId, sessionId, schedID, errorMessage, sessionRunnerStatus, userId, tag);
            var converted = sessionCreatedData.To();
            Assert.AreEqual(converted.State, (int)sessionCreatedData.State);
            Assert.AreEqual(Guid.Parse(converted.ResourceId), sessionCreatedData.ResourceId);
            Assert.AreEqual(Guid.Parse(converted.ProcessId), sessionCreatedData.ProcessId);
            Assert.AreEqual(Guid.Parse(converted.SessionId), sessionCreatedData.SessionId);            
            Assert.AreEqual(converted.ScheduledSessionId, sessionCreatedData.ScheduledSessionId);
            Assert.AreEqual(converted.ErrorMessage, sessionCreatedData.ErrorMessage);
            Assert.AreEqual(converted.Data.ElementAt(0).Key, sessionCreatedData.Data.ElementAt(0).Key);
            Assert.AreEqual(converted.Data.ElementAt(0).ResourceStatus, Convert.ToInt32(sessionCreatedData.Data.ElementAt(0).Value));
            Assert.AreEqual(Guid.Parse(converted.UserId), sessionCreatedData.UserId);
            Assert.AreEqual(schedID, converted.ScheduledSessionId);
        }        

        [Test]
        public void To_Extension_ConvertResourcesChangedDataResourcesChangedMessage()
        {
            var resourceStatusChange = ResourceStatusChange.OfflineChange;
            var changes = new Dictionary<string, ResourceStatusChange>();

            var resourcesChangedData = new ResourcesChangedData(resourceStatusChange, changes);
            var converted = resourcesChangedData.To();
            Assert.AreEqual((int)converted.OverallChange, (int)resourceStatusChange);
            Assert.AreEqual(converted.Changes, changes);
        }

        [Test]
        public void To_Extension_ConvertSessionDeletedDataToSessionDeletedMessage()
        {
            var sessID = Guid.NewGuid();
            var errmesg = "Error Message";
            var userID = Guid.NewGuid();
            var schedId = 4243;

            var resourcesChangedData = new SessionDeletedData(sessID, errmesg, userID, schedId);
            var converted = resourcesChangedData.To();
            Assert.AreEqual(converted.SessId, sessID.ToString());
            Assert.AreEqual(converted.ErrMsg, errmesg);
            Assert.AreEqual(converted.UserId, userID.ToString());
            Assert.AreEqual(converted.ScheduledSessionId, schedId);
        }

        [Test]
        public void To_Extension_ConvertSessionEndDataToSessionEndMessage()
        {
            var sessID = Guid.NewGuid();
            var status = "Status Message";

            var sessionEndData = new SessionEndData(sessID, status);
            var converted = sessionEndData.To();
            Assert.AreEqual(converted.SessId, sessID.ToString());
            Assert.AreEqual(converted.Status, status);
        }

        [Test]
        public void To_Extension_ConvertSessionStartedDataToSessionStartedMessage()
        {
            var sessID = Guid.NewGuid();
            var errmesg = "Error Message";
            var userID = Guid.NewGuid();
            var usrmsg = "User Message";
            var schedid = 42345;

            var resourcesChangedData = new SessionStartedData(sessID, errmesg, userID, usrmsg, schedid);
            var converted = resourcesChangedData.To();
            Assert.AreEqual(converted.SessId, sessID.ToString());
            Assert.AreEqual(converted.ErrMsg, errmesg);
            Assert.AreEqual(converted.UserId, userID.ToString());
            Assert.AreEqual(converted.UserMsg, usrmsg);
            Assert.AreEqual(converted.ScheduledSessionId, schedid);
        }

        [Test]
        public void To_Extension_ConvertSessionVariableUpdatedDataToSessionVariableUpdatedMessage()
        {
            var jsonData = "legit test string";

            var sessionVariableUpdatedData = new SessionVariablesUpdatedData(jsonData, string.Empty);
            var converted = sessionVariableUpdatedData.To();
            Assert.AreEqual(converted.SessVar, jsonData);
        }

        [Test]
        public void ToArgs_EndToEnd_SessionCreate()
        {
            //SessionXXXEventArgs -> (Ctor)SessionXXXData -> (To)SessionXXXDataMessage -> (ToArgs)SessionXXXEventArgs
            var initialArgs = new SessionCreateEventArgs(
                resourceId: Guid.NewGuid(),
                processId: Guid.NewGuid(),
                sessionId: Guid.NewGuid(),
                userId: Guid.NewGuid(),
                schedSessId: 56457,
                errMsg: "some message",
                data: new Dictionary<Guid, RunnerStatus>()
                {
                    { Guid.NewGuid(), RunnerStatus.PENDING },
                    { Guid.NewGuid(), RunnerStatus.STOPPED },
                    { Guid.NewGuid(), RunnerStatus.RUNNING },
                    { Guid.NewGuid(), RunnerStatus.UNKNOWN },
                },
                tag: Guid.NewGuid());

            var data = new SessionCreatedData(
                initialArgs.State,
                initialArgs.ResourceId,
                initialArgs.ProcessId,
                initialArgs.SessionId,
                initialArgs.ScheduledSessionId,
                initialArgs.ErrorMessage,
                initialArgs.Data.ToDictionary(k => k.Key.ToString(), v => v.Value),
                initialArgs.UserId,
                initialArgs.Tag);

            var dataMsg = data.To();

            var outArgs = dataMsg.ToArgs();

            Assert.AreEqual(initialArgs.Data, outArgs.Data);
            Assert.AreEqual(initialArgs.ResourceId, outArgs.ResourceId);
            Assert.AreEqual(initialArgs.ProcessId, outArgs.ProcessId);
            Assert.AreEqual(initialArgs.SessionId, outArgs.SessionId);
            Assert.AreEqual(initialArgs.UserId, outArgs.UserId);
            Assert.AreEqual(initialArgs.ScheduledSessionId, outArgs.ScheduledSessionId);
            Assert.AreEqual(initialArgs.ErrorMessage, outArgs.ErrorMessage);
            Assert.AreEqual(Guid.Parse(initialArgs.Tag.ToString()), Guid.Parse(outArgs.Tag.ToString()));
        }

        [Test]
        public void ToArgs_EndToEnd_ResourcesChanged()
        {
            var initialArgs = new ResourcesChangedEventArgs(
                ResourceStatusChange.OnlineOrOfflineChange,
                new Dictionary<string, ResourceStatusChange>()
                {
                    { "some string", ResourceStatusChange.OfflineChange },
                    { "some string1", ResourceStatusChange.EnvironmentChange },
                    { "some string2", ResourceStatusChange.OnlineChange },
                    { "some string3", ResourceStatusChange.None },
                });

            var data = new ResourcesChangedData(initialArgs.OverallChange, initialArgs.Changes);

            var dataMsg = data.To();

            var outArgs = dataMsg.ToArgs();

            Assert.AreEqual(initialArgs.OverallChange, outArgs.OverallChange);
            Assert.AreEqual(initialArgs.Changes, outArgs.Changes);
        }

        [Test]
        public void ToArgs_EndToEnd_SessionDelete()
        {
            var initialArgs = new SessionDeleteEventArgs(Guid.NewGuid(), "some error message", Guid.NewGuid(), 67);

            var data = new SessionDeletedData(initialArgs.SessionId, initialArgs.ErrorMessage, initialArgs.UserId, initialArgs.ScheduledSessionId);

            var dataMsg = data.To();

            var outArgs = dataMsg.ToArgs();

            Assert.AreEqual(initialArgs.SessionId, outArgs.SessionId);
            Assert.AreEqual(initialArgs.ErrorMessage, outArgs.ErrorMessage);
            Assert.AreEqual(initialArgs.UserId, outArgs.UserId);
            Assert.AreEqual(initialArgs.ScheduledSessionId, outArgs.ScheduledSessionId);
        }

        [Test]
        public void ToArgs_EndToEnd_SessionEnd()
        {
            var initialArgs = new SessionEndEventArgs(Guid.NewGuid(), "some status 123");

            var data = new SessionEndData(initialArgs.SessionId, initialArgs.UserMessage);

            var dataMsg = data.To();

            var outArgs = dataMsg.ToArgs();

            Assert.AreEqual(initialArgs.SessionId, outArgs.SessionId);
            Assert.AreEqual(initialArgs.UserMessage, outArgs.UserMessage);
        }

        [Test]
        public void ToArgs_EndToEnd_SessionStarted()
        {
            var initialArgs = new SessionStartEventArgs(sessid: Guid.NewGuid(), errmsg: "some error message", userid: Guid.NewGuid(), usermsg: "some user message", schedId: 834534);

            var data = new SessionStartedData(initialArgs.SessionId, initialArgs.ErrorMessage, initialArgs.UserId, initialArgs.UserMessage, initialArgs.ScheduledSessionId);

            var dataMsg = data.To();

            var outArgs = dataMsg.ToArgs();

            Assert.AreEqual(initialArgs.SessionId, outArgs.SessionId);
            Assert.AreEqual(initialArgs.ErrorMessage, outArgs.ErrorMessage);
            Assert.AreEqual(initialArgs.UserId, outArgs.UserId);
            Assert.AreEqual(initialArgs.ScheduledSessionId, outArgs.ScheduledSessionId);
        }

        [Test]
        public void ToArgs_EndToEnd_SessionVariableUpdated()
        {
            var initialArgs = new SessionVariableUpdatedEventArgs("Test String");

            var data = new SessionVariablesUpdatedData(initialArgs.JSONData, string.Empty);

            var dataMsg = data.To();

            var outArgs = dataMsg.ToArgs();

            Assert.AreEqual(initialArgs.JSONData, outArgs.JSONData);
        }

        [Test]
        public void To_SessionCreated_HandlesNull()
        {
            var args = new SessionCreatedData(
                default(SessionCreateState),
                default(Guid),
                default(Guid),
                default(Guid),
                default(int),
                null,
                null,
                default(Guid),
                null);

            Assert.DoesNotThrow(() => args.To());
        }

        [Test]
        public void To_ResourceChanged_HandlesNull()
        {
            var args = new ResourcesChangedData(
                default(ResourceStatusChange),
                null);

            Assert.DoesNotThrow(() => args.To());
        }

        [Test]
        public void To_SessionDelete_HandlesNull()
        {
            var args = new SessionDeletedData(
                default(Guid),
                null,
                default(Guid),
                default(int));

            Assert.DoesNotThrow(() => args.To());
        }

        [Test]
        public void To_SessionEnd_HandlesNull()
        {
            var args = new SessionEndData(
                default(Guid),
                null);

            Assert.DoesNotThrow(() => args.To());
        }

        [Test]
        public void To_SessionStarted_HandlesNull()
        {
            var args = new SessionStartedData(
                default(Guid),
                null,
                default(Guid),
                null,
                default(int));

            Assert.DoesNotThrow(() => args.To());
        }

        [Test]
        public void To_SessionVariablesUpdated_HandlesNull()
        {
            var args = new SessionVariablesUpdatedData(null, null);

            Assert.DoesNotThrow(() => args.To());
        }

        [Test]
        public void To_FailedOperation_HandlesNull()
        {
            Assert.DoesNotThrow(() => MessageConverterClass.CreateFailedOperationMessage(default(string), default(string), default(int)));
        }
    }
}
