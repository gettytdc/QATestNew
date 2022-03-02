using System;
using System.Collections;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.Serialization;

namespace AutomateControls
{
    /// <summary>
    /// A treelist control provides a treeview with multiple rows,
    /// or a listview with nested rows in a tree structure (whichever
    /// way you choose to think about it).
    /// </summary>
    public class TreeList : AutomateListView
    {

        private StringFormat string_format;

        public TreeList()
        {
            //
            // TODO: Add constructor logic here
            //
            this.View = View.Details;
            this.FullRowSelect = true;
            this.GridLines = true;
            this.root_node = new TreeListItem (this);

            string_format = new StringFormat ();
            string_format.LineAlignment = StringAlignment.Center;
            string_format.Alignment = StringAlignment.Center;

            this.items = new TreeListItemCollection(this);
        }

        
        internal int hbar_offset;

        protected override void LayoutDetails ()
        {
            if (columns.Count == 0) 
            {
                header_control.Visible = false;
                item_control.Visible = false;
                return;
            }

            LayoutHeader ();

            item_control.Visible = true;
            item_control.Location = new Point (0, header_control.Height);

            int y = 0; 
            if (this.ChildItems.Count > 0) 
            {
                OpenTreeListItemEnumerator walk = new OpenTreeListItemEnumerator(this.ChildItems[0]);
                    
                while (walk.MoveNext())
                {
                    walk.CurrentItem.Layout ();
                    walk.CurrentItem.Location = new Point (0, y);
                    //Hack
                    y += walk.CurrentItem.Bounds.Height;
//                  y += walk.CurrentItem.Bounds.Height + 2;
                }

                // some space for bottom gridline
                if (grid_lines)
                    y += 2;
            }

            layout_wd = Math.Max (header_control.Width, item_control.Width);
            layout_ht = y + header_control.Height;
        }

        // Returns the size of biggest item text in a column.
        protected override Size BiggestItem (int col)
        {
            Size temp = Size.Empty;
            Size ret_size = Size.Empty;

            // 0th column holds the item text, we check the size of
            // the various subitems falling in that column and get
            // the biggest one's size.
            OpenTreeListItemEnumerator walk = new OpenTreeListItemEnumerator(this.ChildItems[0]);
            while (walk.MoveNext())
            {
                if (col >= walk.CurrentItem.SubItems.Count)
                    continue;
                //HACK
                Graphics DeviceContext = this.CreateGraphics ();

                temp = Size.Ceiling (DeviceContext.MeasureString
                    (walk.CurrentItem.SubItems [col].Text, this.Font));
                if (temp.Width > ret_size.Width)
                    ret_size = temp;
            }

            // adjustmenft for space
            if (!ret_size.IsEmpty)
                ret_size.Width += 4;

            return ret_size;
        }


        protected override void CalcTextSize ()
        {           
            // clear the old value
            text_size = Size.Empty;

            if (ChildItems.Count == 0)
                return;

            text_size = BiggestItem (0);

            if (view == View.LargeIcon && this.label_wrap) 
            {
                Size temp = Size.Empty;
                if (this.check_boxes)
                    temp.Width += 2 * this.CheckBoxSize.Width;
                if (large_image_list != null)
                    temp.Width += large_image_list.ImageSize.Width;
                if (temp.Width == 0)
                    temp.Width = 43;
                // wrapping is done for two lines only
                if (text_size.Width > temp.Width) 
                {
                    text_size.Width = temp.Width;
                    text_size.Height *= 2;
                }
            }
            else if (view == View.List) 
            {
                // in list view max text shown in determined by the
                // control width, even if scolling is enabled.
                int max_wd = this.Width - (this.CheckBoxSize.Width - 2);
                if (this.small_image_list != null)
                    max_wd -= this.small_image_list.ImageSize.Width;

                if (text_size.Width > max_wd)
                    text_size.Width = max_wd;
            }

            // we do the default settings, if we have got 0's
            if (text_size.Height <= 0)
                text_size.Height = this.Font.Height;
            if (text_size.Width <= 0)
                text_size.Width = this.Width;

            // little adjustment
            text_size.Width += 4;
            text_size.Height += 2;
        }

        #region "TreeListItem Subclass"


        public class TreeListItem : AutomateListViewItem
        {

            /// <summary>
            /// The subnodes of this one.
            /// </summary>
            private TreeListItemCollection m_NodeCollection;

            /// <summary>
            /// The parent node/item.
            /// </summary>
            internal TreeListItem parent;

            /// <summary>
            /// Whether or not the current node is expanded
            /// to show children
            /// </summary>
            internal bool is_expanded = false;


            internal TreeList owner;

            #region "Constructors"

            public TreeListItem (TreeList owner) : base ()
            {
                this.owner = owner;
                this.m_NodeCollection = new TreeListItemCollection (this.owner);
            }

            public TreeListItem (string text) : this (text, -1)
            {
            }

            public TreeListItem (string [] items) : this (items, -1)
            {
            }

            public TreeListItem (AutomateListViewItem.ListViewSubItem [] subItems, int imageIndex)
            {
                this.sub_items = new ListViewSubItemCollection (this);
                this.sub_items.AddRange (subItems);
                this.image_index = imageIndex;
                this.m_NodeCollection = new TreeListItemCollection (this.owner);
            }

            public TreeListItem (string text, int imageIndex)
            {
                this.image_index = imageIndex;
                this.sub_items = new ListViewSubItemCollection (this);
                this.sub_items.Add (text);
                this.m_NodeCollection = new TreeListItemCollection (this.owner);
            }

            public TreeListItem (string [] items, int imageIndex)
            {
                this.sub_items = new ListViewSubItemCollection (this);
                this.sub_items.AddRange (items);
                this.image_index = imageIndex;
                this.m_NodeCollection = new TreeListItemCollection (this.owner);
            }

            public TreeListItem (string [] items, int imageIndex, Color foreColor, 
                Color backColor, Font font)
            {
                this.sub_items = new ListViewSubItemCollection (this);
                this.sub_items.AddRange (items);
                this.image_index = imageIndex;
                this.ForeColor = foreColor;
                this.BackColor = backColor;
                this.Font = font;
                this.m_NodeCollection = new TreeListItemCollection (this.owner);
            }

            #endregion

            public TreeListItem Parent 
            {
                get 
                {
                    TreeList tree_list = TreeList;
                    if (tree_list != null && tree_list.root_node == parent)
                        return null;
                    return parent;
                }
            }


            internal int GetLinesX ()
            {
                int roots = 1;
                //PJW h_marker
                //return (IndentLevel + roots) * TreeList.indent - TreeList.hbar_offset;
                return (IndentLevel + roots) * TreeList.indent - owner.h_marker;

            }


            public TreeListItemCollection ChildItems 
            {
                get { return this.m_NodeCollection; }
            }


            internal int IndentLevel 
            {
                get 
                {
                    TreeListItem walk = this;
                    int res = 0;
                    while (walk.Parent != null) 
                    {
                        walk = walk.Parent;
                        res++;
                    }

                    return res;
                }
            }

            public TreeListItem NextItem 
            {
                get 
                {
                    if (parent == null)
                    {
                        if (owner == null)
                        {
                            return null;
                        }
                        else
                        {
                            int index = Index;
                            if (owner.ChildItems.Count > index + 1)
                                return owner.ChildItems [index + 1];
                            return null;
                        }
                    }
                    else
                    {
                        int index = Index;
                        if (parent.ChildItems.Count > index + 1)
                            return parent.ChildItems [index + 1];
                        return null;
                    }

                }
            }
            

            
            internal override void Layout ()
            {
                int item_ht;
                Rectangle total;
                Size text_size = owner.text_size;
            
                switch (owner.View) 
                {
                    case View.Details:
                        // LAMESPEC: MSDN says, "In all views except the details
                        // view of the AutomateListView, this value specifies the same
                        // bounding rectangle as the Entire value." Actually, it
                        // returns same bounding rectangles for Item and Entire
                        // values in the case of Details view.

                        icon_rect = label_rect = Rectangle.Empty;
                        icon_rect.X = checkbox_rect.Width + 2;
                        item_ht = Math.Max (owner.CheckBoxSize.Height, text_size.Height);

                        if (owner.SmallImageList != null) 
                        {
                            item_ht = Math.Max (item_ht, owner.SmallImageList.ImageSize.Height);
                            icon_rect.Width = owner.SmallImageList.ImageSize.Width;
                        }

                        label_rect.Height = icon_rect.Height = item_ht;
                        checkbox_rect.Y = item_ht - checkbox_rect.Height;

                        label_rect.X = icon_rect.Right + 1;

                        if (owner.Columns.Count > 0)
                            label_rect.Width = Math.Max (text_size.Width, owner.Columns[0].Wd);
                        else
                            label_rect.Width = text_size.Width;

                        item_rect = total = Rectangle.Union
                            (Rectangle.Union (checkbox_rect, icon_rect), label_rect);
                        bounds.Size = total.Size;

                        // Take into account the rest of columns. First column
                        // is already taken into account above.
                        for (int i = 1; i < owner.Columns.Count; i++) 
                        {
                            item_rect.Width += owner.Columns [i].Wd;
                            bounds.Width += owner.Columns [i].Wd;
                        }
                        break;

//                  case View.LargeIcon:
//                      label_rect = icon_rect = Rectangle.Empty;
//
//                      if (owner.LargeImageList != null) 
//                      {
//                          icon_rect.Width = owner.LargeImageList.ImageSize.Width;
//                          icon_rect.Height = owner.LargeImageList.ImageSize.Height;
//                      }
//
//                      if (checkbox_rect.Height > icon_rect.Height)
//                          icon_rect.Y = checkbox_rect.Height - icon_rect.Height;
//                      else
//                          checkbox_rect.Y = icon_rect.Height - checkbox_rect.Height;
//
//
//                      if (text_size.Width <= icon_rect.Width) 
//                      {
//                          icon_rect.X = checkbox_rect.Width + 1;
//                          label_rect.X = icon_rect.X + (icon_rect.Width - text_size.Width) / 2;
//                          label_rect.Y = icon_rect.Bottom + 2;
//                          label_rect.Size = text_size;
//                      } 
//                      else 
//                      {
//                          int centerX = text_size.Width / 2;
//                          icon_rect.X = checkbox_rect.Width + 1 + centerX - icon_rect.Width / 2;
//                          label_rect.X = checkbox_rect.Width + 1;
//                          label_rect.Y = icon_rect.Bottom + 2;
//                          label_rect.Size = text_size;
//                      }
//
//                      item_rect = Rectangle.Union (icon_rect, label_rect);
//                      total = Rectangle.Union (item_rect, checkbox_rect);
//                      bounds.Size = total.Size;
//                      break;
//
//                  case View.List:
//                  case View.SmallIcon:
//                      label_rect = icon_rect = Rectangle.Empty;
//                      icon_rect.X = checkbox_rect.Width + 1;
//                      item_ht = Math.Max (owner.CheckBoxSize.Height, text_size.Height);
//
//                      if (owner.SmallImageList != null) 
//                      {
//                          item_ht = Math.Max (item_ht, owner.SmallImageList.ImageSize.Height);
//                          icon_rect.Width = owner.SmallImageList.ImageSize.Width;
//                          icon_rect.Height = owner.SmallImageList.ImageSize.Height;
//                      }
//
//                      checkbox_rect.Y = item_ht - checkbox_rect.Height;
//                      label_rect.X = icon_rect.Right + 1;
//                      label_rect.Width = text_size.Width;
//                      label_rect.Height = icon_rect.Height = item_ht;
//
//                      item_rect = Rectangle.Union (icon_rect, label_rect);
//                      total = Rectangle.Union (item_rect, checkbox_rect);
//                      bounds.Size = total.Size;
//                      break;
                }
            
            }
            
            public override Rectangle GetBounds (ItemBoundsPortion portion)
            {
                if (owner == null)
                    return Rectangle.Empty;
                
                Rectangle rect;

                switch (portion) 
                {
                    case ItemBoundsPortion.Icon:
                        rect = icon_rect;
                        break;

                    case ItemBoundsPortion.Label:
                        rect = label_rect;
                        break;

                    case ItemBoundsPortion.ItemOnly:
                        rect = item_rect;
                        break;

                    case ItemBoundsPortion.Entire:
                        rect = bounds;
                        rect.X -= owner.h_marker;
                        rect.Y -= owner.v_marker;
                        return rect;                

                    default:
                        throw new ArgumentException ("Invalid value for portion.");
                }

                rect.X += bounds.X - owner.h_marker;
                rect.Y += bounds.Y - owner.v_marker;
                return rect;
            }

            public override int Index
            {
                get 
                {
                    if (parent == null)
                    {
                        if (owner == null)
                        {
                            return -1;
                        }
                        else
                        {
                            return owner.ChildItems.IndexOf(this);
                        }
                    }
                    else
                    {
                        return parent.ChildItems.IndexOf(this);
                    }
                }
            }
            
            

                public TreeListItem LastItem 
            {
                get 
                {
                    return (ChildItems == null || ChildItems.Count == 0) ? null : ChildItems [ChildItems.Count - 1];
                }
            }

            public TreeListItem PrevItem 
            {
                get 
                {
                    if (parent == null)
                        if (owner == null)
                        {
                            return null;
                        }
                        else
                        {
                            int index = Index;
                            if (index <= 0 || index > owner.ChildItems.Count)
                                return null;
                            return owner.ChildItems [index - 1];
                        }
                    else
                    {
                        int index = Index;
                        if (index <= 0 || index > parent.ChildItems.Count)
                            return null;
                        return parent.ChildItems [index - 1];
                    }
                }
            }

                    

            

            public bool IsExpanded 
            {
                get { return is_expanded; }
            }

            public void Toggle () 
            {
                if (is_expanded)
                    Collapse ();
                else
                    Expand ();
            }

            public void Expand () 
            {
                Expand(false);
            }

            private void Expand (bool byInternal)
            {
                if (is_expanded || ChildItems.Count < 1)
                    return;
                
                owner.Redraw(true);
            }

            public void Collapse () 
            {
                Collapse(false);
            }

            private void Collapse (bool byInternal)
            {
                if (!is_expanded || ChildItems.Count < 1)
                    return;

                if (IsRoot)
                    return;

                owner.Redraw(true);
            }

            internal bool IsRoot 
            {
                get 
                {
                    if (owner == null)
                        return false;
                    if (owner.root_node == this)
                        return true;
                    return false;
                }
            }

            internal override Rectangle CheckRectReal 
            {
                get 
                {
                    Rectangle rect = checkbox_rect;
                    rect.X += bounds.X - owner.h_marker;
                    rect.Y += bounds.Y - owner.v_marker;
                    return rect;
                }
            }

            public TreeList TreeList
            {
                get 
                {
                    return owner;
                }
            }

            internal int GetY ()
            {
                if (TreeList == null)
                    return 0;
                //return (visible_order - 1) * TreeList.ItemHeight - (TreeList.skipped_nodes * TreeList.SmallIconItemSize.Height);
                return (visible_order) * TreeList.ItemHeight - (TreeList.skipped_nodes * TreeList.SmallIconItemSize.Height);
            }

            internal int visible_order;

        }



        #endregion



        internal int skipped_nodes;


        #region "TreeListItemCollection Class"
        public class TreeListItemCollection : IList, ICollection, IEnumerable 
        {
            internal ArrayList list;
            internal TreeList owner;

            #region Public Constructor
            public TreeListItemCollection (TreeList owner)
            {
                list = new ArrayList ();
                this.owner = owner;
            }
            #endregion  // Public Constructor

            #region Public Properties
            [Browsable (false)]
            public int Count 
            {
                get { return list.Count; }
            }

            public bool IsReadOnly 
            {
                get { return false; }
            }

            public virtual TreeListItem this [int displayIndex] 
            {
                get 
                {
                    if (displayIndex < 0 || displayIndex >= list.Count)
                        throw new ArgumentOutOfRangeException ("displayIndex");
                    return (TreeListItem) list [displayIndex];
                }

                set 
                {
                    if (displayIndex < 0 || displayIndex >= list.Count)
                        throw new ArgumentOutOfRangeException ("displayIndex");

                    if (list.Contains (value))
                        throw new ArgumentException ("An item cannot be added more than once. To add an item again, you need to clone it.", "value");

                    value.Owner = owner;
                    list [displayIndex] = value;

                    owner.Redraw (true);
                }
            }

            bool ICollection.IsSynchronized 
            {
                get { return true; }
            }

            object ICollection.SyncRoot 
            {
                get { return this; }
            }

            bool IList.IsFixedSize 
            {
                get { return list.IsFixedSize; }
            }

            object IList.this [int index] 
            {
                get { return this [index]; }
                set 
                {
                    if (value is TreeListItem)
                        this [index] = (TreeListItem) value;
                    else
                        this [index] = new TreeListItem (value.ToString ());
                }
            }
            #endregion  // Public Properties

            #region Public Methods
            public virtual TreeListItem Add (TreeListItem value)
            {
                if (list.Contains (value))
                    throw new ArgumentException ("An item cannot be added more than once. To add an item again, you need to clone it.", "value");

                value.owner = owner;
                list.Add (value);

                
                if (owner.Sorting != SortOrder.None)
                    owner.Sort ();

                owner.Redraw (true);

                return value;
            }

            public virtual TreeListItem Add (string text)
            {
                TreeListItem item = new TreeListItem (text);
                item.owner = this.owner;
                return this.Add (item);
            }

            public virtual TreeListItem Add (string text, int imageIndex)
            {
                TreeListItem item = new TreeListItem (text, imageIndex);
                item.owner = this.owner;
                return this.Add (item);
            }

            public void AddRange (TreeListItem [] values)
            {
                list.Clear ();
                owner.SelectedItems.list.Clear ();
                owner.SelectedIndices.list.Clear ();
                owner.CheckedItems.list.Clear ();
                owner.CheckedIndices.list.Clear ();

                foreach (TreeListItem item in values) 
                {
                    item.Owner = owner;
                    item.owner = this.owner;
                    list.Add (item);
                }

                if (owner.Sorting != SortOrder.None)
                    owner.Sort ();

                owner.Redraw (true);
            }

            public virtual void Clear ()
            {
                owner.SetFocusedItem (null);
                owner.h_scroll.Value = owner.v_scroll.Value = 0;
                list.Clear ();
                owner.SelectedItems.list.Clear ();
                owner.SelectedIndices.list.Clear ();
                owner.CheckedItems.list.Clear ();
                owner.CheckedIndices.list.Clear ();
                owner.Redraw (true);
            }

            public bool Contains (TreeListItem item)
            {
                return list.Contains (item);
            }

            public void CopyTo (Array dest, int index)
            {
                list.CopyTo (dest, index);
            }

            public IEnumerator GetEnumerator ()
            {
                return list.GetEnumerator ();
            }

            int IList.Add (object item)
            {
                int result;
                TreeListItem li;

                if (item is TreeListItem) 
                {
                    li = (TreeListItem) item;
                    if (list.Contains (li))
                        throw new ArgumentException ("An item cannot be added more than once. To add an item again, you need to clone it.", "item");
                }
                else
                    li = new TreeListItem (item.ToString ());

                li.Owner = owner;
                result = list.Add (li);
                owner.Redraw (true);

                return result;
            }

            bool IList.Contains (object item)
            {
                return list.Contains (item);
            }

            int IList.IndexOf (object item)
            {
                return list.IndexOf (item);
            }

            void IList.Insert (int index, object item)
            {
                if (item is TreeListItem)
                    this.Insert (index, (TreeListItem) item);
                else
                    this.Insert (index, item.ToString ());
            }

            void IList.Remove (object item)
            {
                Remove ((TreeListItem) item);
            }

            public int IndexOf (TreeListItem item)
            {
                return list.IndexOf (item);
            }

            public TreeListItem Insert (int index, TreeListItem item)
            {
                // LAMESPEC: MSDOCS say greater than or equal to the value of the Count property
                // but it's really only greater.
                if (index < 0 || index > list.Count)
                    throw new ArgumentOutOfRangeException ("index");

                if (list.Contains (item))
                    throw new ArgumentException ("An item cannot be added more than once. To add an item again, you need to clone it.", "item");

                item.Owner = owner;
                list.Insert (index, item);
                owner.Redraw (true);
                return item;
            }

            public TreeListItem Insert (int index, string text)
            {
                return this.Insert (index, new TreeListItem (text));
            }

            public TreeListItem Insert (int index, string text, int imageIndex)
            {
                return this.Insert (index, new TreeListItem (text, imageIndex));
            }

            public virtual void Remove (TreeListItem item)
            {
                if (!list.Contains (item))
                    return;
                    
                owner.SelectedItems.list.Remove (item);
                owner.SelectedIndices.list.Remove (item.Index);
                owner.CheckedItems.list.Remove (item);
                owner.CheckedIndices.list.Remove (item.Index);
                list.Remove (item);
                owner.Redraw (true);                
            }

            public virtual void RemoveAt (int index)
            {
                if (index < 0 || index >= list.Count)
                    throw new ArgumentOutOfRangeException ("index");

                list.RemoveAt (index);
                owner.RecalculateVisibleOrder(owner.ChildItems[0]);
                owner.SelectedItems.list.RemoveAt (index);
                owner.SelectedIndices.list.RemoveAt (index);
                owner.CheckedItems.list.RemoveAt (index);
                owner.CheckedIndices.list.RemoveAt (index);
                owner.Redraw (false);
            }
            #endregion  // Public Methods


        }   // ListViewItemCollection
    
        #endregion
        

internal TreeListItem root_node;
        private int max_visible_order;

        private TreeListItemCollection items;
        public TreeListItemCollection ChildItems
        {
            get { return items; }
        }

        protected override void CalculateScrollBars ()
        {
            Rectangle client_area = ClientRectangle;
            
            if (!this.scrollable || this.ChildItems.Count <= 0) 
            {
                h_scroll.Visible = false;
                v_scroll.Visible = false;
                return;
            }

            // making a scroll bar visible might make
            // other scroll bar visible         
            if (layout_wd > client_area.Right) 
            {
                h_scroll.Visible = true;
                if ((layout_ht + h_scroll.Height) > client_area.Bottom)
                    v_scroll.Visible = true;                    
                else
                    v_scroll.Visible = false;
            } 
            else if (layout_ht > client_area.Bottom) 
            {               
                v_scroll.Visible = true;
                if ((layout_wd + v_scroll.Width) > client_area.Right)
                    h_scroll.Visible = true;
                else
                    h_scroll.Visible = false;
            } 
            else 
            {
                h_scroll.Visible = false;
                v_scroll.Visible = false;
            }           

            item_control.Height = ClientRectangle.Height - header_control.Height;

            if (h_scroll.Visible) 
            {
                h_scroll.Location = new Point (client_area.X, client_area.Bottom - h_scroll.Height);
                h_scroll.Minimum = 0;

                // if v_scroll is visible, adjust the maximum of the
                // h_scroll to account for the width of v_scroll
                if (v_scroll.Visible) 
                {
                    h_scroll.Maximum = layout_wd + v_scroll.Width;
                    h_scroll.Width = client_area.Width - v_scroll.Width;
                }
                else 
                {
                    h_scroll.Maximum = layout_wd;
                    h_scroll.Width = client_area.Width;
                }
   
                h_scroll.LargeChange = client_area.Width;
                h_scroll.SmallChange = Font.Height;
                item_control.Height -= h_scroll.Height;
            }

            if (header_control.Visible)
                header_control.Width = ClientRectangle.Width;
            item_control.Width = ClientRectangle.Width;

            if (v_scroll.Visible) 
            {
                v_scroll.Location = new Point (client_area.Right - v_scroll.Width, client_area.Y);
                v_scroll.Minimum = 0;

                // if h_scroll is visible, adjust the maximum of the
                // v_scroll to account for the height of h_scroll
                if (h_scroll.Visible) 
                {
                    v_scroll.Maximum = layout_ht + h_scroll.Height;
                    v_scroll.Height = client_area.Height - h_scroll.Height; // already done 
                } 
                else 
                {
                    v_scroll.Maximum = layout_ht;
                    v_scroll.Height = client_area.Height;
                }

                v_scroll.LargeChange = client_area.Height;
                v_scroll.SmallChange = Font.Height;
                if (header_control.Visible)
                    header_control.Width -= v_scroll.Width;
                item_control.Width -= v_scroll.Width;
            }
        }

        

        protected override void DrawItems(PaintEventArgs pe) 
        {
            //Draw as a normal Listview first
//then paint treeview bit last if it is visible,
            //whilst chopping it off at the end of
            //the first column
            base.DrawItems (pe);
            System.Threading.Thread.Sleep(500);
            Rectangle firstcolumn = new Rectangle(-h_marker, 0,this.Columns[0].Width, this.item_control.Height);
            firstcolumn.Intersect(pe.ClipRectangle);
            pe.Graphics.Clip = new Region(firstcolumn);
            this.Draw(pe.ClipRectangle, pe.Graphics);
        }

        internal override int LastVisibleIndex 
        {           
            get 
            {                           
                for (int i = FirstVisibleIndex; i < ChildItems.Count; i++) 
                {
                    if (View == View.List || Alignment == ListViewAlignment.Left) 
                    {
                        if (ChildItems[i].Bounds.X > ClientRectangle.Right)
                            return i - 1;                   
                    } 
                    else 
                    {
                        if (ChildItems[i].Bounds.Y > ClientRectangle.Bottom)
                            return i - 1;                   
                    }
                }
                
                return ChildItems.Count - 1;
            }
        }

        internal override int FirstVisibleIndex 
        {
            get 
            {
                // there is no item
                if (this.ChildItems.Count == 0)
                    return 0;
                                    
                if (h_marker == 0 && v_marker == 0)
                    return 0;                   
                
                foreach (TreeListItem item in this.ChildItems) 
                {
                    if (item.Bounds.Right >= 0 && item.Bounds.Bottom >= 0)
                        return item.Index;
                }
                return 0;

            }
        }


        private Pen dash;

        public int ItemHeight
        {
            get
            {
                //return this.SmallIconItemSize.Height;
                return this.text_size.Height;
            }
        }


        private void Draw(Rectangle clip, Graphics dc)
        {
            dc.FillRectangle (new SolidBrush (BackColor), clip);

            Color dash_color = ControlPaint.Dark (BackColor);
            if (dash_color == BackColor)
                dash_color = ControlPaint.Light (BackColor);
            dash = new Pen (dash_color, 1);
            dash.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

            //          Rectangle viewport = ViewportRectangle;
            //          if (clip.Bottom > viewport.Bottom)
            //              clip.Height = viewport.Bottom - clip.Top;

            //Draw as a normal Listview first
//          OpenTreeListItemEnumerator walk = new OpenTreeListItemEnumerator (this.items [0]);
//          while (walk.MoveNext ()) 
//          {
//              TreeListItem current = walk.CurrentItem ;
//
//              // Haven't gotten to visible nodes yet
//              if (current.GetY () + ItemHeight < clip.Top)
//                  continue;
//
//              // Past the visible nodes
//              if (current.GetY () > clip.Bottom)
//                  break;
//
//              UIMethods.DrawListViewItem(dc, this, current);
//          }
            
            RecalculateVisibleOrder (this.items[0]);

            if (this.ChildItems.Count > 0)
            {
                OpenTreeListItemEnumerator walk = new OpenTreeListItemEnumerator (this.items [0]);
                while (walk.MoveNext ()) 
                {
                    TreeListItem current = walk.CurrentItem ;
            
                    // Haven't gotten to visible nodes yet
                    if (current.GetY () + ItemHeight < clip.Top)
                        continue;
            
                    // Past the visible nodes
                    if (current.GetY () > clip.Bottom)
                        break;

                    DrawItem(current, dc, clip);
                    System.Threading.Thread.Sleep(500);
                }
            }
        }

        internal Rectangle ViewportRectangle 
        {
            get 
            {
                Rectangle res = ClientRectangle;

                if (v_scroll != null && v_scroll.Visible)
                    res.Width -= v_scroll.Width;
                if (h_scroll != null && h_scroll.Visible)
                    res.Height -= h_scroll.Height;
                return res;
            }
        }

        private void DrawSelectionAndFocus(TreeListItem Item, Graphics dc, Rectangle r)
        {
            if (Focused && Item.Selected) 
            {
                ControlPaint.DrawFocusRectangle (dc, r, ForeColor, BackColor);
            }
            r.Inflate(-1, -1);
            if ((!HideSelection || Focused) && Item.Selected)
                dc.FillRectangle (new SolidBrush (SystemColors.Highlight), r);
            else
                dc.FillRectangle (new SolidBrush (Item.BackColor), r);
        }

        private bool show_root_lines = true;

        private void DrawItem (TreeListItem Item, Graphics dc, Rectangle clip)
        {
            int child_count = Item.ChildItems.Count;
            int y = Item.GetY ();
            int middle = y + (ItemHeight / 2);

            if (!full_row_select) 
            {
                Rectangle r = new Rectangle (1, y + 2, ViewportRectangle.Width - 2, ItemHeight);
                DrawSelectionAndFocus (Item, dc, r);
            }

            if ((Item.Parent != null) && show_plus_minus && child_count > 0)
                DrawNodePlusMinus (Item, dc, Item.GetLinesX () - indent + 5, middle);

            
            DrawNodeLines (Item, dc, clip, dash, Item.GetLinesX (), middle);

            DrawStaticItem (Item, dc, Item.GetLinesX (),middle);
        }

        private void DrawStaticItem (TreeListItem Item, Graphics dc, int x ,int y)
        {
            if (!full_row_select)
                DrawSelectionAndFocus(Item, dc, Item.Bounds);

            Font font = Item.Font;
            if (Item.Font == null)
                font = Font;
            Color text_color = ((Focused || !HideSelection) && Item.Selected ?
                SystemColors.HighlightText : Item.ForeColor);
            ///HACK
            
            //dc.Clip = new Region(drawingarea);
//          //dc.TranslateClip(-70,0);
//                      dc.DrawString (Item.Text, font,
//                          new SolidBrush (text_color),
//                          x,y + (int) (dc.MeasureString(Item.Text,font).Height /2), string_format);
                        dc.DrawString (Item.Text, font,
                            new SolidBrush (text_color),
                            x + 3,y , string_format);
            //dc.DrawString (Item.Text, font,
                //new SolidBrush (text_color),
                //Item.Bounds.X +10, Item.Bounds.Y, string_format);
            //dc.DrawString(Item.Text,font, new SolidBrush(text_color),10,10);
//          dc.DrawString (Item.Text, font,
//              new SolidBrush (text_color),
//              new RectangleF(0,10,this.ClientSize.Width, 16), string_format);

        }


        private void DrawNodePlusMinus (TreeListItem Item, Graphics dc, int x, int middle)
        {
            dc.DrawRectangle (SystemPens.ControlDark, x, middle - 4, 8, 8);

            if (Item.IsExpanded) 
            {
                dc.DrawLine (SystemPens.ControlDarkDark, x + 2, middle, x + 6, middle); 
            } 
            else 
            {
                dc.DrawLine (SystemPens.ControlDarkDark, x + 2, middle, x + 6, middle);
                dc.DrawLine (SystemPens.ControlDarkDark, x + 4, middle - 2, x + 4, middle + 2);
            }
        }

        private int indent = 19;
        private bool show_plus_minus = true;
        private void DrawNodeLines (TreeListItem Item, Graphics dc, Rectangle clip, Pen dash, int x, int middle)
        {
            int ladjust = 9;
            int radjust = 0;

            if (Item.ChildItems.Count > 0 && show_plus_minus)
                ladjust = 13;
            
//          dc.DrawLine (dash, x - indent + ladjust, middle, x + radjust, middle);
            dc.DrawLine (dash, x - indent + ladjust - h_marker, middle, x + radjust - h_marker, middle);

            if (Item.PrevItem != null || Item.Parent != null) 
            {
                ladjust = 9;
//              dc.DrawLine (dash, x - indent + ladjust, Item.Bounds.Top,
//                  x - indent + ladjust, middle - (show_plus_minus && Item.ChildItems.Count > 0 ? 4 : 0));
                dc.DrawLine (dash, x - indent + ladjust - h_marker, Item.Bounds.Top,
                    x - indent + ladjust - h_marker, middle - (show_plus_minus && Item.ChildItems.Count > 0 ? 4 : 0));
            }

            if (Item.NextItem != null) 
            {
                ladjust = 9;
                //dc.DrawLine (dash, x - indent + ladjust, middle + (show_plus_minus && Item.ChildItems.Count > 0 ? 4 : 0),
                //  x - indent + ladjust, Item.Bounds.Bottom);
                dc.DrawLine (dash, x - indent + ladjust - h_marker, middle + (show_plus_minus && Item.ChildItems.Count > 0 ? 4 : 0),
                    x - indent + ladjust - h_marker, Item.Bounds.Bottom);
            }

            ladjust = 0;
            if (show_plus_minus)
                ladjust = 9;
            TreeListItem parent = Item.Parent;
            while (parent != null) 
            {
                if (parent.NextItem != null) 
                {
                    //PJW hmarker
                    int px = parent.GetLinesX () - indent + ladjust - h_marker;
                    dc.DrawLine (dash, px, Item.Bounds.Top, px, Item.Bounds.Bottom);
                }
                parent = parent.Parent;
            }
        }

        private void DrawLinesToNext (TreeListItem Item, Graphics dc, Rectangle clip, Pen dash, int x, int y)
        {
            int middle = y + (ItemHeight / 2);

            if (Item.NextItem != null) 
            {
                int top = (Item.ChildItems.Count > 0 && show_plus_minus ? middle + 4 : middle);
                int ncap = (Item.NextItem.ChildItems.Count > 0 && show_plus_minus ? 4 : 8);
                int bottom = Math.Min (Item.NextItem.GetY () + ncap, clip.Bottom);

                dc.DrawLine (dash, x - indent + 9, top, x - indent + 9, bottom);
            }

            if (Item.IsExpanded && Item.ChildItems.Count > 0) 
            {
                int top = Item.Bounds.Bottom;
                int ncap = (Item.ChildItems [0].ChildItems.Count > 0 && show_plus_minus ? 4 : 8);
                int bottom = Math.Min (Item.ChildItems [0].GetY () + ncap, clip.Bottom);
                int nx = Item.ChildItems [0].GetLinesX ();

                dc.DrawLine (dash, nx - indent + 9, top, nx - indent + 9, bottom);
            }
        }

        

        internal void RecalculateVisibleOrder (TreeListItem start)
        {

            int order;
            if (start == null) 
            {
                start = root_node;
                order = 0;
            } 
            else
                order = start.visible_order;

            OpenTreeListItemEnumerator walk = new OpenTreeListItemEnumerator (start);
            while (walk.MoveNext ()) 
            {
                walk.CurrentItem.visible_order = order;
                order++;
            }

            max_visible_order = order;
        }
    }
}

