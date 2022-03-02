using System;
using System.Collections.Generic;
using System.Linq;
using BluePrism.NativeMessaging.Browser;
using BluePrism.NativeMessaging.Implementations;
using BluePrism.Utilities.Testing;
using NUnit.Framework;
using FluentAssertions;

namespace NativeMessaging.UnitTests
{
    [TestFixture]
    public class BrowserHostTests : UnitTestBase<BrowserHost>
    {
        private readonly List<Session> _sessions = new List<Session>();

        private Session _sessionOne;
        private Session _sessionTwo;
        private Session _sessionThree;
        private Session _sessionFour;
        private Session _sessionFive;

        private string _firstBpClientId = Guid.NewGuid().ToString();
        private string _secondBpClientId = Guid.NewGuid().ToString();

        public override void Setup()
        {
            base.Setup();

            _firstBpClientId = "7033718c-e024-427f-8401-a38bce991fc3";
            _secondBpClientId = "3b5eec62-cd81-4311-8b2a-3d281dd18199";
            _sessions.Clear();
            FillSessions();
            ClassUnderTest.Sessions.Clear();
            ClassUnderTest.Sessions.AddRange(_sessions);

        }

        protected override BrowserHost TestClassConstructor() => new BrowserHost();

        private void FillSessions()
        {
            _sessionOne = new Session(1, _firstBpClientId);
            var tab1 = new Tab(1, "BBC - Home")
            {
                Pages = new List<Guid>
                {
                    Guid.Parse("4d7d1657-19de-4164-b062-2d60b97f026b"),
                    Guid.Parse("2e777a90-a72d-401a-9392-192a0737c5bf"),
                    Guid.Parse("3fd3ae10-7d41-4385-a497-712fcb3721b3")
                }
            };
            var tab2 = new Tab(2, "Google")
            {
                Pages = new List<Guid>
                {
                    Guid.Parse("c0e3e639-196c-4a76-8b3c-34dd1154a900"),
                    Guid.Parse("4dc3807b-fb97-4639-a45a-d75619c5be5e")
                }
            };
            var tab3 = new Tab(3, "Stack Overflow - Where Developers Learn, Share, & Build Careers")
            {
                Pages = new List<Guid>
                {
                    Guid.Parse("afc903d6-e6b6-40bd-acc7-03942a115169")
                }
            };
            _sessionOne.Tabs.AddRange(new List<Tab> {
                tab1,
                tab2,
                tab3
            });

            _sessionOne.TrackingId = "2a09596c-4eb5-4da8-80f1-9592c48868e9";

            _sessions.Add(_sessionOne);

            _sessionTwo = new Session(2, _firstBpClientId);
            tab1 = new Tab(4, "BBC - Home")
            {
                Pages = new List<Guid>
                {
                    Guid.Parse("7e43ea44-34f0-4aff-96da-c517c6ef4685"),
                    Guid.Parse("a882d28b-befd-42aa-bf68-c68a44a32cb7")
                }
            };

            tab2 = new Tab(5, "Reference Source")
            {
                Pages = new List<Guid>
                {
                    Guid.Parse("1592b07d-c34e-4a4c-a23e-731eab4945bc"),
                    Guid.Parse("95211b6d-420b-4215-b538-50439c20ed81"),
                    Guid.Parse("8a236b3b-212c-43c1-87d0-454e73be8931")
                }
            };

            _sessionTwo.Tabs.AddRange(new List<Tab> {
                tab1,
                tab2
            });
            _sessions.Add(_sessionTwo);

            _sessionThree = new Session(3, _firstBpClientId);
            tab1 = new Tab(6, "BBC - Home")
            {
                Pages = new List<Guid>
                {
                    Guid.Parse("b2d20645-dfcb-4d26-b971-79df5596b763")
                }
            };
            _sessionThree.Tabs.Add(tab1);
            _sessions.Add(_sessionThree);

            _sessionFour = new Session(4, _secondBpClientId);
            tab1 = new Tab(7, "BBC - Home")
            {
                Pages = new List<Guid>
                {
                    Guid.Parse("af93b56c-f57f-4b04-bcd5-9602b522fce6"),
                    Guid.Parse("be99d34d-582a-4096-87a2-2a72f58b8866")
                }
            };
            tab2 = new Tab(8, "Google")
            {
                Pages = new List<Guid>
                {
                    Guid.Parse("2e0441ef-6fa6-419a-86af-d0a3d525496c")
                }
            };

            _sessionFour.Tabs.AddRange(new List<Tab> {
                tab1,
                tab2
            });
            _sessions.Add(_sessionFour);

            _sessionFive = new Session(4, Guid.Empty.ToString());
            tab1 = new Tab(7, "BBC - Home")
            {
                Pages = new List<Guid>
                {
                    Guid.Parse("415e6dde-a2d6-472b-929a-aa39ee6b79a6"),
                    Guid.Parse("a617cfb5-e500-41f7-b026-8f899a734fcd")
                }
            };
            tab2 = new Tab(8, "Google")
            {
                Pages = new List<Guid>
                {
                    Guid.Parse("ac78f979-1590-48c6-b55c-eb45ceaf09b7")
                }
            };

            _sessionFive.Tabs.AddRange(new List<Tab> {
                tab1,
                tab2
            });
            _sessions.Add(_sessionFive);

        }

        [Test]
        public void GetSessionForPage_CorrectPageIdForSession()
        {
            var pageId = "4d7d1657-19de-4164-b062-2d60b97f026b";
            var sessionToFind = _sessionOne;
            var foundSession = ClassUnderTest.GetSessionForPage(pageId);
            foundSession.Should().Be(sessionToFind);
        }

        [Test]
        public void GetSessionForPage_IncorrectPageIdForSession()
        {
            var pageId = "9c96adda-e99c-462d-bc27-8d15e2394150";
            try
            {                
                ClassUnderTest.GetSessionForPage(pageId);
            }
            catch (InvalidOperationException e)
            {
                e.Should().Be(new InvalidOperationException($"Invalid Page ID: {pageId}"));
            }
        }

        [Test]
        public void GetSessionForTab_CorrectTabIdForSession()
        {
            var tabId = 3;
            var sessionToFind = _sessionOne;
            var foundSession = ClassUnderTest.GetSessionForTab(tabId);
            foundSession.Should().Be(sessionToFind);
        }

        [Test]
        public void GetSessionForTab_IncorrectTabIdForSession()
        {
            var tabId = 13;
            var sessionToFind = _sessionOne;
            var foundSession = ClassUnderTest.GetSessionForTab(tabId);
            foundSession.Should().NotBe(sessionToFind);
        }

        [Test]
        public void UnassociatePages_CorrectPageList()
        {
            var sessionCount = _sessions.Count(x => x.BpClientId == Guid.Empty.ToString());

            sessionCount.Should().Be(1);

            var sessionThreePages = new List<Guid>();
            foreach (var tab in _sessionThree.Tabs)
            {
                sessionThreePages.AddRange(tab.Pages);
            }

            ClassUnderTest.UnassociatePages(sessionThreePages);

            sessionCount = _sessions.Count(x => x.BpClientId == Guid.Empty.ToString());

            sessionCount.Should().Be(2);
        }

        [Test]
        public void GetPagesForClient_SecondBluePrismClient()
        {
            var sessionFourPages = new List<Guid>();
            foreach (var tab in _sessionFour.Tabs)
            {
                sessionFourPages.AddRange(tab.Pages);
            }
            var foundPages = ClassUnderTest.GetPagesForClient(_secondBpClientId);
            foundPages.Should().BeEquivalentTo(sessionFourPages);
        }

        [Test]
        public void GetPagesForClient_IncorrectClientId()
        {
            var invalidClientId = Guid.NewGuid().ToString();
            var foundPages = ClassUnderTest.GetPagesForClient(invalidClientId);
            foundPages.Should().BeEquivalentTo(new List<Guid>());
        }

        [Test]
        public void GetUnassociatedPages_PagesInSessions()
        {
            var sessionFivePages = new List<Guid>();
            foreach (var tab in _sessionFive.Tabs)
            {
                sessionFivePages.AddRange(tab.Pages);
            }
            var foundPages = ClassUnderTest.GetUnassociatedPages();
            foundPages.Should().BeEquivalentTo(sessionFivePages);
        }

        [Test]
        public void GetUnassociatedPages_PagesNotInSessions()
        {
            ClassUnderTest.Sessions.Remove(_sessionFive);
            var foundPages = ClassUnderTest.GetUnassociatedPages();
            foundPages.Should().BeEquivalentTo(new List<Guid>());
        }

        [Test]
        public void GetPagesForTrackingId_CorrectTrackingId()
        {
            var trackingId = _sessionOne.TrackingId;
            var sessionOnePages = new List<Guid>();
            foreach (var tab in _sessionOne.Tabs)
            {
                sessionOnePages.AddRange(tab.Pages);
            }
            var foundPages = ClassUnderTest.GetPagesForTrackingId(trackingId);
            foundPages.Should().BeEquivalentTo(sessionOnePages);
        }

        [Test]
        public void GetPagesForTrackingId_IncorrectTrackingId()
        {
            var trackingId = Guid.NewGuid().ToString();
            var foundPages = ClassUnderTest.GetPagesForTrackingId(trackingId);
            foundPages.Should().BeEquivalentTo(new List<Guid>());
        }
    }
}
