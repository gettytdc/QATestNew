Namespace Resources

    Public Class DigitalWorkerContext
        Sub New(startUpOptions As DigitalWorkerStartUpOptions)
            If startUpOptions Is Nothing Then Throw New ArgumentNullException(NameOf(startUpOptions))
            Me.StartUpOptions = startUpOptions
        End Sub

        Public ReadOnly Property StartUpOptions As DigitalWorkerStartUpOptions
    End Class

End Namespace

