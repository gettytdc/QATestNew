using System;
using System.Security.Principal;

namespace BluePrism.Core.WindowsSecurity
{
    public interface IWindowsIdentity : IDisposable
    {
        WindowsIdentity Identity { get; }
        bool IsValidAccountType { get; }
        bool IsAuthenticated { get; }
        SecurityIdentifier Sid { get; }
        WindowsImpersonationContext Impersonate();
    }
}
