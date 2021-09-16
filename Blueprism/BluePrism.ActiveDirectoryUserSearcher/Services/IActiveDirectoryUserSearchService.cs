using BluePrism.Core.ActiveDirectory.DirectoryServices;
using BluePrism.Core.ActiveDirectory.UserQuery;

namespace BluePrism.ActiveDirectoryUserSearcher.Services
{
    public interface IActiveDirectoryUserSearchService
    {		
		PaginatedUserQueryResult FindActiveDirectoryUsers(string searchRootText, UserFilterType filterType, string filterText, DirectorySearcherCredentials credentials, int startIndex, int pageSize);
		
        string GetDistinguishedNameOfCurrentForest();
    }
}
