namespace BluePrism.Api
{
    using System.Collections.Generic;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;
    using Logging;

    public class TraceLoggingActionFilter : ActionFilterAttribute
    {
        private readonly ILoggerFactory _loggerFactory;

        public TraceLoggingActionFilter(ILoggerFactory loggerFactory) =>
            _loggerFactory = loggerFactory;

        public override void OnActionExecuting(HttpActionContext actionContext) =>
            GetLogger(actionContext)
            .Trace($"{actionContext.Request.Method} {actionContext.Request.RequestUri} starting");

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext) =>
            GetLogger(actionExecutedContext.ActionContext)
            .Trace($"{actionExecutedContext.Request.Method} {actionExecutedContext.Request.RequestUri} returned code {actionExecutedContext.Response?.StatusCode ?? 0}");

        private NLog.ILogger GetLogger(HttpActionContext context) =>
            _loggerFactory.GetLogger(context.ControllerContext.RouteData.Values.ValueOrDefault("controller")?.ToString() ?? "CONTROLLER-NOT-FOUND");
    }

    public static class DictionaryExtensionMethods
    {
        public static TValue ValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key) =>
            @this.ContainsKey(key) ? @this[key] : default(TValue);
    }
}
