namespace BluePrism.Api.Models
{
    public class PatchOperationErrorModel
    {
        public string Message { get; set; }
        public PatchOperationErrorReason Reason { get; set; }
    }
}
