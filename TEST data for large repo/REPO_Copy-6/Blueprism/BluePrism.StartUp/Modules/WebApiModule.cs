using Autofac;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateProcessCore;
using BluePrism.AutomateProcessCore.WebApis.AccessTokens;
using BluePrism.AutomateProcessCore.WebApis.Authentication;
using BluePrism.AutomateProcessCore.WebApis.Credentials;
using BluePrism.AutomateProcessCore.WebApis.CustomCode;
using BluePrism.AutomateProcessCore.WebApis.RequestHandling;
using BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent;
using BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent.Multipart;

namespace BluePrism.StartUp.Modules
{
    /// <summary>
    /// Registers Web API components and dependencies
    /// </summary>
    public class WebApiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ServerCredentialStore>().As<ICredentialStore>();
            builder.RegisterType<HttpRequestBuilder>().As<IHttpRequestBuilder>();
            builder.RegisterType<EmptyAuthenticationHandler>().As<IAuthenticationHandler>();
            builder.RegisterType<BasicAuthenticationHandler>().As<IAuthenticationHandler>();
            builder.RegisterType<BearerTokenAuthenticationHandler>().As<IAuthenticationHandler>();
            builder.RegisterType<OAuth2ClientCredentialsAuthenticationHandler>().As<IAuthenticationHandler>();
            builder.RegisterType<OAuth2JwtBearerTokenAuthenticationHandler>().As<IAuthenticationHandler>();
            builder.RegisterType<CustomAuthenticationHandler>().As<IAuthenticationHandler>();
            builder.RegisterType<AccessTokenPool>().As<IAccessTokenPool>().SingleInstance();
            builder.RegisterType<AuthenticationCredentialHelper>().As< IAuthenticationCredentialHelper >();
            builder.RegisterType<OAuth2ClientCredentialsAccessTokenRequester>().As<IOAuth2ClientCredentialsAccessTokenRequester>();
            builder.RegisterType<OAuth2JwtBearerTokenAccessTokenRequester>().As<IOAuth2JwtBearerTokenAccessTokenRequester>();
            builder.RegisterType<TemplateContentGenerator>().As<IBodyContentGenerator>();
            builder.RegisterType<SingleFileContentGenerator>().As<IBodyContentGenerator>();
            builder.RegisterType<FileCollectionContentGenerator>().As<IBodyContentGenerator>();
            builder.RegisterType<NoBodyContentGenerator>().As<IBodyContentGenerator>();
            builder.RegisterType<CustomCodeContentGenerator>().As<IBodyContentGenerator>();
            builder.RegisterType<CustomCodeBuilder>().As<ICustomCodeBuilder>();
            builder.RegisterType<OutputParameterMapper>();
            builder.RegisterType<JwtBuilder>().As<IJwtBuilder>();
            builder.RegisterType<HttpRequester>().As<IHttpRequester>();
            builder.RegisterType<MultiPartBodyContentBuilder>().As<IMultiPartBodyContentBuilder>();
        }
    }
}
