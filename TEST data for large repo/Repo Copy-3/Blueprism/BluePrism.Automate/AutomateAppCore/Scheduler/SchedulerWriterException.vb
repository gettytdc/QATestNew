Imports System.Runtime.Serialization
Imports BluePrism.Server.Domain.Models

''' <summary>
''' Exception to throw if the writer fails
''' </summary>
<Serializable()> _
Public Class ScheduleWriterException
    Inherits BluePrismException

    ''' <summary>
    ''' Create a new exception with the given formatted message.
    ''' </summary>
    ''' <param name="message">The message format string, as defined by the
    ''' String.Format() method.</param>
    ''' <param name="args">The arguments to use in the formatted message
    ''' </param>
    Public Sub New(ByVal message As String, ByVal ParamArray args() As Object)
        MyBase.New(message, args)
    End Sub

    ''' <summary>
    ''' Creates a new exception deserialized from the given objects.
    ''' </summary>
    ''' <param name="info">The serialization information for the object.</param>
    ''' <param name="ctx">The context in which the object is being streamed.</param>
    Protected Sub New(ByVal info As SerializationInfo, ByVal ctx As StreamingContext)
        MyBase.New(info, ctx)
    End Sub
End Class
