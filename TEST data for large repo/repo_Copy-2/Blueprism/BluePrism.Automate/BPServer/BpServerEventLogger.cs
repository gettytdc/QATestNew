using System;
using BluePrism.BPCoreLib;
using NLog;

namespace BPServer
{
    /// <summary>
    /// Proxy class for event logger to enable easier debugging using bpserver UI
    /// </summary>
    internal class BpServerEventLogger : IEventLogger
    {
        private readonly Action<string> _uiLog;

        public BpServerEventLogger(Action<string> uiLog)
        {
            _uiLog = (x) => { uiLog($"DataGateways:{x}"); };
        }      

        public void Debug(string msg, Logger logger)
        {
            _uiLog(msg);
            logger.Debug(msg);
        }

        public void Error(string msg, Logger logger)
        {
            _uiLog(msg);
            logger.Error(msg);
        }

        public void Info(string msg, Logger logger)
        {
            _uiLog(msg);
            logger.Info(msg);
        }

        public void Warn(string msg, Logger logger)
        {
            _uiLog(msg);
            logger.Warn(msg);
        }
    }
}
