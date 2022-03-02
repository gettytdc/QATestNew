using BluePrism.Core.ActiveDirectory.UserQuery;

namespace BluePrism.Core.ActiveDirectory.DirectoryServices
{
    public interface IDirectorySearcherBuilder
    {
        IDirectorySearcher Build();
        IDirectorySearcherBuilder WithPaging(QueryPageOptions pageOptions);
        IDirectorySearcherBuilder WithSearchRoot(string path, DirectorySearcherCredentials credentials);
        IDirectorySearcherBuilder WithSortByCn();
        IDirectorySearcherBuilder WithUserFilter(UserFilter filter);
        IDirectorySearcherBuilder WithUserSearchColumns();
    }

}