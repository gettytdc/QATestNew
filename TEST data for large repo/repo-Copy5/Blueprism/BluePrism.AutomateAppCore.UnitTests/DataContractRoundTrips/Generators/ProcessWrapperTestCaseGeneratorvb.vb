#If UNITTESTS Then
Namespace DataContractRoundTrips.Generators


    Public Class ProcessWrapperTestCaseGeneratorvb
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim processComponent = New ProcessComponent(Nothing, New Guid(), "Process1")
            Dim processWrapper = New ProcessComponent.ProcessWrapper(processComponent, "<xml><process></process></xml>")
            Yield Create("Simple", processWrapper)
        End Function
    End Class

End Namespace
#End If
