Namespace Compilation
    ''' <summary>
    ''' Stores objects cached by <see cref="ObjectCache"/>
    ''' </summary>
    ''' <remarks>Different implementations allow <see cref="ObjectCache"/> to use 
    ''' different types of store, while managing the casting logic</remarks>
    Public Interface ICacheStore

        ''' <summary>
        ''' Stores an object
        ''' </summary>
        ''' <param name="key">The key to use in the cache</param>
        ''' <param name="value">The value to store</param>
        Sub Add(key As String, value As Object)

        ''' <summary>
        ''' Get an object 
        ''' </summary>
        ''' <param name="key">The key used to store the cached value</param>
        ''' <returns>The cached value or Nothing if no value is found</returns>
        Function [Get](key As String) As Object

    End Interface
End Namespace