using System;
using System.Drawing;

namespace BluePrism.UIAutomation
{
    /// <summary>
    /// A simple view of an AutomationElement representing the cached properties of
    /// that element.
    /// </summary>
    internal class CachedAutomationElementFacade : IAutomationElementFacade
    {
        /// <summary>
        /// Creates a new facade based on the given automation element
        /// </summary>
        /// <param name="elem">The element for which a cached facade is required.
        /// </param>
        /// <exception cref="ArgumentNullException">If <paramref name="elem"/> is
        /// null</exception>
        public CachedAutomationElementFacade(IAutomationElement elem)
        {
            Element = elem ?? throw new ArgumentNullException(nameof(elem));
        }

        /// <summary>
        /// Gets the element that this cached facade is representing
        /// </summary>
        public IAutomationElement Element { get; }

        /// <summary>
        /// The screen-based bounding rectangle of the automation element
        /// </summary>
        public Rectangle BoundingRectangle => Element.CachedBoundingRectangle;

        /// <summary>
        /// The screen-based centre point of the automation element
        /// </summary>
        public Point CentrePoint
        {
            get
            {
                var rect = BoundingRectangle;
                var pointX = rect.Left + rect.Width / 2;
                var pointY = rect.Top + rect.Height / 2;
                return new Point(pointX, pointY);
            }
        }

        /// <summary>
        /// The class name of the automation element
        /// </summary>
        public string ClassName => Element.CachedClassName;

        /// <summary>
        /// The automation ID of the element
        /// </summary>
        public string AutomationId => Element.CachedAutomationId;

        /// <summary>
        /// The localized control type of the element
        /// </summary>
        public string LocalizedControlType => Element.CachedLocalizedControlType;

        /// <summary>
        /// Flag indicating if this element is a password
        /// </summary>
        public bool IsPassword => Element.CachedIsPassword;

        /// <summary>
        /// Flag indicating if the element is required for the form
        /// </summary>
        public bool IsRequiredForForm => Element.CachedIsRequiredForForm;

        /// <summary>
        /// The name of the element
        /// </summary>
        public string Name => Element.CachedName;

        /// <summary>
        /// The item status of this element
        /// </summary>
        public string ItemStatus => Element.CachedItemStatus;

        /// <summary>
        /// The item type of this element
        /// </summary>
        public string ItemType => Element.CachedItemType;

        /// <summary>
        /// Flag indicating if this element is offscreen
        /// </summary>
        public bool IsOffscreen => Element.CachedIsOffscreen;

        /// <summary>
        /// The native window handle of this automation element
        /// </summary>
        public IntPtr NativeWindowHandle => Element.CachedNativeWindowHandle;

        /// <summary>
        /// Gets the element that this element is labelled by, or null if there is
        /// no such element
        /// </summary>
        public IAutomationElementFacade LabeledBy =>
            Element.CachedLabeledBy?.Cached;

        /// <summary>
        /// Flag indicating if the element is enabled
        /// </summary>
        public bool IsEnabled => Element.CachedIsEnabled;

        /// <summary>
        /// The accelerator key set for this element
        /// </summary>
        public string AcceleratorKey => Element.CachedAcceleratorKey;

        /// <summary>
        /// The access key set for this element
        /// </summary>
        public string AccessKey => Element.CachedAccessKey;

        /// <summary>
        /// Flag indicating if this element has keyboard focus
        /// </summary>
        public bool HasKeyboardFocus => Element.CachedHasKeyboardFocus;

        /// <summary>
        /// The help text of this element
        /// </summary>
        public string HelpText => Element.CachedHelpText;

        /// <summary>
        /// The ID of the process that this element belongs to
        /// </summary>
        public int ProcessId => Element.CachedProcessId;

        /// <summary>
        /// The orientation of this element
        /// </summary>
        public OrientationType Orientation => Element.CachedOrientation;

        /// <summary>
        /// Gets the path to this element from the root of the UIAutomation model
        /// owned by the same process as this element.
        /// </summary>
        public string Path => Element.CachedPath;

        /// <summary>
        /// Gets the control type of this element
        /// </summary>
        public ControlType ControlType => Element.CachedControlType;
    }
}
