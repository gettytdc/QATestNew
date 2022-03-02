Imports System.Runtime.Serialization
Imports System.Xml.Linq
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Core.Utility

Namespace WebApis.Authentication

    ''' <summary>
    ''' Stores information about a credential used for Web API authentication
    ''' </summary>
    <DataContract([Namespace]:="bp"), Serializable>
    Public Class AuthenticationCredential

        <DataMember>
        Private mCredentialName As String

        <DataMember>
        Private mExposeToProcess As Boolean

        <DataMember>
        Private mInputParameterName As String

        ''' <summary>
        ''' Creates a new AuthenticationCredential
        ''' </summary>
        ''' <param name="credentialName">The name of the credential to use if the 
        ''' credential selection is not exposed to the process or if a credential
        ''' is not specified via the input parameter</param>
        ''' <param name="exposeToProcess">Controls whether the credential selection
        ''' is exposed to the process via an input parameter</param>
        ''' <param name="inputParameterName">The name of the input parameter to use
        ''' if exposed to process</param>
        Public Sub New(credentialName As String, exposeToProcess As Boolean, inputParameterName As String)

            If credentialName Is Nothing Then Throw New ArgumentNullException(NameOf(credentialName))
            If inputParameterName Is Nothing Then Throw New ArgumentNullException(NameOf(inputParameterName))

            mCredentialName = credentialName
            mExposeToProcess = exposeToProcess
            mInputParameterName = inputParameterName
        End Sub

        ''' <summary>
        ''' The name of the credential to use if the credential selection is not 
        ''' exposed to the process or if a credential is not specified via the 
        ''' input parameter
        ''' </summary>
        Public ReadOnly Property CredentialName As String
            Get
                Return mCredentialName
            End Get
        End Property

        ''' <summary>
        ''' Controls whether the credential selection is exposed to the process
        ''' via an input parameter
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ExposeToProcess As Boolean
            Get
                Return mExposeToProcess
            End Get
        End Property

        ''' <summary>
        ''' The name of the input parameter to use if exposed to process
        ''' </summary>
        Public ReadOnly Property InputParameterName As String
            Get
                Return mInputParameterName
            End Get
        End Property

        ''' <inheritdoc />
        Public Function ToXElement() As XElement
            Return <credential
                       credentialname=<%= mCredentialName %>
                       exposetoprocess=<%= mExposeToProcess %>
                       inputparametername=<%= mInputParameterName %>
                   />

        End Function

        ''' <summary>
        ''' Deserializes instance from an XML element
        ''' </summary>
        ''' <param name="element">The XML element from which to deserialize the object</param>
        ''' <returns>The deserialized instance</returns>
        Public Shared Function FromXElement(element As XElement) As AuthenticationCredential

            If Not element.Name.LocalName.Equals("credential") Then _
                Throw New MissingXmlObjectException("credential")

            Dim credentialName = element.Attribute("credentialname")?.Value
            If credentialName Is Nothing Then Throw New MissingXmlObjectException("credentialname")

            Dim exposeToProcess = element.Attribute("exposetoprocess")?.Value(Of Boolean)
            If exposeToProcess Is Nothing Then Throw New MissingXmlObjectException("exposetoprocess")

            Dim inputParameterName = element.Attribute("inputparametername")?.Value
            If inputParameterName Is Nothing Then Throw New MissingXmlObjectException("inputparametername")

            Return New AuthenticationCredential(credentialName, exposeToProcess.Value, inputParameterName)

        End Function

        ''' <inheritdoc/>
        Public Overrides Function Equals(obj As Object) As Boolean
            Dim credential = TryCast(obj, AuthenticationCredential)
            If credential Is Nothing Then Return False

            Return Equals(credential)
        End Function

        ''' <summary>
        ''' Determines whether the specified <see cref="AuthenticationCredential"/>
        ''' object is equal to the current object
        ''' </summary>
        '''<param name="obj">The object to compare with the current object.</param>
        ''' <returns>
        ''' <see langword="true" /> if the specified object  is equal to the current 
        ''' object; otherwise, <see langword="false" />.</returns>
        Public Overloads Function Equals(obj As AuthenticationCredential) As Boolean
            If obj Is Nothing Then Return False

            Return obj.CredentialName = CredentialName AndAlso
                    obj.ExposeToProcess = ExposeToProcess AndAlso
                    obj.InputParameterName = InputParameterName
        End Function

        ''' <inheritdoc/>
        Public Overrides Function GetHashCode() As Integer
            Return CredentialName.GetHashCode() Xor
                ExposeToProcess.GetHashCode() Xor
                InputParameterName.GetHashCode()
        End Function



    End Class

End Namespace
