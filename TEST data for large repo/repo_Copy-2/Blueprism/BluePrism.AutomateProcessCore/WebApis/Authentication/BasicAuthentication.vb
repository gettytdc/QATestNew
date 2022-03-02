Imports System.Linq
Imports System.Runtime.Serialization
Imports System.Xml.Linq
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Core.Utility

Namespace WebApis.Authentication
    ''' <summary>
    ''' Contains configuration for basic authentication within the configuration of 
    ''' a Web API
    ''' </summary>
    <DataContract([Namespace]:="bp"), Serializable>
    Public Class BasicAuthentication : Implements IAuthentication, ICredentialAuthentication

        <DataMember>
        Private ReadOnly mAuthenticationType As AuthenticationType = AuthenticationType.Basic

        <DataMember>
        Private mCredential As AuthenticationCredential

        <DataMember>
        Private mIsPreEmptive As Boolean

        ''' <summary>
        ''' Create a new instance of the <see cref="BasicAuthentication"/> object
        ''' </summary>
        ''' <param name="credential">Configures how the credential to use for basic
        ''' authentication is identified</param>
        ''' <param name="isPreEmptive"></param>
        Public Sub New(credential As AuthenticationCredential, isPreEmptive As Boolean)

            If credential Is Nothing Then
                Throw New ArgumentNullException(NameOf(credential))
            End If

            mCredential = credential
            mIsPreEmptive = isPreEmptive
        End Sub

        ''' <summary>
        ''' Determines whether an Authorize header will be included with the HTTP 
        ''' request. If not specified, authentication details will be supplied in
        ''' response to a 401 challenge.
        ''' </summary>
        Public ReadOnly Property IsPreEmptive As Boolean
            Get
                Return mIsPreEmptive
            End Get

        End Property

        ''' <summary>
        ''' Configures how the credential to use for basic authentication is 
        ''' identified
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
                       preemptive=<%= mIsPreEmptive %>>
                       <%= mCredential.ToXElement() %>
                   </authentication>

        End Function

        ''' <summary>
        ''' Create a new instance of <see cref="IAuthentication"/> object from an 
        ''' XML Element that represents that object.
        ''' </summary>
        ''' <param name="element">The xml element to deserialize. An element 
        ''' representing a <see cref="BasicAuthentication"/> is 
        ''' expected.</param>
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

            Dim preemptive = element.Attribute("preemptive")?.Value(Of Boolean)
            If preemptive Is Nothing Then Throw New MissingXmlObjectException("preemptive")

            Dim credentialElement = element.Element("credential")
            Dim credential = If(credentialElement Is Nothing,
                                Nothing,
                                AuthenticationCredential.FromXElement(credentialElement))
            If credential Is Nothing Then Throw New MissingXmlObjectException("credential")

            Return New BasicAuthentication(credential, preemptive.Value)
        End Function


        ''' <inheritdoc/>
        Friend Function GetInputParameters() As IEnumerable(Of ActionParameter) Implements IAuthentication.GetInputParameters
            Return If(mCredential.ExposeToProcess,
                      {New ActionParameter(mCredential.InputParameterName,
                                                My.Resources.Resources.BasicAuthentication_TheNameOfTheBasicAuthenticationCredential,
                                                DataType.text,
                                                True,
                                                mCredential.CredentialName)},
                       Enumerable.Empty(Of ActionParameter)())
        End Function


    End Class
End Namespace
