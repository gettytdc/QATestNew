Namespace Data

    ''' <summary>
    ''' Utility for <see cref="IDataProvider"/> implementations which delegate to an
    ''' <see cref="IDataReader"/> instance.
    ''' This just provides the boilerplate which can be used in both the single and
    ''' the multiple provider implementations without having to have the same code
    ''' copied across both class hierarchies.
    ''' </summary>
    Public Class ReaderProviderUtil

        ''' <summary>
        ''' Static class - no instantiation
        ''' </summary>
        Private Sub New()
        End Sub

        ''' <summary>
        ''' Gets the item from the backing data reader with the given name.
        ''' </summary>
        ''' <param name="reader">The data reader to draw the data from.</param>
        ''' <param name="name">The name of the object required.</param>
        ''' <exception cref="ObjectDisposedException">If the given reader is null.
        ''' This may seem incongruous, but the assumption is that a reader can only
        ''' ever be null if its owning object has been disposed, so it takes away the
        ''' necessity of dealing with that in the calling classes.</exception>
        Friend Shared Function GetItem( _
         ByVal reader As IDataReader, ByVal name As String) As Object
            If reader Is Nothing Then Throw New ObjectDisposedException("reader")
            ' The following is the quickest way to test for the existence of
            ' a field in the reader - there is no simple 'HasField()' function
            ' and the other ways (querying the schema on the reader or just
            ' trying to get the field and catching the exception) are too slow
            For i As Integer = 0 To reader.FieldCount - 1
                If reader.GetName(i).Equals( _
                 name, StringComparison.InvariantCultureIgnoreCase) Then
                    Return reader(i)
                End If
            Next
            Return Nothing
        End Function
    End Class

End Namespace
