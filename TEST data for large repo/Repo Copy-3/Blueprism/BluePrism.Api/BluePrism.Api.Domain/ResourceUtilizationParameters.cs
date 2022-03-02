namespace BluePrism.Api.Domain
{
    using System;
    using System.Collections.Generic;

    public class ResourceUtilizationParameters
    {
        public DateTime StartDate { get; set; }
        public IReadOnlyCollection<Guid> ResourceIds { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
    }
}
