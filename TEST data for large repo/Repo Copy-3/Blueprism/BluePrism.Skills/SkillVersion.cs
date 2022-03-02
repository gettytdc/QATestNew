using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace BluePrism.Skills
{
    [Serializable, DataContract(Namespace = "bp", Name = "sv"), KnownType(typeof(Bitmap))]
    public abstract class SkillVersion
    {
        [DataMember(Name = "v")]
        private string _versionNumber;
        [DataMember(Name = "n")]
        private string _name;
        [DataMember(Name = "d")]
        private string _description;
        [DataMember(Name = "c")]
        private SkillCategory _category;
        [DataMember(Name = "i")]
        private byte[] _icon;
        [DataMember(Name = "bpVersionC")]
        private string _bpVersionCreatedIn;
        [DataMember(Name = "bpVersionT")]
        private string _bpVersionTestedIn;
        [DataMember(Name = "ia")]
        private DateTime _importedAt;

        public string Name => _name;
        public string VersionNumber => _versionNumber;
        public string Description => _description;
        public SkillCategory Category => _category;
        public byte[] Icon => _icon;
        public string BpVersionCreatedIn => _bpVersionCreatedIn;
        public string BpVersionTestedIn => _bpVersionTestedIn;
        public DateTime ImportedAt => _importedAt;

        public SkillVersion(
            string name,
            SkillCategory category, 
            string versionNumber,
            string description,
            byte[] icon,
            string bpVersionCreated,
            string bpVersionTested,
            DateTime importedAt)
        {
            if (string.IsNullOrEmpty(versionNumber))
                throw new ArgumentNullException(nameof(versionNumber));

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (string.IsNullOrEmpty(description))
                throw new ArgumentNullException(nameof(description));

            if (category == SkillCategory.Unknown)
                throw new ArgumentNullException(nameof(category));

            if (string.IsNullOrEmpty(bpVersionCreated))
                throw new ArgumentNullException(nameof(bpVersionCreated));

            if (string.IsNullOrEmpty(bpVersionTested))
                throw new ArgumentNullException(nameof(bpVersionTested));

            _versionNumber = versionNumber;
            _name = name;
            _description = description;
            _category = category;
            _icon = icon;
            _bpVersionCreatedIn = bpVersionCreated;
            _bpVersionTestedIn = bpVersionTested;
            _importedAt = importedAt;
        }
    }
}
