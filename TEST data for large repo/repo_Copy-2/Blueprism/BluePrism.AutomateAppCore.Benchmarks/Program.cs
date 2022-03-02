using System;
using BenchmarkDotNet.Running;
using BluePrism.AutomateAppCore.ClientServerConnection;
using BluePrism.BPCoreLib;
using BluePrism.StartUp;

namespace BluePrism.AutomateAppCore.Benchmarks
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            ContainerInitialiser.SetUpContainer();
            RegexTimeout.SetDefaultRegexTimeout();
            var options = Options.Instance;
            options.Init(ConfigLocator.Instance(false));
            var serverFactory = new ServerFactoryWrapper();
            serverFactory.ClientInit(options.DbConnectionSetting);
            serverFactory.ValidateCurrentConnection();
            app.gSv.LoginAsAnonResource(Environment.MachineName);

            BenchmarkRunner.Run<CalandarInternalBusinessObjectBenchmark>();
        }
    }
}
