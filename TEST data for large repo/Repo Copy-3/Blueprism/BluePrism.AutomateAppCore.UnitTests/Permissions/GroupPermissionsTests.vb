#If UNITTESTS Then

Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Utilities.Functional
Imports BluePrism.Utilities.Testing

Imports FluentAssertions

Imports Moq

Imports NUnit.Framework

<TestFixture>
Public Class GroupPermissionsTests : Inherits UnitTestBase(Of GroupPermissions)

    Private mPermissionState As PermissionState = PermissionState.Restricted

    Protected Overrides Function TestClassConstructor() As GroupPermissions
        Return New GroupPermissions(mPermissionState) From {GroupLevelPermissions}
    End Function

    <Test>
    <TestCaseSource(NameOf(HasPermissionTestCaseGenerator))>
    Public Sub HasPermissionTest(parameters As (userHasPermission As Boolean, isSystemAdmin As Boolean, isSystemUser As Boolean, permissionState As PermissionState, requestPermission As Permission, expectedResult As Boolean))

        mPermissionState = parameters.permissionState

        Dim mockPermissionData = New PermissionData(Permissions, Groups)

        Dim serverMock = GetMock(Of IServer)()
        serverMock.
            Setup(Function(m) m.GetPermissionData()).
            Returns(mockPermissionData)

        Permission.Init(serverMock.Object)

        Dim userMock = GetMock(Of IUser)()
        userMock.
            Setup(Function(m) m.HasPermission(It.IsAny(Of ICollection(Of Permission)))).
            Returns(parameters.userHasPermission)
        userMock.SetupGet(Function(m) m.IsSystemAdmin).Returns(parameters.isSystemAdmin)
        If parameters.isSystemUser Then
            userMock.SetupGet(Function(m) m.AuthType).Returns(AuthMode.System)
        Else
            userMock.SetupGet(Function(m) m.AuthType).Returns(AuthMode.Native)
        End If
        userMock.
            SetupGet(Function(m) m.Roles).
            Returns(New RoleSet() From {New Role("Test") With {.Id = 0}})

        Dim result = ClassUnderTest.HasPermission(userMock.Object, {parameters.requestPermission.Name})

        result.Should().Be(parameters.expectedResult)

    End Sub

    <Test>
    Public Sub HasPermissionWithMultiplePermissions()

        mPermissionState = PermissionState.Restricted

        Dim mockPermissionData = New PermissionData(Permissions, Groups)

        Dim serverMock = GetMock(Of IServer)()
        serverMock.
            Setup(Function(m) m.GetPermissionData()).
            Returns(mockPermissionData)

        Permission.Init(serverMock.Object)

        Dim userMock = GetMock(Of IUser)()
        userMock.
            Setup(Function(m) m.HasPermission(It.IsAny(Of ICollection(Of Permission)))).
            Returns(True)
        userMock.
            SetupGet(Function(m) m.Roles).
            Returns(New RoleSet() From {New Role("Test") With {.Id = 0}})

        Dim result = ClassUnderTest.HasPermission(userMock.Object, InvalidPermission, Permissions(1))

        result.Should().BeTrue()

    End Sub

    <Test>
    <TestCaseSource(NameOf(MergeSetsCorrectStateTestCaseGenerator))>
    Public Sub MergeSetsCorrectState(parameters As (state1 As PermissionState, state2 As PermissionState, expectedState As PermissionState))

        mPermissionState = parameters.state1
        Dim other = New GroupPermissions(parameters.state2) From {GroupLevelPermissions}

        ClassUnderTest.Merge(other)

        ClassUnderTest.State.Should().Be(parameters.expectedState)

    End Sub

    <Test>
    Public Sub MergeProducesExpectedResultWithNewRole()

        Dim otherPermissions = New GroupLevelPermissions(1234).Tee(Sub(x) x.AddAll(Permissions.Values))

        Dim other = New GroupPermissions(PermissionState.Restricted) From {otherPermissions}

        ClassUnderTest.Merge(other)

        ClassUnderTest.Count.Should().Be(2)
        ClassUnderTest.Select(Function(x) x.Id).Should().BeEquivalentTo(0, 1234)

    End Sub

    <Test>
    Public Sub MergeProducesExpectedResultWithExistingRole()

        Dim anotherPermission = Permission.CreatePermission(321, "Another")
        Dim otherPermissions = New GroupLevelPermissions(0) From {Permissions(0), Permissions(2), anotherPermission}

        Dim other = New GroupPermissions(PermissionState.Restricted) From {otherPermissions}

        ClassUnderTest.Merge(other)

        ClassUnderTest.Count.Should().Be(1)
        ClassUnderTest.SelectMany(Function(x) x.Select(Function(y) y.Name)).Should().BeEquivalentTo(GroupLevelPermissions.Select(Function(x) x.Name).Concat(otherPermissions.Select(Function(x) x.Name)).Distinct)

    End Sub

    Protected Shared Function HasPermissionTestCaseGenerator() As IEnumerable(Of (userHasPermission As Boolean, isSystemAdmin As Boolean, isSystemUser As Boolean, permissionState As PermissionState, requestPermission As Permission, expectedResult As Boolean))

        Return {
                   (False, False, False, PermissionState.UnRestricted, InvalidPermission, False),
                   (True, False, False, PermissionState.UnRestricted, InvalidPermission, True),
                   (True, False, False, PermissionState.Restricted, InvalidPermission, False),
                   (True, False, False, PermissionState.Restricted, Permissions(0), True),
                   (True, False, False, PermissionState.Restricted, Permissions(1), True),
                   (True, False, False, PermissionState.Restricted, Permissions(2), True),
                   (True, False, False, PermissionState.RestrictedByInheritance, InvalidPermission, False),
                   (True, False, False, PermissionState.RestrictedByInheritance, Permissions(0), True),
                   (True, True, False, PermissionState.Restricted, InvalidPermission, True),
                   (True, False, True, PermissionState.Restricted, InvalidPermission, True)
               }

    End Function

    Protected Shared Function MergeSetsCorrectStateTestCaseGenerator() As IEnumerable(Of (state1 As PermissionState, state2 As PermissionState, expectedResult As PermissionState))

        Return {
                    (PermissionState.Unknown, PermissionState.UnRestricted, PermissionState.UnRestricted),
                    (PermissionState.UnRestricted, PermissionState.UnRestricted, PermissionState.UnRestricted),
                    (PermissionState.Restricted, PermissionState.UnRestricted, PermissionState.UnRestricted),
                    (PermissionState.UnRestricted, PermissionState.Restricted, PermissionState.UnRestricted),
                    (PermissionState.Restricted, PermissionState.Restricted, PermissionState.Restricted),
                    (PermissionState.RestrictedByInheritance, PermissionState.UnRestricted, PermissionState.UnRestricted),
                    (PermissionState.UnRestricted, PermissionState.RestrictedByInheritance, PermissionState.UnRestricted),
                    (PermissionState.RestrictedByInheritance, PermissionState.RestrictedByInheritance, PermissionState.RestrictedByInheritance),
                    (PermissionState.RestrictedByInheritance, PermissionState.Restricted, PermissionState.Restricted),
                    (PermissionState.Restricted, PermissionState.RestrictedByInheritance, PermissionState.Restricted)
               }

    End Function

    Private Shared ReadOnly InvalidPermission As Permission =
                                Permission.CreatePermission(123, "Invalid")

    Private Shared ReadOnly Permissions As Dictionary(Of Integer, Permission) =
        {"Test", "Test2", "Test3"}.
        Map(AddressOf AssignIndexes).
        Select(Function(x) Permission.CreatePermission(x.index, x.value)).
        Concat({InvalidPermission}).
        ToDictionary(Function(k) k.Id, Function(v) v)

    Private Shared ReadOnly Groups As Dictionary(Of Integer, PermissionGroup) =
        {"Test", "Test2", "Test3"}.
        Map(AddressOf AssignIndexes).
        Select(Function(x) New PermissionGroup(x.index, x.value)).
        ToDictionary(Function(k) k.Id, Function(v) v)

    Private Shared ReadOnly GroupLevelPermissions As GroupLevelPermissions =
        Permissions.
        Values.
        Where(Function(x) Not x.Equals(InvalidPermission)).
        Map(Function(x) New GroupLevelPermissions(0).Tee(Sub(glp) glp.AddAll(x)))

    Private Shared Function AssignIndexes(Of T)(items As IEnumerable(Of T)) As IEnumerable(Of (index As Integer, value As T))

        Return _
            items.
                Map(Function(x) (x, Enumerable.Range(0, x.Count()))).
                Map(Function(x) _
                       x.Item2.Zip(x.Item1, Function(i1, i2) (i1, i2)))

    End Function

End Class

#End If
