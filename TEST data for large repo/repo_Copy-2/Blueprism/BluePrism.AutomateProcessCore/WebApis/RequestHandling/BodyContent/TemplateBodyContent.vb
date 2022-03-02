Imports System.Linq
Imports System.Runtime.Serialization
Imports System.Xml.Linq
Imports BluePrism.BPCoreLib
Imports BluePrism.Core.Utility
Imports BluePrism.Server.Domain.Models

Namespace WebApis.RequestHandling.BodyContent

    <DataContract([Namespace]:="bp"), Serializable>
    Public Class TemplateBodyContent : Implements IBodyContent

        Private Const BodyType As WebApiRequestBodyType = WebApiRequestBodyType.Template

        <DataMember>
        Private mTemplate As String

        Public ReadOnly Property Template As String
            Get
                Return mTemplate
            End Get
        End Property

        Public Sub New()
            Me.New(String.Empty)
        End Sub

        Public Sub New(template As String)
            mTemplate = template
        End Sub

        Public ReadOnly Property Type As WebApiRequestBodyType Implements IBodyContent.Type
            Get
                Return BodyType
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

            Dim template = element.
                                Elements.
                                FirstOrDefault(Function(x) x.Name = "template")?.
                                Nodes.
                                OfType(Of XCData).
                                GetConcatenatedValue()
            If template Is Nothing Then Throw New MissingXmlObjectException("template")

            Return New TemplateBodyContent(template)
        End Function

        ''' <inheritdoc/>
        Public Function ToXElement() As XElement Implements IBodyContent.ToXElement
            Return <bodycontent type=<%= CInt(Type) %>>
                       <template>
                           <%= From d In New XCData(Template).ToEscapedEnumerable()
                               Select d
                           %>
                       </template>
                   </bodycontent>
        End Function

        ''' <inheritdoc/>
        Public Function GetInputParameters() As IEnumerable(Of ActionParameter) Implements IBodyContent.GetInputParameters
            Return Enumerable.Empty(Of ActionParameter)
        End Function

    End Class

End Namespace
