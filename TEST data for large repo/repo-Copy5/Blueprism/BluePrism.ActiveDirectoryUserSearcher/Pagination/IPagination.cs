using System.Collections.ObjectModel;

namespace BluePrism.ActiveDirectoryUserSearcher.Pagination
{
    public interface IPagination
    {
        int CurrentPageNumber { get; set; }

        ReadOnlyCollection<int> VisiblePageNumbers { get; }                               

        void GoForwards();

        void GoBackwards();

        bool CanGoForwards { get; }

        bool CanGoBackwards { get; }              
                
    }
}
