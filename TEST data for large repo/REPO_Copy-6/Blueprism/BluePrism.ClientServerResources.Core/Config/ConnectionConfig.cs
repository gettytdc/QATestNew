using System;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using BluePrism.BPCoreLib;
using BluePrism.ClientServerResources.Core.Enums;
using BluePrism.Common.Security;

namespace BluePrism.ClientServerResources.Core.Config
{
    [Serializable, DataContract(Namespace = "bp")]
    public class ConnectionConfig
    {
        [NonSerialized]
        public const string ElementName = "CallbackConnection";
        [NonSerialized]
        public const string BindingName = "bpInstructionBinding";
        [NonSerialized]
        public const string EndpointName = "bpinstruct";

        [DataMember(Name = "t")]
        public CallbackConnectionProtocol CallbackProtocol { get; set; } = CallbackConnectionProtocol.Grpc;
        [DataMember(Name = "h")]
        public string HostName { get; set; } = "localhost";
        [DataMember(Name = "p")]
        public int Port { get; set; } = 10000;
        [DataMember(Name = "m")]
        public InstructionalConnectionModes Mode { get; set; } = InstructionalConnectionModes.None;
        [DataMember(Name = "scn")]
        public string CertificateName { get; set; }
        [DataMember(Name = "ccn")]
        public string ClientCertificateName { get; set; }
        [DataMember(Name = "ss")]
        public StoreName ServerStore { get; set; } = StoreName.TrustedPublisher;
        [DataMember(Name = "sc")]
        public StoreName ClientStore { get; set; } = StoreName.TrustedPeople;

        [DataMember(Name = "cc", EmitDefaultValue = true)]
        public SafeString ClientCertificate { get; set; }




        public override string ToString()
           => string.Format("{0} binding to {1}:{2} with mode {3}",
                CallbackProtocol.GetFriendlyName(),
                HostName,
                Port,
                Enum.GetName(typeof(InstructionalConnectionModes), Mode));
    }
}
