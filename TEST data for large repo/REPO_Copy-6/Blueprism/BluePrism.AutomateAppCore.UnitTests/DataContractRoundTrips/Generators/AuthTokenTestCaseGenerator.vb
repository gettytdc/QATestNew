#If UNITTESTS Then
Namespace DataContractRoundTrips.Generators


    Public Class AuthTokenTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim t1 As New clsAuthToken(Guid.NewGuid(), Guid.NewGuid(), Date.Today.AddDays(1), Guid.Empty)
            Yield Create("New token", t1)

            Dim t2 As New clsAuthToken(t1.ToString)
            SetField(t2, "mToken", Guid.NewGuid)
            Yield CreateWithCustomState("From token value", t2,
                                        Function(a) TestCaseGenerator.GetField(a, "mToken"))
        End Function

    End Class

End Namespace
#End If
