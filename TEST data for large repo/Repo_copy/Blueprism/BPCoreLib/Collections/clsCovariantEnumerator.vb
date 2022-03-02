Namespace Collections

    ''' Project  : BPCoreLib
    ''' Class    : clsCovariantEnumerator
    ''' <summary>
    ''' Enumerator class which enables covariance of generic types such that
    ''' an Enumerator of type T can be 'cast' as an Enumerator of type S.
    ''' Obviously, it's not a true cast, the enumerator is wrapped into this
    ''' class and the calls to this object are adapted to the contained
    ''' enumerator.
    ''' </summary>
    ''' <typeparam name="S">The type of enumerator required - this should be
    ''' a base type of the type of enumerator currently held.</typeparam>
    ''' <typeparam name="T">The type of enumerator which is currently held.
    ''' This will be wrapped and calls converted into the required type
    ''' by an instance of this class.</typeparam>
    Public Class clsCovariantEnumerator(Of S, T As {S})
        Implements IEnumerator(Of S)

        ''' <summary>
        ''' The wrapped enumerator
        ''' </summary>
        Private _enumerator As IEnumerator(Of T)

        ''' <summary>
        ''' Creates a new convariant enumerator wrapping the given enumerator
        ''' of type T and exposing its methods as an enumerator of type S.
        ''' </summary>
        ''' <param name="e">The enumerator that this enumerator should wrap
        ''' </param>
        ''' <exception cref="ArgumentNullException">If the given enumerator
        ''' was null.</exception>
        Public Sub New(ByVal e As IEnumerator(Of T))
            If e Is Nothing Then Throw New ArgumentNullException(NameOf(e))
            _enumerator = e
        End Sub


        ''' <summary>
        ''' The current element within the enumerator.
        ''' </summary>
        Public ReadOnly Property Current() As S Implements IEnumerator(Of S).Current
            Get
                Return _enumerator.Current
            End Get
        End Property

        ''' <summary>
        ''' The current element within the enumerator as an Object.
        ''' </summary>
        Private ReadOnly Property NonGenericCurrent() As Object Implements IEnumerator.Current
            Get
                Return Me.Current
            End Get
        End Property

        ''' <summary>
        ''' Attempts to move to the next element within the enumerator.
        ''' </summary>
        ''' <returns>True if the enumerator had more elements to display. If
        ''' returned True, then 'Current' will point to the next element within
        ''' the enumerator. False if there are no more elements to display
        ''' in this enumerator (and, thus, the wrapped enumerator)</returns>
        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
            Return _enumerator.MoveNext
        End Function

        ''' <summary>
        ''' Resets this enumerator, and thus the wrapped enumerator, such that
        ''' it is now pointing before the first element again.
        ''' </summary>
        Public Sub Reset() Implements System.Collections.IEnumerator.Reset
            _enumerator.Reset()
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
                    _enumerator.Dispose()
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
