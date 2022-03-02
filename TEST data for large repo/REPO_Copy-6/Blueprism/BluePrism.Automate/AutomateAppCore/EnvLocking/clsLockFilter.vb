Imports System.Runtime.Serialization
Imports BluePrism.BPCoreLib
Imports LockState = BluePrism.AutomateAppCore.clsLockInfo.LockState

<Serializable, DataContract([Namespace]:="bp")>
Public Class clsLockFilter

    Public Class FilterNames
        Public Const Status As String = "Status"
        Public Const Name As String = "Name"
        Public Const Resource As String = "Resource"
        Public Const Process As String = "Process"
        Public Const LockTime As String = "Lock Time"
        Public Const LastComment As String = "Last Comment"
    End Class

    <DataMember>
    Private mState As clsLockInfo.LockState = clsLockInfo.LockState.None
    <DataMember>
    Private mName As String
    <DataMember>
    Private mResource As String
    <DataMember>
    Private mUser As String
    <DataMember>
    Private mProcess As String
    <DataMember>
    Private mLockTime As clsDateRange
    <DataMember>
    Private mComment As String
    <DataMember>
    Private mRowsPerPage As Integer
    <DataMember>
    Private mCurrentPage As Integer

    Public Sub New(ByVal map As IDictionary(Of String, Object), rowPerPage As Integer, currentPage As Integer)
        ExtractValue(FilterNames.Status, map, mState)
        ExtractValue(FilterNames.Name, map, mName)
        ExtractValue(FilterNames.Resource, map, mResource)
        ExtractValue(FilterNames.Process, map, mProcess)
        ExtractValue(FilterNames.LockTime, map, mLockTime)
        ExtractValue(FilterNames.LastComment, map, mComment)

        mRowsPerPage = rowPerPage
        mCurrentPage = currentPage
    End Sub

    Private Sub ExtractValue(Of T)( _
     ByVal name As String, ByVal map As IDictionary(Of String, Object), ByRef into As T)
        Try
            If map.ContainsKey(name) Then into = DirectCast(map(name), T)
        Catch
        End Try
    End Sub

    Public Sub AppendFilterClause(ByVal sb As StringBuilder, ByVal prefix As String)

        sb.Append(" "c)
        Dim init As Integer = sb.Length

        If mState <> clsLockInfo.LockState.None Then
            sb.AppendFormat("{0} {1}.token is {2} null", _
             "", prefix, IIf(mState = LockState.Held, "not", ""))
        End If
        If mName <> "" Then
            sb.AppendFormat("{0} {1}.name like @name", IIf(sb.Length <> init, "and", ""), prefix)
        End If
        If mResource <> "" Then
            sb.AppendFormat("{0} {1}.runningresourcename like @resource", _
             IIf(sb.Length <> init, "and", ""), prefix)
        End If
        If mProcess <> "" Then
            sb.AppendFormat("{0} {1}.processname like @process", _
             IIf(sb.Length <> init, "and", ""), prefix)
        End If
        If mLockTime IsNot Nothing Then
            sb.AppendFormat("{0} {1}.locktime between @starttime and @endtime", _
             IIf(sb.Length <> init, "and", ""), prefix)
        End If
        If mComment <> "" Then
            sb.AppendFormat("{0} {1}.comments like @comments", _
             IIf(sb.Length <> init, "and", ""), prefix)
        End If

    End Sub

    Public ReadOnly Property State() As clsLockInfo.LockState
        Get
            Return mState
        End Get
    End Property

    Public ReadOnly Property Name() As String
        Get
            Return mName
        End Get
    End Property

    Public ReadOnly Property Resource() As String
        Get
            Return mResource
        End Get
    End Property

    Public ReadOnly Property Process() As String
        Get
            Return mProcess
        End Get
    End Property

    Public ReadOnly Property LockTime() As clsDateRange
        Get
            Return mLockTime
        End Get
    End Property

    Public ReadOnly Property LastComment() As String
        Get
            Return mComment
        End Get
    End Property

    Public ReadOnly Property RowsPerPage() As Integer
    Get
            Return mRowsPerPage
    End Get
    End Property

    Public ReadOnly Property CurrentPage() As Integer
        Get
            Return mCurrentPage
        End Get
    End Property

End Class
