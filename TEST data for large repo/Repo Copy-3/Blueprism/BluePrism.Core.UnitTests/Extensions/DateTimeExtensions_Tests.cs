using System;
using BluePrism.Core.Extensions;
using FluentAssertions;
using NUnit.Framework;

namespace BluePrism.Core.UnitTests.Extensions
{
    public class DateTimeExtensions_Tests
    {
        [TestFixture]
        public class TimeUntilNextHourTests
        {
            private readonly DateTime now = DateTime.Now;

            [Test]
            public void TimeUntilNextHour_FromNowIsInfuture()
            {
                var start = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0);

                var duration = start.TimeUntilNextHour(2);

                Assert.IsTrue(duration.TotalMilliseconds > 0);
            }

            [Test]
            public void TimeUntilNextHour_HoursAreWithin24HourClock()
            {
                Action actNegative = () => now.TimeUntilNextHour(-1);
                Action actTooHigh  = () => now.TimeUntilNextHour(25);

                actNegative.ShouldThrow<ArgumentException>(because: "Hours have to be greater than 0");
                actTooHigh.ShouldThrow<ArgumentException>(because: "Hours have to be less than 25");
            }

            [Test]
            public void TimeUntilNextHour_FindNextHour()
            {
                var target = new DateTime(now.Year, now.Month, now.Day, 22, 0, 0);
                var time   = new DateTime(now.Year, now.Month, now.Day, 8, 33, 12);

                var duration = time.TimeUntilNextHour(22);

                Assert.IsTrue(time + duration == target);
            }

            /// <summary>
            /// Find the time until a passed next hour. Validating it can handle a date before today.
            /// </summary>
            [Test]
            public void TimeUntilNextHour_FindNextHourLastMonth()
            {
                const int targetHour = 5;
                var tomorrow5am = new DateTime(now.Year, now.AddMonths(-1).Month, 15, targetHour, 0, 0);
                var today1pm    = new DateTime(now.Year, now.AddMonths(-1).Month, 15, 1, 0, 0);

                var duration = today1pm.TimeUntilNextHour(targetHour);

                var result = today1pm + duration;
                Assert.IsTrue(result == tomorrow5am);
            }

            /// <summary>
            /// Find the time to an hour that has already passed. Should work out to be tomorrow
            /// </summary>
            [Test]
            public void TimeUntilNextHour_PassedHourIsNextDay()
            {
                const int targetHour = 5;
                var tomorrow5am = new DateTime(now.Year, now.Month, 3, targetHour, 0, 0);
                var today1pm    = new DateTime(now.Year, now.Month, 2, 13, 0, 0);

                var duration = today1pm.TimeUntilNextHour(targetHour);

                var result = today1pm + duration;
                Assert.IsTrue(result == tomorrow5am);
            }           
        }
    }
}
