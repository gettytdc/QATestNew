using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BluePrism.Data.DataModels.WorkQueueAnalysis
{
    [Serializable, DataContract(Namespace = "bp")]
    [KnownType(typeof(TimeZoneInfo.AdjustmentRule))]
    [KnownType(typeof(TimeZoneInfo.AdjustmentRule[]))]
    [KnownType(typeof(TimeZoneInfo.TransitionTime))]
    [KnownType(typeof(DayOfWeek))]
    public class WorkQueueSnapshotInformation
    {
        [DataMember] private int _queueidentifier;
        [DataMember] private long _lastSnapshotId;
        [DataMember] private TimeZoneInfo _timezone;
        [DataMember] private List<SnapshotTriggerInformation> _snapshotsToProcess;

        public WorkQueueSnapshotInformation(int queueIdent, long lastSnapshotId, TimeZoneInfo timeZoneInfo)
        {
            _queueidentifier = queueIdent;
            _lastSnapshotId = lastSnapshotId;
            _timezone = timeZoneInfo;
        }

        public int QueueIdentifier
        {
            get => _queueidentifier;
            set => _queueidentifier = value;
        }

        public long LastSnapshotId
        {
            get => _lastSnapshotId;
            set => _lastSnapshotId = value;
        }

        public TimeZoneInfo Timezone
        {
            get => _timezone;
            set => _timezone = value;
        }

        public List<SnapshotTriggerInformation> SnapshotIdsToProcess
        {
            get => _snapshotsToProcess as List<SnapshotTriggerInformation>;
            set => _snapshotsToProcess = value;
        }

        [Serializable, DataContract]
        public class SnapshotTriggerInformation
        {
            [DataMember] private long _snapshotId;
            [DataMember] private DateTimeOffset _snapshotTimeOffset;
            [DataMember] private SnapshotTriggerEventType _eventType;

            public long SnapshotId
            {
                get => _snapshotId;
                set => _snapshotId = value;
            }

            public DateTimeOffset SnapshotTimeOffset
            {
                get => _snapshotTimeOffset;
                set => _snapshotTimeOffset = value;
            }

            public SnapshotTriggerEventType EventType
            {
                get => _eventType;
                set => _eventType = value;
            }
        }
    }
}