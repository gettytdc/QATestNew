Imports System.Runtime.CompilerServices

Namespace Collections

    Public Module CollectionExtensions
        ''' <summary>
        ''' Adds all the given <paramref name="elements"/> to <paramref name="this"/>
        ''' collection
        ''' </summary>
        ''' <typeparam name="T">The type of element contained in both collections.
        ''' </typeparam>
        ''' <param name="this">The collection to which the
        ''' <paramref name="elements"/> should be added. They will be added in
        ''' enumeration order.</param>
        ''' <param name="elements">The enumerable over the elements to be added to
        ''' the specified collection.</param>
        <Extension>
        Public Sub AddAll(Of T)(
         this As ICollection(Of T), elements As IEnumerable(Of T))
            For Each elem In elements : this.Add(elem) : Next
        End Sub

        ''' <summary>
        ''' Batches the elements in <paramref name="collection"/> by limiting each batch to <paramref name="maxItemsInBatch"/>
        ''' </summary>
        ''' <typeparam name="T">The type of element contained in both collections.
        ''' </typeparam>
        ''' <param name="collection">The collection for which the
        ''' <paramref name="maxItemsInBatch"/> should be the batch limit.</param>
        <Extension>
        Public Function Batch(Of T)(collection As IEnumerable(Of T), maxItemsInBatch As Integer) As IEnumerable(Of IEnumerable(Of T))
            Dim batchedItems = New List(Of List(Of T))

            If (maxItemsInBatch = 0) Then
                batchedItems.Add(collection.ToList())
            Else
                While (collection.Any())
                    Dim batchSelected = collection.Take(maxItemsInBatch).ToList()
                    collection = collection.Skip(maxItemsInBatch).ToList()
                    batchedItems.Add(batchSelected)
                End While
            End If

            Return batchedItems
        End Function
    End Module

End Namespace
