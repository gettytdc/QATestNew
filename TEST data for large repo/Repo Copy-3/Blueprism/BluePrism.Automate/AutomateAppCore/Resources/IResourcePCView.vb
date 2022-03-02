Imports System.Windows.Forms

Namespace Resources
    Public Interface IResourcePCView
        Inherits IDisposable

        Event RefreshStatus(Sender As Object, e As EventArgs)

        Event RestartRequested(Sender As Object, e As EventArgs)

        Event ShutdownRequested(Sender As Object, e As EventArgs)

        Event CloseRequested(Sender As Object, e As FormClosingEventArgs)

        Sub Init()

        Sub DisplayStatus(activeSessions As Integer, pendingSessions As Integer, connectionStatus As String)

        Sub DisplayNotification(notification As ResourceNotification)

        Function ConfirmRestart(activeSessionCount As Integer) As Boolean

        Sub DisplayRestarting()

        Function ConfirmShutdown(activeSessionCount As Integer) As Boolean

        Sub DisplayShuttingDown()

        ReadOnly Property FormDialogResult As DialogResult
        Sub ShowForm()

        Sub CloseForm()

        Sub RunOnUIThread(action As Action)

        Sub BeginRunOnUIThread(action As Action)
    End Interface
End NameSpace