using System;
using BluePrism.DigitalWorker.Messages.Commands.Internal;

namespace BluePrism.DigitalWorker.Messages.Commands.Factory
{
    /// <summary>
    /// Contains factory methods to create messages within .NET applications
    /// </summary>
    public static class DigitalWorkerCommands
    {
        public static RunProcess RunProcess(Guid sessionId, Guid processId) =>
            new RunProcessMessage
            {
                SessionId = sessionId,
                ProcessId = processId
            };

        public static StopProcess StopProcess(Guid sessionId, string userName) =>
            new StopProcessMessage
            {
                SessionId = sessionId,
                Username = userName
            };

        public static RequestStopProcess RequestStopProcess(Guid sessionId) =>
            new RequestStopProcessMessage
            {
                SessionId = sessionId
            };

        public static GetSessionVariables GetSessionVariables(Guid sessionId) =>
            new GetSessionVariablesMessage
            {
                SessionId = sessionId
            };

        public static GetSessionVariablesResponse GetSessionVariablesResponse(bool sessionRunning,
            SessionVariable[] variables) =>
            new GetSessionVariablesResponseMessage
            {
                SessionRunning = sessionRunning, Variables = variables
            };
    }
}
