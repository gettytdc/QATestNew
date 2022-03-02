using BluePrism.Scheduling.Properties;
using System;
using System.Xml;
using System.Runtime.Serialization;

using BluePrism.BPCoreLib;
using BluePrism.BPCoreLib.Data;
using BluePrism.Scheduling.Calendar;
using BluePrism.Scheduling.ScheduleData;
using BluePrism.Server.Domain.Models;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Metadata regarding a trigger which gives a consistent way of representing
    /// data about a trigger to the outside world. This also allows a consistent
    /// way to create triggers from saved data.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "bp")]
    public class TriggerMetaData : IEquatable<TriggerMetaData>
    {
        #region - Member variables -

        [DataMember]
        private bool _userTrigger;

        [DataMember]
        private TriggerMode _mode;
        [DataMember]
        private int _priority;

        [DataMember]
        private DateTime _start;
        [DataMember]
        private DateTime _end;

        [DataMember]
        private string _timeZoneId;
        [DataMember]
        private TimeSpan? _utcOffset;
        [DataMember]
        private IntervalType _interval;
        [DataMember]
        private int _period;
        [DataMember]
        private DaySet _days;
        [DataMember]
        private int _calendarId;
        [DataMember]
        private NthOfMonth _nth;
        [DataMember]
        private NthOfWeek _nthOfWeek;
        [DataMember]
        private NonExistentDatePolicy _policy;

        /// <summary>
        /// The start time at which this trigger is valid. Only really relevant
        /// for 'Hourly' triggers which operate between certain hours.
        /// </summary>
        [DataMember]
        private TimeSpan _startTime;

        /// <summary>
        /// The end time at which this trigger is valid. Only really relevant
        /// for 'Hourly' triggers which operate between certain hours.
        /// </summary>
        [DataMember]
        private TimeSpan _endTime;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new trigger meta data object with default values set.
        /// </summary>
        public TriggerMetaData()
        {
            // Most of the defaults can be their natural defaults, but a
            // few need to be given some sane defaults
            _period = 1;
            _days = new DaySet();
            _start = DateTime.Now;
            _end = DateTime.MaxValue;
            _startTime = DateTime.Now.Date.TimeOfDay; // Midnight today
            _endTime = new TimeSpan(23, 59, 59); // 23:59:59 tonight
        }

        public TriggerMetaData(ScheduleTriggerDatabaseData triggerData)
        {
            _interval = triggerData.UnitType;
            _mode = triggerData.Mode;
            _priority = triggerData.Priority;
            _start = triggerData.StartDate;
            _end = triggerData.EndDate;
            _period = triggerData.Period;

            int startpoint = triggerData.StartPoint;
            int endpoint = triggerData.EndPoint;
            _startTime = (startpoint == -1 ? TimeSpan.Zero : TimeSpan.FromSeconds(startpoint));
            _endTime = (endpoint == -1 ? TimeSpan.MaxValue : TimeSpan.FromSeconds(endpoint));

            _days = triggerData.DaySet;
            _calendarId = triggerData.CalendarId;
            _nth = triggerData.NthOfMonth;
            _policy = triggerData.MissingDatePolicy;
            _userTrigger = triggerData.UserTrigger;
            _timeZoneId = triggerData.TimeZoneId;
            _utcOffset = triggerData.UtcOffset;
        }

        public TriggerMetaData(IDataProvider prov)
        {
            _interval = prov.GetValue("unittype", IntervalType.Never);
            _mode = prov.GetValue("mode", TriggerMode.Indeterminate);
            _priority = prov.GetValue("priority", 1);
            _start = prov.GetValue("startdate", DateTime.MinValue);
            _end = prov.GetValue("enddate", DateTime.MaxValue);
            _period = prov.GetValue("period", 0);

            int startpoint = prov.GetValue("startpoint", -1);
            int endpoint = prov.GetValue("endpoint", -1);
            _startTime = (startpoint == -1 ? TimeSpan.Zero : TimeSpan.FromSeconds(startpoint));
            _endTime = (endpoint == -1 ? TimeSpan.MaxValue : TimeSpan.FromSeconds(endpoint));

            _days = new DaySet(prov.GetValue("dayset", 0));
            _calendarId = prov.GetValue("calendarid", 0);
            _nth = prov.GetValue("nthofmonth", NthOfMonth.None);
            _policy = prov.GetValue("missingdatepolicy", NonExistentDatePolicy.Skip);
            _userTrigger = prov.GetValue("usertrigger", false);
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// Flag to indicate whether the trigger defined by this metadata is
        /// configurable by the user or not
        /// </summary>
        public bool IsUserTrigger
        {
            get { return _userTrigger; }
            set { _userTrigger = value; }
        }

        /// <summary>
        /// The type of interval represented by this trigger.
        /// </summary>
        public IntervalType Interval
        {
            get { return _interval; }
            set { _interval = value; }
        }

        /// <summary>
        /// The mode of this trigger, ie. fire / suppress / indeterminate.
        /// </summary>
        public TriggerMode Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        /// <summary>
        /// The priority of this trigger.
        /// </summary>
        public int Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        /// <summary>
        /// The start date and run time for this trigger.
        /// </summary>
        public DateTime Start
        {
            get { return _start; }
            set { _start = value; }
        }

        /// <summary>
        /// The end date / time for this trigger.
        /// </summary>
        public DateTime End
        {
            get { return _end; }
            set { _end = value; }
        }

        public string TimeZoneId
        {
            get { return _timeZoneId; }
            set { _timeZoneId = value; }
        }

        public TimeSpan? UtcOffset
        {
            get { return _utcOffset; }
            set { _utcOffset = value; }
        }

        public TimeZoneInfo TimeZone => string.IsNullOrWhiteSpace(TimeZoneId) ? null : TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);

        /// <summary>
        /// The number of time periods between trigger activations. The type of
        /// time period is determined by the <see cref="Interval"/>.
        /// </summary>
        public int Period
        {
            get { return _period; }
            set { _period = value; }
        }

        /// <summary>
        /// The range of allowed hours for this trigger - only really relevant
        /// for 'Hourly' triggers which operate between certain hours.
        /// </summary>
        public clsTimeRange AllowedHours
        {
            get { return new clsTimeRange(_startTime, _endTime); }
            set { _startTime = value.StartTime; _endTime = value.EndTime; }
        }

        /// <summary>
        /// The set of days on which this trigger is activated. Used in weekly
        /// triggers to run on (say) Tuesdays... or monthly triggers to run
        /// on (say) the second Wednesday of the month.
        /// </summary>
        public DaySet Days
        {
            get { return _days; }
            set { _days = value; }
        }

        /// <summary>
        /// The ID of the calendar used by this trigger.
        /// </summary>
        public int CalendarId
        {
            get { return _calendarId; }
            set { _calendarId = value; }
        }

        /// <summary>
        /// Which occurrence of the month is this trigger to use.
        /// </summary>
        public NthOfMonth Nth
        {
            get { return _nth; }
            set { _nth = value; }
        }

        public NthOfWeek NthOfWeek
        {
            get { return _nthOfWeek; }
            set { _nthOfWeek = value; }
        }

        /// <summary>
        /// The policy of this trigger if it is set to activate on a day within
        /// a month for which that day does not exist.
        /// </summary>
        public NonExistentDatePolicy MissingDatePolicy
        {
            get { return _policy; }
            set { _policy = value; }
        }
        #endregion

        #region - Object overrides and IEquatable implementation -

        /// <summary>
        /// Checks if this meta data is equal to the given metadata.
        /// True if the other metadata is not null and all its values are the
        /// same as this object's values.
        /// </summary>
        /// <param name="other">The meta data to check for equality against.
        /// </param>
        /// <returns>True if the other metadata is not null and matches this
        /// metadata object in value exactly; false otherwise.</returns>
        public bool Equals(TriggerMetaData other)
        {
            return Equals(other, null);
        }

        /// <summary>
        /// Checks if this meta data is equal to the given metadata.
        /// True if the other metadata is not null and all its values are the
        /// same as this object's values - this override checks the calendar's
        /// value rather than the calendar's ID, since the value can be
        /// retrieved from the supplied tore.
        /// </summary>
        /// <param name="other">The meta data to check for equality against.
        /// </param>
        /// <param name="store">The store from where the calendar data can
        /// be retrieved.</param>
        /// <returns>True if the other metadata is not null and matches this
        /// metadata object in value exactly; false otherwise.</returns>
        public bool Equals(TriggerMetaData other, IScheduleCalendarStore store)
        {
            return other != null &&
                _interval == other._interval &&
                _mode == other._mode &&
                _priority == other._priority &&
                _start == other._start &&
                _end == other._end &&
                _period == other._period &&
                _days == other._days &&
                // If we have a store, check calendar valuess
                // otherwise, just check the calendar IDs.
                (store != null
                    ? Object.Equals(GetCalendar(store), other.GetCalendar(store))
                    : _calendarId == other._calendarId
                ) &&
                _nth == other._nth &&
                _policy == other._policy;
        }

        /// <summary>
        /// Checks if this meta data is equal to the given object.
        /// True if the other object is a non-null metadata instance and all
        /// its values are the same as this object's values.
        /// </summary>
        /// <param name="other">The object to check for equality against.
        /// </param>
        /// <returns>true if the other object is an instance of TriggerMetaData
        /// which matches this metadata object in value exactly; false otherwise.
        /// </returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as TriggerMetaData);
        }

        /// <summary>
        /// Gets an integer hash for this object.
        /// The hash created is a function of all values in this metadata.
        /// </summary>
        /// <returns>An integer hash for this trigger metadata object.
        /// </returns>
        public override int GetHashCode()
        {
            return _interval.GetHashCode() ^ _mode.GetHashCode() ^ _priority ^
                _start.GetHashCode() ^ _end.GetHashCode() ^ _period ^
                (_days == null ? 0 : _days.GetHashCode()) ^
                _calendarId ^ _nth.GetHashCode() ^ _policy.GetHashCode();
        }
        #endregion

        #region - Class specific methods -

        /// <summary>
        /// Gets the calendar used by the trigger represented by this
        /// metadata from the supplied store.
        /// </summary>
        /// <param name="store">The store from where the calendar can be
        /// loaded</param>
        /// <returns>Theh calendar corresponding to this trigger.</returns>
        /// <exception cref="NoSuchElementException">If no calendar with the
        /// specified ID was found in the given store.</exception>
        public ScheduleCalendar GetCalendar(IScheduleCalendarStore store)
        {
            if (_calendarId == 0)
                return null;
            ScheduleCalendar cal = store.GetCalendar(_calendarId);
            if (cal == null)
                throw new NoSuchElementException(Resources.NoCalendarFoundWithID0, _calendarId);
            return cal;
        }

        /// <summary>
        /// Writes this trigger out to the given XML Writer.
        /// </summary>
        /// <param name="writer">The writer to which this trigger should be written.
        /// </param>
        public void ToXml(XmlWriter writer, IScheduleStore store)
        {
            writer.WriteStartElement("trigger");

            if (_userTrigger)
                writer.WriteAttributeString("user-trigger", XmlConvert.ToString(true));

            if (_mode != TriggerMode.Fire) // treate 'fire' as default mode
                writer.WriteAttributeString("mode", ((int)_mode).ToString());
            if (_priority != 1) // treat 1 as default priority
                writer.WriteAttributeString("priority", _priority.ToString());

            if (_interval != IntervalType.Once)
                writer.WriteAttributeString("unit-type", ((int)_interval).ToString());

            if (_start != DateTime.MinValue)
                writer.WriteAttributeString("start-date", _start.ToString("u"));
            if (_end != DateTime.MaxValue)
                writer.WriteAttributeString("end-date", _end.ToString("u"));
            if (_startTime != TimeSpan.Zero)
                writer.WriteAttributeString("start-point", ((int)_startTime.TotalSeconds).ToString());
            if (_endTime != TimeSpan.Zero)
                writer.WriteAttributeString("end-point", ((int)_endTime.TotalSeconds).ToString());
            if (_period != 1)
                writer.WriteAttributeString("period", _period.ToString());
            if (!_days.IsEmpty())
                writer.WriteAttributeString("days", _days.ToInt().ToString());
            if (_calendarId != 0)
                writer.WriteAttributeString("calendar", GetCalendar(store).Name);
            if (_nth != NthOfMonth.None)
                writer.WriteAttributeString("nth", ((int)_nth).ToString());
            if (_policy != NonExistentDatePolicy.Skip)
                writer.WriteAttributeString("policy", ((int)_policy).ToString());

            writer.WriteEndElement(); // trigger
        }

        /// <summary>
        /// Checks if the given input is null, returning the specified default if it
        /// is. Otherwise, the given input is converted to the required type and
        /// returned.
        /// </summary>
        /// <param name="input">The input string to check.</param>
        /// <param name="defaultVal">The default value to return if the given string
        /// is null.</param>
        /// <returns>The given string converted into the specified type, or the
        /// default value if the string is null.</returns>
        private static T IfNull<T>(string input, T defaultVal)
        {
            return (input == null
                ? defaultVal
                : (T)Convert.ChangeType(input, typeof(T))
            );
        }

        /// <summary>
        /// Special case for IfNull which parses the date exactly rather than changing
        /// the type using the Convert class. The dates used in the triggers are
        /// currently of 'unspecified' kind, so no timezone conversion should take
        /// place - <c>Convert.ChangeType</c> seems to convert it to a local time
        /// automagically which can break the trigger if it is being imported.
        /// </summary>
        /// <param name="input">The input string to convert to a date.</param>
        /// <param name="defaultVal">The default value to return if the given input
        /// string is null.</param>
        /// <returns>The given string converted into a DateTime or the default value
        /// if the string is null</returns>
        private static DateTime IfNull(string input, DateTime defaultVal)
        {
            if (input == null)
                return defaultVal;
            return DateTime.ParseExact(input, "u", null);
        }

        /// <summary>
        /// Reads the trigger meta data from the given XML reader.
        /// </summary>
        /// <param name="r">The reader from which to draw the trigger meta data,
        /// er, data. This trigger metadata class will move the reader to the content,
        /// which it expects to be a trigger element, and read that, not moving the
        /// reader on any further.</param>
        /// <param name="store">The store from which calendars can be drawn.</param>
        public static TriggerMetaData FromXml(XmlReader r, IScheduleCalendarStore store)
        {
            r.MoveToContent();
            if (r.NodeType != XmlNodeType.Element || r.LocalName != "trigger")
            {
                throw new BluePrismException(
                    Resources.CannotReadATriggerFromANodeOfType0Named1,
                    r.NodeType, r.LocalName);
            }

            TriggerMetaData md = new TriggerMetaData();

            md._userTrigger = IfNull(r["user-trigger"], false);
            md._mode = (TriggerMode)IfNull(r["mode"], (int)TriggerMode.Fire);
            md._priority = IfNull(r["priority"], 1);
            md._interval = (IntervalType)IfNull(r["unit-type"], (int)IntervalType.Once);
            md._start = IfNull(r["start-date"], DateTime.MinValue);
            md._end = IfNull(r["end-date"], DateTime.MaxValue);
            md._startTime = TimeSpan.FromSeconds(IfNull(r["start-point"], 0));
            md._endTime = TimeSpan.FromSeconds(IfNull(r["end-point"], (23 * 60 * 60) + (59 * 60) + 59));
            md._period = IfNull(r["period"], 1);
            md._days = new DaySet(IfNull(r["days"], 0));
            string calendarName = r["calendar"];
            if (calendarName != null)
                md._calendarId = store.GetCalendar(calendarName).Id;
            md._nth = (NthOfMonth)IfNull(r["nth"], (int)NthOfMonth.None);
            md._policy = (NonExistentDatePolicy)IfNull(r["policy"], (int)NonExistentDatePolicy.Skip);

            return md;
        }

        #endregion

    }
}
