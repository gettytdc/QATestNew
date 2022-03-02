#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore

Namespace DataContractRoundTrips.Generators

    Public Class ProcessValueTestCaseGenerator
        Inherits TestCaseGenerator
        Shared Property GetProcessValuesForTesting As Object

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            For Each p In TestHelper.CreateProcessValueDictionary()
                Yield Create(p.Key, p.Value)
            Next

        End Function


    End Class

End Namespace
#End If
