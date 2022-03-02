using System;
using System.Diagnostics;
using System.Timers;
using BluePrism.AutomateAppCore;
using BluePrism.BPCoreLib;
using BluePrism.BPServer.Enums;
using BluePrism.BPServer.Utility;
using BluePrism.Common.Security;
using BluePrism.Core.Encryption;
using BluePrism.Core.Extensions;
using BluePrism.Server.Domain.Models;
using NLog;
using static BPServer.clsBPServer;

namespace BluePrism.BPServer.ServerBehaviours
{
    internal class CertificateExpiryChecker
    {
        private const int OneWeek       = 7;
        private const int OneMonth      = 30;
        private const int SixMonth      = 30 * 6;
        private const int ThreeMonths   = 30 * 3;

        private const int _triggerHour = 12;

        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private string _thumbprint;
        private DateTime _expiryDate;
        private readonly Timer _timer;
        private readonly CertificateServices _certificateServices;

        public event StatusHandler Update;

        internal CertificateExpiryChecker()
            : this(new CertificateStoreService())
        {
        }

        internal CertificateExpiryChecker(ICertificateStoreService store)
        {
            if (store is null) throw new ArgumentNullException(nameof(store));

            _certificateServices = new CertificateServices(store);
            _timer = new Timer();
            _timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Options.Instance.SelectedConfigEncryptionMethod != MachineConfig.ConfigEncryptionMethod.OwnCertificate) return;
            if (string.IsNullOrEmpty(_thumbprint)) return;

            var expiry = _expiryDate - DateTime.Now;

            try { ValidateTimeUntilExpiry(expiry); }
            catch(Exception ex)
            {
                Update?.Invoke(string.Format(Properties.Resources.ErrorValidatingTimeUntilCertificateExpiry, ex.Message),
                    LoggingLevel.Error);
            }
        }

        private void ValidateTimeUntilExpiry(TimeSpan time)
        {
            if (DateTime.Now.Hour != _triggerHour)
                Debug.WriteLine($"Certificate expiry checker should be triggering at {_triggerHour}. Hour now={DateTime.Now.Hour}. Interval={_timer.Interval}");
            if (time.Days < 0)
                throw new InvalidValueException($"{nameof(time)} days cannot be negative. Implies certificate has already expired. Days={time.Days}");

            string message = string.Format(Properties.Resources.Certificate0WillExpireIn1Days2Hours,
                _thumbprint, time.Days, time.Hours);

            var proximity = GetExpiryProximity(time.Days);
            var expiryRule = BPUtil.GetAttributeValue<ExpiryAttribute>(proximity);

            _timer.Interval = expiryRule.Interval;
            var warningType = expiryRule.LogLevel;

            if (proximity == ExpiryProximity.None) return;

            LogResult(message, warningType);
        }

        private ExpiryProximity GetExpiryProximity(int days)
        {
            if (days < OneWeek)
                return ExpiryProximity.OneDay;
            else if (days < OneMonth)
                return ExpiryProximity.OneMonth;
            else if (days < ThreeMonths)
                return ExpiryProximity.ThreeMonths;
            else if (days < SixMonth)
                return ExpiryProximity.SixMonths;
            else
                return ExpiryProximity.None;
        }

        private void LogResult(string message, LoggingLevel logLevel)
        {
            if(logLevel.ToNLogLevel() is LogLevel nLogLevel)
                Log.Log(nLogLevel, message);
            Update?.Invoke(message, logLevel);
        }

        internal void Start(string thumbprint)
        {
            _thumbprint = thumbprint;

            if(!string.IsNullOrEmpty(thumbprint))
                _expiryDate = _certificateServices.CertificateExpiryDateTime(thumbprint);

            _timer.Interval = DateTime.Now.TimeUntilNextHour(_triggerHour).TotalMilliseconds;

            _timer.Start();
        }

        internal void Stop()
        {
            _timer.Stop();
        }        
    }    
}
