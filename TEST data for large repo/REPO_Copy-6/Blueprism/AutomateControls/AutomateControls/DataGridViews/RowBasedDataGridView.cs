using System;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Linq;

namespace AutomateControls.DataGridViews
{
    /// <summary>
    /// Class which implements a DataGridView with some reasonable defaults set which
    /// allow primarily row-based access to the data.
    /// Effectively, a more flexible listview, removing a lot of the cell-based
    /// access and row header / error details.
    /// </summary>
    public class RowBasedDataGridView : DataGridView
    {
        /// <summary>
        /// First row this isnt read only
        /// </summary>
        private int _firstRowNotReadonly => this.Rows.GetFirstRow(DataGridViewElementStates.None, DataGridViewElementStates.ReadOnly);

        /// <summary>
        /// Previously selected row's index. Used within the skip disabled rows functionality
        /// </summary>
        private int _previouslySelectedRowIndex { get ; set; }

        /// <summary>
        /// The direction of which the user has navigated the grid using the arrow keys. 
        /// Used within the skip disabled rows functionality
        /// </summary>
        private DataGridViewNavigationDirection _dataGridViewNavigationDirection = DataGridViewNavigationDirection.None;

        /// <summary>
        /// Event fired when the selected row is changed
        /// </summary>
        public event EventHandler SelectedRowChanged;

        /// <summary>
        /// Creates a new empty row based data grid view.
        /// </summary>
        public RowBasedDataGridView()
        {
            BackgroundColor = SystemColors.Window;
            RowHeadersVisible = false;
            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            AllowUserToResizeRows = false;
            ShowEditingIcon = false;
            ShowRowErrors = false;
            ShowCellErrors = false;
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            DoubleBuffered = true;
        }

        /// <summary>
        /// Gets or sets the background color of the control.
        /// </summary>
        /// <remarks>This is really only here to set a new default value of
        /// <see cref="SystemColors.Window"/> for controls of this type.</remarks>
        [DefaultValue(typeof(Color), "Window")]
        public new Color BackgroundColor
        {
            get { return base.BackgroundColor; }
            set { base.BackgroundColor = value; }
        }

        /// <summary>
        /// Gets or sets whether row headers should be visible in this data grid view
        /// </summary>
        /// <remarks>This is really only here to set a new default value of
        /// false for controls of this type.</remarks>
        [DefaultValue(false)]
        public new bool RowHeadersVisible
        {
            get { return base.RowHeadersVisible; }
            set { base.RowHeadersVisible = value; }
        }

        /// <summary>
        /// Gets or sets whether users are allowed to add rows in this control
        /// </summary>
        /// <remarks>This is really only here to set a new default value of
        /// false for controls of this type.</remarks>
        [DefaultValue(false)]
        public new bool AllowUserToAddRows
        {
            get { return base.AllowUserToAddRows; }
            set { base.AllowUserToAddRows = value; }
        }

        /// <summary>
        /// Gets or sets whether users are allowed to delete rows in this control
        /// </summary>
        /// <remarks>This is really only here to set a new default value of
        /// false for controls of this type.</remarks>
        [DefaultValue(false)]
        public new bool AllowUserToDeleteRows
        {
            get { return base.AllowUserToDeleteRows; }
            set { base.AllowUserToDeleteRows = value; }
        }

        /// <summary>
        /// Gets or sets whether users are allowed to resize rows in this control
        /// </summary>
        /// <remarks>This is really only here to set a new default value of
        /// false for controls of this type.</remarks>
        [DefaultValue(false)]
        public new bool AllowUserToResizeRows
        {
            get { return base.AllowUserToResizeRows; }
            set { base.AllowUserToResizeRows = value; }
        }

        /// <summary>
        /// Gets or sets whether the 'editing' icon is visible in this control
        /// </summary>
        /// <remarks>This is really only here to set a new default value of
        /// false for controls of this type.</remarks>
        [DefaultValue(false)]
        public new bool ShowEditingIcon
        {
            get { return base.ShowEditingIcon; }
            set { base.ShowEditingIcon = value; }
        }

        /// <summary>
        /// Gets or sets whether row errors are shown
        /// </summary>
        /// <remarks>This is really only here to set a new default value of
        /// false for controls of this type.</remarks>
        [DefaultValue(false)]
        public new bool ShowRowErrors
        {
            get { return base.ShowRowErrors; }
            set { base.ShowRowErrors = value; }
        }

        /// <summary>
        /// Gets or sets whether cell errors are shown
        /// </summary>
        /// <remarks>This is really only here to set a new default value of
        /// false for controls of this type.</remarks>
        [DefaultValue(false)]
        public new bool ShowCellErrors
        {
            get { return base.ShowCellErrors; }
            set { base.ShowCellErrors = value; }
        }

        /// <summary>
        /// Gets or sets the selection mode for this control
        /// </summary>
        /// <remarks>This primarily sets a default selection mode of
        /// <see cref="DataGridViewSelectionMode.FullRowSelect"/> for controls of
        /// this type, but also hides it from the designer and editor (ie.
        /// Intellisense).</remarks>
        [DefaultValue(DataGridViewSelectionMode.FullRowSelect),
         Browsable(false), EditorBrowsable(EditorBrowsableState.Never),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new DataGridViewSelectionMode SelectionMode
        {
            get { return base.SelectionMode; }
            set { base.SelectionMode = value; }
        }

        /// <summary>
        /// Gets or sets the cell border style for this control
        /// </summary>
        /// <remarks>This is really only here to set a new default value of
        /// <see cref="DataGridViewCellBorderStyle.SingleHorizontal"/> for controls
        /// of this type.</remarks>
        [DefaultValue(DataGridViewCellBorderStyle.SingleHorizontal)]
        public new DataGridViewCellBorderStyle CellBorderStyle
        {
            get { return base.CellBorderStyle; }
            set { base.CellBorderStyle = value; }
        }

        /// <summary>
        /// Gets or sets whether this control should be double buffered or not
        /// </summary>
        /// <remarks>This is really only here to set a new default value of
        /// true for controls of this type.</remarks>
        [DefaultValue(true)]
        protected override bool DoubleBuffered
        {
            get { return base.DoubleBuffered; }
            set { base.DoubleBuffered = value; }
        }

        /// <summary>
        /// Gets and sets whether this control should prevent the highlighting
        /// of ReadOnly rows, by clicking or navigating using arrow keys
        /// </summary>
        [DefaultValue(false)]
        public bool SkipReadOnlyRows { get; set; }

        /// <summary>
        /// Gets whether this data grid view is still set at the (default) mode of
        /// full row select.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected bool IsFullRowSelect
        {
            get { return (SelectionMode == DataGridViewSelectionMode.FullRowSelect); }
        }

        /// <summary>
        /// Gets or sets the selected row in this data grid view. Setting with a
        /// value of null effectively clears the selection.
        /// If getting the selected row and a row is currently set as the
        /// <see cref="CurrentRow"/>, this will return that as the selected row;
        /// otherwise it will return the first in the current set of
        /// <see cref="SelectedRows"/> or null if there is no selection.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataGridViewRow SelectedRow
        {
            get
            {
                var row = CurrentRow;
                if (row != null && row.Selected) return row;

                var rows = SelectedRows;
                if (rows.Count > 0) return rows[0];

                return null;
            }
            set
            {
                if (value == null)
                {
                    ClearSelection();
                }
                else
                {
                    ClearSelection(IsFullRowSelect ? -1 : 0, value.Index, true);
                    // That handles the selection, but the current cell doesn't
                    // follow it - we need to make it do so
                    var cell = CurrentCell;
                    int colInd = (cell == null ? 0 : cell.ColumnIndex);
                    CurrentCell = Rows.SharedRow(value.Index).Cells[colInd];
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="CurrentCellChanged"/> event, and ensures that the
        /// selected row change is also noted.
        /// </summary>
        protected override void OnCurrentCellChanged(EventArgs e)
        {
            base.OnCurrentCellChanged(e);
            OnSelectedRowChanged(e);
        }

        /// <summary>
        /// Raises the <see cref="SelectedRowChanged"/> event.
        /// </summary>
        protected virtual void OnSelectedRowChanged(EventArgs e)
        {
            EventHandler h = this.SelectedRowChanged;
            if (h != null)
                h(this, e);

            if (SkipReadOnlyRows)
            {
                if (Rows.Cast<DataGridViewRow>().All(row => row.ReadOnly))
                    ClearSelectedRowAndCurrentCell();
                else if (SelectedRow?.ReadOnly == true)
                {
                    if (_dataGridViewNavigationDirection != DataGridViewNavigationDirection.None)
                        SkipReadOnlyRow();
                    else
                        ClearSelectedRowAndCurrentCell();
                }
                else
                    _previouslySelectedRowIndex = SelectedRow?.Index ?? _firstRowNotReadonly;

                _dataGridViewNavigationDirection = DataGridViewNavigationDirection.None;
            }
        }

        /// <summary>
        /// Handles a scroll event occurring in this data grid view.
        /// I've noticed some artefacts being output when scrolling horizontally -
        /// especially in the column headers, but there's no (simple) way to just
        /// invalidate the column headers, so I invalidate it all.
        /// Also, scrolling down - if you keep hitting the scroll down button, you
        /// end up with the last element shuffling up the screen leaving ghost images
        /// behind in the remaining space. This mitigates that, to a certain extent,
        /// though there's something not quite right there on selection.
        /// </summary>
        protected override void OnScroll(ScrollEventArgs e)
        {
            base.OnScroll(e);
            Invalidate();
        }

        /// <summary>
        /// Like the handling of the scroll event, this is a temporary solution to
        /// oddities occurring when the data grid view has scrolled to the end of its
        /// contents and selections are made using the keyboard. It's not ideal -
        /// it's doing quite a lot of repainting work where a lot less is necessary;
        /// however, it removes the artefacts and is reasonably performant in my
        /// reasonably straightforward tests.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSelectionChanged(EventArgs e)
        {
            base.OnSelectionChanged(e);
            Invalidate();
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (this.SkipReadOnlyRows)
                SetNavigateDataGridDirection();

            base.OnKeyDown(e);

            void SetNavigateDataGridDirection()
            {
                if (e.KeyCode == Keys.Down)
                    _dataGridViewNavigationDirection = DataGridViewNavigationDirection.Down;
                else if (e.KeyCode == Keys.Up)
                    _dataGridViewNavigationDirection = DataGridViewNavigationDirection.Up;
            }
        }

        delegate void SetCellIndex(int i);
        delegate void ClearCell();
        
        private void SkipReadOnlyRow()
        {
            var moveToRowIndex = GetRowMoveTo();

            _previouslySelectedRowIndex = moveToRowIndex;

            this.Rows[moveToRowIndex].Selected = true;
            var method = new SetCellIndex(MoveCells);

            this.BeginInvoke(method, moveToRowIndex);

            int GetRowMoveTo()
            {
                var moveToRowAndCellIndex = 0;

                if (_dataGridViewNavigationDirection == DataGridViewNavigationDirection.Down)
                {
                    var nextRowIndex = this.Rows.GetNextRow(this.SelectedRow.Index, DataGridViewElementStates.None, DataGridViewElementStates.ReadOnly);
                    moveToRowAndCellIndex = nextRowIndex == -1 ? _previouslySelectedRowIndex : nextRowIndex;
                }
                else if (_dataGridViewNavigationDirection == DataGridViewNavigationDirection.Up)
                {
                    var nextRowIndex = this.Rows.GetPreviousRow(this.SelectedRow.Index, DataGridViewElementStates.None, DataGridViewElementStates.ReadOnly);
                    moveToRowAndCellIndex = nextRowIndex == -1 ? _previouslySelectedRowIndex : nextRowIndex;
                }
               
                // before first available row
                if (moveToRowAndCellIndex < _firstRowNotReadonly) 
                    moveToRowAndCellIndex = _firstRowNotReadonly;
               
                //past last available row
                if(moveToRowAndCellIndex >= this.Rows.Count)
                    moveToRowAndCellIndex = _previouslySelectedRowIndex;

                return moveToRowAndCellIndex;
            }
        }
        private void MoveCells(int rowIndex)
        {
            this.CurrentCell = this.Rows[rowIndex].Cells[0];
        }
        private void ClearSelectedRowAndCurrentCell()
        {
            ClearSelection();
            var method = new ClearCell(ClearSelectedCell);
            this.BeginInvoke(method);
        }
        private void ClearSelectedCell()
        {
            this.CurrentCell = null;
        }
    }
}
