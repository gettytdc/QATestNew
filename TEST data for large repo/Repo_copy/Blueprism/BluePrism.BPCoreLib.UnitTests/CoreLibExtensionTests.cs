#if UNITTESTS

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using BluePrism.BPCoreLib.Collections;
using BluePrism.BPCoreLib.Data;
using BluePrism.BPCoreLib.Diary;
using BluePrism.Server.Domain.Models;
using BluePrism.BPCoreLib.UnitTests.TestData;
using NUnit.Framework;

namespace BluePrism.BPCoreLib.UnitTests
{

    /// <summary>
    /// Tests the extension methods in the <see cref="Extensions"/> class, defined in the
    /// <see cref="BluePrism.BPCoreLib"/> namespace.
    /// </summary>
    [TestFixture]
    public class CoreLibExtensionTests
    {
        [Flags]
        public enum HasFlagTest
        {
            None = 0,
            One = 1,
            Two = 2,
            Four = 4,
            Eight = 8,
            Sixteen = 16,
            Five = Four | One,
            TwentyNine = Sixteen | Eight | Four | One
        }

        protected static readonly DateTimeTestData[] TestData = new[] {
            new DateTimeTestData()
            {
                StartDate = new DateTime(2019, 3, 16, 6, 56, 50, DateTimeKind.Local),
                ExpectedDate = new DateTime(2019, 3, 16, 0, 0, 0)
            },
            new DateTimeTestData() { StartDate = DateTime.MaxValue, ExpectedDate = new DateTime(9999, 12, 31, 0, 0, 0) },
            new DateTimeTestData() { StartDate = DateTime.MinValue, ExpectedDate = DateTime.MinValue }
        };

        /// <summary>
        /// Tests the <see cref="[Enum].HasFlag"/> method (largely to ensure I understood
        /// what it actually did), and the <see cref="HasAnyFlag"/> extension method
        /// which came about when I grasped the former's limitations.
        /// </summary>
        [Test]
        public void TestHasAnyFlag()
        {
            Assert.True(HasFlagTest.Four.HasFlag(HasFlagTest.Four));
            Assert.False(HasFlagTest.Eight.HasFlag(HasFlagTest.Four));
            Assert.True(HasFlagTest.Five.HasFlag(HasFlagTest.Four));

            // Has All Flags
            Assert.True(HasFlagTest.TwentyNine.HasFlag(HasFlagTest.Eight | HasFlagTest.Four));

            // Has Any Flags *is not* how HasFlag() works.
            Assert.False(HasFlagTest.TwentyNine.HasFlag(HasFlagTest.Eight | HasFlagTest.Two));

            // But it *is* how our HasAnyFlags() works
            // Check that all others operate as expected too
            Assert.True(HasFlagTest.Four.HasAnyFlag(HasFlagTest.Four));
            Assert.False(HasFlagTest.Eight.HasAnyFlag(HasFlagTest.Four));
            Assert.True(HasFlagTest.Five.HasAnyFlag(HasFlagTest.Four));
            Assert.True(HasFlagTest.TwentyNine.HasAnyFlag(HasFlagTest.Eight | HasFlagTest.Four));
            Assert.True(HasFlagTest.TwentyNine.HasAnyFlag(HasFlagTest.Eight | HasFlagTest.Two));

            // Zero (argument) should work the same. For HasFlag, it always returns
            // true; For HasAnyFlag it should always return true
            Assert.True(HasFlagTest.Eight.HasFlag(HasFlagTest.None));
            Assert.True(HasFlagTest.TwentyNine.HasFlag(HasFlagTest.None));
            Assert.True(HasFlagTest.None.HasFlag(HasFlagTest.None));
            Assert.True(HasFlagTest.Eight.HasAnyFlag(HasFlagTest.None));
            Assert.True(HasFlagTest.TwentyNine.HasAnyFlag(HasFlagTest.None));
            Assert.True(HasFlagTest.None.HasAnyFlag(HasFlagTest.None));
        }

        /// <summary>
        /// Tests the <see cref="SetFlags"/> extension method
        /// </summary>
        [Test]
        public void TestSetFlags()
        {
            var x = Direction.None;
            x = x.SetFlags(default);
            Assert.That(x, Is.EqualTo(Direction.None));
            x = x.SetFlags(Direction.Top);
            Assert.That(x, Is.EqualTo(Direction.Top));
            x = x.SetFlags(Direction.Left);
            Assert.That(x, Is.EqualTo(Direction.Top | Direction.Left));
            x = x.SetFlags(Direction.Left);
            Assert.That(x, Is.EqualTo(Direction.Top | Direction.Left));
            x = x.SetFlags((Direction)16);
            Assert.That(x, Is.EqualTo(Direction.Top | Direction.Left | (Direction)16));
            x = Direction.Top;
            x = x.SetFlags(Direction.Left | Direction.Bottom);
            Assert.That(x, Is.EqualTo(Direction.Top | Direction.Left | Direction.Bottom));
            x = x.SetFlags(Direction.None);
            Assert.That(x, Is.EqualTo(Direction.Top | Direction.Left | Direction.Bottom));
        }

        /// <summary>
        /// Tests the <see cref="ClearFlags"/> extension method
        /// </summary>
        [Test]
        public void TestClearFlags()
        {
            var x = Direction.None;
            x = x.ClearFlags(default);
            Assert.That(x, Is.EqualTo(Direction.None));
            x = x.ClearFlags(Direction.Top);
            Assert.That(x, Is.EqualTo(Direction.None));
            x = Direction.Top | Direction.Right | Direction.Bottom | Direction.Left;
            x = x.ClearFlags(Direction.None);
            Assert.That(x, Is.EqualTo(Direction.Top | Direction.Right | Direction.Bottom | Direction.Left));
            x = x.ClearFlags(Direction.Left);
            Assert.That(x, Is.EqualTo(Direction.Top | Direction.Right | Direction.Bottom));
            x = x.ClearFlags((Direction)16);
            Assert.That(x, Is.EqualTo(Direction.Top | Direction.Right | Direction.Bottom));
            x = x | (Direction)16;
            Assert.That(x, Is.EqualTo(Direction.Top | Direction.Right | Direction.Bottom | (Direction)16));
            x = x.ClearFlags((Direction)16);
            Assert.That(x, Is.EqualTo(Direction.Top | Direction.Right | Direction.Bottom));
            x = x.ClearFlags(Direction.Top | Direction.Bottom);
            Assert.That(x, Is.EqualTo(Direction.Right));
        }

        /// <summary>
        /// Tests the <see cref="Bound"/> extension method
        /// </summary>
        [Test]
        public void TestBound()
        {
            Assert.That(0.Bound(0, 0), Is.EqualTo(0));
            Assert.That(3.Bound(1, 5), Is.EqualTo(3));
            Assert.That(3.Bound(3, 5), Is.EqualTo(3));
            Assert.That(3.Bound(1, 3), Is.EqualTo(3));
            Assert.That(3.Bound(3, 3), Is.EqualTo(3));
            Assert.That(3.Bound(1, 2), Is.EqualTo(2));
            Assert.That(3.Bound(4, 5), Is.EqualTo(4));
            Assert.That(0.Bound(4, 5), Is.EqualTo(4));
            Assert.That(int.MaxValue.Bound(4, 5), Is.EqualTo(5));
            Assert.That(int.MinValue.Bound(4, 5), Is.EqualTo(4));
            Assert.That(int.MaxValue.Bound(3, 3), Is.EqualTo(3));
            Assert.That(int.MinValue.Bound(3, 3), Is.EqualTo(3));
            Assert.That(25.Bound(int.MinValue, int.MaxValue), Is.EqualTo(25));
            Assert.That(0.Bound(-5, 5), Is.EqualTo(0));

            // Note that if you call a method on a negative integer literal, the order is
            // such that it will call the method on the positive and then negate the
            // result - eg. '-3.ToString()' will actually call '-(3.ToString())' and thus
            // fail to compile. So we have to enforce the order using brackets. I never
            // really considered this. Incidentally, that precedence works the same in C#
            Assert.That((-5).Bound(-5, 5), Is.EqualTo(-5));
            Assert.That((-5).Bound(3, 5), Is.EqualTo(3));
            Assert.That((-5).Bound(-15, -10), Is.EqualTo(-10));

            // Failure conditions
            var zero = 0;
            Assert.That(() => zero.Bound(5, 4), Throws.TypeOf<InvalidArgumentException>());
            Assert.That(() => zero.Bound(-10, -15), Throws.TypeOf<InvalidArgumentException>());
        }

        /// <summary>
        /// Tests the <see cref="SplitQuoted"/> extension method using simple values with
        /// no elements quoted.
        /// </summary>
        [Test(Description = "Tests the SplitQuoted() extension using unquoted values")]
        public void TestSplitQuoted_Unquoted()
        {
            Assert.That("".SplitQuoted('\\'), Is.EqualTo(new[] { "" }));
            Assert.That(@"\".SplitQuoted('\\'), Is.EqualTo(new[] { "", "" }));
            Assert.That("1,2,3,4".SplitQuoted(','), Is.EqualTo(new[] { "1", "2", "3", "4" }));
            Assert.That("(a,b)".SplitQuoted(','), Is.EqualTo(new[] { "(a", "b)" }));
            Assert.That("1,2,3,".SplitQuoted(','), Is.EqualTo(new[] { "1", "2", "3", "" }));
            Assert.That(",1,2,3,".SplitQuoted(','), Is.EqualTo(new[] { "", "1", "2", "3", "" }));
            Assert.That(",1,,,2,,,,,,,3,,".SplitQuoted(','), Is.EqualTo(new[] { "", "1", "", "", "2", "", "", "", "", "", "", "3", "", "" }));
        }

        /// <summary>
        /// Tests the <see cref="SplitQuoted"/> extension method using simple values with
        /// no elements quoted and with <c>RemoveEmptyEntries</c> set.
        /// </summary>
        [Test(Description = "Tests the SplitQuoted() extension using unquoted values " + "and the RemoveEmptyEntries option")]
        public void TestSplitQuoted_UnquotedRemoveEmpty()
        {
            Assert.That("".SplitQuoted('\\', StringSplitOptions.RemoveEmptyEntries), Is.EqualTo(new string[] { }));
            Assert.That(@"\".SplitQuoted('\\', StringSplitOptions.RemoveEmptyEntries), Is.EqualTo(new string[] { }));
            Assert.That(",1,2,3,".SplitQuoted(',', StringSplitOptions.RemoveEmptyEntries), Is.EqualTo(new[] { "1", "2", "3" }));
            Assert.That(",1,2,3,".SplitQuoted(',', StringSplitOptions.RemoveEmptyEntries), Is.EqualTo(new[] { "1", "2", "3" }));
            Assert.That(",1,2,3,".SplitQuoted(',', StringSplitOptions.RemoveEmptyEntries), Is.EqualTo(new[] { "1", "2", "3" }));
            Assert.That(",1,,,2,,,,,,,3,,".SplitQuoted(',', StringSplitOptions.RemoveEmptyEntries), Is.EqualTo(new[] { "1", "2", "3" }));
        }

        /// <summary>
        /// Tests the <see cref="SplitQuoted"/> extension method using quoted values
        /// which contain the split character.
        /// </summary>
        [Test(Description = "Tests the SplitQuoted() extension using some values containing the split char")]
        public void TestSplitQuoted_SplitCharsInValue()
        {
            Assert.That("\"|\"".SplitQuoted('|'), Is.EqualTo(new[] { "|" }));
            Assert.That("|\"|\"|".SplitQuoted('|'), Is.EqualTo(new[] { "", "|", "" }));
            Assert.That("|\"|||\"|".SplitQuoted('|'), Is.EqualTo(new[] { "", "|||", "" }));
            Assert.That("A|\"P|pe\"|Character".SplitQuoted('|'), Is.EqualTo(new[] { "A", "P|pe", "Character" }));
            Assert.That("When,I,say,\"er,no\",there,is,an,element,\"of,er,\",sarcasm".SplitQuoted(','), Is.EqualTo(new[] { "When", "I", "say", "er,no", "there", "is", "an", "element", "of,er,", "sarcasm" }));
        }

        /// <summary>
        /// Tests the <see cref="SplitQuoted"/> extension method using quoted elements
        /// which contain escaped quote characters.
        /// </summary>
        [Test(Description = "Tests the SplitQuoted() extension using some quoted elements containing quote chars")]
        public void TestSplitQuoted_QuoteHandling()
        {
            Assert.That("\"\"".SplitQuoted('|'), Is.EqualTo(new[] { "" }));
            Assert.That("\"\"".SplitQuoted('|', StringSplitOptions.RemoveEmptyEntries), Is.EqualTo(new string[] { }));
            Assert.That("\"\"||\"\"".SplitQuoted('|'), Is.EqualTo(new[] { "", "", "" }));
            Assert.That("\"\"||\"\"".SplitQuoted('|', StringSplitOptions.RemoveEmptyEntries), Is.EqualTo(new string[] { }));
            Assert.That("I,said,\"Hello, \"\"old\"\" chap\",with,appropriate,emphasis".SplitQuoted(','), Is.EqualTo(new[] { "I", "said", "Hello, \"old\" chap", "with", "appropriate", "emphasis" }));
            Assert.That("|\"\"\"\"|".SplitQuoted('|'), Is.EqualTo(new[] { "", "\"", "" }));
            Assert.That("This \"is\" fine".SplitQuoted('|'), Is.EqualTo(new[] { "This \"is\" fine" }));
            Assert.That("\"This \"\"is\"\" also fine\"".SplitQuoted('|'), Is.EqualTo(new[] { "This \"is\" also fine" }));
            Assert.That("When,I,say,\"er,no\",there,is,an,element,\"of,er,\",sarcasm".SplitQuoted(','), Is.EqualTo(new[] { "When", "I", "say", "er,no", "there", "is", "an", "element", "of,er,", "sarcasm" }));
        }

        [Test(Description = "Tests a few well known SIDs against the IsSid() check - " + "taken from https://support.microsoft.com/en-gb/kb/243330")]
        public void TestIsSid_WellKnownSIDs()
        {
            // All SIDs found on the above mentioned page, omitting those with variables
            // in them (typically 'domain' or 'X'-'Y' for arbitrarily assigned values).
            Assert.That("S-1-0".IsSid(), Is.True); // Null Authority
            Assert.That("S-1-0-0".IsSid(), Is.True); // Nobody
            Assert.That("S-1-1".IsSid(), Is.True); // World Authority
            Assert.That("S-1-1-0".IsSid(), Is.True); // Everyone
            Assert.That("S-1-2".IsSid(), Is.True); // Local Authority
            Assert.That("S-1-2-0".IsSid(), Is.True); // Local
            Assert.That("S-1-2-1".IsSid(), Is.True); // Console Logon
            Assert.That("S-1-3".IsSid(), Is.True); // Creator Authority
            Assert.That("S-1-3-0".IsSid(), Is.True); // Creator Owner
            Assert.That("S-1-3-1".IsSid(), Is.True); // Creator Group
            Assert.That("S-1-3-2".IsSid(), Is.True); // Creator Owner Server
            Assert.That("S-1-3-3".IsSid(), Is.True); // Creator Group Server
            Assert.That("S-1-3-4".IsSid(), Is.True); // Owner Rights
            Assert.That("S-1-5-80-0".IsSid(), Is.True); // All Services
            Assert.That("S-1-4".IsSid(), Is.True); // Non-unique Authority
            Assert.That("S-1-5".IsSid(), Is.True); // NT Authority
            Assert.That("S-1-5-1".IsSid(), Is.True); // Dialup
            Assert.That("S-1-5-2".IsSid(), Is.True); // Network
            Assert.That("S-1-5-3".IsSid(), Is.True); // Batch
            Assert.That("S-1-5-4".IsSid(), Is.True); // Interactive
            Assert.That("S-1-5-6".IsSid(), Is.True); // Service
            Assert.That("S-1-5-7".IsSid(), Is.True); // Anonymous
            Assert.That("S-1-5-8".IsSid(), Is.True); // Proxy
            Assert.That("S-1-5-9".IsSid(), Is.True); // Enterprise Domain Controllers
            Assert.That("S-1-5-10".IsSid(), Is.True);    // Principal Self
            Assert.That("S-1-5-11".IsSid(), Is.True);    // Authenticated Users
            Assert.That("S-1-5-12".IsSid(), Is.True);    // Restricted Code
            Assert.That("S-1-5-13".IsSid(), Is.True);    // Terminal Server Users
            Assert.That("S-1-5-14".IsSid(), Is.True);    // Remote Interactive Logon
            Assert.That("S-1-5-15".IsSid(), Is.True);    // This Organization
            Assert.That("S-1-5-17".IsSid(), Is.True);    // This Organization
            Assert.That("S-1-5-18".IsSid(), Is.True);    // Local System
            Assert.That("S-1-5-19".IsSid(), Is.True);    // NT Authority
            Assert.That("S-1-5-20".IsSid(), Is.True);    // NT Authority
            Assert.That("S-1-5-32-544".IsSid(), Is.True);    // Administrators
            Assert.That("S-1-5-32-545".IsSid(), Is.True);    // Users
            Assert.That("S-1-5-32-546".IsSid(), Is.True);    // Guests
            Assert.That("S-1-5-32-547".IsSid(), Is.True);    // Power Users
            Assert.That("S-1-5-32-548".IsSid(), Is.True);    // Account Operators
            Assert.That("S-1-5-32-549".IsSid(), Is.True);    // Server Operators
            Assert.That("S-1-5-32-550".IsSid(), Is.True);    // Print Operators
            Assert.That("S-1-5-32-551".IsSid(), Is.True);    // Backup Operators
            Assert.That("S-1-5-32-552".IsSid(), Is.True);    // Replicators
            Assert.That("S-1-5-64-10".IsSid(), Is.True); // NTLM Authentication
            Assert.That("S-1-5-64-14".IsSid(), Is.True); // SChannel Authentication
            Assert.That("S-1-5-64-21".IsSid(), Is.True); // Digest Authentication
            Assert.That("S-1-5-80".IsSid(), Is.True);    // NT Service
            Assert.That("S-1-5-80-0".IsSid(), Is.True); // All Services
            Assert.That("S-1-5-83-0".IsSid(), Is.True); // NT VIRTUAL MACHINE\Virtual Machines
            Assert.That("S-1-16-0".IsSid(), Is.True);    // Untrusted Mandatory Level
            Assert.That("S-1-16-4096".IsSid(), Is.True); // Low Mandatory Level
            Assert.That("S-1-16-8192".IsSid(), Is.True); // Medium Mandatory Level
            Assert.That("S-1-16-8448".IsSid(), Is.True); // Medium Plus Mandatory Level
            Assert.That("S-1-16-12288".IsSid(), Is.True);    // High Mandatory Level
            Assert.That("S-1-16-16384".IsSid(), Is.True);    // System Mandatory Level
            Assert.That("S-1-16-20480".IsSid(), Is.True);    // Protected Process Mandatory Level
            Assert.That("S-1-16-28672".IsSid(), Is.True);    // Secure Process Mandatory Level
            Assert.That("S-1-5-32-554".IsSid(), Is.True);    // BUILTIN\Pre-Windows 2000 Compatible Access
            Assert.That("S-1-5-32-555".IsSid(), Is.True);    // BUILTIN\Remote Desktop Users
            Assert.That("S-1-5-32-556".IsSid(), Is.True);    // BUILTIN\Network Configuration Operators
            Assert.That("S-1-5-32-557".IsSid(), Is.True);    // BUILTIN\Incoming Forest Trust Builders
            Assert.That("S-1-5-32-558".IsSid(), Is.True);    // BUILTIN\Performance Monitor Users
            Assert.That("S-1-5-32-559".IsSid(), Is.True);    // BUILTIN\Performance Log Users
            Assert.That("S-1-5-32-560".IsSid(), Is.True);    // BUILTIN\Windows Authorization Access Group
            Assert.That("S-1-5-32-561".IsSid(), Is.True);    // BUILTIN\Terminal Server License Servers
            Assert.That("S-1-5-32-562".IsSid(), Is.True);    // BUILTIN\Distributed COM Users
            Assert.That("S-1-5-32-569".IsSid(), Is.True);    // BUILTIN\Cryptographic Operators
            Assert.That("S-1-5-32-573".IsSid(), Is.True);    // BUILTIN\Event Log Readers
            Assert.That("S-1-5-32-574".IsSid(), Is.True);    // BUILTIN\Certificate Service DCOM Access
            Assert.That("S-1-5-32-575".IsSid(), Is.True);    // BUILTIN\RDS Remote Access Servers
            Assert.That("S-1-5-32-576".IsSid(), Is.True);    // BUILTIN\RDS Endpoint Servers
            Assert.That("S-1-5-32-577".IsSid(), Is.True);    // BUILTIN\RDS Management Servers
            Assert.That("S-1-5-32-578".IsSid(), Is.True);    // BUILTIN\Hyper-V Administrators
            Assert.That("S-1-5-32-579".IsSid(), Is.True);    // BUILTIN\Access Control Assistance Operators
            Assert.That("S-1-5-32-580".IsSid(), Is.True);    // BUILTIN\Remote Management Users
        }

        [Test(Description = "Tests a few things which should not be considered SIDs")]
        public void TestIsSid_NotASid()
        {
            // Null is never a SID
            Assert.That(Extensions.IsSid(null), Is.False);
            string nullStr = null;
            Assert.That(nullStr.IsSid(), Is.False);
            Assert.That("".IsSid(), Is.False);
            Assert.That("1".IsSid(), Is.False);
            Assert.That("S-1".IsSid(), Is.False);
            Assert.That("S-A-1-2".IsSid(), Is.False);
            Assert.That("081 811 8181".IsSid(), Is.False);
            Assert.That("W12 8QT".IsSid(), Is.False);
            Assert.That(Guid.NewGuid().ToString().IsSid(), Is.False);
            Assert.That(DateTime.Now.ToString().IsSid(), Is.False);
            Assert.That(("\t" + null + System.Environment.NewLine).IsSid(), Is.False);
        }

        [Test(Description = "Tests a few standard SIDs from the TrustedDomains env after adding users")]
        public void TestIsSid_NotWellKnown()
        {
            // Retrieved using: "wmic useraccount get name,sid" in DC1 of a TrustedDomains
            // environment after adding a load of users and groups
            Assert.That("S-1-5-21-1153828727-2764981573-1477614438-500".IsSid(), Is.True);
            Assert.That("S-1-5-21-1153828727-2764981573-1477614438-501".IsSid(), Is.True);
            Assert.That("S-1-5-21-1153828727-2764981573-1477614438-502".IsSid(), Is.True);
            Assert.That("S-1-5-21-1153828727-2764981573-1477614438-1000".IsSid(), Is.True);
            Assert.That("S-1-5-21-1153828727-2764981573-1477614438-1104".IsSid(), Is.True);
            Assert.That("S-1-5-21-1153828727-2764981573-1477614438-1115".IsSid(), Is.True);
            Assert.That("S-1-5-21-1153828727-2764981573-1477614438-1116".IsSid(), Is.True);
            Assert.That("S-1-5-21-1153828727-2764981573-1477614438-1117".IsSid(), Is.True);
            Assert.That("S-1-5-21-1153828727-2764981573-1477614438-1118".IsSid(), Is.True);
            Assert.That("S-1-5-21-1153828727-2764981573-1477614438-1119".IsSid(), Is.True);
            Assert.That("S-1-5-21-1153828727-2764981573-1477614438-1120".IsSid(), Is.True);
            Assert.That("S-1-5-21-1153828727-2764981573-1477614438-1121".IsSid(), Is.True);
            Assert.That("S-1-5-21-1153828727-2764981573-1477614438-1122".IsSid(), Is.True);
            Assert.That("S-1-5-21-1153828727-2764981573-1477614438-1123".IsSid(), Is.True);
            Assert.That("S-1-5-21-1153828727-2764981573-1477614438-1124".IsSid(), Is.True);
        }

        [Test]
        public void TestConcreteImplementations()
        {
            var asm = Assembly.GetAssembly(typeof(IDataProvider));
            Assert.That(asm.GetConcreteImplementations<IDataProvider>(), Is.EquivalentTo(new Type[] { typeof(SingletonDataProvider), typeof(DataRowDataProvider), typeof(DataTableDataProvider), typeof(DictionaryDataProvider), typeof(NullDataProvider), typeof(ReaderDataProvider), typeof(ReaderMultiDataProvider), typeof(EmptyDataProvider) }));
            Assert.That(asm.GetConcreteImplementations(typeof(IDiaryEntry)), Is.Empty);
        }

        [Test]
        public void TestConcreteSubclasses()
        {
            var asm = Assembly.GetAssembly(typeof(BaseDataProvider));
            Assert.That(asm.GetConcreteSubclasses<BaseDataProvider>(), Is.EquivalentTo(new Type[] { typeof(SingletonDataProvider), typeof(DataRowDataProvider), typeof(DataTableDataProvider), typeof(DictionaryDataProvider), typeof(ReaderDataProvider), typeof(ReaderMultiDataProvider), typeof(EmptyDataProvider) }));
            // It doesn't work that simply for generic types...
            // See http://stackoverflow.com/q/23324127/430967 for more
            Assert.That(asm.GetConcreteSubclasses(typeof(clsSet<>)), Is.Empty);
        }

        [Test]
        public void TestConcreteDerivedTypes()
        {
            var asm = Assembly.GetAssembly(typeof(IDataProvider));
            Assert.That(asm.GetConcreteDerivedTypes<IDataProvider>(), Is.EquivalentTo(new Type[] { typeof(SingletonDataProvider), typeof(DataRowDataProvider), typeof(DataTableDataProvider), typeof(DictionaryDataProvider), typeof(NullDataProvider), typeof(ReaderDataProvider), typeof(ReaderMultiDataProvider), typeof(EmptyDataProvider) }));
            Assert.That(asm.GetConcreteDerivedTypes<BaseDataProvider>(), Is.EquivalentTo(new Type[] { typeof(SingletonDataProvider), typeof(DataRowDataProvider), typeof(DataTableDataProvider), typeof(DictionaryDataProvider), typeof(ReaderDataProvider), typeof(ReaderMultiDataProvider), typeof(EmptyDataProvider) }));
        }

        // Test Interface used to test the 'GetReferencedTypes' MethodInfo
        // extension method
        protected interface IReferencedTypeTester
        {
            IDataProvider GetProvider();
            void SetProvider(IDataProvider prov);
            void ChangeProvider(ref IDataProvider prof);
            IEnumerable<IDataProvider> EnumerateProviders(ICollection<IDataProvider> prov);
            bool ChangeProviders(ref ICollection<IDataProvider> prov);
            void Indicate(bool left, ref bool right, IEnumerable<IDictionary<Direction, int>> blinks, bool[] enabled, int[][] twice);
            IEnumerable<double> Final(ref ICollection<IDictionary<string[], IEnumerable<int>>>[] mess);
        }

        [Test]
        public void TestGetReferencedTypes()
        {
            var tp = typeof(IReferencedTypeTester);
            var methods = tp.GetMethods().ToDictionary(m => m.Name, m => m);
            Assert.That(methods["GetProvider"].GetReferencedTypes(), Is.EquivalentTo(new Type[] { typeof(IDataProvider) }));
            Assert.That(methods["SetProvider"].GetReferencedTypes(), Is.EquivalentTo(new Type[] { typeof(IDataProvider) }));
            Assert.That(methods["ChangeProvider"].GetReferencedTypes(), Is.EquivalentTo(new Type[] { typeof(IDataProvider) }));
            Assert.That(methods["EnumerateProviders"].GetReferencedTypes(), Is.EquivalentTo(new Type[] { typeof(ICollection<IDataProvider>), typeof(IEnumerable<IDataProvider>), typeof(IDataProvider) }));
            Assert.That(methods["ChangeProviders"].GetReferencedTypes(), Is.EquivalentTo(new Type[] { typeof(ICollection<IDataProvider>), typeof(IDataProvider), typeof(bool) }));
            Assert.That(methods["Indicate"].GetReferencedTypes(), Is.EquivalentTo(new Type[] { typeof(bool), typeof(IEnumerable<IDictionary<Direction, int>>), typeof(IDictionary<Direction, int>), typeof(Direction), typeof(int), typeof(bool[]), typeof(int[][]), typeof(int[]) }));
            Assert.That(methods["Final"].GetReferencedTypes(), Is.EquivalentTo(new Type[] { typeof(ICollection<IDictionary<string[], IEnumerable<int>>>[]), typeof(ICollection<IDictionary<string[], IEnumerable<int>>>), typeof(IDictionary<string[], IEnumerable<int>>), typeof(string[]), typeof(string), typeof(IEnumerable<int>), typeof(int), typeof(IEnumerable<double>), typeof(double) }));
        }

        /// <summary>
        /// Test ASCII character removal extension method
        /// </summary>
        [Test]
        public void TestRemovingNonASCIIChars()
        {
            // Test empty string
            var s = string.Empty;
            Assert.That(s, Is.EqualTo(s.StripNonASCII()));

            // Test all ascii characters
            for (var i = 0; i <= 127; i++)
            {
                s += (char)i;
            }

            Assert.That(s, Is.EqualTo(s.StripNonASCII()));

            // Test normal string
            s = "String with no non-ascii characters";
            Assert.That(s, Is.EqualTo(s.StripNonASCII()));

            // Test non-ascii char
            s = string.Format("String with a non-ascii character here: {0}", '\u0080');
            Assert.That(s, Is.Not.EqualTo(s.StripNonASCII()));

            // Test hidden non-ascii char
            s = "‎f7142efad21f13f235b00a6ce0a15f9c49779c62";
            Assert.That(s, Is.Not.EqualTo(s.StripNonASCII()));
        }

        [Test]
        public void TestDataTableToCsvWithHeaders()
        {
            var dt = new DataTable();
            dt.Columns.Add(new DataColumn("C1"));
            dt.Columns.Add(new DataColumn("C2"));
            dt.Columns.Add(new DataColumn("C3"));
            dt.Rows.Add("R1", "R1", "R1");
            var sb = new StringBuilder();
            sb.AppendLine("C1,C2,C3").AppendLine("R1,R1,R1");
            var csvWithHeaders = sb.ToString();
            Assert.That(dt.DataTableToCsv(true), Is.EqualTo(csvWithHeaders));
        }

        [Test]
        public void TestDataTableToCsvNoHeaders()
        {
            var dt = new DataTable();
            dt.Columns.Add(new DataColumn("C1"));
            dt.Columns.Add(new DataColumn("C2"));
            dt.Columns.Add(new DataColumn("C3"));
            dt.Rows.Add("R1", "R1", "R1");
            var sb = new StringBuilder();
            sb.AppendLine("R1,R1,R1");
            var csvWithoutHeaders = sb.ToString();
            Assert.That(dt.DataTableToCsv(false), Is.EqualTo(csvWithoutHeaders));
        }

        [Test]
        public void TestDataTableToCsvNullField()
        {
            var dt = new DataTable();
            dt.Columns.Add(new DataColumn("C1"));
            dt.Columns.Add(new DataColumn("C2"));
            dt.Columns.Add(new DataColumn("C3"));
            dt.Rows.Add("R2", null, "R2");
            var sb = new StringBuilder();
            sb.AppendLine("R2,,R2");
            var csvWithNullValue = sb.ToString();
            Assert.That(dt.DataTableToCsv(false), Is.EqualTo(csvWithNullValue));
        }

        [Test]
        public void TestStringWithCommaToCsvValue()
        {
            const string testString = "abc,def";
            const string expected = "\"abc,def\"";
            var result = testString.ToEscapedCsvString();
            Assert.AreEqual(result, expected);
        }

        [Test]
        public void TestStringWithNewLineToCsvValue()
        {
            const string testString = @"abc\ndef";
            const string expected = @"""abc\ndef""";
            var result = testString.ToEscapedCsvString();
            Assert.AreEqual(result, expected);
        }

        [Test]
        public void TestStringWithCarriageReturnToCsvValue()
        {
            const string testString = @"abc\rdef";
            const string expected = @"""abc\rdef""";
            var result = testString.ToEscapedCsvString();
            Assert.AreEqual(result, expected);
        }

        [Test]
        public void TestStringWithDoubleQuoteToCsvValue()
        {
            const string testString = "abc\"def";
            const string expected = "\"abc\"\"def\"";
            var result = testString.ToEscapedCsvString();
            Assert.AreEqual(result, expected);
        }

        [Test]
        public void TestDataTableWithCommasToCsv()
        {
            var dt = new DataTable();
            dt.Columns.Add(new DataColumn("C1"));
            dt.Columns.Add(new DataColumn("C2"));
            dt.Columns.Add(new DataColumn("C3"));
            dt.Rows.Add("R,1", "R\"1", "R1");
            var sb = new StringBuilder();
            sb.AppendLine("C1,C2,C3").AppendLine("\"R,1\"" + "," + "\"R\"\"1\"" + "," + "R1");
            var csvWithHeaders = sb.ToString();
            var result = dt.DataTableToCsv(true);
            Assert.That(result, Is.EqualTo(csvWithHeaders));
        }

        [Test]
        public void TestToMidnight([ValueSource(nameof(TestData))] UnitTests.TestData.DateTimeTestData testData)
        {
            Assert.That(testData.StartDate.ToMidnight(), Is.EqualTo((object)testData.ExpectedDate));
        }
    }
}

#endif
