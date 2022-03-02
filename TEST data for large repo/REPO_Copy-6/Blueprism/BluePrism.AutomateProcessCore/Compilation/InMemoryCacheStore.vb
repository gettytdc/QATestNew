Imports BluePrism.Core.Utility

Namespace Compilation

    ''' <summary>
    ''' <see cref="ICacheStore"/> implementation that caches in memory, with scope 
    ''' local to specific instance
    ''' </summary>
    Public Class InMemoryCacheStore
        Implements ICacheStore

        Private ReadOnly mValues As New Dictionary(Of String, Object)

        Public Sub Add(key As String, value As Object) Implements ICacheStore.Add
            mValues(key) = value
        End Sub

        Public Function [Get](key As String) As Object Implements ICacheStore.[Get]
            Return mValues.GetOrDefault(key)
        End Function

    End Class

End Namespace