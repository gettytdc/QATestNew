
Namespace Data

    ''' <summary>
    ''' Simple interface which provides a basic mechanism for the provision
    ''' of data through named objects.
    ''' </summary>
    Public Interface IDataProvider

        ''' <summary>
        ''' Gets the data item with the given name or 'Nothing' if that item
        ''' did not exist within this provider.
        ''' </summary>
        ''' <param name="name">The name of the data item required.</param>
        Default ReadOnly Property Item(ByVal name As String) As Object

        ''' <summary>
        ''' Gets the value of the given property as the specified type, returning the
        ''' default value if it is not present
        ''' </summary>
        ''' <typeparam name="T">The type that is required of the given property
        ''' </typeparam>
        ''' <param name="name">The name associated with the required value.</param>
        ''' <param name="defaultValue">The default value to use if the named item
        ''' does not exist within this provider.</param>
        ''' <returns>The value found with the given name or the default value 
        ''' provided the name represented a null or missing value</returns>
        Function GetValue(Of T)(ByVal name As String, ByVal defaultValue As T) As T

        ''' <summary>
        ''' Gets the string value of the given property, returning null if it is
        ''' not present within this provider.
        ''' </summary>
        ''' <param name="name">The name of the property required.</param>
        ''' <returns>The string value of the given property, or null if it is not
        ''' present.</returns>
        Function GetString(ByVal name As String) As String

        ''' <summary>
        ''' Gets the string value of the given property, returning
        ''' <see cref="Guid.Empty"/> if it is not present within this provider.
        ''' </summary>
        ''' <param name="name">The name of the property required.</param>
        ''' <returns>The GUID value of the given property, or
        ''' <see cref="Guid.Empty"/> if it is not present.</returns>
        Function GetGuid(name As String) As Guid

        ''' <summary>
        ''' Gets the int value of the given property, returning 0 if it is not
        ''' present within this provider.
        ''' </summary>
        ''' <param name="name">The name of the property required</param>
        ''' <returns>The integer value of the given property, or 0 if it is not
        ''' present.</returns>
        Function GetInt(name As String) As Integer

        ''' <summary>
        ''' Gets the int value of the given property, returning the default value
        ''' if it is not present within this provider.
        ''' </summary>
        ''' <param name="name">The name of the property required</param>
        ''' <param name="defaultValue">The default value.</param>
        ''' <returns>The integer value of the given property, or <paramref name="defaultValue"/>
        ''' if it is not present.</returns>
        ''' <exception cref="NotImplementedException"></exception>
        Function GetInt(name As String, defaultValue As Integer) As Integer

        ''' <summary>
        ''' Gets the given property as a boolean value.
        ''' </summary>
        ''' <param name="name">The name.</param>
        ''' <returns>The boolean value of the property or <c>false</c> if the value is null</returns>
        Function GetBool(name As String) As Boolean

    End Interface

End Namespace
