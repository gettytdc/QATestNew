namespace BluePrism.Api.Mappers
{
    using System;
    using System.Linq;
    using Domain;
    using Domain.Filters;
    using Domain.PagingTokens;
    using FilterMappers;
    using Func;
    using static Func.OptionHelper;

    public static class ScheduleParametersMapper
    {
        private const int DefaultItemsPerPage = 10;

        public static ScheduleParameters ToDomainObject(this Models.ScheduleParameters @this) =>
            new ScheduleParameters
            {
                ItemsPerPage = @this.ItemsPerPage ?? DefaultItemsPerPage,
                PagingToken = @this.PagingToken.ToDomainPagingToken(),
                Name = @this.Name.ToDomain(),
                RetirementStatus = @this.RetirementStatus?.Select(x => x.ToDomain()).ToMultiValueFilter()
                                   ?? new MultiValueFilter<RetirementStatus>(new Filter<RetirementStatus>[0])
            };

        private static RetirementStatus ToDomain(this Models.RetirementStatus retirementStatus)
        {
            switch (retirementStatus)
            {
                case Models.RetirementStatus.Active:
                    return RetirementStatus.Active;
                case Models.RetirementStatus.Retired:
                    return RetirementStatus.Retired;
                default:
                    throw new ArgumentException($"Unable to map retirement status value {retirementStatus}");
            }
        }
    }
}
