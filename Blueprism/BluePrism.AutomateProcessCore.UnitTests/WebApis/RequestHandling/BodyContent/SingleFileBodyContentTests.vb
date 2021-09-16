#If UNITTESTS Then

Imports System.Xml.Linq
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent
Imports BluePrism.Server.Domain.Models
Imports NUnit.Framework

Namespace WebApis.RequestHandling.BodyContent

    Public Class SingleFileBodyContentTests
        <Test>
        Public Sub FromXElement_WhenValidXElement_ShouldReturnSingleFileBodyContent()

            Dim element = <bodycontent type="2" fileinputparametername="file"></bodycontent>

            Dim bodyContent = SingleFileBodyContent.FromXElement(element)

            Assert.That(bodyContent, [Is].InstanceOf(Of SingleFileBodyContent))
            Assert.That(bodyContent.Type, [Is].EqualTo(WebApiRequestBodyType.SingleFile))
        End Sub

        <Test>
        Public Sub FromXElement_WhenElementIsMissingRoot_ShouldReturnMissingXmlObjectException()

            Dim element = <notbodycontent>test</notbodycontent>

            Assert.Throws(Of MissingXmlObjectException)(Sub() SingleFileBodyContent.FromXElement(element))
        End Sub

        <Test>
        Public Sub FromXElement_WhenElementIsMissingTypeAttribute_ShouldReturnMissingXmlObjectException()

            Dim element = <bodycontent fileinputparametername="file"><template>test</template></bodycontent>

            Assert.Throws(Of MissingXmlObjectException)(Sub() SingleFileBodyContent.FromXElement(element))
        End Sub

        <Test>
        Public Sub FromXElement_WhenElementIsMissingParameterAttribute_ShouldReturnMissingXmlObjectException()

            Dim element = <bodycontent type="2"></bodycontent>

            Assert.Throws(Of MissingXmlObjectException)(Sub() SingleFileBodyContent.FromXElement(element))
        End Sub

        <Test>
        Public Sub ToXElement_ShouldReturnAValidXElement()

            Dim bodyContent = New SingleFileBodyContent("fakeparametername")

            Dim element = bodyContent.ToXElement()

            Assert.That(element, [Is].InstanceOf(Of XElement))
            Assert.That(element.Name.LocalName, [Is].SameAs("bodycontent"))
            Assert.That(element.Attribute("type")?.Value, [Is].EqualTo("2"))
            Assert.That(element.Attribute("fileinputparametername")?.Value, [Is].SameAs("fakeparametername"))
        End Sub

        <Test>
        Public Sub GetInputParameters_ShouldReturnActionParameter()

            Dim bodyContent = New SingleFileBodyContent()

            Dim parameters = bodyContent.GetInputParameters()

            Assert.That(parameters, [Is].InstanceOf(Of IEnumerable(Of ActionParameter)))
            Assert.That(parameters, Has.Exactly(1).Property("Name").EqualTo("File"))
        End Sub
    End Class

End Namespace

#End If
