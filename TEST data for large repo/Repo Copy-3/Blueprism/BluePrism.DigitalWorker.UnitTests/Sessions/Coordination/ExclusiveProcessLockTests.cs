using BluePrism.DigitalWorker.Sessions.Coordination;
using FluentAssertions;
using NUnit.Framework;

namespace BluePrism.DigitalWorker.UnitTests.Sessions.Coordination
{
    public class ExclusiveProcessLockTests
    {
        [Test]
        public void Initialise_ShouldCreateWithUnlockedState()
        {
            var @lock = new ExclusiveProcessLock();

            @lock.State.Should().Be(ExclusiveProcessLockState.Unlocked);
        }

        [Test]
        public void Lock_WhenUnlocked_ShouldUpdateState()
        {
            var @lock = new ExclusiveProcessLock();

            @lock.Lock();
            @lock.State.Should().Be(ExclusiveProcessLockState.Locked);
        }

        [Test]
        public void Lock_WhenLocked_ShouldRetainState()
        {
            var @lock = new ExclusiveProcessLock();
            @lock.Lock();
            @lock.Lock();

            @lock.State.Should().Be(ExclusiveProcessLockState.Locked);
        }

        [Test]
        public void Unlock_WhenLocked_ShouldUpdateState()
        {
            var @lock = new ExclusiveProcessLock();
            @lock.Lock();

            @lock.Unlock();
            @lock.State.Should().Be(ExclusiveProcessLockState.Unlocked);
        }

        [Test]
        public void Unlock_WhenUnlocked_ShouldRetainState()
        {
            var @lock = new ExclusiveProcessLock();
            @lock.Unlock();

            @lock.Unlock();

            @lock.State.Should().Be(ExclusiveProcessLockState.Unlocked);
        }

        [Test]
        public void Wait_WhenLocked_ShouldWait()
        {
            var @lock = new ExclusiveProcessLock();
            @lock.Lock();

            var task = @lock.Wait(new System.Threading.CancellationToken(false));
            task.IsCompleted.Should().BeFalse();
        }

        [Test]
        public void Wait_WhenUnlocked_ShouldNotWait()
        {
            var @lock = new ExclusiveProcessLock();
            @lock.Unlock();

            var task = @lock.Wait(new System.Threading.CancellationToken(false));
            task.IsCompleted.Should().BeTrue();
        }
    }
}