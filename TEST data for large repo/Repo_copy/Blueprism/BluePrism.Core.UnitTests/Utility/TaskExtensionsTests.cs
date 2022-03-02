using System;
using System.Threading;
using System.Threading.Tasks;
using BluePrism.Core.Utility;
using FluentAssertions;
using NUnit.Framework;

namespace BluePrism.Core.UnitTests.Utility
{
    public class TaskExtensionsTests
    {
        [Test]
        public void Await_ShouldReturnWhenTaskComplete()
        {
            var taskSource = new TaskCompletionSource<bool>();
            Task task = taskSource.Task;

            var monitoringTask = Task.Run(() =>
            {
                task.Await();
                return Task.CompletedTask;
            });
            monitoringTask.IsCompleted.Should().BeFalse();
            taskSource.SetResult(true);
            monitoringTask.Wait(250);
            monitoringTask.IsCompleted.Should().BeTrue();
        }
        
        [Test]
        public void Await_ExceptionThrown_ShouldUnwrapException()
        {
            var task = Task.Run(() => throw new TestException());

            try
            {
                task.Await();
            }
            catch (Exception exception)
            {
                exception.Should().BeOfType<TestException>();
            }
        }

        [Test]
        public void AwaitWithResult_ShouldReturnResultWhenTaskComplete()
        {
            var taskSource = new TaskCompletionSource<int>();
            var task = taskSource.Task;

            var monitoringTask = Task.Run(() =>
            {
                int result = task.Await();
                return Task.FromResult(result);
            });
            monitoringTask.IsCompleted.Should().BeFalse();
            taskSource.SetResult(2);
            monitoringTask.Wait(250);
            monitoringTask.Result.Should().Be(2);
        }

        [Test]
        public void AwaitWithResult_ExceptionThrown_ShouldUnwrapException()
        {
            int TestFunc() => throw new TestException();

            var task = Task.Run((Func<int>) TestFunc);

            try
            {
                int result = task.Await();
            }
            catch (Exception exception)
            {
                exception.Should().BeOfType<TestException>();
            }
        }

        [Test]
        public async Task WithCancellation_ShouldComplete()
        {
            const int TestValue = 100;
            var cs = new CancellationTokenSource();
            var task = Task.Run(() =>
            {
                return TestValue;
            });
            Assert.AreEqual(await task.WithCancellation(cs.Token), TestValue);
        }

        [Test]
        public async Task WithCancellation_ShouldCancel()
        {
            const int TestValue = 100;
            var cs = new CancellationTokenSource();
            var task = Task.Run(async () =>
            {
                await Task.Delay(5000);
                return TestValue;
            }, cs.Token);

            _ = Task.Run(() =>
            {
                Thread.Sleep(50);
                cs.Cancel();
            });

            var x = 0;
            try
            {
                x = await task.WithCancellation(cs.Token);
            }
            catch (OperationCanceledException)
            {
                Assert.IsFalse(task.IsCompleted);
                Assert.AreNotEqual(x, TestValue);
            }
        }

        public class TestException : Exception
        {

        }
    }
}
