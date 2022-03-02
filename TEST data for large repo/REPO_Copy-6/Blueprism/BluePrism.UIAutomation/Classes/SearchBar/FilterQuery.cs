using System.Collections.Generic;

namespace BluePrism.UIAutomation.Classes.SearchBar
{

    public class FilterQuery
    {
        public string ColumnName { get; set; } = "";
        private bool UsingIn { get; set; } = false;
        private bool ColumnNameResolved { get; set; } = false;
        public string SearchText { get; set; } = "";
        public int MaxAutoFills { get; set; } = 5;
        public int MaxPreviousFills { get; set; } = 2;
        private SearchTreeNode RootNode { get; set; } = new SearchTreeNode(null, new List<SearchTreeNode>());
        private readonly string _localName = "name";
        private readonly string _localIn = "in";

        public FilterQuery(string localName, string localIn)
        {
            _localIn = string.Concat(localIn, ":");
            RootNode.Children.Add(new SearchTreeNode(_localIn, new List<SearchTreeNode>()));
            _localName = localName;
        }

        public void AddColumnNode(SearchTreeNode node)
        {
            RootNode.Children[0].Children.Add(node);
        }

        public void AddLeafToColumnNode(string column, SearchTreeNode node)
        {
            if (!string.IsNullOrWhiteSpace(node.Data))
            {
                RootNode.Children[0].GetChild(column).AddChild(node);
            }
        }

        public void PruneColumnNodes()
        {
            foreach (var node in RootNode.Children[0].Children)
            {
                node.Children = new List<SearchTreeNode>();
            }
        }

        public void Evaluate(string query)
        {
            UsingIn = false;
            ColumnNameResolved = false;
            var queryLow = query.ToLower();

            if (query.Length >= _localIn.Length)
            {
                if (query.Substring(0, _localIn.Length) == _localIn)
                {
                    UsingIn = true;
                    queryLow = queryLow.Substring(_localIn.Length).Trim();
                    SearchText = ResolveColumn(queryLow);
                }
                else
                {
                    ColumnName = _localName;
                    SearchText = queryLow;
                }
            }
            else
            {
                ColumnName = _localName;
                SearchText = queryLow;
            }
        }

        
        private string ResolveColumn(string query)
        {
            var ColumnList = RootNode.GetChild(_localIn).GetChildrenData();
            foreach (var col in ColumnList)
            {
                if (query.Length >= col.Length && query.Substring(0, col.Length) == col.ToLower())
                {
                    ColumnNameResolved = true;
                    ColumnName = col;
                    return query.Substring(col.Length).Trim();
                }
            }
            return query;
        }

        public List<string> GetAutoFills()
        {
            if (!UsingIn)
            {
                return new List<string>();
            }

            if (ColumnNameResolved)
            {
                return GetColumnTermsAutoFills();
            }
            else
            {
                 return GetColumnAutoFills();
            }
             
        }

        private List<string> GetColumnTermsAutoFills()
        {
            var sn = RootNode.GetChild(_localIn).GetChild(ColumnName);
            var cells = sn.GetChildrenData();
            var autofills = new List<string>();
              
            foreach (var str in cells)
            {
                if (str.ToLower().Contains(SearchText.ToLower()))
                {
                    autofills.Add(string.Join(" ", _localIn, sn.Data, str));
                }
            }

            if (autofills.Count > MaxAutoFills)
            {
                return autofills.GetRange(0, MaxAutoFills);
            }

            return autofills;
        }

        private List<string> GetColumnAutoFills()
        {
            var autos = new List<string>();
            var columns = RootNode.GetChild(_localIn).GetChildrenData();

            foreach (var col in columns)
            {
                var strCol = col.ToLower();
                if (strCol.Length > SearchText.Length)
                {
                    strCol = strCol.Substring(0, SearchText.Length);
                }
                if (SearchText == strCol)
                {
                    autos.Add(string.Join(" ", _localIn, col));
                }
            }

            if (autos.Count > MaxAutoFills)
            {
                return autos.GetRange(0, MaxAutoFills);
            }
            else
            {
                foreach (var col in columns)
                {
                    var strCol = col.ToLower();
                    if (strCol.Length > SearchText.Length)
                    {
                        strCol = strCol.Substring(0, SearchText.Length);
                    }
                    if (SearchText == strCol)
                    {
                        var sn = RootNode.GetChild(_localIn).GetChild(col);
                        var queries = sn.GetChildrenData();
                        foreach (var s in queries)
                        {
                            autos.Add(string.Join(" ", _localIn, sn.Data, s));
                        }
                    }
                }
            }

            if (autos.Count > MaxAutoFills)
            {
                return autos.GetRange(0, MaxAutoFills);
            }

            return autos;
        }
    }
}
