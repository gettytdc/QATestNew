Namespace Data

    ''' <summary>
    ''' Data provider over a single name/value pair. Asking it for any other name
    ''' than that it is initialised with will cause null to be returned.
    ''' </summary>
    Public Class SingletonDataProvider : Inherits BaseDataProvider

        ' The name to look up in this provider
        Private mName As String

        ' The value to return for the specified name
        Private mValue As Object

        ''' <summary>
        ''' Creates a new data provider.
        ''' </summary>
        ''' <param name="name">The single name to provide a value for</param>
        ''' <param name="value">The single value to provide</param>
        Public Sub New(name As String, value As Object)
            If name Is Nothing Then Throw New ArgumentNullException(NameOf(name))
            mName = name
            mValue = value
        End Sub

        ''' <summary>
        ''' Gets the item corresponding to the given name or null if no such name was
        ''' found in this provider.
        ''' </summary>
        ''' <param name="name">The name of the data to be provided.</param>
        Default Public Overrides ReadOnly Property Item(name As String) As Object
            Get
                If name = mName Then Return mValue
                Return Nothing
            End Get
        End Property
    End Class

End Namespace
