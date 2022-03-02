using System;
using NUnit.Framework;

namespace AutomateUI.UnitTests
{

    /// <summary>A class to test that the correct filter date is returned for each of the
    /// values used on the Start and End date columns when filtering in session management
    /// </summary>
    [TestFixture]
    public partial class SessionManagementFilterTests
    {
        /// <summary>
        /// Helper method to test date filtering in session management.
        /// Very simple test of the Extract Date function of ctlSessionManagement which
        /// returns a date to filter the column on, based on the text description
        /// in the combo box
        /// </summary>
        public void Test_ExtractDate(DateTime @base, string filterText, DateTime expectedDate)
        {
            Assert.AreEqual(ctlSessionManagement.ExtractDate(@base, filterText), expectedDate);
        }

        /// <summary>
        /// Method which calls the test on each of the test cases
        /// Fixed values are built into the combo boxes, so test that each of those returns
        /// the expected date.
        /// </summary>
        /// <remarks>
        /// Fixed values are currently:
        /// - Last 15 minutes,
        /// - Last 30 minutes,
        /// - Last 1 hour,
        /// - Last 2 hours,
        /// - Last 4, hours,
        /// - Last 8 hours,
        /// - Last 12 hours,
        /// - Last 18 hours,
        /// - Last 24 hours.
        /// - Today
        /// - Last 7 days
        /// - Last 31 days
        /// </remarks>
        [Test]
        public void Test_ExtractDate()
        {
            var baseDate = new DateTime(2016, 11, 1, 10, 30, 0);
            Test_ExtractDate(baseDate, "Last 15 Minutes", new DateTime(2016, 11, 1, 10, 15, 0));
            Test_ExtractDate(baseDate, "Last 30 Minutes", new DateTime(2016, 11, 1, 10, 0, 0));
            Test_ExtractDate(baseDate, "Last 1 Hour", new DateTime(2016, 11, 1, 9, 30, 0));
            Test_ExtractDate(baseDate, "Last 2 Hours", new DateTime(2016, 11, 1, 8, 30, 0));
            Test_ExtractDate(baseDate, "Last 4 Hours", new DateTime(2016, 11, 1, 6, 30, 0));
            Test_ExtractDate(baseDate, "Last 8 Hours", new DateTime(2016, 11, 1, 2, 30, 0));
            Test_ExtractDate(baseDate, "Last 12 Hours", new DateTime(2016, 10, 31, 22, 30, 0));
            Test_ExtractDate(baseDate, "Last 18 Hours", new DateTime(2016, 10, 31, 16, 30, 0));
            Test_ExtractDate(baseDate, "Last 24 Hours", new DateTime(2016, 10, 31, 10, 30, 0));

            // "Day" values return only a date
            Test_ExtractDate(baseDate, "Today", new DateTime(2016, 11, 1));
            Test_ExtractDate(baseDate, "Last 7 Days", new DateTime(2016, 10, 25));
            Test_ExtractDate(baseDate, "Last 31 Days", new DateTime(2016, 10, 1));
        }
    }

}
