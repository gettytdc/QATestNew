using System.Collections;

namespace AutomateControls.TreeList

{

    public class TreeListViewItemEnumerator : IEnumerator {

        private TreeList.TreeListViewItem start;
        private TreeList.TreeListViewItem current;
        private bool started;

        public TreeListViewItemEnumerator (TreeList.TreeListViewItem start)
        {
            this.start = start;
        }

        public object Current {
            get { return current; }
        }

        public TreeList.TreeListViewItem CurrentItem {
            get { return current; }
        }

        public bool MoveNext ()
        {
            if (!started) {
                started = true;
                current = start;
                return (current != null);
            }

            if (current.Items.Count > 0) {
                current = current.Items [0];
                return true;
            }

            TreeList.TreeListViewItem prev = current;
            TreeList.TreeListViewItem next = current.NextItem;
            while (next == null) {
                // The next item is null so we need to move back up the tree until we hit the top
                if (prev.Parent == null)
                    return false;
                prev = prev.Parent;
                if (prev.Parent != null)
                    next = prev.NextItem;
            }
            current = next;
            return true;
        }
        
        public bool MovePrevious ()
        {
            if (!started) {
                started = true;
                current = start;
                return (current != null);
            }

            if (current.PrevItem != null) {
                current = current.PrevItem;
                return true;
            }

            if (current.Parent == null)
//              ||
//                  current.Parent == current.TreeListView.root_node)
                return false;

            current = current.Parent;
            return true;
        }

        public void Reset ()
        {
            started = false;
        }
    }
}

