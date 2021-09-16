using BluePrism.Core.ActiveDirectory.DirectoryServices;
using System.Collections.Generic;

namespace BluePrism.Core.ActiveDirectory.UserQuery
{
    public interface IMappedUserFinder
    {
        HashSet<string> AlreadyMappedSids(IEnumerable<ISearchResult> activeDirectoryUsers); 
    }
}
