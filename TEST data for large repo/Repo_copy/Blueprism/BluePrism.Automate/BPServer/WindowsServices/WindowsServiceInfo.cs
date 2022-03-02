using BluePrism.Core.WindowsSecurity;
using System;

namespace BluePrism.BPServer.WindowsServices
{
    /// <summary>
    /// Contains information about a windows service
    /// </summary>
    public class WindowsServiceInfo
    {
        private string _state;
        private string _startmode;

        /// <summary>
        /// Create a new instance of the <cref="WindowsSeviceInfo"/> class
        /// </summary>
        /// <param name="servicePath">The full path to the service executable file, including arguments </param>
        /// <param name="name">The name of the service</param>
        /// <param name="userAccount">The account that the service runs under</param>
        /// <param name="startMode">Start mode of the Windows service.</param>
        /// <param name="state">Current state of the Windows service.</param>
        /// <param name="hasUrlPermission">Whether the StartName user has the correct permissions for the service</param>
        /// <exception cref="System.ArgumentNullException">Thrown if no user account data is provided.</exception>
        public WindowsServiceInfo(string servicePath, string name, UserAccountIdentifier userAccount, 
            string startMode, string state, bool hasUrlPermission)
        {
            if (userAccount == null) throw new ArgumentNullException(nameof(userAccount));
            
            PathName = servicePath;
            Name = name;
            UserAccount = userAccount;
            StartMode = startMode;
            State = state;
            HasUrlPermission = hasUrlPermission;
        }

        /// <summary>
        /// The full path to the service executable file, including arguments 
        /// </summary>
        public string PathName  { get; private set; }

        /// <summary>
        /// The name of the service.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The user account that the service runs under
        /// </summary>
        public UserAccountIdentifier UserAccount { get; private set; }

        /// <summary>
        /// The account name that the service runs under. The account name may be in 
        /// the form of "DomainName\Username" or UPN format ("Username@DomainName").
        /// </summary>
        public string StartName  { get { return UserAccount.Name; } }

        /// <summary>
        /// The Sid of the account that the service runs under
        /// </summary>
        public string StartSid { get { return UserAccount.Sid; } }
        
        /// <summary>
        /// Start mode of the Windows service.
        /// </summary>
        public string StartMode
        {
            get { return _startmode; }
            set
            {
                switch (value.ToLower())
                {
                    case "automatic":
                    case "auto":
                        _startmode = Properties.Resources.Automatic;
                        break;
                    case "boot":
                        _startmode = Properties.Resources.Boot;
                        break;
                    case "disabled":
                        _startmode = Properties.Resources.Disabled;
                        break;
                    case "manual":
                        _startmode = Properties.Resources.Manual;
                        break;
                    case "system":
                        _startmode = Properties.Resources.System;
                        break;
                    default:
                        _startmode = value;
                        break;
                }
            }
        }

        /// <summary>
        /// Current state of the Windows service.
        /// </summary>
        public string State
        {
            get { return _state; }
            set
            {
                switch (value.ToLower())
                {
                    case "running":
                        _state = Properties.Resources.Running;
                        break;
                    case "stopped":
                        _state = Properties.Resources.Stopped;
                        break;
                    case "paused":
                        _state = Properties.Resources.Paused;
                        break;
                    default:
                        _state = value;
                        break;
                }
            }
        }
                
        /// <summary>
        /// Whether the StartName user has the correct permissions for the service
        /// </summary>
        public bool HasUrlPermission { get; private set; }

    }

}


