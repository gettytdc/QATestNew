#If UNITTESTS Then
Imports System.Xml.Linq
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent
Imports BluePrism.Server.Domain.Models
Imports NUnit.Framework

Namespace WebApis.RequestHandling.BodyContent

    Public Class TemplateBodyContentTests

        <Test>
        Public Sub FromXElement_WhenValidXElement_ShouldReturnTemplateBodyContent()

            Dim element = <bodycontent type="1"><template>test</template></bodycontent>

            Dim bodyContent = TemplateBodyContent.FromXElement(element)

            Assert.That(bodyContent, [Is].InstanceOf(Of TemplateBodyContent))
            Assert.That(bodyContent.Type, [Is].EqualTo(WebApiRequestBodyType.Template))
        End Sub

        <Test>
        Public Sub FromXElement_WhenElementIsMissingRoot_ShouldReturnMissingXmlObjectException()

            Dim element = <template>test</template>

            Assert.Throws(Of MissingXmlObjectException)(Sub() TemplateBodyContent.FromXElement(element))
        End Sub

        <Test>
        Public Sub FromXElement_WhenElementIsMissingTypeAttribute_ShouldReturnMissingXmlObjectException()

            Dim element = <bodycontent><template>test</template></bodycontent>

            Assert.Throws(Of MissingXmlObjectException)(Sub() TemplateBodyContent.FromXElement(element))
        End Sub

        <Test>
        Public Sub FromXElement_WhenElementIsMissingTemplateNode_ShouldReturnMissingXmlObjectException()

            Dim element = <bodycontent type="1"></bodycontent>

            Assert.Throws(Of MissingXmlObjectException)(Sub() TemplateBodyContent.FromXElement(element))
        End Sub

        <Test>
        Public Sub ToXElement_ShouldReturnAValidXElement()

            Dim bodyContent = New TemplateBodyContent("test")

            Dim element = bodyContent.ToXElement()

            Assert.That(element, [Is].InstanceOf(Of XElement))
            Assert.That(element.Name.LocalName, [Is].SameAs("bodycontent"))
            Assert.That(element.Attribute("type")?.Value, [Is].EqualTo("1"))
            Assert.That(element.Element("template")?.Value, [Is].Not.Empty)
        End Sub

        <Test>
        Public Sub GetInputParameters_ShouldReturnEmpty()

            Dim bodyContent = New TemplateBodyContent()

            Dim parameters = bodyContent.GetInputParameters()

            Assert.That(parameters, [Is].InstanceOf(Of IEnumerable(Of ActionParameter)))
            Assert.That(parameters, [Is].Empty)
        End Sub
    End Class

End Namespace

#End If
