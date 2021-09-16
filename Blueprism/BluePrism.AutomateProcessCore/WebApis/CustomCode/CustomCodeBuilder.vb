Imports System.CodeDom.Compiler
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports BluePrism.AutomateProcessCore.Compilation
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling

Namespace WebApis.CustomCode

    ''' <summary>
    ''' Handles the compiling custom code methods within a Web API
    ''' </summary>
    Public Class CustomCodeBuilder : Implements ICustomCodeBuilder

        Private ReadOnly mGetCompiler As Func(Of CodeLanguage, Compilation.ICodeCompiler)
        Private ReadOnly mCache As IObjectCache

        Sub New(getCompiler As Func(Of CodeLanguage, Compilation.ICodeCompiler), cache As IObjectCache)

            mGetCompiler = getCompiler
            mCache = cache
        End Sub

        Public Function GetAssembly(context As ActionContext) As AssemblyData Implements ICustomCodeBuilder.GetAssembly
            Return GetOrCreateAssemblyData(context)
        End Function

        Private Function GetOrCreateAssemblyData(context As ActionContext) _
            As AssemblyData
            Dim key = $"{GetType(CustomCodeBuilder).FullName}.CachedAssemblyData.{context.WebApiId}"
            Dim cachedData = mCache.[Get](Of AssemblyData)(key)
            If cachedData?.Assembly IsNot Nothing Then
                Return cachedData
            End If
            Dim data = CreateAssembly(context)
            mCache.Add(key, data)
            Return data
        End Function

        Private Function CreateAssembly(context As ActionContext) _
            As AssemblyData

            Dim assemblyDefinition = CreateAssemblyDefinition(context)
            Dim assembly = CompileAssembly(context, assemblyDefinition)
            Return New AssemblyData(assemblyDefinition, assembly)
        End Function

        ''' <summary>
        ''' Create a single assembly definition based on all of the custom code
        ''' defined in the specified Web Api's configuration. This will include
        ''' any common shared code stored against the Web API, a method for each action that
        ''' generates the HTTP Request body using code and a method for each action to
        ''' transform the HTTP Response body to output parameters.
        ''' </summary>
        Private Function CreateAssemblyDefinition(context As ActionContext) As AssemblyDefinition
            Dim className = $"Web API {context.WebApiId}"
            Return AssemblyDefinitionMapper.Create(className, context.Configuration)
        End Function

        Private Function CompileAssembly(context As ActionContext, assemblyDefinition As AssemblyDefinition) As Assembly
            Using compiler = mGetCompiler(assemblyDefinition.Language)
                ' No need to cache assembly based on source code, as we handle caching
                ' of assembly and AssemblyDefinition in GetOrCreateAssemblyData
                Dim result1 = compiler.Compile(assemblyDefinition, Nothing)
                Dim result = result1
                ThrowIfCompilationErrors(context, result)
                Return result.Assembly
            End Using
        End Function


        Private Sub ThrowIfCompilationErrors(context As ActionContext, result As CompiledCodeResult)
            If result.Errors.HasErrors Then
                Dim fileNamesWithErrors = result.Errors.OfType(Of CompilerError).
                        Where(Function(e) Not e.IsWarning).
                        Select(Function(e) Path.GetFileName(e.FileName)).
                        Distinct()
                Dim errorLocations = fileNamesWithErrors.Select(Function(f) GetCompilerErrorLocationTitle(f))
                Dim list = String.Join(Environment.NewLine, errorLocations)
                Dim message = String.Format(WebApiResources.CustomCodeRunTimeCompilationErrorTemplate,
                                            context.ActionName,
                                            list)
                Throw New InvalidCodeException(message)
            End If
        End Sub

        Private Function GetCompilerErrorLocationTitle(fileName As String) As String
            If fileName = AssemblyDefinitionMapper.SharedCodeFileName Then
                Return WebApiResources.SharedCodeLocationTitle
            Else
                Return String.Format(WebApiResources.CustomCodeLocationTitleTemplate, fileName)
            End If
        End Function

    End Class

End Namespace
