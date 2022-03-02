namespace BluePrism.Api.UnitTests.Validators
{
    using System.Collections.Generic;
    using Api.Mappers.FilterMappers;
    using Api.Validators;
    using Domain;
    using FluentValidation.TestHelper;
    using Models;
    using NUnit.Framework;
    using WorkQueueItemParameters = Models.WorkQueueItemParameters;

    public class WorkQueueItemParametersValidatorTests
    {
        private WorkQueueItemParametersValidator _validator;

        [SetUp]
        public void Setup()
        {
            FilterModelMapper.SetFilterModelMappers(new List<IFilterModelMapper>());
            _validator = new WorkQueueItemParametersValidator(new PagingSettings { MaxItemsPerPage = 1000 });
        }

        [Test]
        public void Validate_ShouldHaveNoError_WhenSortByIsSet()
        {
            FilterModelMapper.SetFilterModelMappers(new List<IFilterModelMapper>());

            var model = new WorkQueueItemParameters()
            {
                SortBy = WorkQueueItemSortBy.AttemptAsc
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.SortBy);
        }

        [Test]
        public void Validate_ShouldHaveNoError_WhenSortByIsNull()
        {
            var model = new WorkQueueItemParameters()
            {
                SortBy = null
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.SortBy);
        }

    }
}
