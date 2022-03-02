#If UNITTESTS Then
Namespace DataContractRoundTrips.Generators


    Public Class EncryptionSchemeTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim sch As New clsEncryptionScheme(1, "Credentials Key") With {
                    .Algorithm = EncryptionAlgorithm.AES256,
                    .KeyLocation = EncryptionKeyLocation.Server,
                    .IsAvailable = True}
            sch.GenerateKey()

            Yield Create("Credentials Key", sch)

        End Function

    End Class

End Namespace
#End If
