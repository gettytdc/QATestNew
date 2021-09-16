#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore
Imports Moq

Namespace DataContractRoundTrips.Generators

    Public Class ProcessSubSheetTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim mockSubSheet = New clsProcessSubSheet(New clsProcess(Mock.Of(Of IGroupObjectDetails), Processes.DiagramType.Process, False))

            Yield Create("Process Sub Sheet", mockSubSheet, Function(options) options.Excluding(Function(x) x.Index).
                            Excluding(Function(x) x.StartStage).
                            Excluding(Function(x) x.EndStage))
        End Function
    End Class
End Namespace
#End If
