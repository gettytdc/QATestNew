namespace BluePrism.Api.Models
{
    public class CreateWorkQueueRequestModel
    {
        public string Name { get; set; }
        public string KeyField { get; set; }
        public int MaxAttempts { get; set; }
        public QueueStatus Status { get; set; }
        public int EncryptionKeyId { get; set; }
    }
}
