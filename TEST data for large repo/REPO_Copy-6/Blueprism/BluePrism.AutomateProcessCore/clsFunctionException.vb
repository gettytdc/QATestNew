Imports System.Runtime.Serialization
Imports BluePrism.Server.Domain.Models

''' <summary>
''' Represents an Function Exception that is raised during evaluation of a function.
''' </summary>
<Serializable()> _
Public Class clsFunctionException : Inherits BluePrismException

    ''' <summary>
    ''' Creates a new exception with a message
    ''' </summary>
    ''' <param name="msg">The message of the exception</param>
    Public Sub New(ByVal msg As String)
        MyBase.New(msg)
    End Sub

    ''' <summary>
    ''' Creates a new exception with a formatted error message
    ''' </summary>
    ''' <param name="formatMsg">The message, with format placeholders.</param>
    ''' <param name="args">The arguments to insert into the message.</param>
    Public Sub New(ByVal formatMsg As String, ByVal ParamArray args() As Object)
        MyBase.New(formatMsg, args)
    End Sub

    ''' <summary>
    ''' Creates a new exception with a message and cause
    ''' </summary>
    ''' <param name="cause">The cause of this exception, to be used as the inner
    ''' exception of this exception.</param>
    ''' <param name="msg">The message of the exception</param>
    Public Sub New(ByVal cause As Exception, ByVal msg As String)
        MyBase.New(cause, msg)
    End Sub

    ''' <summary>
    ''' Creates a new exception with a formatted error message and cause
    ''' </summary>
    ''' <param name="cause">The cause of this exception, to be used as the inner
    ''' exception of this exception.</param>
    ''' <param name="formatMsg">The message, with format placeholders.</param>
    ''' <param name="args">The arguments to insert into the message.</param>
    Public Sub New(ByVal cause As Exception, _
     ByVal formatMsg As String, ByVal ParamArray args() As Object)
        MyBase.New(cause, formatMsg, args)
    End Sub

    ''' <summary>
    ''' Creates a new exception from the given serialized form
    ''' </summary>
    ''' <param name="info">The object containing the info regarding the exception
    ''' from the serialized form.</param>
    ''' <param name="ctx">The streaming context around the serialization.</param>
    Protected Sub New(ByVal info As SerializationInfo, ByVal ctx As StreamingContext)
        MyBase.New(info, ctx)
    End Sub

End Class
