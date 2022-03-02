using System;
using System.Runtime.Serialization;

namespace BluePrism.DataPipeline
{
    [DataContract(Namespace = "bp"), Serializable]
    public partial class DataGatewayProcessStatus
    {
        public DataGatewayProcessStatus(DataGatewayProcessState status, string message, DateTime lastUpdated)
        {
            Status = status;
            Message = message;
            LastUpdated = lastUpdated;
        }

        [DataMember]
        public DataGatewayProcessState Status { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public DateTime LastUpdated { get; set; }

    }
}
