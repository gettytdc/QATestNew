using System;
using System.Threading;
using System.Threading.Tasks;
using BluePrism.ClientServerResources.Core.Events;
using BluePrism.ClientServerResources.Core.Exceptions;
using BluePrism.ClientServerResources.Grpc.Events;
using Grpc.Core;
using InstructionalConnection;
using NLog;

namespace BluePrism.ClientServerResources.Grpc
{
    public class RegisterClientCallback
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly int _keepAliveTimeMS;
        private readonly int _keepAliveTimeoutMS;
        private IAsyncStreamReader<RegisterClientResponse> _responseStream;
        private IClientStreamWriter<RegisterClientRequest> _requestStream;
        private CancellationTokenSource _cancellationToken;

        public event EventHandler<ResourcesChangedEventArgs> ResourceStatus;
        public event EventHandler<SessionCreateEventArgs> SessionCreated;
        public event EventHandler<SessionDeleteEventArgs> SessionDeleted;
        public event EventHandler<SessionEndEventArgs> SessionEnd;
        public event EventHandler<SessionStartEventArgs> SessionStarted;
        public event EventHandler<SessionStopEventArgs> SessionStop;
        public event EventHandler<FailedCallbackOperationEventArgs> ErrorReceived;
        public event EventHandler<SessionVariableUpdatedEventArgs> SessionVariableUpdated;
        private event EventHandler<GetStatusResponseEventArgs> PingRecv;

        public RegisterClientCallback(int keepAliveMs, int keepAliveTimeoutMS)
        {
            if (keepAliveMs <= 0)        throw new ArgumentException($"{nameof(keepAliveMs)} cannot be less than zero");
            if (keepAliveTimeoutMS <= 0) throw new ArgumentException($"{nameof(keepAliveTimeoutMS)} cannot be less than zero");

            _keepAliveTimeMS = keepAliveMs;
            _keepAliveTimeoutMS = keepAliveTimeoutMS;
        }

        public void Attach(
            IAsyncStreamReader<RegisterClientResponse> responseStream,
            IClientStreamWriter<RegisterClientRequest> requestStream,
            CancellationToken token)
        {
            _responseStream = responseStream ?? throw new ArgumentNullException(nameof(responseStream));
            _requestStream = requestStream ?? throw new ArgumentNullException(nameof(requestStream));
            _cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(token);
        }        

        protected async Task ListenTask()
        {
            if (_responseStream is null)
            {
                throw new InvalidProgramException("Streams must be attached.");
            }

            while (await _responseStream.MoveNext(_cancellationToken.Token).ConfigureAwait(false))
            {
                try
                {
                    var current = _responseStream.Current;
                    var data = current.Message;

                    if (data.Is(ResourcesChangedDataMessage.Descriptor))
                    {
                        var resourcesChangedDataMessage = data.Unpack<ResourcesChangedDataMessage>();
                        ResourceStatus?.BeginInvoke(this, resourcesChangedDataMessage.ToArgs(),
                            HandleEndInvoke<ResourcesChangedEventArgs>(ResourceStatus, nameof(ResourceStatus)), null);
                    }
                    else if (data.Is(SessionCreatedDataMessage.Descriptor))
                    {
                        var sessionCreatedDataMessage = data.Unpack<SessionCreatedDataMessage>();
                        SessionCreated?.BeginInvoke(this, sessionCreatedDataMessage.ToArgs(),
                            HandleEndInvoke<SessionCreateEventArgs>(SessionCreated, nameof(SessionCreated)), null);
                    }
                    else if (data.Is(SessionDeletedDataMessage.Descriptor))
                    {
                        var sessionDeletedDataMessage = data.Unpack<SessionDeletedDataMessage>();
                        SessionDeleted?.BeginInvoke(this, sessionDeletedDataMessage.ToArgs(),
                            HandleEndInvoke<SessionDeleteEventArgs>(SessionDeleted, nameof(SessionDeleted)), null);
                    }
                    else if (data.Is(SessionEndDataMessage.Descriptor))
                    {
                        var sessionEndDataMessage = data.Unpack<SessionEndDataMessage>();
                        SessionEnd.BeginInvoke(this, sessionEndDataMessage.ToArgs(),
                            HandleEndInvoke<SessionEndEventArgs>(SessionEnd, nameof(SessionEnd)), null);
                    }
                    else if (data.Is(SessionStartedDataMessage.Descriptor))
                    {
                        var sessionStartedDataMessage = data.Unpack<SessionStartedDataMessage>();
                        SessionStarted?.BeginInvoke(this, sessionStartedDataMessage.ToArgs(),
                            HandleEndInvoke<SessionStartEventArgs>(SessionStarted, nameof(SessionStarted)), null);
                    }
                    else if (data.Is(SessionStopDataMessage.Descriptor))
                    {
                        var sessionStopDataMessage = data.Unpack<SessionStopDataMessage>();
                        SessionStop?.BeginInvoke(this, sessionStopDataMessage.ToArgs(),
                            HandleEndInvoke<SessionStopEventArgs>(SessionStop, nameof(SessionStop)), null);
                    }
                    else if (data.Is(SessionVariablesUpdatedDataMessage.Descriptor))
                    {
                        var sessionVariablesUpdatedDataMessage = data.Unpack<SessionVariablesUpdatedDataMessage>();
                        SessionVariableUpdated?.BeginInvoke(this, sessionVariablesUpdatedDataMessage.ToArgs(),
                            HandleEndInvoke<SessionVariableUpdatedEventArgs>(SessionVariableUpdated, nameof(SessionVariableUpdated)), null);
                    }
                    else if (data.Is(GetStatusResponse.Descriptor))
                    {
                        var statusResponse = data.Unpack<GetStatusResponse>();
                        PingRecv?.Invoke(this, new GetStatusResponseEventArgs(statusResponse));
                    }
                    else if (!current.Success)
                    {
                        var message = string.Empty;
                        var errorMsg = string.Empty;
                        // we would expect a failed response to hold a FailedOperationMessage
                        if (current.Message.Is(FailedOperationMessage.Descriptor))
                        {
                            var failedOpMessage = current.Message.Unpack<FailedOperationMessage>();
                            message = failedOpMessage.Message;
                            errorMsg = failedOpMessage.ErrMsg;
                        }
                        else
                        {
                            Log.Warn($"unknown Any type error received. typeof - {current.Message.GetType()}");
                            message = "Unknown error received";
                            errorMsg = current.Error;
                        }

                        ErrorReceived?.BeginInvoke(this, new FailedCallbackOperationEventArgs(message, errorMsg),
                            HandleEndInvoke<FailedCallbackOperationEventArgs>(ErrorReceived, nameof(ErrorReceived)), null);
                    }
                }
                catch (Exception ex)
                {
                    Log.Info(ex, "gRPC callback client listener errored.");
                    throw;
                }
            }
        }

        protected async Task StreamHeartbeat()
        {
            if (_requestStream is null)
            {
                throw new InvalidProgramException("Streams must be attached.");
            }

            using (var reset = new AutoResetEvent(false))
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    GetStatusResponseEventArgs recvArgs = null;
                    void handler(object s, GetStatusResponseEventArgs e)
                    {
                        recvArgs = e;
                        reset.Set();
                    }

                    PingRecv += handler;

                    try
                    {
                        await _requestStream.WriteAsync(new RegisterClientRequest()
                        {
                            Message = Google.Protobuf.WellKnownTypes.Any.Pack(new GetStatusRequest())
                        });
                    }
                    catch (Exception ex)
                    {
                        Log.Debug(ex, "Failed to write gRpc hearbeat message");
                        throw;
                    }

                    reset.WaitOne(TimeSpan.FromMilliseconds(_keepAliveTimeoutMS));

                    if (recvArgs == null)
                    {
                        _cancellationToken.Cancel();
                        Log.Info("gRPC client heartbeat received no response. closing down.");
                    }
                    PingRecv -= handler;
                    await Task.Delay(TimeSpan.FromMilliseconds(_keepAliveTimeMS), _cancellationToken.Token);
                }
            }
        }

        private AsyncCallback HandleEndInvoke<T>(EventHandler<T> handler, string handlerName)
        {
            return new AsyncCallback((IAsyncResult ar) =>
            {
                try
                {
                    handler?.EndInvoke(ar);
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, $"Exception calling {handlerName} Callback Event");
                }
            });
        }

        /// <summary>
        /// Send token session id and validation token
        /// to the attached server method <see cref="Services.InstructionalConnectionServiceImpl.RegisterClient(IAsyncStreamReader{RegisterClientRequest}, IServerStreamWriter{RegisterClientResponse}, ServerCallContext)"/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        internal async Task<RegisterClientResponse> HandshakeAsync(Guid id, string token)
        {
            if (_responseStream is null || _requestStream is null)
            {
                throw new InvalidProgramException("Streams must be attached.");
            }
            await _requestStream.WriteAsync(new RegisterClientRequest()
            {
                ClientId = id.ToString(),
                Token = token
            });
            if (await _responseStream.MoveNext())
            {
                return await Task.FromResult(_responseStream.Current);
            }
            return await Task.FromException<RegisterClientResponse>(
                new InvalidInstructionalConnectionException("no response from server"));
        }

        internal virtual async Task Run()
        {
            try
            {
                await Task.WhenAll(
                    ListenTask(),
                    StreamHeartbeat());
            }
            catch (RpcException ex) when (_cancellationToken.IsCancellationRequested)
            {
                Log.Debug("Grpc threw error on cancellation", ex);
            }
            finally
            {
                _cancellationToken.Dispose();
            }
        }
    }
}
