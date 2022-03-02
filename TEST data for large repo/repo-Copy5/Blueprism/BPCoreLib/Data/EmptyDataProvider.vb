Namespace Data

    ''' <summary>
    ''' An empty data provider which can be used to initialise an object which takes
    ''' its data from a provider - it will always return the default value for any
    ''' requested data.
    ''' </summary>
    Public Class EmptyDataProvider : Inherits BaseDataProvider

        ''' <summary>
        ''' A shared instance of an empty data provider
        ''' </summary>
        Public Shared Instance As IDataProvider = New EmptyDataProvider()

        ''' <summary>
        ''' Gets the data item with the given name or 'Nothing' if that item
        ''' did not exist within this provider.
        ''' </summary>
        ''' <param name="name">The name of the data item required.</param>
        ''' <remarks>This implementation will always return Nothing</remarks>
        Default Public Overrides ReadOnly Property Item(ByVal name As String) As Object
            Get
                Return Nothing
            End Get
        End Property
    End Class

End Namespace
