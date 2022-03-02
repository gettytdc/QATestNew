#If UNITTESTS Then
Imports System.Data.SqlClient
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.clsServerPartialClasses
Imports BluePrism.Data
Imports BluePrism.Server.Domain.Models
Imports BluePrism.UnitTesting
Imports BluePrism.UnitTesting.TestSupport
Imports Moq
Imports NUnit.Framework

Namespace Resources

    <TestFixture>
    Public Class ResourceScreenCaptureTests

        <SetUp>
        Public Sub Setup()
            LegacyUnitTestHelper.SetupDependencyResolver()
        End Sub

        ''' <summary>
        ''' Tests that the CheckExceptionscreenshotAvailable method returns true if the resource is unrestricted and a screenshot is available.
        ''' </summary>
        <Test>
        Public Sub CheckExceptionScreenshotAvailable_ResourceIsUnrestricted_ReturnsTrue()

            Dim server = New clsServer()
            Dim databaseConnectionMock = New Mock(Of IDatabaseConnection)()

            Dim userMock = New Mock(Of IUser)()

            databaseConnectionMock.
                Setup(Function(x) x.ExecuteReturnScalar(Moq.It.IsAny(Of SqlCommand)())).
                Returns(Function() 1)


            Dim roleSet = New RoleSet From {
                    New Role("Test") With {.Id = 1}
                    }

            userMock.
                SetupGet(Function(m) m.Roles).
                Returns(roleSet)

            userMock.
                Setup(Function(m) m.HasPermission(Moq.It.IsAny(Of ICollection(Of Permission))())).
                Returns(True)

            ReflectionHelper.SetPrivateField("mLoggedInUser", server, userMock.Object)

            Dim viewResourceScreenCapturePerm = New TestPermission(1, Permission.Resources.ViewResourceScreenCaptures)

            ReflectionHelper.SetPrivateField(Of Permission)("mByName", Nothing, New Dictionary(Of String, Permission) From {{viewResourceScreenCapturePerm.Name, viewResourceScreenCapturePerm}})
            ReflectionHelper.SetPrivateField(Of Permission)("mById", Nothing, New Dictionary(Of Integer, Permission) From {{viewResourceScreenCapturePerm.Id, viewResourceScreenCapturePerm}})


            Dim resourcePerms = New MemberPermissions(Nothing)
            resourcePerms.State = PermissionState.UnRestricted

            Dim result = CType(ReflectionHelper.InvokePrivateMethod("CheckExceptionScreenshotAvailable", server, databaseConnectionMock.Object, New Guid(), resourcePerms), Boolean)

            Assert.IsTrue(result)
        End Sub

        ''' <summary>
        ''' Tests that the CheckExceptionscreenshotAvailable method returns true if the resource is restricted, 
        ''' the user has the View Resource screen capture permission, and a screenshot is available.
        ''' </summary>
        <Test>
        Public Sub CheckExceptionScreenshotAvailable_ResourceIsRestricted_UserHasViewResourceScreenCapPerm_ReturnsTrue()

            Dim server = New clsServer()
            Dim databaseConnectionMock = New Mock(Of IDatabaseConnection)()

            Dim userMock = New Mock(Of IUser)()

            databaseConnectionMock.
                Setup(Function(x) x.ExecuteReturnScalar(Moq.It.IsAny(Of SqlCommand)())).
                Returns(Function() 1)


            Dim roleSet = New RoleSet From {
                    New Role("Test") With {.Id = 1}
                    }

            userMock.
                SetupGet(Function(m) m.Roles).
                Returns(roleSet)

            userMock.
                Setup(Function(m) m.HasPermission(Moq.It.IsAny(Of ICollection(Of Permission))())).
                Returns(True)

            ReflectionHelper.SetPrivateField("mLoggedInUser", server, userMock.Object)

            Dim viewResourceScreenCapturePerm = New TestPermission(1, Permission.Resources.ViewResourceScreenCaptures)

            ReflectionHelper.SetPrivateField(Of Permission)("mByName", Nothing, New Dictionary(Of String, Permission) From {{viewResourceScreenCapturePerm.Name, viewResourceScreenCapturePerm}})
            ReflectionHelper.SetPrivateField(Of Permission)("mById", Nothing, New Dictionary(Of Integer, Permission) From {{viewResourceScreenCapturePerm.Id, viewResourceScreenCapturePerm}})

            Dim groupPermissions = New GroupPermissions(PermissionState.Restricted)
            Dim grpLevelPerms = New GroupLevelPermissions(roleSet.Roles(0).Id)
            grpLevelPerms.Add(viewResourceScreenCapturePerm)
            groupPermissions.Add(grpLevelPerms)

            Dim resourcePerms = New MemberPermissions(groupPermissions)


            Dim result = CType(ReflectionHelper.InvokePrivateMethod("CheckExceptionScreenshotAvailable", server, databaseConnectionMock.Object, New Guid, resourcePerms), Boolean)

            Assert.IsTrue(result)
        End Sub

        ''' <summary>
        ''' Tests that the CheckExceptionscreenshotAvailable method returns false if the resource is restricted, 
        ''' the user doesnt have the View Resource Screen Capture permission and a screenshot is available.
        ''' </summary>
        <Test>
        Public Sub CheckExceptionScreenshotAvailable_ResourceIsRestricted_UserDoesntHaveViewResourceScreenCapPerm_ReturnsFalse()

            Dim server = New clsServer()
            Dim databaseConnectionMock = New Mock(Of IDatabaseConnection)()

            Dim userMock = New Mock(Of IUser)()

            databaseConnectionMock.
                Setup(Function(x) x.ExecuteReturnScalar(Moq.It.IsAny(Of SqlCommand)())).
                Returns(Function() 1)


            Dim roleSet = New RoleSet From {
                    New Role("Test") With {.Id = 1}
                    }

            userMock.
                SetupGet(Function(m) m.Roles).
                Returns(roleSet)

            userMock.
                Setup(Function(m) m.HasPermission(Moq.It.IsAny(Of ICollection(Of Permission))())).
                Returns(True)

            ReflectionHelper.SetPrivateField("mLoggedInUser", server, userMock.Object)

            Dim viewResourceScreenCapturePerm = New TestPermission(1, Permission.Resources.ViewResourceScreenCaptures)

            ReflectionHelper.SetPrivateField(Of Permission)("mByName", Nothing, New Dictionary(Of String, Permission) From {{viewResourceScreenCapturePerm.Name, viewResourceScreenCapturePerm}})
            ReflectionHelper.SetPrivateField(Of Permission)("mById", Nothing, New Dictionary(Of Integer, Permission) From {{viewResourceScreenCapturePerm.Id, viewResourceScreenCapturePerm}})


            Dim groupPermissions = New GroupPermissions(PermissionState.Restricted)
            Dim grpLevelPerms = New GroupLevelPermissions(roleSet.Roles(0).Id)
            groupPermissions.Add(grpLevelPerms)

            Dim resourcePerms = New MemberPermissions(groupPermissions)

            Dim result = CType(ReflectionHelper.InvokePrivateMethod("CheckExceptionScreenshotAvailable", server, databaseConnectionMock.Object, New Guid, resourcePerms), Boolean)

            Assert.IsFalse(result)
        End Sub


        ''' <summary>
        ''' Tests that the GetExceptionScreenshot method returns a screen capture if the resource is unrestricted, 
        ''' </summary>
        <Test>
        Public Sub GetExceptionScreenshot_ResourceIsUnrestricted_ReturnsScreenCapture()

            Dim server = New clsServer()
            Dim databaseConnectionMock = New Mock(Of IDatabaseConnection)()
            Dim dataReaderMock = New Mock(Of IDataReader)()
            Dim resourceId = New Guid()
            Dim userMock = New Mock(Of IUser)()

            databaseConnectionMock.
                Setup(Function(x) x.ExecuteReturnDataReader(Moq.It.IsAny(Of SqlCommand)())).
                Returns(Function() dataReaderMock.Object)


            dataReaderMock.SetupSequence(Function(x) x.Read()).
                Returns(True).
                Returns(False)

            dataReaderMock.
                SetupGet(Function(m) m.FieldCount).
                Returns(1)

            dataReaderMock.
                SetupSequence(Function(m) m.GetName(Moq.It.IsAny(Of Integer)())).
                Returns("screenshot").
                Returns("encryptid").
                Returns("stageid").
                Returns("processname").
                Returns("lastupdated").
                Returns("timezoneoffset")


            dataReaderMock.
                SetupSequence(Function(m) m(Moq.It.IsAny(Of Integer)())).
                Returns("c:/screenshot/path").
                Returns(1).
                Returns(New Guid).
                Returns("Test Process").
                Returns(DateTime.UtcNow).
                Returns(0)

            Dim roleSet = New RoleSet From {
                    New Role("Test") With {.Id = 1}
                    }

            userMock.
                SetupGet(Function(m) m.Roles).
                Returns(roleSet)

            userMock.
                Setup(Function(m) m.HasPermission(Moq.It.IsAny(Of ICollection(Of Permission))())).
                Returns(True)

            ReflectionHelper.SetPrivateField("mLoggedInUser", server, userMock.Object)

            Dim viewResourceScreenCapturePerm = New TestPermission(1, Permission.Resources.ViewResourceScreenCaptures)

            ReflectionHelper.SetPrivateField(Of Permission)("mByName", Nothing, New Dictionary(Of String, Permission) From {{viewResourceScreenCapturePerm.Name, viewResourceScreenCapturePerm}})
            ReflectionHelper.SetPrivateField(Of Permission)("mById", Nothing, New Dictionary(Of Integer, Permission) From {{viewResourceScreenCapturePerm.Id, viewResourceScreenCapturePerm}})


            Dim resourcePerms = New MemberPermissions(Nothing)
            resourcePerms.State = PermissionState.UnRestricted

            Dim result = CType(ReflectionHelper.InvokePrivateMethod("GetExceptionScreenshot", server, databaseConnectionMock.Object, resourceId, resourcePerms), clsScreenshotDetails)

            Assert.AreEqual(resourceId, result.ResourceId)
        End Sub

        ''' <summary>
        ''' Tests that the GetExceptionScreenshot method returns a screen capture if the resource is Restricted and the user has
        ''' view resource screen capture permission, 
        ''' </summary>
        <Test>
        Public Sub GetExceptionScreenshot_ResourceIsRestricted_UserHasViewResourceScreenCapPermission_ReturnsScreenCapture()

            Dim server = New clsServer()
            Dim databaseConnectionMock = New Mock(Of IDatabaseConnection)()
            Dim dataReaderMock = New Mock(Of IDataReader)()
            Dim resourceId = New Guid()
            Dim userMock = New Mock(Of IUser)()

            databaseConnectionMock.
                Setup(Function(x) x.ExecuteReturnDataReader(Moq.It.IsAny(Of SqlCommand)())).
                Returns(Function() dataReaderMock.Object)


            dataReaderMock.SetupSequence(Function(x) x.Read()).
                Returns(True).
                Returns(False)

            dataReaderMock.
                SetupGet(Function(m) m.FieldCount).
                Returns(1)

            dataReaderMock.
                SetupSequence(Function(m) m.GetName(Moq.It.IsAny(Of Integer)())).
                Returns("screenshot").
                Returns("encryptid").
                Returns("stageid").
                Returns("processname").
                Returns("lastupdated").
                Returns("timezoneoffset")


            dataReaderMock.
                SetupSequence(Function(m) m(Moq.It.IsAny(Of Integer)())).
                Returns("c:/screenshot/path").
                Returns(1).
                Returns(New Guid).
                Returns("Test Process").
                Returns(DateTime.UtcNow).
                Returns(0)

            Dim roleSet = New RoleSet From {
                    New Role("Test") With {.Id = 1}
                    }

            userMock.
                SetupGet(Function(m) m.Roles).
                Returns(roleSet)

            userMock.
                Setup(Function(m) m.HasPermission(Moq.It.IsAny(Of ICollection(Of Permission))())).
                Returns(True)

            ReflectionHelper.SetPrivateField("mLoggedInUser", server, userMock.Object)

            Dim viewResourceScreenCapturePerm = New TestPermission(1, Permission.Resources.ViewResourceScreenCaptures)

            ReflectionHelper.SetPrivateField(Of Permission)("mByName", Nothing, New Dictionary(Of String, Permission) From {{viewResourceScreenCapturePerm.Name, viewResourceScreenCapturePerm}})
            ReflectionHelper.SetPrivateField(Of Permission)("mById", Nothing, New Dictionary(Of Integer, Permission) From {{viewResourceScreenCapturePerm.Id, viewResourceScreenCapturePerm}})


            Dim groupPermissions = New GroupPermissions(PermissionState.Restricted)
            Dim grpLevelPerms = New GroupLevelPermissions(roleSet.Roles(0).Id)
            grpLevelPerms.Add(viewResourceScreenCapturePerm)
            groupPermissions.Add(grpLevelPerms)

            Dim resourcePerms = New MemberPermissions(groupPermissions)

            Dim result = CType(ReflectionHelper.InvokePrivateMethod("GetExceptionScreenshot", server, databaseConnectionMock.Object, resourceId, resourcePerms), clsScreenshotDetails)

            Assert.AreEqual(resourceId, result.ResourceId)
        End Sub

        ''' <summary>
        ''' Tests that the GetExceptionScreenshot method throws an exception if the resource is Restricted and the user doesn't have the 
        ''' view resource screen capture permission, 
        ''' </summary>
        <Test>
        Public Sub GetExceptionScreenshot_ResourceIsRestricted_UserDoesntHaveViewResourceScreenCapPermission_ThrowsException()

            Dim server = New clsServer()
            Dim databaseConnectionMock = New Mock(Of IDatabaseConnection)()
            Dim dataReaderMock = New Mock(Of IDataReader)()
            Dim resourceId = New Guid()
            Dim userMock = New Mock(Of IUser)()

            databaseConnectionMock.
                Setup(Function(x) x.ExecuteReturnDataReader(Moq.It.IsAny(Of SqlCommand)())).
                Returns(Function() dataReaderMock.Object)


            dataReaderMock.SetupSequence(Function(x) x.Read()).
                Returns(True).
                Returns(False)

            dataReaderMock.
                SetupGet(Function(m) m.FieldCount).
                Returns(1)

            dataReaderMock.
                SetupSequence(Function(m) m.GetName(Moq.It.IsAny(Of Integer)())).
                Returns("screenshot").
                Returns("encryptid").
                Returns("stageid").
                Returns("processname").
                Returns("lastupdated").
                Returns("timezoneoffset")


            dataReaderMock.
                SetupSequence(Function(m) m(Moq.It.IsAny(Of Integer)())).
                Returns("c:/screenshot/path").
                Returns(1).
                Returns(New Guid).
                Returns("Test Process").
                Returns(DateTime.UtcNow).
                Returns(0)

            Dim roleSet = New RoleSet From {
                    New Role("Test") With {.Id = 1}
                    }

            userMock.
                SetupGet(Function(m) m.Roles).
                Returns(roleSet)

            userMock.
                Setup(Function(m) m.HasPermission(Moq.It.IsAny(Of ICollection(Of Permission))())).
                Returns(True)

            ReflectionHelper.SetPrivateField("mLoggedInUser", server, userMock.Object)

            Dim viewResourceScreenCapturePerm = New TestPermission(1, Permission.Resources.ViewResourceScreenCaptures)

            ReflectionHelper.SetPrivateField(Of Permission)("mByName", Nothing, New Dictionary(Of String, Permission) From {{viewResourceScreenCapturePerm.Name, viewResourceScreenCapturePerm}})
            ReflectionHelper.SetPrivateField(Of Permission)("mById", Nothing, New Dictionary(Of Integer, Permission) From {{viewResourceScreenCapturePerm.Id, viewResourceScreenCapturePerm}})


            Dim groupPermissions = New GroupPermissions(PermissionState.Restricted)
            Dim grpLevelPerms = New GroupLevelPermissions(roleSet.Roles(0).Id)
            groupPermissions.Add(grpLevelPerms)

            Dim resourcePerms = New MemberPermissions(groupPermissions)

            Assert.Throws(Of BluePrismException)(Function() CType(ReflectionHelper.InvokePrivateMethod("GetExceptionScreenshot", server, databaseConnectionMock.Object, resourceId, resourcePerms), clsScreenshotDetails))
        End Sub


        Private Class TestPermission : Inherits Permission
            Public Sub New(id As Integer, name As String)
                MyBase.New(id, name, Feature.None)
            End Sub
        End Class

    End Class

End Namespace
#End If
