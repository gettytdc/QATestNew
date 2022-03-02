using System.Security.Principal;

namespace BluePrism.Core.WindowsSecurity
{
    public class WindowsIdentityWrapper : IWindowsIdentity
    {
        public WindowsIdentityWrapper(WindowsIdentity windowsIdentity)
        {
            Identity = windowsIdentity;
        }

        public WindowsIdentity Identity { get; }

        public bool IsValidAccountType 
            => !Identity.IsSystem && !Identity.IsGuest && !Identity.IsAnonymous;

        public bool IsAuthenticated => Identity.IsAuthenticated;

        public SecurityIdentifier Sid => Identity.User;

        public WindowsImpersonationContext Impersonate() => Identity.Impersonate();

        public void Dispose() => Identity?.Dispose();

       
    }
}
