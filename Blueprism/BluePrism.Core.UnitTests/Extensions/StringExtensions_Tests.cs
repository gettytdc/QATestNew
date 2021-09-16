using BluePrism.Core.Extensions;
using NUnit.Framework;

namespace BluePrism.Core.UnitTests.Extensions
{
    public class StringExtensions_Tests
    {
        [TestFixture]
        public class GetIntTests
        {
            [Test]
            public void GetInt_ReturnsValue()
            {
                const int expected = 45234;
                string source = $"Thi$ s%nTenCE has a {expected} in IT";

                int? result = source.GetInt();

                Assert.IsTrue(result.HasValue);
                Assert.AreEqual(result.Value, expected);
            }

            [Test]
            public void GetInt_ReturnsNull()
            {
                const string source = null;

                int? result = source.GetInt();

                Assert.IsFalse(result.HasValue);
            }

            [Test]
            public void GetInt_ReturnsZero()
            {
                const string source = "sentence does not contain any number";

                int? result = source.GetInt();

                Assert.IsTrue(result.HasValue);
                Assert.IsTrue(result.Value == 0);
            }

            private static readonly string[] ValidTestUrls =
            {
                "http://www.google.com",
                "https://www.google.com",
                "http://google.com",
                "https://google.com",
                "http://localhost:8080",
                "https://localhost:8080",
                "http://localhost/something/something/dark/side.html:8080",
                "www.google.com",
                "www.google.com",
                "google.com",
                "google.com",
                "localhost:8080",
                "localhost:8080",
                "localhost/something/something/dark/side.html:8080",
                "127.0.0.1:8080",
                "127.0.0.1:8080",
                "127.0.0.1/something/something/dark/side.html:8080"
            };

            private static readonly string[] InvalidTestUrls =
            {
                "google",
                "www.google .com",
                "NotAURl",
                "Fred",
                "file://192.168.1.57/~User/2ndFile.html",
                ""
            };

            [Test, TestCaseSource("ValidTestUrls")]
            public void IsValidUrl_Returns_True_For_Valid_URL(string url)
            {
                var actual = url.IsValidUrl();

                Assert.IsTrue(actual);
            }

            [Test, TestCaseSource("InvalidTestUrls")]
            public void IsValidUrl_Returns_False_For_Invalid_URL(string url)
            {
                var actual = url.IsValidUrl();

                Assert.IsFalse(actual);
            }
        }
    }
}
