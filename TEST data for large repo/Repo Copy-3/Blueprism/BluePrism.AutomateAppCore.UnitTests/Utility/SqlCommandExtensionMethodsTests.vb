#If UNITTESTS Then

Imports System.Data.SqlClient
Imports BluePrism.AutomateAppCore.Utility
Imports NUnit.Framework
Imports FluentAssertions
Imports Moq
Imports BluePrism.Server.Domain.Models.DataFilters
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Auth.Role

Namespace Utility

    <TestFixture()>
    Public Class SqlCommandExtensionMethodsTests

        <Test>
        Public Sub BuildSqlInStatement_WithNoValues_ShouldReturnFalseStatement()

            Dim cmd As New SqlCommand
            Dim result = cmd.BuildSqlInStatement("id", Enumerable.Empty(Of String))
            Assert.That(result, Iz.EqualTo("0 <> 0"))

        End Sub

        <Test>
        Public Sub BuildSqlInStatement_WithNoValues_ShouldAddNoParametersToCommand()

            Dim cmd As New SqlCommand
            cmd.BuildSqlInStatement("id", Enumerable.Empty(Of String))
            Assert.That(cmd.Parameters.Count, Iz.EqualTo(0))

        End Sub

        <Test>
        Public Sub BuildSqlInStatement_WithValues_ShouldReturnCorrectInStatement()

            Dim cmd As New SqlCommand
            Dim result = cmd.BuildSqlInStatement("parameterid", {1, 2, 3})
            Assert.That(result, Iz.EqualTo("parameterid in (@param0, @param1, @param2)").IgnoreCase)


        End Sub


        <Test>
        Public Sub BuildSqlInStatement_WithIntegerValues_ShouldAddParametersToCommand()

            Dim cmd As New SqlCommand
            cmd.BuildSqlInStatement("parameterid", {1, 2, 3})

            Dim expectedResult As New Dictionary(Of String, Object)
            expectedResult.Add("@param0", 1)
            expectedResult.Add("@param1", 2)
            expectedResult.Add("@param2", 3)

            Dim actualResult = cmd.
                                Parameters.
                                OfType(Of SqlParameter).
                                ToDictionary(Function(x) x.ParameterName,
                                             Function(x) x.Value)

            actualResult.ShouldBeEquivalentTo(expectedResult)

        End Sub

        <Test>
        Public Sub BuildSqlInStatement_WithStringValues_ShouldAddCorrectParametersToCommand()

            Dim cmd As New SqlCommand
            cmd.BuildSqlInStatement("parameterid", {"Test", "This"})

            Dim expectedResult As New Dictionary(Of String, Object)
            expectedResult.Add("@param0", "Test")
            expectedResult.Add("@param1", "This")

            Dim actualResult = cmd.
                                Parameters.
                                OfType(Of SqlParameter).
                                ToDictionary(Function(x) x.ParameterName, Function(x) x.Value)

            actualResult.ShouldBeEquivalentTo(expectedResult)

        End Sub

        <Test>
        Public Sub BuildSqlInStatement_WithGuidValues_ShouldAddCorrectParametersToCommand()

            Dim g1 = Guid.NewGuid()
            Dim g2 = Guid.NewGuid()

            Dim cmd As New SqlCommand
            cmd.BuildSqlInStatement("parameterid", {g1, g2})

            Dim expectedResult As New Dictionary(Of String, Object)
            expectedResult.Add("@param0", g1)
            expectedResult.Add("@param1", g2)

            Dim actualResult = cmd.
                                Parameters.
                                OfType(Of SqlParameter).
                                ToDictionary(Function(x) x.ParameterName, Function(x) x.Value)

            actualResult.ShouldBeEquivalentTo(expectedResult)

        End Sub

        <Test>
        Public Sub BuildSqlInStatement_WithBooleanValues_ShouldAddCorrectParametersToCommand()

            Dim cmd As New SqlCommand
            cmd.BuildSqlInStatement("parameterid", {True, False})

            Dim expectedResult As New Dictionary(Of String, Object)
            expectedResult.Add("@param0", True)
            expectedResult.Add("@param1", False)

            Dim actualResult = cmd.
                                Parameters.
                                OfType(Of SqlParameter).
                                ToDictionary(Function(x) x.ParameterName, Function(x) x.Value)

            actualResult.ShouldBeEquivalentTo(expectedResult)

        End Sub

        <Test>
        Public Sub BuildSqlInStatement_WithExistingParams_ShouldAddToNotOverwriteExistingParams()

            Dim cmd As New SqlCommand
            cmd.Parameters.AddWithValue("@existingparam", 1)
            cmd.BuildSqlInStatement("parameterid", {2})

            Dim expectedResult As New Dictionary(Of String, Object)
            expectedResult.Add("@existingparam", 1)
            expectedResult.Add("@param0", 2)

            Dim actualResult = cmd.
                                Parameters.
                                OfType(Of SqlParameter).
                                ToDictionary(Function(x) x.ParameterName, Function(x) x.Value)

            actualResult.ShouldBeEquivalentTo(expectedResult)

        End Sub


        <Test>
        Public Sub BuildSqlInStatement_WithParamPrefix_ShouldReturnCorrectInStatement()

            Dim cmd As New SqlCommand
            Dim result = cmd.BuildSqlInStatement("parameterid", {1, 2, 3}, "test")
            Assert.That(result, Iz.EqualTo("parameterid in (@test0, @test1, @test2)").IgnoreCase)


        End Sub

        <Test>
        Public Sub BuildSqlInStatement_WithParamPrefix_ShouldAddParametersToCommand()

            Dim cmd As New SqlCommand
            cmd.BuildSqlInStatement("parameterid", {1, 2, 3}, "test")

            Dim expectedResult As New Dictionary(Of String, Object)
            expectedResult.Add("@test0", 1)
            expectedResult.Add("@test1", 2)
            expectedResult.Add("@test2", 3)

            Dim actualResult = cmd.
                                Parameters.
                                OfType(Of SqlParameter).
                                ToDictionary(Function(x) x.ParameterName,
                                             Function(x) x.Value)

            actualResult.ShouldBeEquivalentTo(expectedResult)

        End Sub

        <Test>
        <TestCase(DefaultNames.RuntimeResources)>
        <TestCase(DefaultNames.SystemAdministrators)>
        Public Sub GetQueryAndSetParameters_WithValidGlobalRoles_ShouldCallBuildQueryStringWithNoEnumerables(role As String)

            Dim mteMock = New Mock(Of IMteSqlGenerator)
            Dim mteUser = New Mock(Of IUser)
            mteUser.Setup(Function(user) user.Roles).Returns(New RoleSet From {
                New Role(role)
            })

            GetQueryAndSetParameters(mteMock.Object, mteUser.Object, New SqlCommand())

            mteMock.Verify(Function(sql) sql.BuildQueryString(
                It.IsAny(Of SqlCommand),
                Nothing,
                Nothing,
                Nothing), Times.Once)

        End Sub

        <Test>
        Public Sub GetQueryAndSetParameters_WithoutValidGlobalRoles_ShouldCallBuildQueryStringOnce()

            Dim permDict = Permission.ProcessStudio _
            .AllProcessPermissionsAllowingTreeView _
            .Union(Permission.Resources.ImpliedViewResource) _
            .Select(Function(perm, index) New With {index, perm}) _
            .ToDictionary(Function(item) item.index, Function(item) Permission.CreatePermission(item.index, item.perm))

            Dim permGroup = permDict _
                 .Select(Function(keyValuePair) New PermissionGroup(keyValuePair.Key, keyValuePair.Value.Name)) _
                 .ToDictionary(Function(permGroupItem) permGroupItem.Id)

            Dim mockPermissionData = New PermissionData(permDict, permGroup)

            Dim serverMock = New Mock(Of IServer)
            serverMock.Setup(Function(m) m.GetPermissionData()).Returns(mockPermissionData)

            Permission.Init(serverMock.Object)

            Dim mteMock = New Mock(Of IMteSqlGenerator)
            Dim mteUser = New Mock(Of IUser)
            mteUser.Setup(Function(user) user.Roles).Returns(New RoleSet From {New Role(DefaultNames.Developers)})

            GetQueryAndSetParameters(mteMock.Object, mteUser.Object, New SqlCommand())

            mteMock.Verify(Function(sql) sql.BuildQueryString(
                It.IsAny(Of SqlCommand),
                It.IsAny(Of IReadOnlyCollection(Of Integer)),
                It.IsAny(Of IReadOnlyCollection(Of Integer)),
                It.IsAny(Of IReadOnlyCollection(Of Integer))), Times.Once)

        End Sub

        <Test>
        <TestCase(DefaultNames.RuntimeResources)>
        <TestCase(DefaultNames.SystemAdministrators)>
        Public Sub GetResourceQueryAndSetParameters_WithValidGlobalRoles_ShouldCallBuildQueryStringWithNoEnumerables(role As String)

            Dim mteMock = New Mock(Of IMteResourceSqlGenerator)
            Dim mteUser = New Mock(Of IUser)
            mteUser.Setup(Function(user) user.Roles).Returns(New RoleSet From {
                New Role(role)
            })

            GetResourceQueryAndSetParameters(mteMock.Object, mteUser.Object, New SqlCommand())

            mteMock.Verify(Function(sql) sql.ReplaceTokenAndAddParameters(
                It.IsAny(Of SqlCommand),
                Nothing,
                Nothing), Times.Once)

        End Sub

        <Test>
        Public Sub GetResourceQueryAndSetParameters_WithoutValidGlobalRoles_ShouldCallBuildQueryStringOnce()

            Dim permDict = Permission.Resources.ImpliedViewResource _
            .Select(Function(perm, index) New With {index, perm}) _
            .ToDictionary(Function(item) item.index, Function(item) Permission.CreatePermission(item.index, item.perm))

            Dim permGroup = permDict _
                 .Select(Function(keyValuePair) New PermissionGroup(keyValuePair.Key, keyValuePair.Value.Name)) _
                 .ToDictionary(Function(permGroupItem) permGroupItem.Id)

            Dim mockPermissionData = New PermissionData(permDict, permGroup)

            Dim serverMock = New Mock(Of IServer)
            serverMock.Setup(Function(m) m.GetPermissionData()).Returns(mockPermissionData)

            Permission.Init(serverMock.Object)

            Dim mteMock = New Mock(Of IMteResourceSqlGenerator)
            Dim mteUser = New Mock(Of IUser)
            mteUser.Setup(Function(user) user.Roles).Returns(New RoleSet From {New Role(DefaultNames.Developers)})

            GetResourceQueryAndSetParameters(mteMock.Object, mteUser.Object, New SqlCommand())

            mteMock.Verify(Function(sql) sql.ReplaceTokenAndAddParameters(
                It.IsAny(Of SqlCommand),
                It.IsAny(Of IReadOnlyCollection(Of Integer)),
                It.IsAny(Of IReadOnlyCollection(Of Integer))), Times.Once)

        End Sub

    End Class
End Namespace

#End If
