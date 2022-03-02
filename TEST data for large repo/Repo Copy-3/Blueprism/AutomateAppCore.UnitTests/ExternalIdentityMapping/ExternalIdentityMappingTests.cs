using FluentAssertions;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.ExternalIdentityMapping
{
    [TestFixture]
    public class ExternalIdentityMappingTests
    {
        [Test]
        public void TestEquals_ObjectsAreEqual_ReturnsTrue()
        {
            var externalMapping1 = new BluePrism.AutomateAppCore.Auth.ExternalIdentityMapping("dave@salesforce.com", "Salesforce", "SAML");
            var externalMapping2 = new BluePrism.AutomateAppCore.Auth.ExternalIdentityMapping("dave@salesforce.com", "Salesforce", "SAML");
            var result = externalMapping1.Equals(externalMapping2);
            result.Should().BeTrue();
        }

        [Test]
        public void TestEquals_ObjectIsNull_ReturnsFalse()
        {
            var externalMapping1 = new BluePrism.AutomateAppCore.Auth.ExternalIdentityMapping("dave@salesforce.com", "Salesforce", "SAML");
            object externalMapping2 = null;
            var result = externalMapping1.Equals(externalMapping2);
            result.Should().BeFalse();
        }

        [Test]
        public void TestEquals_ObjectsAreDifferentType_ReturnsFalse()
        {
            var externalMapping1 = new BluePrism.AutomateAppCore.Auth.ExternalIdentityMapping("dave@salesforce.com", "Salesforce", "SAML");
            var externalMapping2 = string.Empty;
            var result = externalMapping1.Equals(externalMapping2);
            result.Should().BeFalse();
        }

        [Test]
        public void TestEquals_ExternalIdsDiffer_ReturnsFalse()
        {
            var externalMapping1 = new BluePrism.AutomateAppCore.Auth.ExternalIdentityMapping("dave@salesforce.com", "Salesforce", "SAML");
            var externalMapping2 = new BluePrism.AutomateAppCore.Auth.ExternalIdentityMapping("steve@salesforce.com", "Salesforce", "SAML");
            var result = externalMapping1.Equals(externalMapping2);
            result.Should().BeFalse();
        }

        [Test]
        public void TestEquals_IdProvidersDiffer_ReturnsFalse()
        {
            var externalMapping1 = new BluePrism.AutomateAppCore.Auth.ExternalIdentityMapping("dave@salesforce.com", "Salesforce", "SAML");
            var externalMapping2 = new BluePrism.AutomateAppCore.Auth.ExternalIdentityMapping("dave@salesforce.com", "facebook", "OpenID Connect");
            var result = externalMapping1.Equals(externalMapping2);
            result.Should().BeFalse();
        }

        [Test]
        public void TestEquals_ExternalIdsDifferByCaseOnly_ReturnsFalse()
        {
            var externalMapping1 = new BluePrism.AutomateAppCore.Auth.ExternalIdentityMapping("dave@salesforce.com", "Salesforce", "SAML");
            var externalMapping2 = new BluePrism.AutomateAppCore.Auth.ExternalIdentityMapping("Dave@salesforce.com", "Salesforce", "SAML");
            var result = externalMapping1.Equals(externalMapping2);
            result.Should().BeFalse();
        }

        [Test]
        public void TestEquals_IdProvidersDifferByCaseOnly_ReturnsTrue()
        {
            var externalMapping1 = new BluePrism.AutomateAppCore.Auth.ExternalIdentityMapping("dave@salesforce.com", "salesforce", "SAML");
            var externalMapping2 = new BluePrism.AutomateAppCore.Auth.ExternalIdentityMapping("dave@salesforce.com", "Salesforce", "SAML");
            var result = externalMapping1.Equals(externalMapping2);
            result.Should().BeTrue();
        }

        [Test]
        public void TestEqualsOperator_ObjectsAreEqual_ReturnsTrue()
        {
            var externalMapping1 = new BluePrism.AutomateAppCore.Auth.ExternalIdentityMapping("dave@salesforce.com", "Salesforce", "SAML");
            var externalMapping2 = new BluePrism.AutomateAppCore.Auth.ExternalIdentityMapping("dave@salesforce.com", "Salesforce", "SAML");
            var result = externalMapping1 == externalMapping2;
            result.Should().BeTrue();
        }

        [Test]
        public void TestEqualsOperator_ObjectIsNull_ReturnsFalse()
        {
            var externalMapping1 = new BluePrism.AutomateAppCore.Auth.ExternalIdentityMapping("dave@salesforce.com", "Salesforce", "SAML");
            object externalMapping2 = null;
            var result = externalMapping1 == (BluePrism.AutomateAppCore.Auth.ExternalIdentityMapping)externalMapping2;
            result.Should().BeFalse();
        }

        [Test]
        public void TestNotEqualsOperator_ObjectsAreEqual_ReturnsFalse()
        {
            var externalMapping1 = new BluePrism.AutomateAppCore.Auth.ExternalIdentityMapping("dave@salesforce.com", "Salesforce", "SAML");
            var externalMapping2 = new BluePrism.AutomateAppCore.Auth.ExternalIdentityMapping("dave@salesforce.com", "Salesforce", "SAML");
            var result = externalMapping1 != externalMapping2;
            result.Should().BeFalse();
        }

        [Test]
        public void TestNotEqualsOperator_ObjectIsNull_ReturnsTrue()
        {
            var externalMapping1 = new BluePrism.AutomateAppCore.Auth.ExternalIdentityMapping("dave@salesforce.com", "Salesforce", "SAML");
            object externalMapping2 = null;
            var result = externalMapping1 != (BluePrism.AutomateAppCore.Auth.ExternalIdentityMapping)externalMapping2;
            result.Should().BeTrue();
        }
    }
}
