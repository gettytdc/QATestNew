using System;
using System.Collections.Generic;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateProcessCore;
using BluePrism.DigitalWorker.Sessions;

namespace BluePrism.DigitalWorker.EnvironmentFunctions
{
    public class GetUserNameFunction : GetUserNameFunctionBase
    {
        private readonly IRunningSessionRegistry _runningSessionRegistry;

        public GetUserNameFunction(IRunningSessionRegistry runningSessionRegistry)
        {
            _runningSessionRegistry = runningSessionRegistry ?? throw new ArgumentNullException(nameof(runningSessionRegistry));
        }

        protected override clsProcessValue InnerEvaluate(IList<clsProcessValue> parameters, clsProcess process)
        {
            if (process.Session == null)
                return string.Empty;

            var sessionContext = _runningSessionRegistry.Get(process.Session.ID);

            if (sessionContext == null)
                return string.Empty;

            return sessionContext.StartedByUsername;
        }
    }
}
