namespace BluePrism.Api.Mappers.Dashboard
{
    using System.Collections.Generic;
    using System.Linq;
    using Models.Dashboard;

    public static class ResourceUtilizationModelMapper
    {  
        public static IEnumerable<ResourceUtilization> ToModel(this IEnumerable<Domain.Dashboard.ResourceUtilization> @this) =>
            @this.Select(x => x.ToModel());

        public static ResourceUtilization ToModel(this Domain.Dashboard.ResourceUtilization @this) =>
            new ResourceUtilization()
            {
                ResourceId = @this.ResourceId,
                DigitalWorkerName = @this.DigitalWorkerName,
                UtilizationDate = @this.UtilizationDate,
                Usages = @this.Usages
            };
    }
}
