Imports System.Runtime.Serialization

''' <summary>
''' Exception thrown when some resource which is expected to be initialised has not
''' been so initialised.
''' </summary>
<Serializable()> _
Public Class NotInitialisedException : Inherits InvalidStateException

    ''' <summary>
    ''' Creates a new exception with a default message.
    ''' </summary>
    Public Sub New()
        MyBase.New()
    End Sub

    ''' <summary>
    ''' Creates a new exception with the given message.
    ''' </summary>
    ''' <param name="msg">The message indicating the reason for the exception</param>
    Public Sub New(ByVal msg As String)
        MyBase.New(msg)
    End Sub

    ''' <summary>
    ''' Creates a new exception with the given parameterized message
    ''' was found.
    ''' </summary>
    ''' <param name="msg">The message for this exception, with arg placeholders
    ''' </param>
    ''' <param name="args">The arguments to use in the format message</param>
    Public Sub New(ByVal msg As String, ByVal ParamArray args() As Object)
        MyBase.New(msg, args)
    End Sub

    ''' <summary>
    ''' Creates a new exception from the given de-serializer
    ''' </summary>
    ''' <param name="info">The serialization info from which this exception
    ''' should draw its data.</param>
    ''' <param name="ctx">The context defining the context for the current
    ''' deserialization stream.</param>
    Protected Sub New(ByVal info As SerializationInfo, ByVal ctx As StreamingContext)
        MyBase.New(info, ctx)
    End Sub

End Class
