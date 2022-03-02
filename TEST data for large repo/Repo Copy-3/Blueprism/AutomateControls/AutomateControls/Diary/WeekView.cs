using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using AutomateControls.Properties;
using BluePrism.BPCoreLib.Collections;
using BluePrism.BPCoreLib.Diary;
// 2 ambiguous classes - ensure we are using the correct ones.
using Timer = System.Timers.Timer;

namespace AutomateControls.Diary
{
    /// <summary>
    /// Control which represents a view of a diary with days stretching
    /// from midnight at the top to the following midnight at the bottom, and
    /// a specified number of days starting from the start date on the left
    /// to the far right of the control.
    /// </summary>
    public partial class WeekView : System.Windows.Forms.ScrollableControl
    {
        #region DiaryEntrySource implementations

        /// <summary>
        /// Class which just represents a diary entry source with no entries
        /// available in it.
        /// </summary>
        private class EmptyDiaryEntrySource : IDiaryEntrySource
        {
            // Empty array, just so we don't need to recreate an object each time.
            private static readonly IDiaryEntry[] EMPTY_ARRAY = new IDiaryEntry[0];

            /// <summary>
            /// Gets the entries for the given range from this source.
            /// Always returns an empty collection.
            /// </summary>
            /// <param name="from">The start date</param>
            /// <param name="to">The end date</param>
            /// <returns>An empty collection.</returns>
            public ICollection<IDiaryEntry> GetEntries(DateTime from, DateTime to)
            {
                return EMPTY_ARRAY;
            }
        }

        /// <summary>
        /// Class which represents a basic source for diary entries - this just
        /// wraps a simple collection, and is relatively static, although if
        /// the collection is changed elsewhere, subsequent calls to GetEntries
        /// will use the updated data in the collection.
        /// </summary>
        private class BasicDiaryEntrySource : IDiaryEntrySource
        {
            // The entries wrapped by this source.
            private ICollection<IDiaryEntry> _entries;

            /// <summary>
            /// Creates a new diary entry source using the given collection as
            /// its source of entries.
            /// </summary>
            /// <param name="entries">The collection of diary entries to use as 
            /// the source for this object.</param>
            /// <exception cref="ArgumentNullException">If the given collection 
            /// is null.</exception>
            public BasicDiaryEntrySource(ICollection<IDiaryEntry> entries)
            {
                if (entries == null)
                    throw new ArgumentNullException(nameof(entries));

                _entries = entries;
            }

            /// <summary>
            /// Gets all the diary entries which occur between the given dates
            /// from this source.
            /// </summary>
            /// <param name="from">The inclusive start date/time at which diary 
            /// entries should be returned.</param>
            /// <param name="to">The exclusive end date/time at which diary entries 
            /// should be returned.</param>
            /// <returns>The non-null collection of diary entries which occur
            /// between the specified dates.</returns>
            /// <remarks>If the dates are reversed, such that
            /// <paramref name="from"/> is after <paramref name="to"/>, then
            /// this will return an empty collection.</remarks>
            public ICollection<IDiaryEntry> GetEntries(DateTime from, DateTime to)
            {
                List<IDiaryEntry> list = new List<IDiaryEntry>();
                foreach (IDiaryEntry entry in _entries)
                {
                    DateTime dt = entry.Time;
                    if (dt >= from && dt < to)
                        list.Add(entry);
                }
                return list;
            }
        }

        #endregion

        #region Constants / enums

        /// <summary>
        /// The default number of days to display in this view.
        /// </summary>
        private const int DEFAULT_NUM_DAYS = 7;

        /// <summary>
        /// The default height of the header for this view.
        /// </summary>
        private const int DEFAULT_HEADER_HEIGHT = 32;

        /// <summary>
        /// The default width of the left panel in this view.
        /// </summary>
        private const int DEFAULT_PANEL_WIDTH = 64;

        /// <summary>
        /// The default height of each cell in this view.
        /// </summary>
        private const int DEFAULT_CELL_HEIGHT = 32;

        /// <summary>
        /// The default date format to use for this control
        /// </summary>
        private const string DEFAULT_DATE_FORMAT = "ddd\ndd MMM";

        /// <summary>
        /// The default colour to use for the text on this control
        /// </summary>
        private static readonly Color DEFAULT_TEXT_COLOR = Color.FromArgb(0x88, 0x99, 0xaa);

        /// <summary>
        /// The default colour to use for the lines / borders on this control
        /// </summary>
        private static readonly Color DEFAULT_BORDER_COLOR = Color.FromArgb(0xcc, 0xdd, 0xee);

        /// <summary>
        /// Constant timespan representing one hour.
        /// </summary>
        private static readonly TimeSpan DEFAULT_CELL_TIME = new TimeSpan(1, 0, 0);

        /// <summary>
        /// Constant timespan representing the end of the day when drawing the hours
        /// </summary>
        private static readonly TimeSpan END_OF_DAY = new TimeSpan(24, 0, 0);

        /// <summary>
        /// The margin between entries
        /// </summary>
        private const int _entryMargin = 1;

        /// <summary>
        /// The colours to use for the background gradient of the entries drawn.
        /// These are rotated over using the _colourIndex variable
        /// </summary>
        private static readonly IList<Color> ENTRY_COLOURS = GetReadOnly.IList(new Color[]{
            Color.SkyBlue,
            Color.LightGreen,
            Color.Goldenrod,
            Color.PeachPuff,
            Color.LightSlateGray,
            Color.Olive,
            Color.Khaki         
        });

        /// <summary>
        /// Enumeration of the valid states for the navigation buttons.
        /// 'Active' in this sense means that it is being hovered over, such
        /// that a click would fire the appropriate navigation button action.
        /// </summary>
        private enum NavButtonState { Dormant, PreviousActive, NextActive }

        #endregion

        #region Property backing variables

        // Delegate to check if the cell should be highlighted or not.
        private CellHighlighter _highlighter;

        // The width of the left panel
        private int _leftPanelWidth;

        // The height of the header
        private int _headerHeight;

        // The height in pixels of a single hour.
        private int _cellHeight;

        // The number of days to display in the control
        private int _numDays;

        // The string for formatting the date within the control.
        private string _dateFormatString;

        // The colour used for the lines in the control. This is used for all lines - 
        // the date and hour separators; the borders around the left panel and entry
        // table, and the borders around each entry.
        private Color _borderColour;

        // The date to use as the start date for the purposes of this control.
        private DateTime _from;

        // The source of the diary entries for this view.
        private IDiaryEntrySource _source;

        // The span of time that each cell represents
        private TimeSpan _cellTime;

        private ICollection<IDiaryEntry> _diaryEntries;

        #endregion

        #region Internal variables

        /// <summary>
        /// The tooltip for the this control - used to show the full name for the
        /// schedule instance that is currently being hovered over.
        /// </summary>
        private ToolTip _tooltip;

        /// <summary>
        /// Timer used to process the tooltips when a diary entry is entered.
        /// </summary>
        private Timer _timer;

        // The dictionary containing the cells mapped against the time slot they represent.
        private IDictionary<DateTime, Cell> _cells;

        /// <summary>
        /// Map of the (viewable) rectangles which contain the rectangles within the
        /// control against the cell entries that they represent. Note that the 
        /// entries are wrapped inside an <see cref="CellEntry"/> object in order
        /// to ensure that the hash code returned is consistent among similarly
        /// valued entries.
        /// </summary>
        private IDictionary<CellEntry, Rectangle> _entryBounds;

        /// <summary>
        /// Flag to indicate that the control key is currently held down.
        /// </summary>
        private bool _isControlDown;

        /// <summary>
        /// The diary entry that is currently being hovered over, or null
        /// if no instance is being hovered over.
        /// </summary>
        private CellEntry _hovered;

        /// <summary>
        /// The times of the day which contain dividers, mapped against the Y location
        /// within the control of those times.
        /// </summary>
        private IDictionary<int, TimeSpan> _dividers;

        /// <summary>
        /// The currently active time of the day which has been grabbed and is being
        /// dragged when the user is changing the hour height.
        /// This will have no value if the user is not in the process of changing
        /// the height.
        /// </summary>
        private TimeSpan? _activeDivider;

        /// <summary>
        /// Colours mapped to entry titles - this isn't refreshed whenever the cells
        /// are updated, so you get consistent colours within the lifetime of this object.
        /// </summary>
        private IDictionary<string, Color> _colourMap;

        /// <summary>
        /// The index of the last colour within ENTRY_COLOURS to be used for an entry.
        /// Again, not reset between cell updates so there is some consistency.
        /// -1 indicates that no colours have yet been used.
        /// </summary>
        private int _colourIndex;

        /// <summary>
        /// The rectangle defining the bonuds of the 'previous' button
        /// </summary>
        private Rectangle _prevRect;

        /// <summary>
        /// The rectangle defining the bounds of the 'next' button.
        /// </summary>
        private Rectangle _nextRect;

        /// <summary>
        /// The navigation button state.
        /// </summary>
        private NavButtonState _navMode;



        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new empty day view starting from the first Monday on or
        /// before today.
        /// </summary>
        public WeekView() : this(GetDefaultStartDate()) { }

        /// <summary>
        /// Creates a new empty day view starting from the given day.
        /// </summary>
        /// <param name="from">The date from which this weekview should show.
        /// </param>
        public WeekView(DateTime from)
        {
            _from = from;

            //Double buffer the control
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.DoubleBuffer |
                ControlStyles.Selectable, true);

            // timer to use for handling tooltips
            _timer = new TooltipTimer(this, 350);
            _tooltip = new ToolTip();

            _dividers = new Dictionary<int, TimeSpan>();
            _entryBounds = new Dictionary<CellEntry, Rectangle>();
            _cells = new Dictionary<DateTime, Cell>();
            _colourMap = new Dictionary<string, Color>();
            _colourIndex = -1;
            _navMode = NavButtonState.Dormant;

            InitializeComponent();

            // Some defaults
            _numDays = DEFAULT_NUM_DAYS;
            _borderColour = DEFAULT_BORDER_COLOR;
            _leftPanelWidth = DEFAULT_PANEL_WIDTH;
            _headerHeight = DEFAULT_HEADER_HEIGHT;
            _cellHeight = DEFAULT_CELL_HEIGHT;
            _cellTime = DEFAULT_CELL_TIME;

            ForeColor = DEFAULT_TEXT_COLOR;

            // Set it to auto-scroll and initialise the minsize in order to
            // size the scrollbars correctly.
            this.AutoScroll = true;
            SetAutoScrollMinSize();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the inclusive start date which is currently being viewed
        /// on this control. Note that the time component of any date/time set
        /// with this property is discarded.
        /// </summary>
        [Description("The date from which to start the weekview"),
         Category("Data"),
         Browsable(true)]
        public DateTime StartDate
        {
            get { return _from; }
            set
            {
                value = value.Date;
                FireDateRangeChanging(value, _numDays);
                _from = value;
                UpdateCellData();               
            }
        }

        /// <summary>
        /// Gets or sets the exclusive end date which is currently being viewed on
        /// this control.
        /// </summary>
        /// <exception cref="ArgumentException">If the end date being set to is on
        /// or before the start date currently set.</exception>
        /// <remarks>Note that this actually changes the number of days to display.
        /// This means that if you set an end date and then change the start date,
        /// the end date will follow the start date, not remain the same as the
        /// absolute value set in it.</remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime EndDate
        {
            get { return _from.AddDays(_numDays); }
            set
            {
                value = value.Date;
                if (value <= _from)
                    throw new ArgumentException("End date must be after start date");
                // The property handles any events which need to be changed.
                this.NumberOfDays = (value - _from).Days;
            }
        }

        /// <summary>
        /// The number of days to show in this control
        /// </summary>
        [DisplayName("Number of days"), Description("The number of days to display in the control"),
         Category("Data"), DefaultValue(WeekView.DEFAULT_NUM_DAYS), Browsable(true)]
        public int NumberOfDays
        {
            get { return _numDays; }
            set
            {
                FireDateRangeChanging(_from, value);
                _numDays = value;
                UpdateCellData();
            }
        }


        /// <summary>
        /// The colour of the lines / borders drawn by this control.
        /// </summary>
        [DisplayName("Border Colour"), Description("The colour of the lines on this control"),
         Category("Appearance"), Browsable(true)]
        public Color BorderColor
        {
            get { return _borderColour; }
            set { _borderColour = value; }
        }

        #region Overridden properties - only for the designer

        /// <summary>
        /// The colour of the text on the control
        /// </summary>
        [DisplayName("Text Colour"), Description("The colour of the text on this control"),
         Category("Appearance"), Browsable(true)]
        public override Color ForeColor
        {
            get { return base.ForeColor; }
            set { base.ForeColor = value; }
        }

        /// <summary>
        /// Gets / Sets whether this control is auto-scrolled or not.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool AutoScroll
        {
            get { return base.AutoScroll; }
            set { base.AutoScroll = value; }
        }

        #endregion

        /// <summary>
        /// The date format for the dates to be displayed at the top of the control
        /// </summary>
        [DisplayName("Date Format"), Description("The format of the date displayed in the header"),
         Category("Appearance"), DefaultValue(WeekView.DEFAULT_DATE_FORMAT), Browsable(true)]
        public string DateFormat
        {
            get { return (_dateFormatString ?? DEFAULT_DATE_FORMAT); }
            set
            {
                _dateFormatString = value;
                Invalidate(AddScrollOffset(HeaderRect));
            }
        }

        /// <summary>
        /// The height in pixels of the area for each hour.
        /// </summary>
        [DisplayName("Cell Height"), Description("The initial height of the area for each cell"),
         Category("Appearance"), DefaultValue(WeekView.DEFAULT_CELL_HEIGHT), Browsable(true)]
        public int CellHeight
        {
            get { return _cellHeight; }
            set
            {
                _cellHeight = value;
                SetAutoScrollMinSize();
            }
        }

        /// <summary>
        /// The time that each cell should represent.
        /// </summary>
        [DisplayName("Cell Time"), Description("The length of time each cell should represent"),
         Category("Appearance"), Browsable(true)]
        public TimeSpan CellTime
        {
            get { return _cellTime; }
            set
            {
                _cellTime = value;
                UpdateCellData();
            }
        }

        /// <summary>
        /// The width of the left panel with the hour times on it.
        /// </summary>
        [DisplayName("Left Panel Width"), Description("The width of the left panel"),
         Category("Appearance"), DefaultValue(WeekView.DEFAULT_PANEL_WIDTH), Browsable(true)]
        public int LeftPanelWidth
        {
            get { return _leftPanelWidth; }
            set { _leftPanelWidth = value; }
        }

        /// <summary>
        /// The height of the header which has the dates on it.
        /// </summary>
        [DisplayName("Header Height"), Description("The height of the weekview's header"),
         Category("Appearance"), DefaultValue(DEFAULT_HEADER_HEIGHT), Browsable(true)]
        public int HeaderHeight
        {
            get { return _headerHeight; }
            set { _headerHeight = value; }
        }

        /// <summary>
        /// Gets or sets the source of diary entries to the given collection.
        /// <strong>Note: </strong> this has a subtle difference between what
        /// is set and got by its accessors / mutators. Setting this property
        /// will accept and use entries in any date range, whether it falls
        /// within the currently viewable date range or not. When the
        /// collection of entries is accessed via this property, only the
        /// entries which fall within the currently visible date range will
        /// be returned.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ICollection<IDiaryEntry> Entries
        {
            get { return Source.GetEntries(StartDate, EndDate); }
            set { Source = new BasicDiaryEntrySource(value); }
        }

        /// <summary>
        /// Gets or sets the cell highlighter implementation responsible for
        /// determining if a cell should be highlighted, and determining the
        /// colour for that highlight.
        /// This will never be null - if no value is set, explicitly a 
        /// highlighter which never highlights a cell will be used.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CellHighlighter Highlighter
        {
            get
            {
                if (_highlighter == null)
                    _highlighter = new EmptyHighlighter();
                return _highlighter;
            }
            set { _highlighter = value; }
        }

        /// <summary>
        /// Gets the cell entries which are pertinent to this week view 
        /// control within the dates it is currently set to.
        /// </summary>
        private IDictionary<DateTime,Cell> CellEntries
        {
            get { return _cells; }
        }

        /// <summary>
        /// Gets or sets the source of diary entries for this view.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IDiaryEntrySource Source
        {
            get { return (_source ?? new EmptyDiaryEntrySource()); }
            set
            {
                // if this is the same source as before, do nothing...
                if (object.ReferenceEquals(_source, value))
                    return;

                _source = value;
                // If the source has changed, get the cell data again.
                UpdateCellData();
            }
        }
      
        /// <summary>
        /// The rectangle defining the edges of the left panel
        /// </summary>
        [Browsable(false)]
        protected Rectangle LeftPanelRect
        {
            get
            {
                Rectangle rect = this.ClientRectangle;
                return new Rectangle(
                    rect.X - scrollx, rect.Y + _headerHeight - scrolly,
                    _leftPanelWidth - 1, rect.Height - 2);
            }
        }

        /// <summary>
        /// The rectangle for the origin - ie. the top left corner of the 
        /// control - ie. the space defined by the height of the header
        /// and the width of the left panel.
        /// </summary>
        [Browsable(false)]
        protected Rectangle OriginRect
        {
            get
            {
                Rectangle rect = this.ClientRectangle;
                return new Rectangle(
                    rect.X - scrollx, rect.Y - scrolly,
                    _leftPanelWidth, _headerHeight);

            }
        }

        /// <summary>
        /// The rectangle for the table of diary entries which is being
        /// displayed by this control.
        /// </summary>
        [Browsable(false)]
        protected Rectangle EntryTableRect
        {
            get
            {
                Rectangle rect = this.ClientRectangle;
                return new Rectangle(
                    rect.X + _leftPanelWidth, rect.Y + _headerHeight,
                    rect.Width - _leftPanelWidth, rect.Height);
            }
        }

        /// <summary>
        /// The rectangle for the header area of this control
        /// </summary>
        [Browsable(false)]
        protected Rectangle HeaderRect
        {
            get
            {
                Rectangle entryTableRect = this.EntryTableRect;
                return new Rectangle(entryTableRect.X, 0, entryTableRect.Width, _headerHeight);
            }
        }

        /// <summary>
        /// The minimum size for the autoscroll.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Size AutoScrollMinSize
        {
            get { return base.AutoScrollMinSize; }
            set { base.AutoScrollMinSize = value; }
        }

        #endregion

        #region ShouldSerialize and Reset methods for the Forms Designer

        /// <summary>
        /// Resets the start date to its default value.
        /// </summary>
        public void ResetStartDate()
        {
            _from = GetDefaultStartDate();
        }

        /// <summary>
        /// Checks if the designer should serialize the value set in the 
        /// StartDate property of this weekview.
        /// </summary>
        /// <returns>True if the start date is not the default start date,
        /// false otherwise.</returns>
        private bool ShouldSerializeStartDate()
        {
            return (_from != GetDefaultStartDate());
        }

        /// <summary>
        /// Resets the border colour to its default value.
        /// </summary>
        public void ResetBorderColor()
        {
            _borderColour = DEFAULT_BORDER_COLOR;
        }

        /// <summary>
        /// Checks if the designer should serialize the border colour, or
        /// whether it is currently set to default.
        /// </summary>
        /// <returns>True if the value of the border colour should be
        /// serialized into the designer class; false otherwise.</returns>
        private bool ShouldSerializeBorderColor()
        {
            return (_borderColour != DEFAULT_BORDER_COLOR);
        }

        /// <summary>
        /// Resets the forecolor property to its default value.
        /// </summary>
        public override void ResetForeColor()
        {
            this.ForeColor = DEFAULT_TEXT_COLOR;
        }

        /// <summary>
        /// Checks if the forecolor value should be serialized or not
        /// </summary>
        /// <returns>True if the value of the text colour should be serialized
        /// into the designer class; false otherwise.</returns>
        public bool ShouldSerializeForeColor()
        {
            return (ForeColor != DEFAULT_TEXT_COLOR);
        }

        /// <summary>
        /// Resets the cell time property for this object.
        /// </summary>
        public void ResetCellTime()
        {
            _cellTime = DEFAULT_CELL_TIME;
        }

        /// <summary>
        /// Checks if the cell time property should be serialized or not.
        /// </summary>
        /// <returns>True if the value of the cell time in this object should
        /// be serialized into the designer class, false otherwise.</returns>
        public bool ShouldSerializeCellTime()
        {
            return (_cellTime != DEFAULT_CELL_TIME);
        }

        #endregion

        #region Member / static methods

        /// <summary>
        /// Attempts to find the first Monday on or before the given date.
        /// If the first monday back from the given date goes before
        /// <see cref="DateTime.MinValue"/>, this will just return the given
        /// date unchanged.
        /// </summary>
        /// <param name="date">The date for which the first Monday on or before
        /// it is required.</param>
        /// <returns>The date on or before the given date which is a Monday, or
        /// the same date given if the search for a Monday went out of the 
        /// DateTime's supported date range.</returns>
        public static DateTime FindMonday(DateTime date)
        {
            DateTime dt = date;
            try
            {
                while (dt.DayOfWeek != DayOfWeek.Monday)
                    dt = dt.AddDays(-1);
                return dt;
            }
            catch (ArgumentOutOfRangeException)
            {   // We hit < DateTime.MinValue - just return the original date.
                return date;
            }
        }

        /// <summary>
        /// Gets the default start date for this weekview.
        /// This is the first Monday which occurs on or before today.
        /// </summary>
        /// <returns>The default start date for this control.</returns>
        private static DateTime GetDefaultStartDate()
        {
            return FindMonday(DateTime.Today);
        }

        /// <summary>
        /// Gets the (visible) cell entry at the given point, as well as the
        /// rectangle defining its (visible) bounds.
        /// </summary>
        /// <param name="p">The point at which the cell entry is required.
        /// </param>
        /// <param name="rect">On successful exit, the rectangle defining the
        /// bounds of the returned cell entry. This will be an empty
        /// rectangle if no cell entry was found.</param>
        /// <returns>The cell entry which is at the point p in this control,
        /// or null if there is no cell entry at that point.</returns>
        private CellEntry GetCellEntryAt(Point p, out Rectangle rect)
        {
            // Not the best or fastest method - go through all the cell entries
            // in the entry bounds and see if the given point falls into any
            // of their bounds.
            // On the plus side, _entryBounds only contains entries which are
            // visible, but it's still a naive way of getting the correct entry.
            foreach (KeyValuePair<CellEntry, Rectangle> pair in _entryBounds)
            {
                // If we find a rectangle which contains the current location
                // ensure that the hover state is set accordingly
                if (pair.Value.Contains(p))
                {
                    rect = pair.Value;
                    return pair.Key;
                }
            }
            rect = Rectangle.Empty;
            return null;
        }

        /// <summary>
        /// Updates the cached cell data from the source with the currently
        /// set start and end dates.
        /// </summary>
        protected void UpdateCellData()
        {
            if (_diaryEntries == null) return;

            IDictionary<DateTime, Cell> map = _cells;
            map.Clear();

            Rectangle rect = this.EntryTableRect;
            foreach (IDiaryEntry entry in _diaryEntries)
            {
                DateTime resolved = ResolveDate(entry.Time);
                Cell c;
                if (!map.TryGetValue(resolved, out c))
                {
                    c = new Cell(this, rect, resolved);
                    map[resolved] = c;
                }
                c.Add(entry);
            }
            Invalidate();
        }

        /// <summary>
        /// 'Packs' this control such that the hour height is (just) big enough to
        /// be able to contain the most entries in any one cell that occurs within the
        /// cells currently held in this control.
        /// </summary>
        public void Pack()
        {
            // We need to set the hour height to be big enough to contain the
            // cell with the most entries, so go through them and find out.
            int maxEntries = 0;
            foreach (Cell c in CellEntries.Values)
            {
                maxEntries = Math.Max(maxEntries, c.Count);
            }
            if (maxEntries == 0)
            {
                _cellHeight = DEFAULT_CELL_HEIGHT;
            }
            else
            {
                _cellHeight = _entryMargin + (maxEntries * (Font.Height + 2 + (2 * _entryMargin)));
            }
            SetAutoScrollMinSize();
            UpdateCellData();
        }

        /// <summary>
        /// Checks if this dayview would show any entries with the given date/time.
        /// </summary>
        /// <param name="when">The date time to check</param>
        /// <returns>True if this day view contains the given date/time, false otherwise.
        /// </returns>
        private bool Contains(DateTime when)
        {
            return (when >= _from && when < _from.AddDays(_numDays));
        }

        /// <summary>
        /// Sets the scroll position of this control such that the given time is at the
        /// top of the scrollable pane.
        /// </summary>
        /// <param name="time">The time which should be used as the first viewable 
        /// time in this control.</param>
        public void ScrollTimeIntoView(TimeSpan time)
        {
            ScrollTimeIntoView(time, 10);
        }

        /// <summary>
        /// Sets the scroll position of this control such that the given time is at the
        /// top of the scrollable pane.
        /// </summary>
        /// <param name="time">The time which should be used as the first viewable 
        /// time in this control.</param>
        /// <param name="pixOffset">The number of extra pixels padding to push the 
        /// scrollable pane down in order to separate the desired time from the header.
        /// </param>
        public void ScrollTimeIntoView(TimeSpan time, int pixOffset)
        {
            // get the y position of the time, taking into account the height of
            // the header and an arbitrary padding amount just to take the time off
            // the border with the header.
            int y = getY(time) - HeaderHeight - pixOffset;
            scrolly = (y <= 0 ? 0 : y);
        }

        /// <summary>
        /// Gets the width of a day column given the rectangle defining the cells.
        /// </summary>
        /// <param name="rect">The rectangle which defines the bounds of the table of
        /// cells within this control.</param>
        /// <returns>The width of a single day column - note that the last column
        /// may be a slightly different width to this value because it takes the
        /// remainder of the width of the table rather than a fixed amount as the
        /// other columns do.</returns>
        private int GetDayWidth(Rectangle rect)
        {
            const int dayMargin = 1;
            return (rect.Width / _numDays) + dayMargin;
        }

        #endregion

        #region Scroll offset handling

        /// <summary>
        /// Gets the x coordinate of the current autoscroll position of this control.
        /// </summary>
        private int scrollx { get { return AutoScrollPosition.X; } }

        /// <summary>
        /// Gets or sets the y coordinate of the current autoscroll position of this
        /// control.
        /// </summary>
        private int scrolly
        {
            get { return AutoScrollPosition.Y; }
            set
            {
                UpdateCellData();
                AutoScrollPosition = new Point(AutoScrollPosition.X, value);
            }
        }

        /// <summary>
        /// Subtracts the current scroll offset from the given point.
        /// </summary>
        /// <param name="p">The point from which the scroll offset should
        /// be subtracted.</param>
        /// <returns>The given point with the scroll offset subtracted from
        /// it.</returns>
        private Point SubtractScrollOffset(Point p)
        {
            p.Offset(-scrollx, -scrolly);
            return p;
        }

        /// <summary>
        /// Adds the current scroll offset from the given point.
        /// </summary>
        /// <param name="p">The point to which the scroll offset should
        /// be added.</param>
        /// <returns>The given point with the scroll offset added to it.
        /// </returns>
        private Point AddScrollOffset(Point p)
        {
            p.Offset(scrollx, scrolly);
            return p;
        }

        /// <summary>
        /// Subtracts the scroll offset from the given rectangle.
        /// </summary>
        /// <param name="rect">The rectangle from which the scroll offset
        /// should be subtracted.</param>
        /// <returns>The rectangle after the scroll offset has been subtracted
        /// from it.</returns>
        private Rectangle SubtractScrollOffset(Rectangle rect)
        {
            rect.Offset(-scrollx, -scrolly);
            return rect;
        }

        /// <summary>
        /// Adds the scroll offset to the given rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to which the scroll offset
        /// should be added.</param>
        /// <returns>The rectangle after the scroll offset has been added to it.
        /// </returns>
        private Rectangle AddScrollOffset(Rectangle rect)
        {
            rect.Offset(scrollx,scrolly);
            return rect;
        }

        /// <summary>
        /// Sets the minimum size for the autoscroll handling of this control.
        /// </summary>
        private void SetAutoScrollMinSize()
        {
            // 150px is an arbitrary choice for a 'minimum width'
            // The height is correct for the current height of the control
            AutoScrollMinSize = new Size(_leftPanelWidth + 150, _headerHeight + (24 * _cellHeight));
        }

        #endregion

        #region Cell & Cell Entry

        /// <summary>
        /// Class representing a single cell in this weekview.
        /// </summary>
        private class Cell
        {
            // The outer weekview instance holding this cell.
            private WeekView outer;

            // The list of cell entries which make up this cell.
            private IList<CellEntry> _contents;

            // The bounds of this cell.
            private Rectangle _bounds;

            // The (resolved) date which this cell represents.
            private DateTime _date;

            /// <summary>
            /// Creates a new cell with the given parameters.
            /// </summary>
            /// <param name="host">The weekview which is hosting this cell.</param>
            /// <param name="container">The container rectangle which will form the
            /// bounds of all cells in the outer control.</param>
            /// <param name="date">The date that this cell should cover. This
            /// date will be resolved before it is used in this cell.</param>
            /// <param name="contents">The collection of cell entries that this
            /// cell should hold.</param>
            public Cell(WeekView host, Rectangle container, DateTime date, ICollection<CellEntry> contents)
            {
                outer = host;

                if (contents == null)
                    contents = new CellEntry[0];
                _contents = new List<CellEntry>(contents);
                _date = outer.ResolveDate(date);
                _bounds = InitBounds(container);
            }

            /// <summary>
            /// Creates a new empty cell with the given parameters.
            /// </summary>
            /// <param name="host">The weekview which is hosting this cell.</param>
            /// <param name="container">The container rectangle which will form the
            /// bounds of all cells in the outer control.</param>
            /// <param name="date">The date that this cell should cover. This
            /// date will be resolved before it is used in this cell.</param>
            public Cell(WeekView host, Rectangle container, DateTime date)
                : this(host, container, date, null) { }

            /// <summary>
            /// The (resolved) date that this cell represents.
            /// </summary>
            public DateTime Date { get { return _date; } }

            /// <summary>
            /// The bounds describing this cell.
            /// </summary>
            public Rectangle Bounds { get { return _bounds; } }

            /// <summary>
            /// The contents of this cell.
            /// </summary>
            public ICollection<CellEntry> Contents
            {
                get { return _contents; }
            }

            /// <summary>
            /// The number of cell entries in this cell
            /// </summary>
            public int Count
            {
                get { return _contents.Count; }
            }

            /// <summary>
            /// Checks if this cell contains the given entry.
            /// </summary>
            /// <param name="entry">The entry to check for in this cell.</param>
            /// <returns>True if this cell contains an entry with the same
            /// title and <em>resolved</em> time as the given entry.</returns>
            public bool Contains(CellEntry entry)
            {
                return _contents.Contains(entry);
            }

            /// <summary>
            /// Checks if this cell contains the given entry.
            /// </summary>
            /// <param name="entry">The entry to check for in this cell.</param>
            /// <returns>True if this cell contains an entry with the same
            /// title and <em>resolved</em> time as the given entry.</returns>
            public bool Contains(IDiaryEntry entry)
            {
                return (Find(entry) != null);
            }

            /// <summary>
            /// Gets the entry bounds of the given cell entry 
            /// </summary>
            /// <param name="entry"></param>
            /// <param name="elementHeight"></param>
            /// <returns></returns>
            public Rectangle GetEntryBounds(CellEntry entry, int elementHeight)
            {
                if (outer.ResolveDate(entry.Time) != _date)
                    throw new ArgumentException("Invalid cell entry : " + entry);

                int offsetY = (IndexOf(entry) * (elementHeight + 2 * _entryMargin));
                return new Rectangle(
                    _bounds.X + _entryMargin, _bounds.Y + _entryMargin + offsetY,
                    _bounds.Width - (2 * _entryMargin), elementHeight);
            }

            /// <summary>
            /// Initialises the bounds of this cell using the given rectangle as
            /// the bounds of the table containing this cell.
            /// </summary>
            /// <param name="rect">The rectangle defining the bounds of the weekview
            /// table which is holding this cell.</param>
            /// <returns>The rectangle defining the bounds of this cell within the
            /// given table rectangle.</returns>
            private Rectangle InitBounds(Rectangle rect)
            {
                DateTime from = outer._from;
                // which column ? 
                int dayNo = (_date - from).Days;
                if (dayNo > 6)
                {
                    throw new IndexOutOfRangeException(string.Format(
                           "Only dates between {0} and {1} allowed. Found : {2}",
                           from, from.AddDays(6), _date));
                }

                int dayWidth = outer.GetDayWidth(rect);

                // So our x is (dayNo * dayWidth) from the margin of the tableRect.
                int x = rect.X + (dayNo * dayWidth);
                // We get our Y from the time of day of the entry
                int y = outer.getY(_date.TimeOfDay);

                // We currently only do hour-granularity, so our rectangle is the
                // entire column (day) and entire row (hour)
                return new Rectangle(x, y, dayWidth, outer._cellHeight);
            }

            /// <summary>
            /// Adds the given diary entry to this cell, returning the resultant
            /// cell entry (which may be populated by other diary entries if
            /// similarly named entries already exist for this cell).
            /// </summary>
            /// <param name="entry">The entry to add to this cell.</param>
            /// <returns>The cell entry within this cell which contains the
            /// provided entry.</returns>
            /// <exception cref="ArgumentException">If the given entry's date
            /// does not fall into the date range covered by this cell.
            /// </exception>
            public CellEntry Add(IDiaryEntry entry)
            {
                if (outer.ResolveDate(entry.Time) != _date)
                {
                    throw new ArgumentException(string.Format(
                        "Cell Entry '{0}' does not fit in cell '{1}'", entry, this));
                }
                CellEntry wrapper = Find(entry);
                if (wrapper == null)
                {
                    wrapper = new CellEntry(outer, entry);
                    _contents.Add(wrapper);
                }
                else
                {
                    wrapper.Add(entry);
                }
                return wrapper;
            }

            /// <summary>
            /// Adds the given cell entry to this cell. This will see if an entry
            /// with the same title already exists in this cell and merge the two
            /// entries if it does. Otherwise it will just add the cell entry as
            /// provided.
            /// </summary>
            /// <param name="wrapper">The wrapper whose entries should be added to
            /// this cell.</param>
            /// <returns>The cell entry which was the result of adding the given
            /// entry to this cell. This may be a pre-existing entry which had the
            /// same title as the given entry, or it may be the given entry itself.
            /// </returns>
            /// <exception cref="ArgumentException">If the given wrapper's date
            /// does not fall into the date range that this cell covers.</exception>
            public CellEntry Add(CellEntry wrapper)
            {
                if (outer.ResolveDate(wrapper.Time) != _date)
                {
                    throw new ArgumentException(string.Format(
                        "Cell Entry '{0}' does not fit in cell '{1}'", wrapper, this));
                }
                foreach (CellEntry el in _contents)
                {
                    if (object.Equals(el.Title, wrapper.Title))
                    {
                        foreach (IDiaryEntry entry in wrapper.Entries)
                            el.Add(entry);
                        return el;
                    }
                }
                // if we reach here, then we found no equally-titled wrapper in this cell.
                // just add it.
                _contents.Add(wrapper);
                return wrapper;
            }

            /// <summary>
            /// Gets the index of the cell entry containing the given diary entry,
            /// or the next available index at which the entry can go. 
            /// </summary>
            /// <param name="entry">The entry to search this cell for.</param>
            /// <returns>The index at which the entry resides within this cell,
            /// or the next available index which is where the entry <em>will</em>
            /// reside if it is immediately added.</returns>
            private int IndexOf(IDiaryEntry entry)
            {
                for (int i = 0, len = _contents.Count; i < len; i++)
                {
                    if (object.Equals(_contents[i].Title, entry.Title))
                        return i;
                }
                // Otherwise, it will be a new entry in this cell - mark it
                // entering at the next available spot.
                return _contents.Count;
            }

            /// <summary>
            /// Finds the cell entry which corresponds to the given diary entry
            /// in this cell, or null if no such cell entry exists.
            /// </summary>
            /// <param name="entry">The diary entry to search for.</param>
            /// <returns>The cell entry with the same title as the given diary
            /// entry, or null if no such cell entry exists.</returns>
            public CellEntry Find(IDiaryEntry entry)
            {
                // If the given entry does not fall in this cell, return null
                // to indicate that the entry is not here.
                DateTime dt = outer.ResolveDate(entry.Time);
                if (dt != this._date)
                    return null;

                // Go through each for the cell entries to see if it matches the
                // given diary entry.
                foreach (CellEntry wrapper in _contents)
                {
                    if (object.Equals(wrapper.Title, entry.Title))
                        return wrapper;
                }
                return null;
            }
        }

        /// <summary>
        /// A wrapper for diary entries when storing the rectangles which represent
        /// them in the appropriate dictionary.
        /// </summary>
        private class CellEntry : IDiaryEntry
        {
            /// <summary>
            /// The outer weekview instance holding this wrapper.
            /// </summary>
            private WeekView outer;

            // The entry which is being wrapped.
            // private IDiaryEntry _entry;
            private ICollection<IDiaryEntry> _entries;

            /// <summary>
            /// The title to use for this entry wrapper.
            /// </summary>
            private readonly string _title;

            /// <summary>
            /// The time 'bucket' that this entry wrapper is to be held in.
            /// </summary>
            private readonly DateTime _resolvedTime;

            /// <summary>
            /// The diary entry represented by this object.
            /// </summary>
            public ICollection<IDiaryEntry> Entries { get { return _entries; } }

            /// <summary>
            /// The first entry held in this wrapped entry, or null if it
            /// contains no entries.
            /// </summary>
            public IDiaryEntry FirstEntry
            {
                get
                {
                    return _entries.OfType<IDiaryEntry>().FirstOrDefault();
                }
            }

            /// <summary>
            /// Gets the count of cell diary entries within this cell entry.
            /// </summary>
            public int Count { get { return _entries.Count; } }

            /// <summary>
            /// Creates a new cell entry hosted by the given weekview and 
            /// containing, as its first entry, the given diary entry.
            /// </summary>
            /// <param name="host">The weekview hosting this cell entry.
            /// </param>
            /// <param name="entry">The diary entry to add to this cell entry.
            /// </param>
            public CellEntry(WeekView host, IDiaryEntry entry)
            {
                if (entry == null)
                {
                    throw new ArgumentException("Wrapper must contain at least one entry", nameof(entry));
                }

                outer = host;
                _title = entry.Title;
                _resolvedTime = outer.ResolveDate(entry.Time);
                _entries = new List<IDiaryEntry>();
                Add(entry);
            }

            /// <summary>
            /// Adds the given entry to this wrapper.
            /// </summary>
            /// <param name="entry">The entry to add to this wrapper.</param>
            /// <exception cref="ArgumentException">If the given entry has a 
            /// different title, or fits into a different time slot than the
            /// other entries held in this wrapper.</exception>
            public void Add(IDiaryEntry entry)
            {
                if (entry == null)
                    throw new ArgumentNullException(nameof(entry));

                if (entry.Title == null)
                    throw new ArgumentException(
                        "Cannot create an entry wrapper without a title", "entry.Title");

                // Check that the entry fits in with others in this wrapper before
                // adding it - ie. it should have the same title and the same
                // resolved date as any others held in this weekview.
                if (_title != entry.Title)
                {
                    throw new ArgumentException(string.Format(
                        "This wrapper is for diary entries title '{0}'; Entry has title '{1}'",
                        _title, entry.Title), nameof(entry));
                }
                else if (_resolvedTime != outer.ResolveDate(entry.Time))
                {
                    throw new ArgumentException(string.Format(
                        "This wrapper is for diary entries in the '{0}' cell; Entry fits in '{1}'",
                        _resolvedTime, outer.ResolveDate(entry.Time)), nameof(entry));
                }

                _entries.Add(entry);
            }

            /// <summary>
            /// The title for this entry.
            /// </summary>
            public string Title { get { return (_title ?? ""); } }

            /// <summary>
            /// The resolved time for this entry.
            /// </summary>
            public DateTime Time { get { return _resolvedTime; } }

            /// <summary>
            /// The actual (unresolved) times for all entries in this wrapper.
            /// </summary>
            public ICollection<DateTime> ActualTimes
            {
                get
                {
                    ICollection<DateTime> times = new List<DateTime>();
                    foreach (IDiaryEntry entry in _entries)
                        times.Add(entry.Time);
                    return times;
                }
            }

            /// <summary>
            /// Checks if this represents the same entry as the given object.
            /// </summary>
            /// <param name="obj">The object to check to see if it represents the
            /// same diary entry as this object.</param>
            /// <returns>true if the given object is a non-null cell entry,
            /// representing the same resolved time and title as this cell entry;
            /// false otherwise.</returns>
            public override bool Equals(object obj)
            {
                if (!(obj is CellEntry))
                    return false;

                CellEntry ew = (CellEntry)obj;
                return (_resolvedTime == ew._resolvedTime && object.Equals(_title, ew._title));
            }

            /// <summary>
            /// Gets a hash code for this cell entry - just a function of the title
            /// and resolved time that this entry represents.
            /// </summary>
            /// <returns>A hashcode for this object based on the title and time of
            /// the cell entry.</returns>
            public override int GetHashCode()
            {
                return _title.GetHashCode() ^ _resolvedTime.GetHashCode();
            }

            /// <summary>
            /// Gets a string representation of this entry.
            /// </summary>
            /// <returns>A string describing the entry wrapped by this object.</returns>
            public override string ToString()
            {
                return _resolvedTime.ToString("[dd/MM/yyyy HH:mm:ss]") + " " + _title;
            }
        }

        #endregion

        #region OnMouseXXX event handling

        /// <summary>
        /// Handler for the mouse being moved.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            Point offsetLocn = SubtractScrollOffset(e.Location);

            if (_activeDivider.HasValue) // ie. hour-size is currently being changed.
            {
                TimeSpan curr = _activeDivider.Value;
                // Find out how far the user has moved from the current y co-ord
                // of the active time
                int moved = offsetLocn.Y - getY(curr);
                // If they've moved more than the active time's number of hours,
                // change the hour height.
                //
                // We need to do this to ensure that the cursor stays close to
                // the active hour. eg. if they are shrinking hour 6, it must
                // wait until 6 pixels have been traversed before changing the
                // hour height, since it changes hours 6, 5, 4, 3, 2 & 1, meaning
                // that for each pixel difference in hour height, hour 6's Y
                // coordinate within the control will change by 6 pixels.
                if (Math.Abs(moved) > curr.Hours)
                {
                    _cellHeight = Math.Max(_cellHeight + (moved / curr.Hours), 12);
                    UpdateCellData();
                }
                return;
            }

            if (_navMode != NavButtonState.Dormant)
            {
                if (_navMode == NavButtonState.PreviousActive)
                {
                    if (_prevRect.Contains(offsetLocn))
                        return;
                    this.Cursor = Cursors.Default;
                    _navMode = NavButtonState.Dormant;
                    Invalidate(AddScrollOffset(_prevRect));
                }
                else // ie. 'NextActive'
                {
                    if (_nextRect.Contains(offsetLocn))
                        return;
                    this.Cursor = Cursors.Default;
                    _navMode = NavButtonState.Dormant;
                    Invalidate(AddScrollOffset(_nextRect));
                }
            }
            else if (_prevRect.Contains(offsetLocn))
            {
                this.Cursor = Cursors.Hand;
                _navMode = NavButtonState.PreviousActive;
                Invalidate(AddScrollOffset(_prevRect));
            }
            else if (_nextRect.Contains(offsetLocn))
            {
                this.Cursor = Cursors.Hand;
                _navMode = NavButtonState.NextActive;
                Invalidate(AddScrollOffset(_nextRect));
            }
            else if (_hovered != null) // If a diary entry is currently set as being hovered over.
            {
                // Check that the mouse is still within the entry's bounds
                Rectangle rect;

                // Check that the cursor is still within the bounds of the hovered entry,
                // reset the tooltip / hovered state if it is not.
                // If the entry is not in the entry bounds dictionary, that means it has
                // been scrolled out of view.
                if (!_entryBounds.TryGetValue(_hovered, out rect) || 
                    !rect.Contains(offsetLocn))
                {
                    this.Cursor = Cursors.Default;
                    _hovered = null;
                    Invalidate(AddScrollOffset(rect));
                    _tooltip.Hide(this);
                    _tooltip.Tag = null;
                }
            }
            // not (currently) hovering over an entry or dragging an hour divider
            // check the dividers to see if cursor should be changed.
            else if (_dividers.ContainsKey(offsetLocn.Y))
            {
                this.Cursor = Cursors.HSplit;
            }
            // finally check if we have just started hovering over an entry
            else
            {
                this.Cursor = Cursors.Default;
                Rectangle rect;
                CellEntry entry = GetCellEntryAt(offsetLocn, out rect);
                if (entry != null)
                {
                    Cursor = Cursors.Hand;
                    _hovered = entry;
                    Invalidate(AddScrollOffset(rect));
                    _timer.Start();
                }
            }
            base.OnMouseMove(e);
        }

        /// <summary>
        /// Handler for mouseleave events.
        /// Just ensures that no entry remains in a state of hovered over.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeave(EventArgs e)
        {
            if (_hovered != null)
            {
                this.Cursor = Cursors.Default;
                Rectangle rect;
                if (_entryBounds.TryGetValue(_hovered, out rect))
                    Invalidate(AddScrollOffset(rect));
                _hovered = null;
                _tooltip.Hide(this);
                _tooltip.Tag = null;
            }
            base.OnMouseLeave(e);
        }

        /// <summary>
        /// Handler for mousedown events.
        /// Checks to see if the hour dividers are currently underneath the cursor
        /// and sets the active divider if they are.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Check if the location of the mouse when the button was pressed
                // matches any of the dividers set when the control was painted
                TimeSpan curr;
                if (_dividers.TryGetValue(e.Y - scrolly, out curr))
                    _activeDivider = curr;
            }
            base.OnMouseDown(e);
        }

        /// <summary>
        /// Handler for the mouseup event.
        /// Checks to see if an hour divider is currently active, and, if so,
        /// ensures that it is set inactive and that the control is redrawn,
        /// and that the autoscroll minsize has been set (so that the scroll
        /// bars are set to the correct size / location).
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _activeDivider.HasValue)
            {
                SetAutoScrollMinSize();
                UpdateCellData();
                _activeDivider = null;
            }
            base.OnMouseUp(e);
        }

        /// <summary>
        /// Handler for mouse double-clicks. This 'packs' the control and then
        /// resets the view such that the divider under the mouse remains under
        /// the mouse after the control has been resized.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDoubleClick(EventArgs e)
        {
            Point mouseLocn = PointToClient(Cursor.Position); 
            TimeSpan curr;
            if (_dividers.TryGetValue(mouseLocn.Y - scrolly, out curr))
            {
                Pack();
                ScrollTimeIntoView(curr, mouseLocn.Y - _headerHeight);
            }
        }

        /// <summary>
        /// Handler for the mouse being clicked.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (_navMode == NavButtonState.PreviousActive)
                {
                    StartDate = StartDate.AddDays(-NumberOfDays);
                }
                else if (_navMode == NavButtonState.NextActive)
                {
                    StartDate = StartDate.AddDays(NumberOfDays);
                }
                if (_hovered != null)
                {
                    OnDiaryEntryClicked(new DiaryEntryClickedEventArgs(e, _hovered.Entries));
                }
            }
            base.OnMouseClick(e);
        }



        #endregion

        #region OnPaint and the DrawXXX methods

        /// <summary>
        /// Handler for painting this control.
        /// </summary>
        /// <param name="pe">The event args detailing the paint event.</param>
        protected override void OnPaint(PaintEventArgs pe)
        {
            Graphics g = pe.Graphics;

            // take into account the scrollbar locations...
            g.Transform = new Matrix(1, 0, 0, 1, scrollx, scrolly);
            g.Clear(Color.White);

            Font f = this.Font;

            // left panel first
            DrawLeftPanel(g, this.LeftPanelRect);

            DrawOrigin(g, this.OriginRect);
            DrawHeader(g, this.HeaderRect, f);

            Rectangle entryTableRect = this.EntryTableRect;
            Rectangle viewableTableRect = SubtractScrollOffset(entryTableRect);

            g.SetClip(viewableTableRect);

            // Draw the hours into the entry list
            DrawHours(g, entryTableRect, null);

            // Now draw the entries themselves
            DrawEntries(g, entryTableRect, f);

            using (Pen pen = new Pen(_borderColour))
            {
                // just allows the rounded paths (left panel + table) to merge
                viewableTableRect.X--;
                GraphicsPath path = GraphicsUtil.CreateRoundedRectangle(viewableTableRect, 5);
                g.DrawPath(pen, path);
            }
        }

        /// <summary>
        /// Draws the origin of the dayview - the top left corner of the control
        /// </summary>
        /// <param name="g">The graphics context with which to draw the origin.
        /// </param>
        /// <param name="rect">The rectangle detailing the area constraining the
        /// origin.</param>
        protected void DrawOrigin(Graphics g, Rectangle rect)
        {
            if (!g.IsVisible(rect)) // if it's being clipped, don't bother doing the work.
                return;

            const int navButtonSize = 15;
                        
            g.FillRectangle(new LinearGradientBrush(
                rect, Color.PowderBlue, Color.White, LinearGradientMode.Vertical), rect);

            _prevRect = new Rectangle(
                rect.X + (rect.Width / 2) - navButtonSize - 1,
                rect.Y + ((rect.Height - navButtonSize) / 2),
                navButtonSize, navButtonSize);

            _nextRect = new Rectangle(
                rect.X + (rect.Width / 2) + 1,
                rect.Y + ((rect.Height - navButtonSize) / 2), 
                navButtonSize, navButtonSize);

            Point mouseLocn = PointToClient(Cursor.Position);

            /* For some reason, these rounded rectangles are coming out only rounded
             * on top... can't figure out why.
            g.FillPath(Brushes.DarkCyan, GraphicsUtil.CreateRoundedRectangle(before, 1));
            g.FillPath(Brushes.DarkCyan, GraphicsUtil.CreateRoundedRectangle(after, 1));
             * */

            g.FillRectangle(
                _navMode == NavButtonState.PreviousActive ? Brushes.MediumBlue : Brushes.SkyBlue,
                _prevRect);

            g.FillRectangle(
                _navMode == NavButtonState.NextActive ? Brushes.MediumBlue : Brushes.SkyBlue,
                _nextRect);

            g.FillPolygon(Brushes.LightGoldenrodYellow, new Point[]{
                new Point(_prevRect.Right - 3, _prevRect.Y + 2),
                new Point(_prevRect.Right - 3, _prevRect.Bottom - 2),
                new Point(_prevRect.X + 3, _prevRect.Y + (_prevRect.Height / 2))}
            );

            g.FillPolygon(Brushes.LightGoldenrodYellow, new Point[]{
                new Point(_nextRect.X + 3, _nextRect.Y + 2),
                new Point(_nextRect.X + 3, _nextRect.Bottom - 2),
                new Point(_nextRect.Right - 3, _nextRect.Y + (_nextRect.Height / 2))}
            );
        }

        /// <summary>
        /// Draws an entry on the control with the given characteristics.
        /// </summary>
        /// <param name="g">The graphics context with which to draw the entry.
        /// </param>
        /// <param name="rect">The rectangle defining the bounds of the entry.
        /// </param>
        /// <param name="label">The label to draw on the entry.</param>
        /// <param name="f">The font to use for the entry.</param>
        /// <param name="c">The background colour to use for the entry.
        /// </param>
        protected void DrawEntry(Graphics g, Rectangle rect, string label, Font f, Color c)
        {
            //if (!IsVisible(g, rect)) // if it's being clipped, don't bother doing the work.
            //    return;

            // no point in drawing something no-one will see.
            if (rect.Width <= 0 || rect.Height <= 0)
                return;

            GraphicsPath path = GraphicsUtil.CreateRoundedRectangle(rect, 3);

            g.FillPath(
             new LinearGradientBrush(rect, Color.White, c, LinearGradientMode.Horizontal),
             path);
            g.DrawPath(new Pen(_borderColour), path);
            StringFormat sf = new StringFormat(StringFormatFlags.NoWrap);
            sf.Trimming = StringTrimming.None;
            g.DrawString(label, f, Brushes.Black, GraphicsUtil.Shrink(rect, 1), sf);
        }

        /// <summary>
        /// Draws the header area of the control with the dates across the top.
        /// </summary>
        /// <param name="g">The graphics context to use to draw the header.
        /// </param>
        /// <param name="rect">The rectangle defining the bounds of the header.
        /// </param>
        /// <param name="f">The font to use to draw the dates across the top.
        /// </param>
        protected void DrawHeader(Graphics g, Rectangle rect, Font f)
        {
            if (!IsVisible(g, rect)) // if it's being clipped, don't bother doing the work.
                return;

            StringFormat sf = new StringFormat(StringFormatFlags.NoWrap);
            sf.Trimming = StringTrimming.EllipsisPath;
            sf.Alignment = StringAlignment.Center;

            using (Brush brush = new SolidBrush(this.ForeColor))
            {
                rect = SubtractScrollOffset(rect);
                g.FillRectangle(
                 new LinearGradientBrush(rect, Color.PowderBlue, Color.White, LinearGradientMode.Vertical),
                 rect);

                // Take the first date as the height of the string
                Size size = g.MeasureString(_from.ToString(DateFormat, CultureInfo.CurrentCulture), f).ToSize();

                for (int i = 0; i < _numDays; i++)
                {
                    DateTime dt = _from.AddDays(i);
                    int x = getX(dt, rect);
                    int y = _headerHeight - size.Height;
                    Rectangle r = new Rectangle(x, rect.Y + y, rect.Width / _numDays, _headerHeight);
                    g.DrawString(dt.ToString(this.DateFormat, CultureInfo.CurrentCulture), f, brush, r, sf);
                }
            }
        }

        /// <summary>
        /// Checks if the given rectangle is visible within the given graphics context
        /// or whether it will be clipped, taking into account the current scroll offset.
        /// </summary>
        /// <param name="g">The graphics context to check for visibility.</param>
        /// <param name="rect">The rectangle to check is visible within the
        /// graphics context.</param>
        /// <returns>true if the given rectangle is visible, false otherwise.</returns>
        private bool IsVisible(Graphics g, Rectangle rect)
        {
            return g.IsVisible(SubtractScrollOffset(rect));
        }

        /// <summary>
        /// Draws the entries themselves in the given table rectangle
        /// </summary>
        /// <param name="g">The graphics context with which the entries should be
        /// drawn.</param>
        /// <param name="rect">The rectangle defining the entry table - ie. below the
        /// header and to the right of the left panel.</param>
        /// <param name="f">The font to use to  write the entry titles in the
        /// entries themselves.</param>
        protected void DrawEntries(Graphics g, Rectangle rect, Font f)
        {
            if (!IsVisible(g, rect)) // if it's being clipped, don't bother doing the work.
                return;

            int elementHeight = f.Height + 2;
            Dictionary<DateTime, int> entered = new Dictionary<DateTime, int>();

            int dayWidth = GetDayWidth(rect);

            // Draw the date-lines
            using (Pen pen = new Pen(_borderColour))
            {
                pen.DashStyle = DashStyle.Dash;
                for (int i = 1; i < _numDays; i++)
                {
                    int x = rect.X + (i * dayWidth);
                    int y = rect.Y - scrolly;
                    g.DrawLine(pen, x, y, x, y + rect.Height);
                }
            }
            _entryBounds.Clear();

            foreach (Cell cell in CellEntries.Values)
            {
                Rectangle cellRect = cell.Bounds;

                // If the cell isn't visible at all, skip it.
                //if (!IsVisible(g, cellRect))
                //    continue;

                // Clip to the cell.
                Region originalClip = g.Clip;
                
                // GraphicsState state = g.Save();
                g.IntersectClip(cell.Bounds);

                foreach (CellEntry entry in cell.Contents)
                {
                    Rectangle entryRect = cell.GetEntryBounds(entry, elementHeight);
                    if (!g.IsVisible(entryRect))
                        continue;

                    // Get the colour for the entry - simple if it's being hovered over,
                    // otherwise either get the already set colour from _entryColours.
                    // If it's not been set yet, pick the next colour from _availableColours
                    // and assign it to that title.
                    Color col;
                    if (entry.Equals(_hovered))
                    {
                        col = Color.PaleVioletRed;
                    }
                    else
                    {
                        if (!_colourMap.TryGetValue(entry.Title, out col))
                        {
                            col = ENTRY_COLOURS[(++_colourIndex) % ENTRY_COLOURS.Count];
                            _colourMap[entry.Title] = col;
                        }
                    }

                    DrawEntry(g, entryRect, entry.Title, f, col);

                    // We want to store the visible rectangle for the entry, not the full rectangle
                    // so that hovering over a clipped entry will register the correct entry (ie.
                    // the one above the clipped one - the visible one).
                    entryRect.Intersect(Rectangle.Round(g.ClipBounds));
                    _entryBounds[entry] = entryRect;

                }

                // Remove the cell's clipping from the c
                g.Clip = originalClip;
            }
        }

        /// <summary>
        /// Draws the dividers and hours in the left panel.
        /// </summary>
        /// <param name="g">The graphics context to use to draw the hours</param>
        /// <param name="rect">The rectangle defining the panel in which the hours
        /// should be drawn.</param>
        /// <param name="f">The font with which the hours should be drawn, null to
        /// indicate that the hours should not be drawn.</param>
        protected void DrawHours(Graphics g, Rectangle rect, Font f)
        {
            // if it's being clipped, don't bother doing the work.
            // We need to subtract the offset for the table, but not for the left panel,
            // hence the 'f==null' check - if f==null then it's the table, otherwise
            // it's the left panel
            if (!g.IsVisible(f==null ? SubtractScrollOffset(rect) : rect)) 
                return;

            TimeSpan highlightFrom = new TimeSpan(9, 0, 0);
            TimeSpan highlightTo = new TimeSpan(17, 0, 0);

            using (Pen pen = new Pen(_borderColour))
            {
                pen.DashStyle = DashStyle.Dot;
                _dividers.Clear();
                for (TimeSpan time = TimeSpan.Zero; time < END_OF_DAY; time += _cellTime)
                {
                    int y = getY(time);

                    // If we're drawing the table, check with the highlighter to 
                    // see if the cells should be highlighted or not.
                    // If they're not visible, then don't bother.
                    if (f == null && 
                        (g.IsVisible(rect.Left, y + _cellHeight) || g.IsVisible(rect.Left, y)))
                    {
                        int dayWidth = GetDayWidth(rect); 
                        for (int i = 0; i < _numDays; i++)
                        {
                            Color? c = Highlighter.GetHighlight(_from.AddDays(i) + time);
                            if (c.HasValue)
                            {
                                int x = rect.X + (i * dayWidth);

                                g.FillRectangle(new SolidBrush(c.Value),
                                    new Rectangle(x, y, dayWidth, _cellHeight));
                            }
                        }
                    }
                    

                    // if it's not being clipped out, add the line for the hours,
                    // the resize dividers and write the time string out
                    if (g.IsVisible(rect.Left, y))
                    {
                        if (time != TimeSpan.Zero) // skip the midnight line...
                        {
                            g.DrawLine(pen, rect.Left, y, rect.Right, y);
                            _dividers.Add(y - 1, time); // a bit of give either way
                            _dividers.Add(y, time);
                            _dividers.Add(y + 1, time);

                        }
                        if (f != null)
                        {
                            string timeStr = (DateTime.Today + time).ToString(Resources.DateTimeFormat_WeekView_DrawHours_HHMm);
                            g.DrawString(timeStr, f,
                                new SolidBrush(this.ForeColor),
                                (float)rect.Right - g.MeasureString(timeStr, f).Width, y);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draws the left hand panel and the hours therein.
        /// </summary>
        /// <param name="g">The graphics context with which to draw the panel.
        /// </param>
        /// <param name="rect">The rectangle defining the bounds of the panel.
        /// </param>
        protected void DrawLeftPanel(Graphics g, Rectangle rect)
        {
            if (!g.IsVisible(rect)) // if it's being clipped, don't bother doing the work.
                return;

            // no point in drawing something no-one will see.
            if (rect.Width <= 0 || rect.Height <= 0)
                return;

            GraphicsState state = g.Save();
            GraphicsPath path = GraphicsUtil.CreateRoundedRectangle(rect, 5);

            using (Brush brush = new LinearGradientBrush(
                rect, Color.LightGoldenrodYellow, Color.White, LinearGradientMode.Vertical))
            {
                g.FillPath(brush, path);
            }

            using (Pen pen = new Pen(_borderColour))
            {
                g.DrawPath(pen, path);
            }
            g.SetClip(rect);
            DrawHours(g, rect, this.Font);

            g.Restore(state);
            // now work down from midnight and draw the ticks along the way
        }

        #endregion

        #region Other UI event handlers

        /// <summary>
        /// Handles the control being resized - just ensures that the cell data
        /// is updated.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            UpdateCellData();
            base.OnResize(e);
        }

        #endregion

        #region DayView-specific events

        /// <summary>
        /// Delegate to handle a diary entry event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments detailing the diary entry which
        /// is affected by the event.</param>
        public delegate void DiaryEntryEventHandler(object sender, DiaryEntryClickedEventArgs e);

        /// <summary>
        /// Delegate to handle the date being changed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments detailing the event.</param>
        public delegate void DateChangedEventHandler(
            object sender, DiaryViewDateRangeChangingEventArgs e);

        /// <summary>
        /// Event fired when a diary entry has been clicked.
        /// </summary>
        public event DiaryEntryEventHandler DiaryEntryClick;

        /// <summary>
        /// Event fired when the date range that is being viewed in this control
        /// is changed.
        /// </summary>
        public event DateChangedEventHandler DateRangeChanging;

        /// <summary>
        /// Handler for a diary entry being clicked within the day view.
        /// </summary>
        /// <param name="e">The arguments detailing the diary entry which has
        /// been clicked.</param>
        protected virtual void OnDiaryEntryClicked(DiaryEntryClickedEventArgs e)
        {
            if (DiaryEntryClick != null)
            {
                DiaryEntryClick(this, e);
            }
        }

        /// <summary>
        /// Handler for the view's date range being changed.
        /// </summary>
        /// <param name="e">The arguments detailing the event.</param>
        protected virtual void OnDateRangeChanging(DiaryViewDateRangeChangingEventArgs e)
        {
            if (DateRangeChanging != null)
            {
                DateRangeChanging(this, e);
                
            }
            _diaryEntries = Source?.GetEntries(e.From, e.To);
        }

        /// <summary>
        /// Fires the date range changing event.
        /// </summary>
        /// <param name="from">The start date that the date range is changing to
        /// </param>
        /// <param name="numDays">The number of days being changed to in this
        /// view.</param>
        protected void FireDateRangeChanging(DateTime from, int numDays)
        {
            OnDateRangeChanging(
                new DiaryViewDateRangeChangingEventArgs(from, from.AddDays(numDays)));
        }

        #endregion

        #region Translating dates/times to locations/regions

        /// <summary>
        /// Gets the index of the row in which the given time falls - effectively
        /// the 'y cell' coordinate.
        /// </summary>
        /// <param name="time">The time for which the cell number is required.
        /// </param>
        /// <returns>The zero-based index indicating which row within the table
        /// that the given time falls in.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the given time was
        /// negative or at or beyond the end of the day</exception>
        private int GetRow(TimeSpan time)
        {
            if (time < TimeSpan.Zero || time >= END_OF_DAY)
            {
                throw new ArgumentOutOfRangeException(nameof(time),
                    "Time must be between 00:00 and 23:59:59.999");
            }

            return (int)(time.Ticks / _cellTime.Ticks);
        }

        /// <summary>
        /// Resolves the given time into one which is represented as a row on
        /// this control. Currently that means that it gets the whole hour of
        /// the specified time, since each row represents an hour.
        /// </summary>
        /// <param name="time">The time to be resolved.</param>
        /// <returns>The time which directly maps onto a row in this control.
        /// </returns>
        private TimeSpan ResolveTime(TimeSpan time)
        {
            // To resolve the time, we divide the time by the celltime to find out
            // how many cells down it ends up in, and multiply that (floored) value
            // by the cell time itself.
            return new TimeSpan(GetRow(time) * _cellTime.Ticks);
        }

        /// <summary>
        /// Resolves the given date/time into which is directly represented
        /// as a cell on this control.
        /// </summary>
        /// <param name="date">The date to be resolved.</param>
        /// <returns>The date which directly maps onto a cell on this control.
        /// </returns>
        private DateTime ResolveDate(DateTime date)
        {
            return date.Date + ResolveTime(date.TimeOfDay);
        }

        /// <summary>
        /// Gets the X co-ordinate of the origin of the cell for the given 
        /// date/time, given the specified rectangle for the table.
        /// </summary>
        /// <param name="dt">The date/time for which the x coordinate is
        /// required.</param>
        /// <param name="tableRect">The rectangle detailing the table of
        /// diary entries shown on this control.</param>
        /// <returns>The x coordinate of the given date/time using the
        /// specified rectangle as a base.</returns>
        private int getX(DateTime dt, Rectangle tableRect)
        {
            // which column ? 
            int dayNo = (dt - _from).Days;
            if (dayNo >= _numDays)
            {
                throw new IndexOutOfRangeException(string.Format(
                       "Only dates between {0} and {1} allowed. Found : {2}",
                       _from, _from.AddDays(_numDays-1), dt));
            }

            // So our x is (dayNo * day-width) from the margin of the listRect.
            return tableRect.X + _entryMargin + (dayNo * GetDayWidth(tableRect));
        }

        /// <summary>
        /// Gets the Y coordinate of the required time of day.
        /// </summary>
        /// <param name="time">The time of day for which the y coordinate
        /// is required.</param>
        /// <returns>The y coordinate of the origin of the row which represents
        /// the given time of day.</returns>
        protected int getY(TimeSpan time)
        {
            return _headerHeight + (GetRow(time) * _cellHeight);
        }

        /// <summary>
        /// Gets the rectangle for the given date/time using the given rectangle
        /// as a base.
        /// </summary>
        /// <param name="tableRect">The rectangle definining the entry table
        /// in which the cells are defined.</param>
        /// <param name="time">The date / time for which the requisite cell is
        /// required.</param>
        /// <returns>The rectangle defining the cell in which the given date /
        /// time falls.</returns>
        protected Rectangle GetCellRectangle(Rectangle tableRect, DateTime time)
        {
            // which column ? 
            int dayNo = (time - _from).Days;
            if (dayNo > 6)
            {
                throw new IndexOutOfRangeException(string.Format(
                       "Only dates between {0} and {1} allowed. Found : {2}",
                       _from, _from.AddDays(6), time));
            }

            int dayWidth = GetDayWidth(tableRect);

            // So our x is (dayNo * dayWidth) from the margin of the tableRect.
            int x =  tableRect.X + (dayNo * dayWidth);
            // We get our Y from the time of day of the entry
            int y = getY(time.TimeOfDay);

            // We currently only do hour-granularity, so our rectangle is the
            // entire column (day) and entire row (hour)
            return new Rectangle(x, y, dayWidth, _cellHeight);
        }

        #endregion

        #region Scroll handling

        /// <summary>
        /// Checks if the given key is an input key or not.
        /// </summary>
        /// <param name="keyData">The key to check if it is an input key on this
        /// control.</param>
        /// <returns>true if the key is a cursor key, otherwise this delegates the
        /// check to the base class.</returns>
        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        /// <summary>
        /// Handler for a keydown event.
        /// </summary>
        /// <param name="e">The arguments detailing the keydown event</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            _isControlDown = e.Control;
            switch (e.KeyCode)
            {
                case Keys.Up: scrolly = -scrolly - 10; break;
                case Keys.Down: scrolly = -scrolly + 10; break;
                case Keys.PageUp: scrolly = -scrolly - 200; break;
                case Keys.PageDown: scrolly = -scrolly + 200; break;
                default:
                    base.OnKeyDown(e);
                    break;
            }
        }

        /// <summary>
        /// Handler for the keyup event.
        /// </summary>
        /// <param name="e">The arguments detailing the keyup event.</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            _isControlDown = false;
            base.OnKeyUp(e);
        }

        /// <summary>
        /// Handler for the mousewheel event occurring on this control.
        /// </summary>
        /// <param name="e">The arguments detailing the event.</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            // ctrl-mousewheel 'zooms' in and out of the view.
            // if we're zooming, the cell data (including rectangles) needs
            // to be updated. If just scrolling then it's not necessary.
            if (_isControlDown)
            {
                _cellHeight = Math.Max(12, _cellHeight + (e.Delta < 0 ? -2 : 2));
                SetAutoScrollMinSize();
                UpdateCellData();
            }
            else
            {
                base.OnMouseWheel(e);
                Invalidate();
            }
            
        }

        #endregion

        #region Windows Message handling

        /// <summary>Windows messages (WM_*, look in winuser.h)</summary>
        private enum WindowsMessage
        {
            WM_MOUSEWHEEL = 0x020A,
            WM_HSCROLL = 0x0114,
            WM_VSCROLL = 0x0115
        }

        /// <summary>
        /// Overrides the windows message procesing for the scroll / mousewheel
        /// events by invalidating the control, thus ensuring that the control
        /// is repainted.
        /// </summary>
        /// <param name="m">The windows message to process.</param>
        /// <remarks>This is here rather than in an OnScroll override because
        /// when the invalidation was done on the scroll event, the fixed areas
        /// of the control would judder. Here, it has a chance to alter the
        /// paint location before the painting done by autoscroll is performed,
        /// and the fixed areas remain fixed.</remarks>
        protected override void WndProc(ref Message m)
        {
            switch ((WindowsMessage)m.Msg)
            {
                case WindowsMessage.WM_VSCROLL:
                case WindowsMessage.WM_HSCROLL:
                case WindowsMessage.WM_MOUSEWHEEL:
                    Invalidate();
                    break;
            }
            base.WndProc(ref m);
        }

        #endregion

        #region Tooltip handling

        /// <summary>
        /// A timer which handles the tooltip showing if it needs to be shown.
        /// </summary>
        private class TooltipTimer : Timer
        {
            /// <summary>
            /// The day view that this timer is acting on behalf of.
            /// </summary>
            private WeekView outer;

            /// <summary>
            /// Creates a new tooltip timer which acts on behalf of the specified
            /// day view, and checks if a tooltip is required at the given interval
            /// </summary>
            /// <param name="outer">The dayview for which a tooltip is being shown
            /// where appropriate.</param>
            /// <param name="interval">The interval in milliseconds that this timer
            /// should fire, invoking the checking for and subsequent display of
            /// the tooltip.</param>
            public TooltipTimer(WeekView outer, double interval)
                : base(interval)
            {
                this.outer = outer;
                this.AutoReset = true;
                this.Elapsed += HandleTick;
            }

            /// <summary>
            /// Event handler for the timer being fired.
            /// </summary>
            private void HandleTick(object sender, ElapsedEventArgs e)
            {
                if (outer.IsHandleCreated)
                {
                    outer.BeginInvoke((MethodInvoker)delegate() { outer.ShowToolTip(); });
                }
            }
        }

        /// <summary>
        /// Shows or hides the tooltip depending on whether the cursor is currently
        /// over a schedule instance or not.
        /// </summary>
        private void ShowToolTip()
        {
            CellEntry wrapper = _hovered;
            if (wrapper != null)
            {
                // No point in reshowing if it's already there
                if (_tooltip.Active && object.ReferenceEquals(wrapper, _tooltip.Tag))
                    return;

                Point mouseLocn = PointToClient(Cursor.Position);
                // Check that the mouse is still within the entry's bounds
                Rectangle rect;

                // If the entry is not in the entry bounds dictionary, that means it has
                // been scrolled out of view.
                if (_entryBounds.TryGetValue(_hovered, out rect) && 
                    rect.Contains(SubtractScrollOffset(mouseLocn)))
                {
                    mouseLocn.Offset(16, 16);
                    _tooltip.Tag = wrapper;
                    StringBuilder sb = new StringBuilder(wrapper.Title).Append(Environment.NewLine);
                    foreach (DateTime time in new clsSortedSet<DateTime>(wrapper.ActualTimes))
                    {
                        sb.Append(time.ToString(Resources.DateTimeFormat_WeekView_ShowToolTip_HHMmSs));
                    }
                    sb.Length -= 2;
                    _tooltip.Show(sb.ToString(), this, mouseLocn);
                }
            }
            else
            {
                _tooltip.Tag = null;
                _tooltip.Hide(this);
            }
        }

        #endregion

    }

}
