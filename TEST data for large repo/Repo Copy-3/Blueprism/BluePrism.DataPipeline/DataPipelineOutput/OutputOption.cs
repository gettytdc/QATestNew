using System;
using System.Runtime.Serialization;

namespace BluePrism.DataPipeline.DataPipelineOutput
{
    [DataContract(Namespace = "bp"), Serializable]
    [KnownType(typeof(AuthorizationOutputOption))]
    public class OutputOption
    {
        public OutputOption(string id, string value, bool isObject = false)
        {
            Id = id;
            Value = value;
            IsObject = isObject;
        }

        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Value { get; set; }
        [DataMember]
        public bool IsObject { get; set; }

        public virtual string GetConfig()
        {
            return IsObject ? $"{Id} => {Value}" : $"{Id} => \"{Value}\"";
        }
    }
}