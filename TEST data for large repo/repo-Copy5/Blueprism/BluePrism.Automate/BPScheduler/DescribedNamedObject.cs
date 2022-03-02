using System;
using System.Runtime.Serialization;
using BluePrism.BPCoreLib;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Basic class to hold a name and description
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "bp", IsReference = true)]
    public class DescribedNamedObject : clsDataMonitor
    {
        /// <summary>
        /// The name of this object
        /// </summary>
        [DataMember]
        private string _name;

        /// <summary>
        /// The description of this object.
        /// </summary>
        [DataMember]
        private string _desc;

        /// <summary>
        /// The maximum length of the name for this object.
        /// 0 indicates no maximum length
        /// </summary>
        [DataMember]
        private int _nameLen;

        /// <summary>
        /// The maximum length of the description for this object.
        /// 0 indicates no maximum length
        /// </summary>
        [DataMember]
        private int _descLen;

        /// <summary>
        /// Creates a new object with an empty name and description and no
        /// limit on their lengths.
        /// </summary>
        public DescribedNamedObject() : this(0, 0) { }

        /// <summary>
        /// Creates a new object with an empty name of the given maximum
        /// length and a description with no length limit.
        /// </summary>
        /// <param name="maxNameLength">The maximum number of characters
        /// allowed in the name - zero indicates that there is no maximum.
        /// </param>
        protected DescribedNamedObject(int maxNameLength) : this(maxNameLength, 0) { }

        /// <summary>
        /// Creates a new object with the specified length limits on its
        /// name and description.
        /// </summary>
        /// <param name="maxNameLength">The maximum number of characters
        /// allowed for the name - zero indicates that there is no maximum.
        /// </param>
        /// <param name="maxDescLength">The maximum number of characters
        /// allowed for the description - zero indicates that there is no
        /// maximum.</param>
        protected DescribedNamedObject(int maxNameLength, int maxDescLength)
        {
            _nameLen = maxNameLength;
            _descLen = maxDescLength;
        }

        /// <summary>
        /// The name of this object.
        /// </summary>
        public virtual string Name
        {
            get { return _name ?? ""; }
            set { ChangeData("Name", ref _name, value == "" ? null : value); }
        }

        /// <summary>
        /// The description of this object.
        /// </summary>
        public virtual string Description
        {
            get { return _desc ?? ""; }
            set { ChangeData("Description", ref _desc, value == "" ? null : value); }
        }
    }
}
