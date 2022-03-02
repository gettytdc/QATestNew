using BluePrism.AutomateAppCore;
using BluePrism.Core.ActiveDirectory;
using BluePrism.Core.ActiveDirectory.DirectoryServices;
using BluePrism.Core.ActiveDirectory.UserQuery;

namespace BluePrism.ActiveDirectoryUserSearcher.Services
{
    public class ActiveDirectoryUserSearchService : IActiveDirectoryUserSearchService
    {
        private readonly IServer _server;

        public ActiveDirectoryUserSearchService(IServer server)
        {
            _server = server;
        }

        public PaginatedUserQueryResult FindActiveDirectoryUsers(string searchRootText, UserFilterType filterType, string filterText, DirectorySearcherCredentials credentials, int startIndex, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(searchRootText))
            {
                return PaginatedUserQueryResult.InvalidQuery();
            }

            var queryPageOptions = new QueryPageOptions(startIndex, pageSize);
            return _server.FindActiveDirectoryUsers(searchRootText, filterType, filterText, queryPageOptions, credentials);
        }
		
        public string GetDistinguishedNameOfCurrentForest()
        {
            return _server.GetDistinguishedNameOfCurrentForest();
        }
    }
}
