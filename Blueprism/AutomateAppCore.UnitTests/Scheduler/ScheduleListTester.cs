using BluePrism.AutomateAppCore;
using NUnit.Framework;
using System;

namespace AutomateAppCore.UnitTests.Scheduler
{
    [TestFixture]
    public class ScheduleListTester
    {
        private static void TestRelativeDates(ScheduleListType type, ScheduleRelativeDate dt, int dist, DateTime startDate, DateTime endDate)
        {
            var l = new ScheduleList
            {
                ListType = type,
                RelativeDate = dt,
                DaysDistance = dist
            };
            Assert.That(l.GetStartDate(), Is.EqualTo(startDate.Date), 
                "{0} days {1} {2}. Expected start date : {3}; Found : {4}", dist, type == ScheduleListType.Report? "ending": "starting", dt, startDate, l.GetStartDate());

            Assert.That(l.GetEndDate(), Is.EqualTo(endDate.Date), 
                "{0} days {1} {2}. Expected end date : {3}; Found : {4}", dist, type == ScheduleListType.Report? "ending": "starting", dt, endDate, l.GetEndDate());
        }

        private static void TestAbsoluteDates(ScheduleListType type, DateTime dt, int dist, DateTime startDate, DateTime endDate)
        {
            // Test setting the relative date explicitly and not doing so
            TestAbsoluteDates(type, dt, dist, true, startDate, endDate);
            TestAbsoluteDates(type, dt, dist, false, startDate, endDate);
        }

        private static void TestAbsoluteDates(ScheduleListType type, DateTime dt, int dist, bool setRelative, DateTime startDate, DateTime endDate)
        {
            var l = new ScheduleList
            {
                ListType = type
            };
            // This shouldn't be necessary any more, but it also has to work
            // correctly if you do do this (back-compat)
            if (setRelative)
                l.RelativeDate = ScheduleRelativeDate.None;
            l.AbsoluteDate = dt;
            l.DaysDistance = dist;
            Assert.That(l.GetStartDate(), Is.EqualTo(startDate.Date), "{0} day(s) {1} {2}. Expected start date : {3}; Found : {4}", dist, type == ScheduleListType.Report? "ending": "starting", dt, startDate, l.GetStartDate());
            Assert.That(l.GetEndDate(), Is.EqualTo(endDate.Date), "{0} day(s) {1} {2}. Expected end date : {3}; Found : {4}", dist, type == ScheduleListType.Report? "ending": "starting", dt, endDate, l.GetEndDate());
        }

        [Test]
        public void TestDates()
        {
            TestRelativeDates(ScheduleListType.Report, ScheduleRelativeDate.Today, 1, DateTime.Today, DateTime.Today.AddDays((double)1));
            TestRelativeDates(ScheduleListType.Report, ScheduleRelativeDate.Yesterday, 1, DateTime.Today.AddDays((double)-1), DateTime.Today);
            TestAbsoluteDates(ScheduleListType.Report, DateTime.Today, 1, DateTime.Today, DateTime.Today.AddDays((double)1));

            // 2 days ending yesterday, should include dates of 2 days ago and yesterday,
            // thus start date should be 00:00 2 days ago and end date should be 00:00 today
            TestAbsoluteDates(ScheduleListType.Report, DateTime.Today.AddDays((double)-1), 2, DateTime.Today.AddDays((double)-2), DateTime.Today);
            TestRelativeDates(ScheduleListType.Timetable, ScheduleRelativeDate.Today, 1, DateTime.Today, DateTime.Today.AddDays((double)1));
            TestRelativeDates(ScheduleListType.Timetable, ScheduleRelativeDate.Tomorrow, 1, DateTime.Today.AddDays((double)1), DateTime.Today.AddDays((double)2));
            TestAbsoluteDates(ScheduleListType.Timetable, DateTime.Today, 1, DateTime.Today, DateTime.Today.AddDays((double)1));
            TestAbsoluteDates(ScheduleListType.Timetable, DateTime.Today.AddDays((double)1), 1, DateTime.Today.AddDays((double)1), DateTime.Today.AddDays((double)2));
        }

        [Test]
        public void TestDateRange()
        {
            var startDate = new DateTime(2010, 8, 11, 12, 0, 0);
            var endDate = new DateTime(2010, 8, 17, 12, 0, 0);
            var list = new ScheduleList
            {
                ListType = ScheduleListType.Timetable
            };
            list.SetDateRange(startDate, endDate);
            Assert.That(list.GetStartDate(), Is.EqualTo(startDate));
            Assert.That(list.GetEndDate(), Is.EqualTo(endDate));
            list.ListType = ScheduleListType.Report;
            list.SetDateRange(startDate, endDate);
            Assert.That(list.GetStartDate(), Is.EqualTo(startDate));
            Assert.That(list.GetEndDate(), Is.EqualTo(endDate));
        }
    }
}
