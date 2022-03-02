#if UNITTESTS

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using BluePrism.BPCoreLib.Collections;
using BluePrism.Server.Domain.Models;
using NUnit.Framework;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace BluePrism.BPCoreLib.UnitTests
{
    /// <summary>
    /// Test fixture for BP Util
    /// </summary>
    [TestFixture]
    public class TestBPUtil
    {

        /// <summary>
        /// Class to test the auto-parsing of strings into classes with a Parse method.
        /// </summary>
        protected class Testee
        {
            private readonly string value;

            public Testee(string val) => value = val;

            public static Testee Parse(string input) => new Testee("<" + input + ">");

            public override string ToString() => value;
        }

        /// <summary>
        /// Enumeration used in the GetAttributeValue tests
        /// </summary>
        private enum TestE
        {
            [Description("NONE")] [Localizable(false)]
            Zero,
            [Description("1")] [ReadOnly(true)] One,

            [Description("II")] [ReadOnly(false)] [Localizable(true)]
            Two
        }

        /// <summary>
        /// Converts the given command line to a pointer to pointer of wide strings, as
        /// defined in the CommandLineToArgvW function in shell32.dll
        /// </summary>
        /// <param name="lpCmdLine">The command line to split into separate args</param>
        /// <param name="pNumArgs">On output, the number of args created from the given
        /// command line.</param>
        /// <returns>A pointer to the pointers of strings representing the command line
        /// args.</returns>
        [DllImport("shell32.dll", SetLastError = true)]
        private static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine,
            ref int pNumArgs);

        /// <summary>
        /// Converts the given command line to separate arguments, using the
        /// <see cref="CommandLineToArgvW"/> function in shell32.dll.
        /// </summary>
        /// <param name="cmdLine">The command line to split into arguments</param>
        /// <returns>The array of arguments generated from splitting
        /// <paramref name="cmdLine"/> into separate arguments.</returns>
        private string[] CommandLineToArgs(string cmdLine)
        {
            var argc = default(int);
            var argv = CommandLineToArgvW(cmdLine, ref argc);
            if (argv == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            try
            {
                var args = new string[argc];
                for (int i = 0, loopTo = args.Length - 1; i <= loopTo; i++)
                {
                    args[i] = Marshal.PtrToStringUni(Marshal.ReadIntPtr(argv, i * IntPtr.Size));
                }

                return args;
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            }
        }

        [Test]
        public void TestGetAttributeValue()
        {
            Assert.That(BPUtil.GetAttributeValue<LocalizableAttribute>(TestE.Zero).IsLocalizable, Is.False);
            Assert.That(BPUtil.GetAttributeValue<DescriptionAttribute>(TestE.Zero).Description, Is.EqualTo("NONE"));
            Assert.That(BPUtil.GetAttributeValue<ReadOnlyAttribute>(TestE.Zero), Is.Null);
            Assert.That(BPUtil.GetAttributeValue<LocalizableAttribute>(TestE.One), Is.Null);
            Assert.That(BPUtil.GetAttributeValue<DescriptionAttribute>(TestE.One).Description, Is.EqualTo("1"));
            Assert.That(BPUtil.GetAttributeValue<ReadOnlyAttribute>(TestE.One).IsReadOnly, Is.True);
            Assert.That(BPUtil.GetAttributeValue<LocalizableAttribute>(TestE.Two).IsLocalizable, Is.True);
            Assert.That(BPUtil.GetAttributeValue<DescriptionAttribute>(TestE.Two).Description, Is.EqualTo("II"));
            Assert.That(BPUtil.GetAttributeValue<ReadOnlyAttribute>(TestE.One).IsReadOnly, Is.True);
        }

        /// <summary>
        /// Test the BPUtil.IfNull() methods
        /// </summary>
        [Test]
        public void IfNullTests()
        {

            // May seem silly, but make sure that a default value of null is allowed
            Assert.IsNull(BPUtil.IfNull<string>(DBNull.Value, null));
            Assert.IsNull(BPUtil.IfNull<string>(null, null));

            // And that using 'Nothing' with value types does the right thing
            Assert.That(BPUtil.IfNull(DBNull.Value, default(int)), Is.EqualTo(0));
            Assert.That(BPUtil.IfNull(null, default(TimeSpan)), Is.EqualTo(TimeSpan.Zero));
            Assert.That(BPUtil.IfNull<Guid>(null, default), Is.EqualTo(Guid.Empty));

            // Let's test some GUIDs
            var g = Guid.NewGuid();

            // Test null
            Assert.That(BPUtil.IfNull(null, g), Is.EqualTo(g));

            // Test non-null
            Assert.That(BPUtil.IfNull(g, Guid.Empty), Is.EqualTo(g));

            // Test DBNull
            Assert.That(BPUtil.IfNull(DBNull.Value, g), Is.EqualTo(g));
            object obj = g.ToString();

            // Check conversion
            Assert.That(BPUtil.IfNull(obj, Guid.Empty), Is.EqualTo(g));

            // Try something else... oooh, Ints
            obj = 33;
            Assert.That(BPUtil.IfNull(null, 27), Is.EqualTo(27));
            Assert.That(BPUtil.IfNull(obj, 27), Is.EqualTo(33));
            Assert.That(BPUtil.IfNull(DBNull.Value, 27), Is.EqualTo(27));

            // Something with a (very ropey) Parse method
            obj = "test";
            Assert.That(BPUtil.IfNull(null, new Testee("nada")).ToString(), Is.EqualTo("nada"));
            Assert.That(BPUtil.IfNull(DBNull.Value, new Testee("nada")).ToString(), Is.EqualTo("nada"));
            Assert.That(BPUtil.IfNull(obj, new Testee("nada")).ToString(), Is.EqualTo("<test>"));
            // Test that a nonsense object gives a nonsense result.
            try
            {
                BPUtil.IfNull(74, new Testee("wassup?"));
                Assert.Fail("BPUtil.IfNull(74, New Testee(\"wassup?\")) worked - it really shouldn't");
            }
            catch (InvalidCastException)
            {
                // Goody goody gumdrops
            }

            // Finally, dates.
            obj = new DateTime(2010, 2, 16, 10, 0, 0);
            var unspecDate = new DateTime(2010, 2, 16, 10, 0, 0);
            var utcDate = new DateTime(2010, 2, 16, 10, 0, 0, DateTimeKind.Utc);
            Assert.That(BPUtil.IfNull(null, DateTime.MinValue), Is.EqualTo(DateTime.MinValue));
            Assert.That(BPUtil.IfNull(DBNull.Value, DateTime.MaxValue), Is.EqualTo(DateTime.MaxValue));
            Assert.That(BPUtil.IfNull(obj, DateTime.MinValue), Is.EqualTo(unspecDate));
            Assert.That(BPUtil.IfNull("2010-02-16 10:00:00Z", DateTime.MinValue), Is.EqualTo(unspecDate.ToLocalTime()));

            // Some date-y round trips.
            var strUnspec = unspecDate.ToString("u");
            Assert.That(BPUtil.IfNull(strUnspec, DateTime.MinValue), Is.EqualTo(unspecDate.ToLocalTime()));
            var strUtc = utcDate.ToString("u");
            Assert.That(BPUtil.IfNull(strUtc, DateTime.MinValue), Is.EqualTo(utcDate.ToLocalTime()));
        }

        [Test]
        public void TestHasSingleBitSet()
        {
            Assert.That(BPUtil.HasSingleBitSet(0L), Is.False);
            Assert.That(BPUtil.HasSingleBitSet(-1), Is.False);
            Assert.That(BPUtil.HasSingleBitSet(1L), Is.True);
            Assert.That(BPUtil.HasSingleBitSet(2L), Is.True);
            Assert.That(BPUtil.HasSingleBitSet(4L), Is.True);
            Assert.That(BPUtil.HasSingleBitSet(8L), Is.True);
            Assert.That(BPUtil.HasSingleBitSet(16L), Is.True);
            Assert.That(BPUtil.HasSingleBitSet(128L), Is.True);
            Assert.That(BPUtil.HasSingleBitSet((long)Math.Pow(2d, 31d)), Is.True);
            Assert.That(BPUtil.HasSingleBitSet(3L), Is.False);
            Assert.That(BPUtil.HasSingleBitSet(5L), Is.False);
            Assert.That(BPUtil.HasSingleBitSet(int.MaxValue), Is.False);
            Assert.That(BPUtil.HasSingleBitSet(long.MaxValue), Is.False);
            Assert.That(BPUtil.HasSingleBitSet(long.MinValue), Is.True);
        }

        [Test]
        public void TestHasMultipleBitsSet()
        {
            Assert.That(BPUtil.HasMultipleBitsSet(0L), Is.False);
            Assert.That(BPUtil.HasMultipleBitsSet(-1), Is.True);
            Assert.That(BPUtil.HasMultipleBitsSet(1L), Is.False);
            Assert.That(BPUtil.HasMultipleBitsSet(2L), Is.False);
            Assert.That(BPUtil.HasMultipleBitsSet(4L), Is.False);
            Assert.That(BPUtil.HasMultipleBitsSet(8L), Is.False);
            Assert.That(BPUtil.HasMultipleBitsSet(16L), Is.False);
            Assert.That(BPUtil.HasMultipleBitsSet(128L), Is.False);
            Assert.That(BPUtil.HasMultipleBitsSet((long)Math.Pow(2d, 31d)), Is.False);
            Assert.That(BPUtil.HasMultipleBitsSet(3L), Is.True);
            Assert.That(BPUtil.HasMultipleBitsSet(5L), Is.True);
            Assert.That(BPUtil.HasMultipleBitsSet(int.MaxValue), Is.True);
            Assert.That(BPUtil.HasMultipleBitsSet(long.MaxValue), Is.True);
            Assert.That(BPUtil.HasMultipleBitsSet(long.MinValue), Is.False);
        }

        [Test]
        public void TestSanitizeXmlValue()
        {
            Assert.That(BPUtil.SanitizeXmlString(null), Is.Null);
            Assert.That(BPUtil.SanitizeXmlString(""), Is.EqualTo(""));
            Assert.That(BPUtil.SanitizeXmlString("12345"), Is.EqualTo("12345"));
            Assert.That(BPUtil.SanitizeXmlString("A valid XML string"), Is.EqualTo("A valid XML string"));
            Assert.That(BPUtil.SanitizeXmlString("Line 1" + Environment.NewLine + "Line 2"),
                Is.EqualTo("Line 1" + Environment.NewLine + "Line 2"));
            Assert.That(BPUtil.SanitizeXmlString("Back to top" + "\f"), Is.EqualTo("Back to top"));
            Assert.That(BPUtil.SanitizeXmlString("Normal Data" + '\0' + "_Nefarious Buffer Overflow Actions"),
                Is.EqualTo("Normal Data_Nefarious Buffer Overflow Actions"));
        }

        [Test]
        public void TestReplaceAny_Chars()
        {
            Assert.That(BPUtil.ReplaceAny("abcdefg", GetEmpty.IDictionary<char, char>()), Is.EqualTo("abcdefg"));
            Assert.That(BPUtil.ReplaceAny("abcdefg", GetSingleton.IDictionary('a', '1')), Is.EqualTo("1bcdefg"));
            Assert.That(BPUtil.ReplaceAny("", GetSingleton.IDictionary('a', '1')), Is.EqualTo(""));
            Assert.That(BPUtil.ReplaceAny(null, GetSingleton.IDictionary('a', '1')), Is.Null);
            Assert.That(BPUtil.ReplaceAny("a", GetSingleton.IDictionary('a', '1')), Is.EqualTo("1"));
            Assert.That(BPUtil.ReplaceAny("b", GetSingleton.IDictionary('a', '1')), Is.EqualTo("b"));

            // No replacements should return the same (by reference) object
            const string str = "test";
            Assert.That(BPUtil.ReplaceAny(str, GetSingleton.IDictionary('a', '1')), Is.EqualTo(str));
            Assert.That(BPUtil.ReplaceAny(str, GetSingleton.IDictionary('a', '1')), Is.SameAs(str));
            Assert.That(BPUtil.ReplaceAny(str, GetEmpty.IDictionary<char, char>()), Is.SameAs(str));

            // Even if it there are matches in the dictionary - if the string is not
            // changed as a result, this should return the same instance
            Assert.That(BPUtil.ReplaceAny(str, GetSingleton.IDictionary('t', 't')), Is.EqualTo(str));
            Assert.That(BPUtil.ReplaceAny(str, GetSingleton.IDictionary('t', 't')), Is.SameAs(str));
            var map = new clsOrderedDictionary<char, char>
            {
                ['a'] = '1',
                ['b'] = '2',
                ['c'] = '3',
                ['d'] = '4',
                ['e'] = '5',
                ['f'] = '6'
            };
            Assert.That(BPUtil.ReplaceAny("abcdef", map), Is.EqualTo("123456"));
            Assert.That(BPUtil.ReplaceAny("fedcba", map), Is.EqualTo("654321"));
            Assert.That(BPUtil.ReplaceAny("cake", map), Is.EqualTo("31k5"));
        }

        [Test]
        public void TestReplaceAny_Strings()
        {
            Assert.That(BPUtil.ReplaceAny("abcdefg", GetEmpty.IDictionary<string, string>()), Is.EqualTo("abcdefg"));
            Assert.That(BPUtil.ReplaceAny("abcdefg", GetSingleton.IDictionary("a", "1")), Is.EqualTo("1bcdefg"));
            Assert.That(BPUtil.ReplaceAny("", GetSingleton.IDictionary("a", "1")), Is.EqualTo(""));
            Assert.That(BPUtil.ReplaceAny(null, GetSingleton.IDictionary("a", "1")), Is.Null);
            Assert.That(BPUtil.ReplaceAny("a", GetSingleton.IDictionary("a", "1")), Is.EqualTo("1"));
            Assert.That(BPUtil.ReplaceAny("b", GetSingleton.IDictionary("a", "1")), Is.EqualTo("b"));

            // No replacements should return the same (by reference) object
            const string str = "test";
            Assert.That(BPUtil.ReplaceAny(str, GetSingleton.IDictionary("a", "1")), Is.EqualTo(str));
            Assert.That(BPUtil.ReplaceAny(str, GetSingleton.IDictionary("a", "1")), Is.SameAs(str));
            Assert.That(BPUtil.ReplaceAny(str, GetEmpty.IDictionary<string, string>()), Is.SameAs(str));

            // Even if it there are matches in the dictionary - if the string is not
            // changed as a result, this should return the same instance
            Assert.That(BPUtil.ReplaceAny(str, GetSingleton.IDictionary("t", "t")), Is.EqualTo(str));
            Assert.That(BPUtil.ReplaceAny(str, GetSingleton.IDictionary("t", "t")), Is.SameAs(str));
            var map = new clsOrderedDictionary<string, string>
            {
                ["a"] = "1",
                ["b"] = "2",
                ["c"] = "3",
                ["d"] = "4",
                ["e"] = "5",
                ["f"] = "6"
            };
            Assert.That(BPUtil.ReplaceAny("abcdef", map), Is.EqualTo("123456"));
            Assert.That(BPUtil.ReplaceAny("fedcba", map), Is.EqualTo("654321"));
            Assert.That(BPUtil.ReplaceAny("cake", map), Is.EqualTo("31k5"));

            // Test the cases outlined in the method documentation in BPUtil
            map.Clear();
            map["te"] = "et";
            map["test"] = "sett";
            map["teststr"] = "";
            Assert.That(BPUtil.ReplaceAny("teststring", map), Is.EqualTo("etststring"));
            map.Clear();
            map["test"] = "sett";
            map["te"] = "et";
            map["teststr"] = "";
            Assert.That(BPUtil.ReplaceAny("teststring", map), Is.EqualTo("settstring"));
            map.Clear();
            map["aa"] = "bb";
            map["b"] = "cc";
            map["a"] = "d";
            Assert.That(BPUtil.ReplaceAny("aaaaa", map), Is.EqualTo("bbbbd"));
        }

        [Test]
        public void TestReplaceAny_StringParams()
        {
            Assert.That(BPUtil.ReplaceAny("abcdefg", GetEmpty.IDictionary<string, string>()), Is.EqualTo("abcdefg"));
            Assert.That(BPUtil.ReplaceAny("abcdefg", "a", "1"), Is.EqualTo("1bcdefg"));
            Assert.That(BPUtil.ReplaceAny("", "a", "1"), Is.EqualTo(""));
            Assert.That(BPUtil.ReplaceAny(null, "a", "1"), Is.Null);
            Assert.That(BPUtil.ReplaceAny("a", "a", "1"), Is.EqualTo("1"));
            Assert.That(BPUtil.ReplaceAny("b", "a", "1"), Is.EqualTo("b"));

            // No replacements should return the same (by reference) object
            const string str = "test";
            Assert.That(BPUtil.ReplaceAny(str, "a", "1"), Is.EqualTo(str));
            Assert.That(BPUtil.ReplaceAny(str, "a", "1"), Is.SameAs(str));
            Assert.That(BPUtil.ReplaceAny(str), Is.SameAs(str));

            // Even if it there are matches in the dictionary - if the string is not
            // changed as a result, this should return the same instance
            Assert.That(BPUtil.ReplaceAny(str, "t", "t"), Is.EqualTo(str));
            Assert.That(BPUtil.ReplaceAny(str, "t", "t"), Is.SameAs(str));
            Assert.That(BPUtil.ReplaceAny("abcdef", "a", "1", "b", "2", "c", "3", "d", "4", "e", "5", "f", "6"),
                Is.EqualTo("123456"));
            Assert.That(BPUtil.ReplaceAny("fedcba", "a", "1", "b", "2", "c", "3", "d", "4", "e", "5", "f", "6"),
                Is.EqualTo("654321"));
            Assert.That(BPUtil.ReplaceAny("cake", "a", "1", "b", "2", "c", "3", "d", "4", "e", "5", "f", "6"),
                Is.EqualTo("31k5"));


            // Test the cases outlined in the method documentation in BPUtil
            Assert.That(BPUtil.ReplaceAny("teststring", "te", "et", "test", "sett", "teststr", ""),
                Is.EqualTo("etststring"));
            Assert.That(BPUtil.ReplaceAny("teststring", "test", "sett", "te", "et", "teststr", ""),
                Is.EqualTo("settstring"));
            Assert.That(BPUtil.ReplaceAny("aaaaa", "aa", "bb", "b", "cc", "a", "d"), Is.EqualTo("bbbbd"));

            // A couple of tag mask replacement calls (about to be used in tags in
            // clsServerWorkQueues, so it has to work correctly)
            Assert.That(
                BPUtil.ReplaceAny("my **tag***", "[", "[[]", "%", "[%]", "_", "[_]", "**", "*", "??", "?", "*", "%",
                    "?", "_"), Is.EqualTo("my *tag*%"));
            Assert.That(
                BPUtil.ReplaceAny("[*00% record] - 5**", "[", "[[]", "%", "[%]", "_", "[_]", "**", "*", "??", "?", "*",
                    "%", "?", "_"), Is.EqualTo("[[]%00[%] record] - 5*"));
            Assert.That(
                BPUtil.ReplaceAny("He punched you in the ****??", "[", "[[]", "%", "[%]", "_", "[_]", "**", "*", "??",
                    "?", "*", "%", "?", "_"), Is.EqualTo("He punched you in the **?"));
        }

        // One different thing to test, ensure that passing invalid 'pairs' throws
        // the appropriate exception
        [Test]
        public void TestReplaceAny_StringParamsException() => Assert.That(() => BPUtil.ReplaceAny("aa", "a"),
            Throws.InstanceOf<InvalidArgumentException>());

        [Test]
        public void TestFindUnique()
        {
            var coll = new List<string>(new[] {"Disc 1", "Disc 2", "Disc 4"});
            Assert.That(BPUtil.FindUnique("Disc {0}", coll.Contains), Is.EqualTo("Disc 3"));
            coll.Add(BPUtil.FindUnique("Disc {0}", x => coll.Contains(x)));
            Assert.That(coll, Is.EqualTo(new[] {"Disc 1", "Disc 2", "Disc 4", "Disc 3"}));
            Assert.That(BPUtil.FindUnique("Disc {0}", coll.Contains), Is.EqualTo("Disc 5"));
            coll.Clear();
            Assert.That(BPUtil.FindUnique("Disc {0}", x => coll.Contains(x)), Is.EqualTo("Disc 1"));
            for (var i = 1; i <= 208; i++)
            {
                coll.Add("ED-" + i);
            }

            Assert.That(BPUtil.FindUnique("ED-{0}", coll.Contains), Is.EqualTo("ED-209"));
            Assert.That(BPUtil.FindUnique("{1}-{0}", x => x != "Blink-182", "Blink"), Is.EqualTo("Blink-182"));
        }

        /// <summary>
        /// Tests the <see cref="BPUtil.ConvertAndFormatUtcDateTime"/> method.
        /// </summary>
        [Test]
        public void TestConvertDisplayUtcDateTimes_BoundaryDates()
        {
            Assert.That(BPUtil.ConvertAndFormatUtcDateTime(DateTime.MinValue), Is.EqualTo(""));
            Assert.That(BPUtil.ConvertAndFormatUtcDateTime(DateTime.MaxValue), Is.EqualTo(""));
            Assert.That(BPUtil.ConvertAndFormatUtcDateTime(BPUtil.DateMaxValueUtc), Is.EqualTo(""));
            Assert.That(BPUtil.ConvertAndFormatUtcDateTime(BPUtil.DateMinValueUtc), Is.EqualTo(""));

            // Round trip the date through the string; re-parse it back and compare to
            // the original date (truncating any millis on the date - the display form
            // of the datetime does not display millis, so it will lose them in the
            // round trip
            var dt = DateTime.UtcNow;
            var dtDisp = BPUtil.ConvertAndFormatUtcDateTime(dt);
            var dtReparsed = DateTime.Parse(dtDisp);
            var dtUtcAgain = dtReparsed.ToUniversalTime();
            Assert.That(dtUtcAgain, Is.EqualTo(dt.AddTicks(-dt.Ticks % TimeSpan.TicksPerSecond)));
        }

        /// <summary>
        /// Tests that a UTC date/time round-trips correctly through the
        /// <see cref="BPUtil.ConvertAndFormatUtcDateTime"/> method.
        /// </summary>
        [Test]
        public void TestConvertDisplayUtcDateTimes_UTCRoundTrip()
        {
            // Round trip the date through the string; re-parse it back, convert it back
            // to UTC and compare to the original date (truncating any millis on the
            // date - the display form of the datetime does not display millis, so it will
            // lose them in the round trip
            var dt = DateTime.UtcNow;
            var dtDisp = BPUtil.ConvertAndFormatUtcDateTime(dt);
            var dtReparsed = DateTime.Parse(dtDisp);
            var dtUtcAgain = dtReparsed.ToUniversalTime();
            Assert.That(dtUtcAgain, Is.EqualTo(dt.AddTicks(-dt.Ticks % TimeSpan.TicksPerSecond)));
        }

        /// <summary>
        /// Tests that a local date/time round-trips correctly through the
        /// <see cref="BPUtil.ConvertAndFormatUtcDateTime"/> method.
        /// </summary>
        [Test]
        public void TestConvertDisplayUtcDateTimes_LocalRoundTrip()
        {
            // Round trip the date through the string; re-parse it back and compare to
            // the original date (truncating any millis on the date - the display form
            // of the datetime does not display millis, so it will lose them in the
            // round trip
            var dt = DateTime.Now;
            var dtDisp = BPUtil.ConvertAndFormatUtcDateTime(dt);
            var dtReparsed = DateTime.Parse(dtDisp);
            Assert.That(dtReparsed, Is.EqualTo(dt.AddTicks(-dt.Ticks % TimeSpan.TicksPerSecond)));
        }

        [Test]
        public void TestBuildArgString_Empty()
        {
            Assert.That(BPUtil.BuildCommandLineArgString(), Is.EqualTo(""));
            Assert.That(BPUtil.BuildCommandLineArgString(), Is.EqualTo(""));
            Assert.That(BPUtil.BuildCommandLineArgString(new object[] {null}), Is.EqualTo(""));
            Assert.That(BPUtil.BuildCommandLineArgString(""), Is.EqualTo(""));
            Assert.That(BPUtil.BuildCommandLineArgString(null, "", null, null, ""), Is.EqualTo(""));
        }

        [Test]
        public void TestBuildArgString_Basic()
        {
            Assert.That(BPUtil.BuildCommandLineArgString("1"), Is.EqualTo("1"));
            Assert.That(BPUtil.BuildCommandLineArgString("--verbose"), Is.EqualTo("--verbose"));
            Assert.That(BPUtil.BuildCommandLineArgString(@"C:\temp\output.csv"), Is.EqualTo(@"C:\temp\output.csv"));
            Assert.That(BPUtil.BuildCommandLineArgString("push", "origin/master"), Is.EqualTo("push origin/master"));
        }

        [Test]
        public void TestBuildArgString_Spaces()
        {
            Assert.That(BPUtil.BuildCommandLineArgString(@"C:\Program Files (x86)", @"C:\Program Files"),
                Is.EqualTo(@"""C:\Program Files (x86)"" ""C:\Program Files"""));
            Assert.That(BPUtil.BuildCommandLineArgString(@"C:\Program Files (x86)\", @"C:\Program Files\"),
                Is.EqualTo(@"""C:\Program Files (x86)\\"" ""C:\Program Files\\"""));
        }

        [Test]
        public void TestBuildArgString_Quotes() => Assert.That(
            BPUtil.BuildCommandLineArgString("Frank \"Ol' Blue Eyes\" Sinatra"),
            Is.EqualTo(@"""Frank \""Ol' Blue Eyes\"" Sinatra"""));

        /// <summary>
        /// Builds the test cases for the <see cref="TestBuildArgString_RoundTrip"/>
        /// test.
        /// </summary>
        /// <returns>An array of string arrays containing input for the build arg string
        /// round trip tests</returns>
        protected static RoundTripTestCase[] GetBuildArgString_RoundTripTestCases() => new[]
        {
            new RoundTripTestCase("-l"), new RoundTripTestCase("<html><head><title>Hi</title></head></html>"),
            new RoundTripTestCase(@"This quote \"" stands alone"),
            new RoundTripTestCase(@"C:\Program Files (x86)\Blue Prism Limited"),
            new RoundTripTestCase(@"C:\Program Files (x86)\Blue Prism Limited\"),
            new RoundTripTestCase("Frank \"Ol' Blue Eyes\" Sinatra"),
            new RoundTripTestCase("William", "\"Billy The Kid\"", "Bonney"),
            new RoundTripTestCase("One", "in a", "million")
        };

        public class RoundTripTestCase
        {
            public RoundTripTestCase(params object[] args) => Args = args;

            public object[] Args { get; set; }

            public override string ToString() => string.Join(", ", Args.Select(x => $"\"{x}\""));
        }

        [Test]
        [TestCaseSource("GetBuildArgString_RoundTripTestCases")]
        public void TestBuildArgString_RoundTrip(RoundTripTestCase testCase)
        {
            var args = testCase.Args;
            var argString = BPUtil.BuildCommandLineArgString(args);
            // Prepend a space, and skip the first returned element.
            // The 'CommandLine' that this converts from includes the program (I think -
            // I couldn't actually verify that, but that was the behaviour I appeared to
            // be seeing).
            var roundTripped = CommandLineToArgs(" " + argString).Skip(1).ToArray();
            Assert.That(roundTripped, Is.EqualTo(args), $"Test:{{{string.Join("|", testCase)}}}");
        }
    }
}

#endif
