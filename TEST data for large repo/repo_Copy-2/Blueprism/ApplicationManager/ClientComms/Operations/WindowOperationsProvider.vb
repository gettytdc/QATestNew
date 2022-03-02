Imports BluePrism.BPCoreLib
Imports BluePrism.Server.Domain.Models

Namespace Operations
    ''' <inheritDoc/>
    Public Class WindowOperationsProvider : Implements IWindowOperationsProvider

        ''' <inheritDoc/>
        Public Sub ForceForeground(windowHandle As IntPtr) Implements IWindowOperationsProvider.ForceForeground

            Try
                ' Very quick shortcut test - if the window we want to use is the current
                ' foreground, just exit now; no need to spawn a new process
                If windowHandle = GetForegroundWindow() Then Return

                Dim bits As Integer ' The bit-ness of the current foreground process

                ' Get the foreground process and check if it is 64 bit
                Using fgProc As Process = BPUtil.GetForegroundProcess()
                    If fgProc Is Nothing Then Throw New OperationFailedException(
                     My.Resources.CouldNotIdentifyProcessOwningTheCurrentForegroundWindow)
                    If BPUtil.Is64BitProcess(fgProc) Then bits = 64 Else bits = 32
                End Using

                ' Now call Activator32.exe or Activator64.exe to activate the window
                ' that we want to activate.
                Using proc As New Process()
                    With proc.StartInfo
                        .FileName = String.Format("Activator{0}.exe", bits)
                        .Arguments = windowHandle.ToInt64().ToString()
                        .UseShellExecute = False
                        .CreateNoWindow = True
                    End With
                    proc.Start()

                    ' Wait for 10 seconds, though there's not a lot we can do if it does
                    ' time out...
                    If proc.WaitForExit(10 * 1000) Then
                        ' Get the response code and test it... again, there's not a lot
                        ' we can do at this point, so we just report it
                        Dim resp As Integer = proc.ExitCode
                        Select Case resp
                            Case 0 ' Success
                            Case 1 : Debug.Print("Checked error in Activator{0}", bits)
                            Case 2 : Debug.Print("Unchecked error in Activator{0}", bits)
                            Case 3 : Debug.Print("Fatal error in Activator{0}", bits)
                        End Select
                    Else
                        Debug.Print("Activator{0} timed out", bits)
                    End If

                End Using

            Catch ofe As OperationFailedException ' Just rethrow these
                Throw

            Catch ex As Exception ' Wrap into an OpFailedException
                Throw New OperationFailedException(
                 My.Resources.ExceptionDuringRemoteSetforegroundwindow0, ex, ex.ToString())

            End Try
        End Sub
    End Class
End Namespace
