Imports BluePrism.Server.Domain.Models

''' <summary>
''' Exception thrown when a collection with some value is supposed to exist,
''' but it is found to be empty.
''' </summary>
<Serializable>
Public Class EmptyCollectionException : Inherits EmptyException

    ''' <summary>
    ''' Creates a new exception with no message.
    ''' </summary>
    Public Sub New()
        Me.New(My.Resources.Resources.EmptyCollectionException_TheCollectionIsEmpty)
    End Sub

    ''' <summary>
    ''' Creates a new exception with the given message.
    ''' </summary>
    ''' <param name="msg">The message explaining the exception</param>
    Public Sub New(ByVal msg As String)
        MyBase.New(msg)
    End Sub

    ''' <summary>
    ''' Creates a new exception with the given formatted message.
    ''' </summary>
    ''' <param name="msg">The message, with optional formatting markers, as
    ''' defined in <see cref="String.Format"/>.</param>
    ''' <param name="args">The arguments for the message.</param>
    Public Sub New(ByVal msg As String, ByVal ParamArray args() As Object)
        MyBase.New(msg, args)
    End Sub

End Class
