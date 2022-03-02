Imports System.Reflection
Imports System.Windows.Forms

Public Class ConfigManager : Inherits MarshalByRefObject : Implements IDisposable

    Public ReadOnly Property IsAlive() As Boolean
        Get
            Return True
        End Get
    End Property

    Public Sub Save(ByVal cfg As MachineConfig)
        cfg.Save()
    End Sub

    Public Sub [Exit]()
        Try
            ' If we are inside automateconfig, exit the application (the daemon which is
            ' hosting this config manager)
            If Assembly.GetEntryAssembly().FullName.StartsWith("AutomateConfig") Then
                Application.Exit()
            End If
        Catch
        End Try
    End Sub

#Region " IDisposable Support "

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Private mDisposed As Boolean

    Public Overridable Sub Dispose(disposing As Boolean)
        If mDisposed Then Return
        Try
            Me.Exit()
        Catch
        End Try
        mDisposed = True
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub

#End Region

End Class