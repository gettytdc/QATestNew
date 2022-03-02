
Namespace Data

    ''' <summary>
    ''' Data Provider which uses a data row as its data source.
    ''' </summary>
    Public Class DataRowDataProvider
        Inherits BaseDataProvider

        ''' <summary>
        ''' The row which this provider is using.
        ''' </summary>
        Private _row As DataRow

        ''' <summary>
        ''' Creates a new data provider using the given row.
        ''' </summary>
        ''' <param name="row">The row that this provider should
        ''' draw its data from</param>
        Public Sub New(ByVal row As DataRow)
            _row = row
        End Sub

        ''' <summary>
        ''' The value from the backing data row with the given name
        ''' </summary>
        ''' <param name="name">The name of the item required.</param>
        Default Public Overrides ReadOnly Property Item(ByVal name As String) As Object
            Get
                Return Normalise(_row(name))
            End Get
        End Property

    End Class

End Namespace
