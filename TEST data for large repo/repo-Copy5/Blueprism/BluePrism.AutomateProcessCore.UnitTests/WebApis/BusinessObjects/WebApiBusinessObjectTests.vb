#If UNITTESTS Then
Imports System.Linq
Imports System.Net.Http
Imports BluePrism.AutomateProcessCore.WebApis
Imports FluentAssertions
Imports NUnit.Framework

Namespace WebApis.BusinessObjects
    <TestFixture, Category("Web APIs")>
    Public Class WebApiBusinessObjectTests
        <Test>
        Public Sub Ctor_FromWebApiWithActions_ContainsActions()
            Dim webApi = BuildWebApi()
            Dim businessObject = New WebApiBusinessObject(webApi)

            Dim actionNames = businessObject.GetActions().
                    Select(Function(a) a.GetName())

            actionNames.Should.BeEquivalentTo("Action1", "Action2")
        End Sub

        <Test>
        Public Sub GetAction_WithValidName_ReturnsCorrectAction()
            Dim webApi = BuildWebApi()
            Dim businessObject = New WebApiBusinessObject(webApi)

            Dim action = businessObject.GetAction("Action1")
            action.Should.NotBeNull()
            action.GetName.Should.Be("Action1")
        End Sub

        <Test>
        Public Sub DoAction_WithUnrecognisedAction_ShouldIndicateFailureReason()
            Dim webApi = BuildWebApi()
            Dim businessObject = New WebApiBusinessObject(webApi)

            Dim result = businessObject.DoAction("non action", Nothing, Nothing, Nothing)
            result.Success.Should.BeFalse()
            result.ExceptionDetail.Should.StartWith("Unrecognised action")
        End Sub

        ''' <summary>
        ''' Builds a WebApi object with known configuration for use in tests
        ''' </summary>
        Private Shared Function BuildWebApi() As WebApi
            Dim config = BuildConfiguration()
            Return New WebApi(Guid.Empty, "Web API 1", True, config)
        End Function

        Private Shared Function BuildConfiguration() As WebApiConfiguration

            Dim commonParameters = {
                                       New ActionParameter("CommonParam1", "This parameter does Nothing", DataType.text,
                                                           True, String.Empty),
                                       New ActionParameter("CommonParam2", String.Empty, DataType.text, True, "456")
                                   }
            Return New WebApiConfigurationBuilder().
                WithBaseUrl("https://www.myapi.org/").
                WithParameters(commonParameters).
                WithHeader("custom-header-1", "value 1").
                WithHeader("custom-header-2", "value 2").
                WithAction("Action1", HttpMethod.Get, "/action1",
                           headers:={
                                          New HttpHeader("Action 1 - Header 1", "header 1 value"),
                                          New HttpHeader("Action 1 - Header 2", "header 2 value")
                                      },
                           parameters:={
                                             New ActionParameter("Action 1 - Parameter 1",
                                                                 "",
                                                                 DataType.text,
                                                                 True,
                                                                 "Parameter 1 initial value"),
                                             New ActionParameter("Action 1 - Parameter 2",
                                                                 "",
                                                                 DataType.text,
                                                                 True,
                                                                 "Parameter 2 initial value")
                                         }).
                WithAction("Action2", HttpMethod.Get, "/action1",
                           headers:={
                                          New HttpHeader("Action 2 - Header 1", "header 1 value"),
                                          New HttpHeader("Action 2 - Header 2", "header 2 value")},
                           parameters:={
                                             New ActionParameter("Action 2 - Parameter 1",
                                                                 "",
                                                                 DataType.text,
                                                                 True,
                                                                 "Parameter 1 initial value"),
                                             New ActionParameter("Action 2 - Parameter 2",
                                                                 "",
                                                                 DataType.text,
                                                                 True,
                                                                 "Parameter 2 initial value")
                                         }).
                Build()
        End Function
    End Class
End Namespace

#End If