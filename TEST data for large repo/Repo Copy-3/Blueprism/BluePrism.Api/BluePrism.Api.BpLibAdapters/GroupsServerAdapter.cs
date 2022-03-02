namespace BluePrism.Api.BpLibAdapters
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Domain.Groups;
    using AutomateAppCore;
    using Func;
    using Mappers;
    using Domain.Errors;
    using BluePrism.Server.Domain.Models;

    using static ServerResultTask;
    using static Func.ResultHelper;

    public class GroupsServerAdapter : IGroupsServerAdapter
    {
        private readonly IServer _server;

        public GroupsServerAdapter(IServer server) => _server = server;

        public Task<Result> AddToGroup(GroupTreeType groupTreeType, Guid toGroupId, IEnumerable<GroupMember> groupMembers) =>
            RunOnServer(() =>
                _server.AddToGroup(groupTreeType.ToBluePrismObject(), toGroupId, groupMembers.ToBluePrismObject()))
                .Catch<PermissionException>(ex => Fail(new PermissionError(ex.Message)));

        public Task<Result<Guid>> GetDefaultGroupId(GroupTreeType groupTreeType) =>
            RunOnServer(() => _server.GetDefaultGroupId(groupTreeType.ToBluePrismObject()));
    }
}
