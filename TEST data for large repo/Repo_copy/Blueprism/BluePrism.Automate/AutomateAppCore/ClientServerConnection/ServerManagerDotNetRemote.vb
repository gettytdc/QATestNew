Imports System.Threading
Imports System.Net.Sockets
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib
Imports System.Runtime.Remoting.Channels
Imports System.Runtime.Remoting.Channels.Tcp
Imports System.Security.Principal
Imports System.Runtime.Remoting.Activation
Imports System.Runtime.Serialization.Formatters
Imports System.Reflection
Imports BluePrism.Core.Utility
Imports BluePrism.Server.Domain.Models

''' Project  : AutomateAppCore
''' Class    : ServerManagerDotNetRemote
''' 
''' <summary>
''' Provides a .Net remoting transport for client server clsServer communication
''' </summary>
Public NotInheritable Class ServerManagerDotNetRemote
    Inherits ServerManager

#Region "Class data"
    ''' <summary>
    ''' Set to an instance when we have registered our client-end .NET Remoting
    ''' channel
    ''' </summary>
    Private mChannel As TcpChannel = Nothing





#End Region

#Region "Implementation of abstract base class methods"

    ''' <summary>
    ''' Opens a .NET remoting connection to the server
    ''' </summary>
    ''' <param name="connectionSetting">The clsDBConnectionSetting to use.</param>
    Public Overrides Sub OpenConnection(connectionSetting As clsDBConnectionSetting, keys As Dictionary(Of String, clsEncryptionScheme), ByRef systemUser As IUser)

        ' Save the setting so we can recreate the connection.
        ConnectionDetails = connectionSetting

        Dim sv As IServer = Nothing

        Try
            'For a connection to a Blue Prism Server, we need to activate the
            'instance on the remote server via .NET Remoting.
            If mChannel Is Nothing Then
                mChannel = RegisterChannel(connectionSetting)
            End If

            Dim server = IPAddressHelper.EscapeForURL(connectionSetting.DBServer)

            Dim addr As String = "tcp://" & server & ":" & connectionSetting.Port.ToString()
            Dim url() As Object = {New UrlAttribute(addr)}
            sv = CType(Activator.CreateInstance(GetType(clsServer), Nothing, url), clsServer)
            LastConnectException = Nothing

            ' Only keep going if a connection was established
            If sv IsNot Nothing Then

                Dim serverVersion = sv.GetBluePrismVersion()
                Dim clientVersion As String = clsServer.GetBluePrismVersionS()
                If Not ApiVersion.AreCompatible(serverVersion, clientVersion) Then
                    Throw New IncompatibleApiException(clientVersion, serverVersion)
                End If

                sv.EnsureDatabaseConnection()

                mServer = sv

                If sv IsNot Nothing Then
                    StartDataMonitor()
                    Permission.Init(sv)
                End If

                ' Kick off the keep alive thread.
                StartKeepAlive()
            End If

        Catch ex As TargetInvocationException
            If sv IsNot Nothing Then CloseConnection()
            LastConnectException = ex.InnerException
            sv = Nothing
            mServer = Nothing
        Catch ex As Exception
            If sv IsNot Nothing Then CloseConnection()
            LastConnectException = ex
            sv = Nothing
            mServer = Nothing
        End Try


    End Sub

    ''' <summary>
    ''' Close the client connection to the server, this ensures the connection
    ''' is re-registered when OpenConnection() is next called.
    ''' </summary>
    Public Overrides Sub CloseConnection()
        If mChannel IsNot Nothing Then _
            ChannelServices.UnregisterChannel(mChannel)
        mChannel = Nothing 'Null indicates need to re-register
    End Sub

    ''' <summary>
    ''' Gets the connection to the server if it is available. The connection
    ''' will be returned immediately if available or it will retry 
    ''' connecting with an ulitmate timeout of 2 minutes.
    ''' </summary>
    Public Overrides ReadOnly Property Server As IServer
        Get
            Dim timer = Stopwatch.StartNew()
            Do
                Try

                    SyncLock ConnectLock
                        If CheckConnection() Then
                            Return mServer
                        Else
                            mServer = RetryConnection()
                            Return mServer
                        End If
                    End SyncLock

                Catch ex As UnavailableException
                    OnConnectionPending()
                    If Not HandlersRegistered Then
                        'Hang 5 and then try again
                        Thread.Sleep(5000)
                    End If
                End Try

            Loop Until timer.Elapsed > ServerReconnectTimeout

            Throw New UnavailableException(My.Resources.ServerManagerDotNetRemote_TheServerIsUnavailableAfterRetrying)
        End Get
    End Property

#End Region

#Region "Private helpers"


    ''' <summary>
    ''' Try to reconnect to the server
    ''' </summary>
    Private Function RetryConnection() As IServer
        ' Close down the connection before re-connecting
        CloseConnection()
        mServer = Nothing

        ' If we have no DB setting, there's nothing to try to connect with
        If ConnectionDetails Is Nothing Then Throw New MissingItemException(My.Resources.ServerManagerDotNetRemote_ThereIsNoDatabaseSetting)

        ' Try and open a connection with the current DB setting
        OpenConnection(ConnectionDetails, Nothing, Nothing)

        ' The server is unavialable
        If mServer Is Nothing Then Throw New UnavailableException(My.Resources.ServerManagerDotNetRemote_TheServerIsUnavailable)

        ' Relogin since we will have been logged out when the connection dropped
        User.ReLogin(mServer)
        Permission.Init(mServer)

        OnConnectionRestored()

        Return mServer
    End Function
#End Region

#Region "Keep Alive"

    ''' <summary>
    ''' Base class function to kick off the keep alive thread.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Friend Sub StartKeepAlive()
        If TestMode Then Return

        ' If the client is using a Blue Prism Server, we need a thread to keep the
        ' remote object alive even if the application is idle.
        If mKeepAlive Is Nothing Then
            mKeepAlive = New Threading.Timer(AddressOf KeepAliveThread, Nothing, 0, (90 * 1000))
        End If
    End Sub

    ''' <summary>
    ''' Thread used to keep the server connection alive.
    ''' </summary>
    Private mKeepAlive As Threading.Timer

    ''' <summary>
    ''' Thread that talks to the database periodically to ensure the remote object is
    ''' kept alive. This is relevant only for a Blue Prism Server connection.
    ''' We use the default lease setup, which has an initial lease time of 5 minutes and
    ''' renews by 2 minutes when a call is made. Thus, by renewing every 1.5 minutes, we
    ''' ensure that it never expires while the application is active.
    ''' </summary>
    Private Sub KeepAliveThread(state As Object)
        Try
            SyncLock ConnectLock
                If ServerAvailable() Then
                    mServer.Nop()
                End If
            End SyncLock
        Catch ex As ThreadAbortException
            'Abort when told to - this means the application is existing.
            Return
        Catch ex As Exception
            'For any other exception, just keep going.
        End Try
    End Sub

#End Region

#Region "Dot Net Remoting Functions"

    ''' <summary>
    ''' Registers the .NET remoting channel for communication to the BPserver
    ''' </summary>
    ''' <param name="connectionSetting">The clsDBConnectionSetting to use.</param>
    Private Function RegisterChannel(connectionSetting As clsDBConnectionSetting) As TcpChannel

        Dim props As New Hashtable()
        props("secure") = (connectionSetting.ConnectionMode = ServerConnection.Mode.DotNetRemotingSecure)
        If connectionSetting.ConnectionMode = ServerConnection.Mode.DotNetRemotingSecure Then
            props("tokenImpersonationLevel") = TokenImpersonationLevel.Identification
        End If
        props("port") = connectionSetting.CallbackPort

        Dim sprov As New BinaryServerFormatterSinkProvider()
        sprov.TypeFilterLevel = TypeFilterLevel.Full

        Try
            Dim channel As New TcpChannel(props, Nothing, sprov)
            ChannelServices.RegisterChannel(channel, (connectionSetting.ConnectionMode = ServerConnection.Mode.DotNetRemotingSecure))

            Return channel
        Catch se As SocketException When se.SocketErrorCode = SocketError.AddressAlreadyInUse
            Throw New BluePrismException(
             "The supplied callback port ({0}) for the BP Server connection '{1}' " &
             "is already in use elsewhere.",
             connectionSetting.CallbackPort, connectionSetting.ConnectionName)

        Catch ex As Exception

            ' If we're not setting a specific callback port, retry it without setting
            ' a callback port at all - none of our software *requires* it, though
            ' it is a nice to have.
            If CInt(props("port")) = 0 Then
                props.Remove("port")
                Dim channel As New TcpChannel(props, Nothing, sprov)
                ChannelServices.RegisterChannel(channel, (connectionSetting.ConnectionMode = ServerConnection.Mode.DotNetRemotingSecure))

                Return channel
            Else ' Otherwise, rethrow the exception
                Throw
            End If
        End Try
    End Function

#End Region

End Class

