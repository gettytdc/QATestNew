using NLog;
using NLog.Layouts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace BluePrism.Core.Analytics
{
    /// <summary>
    /// The message inspector for the server side message interception.
    /// Will work for both client/server side connections
    /// </summary>
    public class CommunicationInspectors : IClientMessageInspector, IDispatchMessageInspector
    {
        private readonly IMessageEventLogger _messageEventLogger;
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly TimeSpan _interval;
        private DateTime _lastRun = DateTime.UtcNow;
        private DateTime? _logExpiry = null;
        private bool _logExpired = false;
        

        public CommunicationInspectors(DateTime? logExpiry, IMessageEventLogger messageEventLogger)
        {
            _messageEventLogger = messageEventLogger;
            _interval = TimeSpan.FromMinutes(5);
            _logExpiry = logExpiry;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reply"></param>
        /// <param name="correlationState"></param>
        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            if (!_logExpired)
            {
                CreateStats(ref reply, correlationState);
            }

        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            if (!_logExpired)
            {
                return DateTime.Now.Ticks;
            }
            else
            {
                return null;
            }
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            if (!_logExpired)
            {
                CreateStats(ref reply, correlationState);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            if (!_logExpired)
            {
                return DateTime.Now.Ticks;
            }
            else
            {
                return null;
            }
        }

      
        /// <summary>
        /// Calculate the message statistics and send it over to the 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="correlationState"></param>
        public void CreateStats(ref Message message, object correlationState)
        {
            var buffer = message.CreateBufferedCopy(Int32.MaxValue);
            message = buffer.CreateMessage();
            var messageString = message.ToString();
            
            var messageId = message.Headers.MessageId;
            long starttime = (correlationState != null) ? Convert.ToInt64(correlationState) : 0;

            var timeTaken = new TimeSpan(DateTime.Now.Ticks - starttime);

            var actionName = TrimAction(message.Headers.Action);

            //record the event within the system
            _messageEventLogger.RecordCallEvent(TrimAction(message.Headers.Action), messageString.Length, timeTaken.TotalMilliseconds, DateTime.UtcNow);
            //run any stats on the data sets
            _messageEventLogger.Analyse();

            // Check if we've just expired.
            if (_logExpiry != null && DateTime.UtcNow >= _logExpiry)
            {
                _logExpired = true;
            }

            //check the last run interval to see if the system is going to log the putput
            if (_lastRun + _interval < DateTime.UtcNow || _logExpired)
            {
                var currentResults = _messageEventLogger.CreateReport(true);
                LogReport(currentResults);
                _lastRun = DateTime.UtcNow;
            }
        }

        public IEnumerable<string> GetCurrentResults()
        {
            return _messageEventLogger.CreateReport(true);
        }


        /// <summary>
        /// Log results string to the logger
        /// </summary>
        /// <param name="results"></param>
        private void LogReport(IEnumerable<string> results)
        {
            foreach (var result in results)
            {
                _logger.Info(result);
            }
            _logger.Info(Environment.NewLine);
        }

        private static string TrimAction(string action)
        {
            const string searchString = "/IServer/";
            int index = action.IndexOf(searchString) + searchString.Length;
            return action.Substring(index, action.Length - index);
        }
    }
}
