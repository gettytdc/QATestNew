namespace BluePrism.Api.Domain
{
    using System;
    using System.Collections.Generic;

    public class ResourcesSummaryUtilizationParameters
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public IReadOnlyCollection<Guid> ResourceIds { get; set; }
    }
}
