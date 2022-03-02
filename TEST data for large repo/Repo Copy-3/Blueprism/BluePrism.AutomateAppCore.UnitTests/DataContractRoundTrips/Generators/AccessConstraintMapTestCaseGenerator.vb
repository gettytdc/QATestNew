#If UNITTESTS Then
Imports BluePrism.BPCoreLib.Collections

Namespace DataContractRoundTrips.Generators

    Public Class AccessConstraintMapTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() _
            As IEnumerable(Of IRoundTripTestCase)

            Dim m As New AccessConstraintMap(Of Guid, Integer)(True)

            Yield Create("Simple map", m)

            Dim s As New AccessSet(Of Integer) With {
                    .AccessToAll = True,
                    .DefaultItemSetsAccessToAll = False}


            Yield Create("Collection", s)

        End Function

    End Class

End Namespace
#End If
