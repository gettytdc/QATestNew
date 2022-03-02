Imports System.Runtime.Serialization

''' <summary>
''' Exception thrown when a work queue item state was invalid for the requested
''' operation to proceed.
''' </summary>
<Serializable()> _
Public Class InvalidWorkItemStateException : Inherits BluePrismException

    ''' <summary>
    ''' Creates a new exception indicating that an invalid work queue item state
    ''' has been discovered, preventing an operation from being performed.
    ''' </summary>
    ''' <param name="msg">The message detailing the exception</param>
    Public Sub New(ByVal msg As String)
        MyBase.New(msg)
    End Sub

    ''' <summary>
    ''' Creates a new exception with the given formatted message
    ''' </summary>
    ''' <param name="formatMsg">The message with formatting placeholders</param>
    ''' <param name="args">The arguments for the above format message</param>
    Public Sub New(ByVal formatMsg As String, ByVal ParamArray args() As Object)
        MyBase.New(formatMsg, args)
    End Sub

    ''' <summary>
    ''' Deserializes an exception indicating that an invalid work queue item state
    ''' has been encountered.
    ''' </summary>
    Protected Sub New( _
                      ByVal info As SerializationInfo, ByVal context As StreamingContext)
        MyBase.New(info, context)
    End Sub

End Class
