Imports System.Linq
Imports System.Net.Http
Imports BluePrism.AutomateProcessCore.Compilation
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateProcessCore.WebApis.CustomCode
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent
Imports FluentAssertions
Imports NUnit.Framework

Namespace WebApis.CustomCode

    Public Class AssemblyDefinitionMapperTests
        Private mConfiguration As WebApiConfiguration
        Private mNamespaces As String()
        Private mReferences As String()
        Private mCommonCode As CodeProperties
        Private Const Action1RequestContentCode = "Request_Content = String.Format(""Param 1: {0}, Param 2: {1}"", Param_1, Param_2);"
        Private Const ExceptionCode = "throw new Exception(""Broken"");"

        Private Function CreateSimpleConfiguration() As WebApiConfiguration

            mNamespaces = {"System", "System.Data"}
            mReferences = {"System.dll", "System.Data.dll"}
            mCommonCode = New CodeProperties("int i = 0; // yeah custom code", CodeLanguage.CSharp, mNamespaces, mReferences)
            Dim action1Content As New CustomCodeBodyContent("Request_Content = String.Format(""Param 1: {0}, Param 2: {1}"", Param_1, Param_2);")

            Dim action2Content As New CustomCodeBodyContent(ExceptionCode)

            Dim outputParameters As IEnumerable(Of ResponseOutputParameter) = {New CustomCodeOutputParameter("Custom Code Parameter 1", "Description", DataType.text),
                                                                              New CustomCodeOutputParameter("Custom Code Parameter 2", "Description", DataType.number)}
            Dim action1OutputParameterConfiguration = New OutputParameterConfiguration(outputParameters, "Custom_Code_Parameter_1 = Response_Content;
                                                                                                          Custom_Code_Parameter_2 = 2;")
            Dim action2OutputParameterConfiguration = New OutputParameterConfiguration(outputParameters, ExceptionCode)

            Dim action1Parameters = {New ActionParameter("Param 2", "", DataType.text, True, New clsProcessValue(DataType.text, ""))}
            Dim action2Parameters = {New ActionParameter("Param 2", "", DataType.text, True, New clsProcessValue(DataType.text, ""))}
            Return New WebApiConfigurationBuilder().
                WithCommonCode(mCommonCode).
                WithParameter("Param 1", DataType.text, True).
                WithAction("Action 1", HttpMethod.Post, "/action1",
                           bodyContent:=action1Content, parameters:=action1Parameters,
                           outputParameterConfiguration:=action1OutputParameterConfiguration).
                WithAction("Action With Error", HttpMethod.Post, "/action2",
                           bodyContent:=action2Content, parameters:=action2Parameters,
                           outputParameterConfiguration:=action2OutputParameterConfiguration).
                Build()

        End Function

        <SetUp>
        Public Sub SetUp()
            mConfiguration = CreateSimpleConfiguration()
        End Sub

        <Test>
        Public Sub Create_ShouldCreateClassName()
            Dim assembly = AssemblyDefinitionMapper.Create("SomeClass", mConfiguration)
            assembly.Classes.Single().Name.Should.Be("SomeClass")
        End Sub

        <Test>
        Public Sub Create_ShouldCorrectlySetSharedCode()
            Dim assembly = AssemblyDefinitionMapper.Create("SomeClass", mConfiguration)
            assembly.Classes.Single().SharedCode.Should.Be(mCommonCode.Code)
        End Sub

        <Test>
        Public Sub Create_ShouldSetLanguage()
            Dim assembly = AssemblyDefinitionMapper.Create("SomeClass", mConfiguration)
            assembly.Language.Should.Be(mCommonCode.Language)
        End Sub

        <Test>
        Public Sub Create_ShouldSetReferences()
            Dim assembly = AssemblyDefinitionMapper.Create("SomeClass", mConfiguration)
            assembly.AssemblyReferences.ShouldAllBeEquivalentTo(mReferences)
        End Sub

        <Test>
        Public Sub Create_ShouldSetNamespaces()
            Dim assembly = AssemblyDefinitionMapper.Create("SomeClass", mConfiguration)
            assembly.Namespaces.ShouldAllBeEquivalentTo(mNamespaces)
        End Sub

        <Test>
        Public Sub Create_ShouldCreateRequestContentMethodForEachAction()
            Dim assembly = AssemblyDefinitionMapper.Create("SomeClass", mConfiguration)
            assembly.
                  Classes.
                  Single().
                  Methods.
                  Where(Function(x) x.Name.StartsWith("GenerateRequestContent")).
                  Should.
                  HaveCount(2)

        End Sub

        <Test>
        Public Sub Create_ShouldOutputParametersMethodForEachAction()
            Dim assembly = AssemblyDefinitionMapper.Create("SomeClass", mConfiguration)
            assembly.
                  Classes.
                  Single().
                  Methods.
                  Where(Function(x) x.Name.StartsWith("GenerateOutputParameters")).
                  Should.
                  HaveCount(2)

        End Sub

        <Test>
        Public Sub Create_ShouldSetCorrectOutputParameterCode()
            Dim assembly = AssemblyDefinitionMapper.Create("SomeClass", mConfiguration)
            Dim outputParameterCode = assembly.
                                        Classes.
                                        Single().
                                        Methods.
                                        Single(Function(x) x.Name.StartsWith("GenerateOutputParameters Action With Error")).
                                        Body

            outputParameterCode.Should.Be(ExceptionCode)

        End Sub

        <Test>
        Public Sub Create_ShouldSetCorrectRequestContentCode()
            Dim assembly = AssemblyDefinitionMapper.Create("SomeClass", mConfiguration)
            Dim outputParameterCode = assembly.
                                        Classes.
                                        Single().
                                        Methods.
                                        Single(Function(x) x.Name.StartsWith("GenerateRequestContent Action 1")).
                                        Body

            outputParameterCode.Should.Be(Action1RequestContentCode)

        End Sub



    End Class

End Namespace
