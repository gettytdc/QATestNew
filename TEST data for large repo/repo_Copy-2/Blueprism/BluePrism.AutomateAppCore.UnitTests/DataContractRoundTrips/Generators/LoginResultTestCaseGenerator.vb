#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Server.Domain.Models

Namespace DataContractRoundTrips.Generators

    Public Class LoginResultTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim result1 = New Auth.LoginResult(LoginResultCode.BadAccount, Nothing)
            Yield Create("Failed", result1)

            Dim user = New Auth.User(AuthMode.Native, Guid.NewGuid(), "Dave", New RoleSet())
            Dim result2 = New Auth.LoginResult(LoginResultCode.Success, user)
            Yield Create("Success", result2)

        End Function
    End Class

End Namespace
#End If
