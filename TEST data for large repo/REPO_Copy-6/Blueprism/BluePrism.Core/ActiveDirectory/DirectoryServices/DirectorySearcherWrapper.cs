using System;
using System.DirectoryServices;

namespace BluePrism.Core.ActiveDirectory.DirectoryServices
{
    public class DirectorySearcherWrapper : IDirectorySearcher
    {
        private DirectorySearcher _directorySearcher;

        public DirectorySearcherWrapper(DirectorySearcher directorySearcher)
        {
            _directorySearcher = directorySearcher;
        }

        public void AddPropertyToLoad(string property) => _directorySearcher.PropertiesToLoad.Add(property);
        
        public ISearchResultCollection FindAll()
            => new SearchResultCollectionWrapper(_directorySearcher.FindAll());

        void IDirectorySearcher.SearchRoot(DirectoryEntry entry)
            => _directorySearcher.SearchRoot = entry;

        void IDirectorySearcher.Filter(string filter)
            => _directorySearcher.Filter = filter;

        void IDirectorySearcher.Sort(SortOption sort)
            => _directorySearcher.Sort = sort;

        void IDirectorySearcher.VirtualListView(DirectoryVirtualListView virtualListView)
            => _directorySearcher.VirtualListView = virtualListView;

        public int ApproximateTotal {  get => _directorySearcher.VirtualListView?.ApproximateTotal 
                                                ?? throw new InvalidOperationException("Can't access approximate total before search"); }

        public void Dispose() => _directorySearcher?.Dispose();
    }
}
