namespace BluePrism.Api.UnitTests.Validators
{
    using System.Collections.Generic;
    using Api.Validators;
    using Domain;
    using FluentValidation.TestHelper;
    using Models;
    using NUnit.Framework;

    [TestFixture]
    public class CreateWorkQueueItemsRequestValidatorTests
    {
        
        [Test]
        public void TestValidateRequest_WhenBatchIsGreaterThanAllowed_ShouldHaveError()
        {
            var validator = new CreateWorkQueueItemsRequestValidator(new CreateWorkQueueItemsSettings()
            {
                MaxCreateWorkQueueRequestsInBatch = 1
            });
            var model = new List<CreateWorkQueueItemModel>()
            {
                new CreateWorkQueueItemModel(),
                new CreateWorkQueueItemModel()
            };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x);
        }

        [Test]
        public void TestValidateRequest_WhenBatchIsLessOrEqualThanAllowed_ShouldSuccess()
        {
            var validator = new CreateWorkQueueItemsRequestValidator(new CreateWorkQueueItemsSettings()
            {
                MaxCreateWorkQueueRequestsInBatch = 1
            });
            var model = new List<CreateWorkQueueItemModel>()
            {
                new CreateWorkQueueItemModel()
            };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

    }
}
