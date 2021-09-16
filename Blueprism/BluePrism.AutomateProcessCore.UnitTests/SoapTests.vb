#If UNITTESTS Then

Imports System.Text
Imports NUnit.Framework
Imports System.IO

''' <summary>
''' Tests for the <see cref="clsSoap"/> class.
''' </summary>
<TestFixture()>
Public Class SoapTests
    <Test>
    Public Sub GetEncoding_ResponseHasValidCharSet_ReturnsCorrectEncoding()
        Dim result = clsSoap.GetEncoding("UTF-32")

        Assert.That(result, Iz.EqualTo(Encoding.UTF32))
    End Sub

    <Test>
    Public Sub GetEncoding_ResponseHasInvalidCharSet_ReturnsNothing()
        Assert.IsNull(clsSoap.GetEncoding("nonsense"))
    End Sub

    <Test>
    Public Sub GetEncoding_ResponseHasEmptyCharSet_ReturnsNothing()
        Assert.IsNull(clsSoap.GetEncoding(""))
    End Sub

    <Test>
    Public Sub GetEncoding_ResponseHasNoCharSet_ReturnsNothing()
        Assert.IsNull(clsSoap.GetEncoding(Nothing))
    End Sub

    <Test>
    Public Sub GetXmlDocumentFromStream_UTF8EncodedStreamWithNoCharSet_ReturnsExpectedXmlDocument()
        Dim stream = New MemoryStream(Encoding.UTF8.GetBytes("<test>Hello</test>"))

        Dim result = clsSoap.GetXmlDocumentFromStream(stream, "")

        Dim expectedXml = "<test>Hello</test>"
        Assert.That(result.OuterXml, Iz.EqualTo(expectedXml))
    End Sub

    <Test>
    Public Sub GetXmlDocumentFromStream_UTF8EncodedStreamWithCharSet_ReturnsExpectedXmlDocument()
        Dim stream = New MemoryStream(Encoding.UTF8.GetBytes("<test>Hello</test>"))

        Dim result = clsSoap.GetXmlDocumentFromStream(stream, "UTF-8")

        Dim expectedXml = "<test>Hello</test>"
        Assert.That(result.OuterXml, Iz.EqualTo(expectedXml))
    End Sub

    <Test>
    Public Sub GetXmlDocumentFromStream_UTF32EncodedStreamWithNoCharSet_ReturnsExpectedXmlDocument()
        Dim stream = New MemoryStream(Encoding.UTF32.GetBytes("<test>Hello</test>"))

        Dim result = clsSoap.GetXmlDocumentFromStream(stream, "")

        Dim expectedXml = "<test>Hello</test>"
        Assert.That(result.OuterXml, Iz.EqualTo(expectedXml))
    End Sub

    <Test>
    Public Sub GetXmlDocumentFromStream_UTF32EncodedStreamWithCharSet_ReturnsExpectedXmlDocument()
        Dim stream = New MemoryStream(Encoding.UTF32.GetBytes("<test>Hello</test>"))

        Dim result = clsSoap.GetXmlDocumentFromStream(stream, "UTF-32")

        Dim expectedXml = "<test>Hello</test>"
        Assert.That(result.OuterXml, Iz.EqualTo(expectedXml))
    End Sub

End Class

#End If