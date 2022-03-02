#If UNITTESTS Then

Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent.Multipart
Imports Moq
Imports NUnit.Framework
Imports FluentAssertions
Imports BluePrism.AutomateProcessCore.WebApis

Namespace WebApis.RequestHandling.BodyContent

    Public Class MultiPartBuilderTests

        Private mMultiPartBuilder As MultiPartBodyContentBuilder
        Private mGenerateBoundaryMock As Mock(Of Func(Of String))
        Private Const mBoundaryId1 = "Boundary11111"
        Private Const mBoundaryId2 = "Boundary22222"
        Private Const mBoundaryIdUnique = "BoundaryUnique"
        Private Property mContentTypeHeaderWithUniqueBoundary As IEnumerable(Of HttpHeader)

        <SetUp>
        Public Sub SetUp()
            mMultiPartBuilder = New MultiPartBodyContentBuilder()

            mGenerateBoundaryMock = New Mock(Of Func(Of String))
            mGenerateBoundaryMock.
                Setup(Function(x) x.Invoke).
                Returns(mBoundaryIdUnique)

        End Sub

        <Test>
        Public Sub Build_DifferentNumberOfBodySections_ShouldReturnCorrectNumberOfBoundaries(<Range(0, 3)> numberOfBodySections As Integer)
            Dim multiPartBodySections = GetBodySections().Take(numberOfBodySections)

            Dim result = mMultiPartBuilder.Build(multiPartBodySections, mGenerateBoundaryMock.Object)

            Assert.That(result?.Content, Iz.Not.Null)
            Assert.That(Regex.Matches(result?.Content, "--" & mBoundaryIdUnique).Count, Iz.EqualTo(numberOfBodySections + 1))
        End Sub

        <Test>
        Public Sub Build_ContentContainsNoBoundaries_ShouldCallGenerateBoundaryOnce()
            Dim sectionContainingNoBoundary = GetBodySections().First(Function(x) x.FieldName = "Content contains no boundary")

            Dim result = mMultiPartBuilder.Build({sectionContainingNoBoundary}, mGenerateBoundaryMock.Object)

            mGenerateBoundaryMock.Verify(Function(x) x.Invoke, Times.Exactly(1))

        End Sub

        <Test>
        Public Sub Build_ContentContainsFirstGeneratedBoundary_ShouldCallGenerateBoundaryTwice()
            Dim sectionContainingBoundary1 = GetBodySections().First(Function(x) x.FieldName = "Content contains boundary 1")

            mGenerateBoundaryMock = New Mock(Of Func(Of String))
            mGenerateBoundaryMock.
                SetupSequence(Function(x) x.Invoke).
                Returns(mBoundaryId1).
                Returns(mBoundaryIdUnique)

            Dim result = mMultiPartBuilder.Build({sectionContainingBoundary1}, mGenerateBoundaryMock.Object)

            mGenerateBoundaryMock.Verify(Function(x) x.Invoke, Times.Exactly(2))

        End Sub

        <Test>
        Public Sub Build_ContentContainsFirstTwoGeneratedBoundaries_ShouldCallGenerateBoundaryThreeTimes()
            Dim sectionContainingBoundary1 = GetBodySections().First(Function(x) x.FieldName = "Content contains boundary 1")
            Dim sectionContainingBoundary2 = GetBodySections().First(Function(x) x.FieldName = "Content contains boundary 2")

            mGenerateBoundaryMock = New Mock(Of Func(Of String))
            mGenerateBoundaryMock.
                SetupSequence(Function(x) x.Invoke).
                Returns(mBoundaryId1).
                Returns(mBoundaryId2).
                Returns(mBoundaryIdUnique)

            Dim result = mMultiPartBuilder.Build({sectionContainingBoundary1, sectionContainingBoundary2}, mGenerateBoundaryMock.Object)

            mGenerateBoundaryMock.Verify(Function(x) x.Invoke, Times.Exactly(3))

        End Sub

        <Test>
        Public Sub Build_ContentContainsNoBoundaries_ShouldReturnHeaderWithCorrectBoundary()
            Dim sectionContainingNoBoundary = GetBodySections().First(Function(x) x.FieldName = "Content contains no boundary")

            Dim result = mMultiPartBuilder.Build({sectionContainingNoBoundary}, mGenerateBoundaryMock.Object)

            result.Headers.First().Value.Should.EndWith($"boundary={ mBoundaryIdUnique }")

        End Sub

        <Test>
        Public Sub Build_ContentContainsFirstGeneratedBoundary_ShouldReturnHeaderWithCorrectBoundary()
            Dim sectionContainingBoundary1 = GetBodySections().First(Function(x) x.FieldName = "Content contains boundary 1")

            mGenerateBoundaryMock = New Mock(Of Func(Of String))
            mGenerateBoundaryMock.
                SetupSequence(Function(x) x.Invoke).
                Returns(mBoundaryId1).
                Returns(mBoundaryIdUnique)

            Dim result = mMultiPartBuilder.Build({sectionContainingBoundary1}, mGenerateBoundaryMock.Object)

            result.Headers.First().Value.Should.EndWith($"boundary={ mBoundaryIdUnique }")

        End Sub


        <Test>
        Public Sub Build_ContentContainsFirstTwoGeneratedBoundaries_ShouldReturnHeaderWithCorrectBoundary()
            Dim sectionContainingBoundary1 = GetBodySections().First(Function(x) x.FieldName = "Content contains boundary 1")
            Dim sectionContainingBoundary2 = GetBodySections().First(Function(x) x.FieldName = "Content contains boundary 2")

            mGenerateBoundaryMock = New Mock(Of Func(Of String))
            mGenerateBoundaryMock.
                SetupSequence(Function(x) x.Invoke).
                Returns(mBoundaryId1).
                Returns(mBoundaryId2).
                Returns(mBoundaryIdUnique)

            Dim result = mMultiPartBuilder.Build({sectionContainingBoundary1, sectionContainingBoundary2}, mGenerateBoundaryMock.Object)

            result.Headers.First().Value.Should.EndWith($"boundary={ mBoundaryIdUnique }")

        End Sub

        <Test>
        Public Sub Build_ContentContainsNoBoundaries_ShouldReturnContentWithCorrectTrailer()
            Dim sectionContainingNoBoundary = GetBodySections().First(Function(x) x.FieldName = "Content contains no boundary")

            Dim result = mMultiPartBuilder.Build({sectionContainingNoBoundary}, mGenerateBoundaryMock.Object)

            result.Content.Should.EndWith($"{vbCrLf}--{mBoundaryIdUnique}--{vbCrLf}")

        End Sub

        <Test>
        Public Sub Build_ContentContainsFirstGeneratedBoundary_ShouldReturnContentWithCorrectTrailer()
            Dim sectionContainingBoundary1 = GetBodySections().First(Function(x) x.FieldName = "Content contains boundary 1")

            mGenerateBoundaryMock = New Mock(Of Func(Of String))
            mGenerateBoundaryMock.
                SetupSequence(Function(x) x.Invoke).
                Returns(mBoundaryId1).
                Returns(mBoundaryIdUnique)

            Dim result = mMultiPartBuilder.Build({sectionContainingBoundary1}, mGenerateBoundaryMock.Object)

            result.Content.Should.EndWith($"{vbCrLf}--{mBoundaryIdUnique}--{vbCrLf}")

        End Sub


        <Test>
        Public Sub Build_ContentContainsFirstTwoGeneratedBoundaries_ShouldReturnContentWithCorrectTrailer()
            Dim sectionContainingBoundary1 = GetBodySections().First(Function(x) x.FieldName = "Content contains boundary 1")
            Dim sectionContainingBoundary2 = GetBodySections().First(Function(x) x.FieldName = "Content contains boundary 2")

            mGenerateBoundaryMock = New Mock(Of Func(Of String))
            mGenerateBoundaryMock.
                SetupSequence(Function(x) x.Invoke).
                Returns(mBoundaryId1).
                Returns(mBoundaryId2).
                Returns(mBoundaryIdUnique)

            Dim result = mMultiPartBuilder.Build({sectionContainingBoundary1, sectionContainingBoundary2}, mGenerateBoundaryMock.Object)

            result.Content.Should.EndWith($"{vbCrLf}--{mBoundaryIdUnique}--{vbCrLf}")

        End Sub


        Private Iterator Function GetBodySections() As IEnumerable(Of MultiPartFileBodySection)
            Dim content1 = Encoding.UTF8.GetBytes("Some text")
            Dim content2 = Encoding.UTF8.GetBytes($"Some text with {mBoundaryId1}")
            Dim content3 = Encoding.UTF8.GetBytes($"Some text with {mBoundaryId2}")

            Yield New MultiPartFileBodySection("Content contains no boundary", String.Empty, content1, String.Empty)
            Yield New MultiPartFileBodySection("Content contains boundary 1", String.Empty, content2, String.Empty)
            Yield New MultiPartFileBodySection("Content contains boundary 2", String.Empty, content3, String.Empty)

        End Function

    End Class

End Namespace

#End If
