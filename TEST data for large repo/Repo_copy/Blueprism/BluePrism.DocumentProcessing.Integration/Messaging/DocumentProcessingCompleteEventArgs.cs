namespace BluePrism.DocumentProcessing.Integration.Messaging
{
    using System;

    public class DocumentProcessingCompleteEventArgs : EventArgs
    {
        public Guid BatchId { get; set; }
        public (Guid Id, Guid Type)[] Documents { get; set; }
    }
}