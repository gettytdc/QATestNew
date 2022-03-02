using System.DirectoryServices;
using System.Security.Principal;

namespace BluePrism.Core.ActiveDirectory.DirectoryServices
{
    public class SearchResultWrapper : ISearchResult
    {
        private readonly SearchResult _searchResult;

        public SearchResultWrapper(SearchResult searchResult)
        {
            _searchResult = searchResult;
        }

        public string UserPrincipalName
        {
            get
            {
                var upnProperty = _searchResult.Properties[LdapAttributes.UserPrincipalName];
                return (upnProperty.Count > 0) ? (string)upnProperty[0] : string.Empty;
            }
        }

        public string DistinguishedName
        {
            get
            {
                var distinguishedNameProperty = _searchResult.Properties[LdapAttributes.Dn];
                return (distinguishedNameProperty.Count > 0) ? (string)distinguishedNameProperty[0] : string.Empty;
            }
        }

        public string Sid
        {
            get
            {
                var sidProperty = _searchResult.Properties[LdapAttributes.Sid];
                if (sidProperty.Count == 0)
                    return string.Empty;

                var sidBytes = (byte[])(sidProperty[0]);
                return new SecurityIdentifier(sidBytes, 0).ToString();
            }
        }      
    }
}
