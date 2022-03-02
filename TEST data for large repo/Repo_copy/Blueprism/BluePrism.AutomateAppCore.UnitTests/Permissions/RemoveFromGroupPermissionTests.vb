#If UNITTESTS Then

Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Server.Domain.Models
Imports NUnit.Framework
Imports Moq

<TestFixture>
Public Class RemoveFromGroupPermissionTests

#Region " Properties "

    Private Property ResourceTree As GroupTree
    Private Property TileTree As GroupTree
    Private Property AdminUser As IUser
    Private Property NormalUser As IUser
    Private ReadOnly Property AdminUserRoleId As Integer = 1
    Private ReadOnly Property NormalUserRoleId As Integer = 2
    Private ReadOnly Property NoEditGroupsRoleId As Integer = 3
    Private ReadOnly Property NoMangeRightsRoleId As Integer = 4

#End Region

#Region " Set-up/Tear-down "

    <SetUp>
    Public Sub InitialiseTrees()
        Dim mockServer = New Mock(Of IServer)(MockBehavior.Strict)
        mockServer.Setup(Function(x) x.GetPermissionData()).Returns(CreatePermissionDataObject())
        Permission.Init(mockServer.Object)

        ResourceTree = CreateResourceTree()
        TileTree = CreateTileTree()
        AdminUser = CreateAdminUser()
        NormalUser = CreateNormalUser(NormalUserRoleId)
    End Sub

    <TearDown>
    Public Sub DisposeTrees()
        ResourceTree = Nothing
        TileTree = Nothing
    End Sub

#End Region

#Region " Tests "

    ''' <summary>
    ''' Test that Remove From Group is not available for ungrouped members
    ''' </summary>
    <Test>
    Public Sub CheckNotAvailableIfNotInGroup()
        Dim ungroupedTile = TileTree.Root.GetMemberAtPath("Tile1")

        Assert.IsFalse(ungroupedTile.CanBeRemovedFromGroup(AdminUser))
    End Sub

    ''' <summary>
    ''' Test that Remove From Group is not available for groups
    ''' </summary>
    <Test()>
    Public Sub CheckNotAvailableIfNotAMember()
        Dim subGroup = ResourceTree.Root.GetMemberAtPath("Full Access Group/Sub Group")

        Assert.IsFalse(subGroup.CanBeRemovedFromGroup(AdminUser))
    End Sub

    ''' <summary>
    ''' Test that Remove From Group is not available if the member only exists
    ''' in the Default group.
    ''' </summary>
    <Test()>
    Public Sub CheckNotAvailableIfOnlyInDefaultGroup()
        Dim resource = ResourceTree.Root.GetMemberAtPath("Default/Resource1")

        Assert.IsFalse(resource.CanBeRemovedFromGroup(AdminUser))
    End Sub

    ''' <summary>
    ''' Test that Remove From Group is available if the item exists in other
    ''' groups besides the Default group.
    ''' </summary>
    <Test()>
    Public Sub CheckAvailableIfAlsoExistsOutsideDefaultGroup()
        Dim resource = ResourceTree.Root.GetMemberAtPath("Default/Resource2")

        Assert.IsTrue(resource.CanBeRemovedFromGroup(CreateNormalUser(NormalUserRoleId)))
    End Sub

    ''' <summary>
    ''' Test that Remove From Group is not available for Resource Pool members.
    ''' </summary>
    <Test()>
    Public Sub CheckNotAvailableIfMemberIsPool()
        Dim poolMember = ResourceTree.Root.GetMemberAtPath("Default/Pool/PoolMember")

        Assert.IsFalse(poolMember.CanBeRemovedFromGroup(AdminUser))
    End Sub

    ''' <summary>
    ''' Test that Remove From Group is not available if the user does not have
    ''' Edit Groups permission on the source group
    ''' </summary>
    <Test()>
    Public Sub CheckNotAvailableIfNoEditGroupsOnSource()
        Dim resource = ResourceTree.Root.GetMemberAtPath("No Edit Group/Resource3")

        Assert.IsFalse(resource.CanBeRemovedFromGroup(NormalUser))
    End Sub

    ''' <summary>
    ''' Test that Remove From Group is not available if the user does not have
    ''' Manage Access Rights on the source group
    ''' </summary>
    <Test()>
    Public Sub CheckNotAvailableIfNoManageRightsOnSource()
        Dim resource = ResourceTree.Root.GetMemberAtPath("No Rights Group/Resource4")

        Assert.IsFalse(resource.CanBeRemovedFromGroup(NormalUser))
    End Sub

    ''' <summary>
    ''' Test that Remove From Group is not available if theuer does not have Edit Groups
    ''' on the target group (i.e. Default group in this case)
    ''' </summary>
    <Test()>
    Public Sub CheckNotAvailableIfNoEditGroupsOnTarget()
        ' Change default group permissions to exclude Edit Groups
        ResourceTree.DefaultGroup.Permissions = New MemberPermissions(New GroupPermissions(PermissionState.Restricted) From {
            New GroupLevelPermissions(NormalUserRoleId) From {
                {Permission.GetPermission(Permission.Resources.ViewResource)},
                {Permission.GetPermission(Permission.Resources.ManageResourceAccessrights)}}})

        Dim resource = ResourceTree.Root.GetMemberAtPath("Full Access Group/Resource5")

        Assert.IsFalse(resource.CanBeRemovedFromGroup(NormalUser))
    End Sub

    ''' <summary>
    ''' Test that Remove From Group is not available if the user does not have Manage Rights
    ''' on the target group (i.e. Default group in this case)
    ''' </summary>
    <Test()>
    Public Sub CheckNotAvailableIfNoManageRightsOnTarget()
        ' Change default group permissions to exclude Manage Rights
        ResourceTree.DefaultGroup.Permissions = New MemberPermissions(New GroupPermissions(PermissionState.Restricted) From {
            New GroupLevelPermissions(NormalUserRoleId) From {
                {Permission.GetPermission(Permission.Resources.ViewResource)},
                {Permission.GetPermission(Permission.Resources.EditResourceGroups)}}})

        Dim resource = ResourceTree.Root.GetMemberAtPath("Full Access Group/Resource5")

        Assert.IsFalse(resource.CanBeRemovedFromGroup(NormalUser))
    End Sub

    ''' <summary>
    ''' Test that Remove From Group is available if the user has Edit Groups and 
    ''' Manage Rights on both source and target groups
    ''' </summary>
    <Test()>
    Public Sub CheckAvailableIfAccessOnSourceAndTarget()
        ' Change default group permissions to include both
        ResourceTree.DefaultGroup.Permissions = New MemberPermissions(New GroupPermissions(PermissionState.Restricted) From {
            New GroupLevelPermissions(NormalUserRoleId) From {
                {Permission.GetPermission(Permission.Resources.ViewResource)},
                {Permission.GetPermission(Permission.Resources.EditResourceGroups)},
                {Permission.GetPermission(Permission.Resources.ManageResourceAccessrights)}}})

        Dim resource = ResourceTree.Root.GetMemberAtPath("Full Access Group/Resource5")

        Assert.IsTrue(resource.CanBeRemovedFromGroup(NormalUser))
    End Sub

#End Region

#Region " Test data helpers "

    ''' <summary>
    ''' Builds a test Resource tree (i.e. with a default group)
    ''' </summary>
    Private Function CreateResourceTree() As GroupTree
        Dim tree = New GroupTree(GroupTreeType.Resources, True)

        ' Setup some permissions
        Dim unrestrictedPermissions = New GroupPermissions(PermissionState.UnRestricted)
        Dim fullAccessPermissions = New GroupPermissions(PermissionState.Restricted) From {
            New GroupLevelPermissions(NormalUserRoleId) From {
                {Permission.GetPermission(Permission.Resources.ViewResource)},
                {Permission.GetPermission(Permission.Resources.EditResourceGroups)},
                {Permission.GetPermission(Permission.Resources.ManageResourceAccessrights)}}}
        Dim noEditGroupsPermissions = New GroupPermissions(PermissionState.Restricted) From {
            New GroupLevelPermissions(NormalUserRoleId) From {
                {Permission.GetPermission(Permission.Resources.ViewResource)},
                {Permission.GetPermission(Permission.Resources.ManageResourceAccessrights)}}}
        Dim noManageRightsPermissions = New GroupPermissions(PermissionState.Restricted) From {
            New GroupLevelPermissions(NormalUserRoleId) From {
                {Permission.GetPermission(Permission.Resources.ViewResource)},
                {Permission.GetPermission(Permission.Resources.EditResourceGroups)}}}

        ' Create some groups with various permission restrictions
        Dim defaultGroup = New Group(True) With {
            .Id = Guid.NewGuid(), .Name = "Default", .Permissions = New MemberPermissions(unrestrictedPermissions)}

        Dim fullAccessGroup = New Group(False) With {
            .Id = Guid.NewGuid(), .Name = "Full Access Group", .Permissions = New MemberPermissions(fullAccessPermissions)}

        Dim noEditGroup = New Group(False) With {
            .Id = Guid.NewGuid(), .Name = "No Edit Group", .Permissions = New MemberPermissions(noEditGroupsPermissions)}

        Dim noRightsGroup = New Group(False) With {
            .Id = Guid.NewGuid(), .Name = "No Rights Group", .Permissions = New MemberPermissions(noManageRightsPermissions)}

        Dim subGroup = New Group(False) With {
            .Id = Guid.NewGuid(), .Name = "Sub Group"}

        ' Create some resources
        Dim resource1 = New ResourceGroupMember(New DictionaryDataProvider(New Hashtable() From {
            {"id", Guid.NewGuid()}, {"name", "Resource1"}, {"status", ResourceStatus.Offline}}))

        Dim resource2 = New ResourceGroupMember(New DictionaryDataProvider(New Hashtable() From {
            {"id", Guid.NewGuid()}, {"name", "Resource2"}, {"status", ResourceStatus.Offline}}))

        Dim resource3 = New ResourceGroupMember(New DictionaryDataProvider(New Hashtable() From {
            {"id", Guid.NewGuid()}, {"name", "Resource3"}, {"status", ResourceStatus.Offline}}))

        Dim resource4 = New ResourceGroupMember(New DictionaryDataProvider(New Hashtable() From {
            {"id", Guid.NewGuid()}, {"name", "Resource4"}, {"status", ResourceStatus.Offline}}))

        Dim resource5 = New ResourceGroupMember(New DictionaryDataProvider(New Hashtable() From {
            {"id", Guid.NewGuid()}, {"name", "Resource5"}, {"status", ResourceStatus.Offline}}))

        Dim pool = New ResourcePool(New DictionaryDataProvider(New Hashtable() From {
            {"id", Guid.NewGuid()}, {"name", "Pool"}}))

        Dim poolMember = New ResourceGroupMember(New DictionaryDataProvider(New Hashtable() From {
            {"Id", Guid.NewGuid()}, {"Name", "PoolMember"}, {"status", ResourceStatus.Offline}, {"ispoolmember", True}}))
        pool.Add(poolMember)

        ' Build the tree
        With tree.Root
            .Add(defaultGroup)
            .Add(fullAccessGroup)
            .Add(noEditGroup)
            .Add(noRightsGroup)
        End With

        With defaultGroup
            ' Resource1/Resource2 inherit permissions from this group
            .Add(resource1) : resource1.Permissions = defaultGroup.Permissions
            .Add(resource2) : resource2.Permissions = defaultGroup.Permissions
            .Add(pool)
        End With

        With fullAccessGroup
            ' Sub-group inherits permissions, but Resource 2 exists in default group as well
            .Add(subGroup) : subGroup.Permissions = fullAccessGroup.Permissions
            .Add(resource2)
            .Add(resource5) : resource5.Permissions = fullAccessGroup.Permissions
        End With

        With noEditGroup
            ' Resource3 inherits permissions from this group
            .Add(resource3) : resource3.Permissions = noEditGroup.Permissions
        End With

        With noRightsGroup
            ' Resource4 inherits permissions from this group
            .Add(resource4) : resource4.Permissions = noRightsGroup.Permissions
        End With

        Return tree
    End Function

    ''' <summary>
    ''' Builds a test Tile tree (no default group)
    ''' </summary>
    Private Function CreateTileTree() As GroupTree
        Dim tree = New GroupTree(GroupTreeType.Users, False)

        Dim tileGroup = New Group(True) With {.Id = Guid.NewGuid(), .Name = "Tile Group 1"}

        Dim tile1 = New TileGroupMember(New DictionaryDataProvider(New Hashtable() From {
            {"id", Guid.NewGuid()}, {"name", "Tile1"}, {"description", "Test tile - ungrouped"}, {"tiletype", TileTypes.Chart}}))

        Dim tile2 = New TileGroupMember(New DictionaryDataProvider(New Hashtable() From {
            {"id", Guid.NewGuid()}, {"name", "Tile2"}, {"description", "Test tile - grouped"}, {"tiletype", TileTypes.Chart}}))

        With tree.Root
            .Add(tileGroup)
            .Add(tile1)
        End With

        With tileGroup
            .Add(tile2)
        End With

        Return tree
    End Function

    ''' <summary>
    ''' Sets up test permissions data (subset of the Resources permissions group)
    ''' </summary>
    Private Function CreatePermissionDataObject() As PermissionData

        Dim permissions = New Dictionary(Of Integer, Permission) From {
            {1, Permission.CreatePermission(1, Permission.Resources.EditResourceGroups)},
            {2, Permission.CreatePermission(2, Permission.Resources.ManageResourceAccessrights)},
            {3, Permission.CreatePermission(3, Permission.Resources.ViewResource)}}

        Dim groups = New Dictionary(Of Integer, Auth.PermissionGroup) From {
            {1, New PermissionGroup(1, "Resources")}}

        Return New PermissionData(permissions, groups)
    End Function

    ''' <summary>
    ''' Sets up an Admin user i.e. full permissions
    ''' </summary>
    Private Function CreateAdminUser() As IUser
        Dim mockUser = New Mock(Of IUser)(MockBehavior.Strict)
        mockUser.Setup(Function(x) x.HasPermission(It.IsAny(Of Permission)())).Returns(True)
        mockUser.Setup(Function(x) x.HasPermission(It.IsAny(Of String)())).Returns(True)
        mockUser.Setup(Function(x) x.HasPermission(It.IsAny(Of ICollection(Of Permission))())).Returns(True)
        mockUser.Setup(Function(x) x.IsSystemAdmin()).Returns(True)
        mockUser.Setup(Function(x) x.AuthType).Returns(AuthMode.Native)

        Dim userRoleSet = New RoleSet()

        Dim dataProvider = New DictionaryDataProvider(New Hashtable() From {
            {"id", AdminUserRoleId}, {"name", "System Administrators"}, {"ssogroup", ""}})

        userRoleSet.Add(New Role(dataProvider))
        mockUser.Setup(Function(x) x.Roles).Returns(userRoleSet)
        Return mockUser.Object
    End Function

    ''' <summary>
    ''' Sets up a normal user with the passed role Id with access to the initialised
    ''' permission set. These can then be restricted at group level
    ''' </summary>
    ''' <param name="roleId">The Id to use for the role</param>
    Private Function CreateNormalUser(roleId As Integer) As IUser

        Dim dataProvider = New DictionaryDataProvider(New Hashtable() From {
            {"id", roleId}, {"name", $"Test Role {roleId}"}, {"ssogroup", ""}})

        Dim roleSet = New RoleSet() From {New Role(dataProvider) From {
                {Permission.GetPermission(Permission.Resources.ViewResource)},
                {Permission.GetPermission(Permission.Resources.EditResourceGroups)},
                {Permission.GetPermission(Permission.Resources.ManageResourceAccessrights)}}}

        Return New User(AuthMode.Native, Guid.NewGuid(), "TestUser", roleSet)
    End Function

#End Region

End Class

#End If
