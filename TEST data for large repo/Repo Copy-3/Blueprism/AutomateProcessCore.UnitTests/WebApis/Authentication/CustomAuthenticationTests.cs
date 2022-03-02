#if UNITTESTS
using System.Linq;
using BluePrism.AutomateProcessCore;
using BluePrism.AutomateProcessCore.WebApis;
using BluePrism.AutomateProcessCore.WebApis.Authentication;
using FluentAssertions;
using NUnit.Framework;

namespace AutomateProcessCore.UnitTests.WebApis.Authentication
{
    public class CustomAuthenticationTests
    {
        private AuthenticationCredential _testCredential;

        [SetUp]
        public void SetUp() => _testCredential = new AuthenticationCredential("test", false, "test");

        [Test]
        public void Check_ConfiguredCorrectly()
        {
            var sut = new CustomAuthentication(_testCredential);
            sut.Type.Should().Be(AuthenticationType.Custom);
            sut.Credential.Should().Be(_testCredential);
        }

        [Test]
        public void InputParameters_CredentialNotExposed_HasNoInputParameters()
        {
            var sut = new CustomAuthentication(_testCredential);
            sut.GetInputParameters().Should().BeEmpty();
        }

        [Test]
        public void InputParameters_CredentialExposed_HasCredentialNameInputParam()
        {
            var exposedCredential = new AuthenticationCredential("credName", true, "inputParamName");
            var sut = new CustomAuthentication(exposedCredential);
            var expectedInputParam = new ActionParameter("inputParamName", "The name of the Custom credential", DataType.text, true, "credName");
            var inputParams = sut.GetInputParameters().ToList();
            inputParams.Count.Should().Be(1);
            inputParams.First().ShouldBeEquivalentTo(expectedInputParam);
        }
    }
}
#endif
