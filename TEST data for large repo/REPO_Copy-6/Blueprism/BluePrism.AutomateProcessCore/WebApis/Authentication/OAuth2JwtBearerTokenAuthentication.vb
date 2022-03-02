Imports System.Linq
Imports System.Runtime.Serialization
Imports System.Xml.Linq
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Core.Utility

Namespace WebApis.Authentication
    ''' <summary>
    ''' Contains configuration for OAuth2 authentication with JWT Bearer Token grant 
    ''' type within the configuration of a Web API
    ''' </summary>
    <DataContract([Namespace]:="bp"), Serializable>
    Public Class OAuth2JwtBearerTokenAuthentication : Implements IAuthentication, ICredentialAuthentication

        <DataMember>
        Private ReadOnly mAuthenticationType As AuthenticationType = AuthenticationType.OAuth2JwtBearerToken

        <DataMember>
        Private ReadOnly mJwtConfiguration As JwtConfiguration

        <DataMember>
        Private mAuthorizationServer As Uri

        Public Sub New(jwtConfiguration As JwtConfiguration, authorizationServer As Uri)

            If jwtConfiguration Is Nothing Then _
                Throw New ArgumentNullException(NameOf(jwtConfiguration))


            If authorizationServer Is Nothing Then _
                Throw New ArgumentNullException(NameOf(authorizationServer))

            mJwtConfiguration = jwtConfiguration
            mAuthorizationServer = authorizationServer

        End Sub

        Public ReadOnly Property Type As AuthenticationType Implements IAuthentication.Type
            Get
                Return AuthenticationType.OAuth2JwtBearerToken
            End Get
        End Property

        Public ReadOnly Property JwtConfiguration As JwtConfiguration
            Get
                Return mJwtConfiguration
            End Get
        End Property

        Public ReadOnly Property AuthorizationServer As Uri
            Get
                Return mAuthorizationServer
            End Get
        End Property

        Public ReadOnly Property Credential As AuthenticationCredential _
            Implements ICredentialAuthentication.Credential
            Get
                Return mJwtConfiguration.Credential
            End Get
        End Property

        ''' <summary>
        ''' Create a new instance of <see cref="IAuthentication"/> object from an 
        ''' XML Element that represents that object.
        ''' </summary>
        ''' <param name="element">The xml element to deserialize. An element 
        ''' representing a <see cref="OAuth2JwtBearerTokenAuthentication"/> is 
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

            Dim jwtConfigElement = element.Element("jwtconfiguration")
            Dim jwtConfig = If(jwtConfigElement Is Nothing,
                                        Nothing,
                                        JwtConfiguration.FromXElement(jwtConfigElement))
            If jwtConfig Is Nothing Then Throw New MissingXmlObjectException("jwtconfiguration")

            Dim authorizationServer = element.Attribute("authorizationserver")?.ValueAsUri()
            If authorizationServer Is Nothing Then _
                Throw New MissingXmlObjectException("authorizationserver")

            Return New OAuth2JwtBearerTokenAuthentication(jwtConfig, authorizationServer)
        End Function

        ''' <inheritdoc/>
        Public Function ToXElement() As XElement Implements IAuthentication.ToXElement
            Return <authentication
                       type=<%= CInt(Type) %>
                       authorizationserver=<%= AuthorizationServer %>>
                       <%= JwtConfiguration.ToXElement() %>
                   </authentication>

        End Function

        ''' <inheritdoc/>
        Public Function GetInputParameters() As IEnumerable(Of ActionParameter) Implements IAuthentication.GetInputParameters
            Return If(mJwtConfiguration.Credential.ExposeToProcess,
                      {New ActionParameter(mJwtConfiguration.Credential.InputParameterName,
                                                My.Resources.Resources.OAuth2JwtBearerTokenAuthentication_TheNameOfTheOAuth20JWTBearerTokenAuthenticationCredential,
                                                DataType.text,
                                                True,
                                                mJwtConfiguration.Credential.CredentialName)},
                       Enumerable.Empty(Of ActionParameter)())
        End Function
    End Class
End Namespace
