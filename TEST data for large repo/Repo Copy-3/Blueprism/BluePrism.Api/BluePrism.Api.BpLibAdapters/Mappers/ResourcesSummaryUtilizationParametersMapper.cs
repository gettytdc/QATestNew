namespace BluePrism.Api.BpLibAdapters.Mappers
{
    using System;
    using Server.Domain.Models;


    public static class ResourcesSummaryUtilizationParametersMapper
    {
        public static ResourcesSummaryUtilizationParameters ToBluePrismObject(this Domain.ResourcesSummaryUtilizationParameters resourcesSummaryUtilizationParameters) =>
            new ResourcesSummaryUtilizationParameters()
            {
                StartDate = resourcesSummaryUtilizationParameters.StartDate,
                EndDate = resourcesSummaryUtilizationParameters.EndDate,
                ResourceIds = resourcesSummaryUtilizationParameters.ResourceIds ?? new Guid[] { }
            };
    }
}
