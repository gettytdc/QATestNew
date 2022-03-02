Imports System.Runtime.Serialization
Imports BluePrism.Server.Domain.Models

''' <summary>
''' Exception indicating that no java elements were found that matched a given query.
''' </summary>
<Serializable>
Public Class NoSuchJavaElementException : Inherits NoSuchElementException

    ''' <summary>
    ''' Creates a new exception with a default message
    ''' </summary>
    Public Sub New()
        MyBase.New(My.Resources.ThereAreNoJavaElementsMatchingTheQuery)
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
