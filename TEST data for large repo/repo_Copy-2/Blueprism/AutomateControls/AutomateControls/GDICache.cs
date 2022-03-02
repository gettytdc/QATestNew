using System;
using System.Drawing;
using System.Collections.Generic;
using System.Drawing.Drawing2D;

namespace AutomateControls
{
    /// <summary>
    /// Class to cache GDI components for a short period of time - Pens and Brushes
    /// are stored here and disposed of when this cache is disposed.
    /// </summary>
    public class GDICache : IDisposable
    {
        #region - Class Scope Declarations -

        /// <summary>
        /// Pen definition - ie. the definition of a pen held in the cache in this
        /// class. Since pens can have different widths, this encapsulates both the
        /// colour and the width of the corresponding pen
        /// </summary>
        private struct PenDefinition
        {
            // The colour of the pen
            private readonly Color _color;
            // The width of the pen
            private readonly int _width;
            // The dash style of the pen defined by this class.
            private readonly DashStyle _style;

            /// <summary>
            /// Creates a new single-line pen of the given colour.
            /// </summary>
            /// <param name="col">The colour of the pen</param>
            public PenDefinition(Color col) : this(col, 1) { }

            /// <summary>
            /// Creates a new solid pen of the given colour and width
            /// </summary>
            /// <param name="col">The colour of the pen</param>
            /// <param name="width">The width of the pen</param>
            public PenDefinition(Color col, int width)
                : this(col, width, DashStyle.Solid) { }

            /// <summary>
            /// Creates a new pen of the given colour and width.
            /// </summary>
            /// <param name="col">The colour of the pen</param>
            /// <param name="width">The width of the pen</param>
            /// <param name="style">The style to use for the pen.</param>
            public PenDefinition(Color col, int width, DashStyle style)
            {
                _color = col;
                _width = width;
                _style = style;
            }

            /// <summary>
            /// The colour of the pen
            /// </summary>
            public Color Color { get { return _color; } }

            /// <summary>
            /// The width of the pen
            /// </summary>
            public int Width { get { return _width; } }

            /// <summary>
            /// The dash style of the pen
            /// </summary>
            public DashStyle Style { get { return _style; } }

            /// <summary>
            /// Checks if the two pen definitions are equal - ie. that they have the
            /// same colour and width values.
            /// </summary>
            /// <param name="d1">The first definition to check</param>
            /// <param name="d2">The second definition to check</param>
            /// <returns>true if the definitions are equal, false otherwise</returns>
            public static bool operator ==(PenDefinition d1, PenDefinition d2)
            {
                return (d1._color == d2._color
                    && d1._width == d2._width && d1._style == d2._style);
            }
            /// <summary>
            /// Checks if the two pen definitions are unequal - ie. that they do not
            /// have the same colour and width values.
            /// </summary>
            /// <param name="d1">The first definition to check</param>
            /// <param name="d2">The second definition to check</param>
            /// <returns>true if the definitions are unequal, false otherwise.
            /// </returns>
            public static bool operator !=(PenDefinition d1, PenDefinition d2)
            {
                return !(d1 == d2);
            }
            /// <summary>
            /// Checks if the given object is equal to this object.
            /// </summary>
            /// <param name="obj">The object to test</param>
            /// <returns>true if the given object is a pen definition with the same
            /// value as this object; false otherwise.</returns>
            public override bool Equals(object obj)
            {
                return obj is PenDefinition && (this == (PenDefinition)obj);
            }

            /// <summary>
            /// Gets the hash code for this pen definition.
            /// </summary>
            /// <returns>An integer hash to represent this definition</returns>
            public override int GetHashCode()
            {
                return _color.GetHashCode() ^ (_width << 16) ^ ((int)_style << 24);
            }

            /// <summary>
            /// Gets a string representation of this pen definition.
            /// </summary>
            /// <returns>This pen definition as a string.</returns>
            public override string ToString()
            {
                return _color.ToString() + ":" + _width +
                    (_style == DashStyle.Solid ? "" : "-" + _style.ToString());
            }
        }

        /// <summary>
        /// Definition of a font for use within the GDI cache
        /// </summary>
        private struct FontDefinition
        {
            /// <summary>
            /// The name of the font
            /// </summary>
            public string Name { get; private set; }

            /// <summary>
            /// The size of the font
            /// </summary>
            public float Size { get; private set; }

            /// <summary>
            /// The style of the font
            /// </summary>
            public FontStyle Style { get; private set; }

            /// <summary>
            /// Creates a new font definition
            /// </summary>
            /// <param name="name">The name of the font</param>
            /// <param name="size">The size of the font</param>
            /// <param name="style">The style of the font</param>
            public FontDefinition(string name, float size, FontStyle style) :this()
            {
                Name = name;
                Size = size;
                Style = style;
            }

            /// <summary>
            /// Checks if this font definition is equal to the given object
            /// </summary>
            /// <param name="o">The object to test for equality with this font
            /// definition.</param>
            /// <returns>True if the given object is a FontDefinition with the same
            /// values set as this object; False otherwise</returns>
            public override bool Equals(object o)
            {
                if (!(o is FontDefinition)) return false;
                FontDefinition fd = (FontDefinition)o;
                return (Name == fd.Name && Size == fd.Size && Style == fd.Style);
            }

            /// <summary>
            /// Gets an integer hash for this object.
            /// </summary>
            /// <returns>An integer hash which represents this object's values
            /// </returns>
            public override int GetHashCode()
            {
                return (Name ?? "").Length << 24 ^
                    ((int)Style << 16) ^
                    Size.GetHashCode();
            }

            /// <summary>
            /// Gets a string representation of this object.
            /// </summary>
            /// <returns>A string describing this object.</returns>
            public override string ToString()
            {
                return "FontDefinition:{" + Name + ":" + Size + "-" + Style + "}";
            }
        }

        #endregion

        #region - Member Variables -

        // The map of brushes to their respective colours
        private IDictionary<Color, Brush> _brushMap;

        // The map of pens to their respective definitions
        private IDictionary<PenDefinition, Pen> _penMap;

        // The map of fonts to their definitions
        private IDictionary<FontDefinition, Font> _fontMap;

        #endregion

        #region - Constructors / Destructors -

        /// <summary>
        /// Creates a new empty cache
        /// </summary>
        public GDICache()
        {
            _brushMap = new Dictionary<Color, Brush>();
            _penMap = new Dictionary<PenDefinition, Pen>();
            _fontMap = new Dictionary<FontDefinition, Font>();
        }

        /// <summary>
        /// Destroys this brush cache object.
        /// </summary>
        ~GDICache()
        {
            Dispose(false);
        }

        #endregion

        #region - Methods -

        /// <summary>
        /// Gets the brush corresponding to the given colour in this cache,
        /// possibly creating it first if this is the first time it is requested.
        /// </summary>
        /// <param name="c">The colour for which a brush is required.</param>
        /// <returns>The brush corresponding to the given colour.</returns>
        public Brush GetBrush(Color c)
        {
            Brush b;
            if (!_brushMap.TryGetValue(c, out b))
            {
                b = new SolidBrush(c);
                _brushMap[c] = b;
            }
            return b;
        }

        /// <summary>
        /// Gets the pen corresponding to the given colour in this cache,
        /// possibly creating it first if this is the first time it is requested.
        /// </summary>
        /// <param name="c">The colour for which a pen is required.</param>
        /// <param name="width">The width of the required pen</param>
        /// <param name="style">The dash style of the required pen</param>
        /// <returns>The pen corresponding to the given colour.</returns>
        /// <remarks>If this method creates a pen, it will create a brush in the
        /// cache as a side-effect. Not entirely necessary, but it means that we can
        /// control the disposing of the associated brush as well (which may be a
        /// good example of premature optimisation)</remarks>
        public Pen GetPen(Color c, int width, DashStyle style)
        {
            PenDefinition pd = new PenDefinition(c, width, style);
            Pen p;
            if (!_penMap.TryGetValue(pd, out p))
            {
                p = new Pen(GetBrush(c), width) { DashStyle = style };
                _penMap[pd] = p;
            }
            return p;
        }

        /// <summary>
        /// Gets the pen corresponding to the given colour in this cache,
        /// possibly creating it first if this is the first time it is requested.
        /// </summary>
        /// <param name="c">The colour for which a pen is required.</param>
        /// <param name="width">The width of the required pen</param>
        /// <returns>The pen corresponding to the given colour.</returns>
        /// <remarks>If this method creates a pen, it will create a brush in the
        /// cache as a side-effect. Not entirely necessary, but it means that we can
        /// control the disposing of the associated brush as well (which may be a
        /// good example of premature optimisation)</remarks>
        public Pen GetPen(Color c, int width)
        {
            return GetPen(c, width, DashStyle.Solid);
        }

        /// <summary>
        /// Gets the pen corresponding to the given colour in this cache,
        /// possibly creating it first if this is the first time it is requested.
        /// </summary>
        /// <param name="c">The colour for which a pen is required.</param>
        /// <returns>The pen corresponding to the given colour.</returns>
        /// <remarks>If this method creates a pen, it will create a brush in the
        /// cache as a side-effect. Not entirely necessary, but it means that we can
        /// control the disposing of the associated brush as well (which may be a
        /// good example of premature optimisation)</remarks>
        public Pen GetPen(Color c)
        {
            return GetPen(c, 1);
        }

        /// <summary>
        /// Gets the font from this cache, based on an existing prototype
        /// </summary>
        /// <param name="prototype">The prototype font from which to draw the details
        /// for the required font.</param>
        /// <param name="appliedStyle">The style to apply to the font.</param>
        /// <returns>A Font, either from the cache, or newly generated and entered
        /// into the cache, which corresponds to the properties given.</returns>
        public Font GetFont(Font prototype, FontStyle appliedStyle)
        {
            return GetFont(prototype.Name, prototype.SizeInPoints, appliedStyle);
        }

        /// <summary>
        /// Gets the font from this cache with a specific definition
        /// </summary>
        /// <param name="name">The name of the font</param>
        /// <param name="em">The size of the font</param>
        /// <param name="style">The style of the font</param>
        /// <returns>A Font, either from the cache, or newly generated and entered
        /// into the cache, which corresponds to the properties given.</returns>
        public Font GetFont(string name, float em)
        {
            return GetFont(name, em, FontStyle.Regular);
        }

        /// <summary>
        /// Gets the font from this cache with a specific definition
        /// </summary>
        /// <param name="name">The name of the font</param>
        /// <param name="em">The size of the font</param>
        /// <param name="style">The style of the font</param>
        /// <returns>A Font, either from the cache, or newly generated and entered
        /// into the cache, which corresponds to the properties given.</returns>
        public Font GetFont(string name, float em, FontStyle style)
        {
            FontDefinition defn = new FontDefinition(name, em, style);
            Font f;
            if (!_fontMap.TryGetValue(defn, out f))
            {
                f = new Font(name, em, style);
                _fontMap[defn] = f;
            }
            return f;
        }

        #endregion

        #region - IDisposable Members -

        /// <summary>
        /// Explicitly disposes of this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of this object.
        /// </summary>
        /// <param name="explicitly">true to indicate that this object is being
        /// explicitly disposed of; false indicates a disposal through a destructor.
        /// </param>
        protected virtual void Dispose(bool explicitly)
        {
            if (explicitly)
            {
                foreach (Brush b in _brushMap.Values)
                {
                    if (b != null) b.Dispose();
                }
                _brushMap.Clear();

                foreach (Pen p in _penMap.Values)
                {
                    if (p != null) p.Dispose();
                }
                _penMap.Clear();

                foreach (Font f in _fontMap.Values)
                {
                    if (f != null) f.Dispose();
                }
                _fontMap.Clear();
            }
        }

        #endregion
    }
}
