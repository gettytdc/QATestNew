Imports System.IO
Imports System.Net.Sockets
Imports System.Threading
Imports System.Collections.Generic
Imports BluePrism.CharMatching
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.BPCoreLib
Imports BluePrism.Server.Domain.Models

Public MustInherit Class clsExternalTargetApp
    Inherits clsTargetApp

    ' Flag indicating if a status has been received which may cause a disconnect
    Private mExpectDisconnect As Boolean = False

    ' The process ID of the actual target application
    Private mTargetPID As Integer = 0

    Private Const ReadTimeOut As Integer = 2000

    ' Flag indicating if the last query timed out when waiting for a reply, and the
    ' app man process is still processing the query.
    Protected mAppManTimedOut As Boolean = False

    Public ReadOnly Property QueueLock As New Object()
    Public ReadOnly Property IncomingLines As New Queue(Of String)

    ''' <summary>
    ''' Get the PID of the target application currently connected.
    ''' </summary>w
    Public Overrides ReadOnly Property PID() As Integer
        Get
            If mTargetPID = 0 Then
                Dim result As String = ProcessQuery("getpid")
                If result.StartsWith("RESULT:") Then
                    mTargetPID = Integer.Parse(result.Substring(7))
                Else
                    Debug.Assert(False, "Failed to read pid - " & result)
                    mTargetPID = 0
                End If
            End If
            Return mTargetPID
        End Get
    End Property
    Protected MustOverride ReadOnly Property Client() As TcpClient
    Protected MustOverride ReadOnly Property Writer() As StreamWriter

    Protected MustOverride Function ParseHeader(ByRef line As String) As clsExternalTargetApp


    ''' <summary>
    ''' Process a query.
    ''' </summary>
    ''' <param name="sQuery">The query to process.</param>
    ''' <returns>The result of the query.</returns>
    ''' <remarks>Low-level logging is performed at this stage if requested.</remarks>
    Public Overrides Function ProcessQuery(sQuery As String) As String
        Return ProcessQuery(sQuery, TimeSpan.Zero)
    End Function

    Protected Function ProcessQueryResponse(result As String, elapsedTime As TimeSpan, timeout As TimeSpan) As String

        If result Is Nothing AndAlso elapsedTime >= timeout _
            AndAlso timeout <> TimeSpan.Zero Then
            ' timed out
            mAppManTimedOut = True
            result = "ERROR:" & My.Resources.ConfiguredTimeoutReachedWaitingForExternalApplicationToRespond
        End If

        If result Is Nothing Then
            ' On termination, the read thread can leave a final message before
            ' closing; see if there's one there
            If IncomingLines.Count > 0 Then
                result = IncomingLines.Dequeue()
            End If

            ' If not, we return an error.
            If result Is Nothing Then
                result = "ERROR:" & My.Resources.ExternalAppManReaderThreadExitedBeforeResponseFromQuery
            End If
        End If

        Return BPUtil.Unescape(result)
    End Function

    ''' <summary>
    ''' Use Process StandardOutput to determine the port to connect to
    ''' </summary>
    ''' <exception cref="OperationFailedException">If the response does not meet 'Waiting for a connection on port: X</exception>
    ''' <exception cref="ArgumentOutOfRangeException">If the parsed response does not fit regex rule '^[A-Za-z\s]+:\s\d+'</exception>
    Protected MustOverride Sub ConnectToProcess()

    ''' <summary>
    ''' Process incoming data. Async events are raised as events, and normal input
    ''' is placed in the incoming queue, as it must be query responses.
    ''' </summary>
    Protected Sub ReadThread()
        Try
            Using reader = New StreamReader(Client.GetStream(), Encoding.UTF8)
                reader.BaseStream.ReadTimeout = ReadTimeOut
                While True
                    Dim line As String
                    Try
                        If reader.EndOfStream Then
                            Return
                        End If
                        line = reader.ReadLine()

                    Catch ex As Exception
                        ' The client is closed and the reader disposed of when the target
                        ' app is disposed; ensure that the crash is handled gracefully,
                        ' otherwise, ensure that the exception is logged correctly
                        Dim socketException = TryCast(ex.InnerException, SocketException)
                        If socketException IsNot Nothing Then
                            If socketException.SocketErrorCode = SocketError.ConnectionReset Then
                                Return
                            End If
                            If Not IsDisposed AndAlso (socketException.SocketErrorCode = SocketError.TimedOut) Then
                                Continue While
                            End If
                        End If
                        If IsDisposed Then
                            Return
                        Else
                            Throw
                        End If
                    End Try

                    If IsDisposed OrElse Not reader.BaseStream.CanRead Then
                        Return
                    End If
                    Dim currentTargetApp = ParseHeader(line)

                    If currentTargetApp Is Nothing Then
                        Continue While
                    End If
                    If line IsNot Nothing Then
                        ProcessLine(line, currentTargetApp)
                    End If
                End While

            End Using
        Catch ex As Exception
            clsConfig.Log("Exception during ReadThread:{0}", ex.Message)
        Finally
            OnDisconnected()
            FinaliseDisconnect()
            ' If we're exiting the read thread for any reason, ensure that
            ' we wake anyone waiting on the incoming lines queue
            SyncLock QueueLock
                ' If a terminate or disconnect message was encountered, and there's
                ' no response waiting, ensure that we indicate that this disconnect
                ' was expected by giving an 'OK' response, otherwise the loop awaiting
                ' a response may wait forever.
                If mExpectDisconnect AndAlso IncomingLines.Count = 0 Then
                    IncomingLines.Enqueue("OK")
                End If
                Monitor.PulseAll(QueueLock)
            End SyncLock
        End Try
    End Sub

    Private Sub ProcessLine(line As String, currentTargetApp As clsExternalTargetApp)

        If line.StartsWith(">>") Then
            Select Case line.Substring(2)
                Case "ExpectDisconnect"
                    currentTargetApp.mExpectDisconnect = True
                Case "Disconnected"
                    currentTargetApp.OnDisconnected()
                    currentTargetApp.FinaliseDisconnect()
                Case Else
                    If line.StartsWith(">>fontload ") Then
                        Try
                            Dim font As FontData =
                                    FontReader.GetFontData(line.Substring(11))
                            currentTargetApp.Writer.WriteLine(
                                "$FONTDATA {0}", font.GetXML(True))
                        Catch
                            currentTargetApp.Writer.WriteLine("$FONTDATA")
                        End Try
                    End If
            End Select
        Else
            SyncLock currentTargetApp.QueueLock
                'If the query that this reply is for has timed out, don't add it to the incoming messages queue - nobody is waiting for it.
                ' But do set the timeout flag back to false, since the external process is now responding again.
                If mAppManTimedOut = False Then
                    currentTargetApp.IncomingLines.Enqueue(line)
                    Monitor.PulseAll(currentTargetApp.QueueLock)
                Else
                    mAppManTimedOut = False
                End If

            End SyncLock
        End If
    End Sub

    Protected MustOverride Sub FinaliseDisconnect()

End Class
