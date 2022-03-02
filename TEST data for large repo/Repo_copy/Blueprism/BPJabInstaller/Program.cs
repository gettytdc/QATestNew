using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BPJabInstaller
{


    static class Program
    {

        ///--------------------------------------------------------------------------
        /// <summary>
        /// Adds a message to the console output.
        /// </summary>
        /// <param name="msg">The message to be output.</param>
        ///--------------------------------------------------------------------------
        private static void WriteStatusLine(string msg)
        {
            Console.WriteLine(msg);
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length > 0)
            {
                if (args[0] != "/install")
                {
                    Console.WriteLine("Only /install is allowed");
                    return 1;
                }

                Version version;
                if (args.Length > 1)
                {
                    try
                    {
                        version = new Version(args[1]);
                    }
                    catch
                    {
                        Console.WriteLine("Invalid version specified");
                        return 1;
                    }
                    if (!clsInstaller.GetVersions().Contains(version))
                    {
                        Console.WriteLine("Version {0} is not available in this installer", version.ToString());
                        return 1;
                    }
                }
                else
                {
                    version = new Version("2.0.4");
                }

                clsInstaller installer = new clsInstaller();
                installer.Status += WriteStatusLine;
                bool result = installer.Install(version, true);
                return result ? 0 : 1;
            }
            Console.WriteLine("Launching GUI...");
            Application.Run(new frmMain());
            return 0;
        }
    }
}