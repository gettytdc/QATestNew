Namespace Collections

    ''' <summary>
    ''' Typed enumerable wrapping a non-generic enumerable.
    ''' </summary>
    ''' <typeparam name="T">The underlying type of the elements in the enumerable.
    ''' </typeparam>
    <Serializable>
    Public Class clsTypedEnumerable(Of T) : Implements IEnumerable(Of T)

        ' The wrapped enumerable
        Private mEnum As IEnumerable

        ''' <summary>
        ''' Creates a new typed enumerable around the given enumerable
        ''' </summary>
        ''' <param name="enu">The enumerable which should be wrapped by this object.
        ''' </param>
        Public Sub New(ByVal enu As IEnumerable)
            If enu Is Nothing Then Throw New ArgumentNullException(NameOf(enu))
            mEnum = enu
        End Sub

        ''' <summary>
        ''' The enumerable wrapped by this object.
        ''' </summary>
        Protected ReadOnly Property WrappedEnumerable() As IEnumerable
            Get
                Return mEnum
            End Get
        End Property

        ''' <summary>
        ''' Gets the generic enumerator exposed by this object.
        ''' </summary>
        ''' <returns>An enumerator over specific types of elements found in the
        ''' wrapped enumerable.</returns>
        Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            Return New clsTypedEnumerator(Of T)(mEnum.GetEnumerator())
        End Function

        ''' <summary>
        ''' Gets a non generic enumerator over the wrapped enumerable.
        ''' </summary>
        ''' <returns>The enumerator offered up by the wrapped enumerable.</returns>
        Private Function GetNonGenericEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return mEnum.GetEnumerator()
        End Function
    End Class

End Namespace
