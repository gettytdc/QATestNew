Namespace Compilation

    ''' <summary>
    ''' <see cref="ICacheStore"/> implementation that caches within a session. If the
    ''' current session is not available then values will not be stored or retrieved.
    ''' </summary>
    Public Class SessionCacheStore
        Implements ICacheStore

        Private ReadOnly mGetCurrentSession As Func(Of clsSession)

        ''' <summary>
        ''' Creates a new <see cref="SessionCacheStore"/>
        ''' </summary>
        ''' <param name="getCurrentSession">A function used to get the current 
        ''' session</param>
        Public Sub New(getCurrentSession As Func(Of clsSession))
            If getCurrentSession Is Nothing Then
                Throw New ArgumentNullException(NameOf(getCurrentSession))
            End If
            mGetCurrentSession = getCurrentSession
        End Sub

        ''' <summary>
        ''' Gets the current session
        ''' </summary>
        ''' <returns>The session or nothing if not available</returns>
        Private ReadOnly Property CurrentSession As clsSession
            Get
                Return mGetCurrentSession()
            End Get
        End Property

        ''' <inheritdoc />
        Public Sub Add(key As String, value As Object) Implements ICacheStore.Add
            CurrentSession?.CacheObject(key, value)
        End Sub

        ''' <inheritdoc />
        Public Function [Get](key As String) As Object Implements ICacheStore.[Get]
            Return CurrentSession?.GetCachedObject(key)
        End Function

    End Class
End NameSpace