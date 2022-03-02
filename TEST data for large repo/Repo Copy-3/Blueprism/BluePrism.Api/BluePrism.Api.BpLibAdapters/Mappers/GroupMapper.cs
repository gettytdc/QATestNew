namespace BluePrism.Api.BpLibAdapters.Mappers
{
    using System;
    using AutomateAppCore.Groups;
    using System.Collections.Generic;
    using System.Linq;

    public static class GroupMapper
    {
        public static IEnumerable<GroupMember> ToBluePrismObject(this IEnumerable<Domain.Groups.GroupMember> groupMembers) =>
            groupMembers.Select(ToBluePrismObject);

        private static GroupMember ToBluePrismObject(this Domain.Groups.GroupMember groupMember)
        {
            switch (groupMember)
            {
                case Domain.Groups.ResourceGroupMember member:
                    return new ResourceGroupMember((Guid)member.Id);
                default:
                    throw new ArgumentException("Invalid group member type");
            }
        }

        public static GroupTreeType ToBluePrismObject(this Domain.Groups.GroupTreeType groupTreeType) =>
            (GroupTreeType)groupTreeType;
    }
}
