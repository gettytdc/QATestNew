namespace BluePrism.Api.BpLibAdapters.UnitTests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutomateAppCore;
    using Domain;
    using FluentAssertions;
    using Func;
    using NUnit.Framework;
    using Utilities.Testing;

    using BpLibEncryptionAlgorithm = AutomateAppCore.EncryptionAlgorithm;
    using BpLibEncryptionKeyLocation = AutomateAppCore.EncryptionKeyLocation;
    using EncryptionAlgorithm = Domain.EncryptionAlgorithm;
    using EncryptionKeyLocation = Domain.EncryptionKeyLocation;

    [TestFixture]
    public class EncryptionSchemeServerAdapterTests : UnitTestBase<EncryptionSchemeServerAdapter>
    {
        [Test]
        public async Task EncryptionSchemesGetSchemes_OnSuccess_ReturnsSchemes()
        {
            clsEncryptionScheme GetClsSchemeWithName(string name) =>
                new clsEncryptionScheme
                { 
                    Name = name,
                    Algorithm = BpLibEncryptionAlgorithm.None,
                    KeyLocation = BpLibEncryptionKeyLocation.Database,
                    IsAvailable = true,
                };

            var testSchemes = new[]
            {
                GetClsSchemeWithName("abc"),
                GetClsSchemeWithName("def"),
            };

            GetMock<IServer>()
                .Setup(m => m.GetEncryptionSchemes())
                .Returns(testSchemes.ToList());

            EncryptionScheme GetSchemeWithName(string name) =>
                new EncryptionScheme
                {
                    Name = name,
                    Algorithm = EncryptionAlgorithm.None,
                    KeyLocation = EncryptionKeyLocation.Database,
                    IsAvailable = true,
                };

            var resultSchemes = new[]
            {
                GetSchemeWithName("abc"),
                GetSchemeWithName("def"),
            };

            var result = await ClassUnderTest.EncryptionSchemesGetSchemes();

            ((Success<IEnumerable<EncryptionScheme>>)result).Value.Should().BeEquivalentTo<EncryptionScheme>(resultSchemes);
        }
    }
}
