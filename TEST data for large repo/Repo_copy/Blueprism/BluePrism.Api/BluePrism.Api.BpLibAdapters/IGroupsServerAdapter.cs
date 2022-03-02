namespace BluePrism.Api.BpLibAdapters
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Func;
    using Domain.Groups;

    public interface IGroupsServerAdapter : IServerAdapter
    {
        Task<Result> AddToGroup(GroupTreeType groupTreeType, Guid toGroupId, IEnumerable<GroupMember> groupMembers);
        Task<Result<Guid>> GetDefaultGroupId(GroupTreeType groupTreeType);
    }
}
