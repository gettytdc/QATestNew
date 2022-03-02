using System;
using System.Runtime.Serialization;

namespace BluePrism.DataPipeline
{
    [DataContract(Namespace = "bp"), Serializable]
    public class DataGatewayProcessStatusInformation
    {
        [DataMember]
        private int _id;

        [DataMember]
        private string _name;

        [DataMember]
        private DataGatewayProcessState _status;

        [DataMember]
        private string _errorMessage;

        public int Id
        {
            get => _id;
            set => _id = value;
        }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public DataGatewayProcessState Status
        {
            get => _status;
            set => _status = value;
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => _errorMessage = value;
        }

        public bool IsInErrorState => _status == DataGatewayProcessState.Error || _status == DataGatewayProcessState.UnrecoverableError;

    }
}