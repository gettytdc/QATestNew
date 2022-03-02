Namespace Collections

    ''' <summary>
    ''' Enumerator which wraps an enumerator and filters elements in it to only
    ''' return those elements which pass a predicate.
    ''' </summary>
    ''' <typeparam name="T">The type of element in the enumerator</typeparam>
    Public Class FilteringEnumerator(Of T) : Implements IEnumerator(Of T)

        ' The wrapped enumerator which provides the elements
        Private mEnum As IEnumerator(Of T)

        ' The predicate used to filter the elements in the enumerator
        Private mPredicate As Predicate(Of T)

        ''' <summary>
        ''' Creates a new filtering enumerator
        ''' </summary>
        ''' <param name="enu">The enumerator which is providing the elements to
        ''' filter.</param>
        ''' <param name="pred">The predicate which determines whether the element in
        ''' the enumerator will be returned or not. If the predicate is passed an
        ''' element and it returns true, the element will be returned in the
        ''' enumeration; otherwise it will be skipped over.</param>
        Public Sub New(enu As IEnumerator(Of T), pred As Predicate(Of T))
            mEnum = enu
            mPredicate = pred
        End Sub

        ''' <summary>
        ''' Gets the current element in the enumerator.
        ''' </summary>
        Public ReadOnly Property Current As T Implements IEnumerator(Of T).Current
            Get
                Return mEnum.Current
            End Get
        End Property

        ''' <summary>
        ''' Gets the current element in the enumerator
        ''' </summary>
        Private ReadOnly Property UntypedCurrent As Object _
         Implements IEnumerator.Current
            Get
                Return Current
            End Get
        End Property

        ''' <summary>
        ''' Moves to the next element in the enumerator which passes the predicate
        ''' set in this enumerator, if any more matching elements exist.
        ''' </summary>
        ''' <returns>True if the enumerator found any matching elements and has moved
        ''' <see cref="Current"/> to the next element in the sequence.</returns>
        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
            Do
                If Not mEnum.MoveNext() Then Return False
                If mPredicate(Current) Then Return True
            Loop
        End Function

        ''' <summary>
        ''' Resets the enumerator such that it is positioned before the sequence of
        ''' elements.
        ''' </summary>
        Public Sub Reset() Implements IEnumerator.Reset
            mEnum.Reset()
        End Sub

        ''' <summary>
        ''' Disposes of this enumerator
        ''' </summary>

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

        Private mDisposed As Boolean

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not mDisposed Then
                If disposing Then
                    mEnum.Dispose()
                End If
            End If
            mDisposed = True
        End Sub

        Protected Overrides Sub Finalize()
            Dispose(False)
            MyBase.Finalize()
        End Sub


    End Class

End Namespace