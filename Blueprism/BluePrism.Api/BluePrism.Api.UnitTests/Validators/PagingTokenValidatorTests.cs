namespace BluePrism.Api.UnitTests.Validators
{
    using Api.Validators;
    using BluePrism.Api.Mappers;
    using FluentValidation.TestHelper;
    using Models;
    using NUnit.Framework;

    public class PagingTokenValidatorTests
    {
        [Test]
        public void Validate_ShouldHaveNoError_WhenHashcodeIsCorrect()
        {
            var instanceToValidate = new SessionLogsParameters
            {
                PagingToken = new PagingTokenModel<long> { PreviousIdValue = 123 }
            };

            instanceToValidate.PagingToken.ParametersHashCode =
                instanceToValidate.ToDomainObject().GetHashCodeForValidation();

            var validator = new TestPageValidator();
            var result = validator.TestValidate(instanceToValidate);

            result.ShouldNotHaveValidationErrorFor(x => x.PagingToken);
        }

        [Test]
        public void Validate_ShouldHaveError_WhenHashcodeIsWrong()
        {
            var instanceToValidate = new SessionLogsParameters
            {
                PagingToken = new PagingTokenModel<long> { PreviousIdValue = 123 }
            };

            instanceToValidate.PagingToken.ParametersHashCode =
                unchecked(instanceToValidate.ToDomainObject().GetHashCodeForValidation() + 1);

            var validator = new TestPageValidator();
            var result = validator.TestValidate(instanceToValidate);

            result.ShouldHaveValidationErrorFor(x => x.PagingToken);
        }
    }

    public class TestPageValidator : PagingTokenValidator<long, SessionLogsParameters>
    {
        
        protected override string GetItemHashCodeForValidation(SessionLogsParameters model) => model.ToDomainObject().GetHashCodeForValidation();
    }
}
