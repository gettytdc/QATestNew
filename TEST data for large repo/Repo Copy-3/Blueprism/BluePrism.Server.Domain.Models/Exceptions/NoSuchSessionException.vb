Imports System.Runtime.Serialization

''' <summary>
''' Exception thrown when a queue name is required and that which has been
''' specified does not exist.
''' </summary>
<Serializable()> _
Public Class NoSuchSessionException : Inherits NoSuchElementException

    ''' <summary>
    ''' Creates a new exception indicating that no session with the given ID exists 
    ''' was found.
    ''' </summary>
    Public Sub New(ByVal sessionId As Guid)
        MyBase.New(My.Resources.NoSuchSessionException_TheSessionWithID0WasNotFound, sessionId)
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
