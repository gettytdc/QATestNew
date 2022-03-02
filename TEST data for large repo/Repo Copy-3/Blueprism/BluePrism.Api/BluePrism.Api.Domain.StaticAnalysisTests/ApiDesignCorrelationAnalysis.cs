// ReSharper disable RedundantExplicitTupleComponentName
namespace BluePrism.Api.Domain.StaticAnalysisTests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using Controllers;
    using Func;
    using Microsoft.OpenApi.Any;
    using Microsoft.OpenApi.Models;
    using Models;
    using NUnit.Framework;

    using static GlobalRoutePrefixProvider;

    [TestFixture]
    public class ApiDesignCorrelationAnalysis
    {
        private OpenApiDocument _openApiSpecification;
        private IReadOnlyCollection<Type> _controllerTypes;
        private IReadOnlyCollection<(MethodInfo Method, string Route, string[] RouteVariables, string HttpMethod)> _actions;

        private static readonly Regex RouteVariableRegex = new Regex(@"\{(?<v>.+?)(?::.+?)?\}", RegexOptions.Compiled);

        [OneTimeSetUp]
        public void Setup()
        {
            var documentationPath = Path.Combine(GetRepositoryRootFolder(), "BluePrism.Api", "BluePrism.Api.Specification", "api.yaml");

            if (!File.Exists(documentationPath))
                Assert.Fail($"Documentation file not found at '{documentationPath}'");

            _openApiSpecification =
                File.ReadAllText(documentationPath)
                .Map(x => new Microsoft.OpenApi.Readers.OpenApiStringReader().Read(x, out _));

            _controllerTypes =
                typeof(ResultControllerBase)
                    .Assembly.GetExportedTypes()
                    .Where(x => x.IsPublic)
                    .Where(x => !x.IsAbstract)
                    .Where(x => typeof(ResultControllerBase).IsAssignableFrom(x))
                    .ToArray();

            _actions =
                _controllerTypes
                .SelectMany(c =>
                {
                    var routePrefix = $"{GlobalRoutePrefix}/{c.GetCustomAttribute<RoutePrefixAttribute>().Prefix}";
                    return
                        c.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .Select(m => (Method: m, RoutePrefix: routePrefix));
                })
                .Select(x => (Method: x.Method, RoutePrefix: x.RoutePrefix, Route: x.Method.GetCustomAttribute<RouteAttribute>()?.Template))
                .Where(x => x.Route != null)
                .Select(x => (Method: x.Method, Route: $"/{x.RoutePrefix}/{x.Route}".TrimEnd('/')))
                .Select(x => (Method: x.Method, Route: ReplaceRouteVariables(x.Route), RouteVariables: GetRouteVariables(x.Route)))
                .SelectMany(x => GetActionHttpMethods(x.Method).Select(m => (Method: x.Method, Route: x.Route, RouteVariables: x.RouteVariables, HttpMethod: m)))
                .ToArray();
        }

        [Test]
        public void FindUnimplementedActions()
        {
            var usedActionsWithMethods =
                _actions
                    .Select(x => (Route: x.Route, Method: GetHttpMethodNameFromMethod(x.Method)));

            var expectedActionsWithMethods =
                _openApiSpecification.Paths
                    .Select(x => (Route: x.Key.Map(ReplaceRouteVariables), Methods: x.Value.Operations.Keys))
                    .SelectMany(x => x.Methods.Select(m => (Route: x.Route, Method: m.ToString())));

            var unimplementedActions =
                expectedActionsWithMethods.Except(usedActionsWithMethods)
                .ToArray();

            if (unimplementedActions.Any())
            {
                Assert.Warn($"The following {unimplementedActions.Length} Actions were documented but not implemented:\r\n\t{string.Join("\r\n\t", unimplementedActions.Select(x => $"{x.Method} {x.Route}"))}");
            }
        }

        [Test]
        public void FindUndocumentedActions()
        {
            var usedActionsWithMethods =
                _actions
                    .Select(x => (Route: x.Route, Method: GetHttpMethodNameFromMethod(x.Method)));

            var expectedActionsWithMethods =
                _openApiSpecification.Paths
                    .Select(x => (Route: x.Key.Map(ReplaceRouteVariables), Methods: x.Value.Operations.Keys))
                    .SelectMany(x => x.Methods.Select(m => (Route: x.Route, Method: m.ToString())));

            var undocumentedActions =
                usedActionsWithMethods.Except(expectedActionsWithMethods)
                    .ToArray();

            if (undocumentedActions.Any())
            {
                Assert.Fail($"The following {undocumentedActions.Length} Actions were implemented but not documented:\r\n\t{string.Join("\r\n\t", undocumentedActions.Select(x => $"{x.Method} {x.Route}"))}");
            }
        }

        [Test]
        public void CheckReturnedModelsAgainstDocumentation()
        {
            var returnModels =
                _actions
                    .Select(x => (Route: x.Route, HttpMethod: x.HttpMethod, Returns: x.Method.ReturnType))
                    .Where(x => x.Returns.IsGenericType && x.Returns.GetGenericTypeDefinition() == typeof(Task<>) && x.Returns.GetGenericArguments().Length == 1)
                    .Select(x => (Route: x.Route, HttpMethod: x.HttpMethod, Returns: x.Returns.GetGenericArguments()[0]))
                    .Where(x => typeof(Result).IsAssignableFrom(x.Returns))
                    .Select(x => (Route: x.Route, HttpMethod: x.HttpMethod, Model: x.Returns.IsGenericType ? x.Returns.GetGenericArguments().Single() : null));

            var expectedActionsWithMethodsAndModels =
                _openApiSpecification.Paths
                    .Select(x => (Route: x.Key.Map(ReplaceRouteVariables), Methods: x.Value.Operations))
                    .SelectMany(x => x.Methods.Keys.Select(m => (Route: x.Route, Method: m.ToString(), ResponseSchema: GetSchemaForSuccessResponse(x.Methods[m]))));

            var actionReturnModels = expectedActionsWithMethodsAndModels
                .Join(
                    returnModels,
                    x => (Route: x.Route, Method: x.Method),
                    x => (Route: x.Route, Method: x.HttpMethod),
                    (x, y) => (Route: x.Route, Method: x.Method, Model: y.Model, ResponseSchema: x.ResponseSchema));

            var results =
                actionReturnModels
                .Select(x => (Route: x.Route, Method: x.Method, TypeMatch: SchemaAndModelTypesMatch(x.ResponseSchema, x.Model), Differences: GetModelAndSchemaDifferences(x.ResponseSchema, x.Model)))
                .ToArray();

            if (results.Any(x => !x.TypeMatch))
            {
                Assert.Fail($"The following differences have been found in the return types for actions:\r\n\t{string.Join("\r\n\t", results.Where(x => !x.TypeMatch).Select(x => GetMessageForResult(x.Route, x.Method, string.Empty, x.Differences.OnlyOnSchema, x.Differences.OnlyOnModel)))}");
            }
        }

        [Test]
        public void CheckExamplesAgainstScheme()
        {
            var returnModels =
                _actions
                    .Select(x => (Route: x.Route, HttpMethod: x.HttpMethod, Returns: x.Method.ReturnType))
                    .Where(x => x.Returns.IsGenericType && x.Returns.GetGenericTypeDefinition() == typeof(Task<>) && x.Returns.GetGenericArguments().Length == 1)
                    .Select(x => (Route: x.Route, HttpMethod: x.HttpMethod, Returns: x.Returns.GetGenericArguments()[0]))
                    .Where(x => typeof(Result).IsAssignableFrom(x.Returns))
                    .Select(x => (Route: x.Route, HttpMethod: x.HttpMethod, Model: x.Returns.IsGenericType ? x.Returns.GetGenericArguments().Single() : null));

            var expectedActionsWithMethodsAndModels =
                _openApiSpecification.Paths
                    .Select(x => (Route: x.Key.Map(ReplaceRouteVariables), Methods: x.Value.Operations))
                    .SelectMany(x => x.Methods.Keys.Select(m => (Route: x.Route, Method: m.ToString(), ResponseSchema: GetSchemaForSuccessResponse(x.Methods[m]))));

            var actionReturnModels = expectedActionsWithMethodsAndModels
                .Join(
                    returnModels,
                    x => (Route: x.Route, Method: x.Method),
                    x => (Route: x.Route, Method: x.HttpMethod),
                    (x, y) => (Route: x.Route, Method: x.Method, Model: y.Model, ResponseSchema: x.ResponseSchema));

            var results =
                actionReturnModels
                .Select(x => (Route: x.Route, Method: x.Method, TypeMatch: SchemaAndExampleTypesMatch(x.ResponseSchema), Differences: GetExampleAndSchemaDifferences(x.ResponseSchema)))
                .ToArray();

            if (results.Any(x => !x.TypeMatch))
            {
                Assert.Fail($"The following differences have been found in the return types for actions:\r\n\t{string.Join("\r\n\t", results.Where(x => !x.TypeMatch).Select(x => GetMessageForResult(x.Route, x.Method, string.Empty, x.Differences.OnlyOnSchema, x.Differences.OnlyOnExample, true)))}");
            }
        }

        [Test]
        public void CheckInputParametersAgainstDocumentation()
        {
            var expectedActionsWithInputModels =
                _openApiSpecification.Paths
                    .Select(x => (Route: ReplaceRouteVariables(x.Key), RouteVariables: GetRouteVariables(x.Key), Methods: x.Value.Operations))
                    .SelectMany(x => x.Methods.Keys.Select(m => (Route: x.Route, RouteVariables: x.RouteVariables, Method: m.ToString(), Parameters: GetSchemaActionInputs(x.Methods[m]))));

            var actionParameters =
                _actions.Select(GetParametersForAction);

            var actionInputModels = expectedActionsWithInputModels
                .Join(
                    actionParameters,
                    x => (Route: x.Route, Method: x.Method),
                    x => (Route: x.Route, Method: x.HttpMethod),
                    (x, y) => (
                        Route: x.Route,
                        Method: x.Method,
                        SchemaParameters: x.Parameters.Except(x.RouteVariables).Select(p => p.ToLowerInvariant()),
                        ModelParameters: y.Parameters.Except(y.RouteVariables).Select(p => p.ToLowerInvariant())));

            var results =
                actionInputModels
                    .Where(x => x.Method != "Patch") //Patch is tricky - probably worth giving it its own test at some point
                    .Select(x => (
                        Route: x.Route,
                        Method: x.Method,
                        OnlyOnModel: x.ModelParameters.Except(x.SchemaParameters).ToArray(),
                        OnlyOnSchema: x.SchemaParameters.Except(x.ModelParameters).ToArray()))
                    .Where(x => x.OnlyOnModel.Any() || x.OnlyOnSchema.Any())
                    .ToArray();

            if (results.Any())
            {
                Assert.Fail($"The following differences have been found in the input types for actions:\r\n\t{string.Join("\r\n\t", results.Select(x => GetMessageForResult(x.Route, x.Method, string.Empty, x.OnlyOnSchema, x.OnlyOnModel)))}");
            }
        }

        [Test]
        public void CheckInputRouteParametersAgainstDocumentation()
        {
            var expectedActionsWithInputModels =
                _openApiSpecification.Paths
                    .Select(x => (Route: ReplaceRouteVariables(x.Key), RouteVariables: GetRouteVariables(x.Key), Methods: x.Value.Operations))
                    .SelectMany(x => x.Methods.Keys.Select(m => (Route: x.Route, RouteVariables: x.RouteVariables, Method: m.ToString())));

            var actionParameters =
                _actions.Select(GetParametersForAction);

            var actionInputModels = expectedActionsWithInputModels
                .Join(
                    actionParameters,
                    x => (Route: x.Route, Method: x.Method),
                    x => (Route: x.Route, Method: x.HttpMethod),
                    (x, y) => (
                        Route: x.Route,
                        Method: x.Method,
                        SchemaParameters: x.RouteVariables.Select(p => p.ToLowerInvariant()),
                        ModelParameters: y.RouteVariables.Select(p => p.ToLowerInvariant())));

            var results =
                actionInputModels
                    .Select(x => (
                        Route: x.Route,
                        Method: x.Method,
                        OnlyOnModel: x.ModelParameters.Except(x.SchemaParameters).ToArray(),
                        OnlyOnSchema: x.SchemaParameters.Except(x.ModelParameters).ToArray()))
                    .Where(x => x.OnlyOnModel.Any() || x.OnlyOnSchema.Any())
                    .ToArray();

            if (results.Any())
            {
                Assert.Fail($"The following differences have been found in the input types for actions:\r\n\t{string.Join("\r\n\t", results.Select(x => GetMessageForResult(x.Route, x.Method, string.Empty, x.OnlyOnSchema, x.OnlyOnModel)))}");
            }
        }

        [Test]
        public void CheckInputEnumsAgainstDocumentation()
        {
            var expectedActionsWithEnumInputValues =
                _openApiSpecification.Paths
                    .Select(x => (Route: ReplaceRouteVariables(x.Key), Methods: x.Value.Operations))
                    .SelectMany(x => x.Methods.Keys.Select(m => (Route: x.Route, Method: m.ToString(), EnumParameters: GetSchemaActionInputEnums(x.Methods[m]))))
                    .Where(x => x.EnumParameters.Any());

            var actionParameters =
                _actions.Select(GetEnumParametersForAction);

            var actionInputModels = expectedActionsWithEnumInputValues
                .Join(
                    actionParameters,
                    x => (Route: x.Route, Method: x.Method),
                    x => (Route: x.Route, Method: x.HttpMethod),
                    (x, y) => (
                        Route: x.Route,
                        Method: x.Method,
                        EnumParameters: x.EnumParameters.Join(
                            y.EnumParameters,
                            z => z.Name.ToLowerInvariant(),
                            z => z.Name.ToLowerInvariant(),
                            (s, m) => (Name: m.Name, SchemaValues: s.EnumValues, ModelValues: m.Values))))
                .SelectMany(x => x.EnumParameters.Select(p => (Route: x.Route, Method: x.Method, Name: p.Name, ModelValues: p.ModelValues, SchemaValues: p.SchemaValues)));

            var results =
                actionInputModels
                    .Select(x => (
                        Route: x.Route,
                        Method: x.Method,
                        Name: x.Name,
                        OnlyOnSchema: x.SchemaValues.Select(v => v.ToLowerInvariant()).Except(x.ModelValues.Select(v => v.ToLowerInvariant())).ToArray(),
                        OnlyOnModel: x.ModelValues.Select(v => v.ToLowerInvariant()).Except(x.SchemaValues.Select(v => v.ToLowerInvariant())).ToArray()
                    ))
                    .Where(x => x.OnlyOnModel.Any() || x.OnlyOnSchema.Any())
                    .ToArray();

            if (results.Any())
            {
                Assert.Fail($"The following differences have been found in the input enums for actions:\r\n\t{string.Join("\r\n\t", results.Select(x => GetMessageForResult(x.Route, x.Method, x.Name, x.OnlyOnSchema, x.OnlyOnModel)))}");
            }
        }

        [Test]
        public void EnsureAllSchemasHaveDescriptions()
        {
            var schemasWithoutDescriptions =
                _openApiSpecification.Components.Schemas
                    .Where(x => string.IsNullOrWhiteSpace(x.Value.Description))
                    .Select(x => x.Key)
                    .ToArray();

            if (schemasWithoutDescriptions.Any())
            {
                Assert.Fail($"The following schemas do not have descriptions:\r\n\t{string.Join("\r\n\t", schemasWithoutDescriptions)}");
            }
        }

        [Test]
        public void EnsureAllSchemaFieldsHaveDescriptions()
        {
            var schemaFieldsWithoutDescriptions =
                _openApiSpecification.Components.Schemas
                    .SelectMany(x =>
                        x.Value.Properties
                            .Where(p => string.IsNullOrWhiteSpace(p.Value.Description))
                            .Select(p => $"{x.Key}.{p.Key}"))
                    .ToArray();

            if (schemaFieldsWithoutDescriptions.Any())
            {
                Assert.Fail($"The following schema fields do not have descriptions:\r\n\t{string.Join("\r\n\t", schemaFieldsWithoutDescriptions)}");
            }
        }

        [Test]
        public void EnsureAllOperationsHaveSummaries()
        {
            var operationsWithoutDescriptions =
                _openApiSpecification.Paths
                    .SelectMany(x =>
                        x.Value.Operations
                            .Where(o => string.IsNullOrWhiteSpace(o.Value.Summary))
                            .Select(o => $"{o.Key.ToString().ToUpperInvariant()} {x.Key}"))
                    .ToArray();

            if (operationsWithoutDescriptions.Any())
            {
                Assert.Fail($"The following operations do not have descriptions:\r\n\t{string.Join("\r\n\t", operationsWithoutDescriptions)}");
            }
        }

        [Test]
        public void EnsureAllOperationParametersHaveDescriptions()
        {
            var operationParametersWithoutDescriptions =
                _openApiSpecification.Paths
                    .SelectMany(x =>
                        x.Value.Operations
                            .Select(o => (Path: $"{o.Key.ToString().ToUpper()} {x.Key}", Operation: o.Value)))
                    .SelectMany(x =>
                        x.Operation.Parameters
                            .Where(p => string.IsNullOrWhiteSpace(p.Description))
                            .Select(p => $"{x.Path} [{p.Name}]"))
                    .ToArray();

            if (operationParametersWithoutDescriptions.Any())
            {
                Assert.Fail($"The following operation parameters do not have descriptions:\r\n\t{string.Join("\r\n\t", operationParametersWithoutDescriptions)}");
            }
        }

        private static string GetHttpMethodNameFromMethod(MethodInfo method) =>
            method.GetCustomAttributes()
                .Single(x => x is IActionHttpMethodProvider)
                .Map(x =>
                    x is HttpGetAttribute ? "Get"
                    : x is HttpPostAttribute ? "Post"
                    : x is HttpPutAttribute ? "Put"
                    : x is HttpPatchAttribute ? "Patch"
                    : x is HttpDeleteAttribute ? "Delete"
                    : throw new ArgumentException("Unexpected attribute type"));

        private static string ReplaceRouteVariables(string route) =>
            RouteVariableRegex.Replace(route, "{var}");

        private static string[] GetRouteVariables(string route) =>
            RouteVariableRegex.Matches(route)
                .OfType<Match>()
                .Select(x => x.Groups["v"].Value)
                .ToArray();

        private static bool SchemaAndModelTypesMatch(OpenApiSchema schema, Type model)
        {
            if (model == null || schema == null)
                return model == null && schema == null;

            if (model.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(model.GetGenericTypeDefinition()))
            {
                if (schema.Type != "array" || schema.Items == null)
                    return false;

                schema = schema.Items;
                model = model.GetGenericArguments().Single();
            }

            var schemaPropertyNames = schema.Properties.Keys.Select(x => x.ToLowerInvariant()).ToArray();
            var modelPropertyNames = model.GetProperties().Select(x => x.Name).Select(x => x.ToLowerInvariant()).ToArray();

            var schemaPropertyTypes = schema.Properties
                .Select(x => (Name: x.Key.ToLowerInvariant(), Type: x.Value.Type))
                .Join(
                    model.GetProperties(),
                    x => x.Name,
                    x => x.Name.ToLowerInvariant(),
                    (s, m) => (
                        Name: s.Name,
                        SchemaType: s.Type,
                        ModelType: m.PropertyType.Map(GetSchemaEquivalentType)));

            return
                SchemaAndModelGenericArgumentTypesMatch(schema, model)
                && !schemaPropertyNames.Except(modelPropertyNames).Any()
                && !modelPropertyNames.Except(schemaPropertyNames).Any()
                && schemaPropertyTypes.All(x => x.ModelType == x.SchemaType);
        }

        private static bool SchemaAndExampleTypesMatch(OpenApiSchema schema)
        {
            if (schema == null)
            {
                return true;
            }

            if (schema.Type == "array" || schema.Items != null)
            {
                schema = schema.Items;
            }

            if (schema.Example == null)
            {
                if (schema.Properties.ContainsKey("items"))
                {
                    return SchemaAndExampleTypesMatch(schema.Properties["items"].Items);
                }

                return true;
            }

            var schemaPropertyNames = schema.Properties.Keys.Select(x => x.ToLowerInvariant()).ToArray();

            var schemaExampleObject = (OpenApiObject)schema.Example;
            var examplePropertyNames = schemaExampleObject.Keys.Select(x => x.ToLowerInvariant()).ToArray();

            var schemaPropertyTypes = schema.Properties
                .Select(x => (Name: x.Key.ToLowerInvariant(), Type: x.Value.Type))
                .Join(
                    schemaExampleObject,
                    x => x.Name,
                    x => x.Key.ToLowerInvariant(),
                    (s, m) => (
                        Name: s.Name,
                        SchemaType: s.Type,
                        ModelType: OpenApiTypeToSchemaEquivalentType(m.Value.GetType())));


            return
                SchemaAndExampleGenericArgumentTypesMatch(schema)
                && !schemaPropertyNames.Except(examplePropertyNames).Any()
                && !examplePropertyNames.Except(schemaPropertyNames).Any()
                && schemaPropertyTypes.All(x => x.ModelType == x.SchemaType);
        }

        private static bool SchemaAndModelGenericArgumentTypesMatch(OpenApiSchema schema, Type model)
        {
            if (!model.IsGenericType || !typeof(ItemsPageModel<>).IsAssignableFrom(model.GetGenericTypeDefinition()))
                return true;

            schema = schema.Properties["items"].Items;
            model = model.GetGenericArguments().Single();

            return SchemaAndModelTypesMatch(schema, model);
        }

        private static bool SchemaAndExampleGenericArgumentTypesMatch(OpenApiSchema schema)
        {
            if (schema?.Example != null)
            {
                if (((OpenApiObject)schema.Example).ContainsKey("items"))
                {
                    if (schema.Properties.ContainsKey("items"))
                    {
                        var parentSchemaNestedExample =
                            ((OpenApiArray)((OpenApiObject)schema.Example)["items"]).FirstOrDefault();

                        schema = schema.Properties["items"].Items;
                        schema.Example = parentSchemaNestedExample;

                        return SchemaAndExampleTypesMatch(schema);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (schema.Properties.ContainsKey("items"))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static string GetSchemaEquivalentType(Type type)
        {
            if (type.IsGenericType && typeof(Nullable<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
                type = type.GetGenericArguments().Single();

            if (type == typeof(string))
                return "string";
            if (type.IsEnum)
            {
                return
                    type.GetCustomAttribute<FlagsAttribute>() == null
                        ? "string"
                        : "array";
            }
            if (type == typeof(DateTimeOffset))
                return "string";
            if (type == typeof(DateTime))
                return "string"; // Ideally we'll remove this in the future once all API endpoints use DateTimeOffset and not DateTime
            if (type == typeof(TimeSpan))
                return "string";
            if (type == typeof(Guid))
                return "string";

            if (type.IsArray)
                return "array";
            if (type.IsGenericType && (
                typeof(IEnumerable<>).IsAssignableFrom(type.GetGenericTypeDefinition())
                || typeof(IReadOnlyCollection<>).IsAssignableFrom(type.GetGenericTypeDefinition())))
            {
                return "array";
            }

            if (type == typeof(bool))
                return "boolean";

            if (type == typeof(long) || type == typeof(ulong) || type == typeof(int) || type == typeof(short) || type == typeof(ushort) || type == typeof(byte) || type == typeof(sbyte))
                return "integer";

            return "object";
        }

        public static string OpenApiTypeToSchemaEquivalentType(Type type)
        {
            if (type == typeof(OpenApiString) || type == typeof(OpenApiDateTime))
                return "string";

            if (type == typeof(OpenApiBoolean))
                return "boolean";

            if (type == typeof(OpenApiInteger))
                return "integer";

            if (type == typeof(OpenApiArray))
                return "array";

            return "object";
        }

        private static (string[] OnlyOnSchema, string[] OnlyOnModel) GetModelAndSchemaDifferences(OpenApiSchema schema, Type model)
        {
            if (model == null || schema == null)
                return (new string[0], new string[0]);

            if (model.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(model.GetGenericTypeDefinition()))
            {
                if (schema.Type != "array" || schema.Items == null)
                    return (new string[0], new string[0]);

                schema = schema.Items;
                model = model.GetGenericArguments().Single();
            }

            var schemaPropertyNames = schema.Properties.Keys.Select(x => x.ToLowerInvariant()).ToArray();
            var modelPropertyNames = model.GetProperties().Select(x => x.Name).Select(x => x.ToLowerInvariant()).ToArray();

            return (
                schemaPropertyNames.Except(modelPropertyNames).ToArray(),
                modelPropertyNames.Except(schemaPropertyNames).ToArray());
        }

        private static (string[] OnlyOnSchema, string[] OnlyOnExample) GetExampleAndSchemaDifferences(OpenApiSchema schema)
        {
            if (schema?.Example == null)
                return (new string[0], new string[0]);

            var schemaPropertyNames = schema.Properties.Keys.Select(x => x.ToLowerInvariant()).ToArray();

            var schemaExampleObject = (OpenApiObject)schema.Example;
            var examplePropertyNames = schemaExampleObject.Keys.Select(x => x.ToLowerInvariant()).ToArray();

            return (
                schemaPropertyNames.Except(examplePropertyNames).ToArray(),
                examplePropertyNames.Except(schemaPropertyNames).ToArray());
        }

        private static (string Route, string[] RouteVariables, string HttpMethod, string[] Parameters) GetParametersForAction((MethodInfo Method, string Route, string[] RouteVariables, string HttpMethod) parameters) =>
        (
            parameters.Route,
            parameters.RouteVariables,
            parameters.HttpMethod,
            parameters.Method.GetParameters().SelectMany(BreakDownParameter).Select(x => x.Name).ToArray()
        );

        private static (string Route, string HttpMethod, (string Name, string[] Values)[] EnumParameters) GetEnumParametersForAction((MethodInfo Method, string Route, string[] RouteVariables, string HttpMethod) parameters) =>
        (
            parameters.Route,
            parameters.HttpMethod,
            parameters.Method.GetParameters()
                .SelectMany(BreakDownParameter)
                .Select(x => (Name: x.Name, Type: GetBaseTypeForExpectedGenerics(x.Type)))
                .Where(x => x.Type.IsEnum)
                .Select(x => (x.Name, Enum.GetNames(x.Type))).ToArray()
        );

        private static bool IsExpectedGeneric(Type type)
        {
            if (!type.IsGenericType)
                return false;

            var typeDefinition = type.GetGenericTypeDefinition();

            return
                typeDefinition == typeof(Nullable<>)
                || typeDefinition == typeof(BasicFilterModel<>)
                || typeDefinition == typeof(CommaDelimitedCollection<>);
        }

        private static Type GetBaseTypeForExpectedGenerics(Type type) =>
            IsExpectedGeneric(type) ? type.GetGenericArguments()[0] : type;

        private static Type GetTypeWithUnwrappedEnumerables(Type type) =>
            type.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(type.GetGenericTypeDefinition())
            ? type.GetGenericArguments().Single()
            : type;

        private static IEnumerable<(string Name, Type Type)> BreakDownParameter(ParameterInfo parameter) =>
            IsAtomicType(GetTypeWithUnwrappedEnumerables(parameter.ParameterType))
                ? new[] { (parameter.Name, GetTypeWithUnwrappedEnumerables(parameter.ParameterType)) }
                : parameter.ParameterType.GetProperties(BindingFlags.Instance | BindingFlags.Public).SelectMany(BreakDownProperty);

        private static IEnumerable<(string Name, Type Type)> BreakDownProperty(PropertyInfo property) =>
            IsAtomicType(property.PropertyType)
                ? new[] { (property.Name, property.PropertyType) }
                : property.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public).SelectMany(BreakDownProperty).Select(x => ($"{property.Name}.{x.Name}", x.Type));

        private static IEnumerable<(string Name, OpenApiSchema Schema)> GetSchemaActionInputSchemas(OpenApiOperation operation) =>
            operation.Parameters.Where(p => !IsComplexTypeOrFilter(p)).Select(p => (p.Name, p.Schema))
                .Concat(operation.RequestBody?.Content["application/json"].Schema.Properties.Select(p => (p.Key, p.Value)) ?? new (string, OpenApiSchema)[0])
                .Concat(operation.Parameters.SelectMany(p => GetNestedSchemaProperties(p.Schema).Select(o => ($"{p.Name}.{o}", p.Schema))));

        private static bool IsComplexTypeOrFilter(OpenApiParameter parameter) =>
            parameter.Schema.OneOf.Any()
            || parameter.Schema.AllOf.Any()
            || parameter.Schema.AnyOf.Any()
            || (parameter.Schema.Reference?.Id.Equals("EqualsFilter") ?? false);

        private static IEnumerable<string> GetSchemaActionInputs(OpenApiOperation operation) =>
            GetSchemaActionInputSchemas(operation)
                .Select(x => x.Name);

        private static IEnumerable<(string Name, IEnumerable<string> EnumValues)> GetSchemaActionInputEnums(OpenApiOperation operation) =>
            GetSchemaActionInputSchemas(operation)
                .Select(x => (Name: x.Name, Schema: x.Schema.Type == "array" ? x.Schema.Items : x.Schema))
                .Where(x => x.Schema.Enum.Any())
                .Select(x => (x.Name, x.Schema.Enum.OfType<Microsoft.OpenApi.Any.OpenApiString>().Select(e => e.Value)));

        private static IEnumerable<string> GetNestedSchemaProperties(OpenApiSchema schema) =>
            schema.Properties.Select(p => p.Key)
                .Concat(schema.AllOf.SelectMany(GetNestedSchemaProperties))
                .Concat(schema.AnyOf.SelectMany(GetNestedSchemaProperties))
                .Concat(schema.OneOf.SelectMany(GetNestedSchemaProperties));

        private static bool IsAtomicType(Type type) =>
            type.IsValueType
            || type.IsEnum
            || type == typeof(string)
            || typeof(IEnumerable).IsAssignableFrom(type)
            || TypeDescriptor.GetConverter(type).CanConvertFrom(typeof(string));

        private static string[] GetActionHttpMethods(MethodInfo method) =>
            method.GetCustomAttributes()
                .Select(x => (Attribute: x, HttpMethods: x.GetType().GetProperty("HttpMethods")))
                .Where(x => x.HttpMethods != null)
                .SelectMany(x => x.HttpMethods.GetValue(x.Attribute) as IEnumerable<HttpMethod>)
                .Select(x => x.Method)
                .Select(NormalizeMethodCasing)
                .ToArray();

        private static string NormalizeMethodCasing(string method) =>
            method[0].ToString().ToUpperInvariant() + method.Substring(1).ToLowerInvariant();

        private static string GetMessageForResult(string route, string method, string extraInfo, string[] onlyOnSchema, string[] onlyOnModel, bool isExampleCheck = false)
        {
            var targetEntity = isExampleCheck ? "example" : "model";
            var resultBuilder = new StringBuilder();
            if (string.IsNullOrEmpty(extraInfo))
                resultBuilder.AppendLine($"{route} ({method}):");
            else
                resultBuilder.AppendLine($"{route} ({method}) - {extraInfo}:");


            if (!onlyOnSchema.Any() && !onlyOnModel.Any())
                resultBuilder.AppendLine($"\t\tThe {targetEntity} differ entirely");

            if (onlyOnSchema.Any())
                resultBuilder.AppendLine($"\t\tOnly {targetEntity} contains: {string.Join(", ", onlyOnSchema)}");
            if (onlyOnModel.Any())
                resultBuilder.AppendLine($"\t\tOnly {targetEntity} contains: {string.Join(", ", onlyOnModel)}");

            return resultBuilder.ToString();
        }

        private static OpenApiResponse GetSuccessResponse(OpenApiOperation operation) =>
            operation.Responses.Single(x => x.Key.StartsWith("2")).Value;

        private static OpenApiSchema GetSchemaForSuccessResponse(OpenApiOperation operation) =>
            GetSuccessResponse(operation).Content.TryGetValue("application/json", out var c)
                ? c.Schema
                : null;

        private static IEnumerable<string> GetParentDirectories(string path) =>
            path != null
                ? new[] { path }.Concat(GetParentDirectories(Directory.GetParent(path)?.FullName))
                : new string[0];

        private static string GetRepositoryRootFolder() =>
            GetRepositoryRootFolderOrDefaultFromPath(Assembly.GetExecutingAssembly().Location)
            ?? GetRepositoryRootFolderOrDefaultFromPath(Directory.GetCurrentDirectory())
            ?? throw new AssertionException("Cannot find repository root folder");

        private static string GetRepositoryRootFolderOrDefaultFromPath(string path) =>
            path
            .Map(Path.GetDirectoryName)
            .Map(GetParentDirectories)
            .FirstOrDefault(x => Directory.GetDirectories(x, ".git").Any());

    }
}
