Imports System.Linq
Imports System.Runtime.Serialization
Imports System.Xml.Linq
Imports BluePrism.Server.Domain.Models

Namespace WebApis.Authentication
    ''' <summary>
    ''' Contains configuration for custom authentication within the configuration of 
    ''' a Web API
    ''' </summary>
    <DataContract([Namespace]:="bp"), Serializable>
    Public Class CustomAuthentication : Implements IAuthentication, ICredentialAuthentication

        <DataMember>
        Private ReadOnly mAuthenticationType As AuthenticationType = AuthenticationType.Custom

        <DataMember>
        Private mCredential As AuthenticationCredential

        ''' <summary>
        ''' Create a new instance of the <see cref="CustomAuthentication"/> object
        ''' </summary>
        ''' <param name="credential">Configures how the credential to use for custom
        ''' authentication is identified</param>
        Public Sub New(credential As AuthenticationCredential)

            If credential Is Nothing Then _
                Throw New ArgumentNullException(NameOf(credential))

            mCredential = credential
        End Sub

        ''' <summary>
        ''' Configures how the credential to use for custom authentication is 
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
                       type=<%= CInt(Type) %>>
                       <%= mCredential.ToXElement() %>
                   </authentication>

        End Function

        ''' <summary>
        ''' Create a new instance of <see cref="IAuthentication"/> object from an 
        ''' XML Element that represents that object.
        ''' </summary>
        ''' <param name="element">The xml element to deserialize. An element 
        ''' representing a <see cref="CustomAuthentication"/> is 
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

            Dim credentialElement = element.Element("credential")
            Dim credential = If(credentialElement Is Nothing,
                                Nothing,
                                AuthenticationCredential.FromXElement(credentialElement))
            If credential Is Nothing Then Throw New MissingXmlObjectException("credential")

            Return New CustomAuthentication(credential)
        End Function

        ''' <inheritdoc/>
        Public Function GetInputParameters() As IEnumerable(Of ActionParameter) Implements IAuthentication.GetInputParameters
            Return If(mCredential.ExposeToProcess,
                      {New ActionParameter(mCredential.InputParameterName,
                                                My.Resources.Resources.CustomAuthentication_TheNameOfTheCustomCredential,
                                                DataType.text,
                                                True,
                                                mCredential.CredentialName)},
                       Enumerable.Empty(Of ActionParameter)())
        End Function
    End Class
End Namespace



