using BluePrism.Scheduling.Properties;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Runtime.Serialization;
using BluePrism.BPCoreLib.Data;
using BluePrism.BPCoreLib.Collections;

namespace BluePrism.Scheduling.Calendar
{
    /// <summary>
    /// Class to draw together all the public holidays and groups into a
    /// single schema.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "bp", IsReference = true)]
    public class PublicHolidaySchema
    {
        /// <summary>
        /// Dictionary mapping IDs against the represented public holiday.
        /// This class assumes that each ID is unique (and immutable).
        /// </summary>
        [DataMember]
        private readonly IDictionary<int, PublicHoliday>
            _holidaysById = new Dictionary<int, PublicHoliday>();

        /// <summary>
        /// Dictionary mapping the group name against the collection of public holidays
        /// which make up the group.
        /// </summary>
        [DataMember]
        private readonly IDictionary<string, ICollection<PublicHoliday>>
            _holidaysByGroup = new Dictionary<string, ICollection<PublicHoliday>>();

        /// <summary>
        /// Creates a new public holiday schema from the given data table.
        /// The table is expected to have the following columns and types :-
        /// <list>
        /// <item>groupname : string</item>
        /// <item>id : int</item>
        /// <item>holidayname : string</item>
        /// <item>dd : int</item>
        /// <item>mm : int</item>
        /// <item>dayofweek : int (System.DayOfWeek)</item>
        /// <item>nthofmonth : int (NthOfMonth)</item>
        /// <item>eastersunday : bool</item>
        /// <item>relativetoholiday : int</item>
        /// <item>relativedaydiff : int</item>
        /// </list>
        /// </summary>
        /// <param name="holidays"></param>
        public PublicHolidaySchema(DataTable holidays)
        {
            Initialise(holidays);
        }

        /// <summary>
        /// Creates a new PublicHolidaySchema from the given holidays and
        /// group assigments.
        /// </summary>
        /// <param name="holidays">The collection of holidays to use in this
        /// schema.</param>
        /// <param name="groups">The groups containing the collection of public
        /// holiday IDs keyed against the group that they are contained in.
        /// </param>
        /// <remarks>This is only here so that specific schemas can be created
        /// for testing purposes. Typically, the DataTable constructor should
        /// be called.</remarks>
        public PublicHolidaySchema(
            ICollection<PublicHoliday> holidays, IDictionary<String, ICollection<int>> groups)
        {
            foreach (PublicHoliday hol in holidays)
            {
                //Console.WriteLine("Adding holiday : " + hol + "[" + hol.Id + "]");
                _holidaysById[hol.Id] = hol;
            }
            foreach (String group in groups.Keys)
            {
                List<PublicHoliday> list = new List<PublicHoliday>();
                _holidaysByGroup[group] = list;

                foreach (int holId in groups[group])
                {
                    //Console.WriteLine("Getting holiday : " + holId);
                    _holidaysByGroup[group].Add(_holidaysById[holId]);
                }
            }
        }

        /// <summary>
        /// Initialises all public holidays using the given data table.
        /// The table is expected to have the following columns and types :-
        /// <list>
        /// <item>groupname : string</item>
        /// <item>id : int</item>
        /// <item>holidayname : string</item>
        /// <item>dd : int</item>
        /// <item>mm : int</item>
        /// <item>dayofweek : int (System.DayOfWeek)</item>
        /// <item>nthofmonth : int (NthOfMonth)</item>
        /// <item>eastersunday : bool</item>
        /// <item>relativetoholiday : int</item>
        /// <item>relativedaydiff : int</item>
        /// </list>
        /// This will delete all current public holiday data and 
        /// </summary>
        /// <param name="allHolidays">The table containing the configuration data
        /// for all public holidays to be modelled within the system.</param>
        private void Initialise(DataTable allHolidays)
        {
            lock (_holidaysById) // don't want to allow public access while we're updating
            {
                // Reset the maps
                _holidaysById.Clear();
                _holidaysByGroup.Clear();

                try
                {
                    // Map of <ID, RelativeID> allowing all holidays to be loaded before
                    // the relative holidays are set.
                    IDictionary<int, int> holidaysToResolve = new Dictionary<int, int>();
                    foreach (DataRow row in allHolidays.Rows)
                    {
                        IDataProvider provider = new DataRowDataProvider(row);
                        PublicHoliday hol = CreateFrom(provider);
                        // we have to deal with the relative holidays here, since 
                        // we have the advantage of a bit of context.
                        int relativeId = provider.GetValue("relativetoholiday", 0);
                        int relativeBy = provider.GetValue("relativedaydiff", 0);
                        int relativeDayOfWeek = provider.GetValue("relativedayofweek", 0);
                        if (relativeId > 0 || relativeDayOfWeek != 0)
                        {
                            hol.RelativeBy = relativeBy;
                            hol.RelativeDayOfWeek = relativeDayOfWeek;
                            // add to holidays to resolve (if it's not already there)
                            if (!holidaysToResolve.ContainsKey(hol.Id))
                                holidaysToResolve.Add(hol.Id, relativeId);
                        }
                    }
                    // Okay, we have all holidays loaded, now resolve the dependencies.
                    // First, make sure there are no circular dependencies.
                    ICollection<int> validIds = new List<int>();
                    Stack<int> stack = new Stack<int>();
                    foreach (int id in holidaysToResolve.Keys)
                    {
                        CheckForCircularDependency(id, holidaysToResolve, stack, validIds);
                        _holidaysById[id].Relative = _holidaysById[holidaysToResolve[id]];
                    }
                    // At this point everything's been resolved; all relatives are where they
                    // are supposed to be, we can sort the public holidays into their
                    // natural order.... this is actually quite an expensive process (since
                    // it involves working out the occurrence of the public holiday for all
                    // entries) so it's preferable to just do it once.
                    IDictionary<string, ICollection<PublicHoliday>> dict =
                        new Dictionary<string, ICollection<PublicHoliday>>();

                    // first sort them into a temp dictionary
                    foreach (string name in _holidaysByGroup.Keys)
                    {
                        dict[name] = new clsSortedSet<PublicHoliday>(_holidaysByGroup[name]);
                    }

                    // then send them back
                    foreach (string name in dict.Keys)
                    {
                        _holidaysByGroup[name] = GetReadOnly.ICollection(dict[name]);
                    }

                }
                catch
                {
                    // if we get an exception, ensure that the holiday collections
                    // are in a defined state (ie. empty)
                    _holidaysById.Clear();
                    _holidaysByGroup.Clear();
                    // rethrow the exception
                    throw;
                }
            }
        }

        /// <summary>
        /// Creates and registers a public holiday from the given data row.
        /// This is expected to have the following columns and types :-
        /// <list>
        /// <item>groupname : string</item>
        /// <item>id : int</item>
        /// <item>holidayname : string</item>
        /// <item>dd : int</item>
        /// <item>mm : int</item>
        /// <item>dayofweek : int</item>
        /// <item>nthofmonth : int</item>
        /// <item>iseastersunday : bool</item>
        /// <item>relativetoholiday : int</item>
        /// <item>relativedaydiff : int</item>
        /// </list>
        /// </summary>
        /// <param name="row">The data row from which to draw the data</param>
        /// <returns>The created public holiday.</returns>
        private PublicHoliday CreateFrom(IDataProvider provider)
        {
            // The holiday we'll be creating
            PublicHoliday hol;

            // the group we'll be putting the holiday in
            ICollection<PublicHoliday> group;

            string groupName = provider.GetValue<string>("groupname", null);
            if (groupName == null) // may not be a member of a group - eg. easter sunday
            {
                group = null;
            }
            else // Get or generate the group depending on whether it's been gen'd before or not
            {
                if (_holidaysByGroup.ContainsKey(groupName))
                {
                    group = _holidaysByGroup[groupName];
                }
                else
                {
                    _holidaysByGroup[groupName] = group = new List<PublicHoliday>();
                }
            }

            // initial bits to identify if we have anything already created
            // Since PublicHoliday is (semantically) immutable, we can safely re-use instances
            // rather than creating them anew each time.
            int id = provider.GetValue("id", 0);
            if (_holidaysById.ContainsKey(id))
            {
                hol = _holidaysById[id];
            }
            else // build it up
            {
                // get the day of week as an int - 'Last' is stored as -1 on the database,
                // we need to convert it to the enum equivalent
                NthOfMonth nth = (NthOfMonth)provider.GetValue("nthofmonth", 0);
                int iDayOfWeek = provider.GetValue("dayofweek", -1);
                DayOfWeek? dayOfWeek;
                if (iDayOfWeek == -1)
                {
                    dayOfWeek = null;
                }
                else
                {
                    dayOfWeek = (DayOfWeek)iDayOfWeek;
                }
                hol = new PublicHoliday(
                    id,
                    provider.GetValue("holidayname", ""),
                    provider.GetValue("dd", 0),
                    provider.GetValue("mm", 0),
                    dayOfWeek,
                    nth,
                    provider.GetValue("eastersunday", false),
                    provider.GetValue("excludesaturday", false),
                    (ShiftDayType)provider.GetValue("shiftdaytypeid", 1));

                // Now add it to the keyed-by-id map
                _holidaysById.Add(id, hol);
            }
            // Add it to the relevant group (if there is one)
            if (group != null)
                group.Add(hol);

            // Look what I've made
            return hol;
        }

        /// <summary>
        /// Recursively checks all dependencies of the given ID within the given map
        /// to ensure that there are no circular dependencies.
        /// </summary>
        /// <param name="id">The ID to check in this iteration</param>
        /// <param name="map">The map of dependencies: key is dependent, value is that which is
        /// depended upon.</param>
        /// <param name="trace">A stack containing the dependency chain processed so far. When
        /// calling from outside this method, an empty stack should be given.</param>
        /// <param name="valid">A shortcut collection used to hold IDs which are known to be
        /// valid, ie. known not to have circular dependencies.</param>
        private void CheckForCircularDependency(int id,
            IDictionary<int, int> map, Stack<int> trace, ICollection<int> valid)
        {
            // if it's already been successfully tested, or it's not dependent 
            // on another holiday, return without error.
            if (valid.Contains(id) || !map.ContainsKey(id))
            {
                return;
            }

            // If we've already traversed this one while checking the root ID for
            // circular dependency... well that there's an error
            if (trace.Contains(id))
            {
                StringBuilder sb = new StringBuilder(
                    Resources.PublicHolidayCircularDependencyStackFollows);
                sb.Append(_holidaysById[id]);
                while (trace.Count > 0)
                {
                    sb.Append(" <= ").Append(_holidaysById[trace.Pop()]);
                }
                throw new CircularReferenceException(sb.ToString());
            }

            // put this id into the trace and recurse further
            trace.Push(id);
            CheckForCircularDependency(map[id], map, trace, valid);
            trace.Pop();
            // If we made it here, id is valid... might as well shortcut it
            valid.Add(id);
        }


        /// <summary>
        /// Gets the group represented by the given name, or throws an appropriate
        /// exception if that name is not registered with this class.
        /// </summary>
        /// <param name="group">The group which is required.</param>
        /// <returns>A collection of public holidays which represents the public
        /// holiday group keyed on the specified name.</returns>
        /// <exception cref="InvalidGroupNameException">If the given name doesn't
        /// represent a public holiday group held by this class.</exception>
        private ICollection<PublicHoliday> GetGroup(string group)
        {
            if (!_holidaysByGroup.ContainsKey(group))
                throw new InvalidGroupNameException(
                    "Public Holiday Group: '" + group + "' not found");
            return _holidaysByGroup[group];
        }

        /// <summary>
        /// Gets all the public holidays in a particular group.
        /// </summary>
        /// <param name="group">The name of the group for which the public holidays
        /// are required. Note that this is strict - ie. case sensitive, whitespace
        /// and non-alphanumeric characters must match precisely.</param>
        /// <returns>A collection of public holidays representing the specified
        /// group.</returns>
        /// <exception cref="InvalidGroupNameException">If the given groupName
        /// does not represent a public holiday group held by this class.</exception>
        public ICollection<PublicHoliday> GetHolidays(string group)
        {
            // Make sure the callee can't modify our collection
            lock (_holidaysById)
            {
                return GetGroup(group);
            }
        }

        public ICollection<PublicHoliday> GetHolidays()
        {
            lock (_holidaysById)
            {
                return this._holidaysById.Values;
            }
        }

        /// <summary>
        /// Gets the public holiday with the given ID.
        /// </summary>
        /// <param name="id">The ID for which a public holiday is required.</param>
        /// <returns>The public holiday with the given name</returns>
        public PublicHoliday GetHoliday(int id)
        {
            lock (_holidaysById)
                return _holidaysById[id];
        }

        /// <summary>
        /// Gets all the public holiday groups registered with this class.
        /// </summary>
        /// <returns>A readonly collection of the groups known about by
        /// this class.</returns>
        public ICollection<string> GetGroups()
        {
            lock (_holidaysById)
                return _holidaysByGroup.Keys;
        }

        /// <summary>
        /// Gets all the public holiday occurrences which occur within the given
        /// DateTime values for the specified group.
        /// </summary>
        /// <param name="groupName">The name of the group for which the public
        /// holiday dates are required.</param>
        /// <param name="start">The date to use as the first date for which
        /// public holidays are required.</param>
        /// <param name="end">The date to use for the last date for which
        /// public holidays are required.</param>
        /// <returns>A collection of DateTime objects whose date parts represent
        /// the dates of public holidays which occur within the given range
        /// for the specified group. This will never be null, but may return
        /// an empty collection.</returns>
        /// <exception cref="InvalidGroupNameException">If the given groupName
        /// does not represent a public holiday group held by this class.</exception>
        public ICollection<DateTime> GetPublicHolidayDates(
            string groupName, DateTime start, DateTime end)
        {
            lock (_holidaysById)
            {
                IDictionary<DateTime, object> set = new SortedDictionary<DateTime, object>();
                foreach (PublicHoliday hol in GetGroup(groupName))
                {
                    // ballache... there's no AddAll() or AddRange() to union 2 collections...
                    foreach (DateTime dt in hol.GetOccurrences(start, end))
                    {
                        set[dt] = null;
                    }
                }
                return set.Keys;
            }
        }

        /// <summary>
        /// Gets the dates of any public holidays within the specified group within the
        /// given year.
        /// </summary>
        /// <param name="groupName">The name of the group for which the public holiday
        /// dates are required.</param>
        /// <param name="year">The year for which the public holiday dates are
        /// required.</param>
        /// <returns>A collection of DateTime objects which represent the date on
        /// which public holidays within the specified group occur.</returns>
        /// <exception cref="InvalidGroupNameException">If the given groupName
        /// does not represent a public holiday group held by this class.</exception>
        public ICollection<DateTime> GetPublicHolidayDates(string groupName, int year)
        {
            return GetPublicHolidayDates(groupName,
                new DateTime(year, 1, 1, 0, 0, 0, 0),
                new DateTime(year, 12, 31, 23, 59, 59, 999));
        }

        /// <summary>
        /// Gets the dates of any public holidays within the specified group within the
        /// given month in the given year.
        /// </summary>
        /// <param name="groupName">The name of the group for which the public holiday
        /// dates are required.</param>
        /// <param name="year">The year for which the public holiday dates are
        /// required.</param>
        /// <param name="month">The month within the specified year for which all
        /// occurrences of the public holidays are required.</param>
        /// <returns>A collection of DateTime objects which represent the date on
        /// which public holidays within the specified group occur.</returns>
        /// <exception cref="InvalidGroupNameException">If the given groupName
        /// does not represent a public holiday group held by this class.</exception>
        public ICollection<DateTime> GetPublicHolidayDates(
            string groupName, int year, int month)
        {
            return GetPublicHolidayDates(groupName,
                new DateTime(year, month, 1, 0, 0, 0, 0),
                new DateTime(year, month, 31, 23, 59, 59, 999));
        }

        /// <summary>
        /// Checks if any of the public holidays in the specified group fall on
        /// the given date. If any does, then it is returned.
        /// </summary>
        /// <param name="groupName">The group to check the public holidays in
        /// </param>
        /// <param name="date">The date to search the public holidays for.</param>
        /// <returns>The (first) public holiday which falls on the specified
        /// day.</returns>
        /// <exception cref="InvalidGroupNameException">If the given groupName
        /// does not represent a public holiday group held by this class.</exception>
        public PublicHoliday FindPublicHoliday(string groupName, DateTime date)
        {
            return FindPublicHoliday(groupName, date, new PublicHoliday[0]);
        }

        /// <summary>
        /// Checks if any of the public holidays in the specified group fall on
        /// the given date. If any does, then it is returned.
        /// The 'ignore' collection can contain any public holidays which should
        /// be ignored by this search.
        /// </summary>
        /// <param name="groupName">The group to check the public holidays in
        /// </param>
        /// <param name="date">The date to search the public holidays for.</param>
        /// <param name="ignore">The collection of public holidays which should
        /// be omitted in the search - an empty collection if all should be 
        /// considered.</param>
        /// <returns>The (first) public holiday which falls on the specified
        /// day.</returns>
        /// <exception cref="InvalidGroupNameException">If the given groupName
        /// does not represent a public holiday group held by this class.</exception>
        public PublicHoliday FindPublicHoliday(
            string groupName, DateTime date, ICollection<PublicHoliday> ignore)
        {
            foreach (PublicHoliday hol in GetHolidays(groupName))
            {
                if (!ignore.Contains(hol) && hol.IsHoliday(date))
                    return hol;
            }
            return null;
        }

    }
}
