using System;
using System.Collections;

namespace AutomateControls

{

    internal class OpenTreeListItemEnumerator : IEnumerator 
    {

        private TreeList.TreeListItem start;
        private TreeList.TreeListItem current;
        private bool started;

        public OpenTreeListItemEnumerator (TreeList.TreeListItem start)
        {
            this.start = start;
        }

        public object Current 
        {
            get { return current; }
        }

        public TreeList.TreeListItem CurrentItem 
        {
            get { return current; }
        }

        public bool MoveNext ()
        {
            if (!started) 
            {
                started = true;
                current = start;
                return (current != null);
            }

            if (current.IsExpanded && current.ChildItems.Count > 0) 
            {
                current = current.ChildItems [0];
                return true;
            }

            TreeList.TreeListItem prev = current;
            TreeList.TreeListItem next = current.NextItem;
            while (next == null) 
            {
                // The next node is null so we need to move back up the tree until we hit the top
                if (prev.parent == null)
                    return false;
                prev = prev.parent;
                if (prev.parent != null)
                    next = prev.NextItem;
            }
            current = next;
            return true;
        }
        
        public bool MovePrevious ()
        {
            if (!started) 
            {
                started = true;
                current = start;
                return (current != null);
            }

            if (current.PrevItem != null) 
            {
                // Drill down as far as possible
                TreeList.TreeListItem prev = current.PrevItem;
                TreeList.TreeListItem walk = prev;
                while (walk != null) 
                {
                    prev = walk;
                    if (!walk.IsExpanded)
                        break;
                    walk = walk.LastItem;
                }
                current = prev;
                return true;
            }

            if (current.parent == null ||
                current.parent == current.owner.root_node)
                return false;

            current = current.parent;
            return true;
        }

        public void Reset ()
        {
            started = false;
        }
    }
}

