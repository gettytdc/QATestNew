Imports System.ServiceModel
Imports System.ServiceModel.Channels
Imports System.ServiceModel.Description
Imports System.Threading
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.DataMonitor
Imports BluePrism.BPCoreLib
Imports BluePrism.Core.Analytics
Imports BluePrism.Core.Utility
Imports BluePrism.Server.Domain.Models
Imports NLog

''' Project  : AutomateAppCore
''' Class    : ServerManagerWCF
''' 
''' <summary>
''' Provides a WCF client-server transport for clsServer.
''' </summary>
Public NotInheritable Class ServerManagerWCF
    Inherits ServerManager

    Private Shared ReadOnly Log As ILogger = LogManager.GetCurrentClassLogger()

#Region "Implementation of abstract base class methods"
    ''' <summary>
    ''' Opens a WCF connection to the server
    ''' </summary>
    ''' <param name="connectionSetting">The clsDBConnectionSetting to use.</param>
    Public Overrides Sub OpenConnection(connectionSetting As clsDBConnectionSetting, keys As Dictionary(Of String, clsEncryptionScheme), ByRef systemUser As IUser)

        ' Save the setting so we can recreate the connection.
        ConnectionDetails = connectionSetting
        LastConnectException = Nothing
        Dim ch As IChannel = Nothing
        Dim sv As IServer = Nothing

        Try
            Dim cf As ChannelFactory(Of IServer) = GetChannelFactory(connectionSetting)
            ' Add in our special message unpacking code to the client message stack.

            If Options.Instance.WcfPerformanceLogMinutes IsNot Nothing Then
                cf.Endpoint.Behaviors.Add(New WCFAnalyticsEndpointBehavior(Options.Instance.WcfPerformanceLogMinutes.Value))
                Log.Info($"WCF Peformance Started for {Options.Instance.WcfPerformanceLogMinutes.Value} minutes(s)")
            End If

            cf.Endpoint.Behaviors.Add(New ClientExceptionHandlingBehaviour())
            sv = cf.CreateChannel()

            ' Try and open the channel first, meaning that we're using the open
            ' timeout and not the send timeout used when we try to (eg.) get the
            ' Blue Prism version
            ch = TryCast(sv, IChannel)
            If ch IsNot Nothing Then ch.Open()

            Trace.WriteLine("Connection Established")

            Dim serverVersion = sv.GetBluePrismVersion()
            Dim clientVersion As String = clsServer.GetBluePrismVersionS()
            If Not ApiVersion.AreCompatible(serverVersion, clientVersion) Then
                Throw New IncompatibleApiException(clientVersion, serverVersion)
            End If

            sv.EnsureDatabaseConnection()

            mServer = sv

            If sv IsNot Nothing Then
                UpdateConnectionCheckRetry()
                StartDataMonitor()
                Permission.Init(sv)
            End If

        Catch ex As Exception
            LastConnectException = ex
            If ch IsNot Nothing AndAlso ch.State = CommunicationState.Opened Then
                Try
                    ch.Close()
                Catch closeEx As Exception
                    Trace.WriteLine("Error closing channel following exception")
                End Try
            End If
            Throw
        End Try

    End Sub

    Protected Overrides Sub MonitoredDataUpdated(sender As Object, e As MonitoredDataUpdateEventArgs)
        Select Case e.Name
            Case DataNames.Preferences
                UpdateConnectionCheckRetry()
        End Select
        MyBase.MonitoredDataUpdated(sender, e)
    End Sub

    ''' <summary>
    ''' Close the client connection to the server, this ensures the connection
    ''' is re-registered when OpenConnection() is next called.
    ''' </summary>
    Public Overrides Sub CloseConnection()
        Dim proxy = TryCast(mServer, ICommunicationObject)
        If proxy IsNot Nothing Then
            Try
                proxy.Close()
            Catch ce As CommunicationException
                proxy.Abort()
            Catch te As TimeoutException
                proxy.Abort()
            End Try
            ' At this point the IServer instance is dead in the water - we want to
            ' make sure that any further calls to gSv create a new connection rather
            ' than trying to use the dead one

            mServer = Nothing
        End If
    End Sub

    ''' <summary>
    ''' Gets the connection to the server if it is available. Will try to
    ''' connect if not
    ''' </summary>
    Public Overrides ReadOnly Property Server As IServer
        Get

            Dim timer = Stopwatch.StartNew()
            Do
                Try

                    ' If this check fails we don't want it to throw an error until  
                    ' we've tried to reconnect.
                    SyncLock ConnectLock
                        Dim connectionOk As Boolean = False
                        Try
                            connectionOk = CheckConnection()
                        Catch ex As Exception
                            ' Do nothing
                        End Try

                        ' If the check exceptioned, let's try and reconnect.
                        If Not connectionOk Then
                            If ConnectionDetails IsNot Nothing Then

                                TryReconnect()

                            Else
                                ' We've tried to reconnect several times, we need
                                ' to throw an exception or else gSv will be nothing.
                                Throw New UnavailableException(My.Resources.ServerManagerWCF_CannotGetServerNoConnectionDetailsSet)
                            End If
                        End If


                        Return mServer
                    End SyncLock

                Catch ex As Exception
                    OnConnectionPending()
                    If Not HandlersRegistered Then
                        'Hang 5 and then try again
                        Thread.Sleep(5000)
                    End If
                End Try
            Loop Until timer.Elapsed > ServerReconnectTimeout
            Throw New UnavailableException(My.Resources.ServerManagerWCF_CannotGetServer)
        End Get
    End Property

#End Region

#Region "WCF Connection Setup"

    ''' <summary>
    ''' Get the WCF binding to use for this connection (assuming it's a BPServer
    ''' connection - otherwise it makes no sense and an exception will be thrown).
    ''' </summary>
    ''' <returns>The binding.</returns>
    Private Function GetBinding(connectionSetting As clsDBConnectionSetting) As Binding
        If connectionSetting.ConnectionType <> ConnectionType.BPServer Then
            Throw New InvalidOperationException("Bindings are only relevant to BPServer connections")
        End If
        Dim binding As New WSHttpBinding()
        binding.Name = "bpserverbinding"
        binding.HostNameComparisonMode = HostNameComparisonMode.StrongWildcard
        Select Case connectionSetting.ConnectionMode
            Case ServerConnection.Mode.WCFSOAPMessageWindows
                binding.Security.Mode = SecurityMode.Message
            Case ServerConnection.Mode.WCFSOAPTransportWindows
                binding.Security.Mode = SecurityMode.TransportWithMessageCredential
            Case ServerConnection.Mode.WCFSOAPTransport
                binding.Security.Mode = SecurityMode.TransportWithMessageCredential
                binding.Security.Message.ClientCredentialType = MessageCredentialType.UserName
            Case ServerConnection.Mode.WCFInsecure
                binding.Security.Mode = SecurityMode.None
        End Select
        binding.TransactionFlow = False
        binding.MaxReceivedMessageSize = Int32.MaxValue
        binding.MaxBufferPoolSize = Int32.MaxValue
        binding.ReaderQuotas.MaxStringContentLength = Int32.MaxValue
        binding.ReaderQuotas.MaxArrayLength = Int32.MaxValue
        binding.ReaderQuotas.MaxBytesPerRead = 1024 * 1024
        binding.ReaderQuotas.MaxDepth = 128

        binding.OpenTimeout = New TimeSpan(0, 0, If(TestMode, 10, 30))
        binding.CloseTimeout = New TimeSpan(0, 0, 30)
        binding.SendTimeout = New TimeSpan(0, 1, 0) ' changed from 15 minutes as it blocks all calls to server.
        binding.ReceiveTimeout = New TimeSpan(0, 0, 30)

        Return EnableReliableSession(binding, connectionSetting)
    End Function

    Private Function EnableReliableSession(binding As WSHttpBinding, connectionSetting As clsDBConnectionSetting) As CustomBinding
        binding.ReliableSession.Enabled = True
        Dim elements = binding.CreateBindingElements()
        Dim reliableSessionElement = elements.Find(Of ReliableSessionBindingElement)
        If reliableSessionElement IsNot Nothing Then
            If (connectionSetting.MaxTransferWindowSize IsNot Nothing) Then
                Try
                    reliableSessionElement.MaxTransferWindowSize = connectionSetting.MaxTransferWindowSize.Value
                Catch ex As ArgumentOutOfRangeException
                    Throw New ApplicationException(String.Format(My.Resources.MaxTransferWindowSizeOutOfRange, connectionSetting.MaxTransferWindowSize.Value), ex)
                End Try
            End If

            If (connectionSetting.MaxPendingChannels IsNot Nothing) Then
                Try
                    reliableSessionElement.MaxPendingChannels = connectionSetting.MaxPendingChannels.Value
                Catch ex As ArgumentOutOfRangeException
                    Throw New ApplicationException(String.Format(My.Resources.MaxPendingChannelsOutOfRange, connectionSetting.MaxPendingChannels.Value), ex)
                End Try
            End If

            If (connectionSetting.Ordered IsNot Nothing) Then reliableSessionElement.Ordered = binding.ReliableSession.Ordered = connectionSetting.Ordered.Value
        End If

        Dim newBinding = New CustomBinding(elements) With {
            .CloseTimeout = binding.CloseTimeout,
            .OpenTimeout = binding.OpenTimeout,
            .ReceiveTimeout = binding.ReceiveTimeout,
            .SendTimeout = binding.SendTimeout,
            .Name = binding.Name,
            .Namespace = binding.Namespace
        }
        Return newBinding
    End Function

    ''' <summary>
    ''' Get the WCF EndPointAddress to use for this connection (assuming it's a
    ''' BPServer connection - otherwise it makes no sense and an exception will be
    ''' thrown).
    ''' </summary>
    ''' <returns>The EndPointAddress.</returns>
    Private Function GetEndPointAddress(connectionSetting As clsDBConnectionSetting) As EndpointAddress

        If connectionSetting.ConnectionType <> ConnectionType.BPServer Then
            Throw New InvalidOperationException("EndPointAddresses are only relevant to BPServer connections")
        End If
        Dim protocol As String
        If connectionSetting.ConnectionMode = ServerConnection.Mode.WCFSOAPTransport OrElse
            connectionSetting.ConnectionMode = ServerConnection.Mode.WCFSOAPTransportWindows Then
            protocol = "https"
        Else
            protocol = "http"
        End If

        Dim server = IPAddressHelper.EscapeForURL(connectionSetting.DBServer)
        Dim Uri As New Uri(protocol & "://" & server & ":" & connectionSetting.Port.ToString() & "/bpserver")

        ' If using a WCF connection mode that uses Windows Authentication, Kerberos authentication requires 
        ' that the client checks that the endpoint returned from the service after the authentication process 
        ' is the same as the endpoint that we are expecting. This is to prevent phising attacks. In order for
        ' this to work the endpoint on the service has an identity based on an SPN (Server Principal Name).
        ' On the client we need to generate an SPN in the same format as the one created on the service endpoint 
        ' so that we can validate that the service endpoint returned is the one we expected.
        If connectionSetting.ConnectionMode = ServerConnection.Mode.WCFSOAPMessageWindows OrElse
           connectionSetting.ConnectionMode = ServerConnection.Mode.WCFSOAPTransportWindows Then

            ' The SPN needs to be in the same format as the SPN generated on the server which is:
            ' HTTP/<Host Name>:<Port>/BPServer
            ' We can get the server's host name using the DB Server value in the connection settings.
            ' This can be either a FQDN, Host Name, IP Address, Alias Name and Alias FQDN and the
            ' function will still be able to resolve the Host Name.
            Dim hostName = System.Net.Dns.GetHostEntry(connectionSetting.DBServer).HostName
            Dim ident = EndpointIdentity.CreateSpnIdentity(String.Format("HTTP/{0}:{1}/BPServer", hostName, connectionSetting.Port))

            Return New EndpointAddress(Uri, ident)
        Else
            ' Don't need to provide an endpoint identity if not using WCF Win Auth mode
            Return New EndpointAddress(Uri)
        End If



    End Function

    ''' <summary>
    ''' Attempts to re-establish the connection after a fault.
    ''' </summary>
    Private Sub TryReconnect()

        If Not CheckConnection() Then
            CloseConnection()
            OpenConnection(ConnectionDetails, Nothing, Nothing)
            User.ReLogin(mServer)
        End If
        OnConnectionRestored()
    End Sub


    ''' <summary>
    ''' Gets a channel factory suitable for this connection, ensuring that any
    ''' required credentials are configured on it before return.
    ''' </summary>
    Private Function GetChannelFactory(connectionSetting As clsDBConnectionSetting) As ChannelFactory(Of IServer)
        Dim cf As New ChannelFactory(Of IServer)(
            GetBinding(connectionSetting),
            GetEndPointAddress(connectionSetting))

        For Each op As OperationDescription In cf.Endpoint.Contract.Operations
            Dim beh = op.Behaviors.Find(Of DataContractSerializerOperationBehavior)()
            If beh IsNot Nothing Then beh.MaxItemsInObjectGraph = Integer.MaxValue
        Next

        cf.Endpoint.Behaviors.Add(New WCFEndpointBehavior())

        If connectionSetting.ConnectionMode = ServerConnection.Mode.WCFSOAPTransport Then
            cf.Credentials.UserName.UserName = "BluePrism"
            cf.Credentials.UserName.Password = "anon"
        End If

        Return cf
    End Function

#End Region

End Class

