namespace BluePrism.DigitalWorker.Messages.Events
{
    public interface ProcessTerminated : ProcessUpdated
    {
        TerminationReason TerminationReason { get; }
        string ExceptionType { get; }
        string ExceptionMessage { get; }     
    }
}
