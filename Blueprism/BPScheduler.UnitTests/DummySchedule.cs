#if UNITTESTS
using System;
using System.Collections.Generic;
using System.Threading;
using BluePrism.BPCoreLib.Collections;
using BluePrism.Scheduling;

namespace BPScheduler.UnitTests
{
    public class DummySchedule : BaseSchedule
    {
        private bool _aborted;

        public DummySchedule(IScheduler owner) : base(owner) { }

        public override void Execute(IScheduleLog log)
        {
            _aborted = false;

            Random rand = new Random();
            log.Start();
            Thread.Sleep(rand.Next(1, 5));
            if (_aborted)
            {
                log.Terminate("Schedule aborted");
            }
            else
            {
                if (rand.NextDouble() < 0.3)
                {
                    log.Terminate("Just the luck of the draw...");
                }
                else
                {
                    log.Complete();
                }
            }
        }

        public override void Abort(string reason)
        {
            _aborted = true;
        }

        public override TriggerMisfireAction Misfire(ITriggerInstance instance, TriggerMisfireReason reason)
        {
            return TriggerMisfireAction.None;
        }

        public override ICollection<IScheduleLog> GetRunningInstances()
        {
            return GetEmpty.ICollection<IScheduleLog>();
        }
    }
}

#endif