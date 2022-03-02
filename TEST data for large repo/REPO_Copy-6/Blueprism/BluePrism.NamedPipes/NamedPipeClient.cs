using System;
using System.IO.Pipes;
using System.Security.Principal;
using System.Threading;
using BluePrism.NamedPipes.Responses;
using Newtonsoft.Json;
using NLog;

namespace BluePrism.NamedPipes
{
    public class NamedPipeClient : IDisposable
    {
        public string ServerName { get; }
        public Guid ClientId { get; }
        public bool IsDisposing { get; private set; }
        public event MessageReceivedDelegate MessageReceived;
        public event PipeDisposedDelegate PipeDisposed;

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private NamedPipeClientStream _pipe;

        private const int PipeTimeout = 15000;
        public NamedPipeClient(string serverName)
        {
            ServerName = serverName;
            ClientId = Guid.NewGuid();
        }

        public void SendMessage(object message)
        {
            if (_pipe.IsConnected)
            {
                var serverCommunication = new ServerCommunication(_pipe);
                serverCommunication.SendMessage(JsonConvert.SerializeObject(message));
            }
        }

        public bool IsConnected() => _pipe.IsConnected;
        public bool Connect()
        {
            var response = string.Empty;
            using (_pipe = new NamedPipeClientStream(".", ServerName, PipeDirection.InOut, PipeOptions.Asynchronous, TokenImpersonationLevel.Impersonation))
            {
                if (ConnectPipe(_pipe, PipeTimeout))
                {
                    using (var serverCommunication = new ServerCommunication(_pipe))
                    {
                        var initialConnection = new NamedPipeConnectionMessage(ClientId.ToString());
                        serverCommunication.SendMessage(JsonConvert.SerializeObject(initialConnection));
                        response = serverCommunication.ReadMessage();
                    }
                }
            }

            if (!response.Contains("PipeId"))
            { return false; }

            var responseObject = JsonConvert.DeserializeObject<ConnectionResponse>(response);

            var pipeId = responseObject.PipeId;

            if (!string.IsNullOrEmpty(pipeId))
            {
                _pipe = new NamedPipeClientStream(".", pipeId, PipeDirection.InOut, PipeOptions.Asynchronous, TokenImpersonationLevel.Impersonation);

                if (ConnectPipe(_pipe, PipeTimeout))
                {
                    return true;
                }
            }

            return false;
        }

        public void Listen()
        {
            if (_pipe.IsConnected)
            {
                var t = new Thread(() => MessagingPipeListen(_pipe));
                t.Start();
            }
        }

        private bool ConnectPipe(NamedPipeClientStream pipe, int timeout = PipeTimeout)
        {
            var connectionSuccessful = false;
            try
            {
                pipe.Connect(timeout);
                var serverCommunication = new ServerCommunication(pipe);

                if (serverCommunication.ReadMessage() == ServerName)
                {
                    connectionSuccessful = true;
                }
            }
            catch(Exception ex)
            {
                Log.Error($"Error connecting to pipe: {ex.ToString()}");
            }
            return connectionSuccessful;
        }

        private void MessagingPipeListen(PipeStream connectedPipe)
        {
            try
            {
                using (connectedPipe)
                {
                    using (var serverCommunication = new ServerCommunication(connectedPipe))
                    {
                        while (connectedPipe.IsConnected)
                        {
                            var responseObject = serverCommunication.ReadMessageAsJObject();
                            if (responseObject != null)
                            {
                                MessageReceived?.Invoke(this, new MessageReceivedDelegateEventArgs(responseObject));
                            }
                        }
                    }
                }

                Dispose();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _pipe.Dispose();
                Log.Info("Disposed");
                PipeDisposed?.Invoke(this, new PipeDisposedDelegateEventArgs(ClientId));
            }
        }
    }
}
