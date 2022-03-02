using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BluePrism.BrowserAutomation.WebMessages
{
    public class WebMessageWrapper
    {
        public string Data { get; }
        public Guid PageId { get; }
        public List<Guid> Pages { get; set; } 
        public string TrackingId { get; }
        public string ExtensionVersion { get; }
        public string ConversationId { get; }
        public WebMessageWrapper(Guid pageId, string data)
        {
            PageId = pageId;
            Data = data;
            Pages = new List<Guid>();
        }

        [JsonConstructor]
        public WebMessageWrapper(Guid pageId, string data, string trackingId, string extensionVersion, string conversationId)
        {
            PageId = pageId;
            Pages = new List<Guid>();
            Data = data;
            TrackingId = trackingId;
            ExtensionVersion = extensionVersion;
            ConversationId = conversationId;
        }

        public WebMessageWrapper(Guid pageId, string data, string trackingId, string extensionVersion)
        {
            PageId = pageId;
            Pages = new List<Guid>();
            Data = data;
            TrackingId = trackingId;
            ExtensionVersion = extensionVersion;
            ConversationId = Guid.Empty.ToString();
        }

        public WebMessageWrapper(List<Guid> pages, string data, string trackingId, string extensionVersion)
        {
            Pages = pages;
            Data = data;
            TrackingId = trackingId;
            ExtensionVersion = extensionVersion;
            ConversationId = Guid.Empty.ToString();
        }
    }
}
