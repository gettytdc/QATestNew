
Imports BluePrism.BPCoreLib.Data
Imports System.Runtime.Serialization

<Serializable()>
<DataContract([Namespace]:="bp")>
Public Class clsLockInfo

    Public Enum LockState
        None
        Held
        Free
    End Enum

    <DataMember>
    Private mName As String

    <DataMember>
    Private mToken As String

    <DataMember>
    Private mSessionId As Guid

    <DataMember>
    Private mResourceId As Guid

    <DataMember>
    Private mResourceName As String

    <DataMember>
    Private mUserName As String

    <DataMember>
    Private mProcessId As Guid

    <DataMember>
    Private mProcessName As String

    <DataMember>
    Private mLockTime As Date

    <DataMember>
    Private mComment As String

    Public Sub New(ByVal prov As IDataProvider)
        mName = prov.GetString("name")
        mToken = prov.GetString("token")
        mSessionId = prov.GetValue("sessionid", Guid.Empty)
        mResourceId = prov.GetValue("resourceid", Guid.Empty)
        mResourceName = prov.GetString("resourcename")
        mUserName = prov.GetString("username")
        mProcessId = prov.GetValue("processid", Guid.Empty)
        mProcessName = prov.GetString("processname")
        mLockTime = prov.GetValue("locktime", Date.MinValue)
        ' lock time is held on the database in UTC - ensure that this
        ' object reflects that.
        If mLockTime.Kind <> DateTimeKind.Utc Then _
         mLockTime = Date.SpecifyKind(mLockTime, DateTimeKind.Utc)

        mComment = prov.GetString("comment")
    End Sub

    Public Overrides Function Equals(ByVal o As Object) As Boolean
        Dim li As clsLockInfo = TryCast(o, clsLockInfo)
        If li Is Nothing Then Return False

        Return li.mName = Me.mName _
         AndAlso li.mToken = Me.mToken _
         AndAlso li.mResourceId = Me.mResourceId _
         AndAlso li.mResourceName = Me.mResourceName _
         AndAlso li.mUserName = Me.mUserName _
         AndAlso li.mProcessId = Me.mProcessId _
         AndAlso li.mProcessName = Me.mProcessName _
         AndAlso li.mLockTime = Me.mLockTime _
         AndAlso li.mComment = Me.mComment

    End Function

    Private Shared Function Hash(ByVal str As String) As Integer
        If str Is Nothing Then Return 0
        Return str.GetHashCode()
    End Function

    Public Overrides Function GetHashCode() As Integer
        Return Hash(mName) Xor Hash(mToken) Xor Hash(mResourceName) Xor Hash(mUserName) _
         Xor Hash(mProcessName) Xor mLockTime.GetHashCode() Xor Hash(mComment)
    End Function

    Public ReadOnly Property State() As LockState
        Get
            If mToken = "" Then Return LockState.Free
            Return LockState.Held
        End Get
    End Property

    Public ReadOnly Property Name() As String
        Get
            Return mName
        End Get
    End Property

    Public ReadOnly Property Token() As String
        Get
            Return mToken
        End Get
    End Property

    Public ReadOnly Property SessionId() As Guid
        Get
            Return mSessionId
        End Get
    End Property

    Public ReadOnly Property ResourceId() As Guid
        Get
            Return mResourceId
        End Get
    End Property

    Public ReadOnly Property ResourceName() As String
        Get
            If mResourceName Is Nothing Then Return ""
            Return mResourceName
        End Get
    End Property

    Public ReadOnly Property UserName() As String
        Get
            If mUserName Is Nothing Then Return ""
            Return mUserName
        End Get
    End Property

    Public ReadOnly Property ProcessId() As Guid
        Get
            Return mProcessId
        End Get
    End Property

    Public ReadOnly Property ProcessName() As String
        Get
            If mProcessName Is Nothing Then Return ""
            Return mProcessName
        End Get
    End Property

    Public ReadOnly Property LockTime() As Date
        Get
            Return mLockTime
        End Get
    End Property

    Public ReadOnly Property LockTimeDisplay() As String
        Get
            If mLockTime = Nothing Then Return ""
            Return mLockTime.ToLocalTime().ToString()
        End Get
    End Property


    Public ReadOnly Property Comment() As String
        Get
            If mComment Is Nothing Then Return ""
            Return mComment
        End Get
    End Property

    <DataMember>
    Public Property CanViewSessionLog() As Boolean

End Class

