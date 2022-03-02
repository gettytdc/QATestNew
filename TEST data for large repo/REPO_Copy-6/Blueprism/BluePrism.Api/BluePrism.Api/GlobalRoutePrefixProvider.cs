namespace BluePrism.Api
{
    using System.Web.Http.Routing;
    using System.Web.Http.Controllers;
    using Func;

    public class GlobalRoutePrefixProvider : DefaultDirectRouteProvider
    {
        private readonly string _globalRoutePrefix;

        public GlobalRoutePrefixProvider() =>
            _globalRoutePrefix = GlobalRoutePrefix;

        protected override string GetRoutePrefix(HttpControllerDescriptor controllerDescriptor) =>
            base.GetRoutePrefix(controllerDescriptor)
                ?.Map(routePrefix => $"{_globalRoutePrefix}/{routePrefix}")
            ?? _globalRoutePrefix;

        public static string GlobalRoutePrefix => $"api/{ApiVersion}";
        public static string ApiVersion => "v7";
    }
}
