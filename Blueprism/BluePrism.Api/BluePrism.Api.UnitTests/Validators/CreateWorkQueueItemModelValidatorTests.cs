
namespace BluePrism.Api.UnitTests.Validators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Validators;
    using Domain;
    using FluentAssertions;
    using Models;
    using NUnit.Framework;
    using FluentValidation.TestHelper;

    [TestFixture]
    public class CreateWorkQueueItemModelValidatorTests
    {
        private CreateWorkQueueItemModelValidator _validator;
        private int _maxStatusLength = 255;
        private int _maxTagLength = 255;

        [SetUp]
        public void Setup() =>
            _validator = new CreateWorkQueueItemModelValidator(new CreateWorkQueueItemsSettings()
            {
                MaxStatusLength = _maxStatusLength,
                MaxTagLength = _maxTagLength
            });

        [Test]
        public void TestValidate_WhenDataIsNull_ShouldHaveError()
        {
            var model = new CreateWorkQueueItemModel {Data = null};
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Data);
        }

        [Test]
        public void TestValidate_WhenDataIsEmpty_ShouldHaveError()
        {
            var model = new CreateWorkQueueItemModel
            {
                Data = new DataCollectionModel()
            };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Data.Rows);
        }

        [Test]
        public void TestValidate_WhenDataRowsIsEmpty_ShouldHaveError()
        {
            var model = new CreateWorkQueueItemModel
            {
                Data = new DataCollectionModel()
                {
                    Rows = new IReadOnlyDictionary<string, DataValueModel>[]{}
                }
            };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Data.Rows);
        }

        [Test]
        public void TestValidate_WhenStatusIsGreaterThanMaxSize_ShouldHaveError()
        {
            var paddingChar = '.';
            var status = "open".PadRight(300, paddingChar);
           
            var model = new CreateWorkQueueItemModel { Status = status };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Status);
        }

        [Test]
        public void TestValidate_WhenStatusIsLessThanMaxSize_ShouldSuccess()
        {
            var status = "open";
            var model = new CreateWorkQueueItemModel { Status = status };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Status);
        }
        
        [Test]
        public void TestValidate_WhenTagsIsGreaterThanMaxSize_ShouldHaveError()
        {
            var paddingChar = '.';
            var tagLengthTooLong = "tag1".PadRight(300, paddingChar);
            var tagLengthOK = "open";
            var model = new CreateWorkQueueItemModel { Tags = new List<string>() {tagLengthTooLong, tagLengthOK}};
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Tags);
        }

        [Test]
        public void TestValidate_WhenTagsIsLessThanMaxSize_ShouldSuccess()
        {
            var paddingChar = '.';
            var tagLengthTooLong = "tag1".PadRight(255, paddingChar);
            var tagLengthOK = "open";
            var model = new CreateWorkQueueItemModel { Tags = new List<string>() { tagLengthTooLong, tagLengthOK } };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Tags);
        }
        
        [Test]
        public void TestValidate_WhenPriorityIsLessThanMaxSize_ShouldSuccess()
        {
            var model = new CreateWorkQueueItemModel { Priority = 9999 };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Priority);
        }

        [Test]
        public void TestValidate_WhenPriorityIsGreaterThanMaxSize_ShouldHaveError()
        {
            var model = new CreateWorkQueueItemModel { Priority = 99999 };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Priority);

            var expectedValidationMessage = "The priority level must be a valid integer value: 0 - 9999";
            var hasExpectedValidationMessage = result.Errors.Any(x => string.Equals(x.ErrorMessage, expectedValidationMessage, StringComparison.CurrentCultureIgnoreCase));
            hasExpectedValidationMessage.Should().BeTrue();
        }

        [Test]
        public void TestValidate_WhenPriorityIsLessThanZero_ShouldHaveError()
        {
            var model = new CreateWorkQueueItemModel { Priority = -1 };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Priority);

            var expectedValidationMessage = "The priority level must be a valid integer value: 0 - 9999";
            var hasExpectedValidationMessage = result.Errors.Any(x => string.Equals(x.ErrorMessage, expectedValidationMessage, StringComparison.CurrentCultureIgnoreCase));
            hasExpectedValidationMessage.Should().BeTrue();
        }
    }
}
