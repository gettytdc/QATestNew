using System;
using MassTransit;

namespace BluePrism.DigitalWorker.UnitTests.Messaging
{
    internal static class TestResponse
    {
        public static TestResponse<T> Create<T>(T message) where T : class
        {
            return new TestResponse<T>(message);
        }
    }

    internal class TestResponse<T> : Response<T> where T : class
    {
        public TestResponse(T message)
        {
            Message = message;
        }

        public Guid? MessageId { get; set; }
        public Guid? RequestId { get; set; }
        public Guid? CorrelationId { get; set; }
        public Guid? ConversationId { get; set; }
        public Guid? InitiatorId { get; set; }
        public DateTime? ExpirationTime { get; set; }
        public Uri SourceAddress { get; set; }
        public Uri DestinationAddress { get; set; }
        public Uri ResponseAddress { get; set; }
        public Uri FaultAddress { get; set; }
        public DateTime? SentTime { get; set; }
        public Headers Headers { get; set; }
        public HostInfo Host { get; set; }
        public T Message { get;  }
        object Response.Message { get; }
    }
}
