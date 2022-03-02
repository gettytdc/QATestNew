namespace BluePrism.Api.UnitTests.Validators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Validators;
    using CommonTestClasses.Extensions;
    using FluentValidation.TestHelper;
    using Models;
    using NUnit.Framework;

    [TestFixture]
    public class WorkQueueCompositionParametersValidatorTests
    {
        private WorkQueueCompositionParametersValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _validator = new WorkQueueCompositionParametersValidator();
        }

        [Test]
        public void Validate_ShouldHaveNoErrors_WhenValidParameters()
        {
            var parameters = new WorkQueueCompositionParameters
            {
                WorkQueueIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
            };

            var result = _validator.TestValidate(parameters);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [TestCaseSource(nameof(WorkQueueIdsInvalidValues))]
        public void Validate_ShouldHaveValidationError_WhenQueueIdsInvalid(IReadOnlyCollection<Guid> workQueueIds)
        {
            var parameters = new WorkQueueCompositionParameters
            {
                WorkQueueIds = workQueueIds,
            };

            var result = _validator.TestValidate(parameters);

            result.ShouldHaveValidationErrorFor(x => x.WorkQueueIds);
        }

        private static IEnumerable<TestCaseData> WorkQueueIdsInvalidValues => new[]
        {
            null,
            new List<Guid>(),
            new List<Guid> { Guid.Empty },
            Enumerable.Range(0, 25).Select(_ => Guid.NewGuid()).ToList(),
        }.ToTestCaseData();
    }
}
