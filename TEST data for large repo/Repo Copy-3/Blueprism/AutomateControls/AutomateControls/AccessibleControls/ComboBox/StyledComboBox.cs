using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms.VisualStyles;
using System.Runtime.InteropServices;
using BluePrism.BPCoreLib.Collections;
using System.Linq;

namespace AutomateControls
{
    /// <summary>
    /// Combo box which supports the styling of its items via instances of the
    /// <see cref="ComboBoxItem"/> class. Note that this control is designed for
    /// drop down lists only; ie. not for combo boxes with an 'edit' component.
    /// 
    /// Any items set in this combo box which are not derived from the associated
    /// ComboBoxItem class will be enabled by default.
    /// </summary>
    [ToolboxBitmap(typeof(ComboBox))]
    public class StyledComboBox : ComboBox
    {
        #region - Class-scope Declarations -

        /// <summary>
        /// Method used to retrieve information about a combo box.
        /// </summary>
        /// <param name="hwnd">The handle of the combo box for which the information
        /// is required.</param>
        /// <param name="info">The outputted info from the call</param>
        /// <returns></returns>
        [DllImport("user32")]
        private static extern int GetComboBoxInfo(IntPtr hwnd, out COMBOBOXINFO info);

        /// <summary>
        /// Simplistic RECT structure used throughout windows
        /// See: https://msdn.microsoft.com/en-us/library/windows/desktop/dd162897%28v=vs.85%29.aspx"
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        struct RECT { public int left, top, right, bottom; }

        /// <summary>
        /// Structure which is used to contain the combo box information
        /// See: https://msdn.microsoft.com/en-us/library/windows/desktop/bb775798%28v=vs.85%29.aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        struct COMBOBOXINFO
        {
            public int cbSize;
            public RECT rcItem;
            public RECT rcButton;
            public int stateButton;
            public IntPtr hwndCombo;
            public IntPtr hwndItem;
            public IntPtr hwndList;
        }

        /// <summary>
        /// A thin wrapper around a native window which maps onto the listbox
        /// dropped down by the combo box when it is clicked.
        /// </summary>
        class NativeComboListBox : NativeWindow
        {
            /// <summary>
            /// Event fired when the dropped down list is clicked. Can be cancelled
            /// by setting the <see cref="CancelEventArgs.Cancel"/> property
            /// </summary>
            public DroppedDownListClickCancelEventHandler DroppedDownListClick;

            // The owner of this native combo listbox
            private StyledComboBox _owner;

            /// <summary>
            /// Creates a new native combo listbox, owned by the given colouring
            /// combo box.
            /// </summary>
            /// <param name="owner">The owner of this native combo listbox.</param>
            public NativeComboListBox(StyledComboBox owner)
            {
                _owner = owner;
            }

            /// <summary>
            /// The item bounds built up when the combo box items were drawn.
            /// If the owner is not set, or the bounds have not been built up, this
            /// will return an empty dictionary.
            /// </summary>
            private IDictionary<int, Rectangle> ItemBounds
            {
                get
                {
                    if (_owner == null || _owner._itemBounds == null)
                        return GetEmpty.IDictionary<int, Rectangle>();
                    return _owner._itemBounds;
                }
            }

            /// <summary>
            /// Handles the given windows message on behalf of the combo box listbox
            /// that this class is representing
            /// </summary>
            /// <param name="m">The windows message to be processed</param>
            protected override void WndProc(ref Message m)
            {
                if (!HandleMsg(m))
                    base.WndProc(ref m);
            }

            /// <summary>
            /// Attempts to handle the given windows message, returning true if the
            /// message has been handled on exit of this method (ie. if the message
            /// does not need to be passed further up the inheritance chain).
            /// </summary>
            /// <param name="m">The message to handle</param>
            /// <returns>true if the message should be considered handled after this
            /// method exits; false if the message should be passed further up for
            /// processing.</returns>
            private bool HandleMsg(Message m)
            {
                if (m.Msg != 0x201) return false; //WM_LBUTTONDOWN = 0x201
                int y = m.LParam.ToInt32() >> 16;
                if (y < 0) return false; // < 0 = clicked on current combo selection

                int x = m.LParam.ToInt32() & 0x00ff;
                Point p = new Point(x, y);

                foreach (KeyValuePair<int, Rectangle> pair in ItemBounds)
                {
                    if (pair.Value.Contains(p))
                    {
                        // See if this click should be cancelled.
                        var e = new DroppedDownListClickCancelEventArgs(pair.Key);
                        OnDroppedDownListClick(e);
                        return e.Cancel;
                    }
                }

                // Didn't find it in the item bounds; pass it up the chain for
                // further processing
                return false;
            }

            /// <summary>
            /// Raises the <see cref="DroppedDownListClick"/> event
            /// </summary>
            /// <param name="e">The args detailing the event</param>
            protected void OnDroppedDownListClick(DroppedDownListClickCancelEventArgs e)
            {
                DroppedDownListClickCancelEventHandler h = this.DroppedDownListClick;
                if (h != null)
                    h(this, e);
            }
        }

        #endregion

        #region - Member variables -

        // The text saved so that if the user clicks on a disabled item,
        // the text can be returned to what it was before they did so.
        private string _savedText;

        // A cache of brushes to colours.
        private GDICache _cache;

        // The current state of this combo box
        private ComboBoxState _state;

        // The window handle of the listbox dropped down by this combo box
        private IntPtr _listHwnd;

        // The native combo list box for this combo box, created when the drop down
        // is activated for the first time
        private NativeComboListBox _native;

        // The bounds of each item in this combo box - updated each time the items
        // are drawn in the DrawItem method. Used by the native list box to determine
        // which item has been clicked
        private IDictionary<int, Rectangle> _itemBounds = new Dictionary<int, Rectangle>();

        // Stores if this combo can select multiple items using check boxes drawn on each entry.        
        public bool Checkable { get; set; }

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new empty combo box which allows for disabled items.
        /// </summary>
        public StyledComboBox()
        {
            this.DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
            DisabledItemColour = Color.Gray;
            DropDownBackColor = SystemColors.Window;
            State = ComboBoxState.Normal;
            IntegralHeight = false;
            MaxDropDownItems = 16;
            _cache = new GDICache();
            // The display inside the visual designer is b0rked if these are
            // set for controls created by the designer. They need to be there for
            // runtime, but they can be skipped for the designer view
            if (!UIUtil.IsInVisualStudio)
            {
                SetStyle(
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint, true);
            }
        }

        #endregion

        #region - Auto Properties -

        /// <summary>
        /// The colour to use for the labels of disabled combo box items.
        /// </summary>
        [Browsable(true), Category("Appearance"),
         DefaultValue(typeof(Color), "Gray"),
         Description("The forecolor to use for disabled items")]
        public Color DisabledItemColour { get; set; }

        /// <summary>
        /// The background colour to use for the drop down list representing
        /// this combo box when it is displayed
        /// </summary>
        [Browsable(true), Category("Appearance"),
         DefaultValue(typeof(Color), "Window"),
         Description("The backcolor to use for the drop down list")]
        public Color DropDownBackColor { get; set; }

        #endregion

        #region - Hidden Properties -

        /// <summary>
        /// Overload of <see cref="ComboBox.MaxDropDownItems"/> just to hang a new
        /// default (16) off it .
        /// </summary>
        [Browsable(true), Category("Behavior"), DefaultValue(16),
         Description("The maximum number of entries to display in the drop down list")]
        public new int MaxDropDownItems
        {
            get { return base.MaxDropDownItems; }
            set { base.MaxDropDownItems = value; }
        }


        /// <summary>
        /// Shadow of the DropDownStyle property of ComboBox which hides the property
        /// from the visual designer.
        /// This combo box doesn't handle the textbox drawing at all well, so it can
        /// only really be used as a drop down list.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ComboBoxStyle DropDownStyle
        {
            get { return base.DropDownStyle; }
            set { base.DropDownStyle = value; }
        }

        /// <summary>
        /// Overload of <see cref="ComboBox.IntegralHeight"/> to give it a new default.
        /// Note that this must be false for <see cref="MaxDropDownItems"/> to work
        /// correctly (see http://tinyurl.com/p7ss7yh for more details)
        /// </summary>
        [Browsable(true), Category("Behavior"), DefaultValue(false), Description(
         "Indicates whether the combo box should resize to avoid showing partial items")]
        public new bool IntegralHeight
        {
            get { return base.IntegralHeight; }
            set { base.IntegralHeight = value; }
        }

        /// <summary>
        /// The draw mode for this colouring combo box. This must be
        /// <see cref="DrawMode.OwnerDrawFixed"/> in order to work as expected. This
        /// shadow just hides it in the designer and editor.
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new DrawMode DrawMode
        {
            get { return base.DrawMode; }
            set { base.DrawMode = value; }
        }

        /// <summary>
        /// Gets or sets the selected combo box item. Note that if the currently
        /// selected item is not a ComboBoxItem, this will return null.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ComboBoxItem SelectedComboBoxItem
        {
            get { return (SelectedItem as ComboBoxItem); }
            set { SelectedItem = value; }
        }

        /// <summary>
        /// Gets the push button state for this combo box, given its current
        /// <see cref="ComboBoxState"/>
        /// </summary>
        /// <remarks>This is used to render the 'button' for the combo box</remarks>
        private PushButtonState ButtonState
        {
            get
            {
                switch (State)
                {
                    case ComboBoxState.Disabled: return PushButtonState.Disabled;
                    case ComboBoxState.Hot: return PushButtonState.Hot;
                    case ComboBoxState.Pressed: return PushButtonState.Pressed;
                    default: return PushButtonState.Normal;
                }
            }
        }

        /// <summary>
        /// Gets or sets the combo box state for this control.
        /// If the state changes as a result of this call, the control is invalidated
        /// </summary>
        /// <remarks>If the combo box is disabled, this will always return a state of
        /// <see cref="ComboBoxState.Disabled"/>, regardless of the current setting
        /// </remarks>
        private ComboBoxState State
        {
            get { return (Enabled ? _state : ComboBoxState.Disabled); }
            set
            {
                if (_state == value) return;
                _state = value;
                Invalidate();
            }
        }
        #endregion

        #region - Internal Helper Methods -

        /// <summary>
        /// Checks if the item at the given index is selectable. An item is
        /// selectable if it is <em>not</em> an instance of
        /// <see cref="ComboBoxItem"/> or if its <see cref="ComboBoxItem.Selectable"/>
        /// property returns <c>true</c>.
        /// </summary>
        /// <param name="index">The index for which the selectable state of
        /// the item is required.</param>
        /// <returns>false if the item at the given index is not selectable
        /// true if it is selectable, or it has no implicit selectable state.
        /// </returns>
        private bool IsSelectable(int index)
        {
            ComboBoxItem item = (index < 0 ? null : Items[index] as ComboBoxItem);
            return (item == null || item.Selectable);
        }

        /// <summary>
        /// Draws an arrow within the given clip rectangle
        /// </summary>
        /// <param name="g">The graphics context to draw the arrow</param>
        /// <param name="r">The clipping rectangle used to defined the combo box,
        /// and therefore define where the arrow should be.</param>
        private void DrawArrow(Graphics g, Rectangle r)
        {
            g.FillPolygon(SystemBrushes.ControlText, new Point[] {
                new Point(r.X + 3, r.Y + r.Height / 2 - 2),
                new Point(r.X + r.Width - 3, r.Y + r.Height / 2 - 2),
                new Point(r.X + r.Width / 2, r.Y + r.Height / 2 + 2)
            });
        }

        /// <summary>
        /// Gets the arrow rectangle from the given clip rectangle.
        /// </summary>
        /// <param name="clip">The clip rectangle for which a rectangle bounding the
        /// drop down arrow is required</param>
        /// <returns>A rectangle that can be used to bound a drop down arrow.
        /// </returns>
        private Rectangle GetArrowRect(Rectangle clip)
        {
            clip.X = clip.Width - 15;
            clip.Width = 13;
            return clip;
        }

        /// <summary>
        /// Gets the button rectangle from the given clip rectangle.
        /// </summary>
        /// <param name="clip">The clip rectangle for which a rectangle bounding the
        /// drop down button is required</param>
        /// <returns>A rectangle that can be used to bound a drop down button.
        /// </returns>
        private Rectangle GetButtonRect(Rectangle clip)
        {
            clip.X = clip.Width - 19;
            clip.Width = 17;
            clip.Y = clip.Y + 2;
            clip.Height = 17;
            return clip;
        }

        /// <summary>
        /// Gets the item text for the given item.
        /// </summary>
        /// <param name="item">The item for which the text is required</param>
        /// <returns>The text to display for the given item</returns>
        protected new string GetItemText(object item)
        {
            var cbitem = item as ComboBoxItem;
            return (cbitem != null ? cbitem.Text : base.GetItemText(item));
        }

        #endregion

        #region - Internal Event Handling -

        /// <summary>
        /// Handles the combo box resizing, ensuring that it is repainted when the
        /// bounds of the control change.
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        /// <summary>
        /// Handles the combo box's handle being created.
        /// This ensures that the HWND for the combo box drop down list can be
        /// retrieved for the <see cref="NativeComboBoxList"/> instance which is
        /// created when the combo is first dropped down
        /// </summary>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            COMBOBOXINFO combo = new COMBOBOXINFO();
            combo.cbSize = Marshal.SizeOf(combo);
            GetComboBoxInfo(this.Handle, out combo);
            _listHwnd = combo.hwndList;
        }

        /// <summary>
        /// Handles the combo box's handle being destroyed, ensuring that the
        /// native combo list's handle is released at the same time.
        /// </summary>
        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (_native != null)
            {
                _native.ReleaseHandle();
                _native = null;
            }
            base.OnHandleDestroyed(e);
        }

        /// <summary>
        /// Handles the combo box being entered.
        /// Saves the initial text so that we can return to it if a disabled
        /// item is selected.
        /// </summary>
        protected override void OnEnter(EventArgs e)
        {
            // save the currently selected index, so that if a disabled item
            // is selected, we can return to that index instead.
            _savedText = Text;
            base.OnEnter(e);
        }

        /// <summary>
        /// Handles the selection change being committed.
        /// </summary>
        protected override void OnSelectionChangeCommitted(EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)(() => OnSelectionChangeCommitted(e)));
                return;
            }

            if (IsSelectable(SelectedIndex))
            {
                _savedText = Text;
                if (Checkable && base.SelectedIndex != -1)
                    ((ComboBoxItem)Items[base.SelectedIndex]).Checked ^= true;
            }
            else
            {
                // The NativeComboListBox should be dealing with this case now...
                // But there are cases where it doesn't seem to, so revert to the
                // old functionality where that occurs

                // Reset the text to its previous value.
                this.Text = _savedText;

                
            }
            base.OnSelectionChangeCommitted(e);
        }

        /// <summary>
        /// Handles the selected index being changed.
        /// Ensures that the event is squashed if the item is disabled.
        /// </summary>
        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            int index = SelectedIndex;
            if (index < 0 || IsSelectable(index))
                base.OnSelectedIndexChanged(e);
        }

        /// <summary>
        /// Handles the selected item being changed.
        /// Ensures that the event is squashed if the item is disabled.
        /// </summary>
        protected override void OnSelectedItemChanged(EventArgs e)
        {
            if (IsSelectable(SelectedIndex))
                base.OnSelectedItemChanged(e);
        }

        /// <summary>
        /// Handles the text changing - this just ensures that the text is
        /// saved in case of selecting disabled combo box items.
        /// </summary>
        protected override void OnTextChanged(EventArgs e)
        {
            _savedText = Text;
            base.OnTextChanged(e);
        }

        /// <summary>
        /// Draws the specified combo box item.
        /// </summary>
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            base.OnDrawItem(e);
            if (e.Index >= 0)
                _itemBounds[e.Index] = e.Bounds;

            e.DrawBackground();

            if (e.Index >= 0 && e.Index < Items.Count)
            {
                object item = Items[e.Index];
                // enabled should default to the state of this combo box.
                bool checkable = false;
                bool enabled = this.Enabled;
                string label = GetItemText(item);
                Color textColor = this.ForeColor;
                FontStyle style = FontStyle.Regular;
                int indent = 0;
                bool check = false;
                Point p = e.Bounds.Location;

                if (item is ComboBoxItem)
                {
                    ComboBoxItem cbItem = (ComboBoxItem)item;
                    // enabled item is trumped by disabled combo box
                    enabled &= cbItem.Enabled;
                    textColor = cbItem.Color;
                    style = cbItem.Style;
                    indent = cbItem.Indent;
                    check = cbItem.Checked;
                    checkable = cbItem.Checkable;
                }

                // enabled overrides the text colour
                if (!enabled)
                    textColor = DisabledItemColour;
                // and selected overrides everything except enabled.
                else if (e.State.HasFlag(DrawItemState.Selected))
                    textColor = SystemColors.HighlightText;

                Graphics g = e.Graphics;
                if (e.State.HasFlag(DrawItemState.Selected))
                {
                    g.FillRectangle(SystemBrushes.Highlight, e.Bounds);
                }
                else // else draw the default background
                {
                    g.FillRectangle(_cache.GetBrush(DropDownBackColor), e.Bounds);
                }


                if (Checkable && e.Index == base.SelectedIndex && (e.State & DrawItemState.ComboBoxEdit) == DrawItemState.ComboBoxEdit)
                {
                    label = GetCaptionText();
                }
                else if (Checkable && checkable)
                {
                    p.Offset(indent, 0);
                    p.Offset(1, 1);
                    CheckBoxState c = CheckBoxState.UncheckedNormal;
                    if (check)
                        c = CheckBoxState.CheckedNormal;

                    CheckBoxRenderer.DrawCheckBox(e.Graphics, p, c);
                    p.Offset(14, -2);
                }
                else p.Offset(indent, 0);

                Font f = _cache.GetFont(this.Font, style);
                g.DrawString(label, f, _cache.GetBrush(textColor), p);
            }
        }

        /// <summary>
        /// Handles the painting of this combo box - not the item drawing, but the
        /// painting of the control itself.
        /// </summary>
        /// <param name="e">The args detailing the paint event</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Use Bounds offset to the origin rather than e.ClipRectangle which
            // suffers from confusion when maximising / restoring.
            Rectangle clip = new Rectangle(Point.Empty, Bounds.Size);
            Graphics g = e.Graphics;
            if (Application.RenderWithVisualStyles)
            {
                if (this.DropDownStyle == ComboBoxStyle.DropDownList)
                {
                    Rectangle btnClip = clip;
                    btnClip.Offset(1, -1);
                    ButtonRenderer.DrawButton(g, btnClip, this.Focused, ButtonState);

                    // The ComboBoxRenderer draws a box around the arrow, rather
                    // awkwardly, so we must roll our own here...
                    Rectangle arrowClip = GetArrowRect(clip);
                    DrawArrow(g, arrowClip);
                }
                else
                {
                    ComboBoxRenderer.DrawTextBox(g, clip, State);
                    ComboBoxRenderer.DrawDropDownButton(g, GetArrowRect(clip), State);
                }
            }
            else
            {
                if (this.DropDownStyle == ComboBoxStyle.DropDownList)
                {
                    Rectangle borderClip = clip;
                    borderClip.Offset(1, -1);
                    borderClip.Inflate(-2, -1);
                    ControlPaint.DrawBorder3D(g, borderClip, Border3DStyle.Sunken);

                    Rectangle btnClip = GetButtonRect(clip);
                    ControlPaint.DrawButton(g, btnClip, System.Windows.Forms.ButtonState.Normal);

                    Rectangle arrowClip = GetArrowRect(clip);
                    arrowClip.Offset(-2, 0);
                    DrawArrow(g, arrowClip);
                }
                else
                {
                    ControlPaint.DrawBorder3D(g, clip);
                    ControlPaint.DrawButton(g, GetArrowRect(clip), System.Windows.Forms.ButtonState.Normal);
                }
            }
            Rectangle rect = clip;
            rect.Inflate(-2, -4);
            rect.Offset(2, -1);
            // That's just touching up - we also want to make sure that we don't
            // overwrite the arrow
            rect.Width -= GetArrowRect(clip).Width;
            if (SelectedIndex > -1 && Items.Count > 0)
            {
                string itemText = "";
                if (Checkable)
                    itemText = GetCaptionText();
                else
                    itemText = GetItemText(Items[SelectedIndex]);

                g.DrawString(
                    itemText,
                    Font,
                    _cache.GetBrush(ForeColor),
                    rect);
            }
        }

        /// <summary>
        /// Return the desired caption for the combo box
        /// Overridable, standard answer is current caption.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetCaptionText()
        {
            return Text;
        }

        /// <summary>
        /// Handles the Enabled property being changed for this combo box, ensuring
        /// that it is invalidated
        /// </summary>
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        /// <summary>
        /// Handles the combo box being dropped down, ensuring that the
        /// <see cref="State"/> is updated accordingly and the control is invalidated
        /// </summary>
        protected override void OnDropDown(EventArgs e)
        {
            base.OnDropDown(e);
            State = ComboBoxState.Pressed;
            if (_native == null)
            {
                _native = new NativeComboListBox(this);
                _native.AssignHandle(_listHwnd);
                _native.DroppedDownListClick += HandleDropDownListClick;
            }
        }

        /// <summary>
        /// Handles when a key is pressed on the combo box
        /// or not.
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Down || e.KeyCode == Keys.Up) &&
                State != ComboBoxState.Pressed && State != ComboBoxState.Disabled)
            {
                DroppedDown = true;
            }
        }

        /// <summary>
        /// Handles a click on the drop down list, checking if the item is enabled
        /// or not.
        /// </summary>
        private void HandleDropDownListClick(object sender, DroppedDownListClickCancelEventArgs e)
        {
            if (!IsSelectable(e.Index))
                e.Cancel = true;
        }

        /// <summary>
        /// Handles the combo box's drop down being closed, ensuring that the
        /// <see cref="State"/> is updated accordingly and the control is invalidated
        /// </summary>
        protected override void OnDropDownClosed(EventArgs e)
        {
            base.OnDropDownClosed(e);
            State = ComboBoxState.Normal;
        }

        /// <summary>
        /// Handles the mouse entering the combo box, ensuring that the
        /// <see cref="State"/> is updated accordingly and the control is invalidated
        /// </summary>
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (State == ComboBoxState.Normal)
                State = ComboBoxState.Hot;
        }

        /// <summary>
        /// Handles the mouse hovering over the combo box, ensuring that the
        /// <see cref="State"/> is updated accordingly and the control is invalidated
        /// </summary>
        protected override void OnMouseHover(EventArgs e)
        {
            base.OnMouseHover(e);
            if (State == ComboBoxState.Normal)
                State = ComboBoxState.Hot;
        }

        /// <summary>
        /// Handles the mouse leaving the combo box, ensuring that the
        /// <see cref="State"/> is updated accordingly and the control is invalidated
        /// </summary>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (State == ComboBoxState.Hot)
                State = ComboBoxState.Normal;
        }


        #endregion

        #region - Other Methods -

        /// <summary>
        /// Gets or sets the selected index on this combo box.
        /// </summary>
        public override int SelectedIndex
        {
            get { return base.SelectedIndex; }
            set
            {
                if (value != base.SelectedIndex)
                {
                    base.SelectedIndex = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets the combo box item held in this combo box with the given tag.
        /// </summary>
        /// <param name="tag">The value held on the tag of the combo box item.
        /// </param>
        /// <returns>The combo box item representing the given tag contained
        /// in this combobox, or null if no such item was found.</returns>
        public virtual ComboBoxItem FindComboBoxItemByTag(object tag)
        {
            foreach (object obj in Items)
            {
                ComboBoxItem item = obj as ComboBoxItem;
                if (item != null && object.Equals(item.Tag, tag))
                    return item;
            }
            return null;
        }

        public void SelectComboBoxByTag(object tag)
        {
            ComboBoxItem cmb = FindComboBoxItemByTag(tag);
            this.SelectedComboBoxItem = cmb;
            this.Invalidate();
        }

        public void SelectFirst()
        {
            if (Items.Count > 0)
                this.SelectedIndex = 0;

            this.Invalidate();
        }

        /// <summary>
        /// Finds the combo box item held in this combo box with the given text
        /// </summary>
        /// <param name="text">The text to search for</param>
        /// <returns>The first combo box item found which has the given text; or
        /// null if no such combo box item could be found.</returns>
        public virtual ComboBoxItem FindComboBoxItemByText(string text)
        {
            foreach (object obj in Items)
            {
                ComboBoxItem it = obj as ComboBoxItem;
                if (it != null && string.Equals(it.Text, text, StringComparison.OrdinalIgnoreCase))
                    return it;
            }
            return null;
        }

        /// <summary>
        /// Disposes of this combo box.
        /// </summary>
        /// <param name="disposing">True if disposing explicitly</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _cache.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Sets all item's selected status to false.
        /// </summary>
        public void ClearCheckedItems()
        {
            Items.Cast<ComboBoxItem>().ToList().ForEach(x => x.Checked = false);
            base.Invalidate();
        }

        /// <summary>
        /// Apply the status of selected to items in the lsit 
        /// </summary>
        /// <param name="name"></param>
        public void CheckItem(string name)
        {
            Items.Cast<ComboBoxItem>().Where(x => x.Text == name).ToList().ForEach(x => x.Checked = true);
            base.Invalidate();
        }

        /// <summary>
        /// Set all selectable items to be the given value
        /// </summary>
        /// <param name="value"></param>
        public void SetAllCheckableItems(bool value)
        {
            Items.Cast<ComboBoxItem>().Where(x => x.Checkable).ToList().ForEach(x => x.Checked = value);
            base.Invalidate();
        }

        #endregion
    }
}
