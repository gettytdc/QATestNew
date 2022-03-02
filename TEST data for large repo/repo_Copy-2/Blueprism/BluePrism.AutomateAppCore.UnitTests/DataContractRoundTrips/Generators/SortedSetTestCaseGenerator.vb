#If UNITTESTS Then
Imports BluePrism.BPCoreLib.Collections

Namespace DataContractRoundTrips.Generators

    Public Class SortedSetTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim s As New clsSortedSet(Of String)
            s.Add("monday")
            s.Add("tuesday")
            s.Add("wednesday")

            Yield Create("Default", New clsSortedSet(Of String))

            Yield Create("Populated", s)

        End Function

    End Class

End Namespace
#End If
