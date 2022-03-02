Imports System.Collections.Generic
Imports System.IO
Imports System.Net.Sockets
Imports System.Threading
Imports BluePrism.BPCoreLib
Imports BluePrism.Core.Extensions

Public Class clsExternalTargetAppLocal
    Inherits clsExternalTargetApp

    ''' <summary>
    ''' Our external application manager process.
    ''' </summary>
    Private mProcess As Process
    ''' <summary>
    ''' TCP connection to our external AppMan.exe.
    ''' </summary>
    Private mClient As TcpClient
    Private mWriter As StreamWriter
    Private mReadThread As Thread

    Protected Overrides ReadOnly Property Client() As TcpClient
        Get
            Return mClient
        End Get
    End Property

    Protected Overrides ReadOnly Property Writer() As StreamWriter
        Get
            Return mWriter
        End Get
    End Property

    Public Sub New(mode As ProcessMode)
        'Select correct suffix for AppMan.exe according to CPU architecture
        'requested...
        Dim suffix As String
        Select Case mode
            Case ProcessMode.Ext32bit
                suffix = "32"
            Case ProcessMode.Ext64bit
                suffix = "64"
            Case ProcessMode.ExtNativeArch
                suffix = ""
            Case ProcessMode.ExtSameArch
                If IntPtr.Size = 8 Then
                    suffix = "64"
                Else
                    suffix = "32"
                End If
            Case Else
                Throw New InvalidOperationException(My.Resources.UnexpectedProcessModeInClsExternalTargetApp)
        End Select

        Dim appManFileName As String = Nothing
        Try
            mProcess = New Process()
            appManFileName = "AppMan" & suffix & ".exe"
            With mProcess.StartInfo
                .FileName = appManFileName
                .UseShellExecute = False
                .CreateNoWindow = True
                .RedirectStandardOutput = True
                .RedirectStandardError = True
            End With
            mProcess.Start()
        Catch ex As Exception
            Throw New InvalidOperationException(String.Format(My.Resources.AppManStartupProblemCouldNotStart0, appManFileName), ex)
        End Try

        Try
            ConnectToProcess()
        Catch ex As Exception When _
            TypeOf ex Is SocketException OrElse TypeOf ex Is TimeoutException
            Throw New InvalidOperationException(String.Format(My.Resources.AppManStartupProblem0,
                                                              My.Resources.TimedOutWaitingForPipeConnection), ex)
        Catch ex As ArgumentOutOfRangeException
            Throw New InvalidOperationException(String.Format(My.Resources.AppManStartupProblem0, My.Resources.NoDataReceived), ex)
        End Try

    End Sub

    Protected Overrides Sub ConnectToProcess()
        Dim port As Integer?

        Using stdout As StreamReader = mProcess.StandardOutput
            Using stderr As StreamReader = mProcess.StandardError

                Dim line As String = stdout.ReadLine()
                If line Is Nothing AndAlso stdout.EndOfStream Then line = stderr.ReadToEnd()

                If line Is Nothing Then Throw New InvalidOperationException(String.Format(
                    My.Resources.AppManStartupProblem0,
                    My.Resources.NoDataReceived))

                port = line.GetInt()

                If Not port.HasValue Then Throw New InvalidOperationException(String.Format(
                    My.Resources.AppManStartupProblem0,
                    line))
            End Using
        End Using

        If mClient Is Nothing Then

            mClient = New TcpClient With {
                .NoDelay = True,
                .SendTimeout = 1500,
                .ReceiveTimeout = 0
                }
            mClient.Connect("localhost", port.Value)


            If Not mClient.Connected Then Throw New SocketException()

            mWriter = New StreamWriter(mClient.GetStream(), New UTF8Encoding(False)) With {
                .AutoFlush = True
                }

            'Start the reader thread...
            mReadThread = New Thread(New ThreadStart(AddressOf ReadThread))
            mReadThread.Start()
        End If

    End Sub

    Public Overrides Function ProcessQuery(sQuery As String, timeout As TimeSpan) As String
        Dim stopwatch = New Stopwatch()
        Dim result As String = Nothing
        stopwatch.Start()

        If Not mReadThread.IsAlive Then
            Return "ERROR:" & My.Resources.ConnectionToTargetApplicationLostReadThreadIsNoLongerAlive
        End If

        If mAppManTimedOut Then
            Return "ERROR:" & My.Resources.ConfiguredTimeoutReachedWaitingForExternalApplicationToRespond
        End If

        mWriter.WriteLine(sQuery)

        While mReadThread.IsAlive AndAlso
              (stopwatch.Elapsed < timeout OrElse timeout = TimeSpan.Zero)
            SyncLock QueueLock
                Monitor.Wait(QueueLock, 25)
                If IncomingLines.Count > 0 Then
                    result = IncomingLines.Dequeue()
                    Exit While
                End If
            End SyncLock
            MessagePump()
        End While

        Return ProcessQueryResponse(result, stopwatch.Elapsed, timeout)

    End Function

    Protected Overrides Function ParseHeader(ByRef line As String) As clsExternalTargetApp
        Return Me 'Do Nothing
    End Function

    Protected Overrides Sub FinaliseDisconnect()
        Return 'Do Nothing
    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If IsDisposed Then Return
        Try
            If mClient IsNot Nothing Then
                mClient.Close()
            End If

            mWriter?.Dispose()

            If mProcess IsNot Nothing Then
                '' Wait for a max of 3s for the process to exit
                If Not mProcess.WaitForExit(3000) Then mProcess.Kill()
                mProcess.Dispose()
                mProcess = Nothing
            End If
        Catch
            'No error to be thrown during dispose
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub
End Class
