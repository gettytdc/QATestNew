
namespace BluePrism.Core.ActiveDirectory.UserQuery
{
    public interface IActiveDirectoryUserQuery
    {
        PaginatedUserQueryResult Run(ActiveDirectoryUserQueryOptions queryOptions);
    }
}
