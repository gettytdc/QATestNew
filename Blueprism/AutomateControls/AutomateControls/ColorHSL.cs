using System.Drawing;
using System.Runtime.InteropServices.APIs;

namespace AutomateControls
{
    /// <summary>
    /// HSL (Hue/Saturation/Luminance) version of a colour structure, equivalent to
    /// the RGB-based colour structure in System.Drawing
    /// </summary>
    public struct ColorHSL
    {
        private float _hue;
        private float _sat;
        private float _lum;

        /// <summary>
        /// Creates a new ColorHSL value
        /// </summary>
        /// <param name="h">The hue of the color</param>
        /// <param name="s">The saturation of the color</param>
        /// <param name="l">The luminance of the color</param>
        public ColorHSL(float h, float s, float l)
        {
            _hue = h;
            _sat = s;
            _lum = l;
        }

        /// <summary>
        /// The hue of this colour
        /// </summary>
        public float Hue { get { return _hue; } }

        /// <summary>
        /// The saturation of this colour
        /// </summary>
        public float Saturation { get { return _sat; } }

        /// <summary>
        /// The luminance of this colour
        /// </summary>
        public float Luminance { get { return _lum; } }

        /// <summary>
        /// Gets this colour, illuminated by the given (possibly negative) percentage
        /// </summary>
        /// <param name="percent">The percentage by which this colour should be
        /// lightened</param>
        /// <returns>The percentage to lighten this colour by</returns>
        public ColorHSL Illuminate(float percent)
        {
            float lum = _lum * ((100f + percent) / 100f);
            if (lum < 0f) lum = 0f;
            if (lum > 255f) lum = 255f;

            return new ColorHSL(_hue, _sat, lum);
        }

        /// <summary>
        /// Converts a given ColorHSL instance to a Color
        /// </summary>
        /// <param name="col">The HSL color value to convert</param>
        /// <returns>The corresponding color value</returns>
        public static implicit operator Color(ColorHSL col)
        {
            float r = 0, g = 0, b = 0;
            ColorUtil.HSLToRGB(col._hue, col._sat, col._lum, ref r, ref g, ref b);
            return Color.FromArgb((int)r, (int)g, (int)b);
        }

        /// <summary>
        /// Converts a given Color instance to a ColorHSL value
        /// </summary>
        /// <param name="col">The color value to convert</param>
        /// <returns>The corresponding ColorHSL value</returns>
        public static implicit operator ColorHSL(Color col)
        {
            float h = 0, s = 0, l = 0;
            ColorUtil.RGBToHSL(col.R, col.G, col.B, ref h, ref s, ref l);
            return new ColorHSL(h, s, l);
        }
    }
}
