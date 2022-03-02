using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace BluePrism.DataPipeline.UnitTests
{
    [TestFixture]
    public class DataPipelineSettingsTests
    {
        private const int IntervalOKInSeconds = 10 * 60; // 10 minutes
        private const int IntervalBelowMinAllowedInSeconds = 1 * 60; // 1 minute
        private const int IntervalAboveMaxAllowedInSeconds = 60 * 60 * 25; // 25 hours

        [Test]
        public void AllSettingsValid_ValidationOk()
        {
            var settings = new DataPipelineSettings(true, true, 30, true,
                new List<PublishedDashboardSettings>() { new PublishedDashboardSettings(Guid.NewGuid(), "dashboard1", IntervalOKInSeconds, DateTime.UtcNow) }, true, "cred",false, 1433);

            settings.Validate();
        }

        [Test]
        public void AllSettingsValid_NoPublishedDashboardSettings_ValidationOk()
        {
            var settings = new DataPipelineSettings(true, true, 30, true, new List<PublishedDashboardSettings>(), true, "cred",false, 1433);

            settings.Validate();
        }

        [Test]
        public void NoSessionLogOutputSpecified_ArgumentOutOfRangeExceptionThrown()
        {
            var settings = new DataPipelineSettings(false, false, 30, true,
                new List<PublishedDashboardSettings>() { new PublishedDashboardSettings(Guid.NewGuid(), "dashboard1", IntervalOKInSeconds, DateTime.UtcNow) }, true, "cred",false, 1433);

            Assert.Throws<ArgumentOutOfRangeException>(() => settings.Validate());
        }

        [Test]
        public void PublishDashboardIntervalBelowMininumAllowed_ArgumentOutOfRangeExceptionThrown()
        {
            var settings = new DataPipelineSettings(true, true, 30, true,
                new List<PublishedDashboardSettings>() { new PublishedDashboardSettings(Guid.NewGuid(), "dashboard1", IntervalBelowMinAllowedInSeconds, DateTime.UtcNow ) }, true, "cred",false, 1433);

            Assert.Throws<ArgumentOutOfRangeException>(() => settings.Validate());
        }

        [Test]
        public void PublishDashboardIntervalAboveMaximumAllowed_ArgumentOutOfRangeExceptionThrown()
        {
            var settings = new DataPipelineSettings(true, true, 30, true,
                new List<PublishedDashboardSettings>() { new PublishedDashboardSettings(Guid.NewGuid(), "dashboard1", IntervalAboveMaxAllowedInSeconds, DateTime.UtcNow ) }, true, "cred",false, 1433);

            Assert.Throws<ArgumentOutOfRangeException>(() => settings.Validate());
        }

        [Test]
        public void InvalidPortSpecified_ArgumentOutOfRangeExceptionThrown()
        {
            var settings = new DataPipelineSettings(true, true, 30, true,
                new List<PublishedDashboardSettings>() { new PublishedDashboardSettings(Guid.NewGuid(), "dashboard1", IntervalOKInSeconds, DateTime.UtcNow) }, true, "cred", false, 0);

            Assert.Throws<ArgumentOutOfRangeException>(() => settings.Validate());
        }
    }
}
