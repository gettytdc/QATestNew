#if UNITTESTS
using BluePrism.Core.WindowsSecurity;
using NUnit.Framework;

namespace BluePrism.Core.UnitTests.WindowsSecurity
{
    public class AccountSidTranslatorTests
    {
        [TestCase("Administrators")]
        public void ShouldTranslateLocalGroupsToSids(string accountName)
        {
            AssertTranslatesToSid(accountName);
        }

        [TestCase("NetworkService")]
        [TestCase("LocalService")]
        [TestCase("Administrator")]
        [TestCase("Guest")]
        public void ShouldTranslateLocalUsersToSids(string accountName)
        {
            AssertTranslatesToSid(accountName);
        }

        [Test]
        public void ShouldTranslateLocalSystemUserNameToSid()
        {
            AssertTranslatesToSid("LocalSystem");
        }

        [TestCase("NS", ExpectedResult = "S-1-5-20")]
        [TestCase("LS", ExpectedResult = "S-1-5-19")]
        [TestCase("SY", ExpectedResult = "S-1-5-18")]
        public string EnsureStandardSidStringShouldTranslateSidContants(string sid)
        {
            return AccountSidTranslator.EnsureStandardSidString(sid);
        }

        private static void AssertTranslatesToSid(string accountName)
        {
            string sid = AccountSidTranslator.GetSidFromUserName(accountName);
            Assert.That(sid, Is.Not.Null);
            Assert.That(sid, Is.Not.Empty);
        }
    }
}
#endif