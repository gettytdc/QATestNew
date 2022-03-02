using BluePrism.Core.ActiveDirectory.UserQuery;

namespace BluePrism.ActiveDirectoryUserSearcher.Models
{
    public class User
    {
        public User(ActiveDirectoryUser user, bool isChecked)
        {
            UserPrincipalName = user.UserPrincipalName;
            DistinguishedName = user.DistinguishedName;
            Sid = user.Sid;
            AlreadyMapped = user.AlreadyMapped;
            IsChecked = isChecked;
        }

        public bool AlreadyMapped { get; }
        public string UserPrincipalName { get; }
        public string DistinguishedName { get; }
        public string Sid { get; }

        public bool IsChecked { get; set; }
    }
}