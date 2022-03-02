using System;
using System.Threading.Tasks;

namespace BluePrism.DigitalWorker.Sessions
{
    public interface ISessionRunner
    {
        Task RunAsync(SessionContext context);
    }
}