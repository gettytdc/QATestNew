namespace BluePrism.Api.UnitTests.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http.ModelBinding;
    using Api.Extensions;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class ModelStateDictionaryExtensionsTests
    {
        [Test]
        public void GetErrors_ShouldReturnAllModelErrorMessages_WhenThereAreErrors()
        {
            const string key1 = "test";
            const string key2 = "test2";
            const string error1 = "error1";
            const string error2 = "error2";

            var modelState = new ModelStateDictionary();
            modelState.AddModelError(key1, error1);
            modelState.AddModelError(key2, error2);
            modelState.Add(new KeyValuePair<string, ModelState>("test3", new ModelState()));

            var modelErrors = modelState.GetErrors();

            modelErrors.Any(x => x.Key.Equals(key1)).Should().BeTrue();
            modelErrors.Any(x => x.Value.Equals(error1)).Should().BeTrue();
            modelErrors.Any(x => x.Key.Equals(key2)).Should().BeTrue();
            modelErrors.Any(x => x.Value.Equals(error2)).Should().BeTrue();
            modelErrors.Count.Should().Be(2);
        }

        [Test]
        public void GetErrors_ShouldReturnEmpty_WhenThereAreNoErrors()
        {
            var modelState = new ModelStateDictionary();

            modelState.GetErrors().Should().BeEmpty();
        }

        [Test]
        public void GetErrors_ShouldReturnErrorMessagesException_WhenThereAreErrorsButErrorMessagesAreEmpty()
        {
            const string key1 = "test1";
            const string key2 = "test2";
            var argumentException = new ArgumentException();
            var formatException = new FormatException();

            var modelState = new ModelStateDictionary();
            modelState.AddModelError(key1, argumentException);
            modelState.AddModelError(key2, formatException);

            var modelErrors = modelState.GetErrors();

            modelErrors.Any(x => x.Key.Equals(key1)).Should().BeTrue();
            modelErrors.Any(x => x.Value.Equals(argumentException.GetType().Name)).Should().BeTrue();
            modelErrors.Any(x => x.Key.Equals(key2)).Should().BeTrue();
            modelErrors.Any(x => x.Value.Equals(formatException.GetType().Name)).Should().BeTrue();
            modelErrors.Count.Should().Be(2);
        }

        [Test]
        public void GetErrors_ShouldReturnExpectedMessages_WhenThereAreErrorsAndExceptionErrorMessages()
        {
            const string key1 = "test";
            const string key2 = "test2";
            var argumentException = new ArgumentException();
            const string error2 = "error2";
            
            var modelState = new ModelStateDictionary();
            modelState.AddModelError(key1, argumentException);
            modelState.AddModelError(key2, error2);

            var modelErrors = modelState.GetErrors();

            modelErrors.Any(x => x.Key.Equals(key1)).Should().BeTrue();
            modelErrors.Any(x => x.Value.Equals(argumentException.GetType().Name)).Should().BeTrue();
            modelErrors.Any(x => x.Key.Equals(key2)).Should().BeTrue();
            modelErrors.Any(x => x.Value.Equals(error2)).Should().BeTrue();
            modelErrors.Count.Should().Be(2);
        }

        [Test]
        public void GetErrors_ShouldReturnExpectedMessages_WhenThereAreDuplicateValidationMessages()
        {
            const string key1 = "test";
            var argumentException = new ArgumentException();

            var modelState = new ModelStateDictionary();
            modelState.AddModelError(key1, argumentException);
            modelState.AddModelError(key1, argumentException);

            var modelErrors = modelState.GetErrors();

            modelErrors.Any(x => x.Key.Equals(key1)).Should().BeTrue();
            modelErrors.Any(x => x.Value.Equals(argumentException.GetType().Name)).Should().BeTrue();
            modelErrors.Count.Should().Be(1);
        }
    }
}
