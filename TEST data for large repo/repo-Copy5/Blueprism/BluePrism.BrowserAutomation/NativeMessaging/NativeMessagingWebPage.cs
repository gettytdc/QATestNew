using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using BluePrism.BrowserAutomation.Behaviors;
using BluePrism.BrowserAutomation.Cryptography;
using BluePrism.BrowserAutomation.Data;
using BluePrism.BrowserAutomation.Events;
using BluePrism.BrowserAutomation.WebMessages;
using BluePrism.Core.Configuration;
using BluePrism.Core.Utility;
using BluePrism.Utilities.Functional;
using Newtonsoft.Json;

namespace BluePrism.BrowserAutomation.NativeMessaging
{
    public class NativeMessagingWebPage : IWebPage
    { 
        private readonly int _defaultCommunicationTimeout;
        private readonly Func<Guid, IWebPage, IWebElement> _webElementFactory;
        private readonly Action<WebMessageWrapper> _messageSender;
        private readonly IMessageCryptographyProvider _messageCryptographyProvider;
        private readonly Dictionary<Guid, IResponseHandler> _temporaryMessageHandlers =
            new Dictionary<Guid, IResponseHandler>();

        /// <inheritdoc />
        public Guid Id { get; }

        public NativeMessagingWebPage(
            Guid id,
            Action<WebMessageWrapper> messageSender,
            IAppSettings appSettings,
            IMessageCryptographyProvider messageCryptographyProvider,
            Func<Guid, IWebPage, IWebElement> webElementFactory)
        {
            Id = id;
            _webElementFactory = webElementFactory;
            _messageSender = messageSender;
            _messageCryptographyProvider = messageCryptographyProvider;

            var timeoutSetting = appSettings["BrowserAutomation.DefaultCommunicationTimeout"];
            int.TryParse(timeoutSetting, out _defaultCommunicationTimeout);
        }

        //For Unit Tests Only
        public NativeMessagingWebPage(Guid id) => Id = id;

        private TResponse SendMessageWaitForResponse<TMessage, TResponse>(TMessage message)
            where TMessage : IWebMessage
            where TResponse : IWebMessage
            =>
            message.Map(SendMessageWaitForResponse<TMessage, TResponse>(_defaultCommunicationTimeout));

        private Func<TMessage, TResponse> SendMessageWaitForResponse<TMessage, TResponse>(int timeoutInMilliseconds)
            where TMessage : IWebMessage
            where TResponse : IWebMessage
            => message =>
            {
                var conversationId = message.ConversationId;

                var response = default(TResponse);
                _temporaryMessageHandlers[conversationId] = new ResponseHandler<TResponse>(m => response = (TResponse)m);

                SendMessage(message);

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                while (response == null)
                {
                    Thread.Sleep(1);
                    if (stopwatch.ElapsedMilliseconds > timeoutInMilliseconds)
                    {
                        throw new TimeoutException();
                    }
                }

                return response;
            };

        private void SendMessage(IWebMessage message, bool encrypted = true)
        {
            if (encrypted)
            {
                message
                    .Map(JsonConvert.SerializeObject)
                    .Map(EncryptMessage)
                    .Map(x => new WebMessageWrapper(Id, x))
                    .Tee(_messageSender);
            }
            else //Only for Launch/Attach messages
            {
                message
                    .Map(JsonConvert.SerializeObject)
                    .Map(x => new WebMessageWrapper(Id, x))
                    .Tee(_messageSender);
            }
        }

        private string EncryptMessage(string message) =>
            message.Map(_messageCryptographyProvider.EncryptMessage(Id.ToString()));

        private string DecryptMessage(string message) =>
            message.Map(_messageCryptographyProvider.DecryptMessage(Id.ToString()));

        /// <inheritdoc />
        public IWebElement GetElementByPath(string path) =>
            new GetElementByPathMessage(path)
                .Map(SendMessageWaitForResponse<GetElementByPathMessage, GetElementByPathResponse>)
                .ElementId
                ?.Map(CreateElement);

        /// <inheritdoc />
        public IWebElement GetElementByCssSelector(string selector) =>
            new GetElementByCssSelectorMessage(selector)
                .Map(SendMessageWaitForResponse<GetElementByCssSelectorMessage, GetElementByCssSelectorResponse>)
                .ElementId
                ?.Map(CreateElement);

        /// <inheritdoc />
        public IWebElement GetElementById(string elementId) =>
            new GetElementByIdMessage(elementId)
                .Map(SendMessageWaitForResponse<GetElementByIdMessage, GetElementByIdResponse>)
                .ElementId
                ?.Map(CreateElement);

        /// <inheritdoc />
        public IReadOnlyCollection<IWebElement> GetElementsByType(string elementType) =>
            (new GetElementsByTypeMessage(elementType)
                .Map(SendMessageWaitForResponse<GetElementsByTypeMessage, GetElementsByTypeResponse>)
                .ElementIds ?? new Guid[0])
                .Select(CreateElement)
                .ToList();

        /// <inheritdoc />
        public IReadOnlyCollection<IWebElement> GetElementsByClass(string className) =>
            (new GetElementsByClassMessage(className)
                .Map(SendMessageWaitForResponse<GetElementsByClassMessage, GetElementsByClassResponse>)
                .ElementIds ?? new Guid[0])
                .Select(CreateElement)
                .ToList();

        /// <inheritDoc />
        public IReadOnlyCollection<IWebElement> GetElementsByName(string name) =>
            (new GetElementsByNameMessage(name)
                .Map(SendMessageWaitForResponse<GetElementsByNameMessage, GetElementsByNameResponse>)
                .ElementIds ?? new Guid[0])
                .Select(CreateElement)
                .ToList();

        /// <inheritdoc />
        public IReadOnlyCollection<IWebElement> GetElementDescendants(IWebElement element) =>
           (new GetDescendantsMessage(element.Id)
                .Map(SendMessageWaitForResponse<GetDescendantsMessage, GetDescendantsResponse>(10000))
                .ElementIds ?? new Guid[0])
                .Select(CreateElement)
                .ToList();

        /// <inheritdoc />
        public IWebElement GetRootElement() =>
             (new GetRootElementMessage()
                .Map(SendMessageWaitForResponse<GetRootElementMessage, GetRootElementResponse>)
                .ElementId
            ?? Guid.Empty)
            .Map(CreateElement);

        /// <inheritdoc />
        public Rectangle GetElementBounds(IWebElement element) =>
           new GetElementBoundsMessage(element.Id)
                .Map(SendMessageWaitForResponse<GetElementBoundsMessage, GetElementBoundsResponse>)
                .Bounds;

        /// <inheritdoc />
        public Point GetCursorPosition() =>
            new GetCursorPositionMessage()
                .Map(SendMessageWaitForResponse<GetCursorPositionMessage, GetCursorPositionResponse>)
                .CursorPosition;

        /// <inheritdoc />
        public string GetElementId(IWebElement element) =>
           new GetElementIdMessage(element.Id)
                .Map(SendMessageWaitForResponse<GetElementIdMessage, GetElementIdResponse>)
                .Id;

        /// <inheritdoc />
        public string GetSelectedText(IWebElement element) =>
            new GetElementSelectedTextMessage(element.Id)
                .Map(SendMessageWaitForResponse<GetElementSelectedTextMessage, GetElementSelectedTextResponse>)
                .Text;

        /// <inheritdoc />
        public void SelectTextRange(IWebElement element, int startIndex, int length) =>
            new SetElementSelectedTextRange(element.Id, startIndex, length)
                .Tee(x => SendMessage(x));

        /// <inheritdoc />
        public string GetTableItemText(IWebElement element, int rowNumber, int columnNumber) =>
            new GetElementTableItemTextMessage(element.Id, rowNumber, columnNumber)
                .Map(SendMessageWaitForResponse<GetElementTableItemTextMessage, GetElementTableItemTextResponse>)
                .CellText;

        /// <inheritdoc />
        public string GetElementName(IWebElement element) =>
            new GetElementNameMessage(element.Id)
                .Map(SendMessageWaitForResponse<GetElementNameMessage, GetElementNameResponse>)
                .Name;

        /// <inheritdoc />
        public string GetLinkAddressText(IWebElement element) =>
            new GetLinkAddressTextMessage(element.Id)
                .Map(SendMessageWaitForResponse<GetLinkAddressTextMessage, GetLinkAddressTextMessageResponse>)
                .Name;

        /// <inheritdoc />
        public string GetElementType(IWebElement element) =>
            new GetElementTypeMessage(element.Id)
                .Map(SendMessageWaitForResponse<GetElementTypeMessage, GetElementTypeResponse>)
                .Type;

        /// <inheritdoc />
        public string GetElementClass(IWebElement element) =>
            new GetElementClassMessage(element.Id)
                .Map(SendMessageWaitForResponse<GetElementClassMessage, GetElementClassResponse>)
                .Class;

        /// <inheritdoc />
        public string GetElementPath(IWebElement element) =>
            new GetElementPathMessage(element.Id)
                .Map(SendMessageWaitForResponse<GetElementPathMessage, GetElementPathResponse>)
                .Path;

        /// <inheritdoc />
        public string GetElementValue(IWebElement element) =>
            new GetElementValueMessage(element.Id)
                .Map(SendMessageWaitForResponse<GetElementValueMessage, GetElementValueResponse>)
                .Value;

        /// <inheritdoc />
        public string GetElementText(IWebElement element) =>
            new GetElementTextMessage(element.Id)
                .Map(SendMessageWaitForResponse<GetElementTextMessage, GetElementTextResponse>)
                .Text;

        /// <inheritdoc />
        public string GetElementHtml(IWebElement element) =>
            new GetElementHtmlMessage(element.Id)
                .Map(SendMessageWaitForResponse<GetElementHtmlMessage, GetElementHtmlResponse>)
                .Html;

        /// <inheritdoc />
        public void SetElementValue(IWebElement element, string value) =>
           new SetElementValueMessage(element.Id, value)
            .Tee(x => SendMessage(x));

        /// <inheritdoc />
        public void SetCheckedState(IWebElement element, bool value) =>
           new SetElementCheckedStateMessage(element.Id, value)
            .Tee(x => SendMessage(x));

        /// <inheritdoc />
        public void ClickElement(IWebElement element) =>
             new ClickElementMessage(element.Id)
                .Tee(x => SendMessage(x));

        /// <inheritdoc />
        public void DoubleClickElement(IWebElement element) =>
            new DoubleClickElementMessage(element.Id)
                .Tee(x => SendMessage(x));

        /// <inheritdoc />
        public void FocusElement(IWebElement element) =>
           new FocusElementMessage(element.Id)
                .Tee(x => SendMessage(x));

        /// <inheritdoc />
        public void HoverElement(IWebElement element) =>
            new HoverElementMessage(element.Id)
                .Tee(x => SendMessage(x));

        /// <inheritdoc />
        public void NavigateTo(string address) =>
           new SetAddressMessage(address)
            .Tee(x => SendMessage(x));

        /// <inheritdoc />
        public string GetAddress() =>
           new GetAddressMessage()
                .Map(SendMessageWaitForResponse<GetAddressMessage, GetAddressResponse>)
                .Address;

        /// <inheritdoc />
        public string GetHTMLSource() =>
            new GetHtmlSourceMessage()
                .Map(SendMessageWaitForResponse<GetHtmlSourceMessage, GetHtmlSourceResponse>)
                .HTMLSource;

        /// <inheritdoc />
        public void CloseWebPage() =>
            new CloseWebPageMessage(string.Empty)
                .Tee(x => SendMessage(x));

        /// <inheritdoc />
        public void HighlightElement(IWebElement element, Color color) =>
            new HighlightElementMessage(element.Id, color.ToWebColor())
                .Tee(x => SendMessage(x));

        /// <inheritdoc />
        public void RemoveHighlighting() =>
           new RemoveHighlightingMessage()
                .Tee(x => SendMessage(x));

        /// <inheritdoc />
        public Rectangle GetElementClientBounds(IWebElement element) =>
            new GetElementClientBoundsMessage(element.Id)
                .Map(SendMessageWaitForResponse<GetElementClientBoundsMessage, GetElementClientBoundsResponse>)
                .Bounds;

        /// <inheritdoc />
        public Rectangle GetElementOffsetBounds(IWebElement element) =>
            new GetElementOffsetBoundsMessage(element.Id)
                .Map(SendMessageWaitForResponse<GetElementOffsetBoundsMessage, GetElementOffsetBoundsResponse>)
                .Bounds;

        /// <inheritdoc />
        public Rectangle GetElementScrollBounds(IWebElement element) =>
            new GetElementScrollBoundsMessage(element.Id)
                .Map(SendMessageWaitForResponse<GetElementScrollBoundsMessage, GetElementScrollBoundsResponse>)
                .Bounds;

        /// <inheritdoc />
        public int GetElementChildCount(IWebElement element) =>
            new GetElementChildCountMessage(element.Id)
                .Map(SendMessageWaitForResponse<GetElementChildCountMessage, GetElementChildCountResponse>)
                .Count;

        /// <inheritdoc />
        public int GetColumnCount(IWebElement element, int rowIndex) =>
            new GetTableColumnCountMessage(element.Id, rowIndex)
            .Map(SendMessageWaitForResponse<GetTableColumnCountMessage, GetTableColumnCountResponse>)
            .ColumnCount;

        /// <inheritdoc />
        public int GetRowCount(IWebElement element) =>
            new GetTableRowCountMessage(element.Id)
            .Map(SendMessageWaitForResponse<GetTableRowCountMessage, GetTableRowCountResponse>)
            .RowCount;

        /// <inheritdoc />
        public bool GetElementIsEditable(IWebElement element) =>
             new GetElementIsEditableMessage(element.Id)
                .Map(SendMessageWaitForResponse<GetElementIsEditableMessage, GetElementIsEditableResponse>)
                .IsEditable;

        /// <inheritdoc />
        public bool GetCheckedState(IWebElement element) =>
            new GetElementIsCheckedMessage(element.Id)
                .Map(SendMessageWaitForResponse<GetElementIsCheckedMessage, GetElementIsCheckedResponse>)
                .IsChecked;

        /// <inheritdoc />
        public string GetElementStyle(IWebElement element) =>
            new GetElementStyleMessage(element.Id)
                .Map(SendMessageWaitForResponse<GetElementStyleMessage, GetElementStyleResponse>)
                .Style;

        /// <inheritdoc />
        public int GetElementTabIndex(IWebElement element) =>
            new GetElementTabIndexMessage(element.Id)
                .Map(SendMessageWaitForResponse<GetElementTabIndexMessage, GetElementTabIndexResponse>)
                .TabIndex;

        /// <inheritdoc />
        public string GetElementAttribute(IWebElement element, string name) =>
            new GetElementAttributeMessage(element.Id, name)
                .Map(SendMessageWaitForResponse<GetElementAttributeMessage, GetElementAttributeResponse>)
                .Value
                ?? string.Empty;

        /// <inheritdoc />
        public void SelectElement(IWebElement element) =>
           new SelectElementMessage(element.Id)
            .Tee(x => SendMessage(x));

        /// <inheritdoc />
        public void SetAttribute(IWebElement element, string attribute, string value) =>
            new SetElementAttributeMessage(element.Id, attribute, value)
                .Tee(x => SendMessage(x));

        /// <inheritdoc />
        public void ScrollToElement(IWebElement element) =>
           new ScrollToElementMessage(element.Id)
                .Tee(x => SendMessage(x));

        ///  <inheritdoc />
        public void SubmitElement(IWebElement element) =>
            new SubmitElementMessage(element.Id)
            .Tee(x => SendMessage(x));

        ///  <inheritdoc />
        public bool GetElementIsVisible(IWebElement element) =>
            new GetElementIsVisibleMessage(element.Id)
                .Map(SendMessageWaitForResponse<GetElementIsVisibleMessage, GetElementIsVisibleResponse>)
                .IsVisible;

        public void UpdateCookie(string cookie) =>
            new UpdateCookieMessage(cookie)
                .Tee(x => SendMessage(x));

        ///  <inheritdoc />
        public bool GetElementIsOnScreen(IWebElement element) =>
            new GetElementIsOnScreenMessage(element.Id)
                .Map(SendMessageWaitForResponse<GetElementIsOnScreenMessage, GetElementIsOnScreenResponse>)
                .IsOnScreen;

        ///  <inheritdoc />
        public void SelectListItem(IWebElement element, int index, string name) =>
            new SelectListItemMessage(element.Id, index, name)
                .Tee(x => SendMessage(x));

        ///  <inheritdoc />
        public void AddToListSelection(IWebElement element, int index, string name) =>
           new AddToListSelectionMessage(element.Id, index, name)
                .Tee(x => SendMessage(x));

        ///  <inheritdoc />
        public void RemoveFromListSelection(IWebElement element, int index, string name) =>
            new RemoveFromListSelectionMessage(element.Id, index, name)
                .Tee(x => SendMessage(x));

        ///  <inheritdoc />
        public IReadOnlyCollection<ListItem> GetListItems(IWebElement element) =>
            new GetListItemsMessage(element.Id)
                .Map(SendMessageWaitForResponse<GetListItemsMessage, GetListItemsResponse>)
                .Items;

        ///  <inheritdoc />
        public void InjectJavascript(string code) =>
            new InjectJavascriptMessage(code)
                .Tee(x => SendMessage(x));

        ///  <inheritdoc />
        public void InvokeJavascript(string functionName, string parameters) =>
            new InvokeJavascriptMessage(functionName, parameters)
                .Tee(x => SendMessage(x));

        ///  <inheritdoc />
        public string GetElementLabel(IWebElement element) =>
            new GetElementLabelMessage(element.Id)
                .Map(SendMessageWaitForResponse<GetElementLabelMessage, GetElementLabelResponse>)
                .Label;

        ///  <inheritdoc />
        public string GetElementAccessKey(IWebElement element) =>
            new GetElementAccessKeyMessage(element.Id)
                .Map(SendMessageWaitForResponse<GetElementAccessKeyMessage, GetElementAccessKeyResponse>)
                .AccessKey;

        ///  <inheritdoc />
        public SliderRange GetSliderRange(IWebElement element) =>
           new GetSliderRangeMessage(element.Id)
                .Map(SendMessageWaitForResponse<GetSliderRangeMessage, GetSliderRangeResponse>)
                .Range;

        ///  <inheritdoc />
        public bool GetIsListItemSelected(IWebElement element) =>
            new GetIsListItemSelectedMessage(element.Id)
            .Map(SendMessageWaitForResponse<GetIsListItemSelectedMessage, GetIsListItemSelectedResponse>)
            .IsSelected;

        /// <inheritdoc />
        public event ElementHoverDelegate ElementHover;

        private IWebElement CreateElement(Guid elementId) =>
            _webElementFactory(elementId, this);

        public void ReceiveMessage(WebMessageWrapper message)
        {
            var decryptedData = message.Data.Map(DecryptMessage);

            var webMessage = decryptedData.Map(JsonConvert.DeserializeObject<WebMessage>);

            if (_temporaryMessageHandlers.ContainsKey(webMessage.ConversationId))
            {
                var handler = _temporaryMessageHandlers[webMessage.ConversationId];
                var response = JsonConvert.DeserializeObject(decryptedData, handler.ExpectedResponseType);
                handler.OnResponseReceived(response as IWebMessage);
                _temporaryMessageHandlers.Remove(webMessage.ConversationId);
            }
            else if (webMessage.MessageType == MessageType.MouseHover)
            {
                var hoverMessage = JsonConvert.DeserializeObject<MouseHoverMessage>(decryptedData);
                HandleMouseHover(hoverMessage.ElementId);
            }
        }
        
        public Rectangle GetElementScreenLocationAndBounds(IWebElement element) =>
            new GetElementScreenLocationAndBoundsMessage(element.Id)
                .Map(SendMessageWaitForResponse<GetElementScreenLocationAndBoundsMessage, GetElementScreenLocationAndBoundsResponse>)
                .Bounds;

        private void HandleMouseHover(Guid elementId) => ElementHover?.Invoke(this, new ElementHoverArgs(elementId, CreateElement(elementId)));

        public bool CheckParentDocumentLoaded(IWebElement element) => true;

        public void Launch(BrowserType browserType, string urls, string trackingId) =>
            new LaunchMessage(browserType, urls, trackingId)
                .Tee(x => SendMessage(x, false));
        public void Attach(BrowserType browserType, string windowTitle, string trackingId) => 
            new AttachMessage(browserType, windowTitle, trackingId)
                .Tee(x => SendMessage(x, false));

        public void Detach(string trackingId) => new DetachMessage(trackingId).Tee(x => SendMessage(x, false));
    }
}
