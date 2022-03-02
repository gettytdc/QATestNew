using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Windows.Management.Deployment;
using Windows.ApplicationModel;
using System.ComponentModel;

namespace BluePrism.ApplicationManager.ContainerApp
{
    public static class clsContainerApp
    {
        enum ActivateOptions
        {
            None = 0x00000000,              // No flags set
            DesignMode = 0x00000001,        // The application is being activated for design mode
            NoErrorUI = 0x00000002,         // Do not show an error dialog if the app fails to activate                                
            NoSplashScreen = 0x00000004,    // Do not show the splash screen when activating the app
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("2E941141-7F97-4756-BA1D-9DECDE894A3d")]
        interface IApplicationActivationManager
        {
            int ActivateApplication([MarshalAs(UnmanagedType.LPWStr)] string appUserModelId, [MarshalAs(UnmanagedType.LPWStr)] string arguments,
                ActivateOptions options, out uint processId);
            int ActivateForFile([MarshalAs(UnmanagedType.LPWStr)] string appUserModelId, IntPtr pShelItemArray,
                [MarshalAs(UnmanagedType.LPWStr)] string verb, out uint processId);
            int ActivateForProtocol([MarshalAs(UnmanagedType.LPWStr)] string appUserModelId, IntPtr pShelItemArray,
                [MarshalAs(UnmanagedType.LPWStr)] string verb, out uint processId);
        }

        [ComImport, Guid("45BA127D-10A8-46EA-8AB7-56EA9078943C")]
        class ApplicationActivationManager{ }

        [DllImport("kernel32")]
        static extern int OpenPackageInfoByFullName([MarshalAs(UnmanagedType.LPWStr)] string fullName, uint reserved, out IntPtr packageInfo);

        [DllImport("kernel32")]
        static extern int GetPackageApplicationIds(IntPtr pir, ref int bufferLength, byte[] buffer, out int count);

        [DllImport("kernel32")]
        static extern int ClosePackageInfo(IntPtr pir);

        /// <summary>
        /// Determines if the passed app name is available to the current user, and
        /// if so returns it's full package name (required for launching).
        /// </summary>
        /// <param name="name">The short name of the app (e.g.
        /// Microsoft.WindowsCalculator</param>
        /// <param name="fullName">The full app package name</param>
        /// <returns>True if the app is available, otherwise False</returns>
        public static bool IsAppInstalled(string name, out string fullName)
        {
            fullName = "";
            PackageManager pm = new PackageManager();
            IEnumerable<Package> pkgs = pm.FindPackagesForUser(string.Empty);

            foreach (Package p in pkgs)
            {
                if (string.Format("<{0}>", p.Id.Name) == name)
                {
                    fullName = p.Id.FullName;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether or not container apps are available to be launched on
        /// the current Windows version.
        /// </summary>
        /// <returns>True if os supports container apps, otherwise false</returns>
        public static bool ContainerAppsAvailable()
        {
            Version ver = Environment.OSVersion.Version;
            if (ver.Major < 6 || (ver.Major == 6 && ver.Minor < 2)) return false;
            return true;
        }

        /// <summary>
        /// Launches the passed container application.
        /// </summary>
        /// <param name="name">The short name of the app (e.g.
        /// Microsoft.WindowsCalculator</param>
        /// <param name="arguments">Any arguments to pass to the app</param>
        /// <returns>The PID of the launched app</returns>
        public static int LaunchApp(string name, string arguments = "")
        {
            IntPtr pir = IntPtr.Zero;
            try
            {
                // Get full name for this app
                string packageFullName = string.Empty;
                if (!IsAppInstalled(name, out packageFullName))
                    throw new InvalidOperationException(String.Format("Application {0} not found.", name));

                // Open the package identified by the passed name
                int res = OpenPackageInfoByFullName(packageFullName, 0, out pir);
                if (res != 0) throw new Win32Exception(res);

                // Get the IDs of the apps in this package - first call is to determine
                // buffer size. Although we're going to assume the first ID is required
                int length = 0;
                int count;
                GetPackageApplicationIds(pir, ref length, null, out count);

                byte[] buffer = new byte[length];
                res = GetPackageApplicationIds(pir, ref length, buffer, out count);
                if (res != 0) throw new Win32Exception(res);

                string appUserModelId = Encoding.Unicode.GetString(buffer, IntPtr.Size * count, length - IntPtr.Size * count);

                // Launch the app and get it's PID
                uint pid;
                IApplicationActivationManager activation = (IApplicationActivationManager)new ApplicationActivationManager();
                res = activation.ActivateApplication(appUserModelId, arguments, ActivateOptions.NoErrorUI, out pid);
                if (res != 0) Marshal.ThrowExceptionForHR(res);

                // Return process id
                return (int)pid;
            }
            finally
            {
                if (pir != IntPtr.Zero) ClosePackageInfo(pir);
            }

        }
    }
}
