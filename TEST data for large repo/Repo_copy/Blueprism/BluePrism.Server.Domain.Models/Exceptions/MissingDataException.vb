Imports System.Runtime.Serialization

''' <summary>
''' Exception indicating that an expected data was missing.
''' </summary>
<Serializable()>
Public Class MissingDataException : Inherits BluePrismException

    ''' <summary>
    ''' Creates a new exception with no message
    ''' </summary>
    Public Sub New()
        MyBase.New()
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
    ''' Creates a new exception with the given message.
    ''' </summary>
    ''' <param name="msg">The message</param>
    Public Sub New(msg As String)
        MyBase.New(msg)
    End Sub

    ''' <summary>
    ''' Creates a new exception with the given message and underlying exception.
    ''' </summary>
    ''' <param name="cause">Underlying exception</param>
    ''' <param name="msg">The message</param>
    Public Sub New(cause As Exception, msg As String)
        MyBase.New(cause, msg)
    End Sub

    ''' <summary>
    ''' Creates a new exception with the given message and underlying exception.
    ''' </summary>
    ''' <param name="cause">Underlying exception</param>
    ''' <param name="formattedMsg">The message, with format placeholders, as defined
    ''' in the <see cref="String.Format"/> method.</param>
    ''' <param name="args">Argument used to format the message</param>
    Public Sub New(cause As Exception, formattedMsg As String, ParamArray args As Object())
        MyBase.New(cause, formattedMsg, args)
    End Sub

    ''' <summary>
    ''' Creates a new exception from the given de-serializer
    ''' </summary>
    ''' <param name="info">The serialization info from which this exception should
    ''' draw its data.</param>
    ''' <param name="ctx">The context defining the context for the current
    ''' deserialization stream.</param>
    Protected Sub New(ByVal info As SerializationInfo, ByVal ctx As StreamingContext)
        MyBase.New(info, ctx)
    End Sub

End Class
