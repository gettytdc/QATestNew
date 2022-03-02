#If UNITTESTS Then
Imports BluePrism.BPCoreLib

Namespace DataContractRoundTrips.Generators

    Public Class LockFilterTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim map As New Dictionary(Of String, Object)
            With map
                .Add(clsLockFilter.FilterNames.Status, clsLockInfo.LockState.Held)
                .Add(clsLockFilter.FilterNames.Name, "My Lock")
                .Add(clsLockFilter.FilterNames.Resource, "My Resource")
                .Add(clsLockFilter.FilterNames.Process, "My Process")
                .Add(clsLockFilter.FilterNames.LockTime, New clsDateRange(Date.Now.AddHours(-1), Date.Now))
                .Add(clsLockFilter.FilterNames.LastComment, "My Comment")
            End With
            Dim lf As New clsLockFilter(map, 1, 1)

            Yield Create("Lock Filter", lf)
        End Function

    End Class

End Namespace
#End If
