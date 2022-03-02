#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.AutomateProcessCore.Stages
Imports Moq

Namespace DataContractRoundTrips.Generators

    Public Class ProcessBreakpointTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim stage = New clsActionStage(New clsProcess(Mock.Of(Of IGroupObjectDetails), DiagramType.Object, True))
            Yield Create(NameOf(clsProcessBreakpoint), New clsProcessBreakpoint(stage), Function(options) options.Excluding(Function(x) x.OwnerStage))
        End Function
    End Class
End Namespace
#End If
