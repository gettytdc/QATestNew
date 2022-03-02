Public Class ErrorEventArgs
    Inherits EventArgs

    Private ReadOnly mErrorMessage As String

    Public Sub New(message As String)
        mErrorMessage = message
    End Sub

    Public ReadOnly Property Message As String
        Get
            Return mErrorMessage
        End Get

    End Property


End Class
