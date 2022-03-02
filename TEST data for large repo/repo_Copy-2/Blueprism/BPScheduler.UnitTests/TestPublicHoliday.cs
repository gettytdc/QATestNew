#if UNITTESTS

using System;
using System.Collections.Generic;
using System.Data;
using BluePrism.Scheduling.Calendar;

using BluePrism.BPCoreLib.Collections;
//using BluePrism.Scheduling.Dirty;

using NUnit.Framework;
using BluePrism.Scheduling;

namespace BPScheduler.UnitTests
{

    [TestFixture]
    public class TestPublicHoliday
    {
        /// <summary>
        /// Slightly neater way to describe a map of int: ICollection&lt;DateTime&gt;.
        /// I got lost in chevrons when I added this to a map.
        /// </summary>
        private class YearBasedDateCollection : Dictionary<int, ICollection<DateTime>> { }

        /// <summary>
        /// Creates a new empty non-database-dependent datatable, with the columns and
        /// their respective types specified.
        /// </summary>
        /// <returns>A blank datatable set up to provide data regarding PublicHolidays
        /// in the same style as the database.</returns>
        private DataTable NewDataTable()
        {
            ScheduleCalendar cal = new ScheduleCalendar(schema);
            DaySet set = cal.WorkingWeek;
            set.Set(DayOfWeek.Thursday);
            set.Set(DayOfWeek.Saturday);
            set.Unset(DayOfWeek.Saturday);
            set.Unset(DayOfWeek.Friday);
            set.SetTo(DaySet.ALL_DAYS);

            DataTable dt = new DataTable();
            dt.Columns.Add("groupname", typeof(string));
            dt.Columns.Add("id", typeof(int));
            dt.Columns.Add("holidayname", typeof(string));
            dt.Columns.Add("dd", typeof(int));
            dt.Columns.Add("mm", typeof(int));
            dt.Columns.Add("dayofweek", typeof(int));
            dt.Columns.Add("nthofmonth", typeof(int));
            dt.Columns.Add("eastersunday", typeof(bool));
            dt.Columns.Add("relativetoholiday", typeof(int));
            dt.Columns.Add("relativedaydiff", typeof(int));
            dt.Columns.Add("excludesaturday", typeof(bool));
            dt.Columns.Add("shiftdaytypeid", typeof(int));
            dt.Columns.Add("relativedayofweek", typeof(int));
            return dt;
        }

        public static PublicHolidaySchema GetSchema()
        {
            DummyStore store = new DummyStore();
            return store.GetSchema();
        }

        private readonly PublicHolidaySchema schema = GetSchema();

        /// <summary>
        /// Tests that adding public holidays with circular dependencies
        /// fails with a CircularHolidayDependencyException.
        /// </summary>
        [Test]
        public void TestCircularDependency()
        {
            Assert.That(() =>
            {
                DataTable dt = NewDataTable();
                dt.Rows.Add(null, 1, "St Stuarts Day", null, null, null, null, null, 2, 1, null);
                dt.Rows.Add(null, 2, "St Stuarts Passover", 14, 12, null, null, null, null, null, null);
                dt.Rows.Add(null, 3, "St Stuarts Eve", null, null, null, null, null, 4, 5, null);
                dt.Rows.Add(null, 4, "St Stuarts Holiday", null, null, null, null, null, 5, -2, null);
                dt.Rows.Add(null, 5, "St Stuarts", null, null, null, null, null, 6, -1, null);
                dt.Rows.Add(null, 6, "St Stuart", null, null, null, null, null, 4, -1, null);

                new PublicHolidaySchema(dt);

            }, Throws.InstanceOf<CircularReferenceException>());
        }

        /// <summary>
        /// Tests a number of years for each of the supported public holiday groups.
        /// </summary>
        [Test]
        public void TestPublicHolidayGroups()
        {
            // Check the bank holidays for England & Wales 2010
            // Source:
            // http://www.direct.gov.uk/en/Governmentcitizensandrights/LivingintheUK/DG_073741

            YearBasedDateCollection englandAndWales = new YearBasedDateCollection();

            englandAndWales[2010] = new DateTime[] {
                new DateTime(2010,1,1),
                new DateTime(2010,4,2),
                new DateTime(2010,4,5),
                new DateTime(2010,5,3),
                new DateTime(2010,5,31),
                new DateTime(2010,8,30),
                new DateTime(2010,12,27),
                new DateTime(2010,12,28)
            };

            englandAndWales[2011] = new DateTime[] {
                new DateTime(2011,1,3),
                new DateTime(2011,4,22),
                new DateTime(2011,4,25),
                new DateTime(2011,5,2),
                new DateTime(2011,5,30),
                new DateTime(2011,8,29),
                new DateTime(2011,12,26),
                new DateTime(2011,12,27)
            };

            YearBasedDateCollection scotland = new YearBasedDateCollection();

            // 2008 tests St Andrews Day going across the end of month boundary
            // plus easter in March
            // Source: http://www.scotland.gov.uk/Publications/2005/01/bankholidays
            scotland[2008] = new DateTime[] {
                new DateTime(2008,1,1),
                new DateTime(2008,1,2),
                new DateTime(2008,3,21),
                new DateTime(2008,5,5),
                new DateTime(2008,5,26),
                new DateTime(2008,8,4),
                new DateTime(2008,12,1),
                new DateTime(2008,12,25),
                new DateTime(2008,12,26)
            };

            scotland[2010] = new DateTime[] {
                new DateTime(2010,1,1),
                new DateTime(2010,1,4),
                new DateTime(2010,4,2),
                new DateTime(2010,5,3),
                new DateTime(2010,5,31),
                new DateTime(2010,8,2),
                new DateTime(2010,11,30),
                new DateTime(2010,12,27),
                new DateTime(2010,12,28)
            };

            YearBasedDateCollection ni = new YearBasedDateCollection();

            // Tenuous - just to check that Orangemen's day moves correctly
            ni[2008] = new DateTime[] {
                new DateTime(2008,1,1),
                new DateTime(2008,3,17),
                new DateTime(2008,3,21),
                new DateTime(2008,3,24),
                new DateTime(2008,5,5),
                new DateTime(2008,5,26),
                new DateTime(2008,7,14),
                new DateTime(2008,8,25),
                new DateTime(2008,12,25),
                new DateTime(2008,12,26)
            };

            ni[2010] = new DateTime[] {
                new DateTime(2010,1,1),
                new DateTime(2010,3,17),
                new DateTime(2010,4,2),
                new DateTime(2010,4,5),
                new DateTime(2010,5,3),
                new DateTime(2010,5,31),
                new DateTime(2010,7,12),
                new DateTime(2010,8,30),
                new DateTime(2010,12,27),
                new DateTime(2010,12,28)
            };

            YearBasedDateCollection eire = new YearBasedDateCollection();

            eire[2010] = new DateTime[] {
                new DateTime(2010,1,1),
                new DateTime(2010,3,17),
                new DateTime(2010,4,5),
                new DateTime(2010,5,3),
                new DateTime(2010,6,7),
                new DateTime(2010,8,2),
                new DateTime(2010,10,25),
                new DateTime(2010,12,27),
                new DateTime(2010,12,28)
            };

            DateTime upcomingSaturday = DateTime.Now.Date.AddDays(1);
            while (upcomingSaturday.DayOfWeek != DayOfWeek.Saturday)
            {
                upcomingSaturday = upcomingSaturday.AddDays(1);
            }
            YearBasedDateCollection hongKong = new YearBasedDateCollection();
            hongKong[upcomingSaturday.Year] = new DateTime[] {
                upcomingSaturday,
                upcomingSaturday.AddDays(2)
            };

            YearBasedDateCollection usa = new YearBasedDateCollection();
            usa[2020] = new DateTime[] {
                new DateTime(2020,1,1)
            };
            usa[2022] = new DateTime[] {
                new DateTime(2021,12,31)
            };

            //Christmas 2021 falls on a Saturday, in France this holiday is not moved to the
            //next Monday as ShiftDayType = NoShift
            YearBasedDateCollection france = new YearBasedDateCollection();
            france[2021] = new DateTime[]
            {
                new DateTime(2021, 12, 25)
            };

            YearBasedDateCollection canada = new YearBasedDateCollection();
            canada[2020] = new DateTime[]
{
                new DateTime(2020, 5, 18)
};
            canada[2021] = new DateTime[]
            {
                new DateTime(2021, 5, 24)
            };

            IDictionary<string, YearBasedDateCollection> expected = new
                clsOrderedDictionary<string, YearBasedDateCollection>();

            expected["England and Wales"] = englandAndWales;
            expected["Scotland"] = scotland;
            expected["Northern Ireland"] = ni;
            expected["Republic of Ireland"] = eire;
            expected["Hong Kong"] = hongKong;
            expected["USA"] = usa;
            expected["France"] = france;
            expected["Canada"] = canada;

            // Initialise the public holidays
            if (schema == null)
            {
                Assert.Fail("Failed to initialise the public holidays");
                return;
            }

            // Okay, now go through the results and... well.. check them.
            foreach (string area in expected.Keys)
            {
                Assert.That(schema.GetGroups(), Contains.Item(area));

                foreach (int year in expected[area].Keys)
                {
                    ICollection<DateTime> coll = expected[area][year];
                    foreach (PublicHoliday hol in schema.GetHolidays(area))
                    {
                        var date = hol.GetOccurrence(year);
                        Assert.That(
                            expected[area][year],
                            Contains.Item(date),
                            "Failed area: " + area + " - year: " + year + " - hol: " + hol);
                        Assert.IsTrue(hol.IsHoliday(date));
                    }
                }
            }
        }
    }
}

#endif

