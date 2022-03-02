using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BluePrism.Skills
{
    [Serializable, DataContract(Namespace = "bp")]
    public class WebSkillVersion : SkillVersion
    {
        [DataMember]
        private Guid _webApiId;

        [DataMember]
        private string _webApiName;

        [DataMember]
        private bool _webApiEnabled;

        [DataMember]
        private List<string> _webApiActionNames;

        public Guid WebApiId => _webApiId;
        public string WebApiName => _webApiName;
        public bool WebApiEnabled => _webApiEnabled;
        public List<string> WebApiActionNames => _webApiActionNames;

        public WebSkillVersion(
            Guid webApiId,
            string webApiName,
            Boolean webApiEnabled,
            string name,
            SkillCategory category,
            string versionNumber,
            string description,
            byte[] icon,
            string bpVersionCreated,
            string bpVersionTested,
            DateTime importedAt,
            List<string> actionNames) : base(name,
                category,
                versionNumber,
                description,
                icon,
                bpVersionCreated,
                bpVersionTested,
                importedAt)
        {
            _webApiActionNames = actionNames;
            _webApiId = webApiId;
            _webApiName = webApiName;
            _webApiEnabled = webApiEnabled;
        }
    }
}
