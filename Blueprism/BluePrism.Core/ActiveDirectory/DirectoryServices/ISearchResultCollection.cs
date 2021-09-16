using System;
using System.Collections.Generic;

namespace BluePrism.Core.ActiveDirectory.DirectoryServices
{
    public interface ISearchResultCollection : IEnumerable<ISearchResult>, IDisposable
    {
        
    }
}
