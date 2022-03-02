#if UNITTESTS

using System;
using NUnit.Framework;

namespace BluePrism.BPCoreLib.UnitTests
{
    [TestFixture]
    [RunInApplicationDomain]
    public class RegexTimeoutTests
    {
        [SetUp]
        public void Setup()
        {
            RegexTimeout.SetDefaultRegexTimeout();
        }

        [Test]
        public void XmlRegexTest()
        {

            // Ensure that a given regex picks up the default as long as it's been set
            var xmlTimeout = BPUtil.InvalidXmlCharRegex.MatchTimeout;
            var tenSecondTimeout = RegexTimeout.DefaultRegexTimeout;
            Assert.AreEqual(tenSecondTimeout, xmlTimeout);
        }

        [Test]
        public void RegexTest()
        {

            // Ensure that a given regex picks up the default as long as it's been set
            var timeout = RegexTimeout.DefaultRegexTimeout;
            var tenSecondTimeout = new TimeSpan(0, 0, 10);
            Assert.AreEqual(tenSecondTimeout, timeout);
        }
    }
}

#endif
