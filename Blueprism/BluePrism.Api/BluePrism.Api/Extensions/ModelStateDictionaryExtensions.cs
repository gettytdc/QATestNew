namespace BluePrism.Api.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http.ModelBinding;

    public static class ModelStateDictionaryExtensions
    {
        public static IReadOnlyDictionary<string, string> GetErrors(this ModelStateDictionary modelState) =>
            modelState.Values
                .Zip(modelState.Keys, (state, key) =>
                    (Key: key, ErrorValues: string.Join("\r\n", state.Errors.Select(GetValidationErrorMessage).Distinct()))
                )
                .Where(x => !string.IsNullOrEmpty(x.ErrorValues))
                .ToDictionary(x => x.Key, x => x.ErrorValues);

        private static string GetValidationErrorMessage(ModelError modelError) => string.IsNullOrEmpty(modelError.ErrorMessage) && modelError.Exception != null ? modelError.Exception.GetType().Name : modelError.ErrorMessage;
    }
}
