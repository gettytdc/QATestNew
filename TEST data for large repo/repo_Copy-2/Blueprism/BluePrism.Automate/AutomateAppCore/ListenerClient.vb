Imports System.IO
Imports System.Net
Imports System.Net.Security
Imports System.Net.Sockets
Imports System.Security.Cryptography.X509Certificates
Imports System.Threading
Imports System.Threading.Tasks
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Commands
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.Core.Utility
Imports BluePrism.Server.Domain.Models
Imports NLog

''' <summary>
''' The ListenerClient class is used internally to handle
''' details of a connected client. A collection of these is stored
''' in clsListener.Clients.
''' </summary>
Public Class ListenerClient
    Implements IListenerClient

    Private Shared ReadOnly Log As Logger = LogManager.GetCurrentClassLogger()

    'We need to know if the tcpclient has been closed, and therefore disposed.
    Public Property TcpClientDisposed As Boolean

    Private ReadOnly mTcpClient As TcpClient
    Private ReadOnly mStream As Stream


    Public ReadOnly Property CommandFactory As ICommandFactory

    ''' <summary>
    ''' The IP address of the connected client.
    ''' </summary>
    Public ReadOnly Property RemoteAddress As IPAddress Implements IListenerClient.RemoteAddress

    ''' <summary>
    ''' The IP address of the connected client. If the IP is a IPv4 mapped IPv6 address, the returned string is the IP address formatted as an IPv4 address.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property RemoteAddressFriendlyString() As String Implements IListenerClient.RemoteAddressFriendlyString
        Get
            With RemoteAddress
                Return If(.IsIPv4MappedToIPv6, .MapToIPv4().ToString(), .ToString())
            End With
        End Get
    End Property

    ''' <summary>
    ''' Gets the Identity of the remote host according to the hostname DNS entry reached from
    ''' this device, and its IP address.
    ''' </summary>
    ''' <returns>The name of the remote host that this client is connected to or
    ''' null if that name could not be retrieved.</returns>
    Public ReadOnly Property RemoteHostIdentity As String Implements IListenerClient.RemoteHostIdentity


    Public Property IsExpectingPing As Boolean = False Implements IListenerClient.IsExpectingPing

    ''' <summary>
    ''' Buffer used for asyncronous reading of the underlying stream. After
    ''' reads complete, this data is appended to mInputBuffer.
    ''' </summary>
    Private ReadOnly mReadBuffer(4096) As Byte

    ''' <summary>
    ''' Buffer containing input received from the client which has not yet been
    ''' processed. As this is appended to asynchronously, accesses need to be
    ''' protected by a Synclock on <cref>mBufferLock</cref>
    ''' </summary>
    Private mInputBuffer As String = ""

    ''' <summary>
    ''' Locking object for <cref>mInputBuffer</cref>. Locks should always be readonly!
    ''' </summary>
    Private ReadOnly mBufferLock As New Object

    ''' <summary>
    ''' For diagostics purposes.
    ''' </summary>
    Private mRemoveReason As String


    Private ReadOnly mLastCommunication As Stopwatch

    Private ReadOnly mPingTimeoutSeconds As Integer
    Private mCancellationTokenSource As CancellationTokenSource = New CancellationTokenSource()

    ''' <summary>
    ''' The reason for this client's removal - either the manually set reason or
    ''' the message from the exception recorded in this client.
    ''' </summary>
    Public Property RemoveReason() As String Implements IListenerClient.RemoveReason
        Get
            If mException IsNot Nothing Then
                If TypeOf mException Is ApplicationException Then
                    Return mException.Message
                Else
                    Return mException.ToString()
                End If
            End If
            Return mRemoveReason
        End Get
        Set(ByVal value As String)
            mRemoveReason = value
            mException = Nothing
        End Set
    End Property

    ''' <summary>
    ''' Gets the full disconnect message for this client.
    ''' </summary>
    Public ReadOnly Property DisconnectMessage() As String Implements IListenerClient.DisconnectMessage
        Get
            Dim msg As String = $"Disconnected {RemoteHostIdentity}"
            Dim reason As String = RemoveReason
            Log.Debug($"{msg} - {reason}")
            If reason IsNot Nothing Then Return $"{msg} - {reason}" Else Return msg
        End Get
    End Property

    ''' <summary>
    ''' The exception which caused this client to fail, if there is one.
    ''' If, when the client is closed, this is null, normal termination is
    ''' assumed.
    ''' If this is non-null, the message in the exception is used as the reason
    ''' why the client failed for the purposes of logging the error.
    ''' </summary>
    Public mException As Exception

    ''' <summary>
    ''' Flag indicating if this listener has errored.
    ''' </summary>
    Public ReadOnly Property IsErrored() As Boolean Implements IListenerClient.IsErrored
        Get
            Return (mException IsNot Nothing)
        End Get
    End Property

    ''' <summary>
    ''' True if we are a pool member and this is a verified connection from the
    ''' controller of our pool. This status is set by the use of the 'controller'
    ''' command.
    ''' </summary>
    Public Property IsControllerConnection As Boolean Implements IListenerClient.IsControllerConnection

    ''' <summary>
    ''' The name of the user for which authentication has been requested
    ''' </summary>
    Public Property RequestedUserName() As String Implements IListenerClient.RequestedUserName

    Public Property RequestedAuthenticationMode As AuthMode Implements IListenerClient.RequestedAuthenticationMode

    ''' <summary>
    ''' True when a user name has been entered, but not yet authenticated.
    ''' Used only to validate the authentication sequence; the user name at this 
    ''' stage may not be valid.
    ''' </summary>
    Public Property UserRequested() As Boolean Implements IListenerClient.UserRequested

    ''' <summary>
    ''' The authenticated user
    ''' </summary>
    Public Property AuthenticatedUser As IUser Implements IListenerClient.AuthenticatedUser

    ''' <summary>
    ''' Indicates whether or not a user has been authenticated
    ''' </summary>
    Public ReadOnly Property UserSet() As Boolean Implements IListenerClient.UserSet
        Get
            Return AuthenticatedUser IsNot Nothing
        End Get
    End Property

    ''' <summary>
    ''' The current authenticated user ID
    ''' </summary>
    Public ReadOnly Property UserId() As Guid Implements IListenerClient.UserId
        Get
            If Not UserSet Then Return Guid.Empty
            Return AuthenticatedUser.Id
        End Get
    End Property

    ''' <summary>
    ''' The current authenticated user name
    ''' </summary>
    Public ReadOnly Property UserName() As String Implements IListenerClient.UserName
        Get
            If Not UserSet Then Return String.Empty
            Return AuthenticatedUser.Name
        End Get
    End Property

    ''' <summary>
    ''' Indicates whether or not the current authenticated user has Control Resource permissions.
    ''' </summary>
    Public ReadOnly Property HasControlResourcePermission() As Boolean Implements IListenerClient.HasControlResourcePermission
        Get
            If Not UserSet Then Return False
            'System user bypasses permissions checks...
            If AuthenticatedUser.AuthType = AuthMode.System Then Return True
            Return AuthenticatedUser.HasPermission(Permission.Resources.ControlResource)
        End Get
    End Property

    ''' <summary>
    ''' Current set of startup parameters to be used on this client connection.
    ''' Set by the 'startp' command, and used when the 'start' command is used.
    ''' Can be Nothing if no parameters have been set.
    ''' </summary>
    Public Property StartupParameters As clsArgumentList Implements IListenerClient.StartupParameters

    ''' <summary>
    ''' For convenience, a copy of StartupParameters in XML format. Only valid when
    ''' StartupParameters isn't Nothing.
    ''' </summary>
    Public Property StartupParametersXml As String Implements IListenerClient.StartupParametersXml

    Public Property LastSessionCreated As Guid Implements IListenerClient.LastSessionCreated

    ''' <summary>
    ''' True if unsolicited status updates (i.e. those preceded by ">>") should be
    ''' send to this client. This is currently enabled by the client's use of the
    ''' 'version' command. They should never be enabled if a client connects by
    ''' HTTP.
    ''' </summary>
    Public Property SendStatusUpdates As Boolean Implements IListenerClient.SendStatusUpdates

    ''' <summary>
    ''' True if the client wants to be sent session variable updates. These are a
    ''' subset of normal status updates, and this flag will only have any effect
    ''' if SendStatusUpdates is also True.
    ''' </summary>
    Public Property SendSessionVariableUpdates As Boolean Implements IListenerClient.SendSessionVariableUpdates


    ''' <summary>
    ''' True when the client command handler is processing a pending HTTP request.
    ''' i.e. the request has started, but not yet completed.
    ''' </summary>
    Public mbPendingHTTPRequest As Boolean

    ''' <summary>
    ''' When handling a POST, this determines which of the three current kinds of
    ''' POST we are dealing with.
    ''' 
    ''' Type a) mbPOSTWSExec=True - we're handling a web service request.
    ''' 
    ''' Type b) mbPOSTExec=False, returns the 'ResourcePCActionPage', and takes
    ''' various parameters which are substituted into that page - e.g. the process
    ''' to run. NOTE: This is potentially obsolete, unless someone is making the 
    ''' HTTP post request externally, as this particular call is not made from 
    ''' within the product.
    ''' 
    ''' Type c) mbPOSTExec=True, is equivalent to what we normally handle as a GET
    ''' request, i.e. a list of listener commands to execute. This is used to get
    ''' around the pesky IE URL length limit.
    ''' </summary>
    Public mbPOSTExec As Boolean
    Public mbPOSTWSExec As Boolean

    ''' <summary>
    ''' When mbPOSTWSExec is True, these contain the information about the process
    ''' we are being asked to run.
    ''' </summary>
    Public WebServiceProcessId As Guid
    Public WebServiceProcessName As String
    Public WebServiceProcessType As DiagramType
    Public WebServiceQueuedPost As String

    ''' <summary>
    ''' When this is not Nothing, we are in the middle of running a process on
    ''' behalf on an HTTP Web Service client.
    ''' </summary>
    Public mWSRunner As ListenerRunnerRecord

    ''' <summary>
    ''' Stores any HTTP credentials received with a request - i.e. the contents
    ''' of the "Authorization" header. Nothing if none have been received.
    ''' </summary>
    Public mHTTPCredentials As String


    ''' <summary>
    ''' When mbPendingHTTPRequest is True, this is either a list of commands to
    ''' execute when the request is complete (GET) or Nothing for a POST /automate
    ''' request, where all relevant information is posted, allowing a clean
    ''' browser URL.
    ''' When mbPendingHTTPRequest is False, this is undefined and irrelevant.
    ''' </summary>
    Public mHTTPRequests As List(Of String)

    ''' <summary>
    ''' True when the client command handler is waiting for posted data to finish
    ''' off an HTTP POST request.
    ''' </summary>
    Public mbPendingHTTPPost As Boolean

    ''' <summary>
    ''' When handling a POST, this is the length of the posted content.
    ''' </summary>
    Public miHTTPPOSTLength As Integer

    ''' <summary>
    ''' When receiving an HTTP request, this is set to True if the client is
    ''' expecting a 100 Continue response.
    ''' </summary>
    Public mHTTPContinueExpected As Boolean


    ''' <summary>
    ''' Details of the current HTTP request, parsed via ParseHTTPCommand.
    ''' </summary>
    Public mHTTPVerb As String                  'e.g. "GET"
    Public mHTTPResource As String              'e.g. "/the/resource/is.this
    Public mHTTPVersion As String               'e.g. "HTTP/1.1"

    ''' <summary>
    ''' Flag to say whether HTTP KeepAlive is enabled for the current request.
    ''' </summary>
    Public mHTTPKeepAlive As Boolean

    ''' <summary>
    ''' Parse the initial HTTP command into its three component parts, which are
    ''' stored in mHTTPVerb,mHTTPResource and mHTTPVersion. Additionally, the
    ''' mHTTPKeepAlive flag is set to the correct default for the HTTP version.
    ''' </summary>
    ''' <param name="command">The initial HTTP command line, e.g.:
    '''   GET /the/resource/is.this HTTP/1.1</param>
    ''' <param name="response">A ListenerResponse object. If an error occurs during
    ''' parsing, this will be filled in with the correct response to return to the
    ''' client.</param>
    ''' <returns>True if successful, False otherwise.</returns>
    Public Function ParseHTTPCommand(ByVal command As String, ByVal response As ListenerResponse) As Boolean Implements IListenerClient.ParseHTTPCommand

        'Get the component parts of the request...
        Dim parts() As String = command.Split(" "c)
        If parts.Length < 2 Or parts.Length > 3 Then
            response.HTTP = True
            response.HTTPStatus = HttpStatusCode.BadRequest
            response.Text = "Invalid syntax in " & command
            Return False
        End If

        mHTTPVerb = parts(0)
        mHTTPResource = parts(1)
        If parts.Length = 3 Then
            mHTTPVersion = parts(2)
        Else
            'If there is no HTTP version, it must be an old-fashioned "Simple
            'Request"
            mHTTPVersion = "HTTP/0.9"
            If mHTTPVerb <> "GET" Then
                response.HTTP = True
                response.HTTPStatus = HttpStatusCode.BadRequest
                response.Text = "Simple request can only use GET"
                Return False
            End If
        End If
        Select Case mHTTPVersion
            Case "HTTP/1.0", "HTTP/0.9"
                mHTTPKeepAlive = False
            Case "HTTP/1.1"
                mHTTPKeepAlive = True
            Case Else
                response.HTTP = True
                response.HTTPStatus = HttpStatusCode.BadRequest
                response.Text = "Invalid HTTP version"
                Return False
        End Select
        Return True

    End Function


    Public Sub New(commandFactory As Func(Of ListenerClient, ICommandFactory),
                   tcpclient As TcpClient, sslcert As X509Certificate, pingTimeoutSeconds As Integer)
        Me.CommandFactory = commandFactory(Me)
        mPingTimeoutSeconds = pingTimeoutSeconds
        IsControllerConnection = False
        AuthenticatedUser = Nothing
        RequestedUserName = String.Empty
        StartupParameters = Nothing
        mbPendingHTTPRequest = False
        mbPendingHTTPPost = False
        SendStatusUpdates = False
        SendSessionVariableUpdates = False
        mbPOSTExec = False
        mbPOSTWSExec = False
        mWSRunner = Nothing
        WebServiceQueuedPost = Nothing
        mHTTPCredentials = Nothing

        mTcpClient = tcpclient
        mTcpClient.SendTimeout = 5000
        mTcpClient.ReceiveTimeout = 5000
        mTcpClient.NoDelay = True
        mTcpClient.ReceiveBufferSize = 4096
        mTcpClient.SendBufferSize = 4096

        'need to know if the mTcpClient has been disposed.
        TcpClientDisposed = False

        'Get the remote IP address...
        RemoteAddress = CType(mTcpClient.Client.RemoteEndPoint, IPEndPoint).Address
        RemoteHostIdentity = GetRemoteHostIdentity()

        If sslcert IsNot Nothing Then
            Dim s = New SslStream(mTcpClient.GetStream(), False)
            mStream = s
            Try
                s.AuthenticateAsServer(sslcert, False, True)
            Catch ex As Exception
                Throw New SslAuthenticationException(ex)
            End Try
        Else
            mStream = mTcpClient.GetStream()
        End If
        mLastCommunication = Stopwatch.StartNew
        StartReaderThread()

    End Sub

    Private Function GetRemoteHostIdentity() As String
        Dim remoteHostName As String
        Try
            remoteHostName = Dns.GetHostEntry(RemoteAddress).HostName
        Catch ex As Exception
            Log.Debug(My.Resources.ListenerClient_FailedToObtainHostname, RemoteAddressFriendlyString, ex)
            remoteHostName = "Unknown"
        End Try
        Return $"{remoteHostName} ({RemoteAddressFriendlyString})"
    End Function

    Private Sub StartReaderThread()
        Const ioExceptionText = "Listener[{0}] An IOException occurred while reading the input stream: {1}"
        Const objDisposedText = "Listener[{0}] An ObjectDisposedException occurred while reading the input stream: {1}"
        Const exceptionText = "Talker[{0}] An error occurred reading the input stream; Exception: {1}"

        Task.Run(Async Function()
                     Try

                         Dim bytes As Integer = -1
                         If mStream?.CanRead Then
                             While bytes <> 0 AndAlso Not mCancellationTokenSource.Token.IsCancellationRequested
                                 Dim task = mStream.ReadAsync(mReadBuffer, 0, mReadBuffer.Length, mCancellationTokenSource.Token)
                                 Try
                                     bytes = Await task.WithCancellation(mCancellationTokenSource.Token)
                                     Log.Debug($"tcp>> Listener New incoming data")
                                 Catch OperationCanceledException As OperationCanceledException
                                     If Not task.IsCompleted Then
                                         Log.Debug($"Task canceled handled")
                                     End If
                                     Exit While
                                 End Try

                                 SyncLock mBufferLock
                                     Dim message = Encoding.UTF8.GetString(mReadBuffer, 0, bytes)
                                     Log.Debug($"tcp>> ListenerClient>> Read message {message} from stream")
                                     mInputBuffer += message
                                 End SyncLock
                                 mLastCommunication.Restart()
                                 Thread.Sleep(50)
                             End While
                         End If
                     Catch ex As IOException
                         Log.Debug(ioExceptionText, RemoteHostIdentity, ex)
                         ' IOException during read is quite normal it simply means
                         ' that the underlying socket was closed by the remote machine
                     Catch ex As ObjectDisposedException
                         Log.Debug(objDisposedText, RemoteHostIdentity, ex)
                         ' ObjectDisposedException means the NetworkStream is closed or
                         ' there is a failure reading from the network.
                     Catch ex As AggregateException
                         ' handle the above exceptions when bundled as part of an AggregateExcpetion, most often by a Parallel task
                         For Each e In ex.InnerExceptions
                             If TypeOf e Is IOException Then
                                 Log.Debug(ioExceptionText, RemoteHostIdentity, e)
                             ElseIf TypeOf e Is ObjectDisposedException Then
                                 Log.Debug(objDisposedText, RemoteHostIdentity, e)
                             ElseIf e IsNot Nothing Then
                                 Log.Info(exceptionText, RemoteHostIdentity, e)
                             End If
                         Next
                     Catch ex As Exception
                         Log.Info(exceptionText, RemoteHostIdentity, ex)
                         'Something else went wrong reading from the stream, so we just won't
                         'trigger the next read, since it must be dead now.
                     Finally
                         Log.Debug($"ListenerClient: Exiting Stream.ReadAsync {RemoteHostIdentity}")
                         mCancellationTokenSource.Dispose()
                         mCancellationTokenSource = Nothing
                     End Try
                 End Function)

    End Sub


    ''' <summary>
    ''' Close the connection.
    ''' </summary>
    Public Sub Close() Implements IListenerClient.Close
        Log.Debug("Closing connection")
        Try
            mStream.Flush()
        Catch ex As Exception
            'Don't care
        End Try
        mCancellationTokenSource?.Cancel()
        Thread.Sleep(0)
        mStream?.Close()
        mTcpClient?.Close()
        TcpClientDisposed = True
    End Sub

    ''' <summary>
    ''' Send bytes to the client.
    ''' </summary>
    ''' <param name="buffer">The buffer containing the data to send.</param>
    Public Sub Send(buffer() As Byte) Implements IListenerClient.Send
        SyncLock mBufferLock
            mStream.Write(buffer, 0, buffer.Length)
            mStream.Flush()
        End SyncLock
        Thread.Yield()
    End Sub

    ''' <summary>
    ''' Read a single line from the buffer. Returns Nothing if no complete line
    ''' is available yet.
    ''' </summary>
    Public Function ReadLine() As String Implements IListenerClient.ReadLine
        SyncLock mBufferLock
            Return LineReader.ReadLine(mInputBuffer)
        End SyncLock
    End Function

    ''' <summary>
    ''' Reads the HTTP Payload from the buffer into a String
    ''' </summary>
    Public Function ReadHTTPPayload() As String Implements IListenerClient.ReadHTTPPayload
        SyncLock mBufferLock
            ' Yuck! This code is horrible but...
            Dim src As Byte() = Encoding.UTF8.GetBytes(mInputBuffer)
            Dim dest(miHTTPPOSTLength) As Byte
            Buffer.BlockCopy(src, 0, dest, 0, miHTTPPOSTLength)
            Dim postdata = Encoding.UTF8.GetString(dest)
            If Not String.IsNullOrEmpty(postdata) Then
                mInputBuffer = mInputBuffer.Substring(postdata.Length - 1)
            End If
            Return postdata
        End SyncLock
    End Function

    ''' <summary>
    ''' Returns true if the HTTP Payload has been fully recieved.
    ''' </summary>
    Public Function HTTPPayloadRecieved() As Boolean Implements IListenerClient.HTTPPayloadRecieved
        SyncLock mBufferLock
            Return Encoding.UTF8.GetByteCount(mInputBuffer) >= miHTTPPOSTLength
        End SyncLock
    End Function

    ''' <summary>
    ''' A quick 'is the client still connected' check.
    ''' </summary>
    ''' <returns>True if the client is probably connected!</returns>
    Public Function IsConnected() As Boolean Implements IListenerClient.IsConnected
        Return mTcpClient.Connected
    End Function

    ''' <summary>
    ''' Do a check to see if this client has disconnected. This returns a more
    ''' categorical answer than IsConnected().
    ''' </summary>
    ''' <returns>True if the client is disconnected.</returns>
    Public Function IsDisconnected() As Boolean Implements IListenerClient.IsDisconnected
        Return mTcpClient.Client.Poll(20000, System.Net.Sockets.SelectMode.SelectRead) AndAlso mTcpClient.Client.Available = 0
    End Function

    ''' <summary>
    ''' Check if this client is local - i.e. running on the same machine as the
    ''' listener. It must have connected via a loopback address for this to be
    ''' so.
    ''' </summary>
    Public Function IsLocal() As Boolean Implements IListenerClient.IsLocal
        With RemoteAddress
            If .Equals(IPAddress.IPv6Loopback) Then Return True
            If .Equals(IPAddress.Loopback) Then Return True

            If .IsIPv4MappedToIPv6 Then
                If .MapToIPv4.Equals(IPAddress.Loopback) Then Return True
            End If
        End With
        Return False
    End Function

    Private Property mCanClose As Boolean = True

    Public Sub PreventClosure()
        mCanClose = False
    End Sub

    Public Sub AllowClosure()
        mCanClose = True
    End Sub

    Public Function ShouldClose() As Boolean
        Return mCanClose AndAlso IsDisconnected()
    End Function

End Class
