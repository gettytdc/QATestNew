using BluePrism.Core.ActiveDirectory.UserQuery;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;

namespace BluePrism.Core.UnitTests
{
    [TestFixture]
    public class UserFilterTests
    {
        [Test]
        public void LdapFilter_FilterTypeIsNone_ShouldJustReturnUserCategoryFilter()
        {
            var userFilter = new UserFilter(UserFilterType.None, "John");
            userFilter.LdapFilter.Should().Be(@"(objectCategory=user)");
        }

        [TestCaseSource(nameof(NullEmptyAndWhiteSpaceStrings))]
        public void LdapFilter_NullEmptyOrWhiteSpaceFilterValue_ShouldJustReturnUserCategoryFilter(string filterValue)
        {
            var userFilter = new UserFilter(UserFilterType.Cn, filterValue);
            userFilter.LdapFilter.Should().Be(@"(objectCategory=user)");
        }

        [Test]
        public void LdapFilter_AttributeAndFilter_ShouldReturnExpectedFilter()
        {
            var userFilter = new UserFilter(UserFilterType.Cn, "John");
            userFilter.LdapFilter.Should().Be(@"(&(objectCategory=user)(cn=John))");
        }

        [Test]
        public void LdapFilter_AttributeAndWildcardFilter_ShouldReturnExpectedFilterWithWildcardNotEscaped()
        {
            var userFilter = new UserFilter(UserFilterType.Cn, "John");
            userFilter.LdapFilter.Should().Be(@"(&(objectCategory=user)(cn=John))");
        }

        [Test]
        public void LdapFilter_AttributeAndFilterThatRequiresEscaping_ShouldReturnExpectedEscapedFilter()
        {
            var userFilter = new UserFilter(UserFilterType.Cn, "EscapeMe\0");
            userFilter.LdapFilter.Should().Be(@"(&(objectCategory=user)(cn=EscapeMe\00))");            
        }
        
        private static IEnumerable<string> NullEmptyAndWhiteSpaceStrings()
        {
            yield return string.Empty;
            yield return null;
            yield return " ";
        }

    }
}
