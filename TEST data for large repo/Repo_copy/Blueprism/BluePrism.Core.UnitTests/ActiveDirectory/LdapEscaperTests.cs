using BluePrism.Core.ActiveDirectory;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;

namespace BluePrism.Core.UnitTests.ActiveDirectory
{
    [TestFixture]
    public class LdapEscaperTests
    {
        [TestCaseSource(nameof(TestCases), new object[] { true })]
        public void EscapeSearchTerm_ShouldReturnSearchTermWithEscapingApplied(string searchTerm, string escapedSearchTerm)
        {
            var result = LdapEscaper.EscapeSearchTerm(searchTerm);
            result.Should().Be(escapedSearchTerm);
        }

        [TestCaseSource(nameof(TestCases), new object[] { true })]
        public void EscapeSearchTerm_EscapeWildcard_ShouldReturnSearchTermWithEscapingApplied(string searchTerm, string escapedSearchTerm)
        {
            var result = LdapEscaper.EscapeSearchTerm(searchTerm, true);
            result.Should().Be(escapedSearchTerm);
        }

        [TestCaseSource(nameof(TestCases), new object[] { false })]
        public void EscapeSearchTerm_DoNotEscapeWildcard_ShouldReturnSearchTermWithEscapingApplied(string searchTerm, string escapedSearchTerm)
        {
            var result = LdapEscaper.EscapeSearchTerm(searchTerm, false);
            result.Should().Be(escapedSearchTerm);
        }

        private static IEnumerable<string[]> TestCases(bool wildCardShouldBeEscaped)
        {
            yield return new string[] { "", "" };
            yield return new string[] { null, null };
            yield return new string[] { "fish", "fish" };
            yield return new string[] { "\0here", @"\00here" };
            yield return wildCardShouldBeEscaped ? new string[] { "a*", @"a\2a" } : new string[] { "a*", "a*"} ;
            yield return new string[] { ")(somethingElse=What?", @"\29\28somethingElse=What?" };
            yield return new string[] { @"/Wassup?\", @"\2fWassup?\5c" };
        }
    }
}
