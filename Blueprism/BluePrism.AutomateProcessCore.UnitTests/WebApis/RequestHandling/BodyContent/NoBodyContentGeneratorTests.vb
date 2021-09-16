#If UNITTESTS Then

Imports System.Net.Http
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent
Imports NUnit.Framework

Namespace WebApis.RequestHandling.BodyContent

    Public Class NoBodyContentGeneratorTests

        Private mfileHandler As NoBodyContentGenerator
        Private mParameters As Dictionary(Of String, clsProcessValue)

        Private ReadOnly mWebApiId As Guid = New Guid("eeb2a2e4-7354-4257-9da0-a5b5ae1bbe1c")
        Private ReadOnly mSession As New clsSession(
            New Guid("8fa4065c-2dad-4f5e-ac56-d5bbb61b6722"), 105, New WebConnectionSettings(5, 5, 5, New List(Of UriWebConnectionSettings)))

        <SetUp>
        Public Sub SetUp()

            mfileHandler = New NoBodyContentGenerator()
            mParameters = New Dictionary(Of String, clsProcessValue)
        End Sub

        <Test>
        Public Sub GetBodyContent_ShouldReturnEmptyBodyContentResult()

            Dim configuration = New WebApiConfigurationBuilder().
                WithAction("TestAction", HttpMethod.Get, "/api/test",
                bodyContent:=New NoBodyContent()).
                Build()
            Dim context As New ActionContext(mWebApiId, configuration, "TestAction", mParameters, mSession)

            Dim result = mfileHandler.GetBodyContent(context)

            Assert.That(result, [Is].InstanceOf(Of EmptyBodyContentResult))
        End Sub

        <Test>
        Public Sub GetContentTypeHeaders_ShouldReturnEmptyHttpHeaderCollection()

            Dim configuration = New WebApiConfigurationBuilder().
                WithAction("TestAction", HttpMethod.Get, "/api/test",
                bodyContent:=New NoBodyContent()).
                Build()
            Dim context As New ActionContext(mWebApiId, configuration, "TestAction", mParameters, mSession)

            Dim result = mfileHandler.GetBodyContent(context)

            Assert.That(result.Headers, [Is].InstanceOf(Of IEnumerable(Of HttpHeader)))
        End Sub

    End Class

End Namespace

#End If