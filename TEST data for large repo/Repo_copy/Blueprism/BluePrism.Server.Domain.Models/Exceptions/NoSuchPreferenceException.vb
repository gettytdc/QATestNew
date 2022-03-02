Imports System.Runtime.Serialization

''' <summary>
''' Exception thrown when a requested preference does not exist.
''' </summary>
<Serializable()> _
Public Class NoSuchPreferenceException : Inherits NoSuchElementException

    ''' <summary>
    ''' Creates a new exception with the given formatted message.
    ''' </summary>
    ''' <param name="formatMessage">The message with optional format markers.</param>
    ''' <param name="args">The formatting arguments for the message.</param>
    Public Sub New(ByVal formatMessage As String, ByVal ParamArray args() As Object)
        MyBase.New(formatMessage, args)
    End Sub

    ''' <summary>
    ''' Deserializes a no such preference exception.
    ''' </summary>
    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
        MyBase.New(info, context)
    End Sub

End Class
