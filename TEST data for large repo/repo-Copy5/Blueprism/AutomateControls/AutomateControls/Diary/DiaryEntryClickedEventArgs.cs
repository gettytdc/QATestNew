using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using BluePrism.BPCoreLib.Diary;

namespace AutomateControls.Diary
{
    /// <summary>
    /// Event arguments class for diary entry clicked events.
    /// </summary>
    public class DiaryEntryClickedEventArgs : MouseEventArgs
    {
        // The entry for which an event has occurred.
        private ICollection<IDiaryEntry> _entries;

        /// <summary>
        /// The entry for which an event has occurred.
        /// </summary>
        public ICollection<IDiaryEntry> Entries { get { return _entries; } }

        /// <summary>
        /// Gets the first entry from this event, or null if it contains no
        /// diary entries.
        /// </summary>
        public IDiaryEntry FirstEntry
        {
            get
            {
                return _entries.OfType<IDiaryEntry>().FirstOrDefault();
            }
        }

        /// <summary>
        /// Creates a new event arguments object which indicates that the mouse
        /// was clicked on the given diary entry.
        /// </summary>
        /// <param name="e">The mouse event arguments which propogated the event
        /// referred to by this arguments object.</param>
        /// <param name="entry">The entry for which an event has occurred.</param>
        public DiaryEntryClickedEventArgs(MouseEventArgs e, ICollection<IDiaryEntry> entries)
            : base(e.Button, e.Clicks, e.X, e.Y, e.Delta)
        {
            _entries = entries;
        }
    }
}
