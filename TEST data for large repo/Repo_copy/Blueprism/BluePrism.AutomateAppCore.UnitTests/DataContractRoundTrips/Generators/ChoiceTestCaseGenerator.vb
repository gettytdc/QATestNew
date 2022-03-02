#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.AutomateProcessCore.Stages
Imports Moq

Namespace DataContractRoundTrips.Generators


    Public Class ChoiceTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim stage = New clsChoiceStartStage(New clsProcess(Mock.Of(Of IGroupObjectDetails), DiagramType.Process, False))
            Yield Create("Choice", New clsChoice(stage), Function(options) options.Excluding(Function(x) x.OwningStage))
        End Function
    End Class
End Namespace
#End If
