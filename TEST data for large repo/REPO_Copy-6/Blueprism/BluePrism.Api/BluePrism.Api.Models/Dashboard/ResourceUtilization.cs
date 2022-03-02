namespace BluePrism.Api.Models.Dashboard
{
    using System;
    using System.Collections.Generic;

    public class ResourceUtilization
    {
        public Guid ResourceId { get; set; }
        public string DigitalWorkerName { get; set; }
        public DateTimeOffset UtilizationDate { get; set; }
        public IEnumerable<int> Usages { get; set; }
    }
}
