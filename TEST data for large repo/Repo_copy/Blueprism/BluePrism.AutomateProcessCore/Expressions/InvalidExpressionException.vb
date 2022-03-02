Imports System.Runtime.Serialization
Imports BluePrism.Server.Domain.Models

''' <summary>
''' Exception thrown when an expression cannot be evaluated due to some error in
''' the expression or a state in the owning process which the expression cannot
''' be used with.
''' </summary>
<Serializable>
Public Class InvalidExpressionException : Inherits InvalidFormatException

    ''' <summary>
    ''' Creates a new, empty exception
    ''' </summary>
    Public Sub New()
        MyBase.New()
    End Sub

    ''' <summary>
    ''' Creates a new exception with a message
    ''' </summary>
    ''' <param name="msg">The full message for this exception</param>
    Public Sub New(ByVal msg As String)
        MyBase.New(msg)
    End Sub

    ''' <summary>
    ''' Creates a new exception with a message
    ''' </summary>
    ''' <param name="msg">The message with argument placeholders, using the standard
    ''' <see cref="String.Format"/> rules</param>
    ''' <param name="args">The arguments to insert into the appropriate placeholders
    ''' in the message</param>
    Public Sub New(ByVal msg As String, ByVal ParamArray args() As Object)
        MyBase.New(msg, args)
    End Sub

    ''' <summary>
    ''' Creates a new blue prism exception from the given de-serializer
    ''' </summary>
    ''' <param name="info">The serialization info from which this exception should
    ''' draw its data.</param>
    ''' <param name="ctx">The context defining the context for the current
    ''' deserialization stream.</param>
    Protected Sub New(ByVal info As SerializationInfo, ByVal ctx As StreamingContext)
        MyBase.New(info, ctx)
    End Sub
End Class

