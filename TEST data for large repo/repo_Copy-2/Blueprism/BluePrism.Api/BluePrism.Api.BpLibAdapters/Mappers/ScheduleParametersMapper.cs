namespace BluePrism.Api.BpLibAdapters.Mappers
{
    using System;
    using FilterMappers;
    using Func;
    using Server.Domain.Models;
    using Server.Domain.Models.Pagination;
    using ScheduleParameters = Server.Domain.Models.ScheduleParameters;

    using static Func.OptionHelper;

    public static class ScheduleParametersMapper
    {
        public static ScheduleParameters ToBluePrismObject(this Domain.ScheduleParameters @this) =>
            new ScheduleParameters
            {
                Name = @this.Name.ToBluePrismObject(),
                RetirementStatus = @this.RetirementStatus.ToBluePrismObject(x => x.ToBluePrism()),
                ItemsPerPage = @this.ItemsPerPage,
                PagingToken = @this.PagingToken.ToBluePrismPagingToken()
            };

        private static Option<SchedulePagingToken> ToBluePrismPagingToken(this Option<Domain.PagingTokens.PagingToken<string>> pagingToken)
        {
            switch (pagingToken)
            {
                case Some<Domain.PagingTokens.PagingToken<string>> t:
                    return Some(new SchedulePagingToken
                    {
                        PreviousIdValue = t.Value.PreviousIdValue,
                        DataType = t.Value.DataType,
                    });
                default:
                    return None<SchedulePagingToken>();
            }
        }

        private static RetirementStatus ToBluePrism(this Domain.RetirementStatus retirementStatus)
        {
            switch (retirementStatus)
            {
                case Domain.RetirementStatus.Active:
                    return RetirementStatus.Active;
                case Domain.RetirementStatus.Retired:
                    return RetirementStatus.Retired;
                default:
                    throw new ArgumentException($"Unable to map retirement status {retirementStatus}");
            }
        }
    }
}
