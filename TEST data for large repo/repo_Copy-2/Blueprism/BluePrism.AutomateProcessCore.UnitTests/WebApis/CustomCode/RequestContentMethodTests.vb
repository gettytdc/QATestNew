#If UNITTESTS Then

Imports System.Net.Http
Imports BluePrism.AutomateProcessCore.Compilation
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateProcessCore.WebApis.CustomCode
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent
Imports FluentAssertions
Imports Moq
Imports NUnit.Framework

Namespace WebApis.CustomCode
    Public Class RequestContentMethodTests

        Private mRequestContentMethod As RequestContentMethod

        Private mConfiguration As WebApiConfiguration
        Private mAction As WebApiAction
        Private mActionNoCode As WebApiAction
        Private mActionWithError As WebApiAction
        Private mActionReturnCommonCode As WebApiAction
        Private mCommonParameter_Exposed As ActionParameter
        Private mCommonParameter_NotExposed As ActionParameter
        Private mActionParameter_Exposed As ActionParameter
        Private mActionParameter_NotExposed As ActionParameter
        Private mRequestContentParameter As RequestContentOutputParameter = New RequestContentOutputParameter("Request_Content")
        Private mBuilder As CustomCodeBuilder
        Private ReadOnly mCode As String = "Request_Content = Common_Param_1 + Action_Param_1;"
        Private ReadOnly mWebApiId As Guid = Guid.NewGuid()
        Private ReadOnly mSession As New clsSession(
            Guid.NewGuid, 105, New WebConnectionSettings(5, 5, 5, New List(Of UriWebConnectionSettings)()))


        Private Function GetSimpleWebApiConfiguration() As WebApiConfiguration
            Dim namespaces = {"System", "System.Data"}
            Dim references = {"System.dll", "System.Data.dll"}
            Dim commonCode As New CodeProperties("int i = 0; // yeah custom code", CodeLanguage.CSharp, namespaces, references)

            Dim action1Content As New CustomCodeBodyContent(mCode)
            Dim actionNoCodeContent As New TemplateBodyContent("Some Template")
            Dim action2WithError As New CustomCodeBodyContent("throw new Exception(""Broken"");")
            Dim actionReturnCommonCodeContent As New CustomCodeBodyContent("Request_Content = i.ToString();")

            Return New WebApiConfigurationBuilder().
                                        WithCommonCode(commonCode).
                                        WithParameters({mCommonParameter_Exposed, mCommonParameter_NotExposed}).
                                        WithAction("Action 1", HttpMethod.Post, "/action1",
                                                   bodyContent:=action1Content, parameters:={mActionParameter_Exposed, mActionParameter_NotExposed}).
                                        WithAction("Action No Code", HttpMethod.Post, "/actionnocode",
                                                   bodyContent:=actionNoCodeContent, parameters:={mActionParameter_Exposed, mActionParameter_NotExposed}).
                                        WithAction("Action With Error", HttpMethod.Post, "/actionwitherror",
                                                    bodyContent:=action2WithError).
                                        WithAction("Action Return Common Code Variable", HttpMethod.Post, "/actionreturncommoncode",
                                                    bodyContent:=actionReturnCommonCodeContent).
                                        Build()


        End Function

        <SetUp>
        Public Sub SetUp()

            mCommonParameter_Exposed = New ActionParameter("Common Param 1", "", DataType.text, True, New clsProcessValue(DataType.text, ""))
            mCommonParameter_NotExposed = New ActionParameter("Common Param 2", "", DataType.text, False, New clsProcessValue(DataType.text, ""))

            mActionParameter_Exposed = New ActionParameter("Action Param 1", "", DataType.text, True, New clsProcessValue(DataType.text, ""))
            mActionParameter_NotExposed = New ActionParameter("Action Param 2", "", DataType.text, False, New clsProcessValue(DataType.text, ""))

            mConfiguration = GetSimpleWebApiConfiguration()

            mRequestContentMethod = New RequestContentMethod()

            mAction = mConfiguration.GetAction("Action 1")
            mActionNoCode = mConfiguration.GetAction("Action No Code")
            mActionWithError = mConfiguration.GetAction("Action With Error")
            mActionReturnCommonCode = mConfiguration.GetAction("Action Return Common Code Variable")

            Dim cache As New ObjectCache(New CustomCodeTestHelper.TestCacheStore())
            mBuilder = CustomCodeTestHelper.CreateCodeBuilder(cache:=cache)



        End Sub

        <Test>
        Public Sub GetParametersFromAction_ShouldOnlyReturnExposedParameters()

            Dim actionParameters = mRequestContentMethod.GetParametersFromAction(mAction, mConfiguration)
            Dim expectedResult = {mCommonParameter_Exposed, mActionParameter_Exposed}

            actionParameters.ShouldAllBeEquivalentTo(expectedResult)

        End Sub

        <Test>
        Public Sub GetCode_ShouldReturnExpectedValue()
            Dim code = mRequestContentMethod.GetCode(mAction)
            code.ShouldBeEquivalentTo(mCode)
        End Sub

        <Test>
        Public Sub GetAdditionalParameters_ShouldReturnCorrectParameters()
            Dim parameterMock1 = New Mock(Of IParameter)
            parameterMock1.Setup(Function(p) p.Name).Returns("Parameter 1")
            Dim parameterMock2 = New Mock(Of IParameter)
            parameterMock2.Setup(Function(p) p.Name).Returns("Parameter 2")

            Dim additionalParameters = mRequestContentMethod.GetAdditionalParameters({parameterMock1.Object, parameterMock2.Object})
            additionalParameters.ShouldBeEquivalentTo({mRequestContentParameter})
        End Sub

        <Test>
        Public Sub GetAdditionalParameters_ExistingParameterCalledRequestContent_ShouldReturnUniquelyNamedParameters()
            Dim additionalParameters = mRequestContentMethod.GetAdditionalParameters(
                New List(Of IParameter) From {New ActionParameter("Request Content", "", DataType.text, True, New clsProcessValue(DataType.text, ""))})
            Dim uniquelyNameParameter = New RequestContentOutputParameter("Request_Content_1")
            additionalParameters.ShouldBeEquivalentTo({uniquelyNameParameter})
        End Sub

        <Test>
        Public Sub GetAdditionalParameters_ExistingParameterCalledRequestContentWithUnderscore_ShouldReturnUniquelyNamedParameters()
            Dim additionalParameters = mRequestContentMethod.GetAdditionalParameters(
                New List(Of IParameter) From {New ActionParameter("Request_Content", "", DataType.text, True, New clsProcessValue(DataType.text, ""))})
            Dim uniquelyNameParameter = New RequestContentOutputParameter("Request_Content_1")
            additionalParameters.ShouldBeEquivalentTo({uniquelyNameParameter})
        End Sub

        <Test>
        Public Sub HasCode_WithCode_ShouldReturnTrue()
            mRequestContentMethod.HasCode(mAction).Should.Be(True)
        End Sub

        <Test>
        Public Sub HasCode_NoCode_ShouldReturnFalse()
            mRequestContentMethod.HasCode(mActionNoCode).Should.Be(False)
        End Sub

        <Test>
        Public Sub Invoke_WithParameters_ShouldReturnExpectedContent()
            Dim parameterValues As New Dictionary(Of String, clsProcessValue) From {{mCommonParameter_Exposed.Name, "Hi"},
                {mActionParameter_Exposed.Name, "There"}}
            Dim context = New ActionContext(mWebApiId, mConfiguration, mAction.Name, parameterValues, mSession)
            Dim assembly = mBuilder.GetAssembly(context)

            Dim content = mRequestContentMethod.Invoke(assembly, context)

            content.Should.Be("HiThere")
        End Sub

        <Test>
        Public Sub Invoke_CodeReturnsCommonCodeVariable_ShouldReturnExpectedContent()
            Dim parameterValues As New Dictionary(Of String, clsProcessValue) From {{mCommonParameter_Exposed.Name, "Hi"}}
            Dim context = New ActionContext(mWebApiId, mConfiguration, mActionReturnCommonCode.Name, parameterValues, mSession)
            Dim assembly = mBuilder.GetAssembly(context)

            Dim content = mRequestContentMethod.Invoke(assembly, context)

            content.Should.Be("0")
        End Sub

        <Test>
        Public Sub Invoke_CodeThrowsException_ShouldThrowException()
            Dim parameterValues As New Dictionary(Of String, clsProcessValue) From {{mCommonParameter_Exposed.Name, "Hi"}}
            Dim context = New ActionContext(mWebApiId, mConfiguration, mActionWithError.Name,
                                            parameterValues, mSession)
            Dim assembly = mBuilder.GetAssembly(context)

            mRequestContentMethod.
                Invoking(Function(x) x.Invoke(assembly, context)).
                ShouldThrow(Of InvalidCodeException)

        End Sub

    End Class

End Namespace

#End If
