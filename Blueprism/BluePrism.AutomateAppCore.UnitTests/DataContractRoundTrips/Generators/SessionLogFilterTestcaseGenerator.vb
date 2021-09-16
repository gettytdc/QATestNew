#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.BPCoreLib

Namespace DataContractRoundTrips.Generators

    Public Class SessionLogFilterTestcaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Yield Create("Empty", New clsSessionLogFilter())

            Dim slf As New clsSessionLogFilter(DiagramType.Object, New Dictionary(Of String, Object) From {
                                                  {clsSessionLogFilter.FilterNames.SessionNo, 842},
                                                  {clsSessionLogFilter.FilterNames.Process, "Test Process"},
                                                  {clsSessionLogFilter.FilterNames.StartDate, New clsDateRange(Date.Today.AddDays(-10), Date.Today)},
                                                  {clsSessionLogFilter.FilterNames.EndDate, New clsDateRange(Date.Today.AddDays(-5), Date.Today)},
                                                  {clsSessionLogFilter.FilterNames.Status, ""},
                                                  {clsSessionLogFilter.FilterNames.SourceLocn, ""},
                                                  {clsSessionLogFilter.FilterNames.TargetLocn, ""},
                                                  {clsSessionLogFilter.FilterNames.WindowsUser, "Admin"}})
            Yield Create("Populated", slf)

        End Function

    End Class

End Namespace
#End If
