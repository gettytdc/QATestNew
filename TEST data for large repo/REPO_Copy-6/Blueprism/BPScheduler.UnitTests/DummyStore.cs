#if UNITTESTS
using System;
using System.Collections.Generic;
using BluePrism.BPCoreLib.Collections;
using BluePrism.Scheduling.Calendar;
using BluePrism.Scheduling;


namespace BPScheduler.UnitTests
{
    /// <summary>
    /// Utility class for generating a public holiday schema
    /// </summary>
    public class SchemaGenerator
    {
        /// <summary>
        /// The collection of public holidays.
        /// </summary>
        private ICollection<PublicHoliday> hols;

        /// <summary>
        /// The dictionary of public holiday group names to collection of
        /// public holiday IDs.
        /// </summary>
        private IDictionary<string, ICollection<int>> groups;

        /// <summary>
        /// Creates a new blank schema generator.
        /// </summary>
        public SchemaGenerator()
        {
            hols = new List<PublicHoliday>();
            groups = new Dictionary<string, ICollection<int>>();
        }

        /// <summary>
        /// Adds the given public holiday to this generator.
        /// </summary>
        /// <param name="hol">The holiday to add.</param>
        public void Add(PublicHoliday hol)
        {
            if (hol != null)
                hols.Add(hol);
        }

        /// <summary>
        /// Assigns the given public holiday to the specified group.
        /// </summary>
        /// <param name="hol">The holiday to add.</param>
        /// <param name="group">The group to add it to.</param>
        public void Assign(PublicHoliday hol, string group)
        {
            AddToGroup(group, hol.Id);
        }

        /// <summary>
        /// Adds the given ID to the group.
        /// </summary>
        /// <param name="group">The group to add the public holiday ID to.</param>
        /// <param name="holId">The ID to add.</param>
        public void AddToGroup(string group, int holId)
        {
            ICollection<int> hols = null;
            if (!groups.TryGetValue(group, out hols))
            {
                hols = new clsSet<int>();
                groups[group] = hols;
            }
            hols.Add(holId);
        }

        /// <summary>
        /// Gets the generated schema from this object.
        /// </summary>
        public PublicHolidaySchema Schema
        {
            get { return new PublicHolidaySchema(hols, groups); }
        }

    }

    /// <summary>
    /// Simplistic implementation of a store which just holds everything in 
    /// memory.
    /// </summary>
    public class DummyStore : MemoryStore
    {
        public static class Holiday
        {
            public static readonly string EnglandAndWales = "England and Wales";
            public static readonly string Scotland = "Scotland";
            public static readonly string NorthernIreland = "Northern Ireland";
            public static readonly string RepublicOfIreland = "Republic of Ireland";
            public static readonly string HongKong = "Hong Kong";
            public static readonly string USA = "USA";
            public static readonly string France = "France";
            public static readonly string Canada = "Canada";
        }

        /// <summary>
        /// Creates a new blank dummy store with some initial values for the
        /// public holiday schema.
        /// </summary>
        public DummyStore()
        {
            SetSchema(GetDummySchema());
        }

        /// <summary>
        /// Gets the dummy schema 
        /// </summary>
        /// <returns></returns>
        private PublicHolidaySchema GetDummySchema()
        {
            SchemaGenerator gen = new SchemaGenerator();

            // Some hardcoded public holidays, albeit taken from the database on 17/06/2010
            PublicHoliday easterSunday =
                new PublicHoliday(1, "Easter Sunday", 0, 0, null, NthOfMonth.None, true, null, 0);

            PublicHoliday xmasDay =
                new PublicHoliday(2, "Christmas Day", 25, 12, null, NthOfMonth.None, false, null, 0);

            PublicHoliday newYearsDay =
                new PublicHoliday(3, "New Years' Day", 1, 1, null, NthOfMonth.None, false, null, 0);

            gen.Add(easterSunday);
            gen.Add(xmasDay);
            gen.Add(newYearsDay);
            gen.Add(new PublicHoliday(4, "Second of January", 0, 0, null, NthOfMonth.None, false, newYearsDay, 1));
            gen.Add(new PublicHoliday(5, "St Patrick's Day", 17, 3, null, NthOfMonth.None, false, null, 0));
            gen.Add(new PublicHoliday(6, "Good Friday", 0, 0, null, NthOfMonth.None, false, easterSunday, -2));
            gen.Add(new PublicHoliday(7, "Easter Monday", 0, 0, null, NthOfMonth.None, false, easterSunday, 1));
            gen.Add(new PublicHoliday(8, "May Day", 0, 5, DayOfWeek.Monday, NthOfMonth.First, false, null, 0));
            gen.Add(new PublicHoliday(9, "May Bank Holiday", 0, 5, DayOfWeek.Monday, NthOfMonth.First, false, null, 0));
            gen.Add(new PublicHoliday(10, "Spring Bank Holiday", 0, 5, DayOfWeek.Monday, NthOfMonth.Last, false, null, 0));
            gen.Add(new PublicHoliday(11, "June Bank Holiday", 0, 6, DayOfWeek.Monday, NthOfMonth.First, false, null, 0));
            gen.Add(new PublicHoliday(12, "Orangemen's Day", 12, 7, null, NthOfMonth.None, false, null, 0));
            gen.Add(new PublicHoliday(13, "August Bank Holiday", 0, 8, DayOfWeek.Monday, NthOfMonth.First, false, null, 0));
            gen.Add(new PublicHoliday(14, "Summer Bank Holiday", 0, 8, DayOfWeek.Monday, NthOfMonth.First, false, null, 0));
            gen.Add(new PublicHoliday(15, "Summer Bank Holiday", 0, 8, DayOfWeek.Monday, NthOfMonth.Last, false, null, 0));
            gen.Add(new PublicHoliday(16, "October Bank Holiday", 0, 10, DayOfWeek.Monday, NthOfMonth.Last, false, null, 0));
            gen.Add(new PublicHoliday(17, "St Andrew's Day", 30, 11, null, NthOfMonth.None, false, null, 0));
            gen.Add(new PublicHoliday(18, "Boxing Day", 0, 0, null, NthOfMonth.None, false, xmasDay, 1));

            DateTime upcomingSaturday = DateTime.Now.Date.AddDays(1);
            while (upcomingSaturday.DayOfWeek != DayOfWeek.Saturday)
            {
                upcomingSaturday = upcomingSaturday.AddDays(1);
            }

            gen.Add(new PublicHoliday(19, "Lose Saturday", upcomingSaturday.Day, upcomingSaturday.Month, null, NthOfMonth.None, false, null, 0, true, ShiftDayType.ShiftForward, 0));
            gen.Add(new PublicHoliday(20, "Shift Sunday", upcomingSaturday.AddDays(1).Day, upcomingSaturday.AddDays(1).Month, null, NthOfMonth.None, false, null, 0, true, ShiftDayType.ShiftForward, 0));

            gen.Add(new PublicHoliday(21, "New Years Day", 1, 1, null, NthOfMonth.None, false, null, 0, false, ShiftDayType.ShiftBackwardOrForward, 0));

            gen.Add(new PublicHoliday(22, "Christmas Day", 25, 12, null, NthOfMonth.None, false, null, 0, false, ShiftDayType.NoShift, 0));

            var monarchsBirthday = new PublicHoliday(23, "Monarch's Birthday", 25, 5, null, NthOfMonth.None, false);
            gen.Add(new PublicHoliday(24, "Victoria Day", 0, 0, null, NthOfMonth.None, false, monarchsBirthday, 0, false, ShiftDayType.NoShift, -1));

            gen.AddToGroup(Holiday.EnglandAndWales, 3);
            gen.AddToGroup(Holiday.EnglandAndWales, 6);
            gen.AddToGroup(Holiday.EnglandAndWales, 7);
            gen.AddToGroup(Holiday.EnglandAndWales, 8);
            gen.AddToGroup(Holiday.EnglandAndWales, 10);
            gen.AddToGroup(Holiday.EnglandAndWales, 15);
            gen.AddToGroup(Holiday.EnglandAndWales, 2);
            gen.AddToGroup(Holiday.EnglandAndWales, 18);

            gen.AddToGroup(Holiday.Scotland, 3);
            gen.AddToGroup(Holiday.Scotland, 4);
            gen.AddToGroup(Holiday.Scotland, 6);
            gen.AddToGroup(Holiday.Scotland, 8);
            gen.AddToGroup(Holiday.Scotland, 10);
            gen.AddToGroup(Holiday.Scotland, 14);
            gen.AddToGroup(Holiday.Scotland, 17);
            gen.AddToGroup(Holiday.Scotland, 2);
            gen.AddToGroup(Holiday.Scotland, 18);

            gen.AddToGroup(Holiday.NorthernIreland, 3);
            gen.AddToGroup(Holiday.NorthernIreland, 5);
            gen.AddToGroup(Holiday.NorthernIreland, 6);
            gen.AddToGroup(Holiday.NorthernIreland, 7);
            gen.AddToGroup(Holiday.NorthernIreland, 8);
            gen.AddToGroup(Holiday.NorthernIreland, 10);
            gen.AddToGroup(Holiday.NorthernIreland, 12);
            gen.AddToGroup(Holiday.NorthernIreland, 15);
            gen.AddToGroup(Holiday.NorthernIreland, 2);
            gen.AddToGroup(Holiday.NorthernIreland, 18);

            gen.AddToGroup(Holiday.RepublicOfIreland, 3);
            gen.AddToGroup(Holiday.RepublicOfIreland, 5);
            gen.AddToGroup(Holiday.RepublicOfIreland, 7);
            gen.AddToGroup(Holiday.RepublicOfIreland, 9);
            gen.AddToGroup(Holiday.RepublicOfIreland, 11);
            gen.AddToGroup(Holiday.RepublicOfIreland, 13);
            gen.AddToGroup(Holiday.RepublicOfIreland, 16);
            gen.AddToGroup(Holiday.RepublicOfIreland, 2);
            gen.AddToGroup(Holiday.RepublicOfIreland, 18);

            gen.AddToGroup(Holiday.HongKong, 19);
            gen.AddToGroup(Holiday.HongKong, 20);

            gen.AddToGroup(Holiday.USA, 21);

            gen.AddToGroup(Holiday.France, 22);

            gen.AddToGroup(Holiday.Canada, 24);

            return gen.Schema;
        }
    }

}

#endif