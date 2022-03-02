using System;
using System.IO.Pipes;
using System.Text;
using BluePrism.NamedPipeServer.Interfaces;
using Newtonsoft.Json.Linq;

namespace BluePrism.NamedPipes
{
    public class ServerCommunication : IServerCommunication
    {
        private readonly PipeStream _stream;
        private readonly UnicodeEncoding _streamEncoding;

        public ServerCommunication(PipeStream stream)
        {
            _stream = stream;
            _stream.ReadMode = PipeTransmissionMode.Message;
            _streamEncoding = new UnicodeEncoding();
        }

        public bool CanRead => _stream.IsConnected;

       
        public string ReadMessage()
        {
            try
            {
                var messageBuffer = new byte[1024];
                var messageBuilder = new StringBuilder();
                do
                {
                    var byteCount = _stream.Read(messageBuffer, 0, messageBuffer.Length);
                    var messageChunk = _streamEncoding.GetString(messageBuffer, 0, byteCount);

                    messageBuilder.Append(messageChunk);
                    messageBuffer = new byte[messageBuffer.Length];
                } while (!_stream.IsMessageComplete && _stream.IsConnected);

                return messageBuilder.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public JObject ReadMessageAsJObject()
        {
            var msg = ReadMessage();
            try
            {
                return msg != null ? JObject.Parse(msg) : new JObject();
            }
            catch
            {
                return new JObject();
            }
        }
        public int SendMessage(string outString)
        {
            var messageBytes = _streamEncoding.GetBytes(outString);
            _stream.Write(messageBytes, 0, messageBytes.Length);

            return messageBytes.Length;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stream.Dispose();
            }
        }
    }
}
