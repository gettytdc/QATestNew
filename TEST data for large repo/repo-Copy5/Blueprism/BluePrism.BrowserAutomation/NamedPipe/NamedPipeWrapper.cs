using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using BluePrism.BrowserAutomation.Events;
using BluePrism.BrowserAutomation.WebMessages;
using BluePrism.NamedPipes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System.Collections.Concurrent;
using BluePrism.BrowserAutomation.WebMessages.Events;

namespace BluePrism.BrowserAutomation.NamedPipe
{
    public class NamedPipeWrapper : INamedPipeWrapper
    {
        private readonly IDictionary<BrowserType, NamedPipeClient> _pipeDictionary =
            new ConcurrentDictionary<BrowserType, NamedPipeClient>();

        public event NativeMessagingHostNotFoundDelegate HostNotFound;
        public event WebMessageReceivedDelegate MessageReceived;
        public event PipeDisposedDelegate PipeDisposed;

        public NamedPipeClientStream Pipe { get; set; }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public NamedPipeWrapper() { }

        public void SendMessage(WebMessageWrapper wrapper)
        {
            string messageType;

            try
            {
                messageType = JObject.Parse(wrapper.Data)["MessageType"].ToString();
            }
            catch
            {
                //Message is encrypted
                messageType = string.Empty;
            }


            if (messageType == "Attach" || messageType == "Launch")
            {
                Enum.TryParse(JObject.Parse(wrapper.Data)["Data"]["BrowserType"].ToString(),
                    out BrowserType browserType);
                try
                {
                    ConnectBrowserPipe(browserType);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    HostNotFound?.Invoke(this, new NativeMessagingHostNotFoundEventArgs(true));
                }
            }

            foreach (var pipe in _pipeDictionary.Values)
            {
                try
                {
                    pipe.SendMessage(wrapper);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }                
            }
        }

        private void ConnectBrowserPipe(BrowserType browserType)
        {
            if (_pipeDictionary.TryGetValue(browserType, out var client))
            {
                if (client.IsConnected())
                {
                    return;
                }
                _pipeDictionary.Remove(browserType);

            }

            var serverNameBase = $"BP{browserType}Listener";
            serverNameBase += System.Security.Principal.WindowsIdentity.GetCurrent().User;

            var namedPipeClient = new NamedPipeClient(serverNameBase);
            namedPipeClient.MessageReceived += MessageReceivedEvent;
            namedPipeClient.PipeDisposed += PipeDisposedEvent;
            if (namedPipeClient.Connect())
            {
                namedPipeClient.Listen();
                _pipeDictionary.Add(browserType, namedPipeClient);
            }
        }

        private void MessageReceivedEvent(object sender, MessageReceivedDelegateEventArgs args) => MessageReceived?.Invoke(sender, new WebMessageReceivedDelegateEventArgs(JsonConvert.DeserializeObject<WebMessageWrapper>(args.Message.ToString())));

        private void PipeDisposedEvent(object sender, PipeDisposedDelegateEventArgs args)
        {
            while (_pipeDictionary.Any(x => x.Value.ClientId == args.ClientId))
            {
                var pipe = _pipeDictionary.First(x => x.Value.ClientId == args.ClientId);
                _pipeDictionary.Remove(pipe);
            }

            if (!_pipeDictionary.Any())
            {
                PipeDisposed?.Invoke(sender, new PipeDisposedDelegateEventArgs(Guid.Empty));
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
                Pipe.Dispose();
                _pipeDictionary.Clear();
                Log.Info("Disposed");
            }
        }
    }
}
