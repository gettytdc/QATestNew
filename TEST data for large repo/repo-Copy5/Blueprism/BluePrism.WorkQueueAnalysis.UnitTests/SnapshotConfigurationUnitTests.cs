#if UNITTESTS

using BluePrism.Data.DataModels.WorkQueueAnalysis;

namespace BluePrism.WorkQueueAnalysis.UnitTests
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using NodaTime;

    public class SnapshotConfigurationUnitTests
    {
        private readonly SnapshotDayConfiguration _daysOfWeek =
            new SnapshotDayConfiguration(true, true, true, true, true, false, false);
        private readonly SnapshotDayConfiguration _everyDayOfWeek =
            new SnapshotDayConfiguration(true, true, true, true, true, true, true);

        private readonly TimeZoneInfo timezone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

        [Test]
        public void GetSnapshotTimes_NoStartEndTimes_12hrInterval_ReturnsCorrectTimes()
        {
            var config = new SnapshotConfiguration(-1, true,
                "Test config", SnapshotInterval.TwelveHours,
                timezone,
                new LocalTime(),
                new LocalTime(),
                _daysOfWeek);
            List<int> expectedTimes = new List<int> { 0, 43200 };
            var result = config.GetSnapshotTimes();
            Assert.AreEqual(expectedTimes, result);
        }

        [Test]
        public void GetSnapshotTimes_NoStartEndTimes_15minInterval_ReturnsCorrectTimes()
        {
            var config = new SnapshotConfiguration(-1, true,
                "Test config", SnapshotInterval.FifteenMinutes,
                timezone,
                new LocalTime(),
                new LocalTime(),
                _daysOfWeek);
            List<int> expectedTimes = new List<int>
            {
                0, 900, 1800, 2700, 3600, 4500, 5400, 6300, 7200, 8100,
                9000,  9900, 10800, 11700, 12600, 13500, 14400, 15300, 16200, 17100,
                18000, 18900, 19800, 20700, 21600, 22500, 23400, 24300, 25200, 26100,
                27000, 27900, 28800, 29700, 30600, 31500, 32400, 33300, 34200, 35100,
                36000, 36900, 37800, 38700, 39600, 40500, 41400, 42300, 43200, 44100,
                45000, 45900, 46800, 47700, 48600, 49500, 50400, 51300, 52200, 53100,
                54000, 54900, 55800, 56700, 57600, 58500, 59400, 60300, 61200, 62100,
                63000, 63900, 64800, 65700, 66600, 67500, 68400, 69300, 70200, 71100,
                72000, 72900, 73800, 74700, 75600, 76500, 77400, 78300, 79200, 80100,
                81000, 81900, 82800, 83700, 84600, 85500
            };
            var result = config.GetSnapshotTimes();
            Assert.AreEqual(expectedTimes, result);
        }
        [Test]
        public void GetSnapshotTimes_IntervalOutsideEndTime_ReturnsOnlyFirstSnapshot()
        {
            var config = new SnapshotConfiguration(-1, true,
                "Test config", SnapshotInterval.TwelveHours,
                timezone,
                new LocalTime(09, 00),
                new LocalTime(17, 00),
                _daysOfWeek);
            List<int> expectedTimes = new List<int> { 32400 };
            var result = config.GetSnapshotTimes();
            Assert.AreEqual(expectedTimes, result);
        }

        [Test]
        public void GetSnapshotTimes_HasStartAndEndTimes_Hourly_ReturnsCorrectList()
        {
            var config = new SnapshotConfiguration(-1, true,
                "Test config", SnapshotInterval.OneHour,
                timezone,
                new LocalTime(09, 00),
                new LocalTime(13, 00),
                _daysOfWeek);
            List<int> expectedTimes = new List<int> { 32400, 36000, 39600, 43200, 46800 };
            var result = config.GetSnapshotTimes();
            Assert.AreEqual(expectedTimes, result);
        }


        [Test]
        public void GetSnapshotTimes_HasStartEndTimes_15minInterval_ReturnsCorrectTimes()
        {
            var config = new SnapshotConfiguration(-1, true,
                "Test config", SnapshotInterval.FifteenMinutes,
                timezone,
                new LocalTime(09, 00),
                new LocalTime(16, 45),
                _daysOfWeek);
            List<int> expectedTimes = new List<int>
            {
                32400, 33300, 34200, 35100,
                36000, 36900, 37800, 38700, 39600, 40500, 41400, 42300, 43200, 44100,
                45000, 45900, 46800, 47700, 48600, 49500, 50400, 51300, 52200, 53100,
                54000, 54900, 55800, 56700, 57600, 58500, 59400, 60300
            };
            var result = config.GetSnapshotTimes();
            Assert.AreEqual(expectedTimes, result);
        }

        [Test]
        public void GetSnapshotTimes_NoStartEndTimes_24HourlyInterval_ReturnsOnlyMidnight()
        {
            var config = new SnapshotConfiguration(-1, true,
                "Test config", SnapshotInterval.TwentyFourHours,
                timezone,
                new LocalTime(00, 00),
                new LocalTime(00, 00),
                _daysOfWeek);
            List<int> expectedTimes = new List<int>
            {
                0
            };
            var result = config.GetSnapshotTimes();
            Assert.AreEqual(expectedTimes, result);
        }

        [Test]
        public void DeepClone_CorrectlyClonesAllProperties()
        {
            var config = new SnapshotConfiguration(-1, true,
                "Test config", SnapshotInterval.FifteenMinutes,
                timezone,
                new LocalTime(09, 00),
                new LocalTime(16, 45),
                _daysOfWeek);
            var configClone = SnapshotConfiguration.DeepClone(config);
            Assert.AreEqual(config.Interval, configClone.Interval);
            Assert.AreEqual(config.Id, configClone.Id);
            Assert.AreEqual(config.Enabled, configClone.Enabled);
            Assert.AreEqual(config.Name, configClone.Name);
            Assert.AreEqual(config.Timezone, configClone.Timezone);
            Assert.AreEqual(config.StartTime, configClone.StartTime);
            Assert.AreEqual(config.EndTime, configClone.EndTime);
            Assert.AreEqual(config.DaysOfTheWeek.Monday, configClone.DaysOfTheWeek.Monday);
            Assert.AreEqual(config.DaysOfTheWeek.Tuesday, configClone.DaysOfTheWeek.Tuesday);
            Assert.AreEqual(config.DaysOfTheWeek.Wednesday, configClone.DaysOfTheWeek.Wednesday);
            Assert.AreEqual(config.DaysOfTheWeek.Thursday, configClone.DaysOfTheWeek.Thursday);
            Assert.AreEqual(config.DaysOfTheWeek.Friday, configClone.DaysOfTheWeek.Friday);
            Assert.AreEqual(config.DaysOfTheWeek.Saturday, configClone.DaysOfTheWeek.Saturday);
            Assert.AreEqual(config.DaysOfTheWeek.Sunday, configClone.DaysOfTheWeek.Sunday);
        }
        [Test]
        public void TestConstructor_EmptyName_SetsEmptyNameProperty()
        {
            var config = new SnapshotConfiguration(-1, true,
                String.Empty, SnapshotInterval.FifteenMinutes,
                timezone,
                new LocalTime(09, 00),
                new LocalTime(16, 45),
                _daysOfWeek);

            Assert.AreEqual(true, config.NameIsNullEmptyOrWhitespace);
            Assert.AreEqual(false, config.IsValidConfiguration());
        }
        [Test]
        public void TestConstructor_EmptyTimezone_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SnapshotConfiguration(-1, true,
                "Test", SnapshotInterval.FifteenMinutes,
                null,
                new LocalTime(09, 00),
                new LocalTime(16, 45),
                _daysOfWeek));
        }
        [Test]
        public void TestConstructor_EmptyDayWeek_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SnapshotConfiguration(-1, true,
                String.Empty, SnapshotInterval.FifteenMinutes,
                timezone,
                new LocalTime(09, 00),
                new LocalTime(16, 45),
                null));
        }

        [Test]
        public void TestSnapshotRowsPerQueue_ReturnsCorrectValue()
        {
            var config = new SnapshotConfiguration(-1, true,
                 "test config", SnapshotInterval.FifteenMinutes,
                 timezone,
                 new LocalTime(00, 00),
                 new LocalTime(00, 00),
                 _daysOfWeek);
            var result = config.ConfiguredSnapshotRowsPerQueue();
            Assert.AreEqual(5 * 96, result);
        }


        [Test]
        public void TestSnapshotRowsPerQueue_midnightToElevenFortyFive_ReturnsCorrectValue()
        {
            var config = new SnapshotConfiguration(-1, true,
                "test config", SnapshotInterval.FifteenMinutes,
                timezone,
                new LocalTime(00, 00),
                new LocalTime(23, 45),
                _daysOfWeek);
            var result = config.ConfiguredSnapshotRowsPerQueue();
            Assert.AreEqual(5 * 96, result);
        }

        [TestCase(1, 0, 1, 0, 0)]
        [TestCase(1, 0, 2, 0, 86400)]
        [TestCase(2, 0, 5, 43200, 302400)] // tue at midnight to fri at midday
        [TestCase(1, 0, 7, 0, 518400)]
        [TestCase(1, 0, 7, 85500, 603900)]
        [TestCase(5, 0, 1, 0, 259200)] // fri midnight to monday midnight 
        [TestCase(3, 6300, 3, 7200, 900)] //wed at 1:45 to wed at 2:00
        [TestCase(3, 7200, 3, 6300, 603900)] //wed at 2:00 to wed at 1:45
        [TestCase(2, 68400, 4, 25200, 129600)] //Tue 7pm to Thur 7am
        public void TestDayAndTimeDifferenceInSeconds(int day1, int time1, int day2, int time2, int expectedResult)
        {
            var result = SnapshotConfiguration.DayAndTimeDifferenceInSeconds(day1, time1, day2, time2);
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void GetInterimSnapshotTimes_ConstantSnapshots_ReturnsEmptyList()
        {
            var config = new SnapshotConfiguration(45, true, "test",
                SnapshotInterval.FifteenMinutes, timezone,
                new LocalTime(00, 00),
                new LocalTime(00, 00),
                _everyDayOfWeek);
            var result = config.GetInterimSnapshotDaysAndTimes();
            Assert.AreEqual(new List<int[]> { }, result);
        }

        [Test]
        public void GetInterimSnapshotTimes_MonToFriAllDaySnapshotsHourly_ReturnsExpected()
        {
            // last snapshot on Friday is 11pm so need interims on Saturday and Sunday at 11pm
            var config = new SnapshotConfiguration(45, true, "test",
                SnapshotInterval.OneHour, timezone,
                new LocalTime(00, 00),
                new LocalTime(00, 00),
                _daysOfWeek);
            var result = config.GetInterimSnapshotDaysAndTimes();
            var expectedResult = new List<int[]>
            {
                new[] { 6, 82800 }, new[] { 7, 82800 }
            };
            Assert.That(result, Is.EquivalentTo(expectedResult));
        }

        [Test]
        public void GetInterimSnapshotTimes_HourlyMonOnlyBetween9And10_ReturnsExpected()
        {
            // last snapshot is Monday at 10am so need interim ones every other day at 10
            var config = new SnapshotConfiguration(2, true, "test",
                SnapshotInterval.OneHour, timezone,
                new LocalTime(09, 00),
                new LocalTime(10, 00),
                new SnapshotDayConfiguration(true, false, false, false, false, false, false));
            var result = config.GetInterimSnapshotDaysAndTimes();
            var expectedResult = new List<int[]> { new[] { 2, 36000 }, new [] {3, 36000},
                new [] {4, 36000}, new[] { 5, 36000 }, new [] {6, 36000}, new[] { 7, 36000 } };
            Assert.That(result, Is.EquivalentTo(expectedResult));
        }

        [Test]
        public void GetInterimSnapshotTimes_MonToFri9To5Snapshots15Minutely_ReturnsExpected()
        {
            // last snapshot on Friday is 5pm so need interims on Sat and Sunday at 5pm
            var config = new SnapshotConfiguration(45, true, "test",
                SnapshotInterval.FifteenMinutes, timezone,
                new LocalTime(09, 00),
                new LocalTime(17, 00),
                _daysOfWeek);
            var result = config.GetInterimSnapshotDaysAndTimes();
            var expectedResult = new List<int[]> { new[] { 6, 61200 }, new[] { 7, 61200 }};
            Assert.That(result, Is.EquivalentTo(expectedResult));

        }

        [Test]
        public void GetInterimSnapshotTimes_JustMonAndThur9To5SnapshotsHalfHourly_ReturnsExpected()
        {
            // last snapshot on Monday is 5pm so need interims on Tue and Wednesday at 5pm
            // last snapshot on Thursday is 5pm so need interims on Friday and Saturday and Sunday at 5pm
            var config = new SnapshotConfiguration(45, true, "test",
                SnapshotInterval.FifteenMinutes, timezone,
                new LocalTime(09, 00),
                new LocalTime(17, 00),
                new SnapshotDayConfiguration(true, false, false, true, false, false, false));
            var result = config.GetInterimSnapshotDaysAndTimes();
            var expectedResult = new List<int[]>
                {
                    new[] { 2, 61200 }, new[] { 3, 61200 }, new[] { 5, 61200 },new[] { 6, 61200 }, new[] { 7, 61200 }
                };
            Assert.That(result, Is.EquivalentTo(expectedResult));
        }

        [Test]
        public void GetInterimSnapshotTimes_JustMonAndSatAllDaySnapshotsSixHourly_ReturnsExpected()
        {
            // last snapshot on Monday is 6pm so need interims on Tue, Wednesday, Thur, Fri at 6pm
            // snapshots then take place Saturday 00:00, 06:00, 12:00, 18:00 so we need an interim on Sunday at 6pm
            var config = new SnapshotConfiguration(45, true, "test",
                SnapshotInterval.SixHours, timezone,
                new LocalTime(00, 00),
                new LocalTime(00, 00),
                new SnapshotDayConfiguration(true, false, false, false, false, true, false));
            var result = config.GetInterimSnapshotDaysAndTimes();
            var expectedResult = new List<int[]>
            {
                new[] { 2, 64800 }, new [] { 3, 64800 }, new [] { 4, 64800 }, new int[] {5 , 64800 }, new [] { 7, 64800 },
            };
            Assert.That(result, Is.EquivalentTo(expectedResult));
        }

        [Test]
        public void GetInterimSnapshotTimes_JustMonAndSatAllDaySnapshots24Hourly_ReturnsExpected()
        {
            // first and last (only) snapshot on Monday is 00:00 so need interims on Tue, Wed, Thur, Friday at 00:00
            // (then the next real snapshot is Saturday at 00:00) so we need another interim Sunday 00:00
            var config = new SnapshotConfiguration(45, true, "test",
                SnapshotInterval.TwentyFourHours, timezone,
                new LocalTime(00, 00),
                new LocalTime(00, 00),
                new SnapshotDayConfiguration(true, false, false, false, false, true, false));
            var result = config.GetInterimSnapshotDaysAndTimes();
            var expectedResult = new List<int[]>
            {
                new [] { 2, 0 },new [] { 3, 0 }, new [] { 4, 0 }, new [] { 5, 0 }, new [] { 7, 0 }
            };
            Assert.That(result, Is.EquivalentTo(expectedResult));
        }

        [Test]
        public void GetInterimSnapshotTimes_JustMonAndSat9amTo11am24Hourly_ReturnsExpected()
        {
            var config = new SnapshotConfiguration(45, true, "test",
                  SnapshotInterval.TwentyFourHours, timezone,
                  new LocalTime(09, 00),
                  new LocalTime(11, 00),
                  new SnapshotDayConfiguration(true, false, false, false, false, true, false));
            var result = config.GetInterimSnapshotDaysAndTimes();
            var expectedResult = new List<int[]>
            {
                new[] { 2, 32400 }, new[] { 3, 32400 }, new[] { 4, 32400 }, new[] { 5, 32400 }, new[] { 7, 32400 }
            };
            Assert.That(result, Is.EquivalentTo(expectedResult));
        }

        [Test]
        public void GetInterimSnapshotTimes_JustMonAndSat9amTo11amHourly_ReturnsExpected()
        {
            var config = new SnapshotConfiguration(45, true, "test",
                 SnapshotInterval.OneHour, timezone,
                 new LocalTime(09, 00),
                 new LocalTime(11, 00),
                 new SnapshotDayConfiguration(true, false, false, false, false, true, false));
            var result = config.GetInterimSnapshotDaysAndTimes();
            var expectedResult = new List<int[]>
            {
                new[] { 2, 39600 }, new [] { 3, 39600 }, new[] { 4, 39600 }, new [] { 5, 39600 }, new[] { 7, 39600 }
            };
            Assert.That(result, Is.EquivalentTo(expectedResult));
        }

        [Test]
        public void GetInterimSnapshotTimes_JustWednesday24Hourly_ReturnsExpected()
        {
            var config = new SnapshotConfiguration(45, true, "test",
                SnapshotInterval.TwentyFourHours, timezone,
                new LocalTime(00, 00),
                new LocalTime(00, 00),
                new SnapshotDayConfiguration(false, false, true, false, false, false, false));
            var result = config.GetInterimSnapshotDaysAndTimes();
            var expectedResult = new List<int[]>
            {
                new[] { 4, 0 }, new [] { 5, 0 }, new [] { 6, 0 }, new[] { 7, 0 }, new[] { 1, 0 }, new [] { 2, 0 },
            };
            Assert.That(result, Is.EquivalentTo(expectedResult));
        }

        private static readonly int _trendCalculationTime = (int)(new LocalTime(00, 15).TickOfDay / NodaConstants.TicksPerSecond);

        [Test]
        public void GetTrendCalcDaysAndTimes_JustMonAndThurReturnsExpected()
        {
            var config = new SnapshotConfiguration(45, true, "test",
                SnapshotInterval.FifteenMinutes, timezone,
                new LocalTime(09, 00),
                new LocalTime(17, 00),
                new SnapshotDayConfiguration(true, false, false, true, false, false, false));
            var result = config.GetTrendCalculationDaysAndTimes();
            var expectedResult = new List<int[]>
                {new int[] {2, _trendCalculationTime}, new int[] {5, _trendCalculationTime}};
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void GetTrendCalcDaysAndTimes_EveryDayReturnsExpected()
        {
            var config = new SnapshotConfiguration(45, true, "test",
                SnapshotInterval.FifteenMinutes, timezone,
                new LocalTime(09, 00),
                new LocalTime(17, 00),
                _everyDayOfWeek);
            var result = config.GetTrendCalculationDaysAndTimes();
            var expectedResult = new List<int[]>
                {new int[] {1, _trendCalculationTime}, new int[] {2, _trendCalculationTime},
                    new int[] {3, _trendCalculationTime}, new int[] {4, _trendCalculationTime},
                    new int[] {5, _trendCalculationTime}, new int[] {6, _trendCalculationTime},
                    new int[] {7, _trendCalculationTime}};
            Assert.That(result, Is.EquivalentTo(expectedResult));
        }

        [Test]
        public void GetTrendCalcDaysAndTimes_JustWedAndSunReturnsExpected()
        {
            var config = new SnapshotConfiguration(45, true, "test",
                SnapshotInterval.FifteenMinutes, timezone,
                new LocalTime(09, 00),
                new LocalTime(17, 00),
                new SnapshotDayConfiguration(false, false, true,
                    false, false, false, true));
            var result = config.GetTrendCalculationDaysAndTimes();
            var expectedResult = new List<int[]> { new int[] { 1, _trendCalculationTime }, new int[] { 4, _trendCalculationTime } };
            Assert.That(result, Is.EquivalentTo(expectedResult));
        }

        [Test]
        public void GetTrendCalcDaysAndTimes_JustWedReturnsExpected()
        {
            var config = new SnapshotConfiguration(45, true, "test",
                SnapshotInterval.FifteenMinutes, timezone,
                new LocalTime(09, 00),
                new LocalTime(17, 00),
                new SnapshotDayConfiguration(false, false, true,
                    false, false, false, false));
            var result = config.GetTrendCalculationDaysAndTimes();
            var expectedResult = new List<int[]> { new int[] { 4, _trendCalculationTime } };
            Assert.That(result, Is.EquivalentTo(expectedResult));
        }
    }
}

#endif