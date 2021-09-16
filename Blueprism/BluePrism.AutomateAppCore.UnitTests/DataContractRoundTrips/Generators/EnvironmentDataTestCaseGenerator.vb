#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.Logging

Namespace DataContractRoundTrips.Generators

    Public Class EnvironmentDataTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim var As New EnvironmentData(EnvironmentType.Client, "FQDN", 8200, "6.7.0.0", Date.UtcNow, Date.UtcNow, DateTime.UtcNow)

            Yield Create("Environment Data", var)

        End Function
    End Class
End Namespace
#End If
