namespace BluePrism.Api.Models
{
    public class ScheduleParameters : IPagingModel<string>
    {
        public StartsWithStringFilterModel Name { get; set; }

        public CommaDelimitedCollection<RetirementStatus> RetirementStatus { get; set; }

        public int? ItemsPerPage { get; set; }

        public PagingTokenModel<string> PagingToken { get;set; }
    }
}
