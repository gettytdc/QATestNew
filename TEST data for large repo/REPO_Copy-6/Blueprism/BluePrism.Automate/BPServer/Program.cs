using BluePrism.BPServer.Properties;
using BluePrism.AutomateAppCore;
using System;
using System.Runtime.Serialization;
using System.Windows.Forms;
using BluePrism.BPServer.Logging;
using BluePrism.Core.CommandLineParameters;
using NLog;
using BluePrism.Core.Utility;

namespace BluePrism.BPServer
{
    using BluePrism.BPCoreLib;
    using StartUp;

    static class Program
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            ContainerInitialiser.SetUpContainer();
            RegexTimeout.SetDefaultRegexTimeout();

            ServerNLogConfig.Configure();
            ServerNLogConfig.SetAppProperties(false);
            Log.Info("Started");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (CultureHelper.IsLatinAmericanSpanish())
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture =
                    new System.Globalization.CultureInfo(CultureHelper.LatinAmericanSpanish);
            }

            bool start=false;
            bool userSpecific = false;
            int? wcfPerformanceMinutes = null;
            for (var index = 0; index < args.Length; index++)
            {
                string s = args[index];
                switch (s)
                {
                    case "/start":
                        start = true;
                        break;
                    case "/useropts":
                        userSpecific = true;
                        break;
                    case "/locale":
                        try
                        {
                            s = args[++index];
                            System.Threading.Thread.CurrentThread.CurrentUICulture =
                                new System.Globalization.CultureInfo(s);
                            System.Threading.Thread.CurrentThread.CurrentCulture =
                                new System.Globalization.CultureInfo(s);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            MessageBox.Show(BluePrism.BPServer.Properties.Resources.LocaleMissing);
                            return 1;
                        }
                        catch (System.Globalization.CultureNotFoundException)
                        {
                            MessageBox.Show(BluePrism.BPServer.Properties.Resources.NotAValidCultureName);
                            return 1;
                        }
                        break;
                    case "/wcfperformance":
                        try
                        {
                            var performanceTestingParameter = new WcfPerformanceTestingParameter(args[++index]);
                            wcfPerformanceMinutes = performanceTestingParameter.PerformanceTestDurationMinutes;
                        }
                        catch (Exception)
                        {
                            MessageBox.Show(BluePrism.Core.Properties.Resource.WCFPerformance_Invalid);
                            return 1;
                        }
                        break;
                    default:
                        MessageBox.Show(string.Format(BluePrism.BPServer.Properties.Resources.InvalidCommandLineParameter0, s));
                        return 1;
                }
            }

            try
            {
                Application.Run(new frmMain(start, userSpecific, wcfPerformanceMinutes));
            }
            catch (InvalidDataContractException e)
            {
                MessageBox.Show(string.Format(Resources.ErrorOccurredDuringSerializationDeserialization0, e));
                Log.Error(e, "Serialization error");
            }
            return 0;
        }
    }
}
