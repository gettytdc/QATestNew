using System;
using System.Threading;
using System.Threading.Tasks;
using BluePrism.Cirrus.Sessions.SessionService.Messages.Commands;
using BluePrism.Cirrus.Sessions.SessionService.Messages.Commands.Factory;
using MassTransit;

namespace BluePrism.DigitalWorker.Messaging
{
    public class SessionServiceClient : ISessionServiceClient
    {
        private readonly IRequestClient<RegisterDigitalWorker> _digitalWorkerRequestClient;
        private readonly IRequestClient<RequestStartProcess> _startProcessRequestClient;

        private static readonly TimeSpan RegisterDigitalWorkerTimeout = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan RequestStartProcessTimeout = TimeSpan.FromMinutes(10);    

        public SessionServiceClient(IRequestClient<RegisterDigitalWorker> digitalWorkerRequestClient,
            IRequestClient<RequestStartProcess> startProcessRequestClient)
        {
            _digitalWorkerRequestClient = digitalWorkerRequestClient;
            _startProcessRequestClient = startProcessRequestClient;
        }

        public async Task<RegisteredStatus> RegisterDigitalWorker(string name)
        {
            var request = SessionServiceCommands.RegisterDigitalWorker(name);
            var response = await _digitalWorkerRequestClient.GetResponse<RegisterDigitalWorkerResponse>(request, default(CancellationToken), RequestTimeout.After(s: (int)RegisterDigitalWorkerTimeout.TotalSeconds))
                .ConfigureAwait(false);

            return response.Message.Status;
        }

        public async Task<StartProcessStatus> RequestStartProcess(Guid sessionId, string digitalWorkerName)
        {
            var request = SessionServiceCommands.RequestStartProcess(sessionId, digitalWorkerName);
            var response = await _startProcessRequestClient.GetResponse<RequestStartProcessResponse>(request, default(CancellationToken), RequestTimeout.After(s: (int)RequestStartProcessTimeout.TotalSeconds))
                .ConfigureAwait(false);

            return response.Message.Status;
        }
    }
}
