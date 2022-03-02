namespace BluePrism.DigitalWorker.Messages.Commands.Internal
{
    internal class GetSessionVariablesResponseMessage : GetSessionVariablesResponse
    {
        public bool SessionRunning { get; set; }
        public SessionVariable[] Variables { get; set; }
    }
}
