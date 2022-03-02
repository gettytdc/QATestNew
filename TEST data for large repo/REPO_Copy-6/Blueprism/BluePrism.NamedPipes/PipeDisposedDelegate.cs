using System;

namespace BluePrism.NamedPipes
{
    public delegate void PipeDisposedDelegate(object sender, PipeDisposedDelegateEventArgs args);

    public class PipeDisposedDelegateEventArgs : EventArgs
    {
        public Guid ClientId { get; }

        public PipeDisposedDelegateEventArgs(Guid clientId) => ClientId = clientId;
    }
}
