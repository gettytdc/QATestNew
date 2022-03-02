Imports BluePrism.Common.Security

<Serializable()>
Public Class RabbitMqConfiguration
    Public Sub New(hostUrl As String, username As String, password As SafeString)
        If String.IsNullOrEmpty(hostUrl) Then
            Throw New ArgumentNullException(NameOf(hostUrl))
        End If

        If String.IsNullOrEmpty(username) Then
            Throw New ArgumentNullException(NameOf(username))
        End If

        If password Is Nothing Then
            Throw New ArgumentNullException(NameOf(password))
        End If

        Me.HostUrl = hostUrl
        Me.Username = username
        Me.Password = password
    End Sub

    Public ReadOnly Property HostUrl As String

    Public ReadOnly Property Username As String

    Public ReadOnly Property Password As SafeString

    Public Overrides Function Equals(obj As Object) As Boolean
        Dim config = TryCast(obj, RabbitMqConfiguration)
        If config Is Nothing Then Return False
        Return HostUrl.Equals(config.HostUrl) AndAlso Username.Equals(config.Username) AndAlso Password.Equals(config.Password)
    End Function

    Public Overrides Function GetHashCode() As Integer
        Return HostUrl.GetHashCode() Xor Username.GetHashCode() Xor Password.GetHashCode()
    End Function


End Class
