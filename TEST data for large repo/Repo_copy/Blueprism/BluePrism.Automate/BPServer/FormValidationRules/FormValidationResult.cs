namespace BluePrism.BPServer.FormValidationRules
{
    public class FormValidationResult
    {
        public bool Result { get; }
        public string ErrorMessage { get; }

        public FormValidationResult(bool result, string errorMsg)
        {
            Result = result;
            ErrorMessage = errorMsg;
        }
    }
}
