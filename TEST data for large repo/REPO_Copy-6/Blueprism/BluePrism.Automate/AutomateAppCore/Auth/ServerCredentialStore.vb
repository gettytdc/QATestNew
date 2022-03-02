Imports BluePrism.AutomateProcessCore.WebApis.Credentials


''' <summary>
''' Loads credentials from server
''' </summary>
Public Class ServerCredentialStore : Implements ICredentialStore

    Private ReadOnly mServer As IServer

    ''' <summary>
    ''' Creates a new ServerCredentialStore
    ''' </summary>
    ''' <param name="server">The server used to load credentials</param>
    Sub New(server As IServer)
        mServer = server
    End Sub

    ''' <inheritdoc/>
    Public Function GetCredential(name As String, sessionId As Guid) As ICredential _
        Implements ICredentialStore.GetCredential

        Return mServer.RequestCredential(sessionId, name)
    End Function

End Class
