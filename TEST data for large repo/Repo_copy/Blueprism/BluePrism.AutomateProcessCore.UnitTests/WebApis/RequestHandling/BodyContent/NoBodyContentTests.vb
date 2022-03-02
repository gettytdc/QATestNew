#If UNITTESTS Then

Imports System.Linq
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent
Imports BluePrism.Server.Domain.Models
Imports NUnit.Framework

Namespace WebApis.RequestHandling.BodyContent

    Public Class NoBodyContentTests
        <Test>
        Public Sub FromXElement_WhenValidXElement_ShouldReturnSingleFileBodyContent()

            Dim element = <bodycontent type="0"></bodycontent>

            Dim bodyContent = NoBodyContent.FromXElement(element)

            Assert.That(bodyContent, [Is].InstanceOf(Of NoBodyContent))
            Assert.That(bodyContent.Type, [Is].EqualTo(WebApiRequestBodyType.None))
        End Sub

        <Test>
        Public Sub FromXElement_WhenElementIsMissingRoot_ShouldReturnMissingXmlObjectException()

            Dim element = <notbodycontent>test</notbodycontent>

            Assert.Throws(Of MissingXmlObjectException)(Sub() NoBodyContent.FromXElement(element))
        End Sub

        <Test>
        Public Sub FromXElement_WhenElementIsMissingTypeAttribute_ShouldReturnMissingXmlObjectException()

            Dim element = <bodycontent></bodycontent>

            Assert.Throws(Of MissingXmlObjectException)(Sub() NoBodyContent.FromXElement(element))
        End Sub

        <Test>
        Public Sub ToXElement_ShouldReturnAValidXElement()

            Dim bodyContent = New NoBodyContent()

            Dim element = bodyContent.ToXElement()

            Assert.That(element.Name.LocalName, [Is].SameAs("bodycontent"))
            Assert.That(element.Attribute("type")?.Value, [Is].EqualTo("0"))
        End Sub

        <Test>
        Public Sub GetInputParameters_ShouldReturnEmpty()

            Dim bodyContent = New NoBodyContent()

            Dim parameterCount = bodyContent.GetInputParameters().Count()

            Assert.That(parameterCount, Iz.EqualTo(0))
        End Sub
    End Class

End Namespace

#End If
