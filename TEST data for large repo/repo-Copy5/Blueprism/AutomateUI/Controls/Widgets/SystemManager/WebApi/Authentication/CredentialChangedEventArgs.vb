Imports BluePrism.AutomateProcessCore.WebApis.Authentication

Namespace Controls.Widgets.SystemManager.WebApi.Authentication

    Public Delegate Sub CredentialChangedEventHandler(sender As Object, e As CredentialChangedEventArgs)

    ''' <summary>
    ''' Describes an event where the credential in a Web API authentication 
    ''' configuration changes
    ''' </summary>
    Public Class CredentialChangedEventArgs : Inherits EventArgs

        ''' <summary>
        ''' Creates a new CredentialChangedEventArgs
        ''' </summary>
        ''' <param name="credential">The updated credential configuration</param>
        Public Sub New(credential As AuthenticationCredential)
            Me.Credential = credential
        End Sub

        ''' <summary>
        ''' The updated credential configuration
        ''' </summary>
        Public ReadOnly Credential As AuthenticationCredential

    End Class
End NameSpace