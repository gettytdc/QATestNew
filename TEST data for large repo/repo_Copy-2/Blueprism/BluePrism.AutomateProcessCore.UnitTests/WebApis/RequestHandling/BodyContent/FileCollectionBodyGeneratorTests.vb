#If UNITTESTS Then

Imports System.Net.Http
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent.Multipart
Imports Moq
Imports NUnit.Framework

Namespace WebApis.RequestHandling.BodyContent

    Public Class FileCollectionContentGeneratorTests

        Private mMultipartBuilderMock As Mock(Of IMultiPartBodyContentBuilder)
        Private mConfiguration As WebApiConfiguration
        Private mfileHandler As FileCollectionContentGenerator
        Private mParameters As Dictionary(Of String, clsProcessValue)
        Private mGenerateBoundaryMock As Mock(Of Func(Of String))
        Private Const mBoundaryId = "Boundary11111"

        Private ReadOnly mWebApiId As Guid = New Guid("eeb2a2e4-7354-4257-9da0-a5b5ae1bbe1c")
        Private ReadOnly mSession As New clsSession(
            New Guid("8fa4065c-2dad-4f5e-ac56-d5bbb61b6722"), 105, New WebConnectionSettings(5, 5, 5, New List(Of UriWebConnectionSettings)))

        <SetUp>
        Public Sub SetUp()
            mMultipartBuilderMock = New Mock(Of IMultiPartBodyContentBuilder)

            mGenerateBoundaryMock = New Mock(Of Func(Of String))
            mGenerateBoundaryMock.
                Setup(Function(x) x.Invoke).
                Returns(mBoundaryId)

            mMultipartBuilderMock.
                Setup(Function(x) x.Build(It.IsAny(Of IEnumerable(Of MultiPartFileBodySection)), It.IsAny(Of Func(Of String)))).
                Returns(New RawDataBodyContentResult(New Byte() {}, MultiPartBodyContentBuilder.GetContentTypeHeaders(mBoundaryId)))

            mConfiguration = New WebApiConfigurationBuilder().
                                    WithAction("TestAction", HttpMethod.Post, "/api/test", bodyContent:=New FileCollectionBodyContent()).
                                    Build()

            mfileHandler = New FileCollectionContentGenerator(mMultipartBuilderMock.Object)

            mParameters = New Dictionary(Of String, clsProcessValue)
        End Sub

        <Test>
        Public Sub GetBodyContent_WhenFilesCollectionIsValid_ThenReturnRawBodyContentResult()

            Dim filesCollection = <collection>
                                      <row>
                                          <field name="File" type="binary" value="AQIDBAU="/>
                                          <field name="Content Type" type="text" value="image/png"/>
                                          <field name="File Name" type="text" value="picture.png"/>
                                          <field name="Field Name" type="text" value="picture"/>
                                      </row>
                                  </collection>
            mParameters.Add("Files", New clsCollection(filesCollection.ToString()))
            Dim context As New ActionContext(mWebApiId, mConfiguration, "TestAction", mParameters, mSession)

            Dim result = mfileHandler.GetBodyContent(context)

            Assert.That(result, [Is].InstanceOf(Of RawDataBodyContentResult))
        End Sub

        <Test>
        Public Sub GetBodyContent_WhenFilesCollectionIsEmpty_ThenThrowArgumentException()

            Dim context As New ActionContext(mWebApiId, mConfiguration, "TestAction", mParameters, mSession)

            Assert.Throws(Of ArgumentException)(Sub() mfileHandler.GetBodyContent(context))
        End Sub

        <Test>
        Public Sub GetBodyContent_WhenFileIsNotBinary_ThenThrowArgumentException()

            Dim filesCollection = <collection>
                                      <row>
                                          <field name="File" type="text" value="THISISSOMETEXT"/>
                                      </row>
                                  </collection>
            mParameters.Add("Files", New clsCollection(filesCollection.ToString()))
            Dim context As New ActionContext(mWebApiId, mConfiguration, "TestAction", mParameters, mSession)

            Assert.Throws(Of ArgumentException)(Sub() mfileHandler.GetBodyContent(context))
        End Sub

        <Test>
        Public Sub GetBodyContent_WhenOnlyFileIsSupplied_ThenReturnRawBodyContentResult()

            Dim filesCollection = <collection>
                                      <row>
                                          <field name="File" type="binary" value="AQIDBAU="/>
                                      </row>
                                  </collection>
            mParameters.Add("Files", New clsCollection(filesCollection.ToString()))
            Dim context As New ActionContext(mWebApiId, mConfiguration, "TestAction", mParameters, mSession)

            Dim result = mfileHandler.GetBodyContent(context)

            Assert.That(result, [Is].InstanceOf(Of RawDataBodyContentResult))
        End Sub

        <Test>
        Public Sub GetContentTypeHeaders_ShouldReturnOneHttpHeaderResult()
            Dim filesCollection = <collection>
                                      <row>
                                          <field name="File" type="binary" value="AQIDBAU="/>
                                          <field name="Content Type" type="text" value="image/png"/>
                                          <field name="File Name" type="text" value="picture.png"/>
                                          <field name="Field Name" type="text" value="picture"/>
                                      </row>
                                  </collection>

            mParameters.Add("Files", New clsCollection(filesCollection.ToString()))
            Dim context As New ActionContext(mWebApiId, mConfiguration, "TestAction", mParameters, mSession)

            Dim result = mfileHandler.GetBodyContent(context)

            Assert.That(result.Headers, Has.Exactly(1).Property("Name").SameAs("Content-Type"))
        End Sub

    End Class

End Namespace

#End If