Namespace Collections

    ''' <summary>
    ''' Class to reverse a given enumerable on the fly.
    ''' Note that this will buffer the entire contents of the underlying collection
    ''' when an enumerator is requested, so it may not be the best way to reverse
    ''' a large collection.
    ''' </summary>
    ''' <typeparam name="T">The type of enumerable</typeparam>
    <Serializable>
    Public Class clsReverseEnumerable(Of T) : Implements IEnumerable(Of T)

        ' The underlying enumerable
        Private mEnum As IEnumerable(Of T)

        ''' <summary>
        ''' Creates a new reverse enumerable wrapping the given collection.
        ''' </summary>
        ''' <param name="enu">The enumerable which is to be reversed</param>
        Public Sub New(ByVal enu As IEnumerable(Of T))
            mEnum = enu
        End Sub

        ''' <summary>
        ''' Gets an enumerator which operates on reverse order on the wrapped collection
        ''' </summary>
        ''' <returns>A reverse enumerator over the wrapped collection.</returns>
        Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            Return New clsReverseEnumerator(Of T)(mEnum.GetEnumerator())
        End Function

        ''' <summary>
        ''' Gets an enumerator which operates on reverse order on the wrapped collection
        ''' </summary>
        ''' <returns>A reverse enumerator over the wrapped collection.</returns>
        Private Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function

    End Class

End Namespace
