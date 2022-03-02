#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore.Compilation
Imports BluePrism.AutomateProcessCore.WebApis.CustomCode
Imports BluePrism.Core.Utility
Imports NUnit.Framework

Namespace WebApis.CustomCode
    Public Class CustomCodeTestHelper

        Public Shared Function CreateCodeBuilder(Optional compiler As ICodeCompiler = Nothing,
                                  Optional cache As IObjectCache = Nothing) As CustomCodeBuilder

            cache = If(cache, New ObjectCache(New TestCacheStore()))
            Dim getCompiler = Function(codeLanguage As CodeLanguage)
                                  Return If(compiler, New CodeCompiler(codeLanguage, TestContext.CurrentContext.TestDirectory))
                              End Function
            Return New CustomCodeBuilder(getCompiler, cache)
        End Function

        Public Class TestCacheStore
            Implements ICacheStore

            Public ReadOnly Property Values As New Dictionary(Of String, Object)

            Public Sub Add(key As String, value As Object) Implements ICacheStore.Add
                Values(key) = value
            End Sub

            Public Function [Get](key As String) As Object Implements ICacheStore.[Get]
                Return Values.GetOrDefault(key)
            End Function

        End Class
    End Class
End Namespace
#End If
