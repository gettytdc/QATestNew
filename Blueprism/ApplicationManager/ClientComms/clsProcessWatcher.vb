Imports System.Threading
Imports BluePrism.BPCoreLib
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities

''' <summary>
''' Class to manage a process handle, and the thread that waits for the process to
''' terminate.
''' </summary>
Friend Class clsProcessWatcher
    Implements IDisposable

    ''' <summary>
    ''' Event fired when the process is no longer connected.
    ''' </summary>
    Public Event Disconnected()

    ''' <summary>
    ''' An unmanaged Handle to the process
    ''' </summary>
    Private mHandle As IntPtr

    ''' <summary>
    ''' Reference to the thread which observes the termination of the target process.
    ''' </summary>
    Private mWaitThread As Thread

    ''' <summary>
    ''' Handle used to signal to the wait thread that it should stop observing the
    ''' target process
    ''' </summary>
    Private mWaitHandle As IntPtr

    Private mWaitHandleBrowser As IntPtr = IntPtr.Zero

    ''' <summary>
    ''' Lock to ensure that the handle is not used and closed at the same time.
    ''' </summary>
    Private mHandleLock As New Object

    ''' <summary>
    ''' Shared method to create a new process watcher from an unmanaged process handle
    ''' </summary>
    ''' <param name="processHandle">The unmanaged process handle to monitor</param>
    Friend Shared Function FromHandle(ByVal processHandle As IntPtr) As clsProcessWatcher
        Return New clsProcessWatcher(processHandle)
    End Function

    ''' <summary>
    ''' Constructor, creates the monitoring thread.
    ''' </summary>
    ''' <param name="processHandle">The unmanaged process handle to monitor</param>
    Private Sub New(ByVal processHandle As IntPtr)
        mHandle = processHandle
        mWaitHandle = CreateEvent(IntPtr.Zero, True, False, String.Empty)
        mWaitHandleBrowser = CreateEvent(IntPtr.Zero, True, False, String.Empty)
        mWaitThread = New Thread(AddressOf WaitThread)
        mWaitThread.Start(mHandle)
    End Sub

    ''' <summary>
    ''' Provides access to the unmanaged process handle
    ''' </summary>
    Friend ReadOnly Property Handle() As IntPtr
        Get
            Return mHandle
        End Get
    End Property

    Friend ReadOnly Property GetBrowserHandle() As IntPtr
        Get
            Return mWaitHandleBrowser
        End Get
    End Property

    ''' <summary>
    ''' Waits for the process with the specified handle to terminate, and then closes
    ''' the process handle.
    ''' </summary>
    Private Sub WaitThread(ByVal processHandle As Object)
        Dim exited As Boolean = False
        Try
            Dim p As IntPtr = CType(processHandle, IntPtr)
            Dim h() As IntPtr = {p, mWaitHandle, mWaitHandleBrowser}
            While True
                Dim resp As WaitResults = CType(WaitForMultipleObjects(3, h, False, 1000), WaitResults)
                Select Case resp
                    Case WaitResults.WAIT_OBJECT_0
                        exited = True
                        Return
                    Case WaitResults.WAIT_OBJECT_1
                        Return
                    Case WaitResults.WAIT_OBJECT_2
                        exited = True
                        Return
                    Case WaitResults.WAIT_TIMEOUT
                        'Do nothing, just continue looping
                    Case WaitResults.WAIT_FAILED
                        clsConfig.LogWin32("WaitForSingleObject failed with return value WAIT_FAILED. " & GetLastWin32Error())
                        Return
                    Case Else
                        clsConfig.LogWin32("Unexpected return value from WaitForSingleObject - " & resp & ". " & GetLastWin32Error())
                        Return
                End Select
            End While
        Catch 'Do nothing, just allow to exit gracefully
        Finally
            CloseHandles(exited)
        End Try
    End Sub

    ''' <summary>
    ''' Closes the process handle the wait handle, and raises the disconnect event
    ''' if the application exited on its own.
    ''' </summary>
    Private Sub CloseHandles(exited As Boolean)
        SyncLock mHandleLock
            CloseHandle(mHandle)
            mHandle = IntPtr.Zero
        End SyncLock

        CloseHandle(mWaitHandle)
        mWaitHandle = IntPtr.Zero

        If mWaitHandleBrowser <> IntPtr.Zero Then
            CloseHandle(mWaitHandleBrowser)
            mWaitHandleBrowser = IntPtr.Zero
        End If

        If exited Then
            RaiseEvent Disconnected()
        End If
    End Sub

    ''' <summary>
    ''' Terminates the application, if it is launched.
    ''' </summary>
    Friend Sub Terminate()
        SyncLock mHandleLock
            If Connected Then
                If Not TerminateProcess(mHandle, 0) Then
                    Throw New InvalidOperationException(My.Resources.FailedToTerminateApplication & GetLastWin32Error())
                End If
            Else
                Throw New InvalidOperationException(My.Resources.NotConnected)
            End If
        End SyncLock
    End Sub

    ''' <summary>
    ''' Detaches the application, by signaling the wait thread to end, and waiting
    ''' for the thread to terminate thus closing the handle.
    ''' </summary>
    Friend Sub Detach()
        SetEvent(mWaitHandle)
        If Connected Then
            Try
                mWaitThread.Join(30000)
            Catch tse As ThreadStateException
                ' the thread has already exited - a perfectly reasonable state
            End Try

            RaiseEvent Disconnected()
        End If
    End Sub

    ''' <summary>
    ''' Provide access to whether the process is connected.
    ''' </summary>
    Private ReadOnly Property Connected() As Boolean
        Get
            SyncLock mHandleLock
                Return mHandle <> IntPtr.Zero
            End SyncLock
        End Get
    End Property

    ''' <summary>
    ''' Dispose method, ensures thread is detached when necessary.
    ''' </summary>
    ''' <param name="disposing"></param>
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Connected Then
            Detach()
        End If
    End Sub

#Region " IDisposable Support "
    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class
