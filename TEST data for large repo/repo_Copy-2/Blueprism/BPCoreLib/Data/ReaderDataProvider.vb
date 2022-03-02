Namespace Data

    ''' <summary>
    ''' A data provider which provides <see cref="IDataProvider"/> access to an
    ''' <see cref="IDataReader"/> instance.
    ''' </summary>
    ''' <remarks>
    ''' This class expects any data traversal across rows to be managed using the
    ''' reader, or to be used in a <c>Do...Loop While Not prov.MoveNext()</c> loop
    ''' rather than the more typical <c>While prov.MoveNext()...End While</c> loop.
    ''' This is because it assumes that it is ready for data before the first
    ''' <see cref="IMultipleDataProvider.MoveNext"/> method call.
    ''' 
    ''' It is here primarily for backward compatibility as most of the database code
    ''' in the product was using it as a single row data provider, and doing any
    ''' traversal using the DataReader instance itself.
    ''' </remarks>
    Public Class ReaderDataProvider : Inherits BaseDataProvider

        ' The reader which we are retrieving the data from
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
        ''' Gets the data item with the given name or 'Nothing' if that item
        ''' did not exist within this provider.
        ''' </summary>
        ''' <param name="name">The name of the data item required.</param>
        Default Public Overrides ReadOnly Property Item(ByVal name As String) As Object
            Get
                Return Normalise(ReaderProviderUtil.GetItem(mReader, name))
            End Get
        End Property
    End Class

End Namespace
