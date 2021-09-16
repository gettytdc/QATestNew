using System;
using System.IO.Pipes;
using BluePrism.BrowserAutomation.Events;
using BluePrism.NamedPipes;
using BluePrism.BrowserAutomation.WebMessages;
using BluePrism.BrowserAutomation.WebMessages.Events;

namespace BluePrism.BrowserAutomation.NamedPipe
{
    public interface INamedPipeWrapper : IDisposable
    {
        NamedPipeClientStream Pipe { get; }

        void SendMessage(WebMessageWrapper wrapper);

        event WebMessageReceivedDelegate MessageReceived;

        event PipeDisposedDelegate PipeDisposed;

        event NativeMessagingHostNotFoundDelegate HostNotFound;

    }
}
