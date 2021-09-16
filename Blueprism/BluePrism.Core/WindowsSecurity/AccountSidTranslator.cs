using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace BluePrism.Core.WindowsSecurity
{
    /// <summary>
    /// Handles translation between Windows account names and SIDs
    /// </summary>
    public static class AccountSidTranslator
    {
        /// <summary>
        /// Get the SID string for an account name
        /// </summary>
        /// <param name="accountName">The account name in the form of a local user name or domain FQDN</param>
        /// <returns>The SID for the account name</returns>
        public static string GetSidFromUserName(string accountName)
        {
            SecurityIdentifier identifier;
            if (accountName == "LocalSystem")
            {
                // https://msdn.microsoft.com/en-us/library/windows/desktop/ms684190(v=vs.85).aspx
                // The LocalSystem account is a predefined local account used by the service control 
                // manager. This account is not recognized by the security subsystem, so you cannot 
                // specify its name in a call to the LookupAccountName function (which looks like it
                // is used internally by NTAccount.Translate used below).

                // The documentation for 2 other local service accounts NetworkService and LocalService 
                // suggests that similar handling would be required:
                //
                // https://msdn.microsoft.com/en-us/library/windows/desktop/ms684272(v=vs.85).aspx
                // https://msdn.microsoft.com/en-us/library/windows/desktop/ms684188(v=vs.85).aspx

                // However, these appear to convert correctly using with the NTAccount.Translate method.
                // This is likely to be because LocalSystem does not have a profile of its own. Instead
                // we need to create an identifier based on its well known SID.

                // The domainSid parameter is not required for LocalSystemSid
                // https://msdn.microsoft.com/en-us/library/214122bs(v=vs.110).aspx
                identifier = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
            }
            else
            {
                var account = new NTAccount(accountName);
                identifier = (SecurityIdentifier)account.Translate(typeof(SecurityIdentifier));
            }
            return identifier != null ? identifier.Value : null;
        }
        
        /// <summary>
        /// Gets the account name for an SID string
        /// </summary>
        /// <param name="sid">The SID to get the account name for</param>
        /// <returns>The account name for the SID</returns>
        public static string GetUserNameFromSid(string sid)
        {
            var identifier = new SecurityIdentifier(sid);
            var account = identifier.Translate(typeof(NTAccount));
            return account.Value;
        }

        /// <summary>
        /// Converts an SID string to the standard format (S-R-I-S-S). If the SID is
        /// already in the standard format it will be returned as-is. If it is one
        /// of the string constants defined in sddl.h it will be converted to the 
        /// standard format. Further details about SID string constants is available at
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/aa379602(v=vs.85).aspx
        /// </summary>
        /// <remarks>
        /// The httpapi.dll stores SIDs using the 2-character constants 
        /// </remarks>
        /// <param name="sid">The SID string to convert to the standard format</param>
        /// <returns>A SID string in a standard format (S-R-I-S-S)</returns>
        public static string EnsureStandardSidString(string sid)
        {
            return new SecurityIdentifier(sid).Value;
        }

        /// <summary>
        /// Gets account names for a sequence of SID strings and returns a dictionary 
        /// mapping SID to account name, keyed by SID
        /// </summary>
        /// <param name="sids">A sequence of SIDs</param>
        /// <returns>A mapped dictionary of SIDS and account names, for a sequence of SIDs</returns>
        public static Dictionary<string, string> GetUserNames(IEnumerable<string> sids)
        {
            if (sids == null) throw new ArgumentNullException(nameof(sids));

            var map = new Dictionary<string,string>();
            foreach (var sid in sids.Distinct())
            {
                map[sid] = GetUserNameFromSid(sid);
            }
            return map;
        }
    }
}
