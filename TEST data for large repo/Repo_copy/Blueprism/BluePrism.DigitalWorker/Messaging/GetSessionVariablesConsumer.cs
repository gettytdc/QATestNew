using BluePrism.DigitalWorker.Sessions;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BluePrism.DigitalWorker.Messages.Commands;
using System.Linq;
using BluePrism.AutomateProcessCore;
using BluePrism.DigitalWorker.Messages.Commands.Factory;

namespace BluePrism.DigitalWorker.Messaging
{
    public class GetSessionVariablesConsumer : IConsumer<GetSessionVariables>
    {
        private readonly IRunningSessionRegistry _runningSessionRegistry;

        public GetSessionVariablesConsumer(IRunningSessionRegistry runningSessionRegistry) =>
            _runningSessionRegistry = runningSessionRegistry ?? throw new ArgumentNullException(nameof(runningSessionRegistry));

        private static void ValidateParameters(ConsumeContext<GetSessionVariables> context)
        {
            if (context.Message.SessionId == Guid.Empty)
            {
                throw new ArgumentException("No valid Session Id provided.");
            }
        }

        public async Task Consume(ConsumeContext<GetSessionVariables> context)
        {
            ValidateParameters(context);

            var sessionVariables = GetResponse(context.Message.SessionId);

            context.Respond(sessionVariables);

            await Task.CompletedTask;
        }

        private GetSessionVariablesResponse GetResponse(Guid sessionId)
        {
            var runner = _runningSessionRegistry.Get(sessionId);

            if (runner == null)
            {
                return DigitalWorkerCommands.GetSessionVariablesResponse(false, new SessionVariable[] { });
            }

            var variables = runner.GetSessionVariables().OrderBy(v => v.Key);
            var mappedVariables = variables.Select(MapSessionVariable);
            return DigitalWorkerCommands.GetSessionVariablesResponse(true, mappedVariables.ToArray());
        }

        private static SessionVariable MapSessionVariable(KeyValuePair<string, clsProcessValue> variable)
        {
            Enum.TryParse<ProcessValueType>(variable.Value.DataTypeName, true, out var processValueType);

            return new SessionVariable(variable.Key, variable.Value.Description,
                new ProcessValue(variable.Value.EncodedValue, processValueType));
        }
    }
}
