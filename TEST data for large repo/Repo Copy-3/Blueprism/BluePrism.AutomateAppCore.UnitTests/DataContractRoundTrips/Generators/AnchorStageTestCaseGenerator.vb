#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.AutomateProcessCore.Stages
Imports Moq

Namespace DataContractRoundTrips.Generators

    Public Class AnchorStageTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim stage = New clsAnchorStage(New clsProcess(Mock.Of(Of IGroupObjectDetails), DiagramType.Process, False))
            Yield Create("Anchor Stage", stage, Function(options) options.Excluding(Function(x) x.SubSheet).
                            Excluding(Function(x) x.Process).
                            Excluding(Function(x) x.DisplayIdentifer).
                            Excluding(Function(x) x.Font).
                            Excluding(Function(x) x.WarningThreshold).
                            Excluding(Function(x) x.OverrideDefaultWarningThreshold))
        End Function
    End Class
End Namespace
#End If
