Imports System.Linq
Imports System.Runtime.Serialization
Imports System.Xml.Linq
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Core.Utility

Namespace WebApis.Authentication
    ''' <summary>
    ''' Contains configuration for OAuth 2.0 authentication within the configuration 
    ''' of a Web API. Note this is specifically OAuth 2.0 using the Client
    ''' Credentials Grant Type.
    ''' </summary>
    <DataContract([Namespace]:="bp"), Serializable>
    Public Class OAuth2ClientCredentialsAuthentication : Implements IAuthentication, ICredentialAuthentication

        <DataMember>
        Private ReadOnly mAuthenticationType As AuthenticationType = AuthenticationType.OAuth2ClientCredentials

        <DataMember>
        Private mCredential As AuthenticationCredential

        <DataMember>
        Private mScope As String

        <DataMember>
        Private mAuthorizationServer As Uri

        ''' <summary>
        ''' Create a new instance of the <see cref="OAuth2ClientCredentialsAuthentication"/> object
        ''' </summary>
        ''' <param name="credential">
        ''' Configures how the credential to use for OAuth 2.0 authentication is 
        ''' identified
        ''' </param>
        Public Sub New(credential As AuthenticationCredential, scope As String,
                       authorizationServer As Uri)

            If credential Is Nothing Then _
                Throw New ArgumentNullException(NameOf(credential))

            If scope Is Nothing Then _
                Throw New ArgumentNullException(NameOf(scope))

            If authorizationServer Is Nothing Then _
             Throw New ArgumentNullException(NameOf(authorizationServer))

            mCredential = credential
            mScope = scope
            mAuthorizationServer = authorizationServer
        End Sub

        ''' <summary>
        ''' Determines the scope of the request made to the authorization server
        ''' when requesting an access token. This in an optional property, but if  
        ''' populated the value should be expressed as a list of space-delimited, 
        ''' case-sensitive strings. Otherwise, it should be an empty string.
        ''' </summary>
        Public ReadOnly Property Scope As String
            Get
                Return mScope
            End Get
        End Property


        ''' <summary>
        ''' The address of Authorization Server to request an access token from
        ''' </summary>
        Public ReadOnly Property AuthorizationServer As Uri
            Get
                Return mAuthorizationServer
            End Get
        End Property

        ''' <summary>
        ''' Information about the credential that stores the client id 
        ''' and client secret that are used to gain the access token from the 
        ''' authorization server
        ''' </summary>
        Public ReadOnly Property Credential As AuthenticationCredential _
            Implements ICredentialAuthentication.Credential
            Get
                Return mCredential
            End Get
        End Property

        ''' <inheritdoc />
        Public ReadOnly Property Type As AuthenticationType Implements IAuthentication.Type
            Get
                Return mAuthenticationType
            End Get
        End Property

        ''' <inheritdoc />
        Public Function ToXElement() As XElement Implements IAuthentication.ToXElement

            Return <authentication
                       type=<%= CInt(Type) %>
                       scope=<%= Scope %>
                       authorizationserver=<%= AuthorizationServer %>>
                       <%= mCredential.ToXElement() %>
                   </authentication>

        End Function

        ''' <summary>
        ''' Create a new instance of <see cref="IAuthentication"/> object from an 
        ''' XML Element that represents that object.
        ''' </summary>
        ''' <param name="element">The xml element to deserialize. An element 
        ''' representing a <see cref="OAuth2ClientCredentialsAuthentication"/> is 
        ''' expected</param>
        ''' <returns>
        ''' A new instance of <see cref="IAuthentication"/> from an XML Element
        ''' that represents that object.
        ''' </returns>
        Public Shared Function FromXElement(element As XElement) As IAuthentication

            If Not element.Name.LocalName.Equals("authentication") Then _
                Throw New MissingXmlObjectException("authentication")

            Dim type = element.
                    Attribute("type")?.
                    Value
            If type Is Nothing Then Throw New MissingXmlObjectException("type")

            Dim scope = element.Attribute("scope")?.Value
            If scope Is Nothing Then Throw New MissingXmlObjectException("scope")

            Dim authorizationServer = element.Attribute("authorizationserver")?.ValueAsUri()
            If authorizationServer Is Nothing Then _
                Throw New MissingXmlObjectException("authorizationserver")

            Dim credentialElement = element.Element("credential")
            Dim credential = If(credentialElement Is Nothing,
                                Nothing,
                                AuthenticationCredential.FromXElement(credentialElement))
            If credential Is Nothing Then Throw New MissingXmlObjectException("credential")

            Return New OAuth2ClientCredentialsAuthentication(credential, scope, authorizationServer)
        End Function

        ''' <inheritdoc/>
        Friend Function GetInputParameters() As IEnumerable(Of ActionParameter) _
            Implements IAuthentication.GetInputParameters

            Return If(mCredential.ExposeToProcess,
                      {New ActionParameter(mCredential.InputParameterName,
                                                My.Resources.Resources.OAuth2ClientCredentialsAuthentication_TheNameOfTheOAuth20ClientCredentialsAuthenticationCredential,
                                                DataType.text,
                                                True,
                                                mCredential.CredentialName)},
                       Enumerable.Empty(Of ActionParameter)())
        End Function


    End Class
End Namespace
