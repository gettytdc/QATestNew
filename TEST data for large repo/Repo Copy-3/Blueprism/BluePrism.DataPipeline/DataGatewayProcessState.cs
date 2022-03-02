using System.ComponentModel;

namespace BluePrism.DataPipeline
{
    public enum DataGatewayProcessState
    {
        [Description("Unknown")]
        Unknown = 0,
        [Description("Online")]
        Online = 1,
        [Description("Offline")]
        Offline = 2,
        [Description("Error")]
        Error = 3,
        [Description("Running")]
        Running = 4,
        [Description("Starting")]
        Starting = 5,
        [Description("Unrecoverable Error")]
        UnrecoverableError = 6
    }
}