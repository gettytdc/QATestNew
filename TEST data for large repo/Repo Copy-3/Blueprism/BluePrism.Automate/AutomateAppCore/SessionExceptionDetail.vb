Imports System.Runtime.Serialization

<Serializable(), DataContract([Namespace]:="bp", Name:="sed")>
Public Class SessionExceptionDetail

    <DataMember(Name:="tr")>
    Private ReadOnly mTerminationReason As SessionTerminationReason
    <DataMember(EmitDefaultValue:=False, Name:="et")>
    Private ReadOnly mExceptionType As String
    <DataMember(EmitDefaultValue:=False, Name:="em")>
    Private ReadOnly mExceptionMessage As String

    Private Sub New(terminationReason As SessionTerminationReason, exceptionType As String, exceptionMessage As String)
        mTerminationReason = terminationReason
        mExceptionType = exceptionType
        mExceptionMessage = exceptionMessage
    End Sub

    Public Shared Function InternalError(exceptionMessage As String) As SessionExceptionDetail
        Return New SessionExceptionDetail(SessionTerminationReason.InternalError, String.Empty, exceptionMessage)
    End Function

    Public Shared Function ProcessError(exceptionType As String, exceptionMessage As String) As SessionExceptionDetail
        Return New SessionExceptionDetail(SessionTerminationReason.ProcessError, exceptionType, exceptionMessage)
    End Function

    Public ReadOnly Property TerminationReason As SessionTerminationReason
        Get
            Return mTerminationReason
        End Get
    End Property
    Public ReadOnly Property ExceptionType As String
        Get
            Return mExceptionType
        End Get
    End Property
    Public ReadOnly Property ExceptionMessage As String
        Get
            Return mExceptionMessage
        End Get
    End Property

End Class
