using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace BluePrism.AppMan.Citrix
{
    internal static class WfApi
    {
        [DllImport("wfapi.dll", SetLastError = true)]
        public static extern IntPtr WFVirtualChannelOpen(IntPtr serverHandle, int sessionId, [MarshalAs(UnmanagedType.LPStr)]string virtualName);

        [DllImport("wfapi.dll", SetLastError = true)]
        public static extern bool WFVirtualChannelRead(IntPtr channelHandle, int timeout, byte[] buffer, int bufferSize, out int bytesRead);

        [DllImport("wfapi.dll", SetLastError = true)]
        public static extern bool WFVirtualChannelReadEx(IntPtr channelHandle, SafeWaitHandle cancelEvent, byte[] buffer, int bufferSize, out int bytesRead);

        [DllImport("wfapi.dll", SetLastError = true)]
        public static extern bool WFVirtualChannelWrite(IntPtr channelHandle, byte[] buffer, int length, out int bytesWritten);

        [DllImport("wfapi.dll", SetLastError = true)]
        public static extern bool WFQuerySessionInformation(IntPtr serverHandle, string sessionId, WfInfoClass wfInfoClass, [MarshalAs(UnmanagedType.LPWStr)]out string pBuffer, out IntPtr pBytesReturned);

        internal enum WfInfoClass
        {
            WfVersion,             // OSVERSIONINFO
            WfInitialProgram,
            WfWorkingDirectory,
            WfOEMId,
            WfSessionId,
            WfUserName,
            WfWinStationName,
            WfDomainName,
            WfConnectState,
            WfClientBuildNumber,
            WfClientName,
            WfClientDirectory,
            WfClientProductId,
            WfClientHardwareId,
            WfClientAddress,
            WfClientDisplay,
            WfClientCache,
            WfClientDrives,
            WfICABufferLength,
            WfLicenseEnabler,
            RESERVED2,
            WfApplicationName,
            WfVersionEx,
            WfClientInfo,
            WfUserInfo,
            WfAppInfo,
            WfClientLatency,
            WfSessionTime,
            WfLicensingModel
        }
    }
}
