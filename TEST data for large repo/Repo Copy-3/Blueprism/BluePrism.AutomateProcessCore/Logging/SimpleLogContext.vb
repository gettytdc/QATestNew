Public Class SimpleLogContext
    Implements IProcessLogContext

    Public Sub New(process As clsProcess)
        Me.Process = process
    End Sub

    Public ReadOnly Property Process As clsProcess Implements IProcessLogContext.Process
End Class
