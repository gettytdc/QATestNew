Imports System.Runtime.Serialization
Imports BluePrism.Server.Domain.Models

''' <summary>
''' Exception indicating that an attempt to retrieve an AccessToken for a web api 
''' request has failed.
''' </summary>
<Serializable()>
Public Class ConfigNotFoundException : Inherits BluePrismException

    ''' <summary>
    ''' Creates a new exception with no message
    ''' </summary>
    Public Sub New()
        MyBase.New()
    End Sub

    ''' <summary>
    ''' Creates a new exception with the given (literal) message.
    ''' </summary>
    ''' <param name="msg">The message detailing the exception, with no placeholder
    ''' strings.</param>
    Public Sub New(ByVal msg As String)
        MyBase.New(msg)
    End Sub
End Class
