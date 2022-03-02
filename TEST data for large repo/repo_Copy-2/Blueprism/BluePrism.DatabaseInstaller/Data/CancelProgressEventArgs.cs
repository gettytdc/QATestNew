using System;

namespace BluePrism.DatabaseInstaller
{
    public delegate void CancelProgressEventHandler(object sender, CancelProgressEventArgs e);

    public class CancelProgressEventArgs : EventArgs
    {
        public CancelStatus Status { get; }

        public CancelProgressEventArgs(CancelStatus status)
        {
            Status = status;
        }
    }
}