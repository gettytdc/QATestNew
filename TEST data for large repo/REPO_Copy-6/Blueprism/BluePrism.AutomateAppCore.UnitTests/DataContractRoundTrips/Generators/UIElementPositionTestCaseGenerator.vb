#If UNITTESTS Then
Namespace DataContractRoundTrips.Generators


    Public Class UIElementPositionTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim el As New clsUIElementPosition("element1", "l"c, 100, 150, True)
            Yield Create("Simple", el)

        End Function
    End Class

End Namespace
#End If
