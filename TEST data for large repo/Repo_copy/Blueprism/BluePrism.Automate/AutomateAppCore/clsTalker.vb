Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Resources

Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.IO

Imports System.Net.Security
Imports System.Security.Cryptography.X509Certificates
Imports System.Security.Authentication
Imports System.Threading.Tasks

Imports BluePrism.BPCoreLib.DependencyInjection
Imports NLog
Imports BluePrism.Core.Utility
Imports BluePrism.Core.Logging
Imports BluePrism.Core.Extensions
Imports BluePrism.Server.Domain.Models
Imports BluePrism.ClientServerResources.Core.Data
Imports BluePrism.ClientServerResources.Core.Enums
Imports BluePrism.ClientServerResources.Core.Events


'Set the following to "True" to have lots and lots of diagnostic
'information written to the debug console. Otherwise, leave it set
'at "False", and don't check the code in with it set to "True"!!
#Const DebugOutput = False

''' Project  : AutomateAppCore
''' Class    : clsTalker
''' 
''' <summary>
''' This class encapsulates a connection to a Resource PC from a client such as
''' Control Room. It provides functionality to allow the client to talk to the
''' Resource PC via the Telnet protocol which is provided on the
''' <see cref="ResourceMachine.DefaultPort">default port</see> when a Resource PC
''' is running. When running a command-line instance of the Resource PC, the port can
''' be specified, and is appended to the registered resource name, e.g. PcName:8182
''' 
''' A corresponding class, clsListener, handles the other end of this connection, and
''' is a better reference point for details of the protocol.
''' 
''' When testing applications that use this class, it can be very useful to manually
''' simulate the actions you intend, to be sure the responses are as expected. The
''' procedure is as follows:
''' 
'''  1. Start a Resource PC
'''  2. Connect by typing in a command prompt: "telnet localhost 8181"
'''     Obviously replace 'localhost' if the Resource PC is elsewhere
'''  3. Type 'help' and press return. The Resource PC will respond
'''     with a list of other commands that are available.
''' 
''' Here is an example command sequence after connecting, to start a process. The
''' server's response to each command is in the right-hand column.
''' 
'''   user {user id}            USER SET
'''   create {process id}       SESSION CREATED {session id}
'''   start {session id}        SESSION STARTED
'''   status                    ...status displayed - process should
'''                                 show as "RUNNING"...
''' 
''' To authenticate using a user name (some commands require authentication)
''' 
'''   user name {user name}
'''   password {users password}
''' 
''' When entering guids in the above commands, you only need to specify the first
''' few characters - enough to uniquely identify the guid you want. The commands
''' 'userlist' and 'proclist' make this easy.
''' 
''' </summary>
Public Class clsTalker : Implements IDisposable

    Private Shared ReadOnly Log As ILogger = LogManager.GetCurrentClassLogger()

#Region " Member Variables / Events "

    ''' <summary>
    ''' Event raised when notification of a session variable changing is received.
    ''' </summary>
    ''' <param name="sv">The changed session variable.</param>
    Public Event SessionVariableChanged(ByVal sv As clsSessionVariable)

    ''' <summary>
    ''' Event raised when notification of a session starting is received.
    ''' </summary>
    Public Event SessionStarted As SessionStartEventHandler

    ''' <summary>
    ''' Event raised when notification of a session ending is received.
    ''' </summary>
    Public Event SessionEnded As SessionEndEventHandler

    ' Regular expression to read a line from a "status" call to the resource PC.
    Private Shared ReadOnly StatusLineRegex As _
     New Regex("^[-\s]*([A-Za-z0-9_]+)\s+([0-9A-Fa-f\-]+)\s*$")

    ' Flag indicating whether we currently have a connection to the remote listener
    Friend mConnected As Boolean

    ' Time of last communication with remote listener.
    Private mLastCommunication As Stopwatch

    ' The client associated with the connection
    Private mClient As ITcpClient

    ' The network stream associated with the connection
    Private mStream As Stream

    ' The last server response line - used between calls to the Say and GetReply
    ' methods.
    Private mReply As String

    ' Flag indicating that there has been a status change detected on the remote
    ' resource PC
    Private mStatusChanged As Boolean

    ' The number of processes running on the remote resource at last report
    Private mProcessesRunning As Integer = 0

    ' The number of processes pending on the remote resource at last report
    Private mProcessesPending As Integer = 0

    ' Flag indicating if this talker has been disposed or not
    Private mDisposed As Boolean

    ''' <summary>
    ''' Buffer used for asyncronous reading of the underlying stream. After
    ''' reads complete, this data is appended to mInputBuffer.
    ''' </summary>
    Private mReadBuffer(4096) As Byte

    ''' <summary>
    ''' Buffer containing input received from the server which has not yet been
    ''' processed. As this is appended to asynchronously, accesses need to be
    ''' protected by a Synclock on <cref>mBufferLock</cref>
    ''' </summary>
    Friend mInputBuffer As String = String.Empty

    ''' <summary>
    ''' Locking object for <cref>mInputBuffer</cref>. Locks should always be readonly!
    ''' </summary>
    Private ReadOnly mBufferLock As New Object


    ''' <summary>
    ''' Locking object to ensure multiple threads are not writing to the stream
    ''' (during the <cref>Say</cref> operation). Previously this was achieved using
    ''' Me, which is explicity discoraged in the programming practices page:
    ''' https://msdn.microsoft.com/en-us/library/3a86s51t.aspx
    ''' </summary>
    Private ReadOnly mSayLock As New Object

    ' The host name that this talker is connected to
    Private mHostName As String

    ' The port number that this talker is connected to
    Private mPort As Integer

    ' Whether this connection represents an SSL connection or not
    Private mSsl As Boolean

    ' The reconnect status of the resource connection
    Private mReconnectStatus As ResourceReconnectStatus
    Private mResourceName As String

    ' Used to create TcpClient instances.
    Private mTcpClientFactory As IIPv6TcpClientFactory

    Private mPingTimeoutSeconds As Integer

    Private ReadOnly mAscrOn As Boolean
    Private mRobotAddressStore As IRobotAddressStore
    Private mCancellationTokenSource As CancellationTokenSource

#End Region

#Region " Constructors / Destructors "

    ''' <summary>
    ''' Creates a new Talker object
    ''' </summary>
    Public Sub New(pingTimeoutSeconds As Integer)
        mTcpClientFactory = DependencyResolver.Resolve(Of IIPv6TcpClientFactory)()
        mRobotAddressStore = DependencyResolver.Resolve(Of IRobotAddressStore)()
        mStatusChanged = False
        mConnected = False
        mPingTimeoutSeconds = pingTimeoutSeconds
        mAscrOn = gSv.GetPref(SystemSettings.UseAppServerConnections, False)
    End Sub

    ''' <summary>
    ''' Ensures that the connection is closed when the object is being GC'ed.
    ''' </summary>
    Protected Overrides Sub Finalize()
        Close()
    End Sub

    ''' <summary>
    ''' Create and use a clsTalker instance to proxy an instruction via a Pool
    ''' Controller to the actualy responsible Resource (Pool Member).
    ''' </summary>
    ''' <param name="m">The target Pool Member.</param>
    ''' <param name="user">The user to proxy the command for</param>
    ''' <param name="poolId">The id of the pool</param>
    ''' <param name="command">The command to send/proxy.</param>
    ''' <param name="expectedReply">The expected reply.</param>
    ''' <param name="timeout">A timeout.</param>
    ''' <param name="startParams">Startup parameters to set, before sending the
    ''' main command, or None. In XML format.</param>
    ''' <param name="withReplyLines">True to expect and include a number of reply
    ''' lines (determined by the reply header) in the reply.</param>
    ''' <param name="errcode">If None is returned, this will contain an 'error
    ''' code' (which are backwards compatible with the previous code before
    ''' this was refactored.</param>
    ''' <returns>The response to the proxied command, or None.</returns>
    Public Shared Function PoolControllerProxyCommand(m As IResourceMachine,
                                                      user As IUser,
                                                      poolId As Guid,
                                                      command As String,
                                                      expectedReply As String,
                                                      timeout As Integer,
                                                      startParams As String,
                                                      withReplyLines As Boolean,
                                                      ByRef errcode As String) As String
        Dim sErr As String = Nothing
        Using t As New clsTalker(timeout)
            If Not t.Connect(m.Name, sErr) Then
                errcode = "CANNOT CONNECT"
                Return Nothing
            End If
            If Not t.Authenticate() Then
                errcode = "AUTHENTICATION FAILURE"
                Return Nothing
            End If
            If Not t.Say("controller " & poolId.ToString, "OK") Then
                errcode = "UNEXPECTED RESPONSE"
                Return Nothing
            End If
            If Not t.Say("proxyfor " & user.Id.ToString, "USER SET") Then
                errcode = "UNEXPECTED RESPONSE"
                Return Nothing
            End If
            If startParams IsNot Nothing Then
                If Not t.Say("startp " & startParams,
                     "PARAMETERS SET", 10000) Then
                    errcode = "FAILED TO SET PARAMETERS"
                    Return Nothing
                End If
            End If
            If Not t.Say(command, expectedReply, timeout) Then
                errcode = "UNEXPECTED RESPONSE"
                Return Nothing
            End If
            Dim reply As String = t.GetReply()
            If Not withReplyLines Then Return reply
            Dim num As Integer = CInt(reply.Substring(expectedReply.Length))
            Dim sb As New StringBuilder(reply)
            While num > 0
                sb.AppendLine().Append(
                    t.GetNextIncomingLine(TimeSpan.FromSeconds(1)))
                num -= 1
            End While
            sb.AppendLine()
            Return sb.ToString()
        End Using
    End Function

    ''' <summary>
    ''' Check the pool contains the resource 
    ''' </summary>
    ''' <param name="poolResource"></param>
    ''' <returns></returns>
    Public Function IsResourcePCStillController(poolResource As ResourceMachine) As Boolean
        Return poolResource IsNot Nothing AndAlso
            poolResource.ChildResourceNames IsNot Nothing AndAlso
            poolResource.ChildResourceNames.Contains(mResourceName)
    End Function


#End Region

#Region " Properties "

    Public ReadOnly Property IsConnnected() As Boolean
        Get
            Return mConnected
        End Get
    End Property

    ''' <summary>
    ''' The number of processes the remote Resource PC is currently running. This is
    ''' kept up to date by status messages.
    ''' </summary>
    Public ReadOnly Property ProcessesRunning() As Integer
        Get

            Return mProcessesRunning
        End Get
    End Property

    ''' <summary>
    ''' The number of processes the remote Resource PC has pending. This is
    ''' kept up to date by status messages.
    ''' </summary>
    Public ReadOnly Property ProcessesPending() As Integer
        Get
            Return mProcessesPending
        End Get
    End Property

    ''' <summary>
    ''' Indicates whether the talker is currently attempting to reconnect a resource 
    ''' </summary>
    Private Property IsAttemptingReconnect As Boolean
        Get
            Return mReconnectStatus.HasFlag(
                ResourceReconnectStatus.AttemptingReconnect)
        End Get
        Set(value As Boolean)
            mReconnectStatus =
             If(value,
                mReconnectStatus Or ResourceReconnectStatus.AttemptingReconnect,
                mReconnectStatus And Not ResourceReconnectStatus.AttemptingReconnect)
        End Set
    End Property

    ''' <summary>
    ''' Indicates whether the talker will attempt to reconnect a resource 
    ''' when an attempt to connect fails.
    ''' </summary>
    Public Property WillAttemptReconnectOnFail As Boolean
        Get
            Return mReconnectStatus.HasFlag(ResourceReconnectStatus.ReconnectOnFail)
        End Get
        Set(value As Boolean)
            mReconnectStatus =
             If(value,
                mReconnectStatus Or ResourceReconnectStatus.ReconnectOnFail,
                mReconnectStatus And Not ResourceReconnectStatus.ReconnectOnFail)
        End Set
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Check that the connection to the Resource PC is active, and Resource PC is still alive.
    ''' The command 'ping' is sent to the Resource PC, and a response of 'pong' is expected. This 
    ''' sequence can be skipped and the connection assumed to be OK if the Resource PC has recently
    ''' been in communication.
    ''' </summary>
    ''' <param name="skipIfRecentlyCommunicated">Controls whether connection check can be skipped
    ''' and the connection assumed to be OK if the Resource PC has recently been in communication
    ''' </param>
    ''' <returns>True if the connection is ok</returns>
    Public Function CheckConnection(skipIfRecentlyCommunicated As Boolean) As Boolean
        If Not mConnected Then Return False

        If skipIfRecentlyCommunicated AndAlso HasRecentlyCommunicated() Then Return True

        Try
            If Not Say("ping", "pong", mPingTimeoutSeconds * 1000) Then
                Return False
            End If

        Catch ex As Exception
            Log.Info(ex, "{0}:{1} Connection not available", mHostName, mPort)
            Return False
        End Try
        Return True
    End Function

    ''' <summary>
    ''' Indicates whether we have communication with the server
    ''' running on the Resource PC within the last 10 seconds
    ''' </summary>
    ''' <returns></returns>
    Private Function HasRecentlyCommunicated() As Boolean

        Return mLastCommunication.Elapsed < TimeSpan.FromSeconds(10)

    End Function

    Public ReadOnly Property LastPingTime As Long
        Get
            If mLastCommunication Is Nothing Then
                Return -1
            Else
                Return CLng((mLastCommunication?.ElapsedMilliseconds))
            End If
        End Get
    End Property

    ''' <summary>
    ''' Check if the status of the Resource PC has changed. The flag
    ''' is then reset.
    ''' </summary>
    ''' <returns>True if the status of the Resource PC has changed
    ''' since the last call to this method.</returns>
    Public Function CheckStatusChanged() As Boolean
        Dim bResult As Boolean
        bResult = mStatusChanged
        mStatusChanged = False
        Return bResult
    End Function

    ''' <summary>
    ''' Process a status update from the Resource PC. These are the 'out of band'
    ''' asyncronous messages that begin with '>>'.
    ''' </summary>
    ''' <param name="line">The status update to process.</param>
    Private Sub ProcessStatusUpdate(ByVal line As String)
        Try
            Log.Trace("Talker[{0}:{1}] ProcessStatusUpdate : {2}", mHostName, mPort, line)
            If line.StartsWith(">>STATUS ") Then
                Dim i, i2 As Integer
                i = line.IndexOf("Pending:")
                If i <> -1 Then
                    i2 = line.IndexOfAny(New Char() {" "c, CChar(vbCr)}, i)
                    If i2 <> -1 Then
                        mProcessesPending = Integer.Parse(line.Substring(i + 8, i2 - i - 8))
                    End If
                End If
                i = line.IndexOf("Running:")
                If i <> -1 Then
                    i2 = line.IndexOfAny(New Char() {" "c, CChar(vbCr)}, i)
                    If i2 <> -1 Then
                        mProcessesRunning = Integer.Parse(line.Substring(i + 8, i2 - i - 8))
                    End If
                End If
            ElseIf line.StartsWith(">>VAR ") Then
                Dim info As String = line.Substring(6)
                Dim index As Integer = info.IndexOf(" "c)
                If index <> -1 Then
                    Dim sv As clsSessionVariable = clsSessionVariable.Parse(info.Substring(index + 1))
                    sv.sessionID = New Guid(info.Substring(0, index))
                    RaiseEvent SessionVariableChanged(sv)
                End If
            Else
                'A status update, so record the fact the
                'Resource PC status has changed...
                mStatusChanged = True
                If line.StartsWith(">>STARTED ") Then
                    Dim index = line.IndexOf(" "c, 10)
                    If index <> -1 Then
                        Dim sessId As Guid = New Guid(line.Substring(8, index - 8))
                        OnSessionStarted(New SessionStartEventArgs(sessid := sessId))
                    End If
                ElseIf line.StartsWith(">>END ") Then
                    Dim index As Integer = line.IndexOf(" "c, 6)
                    If index <> -1 Then
                        Dim sessId As Guid = New Guid(line.Substring(6, index - 6))
                        OnSessionEnded(
                            New SessionEndEventArgs(sessId, line.Substring(index + 1)))
                    End If
                End If
            End If
        Catch ex As Exception
            Log.Error(ex, "Talker[{0}:{1}] Failed to parse resource status",
                mHostName, mPort)
            Debug.Fail("Status parse failure: " & ex.Message)
        End Try

    End Sub

    ''' <summary>
    ''' Raises the <see cref="SessionEnded"/> event.
    ''' </summary>
    Protected Overridable Sub OnSessionEnded(e As SessionEndEventArgs)
        RaiseEvent SessionEnded(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="SessionStarted"/> event.
    ''' </summary>
    Protected Overridable Sub OnSessionStarted(e As SessionStartEventArgs)
        RaiseEvent SessionStarted(Me, e)
    End Sub



    ''' <summary>
    ''' Process any incoming data from the resource PC.
    ''' </summary>
    Public Sub ProcessInput()

        Try

            If Not mConnected Then Return

            SyncLock mBufferLock

                'We need to remove any lines starting with ">>" from
                'the head of the input buffer - there can be multiple ones
                'of these, and we want them all at once.
                While True

                    'See if we have a complete line of text in the input
                    'buffer.
                    Dim index As Integer, line As String
                    index = mInputBuffer.IndexOf(vbLf)
                    If index = -1 Then
                        'Stop if we don't have a line in the buffer at all.
                        Exit While
                    Else

                        'There is a complete line waiting, so copy it into
                        'line...
                        line = mInputBuffer.Left(index)
                        Log.Trace("Talker[{0}:{1}] ProcessInput Line Received : {2}", mHostName, mPort, line)

                        'We are only interested in status updates - anything
                        'else we will ignore...
                        If line.StartsWith(">>") Then
                            ProcessStatusUpdate(line.TrimEnd())

                            'And remove the line from the head of the input
                            'buffer...
                            mInputBuffer = mInputBuffer.Mid(index + 2)
#If DebugOutput Then
                        Debug.WriteLine("LREC_PI: Status Changed : '" & sLine & "'")
#End If

                        Else
                            'There is data in the buffer, but is is no use
                            'to us because it isn't a status update.
                            Exit While
                        End If

                    End If

                End While

            End SyncLock

        Catch ex As Exception
            Log.Info(ex, "Talker[{0}:{1}] An error occurred while processing the input",
             mHostName, mPort)
            ' We should be okay not erroring further - transient errors on the
            ' connection should be picked up by other functions.
#If DebugOutput Then
            Debug.WriteLine("LREC_PI : Disconnected due to exception : " & ex.Message)
#End If
            Log.Trace("Disconnected due to exception {resource}", New With {Key .Hostname = mHostName, Key .Port = mPort, Key .Error = ex.Message})
        End Try

    End Sub

    ''' <summary>
    ''' Gets the sessions currently registered with the resource that this object
    ''' is talking to, along with their current status.
    ''' </summary>
    ''' <returns>A map of session IDs and the current state of those sessions in
    ''' the resource that this object is talking to.</returns>
    Friend Function GetSessions() As IDictionary(Of Guid, RunnerStatus)

        If Not Say("status", "RESOURCE UNIT") Then _
         Throw New BluePrismException("Unable to retrieve resource status")

        Dim map As New clsOrderedDictionary(Of Guid, RunnerStatus)
        Dim line As String = GetNextIncomingLine(TimeSpan.FromSeconds(5))
        Do
            Dim m As Match = StatusLineRegex.Match(line)
            If m.Success Then
                Dim status As RunnerStatus
                clsEnum.TryParse(m.Groups(1).Value, status)
                map(New Guid(m.Groups(2).Value)) = status
            End If
            line = GetNextIncomingLine(TimeSpan.FromSeconds(5))
        Loop While line <> "" AndAlso Not line.StartsWith("Total running:")
        Return map

    End Function

    Friend Function GetNextIncomingLine(timeout As TimeSpan) As String
        Const MaxSleepTime As Integer = 1000
        Dim sleepTime As Integer = 10
        Dim iterationCount As Integer = 0
        Dim timer = Stopwatch.StartNew

        Try
            Do
                iterationCount += 1
                'See if we have a complete line of text in the input
                'buffer.
                Dim line As String = Nothing
                SyncLock mBufferLock
                    line = LineReader.ReadLine(mInputBuffer)
                End SyncLock

                If line IsNot Nothing Then
                    Log.Trace("Talker[{0}:{1}] Next incoming line: {2}", mHostName, mPort, line)
                    'See if the line we received is an unsolicited
                    'status update, or an expected response...
                    If line.StartsWith(">>") Then
                        'A status update, so record the fact the
                        'Resource PC status has changed...
                        ProcessStatusUpdate(line)
                    Else
                        Return line
                    End If
                End If

                ' If we've hit the timeout, return an empty string to indicate a
                ' lack of response within the required time
                If timer.Elapsed > timeout Then
                    Log.Info(
                     "Talker[{0}:{1}] Failed to read next incoming line within {2}",
                     mHostName, mPort, timeout)
                    Return ""
                End If

                If Not mConnected Then
                    Log.Info("Talker[{0}:{1}] Talker has disconnected ", mHostName, mPort)
                    Return String.Empty
                End If


                'Don't hog the processor while waiting for a response
                ' Changed to be dynamic
                Thread.Sleep(sleepTime)
            Loop
        Catch ex As Exception
            Log.Warn(ex,
             "Talker[{0}:{1}] An error occurred getting the next incoming line. " &
             "Assuming that we're disconnected", mHostName, mPort)

            Log.Trace("Disconnected due to exception {resource}", New With {Key .Hostname = mHostName, Key .Port = mPort, Key .Error = ex.Message})
            Return String.Empty
        Finally
            Log.Debug($"{mResourceName} - GetNextIncomingLine took {iterationCount} itterations took {timer.ElapsedMilliseconds}ms, timeout was {timeout}")
        End Try

    End Function


    Private Sub ClearInputBuffer()
        Log.Debug("Clearing input buffer")
        SyncLock mBufferLock
            mInputBuffer = String.Empty
        End SyncLock
    End Sub

    ''' <summary>
    ''' Connect to the remote server.
    ''' </summary>
    ''' <param name="sResourceName">The server (resource PC) name. This
    ''' can either be a standalone PC name, in which case the
    ''' <see cref="ResourceMachine.DefaultPort">default port</see> is assumed, or
    ''' a name:port combination.</param>
    ''' <param name="sErr">On failure, contains an error description.</param>
    ''' <returns>True if successful, False otherwise.</returns>
    Public Function Connect(ByVal sResourceName As String, ByRef sErr As String) As Boolean
        mResourceName = sResourceName
        Log.Debug("Connecting to " & sResourceName)
        Try
            Dim secure = False

            ' This can be improved to just keep the robot record
            Dim r = mRobotAddressStore.GetRobotAddress(sResourceName)
            If r Is Nothing Then Throw New NoSuchResourceException($"Could not find {sResourceName}")
            mPort = r.Port
            mSsl = r.UseSsl
            secure = mRobotAddressStore.RequireSecureResourceConnection
            mHostName = r.HostName

            If Not mSsl AndAlso secure Then
                sErr = My.Resources.clsTalker_ConnectionToUnsecuredResourcesIsDisabled
                GoTo errexit
            End If
        Catch ex As Exception
            Log.Error(ex,
             "Failed getting connection details for resource: {0}",
             sResourceName
            )
        End Try

        mClient = Nothing
        mStream = Nothing

        'Only want to clear the input buffer if we are in ascr mode.  Will not connect correct if old data is present.
        If mAscrOn Then
            ClearInputBuffer()
        End If

        Try
            Dim localHostName = ResourceMachine.GetName(ResourceMachine.DefaultPort)
            mClient = mTcpClientFactory.CreateTcpClient(mHostName, mPort, localHostName)
            If mSsl Then
                mStream = New SslStream(mClient.GetStream(),
                    False,
                    New RemoteCertificateValidationCallback(AddressOf ValidateServerCertificate),
                    Nothing)
                Try
                    Dim ss = CType(mStream, SslStream)
                    ss.AuthenticateAsClient(mHostName)
                Catch ex As AuthenticationException
                    Log.Info(ex,
                     "Talker[{0}:{1}] - Error establishing secure connection: {2}",
                     mHostName, mPort, ex.Message)

                    sErr = String.Format(My.Resources.clsTalker_ErrorEstablishingSecureConnection0, ex.Message())
                    ' Set mConnected flag to true if connected. This will allow the code after errexit to clean up the socket.
                    If mClient?.Connected Then mConnected = True
                    GoTo errexit
                End Try
            Else
                mStream = mClient.GetStream()
            End If

            mConnected = True
            mLastCommunication = Stopwatch.StartNew
            StartReaderThread()

            If mStream.CanWrite Then
                'Send version command...
                Dim cmd = "version"
                Dim sendBytes = Encoding.UTF8.GetBytes(cmd & vbCr)
                Log.Trace("Talker[{0}:{1}] Send Line : {2}", mHostName, mPort, cmd)
                mStream.Write(sendBytes, 0, sendBytes.Length)
            End If

            'Retrieve the response sent by the Resource PC on
            'connection. This will identify the verion of Automate it
            'is running. We want it to be the same as ours...
            Dim t = Stopwatch.StartNew()
            mReply = GetNextIncomingLine(TimeSpan.FromMinutes(2))
            Log.Debug($"Get next incoming line for {sResourceName} took {t.ElapsedMilliseconds}ms")
            Log.Trace("Talker[{0}:{1}] Resource from resource PC {2}", mHostName, mPort, LogOutputHelper.Sanitize(mReply))
            If mReply.StartsWith("AUTOMATE RESOURCE PC") Then
                If mReply.EndsWith(clsListener.ListenerVersion.ToString()) Then
                    'If the resource has been trying to reconnect, then add an event
                    'log entry to say the connection has been re-established
                    If IsAttemptingReconnect() Then
                        Log.Info(
                            "Talker[{0}:{1}] Resource successfully reconnected",
                            mHostName, mPort)
                        ' Now that we have a valid connection, clear the resource 
                        ' reconnection status
                        mReconnectStatus = ResourceReconnectStatus.None
                    End If

                    Return True
                Else
                    sErr = String.Format(My.Resources.clsTalker_CouldNotConnectToTheResourceBecauseItIsRunningAnIncompatibleVersionOf0, ApplicationProperties.ApplicationName)
                    GoTo errexit
                End If
            End If

            Log.Warn("Talker[{0}:{1}] Failed to connect to Resource PC - Reply: '{2}'", mHostName, mPort, LogOutputHelper.Sanitize(mReply))
            'If we fall through to here, the first thing the Resource PC
            'said was not what we expected...
            sErr = My.Resources.clsTalker_FailedToConnectToResourcePC

        Catch ex As Exception
            If WillAttemptReconnectOnFail Then

                'Don't log events for every subsequent reconnection failure
                If Not IsAttemptingReconnect Then
                    'First time a connection fails then add event entry
                    Log.Info(ex,
                        "Talker[{0}:{1}] Error connecting to resource. " &
                        "Attempts to re-establish the connection will occur " &
                        "periodically.", mHostName, mPort)

                    ' Now indicate that we're attempting a reconnect
                    IsAttemptingReconnect = True

                End If

            Else
                'Log all other connection errors
                Log.Info(ex,
                    "Talker[{0}:{1}] Error connecting to resource",
                    mHostName, mPort)

            End If

            sErr = ex.Message

        End Try
errexit:
        Close()
        Return False

    End Function

    ''' <summary>
    ''' Callback for validating the server's certificate at connection time.
    ''' </summary>
    Private Shared Function ValidateServerCertificate(
          sender As Object,
          certificate As X509Certificate,
          chain As X509Chain,
          sslPolicyErrors As SslPolicyErrors) As Boolean
        If sslPolicyErrors = SslPolicyErrors.None Then
            Return True
        End If
        Return False
    End Function

    ''' <summary>
    ''' Asynchronous function for reading underlying network stream.
    ''' The asynchronous reading first begins when the instance is
    ''' created, and this continues once the input has been
    ''' processed, meaning there's always a read in progress.
    ''' </summary>
    Private Sub StartReaderThread()
        Const ioExceptionText = "Talker[{0}:{1}] An IOException occoured while reading the input stream: {2}"
        Const objDisposedText = "Talker[{0}:{1}] An ObjectDisposedException occoured while reading the input stream: {2}"
        Const exceptionText = "Talker[{0}:{1}] An error occurred reading the input stream; Last Reply: {2}; Exception: {3}"
        mCancellationTokenSource = New CancellationTokenSource()
        Task.Run(Async Function()
                     Try

                         Dim bytes As Integer = -1

                         If mStream?.CanRead Then
                             While bytes <> 0 AndAlso Not mCancellationTokenSource.Token.IsCancellationRequested

                                 Dim task = mStream.ReadAsync(mReadBuffer, 0, mReadBuffer.Length, mCancellationTokenSource.Token)
                                 Try
                                     bytes = Await task.WithCancellation(mCancellationTokenSource.Token)
                                     Log.Debug($"tcp>> Talker New incoming data")
                                 Catch OperationCanceledException As OperationCanceledException
                                     If Not task.IsCompleted Then
                                         Log.Debug($"Task canceled handled")
                                     End If
                                     Exit While
                                 Catch ode As ObjectDisposedException
                                     Log.Trace($"ObjectDisposedException {mResourceName} - {ode.Message} Ignore this!")
                                 End Try

                                 Dim message As String
                                 SyncLock mBufferLock
                                     message = Encoding.UTF8.GetString(mReadBuffer, 0, bytes)
                                     mInputBuffer += message
                                     Log.Debug($"tcp>> Read message {message} from stream, mInputBuffer {mInputBuffer}, ThreadId {Thread.CurrentThread.ManagedThreadId}")
                                 End SyncLock
                                 mLastCommunication.Restart()
                                 Thread.Sleep(0)
                             End While
                         End If
                     Catch ex As IOException
                         Log.Debug(ioExceptionText, mHostName, mPort, ex)
                         'IOException during read Is quite normal it simply means
                         ' that the underlying socket was closed by the remote machine
                     Catch ex As ObjectDisposedException
                         Log.Debug(objDisposedText, mHostName, mPort, ex)
                         ' ObjectDisposedException means the NetworkStream is closed or
                         ' there is a failure reading from the network.
                     Catch ex As AggregateException
                         ' handle the above exceptions when bundled as part of an AggregateExcpetion, most often by a Parallel task
                         For Each e In ex.InnerExceptions
                             If TypeOf e Is IOException Then
                                 Log.Debug(ioExceptionText, mHostName, e)
                             ElseIf TypeOf e Is ObjectDisposedException Then
                                 Log.Debug(objDisposedText, mHostName, e)
                             ElseIf e IsNot Nothing Then
                                 Log.Info(exceptionText, mHostName, mPort, GetReply(), ex)
                             End If
                         Next
                     Catch ex As Exception
                         Log.Info(exceptionText, mHostName, mPort, GetReply(), ex)
                         'Something else went wrong reading from the stream, so we just won't
                         'trigger the next read, since it must be dead now.
                     Finally
                         Log.Debug($"Talker: Exiting Stream.ReadAsync {mResourceName}")
                         mCancellationTokenSource?.Dispose()
                         mCancellationTokenSource = Nothing
                     End Try

                 End Function)
    End Sub



    ''' <summary>
    ''' Once connected, this method must be used to register the Automate user making
    ''' the connection with the Resource PC. If this function fails, the connection
    ''' will not be fully functional and should be aborted. A common cause would be
    ''' a Resource PC under the exclusive control of another user. Internally, this
    ''' function uses the Say method, and thus further information on a reason for
    ''' failure may be available via a call to GetReply.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    Public Function Authenticate() As Boolean

        Dim authToken = gSv.RegisterAuthorisationToken()
        Return Say("internalauth " & authToken.ToString(), "AUTH ACCEPTED")

    End Function


    ''' <summary>
    ''' Class used for returning information from GetMembers.
    ''' </summary>
    Public Class MemberInfo
        Public Sub New(ByVal id As Guid, ByVal name As String, ByVal connectionstate As String)
            Me.ID = id
            Me.Name = name
            Me.ConnectionStateString = connectionstate
        End Sub
        Public ID As Guid
        Public Name As String
        Public ConnectionStateString As String
        Public ReadOnly Property ConnectionState As ResourceConnectionState
            Get
                Return clsEnum(Of ResourceConnectionState).Parse(ConnectionStateString)
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Get information about the members of a resource pool from the remote machine,
    ''' which must be the controller of the pool in question! In the event of any
    ''' kind of error, the list will be empty or missing members.
    ''' </summary>
    ''' <returns>A List of MemberInfo objects.</returns>
    Public Function GetMembers() As List(Of MemberInfo)
        Dim lst As New List(Of MemberInfo)
        If Say("members", "MEMBERS", 5000) Then
            Dim rep As String = GetReply()
            Dim count As Integer = Integer.Parse(rep.Substring(10))
            While count > 0
                rep = GetNextIncomingLine(TimeSpan.FromSeconds(5))
                Dim vals() As String = rep.Split(","c)
                If vals.Length = 3 Then
                    lst.Add(New MemberInfo(New Guid(vals(0)), vals(1), vals(2)))
                End If
                count -= 1
            End While
        End If
        Return lst
    End Function

    ''' <summary>
    ''' Get the capabilities of the Resource PC.
    ''' </summary>
    ''' <param name="asCaps">As array to receive the capabilities, each entry being a
    ''' string giving the friendly name for a Business Object.</param>
    ''' <returns>True if successful, False otherwise.</returns>
    Public Function GetCaps(ByRef asCaps() As String) As Boolean

        If Not mStream.CanWrite Then Return False

        'Send the command...
        Dim sCmd As String = "caps" & vbCr
        Dim sendBytes As [Byte]() = System.Text.Encoding.UTF8.GetBytes(sCmd)
        mStream.Write(sendBytes, 0, sendBytes.Length)

        Dim sThisLine As String, sCaps As String
        sCaps = ""
        Do
            sThisLine = GetNextIncomingLine(TimeSpan.FromMinutes(2))
            If sThisLine = "" Then Return False
            If sThisLine = "<EOF>" Then
                Exit Do
            End If
            If sCaps.Length > 0 Then sCaps &= vbCr
            sCaps = sCaps & sThisLine
        Loop
        asCaps = Split(sCaps, vbCr)
        Return True

    End Function

    ''' <summary>
    ''' Send a command to the connected Resource PC. The first line of reply is read
    ''' from the server, and matched with a given expected reply. If it is necessary
    ''' to examine the reply in further detail, a subsequent call to GetReply will
    ''' reveal the content of the first line. Any further lines of response MUST be
    ''' retrieved using GetNextIncomingLine. Generally, commands will have a single
    ''' line response.
    ''' </summary>
    ''' <param name="whatToSay">The command to send - no carriage return is required
    ''' on the end.</param>
    ''' <param name="expectedReplys">An array of strings matched against the first
    ''' line of the reply from the server. The first line must START with the given
    ''' text in order to match.</param>
    ''' <param name="timeout">A timeout (in milliseconds) before it will stop waiting
    ''' for the expected response.</param>
    ''' <returns>True if sucessful and the server response matched the given expected
    ''' reply. False otherwise.</returns>
    Public Function Say(whatToSay As String, expectedReplys As String(), Optional timeout As Integer = 120000) As Boolean
        SyncLock mSayLock
            Try

                If Not mStream.CanWrite Then Return False
                Log.Trace("Talker[{0}:{1}] Say {2}", mHostName, mPort, whatToSay)
                Dim sendBytes = Encoding.UTF8.GetBytes(whatToSay & vbCr)
                mStream.Write(sendBytes, 0, sendBytes.Length)

                mReply = GetNextIncomingLine(TimeSpan.FromMilliseconds(timeout))
                For Each s As String In expectedReplys
                    If mReply.StartsWith(s) Then Return True
                Next

                Return False

            Catch ex As Exception
                Log.Info("{0}:{1} An error occurred while 'Say'ing: {2}; " &
                               "Expected: {3}",
                               mHostName, mPort, whatToSay, String.Join(",", expectedReplys))
                Return False
            End Try
        End SyncLock
    End Function

    ''' <summary>
    ''' Send a command to the connected Resource PC. The first line of reply is read
    ''' from the server, and matched with a given expected reply. If it is necessary
    ''' to examine the reply in further detail, a subsequent call to GetReply will
    ''' reveal the content of the first line. Any further lines of response MUST be
    ''' retrieved using GetNextIncomingLine. Generally, commands will have a single
    ''' line response.
    ''' </summary>
    ''' <param name="sWhatToSay">The command to send - no carriage return is required
    ''' on the end.</param>
    ''' <param name="sExpectedReply">A string matched against the first line of the
    ''' reply from the server. The first line must START with the given text in order
    ''' to match.</param>
    ''' <param name="timeout">A timeout (in milliseconds) before it will stop waiting
    ''' for the expected response.</param>
    ''' <returns>True if sucessful and the server response matched the given expected
    ''' reply. False otherwise.</returns>
    Public Function Say(ByVal sWhatToSay As String, ByVal sExpectedReply As String, Optional ByVal timeout As Integer = 120000) As Boolean
        Return Me.Say(sWhatToSay, New String() {sExpectedReply}, timeout)
    End Function

    ''' <summary>
    ''' Get the first line of response from the last call to Say.
    ''' See the documentation of Say for more information.
    ''' </summary>
    ''' <returns>The first line of server response</returns>
    Public Function GetReply() As String
        Return mReply
    End Function

    ''' <summary>
    ''' Close the connection to the Resource PC.
    ''' </summary>
    Public Sub Close()
        If Not mConnected Then Return

        Log.Trace($"{mResourceName}:Close() down async port")
        mCancellationTokenSource?.Cancel()
        'wait for token to exit the read thread
        '    mExitReadEvent.WaitOne(1000)
        Thread.Yield()

        Try
            Log.Trace("Talker[{0}:{1}] Closing connection", mHostName, mPort)
            If mClient?.Connected AndAlso mStream?.CanWrite Then
                Log.Trace("Talker[{0}:{1}] Send Quit Message", mHostName, mPort)
                Using sr As New StreamWriter(mStream, New UTF8Encoding(False))
                    sr.WriteLine("quit")
                    sr.Flush()
                End Using
            End If
        Catch ex As Exception ' Report, but ultimately ignore, all errors
            Log.Info(ex,
                     "Talker[{0}:{1}] Error while closing connection. Ignoring.",
                     mHostName, mPort)
        Finally

            mStream?.Close()
            mClient?.Close()
            mStream = Nothing
            mClient = Nothing
            mConnected = False

        End Try
    End Sub

    ''' <summary>
    ''' Explicitly disposes of this talker instance
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    ''' <summary>
    ''' Disposes of this talker instance
    ''' </summary>
    ''' <param name="explicit">True if this talker is to be disposed of explicitly as
    ''' part of a direct Dispose() call; False if this talker is being disposed of by
    ''' the garbage collector in its finalization processing</param>
    ''' <remarks>This is currently only ever called explicitly - ie. there is no
    ''' finalizer configured on this class.</remarks>
    Protected Overridable Sub Dispose(ByVal explicit As Boolean)
        If mDisposed OrElse Not explicit Then Return
        Close()
        mDisposed = True
    End Sub

#End Region

End Class
