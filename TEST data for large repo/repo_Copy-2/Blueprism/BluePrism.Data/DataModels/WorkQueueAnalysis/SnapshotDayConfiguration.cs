using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BluePrism.Data.DataModels.WorkQueueAnalysis
{
    [Serializable]
    [DataContract(Namespace = "bp")]
    public class SnapshotDayConfiguration
    {
        [DataMember]
        private bool _monday;
        [DataMember]
        private bool _tuesday;
        [DataMember]
        private bool _wednesday;
        [DataMember]
        private bool _thursday;
        [DataMember]
        private bool _friday;
        [DataMember]
        private bool _saturday;
        [DataMember]
        private bool _sunday;

        public bool Monday
        {
            get => _monday;
            set => _monday = value;
        }

        public bool Tuesday
        {
            get => _tuesday;
            set => _tuesday = value;
        }

        public bool Wednesday
        {
            get => _wednesday;
            set => _wednesday = value;
        }

        public bool Thursday
        {
            get => _thursday;
            set => _thursday = value;
        }

        public bool Friday
        {
            get => _friday;
            set => _friday = value;
        }

        public bool Saturday
        {
            get => _saturday;
            set => _saturday = value;
        }

        public bool Sunday
        {
            get => _sunday;
            set => _sunday = value;
        }

        public SnapshotDayConfiguration(bool monday, bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday, bool sunday)
        {
            _monday = monday;
            _tuesday = tuesday;
            _wednesday = wednesday;
            _thursday = thursday;
            _friday = friday;
            _saturday = saturday;
            _sunday = sunday;
        }

        public bool IsEmpty => Monday.Equals(false) && Tuesday.Equals(false) &&
                Wednesday.Equals(false) && Thursday.Equals(false) &&
                Friday.Equals(false) && Saturday.Equals(false) && Sunday.Equals(false);

        public List<int> GetDaysMap()
        {
            List<int> days = new List<int>();
            if (_monday) days.Add(1);
            if (_tuesday) days.Add(2);
            if (_wednesday) days.Add(3);
            if (_thursday) days.Add(4);
            if (_friday) days.Add(5);
            if (_saturday) days.Add(6);
            if (_sunday) days.Add(7);
            return days;
        }

        public bool IsEqualTo(SnapshotDayConfiguration configToCompare)
        {
            if (_monday != configToCompare.Monday || _tuesday != configToCompare.Tuesday ||
                 _wednesday != configToCompare.Wednesday || _thursday != configToCompare.Thursday ||
                 _friday != configToCompare.Friday || _saturday != configToCompare.Saturday ||
                 _sunday != configToCompare.Sunday)
            {
                return false;
            }
            return true;

        }
    }
}
