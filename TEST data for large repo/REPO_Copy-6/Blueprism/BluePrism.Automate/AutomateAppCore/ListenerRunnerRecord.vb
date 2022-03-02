Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateProcessCore
Imports BluePrism.Core.Plugins

''' <summary>
''' Records details of a 'running' process. It is used to
''' pass information between the listener thread and to keep track of what is running.
''' </summary>
Public Class ListenerRunnerRecord
    Inherits RunnerRecord

    ''' <summary>
    ''' The Session ID that this runner record was created for
    ''' </summary>
    Public ReadOnly Property SessionID As Guid

    ''' <summary>
    ''' The identity of the session that this runner record was created for
    ''' </summary>
    Public ReadOnly Property SessionNo As Integer

    ' The ID of the resource we're running on.
    Private ReadOnly mResourceID As Guid

    ''' <summary>
    ''' ID of the user responsible for the session.
    ''' </summary>
    Private mUserID As Guid

    Public Sub New(
        ByVal procXml As String, ByVal procid As Guid,
        ByVal sessId As Guid, ByVal sessNo As Integer,
        ByVal resID As Guid, ByVal userId As Guid,
        ByVal notifier As ISessionNotifier)

        MyBase.New(procXml,
                   procid,
                   notifier,
                   New SessionInformation(sessId.ToString()),
                   New ServerSessionStatusPersister(sessId))
        mResourceID = resID
        mUserID = userId
        SessionID = sessId
        SessionNo = sessNo
        Debug.Assert(SessionNo > 0, "Missing session number in runner record")
    End Sub

    Public Overrides ReadOnly Property Log() As clsLoggingEngine
        Get
            If mLog Is Nothing Then
                Dim uname As String = String.Format(
                    "{0} (OS username: {1})",
                    User.CurrentName, Environment.UserName)

                Dim ctx As New LogContext(
                    uname, mResourceID, mProcess, False, SessionID, SessionNo)

                EventManager.GetInstance().AddHandler(
                    "Splunk Sender",
                    "Splunk",
                    New Dictionary(Of String, Object)()
                )

                mLog = New CompoundLoggingEngine() From {
                    New clsDBLoggingEngine(ctx),
                    New PluginLoggingEngine(ctx),
                    New clsStageUpdateMonitor(ctx)
                }

            End If
            Return mLog
        End Get
    End Property

    Protected Overrides Sub CreateProcessAlert(type As AlertEventType)
        gSv.CreateProcessAlert(type, SessionID, mResourceID)
    End Sub

    Protected Overrides ReadOnly Property SessionIdentifier As SessionIdentifier
        Get
            Return New RuntimeResourceSessionIdentifier(SessionID, SessionNo)
        End Get
    End Property

    Protected Overrides Sub AddSessionVariableChangeEventHandler()
        'Add event handler to the session, for picking up changes to
        'session variables.
        AddHandler Session.VarChanged, AddressOf VarChanged
    End Sub

    ''' <summary>
    ''' Event Handler for a session variable changing event.
    ''' </summary>
    ''' <param name="session">The session whose var has changed</param>
    ''' <param name="name">The name of the changed session var</param>
    ''' <param name="value">The new value of the session var</param>
    Private Sub VarChanged(ByVal session As clsSession,
        ByVal name As String, ByVal value As clsProcessValue)
        Dim var As New clsSessionVariable()
        var.sessionID = SessionID
        var.Name = name
        var.Value = value
        Notifier.VarChanged(var)
    End Sub

    ''' <summary>
    ''' Gets a status text value for this runner record, incorporating its
    ''' current <see cref="Status">Status</see>, the
    ''' <see cref="SessionID">session ID</see> and, if appropriate, the
    ''' <see cref="mFailReason">reason it failed</see>.
    ''' </summary>
    Public ReadOnly Property StatusText() As String
        Get
            Dim fmt As String
            If IsFailed Then fmt = "{0} {1} - {2}" Else fmt = "{0} {1}"
            Return String.Format(fmt, Status, SessionID, mFailReason)
        End Get
    End Property

    ''' <summary>
    ''' Set the user responsible for this session. Does nothing if there's no
    ''' change - otherwise the session record in the database is updated
    ''' accordingly.
    ''' </summary>
    ''' <param name="userId">The user ID.</param>
    Public Sub SetUser(ByVal userId As Guid)
        If userId = mUserID Then Return
        mUserID = userId
        gSv.SetSessionUserID(SessionID, userId)
    End Sub

End Class
