''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsSession
''' 
''' <summary>
''' Encapsulates a live session - i.e. one that is running a process. This can be
''' either on a Resource PC, or in the debugger (Process Studio).
''' 
''' A session has an ID, and also a set of session variables. This class is
''' responsible for maintaining those variables and managing access to them.
''' 
''' The session variables are exposed as instances of clsProcessValue, referenced
''' by name. The name corresponds to the name of the Data Stage where the session
''' variable is defined. If that stage has a narrative, it will be present in the
''' Description property of the clsProcessValue.
''' </summary>
Public Class clsSession

    ''' <summary>
    ''' Event fired when a session variable changes.
    ''' </summary>
    ''' <param name="session">The session.</param>
    ''' <param name="name">The name of the variable.</param>
    ''' <param name="value">The new value.</param>
    Public Event VarChanged(ByVal session As clsSession, ByVal name As String, ByVal value As clsProcessValue)

    Private mIdentifier As SessionIdentifier

    ''' <summary>
    ''' The session variables defined within this session.
    ''' </summary>
    Private mVars As New Dictionary(Of String, clsProcessValue)

    ''' <summary>
    ''' Stores objects cached during this session.
    ''' </summary>
    Private ReadOnly mCachedObjects As New Dictionary(Of String, Object)

    Public ReadOnly Property WebConnectionSettings As WebConnectionSettings


    ''' <summary>
    ''' Constructor. Creates a new session.
    ''' </summary>
    ''' <param name="id">The ID of the new session.</param>
    ''' <param name="sessNo">The corresponding session number for this session.
    ''' <param name="webConnSettings">The webConnectionSettings retrieved from the database for this session</param>
    ''' </param>
    Public Sub New(ByVal id As Guid, sessNo As Integer, webConnSettings As WebConnectionSettings)
        Me.New(New RuntimeResourceSessionIdentifier(id, sessNo), webConnSettings)
    End Sub

    Public Sub New(sessionIdentifier As SessionIdentifier, webConnSettings As WebConnectionSettings)
        mIdentifier = sessionIdentifier
        WebConnectionSettings = webConnSettings
    End Sub

    Public ReadOnly Property ID() As Guid
        Get
            Return mIdentifier.Id
        End Get
    End Property

    Public ReadOnly Property Identifier As SessionIdentifier
        Get
            Return mIdentifier
        End Get
    End Property

    ''' <summary>
    ''' Get all variables defined in the session. This is a clone of the actual
    ''' information, owned by the caller. There is no point trying to modify it!
    ''' </summary>
    ''' <returns>A Dictionary(Of String, clsProcessValue).</returns>
    Public Function GetAllVars() As Dictionary(Of String, clsProcessValue)
        Dim clone As New Dictionary(Of String, clsProcessValue)
        SyncLock mVars
            For Each key As String In mVars.Keys
                clone(key) = mVars(key).Clone()
            Next
        End SyncLock
        Return clone
    End Function

    ''' <summary>
    ''' Get the value of a session variable.
    ''' </summary>
    ''' <param name="name">The name of the variable.</param>
    ''' <returns>The value, or Nothing if the variable is not defined.</returns>
    Public Function GetVar(ByVal name As String) As clsProcessValue
        SyncLock mVars
            If Not mVars.ContainsKey(name) Then Return Nothing
            Return mVars(name)
        End SyncLock
    End Function


    ''' <summary>
    ''' Set the value of a session variable. If it doesn't already exist, it is
    ''' created.
    ''' </summary>
    ''' <param name="name">The name of the variable.</param>
    ''' <param name="value">The value to set.</param>
    Public Sub SetVar(ByVal name As String, ByVal value As clsProcessValue)
        SyncLock mVars
            If mVars.ContainsKey(name) Then
                If mVars(name).DataType <> value.DataType Then
                    Throw New InvalidOperationException("Incompatible datatype for new value of session variable " & name)
                End If
            End If
            mVars(name) = value
            RaiseEvent VarChanged(Me, name, value.Clone())
        End SyncLock
    End Sub

    ''' <summary>
    ''' Clear (remove) all session variables.
    ''' </summary>
    Public Sub ClearVars()
        SyncLock mVars
            mVars.Clear()
        End SyncLock
    End Sub
    
    ''' <summary>
    ''' Adds a cached object to the session
    ''' </summary>
    ''' <param name="key">The key used to store the cached value</param>
    ''' <param name="value">The value to store</param>
    Public Sub CacheObject(key As String, value As Object)
        mCachedObjects(key) = value
    End Sub

    ''' <summary>
    ''' Get a value that has been cached within the session.
    ''' </summary>
    ''' <param name="key">The key used to find the cached value</param>
    ''' <returns>The cached value or Nothing if no value is found</returns>
    Public Function GetCachedObject(key As String) As Object
        Dim value As Object = Nothing
        mCachedObjects.TryGetValue(key, value)
        Return value
    End Function
    
End Class
