Imports System.Runtime.Serialization

''' <summary>
''' Exception thrown when a requested preference does not exist.
''' </summary>
<Serializable()> _
Public Class NoSuchEncrypterException : Inherits NoSuchElementException

    ''' <summary>
    ''' Creates a new exception with the given formatted message.
    ''' </summary>
    ''' <param name="encrypterName">The name of the encrypter which could not be
    ''' found.</param>
    Public Sub New(ByVal encrypterName As String)
        MyBase.New("No Encrypter/Decrypter with the name '{0}' was found", encrypterName)
    End Sub

    ''' <summary>
    ''' Deserializes a no such preference exception.
    ''' </summary>
    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
        MyBase.New(info, context)
    End Sub

End Class
