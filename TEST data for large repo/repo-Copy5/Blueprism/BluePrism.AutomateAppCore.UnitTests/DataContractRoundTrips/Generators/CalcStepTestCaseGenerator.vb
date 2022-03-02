#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.AutomateProcessCore.Stages
Imports Moq

Namespace DataContractRoundTrips.Generators

    Public Class CalcStepTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim stage = New clsCalculationStage(New clsProcess(Mock.Of(Of IGroupObjectDetails), DiagramType.Process, False))
            Yield Create("Calc Step", New clsCalcStep(stage), Function(options) options.Excluding(Function(x) x.Parent))
        End Function
    End Class
End Namespace
#End If
