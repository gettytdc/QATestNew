Imports System.Runtime.Serialization

''' <summary>
''' Exception raised if there was a problem acquiring a lock due to a lock already
''' being in place on the item to be locked.
''' </summary>
<Serializable()> _
Public Class AlreadyLockedException : Inherits LockFailedException

    ''' <summary>
    ''' Creates a new exception with the given formatted message.
    ''' </summary>
    ''' <param name="formatMessage">The message with optional format markers.</param>
    ''' <param name="args">The formatting arguments for the message.</param>
    Public Sub New(ByVal formatMessage As String, ByVal ParamArray args() As Object)
        MyBase.New(formatMessage, args)
    End Sub

    ''' <summary>
    ''' Deserializes an exception
    ''' </summary>
    Protected Sub New(
                      ByVal info As SerializationInfo, ByVal context As StreamingContext)
        MyBase.New(info, context)
    End Sub

End Class
