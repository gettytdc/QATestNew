Imports System.Runtime.Serialization


''' <summary>
''' Exception thrown when a calendar cannot be deleted (/modified?) because it is
''' currently assigned to something which is preventing such.
''' </summary>
<Serializable()> _
Public Class CalendarInUseException : Inherits BluePrismException
    ''' <summary>
    ''' Deserializes an exception
    ''' </summary>
    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
        MyBase.New(info, context)
    End Sub

    ''' <summary>
    ''' Creates a new exception with the given message
    ''' </summary>
    ''' <param name="msg">The message detailing the error</param>
    Public Sub New(msg As String)
        MyBase.New(msg)
    End Sub

    ''' <summary>
    ''' Creates a new exception with the given formatted message.
    ''' </summary>
    ''' <param name="formatMessage">The message with optional format markers.</param>
    ''' <param name="args">The formatting arguments for the message.</param>
    Public Sub New(ByVal formatMessage As String, ByVal ParamArray args() As Object)
        MyBase.New(formatMessage, args)
    End Sub
End Class
