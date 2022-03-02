namespace BluePrism.Api.UnitTests.Validators
{
    using System;
    using System.Collections.Generic;
    using Api.Validators;
    using Domain;
    using FluentValidation.TestHelper;
    using NUnit.Framework;
    using ResourceUtilizationParameters = Models.ResourceUtilizationParameters;

    [TestFixture]
    public class ResourceUtilizationParametersValidatorTests
    {
        private ResourceUtilizationParametersValidator _validator;
        private const int TestMaxItemsPerPage = 1000;

        [SetUp]
        public void SetUp()
        {
            _validator = new ResourceUtilizationParametersValidator(new PagingSettings { MaxItemsPerPage = TestMaxItemsPerPage });
        }

        [Test]
        public void Validate_ShouldHaveNoErrors_WhenValidParameters()
        {
            var parameters = new ResourceUtilizationParameters
            {
                StartDate = DateTime.Now,
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
            var parameters = new ResourceUtilizationParameters
            {
                StartDate = invalidStartDate
            };

            var result = _validator.TestValidate(parameters);

            result.ShouldHaveValidationErrorFor(x => x.StartDate);
        }

        [Test]
        public void Validate_ShouldHaveNoErrors_WhenValidResourceIds()
        {
            var parameters = new ResourceUtilizationParameters
            {
                ResourceIds = new Guid[] { }
            };

            var result = _validator.TestValidate(parameters);

            result.ShouldNotHaveValidationErrorFor(x => x.ResourceIds);
        }

        private static IEnumerable<TestCaseData> InvalidPageSizeTestData
        {
            get
            {
                yield return new TestCaseData(0);
                yield return new TestCaseData(-10);
                yield return new TestCaseData(TestMaxItemsPerPage + 1);
            }
        }

        [TestCaseSource(nameof(InvalidPageSizeTestData))]
        public void Validate_ShouldHaveErrors_WhenInvalidPageSizeParameter(int invalidPageSize)
        {
            var parameters = new ResourceUtilizationParameters
            {
                PageSize = invalidPageSize
            };

            var result = _validator.TestValidate(parameters);

            result.ShouldHaveValidationErrorFor(x => x.PageSize);
        }

        private static IEnumerable<TestCaseData> InvalidPageNumberTestData
        {
            get
            {
                yield return new TestCaseData(0);
                yield return new TestCaseData(-10);
            }
        }

        [TestCaseSource(nameof(InvalidPageNumberTestData))]
        public void Validate_ShouldHaveErrors_WhenInvalidPageNumberParameter(int invalidPageNumber)
        {
            var parameters = new ResourceUtilizationParameters
            {
                PageNumber = invalidPageNumber
            };

            var result = _validator.TestValidate(parameters);

            result.ShouldHaveValidationErrorFor(x => x.PageNumber);
        }
    }
}
