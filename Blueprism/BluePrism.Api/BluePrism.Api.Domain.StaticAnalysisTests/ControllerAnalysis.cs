namespace BluePrism.Api.Domain.StaticAnalysisTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Web.Http;
    using Controllers;
    using Models;
    using Validators;
    using FluentValidation;
    using NUnit.Framework;

    [TestFixture]
    public class ControllerAnalysis
    {
        [Test]
        public void Do_All_Controller_Parameters_That_Implement_IPagingModel_Have_Validators()
        {
            var type = typeof(SessionsController);
            var controllers = type.Assembly.GetTypes()
                                .Where(x => x.Namespace == type.Namespace && typeof(ApiController).IsAssignableFrom(x))
                                .ToArray();

            var allTypes = typeof(PagingTokenValidator<,>)
                .Assembly
                .GetExportedTypes();

            var modelsWithValidators = new HashSet<Type>(
                allTypes
                .Select(x => new { Type = x, RootType = GetRootType(x) })
                .Where(x => x.RootType.IsGenericType && x.RootType.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
                .Select(x => x.RootType.GetGenericArguments().First())
            );

            foreach (var controller in controllers)
            {
                CheckControllerAttributeUsage(controller, modelsWithValidators);
            }
        }

        private static void CheckControllerAttributeUsage(Type controller, HashSet<Type> modelsWithValidators)
        {
            var tokenParametersWithoutValidators = controller
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .SelectMany(method => method
                    .GetParameters()
                    .Where(ParameterImplementsIPagingModel)
                    .Select(parameterInfo => parameterInfo.ParameterType))
                .Distinct()
                .Where(x => !modelsWithValidators.Contains(x))
                .ToList();

            if (tokenParametersWithoutValidators.Count > 0)
            {
                Assert.Fail("Controller parameters" +
                    Environment.NewLine +
                    string.Join(Environment.NewLine, tokenParametersWithoutValidators.Select(x => x.FullName)) +
                    Environment.NewLine +
                    $"that implement '{nameof(IPagingModel<object>)}' must have an accompanying implementation of '{nameof(PagingTokenValidator<object, IPagingModel<object>>)}'");
            }
        }

        private static bool ParameterImplementsIPagingModel(ParameterInfo y) => y.ParameterType.GetInterfaces().Any(z => z.IsGenericType && z.GetGenericTypeDefinition() == typeof(IPagingModel<>));

        private static Type GetRootType(Type type)
        {
            var baseType = type.BaseType;
            var objType = typeof(object);

            while (baseType != objType && baseType != typeof(ValueType) && !type.IsInterface && !baseType.IsInterface)
            {
                type = baseType;
                baseType = type.BaseType;
            }

            return type;
        }
    }
}
