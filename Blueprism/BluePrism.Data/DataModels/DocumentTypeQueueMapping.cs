namespace BluePrism.Data.DataModels
{
    using System;
    using System.Runtime.Serialization;

    [Serializable, DataContract(Namespace = "bp")]
    public class DocumentTypeQueueMapping
    {
        [DataMember]
        public Guid DocumentTypeId { get; set; }

        [DataMember]
        public string DocumentTypeName { get; set; }

        [DataMember]
        public Guid Queue { get; set; }
    }
}