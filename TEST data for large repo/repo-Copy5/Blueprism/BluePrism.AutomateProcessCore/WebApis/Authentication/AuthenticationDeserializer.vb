Imports System.Xml.Linq
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Core.Utility

Namespace WebApis.Authentication

    ''' <summary>
    ''' Module used to deserialize an XElement serialization of an implementation of 
    ''' the <see cref="IAuthentication"/> interface.
    ''' </summary>
    Public Module AuthenticationDeserializer

        ''' <summary>
        ''' Deserialize the given XML String to the
        ''' <see cref="IAuthentication"/> object it represents.
        ''' </summary>
        ''' <param name="xml">The xml string to deserialize
        ''' is being deserialized</param>
        ''' <returns>A deserialized <see cref="IAuthentication"/> object</returns>
        Public Function Deserialize(xml As String) As IAuthentication
            Return Deserialize(XElement.Parse(xml))
        End Function

        ''' <summary>
        ''' Deserialize the given <see cref="XElement"/> to the
        ''' <see cref="IAuthentication"/> object it represents.
        ''' </summary>
        ''' <param name="element">The xml element to deserializr
        ''' is being deserialized</param>
        ''' <returns>A deserialized <see cref="IAuthentication"/> object</returns>
        Public Function Deserialize(element As XElement) As IAuthentication

            If Not element.Name.LocalName.Equals("authentication") Then _
                Throw New MissingXmlObjectException("authentication")

            Dim typeAttribute = element.Attribute("type")
            If typeAttribute Is Nothing Then Throw New MissingXmlObjectException("value")

            Dim authType = typeAttribute.Value.ParseEnum(Of AuthenticationType)

            Select Case authType
                Case AuthenticationType.None
                    Return EmptyAuthentication.FromXElement(element)
                Case AuthenticationType.Basic
                    Return BasicAuthentication.FromXElement(element)
                Case AuthenticationType.BearerToken
                    Return BearerTokenAuthentication.FromXElement(element)
                Case AuthenticationType.OAuth2ClientCredentials
                    Return OAuth2ClientCredentialsAuthentication.FromXElement(element)
                Case AuthenticationType.OAuth2JwtBearerToken
                    Return OAuth2JwtBearerTokenAuthentication.FromXElement(element)
                Case AuthenticationType.Custom
                    Return CustomAuthentication.FromXElement(element)
                Case Else
                    Throw New NotImplementedException(
                        $"Deserialize not implemented for {authType.ToString()}")
            End Select

        End Function

    End Module

End Namespace
