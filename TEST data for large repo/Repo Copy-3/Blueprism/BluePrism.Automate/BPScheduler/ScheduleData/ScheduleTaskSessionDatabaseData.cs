using System;
using BluePrism.BPCoreLib.Data;

namespace BluePrism.Scheduling.ScheduleData
{
    public class ScheduleTaskSessionDatabaseData
    {
        public int TaskId { get; set; } = default(int);
        public int Id { get; set; } = default(int);
        public Guid ProcessId { get; set; } = default(Guid);
        public string ResourceName { get; set; }
        public string ProcessParams { get; set; }
        public Guid ResourceId { get; set; } = default(Guid);
        public bool CanCurrentUserSeeProcess { get; set; }
        public bool CanCurrentUserSeeResource { get; set; }

        public ScheduleTaskSessionDatabaseData(IDataProvider dataProvider)
        {
            if (dataProvider == null)
                return;

            TaskId = dataProvider.GetInt(nameof(TaskId), 0);
            Id = dataProvider.GetInt(nameof(Id), 0);
            ProcessId = dataProvider.GetValue(nameof(ProcessId), Guid.Empty);
            ResourceName = dataProvider.GetString(nameof(ResourceName));
            ProcessParams = dataProvider.GetString(nameof(ProcessParams));
            ResourceId = dataProvider.GetValue(nameof(ResourceId), Guid.Empty);
        }
    }
}