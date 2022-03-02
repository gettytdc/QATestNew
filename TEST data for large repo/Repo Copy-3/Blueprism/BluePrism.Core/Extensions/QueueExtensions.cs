using System;
using System.Collections.Generic;

namespace BluePrism.Core.Extensions
{
    public static class QueueExtensions
    {
        /// <summary>
        /// Gets the next item off the queue or default if the queue is empty
        /// </summary>
        public static T DequeueOrDefault<T>(this Queue<T> queue)
        {
            if (queue == null)
                throw new ArgumentNullException(nameof(queue));
            return queue.Count == 0
                ? default(T)
                : queue.Dequeue();
        }
    }
}
