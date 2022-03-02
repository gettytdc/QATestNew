#if UNITTESTS

using System;
using BluePrism.BPCoreLib;
using BluePrism.Scheduling;
using BluePrism.Scheduling.Calendar;
using BluePrism.Scheduling.Triggers;
using NUnit.Framework;

namespace BPScheduler.UnitTests
{
    [TestFixture]
    public class TestTriggerFactory

    {
        private const int TestPeriod = 20;
        private const int DefaultCalendarId = 0;

        private readonly TriggerFactory _factory = new TriggerFactory();
        private readonly DateTime _startDateTime = new DateTime(2019, 09, 16, 14, 05, 06);
        private readonly clsTimeRange _allowedHours = new clsTimeRange(new TimeSpan(00, 00, 00),
            new TimeSpan(01, 01, 01));
        private DummySchedule _schedule;
        private ScheduleCalendar _calendar;

        [SetUp]
        public void SetUpScheduleAndCalendar()
        {
            var dummyStore = new DummyStore();
            var scheduler = new InertScheduler(dummyStore);
            var schedule = new DummySchedule(scheduler);

            var calendar = new ScheduleCalendar(dummyStore.GetSchema())
            {
                WorkingWeek = DaySet.FiveDayWeek
            };
            dummyStore.SaveCalendar(calendar);

            _calendar = dummyStore.GetCalendar(1);
            _schedule = schedule;
        }

        [Test]
        public void TestTriggerFactory_CreateTrigger_NullMetadata_ThrowsArgumentNullException()
        {
            Assert.Throws(typeof(ArgumentNullException),
                    () => _factory.CreateTrigger((TriggerMetaData)null));
        }

        [Test]
        public void TestTriggerFactory_CreateTrigger_IntervalTypeNever_ReturnsNeverTrigger()
        {
            var data = new TriggerMetaData()
            {
                Interval = IntervalType.Never
            };

            var triggerToTest = _factory.CreateTrigger(data);
            Assert.IsInstanceOf<NeverTrigger>(triggerToTest);
        }

        [Test]
        public void TestTriggerFactory_CreateTrigger_InvalidIntervalType_ThrowsInvalidDataException()
        {
            var data = new TriggerMetaData()
            {
                Interval = (IntervalType)889
            };

            Assert.Throws(typeof(InvalidDataException),
                () => _factory.CreateTrigger(data));
        }

        [Test]
        public void TestTriggerFactory_CreateTrigger_IntervalTypeOnce_ReturnsOnceTrigger()
        {
            var data = new TriggerMetaData()
            {
                Interval = IntervalType.Once,
                Mode = TriggerMode.Fire,
                Start = _startDateTime
            };

            var triggerToTest = _factory.CreateTrigger(data);
            Assert.IsInstanceOf<OnceTrigger>(triggerToTest);
            Assert.AreEqual(_startDateTime, triggerToTest.Start);
            Assert.AreEqual(TriggerMode.Fire, triggerToTest.Mode);
        }

        [Test]
        public void TestTriggerFactory_CreateTrigger_IntervalTypeMinute_ReturnsEveryNMinutesWithinRangeTrigger()
        {
            var data = new TriggerMetaData()
            {
                Interval = IntervalType.Minute,
                Period = TestPeriod,
                AllowedHours = _allowedHours,
                CalendarId = _calendar.Id
            };

            var triggerToTest = _factory.CreateTrigger(data);
            _schedule.SetTrigger(triggerToTest);

            Assert.IsInstanceOf<EveryNMinutesWithinRange>(triggerToTest);
            Assert.AreEqual(TestPeriod, triggerToTest.PrimaryMetaData.Period);
            Assert.AreEqual(_allowedHours.StartTime, triggerToTest.PrimaryMetaData.AllowedHours.StartTime);
            Assert.AreEqual(_allowedHours.EndTime, triggerToTest.PrimaryMetaData.AllowedHours.EndTime);
            Assert.AreEqual(_calendar.Id, triggerToTest.PrimaryMetaData.CalendarId);
        }

        [Test]
        public void TestTriggerFactory_CreateTrigger_IntervalTypeHour_ReturnsEveryNHoursWithinRangeTrigger()
        {
            var data = new TriggerMetaData()
            {
                Interval = IntervalType.Hour,
                Period = TestPeriod,
                AllowedHours = _allowedHours,
                CalendarId = _calendar.Id
            };

            var triggerToTest = _factory.CreateTrigger(data);
            _schedule.SetTrigger(triggerToTest);

            Assert.IsInstanceOf<EveryNHoursWithinRange>(triggerToTest);
            Assert.AreEqual(TestPeriod, triggerToTest.PrimaryMetaData.Period);
            Assert.AreEqual(_allowedHours.StartTime, triggerToTest.PrimaryMetaData.AllowedHours.StartTime);
            Assert.AreEqual(_allowedHours.EndTime, triggerToTest.PrimaryMetaData.AllowedHours.EndTime);
            Assert.AreEqual(_calendar.Id, triggerToTest.PrimaryMetaData.CalendarId);
        }

        [Test]
        public void TestTriggerFactory_CreateTrigger_IntervalTypeDay_WithCalendar_ReturnsEveryNDaysWithinIdentifiedCalendarTrigger()
        {
            var data = new TriggerMetaData()
            {
                Interval = IntervalType.Day,
                Period = TestPeriod,
                CalendarId = _calendar.Id
            };

            var triggerToTest = _factory.CreateTrigger(data);
            _schedule.SetTrigger(triggerToTest);

            Assert.IsInstanceOf<EveryNDaysWithinIdentifiedCalendar>(triggerToTest);
            Assert.AreEqual(TestPeriod, triggerToTest.PrimaryMetaData.Period);
            Assert.AreEqual(_calendar.Id, triggerToTest.PrimaryMetaData.CalendarId);
        }

        [Test]
        public void TestTriggerFactory_CreateTrigger_IntervalTypeDay_NoCalendar_ReturnsEveryNDaysTrigger()
        {
            var data = new TriggerMetaData()
            {
                Interval = IntervalType.Day,
                Period = TestPeriod
            };

            var triggerToTest = _factory.CreateTrigger(data);
            _schedule.SetTrigger(triggerToTest);

            Assert.IsInstanceOf<EveryNDays>(triggerToTest);
            Assert.AreEqual(TestPeriod, triggerToTest.PrimaryMetaData.Period);
        }

        [Test]
        public void TestTriggerFactory_CreateTrigger_IntervalTypeWeek_WithCalendar_ReturnsEveryNWeeksOnNthDayInIdentifiedCalendarTrigger()
        {
            var data = new TriggerMetaData()
            {
                Interval = IntervalType.Week,
                Period = TestPeriod,
                CalendarId = _calendar.Id,
                NthOfWeek = NthOfWeek.Third
            };

            var triggerToTest = _factory.CreateTrigger(data);
            _schedule.SetTrigger(triggerToTest);

            Assert.IsInstanceOf<EveryNWeeksOnNthDayInIdentifiedCalendar>(triggerToTest);
            Assert.AreEqual(TestPeriod, triggerToTest.PrimaryMetaData.Period);
            Assert.AreEqual(_calendar.Id, triggerToTest.PrimaryMetaData.CalendarId);
            Assert.AreEqual(NthOfWeek.Third, triggerToTest.PrimaryMetaData.NthOfWeek);
        }

        [Test]
        public void TestTriggerFactory_CreateTrigger_IntervalTypeWeek_NoCalendar_ReturnsEveryNWeeksOnNthDayInKnownCalendarTrigger()
        {
            var data = new TriggerMetaData()
            {
                Interval = IntervalType.Week,
                Period = TestPeriod
            };

            var triggerToTest = _factory.CreateTrigger(data);
            _schedule.SetTrigger(triggerToTest);

            Assert.IsInstanceOf<EveryNWeeksOnNthDayInKnownCalendar>(triggerToTest);
            Assert.AreEqual(TestPeriod, triggerToTest.PrimaryMetaData.Period);
            Assert.AreEqual(NthOfWeek.First, triggerToTest.PrimaryMetaData.NthOfWeek);
        }

        [Test]
        public void TestTriggerFactory_CreateTrigger_IntervalTypeMonth_WithDaySet_ReturnsEveryNthKnownCalendarDayInNthMonthTrigger()
        {
            var data = new TriggerMetaData()
            {
                Interval = IntervalType.Month,
                Period = TestPeriod,
                Days = DaySet.FiveDayWeek,
                Nth = NthOfMonth.Fourth
            };

            var triggerToTest = _factory.CreateTrigger(data);
            _schedule.SetTrigger(triggerToTest);

            Assert.IsInstanceOf<EveryNthKnownCalendarDayInNthMonth>(triggerToTest);
            Assert.AreEqual(TestPeriod, triggerToTest.PrimaryMetaData.Period);
            Assert.AreEqual(NthOfMonth.Fourth, triggerToTest.PrimaryMetaData.Nth);
            Assert.AreEqual(DaySet.FiveDayWeek, triggerToTest.PrimaryMetaData.Days);
        }

        [Test]
        public void TestTriggerFactory_CreateTrigger_IntervalTypeMonth_WithCalendarNoDaySet_ReturnsEveryNthIdentifiedCalendarDayInNthMonthTrigger()
        {
            var data = new TriggerMetaData()
            {
                Interval = IntervalType.Month,
                Period = TestPeriod,
                CalendarId = _calendar.Id,
                Nth = NthOfMonth.Fourth
            };

            var triggerToTest = _factory.CreateTrigger(data);
            _schedule.SetTrigger(triggerToTest);

            Assert.IsInstanceOf<EveryNthIdentifiedCalendarDayInNthMonth>(triggerToTest);
            Assert.AreEqual(TestPeriod, triggerToTest.PrimaryMetaData.Period);
            Assert.AreEqual(NthOfMonth.Fourth, triggerToTest.PrimaryMetaData.Nth);
            Assert.AreEqual(_calendar.Id, triggerToTest.PrimaryMetaData.CalendarId);
        }

        [Test]
        public void TestTriggerFactory_CreateTrigger_IntervalTypeMonth_WithCalendarAndDaySet_ReturnsEveryNthKnownCalendarDayInNthMonthTrigger()
        {
            var data = new TriggerMetaData()
            {
                Interval = IntervalType.Month,
                Period = TestPeriod,
                CalendarId = _calendar.Id,
                Days = DaySet.FiveDayWeek,
                Nth = NthOfMonth.Fourth
            };

            var triggerToTest = _factory.CreateTrigger(data);
            _schedule.SetTrigger(triggerToTest);

            Assert.IsInstanceOf<EveryNthKnownCalendarDayInNthMonth>(triggerToTest);
            Assert.AreEqual(TestPeriod, triggerToTest.PrimaryMetaData.Period);
            Assert.AreEqual(NthOfMonth.Fourth, triggerToTest.PrimaryMetaData.Nth);
            Assert.AreEqual(DaySet.FiveDayWeek, triggerToTest.PrimaryMetaData.Days);
            Assert.AreEqual(DefaultCalendarId, triggerToTest.PrimaryMetaData.CalendarId);
        }

        [Test]
        public void TestTriggerFactory_CreateTrigger_IntervalTypeMonth_NoDaySetNoCalendar_ReturnsEveryNthOfNthMonthTrigger()
        {
            var data = new TriggerMetaData()
            {
                Interval = IntervalType.Month,
                Period = TestPeriod,
                MissingDatePolicy = NonExistentDatePolicy.LastSupportedDayInMonth
            };

            var triggerToTest = _factory.CreateTrigger(data);
            _schedule.SetTrigger(triggerToTest);

            Assert.IsInstanceOf<EveryNthOfNthMonth>(triggerToTest);
            Assert.AreEqual(TestPeriod, triggerToTest.PrimaryMetaData.Period);
            Assert.AreEqual(NonExistentDatePolicy.LastSupportedDayInMonth, triggerToTest.PrimaryMetaData.MissingDatePolicy);
            Assert.AreEqual(DefaultCalendarId, triggerToTest.PrimaryMetaData.CalendarId);
        }


        [Test]
        public void TestTriggerFactory_CreateTrigger_IntervalTypeYears_ReturnsEveryNYearsTrigger()
        {
            var data = new TriggerMetaData()
            {
                Interval = IntervalType.Year,
                Period = TestPeriod
            };

            var triggerToTest = _factory.CreateTrigger(data);
            _schedule.SetTrigger(triggerToTest);

            Assert.IsInstanceOf<EveryNYears>(triggerToTest);
            Assert.AreEqual(TestPeriod, triggerToTest.PrimaryMetaData.Period);
            Assert.AreEqual(NonExistentDatePolicy.LastSupportedDayInMonth, triggerToTest.PrimaryMetaData.MissingDatePolicy);
            Assert.AreEqual(DefaultCalendarId, triggerToTest.PrimaryMetaData.CalendarId);
        }

        [Test]
        public void TestTriggerFactory_CreateTrigger_IntervalTypeSecond_ReturnsEveryNSecondsTrigger()
        {
            var data = new TriggerMetaData()
            {
                Interval = IntervalType.Second,
                Period = TestPeriod
            };

            var triggerToTest = _factory.CreateTrigger(data);
            _schedule.SetTrigger(triggerToTest);

            Assert.IsInstanceOf<EveryNSeconds>(triggerToTest);
            Assert.AreEqual(TestPeriod, triggerToTest.PrimaryMetaData.Period);
        }

        [Test]
        public void TestTriggerFactory_CreateTrigger_TriggerMetadata_WithDefaults()
        {
            var data = new TriggerMetaData()
            {
                Interval = IntervalType.Never
            };

            var triggerToTest = _factory.CreateTrigger(data);

            Assert.AreEqual(TriggerMode.Indeterminate, triggerToTest.Mode);
            Assert.AreEqual(0, triggerToTest.Priority);
            Assert.IsNotNull(triggerToTest.Start);
            Assert.IsNotNull(triggerToTest.End);
            Assert.AreEqual(false, triggerToTest.IsUserTrigger);
        }

        [Test]
        public void TestTriggerFactory_CreateTrigger_TriggerMetadata_WithoutDefaults()
        {
            var data = new TriggerMetaData()
            {
                Interval = IntervalType.Never,
                Mode = TriggerMode.Fire,
                Priority = 4,
                Start = _startDateTime,
                End = _startDateTime.AddMinutes(30),
                IsUserTrigger = true
            };

            var triggerToTest = _factory.CreateTrigger(data);

            Assert.AreEqual(TriggerMode.Fire, triggerToTest.Mode);
            Assert.AreEqual(4, triggerToTest.Priority);
            Assert.AreEqual(_startDateTime, triggerToTest.Start);
            Assert.AreEqual(_startDateTime.AddMinutes(30), triggerToTest.End);
            Assert.AreEqual(true, triggerToTest.IsUserTrigger);
        }
    }
}
#endif
