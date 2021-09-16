Imports System.Linq
Imports System.Runtime.Serialization
Imports System.Xml.Linq
Imports BluePrism.Server.Domain.Models

Namespace WebApis.RequestHandling.BodyContent

    <DataContract([Namespace]:="bp"), Serializable>
    Public Class NoBodyContent
        Implements IBodyContent

        Private Const mBodyType As WebApiRequestBodyType = WebApiRequestBodyType.None

        Public ReadOnly Property Type As WebApiRequestBodyType Implements IBodyContent.Type
            Get
                Return mBodyType
            End Get
        End Property

        ''' <summary>
        ''' Create a new instance of <see cref="IBodyContent"/> object from an 
        ''' XML Element that represents that object.
        ''' </summary>
        ''' <param name="element">The xml element to deserialize. An element 
        ''' representing a <see cref="TemplateBodyContent"/> is 
        ''' expected</param>
        ''' <returns>
        ''' A new instance of <see cref="IBodyContent"/> from an XML Element
        ''' that represents that object.
        ''' </returns>
        Public Shared Function FromXElement(element As XElement) As IBodyContent

            If Not element.Name.LocalName.Equals("bodycontent") Then _
                Throw New MissingXmlObjectException("bodycontent")

            Dim type = element.Attribute("type")?.Value
            If type Is Nothing Then Throw New MissingXmlObjectException("type")

            Return New NoBodyContent()
        End Function

        ''' <inheritdoc/>
        Public Function ToXElement() As XElement Implements IBodyContent.ToXElement
            Return <bodycontent type=<%= CInt(Type) %>></bodycontent>
        End Function

        ''' <inheritdoc/>
        Public Function GetInputParameters() As IEnumerable(Of ActionParameter) Implements IBodyContent.GetInputParameters
            Return Enumerable.Empty(Of ActionParameter)
        End Function

    End Class

End Namespace
