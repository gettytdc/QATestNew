using BluePrism.Core.WindowsSecurity;

namespace BluePrism.Core.HttpConfiguration
{
    /// <summary>
    /// Contains information from an individual ACE element within the SDDL string 
    /// of a URL Reservation
    /// </summary>
    /// <remarks>
    /// See the following for background on format and values that apply to URL ACL:
    /// https://msdn.microsoft.com/en-us/library/windows/desktop/aa364653(v=vs.85).aspx
    /// </remarks>
    public class AccessControlEntry
    {

        /// <summary>
        /// Creates a new AccessControlEntry
        /// </summary>
        /// <param name="sid">The SID for the identity</param>
        /// <param name="allowListen">Whether the listen permission is granted</param>
        /// <param name="allowDelegate">Whether the delegate permission is granted</param>
        public AccessControlEntry(string sid, bool allowListen, bool allowDelegate)
        {
            RawSid = sid;
            Sid = AccountSidTranslator.EnsureStandardSidString(sid);
            AllowListen = allowListen;
            AllowDelegate = allowDelegate;

            string permission;
            if (allowListen && allowDelegate)
            {
                permission = "GA";
            }
            else if (allowListen)
            {
                permission = "GX";
            }
            else
            {
                permission = "GW";
            }
            AceString = string.Format("(A;;{0};;;{1})", permission, sid);
        }


        /// <summary>
        /// The raw SID security identifier returned by the HTTP API
        /// </summary>
        public string RawSid { get; private set; }

        /// <summary>
        /// The SID security identifier using the standard string representation (S-R-I-S-S…)
        /// </summary>
        public string Sid { get; private set; }

        /// <summary>
        /// Indicates whether the user can receive requests on the URL
        /// </summary>
        public bool AllowListen { get; private set; }

        /// <summary>
        /// Indicates whether the user can delegate requests to a subtree of the
        /// URL to another user
        /// </summary>
        public bool AllowDelegate { get; private set; }

        /// <summary>
        /// The string representation of this AccessControlEntry within an SSDL
        /// string
        /// </summary>
        public string AceString { get; private set; }

        public override string ToString()
        {
            return string.Format("Sid: {0}, AllowListen: {1}, AllowDelegate: {2}, AceString: {3}", Sid, AllowListen, AllowDelegate, AceString);
        }
    }
}