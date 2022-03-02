#if UNITTESTS

using BluePrism.AutomateAppCore;
using BluePrism.BPCoreLib;
using NUnit.Framework;
using System;

namespace AutomateAppCore.UnitTests.WorkQueues
{
    /// <summary>
    /// A set of tests for the work queue filter management classes - primarily to test
    /// the parsing in the <see cref="clsWorkQueueFilterBuilder"/> class.
    /// 
    /// There are several behavioral helper methods, and the testing is then implemented
    /// column at a time to test the specific conditions for each
    /// 
    /// Init() and Destroy() are called by the helper methods (rather than with the SetUp
    /// and TearDown attributes) so that the filter's properties are re-set with each new
    /// test case and thus the tests are not dependent on the order in which they run.
    /// </summary>
    [TestFixture]
    public class WorkQueueFilterTests
    {
        #region "Helper Methods"
        /// <summary>
        /// Helper method to run a test on a date in the current filter builder.
        /// </summary>
        /// <param name="baseDate">The base date to use in the filter builder.</param>
        /// <param name="colName">The "column name" to apply the filter to - this
        /// corresponds to the columns in the work queues UI in Control Room.</param>
        /// <param name="filterText">The text to use as the filter.</param>
        /// <param name="propName">The name of the property in the filter being built by
        /// the filter builder to test</param>
        /// <param name="expectedDate">The date that is expected to be set in the filter
        /// after the text constraint has been applied</param>
        private static void TestFilterDate_ReturnsTrueAndSetsCorrectDate(DateTime baseDate, string colName, string filterText, string propName, DateTime expectedDate)
        {
            var builder = new clsWorkQueueFilterBuilder();
            Assert.That(builder.ApplyConstraint(baseDate, colName, ref filterText), Is.True);
            var f = builder.Filter;
            var t = f.GetType();
            var prop = t.GetProperty(propName);
            var dt = (DateTime)prop.GetValue(f, null);
            Assert.That(dt, Is.EqualTo(expectedDate));
        }

        private static void TestFilterDate_ReturnsFalseAndDoesNotChangeDate(DateTime baseDate, string colName, string filterText, string propName)
        {
            var builder = new clsWorkQueueFilterBuilder();
            var f = builder.Filter;
            var t = f.GetType();
            var prop = t.GetProperty(propName);
            // Save the initial value of the filter so we can check it doesn't change
            var dtInitial = (DateTime)prop.GetValue(f, null);
            Assert.That(builder.ApplyConstraint(baseDate, colName, ref filterText), Is.False);
            var dt = (DateTime)prop.GetValue(f, null);
            // Ensure that the value doesn't change from before if it fails to read a
            // new filter value.
            Assert.That(dt, Is.EqualTo(dtInitial));
        }


        /// <summary>
        /// Helper method to run a test on a date in the current filter builder.
        /// </summary>
        /// <param name="baseDate">The base date to use in the filter builder.</param>
        /// <param name="colName">The "column name" to apply the filter to - this
        /// corresponds to the columns in the work queues UI in Control Room.</param>
        /// <param name="filterText">The text to use as the filter.</param>
        /// <param name="propName">The name of the property in the filter being built by
        /// the filter builder to test</param>
        private static void TestFilterDate_ReturnsTrueButDoesNotChangeDate(DateTime baseDate, string colName, string filterText, string propName)
        {
            var builder = new clsWorkQueueFilterBuilder();
            var f = builder.Filter;
            var t = f.GetType();
            var prop = t.GetProperty(propName);
            // Save the initial value of the filter so we can check it doesn't change
            var dtInitial = (DateTime)prop.GetValue(f, null);
            Assert.That(builder.ApplyConstraint(baseDate, colName, ref filterText), Is.True);
            var dt = (DateTime)prop.GetValue(f, null);
            // Ensure that the value doesn't change from before 
            Assert.That(dt, Is.EqualTo(dtInitial));
        }

        /// <summary>
        /// Helper method to run a test on a date in the current filter builder.
        /// Ensures that a value of true is returned and that the filter start
        /// and end dates are as expected
        /// </summary>
        /// <param name="baseDate">The base date to use in the filter builder.</param>
        /// <param name="colName">The "column name" to apply the filter to - this
        /// corresponds to the columns in the work queues UI in Control Room.</param>
        /// <param name="filterText">The text to use as the filter.</param>
        /// <param name="propStartDateName">The name of the property corresponding to
        /// the start date of the filter being built</param>
        /// <param name="propEndDateName">The name of the property corresponding to
        /// the end date of the filter being built</param>
        /// <param name="expectedStartDate">The date that is expected to be set as the
        /// start date in the filter after the text constraint has been applied</param>
        /// <param name="expectedEndDate">The date that is expected to be set as the
        /// end date in the filter after the text constraint has been applied</param>
        /// <remarks></remarks>
        private static void TestFilterDate_ReturnsTrueAndSetsCorrectStartAndEndDate(DateTime baseDate, string colName, string filterText, string propStartDateName, string propEndDateName, DateTime expectedStartDate, DateTime expectedEndDate)
        {
            var builder = new clsWorkQueueFilterBuilder();
            Assert.That(builder.ApplyConstraint(baseDate, colName, ref filterText), Is.True);
            var f = builder.Filter;
            // Check the start date
            var t = f.GetType();
            var propStart = t.GetProperty(propStartDateName);
            var dtStart = (DateTime)propStart.GetValue(f, null);
            Assert.That(dtStart, Is.EqualTo(expectedStartDate));
            // check the end date
            var propEnd = t.GetProperty(propEndDateName);
            var dtEnd = (DateTime)propEnd.GetValue(f, null);
            Assert.That(dtEnd, Is.EqualTo(expectedEndDate));
        }

        /// <summary>
        /// Helper method to test that the filter builder returns False when a timespan
        /// is entered in incorrect format as a filter for the WorkTime column
        /// </summary>
        /// <param name="filterText">The filter string entered by the user</param>
        private static void TestWorkTimeFilter_ReturnsFalse(string filterText)
        {
            var builder = new clsWorkQueueFilterBuilder();
            Assert.That(builder.ApplyConstraint("Total Work Time", ref filterText), Is.False);
            var f = builder.Filter;
            Assert.That(f.MinWorkTime, Is.EqualTo(int.MinValue));
            Assert.That(f.MaxWorkTime, Is.EqualTo(int.MaxValue));
        }

        /// <summary>
        /// Helper method to test that the filter builder returns true, and correctly
        /// sets the max and min timespans when filtering the WorkTime column
        /// </summary>
        /// <param name="filterText">The filter string entered by the user.</param>
        /// <param name="expectedMinWorkTimeInSeconds">Expected minimum value</param>
        /// <param name="expectedMaxWorkTimeInSeconds">Expected maximum value</param>
        /// <param name="expectedUpdatedFilterText">The expected final value of the
        /// filter string displayed back to the user after any rounding has been done,
        /// in cases where the user enters a non-integer value</param>
        private static void TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime(string filterText, int expectedMinWorkTimeInSeconds, int expectedMaxWorkTimeInSeconds, string expectedUpdatedFilterText)
        {
            var builder = new clsWorkQueueFilterBuilder();
            Assert.That(builder.ApplyConstraint("Total Work Time", ref filterText), Is.True);
            var f = builder.Filter;
            // Check the Min workTime
            Assert.That(f.MinWorkTime, Is.EqualTo(expectedMinWorkTimeInSeconds));
            // Check max workTime
            Assert.That(f.MaxWorkTime, Is.EqualTo(expectedMaxWorkTimeInSeconds));
            // Check that the text to be displayed in the UI has been correctly updated 
            Assert.That(filterText, Is.EqualTo(expectedUpdatedFilterText));
        }
        #endregion

        /// <summary>
        /// Tests that whitespace in the filter text doesn't fluff up the user's intent
        /// </summary>
        [Test]
        public void TestWhitespace()
        {
            // Ensure that whitespace doesn't throw it
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", " yesterday", "LoadedStartDate", new DateTime(2016, 10, 31, 0, 0, 0));
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", "today ", "LoadedStartDate", new DateTime(2016, 11, 1, 0, 0, 0));
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", "last   1      hours", "LoadedStartDate", new DateTime(2016, 11, 1, 13, 36, 17));
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", "  last 12  hours ", "LoadedStartDate", new DateTime(2016, 11, 1, 2, 36, 17));
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", "  >   2016-10-05     ", "LoadedStartDate", new DateTime(2016, 10, 5).ToUniversalTime());
            TestFilterDate_ReturnsTrueAndSetsCorrectStartAndEndDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", " >  2016-10-05 ,  <   2016-10-20   ", "LoadedStartDate", "LoadedEndDate", new DateTime(2016, 10, 5).ToUniversalTime(), new DateTime(2016, 10, 20).ToUniversalTime());
        }

        /// <summary>
        /// Test that the date is interpreted correctly in locales other than en-GB
        /// </summary>
        [Test]
        public void TestDifferentCultures()
        {

            // USA! USA!
            using (new CultureBlock("en-US"))
            {
                TestFilterDate_ReturnsTrueAndSetsCorrectStartAndEndDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", "05/10/2016", "LoadedStartDate", "LoadedEndDate", new DateTime(2016, 5, 10).ToUniversalTime(), new DateTime(2016, 5, 11).ToUniversalTime());
                TestFilterDate_ReturnsTrueAndSetsCorrectStartAndEndDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", "10/05/2016", "LoadedStartDate", "LoadedEndDate", new DateTime(2016, 10, 5).ToUniversalTime(), new DateTime(2016, 10, 6).ToUniversalTime());
                TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", ">= 10/05/2016", "LoadedStartDate", new DateTime(2016, 10, 5).ToUniversalTime());
                TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", ">= 2016-10-05", "LoadedStartDate", new DateTime(2016, 10, 5).ToUniversalTime());
            }

            // Germanical
            using (new CultureBlock("de-DE"))
            {
                TestFilterDate_ReturnsTrueAndSetsCorrectStartAndEndDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", "05.10.2016", "LoadedStartDate", "LoadedEndDate", new DateTime(2016, 10, 5).ToUniversalTime(), new DateTime(2016, 10, 6).ToUniversalTime());
                TestFilterDate_ReturnsTrueAndSetsCorrectStartAndEndDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", "10/05/2016", "LoadedStartDate", "LoadedEndDate", new DateTime(2016, 5, 10).ToUniversalTime(), new DateTime(2016, 5, 11).ToUniversalTime());
                TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", ">= 10/05/2016", "LoadedStartDate", new DateTime(2016, 5, 10).ToUniversalTime());
                TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", ">= 2016-10-05", "LoadedStartDate", new DateTime(2016, 10, 5).ToUniversalTime());
            }
        }
        /// <summary>Test that dates which can be parsed but are not true dates
        /// do not return true or change the date
        /// </summary>
        [Test]
        public void TestNonValidDates()
        {
            TestFilterDate_ReturnsFalseAndDoesNotChangeDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", "<31/02/2016", "LoadedStartDate");
            TestFilterDate_ReturnsFalseAndDoesNotChangeDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", "<31/02/2016, >01/01/2013", "LoadedStartDate");
            // It may try and parse a comma on its own, but should not be validated
            TestFilterDate_ReturnsFalseAndDoesNotChangeDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", " , ", "LoadedStartDate");
            TestFilterDate_ReturnsFalseAndDoesNotChangeDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", ",", "LoadedStartDate");
            TestFilterDate_ReturnsFalseAndDoesNotChangeDate(new DateTime(2016, 11, 1, 14, 36, 17), "Next Review", "<31/02/2017", "NextReviewEndDate");
            TestFilterDate_ReturnsFalseAndDoesNotChangeDate(new DateTime(2016, 11, 1, 14, 36, 17), "Next Review", "<31/02/2017, >01/02/2017", "NextReviewStartDate");
            TestFilterDate_ReturnsFalseAndDoesNotChangeDate(new DateTime(2016, 11, 1, 14, 36, 17), "Next Review", " , ", "NextReviewStartDate");
            TestFilterDate_ReturnsFalseAndDoesNotChangeDate(new DateTime(2016, 11, 1, 14, 36, 17), "Next Review", ",", "NextReviewStartDate");
        }

#region "ColumnTests"

        /// <summary>
        /// A set of tests for the 'Loaded' (start) date ("Created" column)
        /// </summary>
        [Test]
        public void TestLoadedFilter()
        {
            // "Loaded" is all past tense, so it won't really work with any 
            // "next n days" type filters or "tomorrow"

            // Appropriate Keywords (Today, Tomorrow, Yesterday)
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", "yesterday", "LoadedStartDate", new DateTime(2016, 10, 31, 0, 0, 0));
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", "today", "LoadedStartDate", new DateTime(2016, 11, 1, 0, 0, 0));
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", "YESTERDAY", "LoadedStartDate", new DateTime(2016, 10, 31, 0, 0, 0));
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", "toDay", "LoadedStartDate", new DateTime(2016, 11, 1, 0, 0, 0));

            // "friendly" date ranges (last / next n days/hours/minutes)
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", "Last 5 Days", "LoadedStartDate", new DateTime(2016, 10, 27, 0, 0, 0));
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", "last 5 mins", "LoadedStartDate", new DateTime(2016, 11, 1, 14, 31, 17));
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", "last 5d", "LoadedStartDate", new DateTime(2016, 10, 27, 0, 0, 0));
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", "last 1 hour", "LoadedStartDate", new DateTime(2016, 11, 1, 13, 36, 17));
            // introduce some extra spaces to test they are stripped

            // Specific date ranges (< date, > date)
            // Note that the specific tests here require a conversion to UTC...
            // ">=" and ">" do the same thing... which is weird, but there you go...
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", ">= 2016-10-05", "LoadedStartDate", new DateTime(2016, 10, 5).ToUniversalTime());
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", "> 2016-10-05", "LoadedStartDate", new DateTime(2016, 10, 5).ToUniversalTime());
            TestFilterDate_ReturnsTrueAndSetsCorrectStartAndEndDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", "> 2016-10-05, < 2016-10-20", "LoadedStartDate", "LoadedEndDate", new DateTime(2016, 10, 5).ToUniversalTime(), new DateTime(2016, 10, 20).ToUniversalTime());

            // Test for a single day ((DD/MM/YYYY in UK) or the universal date 
            // format (YYYY-MM-DD))
            TestFilterDate_ReturnsTrueAndSetsCorrectStartAndEndDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", "2016-10-05", "LoadedStartDate", "LoadedEndDate", new DateTime(2016, 10, 5).ToUniversalTime(), new DateTime(2016, 10, 6).ToUniversalTime());
            TestFilterDate_ReturnsTrueAndSetsCorrectStartAndEndDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", "05/10/2016", "LoadedStartDate", "LoadedEndDate", new DateTime(2016, 10, 5).ToUniversalTime(), new DateTime(2016, 10, 6).ToUniversalTime());
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", ">= 05/10/2016", "LoadedStartDate", new DateTime(2016, 10, 5).ToUniversalTime());

            // Now test failure conditions
            TestFilterDate_ReturnsFalseAndDoesNotChangeDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", "this is nonsense", "LoadedStartDate");
            TestFilterDate_ReturnsFalseAndDoesNotChangeDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", "last 6 wiuhwui", "LoadedStartDate");
            TestFilterDate_ReturnsFalseAndDoesNotChangeDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", "fedjio 6 wiuhwui", "LoadedStartDate");

            // An empty string "works" but does nothing to the underlying filter value.
            TestFilterDate_ReturnsTrueButDoesNotChangeDate(new DateTime(2016, 11, 1, 14, 36, 17), "Created", "", "LoadedStartDate");

            // "Tomorrow" just shouldn't work at all
            TestFilterDate_ReturnsFalseAndDoesNotChangeDate(new DateTime(2016, 11, 7, 13, 9, 0), "Created", "tomorrow", "LoadedStartDate");

            // "All" doesn't really work as expected - it just has no effect, so the date
            // remains at what it was previously set as... this works in current code
            // because the object is created anew each time the filter is applied in the
            // interface, but it probably should be a bit more predictable in the back-end
            // code.
            // TestFilterDate(New Date(2016, 11, 1, 14, 36, 17), "Created", "All",
            // "LoadedStartDate", Date.MinValue)
            // this actually will return the MinSQLValue from clsServer

        }

        /// <summary>
        /// A set of tests for the 'Last Updated' Column
        /// </summary>
        [Test]
        public void TestLastUpdatedFilter()
        {
            // "Last Updated" is all past tense, so it won't really work with any 
            // "next n days" type filters or "tomorrow"

            // Appropriate Keywords (Today, Tomorrow, Yesterday)
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Last Updated", "yesterday", "LastUpdatedStartDate", new DateTime(2016, 10, 31, 0, 0, 0));
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Last Updated", "today", "LastUpdatedStartDate", new DateTime(2016, 11, 1, 0, 0, 0));

            // "friendly" date ranges (last / next n days/hours/minutes)
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Last Updated", "Last 5 Days", "LastUpdatedStartDate", new DateTime(2016, 10, 27, 0, 0, 0));
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Last Updated", "last 5 mins", "LastUpdatedStartDate", new DateTime(2016, 11, 1, 14, 31, 17));
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Last Updated", "last 5d", "LastUpdatedStartDate", new DateTime(2016, 10, 27, 0, 0, 0));
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Last Updated", "last 1 hour", "LastUpdatedStartDate", new DateTime(2016, 11, 1, 13, 36, 17));
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Last Updated", "last    1     hours", "LastUpdatedStartDate", new DateTime(2016, 11, 1, 13, 36, 17));

            // Specific date ranges (< date, > date)
            // Note that the specific tests here require a conversion to UTC...
            // ">=" and ">" do the same thing... which is weird, but there you go...
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Last Updated", ">= 2016-10-05", "LastUpdatedStartDate", new DateTime(2016, 10, 5).ToUniversalTime());
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Last Updated", "> 2016-10-05", "LastUpdatedStartDate", new DateTime(2016, 10, 5).ToUniversalTime());
            TestFilterDate_ReturnsTrueAndSetsCorrectStartAndEndDate(new DateTime(2016, 11, 1, 14, 36, 17), "Last Updated", "> 2016-10-05, < 2016-10-20", "LastUpdatedStartDate", "LastUpdatedEndDate", new DateTime(2016, 10, 5).ToUniversalTime(), new DateTime(2016, 10, 20).ToUniversalTime());

            // Test for a single day ((DD/MM/YYYY in UK) or the universal date 
            // format (YYYY-MM-DD))
            TestFilterDate_ReturnsTrueAndSetsCorrectStartAndEndDate(new DateTime(2016, 11, 1, 14, 36, 17), "Last Updated", "2016-10-05", "LastUpdatedStartDate", "LastUpdatedEndDate", new DateTime(2016, 10, 5).ToUniversalTime(), new DateTime(2016, 10, 6).ToUniversalTime());
            TestFilterDate_ReturnsTrueAndSetsCorrectStartAndEndDate(new DateTime(2016, 11, 1, 14, 36, 17), "Last Updated", "05/10/2016", "LastUpdatedStartDate", "LastUpdatedEndDate", new DateTime(2016, 10, 5).ToUniversalTime(), new DateTime(2016, 10, 6).ToUniversalTime());

            // Now test failure conditions
            TestFilterDate_ReturnsFalseAndDoesNotChangeDate(new DateTime(2016, 11, 1, 14, 36, 17), "Last Updated", "this is nonsense", "LastUpdatedStartDate");
            TestFilterDate_ReturnsFalseAndDoesNotChangeDate(new DateTime(2016, 11, 1, 14, 36, 17), "Last Updated", "last 7 kippers", "LastUpdatedStartDate");

            // An empty string "works" but does nothing to the underlying filter value.
            TestFilterDate_ReturnsTrueButDoesNotChangeDate(new DateTime(2016, 11, 1, 14, 36, 17), "Last Updated", "", "LastUpdatedStartDate");

            // "Tomorrow" just shouldn't work at all
            TestFilterDate_ReturnsFalseAndDoesNotChangeDate(new DateTime(2016, 11, 7, 13, 9, 0), "Last Updated", "tomorrow", "LastUpdatedStartDate");
        }

        /// <summary>
        /// A set of tests for the 'Next Review' column
        /// </summary>
        [Test]
        public void TestNextReviewFilter()
        {
            // "Next Review" is all future tense, so it won't really work with any 
            // "last n days" type filters or "yesterday"

            // Appropriate Keywords (Today, Tomorrow, Yesterday)
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Next Review", "tomorrow", "NextReviewEndDate", new DateTime(2016, 11, 3, 0, 0, 0));
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Next Review", "today", "NextReviewEndDate", new DateTime(2016, 11, 2, 0, 0, 0));

            // "friendly" date ranges (last / next n days/hours/minutes)
            TestFilterDate_ReturnsTrueAndSetsCorrectStartAndEndDate(new DateTime(2016, 11, 1, 14, 36, 17), "Next Review", "Next 5 Days", "NextReviewStartDate", "NextReviewEndDate", new DateTime(2016, 11, 1, 14, 36, 17), new DateTime(2016, 11, 6, 0, 0, 0));
            TestFilterDate_ReturnsTrueAndSetsCorrectStartAndEndDate(new DateTime(2016, 11, 1, 14, 36, 17), "Next Review", "next 5 mins", "NextReviewStartDate", "NextReviewEndDate", new DateTime(2016, 11, 1, 14, 36, 17), new DateTime(2016, 11, 1, 14, 41, 17));
            TestFilterDate_ReturnsTrueAndSetsCorrectStartAndEndDate(new DateTime(2016, 11, 1, 14, 36, 17), "Next Review", "next 5d", "NextReviewStartDate", "NextReviewEndDate", new DateTime(2016, 11, 1, 14, 36, 17), new DateTime(2016, 11, 6, 0, 0, 0));
            TestFilterDate_ReturnsTrueAndSetsCorrectStartAndEndDate(new DateTime(2016, 11, 1, 14, 36, 17), "Next Review", "next 1 hour", "NextReviewStartDate", "NextReviewEndDate", new DateTime(2016, 11, 1, 14, 36, 17), new DateTime(2016, 11, 1, 15, 36, 17));
            TestFilterDate_ReturnsTrueAndSetsCorrectStartAndEndDate(new DateTime(2016, 11, 1, 14, 36, 17), "Next Review", "next 1 hour", "NextReviewStartDate", "NextReviewEndDate", new DateTime(2016, 11, 1, 14, 36, 17), new DateTime(2016, 11, 1, 15, 36, 17));
            TestFilterDate_ReturnsTrueAndSetsCorrectStartAndEndDate(new DateTime(2016, 11, 1, 14, 36, 17), "Next Review", "next    1        hours", "NextReviewStartDate", "NextReviewEndDate", new DateTime(2016, 11, 1, 14, 36, 17), new DateTime(2016, 11, 1, 15, 36, 17));

            // Specific date ranges (< date, > date)
            // Note that the specific tests here require a conversion to UTC...
            // ">=" and ">" do the same thing... which is weird, but there you go...
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Next Review", ">= 2016-10-05", "NextReviewStartDate", new DateTime(2016, 10, 5).ToUniversalTime());
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Next Review", "> 2016-10-05", "NextReviewStartDate", new DateTime(2016, 10, 5).ToUniversalTime());
            TestFilterDate_ReturnsTrueAndSetsCorrectStartAndEndDate(new DateTime(2016, 11, 1, 14, 36, 17), "Next Review", "> 2016-10-05, < 2016-10-20", "NextReviewStartDate", "NextReviewEndDate", new DateTime(2016, 10, 5).ToUniversalTime(), new DateTime(2016, 10, 20).ToUniversalTime());

            // Test for a single day ((DD/MM/YYYY in UK) or the universal date 
            // format (YYYY-MM-DD))
            TestFilterDate_ReturnsTrueAndSetsCorrectStartAndEndDate(new DateTime(2016, 11, 1, 14, 36, 17), "Next Review", "2016-10-05", "NextReviewStartDate", "NextReviewEndDate", new DateTime(2016, 10, 5).ToUniversalTime(), new DateTime(2016, 10, 6).ToUniversalTime());
            TestFilterDate_ReturnsTrueAndSetsCorrectStartAndEndDate(new DateTime(2016, 11, 1, 14, 36, 17), "Next Review", "05/10/2016", "NextReviewStartDate", "NextReviewEndDate", new DateTime(2016, 10, 5).ToUniversalTime(), new DateTime(2016, 10, 6).ToUniversalTime());

            // Now test failure conditions
            TestFilterDate_ReturnsFalseAndDoesNotChangeDate(new DateTime(2016, 11, 1, 14, 36, 17), "Next Review", "this is nonsense", "NextReviewEndDate");
            TestFilterDate_ReturnsFalseAndDoesNotChangeDate(new DateTime(2016, 11, 1, 14, 36, 17), "Next Review", "next 9 kippers", "NextReviewEndDate");

            // An empty string "works" but does nothing to the underlying filter value.
            TestFilterDate_ReturnsTrueButDoesNotChangeDate(new DateTime(2016, 11, 1, 14, 36, 17), "Next Review", "", "NextReviewEndDate");

            // "Yesterday" just shouldn't work at all
            TestFilterDate_ReturnsFalseAndDoesNotChangeDate(new DateTime(2016, 11, 7, 13, 9, 0), "Next Review", "yesterday", "NextReviewEndDate");
        }

        /// <summary>
        /// A set of tests for the 'Completed' date
        /// </summary>
        [Test]
        public void TestCompletedFilter()
        {
            // "Completed" is all past tense, so it won't really work with any 
            // "next n days" type filters or "tomorrow"

            // Appropriate Keywords (Today, Tomorrow, Yesterday)
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Completed", "yesterday", "CompletedStartDate", new DateTime(2016, 10, 31, 0, 0, 0));
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Completed", "today", "CompletedStartDate", new DateTime(2016, 11, 1, 0, 0, 0));

            // "friendly" date ranges (last / next n days/hours/minutes)
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Completed", "Last   5    Days", "CompletedStartDate", new DateTime(2016, 10, 27, 0, 0, 0));
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Completed", "last 5 mins", "CompletedStartDate", new DateTime(2016, 11, 1, 14, 31, 17));
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Completed", "last 5d", "CompletedStartDate", new DateTime(2016, 10, 27, 0, 0, 0));
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Completed", "last 1 hour", "CompletedStartDate", new DateTime(2016, 11, 1, 13, 36, 17));
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Completed", "last 1 hours", "CompletedStartDate", new DateTime(2016, 11, 1, 13, 36, 17));

            // Specific date ranges (< date, > date)
            // Note that the specific tests here require a conversion to UTC...
            // ">=" and ">" do the same thing... which is weird, but there you go...
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Completed", ">= 2016-10-05", "CompletedStartDate", new DateTime(2016, 10, 5).ToUniversalTime());
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Completed", "> 2016-10-05", "CompletedStartDate", new DateTime(2016, 10, 5).ToUniversalTime());
            TestFilterDate_ReturnsTrueAndSetsCorrectStartAndEndDate(new DateTime(2016, 11, 1, 14, 36, 17), "Completed", "> 2016-10-05, < 2016-10-20", "CompletedStartDate", "CompletedEndDate", new DateTime(2016, 10, 5).ToUniversalTime(), new DateTime(2016, 10, 20).ToUniversalTime());

            // Test for a single day ((DD/MM/YYYY in UK) or the universal date 
            // format (YYYY-MM-DD))
            TestFilterDate_ReturnsTrueAndSetsCorrectStartAndEndDate(new DateTime(2016, 11, 1, 14, 36, 17), "Completed", "2016-10-05", "CompletedStartDate", "CompletedEndDate", new DateTime(2016, 10, 5).ToUniversalTime(), new DateTime(2016, 10, 6).ToUniversalTime());
            TestFilterDate_ReturnsTrueAndSetsCorrectStartAndEndDate(new DateTime(2016, 11, 1, 14, 36, 17), "Completed", "05/10/2016", "CompletedStartDate", "CompletedEndDate", new DateTime(2016, 10, 5).ToUniversalTime(), new DateTime(2016, 10, 6).ToUniversalTime());

            // Now test failure conditions
            TestFilterDate_ReturnsFalseAndDoesNotChangeDate(new DateTime(2016, 11, 1, 14, 36, 17), "Completed", "this is nonsense", "CompletedStartDate");
            TestFilterDate_ReturnsFalseAndDoesNotChangeDate(new DateTime(2016, 11, 1, 14, 36, 17), "Completed", "asfsa 6 hours", "CompletedStartDate");

            // An empty string "works" but does nothing to the underlying filter value.
            TestFilterDate_ReturnsTrueButDoesNotChangeDate(new DateTime(2016, 11, 1, 14, 36, 17), "Completed", "", "CompletedStartDate");

            // "Tomorrow" just shouldn't work at all
            TestFilterDate_ReturnsFalseAndDoesNotChangeDate(new DateTime(2016, 11, 7, 13, 9, 0), "Completed", "tomorrow", "CompletedStartDate");
        }

        /// <summary>
        /// A set of tests for the Exception Date Column Filter
        /// </summary>
        [Test]
        public void TestExceptionDateFilter()
        {
            // "Exception Date" is all past tense, so it won't really work with any 
            // "next n days" type filters or "tomorrow"

            // Appropriate Keywords (Today, Tomorrow, Yesterday)
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Exception Date", "yesterday", "ExceptionStartDate", new DateTime(2016, 10, 31, 0, 0, 0));
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Exception Date", "today", "ExceptionStartDate", new DateTime(2016, 11, 1, 0, 0, 0));

            // "friendly" date ranges (last / next n days/hours/minutes)
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Exception Date", "Last 5 Days", "ExceptionStartDate", new DateTime(2016, 10, 27, 0, 0, 0));
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Exception Date", "last 5 mins", "ExceptionStartDate", new DateTime(2016, 11, 1, 14, 31, 17));
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Exception Date", "last 5d", "ExceptionStartDate", new DateTime(2016, 10, 27, 0, 0, 0));
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Exception Date", "last 1 hour", "ExceptionStartDate", new DateTime(2016, 11, 1, 13, 36, 17));
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Exception Date", "last    1     hours", "ExceptionStartDate", new DateTime(2016, 11, 1, 13, 36, 17));

            // Specific date ranges (< date, > date)
            // Note that the specific tests here require a conversion to UTC...
            // ">=" and ">" do the same thing... which is weird, but there you go...
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Exception Date", ">= 2016-10-05", "ExceptionStartDate", new DateTime(2016, 10, 5).ToUniversalTime());
            TestFilterDate_ReturnsTrueAndSetsCorrectDate(new DateTime(2016, 11, 1, 14, 36, 17), "Exception Date", "> 2016-10-05", "ExceptionStartDate", new DateTime(2016, 10, 5).ToUniversalTime());
            TestFilterDate_ReturnsTrueAndSetsCorrectStartAndEndDate(new DateTime(2016, 11, 1, 14, 36, 17), "Exception Date", "> 2016-10-05, < 2016-10-20", "ExceptionStartDate", "ExceptionEndDate", new DateTime(2016, 10, 5).ToUniversalTime(), new DateTime(2016, 10, 20).ToUniversalTime());

            // Test for a single day ((DD/MM/YYYY in UK) or the universal date 
            // format (YYYY-MM-DD))
            TestFilterDate_ReturnsTrueAndSetsCorrectStartAndEndDate(new DateTime(2016, 11, 1, 14, 36, 17), "Exception Date", "2016-10-05", "ExceptionStartDate", "ExceptionEndDate", new DateTime(2016, 10, 5).ToUniversalTime(), new DateTime(2016, 10, 6).ToUniversalTime());
            TestFilterDate_ReturnsTrueAndSetsCorrectStartAndEndDate(new DateTime(2016, 11, 1, 14, 36, 17), "Exception Date", "05/10/2016", "ExceptionStartDate", "ExceptionEndDate", new DateTime(2016, 10, 5).ToUniversalTime(), new DateTime(2016, 10, 6).ToUniversalTime());

            // Now test failure conditions
            TestFilterDate_ReturnsFalseAndDoesNotChangeDate(new DateTime(2016, 11, 1, 14, 36, 17), "Exception Date", "this is nonsense", "ExceptionStartDate");
            TestFilterDate_ReturnsFalseAndDoesNotChangeDate(new DateTime(2016, 11, 1, 14, 36, 17), "Exception Date", "last  9 kjashk", "ExceptionStartDate");

            // An empty string "works" but does nothing to the underlying filter value.
            TestFilterDate_ReturnsTrueButDoesNotChangeDate(new DateTime(2016, 11, 1, 14, 36, 17), "Exception Date", "", "ExceptionStartDate");

            // "Tomorrow" just shouldn't work at all
            TestFilterDate_ReturnsFalseAndDoesNotChangeDate(new DateTime(2016, 11, 7, 13, 9, 0), "Exception Date", "tomorrow", "ExceptionStartDate");
        }

        /// <summary>
        /// A set of tests for the Work Time column. The test are pretty comprehensive
        /// because there are lots of different cases as the rounding rules are not
        /// straight forward
        /// </summary>
        /// <remarks></remarks>
        [Test]
        public void TestWorkTimeFilter()
        {
            const int twoMinutesInSeconds = 60 * 2;
            const int threeMinutesInSeconds = 60 * 3;
            const int oneHourInSeconds = 60 * 60;
            const int twoHoursInSeconds = 60 * 60 * 2;

            // Worktime is stored and displayed in whole seconds but for 
            // backwards compatibility we will still allow non-integer values 
            // but round them appropriately to the operator used in the filter
            // The following formats should be allowed
            // -   nn:nn (mins and secs)
            // -   nn:nn.nnn (mins : secs . millisecs) 
            // -   n   (secs)
            // -   n.n (secs.millisecs)
            // -   nn:nn:nn (hours : mins : secs)
            // -   nn:nn:nn.nnn (hours : mins : secs . millisecs)

            // Check that the user is informed if they enter an incorrect 
            // filter format
            TestWorkTimeFilter_ReturnsFalse("1.50.658"); // two decimal points
            TestWorkTimeFilter_ReturnsFalse("etwhethweoitu");
            TestWorkTimeFilter_ReturnsFalse("<=nonsense");
            TestWorkTimeFilter_ReturnsFalse("61"); // this is seconds so bounded by 60
            TestWorkTimeFilter_ReturnsFalse("61:00"); // this is mins so bounded by 60
            TestWorkTimeFilter_ReturnsFalse("25:00:00.000"); // hours bounded by 24
                                                             // cannot use equality in multi-part range
            TestWorkTimeFilter_ReturnsFalse("<6, >2, =7");
            TestWorkTimeFilter_ReturnsFalse("02:00, =04:00");
            TestWorkTimeFilter_ReturnsFalse("02:00, 4:00");
            // Test that correctly formatted inputs return true, filter 
            // the data correctly and return correct modified filter string
            // where rounding takes place

            // First with equality
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("02:00", twoMinutesInSeconds, twoMinutesInSeconds, "02:00");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("=02:00", twoMinutesInSeconds, twoMinutesInSeconds, "=02:00");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("02:00.000", twoMinutesInSeconds, twoMinutesInSeconds, "02:00");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("2", 2, 2, "00:02");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("2.0", 2, 2, "00:02");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("02:00:00", twoHoursInSeconds, twoHoursInSeconds, "02:00:00");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("01:00:00.000", oneHourInSeconds, oneHourInSeconds, "01:00:00");

            // for backwards compatibility, what happens with decimals
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("02:00.021", twoMinutesInSeconds, twoMinutesInSeconds, "02:00");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("=02:00.021", twoMinutesInSeconds, twoMinutesInSeconds, "=02:00");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("02:00.599", twoMinutesInSeconds + 1, twoMinutesInSeconds + 1, "02:01");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("=02:00.599", twoMinutesInSeconds + 1, twoMinutesInSeconds + 1, "=02:01");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("2.1", 2, 2, "00:02");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("1.6", 2, 2, "00:02");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("01:00:00.056", oneHourInSeconds, oneHourInSeconds, "01:00:00");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("01:00:00.789", oneHourInSeconds + 1, oneHourInSeconds + 1, "01:00:01");

            // Then with each of the other operators
            // Less than (strictly less than)
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("<02:00", int.MinValue, twoMinutesInSeconds - 1, "<02:00");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("<02:50.000", int.MinValue, twoMinutesInSeconds + 49, "<02:50");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("<2", int.MinValue, 1, "<00:02");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("<2.0", int.MinValue, 1, "<00:02");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("<01:02:00.000", int.MinValue, oneHourInSeconds + twoMinutesInSeconds - 1, "<01:02:00");

            // with decimals (for less than, round down first then 
            // allow equality)
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("<02:00.021", int.MinValue, twoMinutesInSeconds, "<=02:00");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("<02:00.599", int.MinValue, twoMinutesInSeconds, "<=02:00");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("<2.1", int.MinValue, 2, "<=00:02");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("<1.6", int.MinValue, 1, "<=00:01");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("<01:00:00.056", int.MinValue, oneHourInSeconds, "<=01:00:00");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("<01:00:00.789", int.MinValue, oneHourInSeconds, "<=01:00:00");

            // Greater than (strictly)
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime(">02:00", twoMinutesInSeconds + 1, int.MaxValue, ">02:00");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime(">02:00.000", twoMinutesInSeconds + 1, int.MaxValue, ">02:00");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime(">2", 3, int.MaxValue, ">00:02");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime(">2.0", 3, int.MaxValue, ">00:02");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime(">01:02:00.000", oneHourInSeconds + twoMinutesInSeconds + 1, int.MaxValue, ">01:02:00");
            // and the decimals (for greater than, round up then allow equality)
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime(">02:00.021", twoMinutesInSeconds + 1, int.MaxValue, ">=02:01");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime(">02:00.599", twoMinutesInSeconds + 1, int.MaxValue, ">=02:01");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime(">2.1", 3, int.MaxValue, ">=00:03");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime(">1.6", 2, int.MaxValue, ">=00:02");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime(">01:00:00.056", oneHourInSeconds + 1, int.MaxValue, ">=01:00:01");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime(">01:00:00.789", oneHourInSeconds + 1, int.MaxValue, ">=01:00:01");

            // <=
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("<=02:00", int.MinValue, twoMinutesInSeconds, "<=02:00");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("<=02:00.000", int.MinValue, twoMinutesInSeconds, "<=02:00");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("<=2.0", int.MinValue, 2, "<=00:02");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("<=2", int.MinValue, 2, "<=00:02");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("<=01:02:00.000", int.MinValue, oneHourInSeconds + twoMinutesInSeconds, "<=01:02:00");

            // and the decimals (round down to nearest int for <=)
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("<=02:00.021", int.MinValue, twoMinutesInSeconds, "<=02:00");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("<=02:00.599", int.MinValue, twoMinutesInSeconds, "<=02:00");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("<=2.1", int.MinValue, 2, "<=00:02");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("<=1.6", int.MinValue, 1, "<=00:01");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("<=01:00:00.056", int.MinValue, oneHourInSeconds, "<=01:00:00");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("<=01:00:00.789", int.MinValue, oneHourInSeconds, "<=01:00:00");

            // >= 
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime(">=02:00", twoMinutesInSeconds, int.MaxValue, ">=02:00");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime(">=02:00.000", twoMinutesInSeconds, int.MaxValue, ">=02:00");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime(">=2.0", 2, int.MaxValue, ">=00:02");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime(">=2", 2, int.MaxValue, ">=00:02");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime(">=01:02:00.000", oneHourInSeconds + twoMinutesInSeconds, int.MaxValue, ">=01:02:00");

            // and the decimals (round up to nearest int for >=)
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime(">=02:00.021", twoMinutesInSeconds + 1, int.MaxValue, ">=02:01");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime(">=02:00.599", twoMinutesInSeconds + 1, int.MaxValue, ">=02:01");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime(">=2.1", 3, int.MaxValue, ">=00:03");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime(">=1.6", 2, int.MaxValue, ">=00:02");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime(">=01:00:00.056", oneHourInSeconds + 1, int.MaxValue, ">=01:00:01");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime(">=01:00:00.789", oneHourInSeconds + 1, int.MaxValue, ">=01:00:01");

            // Check that spaces don't break everything
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("    <=    2.0   ", int.MinValue, 2, "<=00:02");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime("   2.0   ", 2, 2, "00:02");

            // these type should work too ">=2:00.000 , <3:00.000"
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime(">=2:02.568 , <3:20.059", twoMinutesInSeconds + 3, threeMinutesInSeconds + 20, ">=02:03, <=03:20");
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime(">2, <=3", 3, 3, ">00:02, <=00:03");

            // Test that 'All' is not case sensitive
            TestWorkTimeFilter_ReturnsTrueAndSetsCorrectMaxAndMinWorkTime(" all ", int.MinValue, int.MaxValue, "all");
        }
        #endregion
    }
}
#endif
