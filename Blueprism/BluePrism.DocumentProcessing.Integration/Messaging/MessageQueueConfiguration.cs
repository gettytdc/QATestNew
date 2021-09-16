namespace BluePrism.DocumentProcessing.Integration.Messaging
{
    using System.Security;

    public class MessageQueueConfiguration
    {
        public string Username { get; set; }
        public SecureString Password { get; set; }
        public string BrokerUrl { get; set; }
        public string QueueName { get; set; }
    }
}