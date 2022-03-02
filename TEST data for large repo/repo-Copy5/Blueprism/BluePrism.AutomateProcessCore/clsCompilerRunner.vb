Imports System.Reflection
Imports System.CodeDom.Compiler
Imports System.Runtime.InteropServices
Imports System.IO
Imports BluePrism.AutomateProcessCore.Compilation
Imports System.Linq
Imports BluePrism.AutomateProcessCore.Stages

''' <summary>
''' Handles compilation of the code within code stages of business objects
''' </summary>
''' <remarks>This is based on the clsCompilerRunner class that used to be used
''' to compile code stage code. The compilation logic has now been moved to the
''' <see cref="Compilation.CodeCompiler"/> class, but the public API of this class 
''' has been retained and is used by code stages in the same way as before.</remarks>
Public Class clsCompilerRunner : Implements IDisposable

    ''' <summary>
    ''' Holds a reference to the result of our attempt to compile the code
    ''' </summary>
    Private mCompiledCodeResult As CompiledCodeResult

    ''' <summary>
    ''' The parent process to which this compiler runner belongs. 
    ''' </summary>
    Private mParentProcess As clsProcess

    ''' <summary>
    ''' Holds a reference to the type of the class that contains the code stage's
    ''' compiled code.
    ''' </summary>
    Private mClass As Type
    
    ''' <summary>
    ''' Holds a reference to the instance of the class that contains the code stages
    ''' compiled code.
    ''' </summary>
    Private mInstance As Object

    ''' <summary>
    ''' The default constructor sets the parent process, and by default we use a
    ''' Visual Basic code provider.
    ''' </summary>
    ''' <param name="parentProcess"></param>
    Public Sub New(ByVal parentProcess As clsProcess)
        mParentProcess = parentProcess
    End Sub

    ''' <summary>
    ''' The <see cref="Compilation.CodeCompiler"/> used to compile the code stages
    ''' </summary>
    Private ReadOnly Property Compiler As Compilation.CodeCompiler
        Get
            Dim language As CodeLanguage
            Dim processInfo = GetParentProcessInfo()
            If Not String.IsNullOrEmpty(processInfo?.Language) Then
                language = CodeLanguage.GetByName(processInfo.Language)
            Else
                language = CodeLanguage.VisualBasic
            End If
            If mCodeCompiler IsNot Nothing Then
                If language = mCodeCompiler.Language Then
                    Return mCodeCompiler
                Else
                    ' Object's language has changed
                    mCodeCompiler.Dispose()
                End If
            End If
            mCodeCompiler = New Compilation.CodeCompiler(language)
            Return mCodeCompiler
        End Get
    End Property

    ''' <summary>
    ''' Gets the <see cref="clsProcessInfoStage"/> from the parent process
    ''' </summary>
    Private Function GetParentProcessInfo() As clsProcessInfoStage
        Dim stage = mParentProcess.GetStageByTypeAndSubSheet(StageTypes.ProcessInfo, mParentProcess.GetMainPage.ID)
        Return TryCast(stage, clsProcessInfoStage)
    End Function

    Private mCodeCompiler As Compilation.CodeCompiler = Nothing

    ''' <summary>
    ''' The name used to refer to the class that is created when compiling code for 
    ''' code stages within this object. This is based on the name of the parent 
    ''' process. Note that this name will be converted to a valid .NET identifier
    ''' and used for the class name within the compiled assembly.
    ''' </summary>
    Private ReadOnly Property CompiledClassName As String
        Get
            Return mParentProcess.Name
        End Get
    End Property

    ''' <summary>
    ''' Compiles all the code stages in the parent process.
    ''' </summary>
    ''' <param name="sErr">On failure, contains an error message.</param>
    ''' <returns>True if successful</returns>
    Public Function Compile(ByRef sErr As String) As Boolean
        Try
            mInstance = Nothing
            mClass = Nothing
            mCompiledCodeResult = Nothing

            Dim assemblyDefinition = CreateAssemblyDefinition()
            Dim processClassDefinition = assemblyDefinition.Classes.Single()
            Dim processClassName = processClassDefinition.Identifier

            If assemblyDefinition.IsEmpty Then Return True
            Dim cacheStore = New SessionCacheStore(Function() mParentProcess.Session)
            Dim cache As New ObjectCache(cacheStore)
            mCompiledCodeResult = Compiler.Compile(assemblyDefinition, cache)

            If mCompiledCodeResult.Errors.HasErrors Then
                sErr = My.Resources.Resources.clsCompilerRunner_CouldNotRunTheObjectBecauseOneOfTheCodeStagesHasACompileErrorUseCheckForErrorsF
                Return False
            End If
            mClass = mCompiledCodeResult.Assembly.GetType(processClassName)
            Return True

        Catch ex As ExternalException
            sErr = String.Format(My.Resources.Resources.clsCompilerRunner_0ErrorCode0MoreDetail1, ex.Message, ex.ErrorCode, ex.ToString)
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try

    End Function

    ''' <summary>
    ''' Maps the code content stored in the business object to a CodeContent object 
    ''' used to compile the code into an in-memory assembly
    ''' </summary>
    ''' <returns></returns>
    Private Function CreateAssemblyDefinition() As AssemblyDefinition
        Dim languageName = CodeLanguage.VisualBasic.Name
        Dim processInfo = GetParentProcessInfo()
        If Not processInfo Is Nothing AndAlso Not String.IsNullOrEmpty(processInfo.Language) Then
            languageName = processInfo.Language
        End If

        Dim name = mParentProcess.Name
        Dim language = If(CodeLanguage.GetByName(languageName), CodeLanguage.VisualBasic)
        Dim namespaces = processInfo.NamespaceImports
        Dim references = processInfo.AssemblyReferences
        Dim classDefinition As ClassDefinition = CreateClassDefinition(processInfo)
        Return New AssemblyDefinition(name, language, namespaces, references, {classDefinition})
    End Function

    ''' <summary>
    ''' Creates class definition. A single class is created for the object that 
    ''' contains a method for each code stage, together with the global code defined 
    ''' for the object
    ''' </summary>
    ''' <param name="infoStage">Shared code that is added to the class</param>
    Private Function CreateClassDefinition(infoStage As clsProcessInfoStage) As ClassDefinition
        Dim methods As New List(Of MethodDefinition)
        For Each codestage As clsCodeStage In mParentProcess.GetStages(Of clsCodeStage)()
            Dim method = CreateMethodDefinition(codestage)
            codestage.Compiled = True
            methods.Add(method)
        Next
        Dim sharedCodeFileName = infoStage.GetStageID.ToString
        Return New ClassDefinition(CompiledClassName, methods, infoStage.CodeText, sharedCodeFileName)
    End Function

    ''' <summary>
    ''' Creates a method based on the content of a code stage
    ''' </summary>
    Private Function CreateMethodDefinition(codeStage As clsCodeStage) As MethodDefinition
        Dim inputParameters = From input In codeStage.GetInputs
                              Select New MethodParameterDefinition(input.Name,
                                                                   input.GetDataType(),
                                                                   False)
        Dim outputParameters = From output In codeStage.GetOutputs
                               Select New MethodParameterDefinition(output.Name,
                                                                    output.GetDataType(),
                                                                    True)
        Dim parameters = inputParameters.Concat(outputParameters).ToList()

        AdjustCodeStageHeaderHeight(codeStage, parameters.Count)

        Dim fileName = codeStage.GetStageID.ToString()
        Return New MethodDefinition(codeStage.Name, fileName, codeStage.CodeText, parameters)
    End Function

    Private Sub AdjustCodeStageHeaderHeight(codeStage As clsCodeStage, parameterCount As Integer)
        ' Following logic preserved as-is during extraction of code compilation 
        ' functionality. HeaderHeight is used within ValidateCode to adjust line numbers
        ' when displaying errors.
        If parameterCount > 15 Then
            codeStage.HeaderHeight = parameterCount + 1
        Else
            codeStage.HeaderHeight = 1
        End If
    End Sub

    ''' <summary>
    ''' Initialises the code class 
    ''' </summary>
    ''' <param name="sErr">On failure, contains an error message.</param>
    ''' <returns>True if successful, False otherwise.</returns>
    Private Function Initialise(ByRef sErr As String) As Boolean
        Try
            If mClass IsNot Nothing Then
                mInstance = Activator.CreateInstance(mClass)
            Else
                sErr = My.Resources.Resources.clsCompilerRunner_CouldNotCreateAnInstanceOfTheCodeClassBecauseTheClassIsNotCompiled
                Return False
            End If
        Catch ex As Exception
            sErr = String.Format(My.Resources.Resources.clsCompilerRunner_CouldNotCreateAnInstanceOfTheCodeClassBecause0, ex.Message)
            Return False
        End Try
        Return True
    End Function

    ''' <summary>
    ''' Returns a list of compiler errors, by compiling the code and pulling all the errors out of the 
    ''' compiler results
    ''' </summary>
    ''' <param name="results"></param>
    Public Sub ValidateCode(ByVal results As ValidationErrorList)
        Dim sErr As String = Nothing
        Compile(sErr)

        Dim errors = mCompiledCodeResult?.Errors
        If errors?.HasErrors OrElse errors?.HasWarnings Then
            Dim processInfo = GetParentProcessInfo()
            For Each compilerError As CompilerError In errors
                Dim file As String = Path.GetFileName(compilerError.FileName)
                Dim res As ValidateProcessResult = Nothing
                If IsGuid(file) Then
                    Dim stage As clsProcessStage = mParentProcess.GetStage(New Guid(file))

                    Debug.Assert(stage IsNot Nothing, "Some code was generated that does not belong to a stage")

                    'Adjust the line number to take account of the method header
                    Dim line As Integer
                    If stage.StageType = StageTypes.Code Then
                        Dim codeStage As clsCodeStage = TryCast(stage, clsCodeStage)
                        line = compilerError.Line - codeStage.HeaderHeight
                    Else
                        line = compilerError.Line
                    End If

                    If compilerError.IsWarning Then
                        res = New ValidateProcessResult(stage, 123, line, compilerError.ErrorText)
                    Else
                        res = New ValidateProcessResult(stage, 124, line, compilerError.ErrorText)
                    End If
                ElseIf processInfo IsNot Nothing Then
                    ' Error is in the global code in the process info stage
                    Dim line As Integer
                    line = compilerError.Line - (processInfo.NamespaceImports.Count + 1)
                    If compilerError.IsWarning Then
                        res = New ValidateProcessResult(processInfo, 125, line, compilerError.ErrorText)
                    Else
                        res = New ValidateProcessResult(processInfo, 126, line, compilerError.ErrorText)
                    End If
                End If
                If res IsNot Nothing Then
                    res.ErrorSource = ValidateProcessResult.SourceTypes.Code
                    results.Add(res)
                End If
            Next
        End If

    End Sub

    ''' <summary>
    ''' Checks to see if the supplied string is the correct format to qualify as a
    ''' guid.
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns></returns>
    Private Function IsGuid(ByVal s As String) As Boolean

        Const H As String = "[A-Fa-f0-9]"
        Const HHHH As String = H & H & H & H
        Const HHHHHHHH As String = HHHH & HHHH
        Const HHHHHHHHHHHH As String = HHHHHHHH & HHHH

        Const Guid As String = HHHHHHHH & "-" & HHHH & "-" & HHHH & "-" & HHHH & "-" & HHHHHHHHHHHH

        If s.Length = 36 Then
            If s Like Guid Then
                Return True
            End If
        End If

        Return False
    End Function

    ''' <summary>
    ''' Executes the code in the code stage by calling the compiled .Net assembly
    ''' </summary>
    ''' <param name="objStage">The stage to execute</param>
    ''' <param name="inputs">A list of input arguments</param>
    ''' <param name="outputs">A list of output arguments, this will be
    ''' overwritten, but there should be sufficient number of argument
    ''' placeholders</param>
    ''' <param name="sErr">An Error message if the execution fails</param>
    ''' <returns></returns>
    Public Function Execute(ByVal objStage As clsCodeStage, ByVal inputs As clsArgumentList, ByVal outputs As clsArgumentList, ByRef sErr As String) As Boolean

        'Lazy initialisation for now.
        If mInstance Is Nothing Then
            If Not Initialise(sErr) Then Return False
        End If

        Dim methodName = Compilation.CodeCompiler.GetIdentifier(objStage.GetName)
        Try
            InvokeMethod(mInstance, methodName, inputs.ToArray(), outputs.ToArray())
        Catch ex As TargetInvocationException
            sErr = My.Resources.Resources.clsCompilerRunner_CouldNotExecuteCodeStageBecauseExceptionThrownByCodeStage & ex.InnerException.Message
            Return False
        Catch ex As Exception
            sErr = My.Resources.Resources.clsCompilerRunner_CouldNotExecuteCodeStage & ex.Message
            Return False
        End Try
        
        Return True
    End Function

    ''' <summary>
    ''' Converts a .Net value into a clsProcessValue
    ''' </summary>
    ''' <param name="val">The .Net value to convert</param>
    ''' <param name="dtype">We need to know the datatype of the value; fortunately
    ''' because of the way inputs an outputs are setup for code stages this
    ''' information will always be known</param>
    ''' <returns>A process value corresponding to the provided .net value</returns>
    Public Shared Function ConvertNetTypeToValue(
        val As Object, dtype As DataType) As clsProcessValue

        Return ProcessValueConvertor.ConvertNetTypeToValue(val, dtype)

    End Function

    ''' <summary>
    ''' Disposes of this runner.
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        mCodeCompiler?.Dispose()
        mCodeCompiler = Nothing
    End Sub

End Class
