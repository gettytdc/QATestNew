using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ServiceModel;
using BluePrism.BPCoreLib.DependencyInjection;
using BluePrism.ClientServerResources.Core.Data;
using BluePrism.ClientServerResources.Core.Interfaces;
using BluePrism.ClientServerResources.Wcf.Data;
using NLog;

namespace BluePrism.ClientServerResources.Wcf.Endpoints
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class NotificationServices : INotificationServices
    {
        private readonly IDictionary<Guid, INotificationServiceCallBack> _clientCallbackList = new ConcurrentDictionary<Guid, INotificationServiceCallBack>();
        private readonly ITokenValidator _tokenValidator;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public NotificationServices()
        {
            _tokenValidator = DependencyResolver.Resolve<ITokenValidator>();
        }

        public Response RegisterClient(Guid id,string token)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("invalid client id - Empty", nameof(id));
            }
            if(string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("supplied token is invalid", nameof(token));
            }

            if (_clientCallbackList.ContainsKey(id))
            {
                return new Response()
                {
                    Success = false,
                    Error = $"client {id} is already registered",
                    Message = "Client already exists"
                };
            }

            var validationresult = _tokenValidator.Validate(token);
            if(!validationresult.Success)
            {
                return new Response()
                {
                    Success = false,
                    Error = validationresult.Reason,
                    Message = "Validation failed"
                };               
            }

            var proxy = GetProxy;
            _clientCallbackList.Add(id, proxy);
            return new Response()
            {
                Success = true
            };
        }

        public Response UnRegisterClient(Guid id)
        {
            if (id == Guid.Empty) 
            { 
                throw new ArgumentException(nameof(id)); 
            }
            Log.Info($"Removing client {id.ToString().Replace('\n', '_').Replace('\r', '_').Replace('\t', '_')}");

            if (!_clientCallbackList.ContainsKey(id))
            {
                return new Response()
                {
                    Success = false,
                    Error = $"client {id} is not registered",
                    Message = "Failed to deregister client"
                };
            }            
            _clientCallbackList.Remove(id);
            return new Response()
            {
                Success = true
            };
        }

        public Response GetStatus(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
            }

            if (!_clientCallbackList.ContainsKey(id))
            {
                return new Response()
                {
                    Success = false,
                    Error = $"client {id} is not registered"
                };
            }

            return new Response()
            {
                Success = true,
                Message = "Server online"
            };
        }

        // callback functions
        public void ResourceStatus(Guid id, ResourcesChangedData data)
        {
            if (data == null) { throw new ArgumentException(nameof(data));}
            INotificationServiceCallBack client = null;

            // if not client id is specified, signal all callbacks.
            if (id == Guid.Empty)
            {
                foreach (var currentClient in _clientCallbackList.Values)
                {
                    client = currentClient;
                    SendOnResourceStatus(client, data);
                }
            }
            else if (TryGetClientProxy(id, out client))
            {
                SendOnResourceStatus(client, data);
            }
        }

        public void SessionCreated(Guid id, SessionCreatedData data)
        {
            if (data == null)
            { throw new ArgumentException(nameof(data)); }
            INotificationServiceCallBack client = null;
            if (id == Guid.Empty)
            {
                foreach (var currentClient in _clientCallbackList.Values)
                {
                    client = currentClient;
                    SendOnSessionCreated(client, data);
                }
            }
            else if (TryGetClientProxy(id, out client))
            {
                SendOnSessionCreated(client, data);
            }
        }

        public void DeleteSession(Guid id, SessionDeletedData data)
        {
            if (data == null)
            { throw new ArgumentException(nameof(data)); }
            INotificationServiceCallBack client = null;
            if (id == Guid.Empty)
            {
                foreach (var currentClient in _clientCallbackList.Values)
                {
                    client = currentClient;
                    SendOnSessionDeleted(client, data);
                }
            }
            else if (TryGetClientProxy(id, out client))
            {
                SendOnSessionDeleted(client, data);
            }
        }

        public void SessionEnd(Guid id, SessionEndData data)
        {
            if (data == null)
            { throw new ArgumentException(nameof(data)); }
            INotificationServiceCallBack client = null;
            if (id == Guid.Empty)
            {
                foreach (var currentClient in _clientCallbackList.Values)
                {
                    client = currentClient;
                    SendOnSessionEnd(client, data);
                }
            }
            else if (TryGetClientProxy(id, out client))
            {
                SendOnSessionEnd(client, data);
            }
        }

        public void SessionStarted(Guid id, SessionStartedData data)
        {
            INotificationServiceCallBack client = null;
            if (id == Guid.Empty)
            {
                foreach (var currentClient in _clientCallbackList.Values)
                {
                    client = currentClient;
                    SendOnSessionStarted(client, data);
                }
            }
            else if (TryGetClientProxy(id, out client))
            {
                SendOnSessionStarted(client, data);
            }
        }

        public void SessionStop(Guid id, SessionStopData data)
        {
            INotificationServiceCallBack client = null;
            if (id == Guid.Empty)
            {
                foreach (var currentClient in _clientCallbackList.Values)
                {
                    client = currentClient;
                    SendOnSessionStop(client, data);
                }
            }
            else if (TryGetClientProxy(id, out client))
            {
                SendOnSessionStop(client, data);
            }
        }

        public void SessionVariablesUpdated(Guid id, SessionVariablesUpdatedData data)
        {
            INotificationServiceCallBack client = null;
            if (id == Guid.Empty)
            {
                foreach (var currentClient in _clientCallbackList.Values)
                {
                    client = currentClient;
                    SendOnSessionVariableUpdated(client, data);
                }
            }
            else if (TryGetClientProxy(id, out client))
            {
                SendOnSessionVariableUpdated(client, data);
            }
        }

        /// <summary>
        /// Return the proxy of the registered client
        /// </summary>
        /// <param name="id"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        protected bool TryGetClientProxy(Guid id, out INotificationServiceCallBack callback)
        {
            if (id == Guid.Empty) { throw new ArgumentException(nameof(id)); }

            if (!_clientCallbackList.ContainsKey(id))
            {
                callback = null;
                return false;
            }

            callback = _clientCallbackList.Where(c => c.Key == id).Select(c => c.Value).First();
            return true;
        }

        protected INotificationServiceCallBack GetProxy => OperationContext.Current.GetCallbackChannel<INotificationServiceCallBack>();


        private void SendOnSessionCreated(INotificationServiceCallBack client, SessionCreatedData data)
        {
            Task.Run(() =>
            {
                try
                {
                    client.OnSessionCreated(data);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "WCF Duplex SessionCreated callback failed.");
                }
            });
        }

        private void SendOnResourceStatus(INotificationServiceCallBack client, ResourcesChangedData data)
        {
            Task.Run(() =>
            {
                try
                {
                    client.OnResourceStatus(data);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "WCF Duplex OnResourceStatus callback failed.");
                }
            });
        }

        private void SendOnSessionDeleted(INotificationServiceCallBack client, SessionDeletedData data)
        {
            Task.Run(() =>
            {
                try
                {
                    client.OnSessionDeleted(data);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "WCF Duplex OnSessionDeleted callback failed.");
                }
            });
        }

        private void SendOnSessionEnd(INotificationServiceCallBack client, SessionEndData data)
        {
            Task.Run(() =>
            {
                try
                {
                    client.OnSessionEnd(data);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "WCF Duplex OnSessionEnd callback failed.");
                }
            });
        }

        private void SendOnSessionStarted(INotificationServiceCallBack client, SessionStartedData data)
        {
            Task.Run(() =>
            {
                try
                {
                    client.OnSessionStarted(data);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "WCF Duplex OnSessionStarted callback failed.");
                }
            });
        }

        private void SendOnSessionStop(INotificationServiceCallBack client, SessionStopData data)
        {
            Task.Run(() =>
            {
                try
                {
                    client.OnSessionStop(data);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "WCF Duplex OnSessionStop callback failed.");
                }
            });
        }

        private void SendOnSessionVariableUpdated(INotificationServiceCallBack client, SessionVariablesUpdatedData data)
        {
            Task.Run(() =>
            {
                try
                {
                    client.OnSessionVariableUpdated(data);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "WCF Duplex OnSessionVariableUpdated callback failed.");
                }
            });
        }
    }
}
