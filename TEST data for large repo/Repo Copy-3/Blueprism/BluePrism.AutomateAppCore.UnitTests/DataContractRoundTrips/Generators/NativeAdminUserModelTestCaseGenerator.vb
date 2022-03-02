#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Server.Domain.Models

Namespace DataContractRoundTrips.Generators

    Public Class NativeAdminUserModelTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim user = New User(AuthMode.Native, New Guid(), "User")
            Dim nativeAdminUser As New NativeAdminUserModel(user, New Common.Security.SafeString())

            Yield Create("Simple", nativeAdminUser)

        End Function
    End Class
End Namespace
#End If
