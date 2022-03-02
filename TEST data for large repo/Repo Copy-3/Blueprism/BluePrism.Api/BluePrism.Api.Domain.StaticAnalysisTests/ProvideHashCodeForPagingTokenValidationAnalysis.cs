namespace BluePrism.Api.Domain.StaticAnalysisTests
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Models;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using PagingTokens;

    [TestFixture]
    public class ProvideHashCodeForPagingTokenValidationAnalysis
    {
        [Test]
        public void ParametersModels_ThatHaveProvideHashCodeForPagingTokenValidation_ShouldUseCustomConverterOnItemsPageProperty()
        {
            var parameterModelsWithoutCustomConverterOnItemsPage = typeof(IProvideHashCodeForPagingTokenValidation)
                .Assembly
                .GetExportedTypes()
                .Where(DoesImplementIProvideHashCodeForPagingTokenValidation)
                .Where(x => x.GetProperties().Any(DoesNotHaveItemsPerPageConverterOnTheItemsPerPageProperty))
                .ToArray();

            if (parameterModelsWithoutCustomConverterOnItemsPage.Any())
            {
                var failingParameterObjects = string.Join("\r\n", parameterModelsWithoutCustomConverterOnItemsPage.Select(x => x.FullName));
                Assert.Fail($"Parameters implementing {nameof(IProvideHashCodeForPagingTokenValidation)} need to use ItemsPerPageConverter on {nameof(ItemsPerPage)} property..\r\n\r\n {failingParameterObjects}");
            }
        }

        [Test]
        public void ParametersModels_ThatHaveProvideHashCodeForPagingTokenValidation_ShouldHaveJsonIgnoreAttributeOnPagingToken()
        {
            var parameterModelsWithoutJsonIgnoreOnPagingToken = typeof(IProvideHashCodeForPagingTokenValidation)
                .Assembly
                .GetExportedTypes()
                .Where(DoesImplementIProvideHashCodeForPagingTokenValidation)
                .Where(x => x.GetProperties().Any(DoesNotHavePagingTokenWithJsonIgnoreAttribute))
                .ToArray();

            if (parameterModelsWithoutJsonIgnoreOnPagingToken.Any())
            {
                var failingParameterObjects = string.Join("\r\n", parameterModelsWithoutJsonIgnoreOnPagingToken.Select(x => x.FullName));
                Assert.Fail($"Parameters implementing {nameof(IProvideHashCodeForPagingTokenValidation)} need to JsonIgnore PagingToken property..\r\n\r\n {failingParameterObjects}");
            }
        }

        private static bool DoesNotHavePagingTokenWithJsonIgnoreAttribute(PropertyInfo propertyInfo) =>
            propertyInfo.PropertyType.IsGenericType &&
            propertyInfo.PropertyType.GenericTypeArguments.Any(x => x.Name.Contains(typeof(PagingToken<>).Name)) &&
            propertyInfo.GetCustomAttributes<JsonIgnoreAttribute>().Any() == false;

        private static bool DoesNotHaveItemsPerPageConverterOnTheItemsPerPageProperty(PropertyInfo propertyInfo) => 
            propertyInfo.PropertyType == typeof(ItemsPerPage) &&
            propertyInfo.GetCustomAttributes<JsonConverterAttribute>().Any() == false;

        private static bool DoesImplementIProvideHashCodeForPagingTokenValidation(Type type) =>
            type.GetInterfaces().Any(i => i == typeof(IProvideHashCodeForPagingTokenValidation));
    }
}
