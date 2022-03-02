Namespace Collections

    ''' <summary>
    ''' Class to wrap a base non-generic enumerator into a strongly typed enumerator
    ''' </summary>
    ''' <typeparam name="T">The type of enumerator required.</typeparam>
    Public Class clsTypedEnumerator(Of T) : Implements IEnumerator(Of T)

        ' The backing enumerator for this object.
        Private mEnum As IEnumerator

        ''' <summary>
        ''' Creates a new strongly typed enumerator based on the given enumerator
        ''' </summary>
        ''' <param name="enumerator">The backing enumerator.</param>
        Public Sub New(ByVal enumerator As IEnumerator)
            If enumerator Is Nothing Then Throw New ArgumentNullException(NameOf(enumerator))
            mEnum = enumerator
        End Sub

        ''' <summary>
        ''' Gets the current element from the enumerator
        ''' </summary>
        Public ReadOnly Property Current() As T Implements IEnumerator(Of T).Current
            Get
                Return DirectCast(mEnum.Current, T)
            End Get
        End Property

        ''' <summary>
        ''' Gets the current element from the enumerator.
        ''' </summary>
        Private ReadOnly Property Current1() As Object Implements System.Collections.IEnumerator.Current
            Get
                Return mEnum.Current
            End Get
        End Property

        ''' <summary>
        ''' Attempts to move to the next element in the enumerator
        ''' </summary>
        ''' <returns>True if the enumerator was successfully moved onto the next
        ''' element; False if there are no more elements to enumerate</returns>
        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
            Return mEnum.MoveNext()
        End Function

        ''' <summary>
        ''' Resets this enumerator.
        ''' </summary>
        Public Sub Reset() Implements IEnumerator.Reset
            mEnum.Reset()
        End Sub

        ''' <summary>
        ''' Disposes of this enumerator.
        ''' </summary>

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

        Private mDisposed As Boolean

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not mDisposed Then
                If disposing Then
                    If TypeOf mEnum Is IDisposable Then DirectCast(mEnum, IDisposable).Dispose()
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
