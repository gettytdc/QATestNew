#If UNITTESTS Then

Imports NUnit.Framework
Imports Moq
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateProcessCore
Imports BluePrism.Data

<TestFixture>
Public Class ProcessDependencyPermissionLogicTests

    ''' <summary>
    ''' Test that a single process reference which the user has permission on returns an empty result set.
    ''' </summary>
    <Test>
    Public Sub ProcessDependencyPermissionLogic_clsProcessNameDependency_UserHasPermissionForVBODependency_ReturnEmptyResult()

        ' Setup: The process testProcess has a dependency on the vbo referencedProcess, which the user has permission on.
        ' [process: testProcess] -> [vbo: referencedProcess]

        Dim mockServer = New Mock(Of IServerPrivate)
        Dim logic = New ProcessDependencyPermissionLogic(mockServer.Object)

        Dim mockUser = New Mock(Of IUser)
        Dim mockConnection = New Mock(Of IDatabaseConnection)

        ' The test process to check if user has permission on all it's dependencies.
        Dim testProcessId = Guid.NewGuid()

        ' A process referenced by the test process.
        Dim referencedProcess = "RefProc1"
        Dim referencedProcessId = Guid.NewGuid()

        ' Add the referenced process as a dependency to the test process.
        Dim testProcessDependencies = New clsProcessDependencyList
        testProcessDependencies.Add(New clsProcessNameDependency(referencedProcess))

        ' Mock the server calls...

        ' Getting references of the test process returns the depency list set up above.
        mockServer.Setup(Function(x) x.GetExternalDependencies(mockConnection.Object, testProcessId)).Returns(testProcessDependencies)

        ' The referenced process has no dependencies of it's own.
        mockServer.Setup(Function(x) x.GetExternalDependencies(mockConnection.Object, referencedProcessId)).Returns(New clsProcessDependencyList())

        mockServer.Setup(Function(x) x.GetProcessIDByName(mockConnection.Object, referencedProcess, True)).Returns(referencedProcessId)

        '  The referenced process member permissions are unrestricted - the user has access to the process.
        mockServer.Setup(Function(x) x.GetEffectiveMemberPermissionsForProcess(mockConnection.Object, referencedProcessId)).Returns(New MemberPermissions(Nothing) With {.State = PermissionState.UnRestricted})

        ' Expected - there are no invalid references returned.
        Dim result = logic.GetInaccessibleReferences(mockConnection.Object, testProcessId, mockUser.Object,
                                                     Function(name As String) mockServer.Object.GetProcessIDByName(mockConnection.Object, name, True))
        Assert.IsEmpty(result)

    End Sub


    ''' <summary>
    ''' Test that if there is a process dependency the user does not have permission on, the process with that dependency is returned.
    ''' </summary>
    <Test>
    Public Sub ProcessDependencyPermissionLogic_clsProcessNameDependency_UserHasNotPermissionForVBODependency_ReturnProcessId()
        ' Setup: The process testProcess has a dependency on the vbo referencedProcess, which the user does not have permission on.
        ' [process: testProcess] -> [vbo: referencedProcess]

        Dim mockServer = New Mock(Of IServerPrivate)
        Dim logic = New ProcessDependencyPermissionLogic(mockServer.Object)

        Dim mockUser = New Mock(Of IUser)
        Dim mockConnection = New Mock(Of IDatabaseConnection)

        ' The test process to check if user has permission on all it's dependencies.
        Dim testProcessId = Guid.NewGuid()
        Dim testProcessName = "TestProc"

        ' A process referenced by the test process.
        Dim referencedProcess = "RefProc1"
        Dim referencedProcessId = Guid.NewGuid()

        ' Add the referenced process as a dependency to the test process.
        Dim testProcessDependencies = New clsProcessDependencyList
        testProcessDependencies.Add(New clsProcessNameDependency(referencedProcess))

        ' Mock the server calls...

        ' Getting references of the test process returns the depency list set up above.
        mockServer.Setup(Function(x) x.GetExternalDependencies(mockConnection.Object, testProcessId)).Returns(testProcessDependencies)

        ' The referenced process has no dependencies of it's own.
        mockServer.Setup(Function(x) x.GetExternalDependencies(mockConnection.Object, referencedProcessId)).Returns(New clsProcessDependencyList())

        mockServer.Setup(Function(x) x.GetProcessNameById(mockConnection.Object, testProcessId)).Returns(testProcessName)
        mockServer.Setup(Function(x) x.GetProcessIDByName(mockConnection.Object, referencedProcess, True)).Returns(referencedProcessId)

        '  The referenced process member permissions are restricted - the user has no access access to the process referenced by testProcess.
        mockServer.Setup(Function(x) x.GetEffectiveMemberPermissionsForProcess(mockConnection.Object, referencedProcessId)).Returns(New MemberPermissions(Nothing) With {.State = PermissionState.Restricted})

        Dim mockclsServer As New Mock(Of IServer)
        mockclsServer.Setup(Function(x) x.GetPermissionData()).Returns(CreatePermissionDataObject())
        Auth.Permission.Init(mockclsServer.Object)

        ' Expected - test process is in the list returned.
        Dim result = CType(logic.GetInaccessibleReferences(mockConnection.Object, testProcessId, mockUser.Object,
            Function(name As String) mockServer.Object.GetProcessIDByName(mockConnection.Object, name, True)), ICollection)
        Assert.Contains(testProcessName, result)

    End Sub


    ''' <summary>
    ''' Test that if there is 2 level deep depencency tree, and the user has permission on all dependencies, there are no processes returned.
    ''' </summary>
    <Test>
    Public Sub ProcessDependencyPermissionLogic_clsProcessNameDependency_UserHasPermissionForVBOSubDependency_ReturnEmptyResult()
        ' Setup: The process testProcess has a dependency on the referencedProcess vbo, which has a dependency on the
        ' subReferencedProcess vbo, which the user does have permission on.
        ' [process: testProcess] -> [vbo: referencedProcess] -> [vbo: subReferencedProcess]

        Dim mockServer = New Mock(Of IServerPrivate)
        Dim logic = New ProcessDependencyPermissionLogic(mockServer.Object)

        Dim mockUser = New Mock(Of IUser)
        Dim mockConnection = New Mock(Of IDatabaseConnection)

        ' The test process to check if user has permission on all it's dependencies.
        Dim testProcessId = Guid.NewGuid()
        Dim testProcessName = "TestProc"

        ' A process referenced by the test process.
        Dim referencedProcess = "RefProc1"
        Dim referencedProcessId = Guid.NewGuid()

        Dim subReferencedProcess = "SubRefProc2"
        Dim subReferencedProcessId = Guid.NewGuid()


        ' Add the referenced process as a dependency to the test process.
        Dim testProcessDependencies = New clsProcessDependencyList
        testProcessDependencies.Add(New clsProcessNameDependency(referencedProcess))


        Dim referencedProcessDependencies = New clsProcessDependencyList()
        referencedProcessDependencies.Add(New clsProcessNameDependency(subReferencedProcess))

        ' Mock the server calls...

        ' Getting references of the test process returns the depency list set up above.
        mockServer.Setup(Function(x) x.GetExternalDependencies(mockConnection.Object, testProcessId)).Returns(testProcessDependencies)

        mockServer.Setup(Function(x) x.GetExternalDependencies(mockConnection.Object, referencedProcessId)).Returns(referencedProcessDependencies)

        ' The sub-referenced process has no dependencies of it's own.
        mockServer.Setup(Function(x) x.GetExternalDependencies(mockConnection.Object, subReferencedProcessId)).Returns(New clsProcessDependencyList())


        mockServer.Setup(Function(x) x.GetProcessNameById(mockConnection.Object, testProcessId)).Returns(testProcessName)
        mockServer.Setup(Function(x) x.GetProcessNameById(mockConnection.Object, referencedProcessId)).Returns(referencedProcess)
        mockServer.Setup(Function(x) x.GetProcessIDByName(mockConnection.Object, referencedProcess, True)).Returns(referencedProcessId)
        mockServer.Setup(Function(x) x.GetProcessIDByName(mockConnection.Object, subReferencedProcess, True)).Returns(subReferencedProcessId)

        '  The referenced process member permissions are unrestricted, and the subreference process referenced by the reference process is
        ' unrestricted - User has access access to the subreferenced process.
        mockServer.Setup(Function(x) x.GetEffectiveMemberPermissionsForProcess(mockConnection.Object, referencedProcessId)).Returns(New MemberPermissions(Nothing) With {.State = PermissionState.UnRestricted})
        mockServer.Setup(Function(x) x.GetEffectiveMemberPermissionsForProcess(mockConnection.Object, subReferencedProcessId)).Returns(New MemberPermissions(Nothing) With {.State = PermissionState.UnRestricted})

        Dim mockclsServer As New Mock(Of IServer)
        mockclsServer.Setup(Function(x) x.GetPermissionData()).Returns(CreatePermissionDataObject())
        Auth.Permission.Init(mockclsServer.Object)

        ' Expected - reference process is in the list returned.
        Dim result = logic.GetInaccessibleReferences(mockConnection.Object, testProcessId, mockUser.Object,
            Function(name As String) mockServer.Object.GetProcessIDByName(mockConnection.Object, name, True))
        Assert.IsEmpty(result)

    End Sub


    ''' <summary>
    ''' Test that if there is a process dependency the user does not have permission on deeper in the tree, the process with that dependency is returned.
    ''' </summary>
    <Test>
    Public Sub ProcessDependencyPermissionLogic_clsProcessNameDependency_UserHasNotPermissionForVBOSubDependency_ReturnReferencedProcessId()
        ' Setup: The process testProcess has a dependency on the referencedProcess vbo, which has a dependency on the
        ' subReferencedProcess vbo, which the user does not have permission on.
        ' [process: testProcess] -> [vbo: referencedProcess] -> [vbo: subReferencedProcess]

        Dim mockServer = New Mock(Of IServerPrivate)
        Dim logic = New ProcessDependencyPermissionLogic(mockServer.Object)

        Dim mockUser = New Mock(Of IUser)
        Dim mockConnection = New Mock(Of IDatabaseConnection)

        ' The test process to check if user has permission on all it's dependencies.
        Dim testProcessId = Guid.NewGuid()
        Dim testProcessName = "TestProc"

        ' A process referenced by the test process.
        Dim referencedProcess = "RefProc1"
        Dim referencedProcessId = Guid.NewGuid()

        Dim subReferencedProcess = "SubRefProc2"
        Dim subReferencedProcessId = Guid.NewGuid()


        ' Add the referenced process as a dependency to the test process.
        Dim testProcessDependencies = New clsProcessDependencyList
        testProcessDependencies.Add(New clsProcessNameDependency(referencedProcess))


        Dim referencedProcessDependencies = New clsProcessDependencyList()
        referencedProcessDependencies.Add(New clsProcessNameDependency(subReferencedProcess))

        ' Mock the server calls...

        ' Getting references of the test process returns the depency list set up above.
        mockServer.Setup(Function(x) x.GetExternalDependencies(mockConnection.Object, testProcessId)).Returns(testProcessDependencies)

        mockServer.Setup(Function(x) x.GetExternalDependencies(mockConnection.Object, referencedProcessId)).Returns(referencedProcessDependencies)

        ' The sub-referenced process has no dependencies of it's own.
        mockServer.Setup(Function(x) x.GetExternalDependencies(mockConnection.Object, subReferencedProcessId)).Returns(New clsProcessDependencyList())


        mockServer.Setup(Function(x) x.GetProcessNameById(mockConnection.Object, testProcessId)).Returns(testProcessName)
        mockServer.Setup(Function(x) x.GetProcessNameById(mockConnection.Object, referencedProcessId)).Returns(referencedProcess)
        mockServer.Setup(Function(x) x.GetProcessIDByName(mockConnection.Object, referencedProcess, True)).Returns(referencedProcessId)
        mockServer.Setup(Function(x) x.GetProcessIDByName(mockConnection.Object, subReferencedProcess, True)).Returns(subReferencedProcessId)

        '  The referenced process member permissions are unrestricted, but the subreference process referenced by the reference process is
        ' restricted -User has no access access to the subreferenced process.
        mockServer.Setup(Function(x) x.GetEffectiveMemberPermissionsForProcess(mockConnection.Object, referencedProcessId)).Returns(New MemberPermissions(Nothing) With {.State = PermissionState.UnRestricted})
        mockServer.Setup(Function(x) x.GetEffectiveMemberPermissionsForProcess(mockConnection.Object, subReferencedProcessId)).Returns(New MemberPermissions(Nothing) With {.State = PermissionState.Restricted})

        Dim mockclsServer As New Mock(Of IServer)
        mockclsServer.Setup(Function(x) x.GetPermissionData()).Returns(CreatePermissionDataObject())
        Auth.Permission.Init(mockclsServer.Object)

        ' Expected - reference process is in the list returned.
        Dim result = CType(logic.GetInaccessibleReferences(mockConnection.Object, testProcessId, mockUser.Object,
            Function(name As String) mockServer.Object.GetProcessIDByName(mockConnection.Object, name, True)), ICollection)
        Assert.Contains(referencedProcess, result)

    End Sub

    ''' <summary>
    ''' Test that a single process reference which the user has permission on returns an empty result set.
    ''' </summary>
    <Test>
    Public Sub ProcessDependencyPermissionLogic_clsProcessIDDependency_UserHasPermissionForProcessDependency_ReturnEmptyResult()

        ' Setup: The process testProcess has a dependency on the referencedProcess process, which the user has permission on.
        ' [process: testProcess] -> [process: referencedProcess]

        Dim mockServer = New Mock(Of IServerPrivate)
        Dim logic = New ProcessDependencyPermissionLogic(mockServer.Object)

        Dim mockUser = New Mock(Of IUser)
        Dim mockConnection = New Mock(Of IDatabaseConnection)

        ' The test process to check if user has permission on all it's dependencies.
        Dim testProcessId = Guid.NewGuid()

        ' A process referenced by the test process.
        Dim referencedProcess = "RefProc1"
        Dim referencedProcessId = Guid.NewGuid()

        ' Add the referenced process as a dependency to the test process.
        Dim testProcessDependencies = New clsProcessDependencyList
        testProcessDependencies.Add(New clsProcessIDDependency(referencedProcessId))

        ' Mock the server calls...

        ' Getting references of the test process returns the depency list set up above.
        mockServer.Setup(Function(x) x.GetExternalDependencies(mockConnection.Object, testProcessId)).Returns(testProcessDependencies)

        ' The referenced process has no dependencies of it's own.
        mockServer.Setup(Function(x) x.GetExternalDependencies(mockConnection.Object, referencedProcessId)).Returns(New clsProcessDependencyList())

        mockServer.Setup(Function(x) x.GetProcessIDByName(mockConnection.Object, referencedProcess, True)).Returns(referencedProcessId)

        '  The referenced process member permissions are unrestricted - the user has access to the process.
        mockServer.Setup(Function(x) x.GetEffectiveMemberPermissionsForProcess(mockConnection.Object, referencedProcessId)).Returns(New MemberPermissions(Nothing) With {.State = PermissionState.UnRestricted})

        ' Expected - there are no invalid references returned.
        Dim result = logic.GetInaccessibleReferences(mockConnection.Object, testProcessId, mockUser.Object,
                Function(name As String) mockServer.Object.GetProcessIDByName(mockConnection.Object, name, True))
        Assert.IsEmpty(result)

    End Sub


    ''' <summary>
    ''' Test that if there is a process dependency the user does not have permission on, the process with that dependency is returned.
    ''' </summary>
    <Test>
    Public Sub ProcessDependencyPermissionLogic_clsProcessIDDependency_UserHasNotPermissionForProcessDependency_ReturnProcessId()

        ' Setup: The process testProcess has a dependency on the referencedProcess process, which the user does not have permission on.
        ' [process: testProcess] -> [process: referencedProcess]

        Dim mockServer = New Mock(Of IServerPrivate)
        Dim logic = New ProcessDependencyPermissionLogic(mockServer.Object)

        Dim mockUser = New Mock(Of IUser)
        Dim mockConnection = New Mock(Of IDatabaseConnection)

        ' The test process to check if user has permission on all it's dependencies.
        Dim testProcessId = Guid.NewGuid()
        Dim testProcessName = "TestProc"

        ' A process referenced by the test process.
        Dim referencedProcess = "RefProc1"
        Dim referencedProcessId = Guid.NewGuid()

        ' Add the referenced process as a dependency to the test process.
        Dim testProcessDependencies = New clsProcessDependencyList
        testProcessDependencies.Add(New clsProcessIDDependency(referencedProcessId))

        ' Mock the server calls...

        ' Getting references of the test process returns the depency list set up above.
        mockServer.Setup(Function(x) x.GetExternalDependencies(mockConnection.Object, testProcessId)).Returns(testProcessDependencies)

        ' The referenced process has no dependencies of it's own.
        mockServer.Setup(Function(x) x.GetExternalDependencies(mockConnection.Object, referencedProcessId)).Returns(New clsProcessDependencyList())

        mockServer.Setup(Function(x) x.GetProcessNameById(mockConnection.Object, testProcessId)).Returns(testProcessName)
        mockServer.Setup(Function(x) x.GetProcessIDByName(mockConnection.Object, referencedProcess, True)).Returns(referencedProcessId)

        '  The referenced process member permissions are restricted - the user has no access access to the process referenced by testProcess.
        mockServer.Setup(Function(x) x.GetEffectiveMemberPermissionsForProcess(mockConnection.Object, referencedProcessId)).Returns(New MemberPermissions(Nothing) With {.State = PermissionState.Restricted})

        Dim mockclsServer As New Mock(Of IServer)
        mockclsServer.Setup(Function(x) x.GetPermissionData()).Returns(CreatePermissionDataObject())
        Auth.Permission.Init(mockclsServer.Object)

        ' Expected - test process is in the list returned.
        Dim result = CType(logic.GetInaccessibleReferences(mockConnection.Object, testProcessId, mockUser.Object,
            Function(name As String) mockServer.Object.GetProcessIDByName(mockConnection.Object, name, True)), ICollection)
        Assert.Contains(testProcessName, result)

    End Sub


    ''' <summary>
    ''' Test that if there is 2 level deep dependency tree, and the user has permission on all dependencies, there are no processes returned.
    ''' </summary>
    <Test>
    Public Sub ProcessDependencyPermissionLogic_clsProcessIDDependency_UserHasPermissionForProcessSubDependency_ReturnEmptyResult()
        ' Setup: The process testProcess has a dependency on the referencedProcess process, which has a dependency on subReferencedProcess which the user does have permission on.
        ' [process: testProcess] -> [process: referencedProcess] -> [process: subReferencedProcess]

        Dim mockServer = New Mock(Of IServerPrivate)
        Dim logic = New ProcessDependencyPermissionLogic(mockServer.Object)

        Dim mockUser = New Mock(Of IUser)
        Dim mockConnection = New Mock(Of IDatabaseConnection)

        ' The test process to check if user has permission on all it's dependencies.
        Dim testProcessId = Guid.NewGuid()
        Dim testProcessName = "TestProc"

        ' A process referenced by the test process.
        Dim referencedProcess = "RefProc1"
        Dim referencedProcessId = Guid.NewGuid()

        Dim subReferencedProcess = "SubRefProc2"
        Dim subReferencedProcessId = Guid.NewGuid()


        ' Add the referenced process as a dependency to the test process.
        Dim testProcessDependencies = New clsProcessDependencyList
        testProcessDependencies.Add(New clsProcessIDDependency(referencedProcessId))


        Dim referencedProcessDependencies = New clsProcessDependencyList()
        referencedProcessDependencies.Add(New clsProcessIDDependency(subReferencedProcessId))

        ' Mock the server calls...

        ' Getting references of the test process returns the depency list set up above.
        mockServer.Setup(Function(x) x.GetExternalDependencies(mockConnection.Object, testProcessId)).Returns(testProcessDependencies)

        mockServer.Setup(Function(x) x.GetExternalDependencies(mockConnection.Object, referencedProcessId)).Returns(referencedProcessDependencies)

        ' The sub-referenced process has no dependencies of it's own.
        mockServer.Setup(Function(x) x.GetExternalDependencies(mockConnection.Object, subReferencedProcessId)).Returns(New clsProcessDependencyList())


        mockServer.Setup(Function(x) x.GetProcessNameById(mockConnection.Object, testProcessId)).Returns(testProcessName)
        mockServer.Setup(Function(x) x.GetProcessNameById(mockConnection.Object, referencedProcessId)).Returns(referencedProcess)
        mockServer.Setup(Function(x) x.GetProcessIDByName(mockConnection.Object, referencedProcess, True)).Returns(referencedProcessId)
        mockServer.Setup(Function(x) x.GetProcessIDByName(mockConnection.Object, subReferencedProcess, True)).Returns(subReferencedProcessId)

        '  The referenced process member permissions are unrestricted, and the subreference process referenced by the reference process is
        ' unrestricted - User has access access to the subreferenced process.
        mockServer.Setup(Function(x) x.GetEffectiveMemberPermissionsForProcess(mockConnection.Object, referencedProcessId)).Returns(New MemberPermissions(Nothing) With {.State = PermissionState.UnRestricted})
        mockServer.Setup(Function(x) x.GetEffectiveMemberPermissionsForProcess(mockConnection.Object, subReferencedProcessId)).Returns(New MemberPermissions(Nothing) With {.State = PermissionState.UnRestricted})

        Dim mockclsServer As New Mock(Of IServer)
        mockclsServer.Setup(Function(x) x.GetPermissionData()).Returns(CreatePermissionDataObject())
        Auth.Permission.Init(mockclsServer.Object)

        ' Expected - reference process is in the list returned.
        Dim result = logic.GetInaccessibleReferences(mockConnection.Object, testProcessId, mockUser.Object,
            Function(name As String) mockServer.Object.GetProcessIDByName(mockConnection.Object, name, True))
        Assert.IsEmpty(result)

    End Sub


    ''' <summary>
    ''' Test that if there is a process dependency the user does not have permission on deeper in the tree, the process with that dependency is returned.
    ''' </summary>
    <Test>
    Public Sub ProcessDependencyPermissionLogic_clsProcessIDDependency_UserHasNotPermissionForProcessSubDependency_ReturnReferencedProcessId()
        ' Setup: The process testProcess has a dependency on the referencedProcess process, which has a dependency on subReferencedProcess which the user does not have permission on.
        ' [process: testProcess] -> [process: referencedProcess] -> [process: subReferencedProcess]

        Dim mockServer = New Mock(Of IServerPrivate)
        Dim logic = New ProcessDependencyPermissionLogic(mockServer.Object)


        Dim mockUser = New Mock(Of IUser)
        Dim mockConnection = New Mock(Of IDatabaseConnection)

        ' The test process to check if user has permission on all it's dependencies.
        Dim testProcessId = Guid.NewGuid()
        Dim testProcessName = "TestProc"

        ' A process referenced by the test process.
        Dim referencedProcess = "RefProc1"
        Dim referencedProcessId = Guid.NewGuid()

        Dim subReferencedProcess = "SubRefProc2"
        Dim subReferencedProcessId = Guid.NewGuid()


        ' Add the referenced process as a dependency to the test process.
        Dim testProcessDependencies = New clsProcessDependencyList
        testProcessDependencies.Add(New clsProcessIDDependency(referencedProcessId))


        Dim referencedProcessDependencies = New clsProcessDependencyList()
        referencedProcessDependencies.Add(New clsProcessIDDependency(subReferencedProcessId))

        ' Mock the server calls...

        ' Getting references of the test process returns the depency list set up above.
        mockServer.Setup(Function(x) x.GetExternalDependencies(mockConnection.Object, testProcessId)).Returns(testProcessDependencies)

        mockServer.Setup(Function(x) x.GetExternalDependencies(mockConnection.Object, referencedProcessId)).Returns(referencedProcessDependencies)

        ' The sub-referenced process has no dependencies of it's own.
        mockServer.Setup(Function(x) x.GetExternalDependencies(mockConnection.Object, subReferencedProcessId)).Returns(New clsProcessDependencyList())


        mockServer.Setup(Function(x) x.GetProcessNameById(mockConnection.Object, testProcessId)).Returns(testProcessName)
        mockServer.Setup(Function(x) x.GetProcessNameById(mockConnection.Object, referencedProcessId)).Returns(referencedProcess)
        mockServer.Setup(Function(x) x.GetProcessIDByName(mockConnection.Object, referencedProcess, True)).Returns(referencedProcessId)
        mockServer.Setup(Function(x) x.GetProcessIDByName(mockConnection.Object, subReferencedProcess, True)).Returns(subReferencedProcessId)

        '  The referenced process member permissions are unrestricted, but the subreference process referenced by the reference process is
        ' restricted - User has no access access to the subreferenced process.
        mockServer.Setup(Function(x) x.GetEffectiveMemberPermissionsForProcess(mockConnection.Object, referencedProcessId)).Returns(New MemberPermissions(Nothing) With {.State = PermissionState.UnRestricted})
        mockServer.Setup(Function(x) x.GetEffectiveMemberPermissionsForProcess(mockConnection.Object, subReferencedProcessId)).Returns(New MemberPermissions(Nothing) With {.State = PermissionState.Restricted})

        Dim mockclsServer As New Mock(Of IServer)
        mockclsServer.Setup(Function(x) x.GetPermissionData()).Returns(CreatePermissionDataObject())
        Auth.Permission.Init(mockclsServer.Object)

        ' Expected - reference process is in the list returned.
        Dim result = CType(logic.GetInaccessibleReferences(mockConnection.Object, testProcessId, mockUser.Object,
            Function(name As String) mockServer.Object.GetProcessIDByName(mockConnection.Object, name, True)), ICollection)
        Assert.Contains(referencedProcess, result)

    End Sub


    ''' <summary>
    ''' Test with something a bit more... tree-like.
    ''' </summary>
    <Test>
    Public Sub ProcessDependencyPermissionLogic_clsProcessNameDependency_MultipleBranches_UserHasNotPermissionForVBODependency_ReturnProcessId()

        ' Setup: user has access to all dependencies except the subReferencedProcess2 vbo referenced by referencedProcess2 vbo.
        ' [process: testProcess] -> [vbo: referencedProcess] -> [vbo: subreferencedProcess]
        '                        -> [vbo: referencedProcess2] -> [vbo: subreferencedProcess2]

        Dim mockServer = New Mock(Of IServerPrivate)
        Dim logic = New ProcessDependencyPermissionLogic(mockServer.Object)

        Dim mockUser = New Mock(Of IUser)
        Dim mockConnection = New Mock(Of IDatabaseConnection)

        ' The test process to check if user has permission on all it's dependencies.
        Dim testProcessId = Guid.NewGuid()
        Dim testProcessName = "TestProc"

        ' A process referenced by the test process.
        Dim referencedProcess = "RefProc1"
        Dim referencedProcessId = Guid.NewGuid()

        Dim referencedProcess2 = "RefProc2"
        Dim referencedProcess2Id = Guid.NewGuid()

        Dim subReferencedProcess = "SubRefProc1"
        Dim subReferencedProcessId = Guid.NewGuid()

        Dim subReferencedProcess2 = "SubRefProc2"
        Dim subReferencedProcess2Id = Guid.NewGuid()


        ' Add the referenced process as a dependency to the test process.
        Dim testProcessDependencies = New clsProcessDependencyList
        testProcessDependencies.Add(New clsProcessNameDependency(referencedProcess))
        testProcessDependencies.Add(New clsProcessNameDependency(referencedProcess2))

        Dim referencedProcess1Dependencies = New clsProcessDependencyList
        referencedProcess1Dependencies.Add(New clsProcessNameDependency(subReferencedProcess))

        Dim referencedProcess2Dependencies = New clsProcessDependencyList
        referencedProcess2Dependencies.Add(New clsProcessNameDependency(subReferencedProcess2))


        ' Mock the server calls...

        ' Getting references of the test process returns the depency list set up above.
        mockServer.Setup(Function(x) x.GetExternalDependencies(mockConnection.Object, testProcessId)).Returns(testProcessDependencies)
        mockServer.Setup(Function(x) x.GetExternalDependencies(mockConnection.Object, referencedProcessId)).Returns(referencedProcess1Dependencies)
        mockServer.Setup(Function(x) x.GetExternalDependencies(mockConnection.Object, referencedProcess2Id)).Returns(referencedProcess2Dependencies)

        ' The subreferenced processes have no dependencies of their own.
        mockServer.Setup(Function(x) x.GetExternalDependencies(mockConnection.Object, subReferencedProcessId)).Returns(New clsProcessDependencyList())
        mockServer.Setup(Function(x) x.GetExternalDependencies(mockConnection.Object, subReferencedProcess2Id)).Returns(New clsProcessDependencyList())

        mockServer.Setup(Function(x) x.GetProcessNameById(mockConnection.Object, testProcessId)).Returns(testProcessName)
        mockServer.Setup(Function(x) x.GetProcessNameById(mockConnection.Object, referencedProcessId)).Returns(referencedProcess)
        mockServer.Setup(Function(x) x.GetProcessNameById(mockConnection.Object, referencedProcess2Id)).Returns(referencedProcess2)

        mockServer.Setup(Function(x) x.GetProcessIDByName(mockConnection.Object, referencedProcess, True)).Returns(referencedProcessId)
        mockServer.Setup(Function(x) x.GetProcessIDByName(mockConnection.Object, referencedProcess2, True)).Returns(referencedProcess2Id)
        mockServer.Setup(Function(x) x.GetProcessIDByName(mockConnection.Object, subReferencedProcess, True)).Returns(subReferencedProcessId)
        mockServer.Setup(Function(x) x.GetProcessIDByName(mockConnection.Object, subReferencedProcess2, True)).Returns(subReferencedProcess2Id)

        '  The referenced process member permissions are restricted - the user has no access access to the process referenced by testProcess.
        mockServer.Setup(Function(x) x.GetEffectiveMemberPermissionsForProcess(mockConnection.Object, referencedProcessId)).Returns(New MemberPermissions(Nothing) With {.State = PermissionState.UnRestricted})
        mockServer.Setup(Function(x) x.GetEffectiveMemberPermissionsForProcess(mockConnection.Object, referencedProcess2Id)).Returns(New MemberPermissions(Nothing) With {.State = PermissionState.UnRestricted})
        mockServer.Setup(Function(x) x.GetEffectiveMemberPermissionsForProcess(mockConnection.Object, subReferencedProcessId)).Returns(New MemberPermissions(Nothing) With {.State = PermissionState.UnRestricted})
        mockServer.Setup(Function(x) x.GetEffectiveMemberPermissionsForProcess(mockConnection.Object, subReferencedProcess2Id)).Returns(New MemberPermissions(Nothing) With {.State = PermissionState.Restricted})


        Dim mockclsServer As New Mock(Of IServer)
        mockclsServer.Setup(Function(x) x.GetPermissionData()).Returns(CreatePermissionDataObject())
        Auth.Permission.Init(mockclsServer.Object)

        ' Expected - referencedProcess2 is in the list returned.
        Dim result = CType(logic.GetInaccessibleReferences(mockConnection.Object, testProcessId, mockUser.Object,
            Function(name As String) mockServer.Object.GetProcessIDByName(mockConnection.Object, name, True)), ICollection)
        Assert.Contains(referencedProcess2, result)
        Assert.IsTrue(result.Count = 1)
    End Sub

    Private Function CreatePermissionDataObject() As Auth.PermissionData
        Dim perms = New Dictionary(Of Integer, Auth.Permission)()
        perms.Add(83, Auth.Permission.CreatePermission(83, Auth.Permission.ProcessStudio.ManageProcessAccessRights))
        perms.Add(73, Auth.Permission.CreatePermission(73, Auth.Permission.ProcessStudio.EditProcessGroups))
        perms.Add(56, Auth.Permission.CreatePermission(56, Auth.Permission.ObjectStudio.ExecuteBusinessObject))
        perms.Add(26, Auth.Permission.CreatePermission(26, Auth.Permission.ObjectStudio.EditBusinessObject))
        perms.Add(27, Auth.Permission.CreatePermission(27, Auth.Permission.ProcessStudio.EditProcess))
        perms.Add(57, Auth.Permission.CreatePermission(57, Auth.Permission.ProcessStudio.ExecuteProcess))

        perms.Add(19, Auth.Permission.CreatePermission(19, Auth.Permission.ObjectStudio.CreateBusinessObject))
        perms.Add(20, Auth.Permission.CreatePermission(20, Auth.Permission.ProcessStudio.CreateProcess))
        Dim groups = New Dictionary(Of Integer, Auth.PermissionGroup)()
        groups.Add(4, New Auth.PermissionGroup(4, "Process Studio"))
        groups.Add(2, New Auth.PermissionGroup(4, "Object Studio"))
        Dim retval = New Auth.PermissionData(perms, groups)
        Return retval
    End Function
End Class
#End If
