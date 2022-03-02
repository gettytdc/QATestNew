namespace BluePrism.DigitalWorker.Messages.Events.Internal
{
    internal class ProcessTerminatedMessage : ProcessUpdatedMessage, ProcessTerminated
    {
        public TerminationReason TerminationReason { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionMessage { get; set; }
    }
}
