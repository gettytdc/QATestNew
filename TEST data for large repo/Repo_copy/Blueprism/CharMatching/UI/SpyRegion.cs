using BluePrism.CharMatching.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Design;
using AutomateControls;
using BluePrism.BPCoreLib;
using BluePrism.BPCoreLib.Collections;
using BluePrism.CharMatching.Internationalization;
using BluePrism.CharMatching.UI.Designer;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Class to represent a spy region
    /// </summary>
    [DefaultProperty("Name")]
    public class SpyRegion
    {
        #region - Class Scope Declarations -

        // The color to use for a region
        private static readonly Color RegionColour = Color.FromArgb(96, 150, 150, 150);

        // The color to use for an active region
        private static readonly Color ActiveColour = Color.FromArgb(96, 150, 100, 120);

        // The color to use for a selected region
        private static readonly Color SelectedColour = Color.FromArgb(96, 80, 130, 100);

        // The color to use for a region being hovered over
        private static readonly Color HoverColour = Color.FromArgb(96, 120, 100, 150);

        // The zone around the border of a region which is interpreted as
        // hovering over the border of the region
        private const int HoverZoneOffset = 2;

        // The minimum width allowed for a region
        private const int MinimumWidth = 5;

        // The minimum height allowed for a region
        private const int MinimumHeight = 5;

        /// <summary>
        /// A map of the guide checks which need to be performed against a
        /// region's resize mode that they must be performed for.
        /// eg. for a bottom-right resize, the bottom and right edges of a
        /// region must be checked for guides.
        /// </summary>
        private static readonly IDictionary<ResizeMode, GuideCheck> __GuideCheckLookup =
            BuildGuideCheckLookup();

        /// <summary>
        /// Builds the guide check lookup map.
        /// </summary>
        /// <returns>A readonly dictionary mapping guide check values against their
        /// relevant resize modes</returns>
        private static IDictionary<ResizeMode, GuideCheck> BuildGuideCheckLookup()
        {
            Dictionary<ResizeMode, GuideCheck> map = new Dictionary<ResizeMode, GuideCheck>();
            map[ResizeMode.TopLeft] = GuideCheck.Top | GuideCheck.Left;
            map[ResizeMode.Top] = GuideCheck.Top;
            map[ResizeMode.TopRight] = GuideCheck.Top | GuideCheck.Right;
            map[ResizeMode.Right] = GuideCheck.Right;
            map[ResizeMode.BottomRight] = GuideCheck.Bottom | GuideCheck.Right;
            map[ResizeMode.Bottom] = GuideCheck.Bottom;
            map[ResizeMode.BottomLeft] = GuideCheck.Bottom | GuideCheck.Left;
            map[ResizeMode.Left] = GuideCheck.Left;
            map[ResizeMode.None] = GuideCheck.All;
            return GetReadOnly.IDictionary<ResizeMode, GuideCheck>(map);
        }

        #endregion

        #region - Events -

        /// <summary>
        /// Event fired when the region's constrained data is changed.
        /// This can be the rectangle defining the region or an internal
        /// schema change for this region
        /// </summary>
        public event EventHandler RegionLayoutChanged;

        /// <summary>
        /// Event fired when the region's constrained data is changing -
        /// ie. that it is in the process of changing, has new values etc.
        /// but may continue to change (eg. it's being moved, being resized
        /// or such like).
        /// A series of RegionLayoutChanging events is usually followed by
        /// a RegionLayoutChanged event which marks the end of the changing
        /// </summary>
        public event EventHandler RegionLayoutChanging;

        /// <summary>
        /// Event fired when the character data in a region has changed - this is
        /// anything which would impact the character matching within a region, eg.
        /// font name, foreground colour, background colour.
        /// </summary>
        public event RegionCharDataChangeEventHandler CharacterDataChanged;

        /// <summary>
        /// Event fired when the name of a region is changing
        /// </summary>
        public event NameChangingEventHandler NameChanging;

        /// <summary>
        /// Event fired when the name of a region is changed
        /// </summary>
        public event EventHandler NameChanged;

        #endregion

        #region - Members -

        // The ID of this region, if it has one.
        private Guid _id;

        // The name of the region
        private string _name;

        // The initial name of the region
        private string _initName;

        // The owner of the region
        private ISpyRegionContainer _container;

        // The rectangle describing the region
        private Rectangle _rect;

        // The current resize mode of this region.
        private ResizeMode _mode;

        // Flag indicating if this region is currently active or not
        private bool _active;

        // The last location of the mouse when a move operation was processed
        private Point _anchor;

        // Flag indicating if this region should retain its image from the spy action
        private bool _retainImage;

        // Flag which determines whether the displayed image of the region is greyscale.
        private bool _greyScale;

        // A copy of the image within the selected inner rectangle.  This can be greyscaled
        // and shown within the region editor.
        private Image _displayedImage;

        // Flag indicating if this region is visible in the region mapper or not.
        private bool _visible;

        // The name of the font associated with this region
        private string _fontName;

        // The rectangle describing this region when it was last activated
        private Rectangle _savedRect;

        // The tag associated with this region
        private object _tag;

        // The externally registered chars associated with this region
        private ICollection<CharData> _chars;

        // Method used to locate region within the application window 
        private RegionLocationMethod _locationMethod;

        // The position that the region is searched for
        private RegionPosition _regionPosition;

        // Method used to locate the region within the application window 
        private SpyRegion _relativeParent;

        // Additional padding around coordinates that will be searched
        private Padding _imageSearchPadding;

        // The tolerance used when comparing each pixel's r, g and b values between images
        private int _colourTolerance;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new spy region within the given owner and describing the
        /// given rectangle
        /// </summary>
        /// <param name="cont">The owning control for this region.
        /// Note that this constructor does not add the new region to the container;
        /// it merely registers the container within the new region.</param>
        /// <param name="rect">The rectangle that this region represents.
        /// </param>
        public SpyRegion(ISpyRegionContainer cont, Rectangle rect)
            : this(cont, rect, cont.GetUniqueRegionName(Resources.Region0)) { }

        /// <summary>
        /// Creates a new spy region within the given owner and describing the
        /// given rectangle
        /// </summary>
        /// <param name="cont">The owning control for this region.
        /// Note that this constructor does not add the new region to the container;
        /// it merely registers the container within the new region.</param>
        /// <param name="rect">The rectangle that this region represents.
        /// </param>
        /// <param name="name">The initial name for this region</param>
        public SpyRegion(ISpyRegionContainer cont, Rectangle rect, string name)
        {
            _container = cont;
            _rect = rect;
            _name = _initName = name;
            _visible = true;
            _retainImage = true;
            _locationMethod = RegionLocationMethod.Image;
            _regionPosition = RegionPosition.Fixed;
            _imageSearchPadding = new Padding(20, 15, 20, 15);
        }

        #endregion

        #region - Browsable Properties -

        /// <summary>
        /// The name of this region
        /// </summary>
        [CMCategory("Identification"),
         CMDescription("The name of this region"),
         CMDisplayName("Name"),
         RefreshProperties(RefreshProperties.All),
         SortOrder(1)]
        public string Name
        {
            get { return (_name ?? ""); }
            set
            {
                NameChangingEventArgs e = new NameChangingEventArgs(Name, value);
                OnNameChanging(e);
                if (e.Cancel)
                    return;

                _name = value;
                OnNameChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// The rectangle described by this region. This raises the
        /// <see cref="RegionLayoutChanged"/> event if the rectangle being
        /// set differs from the currently set rectangle
        /// </summary>
        [Browsable(false),
         CMCategory("Region"),
         CMDisplayName("Rectangle"),
         CMDescription("The rectangle which defines this region"),
         SortOrder(1)]
        public Rectangle Rectangle
        {
            get { return _rect; }
            set
            {
                if (_rect != value)
                {
                    _rect = value;
                    OnRegionLayoutChanged(EventArgs.Empty);
                }
            }
        }
        [Browsable(true),
         CMCategory("Region Rectangle"),
        CMDisplayName("X"),
        CMDescription("The x position of the rectangle"),
        SortOrder(1)]
        public int xValue
        {
            get { return _rect.X; }
            set
            {
                if (_rect.X != value)
                {
                    _rect.X = value;
                    OnRegionLayoutChanged(EventArgs.Empty);
                }
            }
        }
        [Browsable(true),
         CMCategory("Region Rectangle"),
        CMDisplayName("Y"),
        CMDescription("The y position of the rectangle"),
        SortOrder(2)]
        public int yValue
        {
            get { return _rect.Y; }
            set
            {
                if (_rect.Y != value)
                {
                    _rect.Y = value;
                    OnRegionLayoutChanged(EventArgs.Empty);
                }
            }
        }
        [Browsable(true),
         CMCategory("Region Rectangle"),
        CMDisplayName("Width"),
        CMDescription("The width of the rectangle"),
        SortOrder(3)]
        public int Width
        {
            get { return _rect.Width; }
            set
            {
                if (_rect.Width != value)
                {
                    _rect.Width = value;
                    OnRegionLayoutChanged(EventArgs.Empty);
                }
            }
        }

        [Browsable(true),
        CMCategory("Region Rectangle"),
        CMDisplayName("Height"),
        CMDescription("The height of the rectangle"),
        SortOrder(4)]
        public int Height
        {
            get { return _rect.Height; }
            set
            {
                if (_rect.Height != value)
                {
                    _rect.Height = value;
                    OnRegionLayoutChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets whether this region is visible or not in the region mapper
        /// </summary>
        [CMCategory("Appearance"),
         CMDisplayName("Visible"),
         CMDescription("Shows or hides the region within the region editor. " +
         "If invisible, a region can still be selected using the drop down menu"),
         DefaultValue(true),
         SortOrder(999), TypeConverter(typeof(CMBooleanConverter))]
        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        /// <summary>
        /// Gets or sets the font name associated with this region
        /// </summary>
        [CMCategory("Behaviour"),
         CMDisplayName("Font"),
         CMDescription("The Blue Prism font that is used in this region"),
         DefaultValue(null),
         Editor(typeof(FontNameUIEditor), typeof(UITypeEditor)),
         SortOrder(1)]
        public string FontName
        {
            get { return (_fontName ?? ""); }
            set
            {
                // empty string == null for the purposes of font name, so we hold
                // null internally and convert incoming values for consistency.
                if (value == "")
                    value = null;

                if (_fontName != value)
                {
                    string oldValue = _fontName;
                    if (value != null)
                    {
                        // Determine whether the entered font exists (ignoring case)
                        bool newFont = true;
                        foreach (string font in _container.InstalledFontNames)
                        {
                            if (value.Equals(font, StringComparison.CurrentCultureIgnoreCase))
                            {
                                value = font;
                                newFont = false;
                                break;
                            }
                        }
                        if (newFont)
                        {
                            try
                            {
                                _container.CreateEmptyFont(value);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(null, string.Format(
                                    Resources.AnErrorOccurredWhileCreatingTheFont01,
                                    value, e.Message), Resources.Error, MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                                return;
                            }
                        }
                    }
                    _fontName = value;
                    OnCharacterDataChanged(new RegionCharDataChangeEventArgs(
                        RegionCharDataChange.Font, oldValue, value));
                }
            }
        }

        /// <summary>
        /// Flag indicating if this region should retain the image captured
        /// when the control was spied
        /// </summary>
        [CMCategory("Behaviour"),
         CMDisplayName("Retain Image"),
         CMDescription("True to retain the image from the spied region in this region"),
         DefaultValue(false),
         SortOrder(2), TypeConverter(typeof(CMBooleanConverter))]
        public bool RetainImage
        {
            get { return _retainImage; }
            set { _retainImage = value; }
        }

        [CMCategory("Region Location"),
         CMDisplayName("Position"),
         CMDescription("Controls where a region's stored image is searched for within the containing "
             + "element at run-time. \r\n\r\n'Fixed' limits the search to the image location originally "
             + "recorded when modelling the application, plus the additional surrounding area specified in the "
             + "'Search Padding' property. \r\n\r\n'Relative' will search for this region relative to where its ''Relative "
             + "Parent'' is found at run-time. \r\n\r\n'Anywhere' will search the entirety of the "
             + "region's parent element, and is only available when the ''Location Method'' property is set to "
             + "''Image''"),
         RefreshProperties(RefreshProperties.All),
         SortOrder(2),
         TypeConverter(typeof(RegionPositionTypeConverter))]
        public RegionPosition RegionPosition
        {
            get { return _regionPosition; }
            set
            {
                if (value != RegionPosition.Relative) _relativeParent = null;
                _regionPosition = value;
            }
        }

        [CMCategory("Region Location"),
         CMDisplayName("Relative Parent"),
         CMDescription("The region from which this region's location is relatively calculated from at run-time."),
         RefreshProperties(RefreshProperties.All),
         TypeConverter(typeof(RelativeSpyRegionTypeConverter)), SortOrder(3)
        ]
        public SpyRegion RelativeParent
        {
            get { return _relativeParent; }
            set
            {
                _relativeParent = value;
            }
        }

        [CMCategory("Region Location"),
        CMDisplayName("Location Method"),
        CMDescription("Method used to locate a region within the containing element at run-time."
            + "\r\n\r\n'Image' will locate the region by searching for the region's stored image at run-time;"
            + "\r\nIntended for form elements with static content such as labels, icons and buttons."
            + "\r\n\r\n'Coordinates' will locate the region by using the fixed rectangle that defines the region;"
            + "\r\nIntended for elements that contain dynamic content�such as text, checkboxes and combo-boxes."),
        RefreshProperties(RefreshProperties.All),
        SortOrder(1), TypeConverter(typeof(CMEnumConverter))]
        public RegionLocationMethod LocationMethod
        {
            get { return _locationMethod; }
            set
            {
                _locationMethod = value;
                if (value == RegionLocationMethod.Image)
                {
                    RetainImage = true;
                }
            }
        }

        [
            Browsable(false),
            CMCategory("Region Location"),
         CMDisplayName("Search Padding"),
         CMDescription("Specifies the area around an image's original location that the region's stored image will be "
             + "searched for at run-time."),
         SortOrder(4)]
        public Padding ImageSearchPadding
        {
            get { return _imageSearchPadding; }
            set { _imageSearchPadding = value; }
        }

        [CMCategory("Search Padding"),
        CMDisplayName("All"),
        CMDescription("All Padding"),
        SortOrder(1)]
        public int ImageSearchAllPadding
        {
            get { return _imageSearchPadding.All; }
            set { _imageSearchPadding.All = value; }
        }

        [CMCategory("Search Padding"),
        CMDisplayName("Left"),
        CMDescription("Left Padding"),
        SortOrder(2)]
        public int ImageSearchLeftPadding
        {
            get { return _imageSearchPadding.Left; }
            set { _imageSearchPadding.Left = value; }
        }

        [CMCategory("Search Padding"),
        CMDisplayName("Top"),
        CMDescription("Top Padding"),
        SortOrder(2)]
        public int ImageSearchTopPadding
        {
            get { return _imageSearchPadding.Top; }
            set { _imageSearchPadding.Top = value; }
        }

        [CMCategory("Search Padding"),
        CMDisplayName("Right"),
        CMDescription("Right Padding"),
        SortOrder(4)]
        public int ImageSearchRightPadding
        {
            get { return _imageSearchPadding.Right; }
            set { _imageSearchPadding.Right = value; }
        }

        [CMCategory("Search Padding"),
        CMDisplayName("Bottom"),
        CMDescription("Bottom Padding"),
        SortOrder(5)]
        public int ImageSearchBottomPadding
        {
            get { return _imageSearchPadding.Bottom; }
            set { _imageSearchPadding.Bottom = value; }
        }

        [CMCategory("Region Location"),
         CMDisplayName("Colour Tolerance"),
         CMDescription("Specifies the tolerance when comparing the R, G and B components of an image's pixels at run-time. "
             + "Valid values are 0 through 255"),
         RangeAttribute(0, 255),
         SortOrder(5)]
        public int ColourTolerance
        {
            get { return _colourTolerance; }
            set { _colourTolerance = value; }
        }

        /// <summary>
        /// Determines whether the search for the region should use Grey Scale.
        /// </summary>
        [CMCategory("Region Location"),
         CMDisplayName("Greyscale"),
         CMDescription("True to use grayscale in the image to search for this region"),
         DefaultValue(false),
         SortOrder(6), TypeConverter(typeof(CMBooleanConverter))]
        public bool Greyscale
        {
            get { return _greyScale; }
            set
            {
                _greyScale = value;
                DisplayedImage = GetRegionImage(_greyScale);
            }
        }

        #endregion

        #region - Hidden Properties -

        /// <summary>
        /// Arbitrary data associated with this region
        /// </summary>
        [Browsable(false),
         CMCategory("Data"),
         CMDisplayName("Tag"),
         CMDescription("User-defined data associated with the object")]
        public object Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        /// <summary>
        /// The configured chars associated with this region. Note that this is not
        /// generated or maintained directly by this object.
        /// </summary>
        [Browsable(false),
         CMCategory("Data"),
         CMDisplayName("Chars"),
         CMDescription("Externally registered chars associated with the region")]
        public ICollection<CharData> Chars
        {
            get { return _chars; }
            set { _chars = value; }
        }

        /// <summary>
        /// Gets the font currently held in this spy region
        /// </summary>
        [Browsable(false),
        CMDisplayName("Font")]
        public BPFont Font
        {
            get
            {
                return (string.IsNullOrEmpty(_fontName)
                    ? null : _container.Store.GetFont(_fontName));
            }
        }

        /// <summary>
        /// Sets the rectangle described by this region without
        /// firing any events - appropriate events must thus be
        /// handled by the setting code.
        /// </summary>
        [Browsable(false),
         CMDisplayName("InnerRectangle")]
        protected Rectangle InnerRectangle
        {
            set { _rect = value; }
        }

        /// <summary>
        /// The ID of this region, if it has one assigned to it
        /// </summary>
        [Browsable(false),
         CMDisplayName("Id")]
        public Guid Id
        {
            get { return _id; }
            set { _id = value; }
        }

        /// <summary>
        /// The upper left location at which this region resides
        /// </summary>
        [Browsable(false),
         CMCategory("Region"),
         CMDescription("The location in the control that region starts from"),
         CMDisplayName("From")]
        public Point From
        {
            get { return _rect.Location; }
            set { Rectangle = new Rectangle(value, (Size)(this.To - (Size)value)); }
        }

        /// <summary>
        /// The location of the lower right rectangle that this region describes
        /// </summary>
        [Browsable(false),
         CMCategory("Region"),
         CMDescription("The location in the control that region goes to"),
         CMDisplayName("To")]
        public Point To
        {
            get { return new Point(_rect.Right, _rect.Bottom); }
            set
            {
                Rectangle = new Rectangle(
                    _rect.Location, (Size)value - (Size)_rect.Location);
            }
        }

        /// <summary>
        /// Checks if this region falls within the image defined in the container.
        /// If no container is registered on this region, or it has no bitmap
        /// defined, the region will be reported as <em>not</em> within the container
        /// image.
        /// </summary>
        [Browsable(false),
         CMDisplayName("IsWithinContainerImage"), 
         TypeConverter(typeof(CMBooleanConverter))]
        public bool IsWithinContainerImage
        {
            get
            {
                if (_container == null)
                    return false;

                Bitmap b = _container.Bitmap;
                if (b == null)
                    return false;

                if (!_rect.IntersectsWith(new Rectangle(Point.Empty, b.Size)))
                {
                    // The region falls outside the image - there can be no
                    // image in that case.
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// The image from the hosting region container which is described
        /// by this region
        /// </summary>
        [Browsable(false),
         CMDisplayName("Image")]
        public Image Image
        {
            get
            {
                if (!IsWithinContainerImage)
                    return null;

                // We want a normal image, not a greyscaled one.
                return GetRegionImage(false);
            }
        }

        /// <summary>
        /// The image from the hosting region container which is described
        /// by this region
        /// </summary>
        [Browsable(false),
         CMDisplayName("DisplayedImage")]
        public Image DisplayedImage
        {
            get
            {
                if (!IsWithinContainerImage)
                    return null;

                return _displayedImage;
            }

            private set
            {
                if (_displayedImage != null)
                    _displayedImage.Dispose();

                _displayedImage = value;
            }
        }

        /// <summary>
        /// Gets a feathered version of this region - ie. a region within the same
        /// container, with the same name, which is 1 pixel wider and 1 pixel taller
        /// than this region.
        /// Note that the returned region is not registered on this region's
        /// container, though it does refer to it as its container.
        /// </summary>
        [Browsable(false),
         CMDisplayName("Feathered")]
        public SpyRegion Feathered
        {
            get
            {
                Rectangle r = _rect;
                r.Width++;
                r.Height++;
                return new SpyRegion(_container, r, _name);
            }
        }

        /// <summary>
        /// The current resize mode of this region
        /// </summary>
        [Browsable(false),
         CMDisplayName("Mode")]
        public ResizeMode Mode
        {
            get { return _mode; }
        }

        /// <summary>
        /// Flag indicating if this region is active or not
        /// </summary>
        [Browsable(false),
         CMDisplayName("Active"), 
         TypeConverter(typeof(CMBooleanConverter))]
        public bool Active
        {
            get { return _active; }
            protected set
            {
                SpyRegion curr = _container.ActiveRegion;
                if (value)
                {
                    if (curr != null && curr != this)
                        curr.Active = false;
                    _container.ActiveRegion = this;
                    _savedRect = _rect;
                }
                else if (curr == this)
                {
                    _container.ActiveRegion = null;
                }
                _active = value;
            }
        }

        /// <summary>
        /// Flag indicating if this region is currently selected
        /// </summary>
        [Browsable(false),
         CMDisplayName("Selected"), 
         TypeConverter(typeof(CMBooleanConverter))]
        public bool Selected { get { return this == _container.SelectedRegion; } }

        /// <summary>
        /// Flag indicating if this region is currently being hovered over
        /// </summary>
        [Browsable(false),
         CMDisplayName("Hovering"), 
         TypeConverter(typeof(CMBooleanConverter))]
        public bool Hovering { get { return (this == _container.HoverRegion); } }

        /// <summary>
        /// Flag indicating if mouse input is currently being processed by this
        /// region. It is ignored if the region is invisible and not selected.
        /// </summary>
        [Browsable(false),
         CMDisplayName("IsAcceptingMouseInput")]
        internal bool IsAcceptingMouseInput
        {
            get { return _visible || Selected; }
        }


        /// <summary>
        /// The color of the region.
        /// </summary>
        [Browsable(false),
         CMDisplayName("FillColor")]
        public Color FillColor
        {
            get
            {
                // depending on whether this region is selected, active or not, choose the base colour
                Color c;

                if (Active)
                    c = ActiveColour;
                else
                {
                    if (this == _container.SelectedRegion)
                        c = SelectedColour;
                    else
                        c = RegionColour;
                };


                // and if we're being hovered over, we want to lighten the color
                return (this == _container.HoverRegion ? Color.FromArgb(48, c) : c);
            }
        }

        /// <summary>
        /// The colour of the border to use when painting this region.
        /// </summary>
        [Browsable(false),
         CMDisplayName("BorderColor")]
        public Color BorderColor
        {
            get
            {
                // If the region is being hovered over then darken the border
                return (this == _container.HoverRegion) ? Color.FromArgb(150, FillColor) : FillColor;
            }
        }

        /// <summary>
        /// Whether paint a padding rectangle around this region
        /// </summary>
        [Browsable(false),
         CMDisplayName("ShowPadding")]
        private bool ShowPadding
        {
            get
            {
                return (_locationMethod == RegionLocationMethod.Image && (Selected || Hovering));
            }
        }

        /// <summary>
        /// A collection of lines that make up the relative region tree that this 
        /// region belongs to. A relative region tree is every possible way of
        /// joining up a collection of regions using the parent/child relationship defined
        /// by the Relative Parent property. Each line goes from the child region's
        /// top-left point to the parent region's top left point.
        /// </summary>
        [Browsable(false),
         CMDisplayName("RelativeRegionLines")]
        private IEnumerable<Line2D> RelativeRegionLines
        {
            get
            {
                var treeBuilder = new RelativeRegionTreeBuilder(this, _container.SpyRegions);
                var root = treeBuilder.Build();
                return root.GetLines();
            }
        }

        /// <summary>
        /// The rectangle used to display padding when painting a region
        /// </summary>
        [Browsable(false),
         CMDisplayName("PaddingRectangle")]
        private Rectangle PaddingRectangle
        {
            get
            {
                if (_rect == Rectangle.Empty) return Rectangle.Empty;

                // There is no targeted area specified, then set the padding rectangle
                // to the entirety of the containing element
                if (_regionPosition == RegionPosition.Anywhere)
                    return new Rectangle(0, 0, _container.Bitmap.Width, _container.Bitmap.Height);

                // Otherwise, create a rectangle around the image's original coordinates
                // plus the search padding area.
                return new Rectangle(
                            _rect.X - _imageSearchPadding.Left,
                            _rect.Y - _imageSearchPadding.Top,
                            _rect.Width + _imageSearchPadding.Horizontal,
                            _rect.Height + _imageSearchPadding.Vertical);
            }
        }


        /// <summary>
        /// The label to be displayed for this region.
        /// By default, no label is shown - the name is shown if this region is
        /// currently being hovered over
        /// </summary>
        [Browsable(false),
         CMDisplayName("Label")]
        public string Label
        {
            get
            {
                if (!(Hovering && IsAcceptingMouseInput)) return null;

                if (_regionPosition == RegionPosition.Anywhere)
                    return string.Format(Resources._0SearchesForAnImageAnywhereWithinTheParentElement, Name);

                var description = string.Empty;

                if (_locationMethod == RegionLocationMethod.Image)
                {
                    if (_regionPosition == RegionPosition.Relative && RelativeParent != null)
                    {
                        if (_imageSearchPadding != Padding.Empty && _locationMethod != RegionLocationMethod.Coordinates)
                            description =
                                string.Format(Resources.SearchesForAnImageAtAPositionRelativeTo0IncludingSearchPadding,
                                    RelativeParent.Name);
                        else
                            description = string.Format(Resources.SearchesForAnImageAtAPositionRelativeTo0,
                                RelativeParent.Name);
                    }
                    else
                    {
                        if (_imageSearchPadding != Padding.Empty && _locationMethod != RegionLocationMethod.Coordinates)
                            description = Resources.SearchesForAnImageAtAFixedPositionIncludingSearchPadding;
                        else
                            description = Resources.SearchesForAnImageAtAFixedPosition;
                    }
                }
                else
                {
                    if (_regionPosition == RegionPosition.Relative && RelativeParent != null)
                    {
                        if (_imageSearchPadding != Padding.Empty && _locationMethod != RegionLocationMethod.Coordinates)
                            description =
                                string.Format(
                                    Resources
                                        .SearchesForTheRegionUsingCoordinatesAtAPositionRelativeTo0IncludingSearchPadding,
                                    RelativeParent.Name);
                        else
                            description =
                                string.Format(Resources.SearchesForTheRegionUsingCoordinatesAtAPositionRelativeTo0,
                                    RelativeParent.Name);
                    }
                    else
                    {
                        if (_imageSearchPadding != Padding.Empty && _locationMethod != RegionLocationMethod.Coordinates)
                            description = Resources
                                .SearchesForTheRegionUsingCoordinatesAtAFixedPositionIncludingSearchPadding;
                        else
                            description = Resources.SearchesForTheRegionUsingCoordinatesAtAFixedPosition;
                    }
                }

                return $"{Name} {description}";
            }
        }

        /// <summary>
        /// The container in which this region is hosted.
        /// Setting this value does not add or remove the region from the container;
        /// it merely sets the value held in this region
        /// </summary>
        [Browsable(false),
         CMDisplayName("Container")]
        public ISpyRegionContainer Container
        {
            get { return _container; }
            set { _container = value; }
        }

        /// <summary>
        /// The distance at which guides should have an effect for this region.
        /// For moving regions, this is 0, ie. guides should only activate for other
        /// regions on the same X/Y line as this region; for resizing, it is 1 ie.
        /// guides should activate if they are on the same X/Y line or 1 pixel either
        /// side.
        /// Note that an active guide can cause a region to mutate from its current
        /// size/location to that of the guide.
        /// </summary>
        [Browsable(false),
         CMDisplayName("GuideDistance")]
        private int GuideDistance
        {
            get
            {
                if (_mode == ResizeMode.None) // moving or inactive
                    return 0;
                return 1;
            }
        }


        #endregion

        #region - Overridden methods -

        /// <summary>
        /// Returns a String representation of this region
        /// </summary>
        /// <returns>This region described as a string</returns>
        public override string ToString()
        {
            return "[" + _rect + "]" + (Active ? Resources.Active : "");
        }

        #endregion

        #region - Methods -

        /// <summary>
        /// Copies this region, creating an identical one on the same container as
        /// this one.
        /// </summary>
        /// <returns>A copy of this region within the same container that this
        /// region is on</returns>
        public virtual SpyRegion Copy()
        {
            return MemberwiseClone() as SpyRegion;
        }

        /// <summary>
        /// Removes a reference to the SpyRegions relative parent.
        /// </summary>
        public virtual void ClearRelativeParent()
        {
            RelativeParent = null;
        }

        /// <summary>
        /// Paints this region
        /// </summary>
        /// <param name="g">The graphics context with which this region should be
        /// painted</param>
        /// <param name="cache">The cache of pens and brushes to use for painting this
        /// region.</param>
        protected internal virtual void Paint(Graphics g, GDICache cache)
        {
            // more of a placeholder - we might want to increase the border size
            const int borderSize = 1;

            // only need to render if greyscale is true.
            // render the image first then draw on top.
            if (_displayedImage != null && Greyscale && Selected)
                g.DrawImage(_displayedImage, ValidDisplayArea());

            if (!_visible)
            {
                if (Selected)
                {
                    using (Pen semiPen = new Pen(BorderColor, borderSize))
                    {
                        semiPen.DashStyle = DashStyle.Dash;
                        g.DrawRectangle(semiPen, _rect);
                    }
                }
                return;
            }

            // Fill the main region rectangle
            Region fillRegion = new Region(_rect);
            if (_container.HoverRegion != null && this != _container.HoverRegion)
            {
                // Prevent the fill from covering any regions that are currently being
                // hovered over
                fillRegion.Exclude(_container.HoverRegion._rect);
                if (_container.HoverRegion.ShowPadding)
                    fillRegion.Exclude(_container.HoverRegion.PaddingRectangle);
            }
            g.FillRegion(cache.GetBrush(FillColor), fillRegion);
            g.DrawRectangle(cache.GetPen(BorderColor, borderSize), _rect);


            // If there's padding, fill in the section between the region and the padding
            // boundary using hatching
            if (_locationMethod == RegionLocationMethod.Image && ShowPadding)
            {
                using (HatchBrush hatchBrush = new HatchBrush(HatchStyle.DiagonalCross, BorderColor, Color.Transparent))
                {
                    // 
                    Region paddingRegion = new Region(PaddingRectangle);
                    paddingRegion.Exclude(_rect);
                    if (_container.HoverRegion != null && this != _container.HoverRegion)
                    {
                        // Prevent the hatching from covering any regions that are currently being
                        // hovered over
                        paddingRegion.Exclude(_container.HoverRegion._rect);
                        if (_container.HoverRegion.ShowPadding)
                            paddingRegion.Exclude(_container.HoverRegion.PaddingRectangle);
                    }

                    g.FillRegion(hatchBrush, paddingRegion);
                };

                using (Pen paddingPen = new Pen(BorderColor, borderSize))
                {
                    paddingPen.DashStyle = DashStyle.Dash;
                    g.DrawRectangle(paddingPen, PaddingRectangle);
                }

            }

            // If a region is being hovered over or is selected, paint a series of
            // arrows displaying how other regions are relative to it.
            using (Pen p = new Pen(BorderColor, borderSize))
            {
                if (Selected || Hovering)
                {
                    // Loop through each of the lines definining the relationships
                    // in the relative region tree
                    foreach (var line in RelativeRegionLines)
                    {
                        // Draw an arrow from the child to the parent, where the arrow
                        // appears halfway through the line
                        var halfwayPoint = new Point((line.StartPoint.X + line.EndPoint.X) / 2,
                            (line.StartPoint.Y + line.EndPoint.Y) / 2);

                        //First half of the line
                        p.EndCap = LineCap.ArrowAnchor;
                        AdjustableArrowCap bigArrow = new AdjustableArrowCap(5, 5);
                        p.CustomEndCap = bigArrow;
                        g.DrawLine(p, line.StartPoint, halfwayPoint);

                        //Second half of the line
                        p.EndCap = LineCap.NoAnchor;
                        g.DrawLine(p, halfwayPoint, line.EndPoint);

                        bigArrow.Dispose();
                    }
                }

            }
        }


        /// <summary>
        /// Adds guides for the intersection between this region and the given
        /// region, creating a guide if a relevant edge falls within a distance
        /// of 1 pixel either way of this regions edges.
        /// </summary>
        /// <param name="reg">The active region for which guides are required.
        /// </param>
        /// <param name="guides">An accumulative map of guides against their
        /// guide directions.</param>
        /// <returns>True if any guides were added; False otherwise</returns>
        internal bool AddGuides(SpyRegion reg, IDictionary<GuideCheck, Guide> guides)
        {
            return AddGuides(reg, guides, reg.GuideDistance);
        }

        /// <summary>
        /// Adds guides for the intersection between this region and the given
        /// region.
        /// </summary>
        /// <param name="reg">The active region for which guides are required.
        /// </param>
        /// <param name="guides">An accumulative map of guides against their
        /// guide directions.</param>
        /// <param name="distance">The number of pixels from the appropriate
        /// edge which would cause a guid to be created - eg. 0 indicates that
        /// the edge must match exactly to create a guide; 1 indicates that 1
        /// pixel either side of an edge would create a guide</param>
        /// <returns>True if any guides were added; False otherwise</returns>
        internal bool AddGuides(
            SpyRegion reg, IDictionary<GuideCheck, Guide> guides, int distance)
        {
            if (reg == this)
                return false;

            GuideCheck toCheck = __GuideCheckLookup[reg.Mode];
            Rectangle mine = _rect; // Local copy of this region's rectangle
            Rectangle mineExp = mine; // Expanded copy including the 'distance'
            mineExp.Inflate(distance, distance);
            Rectangle theirs = reg.Rectangle; // The region we're checking's rect

            // If their region is nowhere near ours, we can skip it completely
            if ((theirs.Left > mineExp.Right || theirs.Right < mineExp.Left) &&
                (theirs.Top > mineExp.Bottom || theirs.Bottom < mineExp.Top))
                return false;

            // So we have some intersection, get the relevant guide check value for
            // the mode of the check region and test it against this region's
            // relevant values
            int initCount = guides.Count;

            // If we need to check their left border, check if it is within 1px
            // either way of this region's left or right borders.
            // If either is true, add a vertical guide for the appropriate border
            if ((toCheck & GuideCheck.Left) != 0)
            {
                if (Math.Abs(mine.Left - theirs.Left) <= distance)
                    guides[GuideCheck.Left] = new VerticalGuide(mine.Left);
                else if (Math.Abs(mine.Right - theirs.Left) <= distance)
                    guides[GuideCheck.Left] = new VerticalGuide(mine.Right);
            }

            // As above, so below...
            if ((toCheck & GuideCheck.Right) != 0)
            {
                if (Math.Abs(mine.Right - theirs.Right) <= distance)
                    guides[GuideCheck.Right] = new VerticalGuide(mine.Right);
                else if (Math.Abs(mine.Left - theirs.Right) <= distance)
                    guides[GuideCheck.Right] = new VerticalGuide(mine.Left);
            }
            if ((toCheck & GuideCheck.Top) != 0)
            {
                if (Math.Abs(mine.Top - theirs.Top) <= distance)
                    guides[GuideCheck.Top] = new HorizontalGuide(mine.Top);
                else if (Math.Abs(mine.Bottom - theirs.Top) <= distance)
                    guides[GuideCheck.Top] = new HorizontalGuide(mine.Bottom);
            }
            if ((toCheck & GuideCheck.Bottom) != 0)
            {
                if (Math.Abs(mine.Top - theirs.Bottom) <= distance)
                    guides[GuideCheck.Bottom] = new HorizontalGuide(mine.Top);
                else if (Math.Abs(mine.Bottom - theirs.Bottom) <= distance)
                    guides[GuideCheck.Bottom] = new HorizontalGuide(mine.Bottom);
            }

            // Check against the original count to see if we've added any
            return (guides.Count > initCount);
        }

        /// <summary>
        /// Checks if this region (feathered by the <see cref="HoverZoneOffset"/>)
        /// contains the given point.
        /// </summary>
        /// <param name="p">The point to check</param>
        /// <returns>True if the point falls within this region or its 'hover
        /// zone' - a distance of <see cref="HoverZoneOffset"/> pixels beyond this
        /// region's border.</returns>
        public bool Contains(Point p)
        {
            Rectangle r = _rect;
            r.Inflate(HoverZoneOffset * 2, HoverZoneOffset * 2);
            r.Offset(-HoverZoneOffset, -HoverZoneOffset);
            return (r.Contains(p));
        }

        /// <summary>
        /// Raises the RectangleChanged event for this region.
        /// </summary>
        protected virtual void OnRegionLayoutChanged(EventArgs e)
        {
            if (RegionLayoutChanged != null)
                RegionLayoutChanged(this, e);
        }

        /// <summary>
        /// Raises the RectangleChanged event for this region.
        /// </summary>
        protected virtual void OnRegionLayoutChanging(EventArgs e)
        {
            if (RegionLayoutChanging != null)
                RegionLayoutChanging(this, e);
        }

        /// <summary>
        /// Raises the CharacterDataChanged event for this region
        /// </summary>
        protected virtual void OnCharacterDataChanged(RegionCharDataChangeEventArgs e)
        {
            if (CharacterDataChanged != null)
                CharacterDataChanged(this, e);
        }

        /// <summary>
        /// Raises the NameChanging event for this region
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnNameChanging(NameChangingEventArgs e)
        {
            if (NameChanging != null)
                NameChanging(this, e);
        }

        /// <summary>
        /// Raises the NameChanged event for this region.
        /// </summary>
        protected virtual void OnNameChanged(EventArgs e)
        {
            if (NameChanged != null)
                NameChanged(this, e);
        }

        /// <summary>
        /// Handles the mouse down event on this region.
        /// </summary>
        /// <param name="p">The point (within the context of the owning picture
        /// box) at which the mouse was held down.</param>
        public void HandleMouseDown(Point p)
        {
            if (!IsAcceptingMouseInput)
                return;

            _mode = GetPotentialMode(p);
            Active = true;
            _anchor = p;
        }

        /// <summary>
        /// Sets this region from a resize, using the given parameters, and
        /// <em>not</em> enforcing a minimum size.
        /// </summary>
        /// <param name="mode">The resize mode to set from.</param>
        /// <param name="p">The point within the parent control that should be
        /// used as the 'active' point for the resize.</param>
        /// <param name="committed">true to 'commit' the change, false to leave the
        /// change open-ended - in real terms, this decides whether the event fired
        /// as a result of this resize is "LayoutChanging" (open-ended) or
        /// "LayoutChanged" (committed)</param>
        private void SetFromResize(Point p, bool committed)
        {
            SetFromResize(p, committed, false);
        }

        /// <summary>
        /// Sets this region from a resize, using the given parameters, enforcing
        /// a minimum size as necessary
        /// </summary>
        /// <param name="mode">The resize mode to set from.</param>
        /// <param name="p">The point within the parent control that should be
        /// used as the 'active' point for the resize.</param>
        /// <param name="committed">true to 'commit' the change, false to leave the
        /// change open-ended - in real terms, this decides whether the event fired
        /// as a result of this resize is "LayoutChanging" (open-ended) or
        /// "LayoutChanged" (committed)</param>
        /// <param name="enforceMinSize">true to enforce a minimum size for the
        /// region of 5x5 pixels; false to allow any size (including 0x0)</param>
        private void SetFromResize(Point p, bool committed, bool enforceMinSize)
        {
            Rectangle rect = _rect;

            // Either translate or expand/contract the rectangle according to
            // the resize mode and the relative location of the active point.
            switch (_mode)
            {
                case ResizeMode.BottomRight:
                    rect.Size = ((Size)p - (Size)rect.Location);
                    break;

                case ResizeMode.TopLeft:
                    Point to = new Point(rect.Right, rect.Bottom);
                    rect = new Rectangle(p, (Size)(to - (Size)p));
                    break;

                case ResizeMode.TopRight:
                    rect.Width = p.X - rect.X;
                    rect.Height = rect.Height + (rect.Y - p.Y);
                    rect.Y = p.Y;
                    break;

                case ResizeMode.BottomLeft:
                    rect.Height = p.Y - rect.Y;
                    rect.Width = rect.Width + (rect.X - p.X);
                    rect.X = p.X;
                    break;

                case ResizeMode.Right:
                    rect.Width = p.X - rect.X;
                    break;

                case ResizeMode.Bottom:
                    rect.Height = p.Y - rect.Y;
                    break;

                case ResizeMode.Left:
                    rect.Width = rect.Width + (rect.X - p.X);
                    rect.X = p.X;
                    break;

                case ResizeMode.Top:
                    rect.Height = rect.Height + (rect.Y - p.Y);
                    rect.Y = p.Y;
                    break;

                default: // ie. moving, rather than resizing.
                    rect.Offset(p - (Size)_anchor);
                    _anchor = p;
                    break;
            }

            // check if we have a negative width / height - if we do,
            // relocate the rectangle such that it describes the same region
            // but has positive width / height
            int w = rect.Width;
            int h = rect.Height;
            if (w < 0)
            {
                // { 10, 10, -5, 5 } => { 5, 10, 5, 5 }
                rect.Width = -w;
                rect.X = rect.X + w; // effectively a subtract since "w" is negative
                switch (_mode)
                {
                    case ResizeMode.BottomRight:
                        _mode = ResizeMode.BottomLeft;
                        break;
                    case ResizeMode.Right:
                        _mode = ResizeMode.Left;
                        break;
                    case ResizeMode.TopRight:
                        _mode = ResizeMode.TopLeft;
                        break;

                    case ResizeMode.BottomLeft:
                        _mode = ResizeMode.BottomRight;
                        break;
                    case ResizeMode.Left:
                        _mode = ResizeMode.Right;
                        break;
                    case ResizeMode.TopLeft:
                        _mode = ResizeMode.TopRight;
                        break;
                }
            }
            if (h < 0)
            {
                rect.Height = -h;
                rect.Y = rect.Y + h;
                switch (_mode)
                {
                    case ResizeMode.TopLeft:
                        _mode = ResizeMode.BottomLeft;
                        break;
                    case ResizeMode.Top:
                        _mode = ResizeMode.Bottom;
                        break;
                    case ResizeMode.TopRight:
                        _mode = ResizeMode.BottomRight;
                        break;

                    case ResizeMode.BottomLeft:
                        _mode = ResizeMode.TopLeft;
                        break;
                    case ResizeMode.Bottom:
                        _mode = ResizeMode.Top;
                        break;
                    case ResizeMode.BottomRight:
                        _mode = ResizeMode.TopRight;
                        break;
                }
            }

            // If we're enforcing minimum size, check our resize mode and
            // set the appropriate value (eg. if we're resizing on the right
            // and the min width hasn't been reached, move the right border (width);
            // if resizing on the left, move the left border (X)
            if (enforceMinSize)
            {
                if (rect.Width < MinimumWidth)
                {
                    switch (_mode)
                    {
                        case ResizeMode.Right:
                        case ResizeMode.TopRight:
                        case ResizeMode.BottomRight:
                            rect.Width = MinimumWidth;
                            break;
                        case ResizeMode.Left:
                        case ResizeMode.TopLeft:
                        case ResizeMode.BottomLeft:
                            rect.X = rect.Right - MinimumWidth;
                            // Don't allow rectangle to go off screen
                            if (rect.X < 0)
                                rect.X = 0;
                            rect.Width = MinimumWidth;
                            break;
                    }
                }
                if (rect.Height < MinimumHeight)
                {
                    switch (_mode)
                    {
                        case ResizeMode.Bottom:
                        case ResizeMode.BottomLeft:
                        case ResizeMode.BottomRight:
                            rect.Height = MinimumHeight;
                            break;
                        case ResizeMode.Top:
                        case ResizeMode.TopLeft:
                        case ResizeMode.TopRight:
                            rect.Y = rect.Bottom - MinimumHeight;
                            if (rect.Y < 0)
                                rect.Y = 0;
                            rect.Height = MinimumHeight;
                            break;
                    }
                }
            }
            // Set the rectangle into the region - other regions need to
            // know it now. Don't fire any events, though - we're not done yet
            InnerRectangle = rect;

            // check if our location puts us near other boxes, and add guides if
            // we're within a pixel of them
            foreach (KeyValuePair<GuideCheck, Guide> pair in _container.ApplyGuides(this))
            {
                switch (pair.Key)
                {
                    case GuideCheck.Top:
                        int oldTop = rect.Top;
                        rect.Location = new Point(rect.Left, pair.Value.Y);
                        rect.Height += (oldTop - rect.Top);
                        break;

                    case GuideCheck.Bottom:
                        rect.Height = pair.Value.Y - rect.Top;
                        break;

                    case GuideCheck.Left:
                        int oldLeft = rect.Left;
                        rect.Location = new Point(pair.Value.X, rect.Top);
                        rect.Width += (oldLeft - rect.Left);
                        break;

                    case GuideCheck.Right:
                        rect.Width = pair.Value.X - rect.Left;
                        break;
                }
            }

            // Set the new rectangle, firing events as appropriate
            // We want to ensure that a LayoutChanged event is fired even
            // when the rectangle is the same as the last LayoutChanging
            // event, so we take on the event handling ourself (rather than
            // leaving it to the [Inner]Rectangle property).
            InnerRectangle = rect;

            if (committed) // Committed: Send Changed event if changed
            {
                if (_rect != _savedRect)
                    OnRegionLayoutChanged(EventArgs.Empty);
            }
            else
            {
                // Open Ended - ie. still active: Send Changing event
                OnRegionLayoutChanging(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Handles the mouse being moved on this region
        /// </summary>
        /// <param name="p">The location of the mouse cursor when the mouse
        /// move message was sent.</param>
        public void HandleMouseMove(Point p)
        {
            // If this region is active (ie. currently being sized / moved),
            // handle the translation / resizing of the rectangle and the
            // displayed image on the picture box.
            if (_active)
            {
                SetFromResize(p, false);
                DisplayedImage = GetRegionImage(Greyscale);
            }
            // Otherwise, check if the mouse could enter a resize mode
            // on this region.
            else if (IsAcceptingMouseInput)
            {
                _mode = GetPotentialMode(p);
            }
        }


        /// <summary>
        /// Gets an Image with a copy of the pixels from the container region within
        /// the current regions rectangle.  Set the image to be a greyscaled if
        /// greyscale is set to true. 
        /// </summary>
        /// <returns>An Image within the selected region rectangle. </returns>
        private Image GetRegionImage(bool useGreyscale)
        {
            // We know there's a container and we know it has a bitmap because
            // this cannot be 'WithinContainerImage' otherwise.

            const int MinimumRectDepth = 1;
            if (_rect.Width <= MinimumRectDepth || _rect.Height <= MinimumRectDepth)
                return null;

            Bitmap containerBitmap = _container.Bitmap;
            if (containerBitmap == null)
                return null;

            Rectangle validRect = ValidDisplayArea();

            using (var image = containerBitmap.Clone(validRect, containerBitmap.PixelFormat))
            {
                return useGreyscale ? ImageBender.GrayscaleImage(image, null) : (Image)image.Clone();
            }
        }

        /// <summary>
        /// Creates a Rectangle that is the correct area a region currently sits it,
        /// whether it is partly off the edge of the container or not.
        /// </summary>
        /// <returns>A rectangle reflecting the coordinates and area of the region. </returns>
        private Rectangle ValidDisplayArea()
        {
            Rectangle bmpRect = new Rectangle(Point.Empty, _container.Bitmap.Size);
            Rectangle validRect = _rect;
            validRect.Intersect(bmpRect);
            return validRect;
        }

        /// <summary>
        /// Handles the mouse button being released on this region.
        /// </summary>
        /// <param name="point">The location at which the mouse button was
        /// released</param>
        public void HandleMouseUp(Point point)
        {
            if (!_active || !IsAcceptingMouseInput)
                return;
            Active = false;

            if (_rect.X <= 1 || point.X < 1)
                return;

            SetFromResize(point, true, true);
        }

        /// <summary>
        /// Gets the potential resize mode for this region at the given point.
        /// </summary>
        /// <param name="p">The location of the mouse cursor for the purpose
        /// of getting the potential resize mode for this region.</param>
        /// <returns>The potential resize mode for this region - <see
        /// cref="ResizeMode.None"/> if the cursor is not on a region edge.
        /// </returns>
        public ResizeMode GetPotentialMode(Point p)
        {
            if (!IsAcceptingMouseInput)
                return ResizeMode.None;

            int x = _rect.Left, y = _rect.Top, xTo = _rect.Right, yTo = _rect.Bottom;
            int px = p.X, py = p.Y;

            // test for out of this region's range
            if (px < x - HoverZoneOffset || px > xTo + HoverZoneOffset
                || py < y - HoverZoneOffset || py > yTo + HoverZoneOffset)
                return ResizeMode.None;

            // inside the region - not near the edge
            if (px > x + HoverZoneOffset && px < xTo - HoverZoneOffset
                && py > y + HoverZoneOffset && py < yTo - HoverZoneOffset)
                return ResizeMode.None;

            // flags to indicate which regions the pointer is in.
            bool left = false;
            bool top = false;
            bool bottom = false;
            bool right = false;

            // Lean towards bottom right on the probably incorrect assumption that
            // that is the most likely one to be used.
            // This also ensures that for an empty rectangle, bottom-right will be
            // the default mode used to expand the rectangle
            right = (px >= xTo - HoverZoneOffset && px <= xTo + HoverZoneOffset); // in right hand region
            if (!right)
                left = (px >= x - HoverZoneOffset && px <= x + HoverZoneOffset); // in left hand region

            bottom = (py >= yTo - HoverZoneOffset && py <= yTo + HoverZoneOffset);
            if (!bottom)
                top = (py >= y - HoverZoneOffset && py <= y + HoverZoneOffset);

            if (bottom && right)
                return ResizeMode.BottomRight;
            else if (bottom && left)
                return ResizeMode.BottomLeft;
            else if (top && left)
                return ResizeMode.TopLeft;
            else if (top && right)
                return ResizeMode.TopRight;
            else if (right)
                return ResizeMode.Right;
            else if (bottom)
                return ResizeMode.Bottom;
            else if (left)
                return ResizeMode.Left;
            else if (top)
                return ResizeMode.Top;

            return ResizeMode.None;
        }

        /// <summary>
        /// Resets the RetainImage property
        /// </summary>
        private void ResetRetainImage()
        {
            _retainImage = false;
        }

        /// <summary>
        /// Resets the name property to its original name
        /// </summary>
        private void ResetName()
        {
            Name = _initName;
        }

        /// <summary>
        /// Find this region's top level anchor, i.e. the first region that is searched
        /// for when attempting to find this region
        /// </summary>
        /// <returns>This region's top level anchor</returns>
        internal SpyRegion GetTopLevelAnchor()
        {
            var x = this;
            while (x._relativeParent != null)
            {
                x = x._relativeParent;
            };

            return x;
        }

        #endregion

    }

}
