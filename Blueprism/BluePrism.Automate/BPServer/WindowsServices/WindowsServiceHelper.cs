using BluePrism.BPCoreLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using BluePrism.Core.WindowsSecurity;
using BluePrism.Core.HttpConfiguration;
using System.Security.Principal;
using System.DirectoryServices.AccountManagement;
using System.Reflection;

namespace BluePrism.BPServer.WindowsServices
{
    /// <summary>
    /// Helper class for working with BP Server Windows Services
    /// </summary>
    public class WindowsServiceHelper
    {
        /// <summary>
        /// The directory name of the current executing assembly. This is typically
        /// in the same location as the BPServerService.exe.
        /// </summary>
        private readonly string _currentDirectory;

        /// <summary>
        /// Create a new instance of the Windows Service Helper
        /// </summary>
        public WindowsServiceHelper() : this(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) {}

        /// <summary>
        /// Used for unit tests to creates a new instance of the Windows Service 
        /// Helper with the current directory specified.
        /// </summary>
        internal WindowsServiceHelper(string directoryName)
        {
            _currentDirectory = directoryName;
        }

        /// <summary>
        /// Gets windows service info for a specified configuration name
        /// </summary>
        public IEnumerable<WindowsServiceInfo> GetWindowsServicesForConfigurationName(string configName, BindingProperties properties)
        {
            // Get existing URL reservations that apply to binding address
            var service = new HttpConfigurationService();
            var urlReservations = service.GetUrlReservations()
                .Where(x => properties.MatchesReservationUrl(x.Url))
                .ToList();
        
            
            using (var searcher = new ManagementObjectSearcher(
                " SELECT Name, PathName, StartName, StartMode, State" +
                " FROM Win32_Service" +
                " WHERE PathName LIKE \"%bpserverservice.exe%\""))
            {

                return searcher
                    .Get()
                    .OfType<ManagementObject>()
                    .Where(sv => IsServerAssociatedWithConfiguration((string)sv["PathName"], configName))
                    .Select(sv =>
                        {
                            var startName = (string)sv["StartName"];
                            var sid = AccountSidTranslator.GetSidFromUserName(RemoveBuiltInDomainFromAccountName(startName));
                            var userAccount = new UserAccountIdentifier(startName, sid);
                            var hasUrlPermission = DoesUserAccountHaveCorrectPermissionsForUrl(userAccount, urlReservations);

                            return new WindowsServiceInfo(
                                (string)sv["PathName"],
                                (string)sv["Name"],
                                userAccount,
                                (string)sv["StartMode"],
                                (string)sv["State"],
                                hasUrlPermission
                            );
                        });                 
            }
        }


        /// <summary>
        /// Returns true, if the service path is for a BP Server Service and has
        /// a single command line argument set to the configuration name. It will 
        /// also return true if there are no command line arguments, but the 
        /// configuration name is 'Default'.
        /// </summary>
        /// <param name="servicePath">The full path of the service executable, 
        /// including command line args</param>
        /// <param name="configName">The configuation name associated with the service
        /// </param>
        /// <returns>True, if the service path is a bp server sevice associated 
        /// with the specified configuration</returns>
        public bool IsServerAssociatedWithConfiguration(string servicePath, string configName)
        {
            try
            {
                var spaceSeparator = " ".ToCharArray();
                var quoteSeparator = "\"".ToCharArray();
                var executableName = "bpserverservice.exe";

                //In order to get the file name using Path.GetPath, the service path must contain no qputes
                var fileName = Path.GetFileName(servicePath.Replace("\"", "")).Split(spaceSeparator)[0];

                //Check the file name is that of the Blue Prism Server executable
                if (fileName.Equals(executableName, StringComparison.OrdinalIgnoreCase))
                {
                    //Find the starting index of the executable name in the original service path string (including quotes)
                    var i = servicePath.IndexOf(executableName, StringComparison.OrdinalIgnoreCase) + executableName.Length;

                    //Get the substring that appears after the executable name in the file path
                    var argsLine = servicePath.Substring(i, servicePath.Length - i);

                    //If the service path starts with a quote, we need to remove the quote from after the start of the substring
                    //We also need to trim the leading space
                    argsLine = (servicePath.StartsWith("\"")) ?
                        argsLine.TrimStart(quoteSeparator).TrimStart(spaceSeparator) : argsLine.TrimStart(spaceSeparator);

                    //Use the win32 function for correctly splitting a argument line into separate args
                    //This should deal with escaping characters             
                    string[] splitArgs = modWin32.SplitArgs(argsLine);


                    //When there are no arguments supplied for the service, the service is run under the 
                    //'Default' Blue Prism configuration.
                    if (splitArgs.Length == 0 && configName.Equals("Default", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    //Otherwise check the argument matches the config name
                    if (splitArgs.Length == 1 && configName.Equals(splitArgs[0], StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    //The server service does not support multiple arguments
                    if (splitArgs.Length > 1)
                    {
                        return false;
                    }

                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }


        }

        /// <summary>
        /// Returns True if the account has the correct permissions to listen 
        /// </summary>
        private bool DoesUserAccountHaveCorrectPermissionsForUrl(UserAccountIdentifier userAccount, List<UrlReservation> urlReservations)
        {
            if (IsLocalAdministrator(userAccount)) return true;

            return urlReservations
                .Any(x => x.SecurityDescriptor.Entries.Any(e => e.Sid.Equals(userAccount.Sid, StringComparison.OrdinalIgnoreCase) 
                    && e.AllowListen));
        }

        /// <summary>
        /// Checks whether the account identified belongs to
        /// the local machine's built-in Administrators group, or a group that 
        /// derives from the group
        /// </summary>
        /// <param name="accountName">Account name to check</param>
        /// <returns>True if the specified account belongs to the local machine's
        /// built-in Administrators group</returns>
        private bool IsLocalAdministrator(UserAccountIdentifier account)
        {
            var localSystemIdentifier = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
            if (account.Sid == localSystemIdentifier.Value) return true;

            var administratorsIdentifier = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
            string adminGroupSid = administratorsIdentifier.Value;

            using (var context = new PrincipalContext(ContextType.Machine))
            using (var principal = new GroupPrincipal(context))
            using (var groupSearcher = new PrincipalSearcher(principal))
            {
                // It should be noted, that using the Principal Searcher
                // is a far quicker way of retrieving the admin group using its Sid 
                // than using GroupPrincipal.FindByIdentity(),
                // which takes several seconds.
                return groupSearcher
                    .FindAll()
                    .Cast<GroupPrincipal>()
                    .FirstOrDefault(g => g.Sid.Value.Equals(adminGroupSid, StringComparison.OrdinalIgnoreCase))
                    .GetMembers(true)
                    .OfType<UserPrincipal>()
                    .Select(x => x.Sid.Value)
                    .Contains(account.Sid, StringComparer.OrdinalIgnoreCase);
                
            }
        }

        /// <summary>
        /// Remove the leading .\ from an account name, which appears if the account
        /// belongs to the built in domain. If the account doesn't belong to the built
        /// in domain, then the string is unchanged.
        /// </summary>
        internal string RemoveBuiltInDomainFromAccountName(string accountName)
        {
            return accountName.StartsWith(".\\") ? accountName.Remove(0, 2) : accountName;
        }

        /// <summary>
        /// Returns a command that can be used to create a windows service from the 
        /// command line.
        /// </summary>
        /// <param name="configName">The configuration name to associate the service 
        /// against
        /// </param>
        /// <returns>Command text that will create a web service associated with the 
        /// specified configuration.
        /// </returns>
        internal string GetCreateWindowsServiceCommandText(string configName)
        {
            // Ensure the arguments in the command are enclosed in escaped quotes i.e. \",
            // in case the config name contains spaces.
            return String.Format("sc create \"{0}\" binPath= \"{1} \\\"{2}\\\"\"",
                    GetServiceName(configName), 
                    Path.Combine(_currentDirectory, "BPServerService.exe"), 
                    configName);        
        }

        /// <summary>
        /// Get the service path for the current directory, with the executable name 
        /// followed by config name in quotes as a single argument.
        /// </summary>
        /// <param name="configName">The name of the configuration</param>
        /// <returns>A service path with the config name as an argument</returns>
        internal string GetServicePathForCurrentDirectory(string configName)
        {
            // Ensure the arguments in the command are enclosed in escaped quotes i.e. \",
            // in case the config name contains spaces.
            return String.Format("{0} \"{1}\"", Path.Combine(_currentDirectory, "BPServerService.exe"), configName);
        }

        /// <summary>
        /// Gets the name that can be used to create Blue Prism Server Windows 
        /// service, for a specified configuration name.
        /// </summary>
        /// <param name="configName">The configuration name the service runs under</param>
        /// <returns>A service name for a specified configuration name</returns>
        internal string GetServiceName(string configName)
        {
            return String.Format("Blue Prism Server:{0}", configName);
        }

    }
}
