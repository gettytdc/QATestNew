#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.Auth

Namespace DataContractRoundTrips.Generators

    Public Class GroupTreePermissionTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim p1 As New GroupTreePermission(Permission.CreatePermission(1, "Test permission"), GroupPermissionLevel.Member)
            Yield Create("Test with member permission", p1)

            Dim p2 As New GroupTreePermission(Permission.CreatePermission(2, "Another permission"), GroupPermissionLevel.Group)
            Yield Create("Test with group permission", p2)
        End Function

    End Class

End Namespace
#End If
