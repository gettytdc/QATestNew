namespace BluePrism.Api.UnitTests.Validators
{
    using System;
    using System.Collections.Generic;
    using Api.Mappers.FilterMappers;
    using Api.Validators;
    using Domain;
    using FluentValidation.TestHelper;
    using Models;
    using NUnit.Framework;
    using ScheduleLogParameters = Models.ScheduleLogParameters;

    [TestFixture]
    public class ScheduleLogParametersValidatorTests
    {
        private ScheduleLogParametersValidator _validator;

        [SetUp]
        public void SetUp()
        {
            FilterModelMapper.SetFilterModelMappers(new IFilterModelMapper[]
            {
                new RangeFilterModelMapper(),
                new NullFilterModelMapper(),
                new GreaterThanOrEqualToFilterModelMapper(),
                new LessThanOrEqualToFilterModelMapper(),
            });
            _validator = new ScheduleLogParametersValidator(new PagingSettings { MaxItemsPerPage = 1000 });
        }

        [Test]
        public void Validate_ShouldHaveNoErrors_WhenValidParameters()
        {
            var parameters = new ScheduleLogParameters
            {
                ItemsPerPage = 55,
                ScheduleLogStatus = new CommaDelimitedCollection<Models.ScheduleLogStatus>(new[]
                {
                    Models.ScheduleLogStatus.Running,
                    Models.ScheduleLogStatus.Completed
                }),
                StartTime = new RangeFilterModel<DateTimeOffset?> { Gte = DateTimeOffset.UtcNow },
                EndTime = new RangeFilterModel<DateTimeOffset?> { Lte = DateTimeOffset.UtcNow },
            };

            var result = _validator.TestValidate(parameters);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Test]
        public void Validate_ShouldHaveNoErrors_WhenEmptyParameters()
        {
            var parameters = new ScheduleLogParameters();
            var result = _validator.TestValidate(parameters);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Validate_ShouldHaveValidationErrorsForItemsPerPage_WhenItemsPerPageIsBelowOne(int value) =>
            _validator.TestValidate(new ScheduleLogParameters { ItemsPerPage = value })
                .ShouldHaveValidationErrorFor(x => x.ItemsPerPage);

        [Test]
        public void Validate_ShouldHaveValidationErrorsForItemsPerPage_WhenItemsPerPageExceedsMaximumConfiguredLimit() =>
            _validator.TestValidate(new ScheduleLogParameters { ItemsPerPage = 1001 })
                .ShouldHaveValidationErrorFor(x => x.ItemsPerPage);

        [Test]
        public void Validate_ShouldHaveValidationErrorsForStartTime_WhenStartTimeValueIsLessThanMinSqlDateTime() =>
            _validator.TestValidate(new ScheduleLogParameters { StartTime = new RangeFilterModel<DateTimeOffset?> { Eq = DateTimeOffset.Parse("1710-10-15 14:32:27") } })
                .ShouldHaveValidationErrorFor(x => x.StartTime);

        [Test]
        public void Validate_ShouldHaveValidationErrorsForStatus_WhenIntValuesDontMatchEnumValues() =>
            _validator.TestValidate(new ScheduleLogParameters { ScheduleLogStatus = new CommaDelimitedCollection<Models.ScheduleLogStatus>(new List<Models.ScheduleLogStatus> { (Models.ScheduleLogStatus)55 }) })
                .ShouldHaveValidationErrorFor(x => x.ScheduleLogStatus);
    }
}
