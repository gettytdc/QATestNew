#If UNITTESTS Then

Imports System.Net.Http
Imports BluePrism.AutomateProcessCore.Compilation
Imports BluePrism.AutomateProcessCore.UnitTests.WebApis.CustomCode
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent
Imports FluentAssertions
Imports NUnit.Framework

Namespace WebApis.RequestHandling.BodyContent

    Public Class CustomCodeContentGeneratorTests

        Private ReadOnly mSession As New clsSession(
            Guid.NewGuid, 105, New WebConnectionSettings(5, 5, 5, New List(Of UriWebConnectionSettings)))

        <Test>
        Public Sub GetBodyContent_ShouldInvokeCustomCode()
            Dim configuration = New WebApiConfigurationBuilder().
                WithAction("Action 1", HttpMethod.Get, "/action-1",
                           bodyContent:=New CustomCodeBodyContent("Request_Content = ""Test Content""")).
                WithCommonCode(New CodeProperties("", CodeLanguage.VisualBasic, {}, {})).
            Build()
            Dim context As New ActionContext(Guid.NewGuid,
                                             configuration,
                                             "Action 1",
                                             New Dictionary(Of String, clsProcessValue),
                                             mSession)

            Dim cache As New ObjectCache(New CustomCodeTestHelper.TestCacheStore())
            Dim builder = CustomCodeTestHelper.CreateCodeBuilder(cache:=cache)
            Dim handler As New CustomCodeContentGenerator(builder)

            Dim result = handler.GetBodyContent(context)

            result.Content.Should().Be("Test Content")
        End Sub

        <Test>
        Public Sub GetBodyContent_ShouldReturnCorrectHeaders()
            Dim configuration = New WebApiConfigurationBuilder().
                WithAction("Action 1", HttpMethod.Get, "/action-1",
                           bodyContent:=New CustomCodeBodyContent("Request_Content = ""Test Content""")).
                WithCommonCode(New CodeProperties("", CodeLanguage.VisualBasic, {}, {})).
            Build()
            Dim context As New ActionContext(Guid.NewGuid,
                                             configuration,
                                             "Action 1",
                                             New Dictionary(Of String, clsProcessValue),
                                             mSession)

            Dim cache As New ObjectCache(New CustomCodeTestHelper.TestCacheStore())
            Dim builder = CustomCodeTestHelper.CreateCodeBuilder(cache:=cache)
            Dim handler As New CustomCodeContentGenerator(builder)

            Dim result = handler.GetBodyContent(context)

            result.Headers.ShouldBeEquivalentTo({New HttpHeader("Content-Type", $"text/plain; charset=utf-8")})
        End Sub

    End Class

End Namespace

#End If
