Namespace Compilation

    ''' <summary>
    ''' Caches objects within a specific scope
    ''' </summary>
    ''' <remarks>Introduced for use during code compilation, but could be moved to 
    ''' shared location if useful elsewhere</remarks>
    Public Class ObjectCache
        Implements IObjectCache

        Private ReadOnly mStore As ICacheStore 
        
        ''' <summary>
        ''' Creates a new <see cref="ObjectCache"/>
        ''' </summary>
        ''' <param name="store">The <see cref="ICacheStore"/> instance used to store
        ''' values</param>
        Sub New(store As ICacheStore)
            If store Is Nothing Then
                Throw New ArgumentNullException(NameOf(store))
            End If
            mStore = store
        End Sub

        '''<inheritdoc />
        Public Sub Add(key As String, value As Object) Implements IObjectCache.Add
            mStore.Add(key, value)
        End Sub

        '''<inheritdoc />
        Public Function [Get](Of T)(key As String) As T Implements IObjectCache.[Get]
            Dim value = mStore.Get(key)
            If value IsNot Nothing Then
                If Not (TypeOf value Is T) Then
                    Throw New InvalidCastException(String.Format(My.Resources.Resources.ObjectCache_TheValueInTheCacheIsNotOfTheExpectedType0, GetType(T)))
                End If
                Return DirectCast(value, T)
            Else
                Return Nothing
            End If
        End Function
        
    End Class
End NameSpace