using System;
using System.Linq;
using System.Threading;
using BluePrism.MessagingHost.Browser;
using BluePrism.NativeMessaging.Browser;
using BluePrism.NativeMessagingHostTests.Mock;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace BluePrism.NativeMessagingHostTests
{
    [TestFixture]
    internal class HostTests
    {
        private const string PageConnectedMessage =
            @"{
              ""data"": {
                ""sessionId"": 1,
                ""tabID"": 1,
                ""pageId"": ""d1c2ad50-997f-4ed9-a41e-68dc42f97b92"",
                ""tabTitle"": ""BBC"",
                ""url"": ""www.bbc.co.uk"",
                ""data"": ""Page connected"",
                ""extensionVersion"": ""6.0.0.0""
                }
             }";

        private const string PageDisconnectedMessage =
            @"{
              ""data"": {
                ""sessionId"": 1,
                ""tabID"": 1,
                ""pageId"": ""d1c2ad50-997f-4ed9-a41e-68dc42f97b92"",
                ""tabTitle"": ""BBC"",
                ""url"": ""www.bbc.co.uk"",
                ""data"": ""Page disconnected"",
                ""extensionVersion"": ""6.0.0.0""
                }
             }";

        private const string PageAttachedMessage =
            @"{
              ""data"": {
                ""data"": ""attached"",
                ""bpClientId"": ""BPChromeListener-62b89527-9107-4bc7-9167-10b9fee7d2b3"",
                ""title"": ""BBC News"",
                ""trackingId"": ""d578b03f-fce4-45bc-a04a-88e21e81b6c1"",
                ""pages"": [""f2475c4e-06ca-42ae-8508-c80917a60d60"", ""d1c2ad50-997f-4ed9-a41e-68dc42f97b92""],
                ""extensionVersion"": ""6.0.0.0"",
                ""conversationId"": ""7134ec82-c64d-40e0-a102-40c8b7b57d63"",
                ""attempt"": 1
                }
             }";

        private const string TabRemovedMessage =
            @"{
             ""data"": {
                ""tabId"": 2,
                ""data"": ""Tab removed""
                }
             }";

        [Test]
        public void MessageReceived_EncryptedMessagesForwardedOn()
        {
            var mockHost = new MockNativeMessagingHost();
            var host = new Host(mockHost);
            var are = new AutoResetEvent(false);
            
            host.MessageReceived += (s,e) =>
            {
                are.Set();
            };

            host.MessagingHost.ProcessReceivedMessage(JsonConvert.DeserializeObject<JObject>(@"{ ""data"": ""sduifnsidunpicvnucvn"" }"));
            var wasSignaled = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            Assert.True(wasSignaled);
        }
        [Test]
        public void PageConnected_NoExistingSession()
        {
            var mockHost = new MockNativeMessagingHost();
            var host = new Host(mockHost);
            var are = new AutoResetEvent(false);

            host.Attached += (s, e) =>
            {
                are.Set();
            };
            
            host.MessagingHost.ProcessReceivedMessage(JsonConvert.DeserializeObject<JObject>(PageConnectedMessage));
            var wasSignaled = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            Assert.True(wasSignaled);
            Assert.AreEqual(mockHost.MessageSent["message"].ToString(), @"{""message"":""acknowledged"",""pageId"":""d1c2ad50-997f-4ed9-a41e-68dc42f97b92""}");
            Assert.True(host.MessagingHost.Sessions.Count == 1);
            Assert.True(host.MessagingHost.Sessions[0].Tabs.Exists(t => t.Id == 1));
            Assert.NotNull(host.MessagingHost.GetSessionForPage("d1c2ad50-997f-4ed9-a41e-68dc42f97b92"));
        }

        [Test]
        public void PageConnected_MessageDataIsNull()
        {
            var mockHost = new MockNativeMessagingHost();
            var host = new Host(mockHost);
            var are = new AutoResetEvent(false);
            var pageConnectedMessageNoData =
            @"{
              ""data"": {
                            ""sessionId"": 1,
                ""tabID"": 1,
                ""pageId"": ""d1c2ad50-997f-4ed9-a41e-68dc42f97b92"",
                ""tabTitle"": ""BBC"",
                ""url"": ""www.bbc.co.uk"",
                ""data"": """",
                ""extensionVersion"": ""6.0.0.0""
                }
             }";

            host.Attached += (s, e) =>
            {
                are.Set();
            };
            host.MessagingHost.ProcessReceivedMessage(JsonConvert.DeserializeObject<JObject>(pageConnectedMessageNoData));
            var wasSignaled = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            Assert.False(wasSignaled);
        }
        [Test]
        public void PageConnected_ExistingSession_NewTab()
        {
            var mockHost = new MockNativeMessagingHost();
            var session = new Session(1, "test");
            session.Tabs.Add(new Tab(2, "Google"));
            mockHost.Sessions.Add(session);
            var host = new Host(mockHost);
            var are = new AutoResetEvent(false);

            host.Attached += (s, e) =>
            {
                are.Set();
            };

            host.MessagingHost.ProcessReceivedMessage(JsonConvert.DeserializeObject<JObject>(PageConnectedMessage));
            var wasSignaled = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            Assert.True(wasSignaled);
            Assert.AreEqual(mockHost.MessageSent["message"].ToString(), @"{""message"":""acknowledged"",""pageId"":""d1c2ad50-997f-4ed9-a41e-68dc42f97b92""}");
            Assert.True(host.MessagingHost.Sessions.Count == 1);
            Assert.True(host.MessagingHost.Sessions[0].Tabs.Count == 2);
            Assert.True(host.MessagingHost.Sessions[0].Tabs.Exists(t => t.Id == 1));
            Assert.True(host.MessagingHost.GetSessionForPage("d1c2ad50-997f-4ed9-a41e-68dc42f97b92").SessionId == 1);
        }

        [Test]
        public void PageConnected_NewSession()
        {
            var mockHost = new MockNativeMessagingHost();
            var session = new Session(2, "test");
            mockHost.Sessions.Add(session);
            var host = new Host(mockHost);
            var are = new AutoResetEvent(false);

            host.Attached += (s, e) =>
            {
                are.Set();
            };

            host.MessagingHost.ProcessReceivedMessage(JsonConvert.DeserializeObject<JObject>(PageConnectedMessage));
            var wasSignaled = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            Assert.True(wasSignaled);
            Assert.AreEqual(mockHost.MessageSent["message"].ToString(), @"{""message"":""acknowledged"",""pageId"":""d1c2ad50-997f-4ed9-a41e-68dc42f97b92""}");
            Assert.True(host.MessagingHost.Sessions.Count == 2);
            Assert.True(host.MessagingHost.Sessions.FirstOrDefault(s => s.SessionId == 1).Tabs.Count == 1);
            Assert.True(host.MessagingHost.Sessions.FirstOrDefault(s => s.SessionId == 1).Tabs.Exists(t => t.Id == 1));
            Assert.True(host.MessagingHost.GetSessionForPage("d1c2ad50-997f-4ed9-a41e-68dc42f97b92").SessionId == 1);
        }
        [Test]
        public void PageConnected_ExistingSession_ExistingTab_NewPage()
        {
            var mockHost = new MockNativeMessagingHost();
            var session = new Session(1, "test");
            var tab = new Tab(1, "BBC");
            tab.Pages.Add(Guid.NewGuid());
            session.Tabs.Add(tab);
            mockHost.Sessions.Add(session);
            var host = new Host(mockHost);
            var are = new AutoResetEvent(false);

            host.Attached += (s, e) =>
            {
                are.Set();
            };

            host.MessagingHost.ProcessReceivedMessage(JsonConvert.DeserializeObject<JObject>(PageConnectedMessage));
            var wasSignaled = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            Assert.True(wasSignaled);
            Assert.AreEqual(mockHost.MessageSent["message"].ToString(), @"{""message"":""acknowledged"",""pageId"":""d1c2ad50-997f-4ed9-a41e-68dc42f97b92""}");
            Assert.True(host.MessagingHost.Sessions.Count == 1);
            Assert.True(host.MessagingHost.Sessions[0].Tabs.Count == 1);
            Assert.True(host.MessagingHost.Sessions[0].Tabs.Exists(t => t.Id == 1));
            Assert.True(host.MessagingHost.GetSessionForPage("d1c2ad50-997f-4ed9-a41e-68dc42f97b92").SessionId == 1);
        }

        [Test]
        public void PageDisconnected_PageExistsInTab_PageRemovedFromTab()
        {
            var mockHost = new MockNativeMessagingHost();
            var host = new Host(mockHost);
            var session = new Session(1, "test");
            var tab = new Tab(1, "BBC");
            tab.Pages.Add(Guid.Parse("d1c2ad50-997f-4ed9-a41e-68dc42f97b92"));
            session.Tabs.Add(tab);
            host.MessagingHost.Sessions.Add(session);
            var are = new AutoResetEvent(false);

            host.MessageReceived += (s, e) =>
            {
                are.Set();
            };
            host.MessagingHost.ProcessReceivedMessage(JsonConvert.DeserializeObject<JObject>(PageDisconnectedMessage));
            var wasSignaled = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            Assert.True(wasSignaled);
            Assert.IsNull(host.MessagingHost.GetSessionForPage("d1c2ad50-997f-4ed9-a41e-68dc42f97b92"));
            Assert.IsFalse(host.MessagingHost.Sessions.FirstOrDefault(s => s.SessionId == 1).Tabs.FirstOrDefault(t => t.Id == 1).Pages.Any());
        }

        [Test]
        public void Attached_NoMessageSent_NoBpClientId()
        {
            var mockHost = new MockNativeMessagingHost();
            mockHost.Sessions.Add(CreateBasicSession());
            var host = new Host(mockHost);
            var are = new AutoResetEvent(false);

            host.Attached += (s, e) =>
            {
                are.Set();
            };

            host.MessagingHost.ProcessReceivedMessage(JsonConvert.DeserializeObject<JObject>(PageAttachedMessage.Replace("BPChromeListener-62b89527-9107-4bc7-9167-10b9fee7d2b3", string.Empty)));
            var wasSignaled = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            Assert.False(wasSignaled);
        }


        [TestCase("BBC News", true)]
        [TestCase("BBC*", true)]
        [TestCase("*News", true)]
        [TestCase("*BC N*", true)]
        [TestCase("BB*ws", true)]
        [TestCase("BBC ????", true)]
        [TestCase("Google", false)]
        public void Attached_WildCardTitleMatch(string wildCardTest, bool expectedResult)
        {
            var mockHost = new MockNativeMessagingHost();
            mockHost.Sessions.Add(CreateBasicSession());
            var host = new Host(mockHost);
            var are = new AutoResetEvent(false);

            host.Attached += (s, e) =>
            {
                are.Set();
            };

            host.MessagingHost.ProcessReceivedMessage(JsonConvert.DeserializeObject<JObject>(PageAttachedMessage.Replace("BBC News", wildCardTest)));
            var wasSignaled = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            Assert.AreEqual(wasSignaled, expectedResult);
            Assert.AreEqual(host.MessagingHost.Sessions
                .FirstOrDefault(s => s.BpClientId == "BPChromeListener-62b89527-9107-4bc7-9167-10b9fee7d2b3" && s.TrackingId == "d578b03f-fce4-45bc-a04a-88e21e81b6c1")
                ?.Tabs.FirstOrDefault(t => t.Id == 1)?.Pages.Contains(Guid.Parse("f2475c4e-06ca-42ae-8508-c80917a60d60")) != null, expectedResult);
        }

        [Test]
        public void Attached_ValidBpClientId_NoPages_Retry()
        {
            var mockHost = new MockNativeMessagingHost();
            mockHost.Sessions.Add(CreateBasicSession());
            var host = new Host(mockHost);
            var are = new AutoResetEvent(false);

            host.Attached += (s, e) =>
            {
                are.Set();
            };

            host.MessagingHost.ProcessReceivedMessage(JsonConvert.DeserializeObject<JObject>(PageAttachedMessage.Replace(@"[""f2475c4e-06ca-42ae-8508-c80917a60d60"", ""d1c2ad50-997f-4ed9-a41e-68dc42f97b92""]", "[]")));
            var wasSignaled = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            Assert.False(wasSignaled);
            Assert.AreEqual(mockHost.MessageSent["message"].ToString(), @"{""name"":""Attach"",""message"":""Attach"",""title"":""BBC News"",""trackingId"":""d578b03f-fce4-45bc-a04a-88e21e81b6c1"",""bpClientId"":""BPChromeListener-62b89527-9107-4bc7-9167-10b9fee7d2b3"",""conversationId"":""7134ec82-c64d-40e0-a102-40c8b7b57d63"",""attempt"":2}");
        }

        [Test]
        public void TabRemoved_NoMatchingSessionForTab()
        {
            var mockHost = new MockNativeMessagingHost();
            mockHost.Sessions.Add(CreateBasicSession());
            var host = new Host(mockHost);
            var are = new AutoResetEvent(false);

            host.Detached += (s, e) =>
            {
                are.Set();
            };
            host.MessagingHost.ProcessReceivedMessage(JsonConvert.DeserializeObject<JObject>(TabRemovedMessage));
            var wasSignaled = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            Assert.False(wasSignaled);
            Assert.True(host.MessagingHost.Sessions.Any());
        }

        [Test]
        public void TabRemoved_MatchingSessionForTab_TabRemoved_SessionRemaining()
        {
            var mockHost = new MockNativeMessagingHost();
            mockHost.Sessions.Add(CreateBasicSession());
            var tab = new Tab(2, "Google");
            mockHost.Sessions.FirstOrDefault().Tabs.Add(tab);
            var host = new Host(mockHost);
            var are = new AutoResetEvent(false);

            host.Detached += (s, e) =>
            {
                are.Set();
            };
            host.MessagingHost.ProcessReceivedMessage(JsonConvert.DeserializeObject<JObject>(TabRemovedMessage));
            var wasSignaled = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            Assert.True(wasSignaled);
            Assert.True(host.MessagingHost.Sessions.Any());
            Assert.True(host.MessagingHost.Sessions.First().Tabs.Count == 1);
            Assert.False(host.MessagingHost.Sessions.First().Tabs.Exists(t => t.Id == 2));
        }

        [Test]
        public void TabRemoved_MatchingSessionForTab_TabRemoved_SessionRemoved()
        {
            var mockHost = new MockNativeMessagingHost();
            var session = new Session(1, Guid.Empty.ToString());
            session.Tabs.Add(new Tab(2, "Google"));
            mockHost.Sessions.Add(session);
            var host = new Host(mockHost);
            var are = new AutoResetEvent(false);

            host.Detached += (s, e) =>
            {
                are.Set();
            };
            host.MessagingHost.ProcessReceivedMessage(JsonConvert.DeserializeObject<JObject>(TabRemovedMessage));
            var wasSignaled = are.WaitOne(timeout: TimeSpan.FromSeconds(1));
            Assert.True(wasSignaled);
            Assert.False(host.MessagingHost.Sessions.Any());
        }

        private static Session CreateBasicSession()
        {
            var session = new Session(1, Guid.Empty.ToString());
            var tab = new Tab(1, "BBC News");
            tab.Pages.Add(Guid.Parse("f2475c4e-06ca-42ae-8508-c80917a60d60"));
            tab.Pages.Add(Guid.Parse("d1c2ad50-997f-4ed9-a41e-68dc42f97b92"));
            session.Tabs.Add(tab);
            return session;
        }
    }
}
