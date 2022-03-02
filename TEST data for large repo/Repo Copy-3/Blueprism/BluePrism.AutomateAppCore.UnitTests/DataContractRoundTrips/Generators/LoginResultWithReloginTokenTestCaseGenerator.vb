#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Common.Security
Imports BluePrism.Server.Domain.Models

Namespace DataContractRoundTrips.Generators

    Public Class LoginResultWithReloginTokenTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim result1 = New LoginResultWithReloginToken(LoginResultCode.BadAccount)
            Yield Create("Failed", result1)

            Dim user = New Auth.User(AuthMode.Native, Guid.NewGuid(), "Dave", New RoleSet())
            Dim loginResult = New LoginResult(LoginResultCode.Success, user)
            Dim result2 = New LoginResultWithReloginToken(loginResult, New SafeString("token"))
            Yield Create("Success", result2)
        End Function
    End Class

End Namespace
#End If
