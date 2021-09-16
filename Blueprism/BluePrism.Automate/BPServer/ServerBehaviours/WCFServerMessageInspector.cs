using BluePrism.BPServer.Properties;
using BPServer;
using NLog;
using System;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace BluePrism.BPServer.ServerBehaviours
{
    internal class WCFServerMessageInspector : IDispatchMessageInspector, IEndpointBehavior, IInstanceContextInitializer, IChannelInitializer
    {
        private clsBPServer mParent;
        private bool mLogTraffic;
        private ILogger mLog = LogManager.GetCurrentClassLogger();

        public WCFServerMessageInspector(clsBPServer parent, bool logTraffic)
        {
            mParent = parent;
            mLogTraffic = logTraffic;
        }

        public void Initialize(InstanceContext instanceContext, Message message)
        {
            mParent.OnVerbose("Initialising instance context {0}", instanceContext.GetHashCode());
        }

        public void Initialize(IClientChannel channel)
        {
            mParent.ConnectedClients++;
            mParent.OnVerbose("Client {0} initialized", channel.SessionId);
            channel.Closed += ClientDisconnected;
            channel.Faulted += ClientFaulted;
        }

        void ClientDisconnected(object sender, EventArgs e)
        {
            mParent.OnVerbose("Client {0} disconnected", ((IClientChannel)sender).SessionId);
            mParent.ConnectedClients -= 1;
        }

        void ClientFaulted(object sender, EventArgs e)
        {
            mParent.OnVerbose("Client {0} faulted", ((IClientChannel)sender).SessionId);
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            if (mLogTraffic)
            {
                MessageBuffer buffer = request.CreateBufferedCopy(Int32.MaxValue);
                request = buffer.CreateMessage();
                mParent.OnVerbose("Received for {0}:\r\n{1}", instanceContext.GetHashCode(), buffer.CreateMessage().ToString());
            }
            long ticks = DateTime.Now.Ticks;
            return ticks;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            try
            {
                MessageBuffer buffer = reply.CreateBufferedCopy(Int32.MaxValue);
                reply = buffer.CreateMessage();
                String message = buffer.CreateMessage().ToString();

                string name = "";
                var nameStart = message.IndexOf(@"http://tempuri.org/IServer/", StringComparison.InvariantCultureIgnoreCase);
                if (nameStart > 0)
                {
                    nameStart += 27;
                    var nameEnd = message.IndexOf(@"Response", nameStart, StringComparison.InvariantCultureIgnoreCase);
                    name = message.Substring(nameStart, nameEnd - nameStart);
                }

                long startime = 0;
                if (correlationState != null)
                    startime = (long)correlationState;
                TimeSpan timetaken = new TimeSpan(DateTime.Now.Ticks - startime);
                if (name != @"Nop" && !string.IsNullOrEmpty(name))
                { 
                    var logMessage = $"Function: {name},\t\tReply Size: {message.Length},\tTime: {String.Format("{0:0.00}", timetaken.TotalMilliseconds)}ms";
                    mParent.OnVerbose(logMessage);
                    if (mLog.IsDebugEnabled)
                        mLog.Debug(logMessage);
                }
                if (mLog.IsTraceEnabled)
                    mLog.Trace($"Response: {message}");
                if (mLogTraffic)
                    mParent.OnVerbose($"Response: {message}");
            }
            catch (Exception exception)
            {
                mParent.OnInfo("Can't log sending message {0}", exception);

                //This will help us detect when a file is unable to be loaded as it will highlight .dll files that have not been added to the setup project.
                if (exception.InnerException is ReflectionTypeLoadException innerException)
                {
                    foreach (var loaderException in innerException.LoaderExceptions)
                    {
                        mParent.OnInfo("Additional Loader Exception Information {0}", loaderException.Message);

                    }
                }
            }
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(this);
            endpointDispatcher.DispatchRuntime.InstanceContextInitializers.Add(this);
            endpointDispatcher.ChannelDispatcher.ChannelInitializers.Add(this);
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            throw new InvalidOperationException(Resources.NotSupported);
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }
    }
}
