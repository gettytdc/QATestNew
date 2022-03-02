Namespace Data

    ''' <summary>
    ''' Data Provider which uses a IDataReader as its backing store.
    ''' The reader can be manipulated separately to this class, and it will read
    ''' using the current state of the reader. For example, you can create a provider
    ''' around an SqlDataReader, and move through the reader using 
    ''' <see cref="IDataReader.Read"/>, and use the same provider to
    ''' read each row that's returned - ie. you don't need to create a new provider
    ''' for each row.
    ''' </summary>
    Public Class ReaderMultiDataProvider
        Inherits BaseMultipleDataProvider : Implements IDisposable

        ''' <summary>
        ''' The reader from where this provider draws its data
        ''' </summary>
        Private mReader As IDataReader

        ''' <summary>
        ''' Creates a new provider using the data from the given reader
        ''' </summary>
        ''' <param name="reader">The reader from which this provider should draw its
        ''' data.</param>
        ''' <exception cref="ArgumentNullException">If the given data reader is null.
        ''' </exception>
        Public Sub New(ByVal reader As IDataReader)
            If reader Is Nothing Then Throw New ArgumentNullException(NameOf(reader))
            mReader = reader
        End Sub

        ''' <summary>
        ''' Gets the item from the backing data reader with the given name.
        ''' </summary>
        ''' <param name="name">The name of the object required.</param>
        ''' <exception cref="ObjectDisposedException">If this provider has been
        ''' disposed of.</exception>
        Protected Overrides Function GetItem(ByVal name As String) As Object
            Return ReaderProviderUtil.GetItem(mReader, name)
        End Function

        ''' <summary>
        ''' Moves to the next row of data in the reader, returning a boolean value
        ''' indicating whether there is more data to be provided.
        ''' </summary>
        ''' <returns>True if the provider was moved and there is more data available,
        ''' false if there is no more data available.</returns>
        ''' <exception cref="ObjectDisposedException">If this provider has been
        ''' disposed of.</exception>
        Protected Overrides Function InnerMoveNext() As Boolean
            If mReader Is Nothing Then Throw New ObjectDisposedException("reader")
            Return mReader.Read()
        End Function

        ''' <summary>
        ''' Moves the underlying reader to the next result when reading from SQL
        ''' batches.
        ''' </summary>
        ''' <returns>True if there were more results to read in the underlying
        ''' reader; False otherwise.</returns>
        Public Function NextResult() As Boolean
            Dim hasResults As Boolean = mReader.NextResult()
            If hasResults Then State = DataTraversalState.BeforeStart
            Return hasResults
        End Function

        ''' <summary>
        ''' Disposes of this provider, effectively disabling this object. Any further
        ''' use of this class will result in an <see cref="ObjectDisposedException"/>
        ''' being thrown.
        ''' </summary>
        ''' <remarks>This method disposes of the reader it was created with, and that
        ''' is all that it does. If the reader is required for further use, then this
        ''' method should not be called.
        ''' </remarks>
        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

        Private mDisposed As Boolean

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not mDisposed Then
                If disposing Then
                    mReader.Dispose()
                    mReader = Nothing
                End If
            End If
            mDisposed = True
        End Sub

        Protected Overrides Sub Finalize()
            Dispose(False)
            MyBase.Finalize()
        End Sub


    End Class

End Namespace