namespace BluePrism.Api.BpLibAdapters.Mappers
{
    using System;
    using Server.Domain.Models;

    public static class ResourceUtilizationParametersMapper
    {
        public static ResourceUtilizationParameters ToBluePrismObject(this Domain.ResourceUtilizationParameters resourceUtilizationParameters) =>
            new ResourceUtilizationParameters
            {
                StartDate = resourceUtilizationParameters.StartDate,
                ResourceIds = resourceUtilizationParameters.ResourceIds ?? new Guid[] { },
                PageNumber = resourceUtilizationParameters.PageNumber,
                PageSize = resourceUtilizationParameters.PageSize
            };
    }
}
