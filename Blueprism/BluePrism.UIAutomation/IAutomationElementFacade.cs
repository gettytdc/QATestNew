using System;
using System.Drawing;

namespace BluePrism.UIAutomation
{
    /// <summary>
    /// Interface describing a simple automation element - this is either a cached
    /// or a current representation of an automation element and, as such, is more
    /// limited than the standard <see cref="IAutomationElement"/> implementations.
    /// </summary>
    public interface IAutomationElementFacade
    {

        /// <summary>
        /// The underlying element that this facade is wrapping
        /// </summary>
        IAutomationElement Element { get; }

        /// <summary>
        /// The screen-based bounding rectangle of the automation element
        /// </summary>
        Rectangle BoundingRectangle { get; }

        /// <summary>
        /// The screen-based centre point of the automation element
        /// </summary>
        Point CentrePoint { get; }

        /// <summary>
        /// The class name of the automation element
        /// </summary>
        string ClassName { get; }

        /// <summary>
        /// The automation ID of the element
        /// </summary>
        string AutomationId { get; }

        /// <summary>
        /// The localized control type of the element
        /// </summary>
        string LocalizedControlType { get; }

        /// <summary>
        /// Flag indicating if this element is a password
        /// </summary>
        bool IsPassword { get; }

        /// <summary>
        /// Flag indicating if the element is required for the form
        /// </summary>
        bool IsRequiredForForm { get; }

        /// <summary>
        /// The name of the element
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The item status of this element
        /// </summary>
        string ItemStatus { get; }

        /// <summary>
        /// The item type of this element
        /// </summary>
        string ItemType { get; }

        /// <summary>
        /// Flag indicating if this element is offscreen
        /// </summary>
        bool IsOffscreen { get; }

        /// <summary>
        /// The native window handle of this automation element
        /// </summary>
        IntPtr NativeWindowHandle { get; }

        /// <summary>
        /// Gets the element that this element is labelled by, or null if there is
        /// no such element
        /// </summary>
        IAutomationElementFacade LabeledBy { get; }

        /// <summary>
        /// Flag indicating if the element is enabled
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// The accelerator key set for this element
        /// </summary>
        string AcceleratorKey { get; }

        /// <summary>
        /// The access key set for this element
        /// </summary>
        string AccessKey { get; }

        /// <summary>
        /// Flag indicating if this element has keyboard focus
        /// </summary>
        bool HasKeyboardFocus { get; }

        /// <summary>
        /// The help text of this element
        /// </summary>
        string HelpText { get; }

        /// <summary>
        /// The ID of the process that this element belongs to
        /// </summary>
        int ProcessId { get; }

        /// <summary>
        /// The orientation of this element
        /// </summary>
        OrientationType Orientation { get; }

        /// <summary>
        /// Gets the path to this element from the root of the UIAutomation model
        /// owned by the same process as this element.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets the control type of this element
        /// </summary>
        ControlType ControlType { get; }

    }
}
