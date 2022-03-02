using BluePrism.BPServer.Properties;
using BluePrism.Server.Domain.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace BluePrism.BPServer.ServerBehaviours
{

    internal class GenericErrorHandler : IErrorHandler, IServiceBehavior
    {
        #region IErrorHandler Members

        public bool HandleError(Exception error)
        {
            Debug.Print("Error reported in WCF Service: {0}", error);
            return true;
        }

        public void ProvideFault(
            Exception ex, MessageVersion ver, ref Message msg)
        {
            // If ex is already a FaultException or a FaultException<>, just
            // let the existing infrastructure handle it
            if (ex is FaultException ||
                (ex.GetType().IsGenericType &&
                 ex.GetType().GetGenericTypeDefinition().Equals(typeof(FaultException<>))))
                return;
            // Otherwise, create a BPServerFault from the exception
            var fe = BPServerFault.CreateFaultException(ex);
            var fault = fe.CreateMessageFault();
            msg = Message.CreateMessage(ver, fault, fe.Action);
        }

        #endregion

        #region IServiceBehavior Members

        void IServiceBehavior.AddBindingParameters(
            ServiceDescription desc,
            ServiceHostBase hostBase,
            Collection<ServiceEndpoint> endpoints,
            BindingParameterCollection bindingParams)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(
            ServiceDescription desc, ServiceHostBase hostBase)
        {
            foreach (ChannelDispatcher disp in hostBase.ChannelDispatchers)
            {
                disp.ErrorHandlers.Add(this);
            }
        }

        void IServiceBehavior.Validate(
            ServiceDescription desc, ServiceHostBase hostBase)
        {
            // This was largely lifted from MSDN, specifically:
            // http://tinyurl.com/ztecmgk and then tailored for this handler
            foreach (ServiceEndpoint se in desc.Endpoints)
            {
                // Must not examine any metadata endpoint.
                if (se.Contract.Name.Equals("IMetadataExchange")
                  && se.Contract.Namespace.Equals("http://schemas.microsoft.com/2006/04/mex"))
                    continue;

                var opsNoFaults = new HashSet<string>();
                var opsNoBPFault = new HashSet<string>();

                foreach (OperationDescription opDesc in se.Contract.Operations)
                {
                    if (opDesc.Faults.Count == 0)
                    {
                        opsNoFaults.Add(opDesc.Name);
                    }

                    if (!opDesc.Faults.Cast<FaultDescription>().Any(
                        (f) => f.DetailType.Equals(typeof(BPServerFault))))
                    {
                        opsNoBPFault.Add(opDesc.Name);
                    }
                }

                if (opsNoFaults.Count + opsNoBPFault.Count > 0)
                {
                    string detailMsg = "";
                    if (opsNoFaults.Count > 0)
                    {
                        detailMsg =
                            string.Format(Resources.TheFollowingOperationsHaveNoFaultContractAttributeDeclared01, string.Join(", ", opsNoFaults), Environment.NewLine);
                    }
                    if (opsNoBPFault.Count > 0)
                    {
                        detailMsg +=
                            string.Format(Resources.TheFollowingOperationsHaveFaultContractAttributesDeclaredButNoneWhichSpecifyABP, string.Join(", ", opsNoFaults));
                    }
                    throw new InvalidOperationException(
                        string.Format(Resources.GenericErrorHandlerRequiresAFaultContractGetTypeBPServerFaultAttributeToBeSpeci, Environment.NewLine, detailMsg)
                    );
                }
            }
        }

        #endregion
    }
}
