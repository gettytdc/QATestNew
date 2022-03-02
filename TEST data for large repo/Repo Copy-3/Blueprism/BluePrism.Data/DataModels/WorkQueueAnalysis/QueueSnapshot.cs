using System;
using System.Runtime.Serialization;
using NodaTime;

namespace BluePrism.Data.DataModels.WorkQueueAnalysis
{
    [Serializable, DataContract(Namespace = "bp")]
    public class QueueSnapshot
    {
        [DataMember]
        private long _snapshotId;
        [DataMember]
        private int _queueIdentifier;
        [DataMember]
        private LocalTime _timeOfDay;
        [DataMember]
        private IsoDayOfWeek _dayOfWeek;
        [DataMember]
        private SnapshotInterval _interval;
        [DataMember]
        private SnapshotTriggerEventType _eventType;

        public long SnapshotId
        {
            get => _snapshotId;
            set => _snapshotId = value;
        }

        public int QueueIdentifier
        {
            get => _queueIdentifier;
            set => _queueIdentifier = value;
        }

        public LocalTime TimeOfDay
        {
            get => _timeOfDay;
            set => _timeOfDay = value;
        }

        public IsoDayOfWeek DayOfWeek
        {
            get => _dayOfWeek;
            set => _dayOfWeek = value;
        }

        public SnapshotInterval Interval
        {
            get => _interval;
            set => _interval = value;
        }

        public SnapshotTriggerEventType EventType
        {
            get => _eventType;
            set => _eventType = value;
        }

        public QueueSnapshot(long snapshotId, int queueIdentifier, LocalTime timeOfDay, IsoDayOfWeek dayOfWeek, SnapshotInterval interval, SnapshotTriggerEventType eventType)
        {
            _snapshotId = snapshotId;
            _queueIdentifier = queueIdentifier;
            _timeOfDay = timeOfDay;
            _dayOfWeek = dayOfWeek;
            _interval = interval;
            _eventType = eventType;
        }
    }
}