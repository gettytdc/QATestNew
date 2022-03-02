#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore

Namespace DataContractRoundTrips.Generators

    Public Class ProcessParameterStageTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Yield Create("Process Parameter", New clsProcessParameter())
        End Function
    End Class
End Namespace
#End If
