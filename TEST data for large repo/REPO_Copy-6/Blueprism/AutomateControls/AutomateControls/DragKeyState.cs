using System;

namespace AutomateControls
{
    /// <summary>
    /// Flags indicating the drag key states. This is just an int in the
    /// <see cref="DragEventArgs.KeyState"/> property, but this enum represents
    /// the values used, according to MSDN at least. See the page:
    /// https://msdn.microsoft.com/en-us/library/system.windows.forms.drageventargs.keystate%28v=vs.110%29.aspx
    /// for the full definition.
    /// </summary>
    [Flags]
    public enum DragKeyState : int
    {
        LeftMouse = 1,
        RightMouse = 2,
        ShiftKey = 4,
        CtrlKey = 8,
        MiddleMouse = 16,
        Alt = 32
    }
}
