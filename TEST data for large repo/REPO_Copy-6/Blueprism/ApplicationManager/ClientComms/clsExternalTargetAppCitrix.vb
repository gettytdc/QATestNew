Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Net.Sockets
Imports System.Threading
Imports System.Threading.Tasks
Imports BluePrism.BPCoreLib
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class clsExternalTargetAppCitrix
    Inherits clsExternalTargetApp

    Public Shared Property AppMans As New Dictionary(Of Guid, clsExternalTargetApp)
    Private Shared mClient As TcpClient
    Private Shared mWriter As StreamWriter
    Private ReadOnly mAppManId As Guid = Guid.NewGuid()
    Private ReadOnly mCitrixMode As CitrixAppManMessage.CitrixMode

    Private mIsPrimaryInstance As Boolean = False

    Protected Overrides ReadOnly Property Client As TcpClient
        Get
            Return mClient
        End Get
    End Property

    Protected Overrides ReadOnly Property Writer As StreamWriter
        Get
            Return mWriter
        End Get
    End Property


    Public Sub New(mode As ProcessMode)
        Try
            mCitrixMode = GetCitrixMode(mode)
            ConnectToProcess()
        Catch ex As Exception When _
            TypeOf ex Is SocketException AndAlso TryCast(ex, SocketException).ErrorCode = SocketError.ConnectionRefused
            mClient = Nothing
            'This error indicates that the virtual driver is not present, ideally we need a different message below
            Throw New InvalidOperationException(String.Format(My.Resources.AppManStartupProblem0,
                                                              My.Resources.TimedOutWaitingForPipeConnection), ex)
        Catch ex As Exception When _
            TypeOf ex Is SocketException OrElse TypeOf ex Is TimeoutException
            mClient = Nothing
            Throw New InvalidOperationException(String.Format(My.Resources.AppManStartupProblem0,
                                                              My.Resources.TimedOutWaitingForPipeConnection), ex)
        Catch ex As ArgumentOutOfRangeException
            Throw New InvalidOperationException(String.Format(My.Resources.AppManStartupProblem0, My.Resources.NoDataReceived), ex)
        End Try
    End Sub

    Private Shared Function GetCitrixMode(mode As ProcessMode) As CitrixAppManMessage.CitrixMode
        Select Case mode
            Case ProcessMode.Citrix32
                Return CitrixAppManMessage.CitrixMode.Citrix32
            Case ProcessMode.Citrix64
                Return CitrixAppManMessage.CitrixMode.Citrix64
            Case Else
                Throw New InvalidOperationException(My.Resources.UnexpectedProcessModeInClsExternalTargetAppCitrix)
        End Select

    End Function

    Protected Overrides Sub ConnectToProcess()
        Dim port As Integer? = 31926

        If mClient Is Nothing Then
            mIsPrimaryInstance = True
            mClient = New TcpClient With {
                .NoDelay = True,
                .SendTimeout = 1500,
                .ReceiveTimeout = 0
                }
            mClient.Connect("localhost", port.Value)

            If Not mClient.Connected Then Throw New SocketException()

            AppMans.Add(mAppManId, Me)

            mWriter = New StreamWriter(mClient.GetStream(), New UTF8Encoding(False)) With {
                .AutoFlush = True
                }

            'Start the reader thread...
            Task.Run(Sub() ReadThread())
        ElseIf Not AppMans.ContainsKey(mAppManId) Then
            AppMans.Add(mAppManId, Me)
        End If

    End Sub

    Public Overrides Function ProcessQuery(sQuery As String, timeout As TimeSpan) As String
        Dim stopwatch = New Stopwatch()
        Dim result As String = Nothing
        stopwatch.Start()

        Dim message = New CitrixAppManMessage(sQuery, mAppManId, mCitrixMode)
        Dim messageToSend = JsonConvert.DeserializeObject(Of JObject)(JsonConvert.SerializeObject(message)).ToString(Formatting.None)

        mWriter.WriteLine(messageToSend)

        While (stopwatch.Elapsed < timeout OrElse timeout = TimeSpan.Zero)
            SyncLock QueueLock
                Monitor.Wait(QueueLock, 25)
                If IncomingLines.Count > 0 Then
                    result = IncomingLines.Dequeue()
                    Exit While
                End If
            End SyncLock
            MessagePump()
        End While

        stopwatch.Stop()

        Return ProcessQueryResponse(result, stopwatch.Elapsed, timeout)
    End Function

    Protected Overrides Function ParseHeader(ByRef line As String) As clsExternalTargetApp
        Dim appManId As Guid
        If line.StartsWith("AppManId") Then
            appManId = Guid.Parse(line.Substring(9, 36))
            line = line.Substring(9 + 36)
            If AppMans.ContainsKey(appManId) Then
                Return AppMans(appManId)
            End If
        End If

        Return Nothing
    End Function

    Protected Overrides Sub FinaliseDisconnect()
        SyncLock QueueLock
            IncomingLines.Enqueue("OK")
            Monitor.PulseAll(QueueLock)
        End SyncLock
        AppMans.Remove(mAppManId)

        If Not AppMans.Any Then
            If mClient?.Connected Then
                mClient.Close()
            End If
            If mWriter?.BaseStream.CanWrite Then
                mWriter.Close()
            End If
            mWriter = Nothing
            mClient = Nothing
        End If
    End Sub

    Protected Overrides Sub Dispose(disposingExplicitly As Boolean)
        If IsDisposed Then Return

        If mWriter?.BaseStream.CanWrite Then
            mWriter.WriteLine($"ForceKill AppManId {mAppManId}")
        End If

        Dim disposingAsPartOfFinaliser = Not disposingExplicitly
        If disposingAsPartOfFinaliser Then
            mWriter?.Dispose()
            mClient?.Dispose()
        End If

        If Not mIsPrimaryInstance OrElse Not AppMans.Any Then
            MyBase.Dispose(disposingExplicitly)
        End If
    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class
