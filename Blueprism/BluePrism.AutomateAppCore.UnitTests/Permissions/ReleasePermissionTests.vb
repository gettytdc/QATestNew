#If UNITTESTS Then

Imports NUnit.Framework
Imports Moq
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.UnitTesting.TestSupport
Imports BluePrism.AutomateAppCore.clsServerPartialClasses
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.AutomateAppCore.ProcessComponent
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.Server.Domain.Models

<TestFixture>
Public Class ReleasePermissionTests

    ''' <summary>
    ''' Test that any unrestricted components are incuded in the release.
    ''' </summary>
    <Test>
    Public Sub CreateRelease_AllComponentsInPackageUnrestricted_AllComponentsInRelease()

        Dim serverMock = New Mock(Of IServer)

        serverMock.Setup(Function(x) x.GetPathsToMember(Moq.It.IsAny(Of GroupMember))).
            Returns(New Dictionary(Of Guid, String))


        ' Mock the server GetEffectiveMemberPermissions method. 
        ' All  items are unrestricted
        serverMock.Setup(Function(x) x.GetEffectiveMemberPermissions(Moq.It.IsAny(Of IGroupMember))).Returns(New MemberPermissions(Nothing) With {.State = PermissionState.UnRestricted})

        SetupMockServer(serverMock.Object)

        InitPermissions()

        Dim userMock = New Mock(Of IUser)
        Dim roles = New RoleSet()
        roles.Add(New Role("Test Role") With {.Id = 1})
        userMock.Setup(Function(x) x.HasPermission(Moq.It.IsAny(Of ICollection(Of Permission)))).Returns(True)

        userMock.Setup(Function(x) x.Roles).Returns(roles)

        ' Assemble package.
        Dim package = New clsPackage("Test package", DateTime.UtcNow, "User")
        package.Id = New Guid()

        package.Add(New GroupComponent(package, New Guid(), "Test Group", PackageComponentType.Process, False) With {.AssociatedData = New Group(False)})

        Dim processComp = New ProcessComponent(package, New Guid(), "Test Process")
        Dim processWrapper = New ProcessWrapper(processComp, String.Empty)
        Dim process = New clsProcess(Nothing, DiagramType.Process, False) With {.Id = New Guid()}
        ReflectionHelper.SetPrivateField(Of ProcessWrapper)("mCachedProcess", processWrapper, process)
        processComp.AssociatedData = processWrapper
        package.Add(processComp)


        Dim vboComp = New VBOComponent(package, New Guid(), "Test VBO")
        Dim vboProcessWrapper = New ProcessWrapper(vboComp, String.Empty)
        Dim vbo = New clsProcess(Nothing, DiagramType.Object, False) With {.Id = New Guid()}
        ReflectionHelper.SetPrivateField(Of ProcessWrapper)("mCachedProcess", vboProcessWrapper, vbo)
        vboComp.AssociatedData = vboProcessWrapper
        package.Add(vboComp)

        ' Create release from package.
        Dim release = New clsRelease(package, "Test", False)

        ' Generate release contents from package.
        release.GenerateContents(userMock.Object)

        ' check the package and release contain the same items.
        Dim differences = GetDifferenceBetweenPackageAndRelease(release, package)
        Assert.AreEqual(0, differences.Count)

        ReflectionHelper.SetPrivateField(GetType(app), "ServerFactory", Nothing, Nothing)

    End Sub

    ''' <summary>
    ''' Test that if the process component is restricted and the user doesn't have export permission on the referenced process, an expcetion is thrown.
    ''' </summary>
    <Test>
    Public Sub CreateRelease_ProcessComponentRestricted_UserDoesntHaveExportPermission_ExceptionThrown()

        Dim serverMock = New Mock(Of IServer)

        serverMock.Setup(Function(x) x.GetPathsToMember(Moq.It.IsAny(Of GroupMember))).
            Returns(New Dictionary(Of Guid, String))


        InitPermissions()

        ' Mock the server GetEffectiveMemberPermissions method. 
        ' Item is restricted, and user has not got export permission.
        Dim groupPerms = New GroupPermissions(PermissionState.Restricted)
        Dim grpLevelPerms = New GroupLevelPermissions(1)

        groupPerms.Add(grpLevelPerms)
        serverMock.Setup(Function(x) x.GetEffectiveMemberPermissions(Moq.It.IsAny(Of IGroupMember))).Returns(New MemberPermissions(groupPerms) With {.State = PermissionState.Restricted})

        SetupMockServer(serverMock.Object)

        Dim userMock = New Mock(Of IUser)
        Dim roles = New RoleSet()
        roles.Add(New Role("Test Role") With {.Id = 1})
        userMock.Setup(Function(x) x.HasPermission(Moq.It.IsAny(Of ICollection(Of Permission)))).Returns(True)
        userMock.Setup(Function(x) x.Roles).Returns(roles)

        ' Assemble package.
        Dim package = New clsPackage("Test package", DateTime.UtcNow, "User")
        package.Id = New Guid()


        Dim processId = New Guid()
        Dim processComp = New ProcessComponent(package, New Guid(), "Test Process")
        Dim processWrapper = New ProcessWrapper(processComp, String.Empty)
        Dim process = New clsProcess(Nothing, DiagramType.Process, False) With {.Id = processId}
        ReflectionHelper.SetPrivateField(Of ProcessWrapper)("mCachedProcess", processWrapper, process)
        processComp.AssociatedData = processWrapper
        package.Add(processComp)

        ' Create release from package.
        Dim release = New clsRelease(package, "Test", False)

        ' check the exception is thrown as expected when generating release contents.
        Dim exception = Assert.Throws(Of BluePrismException)(Sub() release.GenerateContents(userMock.Object))
        ReflectionHelper.SetPrivateField(GetType(app), "ServerFactory", Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Test that if the process component is restricted and the user has export permission on the referenced vbo, the vbo component is included in the release.
    ''' </summary>
    <Test>
    Public Sub CreateRelease_ProcessRestricted_UserHasExportPermission_ComponentIncludedInRelease()

        Dim serverMock = New Mock(Of IServer)

        serverMock.Setup(Function(x) x.GetPathsToMember(Moq.It.IsAny(Of GroupMember))).
            Returns(New Dictionary(Of Guid, String))


        InitPermissions()

        ' Mock the server GetEffectiveMemberPermissions method. 
        ' Item is restricted, and user has export permission.
        Dim groupPerms = New GroupPermissions(PermissionState.Restricted)
        Dim grpLevelPerms = New GroupLevelPermissions(1)
        grpLevelPerms.Add(Permission.ProcessStudio.ExportProcess)
        groupPerms.Add(grpLevelPerms)
        serverMock.Setup(Function(x) x.GetEffectiveMemberPermissions(Moq.It.IsAny(Of IGroupMember))).Returns(New MemberPermissions(groupPerms) With {.State = PermissionState.Restricted})
        SetupMockServer(serverMock.Object)

        Dim userMock = New Mock(Of IUser)
        Dim roles = New RoleSet()
        roles.Add(New Role("Test Role") With {.Id = 1})
        userMock.Setup(Function(x) x.HasPermission(Moq.It.IsAny(Of ICollection(Of Permission)))).Returns(True)
        userMock.Setup(Function(x) x.Roles).Returns(roles)

        ' Assemble package.
        Dim package = New clsPackage("Test package", DateTime.UtcNow, "User")
        package.Id = New Guid()


        Dim processId = New Guid()
        Dim processComp = New ProcessComponent(package, New Guid(), "Test Process")
        Dim processWrapper = New ProcessWrapper(processComp, String.Empty)
        Dim process = New clsProcess(Nothing, DiagramType.Process, False) With {.Id = processId}
        ReflectionHelper.SetPrivateField(Of ProcessWrapper)("mCachedProcess", processWrapper, process)
        processComp.AssociatedData = processWrapper
        package.Add(processComp)

        ' Create release from package.
        Dim release = New clsRelease(package, "Test", False)

        ' Generate release contents from package.
        release.GenerateContents(userMock.Object)

        ' check the package and release contain the same items.
        Dim differences = GetDifferenceBetweenPackageAndRelease(release, package)
        Assert.AreEqual(0, differences.Count)
        ReflectionHelper.SetPrivateField(GetType(app), "ServerFactory", Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Test that if the vbo component is restricted and the user has export permission on the referenced vbo, the vbo component is included in the release.
    ''' </summary>
    <Test>
    Public Sub CreateRelease_VBORestricted_UserHasExportPermission_ComponentIncludedInRelease()

        Dim serverMock = New Mock(Of IServer)

        serverMock.Setup(Function(x) x.GetPathsToMember(Moq.It.IsAny(Of GroupMember))).
            Returns(New Dictionary(Of Guid, String))


        InitPermissions()

        ' Mock the server GetEffectiveMemberPermissions method. 
        ' Item is restricted, and user has export permission.
        Dim groupPerms = New GroupPermissions(PermissionState.Restricted)
        Dim grpLevelPerms = New GroupLevelPermissions(1)
        grpLevelPerms.Add(Permission.ObjectStudio.ExportBusinessObject)
        groupPerms.Add(grpLevelPerms)
        serverMock.Setup(Function(x) x.GetEffectiveMemberPermissions(Moq.It.IsAny(Of IGroupMember))).Returns(New MemberPermissions(groupPerms) With {.State = PermissionState.Restricted})
        SetupMockServer(serverMock.Object)

        Dim userMock = New Mock(Of IUser)
        Dim roles = New RoleSet()
        roles.Add(New Role("Test Role") With {.Id = 1})
        userMock.Setup(Function(x) x.HasPermission(Moq.It.IsAny(Of ICollection(Of Permission)))).Returns(True)
        userMock.Setup(Function(x) x.Roles).Returns(roles)

        ' Assemble package.
        Dim package = New clsPackage("Test package", DateTime.UtcNow, "User")
        package.Id = New Guid()

        Dim vboComp = New VBOComponent(package, New Guid(), "Test VBO")
        Dim vboProcessWrapper = New ProcessWrapper(vboComp, String.Empty)
        Dim vbo = New clsProcess(Nothing, DiagramType.Object, False) With {.Id = New Guid()}
        ReflectionHelper.SetPrivateField(Of ProcessWrapper)("mCachedProcess", vboProcessWrapper, vbo)
        vboComp.AssociatedData = vboProcessWrapper
        package.Add(vboComp)

        ' Create release from package.
        Dim release = New clsRelease(package, "Test", False)

        ' Generate release contents from package.
        release.GenerateContents(userMock.Object)

        ' check the package and release contain the same items.
        Dim differences = GetDifferenceBetweenPackageAndRelease(release, package)
        Assert.AreEqual(0, differences.Count)
        ReflectionHelper.SetPrivateField(GetType(app), "ServerFactory", Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Test that if the vbo component is restricted and the user doesn't have export permission on the referenced vbo, an expcetion is thrown.
    ''' </summary>
    <Test>
    Public Sub CreateRelease_VBORestricted_UserHasNotExportPermission_ExceptionThrown()

        Dim serverMock = New Mock(Of IServer)

        serverMock.Setup(Function(x) x.GetPathsToMember(Moq.It.IsAny(Of GroupMember))).
            Returns(New Dictionary(Of Guid, String))

        InitPermissions()

        ' Mock the server GetEffectiveMemberPermissions method. 
        ' Item is restricted, and user has not got export permission.

        Dim groupPerms = New GroupPermissions(PermissionState.Restricted)
        Dim grpLevelPerms = New GroupLevelPermissions(1)

        groupPerms.Add(grpLevelPerms)
        serverMock.Setup(Function(x) x.GetEffectiveMemberPermissions(Moq.It.IsAny(Of IGroupMember))).Returns(New MemberPermissions(groupPerms) With {.State = PermissionState.Restricted})
        SetupMockServer(serverMock.Object)

        Dim userMock = New Mock(Of IUser)
        Dim roles = New RoleSet()
        roles.Add(New Role("Test Role") With {.Id = 1})
        userMock.Setup(Function(x) x.HasPermission(Moq.It.IsAny(Of ICollection(Of Permission)))).Returns(True)
        userMock.Setup(Function(x) x.Roles).Returns(roles)

        ' Assemble package.
        Dim package = New clsPackage("Test package", DateTime.UtcNow, "User")
        package.Id = New Guid()

        Dim vboComp = New VBOComponent(package, New Guid(), "Test VBO")
        Dim vboProcessWrapper = New ProcessWrapper(vboComp, String.Empty)
        Dim vbo = New clsProcess(Nothing, DiagramType.Object, False) With {.Id = New Guid()}
        ReflectionHelper.SetPrivateField(Of ProcessWrapper)("mCachedProcess", vboProcessWrapper, vbo)
        vboComp.AssociatedData = vboProcessWrapper
        package.Add(vboComp)

        ' Create release from package.
        Dim release = New clsRelease(package, "Test", False)

        ' check the exception is thrown as expected when generating release contents.
        Dim exception = Assert.Throws(Of BluePrismException)(Sub() release.GenerateContents(userMock.Object))
        ReflectionHelper.SetPrivateField(GetType(app), "ServerFactory", Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Returns any components which were in the original package, but not incuded in the release.
    ''' </summary>
    ''' <param name="release"></param>
    ''' <param name="originalPackage"></param>
    ''' <returns></returns>
    Private Function GetDifferenceBetweenPackageAndRelease(release As ICollection(Of PackageComponent), originalPackage As ICollection(Of PackageComponent)) As ICollection(Of PackageComponent)
        Return originalPackage.
            Except(release).
            ToList()

    End Function


    Private Class MockMethods
        Public Shared Property AppServer As IServer

    End Class


    ''' <summary>
    ''' Initialises the Permission class with the permissions we are using for testing.
    ''' </summary>
    Private Sub InitPermissions()
        Dim exportProcessPerm = New TestPermission(30, Permission.ProcessStudio.ExportProcess)
        Dim exportObjectPerm = New TestPermission(29, Permission.ObjectStudio.ExportBusinessObject)

        ReflectionHelper.SetPrivateField(Of Permission)("mByName", Nothing, New Dictionary(Of String, Permission) From {
                                                        {exportProcessPerm.Name, exportProcessPerm},
                                                        {exportObjectPerm.Name, exportObjectPerm}
                                                        })

        ReflectionHelper.SetPrivateField(Of Permission)("mById", Nothing, New Dictionary(Of Integer, Permission) From {
                                                        {exportProcessPerm.Id, exportProcessPerm},
                                                        {exportObjectPerm.Id, exportObjectPerm}
                                                        })

    End Sub

    Private Sub SetupMockServer(serverMock As IServer)
        Dim serverManagerMock = New Mock(Of ServerManager)()
        serverManagerMock.SetupGet(Function(m) m.Server).Returns(serverMock)

        Dim serverFactoryMock = New Mock(Of ClientServerConnection.IServerFactory)()
        serverFactoryMock.SetupGet(Function(m) m.ServerManager).Returns(serverManagerMock.Object)

        ReflectionHelper.SetPrivateField(GetType(app), "ServerFactory", Nothing, serverFactoryMock.Object)
    End Sub

    Private Class TestPermission : Inherits Permission
        Public Sub New(id As Integer, name As String)
            MyBase.New(id, name, Feature.None)
        End Sub
    End Class

End Class

#End If
