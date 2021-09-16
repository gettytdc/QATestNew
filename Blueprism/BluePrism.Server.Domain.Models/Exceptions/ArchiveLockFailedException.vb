Imports System.Runtime.Serialization

''' <summary>
''' Exception raised if there was a problem acquiring a lock for an archive operation
''' </summary>
<Serializable()> _
Public Class ArchiveLockFailedException : Inherits LockFailedException

    ''' <summary>
    ''' Deserializes an exception indicating that a lock was requested for archiving
    ''' when there is a machine which currently has the archive lock.
    ''' </summary>
    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
        MyBase.New(info, context)
    End Sub

    ''' <summary>
    ''' Creates a new exception with the given formatted message.
    ''' </summary>
    ''' <param name="formatMessage">The message with optional format markers.</param>
    ''' <param name="args">The formatting arguments for the message.</param>
    Public Sub New(ByVal formatMessage As String, ByVal ParamArray args() As Object)
        MyBase.New(formatMessage, args)
    End Sub

End Class
