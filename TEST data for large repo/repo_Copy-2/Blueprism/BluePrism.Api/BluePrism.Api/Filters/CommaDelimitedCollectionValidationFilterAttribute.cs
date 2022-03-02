namespace BluePrism.Api.Filters
{
    using System.Linq;
    using System.Reflection;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;
    using Func;
    using Models;

    /*
     * This is necessary because CommaDelimitedCollection model bind is done via custom model binder, which
     * when validation error is triggered, will bypass validating other properties. So this is a work around.
     */
    public class CommaDelimitedCollectionValidationFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext) =>
            actionContext.ActionArguments.Values
                .Where(x => x != null)
                .ForEach(value => ValidateCommaDelimitedCollectionProperty(actionContext, value))
                .Evaluate();

        private static void ValidateCommaDelimitedCollectionProperty(HttpActionContext actionContext, object value) =>
            value.GetType().GetProperties()
                .ForEach(property =>
                {
                    if (IsCommaDelimitedCollectionType(property))
                        ValidateProperty(actionContext, property, value);
                }).Evaluate();

        private static void ValidateProperty(HttpActionContext actionContext, PropertyInfo property, object value)
        {
            var (hasBoundDataValue, rawStringValues) = GetDataBoundResult(property, value);

            if (hasBoundDataValue is bool hasDataBound && !hasDataBound)
                AddModelError(actionContext, property.Name, rawStringValues);
        }

        private static (object hasBoundDataValue, object rawStringValues) GetDataBoundResult(PropertyInfo property, object value)
        {
            var objValue = property.GetValue(value);
            var dataBoundValue = objValue?.GetType().GetProperty("HasBoundData")?.GetValue(objValue);
            var rawValue = objValue?.GetType().GetProperty("RawStringValues")?.GetValue(objValue);

            return (dataBoundValue, rawValue);
        }

        private static bool IsCommaDelimitedCollectionType(PropertyInfo property) =>
            property.PropertyType.IsGenericType
            && property.PropertyType.GetGenericTypeDefinition() == typeof(CommaDelimitedCollection<>);

        private static void AddModelError(HttpActionContext actionContext, string propertyName, object rawValue) =>
            actionContext.ModelState.AddModelError(propertyName, $"The value '{rawValue}' is not valid for {propertyName}");
    }
}
