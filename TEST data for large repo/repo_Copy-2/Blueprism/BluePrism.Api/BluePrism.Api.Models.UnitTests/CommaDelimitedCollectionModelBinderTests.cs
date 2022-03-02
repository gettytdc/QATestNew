namespace BluePrism.Api.Models.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Web.Http.Controllers;
    using System.Web.Http.Metadata.Providers;
    using System.Web.Http.ModelBinding;
    using System.Web.Http.ValueProviders;
    using CommonTestClasses.Extensions;
    using FluentAssertions;
    using NUnit.Framework;
    using Utilities.Testing;

    [TestFixture]
    public class CommaDelimitedCollectionModelBinderTests : UnitTestBase<CommaDelimitedCollectionModelBinder>
    {
        private const string ModelName = "Collection";

        [TestCaseSource(nameof(SampleCommaDelimitedCollectionTypes))]
        public void BindModel_ShouldReturnTrue_WhenSuccessful(string value, Type type)
        {
            var bindingContext = SetupBindingContextWithValue(value, type);

            var result = ClassUnderTest.BindModel(GetMock<HttpActionContext>().Object, bindingContext);

            result.Should().BeTrue();
        }
        
        [Test]
        public void BindModel_ShouldReturnFalse_WhenNotBindingToCommaDelimitedCollection()
        {
            var bindingContext = SetupBindingContextWithValue(EnumStub.Two.ToString(), typeof(ModelStub<string>));

            var result = ClassUnderTest.BindModel(GetMock<HttpActionContext>().Object, bindingContext);

            result.Should().BeFalse();
        }

        [Test]
        public void BindModel_ShouldContainHasDataBoundTrueInModelBindingContext_WhenSuccessful()
        {
            var bindingContext = SetupBindingContextWithValue(EnumStub.Two.ToString());

            ClassUnderTest.BindModel(GetMock<HttpActionContext>().Object, bindingContext);

            ((CommaDelimitedCollection<EnumStub>)bindingContext.Model).HasBoundData.Should().BeTrue();
        }

        [Test(Description = "Test mimics no value in querystring e.g. parameter=")]
        [TestCase(null)]
        [TestCase("")]
        public void BindModel_ShouldContainEmptyModelInModelBindingContext_WhenNoValueSupplied(string value)
        {
            var bindingContext = SetupBindingContextWithValue(value);

            ClassUnderTest.BindModel(GetMock<HttpActionContext>().Object, bindingContext);

            bindingContext.Model.Should().BeOfType<CommaDelimitedCollection<EnumStub>>();
            ((CommaDelimitedCollection<EnumStub>)bindingContext.Model).Should().BeEmpty();
        }

        [Test(Description = "Test mimics single value in querystring e.g. parameter=value")]
        public void BindModel_ShouldContainCorrectModelInModelBindingContext_WhenSingleValueSupplied()
        {
            var bindingContext = SetupBindingContextWithValue(EnumStub.Two.ToString());

            ClassUnderTest.BindModel(GetMock<HttpActionContext>().Object, bindingContext);

            bindingContext.Model.Should().BeOfType<CommaDelimitedCollection<EnumStub>>();
            ((CommaDelimitedCollection<EnumStub>)bindingContext.Model).Should().Contain(EnumStub.Two);
        }

        [Test(Description = "Test mimics multiple querystring parameter values e.g. parameter1=value1&parameter1=value2")]
        public void BindModel_ShouldContainCorrectModelInModelBindingContext_WhenArrayValuesSupplied()
        {
            var bindingContext = SetupBindingContextWithValue(new[] {EnumStub.Two.ToString(), EnumStub.Three.ToString()});

            ClassUnderTest.BindModel(GetMock<HttpActionContext>().Object, bindingContext);

            bindingContext.Model.Should().BeOfType<CommaDelimitedCollection<EnumStub>>();
            ((CommaDelimitedCollection<EnumStub>)bindingContext.Model).Should().Contain(new[] {EnumStub.Two, EnumStub.Three});
        }

        [Test(Description = "Test mimics comma separated value in querystring e.g. parameter=value1,value2")]
        public void BindModel_ShouldContainCorrectModelInModelBindingContext_WhenCommaSeparatedValuesSupplied()
        {
            var bindingContext = SetupBindingContextWithValue($"{EnumStub.Two},{EnumStub.Three}");

            ClassUnderTest.BindModel(GetMock<HttpActionContext>().Object, bindingContext);

            bindingContext.Model.Should().BeOfType<CommaDelimitedCollection<EnumStub>>();
            ((CommaDelimitedCollection<EnumStub>)bindingContext.Model).Should().Contain(new[] { EnumStub.Two, EnumStub.Three });
        }

        [Test(Description = "Test mimics comma separated value with white space in querystring e.g. parameter=value1 , value2 ")]
        public void BindModel_ShouldContainCorrectModelInModelBindingContext_WhenCommaSeparatedValuesWithWhiteSpaceSupplied()
        {
            var bindingContext = SetupBindingContextWithValue($"{EnumStub.Two} , {EnumStub.Three} ");

            ClassUnderTest.BindModel(GetMock<HttpActionContext>().Object, bindingContext);

            bindingContext.Model.Should().BeOfType<CommaDelimitedCollection<EnumStub>>();
            ((CommaDelimitedCollection<EnumStub>)bindingContext.Model).Should().Contain(new[] { EnumStub.Two, EnumStub.Three });
        }

        [TestCaseSource(nameof(InvalidCommaDelimitedCollectionValues))]
        public void BindModel_ShouldReturnTrue_WhenInvalidValuesSupplied(string invalidValue, Type type)
        {
            var bindingContext = SetupBindingContextWithValue(invalidValue, type);

            var result = ClassUnderTest.BindModel(GetMock<HttpActionContext>().Object, bindingContext);

            result.Should().BeTrue();
        }

        [Test]
        public void BindModel_ShouldContainHasBoundDataFailedWithOriginalValuesInModelBindingContext_WhenInvalidValuesSupplied()
        {
            var invalidValue = $"{EnumStub.Two} , InvalidValue ";

            var bindingContext = SetupBindingContextWithValue(invalidValue);

            ClassUnderTest.BindModel(GetMock<HttpActionContext>().Object, bindingContext);

            var model = (CommaDelimitedCollection<EnumStub>)bindingContext.Model;

            model.Should().BeEmpty();
            model.HasBoundData.Should().BeFalse();
            model.RawStringValues.Should().Be(invalidValue);
        }

        [Test]
        public void BindModel_ShouldContainHasDataBoundFailedWithOriginalValuesInModelBindingContext_WhenInvalidArrayValuesSupplied()
        {
            var invalidValue = new []{ EnumStub.Two.ToString() , "InvalidValue"};

            var bindingContext = SetupBindingContextWithValue(invalidValue);

            ClassUnderTest.BindModel(GetMock<HttpActionContext>().Object, bindingContext);

            var model = (CommaDelimitedCollection<EnumStub>)bindingContext.Model;

            model.Should().BeEmpty();
            model.HasBoundData.Should().BeFalse();
            model.RawStringValues.Should().Be(string.Join(",", invalidValue));
        }

        [Test]
        public void BindModel_ShouldThrowException_WhenIncorrectTypeUsedForCommaDelimitedCollection()
        {
            var bindingContext = SetupBindingContextWithValue("a", typeof(CommaDelimitedCollection<ModelStub<string>>));

            Action action = () => ClassUnderTest.BindModel(GetMock<HttpActionContext>().Object, bindingContext);

            action.ShouldThrow<ArgumentException>();
        }

        private ModelBindingContext SetupBindingContextWithValue(object rawValue, Type type = null)
        {
            var valueProvider = GetMock<IValueProvider>();
            valueProvider.Setup(x => x.GetValue(ModelName)).Returns(GetValueProviderResult(rawValue));

            var modelMetadata = new DataAnnotationsModelMetadataProvider()
                .GetMetadataForType(null, type ?? typeof(CommaDelimitedCollection<EnumStub>));

            return new ModelBindingContext
            {
                ValueProvider = valueProvider.Object,
                ModelMetadata = modelMetadata,
                ModelName = ModelName
            };
        }

        private static ValueProviderResult GetValueProviderResult(object rawValue) =>
            new ValueProviderResult(rawValue, string.Empty, CultureInfo.InvariantCulture);

        private static IEnumerable<TestCaseData> SampleCommaDelimitedCollectionTypes() =>
            new[]
            {
                (EnumStub.Two.ToString(), typeof(CommaDelimitedCollection<EnumStub>)),
                ("a", typeof(CommaDelimitedCollection<string>)),
                ("1", typeof(CommaDelimitedCollection<int>))
            }.ToTestCaseData();
        
        private static IEnumerable<TestCaseData> InvalidCommaDelimitedCollectionValues() =>
            new[]
            {
                ($"{EnumStub.Two} , InvalidValue ", typeof(CommaDelimitedCollection<EnumStub>)),
                ("a", typeof(CommaDelimitedCollection<int>))
            }.ToTestCaseData();

        public class ModelStub<T>
        {
            public T Name { get; set; }
        }

        public class InvalidType { }

        public enum EnumStub
        {
            One,
            Two,
            Three
        }
    }
}
