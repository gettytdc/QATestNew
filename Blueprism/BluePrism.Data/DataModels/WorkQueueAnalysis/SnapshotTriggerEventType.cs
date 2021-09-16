using System;
using System.Runtime.Serialization;

namespace BluePrism.Data.DataModels.WorkQueueAnalysis
{
    [Flags]
    [DataContract(Namespace = "bp")]
    public enum SnapshotTriggerEventType
    {
        [EnumMember(Value = "0")]
        None = 0,
        [EnumMember(Value = "1")]
        Snapshot = 1,
        [EnumMember(Value = "2")]
        InterimSnapshot = 2,
        [EnumMember(Value = "4")]
        Trend = 4
    }
}