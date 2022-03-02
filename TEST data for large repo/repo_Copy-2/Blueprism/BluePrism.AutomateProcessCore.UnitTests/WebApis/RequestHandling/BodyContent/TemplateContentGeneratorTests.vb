#If UNITTESTS Then

Imports System.Net.Http
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent
Imports NUnit.Framework

Namespace WebApis.RequestHandling.BodyContent

    Public Class TemplateContentGeneratorTests

        Private mfileHandler As TemplateContentGenerator
        Private mParameters As Dictionary(Of String, clsProcessValue)

        Private ReadOnly mWebApiId As Guid = New Guid("eeb2a2e4-7354-4257-9da0-a5b5ae1bbe1c")
        Private ReadOnly mSession As New clsSession(
            New Guid("8fa4065c-2dad-4f5e-ac56-d5bbb61b6722"), 105, New WebConnectionSettings(5, 5, 5, New List(Of UriWebConnectionSettings)))

        <SetUp>
        Public Sub SetUp()

            mfileHandler = New TemplateContentGenerator()
            mParameters = New Dictionary(Of String, clsProcessValue)
        End Sub

        <Test>
        Public Sub GetBodyContent_WhenTemplateIsValid_ThenReturnStringBodyContentResult()

            Dim configuration = New WebApiConfigurationBuilder().
                WithAction("TestAction", HttpMethod.Post, "/api/test",
                bodyContent:=New TemplateBodyContent("This is a template, honest!")).
                Build()
            Dim context As New ActionContext(mWebApiId, configuration, "TestAction", mParameters, mSession)

            Dim result = mfileHandler.GetBodyContent(context)

            Assert.That(result, [Is].InstanceOf(Of StringBodyContentResult))
        End Sub

        <Test>
        Public Sub GetBodyContent_WhenTemplateIsNothing_ThenReturnEmptyBodyContentResult()

            Dim configuration = New WebApiConfigurationBuilder().
                WithAction("TestAction", HttpMethod.Post, "/api/test",
                bodyContent:=New TemplateBodyContent(String.Empty)).
                Build()
            Dim context As New ActionContext(mWebApiId, configuration, "TestAction", mParameters, mSession)

            Dim result = mfileHandler.GetBodyContent(context)

            Assert.That(result, [Is].InstanceOf(Of EmptyBodyContentResult))
        End Sub

        <Test>
        Public Sub GetContentTypeHeaders_ShouldReturnOneHttpHeaderResult()

            Dim configuration = New WebApiConfigurationBuilder().
                WithAction("TestAction", HttpMethod.Post, "/api/test",
                bodyContent:=New TemplateBodyContent("This is a template, honest!")).
                Build()
            Dim context As New ActionContext(mWebApiId, configuration, "TestAction", mParameters, mSession)

            Dim result = mfileHandler.GetBodyContent(context)

            Assert.That(result.Headers, Has.Exactly(1).Property("Name").SameAs("Content-Type"))
        End Sub

    End Class

End Namespace

#End If