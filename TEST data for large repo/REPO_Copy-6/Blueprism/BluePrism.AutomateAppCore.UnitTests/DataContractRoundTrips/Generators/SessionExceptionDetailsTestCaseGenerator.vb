#If UNITTESTS Then

Namespace DataContractRoundTrips.Generators
    Public Class SessionExceptionDetailsTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Yield Create("Internal Error", SessionExceptionDetail.InternalError("Test mesage Internal"))

            Yield Create("Process Error", SessionExceptionDetail.ProcessError("test type", "Test mesage Process"))

        End Function
    End Class

End Namespace
#End If

