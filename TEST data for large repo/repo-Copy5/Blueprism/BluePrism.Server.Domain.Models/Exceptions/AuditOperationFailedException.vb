Imports System.Data.SqlClient
Imports System.Runtime.Serialization

''' <summary>
''' Exception thrown when an audit operation failed
''' </summary>
<Serializable>
Public Class AuditOperationFailedException : Inherits OperationFailedException

    ''' <summary>
    ''' Creates a new exception with no message
    ''' </summary>
    Public Sub New()
        MyBase.New()
    End Sub

    ''' <summary>
    ''' Creates a new exception with the given message
    ''' </summary>
    ''' <param name="msg">The message detailing the problem</param>
    Public Sub New(ByVal msg As String)
        MyBase.New(msg)
    End Sub

    ''' <summary>
    ''' Creates a new exception with the given formatted message.
    ''' </summary>
    ''' <param name="formattedMsg">The message, with format placeholders, as defined
    ''' in the <see cref="String.Format"/> method.</param>
    ''' <param name="args">The arguments for the formatted message.</param>
    Public Sub New(ByVal formattedMsg As String, ByVal ParamArray args() As Object)
        MyBase.New(formattedMsg, args)
    End Sub

    ''' <summary>
    ''' Creates a new exception with the given cause
    ''' </summary>
    ''' <param name="cause">The cause of the operation failure</param>
    Public Sub New(cause As SqlException)
        Me.New(cause,
               My.Resources.AuditOperationFailedException_AnErrorOccurredWhileWritingAnAuditRecord0, cause.Message)
    End Sub

    ''' <summary>
    ''' Creates a new exception with the given message and cause.
    ''' </summary>
    ''' <param name="cause">The root cause of this exception.</param>
    ''' <param name="msg">The message detailing the exception.</param>
    Public Sub New(ByVal cause As Exception, ByVal msg As String)
        MyBase.New(cause, msg)
    End Sub

    ''' <summary>
    ''' Creates a new exception with the given formatted message and cause
    ''' </summary>
    ''' <param name="cause">The root cause of this exception.</param>
    ''' <param name="formattedMsg">The message, with format placeholders, as defined
    ''' in the <see cref="String.Format"/> method.</param>
    ''' <param name="args">The arguments for the formatted message.</param>
    Public Sub New(ByVal cause As Exception,
                   ByVal formattedMsg As String, ByVal ParamArray args() As Object)
        MyBase.New(cause, formattedMsg, args)
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
