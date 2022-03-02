using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Xml;
namespace BluePrism.Core.Network
{
    /// <summary>
    /// A behavior which allows the use of a <see cref="NetDataContractSerializer"/>
    /// rather than the standard <see cref="DataContractSerializer"/> to serialize
    /// the arguments for an operation.
    /// </summary>
    /// <remarks>
    /// This is "inspired" by a blog post describing this type of serialization at:
    /// https://lunaverse.wordpress.com/2007/05/09/remoting-using-wcf-and-nhibernate/
    /// </remarks>
    public class NetDataContractOperationBehavior : DataContractSerializerOperationBehavior
    {
        public NetDataContractOperationBehavior(OperationDescription operation)
            : base(operation) { }
        public NetDataContractOperationBehavior(
            OperationDescription op, DataContractFormatAttribute attr)
            : base(op, attr) { }
        public override XmlObjectSerializer CreateSerializer(
            Type type, string name, string ns, IList<Type> knownTypes)
        {
            return new NetDataContractSerializer(name, ns);
        }
        public override XmlObjectSerializer CreateSerializer(
            Type type, XmlDictionaryString name,
            XmlDictionaryString ns, IList<Type> knownTypes)
        {
            return new NetDataContractSerializer(name, ns);
        }
    }
}
