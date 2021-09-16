using System;
using System.Drawing;
using Win32 = BluePrism.BPCoreLib.modWin32;

namespace AutomateControls.WindowsSupport
{
    /// <summary>
    /// Class creating and representing a device context, given an active handle.
    /// </summary>
    public class DeviceContext : IDisposable
    {
        // Flag indicating if this object has already been disposed of.
        private bool _disposed;

        // A lock used for creating the graphics object on the device context,
        // ensuring we only create one for this DC
        private readonly object _graphicsLock;

        // The window handle that we are representing a device context on.
        private IntPtr _hwnd;

        // The device context pointer itself.
        private IntPtr _dc;

        // The graphics object, lazily initialised from the device context.
        private Graphics _g;

        /// <summary>
        /// Creates a new device context for the window referenced by the given
        /// window handle.
        /// </summary>
        /// <param name="hwnd">The window handle for the window on which a
        /// device context is required.</param>
        public DeviceContext(IntPtr hwnd)
        {
            _hwnd = hwnd;
            _dc = Win32.GetDC(hwnd);
            _graphicsLock = new object();
        }

        /// <summary>
        /// Destroys this device context instance
        /// </summary>
        ~DeviceContext()
        {
            Dispose(false);
        }

        /// <summary>
        /// The graphics object backed by this device context.
        /// </summary>
        public Graphics Graphics
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException("Device Context has been disposed");

                lock (_graphicsLock)
                {
                    if (_g == null)
                        _g = Graphics.FromHdc(_dc);
                    return _g;
                }
            }
        }

        /// <summary>
        /// Explicitly disposes of this device context.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Handles the disposing of this device context.
        /// </summary>
        /// <param name="explicitly">true if this object is being disposed of
        /// explicitly, false if it is being disposed of as a result of the
        /// garbage collector running finalizers.</param>
        protected virtual void Dispose(bool explicitly)
        {
            if (!_disposed )
            {
                if (explicitly)
                {
                    // free managed resources
                    if (_g != null)
                    {
                        _g.Dispose();
                        _g = null;
                    }
                }

                // free unmanaged resources
                if (_dc != IntPtr.Zero)
                    Win32.ReleaseDC(_hwnd, _dc);

                _disposed = true;
            }
        }

    }
}
