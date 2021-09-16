using System.Collections.Generic;

namespace BluePrism.UIAutomation.Classes.SearchBar
{
    public class SearchTreeNode
    {
        public string Data { get; set; }
        public List<SearchTreeNode> Children { get; set; } = new List<SearchTreeNode>();
        public SearchTreeNode(string data, List<SearchTreeNode> children)
        {
            this.Data = data;
            this.Children = children;
        }

        public void AddChild(SearchTreeNode node)
        {
            foreach (var child in Children)
            {
                if (node.Data == child.Data)
                    return;
            }
            Children.Add(node);
        }

        public SearchTreeNode GetChild(string data)
        {
            foreach (var child in Children)
            {
                if (data == child.Data)
                    return child;
            }
            return null;
        }

        public List<string> GetChildrenData()
        {
            List<string> data = new List<string>();
            foreach (var child in Children)
                data.Add(child.Data);
            return data;
        }
    }

}
