Imports System.Linq
Imports System.Runtime.Serialization
Imports System.Xml.Linq
Imports BluePrism.Core.Utility
Imports BluePrism.Server.Domain.Models

Namespace WebApis.RequestHandling.BodyContent

    <DataContract([Namespace]:="bp"), Serializable>
    Public Class CustomCodeBodyContent : Implements IBodyContent

        Private Const BodyType As WebApiRequestBodyType = WebApiRequestBodyType.CustomCode

        <DataMember>
        Private mCode As String

        Public ReadOnly Property Code As String
            Get
                Return mCode
            End Get
        End Property

        Public Sub New(code As String)
            mCode = code
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

            Dim code = element.
                                Elements.
                                FirstOrDefault(Function(x) x.Name = "code")?.
                                Nodes.
                                OfType(Of XCData).
                                GetConcatenatedValue()
            If code Is Nothing Then Throw New MissingXmlObjectException("code")

            Return New CustomCodeBodyContent(code)
        End Function

        Public Function ToXElement() As XElement Implements IBodyContent.ToXElement
            Return <bodycontent type=<%= CInt(Type) %>>
                       <code>
                           <%= From d In New XCData(Code).ToEscapedEnumerable()
                               Select d
                           %>
                       </code>
                   </bodycontent>
        End Function

        Public Function GetInputParameters() As IEnumerable(Of ActionParameter) Implements IBodyContent.GetInputParameters
            Return Enumerable.Empty(Of ActionParameter)
        End Function

    End Class

End Namespace
