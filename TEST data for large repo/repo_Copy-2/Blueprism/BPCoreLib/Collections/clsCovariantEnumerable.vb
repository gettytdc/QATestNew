Namespace Collections

    ''' Project  : BPCoreLib
    ''' Class    : clsCovariantEnumerable
    ''' <summary>
    ''' A class to enable covariance in enumerables. This really will be
    ''' unnecessary once we start using .net 4.0 since covariant enumerables are
    ''' fully supported by the CLR and the primary languages (VB &amp; C#)
    ''' </summary>
    ''' <typeparam name="S">The type of enumerable required.</typeparam>
    ''' <typeparam name="T">The type of enumerable currently held</typeparam>
    <Serializable> _
    Public Class clsCovariantEnumerable(Of S, T As {S})
        Implements IEnumerable(Of S)

        ''' <summary>
        ''' The wrapped enumerable.
        ''' </summary>
        Private mEnumerable As IEnumerable(Of T)

        ''' <summary>
        ''' Creates a new covariant enumerable wrapping the given enumerable.
        ''' </summary>
        ''' <param name="en">The enumerable to wrap.</param>
        Public Sub New(ByVal en As IEnumerable(Of T))
            mEnumerable = en
        End Sub

        ''' <summary>
        ''' Gets the enumerator for the wrapped enumerable.
        ''' </summary>
        ''' <returns>An enumerator for the enumerable.</returns>
        Public Function GetEnumerator() As IEnumerator(Of S) Implements IEnumerable(Of S).GetEnumerator
            Return New clsCovariantEnumerator(Of S, T)(mEnumerable.GetEnumerator())
        End Function

        ''' <summary>
        ''' Gets the enumerator for the wrapped enumerable.
        ''' </summary>
        ''' <returns>An enumerator over the wrapped enumerable.</returns>
        Public Function GetNonGenericEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return mEnumerable.GetEnumerator()
        End Function
    End Class

End Namespace
