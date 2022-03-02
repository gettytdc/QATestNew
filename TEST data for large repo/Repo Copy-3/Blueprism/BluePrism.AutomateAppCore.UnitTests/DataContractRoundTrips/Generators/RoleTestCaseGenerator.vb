#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib.Data

Namespace DataContractRoundTrips.Generators

    Public Class RoleTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim role1 = New Role("Role1")
            Yield Create("without AD Group", role1)


            Dim dataProvider = New DictionaryDataProvider(New Hashtable() From {
                                                             {"id", 101},
                                                             {"name", "Test role"},
                                                             {"ssogroup", "Test AD Group"}
                                                             })

            Dim role2 = New Role(dataProvider)
            role2.CopiedFromRoleID = 12

            role2.Add(Permission.CreatePermission(1, "Permission 1"))
            Yield Create("With AD group, and permission", role2)

        End Function

    End Class

End Namespace
#End If
