Imports BluePrism.Server.Domain.Models
Imports System.Runtime.Serialization
Imports BluePrism.ApplicationManager.CommandHandlers.UIAutomation
Imports BluePrism.UIAutomation.Patterns

''' <summary>
''' Exception thrown when no valid UIAutomation patterns were on an automation
''' element
''' </summary>
<Serializable>
Public Class MissingPatternException : Inherits MissingItemException

    ''' <summary>
    ''' The format of the message for this exception
    ''' </summary>
    Private Shared ReadOnly DefaultMessageFormat As String =
        UIAutomationErrorResources.MissingPatternException_DefaultMessageTemplate

    ''' <summary>
    ''' Creates a new exception with no message
    ''' </summary>
    Public Sub New(ParamArray types() As PatternType)
        MyBase.New(String.Format(DefaultMessageFormat, String.Join(", ", types)))
    End Sub

    ''' <summary>
    ''' Creates a new exception with no message
    ''' </summary>
    Public Sub New(cause As Exception, ParamArray types() As PatternType)
        MyBase.New(cause, String.Format(DefaultMessageFormat, String.Join(", ", types)))
    End Sub

    ''' <summary>
    ''' Creates a new exception with a custom message
    ''' </summary>
    ''' <param name="messageFormat">Message format string used to format the message. The
    ''' pattern type will be inserted into the &quot;{0}&quot; placeholder.</param>
    Public Sub New(messageFormat As String, ParamArray types() As PatternType)
        MyBase.New(String.Format(messageFormat, String.Join(", ", types)))
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
