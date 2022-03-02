using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using NLog;

namespace BluePrism.AppMan.Citrix
{

    public class VirtualChannelStream : Stream
    {
        readonly IntPtr _handle;
        private static ManualResetEvent _closeReader;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private VirtualChannelStream(IntPtr handle, ManualResetEvent closeReader)
        {
           _closeReader = closeReader;
            _handle = handle;
        }

        public static VirtualChannelStream AwaitVirtualChannelStream(string channelName, ManualResetEvent closeReader)
        {
#pragma warning disable S125
            // This code may be needed when we execute apps inside a session// Sections of code should not be commented out
            /*
                            var wfSessionId = "";
                            var wfCurrentServer = IntPtr.Zero;
                            WfApi.WFQuerySessionInformation(wfCurrentServer, wfSessionId, WfApi.WfInfoClass.WfSessionId,
                                out var sessionInfo, out var bytesReturned);
                            */
#pragma warning restore S125 // Sections of code should not be commented out

            var handle = IntPtr.Zero;
            while (handle == IntPtr.Zero)
            {
                handle = WfApi.WFVirtualChannelOpen(IntPtr.Zero, -1, channelName);
                if (handle == IntPtr.Zero)
                {
                    Log.Debug($"Failed to open the channel, Sleeping for 10 seconds");
                    Thread.Sleep(10000);
                }
            }

            Log.Debug($"Opened the channel, name is {channelName}, handle is {handle}");
            return new VirtualChannelStream(handle, closeReader);
        }
        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Flush()
        {
        }
       
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (WfApi.WFVirtualChannelReadEx(_handle, _closeReader.SafeWaitHandle, buffer, count, out var bytesRead))
            {
                return bytesRead;
            }
            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                if (WfApi.WFVirtualChannelWrite(_handle, buffer, count, out var bytesWritten))
                {
                    count -= bytesWritten;
                }
            }
        }
    }

}
