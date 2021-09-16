using System;
using System.Threading.Tasks;
using BluePrism.Cirrus.Sessions.SessionService.Messages.Commands;

namespace BluePrism.DigitalWorker.Messaging
{
    public interface ISessionServiceClient
    {
        Task<RegisteredStatus> RegisterDigitalWorker(string name);

        Task<StartProcessStatus> RequestStartProcess(Guid sessionId, string digitalWorkerName);
    }
}
