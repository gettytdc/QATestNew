Imports System.Runtime.Serialization

''' <summary>
''' Exception thrown when the key value on the work item differs from the key
''' value in the data which is to be set in it.
''' </summary>
<Serializable()> _
Public Class KeyValueDifferenceException : Inherits BluePrismException

    ''' <summary>
    ''' Creates a new exception with no message
    ''' </summary>
    Public Sub New()
        MyBase.New()
    End Sub

    ''' <summary>
    ''' Creates a new exception with the given formatted message.
    ''' </summary>
    ''' <param name="msg">The message with format placeholders</param>
    ''' <param name="args">The args to slot into the message.</param>
    Public Sub New(ByVal msg As String, ByVal ParamArray args() As Object)
        MyBase.New(msg, args)
    End Sub

    ''' <summary>
    ''' Creates a new exception, serializing it from the given source.
    ''' </summary>
    ''' <param name="info"></param>
    ''' <param name="ctx"></param>
    Protected Sub New(ByVal info As SerializationInfo, ByVal ctx As StreamingContext)
        MyBase.New(info, ctx)
    End Sub

End Class
