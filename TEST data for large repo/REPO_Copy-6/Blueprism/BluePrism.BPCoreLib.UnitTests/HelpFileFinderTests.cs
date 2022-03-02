#if UNITTESTS

using System.Globalization;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace BluePrism.BPCoreLib.UnitTests
{

    public class HelpFileFinderTests
    {
        [Test]
        public void GetPossiblePaths_WithManyCultures_ShouldIncludePossiblePathsBaseOnCultureOrder()
        {
            var cultures = new[] {new CultureInfo("zh-CN"), new CultureInfo("zh-Hans")};
            var paths = HelpFileFinder.GetPossiblePaths(cultures, @"C:\Dev\BluePrism").ToList();
            paths.Should().Equal(@"C:\Dev\BluePrism\..\BluePrism.Automate\Help\l10n\zh-CN\AutomateHelp.chm",
                @"C:\Dev\BluePrism\..\BluePrism.Automate\Help\l10n\zh-Hans\AutomateHelp.chm",
                @"C:\Dev\BluePrism\AutomateHelp_zh-CN.chm", @"C:\Dev\BluePrism\AutomateHelp_zh-Hans.chm",
                @"C:\Dev\BluePrism\..\BluePrism.Automate\Help\AutomateHelp.chm", @"C:\Dev\BluePrism\AutomateHelp.chm");
        }
    }
}

#endif
