namespace BluePrism.Api.Extensions
{
    using System.Linq;
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.JsonPatch.Exceptions;
    using Microsoft.AspNetCore.JsonPatch.Operations;
    using Models;

    public static class JsonPatchDocumentExtensions
    {
        /// <summary>
        /// Create mapper for JsonPatchDocument that will enable you to map to a JsonPatchDocument with a different model. e.g. using Map().To&lt;Type&gt;()
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static JsonPatchDocumentHelper<TModel> Map<TModel>(this JsonPatchDocument<TModel> @this) where TModel : class =>
            new JsonPatchDocumentHelper<TModel>(@this);

        public static bool IsValid<TModel>(this JsonPatchDocument<TModel> @this) where TModel : class =>
            @this?.Operations.Count > 0;

        public static bool TryPatch<TModel>(this JsonPatchDocument<TModel> @this, out PatchModelState<TModel> modelState) where TModel : class, new()
        {
            try
            {
                var model = new TModel();
                @this.ApplyTo(model);
                modelState = new PatchModelState<TModel>(model, string.Empty);
                return true;
            }
            catch (JsonPatchException ex)
            {
                modelState = new PatchModelState<TModel>(null, $"Patch operation error: {ex.Message}");
                return false;
            }
        }
    }

    public class JsonPatchDocumentHelper<TModel> where TModel : class
    {
        private readonly JsonPatchDocument<TModel> _model;

        public JsonPatchDocumentHelper(JsonPatchDocument<TModel> model) =>
            _model = model;

        public JsonPatchDocument<TDestination> To<TDestination>() where TDestination : class =>
            new JsonPatchDocument<TDestination>(
                _model.Operations.Select(x => new Operation<TDestination>(x.op, x.path, x.from, x.value)).ToList(),
                _model.ContractResolver);
    }
}
