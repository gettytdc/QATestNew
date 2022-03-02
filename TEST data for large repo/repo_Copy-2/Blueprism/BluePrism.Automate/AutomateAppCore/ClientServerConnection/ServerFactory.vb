Imports BluePrism.BPCoreLib
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Server.Domain.Models

''' <summary>
''' This factory allows the use of different transports between the client and server
''' for the IServer remote interface.
''' </summary>
Public Class ServerFactory

    ''' <summary>
    ''' Holds the current server manager.
    ''' </summary>
    Private Shared mServerManager As ServerManager = Nothing

    ''' <summary>
    ''' Initialises the server manager connection for clients
    ''' </summary>
    ''' <param name="conn">The connection setting to use</param>
    Public Shared Function ClientInit(conn As clsDBConnectionSetting) As ServerManager
        Dim unused As Exception

        Return ClientInit(conn, unused)
    End Function

    Public Shared Function ClientInit(conn As clsDBConnectionSetting, ByRef exception As Exception) As ServerManager
        ' Try and close an existing connection
        Close()

        ' Open a new one
        Try
            mServerManager = CreateServerManager(conn.ConnectionType, conn.ConnectionMode)

            ' Start the connection off
            mServerManager.OpenConnection(conn, Nothing, Nothing)

        Catch ex As Exception
            exception = ex
            Return Nothing
        End Try

        Return mServerManager
    End Function


    ''' <summary>
    ''' Initialises the server manager connection for servers
    ''' </summary>
    ''' <param name="listenMode">The mode to use for the connection</param>
    ''' <param name="dbConnection">The connection setting to use</param>
    ''' <param name="keys">Keys for the encryption schemes</param>
    ''' <param name="systemUser">The system user</param>
    Public Shared Function ServerInit(listenMode As ServerConnection.Mode,
                                      ByRef dbConnection As clsDBConnectionSetting,
                                      keys As Dictionary(Of String, clsEncryptionScheme),
                                      ByRef systemUser As IUser) As ServerManager

        mServerManager = CreateServerManager(dbConnection.ConnectionType, listenMode)

        ' Start the connection off
        mServerManager.OpenConnection(dbConnection, keys, systemUser)

        Return mServerManager
    End Function

    ''' <summary>
    ''' Creates a server manager.
    ''' </summary>
    ''' <param name="connectionType">The type of connection</param>
    ''' <param name="transportMode">The transport mode</param>
    ''' <returns>A new server manager</returns>
    Private Shared Function CreateServerManager(connectionType As ConnectionType, transportMode As ServerConnection.Mode) As ServerManager
        If Not connectionType = ConnectionType.BPServer Then
            ' Direct connection - but needs to know listener connection type for exceptions
            Return New ServerManagerDirect()
        Else
            Select Case transportMode
                Case ServerConnection.Mode.DotNetRemotingInsecure,
                   ServerConnection.Mode.DotNetRemotingSecure
                    Return New ServerManagerDotNetRemote()
                Case ServerConnection.Mode.WCFInsecure,
                    ServerConnection.Mode.WCFSOAPMessageWindows,
                    ServerConnection.Mode.WCFSOAPTransport,
                    ServerConnection.Mode.WCFSOAPTransportWindows
                    Return New ServerManagerWCF()
                Case Else
                    Throw New NotImplementedException("Connection of type " & transportMode.ToString & " is not supported")
            End Select
        End If

    End Function

    ''' <summary>
    ''' Returns the current instance of server manager.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared ReadOnly Property ServerManager() As ServerManager
        Get
            Return mServerManager
        End Get
    End Property

    ''' <summary>
    ''' Check to see if the server is available and connected or not.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function ServerAvailable() As Boolean
        If mServerManager Is Nothing Then Return False
        Return mServerManager.ServerAvailable
    End Function

    ''' <summary>
    ''' Checks if the server is available actively seeking a reconnection if it is
    ''' not immediately available.
    ''' </summary>
    ''' <returns>True if a connection to the server is available through this server
    ''' manager; False if no connection was available and a reconnection attempt
    ''' failed.</returns>
    Friend Shared Function CheckServerAvailability() As Boolean
        Return If(mServerManager?.CheckServerAvailability(), False)
    End Function

    ''' <summary>
    ''' Check if the current client to server connection is valid. An exception will
    ''' be thrown if the connection is not valid.
    ''' </summary>
    Public Shared Sub CurrentConnectionValid()
        ' Throw exception recorded while attempting to connect if available as this 
        ' provides useful details
        If mServerManager?.LastConnectException IsNot Nothing Then
            Throw mServerManager.LastConnectException
        End If

        If mServerManager Is Nothing OrElse Not mServerManager.ServerAvailable Then
            Throw New BluePrismException("Server is unavailable")
        End If

        mServerManager.Server.Validate()
    End Sub

    ''' <summary>
    ''' Attempts to send a close message to the server. Should help update the
    ''' connection count.
    ''' </summary>
    Public Shared Sub Close()
        If mServerManager IsNot Nothing Then
            Try
                mServerManager.CloseConnection()
            Catch ex As Exception
                ' do nothing.
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Tests the validity of the specified connection setting by
    ''' attempting a sample read and a sample write operation.
    ''' </summary>
    ''' <param name="connectionSetting">The connection setting to be tested.
    ''' </param>
    Public Shared Sub Validate(connectionSetting As clsDBConnectionSetting)
        If Not connectionSetting.IsComplete Then
            Throw New BluePrismException("A Database connection setting is blank. Please edit the connection settings")
        End If

        Dim testServMan As ServerManager = Nothing
        Try
            testServMan = CreateServerManager(connectionSetting.ConnectionType, connectionSetting.ConnectionMode)
            testServMan.TestMode = True
            testServMan.OpenConnection(connectionSetting, Nothing, Nothing)

            If testServMan.Server Is Nothing Then
                Throw testServMan.LastConnectException
            End If
            testServMan.Server.Validate()

        Finally
            If testServMan IsNot Nothing Then testServMan.CloseConnection()
        End Try
    End Sub

End Class
