using System;
using System.Threading.Tasks;

namespace BluePrism.DigitalWorker
{
    public interface ITaskDelay
    {
        Task Delay(TimeSpan delay);
    }
}
