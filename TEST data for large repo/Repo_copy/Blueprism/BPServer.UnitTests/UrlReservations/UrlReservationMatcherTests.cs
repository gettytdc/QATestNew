#if UNITTESTS
using BluePrism.BPServer.UrlReservations;
using NUnit.Framework;

namespace BluePrism.BPServer.UnitTests.UrlReservations
{
    public class UrlReservationMatcherTests

        //The ACL check should only accept wildcard as matching if there is no binding, and it should only accept a specific (non case sensitive match) where there is a binding.
        //The ACL check should cater for both where the URL ends with /bpserver/ and where it does not
    {
        // Exact matches
        [TestCase("https://*:8199/", "https://*:8199/", UrlReservationMatchType.ExactMatch)]
        [TestCase("https://+:8199/", "https://+:8199/", UrlReservationMatchType.ExactMatch)]
        [TestCase("https://+:8199/", "https://*:8199/bpserver/", UrlReservationMatchType.ExactMatch)]
        [TestCase("https://*:8199/", "https://*:8199/bpserver/", UrlReservationMatchType.ExactMatch)]
        [TestCase("https://+:8199/bpserver/", "https://+:8199/", UrlReservationMatchType.ExactMatch)]
        [TestCase("https://*:8199/", "https://+:8199/", UrlReservationMatchType.ExactMatch)]
        [TestCase("https://*:8199/bpserver/", "https://+:8199/", UrlReservationMatchType.ExactMatch)]
        [TestCase("http://myserver:8199/", "http://myserver:8199/", UrlReservationMatchType.ExactMatch)]
        [TestCase("http://myserver:8199/bpserver/", "http://myserver:8199/", UrlReservationMatchType.ExactMatch)]

       
        // One wildcard URL with a binding url is a conflict. Causes connection to server to fail
        // when using a binding with both a specific url and wildcard url reservation.
        // see BG-821.
        [TestCase("http://myserver:8199/", "http://*:8199/", UrlReservationMatchType.Conflict)]
        [TestCase("http://myserver:8199/", "http://+:8199/", UrlReservationMatchType.Conflict)]
        [TestCase("http://myserver:8199/bpserver/", "http://*:8199/bpserver/", UrlReservationMatchType.Conflict)]

        // ...even with different protocols
        [TestCase("http://*:8199/", "https://myserver:8199/", UrlReservationMatchType.None)]
        [TestCase("https://+:8199/bpserver/", "http://129.24.0.1:8199/bpserver/", UrlReservationMatchType.None)]

        // Different hostnames / wildcards using opposite protocols conflict
        [TestCase("http://myserver1:8199/", "https://myserver2:8199/", UrlReservationMatchType.Conflict)]
        [TestCase("http://+:8199/", "https://*:8199/", UrlReservationMatchType.Conflict)]
        [TestCase("http://+:8199/bpserver/", "https://*:8199/bpserver/", UrlReservationMatchType.Conflict)]
       
        // Different IP addresses with opposite protocols don't match/conflict
        [TestCase("http://127.0.0.1:8199/", "https://127.0.0.2:8199/", UrlReservationMatchType.None)]

        // OK if different ports
        [TestCase("http://myserver:8199/", "https://myserver:8299/", UrlReservationMatchType.None)]
        [TestCase("https://myserver:8199/", "http://myserver:8299/", UrlReservationMatchType.None)]
        [TestCase("http://127.0.0.1:8199/", "https://127.0.0.2:8299/", UrlReservationMatchType.None)]

        public void ShouldDetectUrlMatchType(string url1, string url2, UrlReservationMatchType expectedMatchType)
        {
            var result = UrlReservationMatcher.Compare(url1, url2);
            Assert.That(result, Is.EqualTo(expectedMatchType));
            result = UrlReservationMatcher.Compare(url2, url1);
            Assert.That(result, Is.EqualTo(expectedMatchType));
        }


        [TestCase("http://+:8199/bpserverJustaLoadOfOldRubbishToSeeIfItMatches", "https://*:8199/")]
        [TestCase("https://+:8199/bpserver", "https://+:8199/")]
        [TestCase("https://+:8199/bpserver/", "https://+:8199")]
        public void ShouldDetectInvalidURL(string url1, string url2)
        {
            var ex = Assert.Throws<System.FormatException>(() => UrlReservationMatcher.Compare(url1, url2));
        }
    }
}

#endif