Imports BluePrism.Server.Domain.Models

''' <summary>
''' Exception thrown when an expected argument is found to be empty when it
''' should hold a value.
''' </summary>
<Serializable>
Public Class EmptyArgumentException : Inherits EmptyException

    ''' <summary>
    ''' Creates a new exception with no message.
    ''' </summary>
    Public Sub New()
        MyBase.New()
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
