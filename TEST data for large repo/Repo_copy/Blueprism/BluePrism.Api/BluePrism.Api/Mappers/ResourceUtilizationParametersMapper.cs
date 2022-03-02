namespace BluePrism.Api.Mappers
{
    using System;
    using Models;

    public static class ResourceUtilizationParametersMapper
    {
        public static Domain.ResourceUtilizationParameters ToDomainObject(this ResourceUtilizationParameters resourceUtilizationParameters) =>
            new Domain.ResourceUtilizationParameters
            {
                StartDate = resourceUtilizationParameters.StartDate,
                ResourceIds = resourceUtilizationParameters.ResourceIds ?? new Guid[] { },
                PageNumber = resourceUtilizationParameters.PageNumber,
                PageSize = resourceUtilizationParameters.PageSize,
            };
    }
}
