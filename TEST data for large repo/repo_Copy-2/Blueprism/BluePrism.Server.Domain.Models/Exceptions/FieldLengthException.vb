Imports System.Runtime.Serialization

''' <summary>
''' Exception thrown when a field's length restrictions are contravened, usually
''' because a value is set which is too long for the given field.
''' </summary>
<Serializable()> _
Public Class FieldLengthException : Inherits BluePrismException
    ''' <summary>
    ''' Creates a new exception with no message
    ''' </summary>
    Public Sub New()
        MyBase.New()
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
