using System;
using System.DirectoryServices;
using System.Linq;
using BluePrism.Core.ActiveDirectory.DirectoryServices;

namespace BluePrism.Core.ActiveDirectory.UserQuery
{
    public class ActiveDirectoryUserQuery : IActiveDirectoryUserQuery
    {        
        private readonly Func<IDirectorySearcherBuilder> _builderFactory;
        private readonly IMappedUserFinder _userFinder;

        public ActiveDirectoryUserQuery(Func<IDirectorySearcherBuilder> builderFactory, IMappedUserFinder userFinder)
        {
            _builderFactory = builderFactory;
            _userFinder = userFinder;
        }        
        
        public PaginatedUserQueryResult Run(ActiveDirectoryUserQueryOptions queryOptions)
        {
            using (var searcher = BuildDirectorySearcher(queryOptions))
            {
                ISearchResultCollection searchResult = null;
                
                try
                {
                    searchResult = searcher.FindAll();
                    var alreadyMappedUsers = _userFinder.AlreadyMappedSids(searchResult);

                    var users = searchResult
                                    .Select(user =>
                                    {
                                        var sid = user.Sid;
                                        return new ActiveDirectoryUser(user.UserPrincipalName, sid, user.DistinguishedName, 
                                            alreadyMappedUsers.Contains(sid));
                                    })                                    
                                    .ToList();

                    return PaginatedUserQueryResult.Success(users, searcher.ApproximateTotal);
                }
                catch (DirectoryServicesCOMException directoryServicesException) when (directoryServicesException.ErrorCode == -2147023570)
                {
                    return PaginatedUserQueryResult.InvalidCredentials();
                }                
                catch (Exception)
                {
                    return PaginatedUserQueryResult.InvalidQuery();
                }  
                finally
                {
                    searchResult?.Dispose();                    
                }
            }                                                                           
        }

        private IDirectorySearcher BuildDirectorySearcher(ActiveDirectoryUserQueryOptions queryOptions)
            => _builderFactory()
                    .WithSearchRoot(queryOptions.SearchRoot, queryOptions.Credentials)
                    .WithUserFilter(queryOptions.UserFilter)
                    .WithUserSearchColumns()
                    .WithSortByCn()
                    .WithPaging(queryOptions.PageOptions)
                    .Build();                                          
    }
}
