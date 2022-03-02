using System;
using Newtonsoft.Json.Linq;

namespace BluePrism.NamedPipeServer.Interfaces
{
    public interface IServerCommunication : IDisposable
    {

        bool CanRead { get; }

        string ReadMessage();


        JObject ReadMessageAsJObject();


        int SendMessage(string outString);

    }
}
