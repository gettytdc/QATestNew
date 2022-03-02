namespace BluePrism.Api.UnitTests.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.JsonPatch.Operations;
    using Newtonsoft.Json.Serialization;
    using NUnit.Framework;
    using static BluePrism.Api.Extensions.JsonPatchDocumentExtensions;

    [TestFixture]
    public class JsonPatchDocumentExtensionsTests
    {
        [Test]
        public void IsValid_ShouldReturnTrue_WhenThereAreValidPatchOperations()
        {
            var patchDocument = GetPatchDocument<StubModelA>("replace", "/prop", null, "val");

            patchDocument.IsValid().Should().BeTrue();
        }

        [Test]
        public void IsValid_ShouldReturnFalse_WhenPatchDocumentIsNull()
        {
            JsonPatchDocument<StubModelA> patchDocument = null;

            // ReSharper disable once ExpressionIsAlwaysNull
            patchDocument.IsValid().Should().BeFalse();
        }

        [Test]
        public void IsValid_ShouldReturnFalse_WhenThereAreNoPatchOperations()
        {
            var patchDocument = new JsonPatchDocument<StubModelA>();

            patchDocument.IsValid().Should().BeFalse();
        }

        [Test]
        public void TryPatch_ShouldReturnTrue_WhenOperationsAppliedAgainstValidProperties()
        {
            var patchDocument = GetPatchDocument<StubModelA>("replace", "/prop", null, "val");

            patchDocument.TryPatch(out _).Should().BeTrue();
        }

        [Test]
        public void TryPatch_ShouldReturnFalseWithErrorMessage_WhenOperationsAppliedAgainstInvalidProperties()
        {
            var patchDocument = GetPatchDocument<StubModelA>("replace", "/invalidProp", null, "val");

            patchDocument.TryPatch(out var patchModelState).Should().BeFalse();
            patchModelState.FailedErrorMessage.Should().NotBeEmpty();
        }

        [Test]
        public void TryPatch_ShouldReturnFalseWithErrorMessage_WhenInvalidOperationSupplied()
        {
            var patchDocument = GetPatchDocument<StubModelA>("invalidOp", "/prop", null, "val");

            patchDocument.TryPatch(out var patchModelState).Should().BeFalse();
            patchModelState.FailedErrorMessage.Should().NotBeEmpty();
        }

        [Test]
        public void Map_To_ShouldReturnIdenticalOperationsOnNewModel_WhenCalled()
        {
            var patchDocument = GetPatchDocument<StubModelA>("replace", "/prop", null, "val");

            var mappedDocument = patchDocument.Map().To<StubModelB>();

            mappedDocument.Should().BeOfType<JsonPatchDocument<StubModelB>>();
            mappedDocument.Operations.Single().op.Should().Be("replace");
            mappedDocument.Operations.Single().path.Should().Be("/prop");
            mappedDocument.Operations.Single().value.Should().Be("val");
        }

        private JsonPatchDocument<TModel> GetPatchDocument<TModel>(string op, string path, string from, object value) where TModel : class =>
            new JsonPatchDocument<TModel>(
                new List<Operation<TModel>>
                {
                    new Operation<TModel>(op, path, from, value)
                },
                new DefaultContractResolver()
            );

        private class StubModelA
        {
            // ReSharper disable once UnusedMember.Local
            public string Prop { get; set; }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class StubModelB
        {
            // ReSharper disable once UnusedMember.Local
            public string Prop { get; set; }
        }
    }
}
