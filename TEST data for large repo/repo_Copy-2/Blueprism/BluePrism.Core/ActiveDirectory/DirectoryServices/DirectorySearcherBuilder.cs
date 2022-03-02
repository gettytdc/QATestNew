using BluePrism.Common.Security;
using BluePrism.Core.ActiveDirectory.UserQuery;
using System;
using System.DirectoryServices;

namespace BluePrism.Core.ActiveDirectory.DirectoryServices
{
   
    public class DirectorySearcherBuilder : IDirectorySearcherBuilder
    {        
        private IDirectorySearcher _directorySearcher;
        
        public DirectorySearcherBuilder(Func<IDirectorySearcher> directorySearcherFactory)
        {
            _directorySearcher = directorySearcherFactory();
        }

        public IDirectorySearcherBuilder WithSearchRoot(string path, DirectorySearcherCredentials credentials)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            var searchRoot = new DirectoryEntry() { Path = $"GC://{path}" };
            if (credentials != null)
            {
                searchRoot.Username = credentials.Username;
                searchRoot.Password = credentials.Password.AsString();
            }                

            _directorySearcher.SearchRoot(searchRoot);
            return this;
        }

        public IDirectorySearcherBuilder WithUserFilter(UserFilter filter)
        {
            if (filter is null)
                throw new ArgumentNullException(nameof(filter));

            _directorySearcher.Filter(filter.LdapFilter);
            return this;
        }

        public IDirectorySearcherBuilder WithSortByCn()
        {
            _directorySearcher.Sort(new SortOption(LdapAttributes.Cn, SortDirection.Ascending));
            return this;
        }

        public IDirectorySearcherBuilder WithUserSearchColumns()
        {
            _directorySearcher.AddPropertyToLoad(LdapAttributes.Dn);
            _directorySearcher.AddPropertyToLoad(LdapAttributes.UserPrincipalName);
            _directorySearcher.AddPropertyToLoad(LdapAttributes.Sid);
            return this;
        }

        public IDirectorySearcherBuilder WithPaging(QueryPageOptions pageOptions)
        {
            if (pageOptions is null)
                throw new ArgumentNullException(nameof(pageOptions));

            var oneBasedStartIndex = pageOptions.StartIndex + 1;
            var afterCount = pageOptions.PageSize - 1;

            _directorySearcher.VirtualListView(new DirectoryVirtualListView(0, afterCount, oneBasedStartIndex));
            return this;
        }

        public IDirectorySearcher Build() => _directorySearcher;

    }


    
}
