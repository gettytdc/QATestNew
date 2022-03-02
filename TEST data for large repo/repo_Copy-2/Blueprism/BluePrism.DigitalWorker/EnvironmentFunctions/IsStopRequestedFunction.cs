using System;
using System.Collections.Generic;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateProcessCore;
using BluePrism.DigitalWorker.Sessions;

namespace BluePrism.DigitalWorker.EnvironmentFunctions
{
    public class IsStopRequestedFunction : IsStopRequestedFunctionBase
    {
        private readonly IRunningSessionRegistry _runningSessionRegistry;

        public IsStopRequestedFunction(IRunningSessionRegistry runningSessionRegistry) 
        {
            _runningSessionRegistry = runningSessionRegistry ?? throw new ArgumentNullException(nameof(runningSessionRegistry));
        }

        protected override clsProcessValue InnerEvaluate(IList<clsProcessValue> parameters, clsProcess process)
        {
            if (process.Session == null)
                return false;

            var session = _runningSessionRegistry.Get(process.Session.ID);

            return session?.StopRequested ?? false;
        }
    }
}
