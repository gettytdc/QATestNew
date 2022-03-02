using System;
using System.Collections.Generic;
using System.Linq;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Resources;
using BluePrism.AutomateProcessCore;
using BluePrism.Server.Domain.Models;

namespace BluePrism.DigitalWorker.EnvironmentFunctions
{    
    public class GetResourceNameFunction : GetResourceNameFunctionBase
    {
        private readonly Func<DigitalWorkerContext> _digitalWorkerContextFactory;

        public GetResourceNameFunction(Func<DigitalWorkerContext> digitalWorkerContextFactory)
        {
            _digitalWorkerContextFactory = digitalWorkerContextFactory;
        }

        protected override clsProcessValue InnerEvaluate(IList<clsProcessValue> parameters, clsProcess process)
        {
            if (parameters.Any())            
                throw GetResourceNameFunctionShouldNotHaveAnyParamsException;

            return _digitalWorkerContextFactory().StartUpOptions.Name.FullName;
        }
    }
}
