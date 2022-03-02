namespace BluePrism.Api.Middleware
{
    using System.Threading.Tasks;
    using Microsoft.Owin;

    using static GlobalRoutePrefixProvider;

    public class VersionResponseHeaderMiddleware : OwinMiddleware
    {
        public VersionResponseHeaderMiddleware(OwinMiddleware next) : base(next) { }

        public override async Task Invoke(IOwinContext context)
        {
            context.Response.Headers.Set("api-version", ApiVersion);
            await Next.Invoke(context);
        }
    }
}
