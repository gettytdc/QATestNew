#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.Sessions

Namespace DataContractRoundTrips.Generators

    Public Class SessionSortInfoTestCaseGenerator
        Inherits TestCaseGenerator
        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Yield Create("Simple", New SessionSortInfo(SessionManagementColumn.UserName, SessionSortInfo.SortDirection.Descending))
        End Function
    End Class
End Namespace
#End If
