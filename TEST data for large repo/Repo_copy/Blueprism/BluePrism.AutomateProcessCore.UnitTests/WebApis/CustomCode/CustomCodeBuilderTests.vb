#If UNITTESTS Then

Imports System.Net.Http
Imports BluePrism.AutomateProcessCore.Compilation
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent
Imports FluentAssertions
Imports Moq
Imports NUnit.Framework

Namespace WebApis.CustomCode

    Public Class CustomCodeBuilderTests

        Private Function CreateSimpleConfiguration(Optional commonCode As String = "int i = 0; // yeah custom code") As WebApiConfiguration

            Dim namespaces = {"System", "System.Data"}
            Dim references = {"System.dll", "System.Data.dll"}
            Dim sharedCode As New CodeProperties(commonCode, CodeLanguage.CSharp, namespaces, references)
            Dim action1Content As New CustomCodeBodyContent("Request_Content = String.Format(""Param 1: {0}, Param 2: {1}"", Param_1, Param_2);")

            Dim outputParameters As IEnumerable(Of ResponseOutputParameter) = {New CustomCodeOutputParameter("Custom Code Parameter 1", "Description", DataType.text),
                                                                              New CustomCodeOutputParameter("Custom Code Parameter 2", "Description", DataType.number)}
            Dim action1OutputParameterConfiguration = New OutputParameterConfiguration(outputParameters, "Custom_Code_Parameter_1 = Response_Content;
                                                                                                          Custom_Code_Parameter_2 = 2;")

            Dim action1Parameters = {New ActionParameter("Param 2", "", DataType.text, True, New clsProcessValue(DataType.text, ""))}
            Return New WebApiConfigurationBuilder().
                WithCommonCode(sharedCode).
                WithParameter("Param 1", DataType.text, True).
                WithAction("Action 1", HttpMethod.Post, "/action1",
                           bodyContent:=action1Content, parameters:=action1Parameters,
                           outputParameterConfiguration:=action1OutputParameterConfiguration).
                Build()

        End Function

        Private Shared Function CreateActionContext(configuration As WebApiConfiguration, actionName As String) _
            As ActionContext
            Dim parameters As New Dictionary(Of String, clsProcessValue) From
                    {{"Param 1", "Param 1 Value"}, {"Param 2", "Param 2 Value"}}
            Dim sessionNumber = 20
            Dim session = New clsSession(Guid.NewGuid, sessionNumber,
                                         New WebConnectionSettings(5, 5, 10, New List(Of UriWebConnectionSettings)()))
            Return New ActionContext(Guid.NewGuid, configuration, actionName, parameters, session)
        End Function

        <Test>
        Public Sub GetAssembly_ShouldCacheAssembly()
            Dim configuration = CreateSimpleConfiguration()
            Dim context As ActionContext = CreateActionContext(configuration, "Action 1")
            Dim store As New CustomCodeTestHelper.TestCacheStore()
            Dim cache As New ObjectCache(store)

            Dim builder = CustomCodeTestHelper.CreateCodeBuilder(cache:=cache)
            Dim content = builder.GetAssembly(context)

            store.Values.Keys.Should.HaveCount(1)

        End Sub

        <Test>
        Public Sub GetAssembly_CalledTwiceWithCachedAssembly_ShouldNotRecompile()
            Dim configuration = CreateSimpleConfiguration()
            Dim context As ActionContext = CreateActionContext(configuration, "Action 1")
            Dim cache As New ObjectCache(New CustomCodeTestHelper.TestCacheStore())
            ' Run once to cache data
            Dim builder1 = CustomCodeTestHelper.CreateCodeBuilder(cache:=cache)
            Dim assembly1 = builder1.GetAssembly(context)

            Dim compilerMock As New Mock(Of ICodeCompiler)
            Dim runner2 = CustomCodeTestHelper.CreateCodeBuilder(compilerMock.Object, cache)
            Dim additionalParameterValues = New Dictionary(Of String, clsProcessValue) From {{"Response_Content", "Expected Value"}}
            Dim assembly2 = runner2.GetAssembly(context)

            compilerMock.Verify(Function(c) c.Compile(It.IsAny(Of AssemblyDefinition), It.IsAny(Of IObjectCache)),
                                Times.Never)

        End Sub

        <Test>
        Public Sub GetAssembly_InvalidCode_ShouldThrowException()
            Dim configuration = CreateSimpleConfiguration("this code will not compile")
            Dim context As ActionContext = CreateActionContext(configuration, "Action 1")
            Dim store As New CustomCodeTestHelper.TestCacheStore()
            Dim cache As New ObjectCache(store)

            Dim builder = CustomCodeTestHelper.CreateCodeBuilder(cache:=cache)

            builder.Invoking(Sub(x) x.GetAssembly(context)).ShouldThrow(Of InvalidCodeException)

        End Sub

    End Class

End Namespace

#End If
