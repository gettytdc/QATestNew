#If UNITTESTS Then


Imports NUnit.Framework
Imports Moq
Imports BluePrism.AutomateAppCore.Auth
Imports System.Data.SqlClient
Imports BluePrism.AutomateAppCore.clsServerPartialClasses
Imports BluePrism.UnitTesting.TestSupport
Imports BluePrism.Data
Imports BluePrism.UnitTesting

<TestFixture>
Public Class ProcessAlertsTests

    <SetUp>
    Public Sub Setup()
        LegacyUnitTestHelper.SetupDependencyResolver()
    End Sub

    ''' <summary>
    ''' Tests that if the process is unrestricted the process is returned from the GetAlertProcessDetails method.
    ''' </summary>
    <Test>
    Public Sub GetAlertProcessDetails_ProcessIsUnrestricted_ReturnsProcess()

        Dim server = New clsServer()
        Dim processid = Guid.NewGuid()
        Dim databaseConnectionMock = New Mock(Of IDatabaseConnection)()
        Dim dataReaderMock = New Mock(Of IDataReader)()
        Dim userMock = New Mock(Of IUser)()

        Dim roleSet = New RoleSet From {
                New Role("Test") With {.Id = 1}
                }

        userMock.
            SetupGet(Function(m) m.Roles).
            Returns(roleSet)

        Dim dataTable = New DataTable()
        dataTable.Columns.Add("ProcessID", GetType(Guid))
        dataTable.Columns.Add("Name", GetType(String))
        dataTable.Columns.Add("Description", GetType(String))
        dataTable.Columns.Add("Checked", GetType(Boolean))

        dataTable.Rows.Add(processid, "TestProcess", "A Test", True)

        databaseConnectionMock.
            Setup(Function(x) x.ExecuteReturnDataTable(It.IsAny(Of SqlCommand)())).
            Returns(Function() dataTable)

        Dim getEffectivePermsFunc = New Func(Of Guid, IMemberPermissions)(Function(id) New MemberPermissions(Nothing) With {.State = PermissionState.UnRestricted})

        Dim result = CType(ReflectionHelper.InvokePrivateMethod("GetAlertProcessDetails", server, databaseConnectionMock.Object, userMock.Object, getEffectivePermsFunc), DataTable)

        Assert.AreEqual(1, result.Rows.Count)
        Assert.AreEqual(result.Rows(0)("ProcessID"), processid)
    End Sub


    ''' <summary>
    ''' Tests that if the process is restricted and the user any permission on the process, the process is returned from the GetAlertProcessDetails method.
    ''' </summary>
    ''' 
    <Test>
    <TestCaseSource(NameOf(ProcessPermissionGenerator))>
    Public Sub GetAlertProcessDetails_ProcessRestrictedUserHasAnyPermission_ReturnsProcess(perm As Permission)

        Dim server = New clsServer()
        Dim processid = Guid.NewGuid()
        Dim databaseConnectionMock = New Mock(Of IDatabaseConnection)()
        Dim dataReaderMock = New Mock(Of IDataReader)()
        Dim userMock = New Mock(Of IUser)()


        Dim dataTable = New DataTable()
        dataTable.Columns.Add("ProcessID", GetType(Guid))
        dataTable.Columns.Add("Name", GetType(String))
        dataTable.Columns.Add("Description", GetType(String))
        dataTable.Columns.Add("Checked", GetType(Boolean))

        dataTable.Rows.Add(processid, "TestProcess", "A Test", True)

        databaseConnectionMock.
            Setup(Function(x) x.ExecuteReturnDataTable(It.IsAny(Of SqlCommand)())).
            Returns(Function() dataTable)

        userMock.
            Setup(Function(m) m.HasPermission(It.IsAny(Of ICollection(Of Permission))())).
            Returns(True)

        Dim roleSet = New RoleSet From {
                New Role("Test") With {.Id = 1}
                }

        userMock.
            SetupGet(Function(m) m.Roles).
            Returns(roleSet)

        InitPermissions()

        Dim processPerms = CreateTestMemberPermissions(PermissionState.Restricted, perm)

        Dim getEffectivePermsFunc = New Func(Of Guid, IMemberPermissions)(Function(id) processPerms)

        Dim result = CType(ReflectionHelper.InvokePrivateMethod("GetAlertProcessDetails", server, databaseConnectionMock.Object, userMock.Object, getEffectivePermsFunc), DataTable)

        Assert.AreEqual(1, result.Rows.Count)
        Assert.AreEqual(result.Rows(0)("ProcessID"), processid)
    End Sub

    ''' <summary>
    ''' If the process is restricted and the user has no permissions on the process it is not returned from the GetAlertProcessDetails method.
    ''' </summary>
    <Test>
    Public Sub GetAlertProcessDetails_ProcessRestrictedUserHasNoPermissions_ReturnsEmptyDataTable()

        Dim server = New clsServer()
        Dim processid = Guid.NewGuid()
        Dim databaseConnectionMock = New Mock(Of IDatabaseConnection)()
        Dim dataReaderMock = New Mock(Of IDataReader)()
        Dim userMock = New Mock(Of IUser)()

        Dim dataTable = New DataTable()
        dataTable.Columns.Add("ProcessID", GetType(Guid))
        dataTable.Columns.Add("Name", GetType(String))
        dataTable.Columns.Add("Description", GetType(String))
        dataTable.Columns.Add("Checked", GetType(Boolean))

        dataTable.Rows.Add(processid, "TestProcess", "A Test", True)

        databaseConnectionMock.
            Setup(Function(x) x.ExecuteReturnDataTable(It.IsAny(Of SqlCommand)())).
            Returns(Function() dataTable)

        userMock.
            Setup(Function(m) m.HasPermission(It.IsAny(Of ICollection(Of Permission))())).
            Returns(True)

        Dim roleSet = New RoleSet From {
                New Role("Test") With {.Id = 1}
                }

        userMock.
            SetupGet(Function(m) m.Roles).
            Returns(roleSet)

        InitPermissions()

        Dim memberPerms = New MemberPermissions(Nothing) With {.State = PermissionState.Restricted}

        Dim getEffectivePermsFunc = New Func(Of Guid, IMemberPermissions)(Function(id) memberPerms)

        Dim result = CType(ReflectionHelper.InvokePrivateMethod("GetAlertProcessDetails", server, databaseConnectionMock.Object, userMock.Object, getEffectivePermsFunc), DataTable)

        Assert.AreEqual(0, result.Rows.Count)

    End Sub

    ''' <summary>
    ''' Test if the process and the executing resource is unrestricted, the user can get the alert history.
    ''' </summary>
    <Test>
    Public Sub GetAlertHistory_ProcessAndResourceIsUnrestricted_AlertReturnedInDatatable()

        Dim server = New clsServer()
        Dim processid = Guid.NewGuid()
        Dim resourceid = Guid.NewGuid()

        Dim databaseConnectionMock = New Mock(Of IDatabaseConnection)()
        Dim dataReaderMock = New Mock(Of IDataReader)()
        Dim userMock = New Mock(Of IUser)()

        Dim dataTable = New DataTable()
        dataTable.Columns.Add("Date", GetType(DateTime))
        dataTable.Columns.Add("Process", GetType(String))
        dataTable.Columns.Add("Message", GetType(String))
        dataTable.Columns.Add("Resource", GetType(String))
        dataTable.Columns.Add("ResourceID", GetType(Guid))
        dataTable.Columns.Add("ProcessID", GetType(Guid))
        dataTable.Columns.Add("Schedule", GetType(String))
        dataTable.Columns.Add("Task", GetType(String))
        dataTable.Columns.Add("Type", GetType(AlertEventType))
        dataTable.Columns.Add("Method", GetType(AlertNotificationType))



        dataTable.Rows.Add(DateTime.UtcNow, "TestProcess", "Something happened", "Test Resource",
                           resourceid, processid, "A Schedule", " A Task", AlertEventType.ProcessRunning,
                           AlertNotificationType.PopUp)


        databaseConnectionMock.
            Setup(Function(x) x.ExecuteReturnDataTable(It.IsAny(Of SqlCommand)())).
            Returns(Function() dataTable)

        userMock.
            Setup(Function(m) m.HasPermission(It.IsAny(Of ICollection(Of Permission))())).
            Returns(True)

        Dim roleSet = New RoleSet From {
                New Role("Test") With {.Id = 1}
                }

        userMock.
            SetupGet(Function(m) m.Roles).
            Returns(roleSet)
        InitPermissions()

        Dim processPerms = New MemberPermissions(Nothing) With {.State = PermissionState.UnRestricted}
        Dim resourcePerms = New MemberPermissions(Nothing) With {.State = PermissionState.UnRestricted}

        Dim getProcessPermsFunc = New Func(Of Guid, IMemberPermissions)(Function(id) processPerms)
        Dim getResourcePermsFunc = New Func(Of Guid, IMemberPermissions)(Function(id) resourcePerms)

        Dim result = CType(ReflectionHelper.InvokePrivateMethod("GetAlertHistory",
                                                                server,
                                                                databaseConnectionMock.Object,
                                                                userMock.Object,
                                                                DateTime.UtcNow - TimeSpan.FromHours(1),
                                                                getProcessPermsFunc,
                                                                getResourcePermsFunc), DataTable)

        Assert.AreEqual(1, result.Rows.Count)
        Assert.AreEqual(processid, result.Rows(0)("ProcessID"))
        Assert.AreEqual(resourceid, result.Rows(0)("ResourceID"))

    End Sub

    ''' <summary>
    ''' Test if the process and the executing resource is restricted and the user has all the required permissions, the user can get the alert history.
    ''' </summary>
    <Test>
    <TestCaseSource(NameOf(ProcessPermissionGenerator))>
    Public Sub GetAlertHistory_ProcessAndResourceIsRestricted_UserHasAllRelevantPermissions_AlertReturnedInDatatable(processPerm As Permission)

        Dim server = New clsServer()
        Dim processid = Guid.NewGuid()
        Dim resourceid = Guid.NewGuid()

        Dim databaseConnectionMock = New Mock(Of IDatabaseConnection)()
        Dim dataReaderMock = New Mock(Of IDataReader)()
        Dim userMock = New Mock(Of IUser)()

        Dim dataTable = New DataTable()
        dataTable.Columns.Add("Date", GetType(DateTime))
        dataTable.Columns.Add("Process", GetType(String))
        dataTable.Columns.Add("Message", GetType(String))
        dataTable.Columns.Add("Resource", GetType(String))
        dataTable.Columns.Add("ResourceID", GetType(Guid))
        dataTable.Columns.Add("ProcessID", GetType(Guid))
        dataTable.Columns.Add("Schedule", GetType(String))
        dataTable.Columns.Add("Task", GetType(String))
        dataTable.Columns.Add("Type", GetType(AlertEventType))
        dataTable.Columns.Add("Method", GetType(AlertNotificationType))



        dataTable.Rows.Add(DateTime.UtcNow, "TestProcess", "Something happened", "Test Resource",
                           resourceid, processid, "A Schedule", " A Task", AlertEventType.ProcessRunning,
                           AlertNotificationType.PopUp)


        databaseConnectionMock.
            Setup(Function(x) x.ExecuteReturnDataTable(It.IsAny(Of SqlCommand)())).
            Returns(Function() dataTable)

        userMock.
            Setup(Function(m) m.HasPermission(It.IsAny(Of ICollection(Of Permission))())).
            Returns(True)

        Dim roleSet = New RoleSet From {
                New Role("Test") With {.Id = 1}
                }

        userMock.
            SetupGet(Function(m) m.Roles).
            Returns(roleSet)

        InitPermissions()

        Dim processPerms = CreateTestMemberPermissions(PermissionState.Restricted, processPerm)
        Dim resourcePerms = CreateTestMemberPermissions(PermissionState.Restricted, GetViewResourcePermission())

        Dim getProcessPermsFunc = New Func(Of Guid, IMemberPermissions)(Function(id) processPerms)
        Dim getResourcePermsFunc = New Func(Of Guid, IMemberPermissions)(Function(id) resourcePerms)

        Dim result = CType(ReflectionHelper.InvokePrivateMethod("GetAlertHistory",
                                                                server,
                                                                databaseConnectionMock.Object,
                                                                userMock.Object,
                                                                DateTime.UtcNow - TimeSpan.FromHours(1),
                                                                getProcessPermsFunc,
                                                                getResourcePermsFunc), DataTable)

        Assert.AreEqual(1, result.Rows.Count)
        Assert.AreEqual(processid, result.Rows(0)("ProcessID"))
        Assert.AreEqual(resourceid, result.Rows(0)("ResourceID"))

    End Sub

    ''' <summary>
    ''' Test if the process and the executing resource is restricted and the user doesn't have a permission on the process, the user can't get the alert history.
    ''' </summary>
    <Test>
    Public Sub GetAlertHistory_ProcessAndResourceIsRestricted_UserDoesntHavePermissionToViewProcess_AlertNotReturnedInDatatable()

        Dim server = New clsServer()
        Dim processid = Guid.NewGuid()
        Dim resourceid = Guid.NewGuid()

        Dim databaseConnectionMock = New Mock(Of IDatabaseConnection)()
        Dim dataReaderMock = New Mock(Of IDataReader)()
        Dim userMock = New Mock(Of IUser)()

        Dim dataTable = New DataTable()
        dataTable.Columns.Add("Date", GetType(DateTime))
        dataTable.Columns.Add("Process", GetType(String))
        dataTable.Columns.Add("Message", GetType(String))
        dataTable.Columns.Add("Resource", GetType(String))
        dataTable.Columns.Add("ResourceID", GetType(Guid))
        dataTable.Columns.Add("ProcessID", GetType(Guid))
        dataTable.Columns.Add("Schedule", GetType(String))
        dataTable.Columns.Add("Task", GetType(String))
        dataTable.Columns.Add("Type", GetType(AlertEventType))
        dataTable.Columns.Add("Method", GetType(AlertNotificationType))



        dataTable.Rows.Add(DateTime.UtcNow, "TestProcess", "Something happened", "Test Resource",
                           resourceid, processid, "A Schedule", " A Task", AlertEventType.ProcessRunning,
                           AlertNotificationType.PopUp)


        databaseConnectionMock.
            Setup(Function(x) x.ExecuteReturnDataTable(It.IsAny(Of SqlCommand)())).
            Returns(Function() dataTable)

        userMock.
            Setup(Function(m) m.HasPermission(It.IsAny(Of ICollection(Of Permission))())).
            Returns(True)

        Dim roleSet = New RoleSet From {
                New Role("Test") With {.Id = 1}
                }

        userMock.
            SetupGet(Function(m) m.Roles).
            Returns(roleSet)

        InitPermissions()

        Dim processPerms = CreateTestMemberPermissions(PermissionState.Restricted)
        Dim resourcePerms = CreateTestMemberPermissions(PermissionState.Restricted, GetViewResourcePermission())

        Dim getProcessPermsFunc = New Func(Of Guid, IMemberPermissions)(Function(id) processPerms)
        Dim getResourcePermsFunc = New Func(Of Guid, IMemberPermissions)(Function(id) resourcePerms)

        Dim result = CType(ReflectionHelper.InvokePrivateMethod("GetAlertHistory",
                                                                server,
                                                                databaseConnectionMock.Object,
                                                                userMock.Object,
                                                                DateTime.UtcNow - TimeSpan.FromHours(1),
                                                                getProcessPermsFunc,
                                                                getResourcePermsFunc), DataTable)

        Assert.AreEqual(0, result.Rows.Count)

    End Sub


    ''' <summary>
    ''' Test if the process and the executing resource is restricted and the user doesn't have 'view resource' permission on the resource, the user can't get the alert history.
    ''' </summary>
    <Test>
    <TestCaseSource(NameOf(ProcessPermissionGenerator))>
    Public Sub GetAlertHistory_ProcessAndResourceIsRestricted_UserDoesntHavePermissionToViewResource_AlertNotReturnedInDatatable(processPerm As Permission)

        Dim server = New clsServer()
        Dim processid = Guid.NewGuid()
        Dim resourceid = Guid.NewGuid()

        Dim databaseConnectionMock = New Mock(Of IDatabaseConnection)()
        Dim dataReaderMock = New Mock(Of IDataReader)()
        Dim userMock = New Mock(Of IUser)()

        Dim dataTable = New DataTable()
        dataTable.Columns.Add("Date", GetType(DateTime))
        dataTable.Columns.Add("Process", GetType(String))
        dataTable.Columns.Add("Message", GetType(String))
        dataTable.Columns.Add("Resource", GetType(String))
        dataTable.Columns.Add("ResourceID", GetType(Guid))
        dataTable.Columns.Add("ProcessID", GetType(Guid))
        dataTable.Columns.Add("Schedule", GetType(String))
        dataTable.Columns.Add("Task", GetType(String))
        dataTable.Columns.Add("Type", GetType(AlertEventType))
        dataTable.Columns.Add("Method", GetType(AlertNotificationType))



        dataTable.Rows.Add(DateTime.UtcNow, "TestProcess", "Something happened", "Test Resource",
                           resourceid, processid, "A Schedule", " A Task", AlertEventType.ProcessRunning,
                           AlertNotificationType.PopUp)


        databaseConnectionMock.
            Setup(Function(x) x.ExecuteReturnDataTable(It.IsAny(Of SqlCommand)())).
            Returns(Function() dataTable)

        userMock.
            Setup(Function(m) m.HasPermission(It.IsAny(Of ICollection(Of Permission))())).
            Returns(True)

        Dim roleSet = New RoleSet From {
                New Role("Test") With {.Id = 1}
                }

        userMock.
            SetupGet(Function(m) m.Roles).
            Returns(roleSet)

        InitPermissions()
        Dim processPerms = CreateTestMemberPermissions(PermissionState.Restricted, processPerm)

        Dim resourcePerms = CreateTestMemberPermissions(PermissionState.Restricted)

        Dim getProcessPermsFunc = New Func(Of Guid, IMemberPermissions)(Function(id) processPerms)
        Dim getResourcePermsFunc = New Func(Of Guid, IMemberPermissions)(Function(id) resourcePerms)

        Dim result = CType(ReflectionHelper.InvokePrivateMethod("GetAlertHistory",
                                                                server,
                                                                databaseConnectionMock.Object,
                                                                userMock.Object,
                                                                DateTime.UtcNow - TimeSpan.FromHours(1),
                                                                getProcessPermsFunc,
                                                                getResourcePermsFunc), DataTable)

        Assert.AreEqual(0, result.Rows.Count)

    End Sub

    ''' <summary>
    ''' Test if the process and the executing resource is unrestricted the user can get the alert.
    ''' </summary>
    <Test>
    Public Sub UpdateAndAcknowledgeAlerts_ProcessAndResourceIsUnRestricted__AlertReturnedInDatatable()

        Dim server = New clsServer()
        Dim processid = Guid.NewGuid()
        Dim resourceid = Guid.NewGuid()

        Dim databaseConnectionMock = New Mock(Of IDatabaseConnection)()
        Dim dataReaderMock = New Mock(Of IDataReader)()
        Dim userMock = New Mock(Of IUser)()

        Dim dataTable = New DataTable()
        dataTable.Columns.Add("AlertEventID", GetType(Integer))
        dataTable.Columns.Add("AlertNotificationType", GetType(AlertNotificationType))
        dataTable.Columns.Add("Message", GetType(String))
        dataTable.Columns.Add("Date", GetType(DateTime))
        dataTable.Columns.Add("SessionID", GetType(Guid))
        dataTable.Columns.Add("ResourceID", GetType(Guid))
        dataTable.Columns.Add("ResourceName", GetType(String))
        dataTable.Columns.Add("ProcessName", GetType(String))
        dataTable.Columns.Add("ScheduleName", GetType(String))
        dataTable.Columns.Add("TaskName", GetType(String))
        dataTable.Columns.Add("ProcessID", GetType(Guid))

        dataTable.Rows.Add(1, AlertNotificationType.PopUp, "Something happened", DateTime.UtcNow, New Guid(),
                           resourceid, "Test Resource", "Test Process", "A Schedule", "A Task", processid)


        databaseConnectionMock.
            Setup(Function(x) x.ExecuteReturnDataTable(It.IsAny(Of SqlCommand)())).
            Returns(Function() dataTable)

        userMock.
            Setup(Function(m) m.HasPermission(It.IsAny(Of ICollection(Of Permission))())).
            Returns(True)

        Dim roleSet = New RoleSet From {
                New Role("Test") With {.Id = 1}
                }

        userMock.
            SetupGet(Function(m) m.Roles).
            Returns(roleSet)

        InitPermissions()
        Dim processPerms = New MemberPermissions(Nothing) With {.State = PermissionState.UnRestricted}

        Dim resourcePerms = New MemberPermissions(Nothing) With {.State = PermissionState.UnRestricted}

        Dim getProcessPermsFunc = New Func(Of Guid, IMemberPermissions)(Function(id) processPerms)
        Dim getResourcePermsFunc = New Func(Of Guid, IMemberPermissions)(Function(id) resourcePerms)

        Dim result = CType(ReflectionHelper.InvokePrivateMethod("UpdateAndAcknowledgeAlerts",
                                                                server,
                                                                databaseConnectionMock.Object,
                                                                New List(Of Guid)(),
                                                                resourceid,
                                                                userMock.Object,
                                                                getProcessPermsFunc,
                                                                getResourcePermsFunc), DataTable)

        Assert.AreEqual(1, result.Rows.Count)
        Assert.AreEqual(processid, result.Rows(0)("ProcessID"))
        Assert.AreEqual(resourceid, result.Rows(0)("ResourceID"))

    End Sub

    ''' <summary>
    ''' Test if the process and the executing resource is restricted and the user has the required permissions, they can get the alert.
    ''' </summary>
    <Test>
    <TestCaseSource(NameOf(ProcessPermissionGenerator))>
    Public Sub UpdateAndAcknowledgeAlerts_ProcessAndResourceIsRestricted_UserHasAllRelevantPermissions_AlertReturnedInDatatable(processPerm As Permission)

        Dim server = New clsServer()
        Dim processid = Guid.NewGuid()
        Dim resourceid = Guid.NewGuid()

        Dim databaseConnectionMock = New Mock(Of IDatabaseConnection)()
        Dim dataReaderMock = New Mock(Of IDataReader)()
        Dim userMock = New Mock(Of IUser)()

        Dim dataTable = New DataTable()
        dataTable.Columns.Add("AlertEventID", GetType(Integer))
        dataTable.Columns.Add("AlertNotificationType", GetType(AlertNotificationType))
        dataTable.Columns.Add("Message", GetType(String))
        dataTable.Columns.Add("Date", GetType(DateTime))
        dataTable.Columns.Add("SessionID", GetType(Guid))
        dataTable.Columns.Add("ResourceID", GetType(Guid))
        dataTable.Columns.Add("ResourceName", GetType(String))
        dataTable.Columns.Add("ProcessName", GetType(String))
        dataTable.Columns.Add("ScheduleName", GetType(String))
        dataTable.Columns.Add("TaskName", GetType(String))
        dataTable.Columns.Add("ProcessID", GetType(Guid))

        dataTable.Rows.Add(1, AlertNotificationType.PopUp, "Something happened", DateTime.UtcNow, New Guid(),
                           resourceid, "Test Resource", "Test Process", "A Schedule", "A Task", processid)


        databaseConnectionMock.
            Setup(Function(x) x.ExecuteReturnDataTable(It.IsAny(Of SqlCommand)())).
            Returns(Function() dataTable)

        userMock.
            Setup(Function(m) m.HasPermission(It.IsAny(Of ICollection(Of Permission))())).
            Returns(True)

        Dim roleSet = New RoleSet From {
                New Role("Test") With {.Id = 1}
                }

        userMock.
            SetupGet(Function(m) m.Roles).
            Returns(roleSet)

        InitPermissions()
        Dim processPerms = CreateTestMemberPermissions(PermissionState.Restricted, processPerm)

        Dim resourcePerms = CreateTestMemberPermissions(PermissionState.Restricted, GetViewResourcePermission())

        Dim getProcessPermsFunc = New Func(Of Guid, IMemberPermissions)(Function(id) processPerms)
        Dim getResourcePermsFunc = New Func(Of Guid, IMemberPermissions)(Function(id) resourcePerms)

        Dim result = CType(ReflectionHelper.InvokePrivateMethod("UpdateAndAcknowledgeAlerts",
                                                                server,
                                                                databaseConnectionMock.Object,
                                                                New List(Of Guid)(),
                                                                resourceid,
                                                                userMock.Object,
                                                                getProcessPermsFunc,
                                                                getResourcePermsFunc), DataTable)

        Assert.AreEqual(1, result.Rows.Count)
        Assert.AreEqual(processid, result.Rows(0)("ProcessID"))
        Assert.AreEqual(resourceid, result.Rows(0)("ResourceID"))

    End Sub

    ''' <summary>
    ''' Test if the process and the executing resource is restricted and the user doesn't have 'view resource' permission, they can't get the alert.
    ''' </summary>
    <Test>
    <TestCaseSource(NameOf(ProcessPermissionGenerator))>
    Public Sub UpdateAndAcknowledgeAlerts_ProcessAndResourceIsRestricted_UserDoesntHaveViewResourcePermission_AlertNotReturnedInDatatable(processPerm As Permission)

        Dim server = New clsServer()
        Dim processid = Guid.NewGuid()
        Dim resourceid = Guid.NewGuid()

        Dim databaseConnectionMock = New Mock(Of IDatabaseConnection)()
        Dim dataReaderMock = New Mock(Of IDataReader)()
        Dim userMock = New Mock(Of IUser)()

        Dim dataTable = New DataTable()
        dataTable.Columns.Add("AlertEventID", GetType(Integer))
        dataTable.Columns.Add("AlertNotificationType", GetType(AlertNotificationType))
        dataTable.Columns.Add("Message", GetType(String))
        dataTable.Columns.Add("Date", GetType(DateTime))
        dataTable.Columns.Add("SessionID", GetType(Guid))
        dataTable.Columns.Add("ResourceID", GetType(Guid))
        dataTable.Columns.Add("ResourceName", GetType(String))
        dataTable.Columns.Add("ProcessName", GetType(String))
        dataTable.Columns.Add("ScheduleName", GetType(String))
        dataTable.Columns.Add("TaskName", GetType(String))
        dataTable.Columns.Add("ProcessID", GetType(Guid))

        dataTable.Rows.Add(1, AlertNotificationType.PopUp, "Something happened", DateTime.UtcNow, New Guid(),
                           resourceid, "Test Resource", "Test Process", "A Schedule", "A Task", processid)


        databaseConnectionMock.
            Setup(Function(x) x.ExecuteReturnDataTable(It.IsAny(Of SqlCommand)())).
            Returns(Function() dataTable)

        userMock.
            Setup(Function(m) m.HasPermission(It.IsAny(Of ICollection(Of Permission))())).
            Returns(True)

        Dim roleSet = New RoleSet From {
                New Role("Test") With {.Id = 1}
                }

        userMock.
            SetupGet(Function(m) m.Roles).
            Returns(roleSet)

        InitPermissions()
        Dim processPerms = CreateTestMemberPermissions(PermissionState.Restricted, processPerm)

        Dim resourcePerms = CreateTestMemberPermissions(PermissionState.Restricted)

        Dim getProcessPermsFunc = New Func(Of Guid, IMemberPermissions)(Function(id) processPerms)
        Dim getResourcePermsFunc = New Func(Of Guid, IMemberPermissions)(Function(id) resourcePerms)

        Dim result = CType(ReflectionHelper.InvokePrivateMethod("UpdateAndAcknowledgeAlerts",
                                                                server,
                                                                databaseConnectionMock.Object,
                                                                New List(Of Guid)(),
                                                                resourceid,
                                                                userMock.Object,
                                                                getProcessPermsFunc,
                                                                getResourcePermsFunc), DataTable)

        Assert.AreEqual(0, result.Rows.Count)

    End Sub

    ''' <summary>
    ''' Test if the process and the executing resource is restricted and the user doesn't have any permissions on the process, they can't get the alert.
    ''' </summary>
    <Test>
    Public Sub UpdateAndAcknowledgeAlerts_ProcessAndResourceIsRestricted_UserDoesntHaveAnyPermissionOnProcess_AlertNotReturnedInDatatable()

        Dim server = New clsServer()
        Dim processid = Guid.NewGuid()
        Dim resourceid = Guid.NewGuid()

        Dim databaseConnectionMock = New Mock(Of IDatabaseConnection)()
        Dim dataReaderMock = New Mock(Of IDataReader)()
        Dim userMock = New Mock(Of IUser)()

        Dim dataTable = New DataTable()
        dataTable.Columns.Add("AlertEventID", GetType(Integer))
        dataTable.Columns.Add("AlertNotificationType", GetType(AlertNotificationType))
        dataTable.Columns.Add("Message", GetType(String))
        dataTable.Columns.Add("Date", GetType(DateTime))
        dataTable.Columns.Add("SessionID", GetType(Guid))
        dataTable.Columns.Add("ResourceID", GetType(Guid))
        dataTable.Columns.Add("ResourceName", GetType(String))
        dataTable.Columns.Add("ProcessName", GetType(String))
        dataTable.Columns.Add("ScheduleName", GetType(String))
        dataTable.Columns.Add("TaskName", GetType(String))
        dataTable.Columns.Add("ProcessID", GetType(Guid))

        dataTable.Rows.Add(1, AlertNotificationType.PopUp, "Something happened", DateTime.UtcNow, New Guid(),
                           resourceid, "Test Resource", "Test Process", "A Schedule", "A Task", processid)


        databaseConnectionMock.
            Setup(Function(x) x.ExecuteReturnDataTable(It.IsAny(Of SqlCommand)())).
            Returns(Function() dataTable)

        userMock.
            Setup(Function(m) m.HasPermission(It.IsAny(Of ICollection(Of Permission))())).
            Returns(True)

        Dim roleSet = New RoleSet From {
                New Role("Test") With {.Id = 1}
                }

        userMock.
            SetupGet(Function(m) m.Roles).
            Returns(roleSet)

        InitPermissions()
        Dim processPerms = CreateTestMemberPermissions(PermissionState.Restricted)

        Dim resourcePerms = CreateTestMemberPermissions(PermissionState.Restricted, GetViewResourcePermission())


        Dim getProcessPermsFunc = New Func(Of Guid, IMemberPermissions)(Function(id) processPerms)
        Dim getResourcePermsFunc = New Func(Of Guid, IMemberPermissions)(Function(id) resourcePerms)

        Dim result = CType(ReflectionHelper.InvokePrivateMethod("UpdateAndAcknowledgeAlerts",
                                                                server,
                                                                databaseConnectionMock.Object,
                                                                New List(Of Guid)(),
                                                                resourceid,
                                                                userMock.Object,
                                                                getProcessPermsFunc,
                                                                getResourcePermsFunc), DataTable)

        Assert.AreEqual(0, result.Rows.Count)

    End Sub

    ''' <summary>
    ''' Initialises the Permission class with the permissions we are using for testing.
    ''' </summary>
    Private Sub InitPermissions()
        Dim editProcessPerm = GetEditProcessPermission()
        Dim createProcessPerm = GetCreateProcessPermission()
        Dim deleteProcessPerm = GetDeleteProcessPermission()
        Dim editProcessGroupPerm = GetEditProcessGroupsPermission()
        Dim executeProcessPerm = GetExecuteProcessPermission()
        Dim viewProcessPerm = GetViewProcessPermission()
        Dim exportProcessPerm = GetExportProcessPermission()
        Dim importProcessPerm = GetImportProcessPermission()
        Dim controlResourcePerm = GetControlResourcePermission()
        Dim viewResourcePerm = GetViewResourcePermission()
        Dim configureResourcePerm = GetConfigureResourcePermission()
        Dim authAsResourcePerm = GetAuthAsResourcePermission()
        Dim viewResourceScreenCapPerm = GetViewResourceScreenCapturePermission()
        Dim manageResourceAccessRights = GetManageResourceAccessRightsPermission()
        Dim accessResourceDetailsPerm = GetResourceDetailsPermission()

        ReflectionHelper.SetPrivateField(Of Permission)("mByName", Nothing, New Dictionary(Of String, Permission) From {
                                                        {editProcessPerm.Name, editProcessPerm},
                                                        {createProcessPerm.Name, createProcessPerm},
                                                        {deleteProcessPerm.Name, deleteProcessPerm},
                                                        {editProcessGroupPerm.Name, editProcessGroupPerm},
                                                        {executeProcessPerm.Name, executeProcessPerm},
                                                        {viewProcessPerm.Name, viewProcessPerm},
                                                        {exportProcessPerm.Name, exportProcessPerm},
                                                        {importProcessPerm.Name, importProcessPerm},
                                                        {controlResourcePerm.Name, controlResourcePerm},
                                                        {viewResourcePerm.Name, viewResourcePerm},
                                                        {configureResourcePerm.Name, configureResourcePerm},
                                                        {authAsResourcePerm.Name, authAsResourcePerm},
                                                        {viewResourceScreenCapPerm.Name, viewResourceScreenCapPerm},
                                                        {manageResourceAccessRights.Name, manageResourceAccessRights},
                                                           {accessResourceDetailsPerm.Name, accessResourceDetailsPerm}
                                                        })

        ReflectionHelper.SetPrivateField(Of Permission)("mById", Nothing, New Dictionary(Of Integer, Permission) From {
                                                        {editProcessPerm.Id, editProcessPerm},
                                                        {createProcessPerm.Id, createProcessPerm},
                                                        {deleteProcessPerm.Id, deleteProcessPerm},
                                                        {editProcessGroupPerm.Id, editProcessGroupPerm},
                                                        {executeProcessPerm.Id, executeProcessPerm},
                                                        {viewProcessPerm.Id, viewProcessPerm},
                                                        {exportProcessPerm.Id, exportProcessPerm},
                                                        {importProcessPerm.Id, importProcessPerm},
                                                        {controlResourcePerm.Id, controlResourcePerm},
                                                        {viewResourcePerm.Id, viewResourcePerm},
                                                        {configureResourcePerm.Id, configureResourcePerm},
                                                        {authAsResourcePerm.Id, authAsResourcePerm},
                                                        {viewResourceScreenCapPerm.Id, viewResourceScreenCapPerm},
                                                        {manageResourceAccessRights.Id, manageResourceAccessRights},
                                                           {accessResourceDetailsPerm.Id, accessResourceDetailsPerm}
        })

    End Sub


    Private Shared Function CreateTestMemberPermissions(permState As PermissionState, ParamArray Perms() As Permission) As MemberPermissions
        Dim groupPermissions = New GroupPermissions(PermissionState.Restricted)
        Dim grpLevelPerms = New GroupLevelPermissions(1)

        For Each perm As Permission In Perms
            grpLevelPerms.Add(perm)
        Next
        groupPermissions.Add(grpLevelPerms)
        Return New MemberPermissions(groupPermissions)
    End Function

    ''' <summary>
    ''' Generates test cases using each Process Permission.
    ''' This is to test the situations where the user requires any process permission to perform an action.
    ''' It ensures that we get the expected result with any permission.
    ''' </summary>
    ''' <returns></returns>
    Protected Shared Function ProcessPermissionGenerator() As IEnumerable(Of Permission)

        Return {
            GetEditProcessPermission(),
            GetCreateProcessPermission(),
            GetDeleteProcessPermission(),
            GetEditProcessGroupsPermission(),
            GetExecuteProcessPermission(),
            GetViewProcessPermission(),
            GetExportProcessPermission(),
            GetImportProcessPermission()
        }

    End Function

    Private Shared Function GetEditProcessPermission() As Permission
        Return New TestPermission(27, Permission.ProcessStudio.EditProcess)
    End Function

    Private Shared Function GetCreateProcessPermission() As Permission
        Return New TestPermission(20, Permission.ProcessStudio.CreateProcess)
    End Function

    Private Shared Function GetDeleteProcessPermission() As Permission
        Return New TestPermission(24, Permission.ProcessStudio.DeleteProcess)
    End Function

    Private Shared Function GetEditProcessGroupsPermission() As Permission
        Return New TestPermission(73, Permission.ProcessStudio.EditProcessGroups)
    End Function

    Private Shared Function GetExecuteProcessPermission() As Permission
        Return New TestPermission(57, Permission.ProcessStudio.ExecuteProcess)
    End Function

    Private Shared Function GetViewProcessPermission() As Permission
        Return New TestPermission(59, Permission.ProcessStudio.ViewProcess)
    End Function

    Private Shared Function GetExportProcessPermission() As Permission
        Return New TestPermission(30, Permission.ProcessStudio.ExportProcess)
    End Function

    Private Shared Function GetImportProcessPermission() As Permission
        Return New TestPermission(34, Permission.ProcessStudio.ImportProcess)
    End Function


    Private Shared Function GetControlResourcePermission() As Permission
        Return New TestPermission(32, Permission.Resources.ControlResource)
    End Function

    Private Shared Function GetViewResourcePermission() As Permission
        Return New TestPermission(42, Permission.Resources.ViewResource)
    End Function

    Private Shared Function GetConfigureResourcePermission() As Permission
        Return New TestPermission(43, Permission.Resources.ConfigureResource)
    End Function

    Private Shared Function GetAuthAsResourcePermission() As Permission
        Return New TestPermission(85, Permission.Resources.AuthenticateAsResource)
    End Function

    Private Shared Function GetViewResourceScreenCapturePermission() As Permission
        Return New TestPermission(80, Permission.Resources.ViewResourceScreenCaptures)
    End Function

    Private Shared Function GetResourceDetailsPermission() As Permission
        Return New TestPermission(82, Permission.Resources.ViewResourceDetails)
    End Function

    Private Shared Function GetManageResourceAccessRightsPermission() As Permission
        Return New TestPermission(105, Permission.Resources.ManageResourceAccessrights)
    End Function

    Private Class TestPermission : Inherits Permission
        Public Sub New(id As Integer, name As String)
            MyBase.New(id, name, Feature.None)
        End Sub
    End Class


End Class

#End If
