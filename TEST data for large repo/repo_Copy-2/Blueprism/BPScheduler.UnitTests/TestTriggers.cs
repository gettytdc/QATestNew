#if UNITTESTS
using System;
using System.Collections.Generic;
using System.Globalization;
using BluePrism.Scheduling.Triggers;
using BluePrism.Scheduling.Calendar;
using BluePrism.BPCoreLib.Collections;
using NUnit.Framework;
using BluePrism.BPCoreLib;
using BluePrism.Scheduling;

namespace BPScheduler.UnitTests
{
    /// <summary>
    /// Test class for the triggers within the scheduler.
    /// Ensuring that they choose (and omit) the correct times as configured.
    /// </summary>
    [TestFixture]
    public class TestTriggers
    {
        /// <summary>
        /// Dummy trigger class used to test BaseTrigger (since we can't instantiate
        /// BaseTrigger directly).
        /// Currently, this is functionally (almost) identical to NeverTrigger, but
        /// we shouldn't rely on that always being the case.
        /// </summary>
        private class DummyTrigger : BaseTrigger
        {
            public override ITriggerInstance GetNextInstance(DateTime after)
            {
                return null;
            }
        }

        /// <summary>
        /// Simplistic calendar which only allows running on dates after 
        /// a certain threshold.
        /// </summary>
        public class DateBasedCalendar : ScheduleCalendar
        {
            // The date at which this calendar starts allowing schedules to run.
            private DateTime _active;

            /// <summary>
            /// Creates a new date based calendar which becomes active on the
            /// given date.
            /// </summary>
            /// <param name="activeDate">The date at which this calendar begins
            /// allowing instances to run.</param>
            public DateBasedCalendar(DateTime activeDate)
                : base(null)
            {
                _active = activeDate;
            }

            /// <summary>
            /// Checks if this calendar allows running on the given date.
            /// </summary>
            /// <param name="dt">The date to check.</param>
            /// <returns>true to indicate this calendar will not inhibit
            /// the running of an instance on the given date, false to 
            /// indicate that it is not allowed by this calendar.</returns>
            public override bool CanRun(DateTime dt)
            {
                return dt >= _active;
            }

        }

        /// <summary>
        /// Utility method to test the activation dates/times for a trigger, given a
        /// specific start date.
        /// This will set the start date in the trigger, and then, beginning with a
        /// date/time one second before the start date, will request activation times
        /// from the trigger, each time through the loop using the resultant time
        /// as the input time for the next iteration.
        /// </summary>
        /// <param name="trig">The trigger to test. Any data required on the trigger
        /// for the purposes of the test (with the exception of the start date)
        /// should be set before calling this method. The start date will be set
        /// from this method.</param>
        /// <param name="start">The start date to set in the trigger. This also provides
        /// the initial date to use to begin getting activation times from the trigger.
        /// Note that the initial date used is <em>one second</em> before the start
        /// date, not the start date itself.
        /// </param>
        /// <param name="expectedValues">The expected activation times for the trigger
        /// beginning with the first value expected after one second before the start
        /// date/time. A value of DateTime.MinValue indicates that there should be
        /// no further instances returned. If that value is found (and the trigger
        /// returns no further instances) the method returns and any subsequent values
        /// will remain untested, since by definition, the trigger cannot return them.
        /// </param>
        private void RunTest(
            BaseTrigger trig, DateTime start, params DateTime[] expectedValues)
        {
            trig.Start = start;
            DateTime inst = start.AddSeconds(-1);
            foreach (DateTime dt in expectedValues)
            {
                try
                {
                    // MinValue indicates that there should be no next instance...
                    if (dt == DateTime.MinValue)
                    {
                        Assert.That(trig.GetNextInstance(inst), Is.Null);
                        // We have no further date to use at this point, so quit now.
                        return;
                    }
                    Assert.That(inst = trig.GetNextInstance(inst).When, Is.EqualTo(dt));
                }
                catch (NullReferenceException)
                {
                    Assert.Fail("Failed to find the next instance from {0:dd/MM/yyyy HH:mm:ss}: " +
                        "expected {1:dd/MM/yyyy HH:mm:ss}", inst, dt);
                }
            }
        }

        /// <summary>
        /// Utility method to test the activation dates/times for a trigger, given a
        /// specific start date.
        /// This will set the start date in the trigger, and then, beginning with a
        /// date/time one second before the start date, will request activation times
        /// from the trigger, each time through the loop using the resultant time
        /// as the input time for the next iteration.
        /// </summary>
        /// <param name="trig">The trigger to test. Any data required on the trigger
        /// for the purposes of the test (with the exception of the start date)
        /// should be set before calling this method. The start date will be set
        /// from this method.</param>
        /// <param name="start">The start date to set in the trigger. This also provides
        /// the initial date to use to begin getting activation times from the trigger.
        /// Note that the initial date used is <em>one second</em> before the start
        /// date, not the start date itself.
        /// This should be in the format "dd/MM/yyyy[ HH:mm[:ss]]" or
        /// "yyyy-MM-dd[ HH:mm[:ss]]", time is optional and, within that, seconds are
        /// optional. A null or empty string value is allowed in the expected values
        /// and is treated as DateTime.MinValue, ie. 'no expected time'
        /// </param>
        /// <param name="expectedValues">The expected activation times for the trigger
        /// beginning with the first value expected after one second before the start
        /// date/time. A value of DateTime.MinValue indicates that there should be
        /// no further instances returned. If that value is found (and the trigger
        /// returns no further instances) the method returns and any subsequent values
        /// will remain untested, since by definition, the trigger cannot return them.
        /// </param>
        private void RunTest(BaseTrigger trig, string start, params string[] expected)
        {
            string[] dateFmt = {
                "dd/MM/yyyy HH:mm:ss", "yyyy-MM-dd HH:mm:ss",
                "dd/MM/yyyy HH:mm", "yyyy-MM-dd HH:mm",
                "dd/MM/yyyy", "yyyy-MM-dd"
            };
            DateTime startTime =
                DateTime.ParseExact(start, dateFmt, null, DateTimeStyles.None);

            DateTime[] expectedDates = new DateTime[expected.Length];
            int index = -1;
            foreach (string str in expected)
            {
                DateTime dt;
                if (str == null || str.Length == 0)
                {
                    dt = DateTime.MinValue;
                }
                else
                {
                    dt = DateTime.ParseExact(str, dateFmt, null, DateTimeStyles.None);
                }
                expectedDates[++index] = dt;
            }
            RunTest(trig, startTime, expectedDates);
        }

        /// <summary>
        /// Tests that the trigger maintains second granularity, truncating any
        /// millisecond or below part of the date/time it is configured with.
        /// </summary>
        [Test]
        public void TestSecondGranularity()
        {
            DateTime date =
                new DateTime(2010, 5, 3, 23, 15, 12, 149);
            DateTime resolved =
                new DateTime(2010, 5, 3, 23, 15, 12, 0);

            // Test for BaseTrigger
            BaseTrigger trig = new DummyTrigger();
            trig.Start = date;
            Assert.That(trig.Start, Is.EqualTo(resolved));

            date = new DateTime(2010, 5, 3, 23, 15, 12, 999); // The same but with 999ms
            trig.Start = date;
            // should be the same - it should truncate, not round.
            Assert.That(trig.Start, Is.EqualTo(resolved));

            // Quick check for the end date too.
            trig.End = new DateTime(2010, 5, 10, 1, 30, 15, 999);
            Assert.That(trig.End, Is.EqualTo(new DateTime(2010, 5, 10, 1, 30, 15)));

        }

        /// <summary>
        /// Tests the basic 'every n days' trigger.
        /// </summary>
        [Test]
        public void TestEveryNDays()
        {
            // Test invalid values.
            try
            {
                new EveryNDays(0);
                Assert.Fail("EveryNDays(0) should fail with an ArgumentException");
            }
            catch (ArgumentException) { }

            try
            {
                new EveryNDays(-1);
                Assert.Fail("EveryNDays(-1) should fail with an ArgumentException");
            }
            catch (ArgumentException) { }

            EveryNDays trig = new EveryNDays(5);

            // This is a basic period-based trigger, test the standard case.
            RunTest(trig, new DateTime(2010, 8, 5, 9, 0, 0),
                new DateTime(2010, 8, 5, 9, 0, 0),
                new DateTime(2010, 8, 10, 9, 0, 0),
                new DateTime(2010, 8, 15, 9, 0, 0),
                new DateTime(2010, 8, 20, 9, 0, 0),
                new DateTime(2010, 8, 25, 9, 0, 0),
                new DateTime(2010, 8, 30, 9, 0, 0),
                new DateTime(2010, 9, 4, 9, 0, 0)
            );

            // Test that it stops at end, and that end is inclusive
            trig.End = new DateTime(2010, 8, 25, 9, 0, 0);
            RunTest(trig, new DateTime(2010, 8, 5, 9, 0, 0),
                new DateTime(2010, 8, 5, 9, 0, 0),
                new DateTime(2010, 8, 10, 9, 0, 0),
                new DateTime(2010, 8, 15, 9, 0, 0),
                new DateTime(2010, 8, 20, 9, 0, 0),
                new DateTime(2010, 8, 25, 9, 0, 0),
                DateTime.MinValue
            );

            // Test that end of 1s before an activation date doesn't trigger on that date.
            trig.End = new DateTime(2010, 8, 25, 8, 59, 59);
            RunTest(trig, new DateTime(2010, 8, 5, 9, 0, 0),
                new DateTime(2010, 8, 5, 9, 0, 0),
                new DateTime(2010, 8, 10, 9, 0, 0),
                new DateTime(2010, 8, 15, 9, 0, 0),
                new DateTime(2010, 8, 20, 9, 0, 0),
                DateTime.MinValue
            );
        }

        /// <summary>
        /// Tests a trigger which activates every n days only on days which fall
        /// within a particular dayset.
        /// </summary>
        [Test]
        public void TestEveryNDaysWithinDaySet()
        {
            // Basic test.
            BaseTrigger trig = new EveryNDaysWithinDaySet(5, DaySet.FiveDayWeek);
            RunTest(trig, new DateTime(2008, 8, 2),
                new DateTime(2008, 8, 7),
                new DateTime(2008, 8, 12),
                new DateTime(2008, 8, 22),
                new DateTime(2008, 8, 27),
                new DateTime(2008, 9, 1)
            );
            // Test end date stops it and that it's inclusive
            trig.End = new DateTime(2008, 8, 27);
            RunTest(trig, new DateTime(2008, 8, 2),
                new DateTime(2008, 8, 7),
                new DateTime(2008, 8, 12),
                new DateTime(2008, 8, 22),
                new DateTime(2008, 8, 27),
                DateTime.MinValue
            );

            // An empty dayset should return nothing.
            RunTest(new EveryNDaysWithinDaySet(new DaySet()),
                new DateTime(2008, 8, 2), DateTime.MinValue);

            // Look mum, no Mondays.
            DaySet allButMonday = new DaySet(
                DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday,
                DayOfWeek.Saturday, DayOfWeek.Sunday);

            RunTest(new EveryNDaysWithinDaySet(7, allButMonday),
                new DateTime(2008, 8, 18), DateTime.MinValue);

        }

        /// <summary>
        /// Tests the 'Every N Days Within ID'd Calendar' trigger.
        /// The way this works is to go every n days and see if that is a valid
        /// activation time according to the calendar, effectively 'skipping' 
        /// activation times which are disallowed by the calendar.
        /// </summary>
        [Test]
        public void TestEveryNDaysWithinIdentifiedCalendar()
        {
            DummyStore store = new DummyStore();
            ScheduleCalendar cal = new ScheduleCalendar(store.GetSchema());
            cal.WorkingWeek = DaySet.FiveDayWeek;
            cal.PublicHolidayGroup = DummyStore.Holiday.EnglandAndWales;
            store.SaveCalendar(cal);

            // We need a scheduler & schedule now to bind all this together
            // and allow the trigger access to the store (to retrieve the calendar)
            InertScheduler scheduler = new InertScheduler(store);
            DummySchedule sched = new DummySchedule(scheduler);

            try
            {
                new EveryNDaysWithinIdentifiedCalendar(0, cal.Id);
                Assert.Fail("EveryNDaysWithinIdentifiedCalendar(0, <id>) " +
                    "should fail with an ArgumentException");
            }
            catch (ArgumentException) { }

            try
            {
                new EveryNDaysWithinIdentifiedCalendar(-1, cal.Id);
                Assert.Fail("EveryNDaysWithinIdentifiedCalendar(-1, <id>) " +
                    "should fail with an ArgumentException");
            }
            catch (ArgumentException) { }

            BaseTrigger trig = new EveryNDaysWithinIdentifiedCalendar(2, cal.Id);
            // add to sched so trigger can get calendar from schedule => scheduler => store.
            sched.SetTrigger(trig);

            // Check it doesn't run over the weekend.
            RunTest(trig, new DateTime(2010, 8, 2, 9, 0, 0),
                new DateTime(2010, 8, 2, 9, 0, 0),
                new DateTime(2010, 8, 4, 9, 0, 0),
                new DateTime(2010, 8, 6, 9, 0, 0),
                new DateTime(2010, 8, 10, 9, 0, 0),
                new DateTime(2010, 8, 12, 9, 0, 0)
            );

            // Check the (August) bank holiday - falls on 30/08
            RunTest(trig, new DateTime(2010, 8, 26, 9, 0, 0),
                new DateTime(2010, 8, 26, 9, 0, 0),
                new DateTime(2010, 9, 1, 9, 0, 0),
                new DateTime(2010, 9, 3, 9, 0, 0),
                new DateTime(2010, 9, 7, 9, 0, 0)
            );

            // Finally, check that the end date is inclusive and works as expected.
            trig.End = new DateTime(2010, 9, 1, 9, 0, 0);
            RunTest(trig, new DateTime(2010, 8, 26, 9, 0, 0),
                new DateTime(2010, 8, 26, 9, 0, 0),
                new DateTime(2010, 9, 1, 9, 0, 0)
            );

            store.DeleteCalendar(cal);
        }

        /// <summary>
        /// Tests the basic 'every n hours' trigger.
        /// </summary>
        [Test]
        public void TestEveryNHours()
        {
            // Test invalid values.
            try
            {
                new EveryNHours(0);
                Assert.Fail("EveryNHours(0) should fail with an ArgumentException");
            }
            catch (ArgumentException) { }

            try
            {
                new EveryNHours(-1);
                Assert.Fail("EveryNHours(-1) should fail with an ArgumentException");
            }
            catch (ArgumentException) { }

            // This is a basic period-based trigger, test the standard case.
            RunTest(new EveryNHours(5), new DateTime(2010, 8, 5, 9, 0, 0),
                new DateTime(2010, 8, 5, 9, 0, 0),
                new DateTime(2010, 8, 5, 14, 0, 0),
                new DateTime(2010, 8, 5, 19, 0, 0),
                new DateTime(2010, 8, 6, 0, 0, 0),
                new DateTime(2010, 8, 6, 5, 0, 0),
                new DateTime(2010, 8, 6, 10, 0, 0)
            );

            BaseTrigger trig = new EveryNHours(5);
            trig.End = new DateTime(2010, 8, 6);
            RunTest(trig, new DateTime(2010, 8, 5, 9, 0, 0),
                new DateTime(2010, 8, 5, 9, 0, 0),
                new DateTime(2010, 8, 5, 14, 0, 0),
                new DateTime(2010, 8, 5, 19, 0, 0),
                new DateTime(2010, 8, 6, 0, 0, 0),
                DateTime.MinValue // should be no more after the end date.
            );

        }

        /// <summary>
        /// Tests the EveryNHoursWithinRange trigger, ensuring that the trigger is
        /// not activated outside the specified range.
        /// </summary>
        [Test]
        public void TestEveryNHoursWithinRange()
        {
            // Test invalid values for all constructors.
            {
                // Values to use for the invalid constructors
                TimeSpan startTime = new TimeSpan(9, 0, 0);
                TimeSpan endTime = new TimeSpan(17, 0, 0);
                clsTimeRange range = new clsTimeRange(startTime, endTime);

                try
                {
                    new EveryNHoursWithinRange(0, range);
                    Assert.Fail("EveryNHoursWithinRange(0,<TimeRange>) " +
                        "should fail with an ArgumentException");
                }
                catch (ArgumentException) { }

                try
                {
                    new EveryNHoursWithinRange(-1, range);
                    Assert.Fail("EveryNHoursWithinRange(-1,<TimeRange>) " +
                        "should fail with an ArgumentException");
                }
                catch (ArgumentException) { }

                try
                {
                    new EveryNHoursWithinRange(0, startTime, endTime);
                    Assert.Fail("EveryNHoursWithinRange(0,<TimeSpan>,<TimeSpan>) " +
                        "should fail with an ArgumentException");
                }
                catch (ArgumentException) { }

                try
                {
                    new EveryNHoursWithinRange(-1, startTime, endTime);
                    Assert.Fail("EveryNHoursWithinRange(-1,<TimeSpan>,<TimeSpan>) " +
                        "should fail with an ArgumentException");
                }
                catch (ArgumentException) { }

                try
                {
                    new EveryNHoursWithinRange(0, startTime, endTime, 1);
                    Assert.Fail("EveryNHoursWithinRange(0,<TimeSpan>,<TimeSpan>,<int>) " +
                        "should fail with an ArgumentException");
                }
                catch (ArgumentException) { }

                try
                {
                    new EveryNHoursWithinRange(-1, startTime, endTime, 1);
                    Assert.Fail("EveryNHoursWithinRange(-1,<TimeSpan>,<TimeSpan>,<int>) " +
                        "should fail with an ArgumentException");
                }
                catch (ArgumentException) { }

                try
                {
                    new EveryNHoursWithinRange(0, range, 1);
                    Assert.Fail("EveryNHoursWithinRange(0,<TimeRange>,<int>) " +
                        "should fail with an ArgumentException");
                }
                catch (ArgumentException) { }

                try
                {
                    new EveryNHoursWithinRange(-1, range, 1);
                    Assert.Fail("EveryNHoursWithinRange(-1,<TimeRange>,<int>) " +
                        "should fail with an ArgumentException");
                }
                catch (ArgumentException) { }

                // Test that it fails when start time is after end time
                try
                {
                    new EveryNHoursWithinRange(1, endTime, startTime);
                    Assert.Fail(
                        "EveryNHoursWithinRange(<int>,'{0:HH:mm:ss}','{1:HH:mm:ss}') " +
                        "should fail with an ArgumentException", endTime, startTime);
                }
                catch (ArgumentException) { }

                try
                {
                    new EveryNHoursWithinRange(1, endTime, startTime, 1);
                    Assert.Fail(
                        "EveryNHoursWithinRange(<int>,'{0:HH:mm:ss}','{1:HH:mm:ss}',<int>) " +
                        "should fail with an ArgumentException", endTime, startTime);
                }
                catch (ArgumentException) { }
            }

            EveryNHoursWithinRange trig =
                new EveryNHoursWithinRange(1, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0));

            // Test that it returns both the boundary dates/times are included, they are
            // supposed to be inclusive.
            RunTest(trig, new DateTime(2010, 5, 5, 9, 0, 0),
                new DateTime(2010, 5, 5, 9, 0, 0),  // boundary date #1
                new DateTime(2010, 5, 5, 10, 0, 0),
                new DateTime(2010, 5, 5, 11, 0, 0),
                new DateTime(2010, 5, 5, 12, 0, 0),
                new DateTime(2010, 5, 5, 13, 0, 0),
                new DateTime(2010, 5, 5, 14, 0, 0),
                new DateTime(2010, 5, 5, 15, 0, 0),
                new DateTime(2010, 5, 5, 16, 0, 0),
                new DateTime(2010, 5, 5, 17, 0, 0), // boundary date #2
                new DateTime(2010, 5, 6, 9, 0, 0)   // boundary date #1 for next day.
            );


            // Test a start date of before the time range

            // It goes every hour from the start time, ie. 6:21 - 7:21 - 8:21 - [9:21] - etc...
            RunTest(trig, new DateTime(2010, 5, 5, 6, 21, 0),
                new DateTime(2010, 5, 5, 9, 21, 0),
                new DateTime(2010, 5, 5, 10, 21, 0),
                new DateTime(2010, 5, 5, 11, 21, 0),
                new DateTime(2010, 5, 5, 12, 21, 0),
                new DateTime(2010, 5, 5, 13, 21, 0),
                new DateTime(2010, 5, 5, 14, 21, 0),
                new DateTime(2010, 5, 5, 15, 21, 0),
                new DateTime(2010, 5, 5, 16, 21, 0),
                new DateTime(2010, 5, 6, 9, 21, 0)
            );


            // Half way through the day:
            RunTest(trig, new DateTime(2010, 5, 5, 12, 31, 0),
                new DateTime(2010, 5, 5, 12, 31, 0),
                new DateTime(2010, 5, 5, 13, 31, 0),
                new DateTime(2010, 5, 5, 14, 31, 0),
                new DateTime(2010, 5, 5, 15, 31, 0),
                new DateTime(2010, 5, 5, 16, 31, 0),
                new DateTime(2010, 5, 6, 9, 31, 0),
                new DateTime(2010, 5, 6, 10, 31, 0),
                new DateTime(2010, 5, 6, 11, 31, 0),
                new DateTime(2010, 5, 6, 12, 31, 0)
            );


            // After the working day
            RunTest(trig, new DateTime(2010, 5, 5, 19, 41, 0),
                new DateTime(2010, 5, 6, 9, 41, 0),
                new DateTime(2010, 5, 6, 10, 41, 0),
                new DateTime(2010, 5, 6, 11, 41, 0),
                new DateTime(2010, 5, 6, 12, 41, 0),
                new DateTime(2010, 5, 6, 13, 41, 0),
                new DateTime(2010, 5, 6, 14, 41, 0),
                new DateTime(2010, 5, 6, 15, 41, 0),
                new DateTime(2010, 5, 6, 16, 41, 0),
                new DateTime(2010, 5, 7, 9, 41, 0)
            );


            // test with a calendar
            {
                DummyStore store = new DummyStore();
                ScheduleCalendar cal = new ScheduleCalendar(store.GetSchema());
                cal.WorkingWeek = DaySet.FiveDayWeek;
                store.SaveCalendar(cal);

                // We need a scheduler & schedule now to bind all this together
                // and allow the trigger access to the store (to retrieve the calendar)
                InertScheduler scheduler = new InertScheduler(store);
                DummySchedule sched = new DummySchedule(scheduler);

                trig = new EveryNHoursWithinRange(
                    1, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), cal.Id);

                // add to sched so trigger can get calendar from schedule => scheduler => store.
                sched.SetTrigger(trig);

                RunTest(trig, new DateTime(2010, 5, 5, 19, 41, 0),
                    new DateTime(2010, 5, 6, 9, 41, 0),
                    new DateTime(2010, 5, 6, 10, 41, 0),
                    new DateTime(2010, 5, 6, 11, 41, 0),
                    new DateTime(2010, 5, 6, 12, 41, 0),
                    new DateTime(2010, 5, 6, 13, 41, 0),
                    new DateTime(2010, 5, 6, 14, 41, 0),
                    new DateTime(2010, 5, 6, 15, 41, 0),
                    new DateTime(2010, 5, 6, 16, 41, 0),
                    new DateTime(2010, 5, 7, 9, 41, 0),
                    new DateTime(2010, 5, 7, 10, 41, 0),
                    new DateTime(2010, 5, 7, 11, 41, 0),
                    new DateTime(2010, 5, 7, 12, 41, 0),
                    new DateTime(2010, 5, 7, 13, 41, 0),
                    new DateTime(2010, 5, 7, 14, 41, 0),
                    new DateTime(2010, 5, 7, 15, 41, 0),
                    new DateTime(2010, 5, 7, 16, 41, 0),
                    // 8th and 9th are Sat + Sun, so this should now skip to Monday
                    new DateTime(2010, 5, 10, 9, 41, 0),
                    new DateTime(2010, 5, 10, 10, 41, 0),
                    new DateTime(2010, 5, 10, 11, 41, 0)
                );
                store.DeleteCalendar(cal);
            }

            // test that not giving a time range works as expected
            RunTest(new EveryNHoursWithinRange(1), new DateTime(2010, 5, 5),
                new DateTime(2010, 5, 5, 0, 0, 0),
                new DateTime(2010, 5, 5, 1, 0, 0),
                new DateTime(2010, 5, 5, 2, 0, 0),
                new DateTime(2010, 5, 5, 3, 0, 0),
                new DateTime(2010, 5, 5, 4, 0, 0),
                new DateTime(2010, 5, 5, 5, 0, 0),
                new DateTime(2010, 5, 5, 6, 0, 0),
                new DateTime(2010, 5, 5, 7, 0, 0),
                new DateTime(2010, 5, 5, 8, 0, 0),
                new DateTime(2010, 5, 5, 9, 0, 0),
                new DateTime(2010, 5, 5, 10, 0, 0),
                new DateTime(2010, 5, 5, 11, 0, 0),
                new DateTime(2010, 5, 5, 12, 0, 0),
                new DateTime(2010, 5, 5, 13, 0, 0),
                new DateTime(2010, 5, 5, 14, 0, 0),
                new DateTime(2010, 5, 5, 15, 0, 0),
                new DateTime(2010, 5, 5, 16, 0, 0),
                new DateTime(2010, 5, 5, 17, 0, 0),
                new DateTime(2010, 5, 5, 18, 0, 0),
                new DateTime(2010, 5, 5, 19, 0, 0),
                new DateTime(2010, 5, 5, 20, 0, 0),
                new DateTime(2010, 5, 5, 21, 0, 0),
                new DateTime(2010, 5, 5, 22, 0, 0),
                new DateTime(2010, 5, 5, 23, 0, 0),
                new DateTime(2010, 5, 6, 0, 0, 0),
                new DateTime(2010, 5, 6, 1, 0, 0)
            );
        }

        /// <summary>
        /// Tests the basic 'every n minutes' trigger.
        /// </summary>
        [Test]
        public void TestEveryNMinutes()
        {
            // Test invalid values.
            try
            {
                new EveryNMinutes(0);
                Assert.Fail("EveryNMinutes(0) should fail with an ArgumentException");
            }
            catch (ArgumentException) { }

            try
            {
                new EveryNMinutes(-1);
                Assert.Fail("EveryNMinutes(-1) should fail with an ArgumentException");
            }
            catch (ArgumentException) { }

            // This is a basic period-based trigger, test the standard case.
            RunTest(new EveryNMinutes(30), new DateTime(2010, 8, 5, 9, 0, 0),
                new DateTime(2010, 8, 5, 9, 0, 0),
                new DateTime(2010, 8, 5, 9, 30, 0),
                new DateTime(2010, 8, 5, 10, 0, 0),
                new DateTime(2010, 8, 5, 10, 30, 0),
                new DateTime(2010, 8, 5, 11, 0, 0),
                new DateTime(2010, 8, 5, 11, 30, 0),
                new DateTime(2010, 8, 5, 12, 0, 0)
            );

            BaseTrigger trig = new EveryNMinutes(30);
            trig.End = new DateTime(2010, 8, 6);
            RunTest(trig, new DateTime(2010, 8, 5, 23, 0, 0),
                new DateTime(2010, 8, 5, 23, 0, 0),
                new DateTime(2010, 8, 5, 23, 30, 0),
                new DateTime(2010, 8, 6, 0, 0, 0),
                DateTime.MinValue // should be no more after the end date.
            );
        }

        /// <summary>
        /// Tests the "every n minutes within a range" trigger.
        /// Veru similary set of tests to the "every n hours within a range" trigger,
        /// since only the unit differs between them.
        /// </summary>
        [Test]
        public void TestEveryNMinutesWithinRange()
        {

            // Test invalid values for all constructors.
            {
                // Values to use for the invalid constructors
                TimeSpan startTime = new TimeSpan(9, 0, 0);
                TimeSpan endTime = new TimeSpan(17, 0, 0);
                clsTimeRange range = new clsTimeRange(startTime, endTime);

                try
                {
                    new EveryNMinutesWithinRange(0, range);
                    Assert.Fail("EveryNMinutesWithinRange(0,<TimeRange>) " +
                        "should fail with an ArgumentException");
                }
                catch (ArgumentException) { }

                try
                {
                    new EveryNMinutesWithinRange(-1, range);
                    Assert.Fail("EveryNMinutesWithinRange(-1,<TimeRange>) " +
                        "should fail with an ArgumentException");
                }
                catch (ArgumentException) { }

                try
                {
                    new EveryNMinutesWithinRange(0, startTime, endTime);
                    Assert.Fail("EveryNMinutesWithinRange(0,<TimeSpan>,<TimeSpan>) " +
                        "should fail with an ArgumentException");
                }
                catch (ArgumentException) { }

                try
                {
                    new EveryNMinutesWithinRange(-1, startTime, endTime);
                    Assert.Fail("EveryNMinutesWithinRange(-1,<TimeSpan>,<TimeSpan>) " +
                        "should fail with an ArgumentException");
                }
                catch (ArgumentException) { }

                try
                {
                    new EveryNMinutesWithinRange(0, startTime, endTime, 1);
                    Assert.Fail("EveryNMinutesWithinRange(0,<TimeSpan>,<TimeSpan>,<int>) " +
                        "should fail with an ArgumentException");
                }
                catch (ArgumentException) { }

                try
                {
                    new EveryNMinutesWithinRange(-1, startTime, endTime, 1);
                    Assert.Fail("EveryNMinutesWithinRange(-1,<TimeSpan>,<TimeSpan>,<int>) " +
                        "should fail with an ArgumentException");
                }
                catch (ArgumentException) { }

                try
                {
                    new EveryNMinutesWithinRange(0, range, 1);
                    Assert.Fail("EveryNMinutesWithinRange(0,<TimeRange>,<int>) " +
                        "should fail with an ArgumentException");
                }
                catch (ArgumentException) { }

                try
                {
                    new EveryNMinutesWithinRange(-1, range, 1);
                    Assert.Fail("EveryNMinutesWithinRange(-1,<TimeRange>,<int>) " +
                        "should fail with an ArgumentException");
                }
                catch (ArgumentException) { }

                // Test that it fails when start time is after end time
                try
                {
                    new EveryNMinutesWithinRange(1, endTime, startTime);
                    Assert.Fail(
                        "EveryNMinutesWithinRange(<int>,'{0:HH:mm:ss}','{1:HH:mm:ss}') " +
                        "should fail with an ArgumentException", endTime, startTime);
                }
                catch (ArgumentException) { }

                try
                {
                    new EveryNMinutesWithinRange(1, endTime, startTime, 1);
                    Assert.Fail(
                        "EveryNMinutesWithinRange(<int>,'{0:HH:mm:ss}','{1:HH:mm:ss}',<int>) " +
                        "should fail with an ArgumentException", endTime, startTime);
                }
                catch (ArgumentException) { }
            }

            EveryNMinutesWithinRange trig =
                new EveryNMinutesWithinRange(15, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0));

            // Test that the boundary times hold - early boundary time.
            RunTest(trig, new DateTime(2008, 8, 15, 9, 0, 0),
                new DateTime(2008, 8, 15, 9, 0, 0),
                new DateTime(2008, 8, 15, 9, 15, 0)
            );

            // Late boundary time
            RunTest(trig, new DateTime(2008, 8, 15, 17, 0, 0),
                new DateTime(2008, 8, 15, 17, 0, 0),
                new DateTime(2008, 8, 16, 9, 0, 0)
            );

            // Start date before the range
            RunTest(trig, new DateTime(2008, 8, 15),
                new DateTime(2008, 8, 15, 9, 0, 0),
                new DateTime(2008, 8, 15, 9, 15, 0)
            );

            // Start date 'after' the range
            RunTest(trig, new DateTime(2008, 8, 15, 18, 0, 0),
                new DateTime(2008, 8, 16, 9, 0, 0),
                new DateTime(2008, 8, 16, 9, 15, 0)
            );

            // Start date in the (relative) middle of the range *not* on a 15 min boundary
            RunTest(trig, new DateTime(2008, 8, 15, 16, 40, 0),
                new DateTime(2008, 8, 15, 16, 40, 0),
                new DateTime(2008, 8, 15, 16, 55, 0),
                new DateTime(2008, 8, 16, 9, 10, 0),
                new DateTime(2008, 8, 16, 9, 25, 0)
            );

            // Test with a calendar.
            {
                DummyStore store = new DummyStore();
                ScheduleCalendar cal = new ScheduleCalendar(store.GetSchema());
                cal.WorkingWeek = DaySet.FiveDayWeek;
                store.SaveCalendar(cal);

                // We need a scheduler & schedule now to bind all this together
                // and allow the trigger access to the store (to retrieve the calendar)
                InertScheduler scheduler = new InertScheduler(store);
                DummySchedule sched = new DummySchedule(scheduler);

                trig = new EveryNMinutesWithinRange(
                    15, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), cal.Id);

                // add to sched so trigger can get calendar from schedule => scheduler => store.
                sched.SetTrigger(trig);

                RunTest(trig, new DateTime(2008, 8, 15, 16, 40, 0),
                    new DateTime(2008, 8, 15, 16, 40, 0),
                    new DateTime(2008, 8, 15, 16, 55, 0), // 15/08/2008 was a Friday - skip weekend
                    new DateTime(2008, 8, 18, 9, 10, 0), // 18/08/2008 was the following Monday.
                    new DateTime(2008, 8, 18, 9, 25, 0)
                );

                store.DeleteCalendar(cal);
            }

        }

        /// <summary>
        /// Tests the every n seconds trigger. This is used as a base for
        /// quite a few of the basic "every n &lt;interval&gt;" triggers, so
        /// it's an important one to get right.
        /// </summary>
        [Test]
        public void TestEveryNSeconds()
        {
            DateTime start = DateTime.Now;
            EveryNSeconds trig = new EveryNSeconds(1);
            trig.Start = start;
            // trig.Start resolves the date to remove any sub-second component of the trigger.
            // so the round trip will (probably) make it a different start time
            start = trig.Start;

            IDictionary<DateTime, DateTime> map = new Dictionary<DateTime, DateTime>();

            // For map : key is "after time" - value is "result time"
            // ie. when GetNextInstance() is called with the key, the result should be the value.
            map[start] = start.AddMilliseconds(1000);
            map[start.AddMilliseconds(1000)] = start.AddMilliseconds(2000);
            map[start.AddDays(20)] = start.AddDays(20).AddMilliseconds(1000);

            foreach (DateTime key in map.Keys)
            {
                ITriggerInstance inst = trig.GetNextInstance(key);
                // make sure we get an instance.
                Assert.That(inst, Is.Not.Null);

                // Should be fire (by default)
                Assert.That(inst.Mode, Is.EqualTo(TriggerMode.Fire));

                // It should point to the correct trigger.
                Assert.That(inst.Trigger, Is.EqualTo(trig));

                // Finally, the date/time should match the value in the map
                Assert.That(inst.When, Is.EqualTo(map[key]),
                    "Bad Trigger Time: Start: {0}; Trigger Says: {1}; Should say: {2}",
                    start, inst.When, map[key]);

            }

            // Set it to end 1s less than 1 day later
            trig.End = start.AddDays(1).AddSeconds(-1);

            // Check it doesn't return instances past its end date
            {
                // There should be no instances after 500ms less than 1 day later
                DateTime justBeforeEnd = start.AddDays(1).AddMilliseconds(-500);
                ITriggerInstance inst = trig.GetNextInstance(justBeforeEnd);
                Assert.That(inst, Is.Null,
                    "Found trigger after End - End: {0}; After: {1}; Trigger Time: {2}",
                    trig.End, justBeforeEnd, (inst == null ? DateTime.MinValue : inst.When));
            }
        }

        [Test]
        public void TestEveryNthIdentifiedCalendarDayInNthMonth()
        {
            // Test invalid values.
            try
            {
                new EveryNthIdentifiedCalendarDayInNthMonth(0, NthOfMonth.First, 1);
                Assert.Fail("EveryNthIdentifiedCalendarDayInNthMonth(0, <NthOfMonth>, <int>) " +
                    "should fail with an ArgumentException");
            }
            catch (ArgumentException) { }

            try
            {
                new EveryNthIdentifiedCalendarDayInNthMonth(-1, NthOfMonth.First, 1);
                Assert.Fail("EveryNthIdentifiedCalendarDayInNthMonth(-1, <NthOfMonth>, <int>) " +
                    "should fail with an ArgumentException");
            }
            catch (ArgumentException) { }

            DummyStore store = new DummyStore();

            ScheduleCalendar cal = new ScheduleCalendar(store.GetSchema());
            cal.WorkingWeek = DaySet.FiveDayWeek;
            cal.PublicHolidayGroup = DummyStore.Holiday.EnglandAndWales;

            ScheduleCalendar emptyCal = new ScheduleCalendar(store.GetSchema());
            emptyCal.WorkingWeek = new DaySet();

            ScheduleCalendar dateCal =
                new DateBasedCalendar(TriggerBounds.LatestSupportedDate.Date);

            store.SaveCalendar(cal);
            store.SaveCalendar(emptyCal);
            store.SaveCalendar(dateCal);

            // We need a scheduler & schedule now to bind all this together
            // and allow the trigger access to the store (to retrieve the calendar)
            InertScheduler scheduler = new InertScheduler(store);
            DummySchedule sched = new DummySchedule(scheduler);

            EveryNthIdentifiedCalendarDayInNthMonth trig;

            // NthOfMonth.None should never activate
            trig = new EveryNthIdentifiedCalendarDayInNthMonth(1, NthOfMonth.None, cal.Id);
            sched.SetTrigger(trig);
            Assert.That(trig.GetNextInstance(DateTime.MinValue), Is.Null);

            // Check that a calendar with no valid days doesn't return a next instance
            trig = new EveryNthIdentifiedCalendarDayInNthMonth(6, NthOfMonth.First, emptyCal.Id);
            sched.SetTrigger(trig);
            RunTest(trig, new DateTime(2009, 6, 1, 9, 0, 0), DateTime.MinValue);

            // Check that at least up to LatestSupportedDate is included.
            trig = new EveryNthIdentifiedCalendarDayInNthMonth(1, NthOfMonth.Last, dateCal.Id);
            sched.SetTrigger(trig);
            RunTest(trig, new DateTime(2009, 6, 1, 9, 0, 0),
                TriggerBounds.LatestSupportedDate.Date.AddHours(9),
                DateTime.MinValue
            );

            // Last of the month
            trig = new EveryNthIdentifiedCalendarDayInNthMonth(2, NthOfMonth.Last, cal.Id);
            sched.SetTrigger(trig);
            RunTest(trig, new DateTime(2009, 6, 1, 9, 0, 0),
                new DateTime(2009, 6, 30, 9, 0, 0),
                new DateTime(2009, 8, 28, 9, 0, 0),
                new DateTime(2009, 10, 30, 9, 0, 0)
            );

            // Check end date works and is inclusive
            trig.End = new DateTime(2009, 8, 28, 9, 0, 0);
            RunTest(trig, new DateTime(2009, 6, 1, 9, 0, 0),
                new DateTime(2009, 6, 30, 9, 0, 0),
                new DateTime(2009, 8, 28, 9, 0, 0),
                DateTime.MinValue
            );

            // First of the month
            trig = new EveryNthIdentifiedCalendarDayInNthMonth(1, NthOfMonth.First, cal.Id);
            sched.SetTrigger(trig);
            RunTest(trig, new DateTime(2009, 6, 1, 9, 0, 0),
                new DateTime(2009, 6, 1, 9, 0, 0),
                new DateTime(2009, 7, 1, 9, 0, 0),
                new DateTime(2009, 8, 3, 9, 0, 0),
                new DateTime(2009, 9, 1, 9, 0, 0),
                new DateTime(2009, 10, 1, 9, 0, 0),
                new DateTime(2009, 11, 2, 9, 0, 0),
                new DateTime(2009, 12, 1, 9, 0, 0),
                new DateTime(2010, 1, 4, 9, 0, 0)
            );

            // Second of the month
            trig = new EveryNthIdentifiedCalendarDayInNthMonth(1, NthOfMonth.Second, cal.Id);
            sched.SetTrigger(trig);
            RunTest(trig, new DateTime(2009, 6, 1, 9, 0, 0),
                new DateTime(2009, 6, 2, 9, 0, 0),
                new DateTime(2009, 7, 2, 9, 0, 0),
                new DateTime(2009, 8, 4, 9, 0, 0),
                new DateTime(2009, 9, 2, 9, 0, 0),
                new DateTime(2009, 10, 2, 9, 0, 0),
                new DateTime(2009, 11, 3, 9, 0, 0),
                new DateTime(2009, 12, 2, 9, 0, 0),
                new DateTime(2010, 1, 5, 9, 0, 0)
            );

            // Third of the month
            trig = new EveryNthIdentifiedCalendarDayInNthMonth(1, NthOfMonth.Third, cal.Id);
            sched.SetTrigger(trig);
            RunTest(trig, new DateTime(2009, 6, 1, 9, 0, 0),
                new DateTime(2009, 6, 3, 9, 0, 0),
                new DateTime(2009, 7, 3, 9, 0, 0),
                new DateTime(2009, 8, 5, 9, 0, 0),
                new DateTime(2009, 9, 3, 9, 0, 0),
                new DateTime(2009, 10, 5, 9, 0, 0),
                new DateTime(2009, 11, 4, 9, 0, 0),
                new DateTime(2009, 12, 3, 9, 0, 0),
                new DateTime(2010, 1, 6, 9, 0, 0)
            );

            // Fourth of the month
            trig = new EveryNthIdentifiedCalendarDayInNthMonth(1, NthOfMonth.Fourth, cal.Id);
            sched.SetTrigger(trig);
            RunTest(trig, new DateTime(2009, 6, 1, 9, 0, 0),
                new DateTime(2009, 6, 4, 9, 0, 0),
                new DateTime(2009, 7, 6, 9, 0, 0),
                new DateTime(2009, 8, 6, 9, 0, 0),
                new DateTime(2009, 9, 4, 9, 0, 0),
                new DateTime(2009, 10, 6, 9, 0, 0),
                new DateTime(2009, 11, 5, 9, 0, 0),
                new DateTime(2009, 12, 4, 9, 0, 0),
                new DateTime(2010, 1, 7, 9, 0, 0)
            );

            // Fifth of the month
            trig = new EveryNthIdentifiedCalendarDayInNthMonth(1, NthOfMonth.Fifth, cal.Id);
            sched.SetTrigger(trig);
            RunTest(trig, new DateTime(2009, 6, 1, 9, 0, 0),
                new DateTime(2009, 6, 5, 9, 0, 0),
                new DateTime(2009, 7, 7, 9, 0, 0),
                new DateTime(2009, 8, 7, 9, 0, 0),
                new DateTime(2009, 9, 7, 9, 0, 0),
                new DateTime(2009, 10, 7, 9, 0, 0),
                new DateTime(2009, 11, 6, 9, 0, 0),
                new DateTime(2009, 12, 7, 9, 0, 0),
                new DateTime(2010, 1, 8, 9, 0, 0)
            );

            // check with scottish holidays - 3rd Aug and 2nd of Jan become holidays - means that
            // the first working day in Aug2009 is 4th (not the 3rd), and first working day in
            // 2010 is the 5th (not the 4th). All else should remain the same.
            cal.PublicHolidayGroup = DummyStore.Holiday.Scotland;
            store.SaveCalendar(cal);

            // First of the month (Scottish Holidays)
            trig = new EveryNthIdentifiedCalendarDayInNthMonth(1, NthOfMonth.First, cal.Id);
            sched.SetTrigger(trig);
            RunTest(trig, new DateTime(2009, 6, 1, 9, 0, 0),
                new DateTime(2009, 6, 1, 9, 0, 0),
                new DateTime(2009, 7, 1, 9, 0, 0),
                new DateTime(2009, 8, 4, 9, 0, 0),
                new DateTime(2009, 9, 1, 9, 0, 0),
                new DateTime(2009, 10, 1, 9, 0, 0),
                new DateTime(2009, 11, 2, 9, 0, 0),
                new DateTime(2009, 12, 1, 9, 0, 0),
                new DateTime(2010, 1, 5, 9, 0, 0)
            );

            // Second of the month (Scottish Holidays)
            trig = new EveryNthIdentifiedCalendarDayInNthMonth(1, NthOfMonth.Second, cal.Id);
            sched.SetTrigger(trig);
            RunTest(trig, new DateTime(2009, 6, 1, 9, 0, 0),
                new DateTime(2009, 6, 2, 9, 0, 0),
                new DateTime(2009, 7, 2, 9, 0, 0),
                new DateTime(2009, 8, 5, 9, 0, 0),
                new DateTime(2009, 9, 2, 9, 0, 0),
                new DateTime(2009, 10, 2, 9, 0, 0),
                new DateTime(2009, 11, 3, 9, 0, 0),
                new DateTime(2009, 12, 2, 9, 0, 0),
                new DateTime(2010, 1, 6, 9, 0, 0)
            );

            // Third of the month (Scottish Holidays)
            trig = new EveryNthIdentifiedCalendarDayInNthMonth(1, NthOfMonth.Third, cal.Id);
            sched.SetTrigger(trig);
            RunTest(trig, new DateTime(2009, 6, 1, 9, 0, 0),
                new DateTime(2009, 6, 3, 9, 0, 0),
                new DateTime(2009, 7, 3, 9, 0, 0),
                new DateTime(2009, 8, 6, 9, 0, 0),
                new DateTime(2009, 9, 3, 9, 0, 0),
                new DateTime(2009, 10, 5, 9, 0, 0),
                new DateTime(2009, 11, 4, 9, 0, 0),
                new DateTime(2009, 12, 3, 9, 0, 0),
                new DateTime(2010, 1, 7, 9, 0, 0)
            );

            // Fourth of the month (Scottish Holidays)
            trig = new EveryNthIdentifiedCalendarDayInNthMonth(1, NthOfMonth.Fourth, cal.Id);
            sched.SetTrigger(trig);
            RunTest(trig, new DateTime(2009, 6, 1, 9, 0, 0),
                new DateTime(2009, 6, 4, 9, 0, 0),
                new DateTime(2009, 7, 6, 9, 0, 0),
                new DateTime(2009, 8, 7, 9, 0, 0),
                new DateTime(2009, 9, 4, 9, 0, 0),
                new DateTime(2009, 10, 6, 9, 0, 0),
                new DateTime(2009, 11, 5, 9, 0, 0),
                new DateTime(2009, 12, 4, 9, 0, 0),
                new DateTime(2010, 1, 8, 9, 0, 0)
            );

            // Fifth of the month (Scottish Holidays)
            trig = new EveryNthIdentifiedCalendarDayInNthMonth(1, NthOfMonth.Fifth, cal.Id);
            sched.SetTrigger(trig);
            RunTest(trig, new DateTime(2009, 6, 1, 9, 0, 0),
                new DateTime(2009, 6, 5, 9, 0, 0),
                new DateTime(2009, 7, 7, 9, 0, 0),
                new DateTime(2009, 8, 10, 9, 0, 0),
                new DateTime(2009, 9, 7, 9, 0, 0),
                new DateTime(2009, 10, 7, 9, 0, 0),
                new DateTime(2009, 11, 6, 9, 0, 0),
                new DateTime(2009, 12, 7, 9, 0, 0),
                new DateTime(2010, 1, 11, 9, 0, 0)
            );
        }

        [Test]
        public void TestEveryNthKnownCalendarDayInNthMonth()
        {
            EveryNthKnownCalendarDayInNthMonth trig;

            ICalendar workingWeekCal = new DaySetCalendar(DaySet.FiveDayWeek);

            // The last working day of every quarter
            trig = new EveryNthKnownCalendarDayInNthMonth(
                3, NthOfMonth.Last, workingWeekCal);

            // Test that this is inclusive
            trig.End = new DateTime(2012, 12, 31);

            RunTest(trig, "31/12/2009",
                "31/12/2009", // Start Date = Thu
                "31/03/2010", // Wed
                "30/06/2010", // Wed
                "30/09/2010", // Thu
                "31/12/2010", // Fri
                "31/03/2011", // Thu
                "30/06/2011", // Thu
                "30/09/2011", // Fri
                "30/12/2011", // 31=Sat
                "30/03/2012", // 31=Sat
                "29/06/2012", // 30=Sat
                "28/09/2012", // 30=Sun
                "31/12/2012", // 31=Mon
                null);

            trig = new EveryNthKnownCalendarDayInNthMonth(
                1, NthOfMonth.Second, workingWeekCal);
            RunTest(trig, "01/01/2010",
                "04/01/2010",
                "02/02/2010",
                "02/03/2010",
                "02/04/2010",
                "04/05/2010");

            // Every 5 months, ending in a month where the first working day
            // is the first day of the month
            trig = new EveryNthKnownCalendarDayInNthMonth(
                5, NthOfMonth.First, workingWeekCal);
            trig.End = new DateTime(2013, 5, 31);
            RunTest(trig, "01/01/2010",
                "01/01/2010",
                "01/06/2010",
                "01/11/2010",
                "01/04/2011",
                "01/09/2011",
                "01/02/2012",
                "02/07/2012",
                "03/12/2012",
                "01/05/2013",
                null);

            // Same test, but ending in a month where the first working day
            // is *not* the first day of the month
            trig.End = new DateTime(2012, 12, 31);
            RunTest(trig, "01/01/2010",
                "01/01/2010",
                "01/06/2010",
                "01/11/2010",
                "01/04/2011",
                "01/09/2011",
                "01/02/2012",
                "02/07/2012",
                "03/12/2012",
                null);

            // Same test, but ending in a month in which the trigger would activate,
            // but because of the calendar, the date it would activate is after the
            // end date.
            trig.End = new DateTime(2012, 12, 01);
            RunTest(trig, "01/01/2010",
                "01/01/2010",
                "01/06/2010",
                "01/11/2010",
                "01/04/2011",
                "01/09/2011",
                "01/02/2012",
                "02/07/2012",
                null);
        }

        [Test]
        public void TestEveryNthOfNthMonth()
        {
            // Test invalid values.
            try
            {
                new EveryNthOfNthMonth(0, NonExistentDatePolicy.Skip);
                Assert.Fail("EveryNthOfNthMonth(0,...) " +
                    "should fail with an ArgumentException");
            }
            catch (ArgumentException) { }

            try
            {
                new EveryNthOfNthMonth(-1, NonExistentDatePolicy.Skip);
                Assert.Fail("EveryNthOfNthMonth(-1,...) " +
                    "should fail with an ArgumentException");
            }
            catch (ArgumentException) { }

            EveryNthOfNthMonth trig;

            // Basic case - once a month
            trig = new EveryNthOfNthMonth(1, NonExistentDatePolicy.Skip);

            RunTest(trig, "01/01/2010",
                "01/01/2010",
                "01/02/2010",
                "01/03/2010",
                "01/04/2010",
                "01/05/2010",
                "01/06/2010",
                "01/07/2010",
                "01/08/2010",
                "01/09/2010",
                "01/10/2010",
                "01/11/2010",
                "01/12/2010",
                "01/01/2011");

            // Test that it stops at the end and that the end is inclusive
            trig.End = new DateTime(2010, 10, 1);
            RunTest(trig, "01/01/2010",
                "01/01/2010",
                "01/02/2010",
                "01/03/2010",
                "01/04/2010",
                "01/05/2010",
                "01/06/2010",
                "01/07/2010",
                "01/08/2010",
                "01/09/2010",
                "01/10/2010",
                null);

            // Test that end of 1s before an activation date doesn't trigger on that date.
            trig.End = new DateTime(2010, 9, 30, 23, 59, 59);
            RunTest(trig, "01/01/2010",
                "01/01/2010",
                "01/02/2010",
                "01/03/2010",
                "01/04/2010",
                "01/05/2010",
                "01/06/2010",
                "01/07/2010",
                "01/08/2010",
                "01/09/2010",
                null);

            // Test skipping dates which don't exist (End is still 30/09/2010)
            RunTest(trig, "31/01/2010",
                "31/01/2010",
                "31/03/2010",
                "31/05/2010",
                "31/07/2010",
                "31/08/2010",
                null);

            // Test last supported day policy
            trig = new EveryNthOfNthMonth(1,
                NonExistentDatePolicy.LastSupportedDayInMonth);

            // Last day falls on existing day - 31/12/2010
            trig.End = new DateTime(2010, 12, 31);
            RunTest(trig, "31/01/2010",
                "31/01/2010",
                "28/02/2010",
                "31/03/2010",
                "30/04/2010",
                "31/05/2010",
                "30/06/2010",
                "31/07/2010",
                "31/08/2010",
                "30/09/2010",
                "31/10/2010",
                "30/11/2010",
                "31/12/2010",
                null);

            // Last day falls on fallback day - 30/11/2010
            trig.End = new DateTime(2010, 11, 30);
            RunTest(trig, "31/01/2010",
                "31/01/2010",
                "28/02/2010",
                "31/03/2010",
                "30/04/2010",
                "31/05/2010",
                "30/06/2010",
                "31/07/2010",
                "31/08/2010",
                "30/09/2010",
                "31/10/2010",
                "30/11/2010",
                null);

            // Test in leap year
            trig.End = new DateTime(2012, 04, 30);
            RunTest(trig, "31/01/2011",
                "31/01/2011",
                "28/02/2011",
                "31/03/2011",
                "30/04/2011",
                "31/05/2011",
                "30/06/2011",
                "31/07/2011",
                "31/08/2011",
                "30/09/2011",
                "31/10/2011",
                "30/11/2011",
                "31/12/2011",
                "31/01/2012",
                "29/02/2012",
                "31/03/2012",
                "30/04/2012",
                null);

            // Test first supported day
            trig = new EveryNthOfNthMonth(1,
             NonExistentDatePolicy.FirstSupportedDayInNextMonth);

            // Last day falls on existing day - 31/12/2010
            trig.End = new DateTime(2010, 12, 31);
            RunTest(trig, "31/01/2010",
                "31/01/2010",
                "01/03/2010",
                "31/03/2010",
                "01/05/2010",
                "31/05/2010",
                "01/07/2010",
                "31/07/2010",
                "31/08/2010",
                "01/10/2010",
                "31/10/2010",
                "01/12/2010",
                "31/12/2010",
                null);

            // Last day falls on next supporting day - 01/12/2010
            trig.End = new DateTime(2010, 12, 01);
            RunTest(trig, "31/01/2010",
                "31/01/2010",
                "01/03/2010",
                "31/03/2010",
                "01/05/2010",
                "31/05/2010",
                "01/07/2010",
                "31/07/2010",
                "31/08/2010",
                "01/10/2010",
                "31/10/2010",
                "01/12/2010",
                null);

            // Last day falls on last day of month with missing day - 30/11/2010
            trig.End = new DateTime(2010, 11, 30);
            RunTest(trig, "31/01/2010",
                "31/01/2010",
                "01/03/2010",
                "31/03/2010",
                "01/05/2010",
                "31/05/2010",
                "01/07/2010",
                "31/07/2010",
                "31/08/2010",
                "01/10/2010",
                "31/10/2010",
                null);

            // Reset the end and test the leap year
            trig.End = DateTime.MaxValue;
            RunTest(trig, "29/01/2012",
                "29/01/2012", "29/02/2012", "29/03/2012");
        }

        [Test]
        public void TestEveryNWeeks()
        {
            // Test invalid values.
            try
            {
                new EveryNWeeks(0);
                Assert.Fail("EveryNWeeks(0) should fail with an ArgumentException");
            }
            catch (ArgumentException) { }

            try
            {
                new EveryNWeeks(-1);
                Assert.Fail("EveryNWeeks(-1) should fail with an ArgumentException");
            }
            catch (ArgumentException) { }

            EveryNWeeks trig = new EveryNWeeks(3);

            // This is a basic period-based trigger, test the standard case.
            RunTest(trig, new DateTime(2010, 7, 5, 9, 0, 0),
                new DateTime(2010, 7, 5, 9, 0, 0),
                new DateTime(2010, 7, 26, 9, 0, 0),
                new DateTime(2010, 8, 16, 9, 0, 0),
                new DateTime(2010, 9, 6, 9, 0, 0),
                new DateTime(2010, 9, 27, 9, 0, 0)
            );

            // Test that it stops at end, and that end is inclusive
            trig.End = new DateTime(2010, 8, 16, 9, 0, 0);
            RunTest(trig, new DateTime(2010, 7, 5, 9, 0, 0),
                new DateTime(2010, 7, 5, 9, 0, 0),
                new DateTime(2010, 7, 26, 9, 0, 0),
                new DateTime(2010, 8, 16, 9, 0, 0),
                DateTime.MinValue
            );

            // Test that end of 1s before an activation date doesn't trigger on that date.
            trig.End = new DateTime(2010, 8, 16, 8, 59, 59);
            RunTest(trig, new DateTime(2010, 7, 5, 9, 0, 0),
                new DateTime(2010, 7, 5, 9, 0, 0),
                new DateTime(2010, 7, 26, 9, 0, 0),
                DateTime.MinValue
            );
        }

        [Test]
        public void TestEveryNWeeksOnNthDayInIdentifiedCalendar()
        {
            DummyStore store = new DummyStore();
            ScheduleCalendar cal = new ScheduleCalendar(store.GetSchema());
            cal.WorkingWeek = DaySet.FiveDayWeek;
            cal.PublicHolidayGroup = DummyStore.Holiday.EnglandAndWales;
            store.SaveCalendar(cal);

            // We need a scheduler & schedule now to bind all this together
            // and allow the trigger access to the store (to retrieve the calendar)
            InertScheduler scheduler = new InertScheduler(store);
            DummySchedule sched = new DummySchedule(scheduler);

            try
            {
                new EveryNWeeksOnNthDayInIdentifiedCalendar(0, cal.Id);
                Assert.Fail("EveryNWeeksOnNthDayInIdentifiedCalendar(0, <id>) " +
                    "should fail with an ArgumentException");
            }
            catch (ArgumentException) { }

            try
            {
                new EveryNWeeksOnNthDayInIdentifiedCalendar(-1, cal.Id);
                Assert.Fail("EveryNWeeksOnNthDayInIdentifiedCalendar(-1, <id>) " +
                    "should fail with an ArgumentException");
            }
            catch (ArgumentException) { }

           try
            {
                new EveryNWeeksOnNthDayInIdentifiedCalendar(0, NthOfWeek.Fourth, cal.Id);
                Assert.Fail("EveryNWeeksOnNthDayInIdentifiedCalendar(0, NthOfMonth.Fourth, <id>) " +
                            "should fail with an ArgumentException");
            }
            catch (ArgumentException) { }

            try
            {
                new EveryNWeeksOnNthDayInIdentifiedCalendar(-1, NthOfWeek.Fourth, cal.Id);
                Assert.Fail("EveryNWeeksOnNthDayInIdentifiedCalendar(-1, NthOfMonth.Fourth, <id>) " +
                            "should fail with an ArgumentException");
            }
            catch (ArgumentException) { }

            BaseTrigger trig = new EveryNWeeksOnNthDayInIdentifiedCalendar(1, NthOfWeek.First,  cal.Id);
            // add to sched so trigger can get calendar from schedule => scheduler => store.
            sched.SetTrigger(trig);

            // Check expected outcome for first day of each week
            RunTest(trig, new DateTime(2010, 2, 1, 1, 0, 0),
                new DateTime(2010, 2, 1, 1, 0, 0),
                new DateTime(2010, 2, 8, 1, 0, 0),
                new DateTime(2010, 2, 15, 1, 0, 0),
                new DateTime(2010, 2, 22, 1, 0, 0)
            );

            // Check skips weekend
            RunTest(trig, new DateTime(2010, 1, 2, 9, 0, 0),
                new DateTime(2010, 1, 4, 9, 0, 0),
                new DateTime(2010, 1, 11, 9, 0, 0),
                new DateTime(2010, 1, 18, 9, 0, 0)
            );

           // Check skips weekend
            RunTest(trig, new DateTime(2010, 1, 2, 1, 0, 0),
                new DateTime(2010, 1, 4, 1, 0, 0),
                new DateTime(2010, 1, 11, 1, 0, 0),
                new DateTime(2010, 1, 18, 1, 0, 0)
            );            
            
            // Check skips bank holiday
            RunTest(trig, new DateTime(2010, 8, 30, 10, 0, 0),
                new DateTime(2010, 8, 31, 10, 0, 0),
                new DateTime(2010, 9, 6, 10, 0, 0),
                new DateTime(2010, 9, 13, 10, 0, 0)
            );

            // Finally, check that the end date is inclusive and works as expected.
            trig.End = new DateTime(2010, 2, 20, 1, 0, 0);
            RunTest(trig, new DateTime(2010, 2, 1, 1, 0, 0),
                new DateTime(2010, 2, 1, 1, 0, 0),
                new DateTime(2010, 2, 8, 1, 0, 0),
                new DateTime(2010, 2, 15, 1, 0, 0),
                DateTime.MinValue
            );

            store.DeleteCalendar(cal);
        }

        [Test]
        public void TestEveryNWeeksOnNthDayInKnownCalendar()
        {
            DummyStore store = new DummyStore();
            ScheduleCalendar cal = new ScheduleCalendar(store.GetSchema());
            cal.WorkingWeek = DaySet.FiveDayWeek;
            cal.PublicHolidayGroup = DummyStore.Holiday.EnglandAndWales;
            store.SaveCalendar(cal);

            // We need a scheduler & schedule now to bind all this together
            // and allow the trigger access to the store (to retrieve the calendar)
            InertScheduler scheduler = new InertScheduler(store);
            DummySchedule sched = new DummySchedule(scheduler);

            try
            {
                new EveryNWeeksOnNthDayInKnownCalendar(0, cal);
                Assert.Fail("EveryNWeeksOnNthDayInKnownCalendar(0, <id>) " +
                    "should fail with an ArgumentException");
            }
            catch (ArgumentException) { }

            try
            {
                new EveryNWeeksOnNthDayInKnownCalendar(-1, cal);
                Assert.Fail("EveryNWeeksOnNthDayInKnownCalendar(-1, <id>) " +
                    "should fail with an ArgumentException");
            }
            catch (ArgumentException) { }

            try
            {
                new EveryNWeeksOnNthDayInKnownCalendar(0, NthOfWeek.Fourth, cal);
                Assert.Fail("EveryNWeeksOnNthDayInKnownCalendar(0, NthOfMonth.Fourth, <id>) " +
                            "should fail with an ArgumentException");
            }
            catch (ArgumentException) { }

            try
            {
                new EveryNWeeksOnNthDayInKnownCalendar(-1, NthOfWeek.Fourth, cal);
                Assert.Fail("EveryNWeeksOnNthDayInKnownCalendar(-1, NthOfMonth.Fourth, <id>) " +
                            "should fail with an ArgumentException");
            }
            catch (ArgumentException) { }

            BaseTrigger trig = new EveryNWeeksOnNthDayInKnownCalendar(1, NthOfWeek.First, cal);
            // add to sched so trigger can get calendar from schedule => scheduler => store.
            sched.SetTrigger(trig);

            // Check expected outcome for first day of each week
            RunTest(trig, new DateTime(2010, 2, 1, 1, 0, 0),
                new DateTime(2010, 2, 1, 1, 0, 0),
                new DateTime(2010, 2, 8, 1, 0, 0),
                new DateTime(2010, 2, 15, 1, 0, 0),
                new DateTime(2010, 2, 22, 1, 0, 0)
            );

            // Check skips weekend
            RunTest(trig, new DateTime(2010, 1, 2, 9, 0, 0),
                new DateTime(2010, 1, 4, 9, 0, 0),
                new DateTime(2010, 1, 11, 9, 0, 0),
                new DateTime(2010, 1, 18, 9, 0, 0)
            );

            // Check skips weekend
            RunTest(trig, new DateTime(2010, 1, 2, 1, 0, 0),
                new DateTime(2010, 1, 4, 1, 0, 0),
                new DateTime(2010, 1, 11, 1, 0, 0),
                new DateTime(2010, 1, 18, 1, 0, 0)
            );

            // Check skips bank holiday
            RunTest(trig, new DateTime(2010, 8, 30, 10, 0, 0),
                new DateTime(2010, 8, 31, 10, 0, 0),
                new DateTime(2010, 9, 6, 10, 0, 0),
                new DateTime(2010, 9, 13, 10, 0, 0)
            );

            // Finally, check that the end date is inclusive and works as expected.
            trig.End = new DateTime(2010, 2, 20, 1, 0, 0);
            RunTest(trig, new DateTime(2010, 2, 1, 1, 0, 0),
                new DateTime(2010, 2, 1, 1, 0, 0),
                new DateTime(2010, 2, 8, 1, 0, 0),
                new DateTime(2010, 2, 15, 1, 0, 0),
                DateTime.MinValue
            );

            store.DeleteCalendar(cal);
        }

        /// <summary>
        /// Tests the basic EveryNYears trigger
        /// </summary>
        [Test]
        public void TestEveryNYears()
        {
            // By default, it will skip days which don't exist in subsequent years
            // Use 2092 => 2112 to ensure that it *doesn't* activate in 2100, which
            // is not a leap year despite being divisible by 4
            RunTest(new EveryNYears(2), new DateTime(2092, 2, 29, 9, 30, 0),
                new DateTime(2092, 2, 29, 9, 30, 0),
                new DateTime(2096, 2, 29, 9, 30, 0),
                new DateTime(2104, 2, 29, 9, 30, 0),
                new DateTime(2108, 2, 29, 9, 30, 0),
                new DateTime(2112, 2, 29, 9, 30, 0)
            );

            // By the same token, ensure that 2000 *is* activated correctly.
            RunTest(new EveryNYears(2), new DateTime(1996, 2, 29, 9, 30, 0),
                new DateTime(1996, 2, 29, 9, 30, 0),
                new DateTime(2000, 2, 29, 9, 30, 0),
                new DateTime(2004, 2, 29, 9, 30, 0)
            );

            // Check first supported day works as expected
            RunTest(new EveryNYears(2, NonExistentDatePolicy.FirstSupportedDayInNextMonth),
                new DateTime(1996, 2, 29, 9, 30, 0),
                new DateTime(1996, 2, 29, 9, 30, 0),
                new DateTime(1998, 3, 1, 9, 30, 0),
                new DateTime(2000, 2, 29, 9, 30, 0),
                new DateTime(2002, 3, 1, 9, 30, 0),
                new DateTime(2004, 2, 29, 9, 30, 0),
                new DateTime(2006, 3, 1, 9, 30, 0)
            );

            // Check last supported day works as expected
            RunTest(new EveryNYears(2, NonExistentDatePolicy.LastSupportedDayInMonth),
                new DateTime(1996, 2, 29, 9, 30, 0),
                new DateTime(1996, 2, 29, 9, 30, 0),
                new DateTime(1998, 2, 28, 9, 30, 0),
                new DateTime(2000, 2, 29, 9, 30, 0),
                new DateTime(2002, 2, 28, 9, 30, 0),
                new DateTime(2004, 2, 29, 9, 30, 0),
                new DateTime(2006, 2, 28, 9, 30, 0)
            );

            // 'Skip' is the default if you don't specify, but, for completion, let's
            // check when you explicitly specify to skip nonexistent dates.
            RunTest(new EveryNYears(2, NonExistentDatePolicy.Skip),
                new DateTime(1996, 2, 29, 9, 30, 0),
                new DateTime(1996, 2, 29, 9, 30, 0),
                new DateTime(2000, 2, 29, 9, 30, 0),
                new DateTime(2004, 2, 29, 9, 30, 0)
            );

        }

        /// <summary>
        /// Tests that the never trigger never fires... 
        /// </summary>
        [Test]
        public void TestNeverTrigger()
        {
            ITrigger trig = new NeverTrigger();
            Assert.That(trig.GetNextInstance(DateTime.MinValue), Is.Null);
        }

        /// <summary>
        /// Tests that the once trigger activates once on the date it is initialised
        /// with and never again.
        /// </summary>
        [Test]
        public void TestOnceTrigger()
        {
            DateTime triggerDate = new DateTime(2010, 10, 10, 9, 15, 0);
            ITrigger trig = new OnceTrigger(triggerDate);

            Assert.That(
                trig.GetNextInstance(DateTime.MinValue).When, Is.EqualTo(triggerDate));

            Assert.That(trig.GetNextInstance(triggerDate), Is.Null);
        }

        
  

    }
}

#endif
