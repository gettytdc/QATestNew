Public Class ActivationEventArgs : Inherits EventArgs
    Private mTarget As Object
    Public Sub New()
        Me.New(Nothing)
    End Sub
    Public Sub New(ByVal target As Object)
        mTarget = target
    End Sub
    Public ReadOnly Property Target() As Object
        Get
            Return mTarget
        End Get
    End Property
End Class
