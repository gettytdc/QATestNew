Namespace Controls.Widgets.SystemManager.WebApi

    Public Delegate Sub CustomCodeChangedEventHandler(sender As Object, e As CustomCodeControlEventArgs)

    Public Class CustomCodeControlEventArgs : Inherits EventArgs

        Public Sub New(newCodeContent As String)
            CodeContent = newCodeContent
        End Sub

        ''' <summary>
        ''' The updated body content configuration
        ''' </summary>
        Public ReadOnly CodeContent As String

    End Class

End Namespace
