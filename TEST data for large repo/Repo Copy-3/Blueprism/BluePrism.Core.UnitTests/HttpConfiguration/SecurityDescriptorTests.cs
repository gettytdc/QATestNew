#if UNITTESTS

using System;
using BluePrism.Core.HttpConfiguration;
using NUnit.Framework;

namespace BluePrism.Core.UnitTests.HttpConfiguration
{
    /// <summary>
    /// Tests for parsing logic used internally within HttpConfigurationService
    /// </summary>
    public class SecurityDescriptorTests
    {
        [TestCaseSource("Cases")]
        public void ParseShouldParseSingleEntry(string input, bool allowRegister, bool allowDelegate, string ssid)
        {
            var descriptor = SecurityDescriptor.Parse(input);
            Assert.That(descriptor.Entries, Has.Count.EqualTo(1));
            var entry = descriptor.Entries[0];
            AssertValidEntry(entry, allowRegister, allowDelegate, ssid);
        }

        protected static object[] Cases =
        {
            new object[]{"D:(A;;GX;;;S-1-5-21-988206371-2963552263-2054333697-1001)", true, false,
                "S-1-5-21-988206371-2963552263-2054333697-1001"},
            new object[]{"D:(A;;GA;;;S-1-5-21-988206371-2963552263-2054333697-1002)", true, true,
                "S-1-5-21-988206371-2963552263-2054333697-1002"},
            new object[]{"D:(A;;GW;;;S-1-5-21-988206371-2963552263-2054333697-1003)", false, true,
                "S-1-5-21-988206371-2963552263-2054333697-1003"}
        };

        [Test]
        public void ParseShouldParseMultipleEntries()
        {
            const string input = "D:(A;;GX;;;S-1-5-21-988206371-2963552263-2054333697-1001)" 
                + "(A;;GA;;;S-1-5-21-988206371-2963552263-2054333697-1002)" 
                + "(A;;GW;;;S-1-5-21-988206371-2963552263-2054333697-1003)";
            var descriptor = SecurityDescriptor.Parse(input);
            
            Assert.That(descriptor.Entries, Has.Count.EqualTo(3));
            AssertValidEntry(descriptor.Entries[0], true, false, "S-1-5-21-988206371-2963552263-2054333697-1001");
            AssertValidEntry(descriptor.Entries[1], true, true, "S-1-5-21-988206371-2963552263-2054333697-1002");
            AssertValidEntry(descriptor.Entries[2], false, true, "S-1-5-21-988206371-2963552263-2054333697-1003");
        }

       [Test]
        public void ParseShouldFailWithInvalidInput()
        {
            Assert.Throws<FormatException>(() => SecurityDescriptor.Parse("nonsense"));
        }

        [Test]
        public void TryParseShouldSucceedWithValidInput()
        {
            SecurityDescriptor descriptor;
            string input = "D:(A;;GX;;;S-1-5-21-988206371-2963552263-2054333697-1001)";
            bool result = SecurityDescriptor.TryParse(input, out descriptor);
            Assert.That(result, Is.True);
            Assert.That(descriptor, Is.Not.Null);
        }

        [Test]
        public void TryParseShouldFailWithInvalidInput()
        {
            SecurityDescriptor descriptor;
            string input = "asdfasd!";
            bool result = SecurityDescriptor.TryParse(input, out descriptor);
            Assert.That(result, Is.False);
            Assert.That(descriptor, Is.Null);
        }

        [Test]
        public void SsdlStringShouldMatchEntries()
        {
            var entry1 = new AccessControlEntry("S-1-5-21-988206371-2963552263-2054333697-1001", true, false);
            var entry2 = new AccessControlEntry("S-1-5-21-988206371-2963552263-2054333697-1002", false, true);
            var entry3 = new AccessControlEntry("S-1-5-21-988206371-2963552263-2054333697-1003", true, true);
            var descriptor = new SecurityDescriptor(new[] { entry1, entry2, entry3 });

            const string expected = "D:(A;;GX;;;S-1-5-21-988206371-2963552263-2054333697-1001)"
                                + "(A;;GW;;;S-1-5-21-988206371-2963552263-2054333697-1002)"
                                + "(A;;GA;;;S-1-5-21-988206371-2963552263-2054333697-1003)";

            Assert.That(descriptor.SsdlString, Is.EqualTo(expected));
            
        }

        private void AssertValidEntry(AccessControlEntry entry, bool allowRegister, bool allowDelegate, string sid)
        {
            Assert.That(entry.AllowListen, Is.EqualTo(allowRegister));
            Assert.That(entry.AllowDelegate, Is.EqualTo(allowDelegate));
            Assert.That(entry.Sid, Is.EqualTo(sid));
        }
    }
}

#endif