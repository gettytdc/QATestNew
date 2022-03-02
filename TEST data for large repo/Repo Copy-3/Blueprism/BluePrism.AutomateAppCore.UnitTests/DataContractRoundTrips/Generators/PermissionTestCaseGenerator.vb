#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.Auth

Namespace DataContractRoundTrips.Generators

    Public Class PermissionTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim p As Permission = Permission.CreatePermission(1, "testPerm")
            Yield Create("Permission", p)

        End Function
    End Class

End Namespace
#End If
