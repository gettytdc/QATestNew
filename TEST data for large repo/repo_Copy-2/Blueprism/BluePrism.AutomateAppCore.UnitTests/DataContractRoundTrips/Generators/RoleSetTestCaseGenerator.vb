#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.Auth

Namespace DataContractRoundTrips.Generators

    Public Class RoleSetTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim roleSet = New RoleSet()

            roleSet.Add(New Role("Role 1"))
            roleSet.Add(New Role("Role 2"))
            roleSet.Add(New Role("Role 3"))

            Yield Create("", roleSet)
        End Function
    End Class

End Namespace
#End If
