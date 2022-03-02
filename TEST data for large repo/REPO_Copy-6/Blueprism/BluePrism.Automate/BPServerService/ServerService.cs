using BluePrism.AutomateAppCore;
using BluePrism.BPCoreLib;
using BluePrism.BPCoreLib.Collections;
using BluePrism.BPServer.Enums;
using BluePrism.BPServer.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using BluePrism.Core.CommandLineParameters;
using BluePrism.Server.Domain.Models;

namespace BPServer
{
    public partial class ServerService : ServiceBase
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        // Flag indicating if this service is running in 'verbose' mode or not
        private bool mVerbose;
        
        // The server instance
        private clsBPServer mBPServer;

        // The name of the server instance
        private String mServerName;

        // localization lookup string 
        private const string LocaleStringLookup = "locale=";

        // useropts lookup string
        private const string UseroptsStringLookup = "useropts=";

        private const string WcfperformanceStringLookup = "wcfperformance=";

        public ServerService()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the starting of this service
        /// </summary>
        /// <param name="args">The arguments passed to the service</param>
        protected override void OnStart(string[] args)
        {
            try
            {
                ServerNLogConfig.Configure();
                ServerNLogConfig.SetAppProperties(true);
                DoStart(args);
            }
            catch (Exception e)
            {
                Log.Fatal(e, "An error occurred while trying to start the server");
                throw;
            }
            // Add a last resort handler for any unhandled exceptions in the service
            AppDomain.CurrentDomain.UnhandledException += ReportError;
        }

        /// <summary>
        /// Reports the given unhandled exception to the event log
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The args detailing the unhandled exception</param>
        private void ReportError(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Fatal("An unhandled exception occurred in the BP server service:{0}{1}",
                Environment.NewLine, e.ExceptionObject);
        }

        /// <summary>
        /// Performs the start of the service
        /// </summary>
        /// <param name="args">The arguments passed to the program to start it.
        /// </param>
        private void DoStart(string[] args)
        {
            bool userSpecific = false;
            int? wcfPerformanceMinutes = null;

            mServerName = MachineConfig.DefaultServerConfigName;
            var realArgs = ProcessArgumentList(args);
            
            foreach (var arg in realArgs)
            {
                if (arg.StartsWith(LocaleStringLookup))
                {
                    SwitchLocale(arg);
                }
                else if (arg.StartsWith(UseroptsStringLookup))
                {
                    userSpecific = GetUserOptsBool(arg);
                }
                else if (arg.StartsWith(WcfperformanceStringLookup))
                {
                    wcfPerformanceMinutes = GetWcfInt(arg);
                }
                else
                {
                    mServerName = arg;
                }
            }

            ServerNLogConfig.SetAppProperties(true, mServerName);

            // Load options from config file...
            try
            {
                Options.Instance.Init(ConfigLocator.Instance(userSpecific));
            }
            catch(Exception ex)
            {
                throw new OperationFailedException(
                "Could not load configuration - {0}", ex.Message);
            }

            // Find the requested server configuration...
            MachineConfig.ServerConfig srv = Options.Instance.GetServerConfig(mServerName);
            if (srv == null) throw new InvalidValueException(
                "Server configuration '{0}' is not defined", mServerName);

            if (wcfPerformanceMinutes != null)
            {
                Options.Instance.WcfPerformanceLogMinutes = wcfPerformanceMinutes;
            }

            // Initialise everything and start the server...
            mBPServer = new clsBPServer();
            if (srv.StatusEventLogging)
            {
                mBPServer.Err += Err;
                mBPServer.Warn += Warn;
                mBPServer.Info += Info;
                mBPServer.Verbose += Verbose;
                mVerbose = srv.Verbose;
            }
                        
            mBPServer.Start(srv);
        }

        /// <summary>
        /// Attempt to process the command line args using old and new method
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static IEnumerable<string> ProcessArgumentList(string[] args)
        {
            var envCommandLineArgs = Environment.GetCommandLineArgs();
            //create the argument list, we don't care about the name of the exe so remove.
            var argsList = new List<string>(envCommandLineArgs.Skip(1).Take(envCommandLineArgs.Count() - 1));
            argsList.AddRange(args);
            //make sure nothing is duplicated
            var realArgs = argsList.Distinct().ToArray();

            if (realArgs.Length >= 5)
            {
                throw new InvalidArgumentException("Invalid arguments: {0}", CollectionUtil.Join(args, ","));
            }
            //don't these specified twice.
            if (realArgs.Count(r => r.Contains(LocaleStringLookup)) > 1)
            {
                throw new InvalidArgumentException("Invalid arguments: {0}", CollectionUtil.Join(args, ","));
            }
            if (realArgs.Count(r => r.Contains(UseroptsStringLookup)) > 1)
            {
                throw new InvalidArgumentException("Invalid arguments: {0}", CollectionUtil.Join(args, ","));
            }
            if (realArgs.Count(r => r.Contains(WcfperformanceStringLookup)) > 1)
            {
                throw new InvalidArgumentException("Invalid arguments: {0}", CollectionUtil.Join(args, ","));
            }

            return realArgs;
        }

        private static bool GetUserOptsBool(string arg)
        {
            if (bool.TryParse(arg.Substring(UseroptsStringLookup.Length), out var result))
            {
                return result;
            }

            throw new InvalidArgumentException("Invalid useropts value.");
        }

        private static int GetWcfInt(string arg)
        {
             var performanceTestingParameter = new WcfPerformanceTestingParameter(arg.Substring(WcfperformanceStringLookup.Length));
             return performanceTestingParameter.PerformanceTestDurationMinutes;
        }

        private static void SwitchLocale(string localeArg)
        {
            try
            {
                string locale = localeArg.Substring(LocaleStringLookup.Length);
            System.Threading.Thread.CurrentThread.CurrentUICulture =
                new System.Globalization.CultureInfo(locale);
            System.Threading.Thread.CurrentThread.CurrentCulture =
                new System.Globalization.CultureInfo(locale);
            }
            catch (IndexOutOfRangeException)
            {
                throw new InvalidArgumentException("Locale Missing");
            }
            catch (System.Globalization.CultureNotFoundException)
            {
                throw new InvalidArgumentException("Not a valid culture name.");
            }
        }

        private void Err(string msg, LoggingLevel level) { Log.Error(msg); }

        private void Warn(string msg, LoggingLevel level) { Log.Warn(msg); }

        private void Info(string msg, LoggingLevel level) { Log.Info(msg); }

        private void Verbose(string msg, LoggingLevel level)
        {
            if (mVerbose)
                Log.Info(msg);
            else
                Log.Debug(msg);
        }

        protected override void OnStop()
        {
            mBPServer.Stop();
        }
    }
}
