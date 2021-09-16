using BluePrism.Cirrus.Common.MassTransit;

namespace BluePrism.DigitalWorker.MessagingClient
{
    public static class DigitalWorkerQueues
    {
        private const string ServiceName = "DigitalWorker";
        private const string RunProcessMessageType = "RunProcess";
        private static readonly QueueNameBuilder Builder = new QueueNameBuilder(ServiceName);

        public static string Consecutive => Builder.Build(RunProcessMessageType, "Consecutive");

        public static string Concurrent => Builder.Build(RunProcessMessageType, "Concurrent");

        public static string Control(string digitalWorkerName) => Builder.Build(digitalWorkerName, "Control");

        public static string StopProcess(string digitalWorkerName) => Control(digitalWorkerName);

        public static string RequestStopProcess(string digitalWorkerName) => Control(digitalWorkerName);

        public static string GetSessionVariables(string digitalWorkerName) => Control(digitalWorkerName);
    }
}
