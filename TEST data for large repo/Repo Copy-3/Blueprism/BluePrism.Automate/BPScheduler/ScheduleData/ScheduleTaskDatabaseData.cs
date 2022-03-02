using BluePrism.BPCoreLib.Data;

namespace BluePrism.Scheduling.ScheduleData
{
    public class ScheduleTaskDatabaseData
    {
        public int Id { get; set; } = default(int);
        public string Name { get; set; }
        public string Description { get; set; }
        public bool FailFastOnError { get; set; }
        public int DelayAfterEnd { get; set; } = default(int);
        public int OnSuccess { get; set; } = default(int);
        public int OnFailure { get; set; } = default(int);

        public ScheduleTaskDatabaseData(IDataProvider dataProvider)
        {
            if (dataProvider == null)
                return;

            Id = dataProvider.GetInt(nameof(Id));
            Name = dataProvider.GetString(nameof(Name));
            Description = dataProvider.GetString(nameof(Description));
            FailFastOnError = dataProvider.GetBool(nameof(FailFastOnError));
            DelayAfterEnd = dataProvider.GetInt(nameof(DelayAfterEnd));
            OnSuccess = dataProvider.GetInt(nameof(OnSuccess));
            OnFailure = dataProvider.GetInt(nameof(OnFailure));
        }
    }
}