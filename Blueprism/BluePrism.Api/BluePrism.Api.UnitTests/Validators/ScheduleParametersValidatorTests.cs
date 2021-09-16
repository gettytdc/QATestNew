namespace BluePrism.Api.UnitTests.Validators
{
    using Api.Mappers;
    using Api.Mappers.FilterMappers;
    using Api.Validators;
    using Domain;
    using Domain.PagingTokens;
    using FluentValidation.TestHelper;
    using Logging;
    using Models;
    using Moq;
    using NUnit.Framework;
    using RetirementStatus = Models.RetirementStatus;
    using ScheduleParameters = Models.ScheduleParameters;

    [TestFixture]
    public class ScheduleParametersValidatorTests
    {
        private ScheduleParametersValidator _validator;

        [SetUp]
        public void SetUp()
        {
            FilterModelMapper.SetFilterModelMappers(new IFilterModelMapper[]
            {
                new EqualFilterModelMapper(), new StartsWithStringFilterModelMapper(), new NullFilterModelMapper()
            });
            _validator = new ScheduleParametersValidator(Mock.Of<ILogger<ScheduleParametersValidator>>(), new PagingSettings { MaxItemsPerPage = 1000 });
        }

        [Test]
        public void Validate_ShouldHaveNoErrors_WhenValidParameters()
        {
            var parameters = new ScheduleParameters
            {
                Name = new StartsWithStringFilterModel { Strtw = "test" },
                RetirementStatus = new[] { RetirementStatus.Active }
            };
            var result = _validator.TestValidate(parameters);
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
            result.ShouldNotHaveValidationErrorFor(x => x.RetirementStatus);
        }

        [Test]
        public void Validate_ShouldHaveValidationErrorForPagingToken_WhenInvalidPagingTokenFormat()
        {
            var parameters = new ScheduleParameters
            {
                Name = new StartsWithStringFilterModel(),
                RetirementStatus = new[] { RetirementStatus.Active },
                PagingToken = "invalidPagingToken"
            };
            var result = _validator.TestValidate(parameters);
            result.ShouldHaveValidationErrorFor(x => x.PagingToken)
                .WithErrorMessage("Paging Token is malformed");
        }

        [Test]
        public void Validate_ShouldHaveValidationErrorForPagingToken_WhenParametersChanged()
        {
            var initialParameters = new ScheduleParameters
            {
                Name = new StartsWithStringFilterModel(),
                RetirementStatus = new[] { RetirementStatus.Active }
            };
            var pagingToken = new PagingTokenModel<string> { ParametersHashCode = initialParameters.ToDomainObject().GetHashCodeForValidation() };
            var parameters = new ScheduleParameters
            {
                Name = new StartsWithStringFilterModel{Eq = "test"},
                RetirementStatus = new[] { RetirementStatus.Active, RetirementStatus.Retired },
                PagingToken = pagingToken
            };
            var result = _validator.TestValidate(parameters);
            result.ShouldHaveValidationErrorFor(x => x.PagingToken);
        }

        [Test]
        public void Validate_ShouldHaveNoErrors_WhenEmptyParameters()
        {
            var parameters = new ScheduleParameters();
            var result = _validator.TestValidate(parameters);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Validate_ShouldHaveValidationErrorsForItemsPerPage_WhenItemsPerPageIsBelowOne(int value) =>
            _validator.TestValidate(new ScheduleParameters { ItemsPerPage = value })
                .ShouldHaveValidationErrorFor(x => x.ItemsPerPage);
        
        [Test]
        public void Validate_ShouldHaveValidationErrorsForItemsPerPage_WhenItemsPerPageExceedsMaximumConfiguredLimit() =>
            _validator.TestValidate(new ScheduleParameters { ItemsPerPage = 1001 })
                .ShouldHaveValidationErrorFor(x => x.ItemsPerPage);
    }
}
