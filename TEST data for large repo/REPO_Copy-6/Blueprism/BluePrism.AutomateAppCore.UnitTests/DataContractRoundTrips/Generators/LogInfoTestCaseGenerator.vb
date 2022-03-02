#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore

Namespace DataContractRoundTrips.Generators

    Public Class LogInfoTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim li As New LogInfo() With {
                    .Inhibit = True,
                    .InhibitParams = True,
                    .TargetAppName = "Calculator",
                    .TargetAppWorkingSet = 1024L,
                    .AutomateWorkingSet = 1024L}

            Yield Create("Default", New LogInfo())

            Yield Create("Populated", li)

        End Function

    End Class

End Namespace
#End If
