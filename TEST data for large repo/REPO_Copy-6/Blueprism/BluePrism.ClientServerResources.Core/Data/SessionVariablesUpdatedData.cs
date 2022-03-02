using System;
using System.Runtime.Serialization;

namespace BluePrism.ClientServerResources.Core.Data
{
    [Serializable]
    [DataContract(Namespace = "bp")]
    public class SessionVariablesUpdatedData
    {
        [DataMember]
        public string JSONData { get; set;  }

        [DataMember]
        public string ErrorMessage { get; set; }

        public SessionVariablesUpdatedData(string jsonData, string errorMessage)
        {
            JSONData = jsonData;
            ErrorMessage = errorMessage;
        }
    }
}
