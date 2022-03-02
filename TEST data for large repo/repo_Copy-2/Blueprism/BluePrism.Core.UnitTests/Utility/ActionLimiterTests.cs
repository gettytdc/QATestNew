#if UNITTESTS

using BluePrism.Core.Utility;
using NUnit.Framework;
using System;

namespace BluePrism.Core.UnitTests.Utility
{
    public class ActionLimiterTests
    {

        [Test]
        public void WhenCreatingShouldSetIntervalOnTimer()
        {
            var limiter = new ActionLimiter(TimeSpan.FromMilliseconds(250));
            Assert.That(limiter.Timer.Interval, Is.EqualTo(TimeSpan.FromMilliseconds(250)));
        }

        [Test]
        public void TriggerShouldExecuteActionWhenTimerElapsed()
        {
            bool executeRaised = false;
            var limiter = new ActionLimiter(TimeSpan.FromMilliseconds(250));
            limiter.Execute += (sender, e) => { executeRaised = true; };

            limiter.Timer.TriggerElapsed();

            Assert.That(executeRaised, Is.True);
        }

        [Test]
        public void TriggerShouldNotExecuteActionUntilTimerElapsed()
        {
            bool executeRaised = false;
            var limiter = new ActionLimiter(TimeSpan.FromMilliseconds(250));
            limiter.Execute += (sender, e) => { executeRaised = true; };
            

            Assert.That(executeRaised, Is.False);

        }

        [Test]
        public void FirstTriggerShouldStartTimer()
        {
            bool timerStarted = false;
            var limiter = new ActionLimiter(TimeSpan.FromMilliseconds(999));
            
            limiter.Timer.Started += (sender, e) => { timerStarted = true; };
            limiter.Trigger();
            
            Assert.That(timerStarted, Is.True);

        }

        [Test]
        public void SecondTriggerShouldNotRestartTimerImmediately()
        {
            bool timerStarted = false;
            var limiter = new ActionLimiter(TimeSpan.FromMilliseconds(999));

            limiter.Trigger();
            limiter.Timer.Started += (sender, e) => { timerStarted = true; };
            limiter.Trigger();

            Assert.That(timerStarted, Is.False);

        }


        [Test]
        public void TwoTriggersBeforeTimerElapsesShouldRestartTimerAndNotExecuteAction()
        {
            bool timerStarted = false;
            bool executeRaised = false;
            var limiter = new ActionLimiter( TimeSpan.FromMilliseconds(100));
            limiter.Execute += (sender, e) => { executeRaised = true; };

            limiter.Trigger();
            limiter.Timer.Started += (sender, e) => { timerStarted = true; };
            limiter.Trigger();
            limiter.Timer.TriggerElapsed();
                    
            
            Assert.That(timerStarted, Is.True);
            Assert.That(executeRaised, Is.False);

        }

        [Test]
        public void NoTriggersAfterTimerElapseShouldExecuteActionAndNotRestartTimer()
        {
            bool timerStarted = false;
            bool executeRaised = false;
            var limiter = new ActionLimiter(TimeSpan.FromMilliseconds(100));
            limiter.Execute += (sender, e) => { executeRaised = true; };

            limiter.Trigger();
            limiter.Trigger();
            limiter.Timer.TriggerElapsed();
            limiter.Timer.Started += (sender, e) => { timerStarted = true; };
            limiter.Timer.TriggerElapsed();
            

            Assert.That(timerStarted, Is.False);
            Assert.That(executeRaised, Is.True);

        }

    }
}

#endif
