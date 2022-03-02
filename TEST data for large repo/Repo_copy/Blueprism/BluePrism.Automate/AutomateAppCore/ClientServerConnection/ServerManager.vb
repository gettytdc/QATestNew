Imports BluePrism.AutomateAppCore.Auth
Imports System.Threading
Imports BluePrism.BPCoreLib
Imports System.Runtime.Remoting
Imports System.IO
Imports System.Net.Sockets
Imports System.ServiceModel
Imports BluePrism.AutomateAppCore.DataMonitor
Imports BluePrism.Server.Domain.Models

Public MustInherit Class ServerManager : Implements IDisposable

#Region "Constants"

    ''' <summary>
    ''' The poll period (in seconds) for the data monitor
    ''' </summary>
    Private Const DataMonitorPollSeconds As Integer = 60

    ''' <summary>
    ''' The default number of seconds between each ConnectionCheck call
    ''' </summary>
    Private Const DefaultConnectionCheckRetrySeconds As Integer = 5

#End Region

#Region "Properties"


    ''' <summary>
    ''' The time when we last verified that our 'current instance' of clsServer was
    ''' actually working.
    ''' 
    ''' We track this internally, and occasionally verify the connection, so that
    ''' it's possible for a broken connection to be recovered without having to place
    ''' that kind of logic throughout the application.
    ''' </summary>
    Protected Property LastVerified As Date = Date.MinValue

    ''' <summary>
    ''' The timeout for server reconnects
    ''' </summary>
    Protected ReadOnly Property ServerReconnectTimeout As TimeSpan
        Get
            Return TimeSpan.FromMinutes(2)
        End Get
    End Property

    ''' <summary>
    ''' Indicates whether this ServerManager instance is being used to test the connection setup. 
    ''' Different timeouts and retry behaviour may apply when testing a connection.
    ''' </summary>
    Public Property TestMode As Boolean

    ''' <summary>
    ''' Boolean to indicate that a connection to the server is pending.
    ''' </summary>
    Private mServerPending As Boolean = False

    ''' <summary>
    ''' The number of seconds between each ConnectionCheck call
    ''' </summary>
    Protected mConnectionCheckRetrySeconds As Integer = DefaultConnectionCheckRetrySeconds

    ''' <summary>
    ''' The monitor to check and get the mConnectionCheckRetrySeconds from the server (if it is updated)
    ''' </summary>
    Private mMonitor As IDataMonitor

    ''' <summary>
    ''' Lock object for connection/reconnection to the server.
    ''' </summary>
    Protected ConnectLock As New Object()

    ''' <summary>
    ''' Our current clsServer instance, or Nothing if we don't have one.
    ''' </summary>
    Protected Friend mServer As IServer

    ''' <summary>
    ''' Boolean to indicate that handlers are registered. This ensures if no handlers
    ''' are registered a sensible sleep in the CurrentInstance property is used
    ''' </summary>
    Friend Property HandlersRegistered As Boolean = False

    ''' <summary>
    ''' The database connection setting currently in use - stored so we can reconnect
    ''' if necessary.
    ''' </summary>
    Protected Friend Property ConnectionDetails As clsDBConnectionSetting = Nothing

    ''' <summary>
    ''' Keeps track of the last reason that OpenConnection failed with.
    ''' if necessary.
    ''' </summary>
    Public ReadOnly Property LastConnectFailReason() As String
        Get
            If mLastConnectException Is Nothing Then Return Nothing
            Return mLastConnectException.Message
        End Get
    End Property

    ''' <summary>
    ''' The exception that was thrown last when OpenConnection() failed
    ''' </summary>
    Public Property LastConnectException() As Exception
        Get
            Return mLastConnectException
        End Get
        Protected Set(value As Exception)
            mLastConnectException = value
        End Set
    End Property
    Private mLastConnectException As Exception = Nothing

#End Region

#Region "abstract/overridable methods"
    ''' <summary>
    ''' This method creates the transport connection between client and server. As this is implementation specific, it is abstract.
    ''' </summary>
    Public MustOverride Sub OpenConnection(cons As clsDBConnectionSetting, keys As Dictionary(Of String, clsEncryptionScheme), ByRef systemUser As IUser)

    ''' <summary>
    ''' This method closes the transport connection between client and server. As this is implementation specific, it is abstract.
    ''' </summary>
    Public MustOverride Sub CloseConnection()

    ''' <summary>
    ''' Returns true if the Server is pending, I.e. waiting for reconnection.
    ''' We don't really want this function to block but on the other hand if we can
    ''' get a lock we would rather check the connection is actually working.
    ''' </summary>
    Protected Friend ReadOnly Property ServerPending As Boolean
        Get
            If Monitor.TryEnter(ConnectLock) Then
                Try
                    ' We can only rely on checkconnection when it
                    ' returns false as it might just skip the check.
                    If Not CheckConnection() Then
                        mServerPending = True
                    End If
                Finally
                    Monitor.Exit(ConnectLock)
                End Try
            End If
            Return mServerPending
        End Get
    End Property

    ''' <summary>
    ''' Check that the connection is available, but only if the connection has not
    ''' been verified for 5 seconds (to reduce server load)
    ''' </summary>
    Protected Function CheckConnection() As Boolean
        Try
            If Not ServerAvailable() Then Return False

            Dim current = Date.Now
            If mConnectionCheckRetrySeconds > 0 AndAlso current - LastVerified > TimeSpan.FromSeconds(mConnectionCheckRetrySeconds) Then
                mServer.Nop()
                LastVerified = current
            End If
            Return True

        Catch ex As Exception When TypeOf ex Is RemotingException _
         OrElse TypeOf ex Is SocketException _
         OrElse TypeOf ex Is IOException _
        OrElse TypeOf ex Is CommunicationObjectFaultedException
            Return False
        End Try
    End Function

    Protected Sub UpdateConnectionCheckRetry()
        If mServer IsNot Nothing Then _
            mConnectionCheckRetrySeconds = mServer.GetConnectionCheckRetrySecondsPref(DefaultConnectionCheckRetrySeconds)
    End Sub

    ''' <summary>
    ''' Handle server going into the pending connection state.
    ''' </summary>
    Protected Overridable Sub OnConnectionPending()
        mServerPending = True
        RaiseEvent ConnectionPending()
    End Sub

    ''' <summary>
    ''' Handles server going into the connection restored state.
    ''' </summary>
    Protected Overridable Sub OnConnectionRestored()
        mServerPending = False
        RaiseEvent ConnectionRestored()
    End Sub

    Protected Sub StartDataMonitor()
        If TestMode Then Return

        If mMonitor Is Nothing Then
            mMonitor = New TimerDataMonitor(New DatabaseMonitoredDataStore()) With {
                .Interval = TimeSpan.FromSeconds(DataMonitorPollSeconds),
                .Enabled = True
            }
            AddHandler mMonitor.MonitoredDataUpdated, AddressOf MonitoredDataUpdated
        End If
    End Sub

    ''' <summary>
    ''' The current server connection instance - i.e. an IServer
    ''' </summary>
    Public Overridable ReadOnly Property Server As IServer
        Get
            Return mServer
        End Get
    End Property

#End Region

#Region "Public Methods"



    ''' <summary>
    ''' Returns true if the connection is available.
    ''' </summary>
    Friend ReadOnly Property ServerAvailable() As Boolean
        Get
            Return mServer IsNot Nothing
        End Get
    End Property

    ''' <summary>
    ''' Checks if the server is available actively seeking a reconnection via the
    ''' <see cref="Server"/> property if it is not immediately available.
    ''' </summary>
    ''' <returns>True if a connection to the server is available through this server
    ''' manager; False if no connection was available and a reconnection attempt
    ''' failed.</returns>
    Friend Function CheckServerAvailability() As Boolean
        Try
            Return (Server IsNot Nothing)
        Catch ue As UnavailableException
            Return False
        End Try

    End Function


#End Region

#Region "Event handling code"

    Protected Overridable Sub MonitoredDataUpdated(sender As Object, e As MonitoredDataUpdateEventArgs)
        Select Case e.Name
            Case DataNames.Licensing
                If User.LoggedIn Then _
                    clsLicenseQueries.RefreshLicense()
        End Select
    End Sub

    ''' <summary>
    ''' Connection pending event. This event should be registered using
    ''' RegisterHandlers()
    ''' </summary>
    Friend Event ConnectionPending As Action

    ''' <summary>
    ''' Connection restored event. This event should be registered using
    ''' RegisterHandlers()
    ''' </summary>
    Friend Event ConnectionRestored As Action


    ''' <summary>
    ''' Registers handlers for ConnectionPending and ConnectionRestored events.
    ''' The ConnectionPending handler will be called in a loop so must allow the
    ''' user interface to update using Application.DoEvents. The method ensures
    ''' that handlers can only be registered once.
    ''' </summary>
    ''' <param name="pending">The handler for the ConnectionPending event</param>
    ''' <param name="restored">The handler for the ConnectionRestored event</param>
    Public Sub RegisterHandlers(pending As Action, restored As Action)
        If Not HandlersRegistered Then
            AddHandler ConnectionPending, pending
            AddHandler ConnectionRestored, restored
            HandlersRegistered = True
        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        If Me.mMonitor IsNot Nothing Then Me.mMonitor.Dispose()
        Me.mMonitor = Nothing
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overrides Sub Finalize()
        Me.Dispose()
        MyBase.Finalize()
    End Sub

#End Region

End Class
