#if UNITTESTS
using System.Linq;
using System.Collections.Generic;
using BluePrism.Core.Utility;
using NUnit.Framework;
using System.Xml.Linq;

namespace BluePrism.Core.UnitTests.Utility
{
    [TestFixture]
    public class XCDataExtensionMethodsTests
    {
        [Test, TestCaseSource("CDataTestCases")]
        public void XCData_RoundTrip_ShouldRetainValueAfterEscaping(string text)
        {
            var cData = new XCData(text);
            var roundtrip = cData.ToEscapedEnumerable().GetConcatenatedValue();

            Assert.That(roundtrip.Equals(text));
        }

        [Test, TestCaseSource("CDataTestCases")]
        public void XCData_ToEscapedEnumerables_ShouldNotContainTheCDataEscapeCharacter(string text)
        {
            var cData = new XCData(text);
            Assert.False(cData.ToEscapedEnumerable().Any(x => x.Value.Contains("]]>")));
        }

        /// <summary>
        /// Gets the values of data that may be stored in XCData nodes i.e. CData Nodes
        /// </summary>
        /// <returns></returns>
        protected static IEnumerable<string> CDataTestCases()
        {
            yield return "contains no escaped chars";
            yield return " ";
            yield return "";
            yield return "]]>";
            yield return "]]> ]]>";
            yield return "text and stuff ]]> and more stuff";
            yield return "]";
            yield return "]>";
            yield return "]]";
        }
    }
}
#endif