using System.Windows.Forms;
using System.ComponentModel;

namespace AutomateControls
{
    /// <summary>
    /// Just a list view with some sensible defaults, namely :- <list>
    /// <item><see cref="ListView.FullRowSelect"/> := true</item>
    /// <item><see cref="ListView.View"/> := View.Details</item>
    /// <item><see cref="ListView.HideSelection"/> := false</item>
    /// </list>
    /// </summary>
    public class DetailListView : ListView
    {
        /// <summary>
        /// Creates a new, empty detail list view
        /// </summary>
        public DetailListView()
        {
            this.FullRowSelect = true;
            this.View = View.Details;
            this.HideSelection = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether clicking an item selects all its
        /// subitems.
        /// </summary>
        /// <returns>
        /// true if clicking an item selects the item and all its subitems; false if
        /// clicking an item selects only the item itself. The default is true.
        /// </returns>
        [DefaultValue(true)]
        public new bool FullRowSelect
        {
            get { return base.FullRowSelect; }
            set { base.FullRowSelect = value; }
        }

        /// <summary>
        /// Gets or sets how items are displayed in the control.
        /// </summary>
        /// <returns>
        /// One of the <see cref="System.Windows.Forms.View"/> values.
        /// The default is <see cref="View.Details"/>
        /// </returns>
        /// <exception cref="InvalidEnumArgumentException">The value specified is not
        /// one of the <see cref="System.Windows.Forms.View"/> values</exception>
        [DefaultValue(View.Details)]
        public new View View
        {
            get { return base.View; }
            set { base.View = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the selected item in the control
        /// remains highlighted when the control loses focus.
        /// </summary>
        /// <returns>
        /// true if the selected item does not appear highlighted when the control loses
        /// focus; false if the selected item still appears highlighted when the control
        /// loses focus. The default is false.
        /// </returns>
        [DefaultValue(false)]
        public new bool HideSelection
        {
            get { return base.HideSelection; }
            set { base.HideSelection = value; }
        }
    }
}
