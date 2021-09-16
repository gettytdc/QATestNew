using BluePrism.Core.ActiveDirectory.DirectoryServices;
using System;

namespace BluePrism.Core.ActiveDirectory.UserQuery
{
    public class ActiveDirectoryUserQueryOptions
    {
        public ActiveDirectoryUserQueryOptions(string searchRoot, UserFilterType filterType, string filter, QueryPageOptions pageOptions, DirectorySearcherCredentials credentials)
        {            
            PageOptions = pageOptions ?? throw new ArgumentNullException(nameof(pageOptions));

            if (string.IsNullOrEmpty(searchRoot))
                throw new ArgumentNullException(nameof(searchRoot));

            SearchRoot = searchRoot;
            UserFilter = new UserFilter(filterType, filter);
            Credentials = credentials;
        }
        public string SearchRoot { get; }
        public UserFilter UserFilter { get; }
        public QueryPageOptions PageOptions { get; }
        public DirectorySearcherCredentials Credentials { get; }
    }
}