Imports System.Runtime.Serialization
Imports System.Xml.Linq
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Core.Utility
Imports Microsoft.IdentityModel.Tokens

Namespace WebApis.Authentication

    ''' <summary>
    ''' Class that contains the configuration that allows a JWT (JSON Web Token) to 
    ''' be generated, which can be used within one of the Web API authentication types.
    ''' See  https://tools.ietf.org/html/rfc7519 for the JWT spec.
    ''' </summary>
    <DataContract([Namespace]:="bp"), Serializable>
    Public Class JwtConfiguration

        <DataMember>
        Private ReadOnly mAlgorithm As String = SecurityAlgorithms.RsaSha256

        <DataMember>
        Private ReadOnly mAudience As String

        <DataMember>
        Private ReadOnly mScope As String

        <DataMember>
        Private ReadOnly mSubject As String

        <DataMember>
        Private ReadOnly mJwtExpiry As Integer

        <DataMember>
        Private ReadOnly mCredential As AuthenticationCredential

        ''' <summary>
        ''' Create a new instance of the <see cref="JwtConfiguration"/> object
        ''' </summary>
        ''' <param name="audience">The value that populates the JWT's "aud" claim</param>
        ''' <param name="scope">The value that populates the JWT's "scope" claim</param>
        ''' <param name="subject">The value that populates the JWT's "sub" claim</param>
        ''' <param name="jwtExpiry">The value in seconds, that the JWT is valid for</param>
        ''' <param name="credential">Configures how the credential to use with this 
        ''' JWT configuration is identified</param>
        Public Sub New(audience As String, scope As String, subject As String,
                       jwtExpiry As Integer, credential As AuthenticationCredential)

            If audience Is Nothing Then _
                Throw New ArgumentNullException(NameOf(audience))

            If scope Is Nothing Then _
                Throw New ArgumentNullException(NameOf(scope))

            If subject Is Nothing Then _
             Throw New ArgumentNullException(NameOf(subject))

            If credential Is Nothing Then _
                Throw New ArgumentNullException(NameOf(credential))

            mAudience = audience
            mScope = scope
            mSubject = subject
            mJwtExpiry = jwtExpiry
            mCredential = credential

        End Sub

        ''' <summary>
        ''' The signing algorithm used to sign the JWT. Currently hard-coded to 
        ''' RsaSha256.
        ''' </summary>
        Public ReadOnly Property Algorithm As String
            Get
                Return mAlgorithm
            End Get
        End Property

        ''' <summary>
        ''' The intended recipients of this JWT. 
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Audience As String
            Get
                Return mAudience
            End Get
        End Property

        ''' <summary>
        ''' A space-delimited list of the permissions that JWT token is attempting to
        ''' gain access to. This is used to populate the "aud" claim when generating 
        ''' the JWT. This is not a part of the JWT spec and is a custom claim
        ''' used specifically for authenticating Google APIs.
        ''' </summary>
        Public ReadOnly Property Scope As String
            Get
                Return mScope
            End Get
        End Property

        ''' <summary>
        ''' The principal that is the subject of the JWT. For example, in Google APIs
        ''' this is the email address of the user for which the application is 
        ''' requesting delegated access. This is used to populate the "sub" claim 
        ''' when generating the JWT. 
        ''' </summary>
        Public ReadOnly Property Subject As String
            Get
                Return mSubject
            End Get
        End Property

        ''' <summary>
        ''' The value in seconds, that the JWT (once generated) is valid for
        ''' </summary>
        Public ReadOnly Property JwtExpiry As Integer
            Get
                Return mJwtExpiry
            End Get
        End Property

        ''' <summary>
        ''' Configures how the credential to use with this JWT configuration is 
        ''' identified. This credential will contain: 
        ''' 1) The Issuer field - this identifies the client that issued the JWT
        ''' and will populate the "iss" claim in the JWT.
        ''' 2) A Private Key - this contains a private key used to sign the JWT.
        ''' </summary>
        Public ReadOnly Property Credential As AuthenticationCredential
            Get
                Return mCredential
            End Get
        End Property

        ''' <summary>
        ''' Create a new instance of <see cref="JwtConfiguration"/> object from an 
        ''' XML Element that represents that object.
        ''' </summary>
        ''' <param name="element">The xml element to deserialize</param>
        ''' <returns>
        ''' A new instance of <see cref="JwtConfiguration"/> from an XML Element
        ''' that represents that object.
        ''' </returns>
        Public Shared Function FromXElement(element As XElement) As JwtConfiguration

            If Not element.Name.LocalName.Equals("jwtconfiguration") Then _
                Throw New MissingXmlObjectException("jwtconfiguration")

            Dim audience = element.Attribute("audience")?.Value
            If audience Is Nothing Then _
                Throw New MissingXmlObjectException("audience")

            Dim scope = element.Attribute("scope")?.Value
            If scope Is Nothing Then Throw New MissingXmlObjectException("scope")

            Dim subject = element.Attribute("subject")?.Value
            If subject Is Nothing Then Throw New MissingXmlObjectException("subject")

            Dim jwtExpiry = element.Attribute("jwtexpiry")?.Value(Of Integer)
            If jwtExpiry Is Nothing Then Throw New MissingXmlObjectException("jwtexpiry")


            Dim credentialElement = element.Element("credential")
            Dim credential = If(credentialElement Is Nothing,
                                Nothing,
                                AuthenticationCredential.FromXElement(credentialElement))
            If credential Is Nothing Then Throw New MissingXmlObjectException("credential")

            Return New JwtConfiguration(audience, scope, subject,
                                                   jwtExpiry.GetValueOrDefault(), credential)
        End Function

        ''' <summary>
        ''' Serialize this <see cref="JwtConfiguration"/> object to an XElement.
        ''' </summary>
        ''' <returns> This object serialized with XML </returns>
        Public Function ToXElement() As XElement
            Return <jwtconfiguration
                       algorithm=<%= Algorithm %>
                       audience=<%= Audience %>
                       scope=<%= Scope %>
                       subject=<%= Subject %>
                       jwtexpiry=<%= JwtExpiry %>
                       <%= mCredential.ToXElement() %>>
                   </jwtconfiguration>

        End Function



    End Class

End Namespace
