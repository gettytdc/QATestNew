using System;
using System.Threading;
using System.Threading.Tasks;

namespace BluePrism.Core.Utility
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Waits for the task to complete. If an exception is thrown, then the AggregateException
        /// thrown by the task will be unwrapped and the  underlying exception thrown, matching
        /// the behaviour of when Tasks are executed in an asynchronous context using the await
        /// keyword. Suitable for handling a Task returned by an asynchronous method in a
        /// synchronous context, such as an event handler in the UI.
        /// </summary>
        /// <param name="task">The task to await</param>
        public static void Await(this Task task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));
            task.GetAwaiter().GetResult();
        }

        /// <summary>
        /// Waits for the task to complete and returns the result. If an exception is thrown,
        /// then the AggregateException thrown by the task will be unwrapped and the
        /// underlying exception thrown, which matches the behaviour of when Tasks are executed
        /// in an asynchronous context using the await keyword. Suitable for handling a Task
        /// returned by an asynchronous method in a synchronous context, such as an event handler
        /// in the UI.
        /// </summary>
        /// <param name="task">The task to await</param>
        public static T Await<T>(this Task<T> task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));
            return task.GetAwaiter().GetResult();
        }

        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), taskCompletionSource))
            {
                if (task != await Task.WhenAny(task, taskCompletionSource.Task))
                {
                    throw new OperationCanceledException(cancellationToken);
                }
                return await task;
            }
        }
    }
}
