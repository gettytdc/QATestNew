Imports BluePrism.ClientServerResources.Core.Interfaces

Imports NLog

Public Class TokenRegistration
    Implements ITokenRegistration

    Private ReadOnly mLog As Logger = LogManager.GetCurrentClassLogger()

    Public Function RegisterTokenWithExpiry(expiryTime As Integer) As String Implements ITokenRegistration.RegisterTokenWithExpiry
        Try
            Dim token = gSv.RegisterAuthorisationTokenWithExpiryTime(expiryTime)
            Return token.ToString()
        Catch ex As Exception
            mLog.Debug(ex, $"Exception registring a new token")
            Throw
        End Try
    End Function
End Class
