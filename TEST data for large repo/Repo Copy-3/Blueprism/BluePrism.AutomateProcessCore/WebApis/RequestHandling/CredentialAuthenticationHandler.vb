Imports System.Net
Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports BluePrism.AutomateProcessCore.WebApis.Credentials

Namespace WebApis.RequestHandling
    ''' <summary>
    ''' Abstract class that defines an authentication handler that uses a credential
    ''' to store the data that uniquely identifies who is making the Web API Request
    ''' </summary>
    ''' <typeparam name="TAuthentication">The type of authentication that this 
    ''' handler handles. The type must be a class that implements both the
    ''' <see cref="IAuthentication"/> and <see cref="ICredentialAuthentication"/>
    ''' interfaces.</typeparam>
    Public MustInherit Class CredentialAuthenticationHandler _
        (Of TAuthentication As {IAuthentication, ICredentialAuthentication, Class})
        Implements IAuthenticationHandler

        Private ReadOnly mCredentialHelper As IAuthenticationCredentialHelper

        Sub New(credentialHelper As IAuthenticationCredentialHelper)
            mCredentialHelper = credentialHelper
        End Sub

        ''' <inheritdoc/>
        Public MustOverride ReadOnly Property RetryAttemptsOnUnauthorizedException As Integer _
            Implements IAuthenticationHandler.RetryAttemptsOnUnauthorizedException

        ''' <inheritdoc/>
        Public MustOverride Sub Handle(request As HttpWebRequest, context As ActionContext) _
            Implements IAuthenticationHandler.Handle

        ''' <inheritdoc/>
        Public MustOverride Sub BeforeRetry(context As ActionContext) _
            Implements IAuthenticationHandler.BeforeRetry

        ''' <inheritdoc/>
        Public Overridable Function GetCredentialParameters(context As ActionContext) As Dictionary(Of String, clsProcessValue) _
            Implements IAuthenticationHandler.GetCredentialParameters
            Return New Dictionary(Of String, clsProcessValue)
        End Function

        ''' <inheritdoc/>
        Public Function CanHandle(auth As IAuthentication) As Boolean _
            Implements IAuthenticationHandler.CanHandle
            Return TryCast(auth, TAuthentication) IsNot Nothing
        End Function

        ''' <summary>
        ''' Get the credential from the credential store that will be used to 
        ''' uniquely identify who is making this Web API request
        ''' </summary>
        ''' <param name="context">The information about the Web API request that is
        ''' being made</param>
        ''' <returns>The credential that identitifes who is making the Web API request
        ''' </returns>
        Protected Function GetCredential(context As ActionContext) As ICredential
            Dim authentication = TryCast(context.Configuration.CommonAuthentication, TAuthentication)
            If authentication Is Nothing Then _
                Throw New ArgumentException(String.Format(My.Resources.Resources.CredentialAuthenticationHandler_0DidNotExpect1, Me.GetType(), authentication.GetType()))
            Return mCredentialHelper.GetCredential(authentication.Credential, 
                                                   context.Parameters, 
                                                   context.SessionId)
        End Function
    End Class

End Namespace


