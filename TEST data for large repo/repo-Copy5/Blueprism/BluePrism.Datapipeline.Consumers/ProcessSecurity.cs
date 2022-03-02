using System;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace BluePrism.Datapipeline.Logstash
{
    /// <summary>
    /// This class is used to allow a process created for a different user account to have access to the desktop.
    /// </summary>
    internal class ProcessSecurity
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetProcessWindowStation();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetThreadDesktop(int dwThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int GetCurrentThreadId();

        public static void GrantAccessToWindowStationAndDesktop(string username)
        {
            const int WindowStationAllAccess = 0x000f037f;
            const int DesktopRightsAllAccess = 0x000f01ff;

            GrantAccess(username, 
                        GetProcessWindowStation(), 
                        WindowStationAllAccess);           
            GrantAccess(username, 
                        GetThreadDesktop(GetCurrentThreadId()), 
                        DesktopRightsAllAccess);
        }

        private static void GrantAccess(string username, IntPtr handle, int accessMask)
        {
            SafeHandle safeHandle = new NoopSafeHandle(handle);
            GenericSecurity security =
                new GenericSecurity(
                    false, ResourceType.WindowObject, safeHandle, AccessControlSections.Access);

            security.AddAccessRule(
                new GenericAccessRule(
                    new NTAccount(username), accessMask, AccessControlType.Allow));
            security.Persist(safeHandle, AccessControlSections.Access);
        }

        private class GenericAccessRule : AccessRule
        {
            public GenericAccessRule(
                IdentityReference identity, int accessMask, AccessControlType type) :
                base(identity, accessMask, false, InheritanceFlags.None,
                     PropagationFlags.None, type)
            {
            }
        }

        private class GenericSecurity : NativeObjectSecurity
        {
            public GenericSecurity(
                bool isContainer, ResourceType resType, SafeHandle objectHandle,
                AccessControlSections sectionsRequested)
                : base(isContainer, resType, objectHandle, sectionsRequested)
            {
            }

            new public void Persist(SafeHandle handle, AccessControlSections includeSections) 
                => base.Persist(handle, includeSections);

            new public void AddAccessRule(AccessRule rule) => base.AddAccessRule(rule);

            #region NativeObjectSecurity Abstract Method Overrides

            public override Type AccessRightType => throw new NotImplementedException(); 


            public override AccessRule AccessRuleFactory(
                System.Security.Principal.IdentityReference identityReference,
                int accessMask, bool isInherited, InheritanceFlags inheritanceFlags,
                PropagationFlags propagationFlags, AccessControlType type) 
                => throw new NotImplementedException();
            

            public override Type AccessRuleType => typeof(AccessRule);

            public override AuditRule AuditRuleFactory(
                System.Security.Principal.IdentityReference identityReference, int accessMask,
                bool isInherited, InheritanceFlags inheritanceFlags,
                PropagationFlags propagationFlags, AuditFlags flags) 
                => throw new NotImplementedException();

            public override Type AuditRuleType => typeof(AuditRule);          

            #endregion
        }

        // Handles returned by GetProcessWindowStation and GetThreadDesktop should not be closed
        private class NoopSafeHandle : SafeHandle
        {
            public NoopSafeHandle(IntPtr handle) :
                base(handle, false)
            {
            }

            public override bool IsInvalid => false; 

            protected override bool ReleaseHandle() => true;
        }
    }
}
