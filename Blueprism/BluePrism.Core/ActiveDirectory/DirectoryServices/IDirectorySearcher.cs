using System;
using System.DirectoryServices;

namespace BluePrism.Core.ActiveDirectory.DirectoryServices
{
    public interface IDirectorySearcher : IDisposable
    {
        void AddPropertyToLoad(string property);
        void SearchRoot(DirectoryEntry entry);
        void Filter(string filter);
        void Sort (SortOption sortOption);
        void VirtualListView(DirectoryVirtualListView virtualListView);
        ISearchResultCollection FindAll();
        int ApproximateTotal { get; }
    }
}