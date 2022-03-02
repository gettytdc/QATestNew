using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using BluePrism.ApplicationManager.AppMan.Service.Events;
using BluePrism.Core.Extensions;
using NLog;

namespace BluePrism.ApplicationManager.AppMan.Service
{
    public class AppMan : IDisposable
    {

        private readonly string _argString;
        private bool _processExited;
        private readonly TcpClient _client;
        private readonly object _controllerStreamLock;
        private readonly StreamWriter _controllerWriter;
        private readonly Guid _id;
        private readonly StreamReader _appManReader;
        public Process Process { get; }
        public StreamWriter AppManWriter { get; }
        public event EventHandler<DisconnectedEventArgs> Disconnected;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public AppMan(Guid id, Stream controllerStream, string argString, object controllerStreamLock, CitrixAppManMessage.CitrixMode mode)
        {
            _id = id;
            _argString = argString;
            _controllerStreamLock = controllerStreamLock;
            Process = StartProcess(mode, out var port);
            Task.Run(() => WaitForProcessExit());

            _client = new TcpClient();
            _client.Connect("localhost", port.Value);
            Log.Debug("tcpClient connected");

            var appManStream = _client.GetStream();
            AppManWriter = new StreamWriter(appManStream) {AutoFlush = true};
            _appManReader = new StreamReader(appManStream);
            _controllerWriter = new StreamWriter(controllerStream) {AutoFlush = true};
            Task.Run(() => ReadThread());
        }

        private void ReadThread()
        {
            try
            {
                using (_appManReader)
                {
                    while (!_processExited)
                    {
                        var line = _appManReader.ReadLine();
                        if (string.IsNullOrEmpty(line))
                        {
                            continue;
                        }

                        lock (_controllerStreamLock)
                        {
                            _controllerWriter.WriteLine($"AppManId {_id}{line}");
                        }
                    }
                }

                Disconnected?.Invoke(this, new DisconnectedEventArgs(_id));
            }
            catch (Exception ex)
            {
                Log.Debug($"AppMan.ReadThread Id: {_id}, Error: {ex}");
            }
        }

        private Process StartProcess(CitrixAppManMessage.CitrixMode mode, out int? port)
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException(), $"Appman{GetSuffix(mode)}.exe");
            Log.Debug("Now Launching Appman");
            var p = new Process { StartInfo = new ProcessStartInfo { FileName = path, Arguments = _argString, CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true } };
            p.Start();
            Log.Debug("Appman Launched");

            using (var stdOut = p.StandardOutput)
            {
                var line = stdOut.ReadLine();
                Log.Debug("Readline: " + line);
                port = line.GetInt();

                if (!port.HasValue)
                {
                    throw new InvalidDataException("No port value");
                }
            }
            return p;
        }

        private static string GetSuffix(CitrixAppManMessage.CitrixMode mode)
        {
            switch (mode)
            {
                case CitrixAppManMessage.CitrixMode.Citrix32:
                    return "32";
                case CitrixAppManMessage.CitrixMode.Citrix64:
                    return "64";
                default:
                    throw new InvalidEnumArgumentException(nameof(mode), (int)mode, typeof(CitrixAppManMessage.CitrixMode));
            }
        }

        private void WaitForProcessExit()
        {
            Process.WaitForExit();

            _processExited = true;

            Log.Debug($"Appman {_id} has exited");
        }

        public void Close()
        {
            AppManWriter?.Close();

            if (!Process.WaitForExit(100))
            {
                Process.Kill();
            }

            Process.Dispose();
            Log.Debug($"Appman {_id} closed");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            AppManWriter.Dispose();
            _appManReader.Dispose();
            _controllerWriter.Dispose();
            _client.Dispose();
            Log.Debug($"AppMan {_id} Disposed");
        }
    }
}
