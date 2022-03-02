#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.Auth

Namespace DataContractRoundTrips.Generators

    Public Class PermissionDataTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim permissions = New Dictionary(Of Integer, Permission)
            permissions.Add(1, Permission.CreatePermission(1, "Perm1"))
            permissions.Add(2, Permission.CreatePermission(2, "Perm2"))

            Dim permissionGroups = New Dictionary(Of Integer, PermissionGroup)
            Dim permGroup1 = New PermissionGroup(1, "group1")
            permGroup1.Add(permissions(1))
            Dim permGroup2 = New PermissionGroup(2, "group2")
            permGroup2.Add(permissions(2))
            permissionGroups.Add(1, permGroup1)
            permissionGroups.Add(2, permGroup2)

            Dim result1 = New PermissionData(permissions, permissionGroups)
            Yield Create("Permission Data", result1)
        End Function
    End Class

End Namespace
#End If
