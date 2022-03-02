#If UNITTESTS Then

Imports NUnit.Framework
Imports System.Linq
Imports System.Net.Http
Imports BluePrism.AutomateProcessCore.WebApis
Imports FluentAssertions

Namespace WebApis.BusinessObjects

    <TestFixture, Category("Web APIs")>
    Public Class WebApiBusinessObjectActionTests

        <Test>
        Public Sub GetParameters_WithActionAndCommonParameters_IncludesExposedParametersAsInputParameters()

            Dim action = BuildAction()

            Dim parameters = action.GetParameters().OfType(Of clsProcessParameter).
                Where(Function(p) p.Direction = ParamDirection.In).
                ToList()

            Dim expectedParameters = {
                New With {.Name = "Action 1 - Parameter 1", .ParamType = DataType.text},
                New With {.Name = "Action 1 - Parameter 2", .ParamType = DataType.text},
                New With {.Name = "Common Parameter 1", .ParamType = DataType.text},
                New With {.Name = "Common Parameter 2", .ParamType = DataType.text}
            }
            parameters.ShouldAllBeEquivalentTo(expectedParameters, Function(options) options.ExcludingMissingMembers)

        End Sub

        <Test>
        Public Sub GetParameters_WithAnyAction_IncludesBasicOutputParameters()

            Dim action = BuildAction()

            Dim parameters = action.GetParameters().OfType(Of clsProcessParameter).
                    Where(Function(p) p.Direction = ParamDirection.Out).
                    ToList()

            Dim expectedParameters = {
                New With {.Name = OutputParameters.ResponseContent, .ParamType = DataType.text},
                New With {.Name = OutputParameters.ResponseHeaders, .ParamType = DataType.collection},
                New With {.Name = OutputParameters.StatusCode, .ParamType = DataType.text}
            }
            parameters.ShouldAllBeEquivalentTo(expectedParameters, Function(options) options.ExcludingMissingMembers)

        End Sub

        <Test>
        Public Sub GetParameters_ActionEnablesRequestOutputParameter_IncludesRequestOutputParameter()

            Dim action = BuildAction(enableRequestOutputParam:=True)
            Dim parameters = action.GetParameters().OfType(Of clsProcessParameter).
                    Where(Function(p) p.Direction = ParamDirection.Out).
                    ToList()

            Dim expectedParameters = {
                New With {.Name = OutputParameters.ResponseContent, .ParamType = DataType.text},
                New With {.Name = OutputParameters.ResponseHeaders, .ParamType = DataType.collection},
                New With {.Name = OutputParameters.StatusCode, .ParamType = DataType.text},
                New With {.Name = OutputParameters.RequestData, .ParamType = DataType.text}
            }
            parameters.ShouldAllBeEquivalentTo(expectedParameters, Function(options) options.ExcludingMissingMembers)

        End Sub

        Private Shared Function BuildAction(Optional enableRequestOutputParam As Boolean = False) As WebApiBusinessObjectAction

            Dim configuration = BuildConfiguration(enableRequestOutputParam)
            Dim actionConfiguration = configuration.Actions(0)
            Dim action = New WebApiBusinessObjectAction(actionConfiguration,
                                                        Guid.NewGuid(),
                                                        configuration)
            Return action
        End Function

        Private Shared Function BuildConfiguration(enableRequestOutputParam As Boolean) As WebApiConfiguration

            Dim commonParameters = {
                                       New ActionParameter("Common Parameter 1", "This parameter does Nothing", DataType.text,
                                                           True, String.Empty),
                                       New ActionParameter("Common Parameter 2", String.Empty, DataType.text, True, "456")
                                   }
            Return New WebApiConfigurationBuilder().
                WithBaseUrl("https://www.myapi.org/").
                WithParameters(commonParameters).
                WithHeader("custom-header-1", "value 1").
                WithHeader("custom-header-2", "value 2").
                WithAction("Action1", HttpMethod.Get, "/action1", enableRequestOutputParameter:=enableRequestOutputParam,
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
                                                                 "Parameter 2 initial value"),
                                             New ActionParameter("Action 1 - Parameter 3",
                                                                 "",
                                                                 DataType.text,
                                                                 False,
                                                                 "Parameter 3 initial value")
                                         }).
                Build()
        End Function
    End Class
End Namespace


#End If




