// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2004-2006 Novell, Inc.
//
// Authors:
//  Jordi Mas i Hernandez, jordi@ximian.com
//  Mike Kestner  <mkestner@novell.com>
//
// NOT COMPLETE

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using AutomateControls.UIStructs;
using BluePrism.Server.Domain.Models;

namespace AutomateControls.ComboBoxes
{

    [DefaultProperty("Items")]
    [DefaultEvent("SelectedIndexChanged")]
    //[Designer ("MonoSWF.Design.ComboBoxDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
    public class MonoComboBox : System.Windows.Forms.ListControl
    {
        private System.Windows.Forms.DrawMode draw_mode = System.Windows.Forms.DrawMode.Normal;
        private ComboBoxStyle dropdown_style = (ComboBoxStyle)(-1);
        private int dropdown_width = -1;
        private int selected_index = -1;
        internal ComboBoxItemCollection items = null;
        private bool suspend_ctrlupdate;
        private int maxdrop_items = 8;
        private bool integral_height = true;
        private bool sorted;
        private int max_length;
        internal ComboListBox listbox_ctrl;
        private System.Windows.Forms.TextBox textbox_ctrl;
        private bool process_textchanged_event = true;
        private bool item_height_specified = false;
        private int item_height;
        private int requested_height = -1;
        private Hashtable item_heights;
        private bool show_dropdown_button = false;
        private System.Windows.Forms.ButtonState button_state = System.Windows.Forms.ButtonState.Normal;
        private bool dropped_down;
        private Rectangle text_area;
        private Rectangle button_area;
        private Rectangle listbox_area;
        private const int button_width = 18;

        private Graphics dc_mem;            // Graphics context for double buffering
        private System.Drawing.Drawing2D.GraphicsState dc_state;        // Pristine graphics context to reset after paint code alters dc_mem
        private Bitmap bmp_mem;     // Bitmap for double buffering control


        [ComVisible(true)]
        public class ChildAccessibleObject : System.Windows.Forms.AccessibleObject
        {
            private MonoComboBox owner;
            private IntPtr handle;

            public ChildAccessibleObject(MonoComboBox owner, IntPtr handle)
            {
                this.owner = owner;
                this.handle = handle;
            }

            public override string Name
            {
                get
                {
                    return base.Name;
                }
            }
        }


        public MonoComboBox()
        {
            items = new ComboBoxItemCollection(this);
            DropDownStyle = ComboBoxStyle.DropDown;
            item_height = FontHeight + 2;
            BackColor = SystemColors.ControlLightLight;
            //border_style = BorderStyle.None;

            /* Events */
            MouseDown += new System.Windows.Forms.MouseEventHandler(OnMouseDownCB);
            MouseUp += new System.Windows.Forms.MouseEventHandler(OnMouseUpCB);
            MouseMove += new System.Windows.Forms.MouseEventHandler(OnMouseMoveCB);
            KeyDown += new System.Windows.Forms.KeyEventHandler(OnKeyDownCB);
        }

        #region "Graphics"

        internal void CreateBuffers(int width, int height)
        {
            if (dc_mem != null)
            {
                dc_mem.Dispose();
            }
            if (bmp_mem != null)
                bmp_mem.Dispose();

            if (width < 1)
            {
                width = 1;
            }

            if (height < 1)
            {
                height = 1;
            }

            bmp_mem = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            dc_mem = Graphics.FromImage(bmp_mem);
        }

        internal Graphics DeviceContext
        {
            get
            {
                if (dc_mem == null)
                {
                    CreateBuffers(this.Width, this.Height);
                    dc_state = dc_mem.Save();
                }
                return dc_mem;
            }
        }
        #endregion

        internal void BindDataItems(IList items)
        {
            items.Clear();

            if (this.DataManager != null)
            {
                SetItemsCore(DataManager.List);
            }
        }

        #region events

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new event EventHandler BackgroundImageChanged
        {
            add { base.BackgroundImageChanged += value; }
            remove { base.BackgroundImageChanged -= value; }
        }

        public event System.Windows.Forms.DrawItemEventHandler DrawItem;
        public event EventHandler DropDown;
        public event EventHandler DropDownStyleChanged;
        public event System.Windows.Forms.MeasureItemEventHandler MeasureItem;

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new event System.Windows.Forms.PaintEventHandler Paint
        {
            add { base.Paint += value; }
            remove { base.Paint -= value; }
        }

        public event EventHandler SelectedIndexChanged;
        public event EventHandler SelectionChangeCommitted;
        #endregion Events

        #region Public Properties
        public override Color BackColor
        {
            get { return base.BackColor; }
            set
            {
                if (base.BackColor == value)
                    return;

                base.BackColor = value;             
                Refresh();
            }
        }

        private Color m_DropDownBackColor = SystemColors.ControlLightLight;
        /// <summary>
        /// The back colour of the listbox that
        /// pops up when the combo box is expanded.
        /// </summary>
        public Color DropDownBackColor
        {
            get
            { 
                return this.m_DropDownBackColor; 
            }
            set
            {
                this.m_DropDownBackColor = value;
            }
        }

        private Color m_DropDownForeColor = Color.Black;
        /// <summary>
        /// The fore colour of the listbox that
        /// pops up when the combo box is expanded.
        /// </summary>
        public Color DropDownForeColor
        {
            get
            {
                return this.m_DropDownForeColor;
            }
            set
            {
                this.m_DropDownForeColor = value;
            }
        }
        /// <summary>
        /// Private member to store public property DisabledItemColour
        /// </summary>
        private Color mDisabledItemColour = Color.LightGray;
        /// <summary>
        /// The colour used to render disabled items.
        /// Defaults to grey. This must not be
        /// set to the same colour as the property ForeColor orelse
        /// an exception will be thrown.
        /// 
        /// If any items in this combo box have their FontColour
        /// set to the disabled colour then they will be rendered
        /// using the ForeColour.
        /// </summary>
        public Color DisabledItemColour
        {
            get
            {
                return this.mDisabledItemColour;
            }
            set
            {
                if (value.Equals(this.ForeColor))
                    throw new InvalidOperationException("Disabled colour cannot be the same as the fore colour.");

                this.mDisabledItemColour = value;
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Image BackgroundImage
        {
            get { return base.BackgroundImage; }
            set
            {
                if (base.BackgroundImage == value)
                    return;

                base.BackgroundImage = value;
                Refresh();
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            get { return base.CreateParams; }
        }

        protected override Size DefaultSize
        {
            get { return new Size(121, 21); }
        }

        [RefreshProperties(RefreshProperties.Repaint)]
        [DefaultValue(System.Windows.Forms.DrawMode.Normal)]
        public System.Windows.Forms.DrawMode DrawMode
        {
            get { return draw_mode; }

            set
            {
                if (!Enum.IsDefined(typeof(System.Windows.Forms.DrawMode), value))
                    throw new InvalidEnumArgumentException(string.Format("Enum argument value '{0}' is not valid for DrawMode", value));

                if (draw_mode == value)
                    return;

                if (draw_mode == System.Windows.Forms.DrawMode.OwnerDrawVariable)
                    item_heights = null;
                draw_mode = value;
                if (draw_mode == System.Windows.Forms.DrawMode.OwnerDrawVariable)
                    item_heights = new Hashtable();
                Refresh();
            }
        }

        [DefaultValue(ComboBoxStyle.DropDown)]
        [RefreshProperties(RefreshProperties.Repaint)]
        public ComboBoxStyle DropDownStyle
        {
            get { return dropdown_style; }

            set
            {

                if (!Enum.IsDefined(typeof(ComboBoxStyle), value))
                    throw new InvalidEnumArgumentException(string.Format("Enum argument value '{0}' is not valid for ComboBoxStyle", value));

                if (dropdown_style == value)
                    return;

                if (dropdown_style == ComboBoxStyle.Simple)
                {
                    if (listbox_ctrl != null)
                    {
                        Controls.Remove(listbox_ctrl);
                        listbox_ctrl.Dispose();
                        listbox_ctrl = null;
                    }
                }

                dropdown_style = value;

                if (dropdown_style == ComboBoxStyle.DropDownList && textbox_ctrl != null)
                {
                    Controls.Remove(textbox_ctrl);
                    textbox_ctrl.Dispose();
                    textbox_ctrl = null;
                }

                if (dropdown_style == ComboBoxStyle.Simple)
                {
                    show_dropdown_button = false;

                    CreateComboListBox();

                    if (IsHandleCreated)
                        Controls.Add(listbox_ctrl);
                }
                else
                {
                    show_dropdown_button = true;
                    button_state = System.Windows.Forms.ButtonState.Normal;
                }

                if (dropdown_style != ComboBoxStyle.DropDownList && textbox_ctrl == null)
                {
                    textbox_ctrl = new System.Windows.Forms.TextBox();
                    if (selected_index != -1)
                        textbox_ctrl.Text = Items[selected_index].Text;
                    textbox_ctrl.BorderStyle = System.Windows.Forms.BorderStyle.None;
                    textbox_ctrl.TextChanged += new EventHandler(OnTextChangedEdit);
                    textbox_ctrl.KeyPress += new System.Windows.Forms.KeyPressEventHandler(textbox_ctrl_KeyPress);
                    textbox_ctrl.KeyDown += new System.Windows.Forms.KeyEventHandler(OnKeyDownCB);
                    textbox_ctrl.GotFocus += new EventHandler(textbox_ctrl_GotFocus);
                    textbox_ctrl.LostFocus += new EventHandler(textbox_ctrl_LostFocus);
                    textbox_ctrl.MouseDown += new System.Windows.Forms.MouseEventHandler(textbox_ctrl_MouseDown);
                    textbox_ctrl.MouseMove += new System.Windows.Forms.MouseEventHandler(textbox_ctrl_MouseMove);
                    textbox_ctrl.MouseUp += new System.Windows.Forms.MouseEventHandler(textbox_ctrl_MouseUp);

                    if (IsHandleCreated == true)
                    {
                        Controls.Add(textbox_ctrl);
                    }
                }

                OnDropDownStyleChanged(EventArgs.Empty);

                DoLayout();
                if (IsHandleCreated)
                    UpdateBounds();
                Refresh();
            }
        }

        public int DropDownWidth
        {
            get
            {
                if (dropdown_width == -1)
                    return Width;

                return dropdown_width;
            }
            set
            {
                if (dropdown_width == value)
                    return;

                if (value < 1)
                    throw new ArgumentException("The DropDownWidth value is less than one");

                dropdown_width = value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Bug", "S4275:Getters and setters should access the expected fields", Justification = "DropDownListBox() sets the value of dropped_down")]
        public bool DroppedDown
        {
            get
            {
                if (dropdown_style == ComboBoxStyle.Simple)
                    return true;

                return dropped_down;
            }
            set
            {
                if (dropdown_style == ComboBoxStyle.Simple || dropped_down == value)
                    return;

                if (value)
                    DropDownListBox();
                else
                    listbox_ctrl.Hide();
            }
        }

        public override bool Focused
        {
            get { return base.Focused; }
        }

        public override Color ForeColor
        {
            get { return base.ForeColor; }
            set
            {
                if (base.ForeColor == value)
                    return;

                base.ForeColor = value;
                Refresh();
            }
        }

        [DefaultValue(true)]
        [Localizable(true)]
        public bool IntegralHeight
        {
            get { return integral_height; }
            set
            {
                if (integral_height == value)
                    return;

                integral_height = value;
                UpdateBounds();
                Refresh();
            }
        }

        [Localizable(true)]
        public int ItemHeight
        {
            get
            {
                if (item_height == -1)
                {
                    SizeF sz = DeviceContext.MeasureString("The quick brown Fox", Font);
                    item_height = (int)sz.Height;
                }
                return item_height;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException("The item height value is less than zero");

                item_height_specified = true;
                item_height = value;
                if (IntegralHeight)
                    UpdateBounds();
                DoLayout();
                Refresh();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Localizable(true)]
        //[Editor ("MonoSWF.Design.ListControlStringCollectionEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]       
        public ComboBoxItemCollection Items
        {
            get { return items; }
        }

        [DefaultValue(8)]
        [Localizable(true)]
        public int MaxDropDownItems
        {
            get { return maxdrop_items; }
            set
            {
                if (maxdrop_items == value)
                    return;

                maxdrop_items = value;
            }
        }

        [DefaultValue(0)]
        [Localizable(true)]
        public int MaxLength
        {
            get { return max_length; }
            set
            {
                if (max_length == value)
                    return;

                max_length = value;

                if (dropdown_style != ComboBoxStyle.DropDownList)
                {

                    if (value < 0)
                    {
                        value = 0;
                    }

                    textbox_ctrl.MaxLength = value;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public int PreferredHeight
        {
            get
            {
                return ItemHeight + 5;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override int SelectedIndex
        {
            get { return selected_index; }
            set
            {
                if (value <= -2 || value >= Items.Count)
                    throw new ArgumentOutOfRangeException("Index of out range");

                if (selected_index == value)
                    return;

                selected_index = value;

                if (dropdown_style != ComboBoxStyle.DropDownList)
                {
                    if (selected_index == -1)
                        SetControlText("");
                    else
                    {
                        SetControlText(Items[selected_index].Text);
                        SelectAll();
                    }
                }

                if (listbox_ctrl != null)
                    listbox_ctrl.HighlightedIndex = value;

                OnSelectedValueChanged(new EventArgs());
                OnSelectedIndexChanged(new EventArgs());
                OnSelectedItemChanged(new EventArgs());
                if (DropDownStyle == ComboBoxStyle.DropDownList)
                    Refresh();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Bindable(true)]
        public MonoComboBoxItem SelectedItem
        {
            get
            {
                if (selected_index != -1 && Items != null && Items.Count > 0)
                    return Items[selected_index];
                else
                    return null;
            }
            set
            {
                int index = Items.IndexOf(value);

                if (index == -1)
                    return;

                if (selected_index == index)
                    return;

                SelectedIndex = index;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedText
        {
            get
            {
                if (dropdown_style == ComboBoxStyle.DropDownList)
                {
                    if (SelectedItem != null)
                        return SelectedItem.Text;
                    else
                        return "";
                }

                return textbox_ctrl.SelectedText;
            }
            set
            {
                if (dropdown_style == ComboBoxStyle.DropDownList)
                {
                    foreach (MonoComboBoxItem cbi in this.Items)
                    {
                        if (cbi.Text == value)
                        {
                            this.SelectedItem = cbi;
                            break;
                        }
                    }
                    return;
                }

                textbox_ctrl.SelectedText = value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectionLength
        {
            get
            {
                if (dropdown_style == ComboBoxStyle.DropDownList)
                    return 0;

                return textbox_ctrl.SelectionLength;
            }
            set
            {
                if (dropdown_style == ComboBoxStyle.DropDownList)
                    return;

                if (textbox_ctrl.SelectionLength == value)
                    return;

                textbox_ctrl.SelectionLength = value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectionStart
        {
            get
            {
                if (dropdown_style == ComboBoxStyle.DropDownList)
                    return 0;

                return textbox_ctrl.SelectionStart;
            }
            set
            {
                if (dropdown_style == ComboBoxStyle.DropDownList)
                    return;

                if (textbox_ctrl.SelectionStart == value)
                    return;

                textbox_ctrl.SelectionStart = value;
            }
        }

        [DefaultValue(false)]
        public bool Sorted
        {
            get { return sorted; }

            set
            {
                if (sorted == value)
                    return;

                sorted = value;
                this.Items.Sorted = value;
            }
        }

        [Bindable(true)]
        [Localizable(true)]
        public override string Text
        {
            get
            {
                if (dropdown_style != ComboBoxStyle.DropDownList)
                {
                    if (textbox_ctrl != null)
                    {
                        return textbox_ctrl.Text;
                    }
                }

                if (SelectedItem != null)
                    return SelectedItem.Text;

                return base.Text;
            }
            set
            {
                if (value == null)
                {
                    SelectedIndex = -1;
                    return;
                }

                int index = FindStringExact(value);
                // Includes possibility that index == -1. This is ok
                SelectedIndex = index;

                if (dropdown_style != ComboBoxStyle.DropDownList && SelectedItem != null)
                    textbox_ctrl.Text = SelectedItem.Text;
            }
        }

        #endregion Public Properties

        #region Public Methods
        protected virtual void AddItemsCore(object[] value)
        {

        }

        public void BeginUpdate()
        {
            suspend_ctrlupdate = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (listbox_ctrl != null)
                {
                    listbox_ctrl.Dispose();
                    Controls.Remove(listbox_ctrl);
                    listbox_ctrl = null;
                }

                if (textbox_ctrl != null)
                {
                    Controls.Remove(textbox_ctrl);
                    textbox_ctrl.Dispose();
                    textbox_ctrl = null;
                }

                if (dc_mem != null) dc_mem.Dispose();
                if (bmp_mem != null) bmp_mem.Dispose();
            }

            base.Dispose(disposing);
        }

        public void EndUpdate()
        {
            suspend_ctrlupdate = false;
            UpdatedItems();
            Refresh();
        }

        public int FindString(string s)
        {
            return FindString(s, -1);
        }

        public int FindString(string s, int startIndex)
        {
            if (Items.Count == 0)
                return -1; // No exception throwing if empty

            if (startIndex < -1 || startIndex >= Items.Count - 1)
                throw new ArgumentOutOfRangeException("Index of out range");

            startIndex++;
            for (int i = startIndex; i < Items.Count; i++)
            {
                if ((Items[i]).Text.StartsWith(s))
                    return i;
            }

            return -1;
        }

        public int FindStringExact(string s)
        {
            return FindStringExact(s, -1);
        }

        public int FindStringExact(string s, int startIndex)
        {
            if (Items.Count == 0)
                return -1; // No exception throwing if empty

            if (startIndex < -1 || startIndex >= Items.Count - 1)
                throw new ArgumentOutOfRangeException("Index of out range");

            startIndex++;
            for (int i = startIndex; i < Items.Count; i++)
            {
                if (Items[i].Text.Equals(s))
                    return i;
            }

            return -1;
        }

        public int GetItemHeight(int index)
        {
            if (DrawMode == System.Windows.Forms.DrawMode.OwnerDrawVariable && IsHandleCreated)
            {

                if (index < 0 || index >= Items.Count)
                    throw new ArgumentOutOfRangeException("The item height value is less than zero");

                object item = Items[index];
                if (item_heights.Contains(item))
                    return (int)item_heights[item];

                System.Windows.Forms.MeasureItemEventArgs args = new MeasureItemEventArgs(DeviceContext, index, ItemHeight);
                OnMeasureItem(args);
                item_heights[item] = args.ItemHeight;
                return args.ItemHeight;
            }

            return ItemHeight;
        }

        protected override bool IsInputKey(System.Windows.Forms.Keys keyData)
        {
            switch (keyData)
            {
                case System.Windows.Forms.Keys.Up:
                case System.Windows.Forms.Keys.Down:
                case System.Windows.Forms.Keys.PageUp:
                case System.Windows.Forms.Keys.PageDown:
                    return true;

                default:
                    return false;
            }
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
        }

        protected override void OnDataSourceChanged(EventArgs e)
        {
            base.OnDataSourceChanged(e);
            BindDataItems(items);

            if (DataSource == null || DataManager == null)
            {
                SelectedIndex = -1;
            }
            else
            {
                SelectedIndex = DataManager.Position;
            }
        }

        protected override void OnDisplayMemberChanged(EventArgs e)
        {
            base.OnDisplayMemberChanged(e);

            if (DataManager == null || !IsHandleCreated)
                return;

            BindDataItems(items);
            SelectedIndex = DataManager.Position;
        }

        protected virtual void OnDrawItem(System.Windows.Forms.DrawItemEventArgs e)
        {
            switch (DrawMode)
            {
                case System.Windows.Forms.DrawMode.OwnerDrawFixed:
                case System.Windows.Forms.DrawMode.OwnerDrawVariable:
                    if (DrawItem != null)
                        DrawItem(this, e);
                    break;
                default:
                    this.DrawComboBoxItem(this, e);
                    break;
            }
        }

        protected virtual void OnDropDown(EventArgs e)
        {
            if (DropDown != null)
                DropDown(this, e);
        }

        protected virtual void OnDropDownStyleChanged(EventArgs e)
        {
            if (DropDownStyleChanged != null)
                DropDownStyleChanged(this, e);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);

            if (textbox_ctrl != null)
                textbox_ctrl.Font = Font;

            if (!item_height_specified)
            {
                SizeF sz = DeviceContext.MeasureString("The quick brown Fox", Font);
                item_height = (int)sz.Height;
            }
            if (IsHandleCreated && IntegralHeight)
                   UpdateBounds();
            

            DoLayout();
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnGotFocus(EventArgs e)
        {
            Invalidate();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnLostFocus(EventArgs e)
        {
            Invalidate();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (listbox_ctrl != null)
                Controls.Add(listbox_ctrl);

            if (textbox_ctrl != null)
                Controls.Add(textbox_ctrl);

            DoLayout();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
        }

        protected override void OnKeyPress(System.Windows.Forms.KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
        }

        protected virtual void OnMeasureItem(System.Windows.Forms.MeasureItemEventArgs e)
        {
            if (MeasureItem != null)
                MeasureItem(this, e);
        }

        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);
        }

        protected override void OnResize(EventArgs e)
        {
            DoLayout();
            if (listbox_ctrl != null)
                listbox_ctrl.CalcListBoxArea();
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);

            if (SelectedIndexChanged != null)
                SelectedIndexChanged(this, e);
        }

        protected virtual void OnSelectedItemChanged(EventArgs e)
        {

        }

        protected override void OnSelectedValueChanged(EventArgs e)
        {
            base.OnSelectedValueChanged(e);
        }

        protected virtual void OnSelectionChangeCommitted(EventArgs e)
        {
            if (SelectionChangeCommitted != null)
                SelectionChangeCommitted(this, e);
        }

        protected override void RefreshItem(int index)
        {
            if (index < 0 || index >= Items.Count)
                throw new ArgumentOutOfRangeException("Index of out range");

            if (draw_mode == System.Windows.Forms.DrawMode.OwnerDrawVariable)
                item_heights.Remove(Items[index]);
        }

        public void Select(int start, int length)
        {
            if (start < 0)
                throw new ArgumentException("Start cannot be less than zero");

            if (length < 0)
                throw new ArgumentException("length cannot be less than zero");

            if (dropdown_style == ComboBoxStyle.DropDownList)
                return;

            textbox_ctrl.Select(start, length);
        }

        public void SelectAll()
        {
            if (dropdown_style == ComboBoxStyle.DropDownList)
                return;

            textbox_ctrl.SelectAll();
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, System.Windows.Forms.BoundsSpecified specified)
        {
            if ((specified & BoundsSpecified.Height) != 0)
            {
                requested_height = height;

                if (DropDownStyle == ComboBoxStyle.Simple && height > PreferredHeight)
                {
                    if (IntegralHeight)
                    {
                        int border = 2;

                        int lb_height = height - PreferredHeight - 2;
                        if (lb_height - 2 * border > ItemHeight)
                        {
                            int partial = (lb_height - 2 * border) % ItemHeight;
                            height -= partial;
                        }
                        else
                            height = PreferredHeight;
                    }
                }
                else
                    height = PreferredHeight;
            }

            base.SetBoundsCore(x, y, width, height, specified);
        }

        protected override void SetItemCore(int index, object value)
        {
            if (value.GetType() != typeof(MonoComboBoxItem))
                throw new InvalidArgumentException("Bad argument to method SetItemCore - value should be of type ComboBoxItem");

            if (index < 0 || index >= Items.Count)
                return;

            Items[index] = (MonoComboBoxItem)value;
        }

        protected override void SetItemsCore(IList value)
        {
            Items.AddRange(value);
        }

        public override string ToString()
        {
            return base.ToString() + ", Items.Count:" + Items.Count;
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            switch ((Msg)m.Msg)
            {
                case Msg.WM_MOUSE_LEAVE:
                    Point location = PointToClient(Control.MousePosition);
                    if (ClientRectangle.Contains(location))
                        return;
                    break;
                default:
                    break;
            }
            base.WndProc(ref m);
        }

        #endregion Public Methods

        #region Private Methods

        //      internal override bool InternalCapture {
        //          get { return Capture; }
        //          set {}
        //      }

        private void textbox_ctrl_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            OnKeyPress(e);
        }

        private void textbox_ctrl_GotFocus(object sender, EventArgs e)
        {
            OnGotFocus(e);
        }

        private void textbox_ctrl_LostFocus(object sender, EventArgs e)
        {
            OnLostFocus(e);
        }

        private void textbox_ctrl_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            OnMouseDown(e);
        }

        private void textbox_ctrl_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            OnMouseMove(e);
        }

        private void textbox_ctrl_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            OnMouseUp(e);
        }

        void DoLayout()
        {
            int border = 1;

            text_area = ClientRectangle;
            text_area.Height = PreferredHeight;

            listbox_area = ClientRectangle;
            listbox_area.Y = text_area.Bottom + 3;
            listbox_area.Height -= (text_area.Height + 2);

            Rectangle prev_button_area = button_area;

            if (DropDownStyle == ComboBoxStyle.Simple)
                button_area = Rectangle.Empty;
            else
            {
                button_area = text_area;
                button_area.X = text_area.Right - button_width - border;
                button_area.Y = text_area.Y + border;
                button_area.Width = button_width;
                button_area.Height = text_area.Height - 2 * border;
            }

            if (button_area != prev_button_area)
            {
                prev_button_area.Y -= border;
                prev_button_area.Width += border;
                prev_button_area.Height += 2 * border;
                Invalidate(prev_button_area);
                Invalidate(button_area);
            }

            if (textbox_ctrl != null)
            {
                textbox_ctrl.Location = new Point(text_area.X + border, text_area.Y + border);
                textbox_ctrl.Width = text_area.Width - button_area.Width - border * 2;
                textbox_ctrl.Height = text_area.Height - border * 2;
            }

            if (listbox_ctrl != null && dropdown_style == ComboBoxStyle.Simple)
            {
                listbox_ctrl.Location = listbox_area.Location;
                listbox_ctrl.CalcListBoxArea();
            }
        }

        private void CreateComboListBox()
        {
            listbox_ctrl = new ComboListBox(this);
            if (selected_index != -1)
                listbox_ctrl.HighlightedIndex = selected_index;
        }

        internal void Draw(Rectangle clip, Graphics dc)
        {

            if (DropDownStyle == ComboBoxStyle.Simple)
                dc.FillRectangle(new SolidBrush(Parent.BackColor), ClientRectangle);

            if (clip.IntersectsWith(text_area))
                ControlPaint.DrawBorder(dc, text_area, ControlPaintStyle.ControlBorderColour, ButtonBorderStyle.Solid);

            int border = 2;

            // No edit control, we paint the edit ourselves
            if (dropdown_style == ComboBoxStyle.DropDownList)
            {
                DrawItemState state = DrawItemState.None;
                Rectangle item_rect = text_area;
                item_rect.X += border;
                item_rect.Y += border;
                item_rect.Width -= (button_area.Width + 2 * border);
                item_rect.Height -= 2 * border;

                if (this.Focused)
                {
                    state = DrawItemState.Selected;
                    state |= DrawItemState.Focus;
                }

                state |= DrawItemState.ComboBoxEdit;
                if (selected_index != -1)
                {
                    OnDrawItem(new DrawItemEventArgs(dc, Font, item_rect,
                        selected_index, state, ForeColor, BackColor));
                }
                else if (this.Focused)
                {
                    const int FocusRectBorder = 0;
                    Rectangle FocusRectBounds = new Rectangle(FocusRectBorder, FocusRectBorder, this.Width - this.button_area.Width - (2*FocusRectBorder), this.Height - (2*FocusRectBorder));
                    FocusRectBounds.Inflate(-3,-3);
                    CPDrawFocusRectangle(dc, FocusRectBounds, ForeColor, BackColor);
                }



            }

            if (show_dropdown_button)
            {
                dc.FillRectangle(new SolidBrush(SystemColors.ControlLightLight), button_area);

                if (!this.Enabled)
                    button_state = System.Windows.Forms.ButtonState.Inactive;

                this.CPDrawComboButton(dc, button_area, button_state);
            }
        }

        internal void DropDownListBox()
        {
            if (DropDownStyle == ComboBoxStyle.Simple)
                return;

            if (listbox_ctrl == null)
                CreateComboListBox();

            listbox_ctrl.Location = PointToScreen(new Point(text_area.X, text_area.Y + text_area.Height));

            if (listbox_ctrl.ShowWindow())
            {
                dropped_down = true;
                listbox_ctrl.Focus();
            }

            button_state = System.Windows.Forms.ButtonState.Pushed;
            if (dropdown_style == ComboBoxStyle.DropDownList)
                Invalidate(text_area);

            OnDropDown(EventArgs.Empty);
        }

        internal void DropDownListBoxFinished()
        {
            if (DropDownStyle == ComboBoxStyle.Simple)
                return;

            button_state = System.Windows.Forms.ButtonState.Normal;
            Invalidate(button_area);
            dropped_down = false;
            this.Focus();
        }

        private int FindStringCaseInsensitive(string search)
        {
            if (search.Length == 0)
            {
                return -1;
            }

            for (int i = 0; i < Items.Count; i++)
            {
                if (String.Compare(Items[i].Text, 0, search, 0, search.Length, true) == 0)
                    return i;
            }

            return -1;
        }

        private void OnKeyDownCB(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case System.Windows.Forms.Keys.Space:
                    this.DropDownListBox();
                    break;

                case System.Windows.Forms.Keys.Enter:
                    if (!DroppedDown)
                        this.DropDownListBox();
                    break;

                case System.Windows.Forms.Keys.Up:
                    SelectedIndex = GetNextEnabledIndex(SelectedIndex, true);
                    break;

                case System.Windows.Forms.Keys.Down:
                    SelectedIndex = Math.Min(GetNextEnabledIndex(SelectedIndex, false), Items.Count - 1);
                    break;

                case System.Windows.Forms.Keys.PageUp:
                    if (listbox_ctrl != null)
                        SelectedIndex = Math.Max(GetNextEnabledIndex(SelectedIndex - (listbox_ctrl.max_page_item_size - 1), true), 0);
                    break;

                case System.Windows.Forms.Keys.PageDown:
                    if (listbox_ctrl != null)
                        SelectedIndex = Math.Min(GetNextEnabledIndex(SelectedIndex + (listbox_ctrl.max_page_item_size - 1), false), Items.Count - 1);
                    break;

                default:
                    if (listbox_ctrl == null)
                        CreateComboListBox();
                    break;
            }
        }

        /// <summary>
        /// Gets the index of the next enabled item in the specified direction
        /// </summary>
        /// <param name="StartIndex">The index to start searching from.</param>
        /// <param name="Backwards">True to go backwards in index order;
        /// false to go in increasing order of index.</param>
        /// <returns>Returns the index of the next enabled item, if any,
        /// or the supplied index if none found.</returns>
        private int GetNextEnabledIndex(int StartIndex, bool Backwards)
        {
            int Increment = 1;
            if (Backwards)
                Increment = -1;

            int TrialIndex = StartIndex;
            while ((TrialIndex < this.items.Count - Increment))
            {
                TrialIndex += Increment;

                if (TrialIndex < 0)
                {
                    return StartIndex;
                }

                if (this.items[TrialIndex].Enabled)
                    return TrialIndex;

            }

            return StartIndex;
        }


        private Color mcColourBeforeDisabling = SystemColors.ControlLightLight;
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);

            // The color to use when disabled
            Color DisabledColor = SystemColors.Control;

            //Hide edit textbox when disabled
            if (!(textbox_ctrl == null))
            {
                textbox_ctrl.Visible = this.Enabled;
            }
            //make grey when disabled
            if (this.Enabled)
            {
                if (this.BackColor == DisabledColor)
                    this.BackColor = this.mcColourBeforeDisabling;
                this.button_state = ButtonState.Normal;
            }
            else
            {
                this.mcColourBeforeDisabling = this.BackColor;
                this.BackColor = DisabledColor;
                this.button_state = ButtonState.Inactive;
            }
            this.Invalidate(true);
        }

        void OnMouseDownCB(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Rectangle area;
            if (DropDownStyle == ComboBoxStyle.DropDownList)
                area = ClientRectangle;
            else
                area = button_area;

            if (area.Contains(e.X, e.Y))
            {
                DropDownListBox();
                Invalidate(button_area);
                Update();
            }
            Capture = true;
        }

        void OnMouseMoveCB(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (DropDownStyle == ComboBoxStyle.Simple)
                return;

            if (listbox_ctrl != null && listbox_ctrl.Visible)
            {
                Point location = listbox_ctrl.PointToClient(Control.MousePosition);
                if (listbox_ctrl.ClientRectangle.Contains(location))
                    listbox_ctrl.Capture = true;
            }
        }

        void OnMouseUpCB(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Capture = false;
            OnClick(EventArgs.Empty);
            button_state = ButtonState.Normal;
            this.Invalidate();

            if (dropped_down)
                listbox_ctrl.Capture = true;
        }

        //internal override void OnPaintInternal (System.Windows.Forms.PaintEventArgs pevent)
        protected override void OnPaint(System.Windows.Forms.PaintEventArgs pevent)
        {
            if (suspend_ctrlupdate)
                return;
            // get base class drawing of background etc
            base.OnPaint(pevent);
            Draw(ClientRectangle, pevent.Graphics);
        }

        private void OnTextChangedEdit(object sender, EventArgs e)
        {
            if (process_textchanged_event == false)
                return;

            int item = FindStringCaseInsensitive(textbox_ctrl.Text);

            if (item == -1)
                return;

            if (listbox_ctrl != null)
            {
                listbox_ctrl.SetTopItem(item);
                listbox_ctrl.HighlightedIndex = item;
            }
        }


        internal void SetControlText(string s)
        {
            process_textchanged_event = false;
            textbox_ctrl.Text = s;
            process_textchanged_event = true;
        }

        //      protected override void UpdateBounds ()
        //      {
        //          if (requested_height != -1)
        //              SetBoundsCore (0, 0, 0, requested_height, BoundsSpecified.Height);
        //      }

        internal void UpdatedItems()
        {
            if (listbox_ctrl != null)
            {
                listbox_ctrl.UpdateLastVisibleItem();
                listbox_ctrl.CalcListBoxArea();
                listbox_ctrl.Refresh();
            }
        }


        #region "Painting"

        private void CPDrawComboButton(Graphics graphics, Rectangle rectangle, ButtonState state)
        {
            ControlPaintStyle.DrawComboButton(graphics, rectangle, state);
            return;
        }


        private void CPDrawBorder3D(Graphics graphics, Rectangle rectangle, Border3DStyle style, Border3DSide sides)
        {
            CPDrawBorder3D(graphics, rectangle, style, sides, SystemColors.Control);
        }

        private void CPDrawBorder3D(Graphics graphics, Rectangle rectangle, Border3DStyle style, Border3DSide sides, Color control_color)
        {
            Pen penTopLeft;
            Pen penTopLeftInner;
            Pen penBottomRight;
            Pen penBottomRightInner;
            Rectangle rect = new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
            bool is_ColorControl = control_color.ToArgb() == SystemColors.Control.ToArgb() ? true : false;

            if ((style & Border3DStyle.Adjust) != 0)
            {
                rect.Y -= 2;
                rect.X -= 2;
                rect.Width += 4;
                rect.Height += 4;
            }

            penTopLeft = penTopLeftInner = penBottomRight = penBottomRightInner = is_ColorControl ? SystemPens.Control : new Pen(control_color);

            //CPColor cpcolor = Color.Empty;

            //          if (!is_ColorControl)
            //              cpcolor = ResPool.GetCPColor (control_color);

            switch (style)
            {
                case Border3DStyle.Raised:
                    penTopLeftInner = is_ColorControl ? SystemPens.ControlLightLight : new Pen(SystemColors.ControlLightLight);
                    penBottomRight = is_ColorControl ? SystemPens.ControlDarkDark : new Pen(SystemColors.ControlDarkDark);
                    penBottomRightInner = is_ColorControl ? SystemPens.ControlDark : new Pen(SystemColors.ControlDark);
                    break;
                case Border3DStyle.Sunken:
                    penTopLeft = is_ColorControl ? SystemPens.ControlDark : new Pen(SystemColors.ControlDark);
                    penTopLeftInner = is_ColorControl ? SystemPens.ControlDarkDark : new Pen(SystemColors.ControlDarkDark);
                    penBottomRight = is_ColorControl ? SystemPens.ControlLightLight : new Pen(SystemColors.ControlLightLight);
                    break;
                case Border3DStyle.Etched:
                    penTopLeft = penBottomRightInner = is_ColorControl ? SystemPens.ControlDark : new Pen(SystemColors.ControlDark);
                    penTopLeftInner = penBottomRight = is_ColorControl ? SystemPens.ControlLightLight : new Pen(SystemColors.ControlLightLight);
                    break;
                case Border3DStyle.RaisedOuter:
                    penBottomRight = is_ColorControl ? SystemPens.ControlDarkDark : new Pen(SystemColors.ControlDarkDark);
                    break;
                case Border3DStyle.SunkenOuter:
                    penTopLeft = is_ColorControl ? SystemPens.ControlDark : new Pen(SystemColors.ControlDark);
                    penBottomRight = is_ColorControl ? SystemPens.ControlLightLight : new Pen(SystemColors.ControlLightLight);
                    break;
                case Border3DStyle.RaisedInner:
                    penTopLeft = is_ColorControl ? SystemPens.ControlLightLight : new Pen(SystemColors.ControlLightLight);
                    penBottomRight = is_ColorControl ? SystemPens.ControlDark : new Pen(SystemColors.ControlDark);
                    break;
                case Border3DStyle.SunkenInner:
                    penTopLeft = is_ColorControl ? SystemPens.ControlDarkDark : new Pen(SystemColors.ControlDarkDark);
                    break;
                case Border3DStyle.Flat:
                    penTopLeft = penBottomRight = is_ColorControl ? SystemPens.ControlDark : new Pen(SystemColors.ControlDark);
                    break;
                case Border3DStyle.Bump:
                    penTopLeftInner = penBottomRight = is_ColorControl ? SystemPens.ControlDarkDark : new Pen(SystemColors.ControlDarkDark);
                    break;
                default:
                    break;
            }

            if ((sides & Border3DSide.Middle) != 0)
            {
                Brush brush = is_ColorControl ? SystemBrushes.Control : new SolidBrush(control_color);
                graphics.FillRectangle(brush, rect);
            }

            if ((sides & Border3DSide.Left) != 0)
            {
                graphics.DrawLine(penTopLeft, rect.Left, rect.Bottom - 2, rect.Left, rect.Top);
                graphics.DrawLine(penTopLeftInner, rect.Left + 1, rect.Bottom - 2, rect.Left + 1, rect.Top);
            }

            if ((sides & Border3DSide.Top) != 0)
            {
                graphics.DrawLine(penTopLeft, rect.Left, rect.Top, rect.Right - 2, rect.Top);
                graphics.DrawLine(penTopLeftInner, rect.Left + 1, rect.Top + 1, rect.Right - 3, rect.Top + 1);
            }

            if ((sides & Border3DSide.Right) != 0)
            {
                graphics.DrawLine(penBottomRight, rect.Right - 1, rect.Top, rect.Right - 1, rect.Bottom - 1);
                graphics.DrawLine(penBottomRightInner, rect.Right - 2, rect.Top + 1, rect.Right - 2, rect.Bottom - 2);
            }

            if ((sides & Border3DSide.Bottom) != 0)
            {
                graphics.DrawLine(penBottomRight, rect.Left, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1);
                graphics.DrawLine(penBottomRightInner, rect.Left + 1, rect.Bottom - 2, rect.Right - 2, rect.Bottom - 2);
            }
        }

        private void DrawComboBoxItem(MonoComboBox ctrl, DrawItemEventArgs e)
        {
            if ((e.Index < 0) || (e.Index > this.Items.Count -1 ))
                return;

            MonoComboBoxItem item_to_draw = this.Items[e.Index];

            Color back_color, fore_color;
            Rectangle text_draw = e.Bounds;
            StringFormat string_format = new StringFormat();
            string_format.FormatFlags = StringFormatFlags.LineLimit;
            System.Drawing.Font item_font = (item_to_draw.ItemFont != null) ? item_to_draw.ItemFont : e.Font;

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                back_color = SystemColors.Highlight;
                fore_color = SystemColors.HighlightText;
            }
            else
            {
                back_color = e.BackColor;
                if (item_to_draw.Enabled)
                {
                    if (!item_to_draw.FontColour.Equals(Color.Empty))
                    {
                        fore_color = item_to_draw.FontColour;
                    }
                    else
                    {
                        fore_color = e.ForeColor;
                    }
                }
                else
                {
                    fore_color = item_to_draw.DisabledColour;
                }

            }


            e.Graphics.FillRectangle(new SolidBrush(back_color), e.Bounds);

            if (e.Index != -1)
            {
                e.Graphics.DrawString(item_to_draw.Text, item_font,
                     new SolidBrush(fore_color),
                    text_draw, string_format);
            }

            if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
            {
                CPDrawFocusRectangle(e.Graphics, e.Bounds, fore_color, back_color);
            }

            string_format.Dispose();
        }

        private void CPDrawFocusRectangle(Graphics graphics, Rectangle rectangle, Color foreColor, Color backColor)
        {
            Rectangle rect = rectangle;
            Pen pen;
            HatchBrush brush;

            if (backColor.GetBrightness() >= 0.5)
            {
                foreColor = Color.Transparent;
                backColor = Color.Black;

            }
            else
            {
                backColor = Color.FromArgb(Math.Abs(backColor.R - 255), Math.Abs(backColor.G - 255), Math.Abs(backColor.B - 255));
                foreColor = Color.Black;
            }

            brush = new HatchBrush(HatchStyle.Percent50, backColor, foreColor);
            pen = new Pen(brush, 1);

            rect.Width--;
            rect.Height--;

            graphics.DrawRectangle(pen, rect);
            pen.Dispose();
        }

        #endregion

        #endregion Private Methods

        [ListBindableAttribute(false)]
        public class ObjectCollection : IList, ICollection, IEnumerable
        {

            private MonoComboBox owner;
            internal List<object> object_items = new List<object>();

            public ObjectCollection(MonoComboBox owner)
            {
                this.owner = owner;
            }

            #region Public Properties

            public void CopyTo(Array array, int index)
            {
                throw new NotImplementedException();
            }

            public int Count
            {
                get { return object_items.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public virtual object this[int index]
            {
                get
                {
                    if (index < 0 || index >= Count)
                        throw new ArgumentOutOfRangeException("Index of out range");

                    return object_items[index];
                }
                set
                {
                    if (index < 0 || index >= Count)
                        throw new ArgumentOutOfRangeException("Index of out range");

                    object_items[index] = value;
                }
            }

            bool ICollection.IsSynchronized
            {
                get { return false; }
            }

            object ICollection.SyncRoot
            {
                get { return this; }
            }

            bool IList.IsFixedSize
            {
                get { return false; }
            }

            #endregion Public Properties

            #region Private Properties
            internal List<object> ObjectItems
            {
                get { return object_items; }
                set
                {
                    object_items = value;
                }
            }

            #endregion Private Properties

            #region Public Methods
            public int Add(object item)
            {
                int idx;

                idx = AddItem(item);
                owner.UpdatedItems();
                return idx;
            }

            public void AddRange(object[] items)
            {
                foreach (object mi in items)
                    AddItem(mi);

                owner.UpdatedItems();
            }

            public void Clear()
            {
                owner.selected_index = -1;
                object_items.Clear();
                owner.UpdatedItems();
                owner.Refresh();
            }

            public bool Contains(object obj)
            {
                return object_items.Contains(obj);
            }

            public void CopyTo(object[] dest, int arrayIndex)
            {
                object_items.CopyTo(dest, arrayIndex);
            }

            public IEnumerator GetEnumerator()
            {
                return object_items.GetEnumerator();
            }

            int IList.Add(object item)
            {
                return Add(item);
            }

            public int IndexOf(object value)
            {
                return object_items.IndexOf(value);
            }

            public void Insert(int index, object item)
            {
                if (index < 0 || index > Count)
                    throw new ArgumentOutOfRangeException("Index of out range");

                ObjectCollection new_items = new ObjectCollection(owner);
                object sel_item = owner.SelectedItem;

                owner.BeginUpdate();

                for (int i = 0; i < index; i++)
                {
                    new_items.AddItem(ObjectItems[i]);
                }

                new_items.AddItem(item);

                for (int i = index; i < Count; i++)
                {
                    new_items.AddItem(ObjectItems[i]);
                }

                ObjectItems = new_items.ObjectItems;

                if (sel_item != null)
                {
                    int idx = IndexOf(sel_item);
                    owner.selected_index = idx;
                    owner.listbox_ctrl.HighlightedIndex = idx;
                }

                owner.EndUpdate();  // Calls UpdatedItems
            }

            public void Remove(object value)
            {
                if (IndexOf(value) == owner.SelectedIndex)
                    owner.SelectedItem = null;

                RemoveAt(IndexOf(value));

            }

            public void RemoveAt(int index)
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException("Index of out range");

                if (index == owner.SelectedIndex)
                    owner.SelectedItem = null;

                object_items.RemoveAt(index);
                owner.UpdatedItems();
            }
            #endregion Public Methods

            #region Private Methods
            private int AddItem(object item)
            {
                int cnt = object_items.Count;
                object_items.Add(item);
                return cnt;
            }

            internal void AddRange(IList items)
            {
                foreach (object mi in items)
                    AddItem(mi);

                owner.UpdatedItems();
            }

            #endregion Private Methods
        }

        internal class ComboListBox : System.Windows.Forms.Control
        {
            private MonoComboBox owner;
            private VScrollBarLB vscrollbar_ctrl;
            private int top_item;           /* First item that we show the in the current page, indexed from 0 */
            private int last_item;          /* Last visible item, indexed from 0 */
            internal int max_page_item_size;         /* Number of listbox items per page */
            private Rectangle textarea_drawable;    /* Rectangle of the drawable text area */
            private bool updating;

            internal enum ItemNavigation
            {
                First,
                Last,
                Next,
                Previous,
                NextPage,
                PreviousPage,
            }

            class VScrollBarLB : System.Windows.Forms.VScrollBar
            {
                public VScrollBarLB()
                {
                }

                //              internal override bool InternalCapture {
                //                  get { return Capture; }
                //                  set { }
                //              }

                public bool FireMouseDown(System.Windows.Forms.MouseEventArgs e)
                {
                    if (Visible)
                    {
                        e = TranslateEvent(e);
                        if (ClientRectangle.Contains(e.X, e.Y))
                        {
                            OnMouseDown(e);
                            return true;
                        }
                    }
                    return false;
                }

                public void FireMouseUp(System.Windows.Forms.MouseEventArgs e)
                {
                    if (Visible)
                    {
                        e = TranslateEvent(e);
                        if (ClientRectangle.Contains(e.X, e.Y))
                            OnMouseUp(e);
                    }
                }

                public void FireMouseMove(System.Windows.Forms.MouseEventArgs e)
                {
                    if (Visible)
                    {
                        e = TranslateEvent(e);
                        if (ClientRectangle.Contains(e.X, e.Y))
                            OnMouseMove(e);
                    }
                }

                System.Windows.Forms.MouseEventArgs TranslateEvent(System.Windows.Forms.MouseEventArgs e)
                {
                    Point loc = PointToClient(Control.MousePosition);
                    return new System.Windows.Forms.MouseEventArgs(e.Button, e.Clicks, loc.X, loc.Y, e.Delta);
                }
            }

            public ComboListBox(MonoComboBox owner)
            {
                this.owner = owner;
                top_item = 0;
                last_item = 0;
                max_page_item_size = 0;

                MouseDown += new System.Windows.Forms.MouseEventHandler(OnMouseDownPUW);
                MouseUp += new System.Windows.Forms.MouseEventHandler(OnMouseUpPUW);
                MouseMove += new System.Windows.Forms.MouseEventHandler(OnMouseMovePUW);
                KeyDown += new System.Windows.Forms.KeyEventHandler(OnKeyDownPUW);
                KeyPress += new System.Windows.Forms.KeyPressEventHandler(OnKeyPressPUW);

                SetStyle(System.Windows.Forms.ControlStyles.UserPaint | System.Windows.Forms.ControlStyles.AllPaintingInWmPaint, true);
                SetStyle(System.Windows.Forms.ControlStyles.ResizeRedraw | System.Windows.Forms.ControlStyles.Opaque, true);

                //              if (owner.DropDownStyle == ComboBoxStyle.Simple)
                //                  BorderStyle = BorderStyle.Fixed3D;
                //              else
                //                  BorderStyle = BorderStyle.FixedSingle;
            }

            protected override System.Windows.Forms.CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;
                    if (owner == null || owner.DropDownStyle == ComboBoxStyle.Simple)
                        return cp;

                    cp.Style ^= (int)WindowStyles.WS_CHILD;
                    cp.Style |= (int)WindowStyles.WS_POPUP;
                    cp.ExStyle |= (int)WindowExStyles.WS_EX_TOOLWINDOW | (int)WindowExStyles.WS_EX_TOPMOST;
                    return cp;
                }
            }

            protected override void OnLostFocus(EventArgs e)
            {
                if (!updating)
                {
                    HideWindow();
                }
                base.OnLostFocus(e);
            }

            private void ScrollOnLostFocus(Object sender, EventArgs e)
            {
                this.HideWindow();
            }

            int BorderWidth
            {
                get
                {
                    //                  switch (border_style) {
                    //                  case BorderStyle.Fixed3D:
                    //                      return ThemeEngine.Current.Border3DSize.Width;
                    //                  default:
                    //                      return ThemeEngine.Current.BorderSize.Width;
                    //                  }
                    return 1;
                }
            }

            #region Private Methods
            // Calcs the listbox area
            internal void CalcListBoxArea()
            {
                int width, height;
                bool show_scrollbar = false;

                if (owner.DropDownStyle == ComboBoxStyle.Simple)
                {
                    Rectangle area = owner.listbox_area;
                    width = area.Width;
                    height = area.Height;
                }
                else
                { // DropDown or DropDownList

                    width = owner.DropDownWidth;
                    int pagesize = PageSize;
                    if (owner.DrawMode == System.Windows.Forms.DrawMode.OwnerDrawVariable)
                    {
                        height = 0;
                        for (int i = 0; i < pagesize; i++)
                        {
                            height += owner.GetItemHeight(i);
                        }

                    }
                    else
                    {
                        height = owner.ItemHeight * pagesize;
                    }
                }

                if (owner.Items.Count <= owner.MaxDropDownItems)
                {
                    if (vscrollbar_ctrl != null)
                        vscrollbar_ctrl.Visible = false;
                }
                else
                {
                    /* Need vertical scrollbar */
                    if (vscrollbar_ctrl == null)
                    {
                        vscrollbar_ctrl = new VScrollBarLB();
                        vscrollbar_ctrl.LostFocus += new EventHandler(ScrollOnLostFocus);
                        vscrollbar_ctrl.ValueChanged += new EventHandler(VerticalScrollEvent);
                        Controls.Add(vscrollbar_ctrl);
                    }
                    vscrollbar_ctrl.Minimum = 0;
                    vscrollbar_ctrl.SmallChange = 1;
                    vscrollbar_ctrl.Maximum = owner.Items.Count - 1; // Minus 1 for zero-based index 
                    vscrollbar_ctrl.LargeChange = owner.maxdrop_items;
                    vscrollbar_ctrl.Height = height - 2 * BorderWidth - 1;
                    vscrollbar_ctrl.Location = new Point(width - vscrollbar_ctrl.Width - BorderWidth - 1, 1);
                    
                    show_scrollbar = vscrollbar_ctrl.Visible = true;

                    int hli = HighlightedIndex;
                    if (hli > 0)
                    {
                        hli = Math.Min(hli, vscrollbar_ctrl.Maximum);
                        vscrollbar_ctrl.Value = hli;
                    }
                }

                Size = new Size(width, height);
                textarea_drawable = ClientRectangle;
                textarea_drawable.Width = width;
                textarea_drawable.Height = height;

                if (vscrollbar_ctrl != null && show_scrollbar)
                    textarea_drawable.Width -= vscrollbar_ctrl.Width;

                last_item = LastVisibleItem();
                max_page_item_size = textarea_drawable.Height / owner.ItemHeight;
            }

            private void Draw(Rectangle clip, Graphics dc)
            {
                dc.FillRectangle(new SolidBrush(owner.BackColor), clip);

                if (owner.Items.Count > 0)
                {

                    for (int i = Math.Max(0, Math.Min(this.owner.Items.Count-1, top_item)); i <= Math.Min(this.owner.Items.Count-1, last_item); i++)
                    {
                        Rectangle item_rect = GetItemDisplayRectangle(i, top_item);

                        if (!clip.IntersectsWith(item_rect))
                            continue;

                        DrawItemState state = DrawItemState.None;

                        if (i == HighlightedIndex)
                        {
                            state |= DrawItemState.Selected;

                            if (owner.DropDownStyle == ComboBoxStyle.DropDownList)
                            {
                                state |= DrawItemState.Focus;
                            }
                        }

                        owner.OnDrawItem(new DrawItemEventArgs(dc, owner.Font, item_rect,
                            i, state, owner.DropDownForeColor, owner.DropDownBackColor));
                    }
                }

                ControlPaint.DrawBorder(dc, clip, Color.Black, ButtonBorderStyle.Solid);

            }

            int highlighted_index = -1;

            public int HighlightedIndex
            {
                get { return highlighted_index; }
                set
                {
                    if (highlighted_index == value)
                        return;

                    highlighted_index = Math.Min(Math.Max(0, value), owner.items.Count - 1);

                    if (vscrollbar_ctrl != null)
                    {
                        int temp = -1;
                        if (highlighted_index < top_item)
                        {
                            temp = highlighted_index;
                        }
                        if (highlighted_index >= last_item)
                        {
                            temp = highlighted_index - max_page_item_size + 1;
                        }

                        if (temp != -1)
                        {
                            temp = Math.Max(0, temp);
                            temp = Math.Min(temp, (owner.items.Count - 1) - max_page_item_size + 1);
                            SetTopItem(temp);

                            vscrollbar_ctrl.Value = top_item;

                        }
                    }
                    UpdateLastVisibleItem();
                    Refresh();
                }
            }
        

            private Rectangle GetItemDisplayRectangle(int index, int top_index)
            {
                if (index < 0 || index >= owner.Items.Count)
                    throw new ArgumentOutOfRangeException("GetItemRectangle index out of range.");

                Rectangle item_rect = new Rectangle();
                int height = owner.GetItemHeight(index);

                item_rect.X = 0;
                item_rect.Width = textarea_drawable.Width;
                if (owner.DrawMode == System.Windows.Forms.DrawMode.OwnerDrawVariable)
                {
                    item_rect.Y = 0;
                    for (int i = top_index; i < index; i++)
                        item_rect.Y += owner.GetItemHeight(i);
                }
                else
                    item_rect.Y = height * (index - top_index);

                item_rect.Height = height;
                return item_rect;
            }

            public void HideWindow()
            {
                if (owner.DropDownStyle == ComboBoxStyle.Simple)
                    return;

                Capture = false;
                Hide();
                owner.DropDownListBoxFinished();
            }

            /// <summary>
            /// Gets the Index of the item clicked at the supplied
            /// mouse coordinates.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns>Returns the zero based index of the
            /// item at the specified location, or -1 if no such item
            /// is found.</returns>
            private int IndexFromPointDisplayRectangle(int x, int y)
            {
                top_item = Math.Max(0,top_item);
                for (int i = top_item; i <= last_item; i++)
                {
                    if (GetItemDisplayRectangle(i, top_item).Contains(x, y) == true)
                        return i;
                }

                return -1;
            }

            protected override bool IsInputKey(System.Windows.Forms.Keys keyData)
            {
                return owner.IsInputKey(keyData);
            }

            private int LastVisibleItem()
            {
                Rectangle item_rect;
                int top_y = textarea_drawable.Y + textarea_drawable.Height;
                int i = 0;

                top_item = Math.Max(0, top_item);
                for (i = top_item; i < owner.Items.Count; i++)
                {
                    item_rect = GetItemDisplayRectangle(i, top_item);
                    if (item_rect.Y + item_rect.Height > top_y)
                    {
                        return i;
                    }
                }
                return i - 1;
            }

            private void NavigateItemVisually(ItemNavigation navigation)
            {
                switch (navigation)
                {
                    case ItemNavigation.Next:
                        HighlightedIndex++;
                        break;

                    case ItemNavigation.Previous:
                        HighlightedIndex--;
                        break;

                    case ItemNavigation.NextPage:
                        HighlightedIndex += max_page_item_size;
                        break;

                    case ItemNavigation.PreviousPage:
                        HighlightedIndex -= max_page_item_size;
                        break;

                    default:
                        break;
                }

                    owner.OnSelectionChangeCommitted(new EventArgs());
                    if (owner.DropDownStyle == ComboBoxStyle.Simple)
                        owner.SetControlText(owner.Items[HighlightedIndex].Text);
            }

            /// <summary>
            /// A timestamp of the last keypress used for 
            /// navigation.
            /// </summary>
            private DateTime mLastKeyPress;

            private string mSearchText;
            public void NavigateByTextSearch(char LatestKeyPress)
            {
                // If timeout since last keypress, clear search text
                if (DateTime.Now.Subtract(new TimeSpan(0, 0, 1)) > mLastKeyPress)
                {
                    mSearchText = string.Empty;
                }
                // Record timestamp of most recent keypress
                mLastKeyPress = DateTime.Now;
                mSearchText += LatestKeyPress;
                for (int i = 0; i <= owner.items.Count - 1; i++)
                {
                    MonoComboBoxItem ci = owner.Items[i];
                    if (ci.Text.ToLower().StartsWith(mSearchText))
                    {
                        if (owner.DroppedDown)
                        {
                            HighlightedIndex = i;
                            owner.OnSelectionChangeCommitted(new EventArgs());
                            if (owner.DropDownStyle == ComboBoxStyle.Simple)
                                owner.SetControlText(owner.Items[i].Text);
                        }
                        else
                        {
                            owner.SelectedIndex = i;
                        }
                        return;
                    }
                }
            }


            private void OnKeyDownPUW(object sender, System.Windows.Forms.KeyEventArgs e)
            {
                               
                switch (e.KeyCode)
                {
                    case Keys.Enter:
                        owner.OnSelectionChangeCommitted(new EventArgs());
                        owner.SelectedIndex = HighlightedIndex;
                        HideWindow();
                        break;

                    case Keys.Escape:
                        HideWindow();
                        break;

                    case Keys.Up:
                        mSearchText = string.Empty;
                        NavigateItemVisually(ItemNavigation.Previous);
                        break;

                    case Keys.Down:
                        mSearchText = string.Empty;
                        NavigateItemVisually(ItemNavigation.Next);
                        break;

                    case Keys.PageUp:
                        mSearchText = string.Empty;
                        NavigateItemVisually(ItemNavigation.PreviousPage);
                        break;

                    case Keys.PageDown:
                        mSearchText = string.Empty;
                        NavigateItemVisually(ItemNavigation.NextPage);
                        break;
                }
            }

            private void OnKeyPressPUW(object sender, System.Windows.Forms.KeyPressEventArgs e)
            {
                NavigateByTextSearch(e.KeyChar);
            }

            public void SetTopItem(int item)
            {
                if (top_item == item)
                    return;
                top_item = item;
                UpdateLastVisibleItem();
                if (vscrollbar_ctrl != null)
                    vscrollbar_ctrl.Value = Math.Max(0, Math.Min(item, vscrollbar_ctrl.Maximum));
                Refresh();
            }

            private void OnMouseDownPUW(object sender, System.Windows.Forms.MouseEventArgs e)
            {
                int index = IndexFromPointDisplayRectangle(e.X, e.Y);

                if (index == -1)
                {
                    if (vscrollbar_ctrl == null || !vscrollbar_ctrl.FireMouseDown(e))
                        HideWindow();
                }
                else
                {
                    MonoComboBoxItem item_clicked = owner.items[index];
                    if (item_clicked.Enabled)
                    {
                        owner.DropDownListBoxFinished();
                        owner.OnSelectionChangeCommitted(new EventArgs());
                        owner.SelectedIndex = index;
                        HighlightedIndex = index;
                        HideWindow();
                    }
                }

                if (owner.DropDownStyle == ComboBoxStyle.Simple)
                {
                    owner.OnMouseDown(e);
                    owner.textbox_ctrl.Focus();
                }
            }

            private void OnMouseMovePUW(object sender, System.Windows.Forms.MouseEventArgs e)
            {
                if (owner.DropDownStyle == ComboBoxStyle.Simple)
                {
                    owner.OnMouseMove(e);
                    return;
                }

                Point pt = PointToClient(Control.MousePosition);
                int index = IndexFromPointDisplayRectangle(pt.X, pt.Y);

                if (index != -1)
                {
                    HighlightedIndex = index;
                    return;
                }

                if (vscrollbar_ctrl != null)
                    vscrollbar_ctrl.FireMouseMove(e);
            }

            private void OnMouseUpPUW(object sender, System.Windows.Forms.MouseEventArgs e)
            {
                if (owner.DropDownStyle == ComboBoxStyle.Simple)
                {
                    owner.OnMouseUp(e);
                    return;
                }

                if (vscrollbar_ctrl != null)
                    vscrollbar_ctrl.FireMouseUp(e);
            
            }


            [EditorBrowsable(EditorBrowsableState.Advanced)]
            protected override void OnMouseWheel(MouseEventArgs e)
            {
                if (vscrollbar_ctrl != null)
                {
                    // Decide by how much to scroll
                    int scrollincrement = vscrollbar_ctrl.SmallChange;

                    int MaxAllowable = vscrollbar_ctrl.Maximum - vscrollbar_ctrl.LargeChange + 1;
                    if (e.Delta > 0)
                    {
                        vscrollbar_ctrl.Value = Math.Max(vscrollbar_ctrl.Minimum, vscrollbar_ctrl.Value - scrollincrement);
                    }
                    else
                    {
                        vscrollbar_ctrl.Value = Math.Min(MaxAllowable, vscrollbar_ctrl.Value + scrollincrement);
                    }
                }
                base.OnMouseWheel(e);
            }

            //internal override void OnPaintInternal (System.Windows.Forms.PaintEventArgs pevent)
            protected override void OnPaint(System.Windows.Forms.PaintEventArgs pevent)
            {
                Draw(this.ClientRectangle, pevent.Graphics);
            }

            public bool ShowWindow()
            {
                updating = true;

                if (owner.DropDownStyle == ComboBoxStyle.Simple && owner.Items.Count == 0)
                    return false;

                HighlightedIndex = owner.SelectedIndex;

                CalcListBoxArea();
                Show();
                Focus();

                Refresh();
                owner.OnDropDown(EventArgs.Empty);

                updating = false;

                return true;
            }

            public void UpdateLastVisibleItem()
            {
                last_item = LastVisibleItem();
            }

            // Value Changed
            private void VerticalScrollEvent(object sender, EventArgs e)
            {
                int pagesize = PageSize;
                int NewTopItem = Math.Min((owner.Items.Count -1) - pagesize + 1, vscrollbar_ctrl.Value);
                if (top_item == NewTopItem)
                    return;

                top_item = NewTopItem;
                UpdateLastVisibleItem();
                Refresh();
            }

            #endregion Private Methods

            /// <summary>
            /// The number of items that are displayed in the
            /// drop-down window. This is limited by the MaxDropDownItems
            /// property.
            /// </summary>
            public int PageSize
            {
                get
                {
                    return (owner.Items.Count <= owner.MaxDropDownItems) ? owner.Items.Count : owner.MaxDropDownItems;
                }
            }

        }
    }
}

