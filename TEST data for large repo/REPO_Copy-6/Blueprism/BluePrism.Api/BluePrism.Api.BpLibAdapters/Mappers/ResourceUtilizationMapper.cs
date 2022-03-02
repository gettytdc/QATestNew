namespace BluePrism.Api.BpLibAdapters.Mappers
{
    using Domain.Dashboard;

    public static class ResourceUtilizationMapper
    {
        public static ResourceUtilization ToDomainObject(this Server.Domain.Models.Dashboard.ResourceUtilization resourceUtilization) =>
            new ResourceUtilization
            {
                ResourceId = resourceUtilization.ResourceId,
                UtilizationDate = resourceUtilization.UtilizationDate,
                DigitalWorkerName = resourceUtilization.DigitalWorkerName,
                Usages = resourceUtilization.Usages
            };
    }
}
