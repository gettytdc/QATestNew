#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.AutomateProcessCore.Stages
Imports Moq

Namespace DataContractRoundTrips.Generators

    Public Class ReadStepTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim stage = New clsReadStage(New clsProcess(Mock.Of(Of IGroupObjectDetails), DiagramType.Process, False))
            Yield Create("Read Step", New clsReadStep(stage), Function(options) options.Excluding(Function(x) x.Owner))
        End Function
    End Class
End Namespace
#End If
