using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using BluePrism.ApplicationManager;
using BluePrism.AppMan.Logging;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.AppMan.Citrix;
using BluePrism.AppMan.Properties;
using BluePrism.CharMatching;
using NLog;

namespace BluePrism.AppMan
{
    using BluePrism.BPCoreLib;
    using StartUp;

    public class Program
    {

        // The TCP Server socket listening for commands from the main app
        private static TcpListener _listener;

        // The TCP client used when in TCP mode
        private static TcpClient _client;

        // The stream writer used to talk to the BP application
        private static StreamWriter _writer;

        // The stream reader to read messages from the BP application
        private static StreamReader _reader = null;

        // An event which is set if we are disconnected from the target application
        private static ManualResetEvent _disconnectedEvent =
            new ManualResetEvent(false);

        // An event which is set if there are incoming messages ready to process
        private static ManualResetEvent _incomingReadyEvent =
            new ManualResetEvent(false);

        // The thread which handles the reading of messages from the BP app and
        // storing of them onto the queue
        private static Thread _readThread;

        // The queue of messages received from the BP app
        private static Queue<string> _incomingQueue = new Queue<string>();

        // An event used to detect when a font has been fully loaded
        private static AutoResetEvent _fontLoadedReadyEvent =
            new AutoResetEvent(false);

        // The last loaded font data.
        private static string _loadedFontData;
        private static readonly object Lock = new object();

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        // A font reader implementation which handles the loading of the font
        // asynchronously
        private class RemoteFontStore : IFontStore
        {
            /// <summary>
            /// Gets the font with the given name by requesting the font from the
            /// BP app asynchronously and awaiting a response
            /// </summary>
            /// <param name="name">The name of the required font</param>
            /// <returns>The font with the given name, or null if no response was
            /// received from the BP app after 30 seconds.</returns>
            public BPFont GetFont(string name)
            {
                SendAsync("fontload " + name);
                if (!_fontLoadedReadyEvent.WaitOne(30000, false))
                    return null;
                return new BPFont(name, null, _loadedFontData);
            }

            public string GetFontOcrPlus(string name)
            {
                throw new NotImplementedException();
            }

            public void SaveFont(BPFont font) => throw new NotImplementedException();

            public void SaveFontOcrPlus(string name, string data)
            {
                throw new NotImplementedException();
            }

            public bool DeleteFont(string name) => throw new NotImplementedException();

            public ICollection<string> AvailableFontNames => throw new NotImplementedException();
        }

        /// <summary>
        /// The main body of the program.
        /// </summary>
        /// <param name="args">Not used - any configuration is retrieved from the
        /// <see cref="clsConfig"/> class.</param>
        [STAThread]
        public static int Main(string[] args)
        {
            ContainerInitialiser.SetUpContainer();
            AppManNLogConfig.Configure();
            RegexTimeout.SetDefaultRegexTimeout();


            // Set a font loader...
            FontReader.SetFontStore(new RemoteFontStore());

            // Initialise config and report any issues...
            string sErr = null;
            if (!clsConfig.Init(ref sErr))
            {
                Console.WriteLine(Resources.ConfigProblem0, sErr);
                return -1;
            }

            if (!InitialiseReadThread())
            {
                return 0;
            }

            var targetApp = new clsLocalTargetApp();
            targetApp.ExpectDisconnect += ExpectDisconnect;
            targetApp.Disconnected += Disconnected;

            var returnCode = ProcessCommands(targetApp);

            try
            {
                DisposeObjects();
            }
            catch (Exception ex)
            {
                Console.WriteLine(Resources.ExceptionDuringShutdown0, ex.Message);
                Log.Debug(string.Format(Resources.ExceptionDuringShutdown0, ex.Message));
                Log.Debug(ex);
                return -1;
            }

            return returnCode;
        }

        private static int ProcessCommands(clsTargetApp targetApp)
        {
            try
            {
                var finished = false;
                while (!finished)
                {
                    var which = WaitHandle.WaitAny(new WaitHandle[] { _incomingReadyEvent, _disconnectedEvent }, 500, false);
                    switch (which)
                    {
                        case WaitHandle.WaitTimeout:
                            break;
                        case 1: // mTerminated
                            finished = true;
                            break;
                        case 0: //mIncomingReady
                            lock (_incomingQueue)
                            {
                                _incomingReadyEvent.Reset();
                                while (_incomingQueue.Count > 0)
                                {
                                    var line = _incomingQueue.Dequeue();
                                    var result = targetApp.ProcessQuery(line);
                                    result = result.Replace("\\", "\\\\");
                                    result = result.Replace("\r", "\\r");
                                    result = result.Replace("\n", "\\n");
                                    lock (Lock)
                                    {
                                        _writer.WriteLine(result);
                                    }
                                }
                            }
                            break;
                    }

                    // The following is required to allow messages destined for the
                    // ManagedSpyLib EventTargetWindow to be processed...
                    Application.DoEvents();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Log.Error(e);
                return -1;
            }
            finally
            {
                try
                {
                    targetApp.Dispose();
                }
                catch (Exception ex)
                {
                    SendAsync("EXCEPTION: While disposing - " + ex.Message);
                    Log.Debug(ex);
                }
            }
            return 0;
        }
        private static bool InitialiseReadThread()
        {
            var stream = GetTcpClientStream();
            try
            {
                if (stream is null)
                {
                    var message = $"Failed to get a stream of type: TCP";
                    Console.WriteLine(message);
                    Log.Debug(message);
                    throw new ApplicationException(message);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                return false;
            }

            _reader = null;

            _reader = new StreamReader(stream);
            _writer = new StreamWriter(stream) {AutoFlush = true};

            _readThread = new Thread(new ThreadStart(ReadThread));
            _readThread.Start();
            return true;
        }

        private static Stream GetTcpClientStream()
        {
            _listener = TcpListener.Create(0); //use 0 for random port
            _listener.Start();
            Console.WriteLine(LocaleTools.Properties.GlobalResources.WaitingForAConnectionOnPort +
                              ((IPEndPoint)_listener.LocalEndpoint).Port.ToString());

            var count = 0;
            while (!_listener.Pending())
            {
                Thread.Sleep(50);
                count++;
                if (count > 30000 / 50)
                {
                    Console.WriteLine(Resources.NoConnectionAfter30SecondsTerminating);
                    _listener.Stop();
                    return null;
                }
            }

            _client = _listener.AcceptTcpClient();


            // Stop listening, we only accept one connection.
            _listener.Stop();

            return _client.GetStream();
        }

        private static void DisposeObjects()
        {
            _reader.BaseStream.Close();
            if (_readThread.IsAlive)
            {
                _readThread.Abort();
            }
           
            _reader?.Dispose();
            _writer?.Dispose();
        }

        /// <summary>
        /// Handles the reading of messages from the Blue Prism client/resource app,
        /// and the distribution of those messages to the appropriate place (queue,
        /// font loading event).
        /// </summary>
        private static void ReadThread()
        {
            try
            {
                string line;
                while ((line = _reader.ReadLine()) != null)
                {
                    ProcessLine(line);
                }
            }
            catch (Exception e)
            {
                Log.Debug(e);
            }

            Log.Debug("Read Thread exited");
        }

        private static void ProcessLine(string line)
        {
            if (line.StartsWith("$FONTDATA"))
            {
                if (line.Length > 10)
                {
                    _loadedFontData = line.Substring(10);
                }
                else
                {
                    _loadedFontData = null;
                }

                _fontLoadedReadyEvent.Set();
            }
            else
            {
                lock (_incomingQueue)
                {
                    _incomingQueue.Enqueue(line);
                    _incomingReadyEvent.Set();
                }
            }
        }

        /// <summary>
        /// Sends an asynchronous message to the main application
        /// </summary>
        /// <param name="ev">The string message to send to the application</param>
        private static void SendAsync(string ev)
        {
            if (_writer == null)
                return;
            clsConfig.Log("Async Event: {0}", ev);
            lock (Lock)
            {
                _writer.WriteLine(">>" + ev);
            }
        }

        /// <summary>
        /// Handles the disconnected event occurring from the clsLocalTargetApp
        /// instance being used in this program.
        /// </summary>
        private static void Disconnected()
        {
            try
            {
                SendAsync("Disconnected");
            }
            catch (Exception ex)
            {
                Console.WriteLine(Resources.ExceptionWhileRaisingDisconnectedAsyncEvent0, ex.Message);
                Log.Debug(ex);
            }

            _disconnectedEvent.Set();

        }

        /// <summary>
        /// Handles the disconnecting event occurring from the clsLocalTargetApp
        /// instance being used in this program.
        /// </summary>
        private static void ExpectDisconnect()
        {
            try
            {
                Log.Debug($"Sending Expect Disconnect");
                SendAsync("ExpectDisconnect");
            }
            catch (Exception ex)
            {
                Console.WriteLine(Resources.ExceptionWhileRaisingDisconnectingAsyncEvent0, ex.Message);
                Log.Debug(ex.StackTrace);
            }
        }
    }
}
