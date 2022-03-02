using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace BluePrism.Scheduling.Calendar
{
    /// <summary>
    /// Class to describe a public holiday (or, more accurately within UK, a bank
    /// holiday). This just holds the algorithmic parameters for a holiday rather
    /// than any actual dates. Dates are calculated within a range, and most access
    /// to public holidays is expected to be via the static PublicHoliday.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "bp", IsReference = true)]
    public class PublicHoliday : DescribedNamedObject, IComparable<PublicHoliday>
    {
        #region Class-scope declarations

        /// <summary>
        /// Cache for easter sunday - it's quite a lot of calculating... we might as
        /// well try and skip it if we've already calculated it.
        /// </summary>
        private static readonly IDictionary<int, DateTime>
            EASTER_SUNDAY_CACHE = new Dictionary<int, DateTime>();

        /// <summary>
        /// Lock for accessing the easter sunday cache.
        /// </summary>
        private static object EasterSundayLock = new Object();

        /// <summary>
        /// A set containing the weekdays to check against.
        /// </summary>
        private static readonly DaySet WEEKDAYS = new DaySet(
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday
        );

        #endregion

        #region Member Variables

        /// <summary>
        /// The ID of this public holiday
        /// </summary>
        [DataMember]
        private readonly int _id;

        /// <summary>
        /// Whether this holiday represents easter sunday or not
        /// </summary>
        [DataMember]
        private bool _easterSunday; // this is an override of everything else really

        /// <summary>
        /// The specific day of the month that this holiday occurs on or zero,
        /// if it is not a specific day
        /// </summary>
        [DataMember]
        private int _dd;

        /// <summary>
        /// The specific (1-based) month that this holiday occurs in, or zero,
        /// if it is not tied to a specific month
        /// </summary>
        [DataMember]
        private int _mm;

        /// <summary>
        /// The specific day of the week that this holiday is set to or null
        /// if it is not set to a specific day of the week
        /// </summary>
        [DataMember]
        private DayOfWeek? _day;

        /// <summary>
        /// Which occurrence within a month that this holiday is set to, or
        /// NthOfMonth.None if it is not set to a specific occurrence within the month.
        /// </summary>
        [DataMember]
        private NthOfMonth _nth;

        /// <summary>
        /// <para>The holiday that this public holiday is relative to, or null if it is
        /// not dependent on another public holiday.</para>
        /// <para>Note: this may be null for a dependent holiday while the public holidays are
        /// being constructed and registered (ie. within the scope of PublicHoliday.Initialise().
        /// Dependencies are resolved after all public holidays have been loaded.</para>
        /// </summary>
        [DataMember]
        private PublicHoliday _relative;

        /// <summary>
        /// The number of days to add to the relative public holiday in order to 
        /// calculate the date of this public holiday. Zero for non-relative holidays
        /// </summary>
        [DataMember]
        private int _relativeBy;

        /// <summary>
        /// Gets or sets a value indicating whether or not saturday is lost as a holiday (Hong Kong currently does this)
        /// </summary>
        [DataMember]
        private bool _excludeSaturday;

        /// <summary>
        /// Gets or sets a value indicating whether or not saturday is lost as a holiday (Hong Kong currently does this)
        /// </summary>
        [DataMember]
        private ShiftDayType _shiftDayType;

        /// <summary>
        /// The day of the week before or after the relative holiday, used to calculate the date of this holiday.
        /// </summary>
        [DataMember]
        private int _relativeDayOfWeek;

        private string _localName;

        #endregion

        #region Properties

        /// <summary>
        /// The ID of this holiday - the only publically readable property.
        /// Needed to provide database links to other tables (eg. calendar)
        /// </summary>
        public int Id
        {
            get { return _id; }
        }

        /// <summary>
        /// True to indicate this holiday is easter sunday; false otherwise
        /// </summary>
        internal bool EasterSunday
        {
            get { return _easterSunday; }
            set { _easterSunday = value; }
        }

        /// <summary>
        /// The day of this holiday
        /// </summary>
        internal int DD
        {
            get { return _dd; }
            set { _dd = value; }
        }

        /// <summary>
        /// The month of this holiday
        /// </summary>
        internal int MM
        {
            get { return _mm; }
            set { _mm = value; }
        }

        /// <summary>
        /// The day of week of this holiday, or null if not appropriate
        /// </summary>
        internal DayOfWeek? Day
        {
            get { return _day; }
            set { _day = value; }
        }

        /// <summary>
        /// The Nth of the month that this holiday represents
        /// </summary>
        internal NthOfMonth Nth
        {
            get { return _nth; }
            set { _nth = value; }
        }

        /// <summary>
        /// The holiday that this holiday is relative to, or null if it is not relative
        /// to another holiday
        /// </summary>
        internal PublicHoliday Relative
        {
            get { return _relative; }
            set { _relative = value; }
        }

        /// <summary>
        /// The number of days by which this holiday is relative to its related holiday
        /// </summary>
        internal int RelativeBy
        {
            get { return _relativeBy; }
            set { _relativeBy = value; }
        }


        /// <summary>
        /// The localised name of this object.
        /// </summary>
        public string LocalName
        {
            get { return _localName; }
            set { _localName = value; }
        }

        /// <summary>
        /// The localised name of this object.
        /// </summary>
        public bool ExcludeSaturday
        {
            get { return this._excludeSaturday; }
            set { this._excludeSaturday = value; }
        }

        /// <summary>
        /// The localised name of this object.
        /// </summary>
        public ShiftDayType ShiftDayType
        {
            get { return this._shiftDayType; }
            set { this._shiftDayType = value; }
        }

        /// <summary>
        /// The weekday by which this holiday is relative to its related holiday
        /// </summary>
        internal int RelativeDayOfWeek
        {
            get { return _relativeDayOfWeek; }
            set { _relativeDayOfWeek = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new public holiday with the given values.
        /// </summary>
        /// <param name="id">The unique ID for this public holiday</param>
        /// <param name="name">The name of this public holiday</param>
        /// <param name="dd">The specific day of the month to tie this holiday to - 0
        /// if it should not be tied to a particular day.</param>
        /// <param name="mm">The specific month to tie this holiday to - 0 if it should
        /// not be tied to a particular month</param>
        /// <param name="day">The particular day of the week that this holiday is tied
        /// to; null indicates that it is not tied to a particular day of the week.</param>
        /// <param name="nth">The occurrence within the month to bind this holiday to. 
        /// NthOfMonth.None indicates that it is not bound to a particular occurrence.</param>
        /// <param name="easterSunday">True to indicate that this public holiday is easter
        /// sunday, false otherwise.</param>
        public PublicHoliday(int id, string name, int dd, int mm, DayOfWeek? day, NthOfMonth nth, bool easterSunday)
            : this(id, name, dd, mm, day, nth, easterSunday, null, 0, false, ShiftDayType.ShiftForward, 0) { }

        public PublicHoliday(int id, string name, int dd, int mm, DayOfWeek? day, NthOfMonth nth, bool easterSunday, bool excludeSaturday, ShiftDayType shiftDayType)
            : this(id, name, dd, mm, day, nth, easterSunday, null, 0, excludeSaturday, shiftDayType, 0) { }

        public PublicHoliday(int id, string name, int dd, int mm, DayOfWeek? day, NthOfMonth nth, bool easterSunday, bool excludeSaturday)
            : this(id, name, dd, mm, day, nth, easterSunday, null, 0, excludeSaturday, ShiftDayType.ShiftForward, 0) { }

        public PublicHoliday(
            int id,
            string name,
            int dd,
            int mm,
            DayOfWeek? day,
            NthOfMonth nth,
            bool easterSunday,
            PublicHoliday relativeTo,
            int relativeBy)
            : this(id, name, dd, mm, day, nth, easterSunday, relativeTo, relativeBy, false, ShiftDayType.ShiftForward, 0) { }

        /// <summary>
        /// Creates a new public holiday with the given values.
        /// </summary>
        /// <param name="id">The unique ID for this public holiday</param>
        /// <param name="name">The name of this public holiday</param>
        /// <param name="dd">The specific day of the month to tie this holiday to - 0
        /// if it should not be tied to a particular day.</param>
        /// <param name="mm">The specific month to tie this holiday to - 0 if it should
        /// not be tied to a particular month</param>
        /// <param name="day">The particular day of the week that this holiday is tied
        /// to; null indicates that it is not tied to a particular day of the week.</param>
        /// <param name="nth">The occurrence within the month to bind this holiday to. 
        /// NthOfMonth.None indicates that it is not bound to a particular occurrence.</param>
        /// <param name="easterSunday">True to indicate that this public holiday is easter
        /// sunday, false otherwise.</param>
        /// <param name="relativeTo">The public holiday that this holiday is relative to.
        /// </param>
        /// <param name="relativeBy">The number of days relative to <paramref name="relativeTo"/>
        /// that this holiday is. This can be negative to indicate days before the relative date.
        /// </param>
        public PublicHoliday(
            int id, 
            string name,
            int dd, 
            int mm, 
            DayOfWeek? day, 
            NthOfMonth nth, 
            bool easterSunday,
            PublicHoliday relativeTo, 
            int relativeBy,
            bool excludeSaturday,
            ShiftDayType shiftDayType,
            int relativeDayOfWeek)
        {
            this._id = id;
            this.Name = name;
            this._dd = dd;
            this._mm = mm;
            this._day = day;
            this._nth = nth;
            this._easterSunday = easterSunday;
            this._relative = relativeTo;
            this._relativeBy = relativeBy;
            this._excludeSaturday = excludeSaturday;
            this._shiftDayType = shiftDayType;
            this._relativeDayOfWeek = relativeDayOfWeek;
        }

        #endregion

        #region object Overrides

        /// <summary>
        /// Checks if this public holiday matches the given object.
        /// Since public holidays are unique and immutable, this checks only those
        /// values utilised to generate a hashcode
        /// </summary>
        /// <param name="obj">The object to compare this object to</param>
        /// <returns>true if the given object was a public holiday with the same
        /// ID and name as this object.</returns>
        public override bool Equals(object obj)
        {
            PublicHoliday hol = obj as PublicHoliday;
            return (hol != null && hol._id == this._id && hol.Name.Equals(this.Name));
        }

        /// <summary>
        /// Gets a hash of the value of this public holiday.
        /// This is just a function of the ID and the name.
        /// </summary>
        /// <returns>An integer hash value for this object.</returns>
        public override int GetHashCode()
        {
            return _id;
        }

        /// <summary>
        /// Gets a string representation of this public holiday, just the name.
        /// </summary>
        /// <returns>A string representation of this public holiday</returns>
        public override string ToString()
        {
            return LocalName ?? Name;
        }

        #endregion

        #region IComparable<PublicHoliday> Members

        /// <summary>
        /// Compares this public holiday to the given public holiday.
        /// This uses the occurrence within the (arbitrary) year 2000 to determine
        /// its comparable relationship to the given holiday. If they happen to
        /// fall on the same date, then the name is used as a 'secondary sort' order
        /// decider.
        /// </summary>
        /// <param name="o">The public holiday to compare against</param>
        /// <returns>A negative value, zero or a positive value if this public
        /// holiday is 'less than', 'equal to' or 'greater than' the given
        /// public holiday, respectively.</returns>
        public int CompareTo(PublicHoliday o)
        {
            int days = (int)(GetOccurrence(2000) - o.GetOccurrence(2000)).TotalDays;
            return (days == 0 ? this.Name.CompareTo(o.Name) : days);
        }

        #endregion

        #region PublicHoliday-specific methods

        /// <summary>
        /// Calculates easter sunday for a given year. This uses the 'Nature,1876'
        /// algorithm described at:-
        /// https://portal.blueprism.com/wiki/index.php?title=Public_Holiday_Calculations
        /// to work out the Easter Sunday date for a given year.
        /// </summary>
        /// <param name="year">The year for which easter sunday is required.</param>
        /// <returns>The date on which Easter Sunday falls on the given year.
        /// </returns>
        private DateTime GetEasterSunday(int year)
        {
            DateTime easterSunday;
            lock (EasterSundayLock)
            {
                if (!EASTER_SUNDAY_CACHE.TryGetValue(year, out easterSunday))
                {
                    // This algorithm is detailed in the wiki - see the wiki page
                    // on 'Public Holiday Calculations' - url in method description
                    int a = year % 19;
                    int b = year / 100;
                    int c = year % 100;
                    int d = b / 4;
                    int e = b % 4;
                    int f = (b + 8) / 25;
                    int g = (b - f + 1) / 3;
                    int h = (19 * a + b - d - g + 15) % 30;
                    int i = c / 4;
                    int k = c % 4;
                    int l = (32 + 2 * e + 2 * i - h - k) % 7;
                    int m = (a + 11 * h + 22 * l) / 451;
                    int n = (h + l - 7 * m + 114);

                    int month = n / 31;     // [3=March, 4=April]
                    int day = 1 + (n % 31); // (date in Easter Month)

                    easterSunday = new DateTime(year, month, day);
                    EASTER_SUNDAY_CACHE[year] = easterSunday;
                }
            }
            return easterSunday;
        }

        /// <summary>
        /// Just a debug version of ToString() which returns all the internal
        /// values of the public holiday.
        /// </summary>
        /// <returns>The debug info for this public holiday.</returns>
        internal string ToDebugString()
        {
            return new StringBuilder("PublicHoliday:[").
                Append("id:").Append(_id).
                Append("; name:").Append(Name).
                Append("; dd:").Append(_dd).
                Append("; mm:").Append(_mm).
                Append("; day:").Append(_day).
                Append("; nth:").Append(_nth).
                Append("; relative<id>:").
                    Append(_relative == null ? "<null>" : _relative._id.ToString()).
                Append("; relativeBy:").Append(_relativeBy).
                Append("; easterSunday:").Append(_easterSunday).
                Append(']').ToString();
        }

        /// <summary>
        /// Gets the occurrence of this public holiday in the given year.
        /// </summary>
        /// <param name="year">The year in which the date of this public holiday is
        /// required</param>
        /// <returns>A datetime (set to midday in default timezone) containing the
        /// date on which this public holiday occurs for the specified year.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If the [nth] [weekday] of [month] is described by this public holiday,
        /// and there are less than [n] instances of that weekday in the given month.
        /// </exception>
        /// <exception cref="InvalidDataException">
        /// If the data within this public holiday is invalid.
        /// The data must match any of the following rules, and is checked in this
        /// order :- <list>
        /// <entry>'easterSunday' is true</entry>
        /// <entry>'relativeTo' is set to a valid public holiday</entry>
        /// <entry>'dd' and 'mm' represent a valid day-of-month and (1-indexed) month
        /// </entry>
        /// <entry>'nth' is not 'None', 'dayofweek' is not null and 'mm' is between 1
        /// and 12</entry>
        /// </list>
        /// If none of the above constraints are matched by the internal data then an
        /// InvalidDataException is thrown.
        /// </exception>
        public DateTime GetOccurrence(int year)
        {
            // first off, let's see if we're easter sunday - that nixes all else
            if (_easterSunday)
            {
                return GetEasterSunday(year);
            }

            // Next two (relative holidays and specific holidays) need to skip on 
            // until the next weekday ... set a root date as appropriate, then just
            // keep adding days until we hit a weekday.
            DateTime date = DateTime.MinValue;

            // See if it's relative to another public holiday (eg. Boxing Day
            // is one day after the Christmas Day occurrence).
            if (_relative != null)
            {
                if (_relativeBy != 0)
                {
                    date = _relative.GetOccurrence(year).AddDays(_relativeBy);
                }
                else if (_relativeDayOfWeek != 0)
                {
                    date = GetDateUsingRelativeDayOfWeek(_relative.GetOccurrence(year));                    
                }
            }
            // first let's see if we're a specific dd/mm combo
            else if (_dd > 0 && _mm > 0)
            {
                date = new DateTime(year, _mm, _dd);
            }

            // if relative or specific...
            if (date != DateTime.MinValue)
            {
                if (date.DayOfWeek == DayOfWeek.Saturday && this.ExcludeSaturday == true)
                {
                    return date;
                }

                // we need to step on from this to the next available weekday
                switch (this.ShiftDayType)
                {
                    case ShiftDayType.ShiftBackwardOrForward:
                        // For the USA, if it is saturday, the holiday is moved backwards
                        if (date.DayOfWeek == DayOfWeek.Saturday)
                        {
                            date = date.AddDays(-1);
                        }
                        else if(date.DayOfWeek == DayOfWeek.Sunday)
                        {
                            date = date.AddDays(1);
                        }
                        break;

                    case ShiftDayType.NoShift:
                        //Holiday is static and should not be changed to account for weekends
                        break;

                    case ShiftDayType.ShiftForward:
                    default:
                        while (!WEEKDAYS[date.DayOfWeek])
                        {
                            date = date.AddDays(1);
                        }
                        break;
                    
                }

                return date;
            }

            // okay, all that's left is the <nth> <weekday> of <month>, which is the
            // most convoluted one of the lot.
            if (_day.HasValue && _nth != NthOfMonth.None && _mm > 0)
            {
                // start from the first of the month, go up each day until we hit the
                // correct DayOfWeek. Then increment by 7 days until we hit the
                // correct NthOfMonth... 
                date = new DateTime(year, _mm, 1);
                while (date.DayOfWeek != this._day)
                {
                    date = date.AddDays(1);
                }

                // now we need to store 2 dates... the current one for checking and
                // the previous one, just in case we want the last of the month.
                int count = 0;
                int weekCount = (int)_nth;
                DateTime old = DateTime.MinValue;
                // '!=' here rather than '<', because weekCount might equal -1 to
                // indicate 'Last'
                while (++count != weekCount && date.Month == _mm)
                {
                    old = date;
                    date = date.AddDays(7);
                }
                // If we spill over into the next month and nth==NthOfMonth.Last,
                // then we use the previous value... if nth!=NthOfMonth.Last then
                // we've overrun, so error
                if (date.Month != _mm)
                {
                    if (_nth == NthOfMonth.Last)
                    {
                        // if we have actually set a date.
                        if (old != DateTime.MinValue)
                        {
                            return old;
                        }
                        else
                        {   // really really should never happen...
                            throw new InvalidDataException(
                                string.Format(BluePrism.Scheduling.Properties.Resources.FATALNoOldValueSetForLast0OfMonth1, _day, _mm));
                        }
                    }
                    else // there is no <nth> <weekday> in this month
                    {
                        throw new ArgumentOutOfRangeException(
                            string.Format(BluePrism.Scheduling.Properties.Resources.ThereAreLessThan01SIn2MMMM, ((int)_nth), _day, old));
                    }
                }
                // otherwise... we've hit our 'n'
                return date;
            }
            // If we're here, there's a serious data exception...
            // the state of this public holiday is invalid.
            throw new InvalidDataException(
                string.Format("Bad PublicHoliday data - cannot get date : {0}", ToDebugString()));
        }

        private DateTime GetDateUsingRelativeDayOfWeek(DateTime relativeDate)
        {

            if (_relativeDayOfWeek > 0)
            {
                if (relativeDate.DayOfWeek == (DayOfWeek)_relativeDayOfWeek)
                    return relativeDate.AddDays(7);

                while (relativeDate.DayOfWeek != (DayOfWeek)_relativeDayOfWeek)
                {
                    relativeDate = relativeDate.AddDays(1);
                }
            }
            else
            {
                if (relativeDate.DayOfWeek == (DayOfWeek)(_relativeDayOfWeek * -1))
                    return relativeDate.AddDays(-7);

                while (relativeDate.DayOfWeek != (DayOfWeek)(_relativeDayOfWeek * -1))
                {
                    relativeDate = relativeDate.AddDays(-1);
                }
            }

            return relativeDate;
        }

        public DateTime GetNextOccurrence()
        {
            var nextOccurrence = this.GetOccurrence(DateTime.UtcNow.Year);
            if (nextOccurrence.Date < DateTime.UtcNow.Date)
            {
                nextOccurrence = this.GetOccurrence(DateTime.UtcNow.Year + 1);
            }

            return nextOccurrence;
        }

        /// <summary>
        /// Gets all occurrences of this public holiday within the given dates
        /// (inclusive).
        /// </summary>
        /// <param name="start">The first date for which the public holidays 
        /// are required.</param>
        /// <param name="end">The last date for which public holidays are required.
        /// </param>
        /// <returns>A collection of DateTimes which represent the public holidays
        /// within the given range.</returns>
        public ICollection<DateTime> GetOccurrences(DateTime start, DateTime end)
        {
            // if the range is in reverse... meh, let's allow it.
            if (start > end)
            {
                DateTime temp = end;
                end = start;
                start = temp;
            }
            // set start to 0:00 and end to 23:59 to ensure we get them inclusive
            start = start.Subtract(start.TimeOfDay);
            end = new DateTime(end.Year, end.Month, end.Day, 23, 59, 59, 999);

            // each public holiday occurs once throughout a year, all we need to do
            // is get the occurrence for each year that the given range covers, and
            // ensure we omit the ones which fall before the start or after the end
            IDictionary<int, DateTime> dict = new SortedDictionary<int, DateTime>();
            DateTime curr = start;
            while (curr.Year <= end.Year)
            {
                int year = curr.Year;
                DateTime dt = GetOccurrence(curr.Year);
                if (dt >= start && dt <= end)
                    dict[year] = dt;
                // increment the year and move on
                curr = curr.AddYears(1);
            }
            return dict.Values;
        }

        /// <summary>
        /// Checks if the given date falls on this public holiday
        /// </summary>
        /// <param name="day">The date to check to see if it matches an occurrence
        /// of this public holiday</param>
        /// <returns>true if this public holiday falls on the given date;
        /// false otherwise.</returns>
        public bool IsHoliday(DateTime day)
        {
            // We have to check the previous/next year because a rule might exist that could move a holiday to the next/previous year 
            // eg. New years day moves back to december the 31st on 2022 :S (USA)
            return day.Date == GetOccurrence(day.Year).Date ||
                day.Date == GetOccurrence(day.Year-1).Date ||
                day.Date == GetOccurrence(day.Year+1).Date;
        }

        #endregion

    }
}
