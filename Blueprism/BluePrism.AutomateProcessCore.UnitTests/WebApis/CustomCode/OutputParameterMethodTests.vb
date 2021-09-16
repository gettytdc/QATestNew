#If UNITTESTS Then

Imports System.Linq
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

    Public Class OutputParameterMethodTests

        Private mOutputParameterMethod As OutputParameterMethod

        Private mConfiguration As WebApiConfiguration
        Private mActionWithCode As WebApiAction
        Private mActionWithoutCode As WebApiAction
        Private mActionWithError As WebApiAction
        Private mCustomCodeOutputParameter1 As CustomCodeOutputParameter
        Private mCustomCodeOutputParameter2 As CustomCodeOutputParameter
        Private mCustomCodeOutputParameter3 As CustomCodeOutputParameter
        Private mJsonPathOutputParameter As JsonPathOutputParameter
        Private mResponseContentParameter As ResponseContentInputParameter = New ResponseContentInputParameter("Response_Content")
        Private mBuilder As CustomCodeBuilder
        Private ReadOnly mCode As String = "Custom_Code_Parameter_1 = Response_Content; Custom_Code_Parameter_2 = 2; Custom_Code_Parameter_3=i;"
        Private ReadOnly mWebApiId As Guid = Guid.NewGuid()
        Private ReadOnly mSession As New clsSession(
            Guid.NewGuid, 105, New WebConnectionSettings(5, 5, 5, New List(Of UriWebConnectionSettings)()))


        Private Function GetSimpleWebApiConfiguration() As WebApiConfiguration
            Dim namespaces = {"System", "System.Data"}
            Dim references = {"System.dll", "System.Data.dll"}
            Dim commonCode As New CodeProperties("int i = 0; // yeah custom code", CodeLanguage.CSharp, namespaces, references)

            Dim outputParameters As IEnumerable(Of ResponseOutputParameter) = {mCustomCodeOutputParameter1,
                mCustomCodeOutputParameter2, mCustomCodeOutputParameter3, mJsonPathOutputParameter}

            Dim action1OutputParameterConfiguration = New OutputParameterConfiguration(outputParameters, mCode)
            Dim actionNoCodeOutputParameterConfiguration = New OutputParameterConfiguration({mJsonPathOutputParameter}, "")
            Dim actionWithErrorOutputParameterConfiguration = New OutputParameterConfiguration(outputParameters, ("throw new Exception(""Broken"");"))

            Return New WebApiConfigurationBuilder().
                                        WithCommonCode(commonCode).
                                        WithAction("Action 1", HttpMethod.Post, "/action1",
                                                   bodyContent:=New TemplateBodyContent(""),
                                                   outputParameterConfiguration:=action1OutputParameterConfiguration).
                                        WithAction("Action without code", HttpMethod.Post, "/action2",
                                                   bodyContent:=New TemplateBodyContent(""),
                                                   outputParameterConfiguration:=actionNoCodeOutputParameterConfiguration).
                                        WithAction("Action with error", HttpMethod.Post, "/actionwitherror",
                                                   bodyContent:=New TemplateBodyContent(""),
                                                   outputParameterConfiguration:=actionWithErrorOutputParameterConfiguration).
                                        Build()

        End Function

        <SetUp>
        Public Sub SetUp()

            mCustomCodeOutputParameter1 = New CustomCodeOutputParameter("Custom Code Parameter 1", "Description", DataType.text)
            mCustomCodeOutputParameter2 = New CustomCodeOutputParameter("Custom Code Parameter 2", "Description", DataType.number)
            mCustomCodeOutputParameter3 = New CustomCodeOutputParameter("Custom Code Parameter 3", "Description", DataType.number)
            mJsonPathOutputParameter = New JsonPathOutputParameter("Json Path Parameter", "$", "Description", DataType.text)

            mConfiguration = GetSimpleWebApiConfiguration()

            mOutputParameterMethod = New OutputParameterMethod()

            mActionWithCode = mConfiguration.GetAction("Action 1")
            mActionWithoutCode = mConfiguration.GetAction("Action without code")
            mActionWithError = mConfiguration.GetAction("Action with error")

            Dim cache As New ObjectCache(New CustomCodeTestHelper.TestCacheStore())
            mBuilder = CustomCodeTestHelper.CreateCodeBuilder(cache:=cache)
        End Sub

        <Test>
        Public Sub GetParametersFromAction_ShouldReturnCorrectParameters()
            Dim actionParameters = mOutputParameterMethod.GetParametersFromAction(mActionWithCode, mConfiguration)
            Dim expectedResult = {mCustomCodeOutputParameter1, mCustomCodeOutputParameter2, mCustomCodeOutputParameter3}
            actionParameters.ShouldAllBeEquivalentTo(expectedResult)

        End Sub

        <Test>
        Public Sub GetParametersFromAction_IncludesJsonParameters_ShouldReturnOnlyCustomCodeParameters()
            Dim actionParameters = mOutputParameterMethod.GetParametersFromAction(mActionWithCode, mConfiguration)

            actionParameters.
                OfType(Of JsonPathOutputParameter).
                Any().Should.BeFalse()
        End Sub

        <Test>
        Public Sub GetAdditionalParameters_ExistingParametersDoNotContainResponseContent_ShouldReturnResponseContentParemeter()
            Dim additionalParameters = mOutputParameterMethod.GetAdditionalParameters(
                New List(Of IParameter) From {New ActionParameter("Parameter 1", "", DataType.text, True, New clsProcessValue(DataType.text, ""))})

            additionalParameters.ShouldBeEquivalentTo({mResponseContentParameter})
        End Sub

        <Test>
        Public Sub GetAdditionalParameters_ExistingParametersContainResponseContent_ShouldReturnUniquelyNamedParameters()
            Dim additionalParameters = mOutputParameterMethod.GetAdditionalParameters(
                New List(Of IParameter) From {New CustomCodeOutputParameter("Response_Content", "Description", DataType.text)})
            Dim uniquelyNameParameter = New ResponseContentInputParameter("Response_Content_1")
            additionalParameters.ShouldBeEquivalentTo({uniquelyNameParameter})
        End Sub

        <Test>
        Public Sub GetAdditionalParameters_ExistingParametersContainCodeIdentifierEquivalent_ShouldReturnUniquelyNamedParameters()
            Dim additionalParameters = mOutputParameterMethod.GetAdditionalParameters(
                New List(Of IParameter) From {New CustomCodeOutputParameter("Response Content", "Description", DataType.text)})
            Dim uniquelyNameParameter = New ResponseContentInputParameter("Response_Content_1")
            additionalParameters.ShouldBeEquivalentTo({uniquelyNameParameter})
        End Sub

        <Test>
        Public Sub GetResponseContentParameterName_ExistingParametersDoNotContainResponseContent_ShouldReturnCurrentParameter()
            Dim parameterMock1 = New Mock(Of IParameter)
            parameterMock1.Setup(Function(p) p.Name).Returns("Parameter 1")
            Dim parameterName = OutputParameterMethod.GetResponseContentParameterName({parameterMock1.Object})

            parameterName.Should.Be("Response_Content")

        End Sub

        <Test>
        Public Sub GetResponseContentParameterName_ExistingParametersContainResponseContent_ShouldReturnuniqueParameter()
            Dim parameterMock1 = New Mock(Of IParameter)
            parameterMock1.Setup(Function(p) p.Name).Returns("Response_Content")
            Dim parameterName = OutputParameterMethod.GetResponseContentParameterName({parameterMock1.Object})

            parameterName.Should.Be("Response_Content_1")

        End Sub

        <Test>
        Public Sub GetResponseContentParameterName_ExistingParametersContainCodeIdentifierEquivalent_ShouldReturnuniqueParameter()
            Dim parameterMock1 = New Mock(Of IParameter)
            parameterMock1.Setup(Function(p) p.Name).Returns("Response Content")
            Dim parameterName = OutputParameterMethod.GetResponseContentParameterName({parameterMock1.Object})

            parameterName.Should.Be("Response_Content_1")
        End Sub


        <Test>
        Public Sub GetCode_ShouldReturnExpectedValue()
            Dim code = mOutputParameterMethod.GetCode(mActionWithCode)
            code.ShouldBeEquivalentTo(mCode)
        End Sub

        <Test>
        Public Sub CreateMethods_SingleMethod_ShouldSetCorrectMethodName()
            Dim method = mOutputParameterMethod.CreateMethods(mConfiguration).FirstOrDefault()
            method.Name.Should.Be("GenerateOutputParameters Action 1")

        End Sub

        <Test>
        Public Sub CreateMethod_SingleMethod_ShouldSetCorrectCode()
            Dim method = mOutputParameterMethod.CreateMethods(mConfiguration).FirstOrDefault()
            method.Body.Should.Be(mCode)
        End Sub

        <Test>
        Public Sub HasCode_WithCode_ShouldReturnTrue()
            mOutputParameterMethod.HasCode(mActionWithCode).Should.Be(True)
        End Sub

        <Test>
        Public Sub HasCode_NoCode_ShouldReturnFalse()
            mOutputParameterMethod.HasCode(mActionWithoutCode).Should.Be(False)
        End Sub

        <Test>
        Public Sub Invoke_ParameterUsesContentToGetValue_ShouldReturnExpectedParameter()

            Dim context = New ActionContext(mWebApiId, mConfiguration, mActionWithCode.Name,
                                            New Dictionary(Of String, clsProcessValue), mSession)

            Dim arguments = mOutputParameterMethod.Invoke(mBuilder, context, "This is the response")

            Dim parameter1Value = arguments.Single(Function(x) x.Name = "Custom Code Parameter 1").Value
            parameter1Value.Should.Be("This is the response")
        End Sub

        <Test>
        Public Sub Invoke_ParameterWithNumberDataType_ShouldReturnExpectedParameter()

            Dim context = New ActionContext(mWebApiId, mConfiguration, mActionWithCode.Name,
                                            New Dictionary(Of String, clsProcessValue), mSession)

            Dim arguments = mOutputParameterMethod.Invoke(mBuilder, context, "This is the response")

            Dim parameter1Value = arguments.Single(Function(x) x.Name = "Custom Code Parameter 2").Value
            parameter1Value.Should.Be(New clsProcessValue(2))
        End Sub

        <Test>
        Public Sub Invoke_ParameterUsesCommonCodeToGetValue_ShouldReturnExpectedParameter()

            Dim context = New ActionContext(mWebApiId, mConfiguration, mActionWithCode.Name,
                                            New Dictionary(Of String, clsProcessValue), mSession)

            Dim arguments = mOutputParameterMethod.Invoke(mBuilder, context, "This is the response")

            Dim parameter1Value = arguments.Single(Function(x) x.Name = "Custom Code Parameter 3").Value
            parameter1Value.Should.Be(New clsProcessValue(0))
        End Sub


        <Test>
        Public Sub Invoke_CodeThrowsException_ShouldThrowException()

            Dim context = New ActionContext(mWebApiId, mConfiguration, mActionWithError.Name,
                                            New Dictionary(Of String, clsProcessValue), mSession)


            mOutputParameterMethod.
                Invoking(Function(x) x.Invoke(mBuilder, context, "This is the response")).
                ShouldThrow(Of InvalidCodeException)

        End Sub


    End Class

End Namespace

#End If
