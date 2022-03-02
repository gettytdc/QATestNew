Imports System.Security.Cryptography

Public Class clsFIPSCompliance

    Shared Function CheckForFIPSCompliance(EncryptAlg As EncryptionAlgorithm) As Boolean
        If Not CryptoConfig.AllowOnlyFipsAlgorithms Then Return True 
        Dim symAlg As SymmetricAlgorithm
        Try
            Select Case EncryptAlg
                Case EncryptionAlgorithm.TripleDES
                    symAlg = New TripleDESCryptoServiceProvider()
                Case EncryptionAlgorithm.Rijndael256
                    symAlg = New RijndaelManaged()
                Case EncryptionAlgorithm.AES256
                    symAlg = New AesCryptoServiceProvider()
            End Select
        Catch Ex As InvalidOperationException
            Return False
        End Try
        Return True
    End Function

End Class
