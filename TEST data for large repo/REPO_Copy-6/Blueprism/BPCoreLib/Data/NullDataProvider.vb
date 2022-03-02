Namespace Data

    ''' <summary>
    ''' Data Provider implementation which provides no data. Really just here to act
    ''' as a placeholder for a data provider to avoid unnecessary null checks.
    ''' </summary>
    Public Class NullDataProvider : Implements IDataProvider

        ''' <summary>
        ''' A single instance of a null data provider to use in place of a data
        ''' provider.
        ''' </summary>
        Public Shared ReadOnly Instance As New NullDataProvider()

        ''' <summary>
        ''' Private constructor to ensure nowhere else creates another one of these
        ''' (unnecessary - just use the <see cref="Instance"/>)
        ''' </summary>
        Private Sub New()
        End Sub

        ''' <summary>
        ''' Gets the data item with the given name or 'Nothing' if that item
        ''' did not exist within this provider.
        ''' </summary>
        ''' <param name="name">The name of the data item required.</param>
        Default Public ReadOnly Property Item(name As String) As Object _
         Implements IDataProvider.Item
            Get
                Return Nothing
            End Get
        End Property

        ''' <summary>
        ''' Gets the string value of the given property, returning
        ''' <see cref="Guid.Empty"/> if it is not present within this provider.
        ''' </summary>
        ''' <param name="name">The name of the property required.</param>
        ''' <returns>The GUID value of the given property, or
        ''' <see cref="Guid.Empty"/> if it is not present.</returns>
        ''' <remarks>This is the exact equivalent of a call to :
        ''' <code>GetValue(Of Guid)(name, Guid.Empty)</code>, but just provides a
        ''' shorthand (and less cluttered) way of doing so.
        ''' </remarks>
        Public Function GetGuid(name As String) As Guid _
         Implements IDataProvider.GetGuid
            Return Guid.Empty
        End Function

        ''' <summary>
        ''' Gets the string value of the given property, returning null if it is
        ''' not present within this provider.
        ''' </summary>
        ''' <param name="name">The name of the property required.</param>
        ''' <returns>The string value of the given property, or null if it is not
        ''' present.</returns>
        ''' <remarks>This is the exact equivalent of a call to :
        ''' <code>GetValue(Of String)(name, Nothing)</code>, but just provides a
        ''' shorthand (and less cluttered) way of doing so.
        ''' </remarks>
        Public Function GetString(name As String) As String _
         Implements IDataProvider.GetString
            Return Nothing
        End Function

        ''' <summary>
        ''' Gets the int value of the given property, returning 0 if it is not
        ''' present within this provider.
        ''' </summary>
        ''' <param name="name">The name of the property required</param>
        ''' <returns>The integer value of the given property, or 0 if it is not
        ''' present.</returns>
        ''' <remarks>This is the exact equivalent of a call to:
        ''' <code>GetValue(Of Integer)(name, 0)</code>, but just provides a shorthand
        ''' (and less cluttered) way of doing so.</remarks>
        Public Overridable Function GetInt(name As String) As Integer _
         Implements IDataProvider.GetInt
            Return 0
        End Function

        ''' <summary>
        ''' Gets the int value of the given property, returning the default value
        ''' if it is not present within this provider.
        ''' </summary>
        ''' <param name="name">The name of the property required</param>
        ''' <param name="defaultValue">The default value.</param>
        ''' <returns>The integer value of the given property, or <paramref name="defaultValue"/>
        ''' if it is not present.</returns>
        ''' <exception cref="NotImplementedException"></exception>
        Public Overridable Function GetInt(name As String, defaultValue As Integer) As Integer _
            Implements IDataProvider.GetInt
            Return 0
        End Function

        ''' <summary>
        ''' Gets the given property as a boolean value.
        ''' </summary>
        ''' <param name="name">The name.</param>
        ''' <returns>
        ''' The boolean value of the property or <c>false</c> if the value is null
        ''' </returns>
        ''' <exception cref="NotImplementedException"></exception>
        Public Function GetBool(name As String) As Boolean Implements IDataProvider.GetBool
            Return False
        End Function

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
        Public Function GetValue(Of T)(name As String, defaultValue As T) As T _
         Implements IDataProvider.GetValue
            Return defaultValue
        End Function

    End Class

End Namespace
