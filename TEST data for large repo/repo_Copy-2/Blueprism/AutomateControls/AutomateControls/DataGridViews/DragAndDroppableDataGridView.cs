using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BluePrism.BPCoreLib.Collections;

namespace AutomateControls.DataGridViews
{
    public delegate void DataGridViewRowDragEventHandler(
        object sender, DataGridViewRowDragEventArgs e);

    public class DataGridViewRowDragEventArgs : EventArgs
    {
        private ICollection<DataGridViewRow> _rows;
        public DataGridViewRowDragEventArgs(DataGridViewRow row)
            : this(GetSingleton.ICollection(row)) {}

        public DataGridViewRowDragEventArgs(ICollection<DataGridViewRow> rows)
        {
            _rows = GetReadOnly.ICollection(rows);
        }

        public ICollection<DataGridViewRow> Rows { get { return _rows; } }
    }

    public class DragAndDroppableDataGridView : RowBasedDataGridView
    {
        private Rectangle dragBoxFromMouseDown;

        private int rowIndexFromMouseDown;

        private int rowIndexOfItemUnderMouseToDrop;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                // If the mouse moves outside the rectangle, start the drag.
                if (dragBoxFromMouseDown != Rectangle.Empty &&
                    !dragBoxFromMouseDown.Contains(e.X, e.Y))
                {
                    // Proceed with the drag and drop, passing in the list item.                   
                    DragDropEffects dropEffect = DoDragDrop(
                             Rows[rowIndexFromMouseDown],
                             DragDropEffects.Move);
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // Get the index of the item the mouse is below.
            rowIndexFromMouseDown = HitTest(e.X, e.Y).RowIndex;

            if (rowIndexFromMouseDown != -1)
            {
                // Remember the point where the mouse down occurred.
                // The DragSize indicates the size that the mouse can move
                // before a drag event should be started.               
                Size dragSize = SystemInformation.DragSize;

                // Create a rectangle using the DragSize, with the mouse position being
                // at the center of the rectangle.
                dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2),
                                                               e.Y - (dragSize.Height / 2)),
                                                     dragSize);
            }
            else
            {
                // Reset the rectangle if the mouse is not over an item in the ListBox.
                dragBoxFromMouseDown = Rectangle.Empty;
            }
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            // The mouse locations are relative to the screen, so they must be
            // converted to client coordinates.
            Point clientPoint = PointToClient(new Point(e.X, e.Y));

            // Get the row index of the item the mouse is below.
            rowIndexOfItemUnderMouseToDrop =
                HitTest(clientPoint.X, clientPoint.Y).RowIndex;

            // If the drag operation was a move then remove and insert the row.
            if (e.Effect == DragDropEffects.Move)
            {
                DataGridViewRow rowToMove = e.Data.GetData(
                             typeof(DataGridViewRow)) as DataGridViewRow;
                Rows.RemoveAt(rowIndexFromMouseDown);
                Rows.Insert(rowIndexOfItemUnderMouseToDrop, rowToMove);
            }
        }
    }
}
