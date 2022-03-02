using System;
using System.Collections.Generic;
using System.Linq;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateProcessCore;
using BluePrism.DigitalWorker.Sessions;

namespace BluePrism.DigitalWorker.EnvironmentFunctions
{
    public class GetStartTimeFunction : GetStartTimeFunctionBase
    {
        private readonly IRunningSessionRegistry _runningSessionRegistry;

        public GetStartTimeFunction(IRunningSessionRegistry runningSessionRegistry)
        {
            _runningSessionRegistry = runningSessionRegistry ?? throw new ArgumentNullException(nameof(runningSessionRegistry));
        }

        protected override clsProcessValue InnerEvaluate(IList<clsProcessValue> parameters, clsProcess process)
        {
            if (parameters.Any())
                throw GetStartTimeFunctionShouldHaveNoParametersException;

            if (process.Session == null)
                throw MissingSessionException;

            var digitalWorkerRunnerRecord = _runningSessionRegistry.Get(process.Session.ID);

            if (digitalWorkerRunnerRecord == null)
                throw GetStartTimeFunctionSessionNotRunningException;
            
            return new clsProcessValue(DataType.datetime, digitalWorkerRunnerRecord.SessionStarted.ToUniversalTime().DateTime, false);
        }
    }
}
