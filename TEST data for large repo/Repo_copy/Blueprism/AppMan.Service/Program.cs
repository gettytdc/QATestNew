using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using AppMan.Service.Properties;
using BluePrism.AppMan.Citrix;
using BluePrism.AppMan.Logging;
using BluePrism.StartUp;
using NLog;

namespace BluePrism.ApplicationManager.AppMan.Service
{
    public static class Program
    {
        private static AppManController _appManController;
        private static NotifyIcon _trayIcon;
        private static Thread _notifyThread;
        private static bool _running = true;

        private static readonly ManualResetEvent DisconnectedEvent = new ManualResetEvent(false);
       
        // The name of the virtual channel we use for Citrix
        private const string BluePrismVirtualChannelName = "BPAPMAN";

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                ContainerInitialiser.SetUpContainer();
                AppManNLogConfig.Configure();

                CreateTrayIcon();
                Log.Debug("Starting AppManController");
                while (_running)
                {
                    _ = DisconnectedEvent.Reset();
                    Stream controllerStream;
                    switch (GetMode(args))
                    {
                        case CommunicationMode.Citrix:
                            controllerStream =
                                VirtualChannelStream.AwaitVirtualChannelStream(BluePrismVirtualChannelName,
                                    DisconnectedEvent);
                            break;
                        default:
                            // TODO Add support for different communication methods i.e RDP etc...
                            throw new NotImplementedException();
                    }

                    _appManController = new AppManController(controllerStream);
                    _appManController.BeginReadThread(args);
                    Log.Info("Returned from Read Thread");
                    ResetAppManInstances(false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Log.Debug(ex.Message);
            }
        }

        private enum CommunicationMode
        {
            TCP,
            Citrix
        }

        private static CommunicationMode GetMode(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0].Equals("/mode", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (args.Length > 1)
                    {
                        if (args[1].Equals("citrix", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return CommunicationMode.Citrix;
                        }

                        throw new ArgumentException(string.Format(Resources.TheModeIsNotSupported, args[1]));
                    }

                    throw new ArgumentException(Resources.TheSwitchModeShouldBe);
                }

                throw new ArgumentException(string.Format(Resources.TheSwitchIsNotSupported, args[0]));
            }

            return CommunicationMode.Citrix; //Default to citrix when no argument given.
        }


        private static void CreateTrayIcon()
        {
            _notifyThread = new Thread(
                delegate ()
                {
                    _trayIcon = new NotifyIcon()
                    {
                        Icon = Resources.TrayIcon,
                        ContextMenu = new ContextMenu(new[] {
                            new MenuItem("Exit", Exit)
                        }),
                        Visible = true
                    };
                    Application.Run();
                });
            _notifyThread.Start();
        }

        private static void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            _trayIcon.Visible = false;
            ResetAppManInstances(true);

            Application.Exit();
        }

        private static void ResetAppManInstances(bool disposing)
        {
            _ = DisconnectedEvent.Set();
            if (disposing)
            {
                DisconnectedEvent.Dispose();
            }

            foreach (var appMan in _appManController.AppMans.Values.Where(appMan => !appMan.Process.HasExited))
            {
                appMan.Close();
            }

            _appManController.AppMans.Clear();
        }
    }
}
