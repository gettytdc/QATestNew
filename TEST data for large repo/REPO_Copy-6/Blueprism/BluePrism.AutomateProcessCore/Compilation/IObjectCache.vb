Namespace Compilation
    
    ''' <summary>
    ''' Caches objects within a specific scope
    ''' </summary>
    ''' <remarks>Introduced for use during code compilation, but could be moved to 
    ''' shared location if useful elsewhere</remarks>
    Public Interface IObjectCache

        ''' <summary>
        ''' Adds an object to the cache
        ''' </summary>
        ''' <param name="key">The key to use in the cache</param>
        ''' <param name="value">The value to store</param>
        Sub Add(key As String, value As Object)

        ''' <summary>
        ''' Get a value that has been cached within the session.
        ''' </summary>
        ''' <param name="key">The key used to store the cached value</param>
        ''' <returns>The cached value or Nothing if no value is found</returns>
        ''' <exception cref="InvalidCastException">Exception thrown if an object is
        ''' found with the specified key that is not of the expected type</exception>
        Function [Get](Of T)(key As String) As T

    End Interface
End NameSpace