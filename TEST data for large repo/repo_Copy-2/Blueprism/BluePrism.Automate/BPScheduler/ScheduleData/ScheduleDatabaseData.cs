using System.Collections.Generic;
using BluePrism.BPCoreLib.Data;

namespace BluePrism.Scheduling.ScheduleData
{
    public class ScheduleDatabaseData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int InitialTaskId { get; set; }
        public int VersionNumber { get; set; }
        public bool Retired { get; set; }

        public List<ScheduleTaskDatabaseData> Tasks { get; set; } = new List<ScheduleTaskDatabaseData>();
        public List<ScheduleTaskSessionDatabaseData> TaskSessions { get; set; } = new List<ScheduleTaskSessionDatabaseData>();
        public List<ScheduleTriggerDatabaseData> Triggers { get; set; } = new List<ScheduleTriggerDatabaseData>();

        public ScheduleDatabaseData(IDataProvider dataProvider)
        {
            if (dataProvider == null)
                return;

            Id = dataProvider.GetInt(nameof(Id));
            Name = dataProvider.GetString(nameof(Name));
            Description = dataProvider.GetString(nameof(Description));
            InitialTaskId = dataProvider.GetInt(nameof(InitialTaskId));
            VersionNumber = dataProvider.GetInt(nameof(VersionNumber));
            Retired = dataProvider.GetBool(nameof(Retired));
        }
    }
}