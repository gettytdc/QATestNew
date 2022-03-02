using System;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using BluePrism.BPCoreLib.Collections;
using System.Data;
using BluePrism.BPCoreLib.Data;
using System.Runtime.Serialization;
using LocaleTools;
using BluePrism.Scheduling.Properties;

namespace BluePrism.Scheduling.Calendar
{
    /// <summary>
    /// Class which represents a calendar within the scheduler.
    /// The calendar is primarily responsible for inhibiting the running of any
    /// jobs on specified days.
    /// It does this primarily with 3 components :
    /// <list>
    /// <entry>Working Week - it contains a set of days which are considered
    /// 'working days' - jobs which are scheduled to run on any day which falls
    /// outside of these days will be inhibited by this calendar.</entry>
    /// <entry>Public holidays - it can optionally be set to inhibit on predefined
    /// public holidays. This class holds the public holiday group name which applies
    /// for this calendar. It also contains a collection of public holidays which
    /// should <em>not</em> inhibit any running, overriding the public holiday
    /// group which is currently set.</entry>
    /// <entry>Non-working days - specific dates which are set to inhibit running
    /// of scheduled jobs. These are full dates and are not algorithmically
    /// calculated.</entry>
    /// </list>
    /// </summary>
    [Serializable, DataContract(Namespace = "bp", IsReference = true),
     KnownType(typeof(clsSortedSet<PublicHoliday>)),
     KnownType(typeof(clsSortedSet<DateTime>))]
    public class ScheduleCalendar : DescribedNamedObject, ICalendar
    {
        #region - Temp ID handling -

        // Lock used when changing the __tempId static var
        private static object TempIdLock = new Object();

        // The last used temporary ID value - should be negative after it's been
        // used once (ie. the first temp ID will be -1)
        private static int __tempId;

        /// <summary>
        /// Gets a temp ID for a calendar - this is guaranteed to not have the same
        /// ID as another calendar within this app domain... well, until 2^31 calendars
        /// have been created anyway.
        /// </summary>
        /// <returns>A unique temp ID for use in a non-saved calendar.</returns>
        private static int GetTempId()
        {
            lock (TempIdLock)
            {
                return --__tempId;
            }
        }

        #endregion

        /// <summary>
        /// The unique identifier for this schedule calendar.
        /// </summary>
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        [DataMember]
        private int _id;

        /// <summary>
        /// The public holiday group representing the holidays mapped onto this
        /// calendar.
        /// </summary>
        public string PublicHolidayGroup
        {
            get { return _group; }
            set { _group = string.IsNullOrEmpty(value) ? null : value; }
        }
        [DataMember]
        private string _group;

        /// <summary>
        /// The overrides which force public holidays within the defined group
        /// to be treated as working days by this calendar.
        /// </summary>
        public IBPSet<PublicHoliday> PublicHolidayOverrides
        {
            get { return _overrides; }
            set { _overrides.Clear(); _overrides.Union(value); }
        }
        [DataMember]
        private IBPSet<PublicHoliday> _overrides;

        /// <summary>
        /// The enabled public holidays in this calendar - derived from the
        /// <paramref name="PublicHolidayGroup"/> that it is in, and the
        /// <paramref name="PublicHolidayOverrides"/> that are set in it.
        /// This will always be empty if this calendar has no public holiday
        /// schema loaded.
        /// </summary>
        public IBPSet<PublicHoliday> PublicHolidays
        {
            get
            {
                // if we have no schema or public holiday group, then by
                // definition we have no public holidays
                if (_schema == null || _group == null)
                    return GetEmpty.IBPSet<PublicHoliday>();

                // Otherwise, get the public holidays in the group that this
                // calendar is in - subtract the overrides and return the result
                IBPSet<PublicHoliday> hols =
                    new clsSet<PublicHoliday>(_schema.GetHolidays(_group));
                hols.Subtract(_overrides);

                return hols;
            }
        }


        /// <summary>
        /// The set of flags indicating the days which make up the working week.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Bug", "S4275:Getters and setters should access the expected fields", Justification = "_workingWeek is set using SetTo() method")]
        public DaySet WorkingWeek
        {
            get { return _workingWeek; }
            set { _workingWeek.SetTo(value); }
        }
        [DataMember]
        private DaySet _workingWeek;


        /// <summary>
        /// A collection of specific dates which are considered non-working days.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Bug", "S4275:Getters and setters should access the expected fields", Justification = "_nonWorkingDays is set using SetTo() method")]
        public IBPSet<DateTime> NonWorkingDays
        {
            get { return _nonWorkingDays; }
            set { _nonWorkingDays.Clear(); _nonWorkingDays.Union(value); }
        }
        [DataMember]
        private IBPSet<DateTime> _nonWorkingDays;

        /// <summary>
        /// The data set of public holidays to refer to in this calendar.
        /// </summary>
        [DataMember]
        private PublicHolidaySchema _schema;

        /// <summary>
        /// Creates a new empty schedule calendar.
        /// This has no public holidays assigned, no working week days set on
        /// and no non-working days configured.
        /// </summary>
        /// <param name="schema">The schema to use to get the public holidays
        /// defined in this calendar - this needs to be set if the
        /// <see cref="CanRun"/> method will be called on this calendar.</param>
        public ScheduleCalendar(PublicHolidaySchema schema)
        {
            _overrides = new clsSortedSet<PublicHoliday>();
            _workingWeek = new DaySet();
            _nonWorkingDays = new clsSortedSet<DateTime>();
            _schema = schema;
        }

        /// <summary>
        /// Writes this calendar out to the given XML Writer.
        /// </summary>
        /// <param name="xw">The XML writer to which this calendar should be written.
        /// </param>
        public void ToXml(XmlWriter xw)
        {
            xw.WriteStartElement("schedule-calendar");
            xw.WriteAttributeString("id", _id.ToString());
            xw.WriteAttributeString("name", Name);
            xw.WriteAttributeString("working-week", _workingWeek.ToInt().ToString());
            if (PublicHolidayGroup != null)
            {
                xw.WriteAttributeString("public-holiday-group", PublicHolidayGroup);
                if (_overrides.Count > 0)
                {
                    xw.WriteStartElement("public-holiday-overrides");
                    foreach (PublicHoliday hol in _overrides)
                    {
                        xw.WriteStartElement("public-holiday");
                        xw.WriteAttributeString("id", XmlConvert.ToString(hol.Id));
                        xw.WriteEndElement(); // public-holiday
                    }
                    xw.WriteEndElement(); // public-holiday-overrides
                }
            }
            if (_nonWorkingDays.Count > 0)
            {
                xw.WriteStartElement("other-holidays");
                foreach (DateTime dt in _nonWorkingDays)
                {
                    xw.WriteStartElement("other-date");
                    xw.WriteAttributeString("value", dt.ToString("yyyy-MM-dd"));
                    xw.WriteEndElement(); // other-date             
                }
                xw.WriteEndElement(); // other-holidays
            }
            xw.WriteEndElement(); // schedule-calendar
        }

        /// <summary>
        /// Loads an unsaved schedule calendar from the given XML reader, using
        /// the given store for its public holiday schema.
        /// </summary>
        /// <param name="r">The reader to read the XML data from. Note that this
        /// method will read </param>
        /// <param name="store">The calendar store from where the public holiday
        /// schema can be loaded.</param>
        /// <returns>A schedule calendar, with a temporary ID, not backed by any
        /// store which was loaded from the data given by the XML reader.</returns>
        public static ScheduleCalendar FromXml(XmlReader r, IScheduleCalendarStore store)
        {
            PublicHolidaySchema schema = store.GetSchema();
            ScheduleCalendar cal = new ScheduleCalendar(schema);

            while (r.Read())
            {
                if (r.NodeType == XmlNodeType.Element)
                {
                    switch (r.LocalName)
                    {
                        case "schedule-calendar":
                            cal.Id = GetTempId();
                            cal.Name = r["name"];
                            cal.WorkingWeek = new DaySet(XmlConvert.ToInt32(r["working-week"]));
                            cal.PublicHolidayGroup = r["public-holiday-group"];
                            break;

                        case "public-holiday":
                            // This ID is safe to use, since it's hardcoded into the
                            // DB script - ie. it's not an IDENTITY field - thus the
                            // ID in one database should be identical to that in another
                            cal.PublicHolidayOverrides.Add(
                                schema.GetHoliday(XmlConvert.ToInt32(r["id"]))
                            );
                            break;

                        case "other-date":
                            cal.NonWorkingDays.Add(
                                DateTime.ParseExact(r["value"], "yyyy-MM-dd", null)
                            );
                            break;
                    }
                }
            }
            return cal;
        }

        /// <summary>
        /// Checks if this calendar will allow running on the given date.
        /// </summary>
        /// <param name="dt">The date to check whether it is available
        /// according to this calendar.</param>
        /// <returns>true if this calendar does not inhibit the given date;
        /// false otherwise.</returns>
        public virtual bool CanRun(DateTime dt)
        {
            // calendar currently works at day level...
            // public holidays, overrides, working weeks and non-working days don't
            // go to any finer granularity than that... so all we care about
            // is the date component of the date time.
            dt = dt.Date;
            // first and quickest check - does it fall outside the working week?
            if (!_workingWeek[dt.DayOfWeek])
                return false;

            // no? Okay... does it fall on a non working day?
            if (_nonWorkingDays.Contains(dt))
                return false;

            // Finally - the longest one of the bunch, see if this is a public holiday
            if (_group != null && _schema.FindPublicHoliday(_group, dt, _overrides) != null)
                return false;

            // otherwise, happy happy joy joy
            return true;
        }

        /// <summary>
        /// Finds the enabled public holiday in this calendar which falls
        /// on the given date or null if there is none.
        /// </summary>
        /// <param name="dt">The date on which to look for public holidays,
        /// </param>
        /// <returns>A PublicHoliday which represents an enabled public holiday
        /// on the given date.</returns>
        public PublicHoliday FindPublicHoliday(DateTime dt)
        {
            if (_group == null)
                return null;
            return _schema.FindPublicHoliday(_group, dt, _overrides);
        }

        /// <summary>
        /// Updates the given metadata with the data on this calendar.
        /// This calendar uses an identifier for itself, so it just updates
        /// the calendar ID  property on the metadata.
        /// </summary>
        /// <param name="meta">The metadata to update.</param>
        public void UpdateMetaData(TriggerMetaData meta)
        {
            meta.CalendarId = Id;
        }

        /// <summary>
        /// Gets a string representation of this calendar.
        /// This is here mainly for debug purposes.
        /// 
        /// ToString() is used for display name when displaying this object in a combo box ui element
        /// </summary>
        /// <returns>A string representation of this object.</returns>
        public override string ToString()
        {
            return LTools.GetC(this.Name, "holiday", "calendar");
        }

        /// <summary>
        /// Gets the configuration of this calendar in string form.
        /// </summary>
        public string Configuration
        {
            get
            {
                StringBuilder sb = new StringBuilder(128);
                sb.Append('{');
                sb.AppendFormat(Resources.ScheduleCalendar_Name0, this.ToString());
                sb.AppendFormat(Resources.ScheduleCalendar_Configuration_WorkingWeek0, _workingWeek.Configuration);
                sb.AppendFormat(Resources.ScheduleCalendar_Configuration_PublicHolidays0, LTools.GetC(_group, "holiday") ?? Resources.ScheduleCalendar_None);
                if (!string.IsNullOrEmpty(_group))
                {
                    sb.AppendFormat(Resources.ScheduleCalendar_Configuration_Overrides0,
                        CollectionUtil.Join(_overrides, ","));
                }
                sb.Append(Resources.ScheduleCalendar_Configuration_OtherHolidays);
                sb.Append('[');
                bool first = true;
                foreach (DateTime dt in _nonWorkingDays)
                {
                    if (first)
                        first = false;
                    else
                        sb.Append(',');
                    sb.Append(dt.ToShortDateString());
                }
                sb.Append(']');
                return sb.Append('}').ToString();
            }
        }

        /// <summary>
        /// Loads the schedule calendars from the given data set.
        /// </summary>
        /// <param name="ds">The data set containing 4 tables : PublicHoliday,
        /// Calendar, PublicHolidayOverride and NonWorkingDay and 2 relations:
        /// calendar-publicholidayoverrides which makes Calendar's id parent to
        /// PublicHolidayOverride's calendarid, and calendar-nonworkingdays
        /// which makes Calendar's id parent to NonWorkingDay's calendarid.
        /// </param>
        /// <returns>A map of calendar IDs to their calendars.</returns>
        public static IDictionary<int, ScheduleCalendar> LoadCalendars(
            DataSet ds, PublicHolidaySchema schema)
        {
            IDictionary<int, ScheduleCalendar> map =
                new clsOrderedDictionary<int, ScheduleCalendar>();

            foreach (DataRow calRow in ds.Tables["Calendar"].Rows)
            {
                IDataProvider provider = new DataRowDataProvider(calRow);

                ScheduleCalendar cal = new ScheduleCalendar(schema);
                cal.Id = provider.GetValue("id", 0);
                cal.Name = provider.GetValue("calendarname", "");
                cal.WorkingWeek.SetTo(provider.GetValue("workingweek", 0));
                cal.PublicHolidayGroup = provider.GetValue<string>("holidaygroupname", null);

                foreach (DataRow row in calRow.GetChildRows("calendar-publicholidayoverrides"))
                {
                    cal.PublicHolidayOverrides.Add(schema.GetHoliday(
                        new DataRowDataProvider(row).GetValue("publicholidayid", 0)));
                }

                foreach (DataRow row in calRow.GetChildRows("calendar-nonworkingdays"))
                {
                    cal.NonWorkingDays.Add(
                        new DataRowDataProvider(row).GetValue("nonworkingday", DateTime.MinValue));
                }
                map[cal.Id] = cal;
            }
            return map;

        }

        /// <summary>
        /// Checks if this calendar is equal, in value, to the given object.
        /// It is considered equal if the given object is a non-null calendar
        /// with the same <em>value</em> as this calendar - note that the ID
        /// is not checked in an Equals() check - just the value.
        /// </summary>
        /// <param name="obj">The object to test for equality</param>
        /// <returns>true if the given object is a non-null calendar with the
        /// same value as this calendar, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            ScheduleCalendar cal = obj as ScheduleCalendar;
            if (cal == null)
                return false;
            // we don't check ID here - we're testing value, not DB identity
            return (
                Object.Equals(this.Name, cal.Name) &&
                Object.Equals(this.Description, cal.Description) &&
                Object.Equals(_group, cal._group) &&
                _workingWeek == cal._workingWeek &&
                CollectionUtil.AreEquivalent(_nonWorkingDays, cal._nonWorkingDays) &&
                CollectionUtil.AreEquivalent(_overrides, cal._overrides)
            );
        }

        /// <summary>
        /// Gets a hash for this calendar.
        /// </summary>
        /// <returns>A unique hash for this calendar.</returns>
        public override int GetHashCode()
        {
            return HashAll(Name, Description, _group, _workingWeek, _nonWorkingDays, _overrides);
        }

        /// <summary>
        /// Gets an XOR'ed value of all the hashcodes on the given objects
        /// </summary>
        /// <param name="objects"></param>
        /// <returns></returns>
        private int HashAll(params object[] objects)
        {
            int accum = 0;
            foreach (object o in objects)
            {
                if (o == null) continue;
                accum ^= o.GetHashCode();
            }
            return accum;
        }

        public bool HasAnyWorkingDays()
        {
            return _workingWeek.Count != 0;
        }
    }
}
