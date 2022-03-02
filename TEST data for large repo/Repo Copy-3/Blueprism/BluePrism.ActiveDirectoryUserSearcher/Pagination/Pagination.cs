using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace BluePrism.ActiveDirectoryUserSearcher.Pagination
{
    public class Pagination : IPagination
    {
        private readonly int _totalPages;
        private int _currentPageNumber;

        public int CurrentPageNumber
        {
            get => _currentPageNumber;
            set
            {
                _currentPageNumber = value;
                UpdateCurrentState();
            }
        }

        public ReadOnlyCollection<int> VisiblePageNumbers { get; private set; }

        public bool CanGoForwards { get; private set; }

        public bool CanGoBackwards { get; private set; }

        public Pagination(int resultsPerPage, int maximumVisiblePageNumbers, int totalRecords)
        {
            _totalPages = Math.Max(1, Convert.ToInt32(Math.Ceiling((double)totalRecords / resultsPerPage)));

            var initialVisiblePageNumbers = Math.Min(_totalPages, maximumVisiblePageNumbers);
            VisiblePageNumbers = Enumerable.Range(1, initialVisiblePageNumbers).ToList().AsReadOnly();
            CurrentPageNumber = 1;            
        }     

        public void GoForwards()
        {
            if(CanGoForwards)
                CurrentPageNumber += 1;            
        }

        public void GoBackwards()
        {
            if (CanGoBackwards)
                CurrentPageNumber -= 1;                     
        }               
                
        private void UpdateCurrentState()
        {            
            CanGoBackwards = _currentPageNumber != 1;
            CanGoForwards = _currentPageNumber < _totalPages;

            var isCurrentPageFirstOrLast = _currentPageNumber == 1 || _currentPageNumber == _totalPages;
            if (isCurrentPageFirstOrLast)
                return;

            var firstVisiblePageNumber = VisiblePageNumbers.First();
            var totalVisiblePageNumbers= VisiblePageNumbers.Count();

            var requiresScrollToLeft = _currentPageNumber <= VisiblePageNumbers.First();
            if (requiresScrollToLeft)
            {
                VisiblePageNumbers = Enumerable
                                        .Range(firstVisiblePageNumber - 1, totalVisiblePageNumbers)
                                        .ToList()
                                        .AsReadOnly();
                return;
            }

            var requiresScrollToRight = _currentPageNumber >= VisiblePageNumbers.Last();
            if (requiresScrollToRight)
            {
                VisiblePageNumbers = Enumerable
                                        .Range(firstVisiblePageNumber + 1, totalVisiblePageNumbers)
                                        .ToList()
                                        .AsReadOnly();
            }
        }
    }
}
