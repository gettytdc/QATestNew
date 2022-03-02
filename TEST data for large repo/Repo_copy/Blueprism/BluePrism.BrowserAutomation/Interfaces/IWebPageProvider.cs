using BluePrism.BrowserAutomation.Events;

namespace BluePrism.BrowserAutomation
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides methods for interacting with active web pages
    /// </summary>
    public interface IWebPageProvider : IDisposable
    {
        /// <summary>
        /// Gets the element currently under the mouse cursor.
        /// </summary>
        IWebElement ElementUnderCursor { get; }

        /// <summary>
        /// Gets all active web pages.
        /// </summary>
        /// <returns>A collection of active web pages.</returns>
        IReadOnlyCollection<IWebPage> GetActiveWebPages(string trackingId = "");

        IWebPage ActiveMessagingHost();

        /// <summary>
        /// disconnects a webPage (used when detaching)
        /// </summary>
        void DisconnectPage(Guid pageId);

        void SetProcHandle(IntPtr procHandle);

        void CloseActivePages();

        void DetachTrackingId(string trackingId);

        void DetachAllTrackedPages();

        bool IsTracking();

        bool IsTracking(string trackingId);

        event WebPageCreatedDelegate OnWebPageCreated;

        event TrackingIdDetachedDelegate OnTrackingIdDetached;
    }
}
