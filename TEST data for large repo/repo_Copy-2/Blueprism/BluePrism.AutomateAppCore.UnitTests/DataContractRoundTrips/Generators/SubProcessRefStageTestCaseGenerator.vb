#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages
Imports Moq

Namespace DataContractRoundTrips.Generators
    Public Class SubProcessRefStageTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim mockExternalObjects = Mock.Of(Of IGroupObjectDetails)
            Dim mockProcess = New clsProcess(mockExternalObjects, Processes.DiagramType.Process, bEditable:=False)
            Dim stage = New clsSubProcessRefStage(mockProcess)

            Yield Create("Sub Process Ref Stage", stage, Function(options) options.Excluding(Function(x) x.SubSheet).
                                                                            Excluding(Function(x) x.Process).
                                                                            Excluding(Function(x) x.DisplayIdentifer).
                                                                            Excluding(Function(x) x.Font).
                                                                            Excluding(Function(x) x.WarningThreshold).
                                                                            Excluding(Function(x) x.OverrideDefaultWarningThreshold))
        End Function
    End Class
End Namespace
#End If
