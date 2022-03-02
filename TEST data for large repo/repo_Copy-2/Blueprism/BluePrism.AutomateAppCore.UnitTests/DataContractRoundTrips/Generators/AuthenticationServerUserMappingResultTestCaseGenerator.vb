#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.AuthenticationServerUserMapping

Namespace DataContractRoundTrips.Generators

    Public Class AuthenticationServerUserMappingResultTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim record = New UserMappingRecord("bpusername", Guid.NewGuid(), "Test", "User", "test@user.com")
            Dim recordResult = UserMappingResult.Success(record)
            Yield Create("Completed", MapUsersResult.Completed(New List(Of UserMappingResult) From {recordResult}))

            Yield Create("Failed", MapUsersResult.Failed(MapUsersErrorCode.InvalidActionInSsoEnvironment))

        End Function

    End Class

End Namespace
#End If
