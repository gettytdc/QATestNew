namespace BluePrism.DocumentProcessing.Integration.Messaging
{
    using System;

    public interface IBatchFinishedEventListener : IDisposable
    {
        event BatchFinishedEventListener.DocumentProcessingCompleteEvent DocumentProcessingComplete;
    }
}