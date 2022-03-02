Public Class IncompatibleApiException : Inherits Exception

    Public ClientVersion As ApiVersion
    Public ServerVersion As ApiVersion

    Public Sub New(clientVersion As String, serverVersion As String)
        Me.New(ApiVersion.Parse(clientVersion), ApiVersion.Parse(serverVersion))
    End Sub

    Public Sub New(clientVersion As ApiVersion, serverVersion As ApiVersion)
        MyBase.New(String.Format(My.Resources.IncompatibleApiException_ClientVersionIsNotCompatibleWithServerVersion, clientVersion.ToFriendlyString, serverVersion.ToFriendlyString))
        Me.ClientVersion = clientVersion
        Me.ServerVersion = serverVersion
    End Sub

    Public ReadOnly Property Detail As String
        Get
            Return String.Format(My.Resources.IncompatibleApiException_DetailClientVersionServerVersion, ClientVersion.ToString, ServerVersion.ToString)
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return String.Format("{1}: {2}{0}{3}", Environment.NewLine, NameOf(IncompatibleApiException), Me.Detail, Me.StackTrace)
    End Function
End Class
