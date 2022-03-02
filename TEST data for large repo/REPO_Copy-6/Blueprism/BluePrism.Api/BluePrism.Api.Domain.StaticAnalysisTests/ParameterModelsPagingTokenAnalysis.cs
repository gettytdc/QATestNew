namespace BluePrism.Api.Domain.StaticAnalysisTests
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Models;
    using NUnit.Framework;

    [TestFixture]
    public class ParameterModelsPagingTokenAnalysis
    {
        [Test]
        public void ParametersModels_ThatHaveAPagingToken_ShouldImplementIPagingModel()
        {
            var parameterModelsWithPagingTokensNotImplementingIPagingModel = typeof(IPagingModel<>)
                .Assembly
                .GetExportedTypes()
                .Where(x => x.GetProperties().Any(PropertyIsAPagingTokenModel))
                .Where(x => !x.IsInterface)
                .Where(DoesNotImplementIPagingModel)
                .ToArray();

            if(parameterModelsWithPagingTokensNotImplementingIPagingModel.Any())
                Assert.Fail($"Parameter models with a {nameof(PagingTokenModel<object>)} must implement the {nameof(IPagingModel<object>)} interface.\r\n\r\n {string.Join("\r\n", parameterModelsWithPagingTokensNotImplementingIPagingModel.Select(x => x.FullName))}");
        }

        private static bool PropertyIsAPagingTokenModel(PropertyInfo propertyInfo) =>
            propertyInfo.PropertyType.IsGenericType &&
            propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(PagingTokenModel<>);

        private static bool DoesNotImplementIPagingModel(Type type) =>
            !type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPagingModel<>));
    }
}
