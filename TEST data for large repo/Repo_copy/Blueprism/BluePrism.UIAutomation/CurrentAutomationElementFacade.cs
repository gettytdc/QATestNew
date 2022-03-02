using System;
using System.Drawing;

namespace BluePrism.UIAutomation
{
    /// <summary>
    /// A simple view of an AutomationElement representing the current properties of
    /// that element.
    /// </summary>
    internal class CurrentAutomationElementFacade : IAutomationElementFacade
    {
        /// <summary>
        /// Creates a new facade based on the given automation element
        /// </summary>
        /// <param name="elem">The element for which a cached facade is required.
        /// </param>
        /// <exception cref="ArgumentNullException">If <paramref name="elem"/> is
        /// null</exception>
        public CurrentAutomationElementFacade(IAutomationElement elem)
        {
            Element = elem ?? throw new ArgumentNullException(nameof(elem));
        }

        /// <summary>
        /// Gets the element that this current facade is representing
        /// </summary>
        public IAutomationElement Element { get; }

        /// <summary>
        /// The screen-based bounding rectangle of the automation element
        /// </summary>
        public Rectangle BoundingRectangle => Element.CurrentBoundingRectangle;

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
        public string ClassName => Element.CurrentClassName;

        /// <summary>
        /// The automation ID of the element
        /// </summary>
        public string AutomationId => Element.CurrentAutomationId;

        /// <summary>
        /// The localized control type of the element
        /// </summary>
        public string LocalizedControlType => Element.CurrentLocalizedControlType;

        /// <summary>
        /// Flag indicating if this element is a password
        /// </summary>
        public bool IsPassword => Element.CurrentIsPassword;

        /// <summary>
        /// Flag indicating if the element is required for the form
        /// </summary>
        public bool IsRequiredForForm => Element.CurrentIsRequiredForForm;
        
        /// <summary>
        /// The name of the element
        /// </summary>
        public string Name => Element.CurrentName;

        /// <summary>
        /// The item status of this element
        /// </summary>
        public string ItemStatus => Element.CurrentItemStatus;

        /// <summary>
        /// The item type of this element
        /// </summary>
        public string ItemType => Element.CurrentItemType;

        /// <summary>
        /// Flag indicating if this element is offscreen
        /// </summary>
        public bool IsOffscreen => Element.CurrentIsOffscreen;

        /// <summary>
        /// The native window handle of this automation element
        /// </summary>
        public IntPtr NativeWindowHandle => Element.CurrentNativeWindowHandle;

        /// <summary>
        /// Gets the element that this element is labelled by, or null if there is
        /// no such element
        /// </summary>
        public IAutomationElementFacade LabeledBy =>
            Element.CurrentLabeledBy?.Current;

        /// <summary>
        /// Flag indicating if the element is enabled
        /// </summary>
        public bool IsEnabled => Element.CurrentIsEnabled;

        /// <summary>
        /// The accelerator key set for this element
        /// </summary>
        public string AcceleratorKey => Element.CurrentAcceleratorKey;
        
        /// <summary>
        /// The access key set for this element
        /// </summary>
        public string AccessKey => Element.CurrentAccessKey;

        /// <summary>
        /// Flag indicating if this element has keyboard focus
        /// </summary>
        public bool HasKeyboardFocus => Element.CurrentHasKeyboardFocus;
        
        /// <summary>
        /// The help text of this element
        /// </summary>
        public string HelpText => Element.CurrentHelpText;

        /// <summary>
        /// The ID of the process that this element belongs to
        /// </summary>
        public int ProcessId => Element.CurrentProcessId;

        /// <summary>
        /// The orientation of this element
        /// </summary>
        public OrientationType Orientation => Element.CurrentOrientation;

        /// <summary>
        /// Gets the path to this element from the root of the UIAutomation model
        /// owned by the same process as this element.
        /// </summary>
        public string Path => Element.CurrentPath;

        /// <summary>
        /// Gets the control type of this element
        /// </summary>
        public ControlType ControlType => Element.CurrentControlType;
    }
}
