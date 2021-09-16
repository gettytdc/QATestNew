namespace BluePrism.WorkQueueAnalysis.Classes
{
    public class SnapshotActionsRequired
    {
        public bool CreateTriggersInDatabase { get; set; }
        public bool ExecuteStoredProcedure { get; set; }
        public bool ClearOrphanedSnapshots { get; set; }

        public bool AnyActionsRequired => ExecuteStoredProcedure || ClearOrphanedSnapshots || CreateTriggersInDatabase;
    }
}