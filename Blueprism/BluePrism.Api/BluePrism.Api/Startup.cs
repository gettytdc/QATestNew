using Microsoft.Owin;

[assembly: OwinStartup(typeof(BluePrism.Api.Startup))]

namespace BluePrism.Api
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Web.Http;

    using Autofac;
    using Autofac.Integration.WebApi;
    using BpLibAdapters.Mappers.FilterMappers;
    using Domain;
    using Filters;
    using FluentValidation.WebApi;
    using Func.AspNet;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;
    using Owin;
    using Swashbuckle.Application;
    using Validators;
    using Func;
    using Mappers.FilterMappers;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.Jwt;
    using Services;
    using SwaggerUI;
    using Extensions;
    using Logging;
    using NLog.Owin.Logging;

    using static LoggingConfiguration;
    using static GlobalRoutePrefixProvider;

    public class Startup
    {
        public void Configuration(IAppBuilder app) =>
            LogStartupErrors(() =>
            {
                var httpConfiguration = new HttpConfiguration();
                var container = ContainerConfigurator.ConfigureContainer(new ContainerBuilder());
                var resolver = new AutofacWebApiDependencyResolver(container);

                app.UseNLog();

                ConfigureFilterMappers(container);

                httpConfiguration.MapHttpAttributeRoutes(new GlobalRoutePrefixProvider());

                httpConfiguration.DependencyResolver = resolver;
                var contractResolver =
                    new CamelCasePropertyNamesContractResolver { NamingStrategy = { ProcessDictionaryKeys = false } };
                httpConfiguration.Formatters.JsonFormatter.SerializerSettings.ContractResolver = contractResolver;
                httpConfiguration.Formatters.JsonFormatter.SupportedMediaTypes
                    .Add(new MediaTypeHeaderValue("text/html"));
                httpConfiguration.Formatters.JsonFormatter.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                httpConfiguration.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                httpConfiguration.Formatters.JsonFormatter.SerializerSettings.Culture = CultureInfo.InvariantCulture;

                httpConfiguration.Formatters.JsonFormatter.SerializerSettings.Converters
                    .Add(new StringEnumConverter());

                var openIdConfiguration = container.Resolve<OpenIdConfiguration>();
                if (ConfigurationManager.AppSettings["Swagger.Enable"].Map(bool.Parse))
                {
                    httpConfiguration
                        .EnableSwagger(c =>
                        {
                            c.SingleApiVersion($"{ApiVersion}_0_0", "Blue Prism API");
                            c.SchemaFilter<PatchSchemaFilter>();
                            c.ApiKey("Bearer")
                                .Description("Insert JWT with 'Bearer' into field")
                                .Name("Authorization")
                                .In("header");
                            c.OAuth2("OAuth2")
                                .Flow("application")
                                .TokenUrl($"{openIdConfiguration.Authority}/connect/token")
                                .Scopes(s =>
                                {
                                    s.Add(openIdConfiguration.Audience, "API scope");
                                    s.Add("bpserver", "Blue Prism Application scope");
                                });
                            c.OperationFilter<AuthOperationFilter>();
                        })
                        .EnableSwaggerUi();
                }

                httpConfiguration.AddResultConversion(configuration =>
                {
                    configuration = configuration.WithErrorResponseConverter(container.Resolve<IErrorResponseConverter>());
                    if (container.IsRegistered<IExceptionResponseConverter>())
                        configuration =
                            configuration.WithExceptionHandler(container.Resolve<IExceptionResponseConverter>());

                    return configuration;
                });

                if (TraceLoggingIsEnabled())
                    httpConfiguration.Filters.Add(
                        httpConfiguration.DependencyResolver.GetService(typeof(TraceLoggingActionFilter)) as
                            TraceLoggingActionFilter);

                httpConfiguration.Filters.Add(container.Resolve<CommaDelimitedCollectionValidationFilterAttribute>());

                FluentValidationModelValidatorProvider.Configure(httpConfiguration, config =>
                {
                    config.ValidatorFactory = new ValidatorFactory(container);
                });

                app.UseApiVersionResponseHeader();
                httpConfiguration.Filters.Add(new AuthorizeAttribute());
                ConfigureAuthentication(app, container, openIdConfiguration);

                app.UseWebApi(httpConfiguration);
            });

        private static void ConfigureAuthentication(IAppBuilder app, IContainer container, OpenIdConfiguration openIdConfiguration)
        {
            var keyResolver = container.Resolve<ISigningKeyResolver>();
            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active,
                TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKeyResolver = (token, securityToken, kid, parameters) => keyResolver.GetSigningKeys(kid),
                    ValidIssuer = openIdConfiguration.Authority ,
                    ValidateIssuerSigningKey = true,
                    RequireSignedTokens = true,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ValidAudience = openIdConfiguration.Audience,
                    ClockSkew = TimeSpan.FromSeconds(5),
                },
            });
            app.Use<TokenAccessMiddleware>(container.Resolve<ITokenAccessor>());

#if DEBUG   //Important that this is never shown in production due to potential GDPR issues
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
#endif
        }

        private static void ConfigureFilterMappers(IContainer container)
        {
            FilterModelMapper.SetFilterModelMappers(container.Resolve<IReadOnlyCollection<IFilterModelMapper>>());
            FilterMapper.SetFilterMappers(container.Resolve<IReadOnlyCollection<IFilterMapper>>());
        }

        private static void LogStartupErrors(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                new LoggerFactory()
                    .GetLogger(nameof(Startup))
                    .Fatal(ex);
                throw;
            }
        }
    }
}
