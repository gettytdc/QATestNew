namespace BluePrism.Api.Models
{
    using System;
    using System.Collections.Generic;

    public class ResourceModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid PoolId { get; set; }
        public string PoolName { get; set; }
        public Guid GroupId { get; set; }
        public string GroupName { get; set; }
        public IEnumerable<ResourceAttribute> Attributes { get; set; }
        public int ActiveSessionCount { get; set; }
        public int WarningSessionCount { get; set; }
        public int PendingSessionCount { get; set; }
        public ResourceDbStatus DatabaseStatus { get; set; }
        public ResourceDisplayStatus DisplayStatus { get; set; }
    }
}
