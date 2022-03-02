namespace BluePrism.Api.Domain
{
    public class ScheduledTask
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool FailFastOnError { get; set; }
        public int DelayAfterEnd { get; set; }
        public int OnSuccessTaskId { get; set; }
        public string OnSuccessTaskName { get; set; }
        public int OnFailureTaskId { get; set; }
        public string OnFailureTaskName { get; set; }
    }
}
