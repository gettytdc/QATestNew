using System;
using System.Diagnostics;
using System.Threading;
using BluePrism.BrowserAutomation.WebMessages;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace BluePrism.NamedPipes.UnitTests
{
    public class ServerTests
    {
        private const string ServerName = "TestServer";
        private bool _called;
        private void MessageHandler(ServerCommunication serverCommunication, string bluePrismClientId)
        {
            while (serverCommunication.CanRead)
            {
                var command = serverCommunication.ReadMessage();
                if (string.IsNullOrEmpty(command))
                {
                    continue;
                }
                _called = true;
            }
        }

        [Test]
        [Ignore("Causing build issues for v7, ignored until after code freeze")]
        public void InstantiatePipeServerShouldHaveValidCorrectName()
        {
            using (var pipeServer = new Server(ServerName, MessageHandler))
            {
                Assert.AreEqual(ServerName, pipeServer.ServerName);
            }
        }

        [Test]
        [Ignore("Causing build issues for v7, ignored until after code freeze")]
        public void PipeServerShouldAcceptConnection()
        {
            bool isConnected;
            using (var pipeServer = new Server(ServerName, MessageHandler))
            {
                using (var clientPipe = new NamedPipeClient(ServerName))
                {
                    isConnected = clientPipe.Connect();
                }
            }

            Assert.IsTrue(isConnected);
        }

        [Test]
        [Ignore("Causing build issues for v7, ignored until after code freeze")]
        public void PipeServerMessageHandlerShouldBeCalled()
        {
            _called = false;

            using (var pipeServer = new Server(ServerName, MessageHandler))
            {
                using (var clientPipe = new NamedPipeClient(ServerName))
                {
                    if (clientPipe.Connect())
                    {
                        clientPipe.Listen();
                        Thread.Sleep(1000);
                        var message = new WebMessageWrapper(Guid.NewGuid(), "{\"message\": \"Bogus message\"");
                        clientPipe.SendMessage(message);
                    }

                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    while (!_called && stopwatch.Elapsed.Seconds < 10)
                    {
                        //Waiting for called to be true
                    }
                    Assert.IsTrue(_called);
                }
            }
        }

        [Test]
        [Ignore("Causing build issues for v7, ignored until after code freeze")]
        public void PipeServerShouldSendMessageAsClientToClient()
        {
            var messageReceived = false;
            using (var pipeServer = new Server(ServerName, MessageHandler))
            {

                var clientPipe = CreateClient();
                clientPipe.MessageReceived += delegate
                {
                    messageReceived = true;
                };

                pipeServer.SendMessageAsClient(JObject.Parse("{\"message\": \"Bogus message\"}"),
                                        $"{ServerName}-{clientPipe.ClientId.ToString()}");


                var stopwatch = new Stopwatch();
                stopwatch.Start();
                while (!messageReceived && stopwatch.Elapsed.Seconds < 10)
                {
                    Thread.Sleep(10);
                }

                clientPipe.Dispose();
            }

            Assert.IsTrue(messageReceived);
        }

        [Test]
        [Ignore("Causing build issues for v7, ignored until after code freeze")]
        public void PipeServerShouldSendMessageAsClientToAllClient()
        {
            var messageReceivedOnClientPipe1 = false;
            var messageReceivedOnClientPipe2 = false;
            var messageReceivedOnClientPipe3 = false;

            using (var pipeServer = new Server(ServerName, MessageHandler))
            {
                var clientPipe1 = CreateClient();
                clientPipe1.MessageReceived += delegate
                {
                    messageReceivedOnClientPipe1 = true;
                };

                var clientPipe2 = CreateClient();
                clientPipe2.MessageReceived += delegate
                {
                    messageReceivedOnClientPipe2 = true;
                };

                var clientPipe3 = CreateClient();
                clientPipe3.MessageReceived += delegate
                {
                    messageReceivedOnClientPipe3 = true;
                };

                pipeServer.SendToAllPipes(JObject.Parse("{\"message\": \"Bogus message\"}"));

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                while (true)
                {
                    if ((messageReceivedOnClientPipe1 &&
                        messageReceivedOnClientPipe2 &&
                        messageReceivedOnClientPipe3) ||
                        stopwatch.Elapsed.Seconds > 15){ break; }

                    Thread.Sleep(10);
                }
            }

            Assert.IsTrue(messageReceivedOnClientPipe1);
            Assert.IsTrue(messageReceivedOnClientPipe2);
            Assert.IsTrue(messageReceivedOnClientPipe3);
        }

        [Test]
        [Ignore("Causing build issues for v7, ignored until after code freeze")]
        public void PipeServerShouldSendMessageAsClientToSomeClients()
        {
            var messageReceivedOnClientPipe1 = false;
            var messageReceivedOnClientPipe2 = false;
            var messageReceivedOnClientPipe3 = false;

            using (var pipeServer = new Server(ServerName, MessageHandler))
            {
                var clientPipe1 = CreateClient();
                var clientPipe2 = CreateClient();
                var clientPipe3 = CreateClient();

                void clientPipe1Delegate(object sender, MessageReceivedDelegateEventArgs args)
                { messageReceivedOnClientPipe1 = true; }
                clientPipe1.MessageReceived += clientPipe1Delegate;

                void clientPipe2Delegate(object sender, MessageReceivedDelegateEventArgs args)
                { messageReceivedOnClientPipe2 = true; }
                clientPipe2.MessageReceived += clientPipe2Delegate;

                void clientPipe3Delegate(object sender, MessageReceivedDelegateEventArgs args)
                { messageReceivedOnClientPipe3 = true; }
                clientPipe3.MessageReceived += clientPipe3Delegate;

                pipeServer.SendToMultiplePipes(JObject.Parse("{\"message\": \"Bogus message\"}"), o => o.Key != $"{ServerName}-{clientPipe2.ClientId.ToString()}");

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                while (true)
                {
                    if ((messageReceivedOnClientPipe1 &&
                         messageReceivedOnClientPipe3) ||
                        stopwatch.Elapsed.Seconds > 15)
                    { break; }

                    Thread.Sleep(10);
                }

                Assert.IsTrue(messageReceivedOnClientPipe1);
                Assert.IsFalse(messageReceivedOnClientPipe2);
                Assert.IsTrue(messageReceivedOnClientPipe3);

                clientPipe1.MessageReceived -= clientPipe1Delegate;
                clientPipe2.MessageReceived -= clientPipe2Delegate;
                clientPipe3.MessageReceived -= clientPipe3Delegate;
            }
        }


        private NamedPipeClient CreateClient()
        {
            var clientPipe = new NamedPipeClient(ServerName);
            if (clientPipe.Connect())
            {
                clientPipe.Listen();
            }

            return clientPipe;
        }
    }
}
