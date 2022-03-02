
namespace BluePrism.Core.ActiveDirectory.UserQuery
{ 
    public class UserFilter
    {        
        public readonly string LdapFilter;                        

        public UserFilter(UserFilterType filterType, string value)
        {
            const string UserCategoryFilter = "(objectCategory=user)";
            LdapFilter = (filterType == UserFilterType.None || string.IsNullOrWhiteSpace(value))
                           ? UserCategoryFilter : $"(&{UserCategoryFilter}({filterType.LdapIdentifier()}={LdapEscaper.EscapeSearchTerm(value, false)}))";                       
        }
    }
}
