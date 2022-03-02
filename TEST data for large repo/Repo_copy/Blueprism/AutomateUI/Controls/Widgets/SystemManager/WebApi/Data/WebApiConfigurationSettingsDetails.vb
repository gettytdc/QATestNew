Imports BluePrism.AutomateProcessCore.WebApis

Public Class WebApiConfigurationSettingsDetails

    Public Property HttpRequestConnectionTimeout As Integer
    Public Property AuthServerRequestConnectionTimeout As Integer

    Private Sub New()
    End Sub

    Friend Shared Function InitialiseWithDefaults() As WebApiConfigurationSettingsDetails
        Return CreateFrom(New WebApiConfigurationSettings())
    End Function

    Friend Shared Function CreateFrom(configurationSettings As WebApiConfigurationSettings) As WebApiConfigurationSettingsDetails
        Dim details = New WebApiConfigurationSettingsDetails()
        ApplySettings(details, configurationSettings)
        Return details
    End Function

    Private Shared Sub ApplySettings(details As WebApiConfigurationSettingsDetails, settings As WebApiConfigurationSettings)
        details.HttpRequestConnectionTimeout = settings.HttpRequestConnectionTimeout
        details.AuthServerRequestConnectionTimeout = settings.AuthServerRequestConnectionTimeout
    End Sub

    Friend Shared Function MapTo(configurationSettings As WebApiConfigurationSettingsDetails) As WebApiConfigurationSettings
        Return New WebApiConfigurationSettings(
            configurationSettings.HttpRequestConnectionTimeout,
            configurationSettings.AuthServerRequestConnectionTimeout)
    End Function

    Public Shared Sub ResetWithDefaults(original As WebApiConfigurationSettingsDetails)
        ApplySettings(original, New WebApiConfigurationSettings())
    End Sub
End Class
