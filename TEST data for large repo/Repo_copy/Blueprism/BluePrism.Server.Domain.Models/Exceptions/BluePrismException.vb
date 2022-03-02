Imports System.Runtime.Serialization
Imports System.Security.Permissions

''' <summary>
''' Base class for exceptions.
''' This is more an aide-memoire so that I can remember what needs to be done
''' for exceptions, since there's more to it than just extending Exception.
''' It also handles the base functionality so that all subclasses need to do
''' is caller the appropriate super constructor rather than reinventing the
''' wheel each time (primarily for formatted messages - Exception handles most
''' of the other functionality).
''' </summary>
<Serializable()> _
Public Class BluePrismException : Inherits ApplicationException

#Region " Class-scope Declarations "

    ''' <summary>
    ''' Formats the message, replacing any placehoders with the given arguments.
    ''' If the message is invalid, eg. it has placeholders for arguments which have
    ''' not been supplied, this will just use the raw message with any placeholders
    ''' left within the string.
    ''' </summary>
    ''' <param name="msg">The message, with formatting placeholders to resolve.
    ''' </param>
    ''' <param name="args">The arguments to use in the placeholders in the message.
    ''' </param>
    ''' <returns>The formatted message, or <paramref name="msg"/> if a formatting
    ''' error occurred while trying to format the message</returns>
    Private Shared Function FormatMessage(msg As String, args() As Object) As String
        Try
            Return String.Format(msg, args)
        Catch ex As FormatException
            Return msg
        End Try
    End Function

#End Region

#Region " Non-Serialization Constructors "

    ''' <summary>
    ''' Creates a new Blue Prism Exception with no message.
    ''' </summary>
    Public Sub New()
        MyBase.New()
    End Sub

    ''' <summary>
    ''' Creates a new Blue Prism Exception with the given message.
    ''' </summary>
    ''' <param name="msg">The message detailing the exception.</param>
    Public Sub New(ByVal msg As String)
        MyBase.New(msg)
    End Sub

    ''' <summary>
    ''' Creates a new BluePrism Exception with the given message and cause.
    ''' </summary>
    ''' <param name="cause">The root cause of this exception.</param>
    ''' <param name="msg">The message detailing the exception.</param>
    Public Sub New(ByVal cause As Exception, ByVal msg As String)
        MyBase.New(msg, cause)
    End Sub

    ''' <summary>
    ''' Creates a new Blue Prism Exception with the given formatted message.
    ''' </summary>
    ''' <param name="formattedMsg">The message, with format placeholders, as
    ''' defined in the <see cref="String.Format"/> method.</param>
    ''' <param name="args">The arguments for the formatted message.</param>
    Public Sub New(ByVal formattedMsg As String, ByVal ParamArray args() As Object)
        MyBase.New(FormatMessage(formattedMsg, args))
    End Sub

    ''' <summary>
    ''' Creates a new Blue Prism Exception with the given formatted message
    ''' and root cause.
    ''' </summary>
    ''' <param name="cause">The root cause of this exception.</param>
    ''' <param name="formattedMsg">The message, with format placeholders, as
    ''' defined in the <see cref="String.Format"/> method.</param>
    ''' <param name="args">The arguments for the formatted message.</param>
    Public Sub New(ByVal cause As Exception, _
     ByVal formattedMsg As String, ByVal ParamArray args() As Object)
        MyBase.New(FormatMessage(formattedMsg, args), cause)
    End Sub

#End Region

#Region " Serialization Handling "

    ''' <summary>
    ''' Creates a new blue prism exception from the given de-serializer
    ''' </summary>
    ''' <param name="info">The serialization info from which this exception
    ''' should draw its data.</param>
    ''' <param name="ctx">The context defining the context for the current
    ''' deserialization stream.</param>
    Protected Sub New(ByVal info As SerializationInfo, ByVal ctx As StreamingContext)
        MyBase.New(info, ctx)
    End Sub

    ''' <summary>
    ''' Gets the object data for this exception into the given serialization
    ''' info object.
    ''' </summary>
    ''' <param name="info">The info into which this object's data should be
    ''' stored.</param>
    ''' <param name="ctx">The context for the streaming</param>
    <SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter:=True)> _
    Public Overrides Sub GetObjectData( _
     ByVal info As SerializationInfo, ByVal ctx As StreamingContext)
        MyBase.GetObjectData(info, ctx)
    End Sub

#End Region

End Class
