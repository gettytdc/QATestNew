#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.Logging
Imports BluePrism.Core.Resources

Namespace DataContractRoundTrips.Generators

    Public Class ResourceSummaryDataTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim var As New ResourceSummaryData("ResourceID", "FQDN", "ResourceId", ResourceAttribute.Retired, "Pool", "ResourceId", "UserId", AutomateProcessCore.clsAPC.Diags.LogMemory, "Test Schedules")

            Yield Create("Resource Logging Data", var)

        End Function
    End Class
End Namespace
#End If
