namespace BluePrism.Api.Extensions
{
    using Middleware;
    using Owin;

    public static class MiddlewareExtensions
    {
        public static IAppBuilder UseApiVersionResponseHeader(this IAppBuilder @this) =>
            @this.Use<VersionResponseHeaderMiddleware>();
    }
}
