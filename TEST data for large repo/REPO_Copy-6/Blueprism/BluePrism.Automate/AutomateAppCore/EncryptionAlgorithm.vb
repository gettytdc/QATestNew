Imports System.Runtime.CompilerServices
Imports System.Security.Cryptography
Imports BluePrism.BPCoreLib
Imports BluePrism.Server.Domain.Models

''' <summary>
''' Supported encryptions algorithms
''' </summary>
Public Enum EncryptionAlgorithm
    None = 0
    <FriendlyName("Triple DES (192 bit)"), EncryptionKeyLength(24), Retired> TripleDES = 1
    <FriendlyName("AES-256 RijndaelManaged (256 bit)"), EncryptionKeyLength(32)> Rijndael256 = 2
    <FriendlyName("AES-256 AesCryptoService (256 bit)"), EncryptionKeyLength(32)> AES256 = 3
End Enum

Public Module EncryptionAlgorithmExtensions
    <Extension()>
    Public Function GetProvider(EncAlg As EncryptionAlgorithm) As SymmetricAlgorithm
        Select Case EncAlg
            Case EncryptionAlgorithm.TripleDES
                Return New TripleDESCryptoServiceProvider()
            Case EncryptionAlgorithm.Rijndael256
                Return New RijndaelManaged()
            Case EncryptionAlgorithm.AES256
                Return New AesCryptoServiceProvider()
            Case Else
                Throw New InvalidValueException(
                 My.Resources.clsEncryptionScheme_CouldNotEncryptDataBecauseTheAlgorithm0IsInvalid, EncAlg)
        End Select
    End Function

    <Extension>
    Public Function IsRetired(algo As EncryptionAlgorithm) As Boolean
        Return BPUtil.GetAttributeValue(Of RetiredAttribute)(algo) IsNot Nothing
    End Function

End Module
