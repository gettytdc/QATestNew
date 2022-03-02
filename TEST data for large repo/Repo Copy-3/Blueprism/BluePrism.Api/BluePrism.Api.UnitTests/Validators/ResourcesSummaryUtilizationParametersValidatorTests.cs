namespace BluePrism.Api.UnitTests.Validators
{
    using System;
    using System.Collections.Generic;
    using Api.Validators;
    using FluentValidation.TestHelper;
    using Models;
    using NUnit.Framework;

    [TestFixture]
    public class ResourcesSummaryUtilizationParametersValidatorTests
    {
        private ResourcesSummaryUtilizationParametersValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _validator = new ResourcesSummaryUtilizationParametersValidator();
        }

        [Test]
        public void Validate_ShouldHaveNoErrors_WhenValidParameters()
        {
            var parameters = new ResourcesSummaryUtilizationParameters()
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1),
                ResourceIds = new[] { Guid.NewGuid() }
            };

            var result = _validator.TestValidate(parameters);

            result.ShouldNotHaveAnyValidationErrors();
        }

        private static IEnumerable<TestCaseData> InvalidStartDateTestData
        {
            get
            {
                yield return new TestCaseData(DateTime.MinValue);
                yield return new TestCaseData(DateTime.MaxValue);
            }
        }


        [TestCaseSource(nameof(InvalidStartDateTestData))]
        public void Validate_ShouldHaveErrors_WhenInValidStartDateParameter(DateTime invalidStartDate)
        {
            var parameters = new ResourcesSummaryUtilizationParameters()
            {
                StartDate = invalidStartDate
            };

            var result = _validator.TestValidate(parameters);

            result.ShouldHaveValidationErrorFor(x => x.StartDate);
        }

        [TestCaseSource(nameof(InvalidStartDateTestData))]
        public void Validate_ShouldHaveErrors_WhenInValidEndDateParameter(DateTime invalidStartDate)
        {
            var parameters = new ResourcesSummaryUtilizationParameters()
            {
                StartDate = invalidStartDate
            };

            var result = _validator.TestValidate(parameters);

            result.ShouldHaveValidationErrorFor(x => x.EndDate);
        }

        [Test]
        public void Validate_ShouldHaveNoErrors_WhenValidResourceIds()
        {
            var parameters = new ResourcesSummaryUtilizationParameters()
            {
                ResourceIds = new Guid[] { }
            };

            var result = _validator.TestValidate(parameters);

            result.ShouldNotHaveValidationErrorFor(x => x.ResourceIds);
        }

        [Test]
        public void Validate_ShouldHaveErrors_WhenResourceIdsIsGreaterThan20()
        {
            var resourceIds = new List<Guid>();
            var greaterThanAllowedResourceIdAmount = 21;
            for (var i = 0; i < greaterThanAllowedResourceIdAmount; i++)
            {
                resourceIds.Add(Guid.NewGuid());
            }
            var parameters = new ResourcesSummaryUtilizationParameters()
            {
                ResourceIds = resourceIds
            };

            var result = _validator.TestValidate(parameters);

            result.ShouldHaveValidationErrorFor(x => x.ResourceIds.Count);
        }

        [Test]
        public void Validate_ShouldHaveErrors_WhenDifferenceBetweenStartDateAndEndDateGreaterThan90Days()
        {
            var parameters = new ResourcesSummaryUtilizationParameters()
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(91)
            };

            var result = _validator.TestValidate(parameters);

            result.ShouldHaveValidationErrorFor("TotalDays");
        }

        [Test]
        public void Validate_ShouldHaveError_WhenEndDateIsLessThanStartDate()
        {
            var parameters = new ResourcesSummaryUtilizationParameters()
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(-1)
            };

            var result = _validator.TestValidate(parameters);

            result.ShouldHaveValidationErrorFor(x => x.EndDate);
        }
    }
}
