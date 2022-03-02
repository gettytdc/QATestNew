using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using BluePrism.BPCoreLib.DependencyInjection;
using BluePrism.ClientServerResources.Core.Data;
using BluePrism.ClientServerResources.Core.Enums;
using BluePrism.ClientServerResources.Core.Exceptions;
using BluePrism.ClientServerResources.Core.Interfaces;
using BluePrism.ClientServerResources.Grpc.Events;
using BluePrism.ClientServerResources.Grpc.Interfaces;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using InstructionalConnection;
using NLog;

namespace BluePrism.ClientServerResources.Grpc.Services
{
    public class InstructionalConnectionServiceImpl : InstructionalConnectionService.InstructionalConnectionServiceBase,
                                                      IInstructionalConnectionService,
                                                      IDisposable
    {
        private class InternalMessage
        {
            public ResourcesChangedData ResourcesChangedData { get; set; }
            public SessionCreatedData SessionCreatedData { get; set; }
            public SessionDeletedData SessionDeletedData { get; set; }
            public SessionEndData SessionEndData { get; set; }
            public SessionStartedData SessionStartedData { get; set; }
            public SessionVariablesUpdatedData SessionVariableUpdatedData { get; set; }

            public SessionStopData SessionStopData { get; set; }
        }

        private event Action<InternalMessage> Enqueued;
        public event EventHandler<ClientRegisteredEventArgs> ClientRegistered;
        public event EventHandler<ClientRegisteredEventArgs> ClientDeregistered;

        // Having _clients value as a struct/class caused issue with creating the observable queue from Enqueued events
        // as the subscriber delegate would become null after leaving constructor's scope
        private readonly IDictionary<Guid, (CancellationTokenSource Token, IObservable<InternalMessage> Queue, SemaphoreSlim WriteLock)> _clients
            = new ConcurrentDictionary<Guid, (CancellationTokenSource, IObservable<InternalMessage>, SemaphoreSlim)>();
        private readonly ITokenValidator _tokenValidator;
        private bool _disposedValue;

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public InstructionalConnectionServiceImpl()
        {
            _tokenValidator = DependencyResolver.Resolve<ITokenValidator>();
        }

        public override async Task RegisterClient(IAsyncStreamReader<RegisterClientRequest> requestStream, 
                                                  IServerStreamWriter<RegisterClientResponse> responseStream, ServerCallContext context)
        {
            Guid clientId = default(Guid);
            try
            {
                Log.Debug("Starting client registration");
                await requestStream.MoveNext().ConfigureAwait(false);
                
                Log.Debug("Validating registration Token");
                ValidationToken(requestStream.Current.Token);
                Log.Debug("Client registration token was validated successfully");

                clientId = OnConnected(requestStream.Current.ClientId, context.CancellationToken);

                // tell the client that everything has gone okay.
                await responseStream.WriteAsync(new RegisterClientResponse
                {
                    Success = true,
                    Error = string.Empty
                });

                var (token, queue, writeLock) = _clients[clientId];

                await Task.WhenAll(
                    ProcessQueue(responseStream, queue, writeLock, token.Token, clientId),
                    RespondToStreamPings(requestStream, responseStream, context, writeLock, token.Token));
            }
            catch(TokenValidationException ex)
            {
                Log.Info($"Token was invalid - {ex.ValidationResult.Reason}");
                await responseStream.WriteAsync(new RegisterClientResponse
                {
                    Success = false,
                    Error = ex.ValidationResult.Reason,
                    Message = Any.Pack(MessageConverterClass.CreateFailedOperationMessage(
                        ex.ValidationResult.Reason,
                        "Validation failed",
                        (int)RegisterClientStatusCode.InvalidToken))
                    
                });
            }
            catch (ClientAlreadyExistsException ex)
            {
                Log.Info($"The client was already connected");
                await responseStream.WriteAsync(new RegisterClientResponse
                {
                    Success = false,
                    Error = "This client has already connected",
                    Message = Any.Pack(MessageConverterClass.CreateFailedOperationMessage(
                        $"The client {ex.UserId} is already connected",
                        "Client already exists",
                        (int)RegisterClientStatusCode.ClientExists))
                });
            }
            catch(AggregateException ex) when (ex.GetBaseException() is InvalidOperationException iex)
            {
                Log.Debug("InvalidOperationException in register client thread:", iex);
            }
            catch(TaskCanceledException)
            {
                Log.Debug("Register client client thread was cancelled");
            }
            catch (Exception ex)
            {
                Log.Info($"Error sending instructional response to client {ex}");
                await responseStream.WriteAsync(new RegisterClientResponse()
                {
                    Success = false,
                    Error = string.Empty,
                    Message = Any.Pack(MessageConverterClass.CreateFailedOperationMessage(
                        ex.Message,
                        $"Client callback channel encountered an unexpected error",
                        (int)RegisterClientStatusCode.ResponseError))
                });                
            }
            finally
            {
                if(_clients.ContainsKey(clientId))
                    _clients.Remove(clientId);
            }
        }

        private async Task ProcessQueue(
            IServerStreamWriter<RegisterClientResponse> responseStream,
            IObservable<InternalMessage> queue,
            SemaphoreSlim writeLock,
            CancellationToken token,
            Guid clientId)
        {
            // Iterate over this client's observable queue.
            // When new items are pushed onto this queue, the ForEachAwaitAsync will run
            await queue
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async (item) =>
                {
                    Log.Trace($"Dequeue for client id: {clientId}");

                    writeLock.Wait(token);
                    await responseStream.WriteAsync(new RegisterClientResponse()
                    {
                        Success = true,
                        Error = string.Empty,
                        Message = CreateProtoBufAny(item)
                    });
                    writeLock.Release();
                }, token)
                .ConfigureAwait(false);
        }

        private async Task RespondToStreamPings(
            IAsyncStreamReader<RegisterClientRequest> requestStream,
            IServerStreamWriter<RegisterClientResponse> responseStream,
            ServerCallContext context,
            SemaphoreSlim writeLock,
            CancellationToken token)
        {
            while (await requestStream.MoveNext(token).ConfigureAwait(false))
            {
                var current = requestStream.Current;

                if (current.Message.Is(GetStatusRequest.Descriptor))
                {
                    writeLock.Wait(token);
                    await responseStream.WriteAsync(new RegisterClientResponse
                    {
                        Message = Any.Pack(new GetStatusResponse
                        {
                            ClientRecognised = true,
                            StatusCode = (int)context.Status.StatusCode,
                            StatusDetail = context.Status.Detail ?? string.Empty
                        }),
                        Success = true,
                        Error = string.Empty

                    });
                    writeLock.Release();
                }
            }
        }

        private void ValidationToken(string token)
        {
            TokenValidationInfo validationResult;
            try
            {
                validationResult = _tokenValidator.Validate(token);
            }
            catch(InvalidOperationException e)
            {
                //if server-call fails we end up here
                Log.Debug($"Failed to validate token {token}", e);
                validationResult = TokenValidationInfo.FailureTokenInfo($"Exception: {e.Message}");
            }
            if (validationResult != null && !validationResult.Success)
            {
                throw new TokenValidationException(validationResult);
            }
        }

        private Any CreateProtoBufAny(InternalMessage item)
        {
            if (item.ResourcesChangedData != null)
            {
                return Any.Pack(item.ResourcesChangedData.To());
            }
            else if (item.SessionCreatedData != null)
            {
                return Any.Pack(item.SessionCreatedData.To());
            }
            else if (item.SessionDeletedData != null)
            {
                return Any.Pack(item.SessionDeletedData.To());
            }
            else if (item.SessionEndData != null)
            {
                return Any.Pack(item.SessionEndData.To());
            }
            else if (item.SessionStartedData != null)
            {
                return Any.Pack(item.SessionStartedData.To());
            }
            else if (item.SessionVariableUpdatedData != null)
            {
                return Any.Pack(item.SessionVariableUpdatedData.To());
            }
            else if (item.SessionStopData != null)
            {
                return Any.Pack(item.SessionStopData.To());
            }
            throw new InvalidOperationException("Invalid message format!");
        }

        private Guid OnConnected(string clientIdString, CancellationToken contextToken)
        {
            var clientId = ParseClientId(clientIdString);

            if (!_clients.ContainsKey(clientId))
            {
                /* 
                 * having each client hold their own observable collection that subscribes to the main Action has the following affect:
                 *      invoking that Action will trickle down into each client thread (RegisterClient)
                 * Alternatively we can have the clients have their own Action plus a 'master' action, and an Observable for both of these actions (.Concat())
                 * Therefore having more control over which clients we can signal a message to.
                 */
                _clients.Add(clientId,
                    (CancellationTokenSource.CreateLinkedTokenSource(contextToken),
                    Observable.FromEvent<InternalMessage>(
                        (x) => Enqueued += x,
                        (x) => Enqueued -= x),
                    new SemaphoreSlim(1, 1)));
            }
            else
            {
                Log.Debug($"Failed to connect client {clientId}, it already exists");
                throw new ClientAlreadyExistsException(clientId);
            }

            ClientRegistered?.Invoke(this, new ClientRegisteredEventArgs(clientId));
            Log.Info($"Connected client id: {clientId}");

            return clientId;
        }

        private void ThrowIfNoSubscribers()
        {
            if (_clients.Any() && Enqueued == null)
            {
               throw new InvalidOperationException("There should not be any known clients whilst {nameof(Enqueued)} has no subscribers (null)");
            }
        }

        public override Task<DeRegisterClientResponse> DeRegister(DeRegisterClientRequest request, ServerCallContext context)
        {
            var error = string.Empty;
            try
            {
                var clientId = ParseClientId(request.ClientId);

                if (!_clients.ContainsKey(clientId))
                {
                    return Task.FromResult(new DeRegisterClientResponse
                    {
                        Success = false,
                        Error = "client is not registered"
                    });
                }

                _clients[clientId].Token.Cancel();
                ClientDeregistered?.Invoke(this, new ClientRegisteredEventArgs(clientId));

                return Task.FromResult(new DeRegisterClientResponse()
                {
                    Success = true,
                    Error = string.Empty
                });
            }
            catch (InvalidOperationException ioe)
            {
                error = ioe.Message;
            }
            catch (Exception ex)
            {
                error = $"Unexpected exception {ex.Message}";
            }
            return Task.FromResult(new DeRegisterClientResponse()
            {
                Success = false,
                Error = error
            });
        }

        public void EnqueueMessage(ResourcesChangedData data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            ThrowIfNoSubscribers();

            Enqueued?.Invoke(new InternalMessage() { ResourcesChangedData = data });
        }

        public void EnqueueMessage(SessionCreatedData data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            ThrowIfNoSubscribers();

            Enqueued?.Invoke(new InternalMessage() { SessionCreatedData = data });
        }

        public void EnqueueMessage(SessionDeletedData data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            ThrowIfNoSubscribers();

            Enqueued?.Invoke(new InternalMessage() { SessionDeletedData = data });
        }

        public void EnqueueMessage(SessionEndData data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            ThrowIfNoSubscribers();

            Enqueued?.Invoke(new InternalMessage() { SessionEndData = data });
        }

        public void EnqueueMessage(SessionStartedData data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            ThrowIfNoSubscribers();

            Enqueued?.Invoke(new InternalMessage() { SessionStartedData = data });
        }

        public void EnqueueMessage(SessionStopData data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            ThrowIfNoSubscribers();

            Enqueued?.Invoke(new InternalMessage() { SessionStopData = data });
        }

        public void EnqueueMessage(SessionVariablesUpdatedData data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            ThrowIfNoSubscribers();

            Enqueued?.Invoke(new InternalMessage() { SessionVariableUpdatedData = data });
        }

        private Guid ParseClientId(string clientIdString)
        {
            if (!Guid.TryParse(clientIdString, out var clientId))
            {
                throw new InvalidOperationException("clientid cannot be parsed");
            }
            return clientId;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach (var c in _clients)
                    {
                        c.Value.Token.Cancel();
                        c.Value.Token.Dispose();
                    }
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
