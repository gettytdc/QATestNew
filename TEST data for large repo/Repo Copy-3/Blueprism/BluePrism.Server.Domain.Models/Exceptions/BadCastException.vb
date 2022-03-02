Imports System.Runtime.Serialization

''' <summary>
''' Exception indicating that an attempt to cast an object invalidly was made.
''' </summary>
''' <remarks>Note that, unlike most exceptions in CoreLib, this does not extend
''' <see cref="BluePrismException"/> - it's really just a bit of a clunky mechanism
''' to add (String, Object...) param handling to an
''' <see cref="InvalidCastException"/></remarks>
<Serializable()> _
Public Class BadCastException : Inherits InvalidCastException

    ''' <summary>
    ''' Creates a new exception with no message
    ''' </summary>
    Public Sub New()
        MyBase.New()
    End Sub

    ''' <summary>
    ''' Creates a new BadCastException with a specified error message
    ''' </summary>
    ''' <param name="msg">The error message that explains the reason for the
    ''' exception</param>
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
        MyBase.New(String.Format(formattedMsg, args))
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
