using BluePrism.BPCoreLib;
using BluePrism.DataPipeline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NLog;

namespace BluePrism.Datapipeline.Logstash
{
    public class TCPCommandListener : ITCPCommandListener
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public event EventHandler<CommandEventArgs> CommandReceived;

        private readonly IEventLogger _eventLogger;
        private readonly int _port;
        private Thread _tcpServerThread;

        public TCPCommandListener(int port, IEventLogger eventLogger)
        {
            _port = port;
            _eventLogger = eventLogger;
        }


        public bool IsRunning { get; private set; } = false;

        public int ListenPort => _port;

        public void Start()
        {
            IsRunning = true;
            _tcpServerThread = new Thread(RunTcpServer);
            _tcpServerThread.Start();

        }

        public void Stop()
        {
            IsRunning = false;
        }


        private void RunTcpServer()
        {
            TcpListener listener = null;
            List<TcpClient> clients = new List<TcpClient>();
            try
            {
                listener = TcpListener.Create(_port);
                listener.Start();
                _eventLogger.Info($"TCP Listener started on port {_port}", Log);

                int disconnectCheckCount = 0;

                while (IsRunning)
                {
                    try
                    {
                        if (listener.Pending())
                        {
                            TcpClient client = listener.AcceptTcpClient();
                            clients.Add(client);
                        }
                    }
                    catch (Exception e)
                    {
                        _eventLogger.Error($"Unable to accept incoming connection: {e.Message}" , Log);
                    }

                    try
                    {
                        disconnectCheckCount = (disconnectCheckCount + 1) % 20;

                        if (disconnectCheckCount == 0)
                        {
                            // remove any disconnected  clients.
                            var disconnectedClients = clients.Where(t => t.Client.Poll(10, SelectMode.SelectRead) && t.Client.Available == 0).ToList();


                            foreach (var client in disconnectedClients)
                            {
                                client.Dispose();
                                clients.Remove(client);
                            }
                        }

                        foreach (var client in clients)
                        {
                            string command;
                            using (var streamReader =
                                new StreamReader(client.GetStream(), Encoding.UTF8, true, 1024, true))
                            {
                                command = streamReader.ReadLine();
                            }

                            if (command == null)
                                continue;

                            _eventLogger.Info($"Command received over TCP connection: {command}", Log);
                            try
                            {
                                CommandReceived?.Invoke(this, new CommandEventArgs(CommandFromString(command)));

                            }
                            catch (Exception e)
                            {
                                _eventLogger.Error($"Error performing command: {command}: {e.Message}", Log);
                            }
                            
                        }

                    }
                    catch (Exception e)
                    {
                        _eventLogger.Error($"Error: {e.Message}", Log);
                    }

                    Thread.Sleep(50);

                }
            }
            catch (Exception e)
            {
                _eventLogger.Error($"Error when running TCP listener: {e.Message}", Log);
            }
            finally
            {
                if (listener != null)
                {
                    listener.Stop();
                    IsRunning = false;
                }
                foreach (var client in clients)
                {
                    client.Dispose();
                }

            }
        }

        private DataPipelineProcessCommand CommandFromString(string commandString)
        {
            if (commandString == "start")
                return DataPipelineProcessCommand.StartProcess;

            if (commandString == "stop")
                return DataPipelineProcessCommand.StopProcess;

            throw new InvalidOperationException($"Unknown command received over TCP connection: {commandString}");
        }

    }
}
