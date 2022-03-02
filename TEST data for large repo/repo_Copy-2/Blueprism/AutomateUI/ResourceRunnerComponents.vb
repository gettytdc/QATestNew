Imports BluePrism.AutomateAppCore.Resources

Public Class ResourceRunnerComponents
    Implements IDisposable
    Public ReadOnly View As IResourcePCView
    Public ReadOnly Runner As IResourceRunner

    Public Sub New(view As IResourcePCView, runner As IResourceRunner)
        Me.View = view
        Me.Runner = runner
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        View.Dispose()
        Runner.Dispose()
    End Sub
End Class