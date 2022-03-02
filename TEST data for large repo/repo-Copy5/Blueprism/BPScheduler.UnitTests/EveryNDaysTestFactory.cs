using BluePrism.Scheduling;
using BluePrism.Scheduling.Triggers;
using Moq;
using System;

namespace BPScheduler.UnitTests
{
    public static class EveryNDaysTestFactory
    {
        public static readonly DateTime SnapshotTestTime = DateTime.Now;

        public static EveryNDays Create(int days = 1,
                                        int addDays = 0,
                                        int priority = 0,
                                        TriggerMode triggerMode = TriggerMode.Fire,
                                        bool userTrigger = false,
                                        bool mockSchedule = false)
        {
            return new EveryNDays(days)
            {
                Start = SnapshotTestTime.AddDays(addDays),
                Priority = priority,
                Mode = triggerMode,
                IsUserTrigger = userTrigger,
                Schedule = mockSchedule ? new Mock<ISchedule>().Object : null
            };
        }
    }
}
