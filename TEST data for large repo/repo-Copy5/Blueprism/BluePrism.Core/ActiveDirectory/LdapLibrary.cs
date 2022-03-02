
using System.DirectoryServices;

namespace BluePrism.Core.ActiveDirectory
{
    public class LdapLibrary
    {
        public static string GetDistinguishedNameOfCurrentForestRootDomain()
        {
            using (var rootDirectoryEntry = new DirectoryEntry("LDAP://RootDSE"))
            {
                var rootDomainDistinguishedName = rootDirectoryEntry.Properties["rootDomainNamingContext"].Value;
                return (string) rootDomainDistinguishedName;
            };            
        }
    }
}
