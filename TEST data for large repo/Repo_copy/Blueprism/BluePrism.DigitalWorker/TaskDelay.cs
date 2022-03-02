using System;
using System.Threading.Tasks;

namespace BluePrism.DigitalWorker
{
    public class TaskDelay : ITaskDelay
    {
        public Task Delay(TimeSpan delay) => Task.Delay(delay);
        
    }
}
