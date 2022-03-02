namespace BluePrism.Api.Validators
{
    using System;
    using Autofac;
    using FluentValidation;

    public class ValidatorFactory : ValidatorFactoryBase
    {
        private readonly IContainer _container;

        public ValidatorFactory(IContainer container)
        {
            _container = container;
        }

        public override IValidator CreateInstance(Type validatorType) =>
            _container.IsRegistered(validatorType)
            ? _container.Resolve(validatorType) as IValidator
            : null; // Returning null as per FluentValidation documentation: https://github.com/FluentValidation/FluentValidation/blob/8.x/docs/_doc/mvc5/ioc.md
    }
}
