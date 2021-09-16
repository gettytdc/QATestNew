using BluePrism.AutomateAppCore;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.ActiveDirectory
{
    [TestFixture]
    public class ActiveDirectoryTests
    {

        /// <summary>
        /// Tests the validation of a DNS Domain Name, using the
        /// <see cref="clsActiveDirectory.IsValidDnsName"/> method
        /// </summary>
        /// <param name="name">The DNS domain name to test</param>
        /// <returns>true if the name is valid according to the rules required for a DNS
        /// domain name; false otherwise.</returns>
        [Test,
        TestCase(null, ExpectedResult=false),
        TestCase("", ExpectedResult=false),
        TestCase(" ", ExpectedResult=false),
        TestCase(@"\r\n", ExpectedResult=false),
        TestCase(@"\r\n\r\n\r\n ", ExpectedResult=false),
        TestCase("a", ExpectedResult=false),
        TestCase("ab", ExpectedResult=true),
        TestCase("-b", ExpectedResult=false),
        TestCase(".b", ExpectedResult=false),
        TestCase("b-", ExpectedResult=false),
        TestCase("b.", ExpectedResult=true),
        TestCase(".", ExpectedResult=false),
        TestCase("a.b", ExpectedResult=true),
        TestCase("a.123.b", ExpectedResult=true),
        TestCase("this.goes.over.twenty.four.chars.i.think.oh.yes",ExpectedResult=true),
        TestCase("1.b", ExpectedResult=false),
        TestCase("a.1", ExpectedResult=true),
        TestCase("no.white-space.com", ExpectedResult=true),
        TestCase("uh-oh.white space.com", ExpectedResult=false),
        TestCase("dots.must.separate...names", ExpectedResult=false),
        TestCase("and.not.-.ie.dashes", ExpectedResult=false),
        TestCase("but.-anything-.else.is.fine", ExpectedResult=true),
        TestCase("multiple-dashes.are---allowed.ok", ExpectedResult=true),
        TestCase("brackets(no)", ExpectedResult=false),
        TestCase("Question-Marks?Not.on.my.watch", ExpectedResult=false),
        TestCase("AnYVariAtion.Of-cASe.is.FINE", ExpectedResult=true),
        TestCase("trailing.dot.is.fine.", ExpectedResult=true),
        TestCase("trailing.two.dots-not.so.much..", ExpectedResult=false)]

        public bool TestValidDnsName(string name)
        {
            return clsActiveDirectory.IsValidDnsName(name);
        }
    }
}
