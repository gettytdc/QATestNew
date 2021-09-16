#if UNITTESTS

namespace BluePrism.BrowserAutomation.UnitTests
{
    using System;
    using BluePrism.Utilities.Testing;
    using Utilities.Functional;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using Cryptography;
    using WebMessages;
    using BluePrism.Core.Configuration;
    using Newtonsoft.Json;
    using BluePrism.BrowserAutomation.Events;
    using System.Collections.Generic;
    using System.Linq;
    using System.Drawing;
    using BluePrism.Core.Utility;
    using BluePrism.BrowserAutomation.Data;
    using BluePrism.BrowserAutomation.NativeMessaging;

    [TestFixture]
    public class WebPageTests : UnitTestBase<NativeMessagingWebPage>
    {
        private Guid _pageId;
        private Func<Guid, IWebPage, IWebElement> _webElementFactory;
        private List<WebMessageWrapper> _sentMessages;
        private Action<WebMessageWrapper> _respondToMessage;
        private Action<WebMessageWrapper> _messageSender;

        public override void Setup()
        {
            base.Setup();

            _pageId = Guid.NewGuid();
            _sentMessages = new List<WebMessageWrapper>();

            _respondToMessage = (message) =>
            {
                var responseMessage = new WebMessage { ConversationId = GetConversationId(message) };
                ClassUnderTest.ReceiveMessage(new WebMessageWrapper(_pageId, JsonConvert.SerializeObject(responseMessage)));
            };

            _messageSender = message => {
                    _sentMessages.Add(message);
                    _respondToMessage(message);
                };

            GetMock<IMessageCryptographyProvider>()
                .Setup(m => m.EncryptMessage(It.IsAny<string>()))
                .Returns(x => x);

            GetMock<IMessageCryptographyProvider>()
                .Setup(m => m.DecryptMessage(It.IsAny<string>()))
                .Returns(x => x);

            _webElementFactory = (id, _) =>
            {
                var element = new Mock<IWebElement>();
                element.Setup(e => e.Id).Returns(id);
                element.Setup(e => e.Page).Returns(ClassUnderTest);

                return element.Object;
            };
        }

        protected override NativeMessagingWebPage TestClassConstructor()
        {
            var iAppSettingsMock = new Mock<IAppSettings>();
            iAppSettingsMock.SetupGet(p => p["BrowserAutomation.DefaultCommunicationTimeout"]).Returns("1000");
            return new NativeMessagingWebPage(
                _pageId,
                _messageSender,
                iAppSettingsMock.Object,
                GetMock<IMessageCryptographyProvider>().Object,
                _webElementFactory);
        }

        private Guid GetConversationId(WebMessageWrapper message) =>
            message.Data
            .Map(JsonConvert.DeserializeObject<WebMessage>)
            .ConversationId;

        [Test]
        public void GetElementByPath_SendsCorrectMessage()
        {
            ClassUnderTest.GetElementByPath("path");

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<GetElementByPathMessage>(message.Data);
            sentMessage.Data.Should().Be("path");
        }

        [Test]
        public void GetElementByPath_FindsElement_GetsElementWithCorrectId()
        {
            var elementId = Guid.NewGuid();

            _respondToMessage = message =>
            {
                var response = new GetElementByPathResponse {
                    ConversationId = GetConversationId(message),
                    Data = elementId
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x => new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            ClassUnderTest.GetElementByPath("path").Id.ShouldBeEquivalentTo(elementId);
        }

        [Test]
        public void GetElementByPath_DoesNotFindElement_GetsNoElement()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementByPathResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x => new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            ClassUnderTest.GetElementByPath("path").Should().BeNull();
        }

        [Test]
        public void GetElementByPath_NoResponse_TimesOut()
        {
            _respondToMessage = _ => { };

            Action getElement = () => ClassUnderTest.GetElementByPath("path");
            getElement.ShouldThrow<TimeoutException>();
        }

        [Test]
        public void GetElementById_SendsCorrectMessage()
        {
            ClassUnderTest.GetElementById("id");

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<GetElementByIdMessage>(message.Data);
            sentMessage.Data.Should().Be("id");
        }

        [Test]
        public void GetElementById_FindsElement_GetsElementWithCorrectId()
        {
            var elementId = Guid.NewGuid();

            _respondToMessage = message =>
            {
                var response = new GetElementByIdResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = elementId
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x => new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            ClassUnderTest.GetElementById("id").Id.ShouldBeEquivalentTo(elementId);
        }

        [Test]
        public void GetElementById_DoesNotFindElement_GetsNoElement()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementByIdResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x => new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            ClassUnderTest.GetElementById("id").Should().BeNull();
        }

        [Test]
        public void GetElementById_NoResponse_TimesOut()
        {
            _respondToMessage = _ => { };

            Action getElement = () => ClassUnderTest.GetElementById("id");
            getElement.ShouldThrow<TimeoutException>();
        }

        [Test]
        public void GetRootElement_SendsCorrectMessage()
        {
            ClassUnderTest.GetRootElement();

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<GetRootElementMessage>(message.Data);
            sentMessage.Data.Should().BeNull();
        }

        [Test]
        public void GetRootElement_FindsElement_GetsElementWithCorrectId()
        {
            var elementId = Guid.NewGuid();

            _respondToMessage = message =>
            {
                var response = new GetRootElementResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = elementId
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x => new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            ClassUnderTest.GetRootElement().Id.ShouldBeEquivalentTo(elementId);
        }

        [Test]
        public void GetRootElement_DoesNotFindElement_GetsNoElement()
        {
            _respondToMessage = message =>
            {
                var response = new GetRootElementResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x => new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            ClassUnderTest.GetRootElement().Id.Should().Be(Guid.Empty);
        }

        [Test]
        public void GetRootElement_NoResponse_TimesOut()
        {
            _respondToMessage = _ => { };

            Action getElement = () => ClassUnderTest.GetRootElement();
            getElement.ShouldThrow<TimeoutException>();
        }

        [Test]
        public void GetAddress_SendsCorrectMessage()
        {
            ClassUnderTest.GetAddress();

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<GetAddressMessage>(message.Data);
            sentMessage.Data.Should().BeNull();
        }

        [Test]
        public void GetAddress_FindsElement_GetsElementWithCorrectId()
        {
            var elementId = Guid.NewGuid();

            _respondToMessage = message =>
            {
                var response = new GetAddressResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = "address"
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x => new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            ClassUnderTest.GetAddress().ShouldBeEquivalentTo("address");
        }

        [Test]
        public void GetAddress_DoesNotFindElement_GetsNoElement()
        {
            _respondToMessage = message =>
            {
                var response = new GetAddressResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x => new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            ClassUnderTest.GetAddress().Should().BeNull();
        }

        [Test]
        public void GetAddress_NoResponse_TimesOut()
        {
            _respondToMessage = _ => { };

            Action getElement = () => ClassUnderTest.GetAddress();
            getElement.ShouldThrow<TimeoutException>();
        }

        [Test]
        public void GetElementAttribute_SendsCorrectMessage()
        {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            ClassUnderTest.GetElementAttribute(webElement, "attr");

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<GetElementAttributeMessage>(message.Data);
            sentMessage.Data.ShouldBeEquivalentTo(new GetElementAttributeMessageBody
            {
                ElementId = elementId,
                AttributeName = "attr"
            });
        }

        [Test]
        public void GetElementAttribute_FindsElement_GetsElementWithCorrectId()
        {
            var elementId = Guid.NewGuid();

            _respondToMessage = message =>
            {
                var response = new GetElementAttributeResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = "address"
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x => new WebMessageWrapper(_pageId,x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementAttribute(webElement, "attr").ShouldBeEquivalentTo("address");
        }

        [Test]
        public void GetElementAttribute_DoesNotFindElement_GetsNoElement()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementAttributeResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x => new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementAttribute(webElement, "attr").Should().BeEmpty();
        }

        [Test]
        public void GetElementAttribute_NoResponse_TimesOut()
        {
            _respondToMessage = _ => { };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            Action getElement = () => ClassUnderTest.GetElementAttribute(webElement, "attr");
            getElement.ShouldThrow<TimeoutException>();
        }

        [Test]
        public void GetElementsByType_SendsCorrectMessage()
        {
            ClassUnderTest.GetElementsByType("type");

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<GetElementsByTypeMessage>(message.Data);
            sentMessage.Data.Should().Be("type");
        }

        [Test]
        public void GetElementsByType_FindsElements_GetsElementWithCorrectIds()
        {
            var elementIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

            _respondToMessage = message =>
            {
                var response = new GetElementsByTypeResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = elementIds
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x => new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            ClassUnderTest.GetElementsByType("type").Select(e => e.Id).ShouldBeEquivalentTo(elementIds);
        }

        [Test]
        public void GetElementsByType_DoesNotFindElements_GetsNoElements()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementsByTypeResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x => new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            ClassUnderTest.GetElementsByType("type").Should().BeEmpty();
        }

        [Test]
        public void GetElementsByType_NoResponse_TimesOut()
        {
            _respondToMessage = _ => { };

            Action getElement = () => ClassUnderTest.GetElementsByType("type");
            getElement.ShouldThrow<TimeoutException>();
        }

        [Test]
        public void GetElementsByClass_SendsCorrectMessage()
        {
            ClassUnderTest.GetElementsByClass("class");

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<GetElementsByClassMessage>(message.Data);
            sentMessage.Data.Should().Be("class");
        }

        [Test]
        public void GetElementsByClass_FindsElements_GetsElementWithCorrectIds()
        {
            var elementIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

            _respondToMessage = message =>
            {
                var response = new GetElementsByClassResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = elementIds
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x => new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            ClassUnderTest.GetElementsByClass("class").Select(e => e.Id).ShouldBeEquivalentTo(elementIds);
        }

        [Test]
        public void GetElementsByClass_DoesNotFindElements_GetsNoElements()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementsByClassResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x => new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            ClassUnderTest.GetElementsByClass("class").Should().BeEmpty();
        }

        [Test]
        public void GetElementsByClass_NoResponse_TimesOut()
        {
            _respondToMessage = _ => { };

            Action getElement = () => ClassUnderTest.GetElementsByClass("type");
            getElement.ShouldThrow<TimeoutException>();
        }

        [Test]
        public void GetElementsByName_SendsCorrectMessage()
        {
            ClassUnderTest.GetElementsByName("name");

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<GetElementsByNameMessage>(message.Data);
            sentMessage.Data.Should().Be("name");
        }

        [Test]
        public void GetElementsByName_FindsElements_GetsElementWithCorrectIds()
        {
            var elementIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

            _respondToMessage = message =>
            {
                var response = new GetElementsByNameResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = elementIds
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x => new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            ClassUnderTest.GetElementsByName("name").Select(e => e.Id).ShouldBeEquivalentTo(elementIds);
        }

        [Test]
        public void GetElementsByName_DoesNotFindElements_GetsNoElements()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementsByNameResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x => new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            ClassUnderTest.GetElementsByName("name").Should().BeEmpty();
        }

        [Test]
        public void GetElementsByName_NoResponse_TimesOut()
        {
            _respondToMessage = _ => { };

            Action getElement = () => ClassUnderTest.GetElementsByName("name");
            getElement.ShouldThrow<TimeoutException>();
        }

        [Test]
        public void GetElementDescendants_SendsCorrectMessage()
        {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            ClassUnderTest.GetElementDescendants(webElement);

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<GetDescendantsMessage>(message.Data);
            sentMessage.Data.Should().Be(elementId);
        }

        [Test]
        public void GetElementDescendants_FindsElements_GetsElementWithCorrectIds()
        {
            var elementIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

            _respondToMessage = message =>
            {
                var response = new GetDescendantsResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = elementIds
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x => new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementDescendants(webElement)
                .Select(e => e.Id).ShouldBeEquivalentTo(elementIds);
        }

        [Test]
        public void GetElementDescendants_DoesNotFindElements_GetsNoElements()
        {
            _respondToMessage = message =>
            {
                var response = new GetDescendantsResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x => new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementDescendants(webElement).Should().BeEmpty();
        }

        [Test]
        public void GetElementDescendants_NoResponse_TimesOut()
        {
            _respondToMessage = _ => { };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            Action getElement = () => ClassUnderTest.GetElementDescendants(webElement);
            getElement.ShouldThrow<TimeoutException>();
        }

        [Test]
        public void GetElementBounds_SendsCorrectMessage()
        {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            ClassUnderTest.GetElementBounds(webElement);

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<GetElementBoundsMessage>(message.Data);
            sentMessage.Data.Should().Be(elementId);
        }

        [Test]
        public void GetElementBounds_FindsElement_GetsCorrectBounds()
        {
            var bounds = new Rectangle(0, 10, 20, 25);
            _respondToMessage = message =>
            {
                var response = new GetElementBoundsResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = new BoundsMessageBody() { X = 0, Y = 10, Width = 20, Height = 25 }
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x => new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementBounds(webElement).ShouldBeEquivalentTo(bounds);
        }

        [Test]
        public void GetElementBounds_DoesNotFindElements_GetsNoElements()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementBoundsResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementBounds(webElement).ShouldBeEquivalentTo(new Rectangle(-1, -1, 0, 0));
        }

        [Test]
        public void GetElementBounds_NoResponse_TimesOut()
            => GetBounds_NoResponse_TimesOut(ClassUnderTest.GetElementBounds);

        [Test]
        public void GetElementClientBounds_SendsCorrectMessage()
        {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            ClassUnderTest.GetElementClientBounds(webElement);

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<GetElementClientBoundsMessage>(message.Data);
            sentMessage.Data.Should().Be(elementId);
        }

        [Test]
        public void GetElementClientBounds_FindsElement_GetsCorrectBounds()
        {
            var bounds = new Rectangle(0, 10, 20, 25);
            _respondToMessage = message =>
            {
                var response = new GetElementClientBoundsResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = new BoundsMessageBody() { X = 0, Y = 10, Width = 20, Height = 25 }
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementClientBounds(webElement).ShouldBeEquivalentTo(bounds);
        }

        [Test]
        public void GetElementClientBounds_DoesNotFindElements_GetsNoElements()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementClientBoundsResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementClientBounds(webElement).ShouldBeEquivalentTo(new Rectangle(-1, -1, 0, 0));
        }

        [Test]
        public void GetElementClientBounds_NoResponse_TimesOut()
            => GetBounds_NoResponse_TimesOut(ClassUnderTest.GetElementClientBounds);

        [Test]
        public void GetElementOffsetBounds_SendsCorrectMessage()
        {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            ClassUnderTest.GetElementOffsetBounds(webElement);

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<GetElementOffsetBoundsMessage>(message.Data);
            sentMessage.Data.Should().Be(elementId);
        }

        [Test]
        public void GetElementOffsetBounds_FindsElement_GetsCorrectBounds()
        {
            var bounds = new Rectangle(0, 10, 20, 25);
            _respondToMessage = message =>
            {
                var response = new GetElementOffsetBoundsResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = new BoundsMessageBody() { X = 0, Y = 10, Width = 20, Height = 25 }
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementOffsetBounds(webElement).ShouldBeEquivalentTo(bounds);
        }

        [Test]
        public void GetElementOffsetBounds_DoesNotFindElements_GetsNoElements()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementOffsetBoundsResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementOffsetBounds(webElement).ShouldBeEquivalentTo(new Rectangle(-1, -1, 0, 0));
        }

        [Test]
        public void GetElementOffsetBounds_NoResponse_TimesOut()
            => GetBounds_NoResponse_TimesOut(ClassUnderTest.GetElementOffsetBounds);

        [Test]
        public void GetElementScrollBounds_SendsCorrectMessage()
        {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            ClassUnderTest.GetElementScrollBounds(webElement);

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<GetElementScrollBoundsMessage>(message.Data);
            sentMessage.Data.Should().Be(elementId);
        }

        [Test]
        public void GetElementScrollBounds_FindsElement_GetsCorrectBounds()
        {
            var bounds = new Rectangle(0, 10, 20, 25);
            _respondToMessage = message =>
            {
                var response = new GetElementScrollBoundsResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = new BoundsMessageBody() { X = 0, Y = 10, Width = 20, Height = 25 }
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementScrollBounds(webElement).ShouldBeEquivalentTo(bounds);
        }

        [Test]
        public void GetElementScrollBounds_DoesNotFindElements_GetsNoElements()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementScrollBoundsResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementScrollBounds(webElement).ShouldBeEquivalentTo(new Rectangle(-1, -1, 0, 0));
        }

        [Test]
        public void GetElementScrollBounds_NoResponse_TimesOut()
            => GetBounds_NoResponse_TimesOut(ClassUnderTest.GetElementScrollBounds);

        private void GetBounds_NoResponse_TimesOut(Func<IWebElement, Rectangle> getBounds)
        {
            _respondToMessage = _ => { };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            Action getElement = () => getBounds(webElement);
            getElement.ShouldThrow<TimeoutException>();
        }

        [Test]
        public void GetCursorPosition_SendsCorrectMessage()
        {
            ClassUnderTest.GetCursorPosition();

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<GetCursorPositionMessage>(message.Data);
            sentMessage.Data.Should().BeNull();
        }

        [Test]
        public void GetCursorPosition_FindsElement_GetsCorrectPoint()
        {
            var point = new Point(11, 12);
            _respondToMessage = message =>
            {
                var response = new GetCursorPositionResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = new GetCursorPositionResponseBody() { X = 11, Y = 12 }
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            ClassUnderTest.GetCursorPosition().ShouldBeEquivalentTo(point);
        }

        [Test]
        public void GetCursorPosition_DoesNotFindElements_GetsDefaultPoint()
        {
            _respondToMessage = message =>
            {
                var response = new GetCursorPositionResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            ClassUnderTest.GetCursorPosition().ShouldBeEquivalentTo(new Point(-1, -1));
        }

        [Test]
        public void GetCursorPosition_NoResponse_TimesOut()
        {
            _respondToMessage = _ => { };

            Action getElement = () => ClassUnderTest.GetCursorPosition();
            getElement.ShouldThrow<TimeoutException>();
        }

        [Test]
        public void GetElementId_SendsCorrectMessage() =>
            GetElementValue_SendsCorrectMessage<GetElementIdMessage>(ClassUnderTest.GetElementId);

        [Test]
        public void GetElementId_FindsElement_GetsCorrectResponse()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementIdResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = "test"
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementId(webElement).ShouldBeEquivalentTo("test");
        }

        [Test]
        public void GetElementId_DoesNotFindElements_GetsNoValue()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementIdResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementId(webElement).Should().BeNull();
        }

        [Test]
        public void GetElementId_NoResponse_TimesOut()
            => GetElementValue_NoResponse_TimesOut(ClassUnderTest.GetElementId);

        [Test]
        public void GetElementName_SendsCorrectMessage() =>
            GetElementValue_SendsCorrectMessage<GetElementNameMessage>(ClassUnderTest.GetElementName);

        [Test]
        public void GetElementHtml_SendsCorrectMessage() =>
            GetElementValue_SendsCorrectMessage<GetElementHtmlMessage>(ClassUnderTest.GetElementHtml);

        [Test]
        public void GetElementName_FindsElement_GetsCorrectResponse()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementNameResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = "test"
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementName(webElement).ShouldBeEquivalentTo("test");
        }

        [Test]
        public void GetElementName_DoesNotFindElements_GetsNoValue()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementNameResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementName(webElement).Should().BeNull();
        }

        [Test]
        public void GetElementName_NoResponse_TimesOut()
            => GetElementValue_NoResponse_TimesOut(ClassUnderTest.GetElementName);

        [Test]
        public void GetSelectedText_SendsCorrectMessage() =>
            GetElementValue_SendsCorrectMessage<GetElementSelectedTextMessage>(ClassUnderTest.GetSelectedText);

        [Test]
        public void GetSelectedText_FindsElement_GetsCorrectResponse()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementSelectedTextResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = "test"
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetSelectedText(webElement).ShouldBeEquivalentTo("test");
        }

        [Test]
        public void GetSelectedText_DoesNotFindElements_GetsNoValue()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementSelectedTextResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetSelectedText(webElement).Should().BeNull();
        }

        [Test]
        public void GetSelectedText_NoResponse_TimesOut()
            => GetElementValue_NoResponse_TimesOut(ClassUnderTest.GetSelectedText);

        [Test]
        public void GetTableItemText_SendsCorrectMessage()
        {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            ClassUnderTest.GetTableItemText(webElement, 1, 2);

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<GetElementTableItemTextMessage>(message.Data);
            sentMessage.Data.ShouldBeEquivalentTo(new GetElementTableItemTextMessageBody()
            {
                ElementId = elementId,
                RowNumber = 1,
                ColumnNumber = 2
            });
        }

        [Test]
        public void GetTableItemText_FindsElement_GetsCorrectResponse()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementTableItemTextResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = "test"
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetTableItemText(webElement, 1, 2).ShouldBeEquivalentTo("test");
        }

        [Test]
        public void GetTableItemText_DoesNotFindElements_GetsNoValue()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementTableItemTextResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetTableItemText(webElement, 1, 2).Should().BeNull();
        }

        [Test]
        public void GetTableItemText_NoResponse_TimesOut()
            => GetElementValue_NoResponse_TimesOut(e => ClassUnderTest.GetTableItemText(e, 1, 2));

        [Test]
        public void GetLinkAddressText_SendsCorrectMessage() =>
            GetElementValue_SendsCorrectMessage<GetLinkAddressTextMessage>(ClassUnderTest.GetLinkAddressText);

        [Test]
        public void GetLinkAddressText_FindsElement_GetsCorrectResponse()
        {
            _respondToMessage = message =>
            {
                var response = new GetLinkAddressTextMessageResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = "test"
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetLinkAddressText(webElement).ShouldBeEquivalentTo("test");
        }

        [Test]
        public void GetLinkAddressText_DoesNotFindElements_GetsNoValue()
        {
            _respondToMessage = message =>
            {
                var response = new GetLinkAddressTextMessageResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetLinkAddressText(webElement).Should().BeNull();
        }

        [Test]
        public void GetLinkAddressText_NoResponse_TimesOut()
            => GetElementValue_NoResponse_TimesOut(ClassUnderTest.GetLinkAddressText);

        [Test]
        public void GetElementType_SendsCorrectMessage() =>
            GetElementValue_SendsCorrectMessage<GetElementTypeMessage>(ClassUnderTest.GetElementType);

        [Test]
        public void GetElementType_FindsElement_GetsCorrectResponse()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementTypeResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = "test"
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementType(webElement).ShouldBeEquivalentTo("test");
        }

        [Test]
        public void GetElementType_DoesNotFindElements_GetsNoValue()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementTypeResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementType(webElement).Should().BeNull();
        }

        [Test]
        public void GetElementType_NoResponse_TimesOut()
            => GetElementValue_NoResponse_TimesOut(ClassUnderTest.GetElementType);

        [Test]
        public void GetElementClass_SendsCorrectMessage() =>
            GetElementValue_SendsCorrectMessage<GetElementClassMessage>(ClassUnderTest.GetElementClass);

        [Test]
        public void GetElementClass_FindsElement_GetsCorrectResponse()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementClassResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = "test"
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementClass(webElement).ShouldBeEquivalentTo("test");
        }

        [Test]
        public void GetElementClass_DoesNotFindElements_GetsNoValue()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementClassResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementClass(webElement).Should().BeNull();
        }

        [Test]
        public void GetElementClass_NoResponse_TimesOut()
            => GetElementValue_NoResponse_TimesOut(ClassUnderTest.GetElementClass);

        [Test]
        public void GetElementPath_SendsCorrectMessage() =>
            GetElementValue_SendsCorrectMessage<GetElementPathMessage>(ClassUnderTest.GetElementPath);

        [Test]
        public void GetElementPath_FindsElement_GetsCorrectResponse()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementPathResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = "test"
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementPath(webElement).ShouldBeEquivalentTo("test");
        }

        [Test]
        public void GetElementPath_DoesNotFindElements_GetsNoValue()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementPathResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementPath(webElement).Should().BeNull();
        }

        [Test]
        public void GetElementPath_NoResponse_TimesOut()
            => GetElementValue_NoResponse_TimesOut(ClassUnderTest.GetElementPath);

        [Test]
        public void GetElementValue_SendsCorrectMessage() =>
            GetElementValue_SendsCorrectMessage<GetElementValueMessage>(ClassUnderTest.GetElementValue);

        [Test]
        public void GetElementValue_FindsElement_GetsCorrectResponse()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementValueResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = "test"
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementValue(webElement).ShouldBeEquivalentTo("test");
        }

        [Test]
        public void GetElementValuet_DoesNotFindElements_GetsNoValue()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementValueResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementValue(webElement).Should().BeNull();
        }

        [Test]
        public void GetElementValue_NoResponse_TimesOut()
            => GetElementValue_NoResponse_TimesOut(ClassUnderTest.GetElementValue);

        [Test]
        public void GetElementText_SendsCorrectMessage() =>
            GetElementValue_SendsCorrectMessage<GetElementTextMessage>(ClassUnderTest.GetElementText);

        [Test]
        public void GetElementText_FindsElement_GetsCorrectResponse()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementTextResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = "test"
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementText(webElement).ShouldBeEquivalentTo("test");
        }

        [Test]
        public void GetElementText_DoesNotFindElements_GetsNoValue()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementTextResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementText(webElement).Should().BeNull();
        }

        [Test]
        public void GetElementHtml_FindsElement_GetsCorrectResponse()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementHtmlResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = "<h1>test</h1>"
                };

                response
                    .Map(JsonConvert.SerializeObject)
                    .Map(x =>new WebMessageWrapper(_pageId, x))
                    .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementHtml(webElement).ShouldBeEquivalentTo("<h1>test</h1>");
        }

        [Test]
        public void GetElementHtml_DoesNotFindElements_GetsNoValue()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementHtmlResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                    .Map(JsonConvert.SerializeObject)
                    .Map(x =>new WebMessageWrapper(_pageId, x))
                    .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementHtml(webElement).Should().BeNull();
        }

        [Test]
        public void GetElementText_NoResponse_TimesOut()
            => GetElementValue_NoResponse_TimesOut(ClassUnderTest.GetElementText);

        [Test]
        public void GetElementHtml_NoResponse_TimesOut()
            => GetElementValue_NoResponse_TimesOut(ClassUnderTest.GetElementHtml);

        [Test]
        public void GetElementStyle_SendsCorrectMessage() =>
            GetElementValue_SendsCorrectMessage<GetElementStyleMessage>(ClassUnderTest.GetElementStyle);

        [Test]
        public void GetElementStyle_FindsElement_GetsCorrectResponse()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementStyleResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = "test"
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementStyle(webElement).ShouldBeEquivalentTo("test");
        }

        [Test]
        public void GetElementStyle_DoesNotFindElements_GetsNoValue()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementStyleResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementStyle(webElement).Should().BeNull();
        }

        [Test]
        public void GetElementStyle_NoResponse_TimesOut()
            => GetElementValue_NoResponse_TimesOut(ClassUnderTest.GetElementStyle);

        [Test]
        public void GetElementLabel_SendsCorrectMessage() =>
            GetElementValue_SendsCorrectMessage<GetElementLabelMessage>(ClassUnderTest.GetElementLabel);

        [Test]
        public void GetElementLabel_FindsElement_GetsCorrectResponse()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementLabelResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = "test"
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementLabel(webElement).ShouldBeEquivalentTo("test");
        }

        [Test]
        public void GetElementLabel_DoesNotFindElements_GetsNoValue()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementLabelResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementLabel(webElement).Should().BeNull();
        }

        [Test]
        public void GetElementLabel_NoResponse_TimesOut()
            => GetElementValue_NoResponse_TimesOut(ClassUnderTest.GetElementLabel);

        [Test]
        public void GetElementAccessKey_SendsCorrectMessage() =>
            GetElementValue_SendsCorrectMessage<GetElementAccessKeyMessage>(ClassUnderTest.GetElementAccessKey);

        [Test]
        public void GetElementAccessKey_FindsElement_GetsCorrectResponse()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementAccessKeyResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = "test"
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementAccessKey(webElement).ShouldBeEquivalentTo("test");
        }

        [Test]
        public void GetElementAccessKey_DoesNotFindElements_GetsNoValue()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementAccessKeyResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementAccessKey(webElement).Should().BeNull();
        }

        [Test]
        public void GetElementAccessKey_NoResponse_TimesOut()
            => GetElementValue_NoResponse_TimesOut(ClassUnderTest.GetElementAccessKey);

        private void GetElementValue_SendsCorrectMessage<T>(Func<IWebElement, string> getValue) where T : WebMessage<Guid>
        {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            getValue(webElement);

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<T>(message.Data);
            sentMessage.Data.Should().Be(elementId);
        }

        private void GetElementValue_NoResponse_TimesOut(Func<IWebElement, string> getValue)
        {
            _respondToMessage = _ => { };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            Action getElementValue = () => getValue(webElement);
            getElementValue.ShouldThrow<TimeoutException>();
        }

        [Test]
        public void SetElementValue_SendsCorrectMessage()
        {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            ClassUnderTest.SetElementValue(webElement, "value");

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<SetElementValueMessage>(message.Data);
            sentMessage.Data.ShouldBeEquivalentTo(new SetElementValueMessageBody
            {
                ElementId = elementId,
                Value = "value"
            });
        }

        [Test]
        public void SetCheckedState_SendsCorrectMessage()
        {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            ClassUnderTest.SetCheckedState(webElement, true);

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<SetElementCheckedStateMessage>(message.Data);
            sentMessage.Data.ShouldBeEquivalentTo(new SetElementCheckedStateMessageBody
            {
                ElementId = elementId,
                Value = true
            });
        }

        [Test]
        public void ClickElement_SendsCorrectMessage()
        {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            ClassUnderTest.ClickElement(webElement);

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<ClickElementMessage>(message.Data);
            sentMessage.Data.ShouldBeEquivalentTo(elementId);
        }

        [Test]
        public void DoubleClickElement_SendsCorrectMessage()
        {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            ClassUnderTest.DoubleClickElement(webElement);

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<DoubleClickElementMessage>(message.Data);
            sentMessage.Data.ShouldBeEquivalentTo(elementId);
        }

        [Test]
        public void FocusElement_SendsCorrectMessage()
        {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            ClassUnderTest.FocusElement(webElement);

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<FocusElementMessage>(message.Data);
            sentMessage.Data.ShouldBeEquivalentTo(elementId);
        }

        [Test]
        public void FocusHover_SendsCorrectMessage()
        {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            ClassUnderTest.HoverElement(webElement);

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<HoverElementMessage>(message.Data);
            sentMessage.Data.ShouldBeEquivalentTo(elementId);
        }

        [Test]
        public void NavigateTo_SendsCorrectMessage()
        {
            ClassUnderTest.NavigateTo("value");

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<SetAddressMessage>(message.Data);
            sentMessage.Data.ShouldBeEquivalentTo("value");
        }

        [Test]
        public void HighlightElement_SendsCorrectMessage()
        {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            ClassUnderTest.HighlightElement(webElement, Color.Red);

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<HighlightElementMessage>(message.Data);
            sentMessage.Data.ShouldBeEquivalentTo(new HighlightElementMessageBody
            {
                ElementId = elementId,
                Color = Color.Red.ToWebColor()
            });
        }

        [Test]
        public void RemoveHighlighting_SendsCorrectMessage()
        {
            ClassUnderTest.RemoveHighlighting();

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<RemoveHighlightingMessage>(message.Data);
            sentMessage.Data.Should().BeNull();
        }

        [Test]
        public void SelectElement_SendsCorrectMessage()
        {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            ClassUnderTest.SelectElement(webElement);

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<SelectElementMessage>(message.Data);
            sentMessage.Data.Should().Be(elementId);
        }

        [Test]
        public void SetAttributeElement_SendsCorrectMessage()
        {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            ClassUnderTest.SetAttribute(webElement, "attr", "value");

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<SetElementAttributeMessage>(message.Data);
            sentMessage.Data.ShouldBeEquivalentTo(new SetElementAttributeMessageBody
            {
                ElementId = elementId,
                AttributeName = "attr",
                Value = "value"
            });
        }

        [Test]
        public void ScrollToElement_SendsCorrectMessage()
        {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            ClassUnderTest.ScrollToElement(webElement);

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<ScrollToElementMessage>(message.Data);
            sentMessage.Data.ShouldBeEquivalentTo(elementId);
        }

        [Test]
        public void SubmitElement_SendsCorrectMessage()
        {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            ClassUnderTest.SubmitElement(webElement);

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<SubmitElementMessage>(message.Data);
            sentMessage.Data.ShouldBeEquivalentTo(elementId);
        }

        [Test]
        public void UpdateCookie_SendsCorrectMessage()
        {
            ClassUnderTest.UpdateCookie("update");

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<UpdateCookieMessage>(message.Data);
            sentMessage.Data.ShouldBeEquivalentTo("update");
        }

        [Test]
        public void SelectTextRange_SendsCorrectMessage()
        {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            ClassUnderTest.SelectTextRange(webElement,1,3);

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<SetElementSelectedTextRange>(message.Data);
            sentMessage.Data.ShouldBeEquivalentTo(new SetElementSelectedTextBody
            {
                ElementId = elementId,
                StartIndex = 1,
                Length = 3
            });
        }

        [Test]
        public void SelectListItem_SendsCorrectMessage()
        {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            ClassUnderTest.SelectListItem(webElement, 1, "name");

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<SelectListItemMessage>(message.Data);
            sentMessage.Data.ShouldBeEquivalentTo(new SelectListItemMessageBody
            {
                ElementId = elementId,
                Index = 1,
                Name = "name"
            });
        }

        [Test]
        public void AddToListSelection_SendsCorrectMessage()
        {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            ClassUnderTest.AddToListSelection(webElement, 1, "name");

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<AddToListSelectionMessage>(message.Data);
            sentMessage.Data.ShouldBeEquivalentTo(new AddToListSelectionMessageBody
            {
                ElementId = elementId,
                Index = 1,
                Name = "name"
            });
        }

        [Test]
        public void RemoveFromListSelection_SendsCorrectMessage()
        {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            ClassUnderTest.RemoveFromListSelection(webElement, 1, "name");

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<RemoveFromListSelectionMessage>(message.Data);
            sentMessage.Data.ShouldBeEquivalentTo(new RemoveFromListSelectionMessageBody
            {
                ElementId = elementId,
                Index = 1,
                Name = "name"
            });
        }

        [Test]
        public void InjectJavascript_SendsCorrectMessage()
        {
            ClassUnderTest.InjectJavascript("code");

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<InjectJavascriptMessage>(message.Data);
            sentMessage.Data.ShouldBeEquivalentTo("code");
        }

        [Test]
        public void InvokeJavascript_SendsCorrectMessage()
        {
            ClassUnderTest.InvokeJavascript("function", "params");

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<InvokeJavascriptMessage>(message.Data);
            sentMessage.Data.ShouldBeEquivalentTo(new InvokeJavascriptMessageBody
            {
                FunctionName = "function",
                Parameters = "params"
            });
        }

        [Test]
        public void GetElementChildCount_SendsCorrectMessage() =>
            GetCount_SendsCorrectMessage<GetElementChildCountMessage>(ClassUnderTest.GetElementChildCount);
        
        [Test]
        public void GetElementChildCount_FindsElement_GetsCorrectBounds()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementChildCountResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = 5
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementChildCount(webElement).ShouldBeEquivalentTo(5);
        }

        [Test]
        public void GetElementChildCount_DoesNotFindElements_GetsNoElements()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementChildCountResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementChildCount(webElement).ShouldBeEquivalentTo(0);
        }

        [Test]
        public void GetElementChildCount_NoResponse_TimesOut()
            => GetCount_NoResponse_TimesOut(ClassUnderTest.GetElementChildCount);

        [Test]
        public void GetColumnCount_SendsCorrectMessage() =>
            GetCount_SendsCorrectMessage<GetTableColumnCountMessage>(ClassUnderTest.GetColumnCount);

        [Test]
        public void GetColumnCount_FindsElement_GetsCorrectBounds()
        {
            _respondToMessage = message =>
            {
                var response = new GetTableRowCountResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = 5
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetColumnCount(webElement, 0).ShouldBeEquivalentTo(5);
        }

        [Test]
        public void GetColumnCount_DoesNotFindElements_GetsNoElements()
        {
            _respondToMessage = message =>
            {
                var response = new GetTableRowCountResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetColumnCount(webElement, 0).ShouldBeEquivalentTo(0);
        }

        [Test]
        public void GetColumnCount_NoResponse_TimesOut()
            => GetCount_NoResponse_TimesOut(ClassUnderTest.GetColumnCount);

        [Test]
        public void GetRowCount_SendsCorrectMessage() =>
            GetCount_SendsCorrectMessage<GetTableRowCountMessage>(ClassUnderTest.GetRowCount);

        [Test]
        public void GetRowCount_FindsElement_GetsCorrectBounds()
        {
            _respondToMessage = message =>
            {
                var response = new GetTableRowCountResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = 5
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetRowCount(webElement).ShouldBeEquivalentTo(5);
        }

        [Test]
        public void GetRowCount_DoesNotFindElements_GetsNoElements()
        {
            _respondToMessage = message =>
            {
                var response = new GetTableRowCountResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetRowCount(webElement).ShouldBeEquivalentTo(0);
        }

        [Test]
        public void GetRowCount_NoResponse_TimesOut()
            => GetCount_NoResponse_TimesOut(ClassUnderTest.GetRowCount);

        [Test]
        public void GetElementTabIndex_SendsCorrectMessage() =>
            GetCount_SendsCorrectMessage<GetElementTabIndexMessage>(ClassUnderTest.GetElementTabIndex);

        [Test]
        public void GetElementTabIndex_FindsElement_GetsCorrectBounds()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementTabIndexResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = 5
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementTabIndex(webElement).ShouldBeEquivalentTo(5);
        }

        [Test]
        public void GetElementTabIndex_DoesNotFindElements_GetsNoElements()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementTabIndexResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementTabIndex(webElement).ShouldBeEquivalentTo(0);
        }

        [Test]
        public void GetElementTabIndex_NoResponse_TimesOut()
            => GetCount_NoResponse_TimesOut(ClassUnderTest.GetElementTabIndex);

        private void GetCount_SendsCorrectMessage<T>(Func<IWebElement, int, int> getCount) where T : WebMessage<GetTableColumnCountMessageBody>
        {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            getCount(webElement, 1234);

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<T>(message.Data);
            sentMessage.Data.ElementId.Should().Be(elementId);
            sentMessage.Data.RowIndex.Should().Be(1234);
        }

        private void GetCount_SendsCorrectMessage<T>(Func<IWebElement, int> getCount) where T : WebMessage<Guid>
        {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            getCount(webElement);

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<T>(message.Data);
            sentMessage.Data.Should().Be(elementId);
        }

        private void GetCount_NoResponse_TimesOut(Func<IWebElement, int, int> getCount)
        {
            GetCount_NoResponse_TimesOut(e => getCount(e, 0));
        }

        private void GetCount_NoResponse_TimesOut(Func<IWebElement, int> getCount)
        {
            _respondToMessage = _ => { };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            Action getElement = () => getCount(webElement);
            getElement.ShouldThrow<TimeoutException>();
        }

        [Test]
        public void GetElementIsEditable_SendsCorrectMessage() =>
            GetIs_SendsCorrectMessage<GetElementIsEditableMessage>(ClassUnderTest.GetElementIsEditable);

        [Test]
        public void GetElementIsEditable_FindsElement_GetsCorrectBounds()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementIsEditableResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = true
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementIsEditable(webElement).Should().BeTrue();
        }

        [Test]
        public void GetElementIsEditable_DoesNotFindElements_GetsNoElements()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementIsEditableResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementIsEditable(webElement).Should().BeFalse();
        }

        [Test]
        public void GetElementIsEditable_NoResponse_TimesOut()
            => GetIs_NoResponse_TimesOut(ClassUnderTest.GetElementIsEditable);

        [Test]
        public void GetCheckedState_SendsCorrectMessage() =>
           GetIs_SendsCorrectMessage<GetElementIsCheckedMessage>(ClassUnderTest.GetCheckedState);

        [Test]
        public void GetCheckedState_FindsElement_GetsCorrectBounds()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementIsCheckedResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = true
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetCheckedState(webElement).Should().BeTrue();
        }

        [Test]
        public void GetCheckedState_DoesNotFindElements_GetsNoElements()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementIsCheckedResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetCheckedState(webElement).Should().BeFalse();
        }

        [Test]
        public void GetCheckedState_NoResponse_TimesOut()
            => GetIs_NoResponse_TimesOut(ClassUnderTest.GetCheckedState);

        [Test]
        public void GetElementIsVisible_SendsCorrectMessage() =>
          GetIs_SendsCorrectMessage<GetElementIsVisibleMessage>(ClassUnderTest.GetElementIsVisible);

        [Test]
        public void GetElementIsVisible_FindsElement_GetsCorrectBounds()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementIsVisibleResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = true
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementIsVisible(webElement).Should().BeTrue();
        }

        [Test]
        public void GetElementIsVisible_DoesNotFindElements_GetsNoElements()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementIsVisibleResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementIsVisible(webElement).Should().BeFalse();
        }

        [Test]
        public void GetElementIsVisible_NoResponse_TimesOut()
            => GetIs_NoResponse_TimesOut(ClassUnderTest.GetElementIsVisible);

        [Test]
        public void GetElementIsOnScreen_SendsCorrectMessage() =>
          GetIs_SendsCorrectMessage<GetElementIsVisibleMessage>(ClassUnderTest.GetElementIsOnScreen);

        [Test]
        public void GetElementIsOnScreen_FindsElement_GetsCorrectBounds()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementIsOnScreenResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = true
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementIsOnScreen(webElement).Should().BeTrue();
        }

        [Test]
        public void GetElementIsOnScreen_DoesNotFindElements_GetsNoElements()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementIsOnScreenResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementIsOnScreen(webElement).Should().BeFalse();
        }

        [Test]
        public void GetElementIsOnScreen_NoResponse_TimesOut()
            => GetIs_NoResponse_TimesOut(ClassUnderTest.GetElementIsOnScreen);

        private void GetIs_SendsCorrectMessage<T>(Func<IWebElement, bool> getCount) where T : WebMessage<Guid>
        {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            getCount(webElement);

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<T>(message.Data);
            sentMessage.Data.Should().Be(elementId);
        }

        private void GetIs_NoResponse_TimesOut(Func<IWebElement, bool> getCount)
        {
            _respondToMessage = _ => { };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            Action getElement = () => getCount(webElement);
            getElement.ShouldThrow<TimeoutException>();
        }

        [Test]
        public void GetListItems_SendsCorrectMessage() {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            ClassUnderTest.GetListItems(webElement);

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<GetListItemsMessage>(message.Data);
            sentMessage.Data.Should().Be(elementId);
        }

        [Test]
        public void GetListItems_FindsElement_GetsCorrectBounds()
        {
            var listItems = new ListItem[] { new ListItem(), new ListItem(), new ListItem() };
            _respondToMessage = message =>
            {
                var response = new GetListItemsResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = listItems
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetListItems(webElement).ShouldBeEquivalentTo(listItems);
        }

        [Test]
        public void GetListItems_DoesNotFindElements_GetsNoElements()
        {
            _respondToMessage = message =>
            {
                var response = new GetListItemsResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetListItems(webElement).Should().BeNull();
        }

        [Test]
        public void GetListItems_NoResponse_TimesOut()
        {
            _respondToMessage = _ => { };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            Action getElement = () => ClassUnderTest.GetListItems(webElement);
            getElement.ShouldThrow<TimeoutException>();
        }

        [Test]
        public void GetIsListItemSelected_SendsCorrectMessage()
        {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            ClassUnderTest.GetIsListItemSelected(webElement);

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<GetIsListItemSelectedMessage>(message.Data);
            sentMessage.Data.Should().Be(elementId);
        }

        [Test]
        public void GetIsListItemSelected_FindsElement_GetsCorrectBounds()
        {
            _respondToMessage = message =>
            {
                var response = new GetIsListItemSelectedResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = true
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetIsListItemSelected(webElement).Should().BeTrue();
        }

        [Test]
        public void GetIsListItemSelected_DoesNotFindElements_GetsNoElements()
        {
            _respondToMessage = message =>
            {
                var response = new GetIsListItemSelectedResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetIsListItemSelected(webElement).Should().BeFalse();
        }

        [Test]
        public void GetIsListItemSelected_NoResponse_TimesOut() => 
            GetIs_NoResponse_TimesOut(ClassUnderTest.GetIsListItemSelected);

        [Test]
        public void GetSliderRange_SendsCorrectMessage()
        {
            var elementId = Guid.NewGuid();
            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == elementId);
            ClassUnderTest.GetSliderRange(webElement);

            var message = _sentMessages.Single();
            message.PageId.Should().Be(_pageId);

            var sentMessage = JsonConvert.DeserializeObject<GetSliderRangeMessage>(message.Data);
            sentMessage.Data.Should().Be(elementId);
        }

        [Test]
        public void GetSliderRange_FindsElement_GetsCorrectBounds()
        {
            var range = new SliderRange{ Minimum = 1, Maximum = 10};
            _respondToMessage = message =>
            {
                var response = new GetSliderRangeResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = range
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetSliderRange(webElement).ShouldBeEquivalentTo(range);
        }

        [Test]
        public void GetSliderRange_DoesNotFindElements_GetsNoElements()
        {
            _respondToMessage = message =>
            {
                var response = new GetSliderRangeResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetSliderRange(webElement).Should().BeNull();
        }

        [Test]
        public void GetSliderRange_NoResponse_TimesOut()
        {
            _respondToMessage = _ => { };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            Action getElement = () => ClassUnderTest.GetSliderRange(webElement);
            getElement.ShouldThrow<TimeoutException>();
        }

        [Test]
        public void MouseHoverMessageReceived_ElementHoverCorrectlyRaised()
        {
            ElementHoverArgs messageRaised = null;
            var elementId = Guid.NewGuid();

            ClassUnderTest.ElementHover += (_, e) => messageRaised = e;

            var message = new MouseHoverMessage(elementId);
            ClassUnderTest.ReceiveMessage(new WebMessageWrapper(_pageId, JsonConvert.SerializeObject(message)));

            messageRaised.ElementId.Should().Be(elementId);
            messageRaised.Element.Id.Should().Be(elementId);
        }

        [Test]
        public void ResponseHasWrongConversationId_ResponseIgnored()
        {
            var elementId = Guid.NewGuid();

            _respondToMessage = message =>
            {
                var response = new GetElementByPathResponse
                {
                    ConversationId = Guid.NewGuid(),
                    Data = elementId
                };

                response
                .Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            Action getElement = () => ClassUnderTest.GetElementByPath("path");

            getElement.ShouldThrow<TimeoutException>();
        }

        [Test]
        public void ResponseHasRepeatedConversationId_ResponseIgnored()
        {
            var firstElementId = Guid.NewGuid();
            var secondElementId = Guid.NewGuid();

            _respondToMessage = message =>
            {
                new GetElementByPathResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = firstElementId
                }.Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);

                new GetElementByPathResponse
                {
                    ConversationId = GetConversationId(message),
                    Data = secondElementId
                }.Map(JsonConvert.SerializeObject)
                .Map(x =>new WebMessageWrapper(_pageId, x))
                .Tee(ClassUnderTest.ReceiveMessage);
            };

            ClassUnderTest.GetElementByPath("path").Id.Should().Be(firstElementId);
        }
        [Test]
        public void GetElementScreenLocationAndBounds_FindsElement_GetsCorrectBounds()
        {
            var bounds = new Rectangle(10, 10, 20, 25);
            _respondToMessage = message =>
            {
                var response = new GetElementScreenLocationAndBoundsResponse 
                {
                    ConversationId = GetConversationId(message),
                    Data = new BoundsMessageBody() { X = 10, Y = 10, Width = 20, Height = 25 }
                };

                response
                    .Map(JsonConvert.SerializeObject)
                    .Map(x =>new WebMessageWrapper(_pageId, x))
                    .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementScreenLocationAndBounds(webElement).ShouldBeEquivalentTo(bounds);
        }

        [Test]
        public void GetElementScreenLocationAndBounds_DoesNotFindElements_GetsNoElements()
        {
            _respondToMessage = message =>
            {
                var response = new GetElementScreenLocationAndBoundsResponse
                {
                    ConversationId = GetConversationId(message)
                };

                response
                    .Map(JsonConvert.SerializeObject)
                    .Map(x =>new WebMessageWrapper(_pageId, x))
                    .Tee(ClassUnderTest.ReceiveMessage);
            };

            var webElement = Moq.Mock.Of<IWebElement>(e => e.Id == Guid.NewGuid());
            ClassUnderTest.GetElementScreenLocationAndBounds(webElement).ShouldBeEquivalentTo(new Rectangle(-1, -1, 0, 0));
        }
    }
}

#endif
