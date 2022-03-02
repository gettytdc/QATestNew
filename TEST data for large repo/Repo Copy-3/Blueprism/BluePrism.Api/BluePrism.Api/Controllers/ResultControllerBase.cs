namespace BluePrism.Api.Controllers
{
    using System.Web.Http;
    using Domain.Errors;
    using Errors;
    using Extensions;
    using Func;
    using Microsoft.AspNetCore.JsonPatch;
    using static Func.ResultHelper;

    public abstract class ResultControllerBase : ApiController
    {
        protected Result ValidateModel() =>
            ModelState.IsValid
            ? Succeed()
            : Fail(new ValidationError(ModelState.GetErrors()));

        protected Result ValidatePatchDocumentModel<TModel>(JsonPatchDocument<TModel> patchModel) where TModel : class, new()
        {
            if (!patchModel.IsValid())
                return Fail(new NoPatchOperationsError());

            if (!patchModel.TryPatch(out var patchModelState))
                return Fail(new PatchOperationError(patchModelState.FailedErrorMessage));

            Validate(patchModelState.Model);

            return ValidateModel();
        }
    }
}
