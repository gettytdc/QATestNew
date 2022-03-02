Imports BluePrism.AutomateProcessCore.WebApis.Authentication

Namespace Controls.Widgets.SystemManager.WebApi.Authentication

    Public Delegate Sub AuthenticationChangedEventHandler(sender As Object, e As AuthenticationChangedEventArgs)

    ''' <summary>
    ''' Describes an event where the authentication of a Web Api configuration changes
    ''' </summary>
    Public Class AuthenticationChangedEventArgs : Inherits EventArgs

        Public Sub New(auth As IAuthentication)
            Authentication = auth
        End Sub

        ''' <summary>
        ''' The updated authentication configuration
        ''' </summary>
        Public ReadOnly Authentication As IAuthentication

    End Class
End NameSpace