Imports System.CodeDom.Compiler
Imports System.IO
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Compilation
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateProcessCore.WebApis.CustomCode

Namespace Controls.Widgets.SystemManager.WebApi.Request

    Public Module CodeValidationHelper


        Friend Sub Validate(commonCode As CodeProperties,
                            methodDefinitions As IEnumerable(Of MethodDefinition),
                            displaySuccess As Action(Of String), displayError As Action(Of String),
                            Optional compiler As Compilation.ICodeCompiler = Nothing)

            Dim className = ($"ValidationTest{Guid.NewGuid()}")
            Dim classDefinition = New ClassDefinition(className, methodDefinitions, commonCode.Code, SharedCodeFileName)
            Dim assemblyDefinition = New AssemblyDefinition(className + "Assembly", commonCode.Language,
                                                            commonCode.Namespaces, commonCode.References, {classDefinition})

            Using codeCompiler = If(compiler, New Compilation.CodeCompiler(assemblyDefinition.Language))
                Dim result = codeCompiler.Compile(assemblyDefinition, Nothing)
                DisplayResult(result, methodDefinitions, displaySuccess, displayError)
            End Using
        End Sub

        Private Sub DisplayResult(result As CompiledCodeResult,
                                  methodDefinitions As IEnumerable(Of MethodDefinition),
                                  displaySuccess As Action(Of String),
                                  displayError As Action(Of String))
            If Not result.Errors.HasErrors Then
                displaySuccess(WebApi_Resources.CodeValidation_SuccessMessage)
            Else
                Dim methodDefinitionNames = methodDefinitions.Select(Function(a) a.Name).ToArray()
                Dim messages = From compilerError In result.Errors.OfType(Of CompilerError)
                               Where Not compilerError.IsWarning
                               Select FormatErrorMessage(compilerError, methodDefinitionNames)
                Dim messageList = String.Join(Environment.NewLine, messages)
                Dim template = WebApi_Resources.CodeValidation_ErrorsFoundMessageTemplate
                Dim summary = String.Format(template, messageList)
                displayError(summary)
            End If
        End Sub


        Private Function FormatErrorMessage(compilerError As CompilerError, methodDefinitionNames As String()) As String
            Dim fileName = Path.GetFileNameWithoutExtension(compilerError.FileName)
            ' Pragma directives in the generated code can be used to link errors back 
            ' to the code from which they originate
            Dim isSharedCodeError = fileName = AssemblyDefinitionMapper.SharedCodeFileName
            Dim isActionCodeError = methodDefinitionNames.Contains(fileName)
            If isSharedCodeError Then
                Dim template = WebApi_Resources.CodeValidation_SharedCodeCompilerErrorTemplate
                Return String.Format(template, compilerError.Line, compilerError.ErrorText)
            ElseIf isActionCodeError Then
                ' Extra line generated for method header is not visible to user
                Dim adjustedLine = compilerError.Line - 1
                Dim template = WebApi_Resources.CodeValidation_RequestContentCodeCompilerErrorTemplate
                Return String.Format(template, fileName, adjustedLine, compilerError.ErrorText)
            Else
                ' Certain invalid code will upset overall structure and errors will be outside of
                ' method or custom code
                Dim template = WebApi_Resources.CodeValidation_GeneralCodeCompilerErrorTemplate
                Return String.Format(template, compilerError.ErrorText)
            End If
        End Function
    End Module

End Namespace