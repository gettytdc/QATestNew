using System.Globalization;
using System.Resources;

namespace BluePrism.Data.DataModels.WorkQueueAnalysis
{
    public class SnapshotConfigurationViewModel
    {
        private const string DisplayPattern = "HH:mm";

        private readonly ResourceManager _resourceManager = WorkQueueAnalysis.WorkQueueAnalysis_Resources.ResourceManager; 

        public string Name { get; set; }
        public bool Enabled { get; set; }
        public string Interval { get; set; }
        public string Timezone { get; set; }
        public string Starttime { get; set; }
        public string Endtime { get; set; }
        public bool Sunday { get; set; }
        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }

        public SnapshotConfigurationViewModel(SnapshotConfiguration configuration)
        {
            Name = configuration.Name;
            Enabled = configuration.Enabled;
            Interval = configuration.Interval.ToLocalizedString(_resourceManager);
            Timezone = configuration.Timezone.DisplayName;
            Starttime = configuration.StartTime.ToString(DisplayPattern, CultureInfo.InvariantCulture);
            Endtime = configuration.EndTime.ToString(DisplayPattern, CultureInfo.InvariantCulture);
            Sunday = configuration.DaysOfTheWeek.Sunday;
            Monday = configuration.DaysOfTheWeek.Monday;
            Tuesday = configuration.DaysOfTheWeek.Tuesday;
            Wednesday = configuration.DaysOfTheWeek.Wednesday;
            Thursday = configuration.DaysOfTheWeek.Thursday;
            Friday = configuration.DaysOfTheWeek.Friday;
            Saturday = configuration.DaysOfTheWeek.Saturday;
        }
    }
}
