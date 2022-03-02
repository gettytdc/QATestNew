Namespace Data

    ''' <summary>
    ''' DataProvider which wraps a dictionary.
    ''' </summary>
    Public Class DictionaryDataProvider
        Inherits BaseDataProvider

        ''' <summary>
        ''' The dictionary 
        ''' </summary>
        Private mDict As IDictionary

        ''' <summary>
        ''' Create a new data provider wrapping a dictionary
        ''' </summary>
        ''' <param name="dict">The dictionary to use for the data in this provider.
        ''' </param>
        Public Sub New(ByVal dict As IDictionary)
            mDict = dict
        End Sub

        ''' <summary>
        ''' Gets the object in the underlying dictionary with the given name.
        ''' </summary>
        ''' <param name="name">The name of the object required.</param>
        Default Public Overrides ReadOnly Property Item(ByVal name As String) As Object
            Get
                Return mDict.Item(name)
            End Get
        End Property

    End Class

End Namespace
