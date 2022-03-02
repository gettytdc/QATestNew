#If UNITTESTS Then
Imports BluePrism.Core.Expressions

Namespace DataContractRoundTrips.Generators

    Public Class BPExpressionTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Yield Create("Expression", BPExpression.FromNormalised("1+2"))
        End Function
    End Class
End Namespace
#End If
