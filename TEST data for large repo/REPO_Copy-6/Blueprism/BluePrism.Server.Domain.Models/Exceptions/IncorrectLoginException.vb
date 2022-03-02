Imports System.Runtime.Serialization

''' <summary>
''' Exception thrown when a login used an incorrect username or password
''' </summary>
<Serializable()> _
Public Class IncorrectLoginException : Inherits BluePrismException

    ''' <summary>
    ''' The default message used for an exception created with no specified message
    ''' </summary>
    Public Const DefaultMessage As String = _
        "The username or password supplied were not valid. Please try again"

    ''' <summary>
    ''' Creates a new exception with the default message
    ''' </summary>
    Public Sub New()
        MyBase.New(My.Resources.IncorrectLoginException_TheUsernameOrPasswordIsIncorrectPleaseTryAgain)
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
    ''' <param name="formattedMsg">The message, with format placeholders, as
    ''' defined in the <see cref="String.Format"/> method.</param>
    ''' <param name="args">The arguments for the formatted message.</param>
    Public Sub New(ByVal formattedMsg As String, ByVal ParamArray args() As Object)
        MyBase.New(formattedMsg, args)
    End Sub

    ''' <summary>
    ''' Creates a new exception from the given de-serializer
    ''' </summary>
    ''' <param name="info">The serialization info from which this exception
    ''' should draw its data.</param>
    ''' <param name="ctx">The context defining the context for the current
    ''' deserialization stream.</param>
    Public Sub New(ByVal info As SerializationInfo, ByVal ctx As StreamingContext)
        MyBase.New(info, ctx)
    End Sub

End Class
