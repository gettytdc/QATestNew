namespace BluePrism.Api.UnitTests.Validators
{
    using System.Collections.Generic;
    using Api.Mappers.FilterMappers;
    using Api.Validators;
    using Domain;
    using FluentValidation.TestHelper;
    using Logging;
    using Models;
    using Moq;
    using NUnit.Framework;
    using ResourceParameters = Models.ResourceParameters;

    [TestFixture]
    public class ResourceParametersValidatorTests
    {
        private ResourceParametersValidator _validator;

        [SetUp]
        public void SetUp()
        {
            FilterModelMapper.SetFilterModelMappers(new IFilterModelMapper[]
            {
                new RangeFilterModelMapper(),
                new NullFilterModelMapper(),
                new StartsWithStringFilterModelMapper(),
                new GreaterThanOrEqualToFilterModelMapper(),
            });
            _validator = new ResourceParametersValidator(Mock.Of<ILogger<ResourceParametersValidator>>(), new PagingSettings { MaxItemsPerPage = 1000 });
        }

        [Test]
        public void Validate_ShouldHaveNoErrors_WhenValidParameters()
        {
            var parameters = new ResourceParameters
            {
                ItemsPerPage = 55,
                Name = new StartsWithStringFilterModel { Strtw = "resource" },
                GroupName = new StartsWithStringFilterModel { Strtw = "group" },
                PoolName = new StartsWithStringFilterModel { Strtw = "pool" },
                ActiveSessionCount = new RangeFilterModel<int?> { Gte = 0 },
                PendingSessionCount = new RangeFilterModel<int?> { Gte = 0 },
                DisplayStatus = new CommaDelimitedCollection<Models.ResourceDisplayStatus>(new[]
                {
                    Models.ResourceDisplayStatus.Working,
                    Models.ResourceDisplayStatus.Offline
                }),
            };

            var result = _validator.TestValidate(parameters);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Test]
        public void Validate_ShouldHaveNoErrors_WhenEmptyParameters()
        {
            var parameters = new ResourceParameters();
            var result = _validator.TestValidate(parameters);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Validate_ShouldHaveValidationErrorsForItemsPerPage_WhenItemsPerPageIsBelowOne(int value) =>
            _validator.TestValidate(new ResourceParameters { ItemsPerPage = value })
                .ShouldHaveValidationErrorFor(x => x.ItemsPerPage);

        [Test]
        public void Validate_ShouldHaveValidationErrorsForItemsPerPage_WhenItemsPerPageExceedsMaximumConfiguredLimit() =>
            _validator.TestValidate(new ResourceParameters { ItemsPerPage = 1001 })
                .ShouldHaveValidationErrorFor(x => x.ItemsPerPage);

        [Test]
        public void Validate_ShouldHaveValidationErrorsForStatus_WhenIntValuesDontMatchEnumValues() =>
            _validator.TestValidate(new ResourceParameters { DisplayStatus = new CommaDelimitedCollection<Models.ResourceDisplayStatus>(new List<Models.ResourceDisplayStatus> { (Models.ResourceDisplayStatus)55 }) })
                .ShouldHaveValidationErrorFor(x => x.DisplayStatus);
    }
}
