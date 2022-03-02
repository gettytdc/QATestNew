using System;
using System.ServiceProcess;

namespace BPServer
{
    using BluePrism.StartUp;
    using BluePrism.BPCoreLib;
    
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ContainerInitialiser.SetUpContainer();
            RegexTimeout.SetDefaultRegexTimeout();
            ServiceBase[] run;
            run=new ServiceBase[] { new ServerService() };
            ServiceBase.Run(run);
        }
    }
}
