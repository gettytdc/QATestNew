#if UNITTESTS

using NUnit.Framework;

namespace BluePrism.BPServer.UnitTests
{
    /// <summary>
    /// Tests for the <see cref="BindingProperties"/> class.
    /// </summary>
    [TestFixture]
    public class BindingPropertiesTests
    {
        [TestCase("", 8199, false, ExpectedResult = "http://+:8199/bpserver/")]
        [TestCase("", 8199, true, ExpectedResult = "https://+:8199/bpserver/")]
        [TestCase("myserver.com", 8199, false, ExpectedResult = "http://myserver.com:8199/bpserver/")]
        [TestCase("myserver-001.com", 9999, true, ExpectedResult = "https://myserver-001.com:9999/bpserver/")]
        [TestCase("fe80::7cb9:6c54:938f:456d", 9999, true, ExpectedResult = "https://[fe80::7cb9:6c54:938f:456d]:9999/bpserver/")]
        [TestCase("::1", 9999, true, ExpectedResult = "https://[::1]:9999/bpserver/")]
        public string ShouldGenerateReservationUrl(string address, int port, bool secure)
        {
            var properties = new BindingProperties(address, port, secure);
            return properties.BindingReservationUrl;
        }

       
        [TestCase("", 8199, false, "http://+:8199/", ExpectedResult = true)]
        [TestCase("", 8199, false, "http://+:8999/", ExpectedResult = false)]
        [TestCase("", 8199, false, "https://+:8199/", ExpectedResult = false)]
        [TestCase("", 8199, true, "https://+:8199/", ExpectedResult = true)]
        [TestCase("myserver", 8199, false, "http://MySeRvEr:8199/BPSERVER/", ExpectedResult = true)]
        [TestCase("myserver", 9999, true, "https://myserver:9999/", ExpectedResult = true)]
        [TestCase("192.168.0.1", 8199, true, "https://192.168.0.1:8199/bpserver/", ExpectedResult = true)]
        [TestCase("fe80::7cb9:6c54:938f:456d", 9999, true, "https://[fe80::7cb9:6c54:938f:456d]:9999/bpserver/", ExpectedResult = true)]
        [TestCase("::1", 9999, true, "https://[::1]:9999/bpserver/", ExpectedResult = true)]
        public bool ShouldMatchValidReservationUrl(string address, int port, bool secure, string urlToTest)
        {
            var properties = new BindingProperties(address, port, secure);
            return properties.MatchesReservationUrl(urlToTest);
        }

        
        [TestCase("myserver", 8199, false, "https://+:8199/", ExpectedResult = false)] //different protocol
        [TestCase("192.168.0.1", 8199, true, "https://192.168.0.1:8200/", ExpectedResult = false)] // different port
        [TestCase("192.168.0.1", 8199, true, "https://192.168.0.11:8199/", ExpectedResult = false)] // different address
        [TestCase("myserver", 8199, true, "https://*:8199/", ExpectedResult = false)] // specific address vs wildcard
        [TestCase("", 8199, true, "https://myserver:8199/", ExpectedResult = false)] // wildcard vs specific address
        public bool ShouldNotMatchInvalidReservationUrl(string Address, int port, bool secure, string urlToTest)
        {
            var Properties = new BindingProperties(Address, port, secure);
            return Properties.MatchesReservationUrl(urlToTest);
        }


        [TestCase("http://+:8199/bpserver/", "", 8199, false)]
        [TestCase("https://+:8200/bpserver/", "", 8200, true)]
        [TestCase("http://*:8199/bpserver/", "", 8199, false)]
        [TestCase("https://*:8200/bpserver/", "", 8200, true)]
        [TestCase("https://*:8200/", "", 8200, true)]
        [TestCase("http://+:8199/", "", 8199, false)]
        [TestCase("http://myserver1:8199/bpserver/", "myserver1", 8199, false)]
        [TestCase("https://myserver2:8200/bpserver/", "myserver2", 8200, true)]
        [TestCase("http://MYSERVER1:8199/bpserver/", "MYSERVER1", 8199, false)]
        [TestCase("https://MYSERVER2:8200/bpserver/", "MYSERVER2", 8200, true)]
        [TestCase("http://MYSERVER-1:8199/bpserver/", "MYSERVER-1", 8199, false)]
        [TestCase("https://MYSERVER-2:8200/bpserver/", "MYSERVER-2", 8200, true)]
        [TestCase("https://MYSERVER_2:8200/bpserver/", "MYSERVER_2", 8200, true)]
        [TestCase("http://myserver1:8199/", "myserver1", 8199, false)]
        [TestCase("https://MYSERVER2:8200/", "MYSERVER2", 8200, true)]
        [TestCase("http://MYSERVER-1:8199/", "MYSERVER-1", 8199, false)]
        [TestCase("http://[fe80::7cb9:6c54:938f:456d]:8199/", "fe80::7cb9:6c54:938f:456d", 8199, false)]
        [TestCase("http://[::1]:8199/", "::1", 8199, false)]
        public void ShouldParseReservationUrl(string reservationUrl, string address, int port, bool secure)
        {
            var properties = BindingProperties.ParseReservationUrl(reservationUrl);
            Assert.That(properties, Is.Not.Null);
            Assert.That(properties.Address, Is.EqualTo(address));
            Assert.That(properties.Port, Is.EqualTo(port));
            Assert.That(properties.Secure, Is.EqualTo(secure));
        }

        /// <summary>
        /// These are examples of the urls returned by the call to _configurationService.GetUrlReservations,
        /// so we need to make sure we can parse them successfully without losing the functionality of 
        /// checking for correct formatting of the url when it has a /bpserver/ suffix.
        /// </summary>
        [TestCase("http://+:80/Temporary_Listen_Addresses/","",80,false)]
        [TestCase("https://+:443/sra_{BA195980-CD49-458b-9E23-C84EE0ADCD75}/", "", 443, true)]
        [TestCase("http://+:10247/apps/","",10247,false)]
        [TestCase("http://+:10246/MDEServer/", "", 10246, false)]
        [TestCase("https://+:10245/WMPNSSv4/", "", 10245, true)]
        [TestCase("http://+:10243/WMPNSSv4/", "", 10243, false)]
        [TestCase("http://+:80/0131501b-d67f-491b-9a40-c4bf27bcb4d4/", "", 80, false)]
        [TestCase("https://+:443/C574AC30-5794-4AEE-B1BB-6651C5315029/", "", 443, true)]
        [TestCase("http://+:80/116B50EB-ECE2-41ac-8429-9F9E963361B7/", "", 80, false)]
        [TestCase("https://+:5986/wsman/", "", 5986, true)]
        [TestCase("http://+:47001/wsman/", "", 47001, false)]
        [TestCase("http://127.0.0.1:47873/help/", "127.0.0.1", 47873, false)]
        [TestCase("http://+:5985/wsman/", "", 5985, false)]
        public void ShouldParseValidMachineURLs(string reservationUrl, string address, int port, bool secure)
        {
            var properties = BindingProperties.ParseReservationUrl(reservationUrl);
            Assert.That(properties, Is.Not.Null);
            Assert.That(properties.Address, Is.EqualTo(address));
            Assert.That(properties.Port, Is.EqualTo(port));
            Assert.That(properties.Secure, Is.EqualTo(secure));
        }


        [TestCase("https://+:8199/", "", 8199, true)]
        [TestCase("https://+:8199/bpserver/", "", 8199, true)]
        [TestCase("https://MYSERVER-2:8200/", "MYSERVER-2", 8200, true)]
        [TestCase("https://MYSERVER-2:8200/bpserver/", "MYSERVER-2", 8200, true)]
        [TestCase("http://[fe80::7cb9:6c54:938f:456d]:8200/bpserver/", "fe80::7cb9:6c54:938f:456d", 8200, false)]
        public void TryParseReservationUrlShouldSucceedIfValid(string url, string address, int port, bool secure)
        {
            var success = BindingProperties.TryParseReservationUrl(url, out var properties);
            Assert.That(success, Is.True);
            Assert.That(properties, Is.Not.Null);
            Assert.That(properties.Address, Is.EqualTo(address));
            Assert.That(properties.Port, Is.EqualTo(port));
            Assert.That(properties.Secure, Is.EqualTo(secure));
        }


        [TestCase("htasdfasd")] // just nonsense
        [TestCase("http://%:8199")] // incorrect wildcard character
        [TestCase("https://+:8199/bpserver")] //missing final forward slashes
        [TestCase("http://+:8199")]
        [TestCase("https://myserver2:8200/bpserver")] 
        [TestCase("http://myserver2:8200")] 
        [TestCase("https://MYSERVER-2:8200/bpServer_JustAMeaninglessLoadOflettersThatStartWithBPServer...")]
        public void TryParseReservationUrlShouldFailIfInvalid(string url)
        {
            var success = BindingProperties.TryParseReservationUrl(url, out var properties);
            Assert.That(success, Is.False);
            Assert.That(properties, Is.Null);
        }


        [Test]
        public void IsIPAddressShouldIndicateValidIPAddress()
        {
            var properties = new BindingProperties("127.0.0.1", 8199, true);
            Assert.That(properties.IsIpAddress, Is.True);
        }


        [Test]
        public void IsIPAddressShouldIndicateInvalidIPAddress()
        {
            var properties = new BindingProperties("myserver", 8199, true);
            Assert.That(properties.IsIpAddress, Is.False);
        }
    }
}

#endif
