Imports System.Runtime.Serialization
Imports BluePrism.Server.Domain.Models

''' ---------------------------------------------------------------------------
''' <summary>
''' Exception thrown when there is no current row for the collection.
''' </summary>
''' ---------------------------------------------------------------------------
<Serializable>
Public Class NoCurrentRowException : Inherits BluePrismException

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Creates a new exception with no message
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Sub New()
        MyBase.New()
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Creates a new exception with the given message.
    ''' </summary>
    ''' <param name="msg">The message explaining the exception</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal msg As String)
        MyBase.New(msg)
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Creates a new exception with the given formatted message.
    ''' </summary>
    ''' <param name="formattedMsg">The message, with format placeholders, as
    ''' defined in the <see cref="String.Format"/> method.</param>
    ''' <param name="args">The arguments for the formatted message.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal formattedMsg As String, ByVal ParamArray args() As Object)
        MyBase.New(formattedMsg, args)
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Creates a new exception from the given de-serializer
    ''' </summary>
    ''' <param name="info">The serialization info from which this exception
    ''' should draw its data.</param>
    ''' <param name="ctx">The context defining the context for the current
    ''' deserialization stream.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(ByVal info As SerializationInfo, ByVal ctx As StreamingContext)
        MyBase.New(info, ctx)
    End Sub

End Class
