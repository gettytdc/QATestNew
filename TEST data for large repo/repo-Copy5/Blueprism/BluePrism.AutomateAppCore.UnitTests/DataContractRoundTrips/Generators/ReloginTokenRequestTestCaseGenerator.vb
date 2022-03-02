#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Common.Security

Namespace DataContractRoundTrips.Generators

    Public Class ReloginTokenRequestTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim result1 = New ReloginTokenRequest("machinename", 1234, New SafeString("token"))
            Yield Create("With Relogin Token", result1)

            Dim result2 = New ReloginTokenRequest("machinename", 1234, Nothing)
            Yield Create("Without Relogin Token", result2)
        End Function
    End Class

End Namespace
#End If
