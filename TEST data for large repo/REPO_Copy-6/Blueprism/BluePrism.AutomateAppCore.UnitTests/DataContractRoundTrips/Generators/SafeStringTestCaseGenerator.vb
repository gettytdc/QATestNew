#If UNITTESTS Then
Imports BluePrism.Common.Security
Imports BluePrism.Core

Namespace DataContractRoundTrips.Generators

    Public Class SafeStringTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() _
            As IEnumerable(Of IRoundTripTestCase)

            For Each p As String In TestUtil.PasswordTests
                Dim s As SafeString = p.AsSecureString()
                'Just need to check that the underlying password survives the round trip
                Yield CreateWithCustomState(String.Format("Password : ""{0}""", p), s,
                                            Function(a) a.AsString)
            Next

        End Function

    End Class

End Namespace
#End If
