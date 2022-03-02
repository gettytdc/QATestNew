using System;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using BluePrism.BPCoreLib.Collections;
using BluePrism.BPCoreLib;

namespace BluePrism.CharMatching.UI
{
    public enum RenderMethod { GDI, GDIPlus }

    /// <summary>
    /// Class to encapsulate the details to be searched in a font search
    /// </summary>
    public class FontSearchDetailSettings
    {
        // The font style settings
        private IBPSet<FontStyle> _styles;

        // The font EM size settings
        private IBPSet<float> _ems;

        // The render method settings
        private IBPSet<RenderMethod> _renders;

        /// <summary>
        /// Creates an empty settings object with no details set.
        /// </summary>
        public FontSearchDetailSettings() : this(null) { }

        /// <summary>
        /// Creates a new settings object with either no details set or a default
        /// set of details as specified.
        /// </summary>
        /// <param name="useDefault">True to use a default set of values; False to
        /// leave the new settings object empty</param>
        public FontSearchDetailSettings(bool useDefault)
            : this(null)
        {
            if (useDefault)
                ResetToDefault();
        }

        /// <summary>
        /// Creates a new settings object with details set from the given encoded
        /// string value.
        /// </summary>
        /// <param name="encoded">The encoded string value - as returned by the
        /// <see cref="Encoded"/> property of this class</param>
        public FontSearchDetailSettings(string encoded)
        {
            _styles = new clsSortedSet<FontStyle>();
            _ems = new clsSortedSet<float>();
            _renders = new clsSortedSet<RenderMethod>();
            if (encoded != null)
                Encoded = encoded;
        }

        /// <summary>
        /// The styles set in this settings object.
        /// </summary>
        public IBPSet<FontStyle> Styles { get { return _styles; } }

        /// <summary>
        /// The EM sizes set in this settings object.
        /// </summary>
        public IBPSet<float> Ems { get { return _ems; } }

        /// <summary>
        /// The render methods set in this settings object.
        /// </summary>
        public IBPSet<RenderMethod> RenderMethods { get { return _renders; } }

        /// <summary>
        /// Checks if this settings object is currently valid - ie. if a valid search
        /// could be run using these settings.
        /// </summary>
        public bool IsValid
        {
            get { return _styles.Count > 0 && _ems.Count > 0 && _renders.Count > 0; }
        }

        /// <summary>
        /// Clears this settings object, leaving an empty set of details
        /// </summary>
        public void Clear()
        {
            _styles.Clear();
            _ems.Clear();
            _renders.Clear();
        }

        /// <summary>
        /// Sets this settings object back to its default set of details
        /// </summary>
        public void ResetToDefault()
        {
            Clear();

            _styles.Add(FontStyle.Regular);
            _ems.Union(
                new float[] { 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 18 });
            _renders.Union(
                new RenderMethod[] { RenderMethod.GDI, RenderMethod.GDIPlus });
        }

        /// <summary>
        /// Gets or sets the details held in this settings object in an encoded
        /// string
        /// </summary>
        public string Encoded
        {
            get
            {
                StringBuilder sb =
                    new StringBuilder(CollectionUtil.Join(_styles, ","))
                    .Append(";").Append(CollectionUtil.Join(_ems, ","))
                    .Append(";").Append(CollectionUtil.Join(_renders, ","));
                return sb.ToString();
            }
            set
            {
                _styles.Clear();
                _ems.Clear();
                _renders.Clear();
                try
                {
                    if (value == null)
                        return;

                    string[] lists = value.Split(';');
                    if (lists.Length != 3)
                        return;

                    foreach (string styleStr in lists[0].Split(','))
                    {
                        FontStyle style = default(FontStyle);
                        if (clsEnum.TryParse(styleStr.Trim(), ref style))
                            _styles.Add(style);
                    }

                    foreach (string emStr in lists[1].Split(','))
                    {
                        float f = default(float);
                        if (float.TryParse(emStr.Trim(), out f))
                            _ems.Add(f);
                    }

                    foreach (string renderStr in lists[2].Split(','))
                    {
                        RenderMethod render = default(RenderMethod);
                        if (clsEnum.TryParse(renderStr.Trim(), ref render))
                            _renders.Add(render);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Fail(ex.ToString());
                }
            }
        }
    }
}
