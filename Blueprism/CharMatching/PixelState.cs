using System;

namespace BluePrism.CharMatching
{
    /// <summary>
    /// A variety of states which can be associated with this character, pixel by
    /// pixel.
    /// </summary>
    [Flags]
    public enum PixelState
    {
        /// <summary>
        /// Default - indicates that the checking of a pixel is supported / sensible
        /// </summary>
        None = 0,

        /// <summary>
        /// Suppresses the checking of a pixel. This is intended to allow other
        /// characters to overlap the white space of this glyph, in a tightly kerned
        /// font.
        /// </summary>
        NoCheck = 1
    }

}
