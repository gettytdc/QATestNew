using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using BluePrism.AutomateAppCore;
using BluePrism.BrowserAutomation.WebMessages;
using BluePrism.Core.Extensions;
using BluePrism.MessagingHost.Browser;
using BluePrism.NamedPipes;
using BluePrism.NativeMessaging.EventArgs;
using BluePrism.NativeMessaging.Implementations;
using BluePrism.NativeMessaging.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Config;

namespace BluePrism.MessagingHost
{
    public static class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static NamedPipes.Interfaces.IServer _pipeServer;
        private static Host _browserHost;

        private static string _bpListenerName = string.Empty;
        private static readonly string[] Browsers = new string[] { "msedge", "chrome", "firefox" };
        private const string ConfigurationFileName = "Automate.NLog.config";
        private const int ThreadSleepInterval = 100;

        [STAThread]
        public static void Main(string[] args)
        {
            ConfigureNLog();
            try
            {                
                var browserType = SetBrowserSpecificValues();
                GlobalDiagnosticsContext.Set("AppName", $"{browserType}NativeMessagingHost");
                AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

                Log.Info("Created the main server thread");
                _pipeServer = new Server(_bpListenerName, ListenForCommands);

                _browserHost = new Host(new BrowserHost());
                _browserHost.MessageReceived += ForwardMessageToBluePrismInstance;
                _browserHost.Attached += Host_Attached;
                _browserHost.Detached += Host_Detached;
                _browserHost.MessagingHost.LostConnection += Host_LostConnectionToBrowser;                


                _browserHost.MessagingHost.Listen();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private static string SetBrowserSpecificValues()
        {
            var browser = GetBrowserName(Process.GetCurrentProcess().Id);
            switch (browser)
            {
                case "chrome":
                    _bpListenerName = "BPChromeListener";
                    break;
                case "msedge":
                    _bpListenerName = "BPEdgeListener";
                    break;
                case "firefox":
                    _bpListenerName = "BPFirefoxListener";
                    break;
                default:
                    throw new NotSupportedException("Unknown browser type");
            }

            _bpListenerName += System.Security.Principal.WindowsIdentity.GetCurrent().User;
            return browser;
        }
        private static string GetBrowserName(int pid)
        {
            //keep checking parents till we find a browser
            try
            {
                var parent = Process.GetProcessById(pid).Parent();
                if (Browsers.Contains(parent.ProcessName))
                {
                    return parent.ProcessName;
                }
                else
                {
                    return GetBrowserName(parent.Id);
                }
            }
            catch { return string.Empty; }
        }


        private static void ConfigureNLog()
        {
            var filePath = Path.Combine(clsFileSystem.CommonAppDataDirectory, ConfigurationFileName);
            if (File.Exists(filePath)) {
                LogManager.Configuration = new XmlLoggingConfiguration(filePath, new LogFactory()
                {
                    ThrowConfigExceptions = false
                });
            }
        }
        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Log.Info("CurrentDomain_ProcessExit");
            Host_LostConnectionToBrowser(sender, e);
            _pipeServer.Dispose();
        }

        private static void Host_LostConnectionToBrowser(object sender, EventArgs e)
        {
            Log.Info("Lost Connection to Browser");
            DetachAllSessions();

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            while (!_browserHost.AllPagesDisconnected && stopWatch.Elapsed.TotalSeconds < 30)
            {
				Log.Info("Some pages still connected, waiting before disposing pipe");
                Thread.Sleep(ThreadSleepInterval);
                // Wait for all pages to disconnect before closing the connected pipes
            }

            //Add sleep to ensure all detach messages have been sent before disposing the pipe
            Thread.Sleep(1000);
            Log.Info("All pages disconnected, disposing pipe");
            _pipeServer.Dispose();
        }

        public static void DetachAllSessions()
        {
            Log.Info($"DetachAllSessions sessions count {_browserHost.MessagingHost.Sessions.Count}");
            foreach (var session in _browserHost.MessagingHost.Sessions)
            {
                Log.Info($"Clearing session {session.SessionId}");
                var bpClientId = session.BpClientId;
                foreach (var tab in session.Tabs)
                {
                    Log.Info($"Clearing tab {tab.Id}");
                    Host_Detached(null,
                    new MessageReceivedEventArgs
                    {
                        Data = JObject.Parse($"{{ BpClientId: \"{bpClientId}\"}}"),
                        Pages = tab.Pages
                    });
                }
            }

            _browserHost.MessagingHost.Sessions.Clear();
        }

        private static void ListenForCommands(ServerCommunication serverCommunication, string bluePrismClientId)
        {
            while (serverCommunication.CanRead)
            {
                var command = serverCommunication.ReadMessage();
                if (string.IsNullOrEmpty(command))
                {
                    continue;
                }
                var responseObject = new ResponseConfirmation(command);

                var commandObject = JsonConvert.DeserializeObject<WebMessageWrapper>(command);
                // The webMessage may be encrypted
                try
                {
                    var webMessage = JsonConvert.DeserializeObject<WebMessage>(commandObject.Data);
                    //We have a command, do something based on the command

                    switch (webMessage.MessageType)
                    {
                        case MessageType.Attach:
                            var attachMessage = JsonConvert.DeserializeObject<AttachMessage>(commandObject.Data);
                            _browserHost.AttachToBrowser(attachMessage, bluePrismClientId);
                            break;

                        case MessageType.Launch:
                            var launchMessage = JsonConvert.DeserializeObject<LaunchMessage>(commandObject.Data);
                            _browserHost.LaunchBrowser(launchMessage, bluePrismClientId);
                            break;

                        case MessageType.Detach:
                            var detachMessage = JsonConvert.DeserializeObject<DetachMessage>(commandObject.Data);
                            _browserHost.DetachBrowser(detachMessage, bluePrismClientId);
                            break;

                        case MessageType.ClosePage:
                            var closeMessage = JsonConvert.DeserializeObject<CloseWebPageMessage>(commandObject.Data);
                            _browserHost.CloseWebPages(closeMessage, bluePrismClientId);
                            break;
                            
                        default:
                            _browserHost.SendMessageToConnectedPages(responseObject.GetJObject(), bluePrismClientId);
                            break;
                    }
                }
                catch
                {
                    _browserHost.SendMessageToConnectedPages(responseObject.GetJObject(), bluePrismClientId);
                }
            }
        }
       
        private static void Host_Attached(object sender, MessageReceivedEventArgs args)
        {
            var data = args.Data["data"];
            if (data == null)
            {
                return;
            }

            var bpClientId = data["bpClientId"]?.ToString();
            var extensionVersion = data["extensionVersion"]?.ToString();
            var trackingId = data["trackingId"]?.ToString();
            var attachedResponse = new WebMessageWrapper(args.Pages, "Attached", trackingId, extensionVersion);

            if (bpClientId != Guid.Empty.ToString())
            {
                var disconnectMessage = new WebMessageWrapper(args.Pages, "Detached", string.Empty, string.Empty);
                //send attached to the named instance, and a disconnect to the others
                _pipeServer.SendToMultiplePipes(disconnectMessage, x => x.Key != bpClientId);
                _pipeServer.SendMessageAsClient(attachedResponse,bpClientId);
            }
            else
            {
                _pipeServer.SendToAllPipes(attachedResponse);
            }
        }

        private static void Host_Detached(object sender, MessageReceivedEventArgs args)
        {
            var bpClientId = args.Data["BpClientId"]?.ToString();
            try
            {
                if (bpClientId != Guid.Empty.ToString())
                {
                    var res = new WebMessageWrapper(args.Pages, "Detached", string.Empty, string.Empty);
                    _pipeServer.SendMessageAsClient(res, bpClientId);
                    Log.Info($"Host_Detached {args.Pages.Count} pages detached.");
                }
            }
            catch (Exception e)
            {
                Log.Error($"Exception sending message to blueprism instance {bpClientId}");
                Log.Error(e);
            }
        }

        private static void ForwardMessageToBluePrismInstance(object sender, MessageReceivedEventArgs args)
        {
            try
            {
                var pageId = args.Data["data"]?["pageId"]?.ToString();
                var sessionStarted = args.Data["data"]?["data"]?.ToString() == "Session Started";
                
                if (!string.IsNullOrEmpty(pageId) && pageId != Guid.Empty.ToString() || sessionStarted)
                {
                    var session = sessionStarted ? null : _browserHost.MessagingHost.GetSessionForPage(pageId);

                    if (session == null || session.BpClientId == Guid.Empty.ToString())
                    {
                        _pipeServer.SendToAllPipes(args.Data["data"]);
                    }
                    else
                    {
                        _pipeServer.SendMessageAsClient(args.Data["data"],session.BpClientId);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}
