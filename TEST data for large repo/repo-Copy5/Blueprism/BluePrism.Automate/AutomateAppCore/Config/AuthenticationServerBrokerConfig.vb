Imports BluePrism.Common.Security

Public Class AuthenticationServerBrokerConfig

    Public Sub New(brokerAddress As String, brokerUsername As String, brokerPassword As SafeString, environmentIdentifier As String)
        If String.IsNullOrWhiteSpace(brokerAddress) Then Throw New ArgumentNullException(NameOf(brokerAddress))
        If String.IsNullOrWhiteSpace(brokerUsername) Then Throw New ArgumentNullException(NameOf(brokerUsername))
        If brokerPassword.SecureString Is Nothing OrElse brokerPassword.SecureString.Length = 0 Then Throw New ArgumentNullException(NameOf(brokerPassword))
        If String.IsNullOrWhiteSpace(environmentIdentifier) Then Throw New ArgumentNullException(NameOf(environmentIdentifier))

        Me.BrokerAddress = brokerAddress
        Me.BrokerUsername = brokerUsername
        Me.BrokerPassword = brokerPassword
        Me.EnvironmentIdentifier = environmentIdentifier
    End Sub

    Public ReadOnly Property BrokerAddress As String

    Public ReadOnly Property BrokerUsername As String

    Public ReadOnly Property BrokerPassword As SafeString

    Public ReadOnly Property EnvironmentIdentifier As String
End Class
