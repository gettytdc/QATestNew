Imports System.Linq
Imports System.Runtime.Serialization
Imports System.Xml.Linq
Imports BluePrism.Server.Domain.Models

Namespace WebApis.Authentication

    ''' <summary>
    ''' The configuration that is used when no authentication is specified for a Web 
    ''' API
    ''' </summary>
    <DataContract([Namespace]:="bp"), Serializable>
    Public Class EmptyAuthentication : Implements IAuthentication

        <DataMember>
        Private ReadOnly mAuthenticationType As AuthenticationType = AuthenticationType.None


        ''' <inheritdoc />
        Public ReadOnly Property Type As AuthenticationType Implements IAuthentication.Type
            Get
                Return mAuthenticationType
            End Get
        End Property

        ''' <inheritdoc />
        Friend Function GetInputParameters() As IEnumerable(Of ActionParameter) Implements IAuthentication.GetInputParameters
            Return Enumerable.Empty(Of ActionParameter)()
        End Function

        ''' <inheritdoc />
        Public Function ToXElement() As XElement Implements IAuthentication.ToXElement
            Return <authentication type=<%= CInt(Type) %>/>
        End Function

        ''' <summary>
        ''' Create a new instance of <see cref="IAuthentication"/> object from an 
        ''' XML Element that represents that object.
        ''' </summary>
        ''' <param name="element">The xml element to deserialize. An element 
        ''' representing a <see cref="EmptyAuthentication"/> is 
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

            Return New EmptyAuthentication()
        End Function

    End Class
End Namespace
