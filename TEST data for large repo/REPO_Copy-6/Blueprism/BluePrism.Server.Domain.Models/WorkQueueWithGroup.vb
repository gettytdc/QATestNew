Imports System.Runtime.Serialization

<Serializable()>
<DataContract([Namespace]:="bp")>
Public Class WorkQueueWithGroup
    <IgnoreDataMember>
    Public Property Id As Guid
    <IgnoreDataMember>
    Public Property Ident As Integer
    <IgnoreDataMember>
    Public Property Name As String
    <IgnoreDataMember>
    Public Property IsRunning As Boolean
    <IgnoreDataMember>
    Public Property KeyField As String
    <IgnoreDataMember>
    Public Property MaxAttempts As Integer
    <IgnoreDataMember>
    Public ReadOnly Property IsEncrypted As Boolean
        Get
            Return EncryptionKeyId > 0
        End Get
    End Property
    <IgnoreDataMember>
    Public Property EncryptionKeyId As Integer
    <IgnoreDataMember>
    Public Property PendingItemCount As Integer
    <IgnoreDataMember>
    Public Property CompletedItemCount As Integer
    <IgnoreDataMember>
    Public Property ExceptionedItemCount As Integer
    <IgnoreDataMember>
    Public Property LockedItemCount As Integer
    <IgnoreDataMember>
    Public Property TotalItemCount As Integer
    <IgnoreDataMember>
    Public Property AverageWorkTime As TimeSpan
    <IgnoreDataMember>
    Public Property TotalCaseDuration As TimeSpan
    <IgnoreDataMember>
    Public Property ProcessId As Guid
    <IgnoreDataMember>
    Public Property ResourceGroupId As Guid
    <IgnoreDataMember>
    Public Property TargetSessionCount As Integer
    <IgnoreDataMember>
    Public Property GroupName As String
    <IgnoreDataMember>
    Public Property GroupId As Guid
End Class
