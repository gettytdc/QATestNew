#If UNITTESTS Then

Imports System.Net.Http
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent
Imports NUnit.Framework

Namespace WebApis.RequestHandling.BodyContent

    Public Class SingleFileContentGeneratorTests

        Private mConfiguration As WebApiConfiguration
        Private mfileHandler As SingleFileContentGenerator
        Private mParameters As Dictionary(Of String, clsProcessValue)

        Private ReadOnly mWebApiId As Guid = New Guid("eeb2a2e4-7354-4257-9da0-a5b5ae1bbe1c")
        Private ReadOnly mSession As New clsSession(
            New Guid("8fa4065c-2dad-4f5e-ac56-d5bbb61b6722"), 105, New WebConnectionSettings(5, 5, 5, New List(Of UriWebConnectionSettings)))

        <SetUp>
        Public Sub SetUp()

            mConfiguration = New WebApiConfigurationBuilder().
                WithAction("TestAction", HttpMethod.Post, "/api/test",
                bodyContent:=New SingleFileBodyContent()).
                Build()

            mfileHandler = New SingleFileContentGenerator()

            mParameters = New Dictionary(Of String, clsProcessValue)
        End Sub

        <Test>
        Public Sub GetBodyContent_WhenFileIsValid_ThenReturnRawDataBodyContentResult()

            mParameters.Add("File", New clsProcessValue(New Byte() {1, 2, 3, 4}))
            Dim context As New ActionContext(mWebApiId, mConfiguration, "TestAction", mParameters, mSession)

            Dim result = mfileHandler.GetBodyContent(context)

            Assert.That(result, [Is].InstanceOf(Of RawDataBodyContentResult))
        End Sub

        <Test>
        Public Sub GetBodyContent_WhenFileIsNotBinary_ThenThrowArgumentException()

            mParameters.Add("File", New clsProcessValue("This should be binary"))
            Dim context As New ActionContext(mWebApiId, mConfiguration, "TestAction", mParameters, mSession)

            Assert.Throws(Of ArgumentException)(Sub() mfileHandler.GetBodyContent(context))
        End Sub

        <Test>
        Public Sub GetBodyContent_WhenFileIsNull_ThenThrowArgumentException()

            Dim context As New ActionContext(mWebApiId, mConfiguration, "TestAction", mParameters, mSession)

            Assert.Throws(Of ArgumentException)(Sub() mfileHandler.GetBodyContent(context))
        End Sub

        <Test>
        Public Sub GetContentTypeHeaders_ShouldReturnOneHttpHeaderResult()

            mParameters.Add("File", New clsProcessValue(New Byte() {1, 2, 3, 4}))
            Dim context As New ActionContext(mWebApiId, mConfiguration, "TestAction", mParameters, mSession)

            Dim result = mfileHandler.GetBodyContent(context)

            Assert.That(result.Headers, Has.Exactly(1).Property("Name").SameAs("Content-Type"))
        End Sub

    End Class

End Namespace

#End If