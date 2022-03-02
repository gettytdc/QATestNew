using System.Collections.Generic;
using BluePrism.Server.Domain.Models;
using NUnit.Framework;

namespace BluePrism.BPCoreLib.UnitTests
{
    [TestFixture]
    public class LicenceImportTests
    {
        // An enterprise licence for 1 of everything throughout January 2019
        private const string LicenceEnterpriseJan2019 =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><license><type>enterprise</type><licensee>BluePrism</licensee><starts>2019-01-01</starts><expires>2019-01-31</expires><maxprocesses>1</maxprocesses><maxresources>1</maxresources><maxconcurrentsessions>1</maxconcurrentsessions><maxprocessalerts>1</maxprocessalerts><Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><SignedInfo><CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\"/><SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\"/><Reference URI=\"\"><Transforms><Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\"/></Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\"/><DigestValue>aqEgONr3JjKy8PCwhGrsWw0ZRF8=</DigestValue></Reference></SignedInfo><SignatureValue>DDVQBF9JjLGSMBAmWNJx6JPmTTd//WMwqDLRNE+yteR0iHRQ9JuC6YYzuNVs9XVx3LiJkIMsWxB9S4NhpHjRvUQXf0Jgk3ws1ifwT7LHHjo4Q7/uAzjJ7idMwgkDLi8IaPsk6USPvkqxDvf32+792PS2RHWFUNCJ6lli3EGuIgA=</SignatureValue></Signature></license>";

        // A standalone (DX) licence for 1 of everything throughout Jan 2019
        private const string LicenceStandaloneJan2019 =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><license><type>enterprise</type><standalone>true</standalone><licensee>BluePrism</licensee><starts>2019-01-01</starts><expires>2019-01-31</expires><maxprocesses>1</maxprocesses><maxresources>1</maxresources><maxconcurrentsessions>1</maxconcurrentsessions><maxprocessalerts>1</maxprocessalerts><Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><SignedInfo><CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\"/><SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\"/><Reference URI=\"\"><Transforms><Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\"/></Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\"/><DigestValue>aqEgONr3JjKy8PCwhGrsWw0ZRF8=</DigestValue></Reference></SignedInfo><SignatureValue>EDVQBF9JjLGSMBAmWNJx6JPmTTd//WMwqDLRNE+yteR0iHRQ9JuC6YYzuNVs9XVx3LiJkIMsWxB9S4NhpHjRvUQXf0Jgk3ws1ifwT7LHHjo4Q7/uAzjJ7idMwgkDLi8IaPsk6USPvkqxDvf32+792PS2RHWFUNCJ6lli3EGuIgA=</SignatureValue></Signature></license>";

        // A standalone (DX) licence for 1 of everything throughout Jan 2019
        private const string LicenceStandaloneFeb2019 =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><license><type>enterprise</type><standalone>true</standalone><licensee>BluePrism</licensee><starts>2019-02-01</starts><expires>2019-03-01</expires><maxprocesses>1</maxprocesses><maxresources>1</maxresources><maxconcurrentsessions>1</maxconcurrentsessions><maxprocessalerts>1</maxprocessalerts><Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><SignedInfo><CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\"/><SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\"/><Reference URI=\"\"><Transforms><Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\"/></Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\"/><DigestValue>aqEgONr3JjKy8PCwhGrsWw0ZRF8=</DigestValue></Reference></SignedInfo><SignatureValue>FDVQBF9JjLGSMBAmWNJx6JPmTTd//WMwqDLRNE+yteR0iHRQ9JuC6YYzuNVs9XVx3LiJkIMsWxB9S4NhpHjRvUQXf0Jgk3ws1ifwT7LHHjo4Q7/uAzjJ7idMwgkDLi8IaPsk6USPvkqxDvf32+792PS2RHWFUNCJ6lli3EGuIgA=</SignatureValue></Signature></license>";

        // A standalone (DX) licence for 1 of everything throughout Jan 2019
        private const string LicenceStandaloneOverLapsStartDateJan2019 =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><license><type>enterprise</type><standalone>true</standalone><licensee>BluePrism</licensee><starts>2019-01-15</starts><expires>2019-02-15</expires><maxprocesses>1</maxprocesses><maxresources>1</maxresources><maxconcurrentsessions>1</maxconcurrentsessions><maxprocessalerts>1</maxprocessalerts><Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><SignedInfo><CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\"/><SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\"/><Reference URI=\"\"><Transforms><Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\"/></Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\"/><DigestValue>aqEgONr3JjKy8PCwhGrsWw0ZRF8=</DigestValue></Reference></SignedInfo><SignatureValue>IDVQBF9JjLGSMBAmWNJx6JPmTTd//WMwqDLRNE+yteR0iHRQ9JuC6YYzuNVs9XVx3LiJkIMsWxB9S4NhpHjRvUQXf0Jgk3ws1ifwT7LHHjo4Q7/uAzjJ7idMwgkDLi8IaPsk6USPvkqxDvf32+792PS2RHWFUNCJ6lli3EGuIgA=</SignatureValue></Signature></license>";

        // An enterprise licence for 1 of everything throughout January 2019
        private const string LicenceEnterpriseOverlapsStartDateJan2019 =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><license><type>enterprise</type><standalone>true</standalone><licensee>BluePrism</licensee><starts>2019-01-15</starts><expires>2019-02-15</expires><maxprocesses>1</maxprocesses><maxresources>1</maxresources><maxconcurrentsessions>1</maxconcurrentsessions><maxprocessalerts>1</maxprocessalerts><Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><SignedInfo><CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\"/><SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\"/><Reference URI=\"\"><Transforms><Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\"/></Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\"/><DigestValue>aqEgONr3JjKy8PCwhGrsWw0ZRF8=</DigestValue></Reference></SignedInfo><SignatureValue>DDVQBF9JjLGSMBAmWNJx6JPmTTd//WMwqDLRNE+yteR0iHRQ9JuC6YYzuNVs9XVx3LiJkIMsWxB9S4NhpHjRvUQXf0Jgk3ws1ifwT7LHHjo4Q7/uAzjJ7idMwgkDLi8IaPsk6USPvkqxDvf32+792PS2RHWFUNCJ6lli3EGuIgA=</SignatureValue></Signature></license>";

        // A standalone (DX) licence for 1 of everything throughout Jan 2019
        private const string LicenceStandaloneOverLapsExpiryDateJan2019 =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><license><type>enterprise</type><standalone>true</standalone><licensee>BluePrism</licensee><starts>2018-12-15</starts><expires>2019-01-15</expires><maxprocesses>1</maxprocesses><maxresources>1</maxresources><maxconcurrentsessions>1</maxconcurrentsessions><maxprocessalerts>1</maxprocessalerts><Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><SignedInfo><CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\"/><SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\"/><Reference URI=\"\"><Transforms><Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\"/></Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\"/><DigestValue>aqEgONr3JjKy8PCwhGrsWw0ZRF8=</DigestValue></Reference></SignedInfo><SignatureValue>IDVQBF9JjLGSMBAmWNJx6JPmTTd//WMwqDLRNE+yteR0iHRQ9JuC6YYzuNVs9XVx3LiJkIMsWxB9S4NhpHjRvUQXf0Jgk3ws1ifwT7LHHjo4Q7/uAzjJ7idMwgkDLi8IaPsk6USPvkqxDvf32+792PS2RHWFUNCJ6lli3EGuIgA=</SignatureValue></Signature></license>";

        // A standalone (DX) licence for 1 of everything throughout Jan 2019
        private const string LicenceStandaloneOverLapsStartAndExpiryDateJan2019 =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><license><type>enterprise</type><standalone>true</standalone><licensee>BluePrism</licensee><starts>2018-12-15</starts><expires>2019-02-15</expires><maxprocesses>1</maxprocesses><maxresources>1</maxresources><maxconcurrentsessions>1</maxconcurrentsessions><maxprocessalerts>1</maxprocessalerts><Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><SignedInfo><CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\"/><SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\"/><Reference URI=\"\"><Transforms><Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\"/></Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\"/><DigestValue>aqEgONr3JjKy8PCwhGrsWw0ZRF8=</DigestValue></Reference></SignedInfo><SignatureValue>IDVQBF9JjLGSMBAmWNJx6JPmTTd//WMwqDLRNE+yteR0iHRQ9JuC6YYzuNVs9XVx3LiJkIMsWxB9S4NhpHjRvUQXf0Jgk3ws1ifwT7LHHjo4Q7/uAzjJ7idMwgkDLi8IaPsk6USPvkqxDvf32+792PS2RHWFUNCJ6lli3EGuIgA=</SignatureValue></Signature></license>";

        // An enterprise licence for 1 of everything throughout January 2019
        private const string LicenceNhsJan2019 =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><license><type>nhs</type><licensee>BluePrism</licensee><starts>2019-01-01</starts><expires>2019-01-31</expires><maxprocesses>1</maxprocesses><maxresources>1</maxresources><maxconcurrentsessions>1</maxconcurrentsessions><maxprocessalerts>1</maxprocessalerts><Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><SignedInfo><CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\"/><SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\"/><Reference URI=\"\"><Transforms><Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\"/></Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\"/><DigestValue>aqEgONr3JjKy8PCwhGrsWw0ZRF8=</DigestValue></Reference></SignedInfo><SignatureValue>GDVQBF9JjLGSMBAmWNJx6JPmTTd//WMwqDLRNE+yteR0iHRQ9JuC6YYzuNVs9XVx3LiJkIMsWxB9S4NhpHjRvUQXf0Jgk3ws1ifwT7LHHjo4Q7/uAzjJ7idMwgkDLi8IaPsk6USPvkqxDvf32+792PS2RHWFUNCJ6lli3EGuIgA=</SignatureValue></Signature></license>";

        /// <summary>
        /// Tests that where the keys contain an Standalone Key that overlaps with the incoming Enterprise key
        /// CanImportLicence Throws LicenceRestrictionException
        /// </summary>
        [Test]
        public void CanImportThrowsExceptionWhenEnterpriseOverlapsStandalone()
        {
            var keys = new List<KeyInfo>();
            var enterpriseOverlappingKey = new KeyInfo(LicenceEnterpriseOverlapsStartDateJan2019);
            var standaloneLicenceJan2019Key = new KeyInfo(LicenceStandaloneJan2019);
            keys.Add(standaloneLicenceJan2019Key);
            Assert.Throws<LicenseOverlapException>(() => Licensing.DoesNotOverlap(enterpriseOverlappingKey, keys));
        }

        /// <summary>
        /// Tests the where the keys contain an enterprise Key that
        /// overlaps with the incoming key CanImportLicence Throws LicenceRestrictionException
        /// </summary>
        [Test]
        public void CanImportThrowsExceptionWhenStandaloneStartDateOverlapsStandalone()
        {
            var keys = new List<KeyInfo>();
            var standaloneOverlappingKey = new KeyInfo(LicenceStandaloneOverLapsStartAndExpiryDateJan2019);
            var standaloneLicenceJan2019Key = new KeyInfo(LicenceStandaloneJan2019);
            keys.Add(standaloneLicenceJan2019Key);
            Assert.Throws<LicenseOverlapException>(() => Licensing.DoesNotOverlap(standaloneOverlappingKey, keys));
        }

        /// <summary>
        /// Tests the where the keys contain an enterprise Key that
        /// overlaps with the incoming key CanImportLicence Throws LicenceRestrictionException
        /// </summary>
        [Test]
        public void CanImportThrowsExceptionWhenStandaloneStartDateOverlapsStandaloneStartBeforeAndExpiryAfter()
        {
            var keys = new List<KeyInfo>();
            var standaloneOverlappingKey = new KeyInfo(LicenceStandaloneOverLapsExpiryDateJan2019);
            var standaloneLicenceJan2019Key = new KeyInfo(LicenceStandaloneJan2019);
            keys.Add(standaloneLicenceJan2019Key);
            Assert.Throws<LicenseOverlapException>(() => Licensing.DoesNotOverlap(standaloneOverlappingKey, keys));
        }

        /// <summary>
        /// Tests the where the keys contain an enterprise Key that
        /// overlaps with the incoming key CanImportLicence Throws LicenceRestrictionException
        /// </summary>
        [Test]
        public void CanImportThrowsExceptionWhenEnterpriseStartDateOverlapsStandalone()
        {
            var keys = new List<KeyInfo>();
            var standaloneOverlappingKey = new KeyInfo(LicenceStandaloneOverLapsExpiryDateJan2019);
            var standaloneLicenceJan2019Key = new KeyInfo(LicenceStandaloneJan2019);
            keys.Add(standaloneLicenceJan2019Key);
            Assert.Throws<LicenseOverlapException>(() => Licensing.DoesNotOverlap(standaloneOverlappingKey, keys));
        }

        /// <summary>
        /// Tests the where the keys contain an enterprise Key that
        /// overlaps with the incoming key CanImportLicence Throws LicenceRestrictionException
        /// </summary>
        [Test]
        public void CanImportThrowsExceptionWhenStandaloneExpiryDateOverlapsStandalone()
        {
            var keys = new List<KeyInfo>();
            var standaloneOverlappingKey = new KeyInfo(LicenceStandaloneOverLapsStartDateJan2019);
            var standaloneLicenceJan2019Key = new KeyInfo(LicenceStandaloneJan2019);
            keys.Add(standaloneLicenceJan2019Key);
            Assert.Throws<LicenseOverlapException>(() => Licensing.DoesNotOverlap(standaloneOverlappingKey, keys));
        }

        /// <summary>
        /// Tests the where the keys contain an enterprise Key that
        /// does not overlaps with the incoming key CanImportLicence returns false
        /// </summary>
        [Test]
        public void CanImportDoesNotThrowExceptionWhenStandaloneDoesNotOverlapEnterprise()
        {
            var keys = new List<KeyInfo>();
            var enterpriseJan2019Key = new KeyInfo(LicenceEnterpriseJan2019);
            var standaloneLicenceFeb2019Key = new KeyInfo(LicenceStandaloneFeb2019);
            keys.Add(enterpriseJan2019Key);
            Assert.DoesNotThrow(() => Licensing.DoesNotOverlap(standaloneLicenceFeb2019Key, keys));
        }

        /// <summary>
        /// Tests the where the keys are empty that
        /// CanImportLicence does not throw exception
        /// </summary>
        [Test]
        public void CanImportDoesNotThrowExceptionsWhenStandaloneImportsIntoEmptyKeys()
        {
            var keys = new List<KeyInfo>();
            var standaloneLicenceFeb2019Key = new KeyInfo(LicenceStandaloneFeb2019);
            Assert.DoesNotThrow(() => Licensing.DoesNotOverlap(standaloneLicenceFeb2019Key, keys));
        }

        /// <summary>
        /// Tests the where the keys contain an enterprise license Key
        /// that you cannot import an Nhs one
        /// </summary>
        [Test]
        public void CanImportThrowsExceptionWhenNhsIsImportedAgainstEnterprise()
        {
            var keys = new List<KeyInfo>();
            var enterpriseJan2019Key = new KeyInfo(LicenceEnterpriseJan2019);
            var nhsLicenseKey = new KeyInfo(LicenceNhsJan2019);
            keys.Add(enterpriseJan2019Key);
            Assert.Throws<InvalidTypeException>(() => Licensing.DoesNotOverlap(nhsLicenseKey, keys));
        }

        /// <summary>
        /// Tests the where the keys an Nhs license Key
        /// that you cannot import an Nhs one
        /// </summary>
        [Test]
        public void CanImportThrowsExceptionWhenEnterpriseIsImportedAgainstNhs()
        {
            var keys = new List<KeyInfo>();
            var enterpriseJan2019Key = new KeyInfo(LicenceEnterpriseJan2019);
            var nhsLicenseKey = new KeyInfo(LicenceNhsJan2019);
            keys.Add(nhsLicenseKey);
            Assert.Throws<InvalidTypeException>(() => Licensing.DoesNotOverlap(enterpriseJan2019Key, keys));
        }

        [Test]
        public void CanImportThrowsExceptionWhenDuplicateLicenseIsPassed()
        {
            var keys = new List<KeyInfo>();
            var enterpriseJan2019Key = new KeyInfo(LicenceEnterpriseJan2019);
            keys.Add(enterpriseJan2019Key);
            Assert.Throws<AlreadyExistsException>(() => Licensing.DoesNotOverlap(enterpriseJan2019Key, keys));
        }
    }
}
