using BluePrism.CharMatching.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BluePrism.BPCoreLib.Collections;
using BluePrism.BPCoreLib;
using BluePrism.Server.Domain.Models;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;
using AutomateControls;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Control used to map regions onto an image
    /// </summary>
    public partial class RegionMapper : UserControl, IImageHandler, ISpyRegionContainer
    {
        #region - Inner Classes / Enums -

        /// <summary>
        /// Enumeration of the tools which can be selected in this control
        /// </summary>
        private enum ToolRegionType { Any, BasicRegion, ListRegion, GridRegion }

        /// <summary>
        /// Enumeration of the behaviours for creating a region
        /// </summary>
        private enum ToolRegionBehaviour { Always, Never, IfNotOnElement }

        /// <summary>
        /// Enumeration of type of action the tool performs - creation or selection.
        /// </summary>
        private enum ToolAction { Create, Select }

        /// <summary>
        /// A tool representing one of the available tools within this control
        /// </summary>
        private class Tool
        {
            // The name of the tool
            private ToolRegionType _regType;

            // The behaviour of the tool with respect to regions
            private ToolRegionBehaviour _behaviour;

            // Whether this tool creates or selects regions
            private ToolAction _action;

            /// <summary>
            /// Creates a new select tool with the given name and behaviour
            /// </summary>
            /// <param name="name">The name of the tool</param>
            /// <param name="behaviour">The behaviour of the tool</param>
            public Tool(ToolRegionType name, ToolRegionBehaviour behaviour)
                : this(name, behaviour, ToolAction.Select) { }

            /// <summary>
            /// Creates a new tool with the given properties
            /// </summary>
            /// <param name="name">The name of the tool</param>
            /// <param name="behaviour">The behaviour of the tool</param>
            /// <param name="action">Whether the tool creates or selects
            /// regions.</param>
            public Tool(ToolRegionType name, ToolRegionBehaviour behaviour, ToolAction action)
            {
                _regType = name;
                _behaviour = behaviour;
                _action = action;
            }

            /// <summary>
            /// The behaviour of the tool
            /// </summary>
            public ToolRegionBehaviour RegionBehaviour
            {
                get { return _behaviour; }
            }

            /// <summary>
            /// Whether the tool creates or selects regions
            /// </summary>
            public ToolAction Action
            {
                get { return _action; }
            }

            public SpyRegion GenerateStubRegion(RegionMapper mapper, Point locn)
            {
                if (_action == ToolAction.Select)
                    return null;
                Rectangle stubRect = new Rectangle(locn, Size.Empty);
                switch (_regType)
                {
                    case ToolRegionType.Any:
                    default:
                        return null;
                    case ToolRegionType.BasicRegion:
                        return mapper.RegisterRegion(new SpyRegion(mapper, stubRect));
                    case ToolRegionType.ListRegion:
                        return mapper.RegisterRegion(new ListSpyRegion(mapper, stubRect));
                    case ToolRegionType.GridRegion:
                        return mapper.RegisterRegion(new GridSpyRegion(mapper, stubRect));
                }
            }
        }

        /// <summary>
        /// Flasher class, used to flash a region.
        /// </summary>
        private class Flasher
        {
            // The rectangle describing the flashing area
            private Rectangle _r;

            // The remaining number of times that the area should flash
            private int _count;

            /// <summary>
            /// Creates a new flasher object for the given rectangle, and setting it
            /// to flash the specified number of times.
            /// </summary>
            /// <param name="r">The rectangle describing the area to flash.</param>
            /// <param name="count">The number of times the rectangle should flash.
            /// </param>
            public Flasher(Rectangle r, int count)
            {
                _r = r;
                _count = count;
            }

            /// <summary>
            /// Checks if this object has any remaining flashes left, decrementing the
            /// count of them if it does.
            /// </summary>
            /// <returns>True if any more flashes should occur for this object;
            /// False otherwise.</returns>
            public bool Flash()
            {
                return (--_count >= 0);
            }
            
            /// <summary>
            /// The rectangle describing the area which should be flashed.
            /// </summary>
            public Rectangle Rectangle { get { return _r; } }
        }

        /// <summary>
        /// ZoomLevel structure for containing the zoom level currently set
        /// in the picture box. Instances of this structure are used in the
        /// combo box with which the zoom level can be set.
        /// </summary>
        private struct ZoomLevel
        {
            // The default zoom level - 100%
            public static ZoomLevel Default = new ZoomLevel(100);

            // The percent zoom represented by this object.
            private readonly int _percent;

            /// <summary>
            /// Creates a new zoom level instance representing the given percentage
            /// zoom level.
            /// </summary>
            /// <param name="percent">The number of percent that the zoom level
            /// should represent.</param>
            public ZoomLevel(int percent)
            {
                _percent = percent;
            }

            /// <summary>
            /// The percentage zoom represented by this object.
            /// </summary>
            public int Percent { get { return _percent; } }

            /// <summary>
            /// The zoom factor represented by this object.
            /// </summary>
            public float Factor { get { return (float)_percent / 100.0f; } }

            /// <summary>
            /// Checks if this object represents the same zoom level as the given
            /// object.
            /// </summary>
            /// <param name="obj">The object to check this zoom level against.</param>
            /// <returns>True if the given object is a ZoomLevel with the same value
            /// as this object.</returns>
            public override bool Equals(object obj)
            {
                return (obj is ZoomLevel && ((ZoomLevel)obj)._percent == _percent);
            }

            /// <summary>
            /// Gets a hashcode representing this zoom level.
            /// </summary>
            /// <returns>An integer hash representing this zoom level.</returns>
            public override int GetHashCode()
            {
                return 0x01020304 ^ _percent;
            }

            /// <summary>
            /// Gets a string representation of this zoom level.
            /// </summary>
            /// <returns>A string representation of this zoom level.</returns>
            public override string ToString()
            {
                return _percent + "%";
            }

            /// <summary>
            /// Checks if the two given zoom levels are equal.
            /// </summary>
            /// <param name="one">The first zoom level to check</param>
            /// <param name="two">The second zoom level to check</param>
            /// <returns>True if the two zoom levels are equal according to
            /// the <see cref="Equals"/> override; False otherwise.</returns>
            public static bool operator ==(ZoomLevel one, ZoomLevel two)
            {
                return one.Equals(two);
            }

            /// <summary>
            /// Checks if the two given zoom levels are not equal.
            /// </summary>
            /// <param name="one">The first zoom level to check</param>
            /// <param name="two">The second zoom level to check</param>
            /// <returns>False if the two zoom levels are equal according to
            /// the <see cref="Equals"/> override; True otherwise.</returns>
            public static bool operator !=(ZoomLevel one, ZoomLevel two)
            {
                return !one.Equals(two);
            }
        }

        #endregion

        #region - Other Static Definitions -

        /// <summary>
        /// A regular expression used to parse a percentage value, with an
        /// option decimal point and percent sign.
        /// </summary>
        private static readonly Regex _zoomRegex = new Regex(@"^\s*(\d*\.?\d*)\s*%?\s*$", RegexOptions.None, RegexTimeout.DefaultRegexTimeout);

        private static readonly Regex _bmpLine = new Regex("^bitmap=(.*)$");

        // The last directory opened when opening an image from this mapper
        private static string _lastDirectory = null;

        #endregion

        #region - Published Events -

        /// <summary>
        /// Event indicating that a 'Spy' operation has been requested.
        /// </summary>
        [Category("Action"),
         Description("Occurs when a spy operation is requested on this mapper")]
        public event SpyRequestEventHandler SpyRequested;

        /// <summary>
        /// Event indicating that a region has been selected.
        /// </summary>
        [Category("Regions"),
         Description("Occurs when the region selection changes")]
        public event SpyRegionEventHandler RegionSelected;

        /// <summary>
        /// Event indicating that the layout of a region has changed
        /// </summary>
        [Category("Regions"),
         Description("Occurs when the layout of a region has changed")]
        public event SpyRegionEventHandler RegionLayoutChanged;

        /// <summary>
        /// Event indicating that the data within a region has changed. This is fired
        /// for all changes which are not layout-related.
        /// </summary>
        [Category("Regions"),
         Description("Occurs when the character data of a region has changed")]
        public event SpyRegionEventHandler RegionCharDataChanged;

        #endregion

        #region - Instance Members -

        // Timer used for flashing rectangles within the picture.
        private Timer flashTimer = new Timer();

        // A map of Tool objects against the toolstrip buttons which represent them
        private readonly IDictionary<ToolStripButton, Tool> _tools;

        // The set of regions held in this region mapper
        private IBPSet<SpyRegion> _regions;

        // The list of regions in a binding list - used for the combo box of regions
        private BindingList<SpyRegion> _regionList;

        // The spy region being hovered over - null if none is being hovered over
        // Note that if multiple regions are being hovered over, one is nominated as
        // the top - this is the _hover value and the one which will be selected if
        // the mouse button is pressed.
        private SpyRegion _hover;

        // The currently selected region, or null if one is not selected
        private SpyRegion _selected;

        // The currently active region - ie. the region that is currently being
        // moved or resized.
        private SpyRegion _active;
        
        // State flag indicating that a toolbar button click is currently being
        // handled - some CheckedChanged events might fire while a click is being
        // processed, and the handlers for such events should be squashed.
        private bool _handlingTools;

        // The nullable rectangle which is to be flashed.
        private Rectangle? _flashRect;

        // The map of current active guides in this mapper - 
        private IDictionary<GuideCheck, Guide> _guides;

        // The font used for the region labels
        private Font _labelFont = null;

        // Buffer to hold cut / pasted spy regions.
        private ICollection<SpyRegion> _buffer = new clsSet<SpyRegion>();

        // The font store for this region mapper
        private IFontStore _store;

        // Flag indicating if this mapper is open in single-region mode or not
        private bool _singleRegionMode;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new empty RegionMapper control
        /// </summary>
        public RegionMapper()
        {
            InitializeComponent();
            this.SetStyle(
             ControlStyles.Selectable | ControlStyles.OptimizedDoubleBuffer
             | ControlStyles.AllPaintingInWmPaint, true);
            Dictionary<ToolStripButton, Tool> toolmap = new Dictionary<ToolStripButton, Tool>();
            toolmap[btnPointer] = new Tool(ToolRegionType.Any, ToolRegionBehaviour.IfNotOnElement);
            toolmap[btnRegion] = new Tool(ToolRegionType.BasicRegion, ToolRegionBehaviour.Always, ToolAction.Create);
            toolmap[btnList] = new Tool(ToolRegionType.ListRegion, ToolRegionBehaviour.Always, ToolAction.Create);
            toolmap[btnGrid] = new Tool(ToolRegionType.GridRegion, ToolRegionBehaviour.Always, ToolAction.Create);
            _tools = GetReadOnly.IDictionary(toolmap);
            btnPointer.Checked = true;

            foreach (int zl in new int[] { 25, 50, 75, 100, 150, 200, 250, 500 })
            {
                cmbZoom.Items.Add(new ZoomLevel(zl));
            }
            cmbZoom.SelectedItem = ZoomLevel.Default;
            cmbZoom.SelectedIndexChanged += new EventHandler(HandleZoomSelected);
            cmbZoom.Validating += new CancelEventHandler(HandleValidatingZoom);
            cmbZoom.Validated += new EventHandler(HandleZoomValidated);
            cmbZoom.KeyPress += new KeyPressEventHandler(HandleZoomKeypress);
            picbox.ZoomLevelChanged += new ZoomLevelChangeHandler(HandlePicboxZoomChanged);

            _regions = new clsOrderedSet<SpyRegion>();
            _regionList = new SortedBindingList<SpyRegion>(
                TypeDescriptor.GetProperties(typeof(SpyRegion))["Name"]);
            cmbRegions.DisplayMember = "Name";
            cmbRegions.DataSource = _regionList;

            flashTimer.Interval = 500;
            flashTimer.Tick += HandleFlasherTick;
            flashTimer.Start();

            propsElement.PropertyValueChanged += HandlePropertyValueChanged;
            ctxMenuPropsGrid.Opening += new CancelEventHandler(HandlePropsContextMenuOpening);
            cmbRegions.SelectedIndexChanged += HandleSpyRegionSelected;
            _guides = GetEmpty.IDictionary<GuideCheck, Guide>();

            KeyDown += new KeyEventHandler(HandleKeyDown);
            picbox.KeyDown += new KeyEventHandler(HandlePictureBoxKeyDown);

        }

        #endregion

        #region - Properties -

        /// <summary>
        /// The font store using which the installed fonts can be loaded and saved
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IFontStore Store
        {
            get { return _store; }
            set { _store = value; }
        }

        /// <summary>
        /// The image currently being used to map regions for this control
        /// </summary>
        [Browsable(true),
         Description("The image displayed in the region mapper"),
         DefaultValue(null)]
        public Image Image
        {
            get { return picbox.Image; }
            set { picbox.Image = value; }
        }

        /// <summary>
        /// Whether the spy button is available in this mapper or not.
        /// </summary>
        [Browsable(true),
         Description("Whether the spy button is available in this mapper"),
         DefaultValue(false)]
        public bool SpyButtonAvailable
        {
            get { return btnOpenImage.Visible; }
            set { btnOpenImage.Visible = value; }
        }

        [Browsable(true),
         Description("Allow one simple non-configurable region at once"),
         DefaultValue(false)]
        public bool SingleRegionMode
        {
            get { return _singleRegionMode; }
            set
            {
                if (value == _singleRegionMode)
                    return;
                _singleRegionMode = value;
                foreach (ToolStripItem item in toolstrip.Items)
                {
                    item.Visible =(!value ||
                        item == btnPointer || item == btnRegion || item == cmbZoom);
                }
                splitMain.Panel2.Enabled = !value;
                splitMain.Panel2Collapsed = value;
            }
        }

        /// <summary>
        /// The currently selected spy region, or null if no region is currently
        /// selected.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SpyRegion SelectedRegion
        {
            get { return _selected; }
            set
            {
                SpyRegion oldReg = _selected;
                _selected = value;
                if (oldReg != _selected)
                {
                    // Tell TypeDescriptor to use our custom descriptor provider for this instance
                    // Note that this AddProvider overload references our instance using a weak 
                    // reference so we don't have any tidying up to do
                    if (value != null)
                    {
                        TypeDescriptor.AddProvider(new SpyRegionTypeDescriptorProvider(), value);
                    }
                    propsElement.SelectedObject = value;
                    cmbRegions.SelectedItem = value;
                    OnRegionSelected(new SpyRegionEventArgs(value));
                }
            }
        }

        /// <summary>
        /// The region currently registered as that being hovered over, or null
        /// if no such region is being hovered
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SpyRegion HoverRegion
        {
            get { return _hover; }
            set { _hover = value; }
        }

        /// <summary>
        /// The region currently registered as the active region (ie. the region
        /// being moved or resized).
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SpyRegion ActiveRegion
        {
            get { return _active; }
            set { _active = value; }
        }

        /// <summary>
        /// Gets the installed font names for this region container.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ICollection<string> InstalledFontNames
        {
            get { return _store.AvailableFontNames; }
        }

        /// <summary>
        /// The image currrently being used to map regions for this control, as
        /// a Bitmap object.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Bitmap Bitmap
        {
            get
            {
                Image img = this.Image;
                if (img == null)
                    return null;
                Bitmap b = img as Bitmap;
                if (b == null)
                    b = new Bitmap(img);
                return b;
            }
        }

        /// <summary>
        /// An image representing the sub-image described by the currently
        /// selected region.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image SelectedImageRegion
        {
            get { return (SelectedRegion == null ? null : SelectedRegion.Image); }
        }

        /// <summary>
        /// The currently selected tool button - ie. for toggling tool buttons,
        /// that which is currently toggled on.
        /// </summary>
        private ToolStripButton SelectedToolButton
        {
            get
            {
                foreach (ToolStripItem item in toolstrip.Items)
                {
                    ToolStripButton btn = item as ToolStripButton;
                    if (btn != null && btn.Checked)
                        return btn;
                }
                return null;
            }
        }

        /// <summary>
        /// The currently active tool within this region mapper
        /// </summary>
        private Tool CurrentTool
        {
            get
            {
                ToolStripButton btn = SelectedToolButton;
                if (btn == null)
                    return null;
                return _tools[btn];
            }
        }

        /// <summary>
        /// The font to use for the region label
        /// </summary>
        private Font LabelFont
        {
            get
            {
                if (_labelFont == null)
                    _labelFont = new Font(this.Font, FontStyle.Bold);
                return _labelFont;
            }
        }

        /// <summary>
        /// Gets or sets the regions associated with this mapper
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ICollection<SpyRegion> SpyRegions
        {
            get { return GetReadOnly.ICollection<SpyRegion>(_regions); }
            set
            {
                ClearRegions();
                AddRegions(value);
                if (_regionList.Count > 0)
                {
                    this.SelectedRegion = (SpyRegion)_regionList[0];
                }
            }
        }

        #endregion

        #region - Find XXX Regions -

        /// <summary>
        /// Finds the guides applicable to the given region.
        /// </summary>
        /// <param name="region">The region to check other regions for guides
        /// </param>
        /// <returns>A map of guides against the relevant guide check border
        /// relating to the given region.</returns>
        private IDictionary<GuideCheck, Guide> FindGuides(SpyRegion region)
        {
            IDictionary<GuideCheck, Guide> guides =
                new Dictionary<GuideCheck, Guide>();
            foreach (SpyRegion reg in _regions)
            {
                reg.AddGuides(region, guides);
            }
            if (guides.Count == 0)
                return GetEmpty.IDictionary<GuideCheck, Guide>();
            return guides;
        }

        /// <summary>
        /// Finds the guides relating to the given region, applies them to this
        /// region mapper and returns them.
        /// </summary>
        /// <param name="reg">The region for which guides should be found and
        /// applied.</param>
        /// <returns>The map of guides set in this region mapper, derived from
        /// the given region.</returns>
        IDictionary<GuideCheck, Guide> ISpyRegionContainer.ApplyGuides(SpyRegion reg)
        {
            return (_guides = FindGuides(reg));
        }

        /// <summary>
        /// Clears the guides currently applied to this region mapper.
        /// </summary>
        private void ClearGuides()
        {
            if (_guides.Count > 0)
                _guides = GetEmpty.IDictionary<GuideCheck, Guide>();
        }

        /// <summary>
        /// Finds all regions which contain the given point
        /// </summary>
        /// <param name="point">The location at which the regions are required.
        /// </param>
        /// <returns>A readonly collection of spy regions which exist at the
        /// given location.</returns>
        private ICollection<SpyRegion> FindRegions(Point point)
        {
            ICollection<SpyRegion> coll = null;
            foreach (SpyRegion region in _regions)
            {
                if (region.Contains(point))
                {
                    if (coll == null)
                        coll = new List<SpyRegion>();
                    coll.Add(region);
                }
            }
            if (coll == null)
                return GetEmpty.ICollection<SpyRegion>();
            return coll;
        }

        /// <summary>
        /// Finds the most likely region at the given point - ie. that which
        /// will be made active if the user clicks the mouse.
        /// Regions whose borders are at the given point are given precedence
        /// over those whose regions contain the given point; Later regions
        /// are given precedence over earlier created ones.
        /// </summary>
        /// <param name="p">The point that a region is required for.</param>
        /// <returns>The region which will be selected if the mouse is clicked
        /// at the given point, or null if no region exists at the given location.
        /// </returns>
        private SpyRegion FindMostLikelyRegion(Point p)
        {
            SpyRegion mostLikely = null;
            SpyRegion resizable = null;
            
            foreach (SpyRegion reg in FindRegions(p))
            {
                // if it's not accepting mouse input, then it's not most likely
                if (!reg.IsAcceptingMouseInput)
                    continue;
                if (reg.GetPotentialMode(p) != ResizeMode.None)
                    resizable = reg;
                mostLikely = reg;
            }
            return (resizable ?? mostLikely);
        }

        /// <summary>
        /// Gets a unique region name, using the specified format string as a
        /// base. The format string should contain a placeholder for an integer,
        /// using the format described by <see cref="String.Format"/> (ie. 
        /// "{0}" - eg. "Region {0}"). The placeholder will be replaced with an
        /// integer until an unused name is found.
        /// </summary>
        /// <param name="formatString">The format string for the region name.
        /// </param>
        /// <returns>A region name unique within this region mapper derived from
        /// the given format string.</returns>
        public string GetUniqueRegionName(string formatString)
        {
            IBPSet<string> names = new clsSet<string>();
            foreach (SpyRegion reg in _regions)
            {
                names.Add(reg.Name);
            }
            for (int i = 1; i < int.MaxValue; i++)
            {
                string name = string.Format(formatString, i);
                if (!names.Contains(name))
                    return name;
            }
            // move onto the next int.MaxValue set of potential names...
            return GetUniqueRegionName(string.Format(formatString, int.MaxValue) + "{0}");
        }

        #endregion

        #region - Virtual OnXXX Event Methods -

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            propsElement.ResizeDescriptionArea(14);
        }

        protected virtual void OnRegionLayoutChanged(SpyRegionEventArgs e)
        {
            SpyRegionEventHandler handler = this.RegionLayoutChanged;
            if (handler != null)
                handler(this, e);
        }

        protected virtual void OnRegionCharDataChanged(SpyRegionEventArgs e)
        {
            SpyRegionEventHandler handler = this.RegionCharDataChanged;
            if (handler != null)
                handler(this, e);
        }

        protected virtual void OnRegionSelected(SpyRegionEventArgs e)
        {
            SpyRegionEventHandler handler = this.RegionSelected;
            if (handler != null)
                handler(this, e);
        }

        protected virtual void OnSpy(SpyRequestEventArgs e)
        {
            SpyRequestEventHandler handler = this.SpyRequested;
            if (handler != null)
                handler(this, e);
        }

        #endregion

        #region - Picbox Event Handlers -

        /// <summary>
        /// Handles a mousedown event on the picture box within this mapper
        /// </summary>
        private void HandlePicboxMouseDown(object sender, MouseEventArgs e)
        {
            Point scaledLocn = PointToModelLocation(e.Location);

            SpyRegion reg = FindMostLikelyRegion(scaledLocn);
            if (reg != null && reg.Mode != ResizeMode.None)
            {
                // We want the regions to deal with their actual values, not
                // the scaled values, so use the original location whenever we
                // are communicating with the regions.
                reg.HandleMouseDown(scaledLocn);
            }
            else
            {
                Tool t = this.CurrentTool;
                switch (t.RegionBehaviour)
                {
                    case ToolRegionBehaviour.Never:
                    case ToolRegionBehaviour.IfNotOnElement:
                        if (reg == null)
                        {
                            SelectedRegion = null;
                            picbox.InvalidatePicture();
                        }
                        else
                        {
                            reg.HandleMouseDown(scaledLocn);
                            if (!reg.Active)
                                SelectedRegion = reg;
                        }
                        break;

                    case ToolRegionBehaviour.Always:
                        SelectedRegion = null;
                        // Single region mode - a new region clears the old ones.
                        if (_singleRegionMode)
                        {
                            _regions.Clear();
                            _regionList.Clear();
                        }
                        SpyRegion r = t.GenerateStubRegion(this, scaledLocn);
                        r.HandleMouseDown(scaledLocn);
                        break;
                }
            }
        }

        /// <summary>
        /// Handles the mousemove event on the picture box within this mapper
        /// </summary>
        private void HandlePicboxMouseMove(object sender, MouseEventArgs e)
        {
            Point scaledLocn = PointToModelLocation(e.Location);
            SpyRegion reg = _active;
            if (reg == null)
                reg = FindMostLikelyRegion(scaledLocn);

            if (reg != null)
            {
                reg.HandleMouseMove(scaledLocn);
                switch (reg.Mode)
                {
                    case ResizeMode.BottomRight:
                    case ResizeMode.TopLeft:
                        this.Cursor = Cursors.SizeNWSE;
                        break;

                    case ResizeMode.TopRight:
                    case ResizeMode.BottomLeft:
                        this.Cursor = Cursors.SizeNESW;
                        break;

                    case ResizeMode.Right:
                    case ResizeMode.Left:
                        this.Cursor = Cursors.SizeWE;
                        break;

                    case ResizeMode.Top:
                    case ResizeMode.Bottom:
                        this.Cursor = Cursors.SizeNS;
                        break;

                    default:
                        this.Cursor = Cursors.Default;
                        break;
                }
                _hover = reg;
                InvalidateLabel();
            }
            else
            {
                this.Cursor = Cursors.Default;
                bool inval = (_hover != null);
                _hover = null;
                if (inval)
                    InvalidateLabel();
            }
        }

        /// <summary>
        /// Handles the mouseup event on the picture box held within this mapper
        /// </summary>
        private void HandlePicboxMouseUp(object sender, MouseEventArgs e)
        {
            SpyRegion reg = _active;
            if (reg != null)
            {
                reg.HandleMouseUp(PointToModelLocation(e.Location));
                ClearGuides();
                if (_regions.Add(reg))
                    _regionList.Add(reg);

                SelectedRegion = reg;
            }
        }

        /// <summary>
        /// Handles the picture box being painted - this overlays the spy regions
        /// over the top of the picture box, and displays the label near the cursor
        /// for the currently hovered region.
        /// </summary>
        private void HandlePicturePaint(object sender, PaintEventArgs e)
        {
            using (GDICache mgr = new GDICache())
            {
                Graphics g = e.Graphics;
                // Set the zoom matrix for the graphics object, based on
                // the zoom level of the pic box
                g.ScaleTransform(picbox.ZoomFactor, picbox.ZoomFactor);
                bool paintedActive = false; // active could be new (ie. not in _regions)
                string lblToPaint = null;
                                                                
                foreach (SpyRegion reg in _regions)
                {
                    reg.Paint(g, mgr);
                                    
                    if (reg.Active)
                        paintedActive = true;
                    
                    if (lblToPaint == null)
                        lblToPaint = reg.Label;
                }

                
                if (!paintedActive && _active != null)
                {
                    _active.Paint(g, mgr);
                }

                if (_flashRect.HasValue)
                {
                    using (Pen p = new Pen(Color.Red, 2.0f))
                    {
                        g.DrawRectangle(p, _flashRect.Value);
                    }
                }
                if (_guides.Count > 0)
                {
                    using (Pen p = new Pen(Color.Green))
                    {
                        foreach (Guide guide in _guides.Values)
                        {
                            int x = guide.X;
                            int y = guide.Y;
                            if (x > 0)
                                g.DrawLine(p, new Point(x, 0), new Point(x, picbox.Height));
                            if (y > 0)
                                g.DrawLine(p, new Point(0, y), new Point(picbox.Width, y));
                        }
                    }
                }

                // Reset the page scale for other operations
                g.ResetTransform();

                // If we have a label to paint, do so near to the cursor
                if (lblToPaint != null)
                {
                    Brush bgBrush = mgr.GetBrush(SystemColors.Info);
                    Brush bordBrush = mgr.GetBrush(SystemColors.ActiveBorder);
                    Brush txtBrush = mgr.GetBrush(SystemColors.InfoText);
                    {
                        // show the label 10x10 pixels offset (below right) from the cursor
                        Point pt = picbox.PointToClient(Cursor.Position);
                        pt.Offset(10, 10);

                        // Allow for the scroll position of the picture box
                        Point sp = picbox.AutoScrollPosition;
                        pt.Offset(-sp.X, -sp.Y);
                        
                        // Measure the string, create a rect big enough for it, pad it
                        // out a bit then draw a bordered box followed by the name of
                        // the region.
                        Size sz = Size.Ceiling(g.MeasureString(lblToPaint, LabelFont));
                        Rectangle r = new Rectangle(pt, sz);
                        r.Inflate(3, 2);

                        // The rectangle r now has the rectangle we want for our label,
                        // use that as the origin of the label drawing
                        g.FillRectangle(bgBrush, r);
                        using (Pen p = new Pen(bordBrush))
                        {
                            g.DrawRectangle(p, r);
                        }

                        r.Offset(2, 3);
                        g.DrawString(lblToPaint, LabelFont, txtBrush, r);
                    }
                }

            }
        }

        /// <summary>
        /// Handles a KeyDown event occurring in the picture box.
        /// </summary>
        private void HandlePictureBoxKeyDown(object sender, KeyEventArgs e)
        {
            HandleKeyDown(sender, e);
        }

        #endregion

        #region - Other Event Handlers -

        /// <summary>
        /// Handles the context menu opening on the property grid
        /// </summary>
        void HandlePropsContextMenuOpening(object sender, CancelEventArgs e)
        {
            GridItem item = propsElement.SelectedGridItem;
            menuResetProperty.Enabled = (item != null && 
                item.PropertyDescriptor.CanResetValue(propsElement.SelectedObject));
        }

        /// <summary>
        /// Handles 'Reset' being chosen for a property in the element's property grid
        /// </summary>
        void HandleResetProperty(object sender, EventArgs e)
        {
            try
            {
                propsElement.ResetSelectedProperty();
            }
            catch (TargetInvocationException tie)
            {
                Exception ex = tie;
                if (tie.InnerException != null)
                    ex = tie.InnerException;

                MessageBox.Show(string.Format(Resources.ThePropertyCouldNotBeReset0, ex.Message), Resources.Error, 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles a keydown event for this control
        /// </summary>
        void HandleKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    HandleDelete(sender, e);
                    e.Handled = true;
                    break;
                case Keys.C:
                    if (e.Control)
                    {
                        HandleCopy(sender, e);
                        e.Handled = true;
                    }
                    break;
                case Keys.V:
                    if (e.Control)
                    {
                        HandlePaste(sender, e);
                        e.Handled = true;
                    }
                    break;
                case Keys.X:
                    if (e.Control)
                    {
                        HandleCut(sender, e);
                        e.Handled = true;
                    }
                    break;
            }

        }
        
        /// <summary>
        /// Handles a property value being changed in the property editor
        /// </summary>
        void HandlePropertyValueChanged(
            object sender, PropertyValueChangedEventArgs e)
        {
            if (e.ChangedItem.PropertyDescriptor.Name == "FontName")
            {
                string fontName = (string)e.ChangedItem.Value;


            }

            picbox.InvalidatePicture();
        }

        /// <summary>
        /// Handles the rectangle within a spy region changing.
        /// </summary>
        void HandleSpyRegionLayoutChanged(object sender, EventArgs e)
        {
            picbox.InvalidatePicture();
            OnRegionLayoutChanged(new SpyRegionEventArgs((SpyRegion)sender));
        }

        /// <summary>
        /// Handles the character data changing in a region
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void HandleSpyRegionCharDataChanged(
            object sender, RegionCharDataChangeEventArgs e)
        {
            OnRegionCharDataChanged(new SpyRegionEventArgs((SpyRegion)sender));
        }

        /// <summary>
        /// Handles the spy region having its name changed.
        /// </summary>
        void HandleSpyRegionNameChanging(object sender, NameChangingEventArgs e)
        {
            // Check that the name doesn't clash with any of our current names
            clsSet<String> names = new clsSet<String>();
            foreach (SpyRegion reg in _regions)
            {
                if (sender != reg)
                    names.Add(reg.Name);
            }
            if (names.Contains(e.NewName))
            {
                e.Cancel = true;
                throw new AlreadyExistsException(
                    Resources.ARegionWithTheName0AlreadyExistsInThisRegionEditor, e.NewName);
            }
        }

        /// <summary>
        /// Handles a spy region name changing
        /// </summary>
        void HandleSpyRegionNameChanged(object sender, EventArgs e)
        {
            SpyRegion reg = sender as SpyRegion;
            int index = _regionList.IndexOf(reg);
            if (index >= 0)
            {
                _regionList.RemoveAt(index);
                _regionList.Add(reg);
                cmbRegions.SelectedItem = reg;
                SelectedRegion = reg;
            }
        }

        /// <summary>
        /// Handles a mode toolbar button being pressed - ie. a toolbar button
        /// which changes the current mode within the region mapper.
        /// </summary>
        void HandleModeToolbarButton(object sender, EventArgs e)
        {
            // Ignore grid region button events
            // FIXME: Temporarily disabled grid region button - see bug 7167
            if (sender == btnGrid)
                return;

            if (_handlingTools)
                return;
            _handlingTools = true;
            try
            {
                ToolStripButton src = (ToolStripButton) sender;
                // if user is switching off a toolbar button, reset it back 'on'.
                // Too confusing otherwise.
                if (!src.Checked)
                {
                    src.Checked = true;
                }
                foreach (ToolStripItem item in toolstrip.Items)
                {
                    ToolStripButton btn = item as ToolStripButton;
                    if (btn != null && btn != src)
                    {
                        btn.Checked = false;
                    }

                }
            }
            finally
            {
                _handlingTools = false;
            }
        }

        /// <summary>
        /// Handles the flasher timer ticking over.
        /// Checks if there are any more flashes to perform and flashes
        /// them as appropriate if so.
        /// </summary>
        void HandleFlasherTick(object sender, EventArgs e)
        {
            if (flashTimer.Tag != null)
            {
                Flasher f = flashTimer.Tag as Flasher;
                if (f.Flash())
                {
                    if (_flashRect.HasValue)
                        _flashRect = null;
                    else
                        _flashRect = f.Rectangle;
                }
                else
                {
                    flashTimer.Tag = null;
                    _flashRect = null;
                }
                picbox.InvalidatePicture();
            }
        }

        /// <summary>
        /// Handles the zoom level of the picture box changing.
        /// </summary>
        void HandlePicboxZoomChanged(object sender, ZoomLevelEventArgs e)
        {
            ZoomLevel zl = new ZoomLevel(e.ZoomPercent);
            cmbZoom.Text = zl.ToString();
            if (cmbZoom.Focused)
                cmbZoom.SelectAll();
        }

        /// <summary>
        /// Handles a zoom value being validated in the zoom combo box
        /// </summary>
        void HandleZoomValidated(object sender, EventArgs e)
        {
            CommitZoom();
        }

        /// <summary>
        /// Handles a keypress in the zoom combo box - this just checks for
        /// an enter key and validates and commits the zoom if it is detected.
        /// </summary>
        void HandleZoomKeypress(object sender, KeyPressEventArgs e)
        {
            if ("\r\n".IndexOf(e.KeyChar) != -1)
            {
                ValidateZoom();
                CommitZoom();
            }
        }

        /// <summary>
        /// Handles the validating of the zoom combo box
        /// </summary>
        void HandleValidatingZoom(object sender, CancelEventArgs e)
        {
            ValidateZoom();
        }

        /// <summary>
        /// Handles a zoom value being selected in the zoom combo box.
        /// </summary>
        void HandleZoomSelected(object sender, EventArgs e)
        {
            ZoomLevel zl = (ZoomLevel)cmbZoom.SelectedItem;
            if (zl.Percent <= 0) // ignore negative / 0 values.
                return;
            picbox.ZoomPercent = zl.Percent;
        }

        /// <summary>
        /// Handles a spy region being selected in the regions combo box.
        /// </summary>
        void HandleSpyRegionSelected(object sender, EventArgs e)
        {
            if (sender == cmbRegions && SelectedRegion != cmbRegions.SelectedItem)
            {
                SelectedRegion = (SpyRegion) cmbRegions.SelectedItem;
                picbox.InvalidatePicture();
            }
        }

        /// <summary>
        /// Handles the 'Show Toolbar Text' button being toggled.
        /// </summary>
        void HandleShowToobarTextClicked(object sender, EventArgs e)
        {
            ToolStripItemDisplayStyle style = ((ToolStripMenuItem)sender).Checked
                ? ToolStripItemDisplayStyle.ImageAndText
                : ToolStripItemDisplayStyle.Image;


            foreach (ToolStripItem item in toolstrip.Items)
            {
                if (item.Text != "")
                    item.DisplayStyle = style;
            }

        }


        /// <summary>
        /// Handles getting an image from a currently open window - this will cause
        /// a bitmap spy operation to be initiated, after which the image in the
        /// mapper will be set to the spied image.
        /// </summary>
        void HandleGetImageFromWindow(object sender, EventArgs e)
        {
            SpyRequestEventArgs sre = new SpyRequestEventArgs();
            OnSpy(sre);
            if (sre.SpiedImage != null)
            {
                ClearRegions();
                this.Image = sre.SpiedImage;
            }
        }

        /// <summary>
        /// Handles getting the image from a file, first requesting the path to the
        /// file from the user and then opening the image, and loading it into the
        /// region mapper.
        /// </summary>
        void HandleGetImageFromFile(object sender, EventArgs e)
        {
            using (OpenFileDialog opener = new OpenFileDialog())
            {
                opener.DereferenceLinks = true;
                opener.Filter =
                    Resources.ImagesBmpPngJpgGifBmpPngJpgGifAllFiles;
                opener.Multiselect = false;
                if (_lastDirectory != null)
                    opener.InitialDirectory = _lastDirectory;
                if (opener.ShowDialog() != DialogResult.OK)
                    return;
                string file = opener.FileName;
                if (string.IsNullOrEmpty(file))
                    return;
                if (!File.Exists(file))
                    return;
                try
                {
                    Bitmap bmp = new Bitmap(file);
                    ClearRegions();
                    this.Image = bmp;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(null, string.Format(
                        Resources.AnErrorOccurredWhileLoadingTheImage01,
                        file, ex.Message), Resources.LoadError,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }
        #endregion

        #region - Cut / Copy / Paste / Delete -

        /// <summary>
        /// Handles a region delete being requested.
        /// </summary>
        private void HandleDelete(object sender, EventArgs e)
        {
            if (_singleRegionMode) // Cut/Copy/Paste/Delete not supported
                return;

            SpyRegion sel = SelectedRegion;
            if (sel != null)
            {
                if (_regions.Remove(sel))
                    _regionList.Remove(sel);
                SelectedRegion = CollectionUtil.Last(_regions);

                ClearRelativeParents(sel);

                picbox.InvalidatePicture();
            }
        }

        /// <summary>
        /// Handles the 'Cut' toolbar button being clicked.
        /// </summary>
        private void HandleCut(object sender, EventArgs e)
        {
            if (_singleRegionMode) // Cut/Copy/Paste/Delete not supported
                return;

            _buffer.Clear();
            if (SelectedRegion != null)
            {
                SpyRegion reg = SelectedRegion;

                ClearRelativeParents(reg);

                _buffer.Add(reg);
                _regions.Remove(reg);
                _regionList.Remove(reg);

                picbox.InvalidatePicture();
            }
        }

        /// <summary>
        /// Encapsulates the removal of the selected regions relations in copy and delete actions.
        /// </summary>
        /// <param name="selectedRegion">The currently user selected region. </param>
        private void ClearRelativeParents(SpyRegion selectedRegion)
        {
            foreach(var region in _regions)
            {
                if (region.RelativeParent == selectedRegion)
                {
                    region.ClearRelativeParent();
                }
            }
        }

        /// <summary>
        /// Handles the 'Copy' toolbar button being clicked.
        /// </summary>
        private void HandleCopy(object sender, EventArgs e)
        {
            if (_singleRegionMode) // Cut/Copy/Paste/Delete not supported
                return;

            _buffer.Clear();
            if (SelectedRegion != null)
            {
                _buffer.Add(SelectedRegion);
            }
        }

        /// <summary>
        /// Handles the 'Paste' toolbar button being clicked.
        /// </summary>
        private void HandlePaste(object sender, EventArgs e)
        {
            if (_singleRegionMode) // Cut/Copy/Paste/Delete not supported
                return;

            if (_buffer.Count > 0)
            {
                SpyRegion reg = CollectionUtil.First(_buffer);

                Rectangle rect = reg.Rectangle;
                Point globalLocn = Cursor.Position;
                Rectangle picboxGlobalBounds =
                    picbox.Parent.RectangleToScreen(picbox.Bounds);

                // If the cursor is within the picture box, use its location as
                // the paste location, otherwise init to a default value, ensuring
                // that we don't paste over the top of an existing region (or at
                // least not at the same location).
                Point startPoint = (picboxGlobalBounds.Contains(globalLocn)
                    ? picbox.PointToClient(globalLocn)
                    : new Point(20, 20)
                );
                // offset by the picture box's scroll position
                startPoint -= (Size)picbox.AutoScrollPosition;
                // And scale according to the current zoom level
                startPoint = PointToModelLocation(startPoint);
                Point locn = FindFreeLocation(startPoint);

                // FindFreeLocation() takes into account and returns a location which
                // is offset by the scroll position of the picture box, so we don't
                // need to worry about that.

                rect.Location = locn;
                reg = RegisterRegion(new SpyRegion(this, rect));
                _regions.Add(reg);
                _regionList.Add(reg);
                SelectedRegion = reg;

                // We want to set the cursor back to where the region is, so we
                // first scale it back to the control-relative position
                locn = PointToControlLocation(locn);

                // and remove the scroll offset value again
                locn.Offset(picbox.AutoScrollPosition);
                // And offset by 10 'model-pixels' just so we're not putting the user
                // on the edge of the region
                int cursorOffset = (int)Math.Round(10.0 * picbox.ZoomFactor);
                locn.Offset(cursorOffset, cursorOffset);

                Cursor.Position = picbox.PointToScreen(locn);
                picbox.InvalidatePicture();
            }
        }

        #endregion

        #region - Other Methods -

        /// <summary>
        /// Attempts to create an empty font with the given name
        /// </summary>
        /// <param name="name">The name of the font to create</param>
        public void CreateEmptyFont(string name)
        {
            if (_store.AvailableFontNames.Contains(name))
            {
                throw new AlreadyExistsException(
                    Resources.AFontWithTheName0AlreadyExistsPleaseChooseAnother, name);
            }
            _store.SaveFont(new BPFont(name, "1.0", new FontData()));
        }

        /// <summary>
        /// Loads the font with the given name.
        /// </summary>
        /// <param name="name">The name of the font to load</param>
        /// <returns>The font with the given name</returns>
        /// <exception cref="NoSuchFontException">If no font with the given name was
        /// found</exception>
        public BPFont LoadFont(string name)
        {
            return _store.GetFont(name);
        }

        /// <summary>
        /// Checks if any of the regions in this mapper contain references to empty
        /// fonts, and returns a set of those names (case-insensitive) if they do
        /// </summary>
        internal ICollection<string> EmptyFontReferences
        {
            get
            {
                clsSet<string> emptyFonts =
                    new clsSet<string>(StringComparer.CurrentCultureIgnoreCase);
                foreach (SpyRegion reg in _regions)
                {
                    BPFont f = reg.Font;
                    if (f != null && f.CharacterData.Count == 0)
                        emptyFonts.Add(f.Name);
                }
                return emptyFonts;
            }
        }

        /// <summary>
        /// Takes the given point, relative to the picture box and finds the point
        /// within the model that it represents.
        /// </summary>
        /// <param name="p">The point to scale - ie. the point relative to the origin
        /// of the picture box</param>
        /// <returns>The point within the model that <paramref name="p"/> represents
        /// </returns>
        private Point PointToModelLocation(Point p)
        {
            return ScalePoint(p, picbox.ZoomFactor, true);
        }

        /// <summary>
        /// Takes the given point describing a point in the model, and finds the
        /// point relative to the picture box control that it represents.
        /// </summary>
        /// <param name="p">The point relative to the model in which the regions
        /// reside.</param>
        /// <returns>The point relative to the origin of the picture box of the given
        /// model-relative point.</returns>
        private Point PointToControlLocation(Point p)
        {
            return ScalePoint(p, picbox.ZoomFactor, false);
        }

        /// <summary>
        /// Scales the given point using the given factor or the inverse of it if
        /// scaling to the model
        /// </summary>
        /// <param name="p">The point to scale</param>
        /// <param name="factor">The factor to scale by (positive - eg. 2.0f
        /// represents a zoom level of 2 - scaling to model would halve the co-ords
        /// in the point; scaling to UI would double them).</param>
        /// <param name="scaleToModel">True to scale from UI to model - effectively,
        /// this inverts the factor by which the point is scaled.</param>
        /// <returns>The given point scaled by the given factor or its inverse. An
        /// empty point if the factor was negative or zero</returns>
        private Point ScalePoint(Point p, float factor, bool scaleToModel)
        {
            if (factor == 1.0f)
                return p;
            if (factor <= 0.0f)
                return Point.Empty;
            // invert the factor in order to get the correct point
            // (eg. if factor is 2.0 and user clicks {20,20}, that corresponds
            // to {10,10} in the model)
            if (scaleToModel)
                factor = 1.0f / factor;

            return new Point(
                (int)Math.Round(p.X * factor), (int)Math.Round(p.Y * factor));
        }

        /// <summary>
        /// Registers the given spy region with this mapper, ensuring that this
        /// mapper is listening to pertinent events on the region
        /// </summary>
        /// <param name="r">The region to register with this mapper</param>
        /// <returns>The given region after registering it</returns>
        private SpyRegion RegisterRegion(SpyRegion r)
        {
            r.RegionLayoutChanged += HandleSpyRegionLayoutChanged;
            r.CharacterDataChanged += HandleSpyRegionCharDataChanged;
            r.NameChanging += HandleSpyRegionNameChanging;
            r.NameChanged += HandleSpyRegionNameChanged;
            return r;
        }

        /// <summary>
        /// Invalidates the label used to display the name of the region being
        /// hovered over
        /// </summary>
        private void InvalidateLabel()
        {
            picbox.InvalidatePicture();
        }

        /// <summary>
        /// Validates the current zoom value in the zoom combo box.
        /// This just parses the zoom using the zoom regular expression,
        /// effectively it allows numerics (including decimal point) and an
        /// optional percent sign - the value is rounded and an appropriate
        /// zoomlevel value is set into the combo box tag - the rounded parsed
        /// value if valid, the current picture box value otherwise.
        /// </summary>
        private void ValidateZoom()
        {
            Match m = _zoomRegex.Match(cmbZoom.Text);
            ZoomLevel zl;
            if (m.Success)
            {
                zl = new ZoomLevel((int)Math.Round(decimal.Parse(m.Groups[1].Value)));
            }
            else
            {
                zl = new ZoomLevel(picbox.ZoomPercent);
            }
            // Set the validating zoom level into the tag
            cmbZoom.Tag = zl;
        }

        /// <summary>
        /// Commits the zoom level set into the tag of the zoom combo box by
        /// the <see cref="ValidateZoom"/> method - this sets the new zoom level
        /// into the picture box (if that value isn't already set in it).
        /// </summary>
        private void CommitZoom()
        {
            ZoomLevel zl = (ZoomLevel)cmbZoom.Tag;
            if (zl.Percent <= 0) // ignore negative / 0 values.
                return;
            if (zl.Percent != picbox.ZoomPercent)
                picbox.ZoomPercent = zl.Percent;
        }
        /// <summary>
        /// Adds the given regions to this region mapper
        /// </summary>
        /// <param name="regions">The regions to add to this mapper</param>
        private void AddRegions(ICollection<SpyRegion> regions)
        {
            foreach (SpyRegion reg in regions)
            {
                reg.Container = this;
                if (_regions.Add(reg))
                {
                    RegisterRegion(reg);
                    _regionList.Add(reg);
                }
            }
        }

        /// <summary>
        /// Clears the regions from this mapper
        /// </summary>
        public void ClearRegions()
        {
            _regionList.Clear();
            _regions.Clear();
            _buffer.Clear();

            _selected = null;
            _hover = null;
            _active = null;
        }

        /// <summary>
        /// Finds a free location within the picture starting at the specified
        /// location. Note that both input and output to this method are relative to
        /// the captured image - ie. using the dimensions that the regions use to
        /// define their bounds, not UI-based dimensions.
        /// </summary>
        /// <param name="startPoint">The start location to use when searching for a
        /// free location within the image, relative to the top left of the captured
        /// image held in this region mapper.</param>
        /// <returns>The first point found at which no region was found to exist and
        /// which was currently visible in the picture box's scroll port.</returns>
        private Point FindFreeLocation(Point startPoint)
        {
            // We start at the given location and stack them diagonally until
            // we find a free space
            Point currStart = startPoint;

            // We need to take into account the scroll position of the picbox
            // The AutoScrollPosition comes back negative - we want positive so
            // we can add it to the local location, hence the subtract
            Point sp = Point.Empty - (Size)picbox.AutoScrollPosition;

            // Use p as our working variable - currStart will remain pointing
            // to the start of this 'column' so that we can move along as we
            // run out of space
            Point p = currStart;

            while (RegionExistsAt(p))
            {
                p.Offset(5, 5);

                // Get the physical point - ie. the UI's client location of the
                // potential region to ensure we don't go off the edge of the box
                Point clientP = p - (Size)sp;

                // If that offset takes it off the bottom edge of the picture box,
                // move along 50 pixels and try again
                if (clientP.Y > picbox.Height)
                {
                    currStart.Offset(30, 0);
                    p = currStart;
                    p.Offset(sp);
                }
                // If we have gone off the right edge of the picture box, then
                // we just have to settle on a value - there's not much else
                // we're going to be able to do.
                if (clientP.X > picbox.Width)
                {
                    // Just go back to our very first position
                    return startPoint + (Size)sp;
                }
            }
            return p;
        }

        /// <summary>
        /// Checks if a region exists at the given point in this mapper.
        /// </summary>
        /// <param name="p">The point at which to test for a region</param>
        /// <returns>True if  aregion was found at this location, false otherwise.
        /// </returns>
        private bool RegionExistsAt(Point p)
        {
            foreach (SpyRegion reg in _regions)
                if (reg.Rectangle.Location == p)
                    return true;
            return false;
        }

        #endregion

    }
}
