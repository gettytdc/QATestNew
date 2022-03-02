using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BluePrism.ClientServerResources.Core.Enums;

namespace BluePrism.ClientServerResources.Core.Data
{
    [Serializable]
    [DataContract(Namespace = "bp")]
    public class ResourcesChangedData
    {
        // The overall status change accumulated from all changes in all resources
        [DataMember]
        public ResourceStatusChange OverallChange { get; set; }

        // The individual status changes keyed on resource name
        [DataMember]
        public IDictionary<string, ResourceStatusChange> Changes { get; set; }

        public ResourcesChangedData(ResourceStatusChange overall, IDictionary<string, ResourceStatusChange> theChanges)
        {
            OverallChange = overall;
            Changes = theChanges is null ? new Dictionary<string, ResourceStatusChange>() : theChanges;
        }
    }
}
