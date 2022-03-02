using System.Collections.Generic;
using BluePrism.AutomateAppCore;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.WorkQueues
{
    [TestFixture]
    public class WorkQueueDatabaseTester
    {

        /// <summary>
        /// Tests that the <see cref="clsServer.ApplyWildcard"/> method works as
        /// expected with some hopefully over-complex values.
        /// </summary>
        [Test]
        public void TestItemTagWildcards()
        {
            var dict = new Dictionary<string, string>();
            dict[""] = "";
            dict["Here, there be no wildcards"] = "Here, there be no wildcards";
            dict["50% of one, half of the other"] = "50[%] of one, half of the other";
            dict["*"] = "%";
            dict["*%"] = "%[%]";
            dict["**%"] = "*[%]";
            dict["***%"] = "*%[%]";
            dict["****%"] = "**[%]";
            dict["The ********ing test isn't working"] = "The ****ing test isn't working";
            dict["%*%**%*%"] = "[%]%[%]*[%]%[%]"; // If anyone actually uses one of these, I may hit them
            dict["[*%]"] = "[[]%[%]]";
            dict["[[["] = "[[][[][[]";
            dict["[_[["] = "[[][_][[][[]";
            dict["[?[["] = "[[]_[[][[]";
            dict["**%***_*?****?_[?]*"] = "*[%]*%[_]%_**_[_][[]_]%";
            dict["**%***_*?****?_[??]*"] = "*[%]*%[_]%_**_[_][[]__]%";
            foreach (var input in dict.Keys)
                Assert.That(clsServer.ApplyWildcard(input), Is.EqualTo(dict[input]));
        }
    }
}
