using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace BluePrism.Core.Analytics
{
    public class WCFAnalyticsEndpointBehavior : IEndpointBehavior
    {
        private DateTime? _logExpiry = null;

        public WCFAnalyticsEndpointBehavior(int wcfPerformanceLogMinutes)
        {
            if (wcfPerformanceLogMinutes > 0)
            {
                _logExpiry = DateTime.UtcNow.AddMinutes(wcfPerformanceLogMinutes);
            }
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
           
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            
            clientRuntime.ClientMessageInspectors.Add(new CommunicationInspectors(_logExpiry, new RecordOverviewAnalyser()));
            
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new CommunicationInspectors(_logExpiry, new RecordOverviewAnalyser()));
        }

        public void Validate(ServiceEndpoint endpoint)
        {
           
        }
    }
}
