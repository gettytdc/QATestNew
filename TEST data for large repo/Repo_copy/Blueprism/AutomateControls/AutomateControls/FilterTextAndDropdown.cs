using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace AutomateControls
{
    public partial class FilterTextAndDropdown : ListControl
    {
        private readonly ToolStripControlHost _controlHost;
        private readonly ListBox _listBox;
        private readonly ToolStripDropDown _popupControl;
        private bool _isDroppedDown = false;
        private Rectangle _rectContent = new Rectangle(0, 0, 1, 1);
        private int _selectedIndex = -1;

        public int DropDownHeight { get; set; } = 200;

        public ListBox.ObjectCollection Items
        {
            get { return _listBox.Items; }
        }

        public string FilterText
        {
            get { return filterTextBox.Text; }
        }

        public int DropDownWidth { get; set; } = 0;

        public int MaxDropDownItems { get; set; } = 6;

        #region Delegates
        public event EventHandler SelectedIndexChanged;

        public event System.Windows.Forms.DrawItemEventHandler DrawItem;

        public event System.Windows.Forms.MeasureItemEventHandler MeasureItem;

        public event EventHandler<EventArgs> FilterTextChangedEventHandler;

        #endregion

        public FilterTextAndDropdown()
        {
            InitializeComponent();

            _listBox = new ListBox
            {
                IntegralHeight = true,
                BorderStyle = BorderStyle.FixedSingle,
                SelectionMode = SelectionMode.One,
                BindingContext = new BindingContext()
            };

            _controlHost = new ToolStripControlHost(_listBox)
            {
                Padding = new Padding(0),
                Margin = new Padding(0),
                AutoSize = false
            };

            _popupControl = new ToolStripDropDown
            {
                Padding = new Padding(0),
                Margin = new Padding(0),
                AutoSize = true,
                DropShadowEnabled = false,
                AutoClose = false
            };
            _popupControl.Items.Add(_controlHost);

            DropDownWidth = this.Width;

            _listBox.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(ListBox_MeasureItem);
            _listBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(ListBox_DrawItem);
            _listBox.MouseMove += new MouseEventHandler(ListBox_MouseMove);
            _listBox.MouseClick += new MouseEventHandler(ListBox_MouseClick);

            _popupControl.Closing += new ToolStripDropDownClosingEventHandler(PopupControl_Closing);
            _popupControl.Click += new EventHandler(PopupControl_Clicked);

            filterTextBox.TextChanged += new EventHandler(TextBox_TextChanged);
            filterTextBox.KeyDown += new KeyEventHandler(TextBox_KeyDown);
            filterTextBox.MouseClick += new MouseEventHandler(TextBox_Click);

            this.ClientSizeChanged += new EventHandler(Resize);
        }

        public bool IsDroppedDown
        {
            get { return _isDroppedDown; }
            set
            {
                if (_isDroppedDown && !value)
                {
                    if (_popupControl.IsDropDown)
                    {
                        _popupControl.Close();
                    }
                }

                _isDroppedDown = value;

                if (_isDroppedDown)
                {
                    _controlHost.Control.Width = DropDownWidth;

                    _listBox.Refresh();

                    if (_listBox.Items.Count > 0)
                    {
                        int h = 0;
                        int i = 0;
                        int maxItemHeight = 0;
                        int highestItemHeight = 0;
                        foreach (object item in _listBox.Items)
                        {
                            int itHeight = _listBox.GetItemHeight(i);
                            if (highestItemHeight < itHeight)
                            {
                                highestItemHeight = itHeight;
                            }
                            h = h + itHeight;
                            if (i <= (MaxDropDownItems - 1))
                            {
                                maxItemHeight = h;
                            }
                            i = i + 1;
                        }

                        if (maxItemHeight > DropDownHeight)
                            _listBox.Height = DropDownHeight + 3;
                        else
                        {
                            if (maxItemHeight > highestItemHeight)
                                _listBox.Height = maxItemHeight + 3;
                            else
                                _listBox.Height = highestItemHeight + 3;
                        }
                    }
                    else
                    {
                        _listBox.Height = 15;
                    }

                    _popupControl.Show(this, CalculateDropPosition(), ToolStripDropDownDirection.BelowRight);
                }

                Invalidate();
            }
        }

        #region NestedControlsEvents
        private void Control_LostFocus(object sender, EventArgs e) => OnLostFocus(e);

        private void Control_GotFocus(object sender, EventArgs e) => OnGotFocus(e);

        private void Control_MouseLeave(object sender, EventArgs e) => OnMouseLeave(e);

        private void Control_MouseEnter(object sender, EventArgs e) => OnMouseEnter(e);

        private void Control_MouseDown(object sender, MouseEventArgs e) => OnMouseDown(e);

        private void TextBox_TextChanged(object sender, EventArgs e) => OnTextChanged(e);

        private void ListBox_MouseMove(object sender, MouseEventArgs e)
        {
            int i;
            for (i = 0; i < _listBox.Items.Count; i++)
            {
                if (_listBox.GetItemRectangle(i).Contains(_listBox.PointToClient(MousePosition)))
                {
                    _listBox.SelectedIndex = i;
                    return;
                }
            }
        }

        private void ListBox_MouseClick(object sender, MouseEventArgs e)
        {
            int i;
            for (i = 0; i < _listBox.Items.Count; i++)
            {
                if (_listBox.GetItemRectangle(i).Contains(_listBox.PointToClient(MousePosition)))
                {
                    _listBox.SelectedIndex = i;
                    return;
                }
            }
        }

        private void ListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                DrawItem?.Invoke(this, e);
            }
        }

        private void ListBox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            MeasureItem?.Invoke(this, e);
        }

        private void PopupControl_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            e.Cancel = false;
        }

        private void PopupControl_Clicked(object sender, EventArgs e)
        {
            if (!(_listBox.SelectedItem.ToString() is null))
            {
                filterTextBox.Text = _listBox.SelectedItem.ToString();
                filterTextBox.SelectionStart = filterTextBox.Text.Length;
            }
        }

        private void TextBox_Click(object sender, MouseEventArgs e)
        {
            if (_listBox.Items.Count > 1)
                IsDroppedDown = true;
        }

        private void TextBox_LocationChanged(object sender, EventArgs e)
        {
            IsDroppedDown = false;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter)
            {
                if (!(_listBox.SelectedItem is null))
                {
                    if (!(_listBox.SelectedItem.ToString() is null))
                    {
                        filterTextBox.Text = _listBox.SelectedItem.ToString();
                        filterTextBox.SelectionStart = filterTextBox.Text.Length;
                        IsDroppedDown = false;
                    }
                }
            }

            if (e.KeyCode == Keys.Down)
            {
                if (_listBox.Items.Count > 0)
                {
                    if (_listBox.SelectedIndex < _listBox.Items.Count - 1)
                    {
                        _listBox.SelectedIndex += 1;
                    }
                    else
                    {
                        _listBox.SelectedIndex = 0;
                    }
                }
                e.Handled = true;
            }

            if (e.KeyCode == Keys.Up)
            {
                if (_listBox.Items.Count > 0)
                {
                    if (_listBox.SelectedIndex > 0)
                    {
                        _listBox.SelectedIndex -= 1;
                    }
                    else
                    {
                        _listBox.SelectedIndex = _listBox.Items.Count - 1;
                    }
                }
                e.Handled = true;
            }
        }
        #endregion

        private Point CalculateDropPosition()
        {
            var point = new Point(0, this.Height);
            if ((this.PointToScreen(new Point(0, 0)).Y + this.Height + _controlHost.Height) > Screen.PrimaryScreen.WorkingArea.Height)
            {
                point.Y = -this._controlHost.Height - 7;
            }
            return point;
        }

        private void filterTextBox1_TextChanged(object sender, EventArgs e)
        {
            FilterTextChangedEventHandler?.Invoke(sender, e);
        }

        private void Resize(object sender, EventArgs e)
        {
            filterTextBox.Width = this.Width;
            _listBox.Width = this.Width;
            DropDownWidth = this.Width;
            IsDroppedDown = false;
        }

        #region Overrides

        protected override void OnDataSourceChanged(EventArgs e)
        {
            this.SelectedIndex = 0;
            base.OnDataSourceChanged(e);
        }

        protected override void OnDisplayMemberChanged(EventArgs e)
        {
            _listBox.DisplayMember = this.DisplayMember;
            this.SelectedIndex = this.SelectedIndex;
            base.OnDisplayMemberChanged(e);
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            e.Control.MouseDown += new MouseEventHandler(Control_MouseDown);
            e.Control.MouseEnter += new EventHandler(Control_MouseEnter);
            e.Control.MouseLeave += new EventHandler(Control_MouseLeave);
            e.Control.GotFocus += new EventHandler(Control_GotFocus);
            e.Control.LostFocus += new EventHandler(Control_LostFocus);
            base.OnControlAdded(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            filterTextBox.Focus();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {

            if (_listBox.Items.Count < 1)
            {
                return;
            }

            if (e.Delta > 0)
            {
                if (_listBox.SelectedIndex > 0)
                {
                    _listBox.SelectedIndex -= 1;
                }
                else
                {
                    _listBox.SelectedIndex = _listBox.Items.Count - 1;
                }
            }

            if (e.Delta < 0)
            {
                if (_listBox.SelectedIndex < _listBox.Items.Count - 1)
                {
                    _listBox.SelectedIndex += 1;
                }
                else
                {
                    _listBox.SelectedIndex = 0;
                }
            }

            base.OnMouseWheel(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            if (!this.ContainsFocus)
            {
                if (!(_listBox.SelectedItem is null) && _isDroppedDown)
                {
                    int i;
                    for (i = 0; i < _listBox.Items.Count; i++)
                    {
                        if (_listBox.GetItemRectangle(i).Contains(_listBox.PointToClient(MousePosition)))
                        {
                            filterTextBox.Text = _listBox.SelectedItem.ToString();
                            filterTextBox.SelectionStart = filterTextBox.Text.Length;
                        }
                    }
                }

                _isDroppedDown = false;
                _popupControl.Close();

                Invalidate();
            }

            base.OnLostFocus(e);
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            SelectedIndexChanged?.Invoke(this, e);

            base.OnSelectedIndexChanged(e);
        }

        protected override void OnValueMemberChanged(EventArgs e)
        {
            _listBox.ValueMember = this.ValueMember;
            this.SelectedIndex = this.SelectedIndex;
            base.OnValueMemberChanged(e);
        }

        protected override void OnResize(EventArgs e)
        {
            if (DesignMode)
            {
                DropDownWidth = this.Width;
            }
        }

        protected override void RefreshItem(int index) => throw new NotImplementedException();
        protected override void SetItemsCore(IList items) => throw new NotImplementedException();

        public override string Text
        {
            get
            {
                return filterTextBox.Text;
            }
            set
            {
                filterTextBox.Text = value;
                base.Text = filterTextBox.Text;
                OnTextChanged(EventArgs.Empty);
            }
        }

        public override int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (_listBox != null)
                {
                    if (_listBox.Items.Count == 0)
                    {
                        return;
                    }

                    if ((this.DataSource != null) && value == -1)
                    {
                        return;
                    }

                    if (value <= (_listBox.Items.Count - 1) && value >= -1)
                    {
                        _listBox.SelectedIndex = value;
                        _selectedIndex = value;
                        filterTextBox.Text = _listBox.GetItemText(_listBox.SelectedItem);
                        OnSelectedIndexChanged(EventArgs.Empty);
                    }
                }
            }
        }
        #endregion
    }
}
