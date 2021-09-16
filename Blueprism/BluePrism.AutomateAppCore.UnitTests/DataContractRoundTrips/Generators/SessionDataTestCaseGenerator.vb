#If UNITTESTS Then
Namespace DataContractRoundTrips.Generators


    Public Class SessionDataTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim sd As New clsServer.SessionData(845, "Admin", "Resource1", "TestProcess")
            Yield Create("Simple", sd)

        End Function

    End Class

End Namespace
#End If
