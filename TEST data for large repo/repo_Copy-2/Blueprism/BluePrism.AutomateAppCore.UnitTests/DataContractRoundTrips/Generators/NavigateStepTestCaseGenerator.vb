#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.AutomateProcessCore.Stages
Imports Moq

Namespace DataContractRoundTrips.Generators

    Public Class NavigateStepTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim stage = New clsNavigateStage(New clsProcess(Mock.Of(Of IGroupObjectDetails), DiagramType.Process, False))
            Yield Create("Navigate Step", New clsNavigateStep(stage), Function(options) options.Excluding(Function(x) x.Owner))
        End Function
    End Class
End Namespace
#End If
