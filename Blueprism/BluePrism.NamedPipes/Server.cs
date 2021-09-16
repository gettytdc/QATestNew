using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Threading;
using BluePrism.NamedPipes.Exceptions;
using BluePrism.NamedPipes.Interfaces;
using BluePrism.NamedPipes.Responses;
using BluePrism.Server.Domain.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace BluePrism.NamedPipes
{

    public class Server : IServer
    {
        public string ServerName { get; }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly IDictionary<string, NamedPipeServerStream> NamedServerPipes = new ConcurrentDictionary<string, NamedPipeServerStream>();

        private static readonly IDictionary<SafeHandle, ManualResetEvent> ProcessFinishedEvents = new ConcurrentDictionary<SafeHandle, ManualResetEvent>();

        private NamedPipeServerStream _connectionListener;

        private Action<ServerCommunication, string> MessageHandler { get; }

        private const int ThreadSleepInterval = 100;

        private volatile bool _isDisposed;

        private volatile bool _isDisposing;

        private bool IsRunning => !_isDisposed && !_isDisposing;

        public Server(string serverName, Action<ServerCommunication, string> messageHandler)
        {
            if (string.IsNullOrWhiteSpace(serverName))
            {
                throw new ArgumentNullException(nameof(serverName));
            }

            ServerName = serverName;
            MessageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
            StartServerThread(serverName);
        }

        public void SendMessageAsClient(object messageObject, string pipeName)
        {
            if (NamedServerPipes.TryGetValue(pipeName, out var pipeServer))
            {
                var message = JsonConvert.SerializeObject(messageObject);
                SendMessageAsClient(message, pipeServer);
            }
            else
            {
                var exception = new BluePrismException("Pipe not found");
                Log.Error(exception);
                throw exception;
            }
        }

        public void SendToMultiplePipes(object messageObject, Expression<Func<KeyValuePair<string, NamedPipeServerStream>, bool>> filter)
        {
            var message = JsonConvert.SerializeObject(messageObject);
            var queryablePipeDictionary = NamedServerPipes.AsQueryable();
            foreach (var pipe in queryablePipeDictionary.Where(filter))
            {
                if (pipe.Value.IsConnected)
                {
                    SendMessageAsClient(message, pipe.Value);
                }
            }
        }

        public void SendToAllPipes(object messageObject)
        {
            SendToMultiplePipes(messageObject, x => true);
        }

        private void StartServerThread(string serverName)
        {
            if (IsRunning)
            {
                var serverThread = new Thread(() => ServerThread(serverName));
                serverThread.Start();
            }
        }

        private void ServerThread(string listenerName)
        {
            try
            {
                using (var pipeServer = new NamedPipeServerStream(listenerName, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous))
                {
                    var processFinishedEvent = new ManualResetEvent(false);

                    ProcessFinishedEvents.Add(pipeServer.SafePipeHandle, processFinishedEvent);

                    // Add the pipeServer to the collection of bp pipe servers
                    // it can be retrieved by its name later
                    if (listenerName != ServerName && !NamedServerPipes.ContainsKey(listenerName))
                    {
                        NamedServerPipes.Add(listenerName, pipeServer);
                    }
                    else if (listenerName == ServerName)
                    {
                        _connectionListener = pipeServer;
                    }

                    while (IsRunning)
                    {
                        // Wait for a client to connect                    
                        if (listenerName == ServerName)
                        {
                            pipeServer.BeginWaitForConnection(ProcessConnectionAttempt, pipeServer);
                        }
                        else
                        {
                            pipeServer.BeginWaitForConnection(ProcessCommand, pipeServer);
                        }

                        _ = WaitHandle.WaitAny(
                            new WaitHandle[]
                            {
                                processFinishedEvent
                            });
                        processFinishedEvent.Reset();
                    }

                    ClearPipe(pipeServer, listenerName);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private void ClearPipe(NamedPipeServerStream pipeServer, string listenerName)
        {
            ProcessFinishedEvents.Remove(pipeServer.SafePipeHandle);
            pipeServer.Close();
            if (listenerName == ServerName)
            {
                _connectionListener = null;
            }
            else
            {
                NamedServerPipes.Remove(listenerName);
            }
        }

        private void ProcessConnectionAttempt(IAsyncResult result)
        {
            NamedPipeServerStream pipeServer = null;
            try
            {
                pipeServer = result.AsyncState as NamedPipeServerStream;
                if (pipeServer == null)
                {
                    return;
                }

                pipeServer.EndWaitForConnection(result);
                // Read the request from the client. Once the client has
                // written to the pipe its security token will be available.

                var serverCommunication = new ServerCommunication(pipeServer);

                serverCommunication.SendMessage(ServerName);
                var command = serverCommunication.ReadMessage();

                var jsonCommand = JsonConvert.DeserializeObject<JObject>(command);
                ConnectionResponse response;
                //We have a command, do something based on the command
                switch (jsonCommand["Name"]?.ToString())
                {
                    case "Connect":
                        response = ProcessConnectRequest(jsonCommand);
                        break;

                    default:
                        response = new ConnectionResponse("invalid command");
                        break;
                }

                SendMessageAsClient(response, serverCommunication, pipeServer);
            }
            catch (ObjectDisposedException e)
            {
                // Pipe Disposed, we can swallow this exception as its expected
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw new BluePrismException(e, "Exception while processing connection attempt");
            }
            finally
            {
                if (pipeServer?.IsConnected == true)
                {
                    pipeServer.Disconnect();
                    if (ProcessFinishedEvents.TryGetValue(pipeServer.SafePipeHandle, out var processFinishedEvent))
                    {
                        processFinishedEvent.Set();
                    }
                }
            }
        }

        private ConnectionResponse ProcessConnectRequest(JObject jsonCommand)
        {
            var clientId = jsonCommand["BpClientId"]?.ToString();
            var pipeId = $"{ServerName}-{clientId}";
            if (!NamedServerPipes.ContainsKey(pipeId))
            {
                StartServerThread(pipeId);
            }

            return new ConnectionResponse(pipeId);
        }

        private void ProcessCommand(IAsyncResult result)
        {
            NamedPipeServerStream pipeServer = null;
            try
            {
                pipeServer = result.AsyncState as NamedPipeServerStream;
                if (pipeServer == null)
                {
                    return;
                }
                pipeServer.EndWaitForConnection(result);

                var clientId = NamedServerPipes.FirstOrDefault(x => x.Value == pipeServer).Key;
                if (string.IsNullOrEmpty(clientId))
                {
                    throw new BluePrismNamedPipeServerException("Pipe Server does not correspond to a registered client");
                }

                // Read the request from the client. Once the client has
                // written to the pipe its security token will be available.

                var serverCommunication = new ServerCommunication(pipeServer);
                serverCommunication.SendMessage(ServerName);

                MessageHandler(serverCommunication, clientId);
            }
            catch (ObjectDisposedException e)
            {
                Log.Error(e);
                // Caused during disposal
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
            finally
            {
                if (pipeServer?.IsConnected == true)
                {
                    pipeServer.Disconnect();
                    if (ProcessFinishedEvents.TryGetValue(pipeServer.SafePipeHandle, out var resetEvent))
                    {
                        resetEvent.Set();
                    }
                }
            }
        }

        private void SendMessageAsClient(object messageObject, ServerCommunication serverCommunication, NamedPipeServerStream pipeServer)
        {
            var message = JsonConvert.SerializeObject(messageObject);
            SendMessageAsClient(message, serverCommunication, pipeServer);
        }

        private void SendMessageAsClient(string message, ServerCommunication serverCommunication, NamedPipeServerStream pipeServer)
        {
            if (pipeServer.IsConnected)
            {
                pipeServer.RunAsClient(() => serverCommunication.SendMessage(message));
            }
            else
            {
                var exception = new BluePrismException("Pipe Not Connected");
                Log.Error(exception);
                throw exception;
            }

        }

        private void SendMessageAsClient(string message, NamedPipeServerStream pipeServer)
        {
            var serverCommunication = new ServerCommunication(pipeServer);
            SendMessageAsClient(message, serverCommunication, pipeServer);
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            Log.Info("Disposing");
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _isDisposing = true;
                    ProcessFinishedEvents.Values.ToList().ForEach(r => r.Set());
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    while (NamedServerPipes.Any() && _connectionListener != null && stopwatch.Elapsed.TotalSeconds < 5)
                    {
                        Thread.Sleep(ThreadSleepInterval);
                        //Waiting for pipes to dispose
                    }
                }

                ProcessFinishedEvents.Clear();
                _isDisposed = true;
            }
        }

        public virtual void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
