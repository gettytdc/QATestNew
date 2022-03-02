Imports System.Runtime.Serialization

''' <summary>
''' Exception thrown when an operation is performed on a view-bound element which no
''' longer operates within the view that it was a part of.
''' </summary>
<Serializable()> _
Public Class NotInModelException : Inherits BluePrismException

    ''' <summary>
    ''' Creates a new exception with a default message.
    ''' </summary>
    Public Sub New()
        MyBase.New("This element is no longer within the model")
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
