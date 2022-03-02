

Namespace Groups

    ''' <summary>
    ''' The types of objects which can be added as a member of a group
    ''' </summary>
    Public Enum GroupMemberType
        None = 0

        <GroupMember("Group", GetType(Group), "BPVGroupedGroups")>
        Group

        <GroupMember("Pool", GetType(ResourcePool), "BPVPools")>
        Pool

        <GroupMember("Process",
                     GetType(ProcessGroupMember),
                     "BPVGroupedProcesses",
                     New String() {"name", "attributes", "webservicename", "useLegacyNamespace", "sharedObject"}
                     )>
        Process

        <GroupMember("Object",
                     GetType(ObjectGroupMember),
                     "BPVGroupedObjects",                     
                     New String() {"name", "attributes", "webservicename", "useLegacyNamespace", "sharedObject"}
                    )>
        [Object]

        <GroupMember("Tile", GetType(TileGroupMember), "BPVGroupedTiles")>
        Tile

        <GroupMember("Queue", GetType(QueueGroupMember), "BPVGroupedQueues")>
        Queue

        <GroupMember("Resource", GetType(ResourceGroupMember), "BPVGroupedResources")>
        Resource

        <GroupMember("User", GetType(UserGroupMember), "BPVGroupedUsers")>
        User


    End Enum

End Namespace