#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.Auth

Namespace DataContractRoundTrips.Generators

    ''' <summary>
    ''' Test case generator for permissions classes
    ''' </summary>
    Public Class PermissionsTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim gp1 = BuildGroupPerms()
            Dim mp1 = BuildMemberPerms()

            'Test different concrete group members
            Yield Create("", gp1)
            Yield Create("", mp1)

        End Function

        Private Function BuildGroupPerms() As GroupPermissions
            Dim g As New GroupPermissions(Guid.NewGuid, PermissionState.UnRestricted) With {.InheritedAncestorID = Guid.NewGuid}

            Dim glp As New GroupLevelPermissions(1)
            glp.Add(Permission.CreatePermission(1, "test1"))
            g.Add(glp)
            Return g
        End Function

        Private Function BuildMemberPerms() As MemberPermissions
            Dim p1 = Permission.CreatePermission(1, "test1")
            Dim p2 = Permission.CreatePermission(2, "test2")

            Dim grpPerms As New GroupPermissions(PermissionState.UnRestricted)
            grpPerms.Add(New GroupLevelPermissions(1) From {p1, p2})

            Dim g As New MemberPermissions(grpPerms)
            g.State = PermissionState.Restricted

            Return g
        End Function

    End Class

End Namespace
#End If
