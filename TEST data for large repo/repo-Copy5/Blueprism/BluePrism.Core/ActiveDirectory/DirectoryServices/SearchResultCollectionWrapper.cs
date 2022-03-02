using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;

namespace BluePrism.Core.ActiveDirectory.DirectoryServices
{
    public class SearchResultCollectionWrapper : ISearchResultCollection
    {
        private readonly SearchResultCollection _searchResults;

        public SearchResultCollectionWrapper(SearchResultCollection searchResults)
        {
            _searchResults = searchResults;
        }

        public void Dispose() => _searchResults?.Dispose();        

        public IEnumerator<ISearchResult> GetEnumerator() 
            => _searchResults
                    .OfType<SearchResult>()
                    .Select(x => new SearchResultWrapper(x))
                    .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
    }
}
