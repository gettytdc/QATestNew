#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.AuthenticationServerUserMapping

Namespace DataContractRoundTrips.Generators

    Public Class AuthenticationServerUserMappingRecordTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim record = New UserMappingRecord("bpusername", Guid.NewGuid(), "Test", "User", "test@user.com")
            Yield Create("Simple", record)

        End Function

    End Class

End Namespace
#End If
