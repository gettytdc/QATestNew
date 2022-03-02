namespace BluePrism.Api.BpLibAdapters.Mappers
{
    public static class ResourcesSummaryUtilizationMapper
    {
        public static Domain.Dashboard.ResourcesSummaryUtilization ToDomainObject(this Server.Domain.Models.Dashboard.ResourcesSummaryUtilization resourcesSummaryUtilization) =>
            new Domain.Dashboard.ResourcesSummaryUtilization
            {
                Dates = resourcesSummaryUtilization.Dates,
                Usage = resourcesSummaryUtilization.Usage
            };
    }
}
