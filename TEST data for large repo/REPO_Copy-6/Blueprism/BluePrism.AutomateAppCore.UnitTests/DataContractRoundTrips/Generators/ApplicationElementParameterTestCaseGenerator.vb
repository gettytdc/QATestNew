#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore

Namespace DataContractRoundTrips.Generators

    Public Class ApplicationElementParameterTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Yield Create("Application Element Parameter", New clsApplicationElementParameter())
        End Function
    End Class

End Namespace
#End If
