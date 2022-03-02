namespace BluePrism.Api.Mappers.Dashboard
{
    using System.Collections.Generic;
    using System.Linq;
    using Models.Dashboard;

    public static class ResourcesSummaryUtilizationModelMapper
    {
        public static IEnumerable<ResourcesSummaryUtilization> ToModel(this IEnumerable<Domain.Dashboard.ResourcesSummaryUtilization> @this) =>
            @this.Select(ToModel);

        public static ResourcesSummaryUtilization ToModel(this Domain.Dashboard.ResourcesSummaryUtilization @this) =>
            new ResourcesSummaryUtilization()
            {
                Dates = @this.Dates,
                Usage = @this.Usage
            };
    }
}
