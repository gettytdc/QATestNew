#if UNITTESTS

using System;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using BluePrism.Core.HttpConfiguration;
using BluePrism.Core.TestSupport;
using NUnit.Framework;

namespace BluePrism.Core.UnitTests.HttpConfiguration
{
    [Explicit("Intended for manual testing - need to be run as administrator")]
    public class HttpConfigurationServiceTests
    {
        /// <summary>
        /// Template used for URLs used for URL reservations - single placeholder
        /// is used for a random GUID value
        /// </summary>
        private const string TestUrlTemplate = "http://localhost:56789/bptest-{0}/";

        [SetUp]
        public void Setup()
        {
            RemoveTestUrlReservations();
        }

        [TearDown]
        public void Teardown()
        {
            RemoveTestUrlReservations();
        }

        [Test]
        public void GetUrlReservationsShouldFindExistingValues()
        {
            var windowsIdentity = WindowsIdentity.GetCurrent();
            string userName = windowsIdentity.Name;
            string userSid = windowsIdentity.User.Value;

            var testUrl1 = CreateTestReservation(userName, null, true, false);
            var testUrl2 = CreateTestReservation(userName, null, false, true);

            var service = new HttpConfigurationService();
            var reservations = service.GetUrlReservations().ToList();

            Assert.That(reservations, Is.Not.Empty);
            var reservation1 = reservations.SingleOrDefault(x => x.Url == testUrl1);
            var reservation2 = reservations.SingleOrDefault(x => x.Url == testUrl2);
            
            AssertValidReservationWithSingleAccessControlEntry(reservation1, userSid, true, false);
            AssertValidReservationWithSingleAccessControlEntry(reservation2, userSid, false, true);
        }

        [Test]
        public void GetUrlReservationsShouldConvertWellKnownSidConstantsToStandardSidStrings()
        {
            // SDDL string representing 3 users
            const string sddl = "D:(A;;GX;;;S-1-5-20)" // Network Service NS
                                + "(A;;GW;;;S-1-5-19)" // Local Service LS
                                + "(A;;GA;;;S-1-5-18)"; // Local System SY
            string testUrl = CreateTestReservation(null, sddl, false, false);

            var service = new HttpConfigurationService();
            var reservations = service.GetUrlReservations().ToList();

            Assert.That(reservations, Is.Not.Empty);
            var reservation = reservations.SingleOrDefault(x => x.Url == testUrl);
            Assert.That(reservation, Is.Not.Null);
            var entries = reservation.SecurityDescriptor.Entries;
            Assert.That(entries, Has.Count.EqualTo(3));
            Assert.That(entries[0].Sid, Is.EqualTo("S-1-5-20"));
            Assert.That(entries[0].RawSid, Is.EqualTo("NS"));
            Assert.That(entries[1].Sid, Is.EqualTo("S-1-5-19"));
            Assert.That(entries[1].RawSid, Is.EqualTo("LS"));
            Assert.That(entries[2].Sid, Is.EqualTo("S-1-5-18"));
            Assert.That(entries[2].RawSid, Is.EqualTo("SY"));
        }

        [Test]
        public void GetUrlReservationsShouldIncludeReservationsForMultipleUsers()
        {
            // SDDL string representing 3 users
            const string sddl = "D:(A;;GX;;;S-1-5-21-988206371-2963552263-2054333697-1002)" 
                + "(A;;GW;;;S-1-5-21-988206371-2963552263-2054333697-1003)" 
                + "(A;;GA;;;S-1-5-21-988206371-2963552263-2054333697-1004)";
            string testUrl = CreateTestReservation(null, sddl, false, false);

            var service = new HttpConfigurationService();
            var reservations = service.GetUrlReservations().ToList();
            
            var reservation = reservations.SingleOrDefault(x => x.Url == testUrl);
            Assert.That(reservation, Is.Not.Null);
            var entries = reservation.SecurityDescriptor.Entries;
            Assert.That(entries, Has.Count.EqualTo(3));
            AssertValidAccessControlEntry(entries[0], "S-1-5-21-988206371-2963552263-2054333697-1002", true, false);
            AssertValidAccessControlEntry(entries[1], "S-1-5-21-988206371-2963552263-2054333697-1003", false, true);
            AssertValidAccessControlEntry(entries[2], "S-1-5-21-988206371-2963552263-2054333697-1004", true, true);
        }

        [Test]
        public void AddUrlReservationShouldAddReservationWithSingleUser()
        {
            string url = CreateTestUrl();
            const string sddl = "D:(A;;GX;;;S-1-5-21-988206371-2963552263-2054333697-1002)";
            var securityDescriptor = SecurityDescriptor.Parse(sddl);
            var reservation1 = new UrlReservation(url, securityDescriptor);
            var service = new HttpConfigurationService();
            service.AddUrlReservation(reservation1);

            var reservations = service.GetUrlReservations();
            var reservation2 = reservations.SingleOrDefault(x => x.Url == url);
            Assert.That(reservation2, Is.Not.Null);
            var entries = reservation2.SecurityDescriptor.Entries;
            Assert.That(entries, Has.Count.EqualTo(1));
            AssertValidAccessControlEntry(entries[0], "S-1-5-21-988206371-2963552263-2054333697-1002", true, false);
        }

        [Test]
        public void AddUrlReservationShouldAddReservationWithManyUsers()
        {
            // SDDL string representing 3 users
            string url = CreateTestUrl();
            const string sddl = "D:(A;;GX;;;S-1-5-21-988206371-2963552263-2054333697-1002)"
                                + "(A;;GW;;;S-1-5-21-988206371-2963552263-2054333697-1003)"
                                + "(A;;GA;;;S-1-5-21-988206371-2963552263-2054333697-1004)";
            var securityDescriptor = SecurityDescriptor.Parse(sddl);
            var reservation1 = new UrlReservation(url, securityDescriptor);
            var service = new HttpConfigurationService();
            service.AddUrlReservation(reservation1);

            var reservations = service.GetUrlReservations();
            var reservation2 = reservations.SingleOrDefault(x => x.Url == url);
            Assert.That(reservation2, Is.Not.Null);
            var entries = reservation2.SecurityDescriptor.Entries;
            Assert.That(entries, Has.Count.EqualTo(3));
            AssertValidAccessControlEntry(entries[0], "S-1-5-21-988206371-2963552263-2054333697-1002", true, false);
            AssertValidAccessControlEntry(entries[1], "S-1-5-21-988206371-2963552263-2054333697-1003", false, true);
            AssertValidAccessControlEntry(entries[2], "S-1-5-21-988206371-2963552263-2054333697-1004", true, true);

        }

        [Test]
        public void RemoveUrlReservationShouldRemoveReservation()
        {
            var windowsIdentity = WindowsIdentity.GetCurrent();
            string userName = windowsIdentity.Name;
            var testUrl = CreateTestReservation(userName, null, true, false);
            var service = new HttpConfigurationService();
            var reservations = service.GetUrlReservations();
            var reservation = reservations.SingleOrDefault(x => x.Url == testUrl);
            Assert.That(reservation, Is.Not.Null);

            service.DeleteUrlReservation(testUrl);
            
            var reservations2 = service.GetUrlReservations();
            var reservation2 = reservations2.SingleOrDefault(x => x.Url == testUrl);
            Assert.That(reservation2, Is.Null);
        }

        /// <summary>
        /// Creates a test URL reservation using netsh and returns the test URL that
        /// was generated for the reservation
        /// </summary>
        /// <returns></returns>
        private static string CreateTestReservation(string userName, string sddl, bool listen, bool @delegate)
        {
            if (sddl == null && userName == null) throw new ArgumentNullException(nameof(sddl), "Must be supplied if userName is null");

            string url = CreateTestUrl();
            string args;
            if (userName != null)
            {
                args = string.Format("http add urlacl url={0} user={1} listen={2} delegate={3}",
                    url,
                    userName,
                    listen ? "yes" : "no",
                    @delegate ? "yes" : "no"
                );
            }
            else
            {
                args = string.Format("http add urlacl url={0} sddl={1}", url, sddl);
            }
            CommandLineHelper.ExecCommand("netsh", args, true);
            return url;
        }

        private static string CreateTestUrl()
        {
            return string.Format(TestUrlTemplate, Guid.NewGuid());
        }

        private void AssertValidReservationWithSingleAccessControlEntry(UrlReservation reservation, string sid, bool allowListen, bool allowDelegate)
        {
            Assert.That(reservation.SecurityDescriptor.Entries.Count == 1);
            var entry = reservation.SecurityDescriptor.Entries[0];
            AssertValidAccessControlEntry(entry, sid, allowListen, allowDelegate);
        }

        private static void AssertValidAccessControlEntry(AccessControlEntry entry, string sid, bool allowListen, bool allowDelegate)
        {
            Assert.That(entry.Sid, Is.EqualTo(sid));
            Assert.That(entry.AllowListen, Is.EqualTo(allowListen));
            Assert.That(entry.AllowDelegate, Is.EqualTo(allowDelegate));
        }

        private static void RemoveTestUrlReservations()
        {
            var result = CommandLineHelper.ExecCommand("netsh", "http show urlacl", true);
            // Get URLs of all reservations that match test URL pattern and delete the
            // URL reservations - primitive but simple
            var match = Regex.Match(result.Output, "http://localhost:56789/bptest-[A-Fa-f0-9-]*/");
            var urls = match.Captures.Cast<Capture>().Select(x => x.Value);
            foreach (string url in urls)
            {
                string args = string.Format("http delete urlacl url={0}", url);
                CommandLineHelper.ExecCommand("netsh", args, true);
            }
        }
    }
}
#endif