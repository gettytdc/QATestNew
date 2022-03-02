Imports System.CodeDom
Imports System.CodeDom.Compiler
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Threading
Imports Microsoft.CodeDom.Providers.DotNetCompilerPlatform
Imports NLog

Namespace Compilation

    ''' <summary>
    ''' Compiles code at runtime into an assembly
    ''' </summary>
    Public Class CodeCompiler
        Implements ICodeCompiler

        Private Shared ReadOnly Log As Logger = LogManager.GetCurrentClassLogger()

        Private Shared mCompileMutex As Mutex = New Mutex(False, "BPCompile")

        Private mCodeDomProvider As CodeDomProvider = Nothing

        ''' <summary>
        ''' Creates a new <see cref="CodeCompiler"/>
        ''' </summary>
        ''' <param name="language">The code language used by the CodeCompiler</param>
        Public Sub New(language As CodeLanguage, Optional compilerBaseDirectory As String = Nothing)
            If compilerBaseDirectory Is Nothing Then
                compilerBaseDirectory = Path.GetDirectoryName(GetType(CodeCompiler).Assembly.Location)
            End If
            If language Is Nothing Then
                Throw New ArgumentNullException(NameOf(language))
            End If
            Me.Language = language
            mCSharpSettings = New CompilerSettings(compilerBaseDirectory, "csc.exe")
            mVBSettings = New CompilerSettings(compilerBaseDirectory, "vbc.exe")
        End Sub

        ''' <summary>
        ''' The language used for compilation
        ''' </summary>
        Public ReadOnly Property Language As CodeLanguage

        Private ReadOnly Property CodeDomProvider As CodeDomProvider
            Get
                If mCodeDomProvider Is Nothing Then
                    Select Case Language
                        Case CodeLanguage.CSharp
                            mCodeDomProvider = New CSharpCodeProvider(mCSharpSettings)
                        Case CodeLanguage.VisualBasic
                            mCodeDomProvider = New VBCodeProvider(mVBSettings)
                        Case Else
                            mCodeDomProvider = CodeDomProvider.CreateProvider(Language.Name)
                    End Select

                End If
                Return mCodeDomProvider
            End Get
        End Property

        Private Const DefaultCompilerServerTTL As Integer = 0

        Private mCSharpSettings As ICompilerSettings

        Private mVBSettings As ICompilerSettings

        Private Class CompilerSettings : Implements ICompilerSettings

            Public Sub New(compilerBaseDirectory As String, compilerName As String)
                CompilerFullPath = Path.Combine(compilerBaseDirectory, "roslyn", compilerName)
            End Sub

            Public ReadOnly Property CompilerFullPath As String Implements ICompilerSettings.CompilerFullPath

            Public ReadOnly Property CompilerServerTimeToLive As Integer Implements ICompilerSettings.CompilerServerTimeToLive
                Get
                    Return DefaultCompilerServerTTL
                End Get
            End Property
        End Class

        ''' <inheritdoc />
        Public Function Compile(assemblyDefinition As AssemblyDefinition, cache As IObjectCache) As CompiledCodeResult Implements ICodeCompiler.Compile

            If assemblyDefinition.IsEmpty Then Return CompiledCodeResult.Empty

            Dim compileUnit As CodeCompileUnit = CreateCompileUnit(assemblyDefinition)
            Dim sourceCode As String = GenerateCode(compileUnit)
            Dim result = cache?.Get(Of CompiledCodeResult)(sourceCode)
            If result IsNot Nothing Then
                Return result
            End If
            Dim results = CompileAssembly(assemblyDefinition, compileUnit)

            If results.Errors.HasErrors Then
                Return New CompiledCodeResult(Nothing, sourceCode, results.Errors)
            Else
                Dim assembly = results.CompiledAssembly
                result = New CompiledCodeResult(assembly, sourceCode, results.Errors)
                cache?.Add(result.SourceCode, result)
                Return result
            End If

        End Function

        Private Function CreateCompileUnit(assemblyDefinition As AssemblyDefinition) As CodeCompileUnit
            ' Main compile unit and root namespace
            Dim compileUnit As New CodeCompileUnit
            Dim rootNamespace As New CodeNamespace
            compileUnit.Namespaces.Add(rootNamespace)

            ' Imports
            Dim namespaceImports = assemblyDefinition.Namespaces.
                Where(Function(n)Not String.IsNullOrEmpty(n)).
                Select(Function(n)New CodeNamespaceImport(n)).
                ToArray()
            rootNamespace.Imports.AddRange(namespaceImports)

            ' Types
            Dim types = assemblyDefinition.Classes.
                    Select(Function(c) CreateType(c)).
                    ToArray()
            rootNamespace.Types.AddRange(types)
            Return compileUnit
        End Function

        Private Function CreateType(classDefinition As ClassDefinition) As CodeTypeDeclaration
            Dim typeDeclaration As New CodeTypeDeclaration(classDefinition.Identifier)

            ' Shared code
            If Not String.IsNullOrWhiteSpace(classDefinition.SharedCode) Then
                Dim sharedCodeSnippet As New CodeSnippetTypeMember(classDefinition.SharedCode)
#If Not DEBUGCODESTAGES Then
                sharedCodeSnippet.LinePragma = New CodeLinePragma(classDefinition.SharedCodeFileName, 1)
#End If
                typeDeclaration.Members.Add(sharedCodeSnippet)
            End If

            ' Methods
            Dim methods = classDefinition.Methods.
                    Select(Function(md)CreateMethod(md)).
                    ToArray()
            typeDeclaration.Members.AddRange(methods)
            Return typeDeclaration
        End Function

        Private Function CreateMethod(method As MethodDefinition) As CodeMemberMethod
            Dim memberMethod As New CodeMemberMethod
            memberMethod.Name = GetIdentifier(method.Name)
            memberMethod.Attributes = MemberAttributes.Public Or MemberAttributes.Final
            memberMethod.Statements.Add(New CodeSnippetStatement(method.Body))
#If Not DEBUGCODESTAGES Then
            memberMethod.LinePragma = New CodeLinePragma(method.FileName, 1)
#End If
            Dim parameters = method.Parameters.
                    Select(Function(p) CreateParameter(p)).
                    Where(Function(p) p IsNot Nothing).
                    ToArray()
            memberMethod.Parameters.AddRange(parameters)
            Return memberMethod
        End Function

        Private Function CreateParameter(parameter As MethodParameterDefinition) As CodeParameterDeclarationExpression
            Dim dotNetType = clsProcessDataTypes.GetFrameworkEquivalentFromDataType(parameter.DataType)
            ' Preserves logic from earlier implementation - don't create for invalid type
            If dotNetType Is Nothing Then Return Nothing

            Dim parameterDeclaration As New CodeParameterDeclarationExpression(dotNetType, GetIdentifier(parameter.Name))
            If parameter.IsOutput Then
                parameterDeclaration.Direction = FieldDirection.Out
            End If
            Return parameterDeclaration
        End Function

        Private Function GenerateCode(unit As CodeCompileUnit) As String
            Using writer As New StringWriter
                Dim options As New CodeGeneratorOptions()
                CodeDomProvider.GenerateCodeFromCompileUnit(unit, writer, options)
                Return writer.ToString()
            End Using
        End Function

        Private Function CompileAssembly(assemblyDefinition As AssemblyDefinition, compileUnit As CodeCompileUnit) As CompilerResults

#If DEBUGCODESTAGES Then
            Dim compilerParameters As New CompilerParameters With
            {
                .GenerateInMemory = False,
                .IncludeDebugInformation = True,
                .TempFiles = New TempFileCollection With {
                    .KeepFiles = True
                }
      
            }
#Else
            Dim compilerParameters As New CompilerParameters With
            {
                .GenerateInMemory = True
            }
#End If
            Dim referencedAssemblies = assemblyDefinition.AssemblyReferences.
                    Select(Function(ar) ResolveAssemblyPath(ar)).
                    Where(Function(ar) Not String.IsNullOrEmpty(ar)).
                    ToArray()
            compilerParameters.ReferencedAssemblies.AddRange(referencedAssemblies)
            If TypeOf CodeDomProvider Is VBCodeProvider Then
                compilerParameters.WarningLevel = Integer.MaxValue
                'Needed because of RoslynCodeDomProvider issue
                compilerParameters.CompilerOptions = "/define:_MYTYPE=""Windows"""
            End If

            Try
                Try
                    mCompileMutex.WaitOne()
                Catch ex As AbandonedMutexException
                    Log.Warn($"Found mutex in abandoned state. Logging and continuing.", ex)
                End Try

                Return CodeDomProvider.CompileAssemblyFromDom(compilerParameters, {compileUnit})
            Finally 'Ensure we release the mutex
                mCompileMutex.ReleaseMutex()
            End Try
        End Function

        ''' <summary>
        ''' Resolves locations with no path given to the blueprism directory.
        ''' </summary>
        ''' <param name="location">The location to resolve</param>
        ''' <returns>The full path</returns>
        Private Function ResolveAssemblyPath(location As String) As String
            Static appPath As String = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)

            'Check whether the given location is just a filename
            Dim fileName = Path.GetFileName(location)
            If fileName = location Then
                'See if the file exists in the application path.
                Dim resolved = Path.Combine(appPath, fileName)
                If File.Exists(resolved) Then Return resolved
            End If

            Return location
        End Function

        ''' <summary>
        ''' Converts a name to a valid identifier for a parameter or method
        ''' </summary>
        ''' <param name="name">The name that we want to escape</param>
        ''' <returns>An escaped name</returns>
        Public Shared Function GetIdentifier(ByVal name As String) As String
            If String.IsNullOrEmpty(name) Then
                Return String.Empty
            End If

            Dim prototype As String = String.Empty
            For Each c As Char In name
                If Char.IsLetterOrDigit(c) Then
                    prototype &= c
                Else
                    prototype &= "_"c
                End If
            Next
            If Not Char.IsLetter(prototype.Chars(0)) Then
                prototype = "_" & prototype
            End If

            'No need to check for kewords in our name, as codedom does this for us

            Return prototype
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            mCodeDomProvider?.Dispose()
        End Sub
    End Class
End Namespace