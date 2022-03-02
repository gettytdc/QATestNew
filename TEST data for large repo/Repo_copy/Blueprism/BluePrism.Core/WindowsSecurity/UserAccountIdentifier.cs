using System;

namespace BluePrism.Core.WindowsSecurity
{
    
    /// <summary>
    /// Contains identifiers for a user account
    /// </summary>
    public class UserAccountIdentifier : IEquatable<UserAccountIdentifier>
    {
        /// <summary>
        /// Creates an UserAccountIdentifier based on an SID
        /// </summary>
        /// <param name="sid"></param>
        /// <returns></returns>
        public static UserAccountIdentifier CreateFromSid(string sid)
        {
            if (sid == null) throw new ArgumentNullException(nameof(sid));
            string name = AccountSidTranslator.GetUserNameFromSid(sid);
            return new UserAccountIdentifier(name, sid);
        }

        /// <summary>
        /// Creates an UserAccountIdentifier based on an SID
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static UserAccountIdentifier CreateFromAccountName(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            string sid = AccountSidTranslator.GetSidFromUserName(name);
            return new UserAccountIdentifier(name, sid);
        }

        /// <summary>
        /// Creates a new UserAccountIdentifier
        /// </summary>
        /// <param name="name">The account name</param>
        /// <param name="sid">The account SID</param>
        public UserAccountIdentifier(string name, string sid)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (sid == null) throw new ArgumentNullException(nameof(sid));
            Name = name;
            Sid = sid;
        }

        /// <summary>
        /// The name of the user account
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The Sid of the user account
        /// </summary>
        public string Sid { get; set; }

        public override string ToString()
        {
            return string.Format("Name: {0}, Sid: {1}", Name, Sid);
        }

        public bool Equals(UserAccountIdentifier other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Sid, other.Sid);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((UserAccountIdentifier) obj);
        }

        public override int GetHashCode()
        {
            return Sid.GetHashCode();
        }
    }

}
