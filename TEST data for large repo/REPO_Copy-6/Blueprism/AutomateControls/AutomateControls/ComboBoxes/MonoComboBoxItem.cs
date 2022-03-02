using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace AutomateControls.ComboBoxes
{
    /// <summary>
    /// Encapsulates the concept of an item which can be added to
    /// a combo box.
    /// </summary>
    public class MonoComboBoxItem : IComparable
    {
        #region - Constructors -

        /// <summary>
        /// Creates a new enabled combo box item with the given text.
        /// </summary>
        /// <param name="text">The label to display for this item.</param>
        public MonoComboBoxItem(string text)
            : this(text, null, true, Color.Empty) { }

        /// <summary>
        /// Creates a new combo box item with the given text and the given
        /// configuration.
        /// </summary>
        /// <param name="text">The label to display for this item.</param>
        /// <param name="tag">The tag object to associate with this item.
        /// </param>
        public MonoComboBoxItem(string text, object tag)
            : this(text, tag, true, Color.Empty) { }

        /// <summary>
        /// Creates a new combo box item with the given text and the given
        /// configuration.
        /// </summary>
        /// <param name="text">The label to display for this item.</param>
        /// <param name="enabled">True to indicate that this item should be
        /// enabled; false to indicate it should be disabled.</param>
        public MonoComboBoxItem(string text, bool enabled)
            : this(text, null, enabled, Color.Empty) { }

        /// <summary>
        /// Creates a new combo box item with the given text and the given
        /// configuration.
        /// </summary>
        /// <param name="text">The label to display for this item.</param>
        /// <param name="tag">The tag object to associate with this item.
        /// </param>
        /// <param name="enabled">True to indicate that this item should be
        /// enabled; false to indicate it should be disabled.</param>
        public MonoComboBoxItem(string text, object tag, bool enabled)
            : this(text, tag, enabled, Color.Empty) { }

        /// <summary>
        /// Creates a new combo box item with the given text and the given
        /// configuration.
        /// </summary>
        /// <param name="text">The label to display for this item.</param>
        /// <param name="tag">The tag object to associate with this item.
        /// </param>
        /// <param name="color">The color of the font to use.</param>
        public MonoComboBoxItem(string text, object tag, Color color)
            : this(text, tag, true, color) { }

        /// <summary>
        /// Creates a new combo box item with the given text and the given
        /// configuration.
        /// </summary>
        /// <param name="text">The label to display for this item.</param>
        /// <param name="tag">The tag object to associate with this item.
        /// </param>
        /// <param name="enabled">True to indicate that this item should be
        /// enabled; false to indicate it should be disabled.</param>
        /// <param name="color">The color of the font to use.</param>
        private MonoComboBoxItem(string text, object tag, bool enabled, Color color)
        {
            mText = text;
            mTag = tag;
            mEnabled = enabled;
            mColor = color;
            mDisabledColour = Color.Empty;
        }

        #endregion


        #region - Public Properties -

        /// <summary>
        /// Private member to store public property Tag()
        /// </summary>
        private Object mTag;
        /// <summary>
        /// Custom object attached to this item
        /// </summary>
        public Object Tag
        {
            get { return mTag; }
            set { mTag = value; }
        }

        /// <summary>
        /// Private member to store public property Text
        /// </summary>
        private string mText;
        /// <summary>
        /// The text displayed in this combo box item
        /// </summary>
        public string Text
        {
            get { return mText; }
            set { mText = value; }
        }

        /// <summary>
        /// Private member to store public property FontColour
        /// </summary>
        private Color mColor;
        /// <summary>
        /// The font colour of this item.
        /// </summary>
        public Color FontColour
        {
            get { return mColor; }
            set { mColor = value; }
        }

        /// <summary>
        /// Private member to store public property DisabledColour
        /// </summary>
        private Color mDisabledColour;
        /// <summary>
        /// The colour to use to display this item when the Enabled property is
        /// False. Set to Color.Empty to use the owner combobox's default colour
        /// (see ComboBox.DisabledItemColour property).
        /// </summary>
        public Color DisabledColour
        {
            get
            {
                if (mDisabledColour == Color.Empty)
                {
                    if (this.Owner != null)
                    {
                        return Owner.DisabledItemColour;
                    }
                    else
                    {
                        return Color.LightGray;
                    }
                }
                else
                {
                    return mDisabledColour;
                }
            }
            set
            {
                mDisabledColour = value;
            }
        }

        /// <summary>
        /// Private member to store public property Font
        /// </summary>
        /// <remarks>May be null. If null then the owning
        /// combo box's font should be used.</remarks>
        private Font mFont;
        /// <summary>
        /// The font of this item.
        /// </summary>
        public Font ItemFont
        {
            get { return mFont; }
            set { mFont= value; }
        }

        /// <summary>
        /// Private member to store public property Enabled.
        /// </summary>
        private bool mEnabled;
        /// <summary>
        /// Determines whether the item is enabled. Disabled items cannot
        /// be selected from the combobox control.
        /// 
        /// True by default.
        /// </summary>
        public bool Enabled
        {
            get { return mEnabled; }
            set { mEnabled = value; }
        }
        /// <summary>
        /// Private member to store public property Owner.
        /// </summary>
        private MonoComboBox mOwner;
        /// <summary>
        /// The combobox owning this item.
        /// </summary>
        public MonoComboBox Owner
        {
            get { return mOwner; }
            set { mOwner = value; }
        }

        #endregion

        #region - IComparable implementation -

        int System.IComparable.CompareTo(object x)
        {
            MonoComboBoxItem item = x as MonoComboBoxItem;
            return (item == null ? 0 : Text.CompareTo(item.Text));
        }
        #endregion
    }


    #region "Class ComboBoxCollection"

    public class ComboBoxItemCollection : IList, ICollection, IEnumerable
{

        private MonoComboBox owner;
        private bool sorted;
        internal List<object> object_items = new List<object>();

        public ComboBoxItemCollection (MonoComboBox owner)
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

        [Browsable (true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual MonoComboBoxItem this [int index] 
        {
            get 
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException ("Index of out range");

                return (MonoComboBoxItem) object_items[index];
            }
            set 
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException ("Index of out range");

                object_items[index] = value;
            }
        }

        [Browsable (false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        object IList.this [int index] 
        {
            get 
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException ("Index of out range");

                return object_items[index];
            }
            set 
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException ("Index of out range");

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

        public bool Sorted
        {
            get
            {
                return sorted;
            }
            set
            {
                this.sorted = value;
                if (value)
                {
                    this.SortInternal();
                }
            }
        }

        #endregion Public Properties
            
        #region Public Methods
        public int Add (MonoComboBoxItem item)
        {
            int idx;

            item.Owner = owner;
            idx = AddItem (item);
            owner.UpdatedItems ();
            return idx;
        }

        public int IndexOf (MonoComboBoxItem item)
        {
return object_items.IndexOf(item);
        }

        public void AddRange (ComboBoxItemCollection[] items)
        {
            foreach (object mi in items)
                AddItem ((MonoComboBoxItem) mi);    
            owner.UpdatedItems ();
        }

        public void Clear ()
        {
            owner.SelectedIndex = -1;
            object_items.Clear ();
            owner.UpdatedItems ();
            owner.Refresh ();
        }

        public bool Contains(ComboBoxItemCollection obj)
        {
            return object_items.Contains (obj);
        }

        private MonoComboBoxItem GetItemWithText(string Text)
        {
            foreach (MonoComboBoxItem cbi in object_items)
            {
                if (cbi.Text == Text)
                    return cbi;
            }
            return null;
        }

        public bool Contains(string Text)
        {
            MonoComboBoxItem cbi = GetItemWithText(Text);
            return (cbi != null);
        }

        bool IList.Contains(object obj)
        {
            return object_items.Contains (obj);
        }

        public void CopyTo (ComboBoxItemCollection[] dest, int arrayIndex)
        {
            object_items.CopyTo (dest, arrayIndex);
        }

        public IEnumerator GetEnumerator ()
        {
            return object_items.GetEnumerator ();
        }

        int IList.Add (object item)
        {
            if (item.GetType() != typeof(MonoComboBoxItem))
                throw new ArgumentException("Invalid argument to Add: item must be of type ComboBoxItem");

            return AddItem ((MonoComboBoxItem) item);
        }

        int IList.IndexOf (object value)
        {
            return object_items.IndexOf (value);
        }

        void IList.Insert (int index,  object item)
        {
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException ("Index of out range");                   
                
            ComboBoxItemCollection new_items = new ComboBoxItemCollection (owner);              
            MonoComboBoxItem sel_item = owner.SelectedItem;
                                                        
            owner.BeginUpdate ();
                
            for (int i = 0; i < index; i++) 
            {
                new_items.AddItem ((MonoComboBoxItem) object_items[i]);
            }

            new_items.AddItem ((MonoComboBoxItem) item);

            for (int i = index; i < Count; i++)
            {
                new_items.AddItem ((MonoComboBoxItem) object_items[i]);
            }               

            object_items = new_items.object_items;
                
            if (sel_item != null) 
            {
                int idx = IndexOf (sel_item);
                owner.SelectedIndex = idx;
                if(owner.listbox_ctrl != null)
                    owner.listbox_ctrl.HighlightedIndex = idx;
            }

            SortInternal();         
            owner.EndUpdate (); // Calls UpdatedItems
        }

        public void Insert(int Index, MonoComboBoxItem Item)
        {
            ((IList) this).Insert(Index, Item);
        }

        void IList.Remove (object value)
        {               
            if (value.GetType() != typeof(MonoComboBoxItem))
                throw new ArgumentException("Bad argument to method Remove() - value should be of type ComboBoxItem");

            if (IndexOf ((MonoComboBoxItem) value) == owner.SelectedIndex)
                owner.SelectedItem = null;
                
            RemoveAt (IndexOf ((MonoComboBoxItem) value));              
                
        }

        public void Remove(string Text)
        {
            MonoComboBoxItem cbi = GetItemWithText(Text);
            if (cbi != null)
                ((IList) this).Remove(cbi);
        }

        public void RemoveAt (int index)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException ("Index of out range");
                    
            if (index == owner.SelectedIndex)
                owner.SelectedItem = null;

            object_items.RemoveAt (index);
            this.SortInternal();
            owner.UpdatedItems ();
        }
        #endregion Public Methods

        #region Private Methods
        private int AddItem (MonoComboBoxItem item)
        {
            int cnt = object_items.Count;
            item.Owner = owner;
            object_items.Add (item);

            this.SortInternal();
            return cnt;
        }
            
        internal void AddRange (IList items)
        {
            foreach (MonoComboBoxItem mi in items)
                AddItem (mi);
                                        
            owner.UpdatedItems ();
        }

        private void SortInternal()
        {
            if (this.Sorted) 
            {
                MonoComboBoxItem OldSelectedItem = owner.SelectedItem;
                this.object_items.Sort();
                this.owner.SelectedItem = OldSelectedItem;
            }
        }

        #endregion Private Methods
    }

    #endregion "ComboBoxCollection"


}