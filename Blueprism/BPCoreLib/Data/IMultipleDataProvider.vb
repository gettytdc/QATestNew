Namespace Data

    ''' <summary>
    ''' An extension of the <see cref="IDataProvider"/> interface which enables a
    ''' table of data to be provided.
    ''' Note that this represents a one-way one-time traversal of the data; there is
    ''' no mechanism to reset the data pointer, or return to the data once the
    ''' traversal is complete.
    ''' </summary>
    Public Interface IMultipleDataProvider : Inherits IDataProvider

        ''' <summary>
        ''' Moves to the next row of data in the provider, returning a boolean value
        ''' indicating whether there is more data to be provided.
        ''' </summary>
        ''' <returns>True if the provider was moved and there is more data available,
        ''' false if there is no more data available.</returns>
        Function MoveNext() As Boolean

    End Interface

End Namespace
