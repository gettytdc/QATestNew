namespace BluePrism.Api.Utilities.DocumentationSplitter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Web.Http;
    using Controllers;
    using Func;
    using Microsoft.OpenApi;
    using Microsoft.OpenApi.Extensions;
    using Microsoft.OpenApi.Models;

    public class Program
    {
        private static readonly Regex RouteVariableRegex = new Regex(@"\{(?<v>.+?)(?::.+?)?\}", RegexOptions.Compiled);

        public static void Main(string[] args)
        {
            var documentationPath = args.Any() ? args[0] : Environment.GetEnvironmentVariable("BluePrism.Api.DocumentationPath");

            if (documentationPath == null)
            {
                Console.WriteLine("No documentation file specified. Either pass a path as an argument or set the 'BluePrism.Api.DocumentationPath' environment variable");
                return;
            }

            if (!File.Exists(documentationPath))
            {
                Console.WriteLine($"Documentation file not found at '{documentationPath}'");
                return;
            }

            var openApiSpecification = GetApiSpecificationFromFile(documentationPath);

            var actions =
                GetApiControllerTypes()
                    .Map(GetActionsFromControllers);

            openApiSpecification.Paths =
                GetImplementedSpecificationActions(actions, openApiSpecification.Paths)
                    .Map(GetImplementedPaths)
                    .Map(CreatePathsFromImplementedPaths);

            openApiSpecification.Info.Version = typeof(ResultControllerBase).Assembly.GetName().Version.ToString();

            File.WriteAllText($@".\api-{openApiSpecification.Info.Version}.yaml", openApiSpecification.SerializeAsYaml(OpenApiSpecVersion.OpenApi3_0));
        }

        private class ImplementedSpecificationAction
        {
            public KeyValuePair<string, OpenApiPathItem> Path { get; set; }
            public KeyValuePair<OperationType, OpenApiOperation> Operation { get; set; }
        }

        private static OpenApiPaths CreatePathsFromImplementedPaths(IDictionary<string, OpenApiPathItem> pathItems) =>
            new OpenApiPaths()
                .Tee(p => pathItems.ForEach(x => p.Add(x.Key, x.Value)).Evaluate());

        private static IDictionary<string, OpenApiPathItem> GetImplementedPaths(IEnumerable<ImplementedSpecificationAction> actions) =>
            actions
                .GroupBy(x => x.Path.Key)
                .Select(x => (Path: x.First().Path, Operations: x.Select(m => m.Operation)))
                .ToDictionary(
                    x => x.Path.Key,
                    x =>
                        new OpenApiPathItem
                        {
                            Description = x.Path.Value.Description,
                            Extensions = x.Path.Value.Extensions,
                            Parameters = x.Path.Value.Parameters,
                            Servers = x.Path.Value.Servers,
                            Summary = x.Path.Value.Summary,
                            Operations = x.Operations
                                .ToDictionary(
                                    o => o.Key,
                                    o => o.Value)
                        });

        private static IEnumerable<ImplementedSpecificationAction> GetImplementedSpecificationActions(IEnumerable<(HttpMethod HttpMethod, string Route)> actions, OpenApiPaths specificationPaths)
            =>
            specificationPaths
                .Select(x => (Path: x, Route: x.Key.Map(ReplaceRouteVariables), Methods: x.Value.Operations))
                .SelectMany(x => x.Methods.Select(m => (Path: x.Path, Operation: m, Route: x.Route, Method: m.Key.ToString())))
                .Where(x => actions.Any(a =>
                    a.Route == x.Route &&
                    a.HttpMethod.Method.Equals(x.Method, StringComparison.InvariantCultureIgnoreCase)))
                .Select(x => new ImplementedSpecificationAction
                {
                    Path = x.Path,
                    Operation = x.Operation
                });

        private static OpenApiDocument GetApiSpecificationFromFile(string path) =>
            File.ReadAllText(path)
                .Map(x => new Microsoft.OpenApi.Readers.OpenApiStringReader().Read(x, out _));

        private static Type[] GetApiControllerTypes() =>
            typeof(ResultControllerBase)
                .Assembly.GetExportedTypes()
                .Where(x => x.IsPublic)
                .Where(x => !x.IsAbstract)
                .Where(x => typeof(ResultControllerBase).IsAssignableFrom(x))
                .ToArray();

        private static (HttpMethod HttpMethod, string Route)[] GetActionsFromControllers(IEnumerable<Type> controllerTypes) =>
            controllerTypes
                .SelectMany(c =>
                {
                    var routePrefix = c.GetCustomAttribute<RoutePrefixAttribute>().Prefix;
                    return
                        c.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                            .Select(m => (Method: m, RoutePrefix: routePrefix));
                })
                .Select(x => (Method: x.Method, RoutePrefix: x.RoutePrefix, Route: x.Method.GetCustomAttribute<RouteAttribute>()?.Template))
                .Where(x => x.Route != null)
                .Select(x => (Method: x.Method, Route: $"/{x.RoutePrefix}/{x.Route}".TrimEnd('/')))
                .Select(x => (Method: x.Method, Route: ReplaceRouteVariables(x.Route)))
                .SelectMany(x => GetActionHttpMethods(x.Method).Select(m => ( HttpMethod: m, Route: x.Route )))
                .ToArray();

        private static HttpMethod[] GetActionHttpMethods(MethodInfo method) =>
            method.GetCustomAttributes()
                .Select(x => (Attribute: x, HttpMethods: x.GetType().GetProperty("HttpMethods")))
                .Where(x => x.HttpMethods != null)
                .SelectMany(x => x.HttpMethods.GetValue(x.Attribute) as IEnumerable<HttpMethod>)
                .ToArray();

        private static string ReplaceRouteVariables(string route) =>
            RouteVariableRegex.Replace(route, "{var}");
    }
}
