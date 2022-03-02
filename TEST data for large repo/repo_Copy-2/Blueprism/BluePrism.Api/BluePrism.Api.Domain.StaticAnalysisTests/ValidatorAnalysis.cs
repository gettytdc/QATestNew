namespace BluePrism.Api.Domain.StaticAnalysisTests
{
    using System;
    using System.Linq;
    using FluentValidation;
    using Models;
    using NUnit.Framework;
    using Validators;

    [TestFixture]
    public class ValidatorAnalysis
    {
        [Test]
        public void Validators_ThatValidateClassesThatImplementIPagingModel_ShouldInheritPagingTokenValidator()
        {
            var validators = typeof(PagingTokenValidator<,>).Assembly
                .GetExportedTypes()
                .Where(x => x.BaseType != null && x.BaseType.IsGenericType)
                .Where(x => x.BaseType?.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
                .Where(x => x.BaseType.GenericTypeArguments.Any(DoesImplementIPagingModel))
                .Where(IsNotPagingTokenValidator)
                .ToArray();

            if (validators.Any())
                Assert.Fail($"Validators that validate classes that implement {nameof(IPagingModel<object>)} must inherit from {typeof(PagingTokenModel<>).Name}.\r\n\r\n {string.Join("\r\n", validators.Select(x => x.FullName))}");
        }

        private static bool DoesImplementIPagingModel(Type type) =>
            type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPagingModel<>));

        private static bool IsNotPagingTokenValidator(Type type) =>
            !(type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(PagingTokenValidator<,>));
    }
}
