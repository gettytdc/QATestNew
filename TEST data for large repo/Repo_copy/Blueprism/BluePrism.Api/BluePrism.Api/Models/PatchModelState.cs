namespace BluePrism.Api.Models
{
    public class PatchModelState<TModel> where TModel : class
    {
        public PatchModelState(TModel model, string failedErrorMessage)
        {
            Model = model;
            FailedErrorMessage = failedErrorMessage;
        }

        public TModel Model { get; set; }
        public string FailedErrorMessage { get; set; }
    }
}
