using System;
using System.Resources;
using BluePrism.Core.Utility;

namespace BluePrism.Data.DataModels.WorkQueueAnalysis
{
    public enum SnapshotInterval
    {
        FifteenMinutes = 0,
        ThirtyMinutes = 1,
        OneHour = 2,
        TwoHours = 3,
        SixHours = 4,
        TwelveHours = 5,
        TwentyFourHours = 6
    }

    public static class SnapshotIntervalExtensions
    {
        public const string ResourceTemplate = "SnapshotInterval_{0}";

        public static int AsMinutes(this SnapshotInterval interval)
        {
            switch (interval)
            {
                case SnapshotInterval.FifteenMinutes:  return 15;
                case SnapshotInterval.ThirtyMinutes : return 30;
                case SnapshotInterval.OneHour : return 60;
                case SnapshotInterval.TwoHours: return 2 * 60;
                case SnapshotInterval.SixHours: return 6 * 60;
                case SnapshotInterval.TwelveHours : return 12 * 60;
                case SnapshotInterval.TwentyFourHours: return 24 * 60;
                default: throw new ArgumentOutOfRangeException(nameof(interval));
            }
        }

        public static string ToLocalizedString(this SnapshotInterval interval, ResourceManager resourceManager)
        {
            return resourceManager.EnsureString(ResourceTemplate, interval);
        }
    }

 }