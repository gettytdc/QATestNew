using System;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// The resize mode for a region
    /// </summary>
    [Flags]
    public enum ResizeMode
    {
        None = 0,
        Top = 1,
        Right = 2,
        Bottom = 4,
        Left = 8,
        TopLeft = Top | Left,
        TopRight = Top | Right,
        BottomRight = Bottom | Right,
        BottomLeft = Bottom | Left
    }

    /// <summary>
    /// Flags indicating the borders to check for guides
    /// </summary>
    [Flags]
    public enum GuideCheck
    {
        None = 0x00,
        Top = 0x01,
        Right = 0x02,
        Bottom = 0x04,
        Left = 0x08,
        Vertical = Left | Right,
        Horizontal = Top | Bottom,
        All = Vertical | Horizontal
    }

}