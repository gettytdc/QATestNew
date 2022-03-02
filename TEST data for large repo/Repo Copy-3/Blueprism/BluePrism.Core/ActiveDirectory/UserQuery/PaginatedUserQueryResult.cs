using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BluePrism.Core.ActiveDirectory.UserQuery
{
    [Serializable, DataContract(Namespace = "bp")]
    public class PaginatedUserQueryResult
    {        
        [DataMember]
        private readonly QueryStatus _queryStatus; 
        [DataMember]
        private readonly IEnumerable<ActiveDirectoryUser> _users;
        [DataMember]
        private readonly int _totalUsers;

        private PaginatedUserQueryResult(QueryStatus queryStatus, IEnumerable<ActiveDirectoryUser> users, int totalUsers)
        {
            _queryStatus = queryStatus;
            _users = users;
            _totalUsers = totalUsers;
        }

        public static PaginatedUserQueryResult Success(IEnumerable<ActiveDirectoryUser> users, int totalUsers)
            => new PaginatedUserQueryResult(QueryStatus.Success, users, totalUsers);

        public static PaginatedUserQueryResult InvalidCredentials()
            => new PaginatedUserQueryResult(QueryStatus.InvalidCredentials, null, 0);

        public static PaginatedUserQueryResult InvalidQuery()
            => new PaginatedUserQueryResult(QueryStatus.InvalidQuery, null, 0);

        public QueryStatus Status { get => _queryStatus; }
        public IEnumerable<ActiveDirectoryUser> RequestedPage { get => _users; }
        public int TotalUsers { get => _totalUsers; }
    }
}
