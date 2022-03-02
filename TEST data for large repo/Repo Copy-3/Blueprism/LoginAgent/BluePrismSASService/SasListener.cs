using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using BluePrism.LoginAgent.Utilities;

namespace BluePrism.LoginAgent.Sas
{
    /// <summary>
    /// Creates a named channel to listen for messages from other processes.
    /// </summary>
    class SasListener
    {
        bool _requiresShutdown;
        object _shutdownLock = new object();

        readonly EventLogger _logger;

        public SasListener(EventLogger logger)
        {
            _logger = logger;
        }


        /// <summary>
        /// Creates a server and listens for messages
        /// </summary>
        /// <param name="name">name of the channel</param>
        /// <param name="processMessage">delegate to process the message</param>
        public void ListenForMessage(string name, Func<string, string> processMessage)
        {
            try
            {
                _logger.WriteLogEntry(EventLogEntryType.Information, $"Starting SAS Listener");
                var pipeSecurity = new PipeSecurity();
                pipeSecurity.SetAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.NetworkSid, null), 
                    PipeAccessRights.FullControl, AccessControlType.Deny));
                pipeSecurity.SetAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                    PipeAccessRights.ReadWrite, AccessControlType.Allow));
                                
                using (var pipeServer = new NamedPipeServerStream(name,
                                                            PipeDirection.InOut,
                                                            1,
                                                            PipeTransmissionMode.Byte,
                                                            PipeOptions.None,
                                                            1024, 1024, pipeSecurity))
                {
                    while (!ShutdownSignalSent)
                    {                       
                        pipeServer.WaitForConnection();
                        if (!ShutdownSignalSent && pipeServer.IsConnected)
                        {
                            Byte[] buffer = new Byte[65535];
                            Int32 count = pipeServer.Read(buffer, 0, 100);
                            if (count > 0)
                            {
                                string message = Encoding.Unicode.GetString(buffer, 0, count);

                                string response = processMessage.Invoke(message);

                                if (response.Length > 0)
                                {
                                    var sendBuffer = new Byte[65535];
                                    sendBuffer = Encoding.Unicode.GetBytes(response);
                                    pipeServer.Write(sendBuffer, 0, sendBuffer.Length);
                                }
                            }
                            Thread.Sleep(1000);
                            pipeServer.Disconnect();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.WriteLogEntry(EventLogEntryType.Error, $"Listener error : {e}");
            }

        }

        public void ShutDown()
        {
            lock (_shutdownLock)
            {
                _requiresShutdown = true;
            }
        }

        private bool ShutdownSignalSent
        {
            get
            {
                lock (_shutdownLock)
                {
                    return _requiresShutdown;
                }
            }
        }
        
    }
}
