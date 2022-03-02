Imports BluePrism.Images
Imports BluePrism.AutomateAppCore.Auth


Namespace Groups

    ''' <summary>
    ''' Types of available tree structures.
    ''' </summary>
    ''' <remarks>This is the code-side equivalent of the BPATree table on the
    ''' database. If it's changed in one place it must be changed in the other.
    ''' </remarks>
    Public Enum GroupTreeType As Integer

        None = 0

        <TreeDefinition("Tile", "Tiles", "TileGroup",
                        ImageLists.Keys.Component.TileTree,
                        Permission.Analytics.CreateEditDeleteTiles,
                        "",
                        Permission.Analytics.CreateEditDeleteTiles,
                        Permission.Analytics.CreateEditDeleteTiles,
                        "",
                        GroupMemberType.Tile)>
        Tiles = 1

        <TreeDefinition("Process", "Processes", "ProcessGroup",
                        ImageLists.Keys.Component.ProcessGroup,
                        Permission.ProcessStudio.EditProcessGroups,
                        Permission.ProcessStudio.ManageProcessAccessRights,
                        Permission.ProcessStudio.CreateProcess,
                        Permission.ProcessStudio.DeleteProcess,
                        Permission.ProcessStudio.ExportProcess,
                        GroupMemberType.Process)>
        Processes = 2

        <TreeDefinition("Object", "Objects", "ObjectGroup",
                        ImageLists.Keys.Component.ObjectGroup,
                        Permission.ObjectStudio.EditObjectGroups,
                        Permission.ObjectStudio.ManageBusinessObjectAccessRights,
                        Permission.ObjectStudio.CreateBusinessObject,
                        Permission.ObjectStudio.DeleteBusinessObject,
                        Permission.ObjectStudio.ExportBusinessObject,
                        GroupMemberType.Object)>
        Objects = 3

        <TreeDefinition("Queue", "Queues", "QueueGroup",
                        ImageLists.Keys.Component.QueueGroup,
                        GroupMemberType.Queue)>
        Queues = 4

        <TreeDefinition("Resource", "Resources", "ResourceGroup",
                        ImageLists.Keys.Component.ClosedGroup,
                        Permission.Resources.EditResourceGroups,
                        Permission.Resources.ManageResourceAccessrights,
                        "",
                        "",
                        "",
                        GroupMemberType.Pool,
                        GroupMemberType.Resource)>
        Resources = 5

        <TreeDefinition("User", "Users", "UserGroup",
                        ImageLists.Keys.Component.UserGroup,
                        GroupMemberType.User)>
        Users = 6


    End Enum

End Namespace
