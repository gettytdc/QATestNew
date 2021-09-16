Imports System.Data.SqlTypes

Namespace Data

    ''' <summary>
    ''' Provides the base methods which implement the dataprovider interface, leaving
    ''' only the default property to be implemented by subclasses.
    ''' </summary>
    Public MustInherit Class BaseDataProvider
        Implements IDataProvider

        ''' <summary>
        ''' Gets the data item with the given name or 'Nothing' if that item
        ''' did not exist within this provider.
        ''' </summary>
        ''' <param name="name">The name of the data item required.</param>
        Default Public MustOverride ReadOnly Property Item(ByVal name As String) As Object _
         Implements IDataProvider.Item

        ''' <summary>
        ''' Gets the string value of the given property, returning null if it is
        ''' not present within this provider.
        ''' </summary>
        ''' <param name="name">The name of the property required.</param>
        ''' <returns>The string value of the given property, or null if it is not
        ''' present.</returns>
        Public Overridable Function GetString(ByVal name As String) As String _
         Implements IDataProvider.GetString
            Return Me(name)?.ToString()
        End Function

        ''' <summary>
        ''' Gets the string value of the given property, returning
        ''' <see cref="Guid.Empty"/> if it is not present within this provider.
        ''' </summary>
        ''' <param name="name">The name of the property required.</param>
        ''' <returns>The GUID value of the given property, or
        ''' <see cref="Guid.Empty"/> if it is not present.</returns>
        ''' <remarks>
        ''' This method exists separate to GetValue to improve performance when
        ''' the type is known
        ''' </remarks>
        Public Overridable Function GetGuid(name As String) As Guid _
         Implements IDataProvider.GetGuid
            Dim value = Me(name)
            Dim result As Guid
            If TypeOf value Is String
                result = Guid.Parse(DirectCast(value, String))
            Else
                result = CType(If(value, Guid.Empty), Guid)
            End If

            Return result
        End Function

        ''' <summary>
        ''' Gets the int value of the given property, returning 0 if it is not
        ''' present within this provider.
        ''' </summary>
        ''' <param name="name">The name of the property required</param>
        ''' <returns>The integer value of the given property, or 0 if it is not
        ''' present.</returns>
        ''' <remarks>
        ''' This method exists separate to GetValue to improve performance when
        ''' the type is known
        ''' </remarks>
        Public Overridable Function GetInt(name As String) As Integer _
         Implements IDataProvider.GetInt
            Return GetInt(name, 0)
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
        Public Function GetInt(name As String, defaultValue As Integer) As Integer _
            Implements IDataProvider.GetInt

            Return GetValue(name, defaultValue)
        End Function

        ''' <summary>
        ''' Gets the given property as a boolean value.
        ''' </summary>
        ''' <param name="name">The name.</param>
        ''' <returns>
        ''' The boolean value of the property or <c>false</c> if the value is null
        ''' </returns>
        Public Overridable Function GetBool(name As String) As Boolean _
            Implements IDataProvider.GetBool

            Return GetValue(name, False)
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
        ''' <remarks>If the underlying object is an SqlDateTime, this will convert it
        ''' silently into a DateTime value - in doing so, it will convert
        ''' SqlDateTime.MinValue and SqlDateTime.MaxValue to DateTime.MinValue and
        ''' DateTime.MaxValue respectively. This is the only special case handled by
        ''' this base data provider.
        ''' However, the provider will specify the same kind on the date retrieved
        ''' by this provider as that specified in the default value. As such, a
        ''' default value of DateTime.MinValue will ensure a returned value with a
        ''' 'Kind' of 'Unspecified', as DateTime.MinValue is. A default value of
        ''' DateTime.Now will return a value with a 'Local' Kind, as DateTime.Now has
        ''' </remarks>
        Public Overridable Function GetValue(Of T)( _
         ByVal name As String, ByVal defaultValue As T) As T Implements IDataProvider.GetValue

            Dim obj As Object = Me(name)

            ' Special case for SqlDateTimes to deal with the disparity between 
            ' Sql Servers min and max dates/times and the system Date equivalents.
            If TypeOf obj Is SqlDateTime Then
                Dim sqlDate As SqlDateTime = DirectCast(obj, SqlDateTime)
                If sqlDate.IsNull Then
                    obj = Nothing
                ElseIf sqlDate = SqlDateTime.MinValue Then
                    obj = DateTime.MinValue
                ElseIf sqlDate = SqlDateTime.MaxValue Then
                    obj = DateTime.MaxValue
                Else
                    obj = sqlDate.Value
                End If
            End If

            If obj Is Nothing Then Return defaultValue

            ' Two special cases dealt with here.
            If TypeOf defaultValue Is Date Then
                ' First, ensure that the 'kind' of date matches that on the default value.

                ' The joys of generics...
                Dim defDate As Date = DirectCast(DirectCast(defaultValue, Object), Date)
                If TypeOf obj Is Date Then
                    obj = DateTime.SpecifyKind(DirectCast(obj, Date), defDate.Kind)
                End If

            ElseIf TypeOf defaultValue Is Guid AndAlso TypeOf obj Is String Then
                ' Another special case... neither IConvertible nor the sql driver handle
                ' GUID<=>String conversions well, if at all. If the object is a string and
                ' a GUID is required, just try to parse it.
                obj = New Guid(DirectCast(obj, String))

            End If

            Return CType(obj, T)

        End Function

        ''' <summary>
        ''' Normalises the given object, ensuring that any <see cref="DBNull"/>
        ''' values are converted to null .net references.
        ''' </summary>
        ''' <param name="value">The value to be normalised</param>
        ''' <returns>The given value if it was anything other than a
        ''' <see cref="DBNull"/> value; null if it is a <see cref="DBNull"/> value.
        ''' </returns>
        Protected Function Normalise(ByVal value As Object) As Object
            If Convert.IsDBNull(value) Then Return Nothing
            Return value
        End Function

    End Class

End Namespace
