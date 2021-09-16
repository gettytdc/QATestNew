namespace BluePrism.Api
{
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Owin;
    using Services;

    public class TokenAccessMiddleware : OwinMiddleware
    {
        private readonly ITokenAccessor _tokenAccessor;

        public TokenAccessMiddleware(OwinMiddleware next, ITokenAccessor tokenAccessor) : base(next)
        {
            _tokenAccessor = tokenAccessor;
        }

        public override Task Invoke(IOwinContext context)
        {
            _tokenAccessor.SetToken(context.Request.Headers.Get(HttpRequestHeader.Authorization.ToString()) ?? string.Empty);
            return Next.Invoke(context);
        }
    }
}
