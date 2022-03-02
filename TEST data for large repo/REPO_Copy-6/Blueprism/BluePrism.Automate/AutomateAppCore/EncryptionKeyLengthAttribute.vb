Imports BluePrism.BPCoreLib

''' <summary>
''' An attribute used to represent the base64 encoded key length for an encryption 
''' algorithm
''' </summary>
Public Class EncryptionKeyLengthAttribute : Inherits Attribute
    Private mLength As Integer = 0

    Public Sub New(ByVal len As Integer)
        mLength = len
    End Sub

    Public ReadOnly Property KeyLength() As Integer
        Get
            Return mLength
        End Get
    End Property

    Public Shared Function GetKeyLengthFor(ByVal enumValue As [Enum]) As Integer
        Dim attr As EncryptionKeyLengthAttribute =
         BPUtil.GetAttributeValue(Of EncryptionKeyLengthAttribute)(enumValue)
        If attr Is Nothing Then Return Nothing
        Return attr.KeyLength
    End Function

End Class