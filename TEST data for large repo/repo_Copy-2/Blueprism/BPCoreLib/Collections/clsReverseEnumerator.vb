Namespace Collections

    ''' <summary>
    ''' Gets a reverse enumerator from the given enumerator.
    ''' Note that this buffers the contents of the given enumerator, so may not be
    ''' the wisest choice for large collections.
    ''' </summary>
    ''' <typeparam name="T">The type of the enumerator</typeparam>
    Public Class clsReverseEnumerator(Of T) : Implements IEnumerator(Of T)

        ' The enumerator over the buffered reversed collection
        Private mEnum As IEnumerator(Of T)

        ''' <summary>
        ''' Creates a new enumerator based on the reverse order of the given enumerator
        ''' </summary>
        ''' <param name="enu">The enumerator on which the reverse is required.</param>
        Public Sub New(ByVal enu As IEnumerator(Of T))
            Dim buf As New List(Of T)
            While enu.MoveNext
                buf.Add(enu.Current)
            End While
            enu.Dispose()
            buf.Reverse()
            mEnum = buf.GetEnumerator()
        End Sub

        ''' <summary>
        ''' The current element in the enumerator
        ''' </summary>
        Public ReadOnly Property Current() As T Implements IEnumerator(Of T).Current
            Get
                Return mEnum.Current
            End Get
        End Property

        ''' <summary>
        ''' The current element in the enumerator
        ''' </summary>
        Private ReadOnly Property Current1() As Object Implements IEnumerator.Current
            Get
                Return Current
            End Get
        End Property

        ''' <summary>
        ''' Moves to the next element in the enumerator
        ''' </summary>
        ''' <returns>True if any more elements remain in the enumerator; False otherwise.
        ''' </returns>
        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
            Return mEnum.MoveNext()
        End Function

        ''' <summary>
        ''' Resets the enumerator to point to before the first element.
        ''' </summary>
        Public Sub Reset() Implements IEnumerator.Reset
            mEnum.Reset()
        End Sub

        ''' <summary>
        ''' Disposes of the enumerator
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
