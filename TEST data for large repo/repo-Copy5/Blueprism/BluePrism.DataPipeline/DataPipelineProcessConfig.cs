using System;
using System.Runtime.Serialization;

namespace BluePrism.DataPipeline
{
    [DataContract(Namespace = "bp"), Serializable]
    public class DataPipelineProcessConfig
    {
        public DataPipelineProcessConfig(int id, string name, string config, bool isCustom)
        {
            _id = id;
            _name = name;
            _config = config;
            _isCustom = isCustom;
        }

        [DataMember]
        private int _id;
        public int Id => _id;

        [DataMember]
        private string _name;
        public string Name => _name;

        [DataMember]
        private string _config;
        public string LogstashConfigFile => _config;

        [DataMember]
        private bool _isCustom;
        public bool IsCustom => _isCustom;
    }
}
