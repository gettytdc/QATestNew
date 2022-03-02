namespace BluePrism.Data.DataModels.WorkQueueAnalysis
{
    public static class WorkQueueAnalysisConstants
    {
        public const string SnapshotLockName = "__WorkQueueSnapshot__";
        public const string ConfiguredSnapshotsCacheKey = "ConfiguredSnapshots";
        public const string WorkQueueAnalysisEnvironmentLockExpiryKey = "EnvironmentLockTimeExpiry.WorkQueueAnalysis.InSeconds";

        public const int DefaultConfiguredSnapshotsCacheExpiryInMinutes = 10;
        public const int DefaultConfiguredSnapshotsPollIntervalInSeconds = 30;
    }
}