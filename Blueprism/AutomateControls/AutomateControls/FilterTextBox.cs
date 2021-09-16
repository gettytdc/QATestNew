using AutomateControls.Properties;
using System;
using System.Windows.Forms;
using System.ComponentModel;
using BluePrism.Images;

namespace AutomateControls
{
    /// <summary>
    /// Textbox specifically designed for dealing with filters.
    /// This accepts text input and triggers a FilterTextChanged event
    /// a second after the user has stopped typing or if they hit Enter.
    /// </summary>
    public class FilterTextBox: IconTextBox
    {
        #region - Constants / Statics -

        /// <summary>
        /// The default tooltip for the clear icon in the textbox
        /// </summary>
        public const string DefaultClearTip = "Clear Filter";

        ///// <summary>
        ///// Enum containing the image numbers for the image list used by
        ///// this filter text box
        ///// </summary>
        //private enum ImageNo
        //{
        //  None = -1, MagGlass = 0, GreyX = 1, RedX1 = 2, RedX2 = 3
        //}

        #endregion

        #region - Published Events -

        /// <summary>
        /// Event fired when the filter text has been changed and committed.
        /// </summary>
        [Category("Filter"),
         Description("Occurs when the filter text has a new, committed value")]
        public event FilterEventHandler FilterTextChanged;

        /// <summary>
        /// Event fired when the user clicks on the filter icon, after the
        /// filter text has been selected.
        /// </summary>
        [Category("Mouse"),
         Description("Occurs when the mouse clicks within the filter image bounds")]
        public event EventHandler FilterIconClick;

        /// <summary>
        /// Event fired when the filter has been cleared. Note that clearing
        /// the filter <em>does not</em> fire the FilterTextChanged event.
        /// </summary>
        [Category("Filter"),
         Description("Occurs when the filter text has been cleared")]
        public event EventHandler FilterCleared;

        #endregion

        #region - Member Variables -

        // The previous filter fired in an event from this textbox.
        private string _prevFilter;

        // The timer waiting a second after typing before firing the event.
        private Timer _timer;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new empty FilterTextBox
        /// </summary>
        public FilterTextBox()
        {
            _prevFilter = "";
            Images = ImageLists.Filtering_16x16;
            NearImageDefaultKey = ImageLists.Keys.Filtering.Search;
            FarImageDefaultKey = ImageLists.Keys.Filtering.Clear_Disabled;
            AlwaysShowHandOnFarHover = true;
            AlwaysShowHandOnNearHover = true;
            FarTip = Resources.FilterTextBox_ClearFilter;
            TrapEnter = true;
            TrapEscape = false;

            _timer = new Timer();
            _timer.Interval = 1000;
            _timer.Tick += HandleTimerTick;
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// The number of milliseconds to wait after the user has stopped
        /// typing before firing a FilterTextChanged event.
        /// </summary>
        [Description(
            "The number of milliseconds to wait after the user has stopped typing " + 
            "before firing a FilterTextChanged event."), DefaultValue(1000)]
        public int PostTypingWaitInterval
        {
            get { return _timer.Interval; }
            set { _timer.Interval = value; }
        }

        [Browsable(true), Category("Appearance"), DefaultValue(null),
         Description("The tooltip to display for the search icon")]
        public string SearchTip
        {
            get { return NearTip; }
            set { NearTip = value; }
        }

        [Browsable(true), Category("Appearance"), DefaultValue(DefaultClearTip),
         Description("The tooltip to display for the clear icon")]
        public string ClearTip
        {
            get { return FarTip; }
            set { FarTip = value; }
        }

        #endregion

        #region - Hiding Overloaded Properties -

        [Bindable(false), EditorBrowsable(EditorBrowsableState.Never), Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ImageList Images
        {
            get { return base.Images; }
            set { base.Images = value; }
        }

        [Bindable(false), EditorBrowsable(EditorBrowsableState.Never), Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new int NearImage
        {
            get { return base.NearImage; }
        }

        [Bindable(false), EditorBrowsable(EditorBrowsableState.Never), Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new int FarImage
        {
            get { return base.FarImage; }
        }

        [Bindable(false), EditorBrowsable(EditorBrowsableState.Never), Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new int NearImageDefault
        {
            get { return base.NearImageDefault; }
            set { base.NearImageDefault = value; }
        }

        [Bindable(false), EditorBrowsable(EditorBrowsableState.Never), Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new int FarImageDefault
        {
            get { return base.FarImageDefault; }
            set { base.FarImageDefault = value; }
        }

        [Bindable(false), EditorBrowsable(EditorBrowsableState.Never), Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new int NearImageHover
        {
            get { return base.NearImageHover; }
            set { base.NearImageHover = value; }
        }

        [Bindable(false), EditorBrowsable(EditorBrowsableState.Never), Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new int FarImageHover
        {
            get { return base.FarImageHover; }
            set { base.FarImageHover = value; }
        }

        [Bindable(false), EditorBrowsable(EditorBrowsableState.Never), Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string NearTip
        {
            get { return base.NearTip; }
            set { base.NearTip = value; }
        }

        [Bindable(false), EditorBrowsable(EditorBrowsableState.Never), Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string FarTip
        {
            get { return base.FarTip; }
            set { base.FarTip = value; }
        }

        [Bindable(false), EditorBrowsable(EditorBrowsableState.Never), Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool TrapEscape
        {
            set { base.TrapEscape = value; }
            get { return base.TrapEscape; }
        }

        [Bindable(false), EditorBrowsable(EditorBrowsableState.Never), Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool TrapEnter
        {
            set { base.TrapEnter = value; }
            get { return base.TrapEnter; }
        }

        #endregion

        #region - Event Handlers -

        /// <summary>
        /// Handles the text changing in this textbox, ensuring that the
        /// timer is reset to fire the filter events a period of time
        /// after the user has stopped typing.
        /// </summary>
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            _timer.Stop();
            _timer.Start();
        }

        /// <summary>
        /// Handles this textbox being activated - this fires the filter
        /// update events immediately.
        /// </summary>
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            FireFilterUpdates();
        }

        /// <summary>
        /// Handles the textbox being escaped - this clears the filter
        /// and fires the filter update event immediately.
        /// </summary>
        protected override void OnEscaped(EventArgs e)
        {
            base.OnEscaped(e);
            this.Text = "";
            FireFilterUpdates();
        }

        /// <summary>
        /// Handles the filter text changed event - occurs after the timer
        /// period has elapsed, or this textbox has been activated.
        /// </summary>
        protected virtual void OnFilterTextChanged(FilterEventArgs e)
        {
            // With a filter in place, we want 'escape' to mean 'clear filter',
            // so ensure we are trapping it
            TrapEscape = true;
            FarImageDefaultKey = ImageLists.Keys.Filtering.Clear;
            FarImageHoverKey = ImageLists.Keys.Filtering.Clear;
            if (FilterTextChanged != null)
                FilterTextChanged(this, e);
        }

        /// <summary>
        /// Handles the filter icon being clicked.
        /// </summary>
        protected virtual void OnFilterIconClick(EventArgs e)
        {
            if (FilterIconClick != null)
                FilterIconClick(this, e);
        }

        /// <summary>
        /// Handles the filter being cleared
        /// </summary>
        protected virtual void OnFilterCleared(EventArgs e)
        {
            // With a clear filter, let 'escape' mean whatever it means in the
            // wider context (cancel form or such like)
            TrapEscape = false;
            FarImageDefaultKey = ImageLists.Keys.Filtering.Clear_Disabled;
            FarImageHoverKey = null;
            if (FilterCleared != null)
                FilterCleared(this, e);
        }

        /// <summary>
        /// Handles the timer ticking over - this means that the user
        /// has typed, and the timer period has elapsed, meaning that
        /// the filter is ready to be updated.
        /// </summary>
        private void HandleTimerTick(object sender, EventArgs e)
        {
            FireFilterUpdates();
        }

        /// <summary>
        /// Handles a near-image click. This is converted into
        /// something more meaningful for this class - namely, it
        /// selects the text, and fires an <see cref="OnFilterIconClick"/>
        /// event.
        /// </summary>
        protected override void OnNearImageClick(MouseEventArgs e)
        {
            SelectAll();
            OnFilterIconClick(EventArgs.Empty);
        }

        /// <summary>
        /// Handles a far-image click.This clears the textbox and fires
        /// an <see cref="OnFilterCleared"/> event, unless the filter
        /// was already clear.
        /// </summary>
        protected override void OnFarImageClick(MouseEventArgs e)
        {
            base.OnFarImageClick(e);
            base.Text = "";
            FireFilterUpdates();
        }

        #endregion

        #region - Other Methods -

        /// <summary>
        /// Fires the appropriate filter updates with the textbox's
        /// current text.
        /// If the text has changes since the last filter change, either
        /// the <see cref="FilterTextChanged"/> event or the
        /// <see cref="FilterCleared"/> event are fired, depending on whether
        /// the text is empty or not.
        /// </summary>
        private void FireFilterUpdates()
        {
            _timer.Stop();
            string txt = this.Text;
            if (txt != _prevFilter)
            {
                _prevFilter = txt;
                if (txt == "")
                    OnFilterCleared(EventArgs.Empty);
                else
                    OnFilterTextChanged(new FilterEventArgs(txt));
            }
        }

        /// <summary>
        /// Disposes of this class, ensuring that the timer and image list
        /// held within are disposed of at the same time.
        /// </summary>
        /// <param name="explicitly">true to indicate that the Dispose()
        /// method was called explicitly, False to indicate it was called
        /// as a result of finalizers being run.</param>
        protected override void Dispose(bool explicitly)
        {
            if (!IsDisposed)
            {
                base.Dispose(explicitly);
                if (explicitly)
                {
                    _timer.Dispose();
                }
            }
        }

        #endregion

    }
}
