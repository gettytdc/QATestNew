#If UNITTESTS Then

Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent.Multipart
Imports NUnit.Framework

Namespace WebApis.RequestHandling.BodyContent

    Public Class MultiPartFileContentTests

        Private Const DefaultContentType = "application/octet-stream"

        <Test>
        Public Sub MultiPartFileBodyContent_WhenInitialisedWithNoContentType_ThenExpectOctetStreamContentType()

            Dim file = New MultiPartFileBodySection(Nothing, Nothing, Nothing, Nothing)

            Assert.That(file.ContentType, Iz.EqualTo(DefaultContentType))

        End Sub

        <Test>
        Public Sub MultiPartFileBodyContent_WhenInitialisedWithContentType_ThenDontExpectOctetStreamContentType()

            Dim file = New MultiPartFileBodySection(Nothing, Nothing, Nothing, "image/png")

            Assert.That(file.ContentType, Iz.EqualTo("image/png"))

        End Sub

        <Test>
        Public Sub Header_WhenSectionHasAllParameters_ThenHeaderContainsSectionsFields()

            Dim section = New MultiPartFileBodySection("test", "testfile.txt",
                                                       New Byte() {1, 2, 3, 4}, "text/plain")

            Dim header = section.Header()

            Assert.That(header, [Is].Not.Null)
            StringAssert.StartsWith("Content-Disposition: form-data;", header)
            StringAssert.Contains("; name=""test"";", header)
            StringAssert.Contains("; filename=""testfile.txt""", header)
            StringAssert.Contains("Content-Type: text/plain", header)
        End Sub

        <Test>
        Public Sub Header_WhenSectionIsMissingFieldName_ThenHeaderShouldNotContainFieldName()

            Dim section = New MultiPartFileBodySection(Nothing, "testfile.txt",
                                                       New Byte() {1, 2, 3, 4}, "text/plain")

            Dim header = section.Header()

            Assert.That(header, [Is].Not.Null)
            StringAssert.DoesNotContain(";name=", header)

            StringAssert.StartsWith("Content-Disposition: form-data;", header)
            StringAssert.Contains("; filename=""testfile.txt""", header)
            StringAssert.Contains("Content-Type: text/plain", header)
        End Sub

        <Test>
        Public Sub Header_WhenSectionIsMissingFileName_ThenHeaderShouldNotContainFileName()

            Dim section = New MultiPartFileBodySection("test", Nothing, New Byte() {1, 2, 3, 4},
                                                       "image/png")

            Dim header = section.Header()

            Assert.That(header, [Is].Not.Null)
            StringAssert.DoesNotContain(";filename=", header)

            StringAssert.StartsWith("Content-Disposition: form-data;", header)
            StringAssert.Contains("; name=""test""", header)
            StringAssert.Contains("Content-Type: image/png", header)
        End Sub

        <Test>
        Public Sub EncodedContent_WhenByteArrayIsSupplied_ShouldReturnUTF7EncodedString()

            Dim section = New MultiPartFileBodySection(Nothing, Nothing, New Byte() {1, 2, 3, 4},
                                                       Nothing)

            Dim content = section.Content()

            Assert.That(content, [Is].Not.Null)
            Assert.That(content, [Is].EqualTo(ChrW(1) & ChrW(2) & ChrW(3) & ChrW(4)))
        End Sub

    End Class

End Namespace

#End If