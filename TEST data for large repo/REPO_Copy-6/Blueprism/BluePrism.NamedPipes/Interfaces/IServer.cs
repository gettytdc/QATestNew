using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq.Expressions;

namespace BluePrism.NamedPipes.Interfaces
{
    public interface IServer : IDisposable
    {
        string ServerName { get; }
        
        void SendMessageAsClient(object messageObject, string pipeName);

        void SendToMultiplePipes(object messageObject,
            Expression<Func<KeyValuePair<string, NamedPipeServerStream>, bool>> filter);

        void SendToAllPipes(object messageObject);

    }
}
