Imports System.Net
Imports BluePrism.AutomateProcessCore.WebApis.Authentication

Namespace WebApis.RequestHandling

    ''' <summary>
    ''' Makes no authentication-based changes during HTTP requests made when executing a 
    ''' Web API action. Used if no authentication behaviour applies.
    ''' </summary>
    Public Class EmptyAuthenticationHandler : Implements IAuthenticationHandler

        ''' <inheritdoc />
        Public Function CanHandle(authentication As IAuthentication) As Boolean _
            Implements IAuthenticationHandler.CanHandle

            Return authentication.Type = AuthenticationType.None

        End Function

        ''' <inheritdoc />
        Public Sub Handle(request As HttpWebRequest, context As ActionContext) _
            Implements IAuthenticationHandler.Handle

        End Sub

        ''' <inheritdoc/>
        Public ReadOnly Property RetryAttemptsOnUnauthorizedException As Integer _
            Implements IAuthenticationHandler.RetryAttemptsOnUnauthorizedException
            Get
                Return 0
            End Get
        End Property

        ''' <inheritdoc/>
        Public Sub BeforeRetry(context As ActionContext) Implements IAuthenticationHandler.BeforeRetry
            Throw New NotImplementedException()
        End Sub

        ''' <inheritdoc/>
        Public Function GetCredentialParameters(context As ActionContext) As Dictionary(Of String, clsProcessValue) _
            Implements IAuthenticationHandler.GetCredentialParameters
            Return New Dictionary(Of String, clsProcessValue)
        End Function
    End Class
End NameSpace