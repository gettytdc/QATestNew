#If UNITTESTS Then
Imports BluePrism.Core.ActiveDirectory

Namespace DataContractRoundTrips.Generators

    Public Class QueryPageOptionsTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Yield Create("Query Page Options", New QueryPageOptions(10, 50))
        End Function

    End Class


End Namespace
#End If
