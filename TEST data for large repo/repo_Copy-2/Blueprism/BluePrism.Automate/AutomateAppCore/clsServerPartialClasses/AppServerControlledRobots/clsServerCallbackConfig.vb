Imports System.Security.Cryptography.X509Certificates

Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.ClientServerResources.Core.Config
Imports BluePrism.ClientServerResources.Core.Enums
Imports BluePrism.Common.Security
Imports BluePrism.Server.Domain.Models

Partial Public Class clsServer

    Private Shared mConfig As ConnectionConfig

    <SecuredMethod>
    Friend Function GetCallbackConnectionConfig() As ConnectionConfig Implements IServer.GetCallbackConnectionConfig
        CheckPermissions()
        If mConfig Is Nothing Then
            Log.Warn($"ascr config should not be null. Has {NameOf(InitCallbackConfig)} been called?")
            Throw New InvalidProgramException(ClientServerResources.Core.Properties.Resources.UnableToRetrieveConfiguration)
        End If
        Return mConfig
    End Function

    <UnsecuredMethod>
    Public Sub InitCallbackConfig(config As ConnectionConfig) Implements IServerPrivate.InitCallbackConfig
        If mLoggedInUser.AuthType <> AuthMode.System Then Throw New PermissionException()
        If config Is Nothing Then Throw New ArgumentNullException(NameOf(config))

        Try
            If config.Mode = InstructionalConnectionModes.Certificate Then
                ValidateConfigCertificates(config)
                config.ClientCertificate = GetCallbackClientCertificate(config)
            End If
        Catch ex As Exceptions.CertificateException
            Throw
        Catch ex As Exception
            Log.Error(ex, "Error occurred trying to validate certificates.")
            Throw
        End Try

        mConfig = config
    End Sub

    <UnsecuredMethod>
    Public Function GetCallbackConfigProtocol() As CallbackConnectionProtocol Implements IServer.GetCallbackConfigProtocol
        Return mConfig.CallbackProtocol
    End Function

    Private Function GetCallbackClientCertificate(config As ConnectionConfig) As SafeString
        Dim certName = config.ClientCertificateName
        Try
            If String.IsNullOrWhiteSpace(certName) Then Throw New ArgumentNullException(ClientServerResources.Core.Properties.Resources.NoCertificateNameProvided)

            Dim certStore = New CertificateStoreService()
            Dim cert As X509Certificate2 = certStore.GetCertificateByName(config.ClientCertificateName, config.ClientStore)

            Dim bytes = cert.Export(X509ContentType.Pfx)

            Return New SafeString(Convert.ToBase64String(bytes))
        Catch ex As Exception
            Log.Error(ex, "Failed to pull client callback certificate from certificate store.")
            Throw
        End Try
        Return Nothing
    End Function

    Private Sub ValidateConfigCertificates(config As ConnectionConfig)
        Dim store = New CertificateStoreService()

        Using serverCert = store.GetCertificateByName(config.CertificateName, config.ServerStore)
            Using clientCert = store.GetCertificateByName(config.ClientCertificateName, config.ClientStore)
                store.ValidateCert(serverCert, CertificateStoreCheckFlags.PrivateKey)
                store.ValidateCert(clientCert, CertificateStoreCheckFlags.PrivateKey)
            End Using
        End Using
    End Sub


End Class
