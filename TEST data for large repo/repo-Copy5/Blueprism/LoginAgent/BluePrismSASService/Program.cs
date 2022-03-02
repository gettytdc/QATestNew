using System.ServiceProcess;

namespace BluePrism.LoginAgent.Sas
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new SasService()
            };
            ServiceBase.Run(ServicesToRun);
        }


    }
}