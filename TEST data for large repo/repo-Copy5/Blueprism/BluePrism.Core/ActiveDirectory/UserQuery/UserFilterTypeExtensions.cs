using System;

namespace BluePrism.Core.ActiveDirectory.UserQuery
{
    static class UserFilterTypeExtensions
    {
        public static string LdapIdentifier(this UserFilterType filterType) => GetLdapIdentifier(filterType);
        
        private static string GetLdapIdentifier(UserFilterType filterType)
        {
            switch (filterType)
            {
                case UserFilterType.None:
                    return string.Empty;
                case UserFilterType.Cn:
                    return LdapAttributes.Cn;
                case UserFilterType.Sid:
                    return LdapAttributes.Sid;
                case UserFilterType.UserPrincipalName:
                    return LdapAttributes.UserPrincipalName;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
