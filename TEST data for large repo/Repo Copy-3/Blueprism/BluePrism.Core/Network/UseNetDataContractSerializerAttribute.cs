using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace BluePrism.Core.Network
{
    /// <summary>
    /// Attribute which can be used to decorate an <see cref="OperationContract"/> to
    /// instruct it to use a <see cref="NetDataContractSerializer"/> rather than the
    /// standard <see cref="DataContractSerializer"/> to serialize the arguments for
    /// an operation.
    /// </summary>
    public class UseNetDataContractSerializerAttribute : Attribute, IOperationBehavior
    {
        public void AddBindingParameters(OperationDescription desc, BindingParameterCollection parms)
        {
        }
        public void ApplyClientBehavior(
            OperationDescription desc, ClientOperation proxy)
        {
            ReplaceDataContractSerializerOperationBehavior(desc);
        }
        public void ApplyDispatchBehavior(
            OperationDescription desc, DispatchOperation dispatch)
        {
            ReplaceDataContractSerializerOperationBehavior(desc);
        }
        public void Validate(OperationDescription description)
        {
        }
        private static void ReplaceDataContractSerializerOperationBehavior(
            OperationDescription desc)
        {
            var behaviors = desc.Behaviors;
            DataContractSerializerOperationBehavior beh =
                behaviors.Find<DataContractSerializerOperationBehavior>();
            if (beh != null)
            {
                behaviors.Remove(beh);
                behaviors.Add(new NetDataContractOperationBehavior(desc));
            }
        }
    }
}
