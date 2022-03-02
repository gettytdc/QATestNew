using System.Collections.Generic;

namespace AutomateControls
{
    public class GroupedComboBoxItem : ComboBoxItem
    {
        public GroupedComboBoxItem(string text, bool enabled) : base(text, enabled) { }

        public GroupedComboBoxItem(string text, object tag) : base(text, tag) { }

        public int Id { get; set; }

        public bool IsGroup { get; set; }

        public GroupedComboBoxItem Group { get; set; }

        public List<GroupedComboBoxItem> Items { get; set; } = new List<GroupedComboBoxItem>();
    }
}
