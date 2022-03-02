Imports BluePrism.Server.Domain.Models
''' <summary>
''' An enumerator over a string with some more flexible functions than a basic
''' forward-only enumerator.
''' </summary>
Public Class StringEnumerator : Implements IEnumerator(Of Char)

    ''' <summary>
    ''' A value for the index that indicates that it is before the start of the
    ''' string being enumerated over, ie. that the enumerating has not yet begun.
    ''' </summary>
    Private Const BeforeStart As Integer = -1

    ''' <summary>
    ''' A value for the index that indicates that it is after the end of the string
    ''' being enumerated over, ie. that the enumerating is complete
    ''' </summary>
    Private Const AfterEnd As Integer = -2

    ' The string being enumerated over. Set to null when this object is disposed
    Private mStr As String

    ' The current index within the string
    Private mIndex As Integer

    ''' <summary>
    ''' Creates a new enumerator over the given string
    ''' </summary>
    ''' <param name="str">The string to be enumerated over.</param>
    ''' <exception cref="ArgumentNullException">If the given string is null.
    ''' </exception>
    Friend Sub New(ByVal str As String)
        If str Is Nothing Then Throw New ArgumentNullException(NameOf(str))
        mStr = str
        mIndex = BeforeStart
    End Sub

    ''' <summary>
    ''' Gets the current index of the enumerator.
    ''' </summary>
    ''' <exception cref="InvalidOperationException">If the enumerator is currently
    ''' before the beginning or after the end of the string.</exception>
    ''' <exception cref="ObjectDisposedException">If this object has been disposed
    ''' </exception>
    Public ReadOnly Property Index() As Integer
        Get
            EnsureState()
            Return mIndex
        End Get
    End Property

    ''' <summary>
    ''' Gets whether this enumerator is currently before the start of the string.
    ''' </summary>
    ''' <exception cref="ObjectDisposedException">If this object has been disposed
    ''' </exception>
    Public ReadOnly Property IsBeforeStart() As Boolean
        Get
            EnsureDisposedState()
            Return (mIndex = BeforeStart)
        End Get
    End Property

    ''' <summary>
    ''' Gets whether this enumerator is currently after the end of the string.
    ''' </summary>
    ''' <exception cref="ObjectDisposedException">If this object has been disposed
    ''' </exception>
    Public ReadOnly Property IsAfterEnd() As Boolean
        Get
            EnsureDisposedState()
            Return (mIndex = AfterEnd)
        End Get
    End Property

    ''' <summary>
    ''' Gets the current char of the string being enumerated over.
    ''' </summary>
    ''' <exception cref="InvalidOperationException">If this enumerator is positioned
    ''' before the start or after the end of the string being enumerated over.
    ''' </exception>
    ''' <exception cref="ObjectDisposedException">If this object has been disposed.
    ''' </exception>
    Public ReadOnly Property Current() As Char _
     Implements IEnumerator(Of Char).Current
        Get
            EnsureState()
            Return mStr(mIndex)
        End Get
    End Property

    ''' <summary>
    ''' Gets the current char of the string being enumerated over.
    ''' </summary>
    ''' <exception cref="InvalidOperationException">If this enumerator is positioned
    ''' before the start or after the end of the string being enumerated over.
    ''' </exception>
    ''' <exception cref="ObjectDisposedException">If this object has been disposed.
    ''' </exception>
    Private ReadOnly Property CurrentObject() As Object _
     Implements IEnumerator.Current
        Get
            Return Current
        End Get
    End Property

    ''' <summary>
    ''' Gets the next char of the string being enumerated over if there is one, or
    ''' <see cref="Char.MinValue"/> (ie. a null char) if there isn't.
    ''' </summary>
    ''' <exception cref="InvalidOperationException">If this enumerator is positioned
    ''' before the start or after the end of the string being enumerated over.
    ''' </exception>
    ''' <exception cref="ObjectDisposedException">If this object has been disposed.
    ''' </exception>
    Public ReadOnly Property Peek() As Char
        Get
            EnsureState()
            If mIndex < mStr.Length - 1 Then Return mStr(mIndex + 1)
            Return Char.MinValue
        End Get
    End Property

    ''' <summary>
    ''' Gets the previous char of the string being enumerated over if there is one,
    ''' or <see cref="Char.MinValue"/> (ie. a null char) if there isn't.
    ''' </summary>
    ''' <exception cref="InvalidOperationException">If this enumerator is positioned
    ''' before the start or after the end of the string being enumerated over.
    ''' </exception>
    ''' <exception cref="ObjectDisposedException">If this object has been disposed.
    ''' </exception>
    Public ReadOnly Property Previous() As Char
        Get
            EnsureState()
            If mIndex > 0 Then Return mStr(mIndex - 1)
            Return Char.MinValue
        End Get
    End Property

    ''' <summary>
    ''' Attempts to move to the previous char in the string being enumerated
    ''' </summary>
    ''' <returns>True if the enumerator found another char to enumerate; False if the
    ''' beginning of the string has been reached.</returns>
    ''' <exception cref="ObjectDisposedException">If this object has been disposed.
    ''' </exception>
    ''' <remarks>If the enumerator is currently after the end of the string, calling
    ''' this method will set it to the last character in the string, as such, you can
    ''' 'reset' a completed enumerator by calling this method until it returns false.
    ''' </remarks>
    Public Function MoveBack() As Boolean
        EnsureDisposedState()
        If mIndex = BeforeStart Then Return False
        If mIndex <= 0 Then mIndex = BeforeStart : Return False
        If mIndex = AfterEnd Then
            If mStr.Length = 0 Then mIndex = BeforeStart : Return False
            mIndex = mStr.Length - 1
        Else
            mIndex -= 1
        End If
        Return True
    End Function

    ''' <summary>
    ''' Attempts to move to the next char in the string being enumerated.
    ''' </summary>
    ''' <returns>True if the enumerator found another char to enumerate; False if the
    ''' end of the string has been reached.</returns>
    ''' <exception cref="ObjectDisposedException">If this object has been disposed.
    ''' </exception>
    Public Function MoveNext() As Boolean Implements IEnumerator(Of Char).MoveNext
        EnsureDisposedState()
        If mIndex = AfterEnd Then Return False
        If mIndex >= mStr.Length - 1 Then mIndex = AfterEnd : Return False
        If mIndex = BeforeStart Then
            If mStr.Length = 0 Then mIndex = AfterEnd : Return False
            mIndex = 0
        Else
            mIndex += 1
        End If
        Return True
    End Function

    ''' <summary>
    ''' Ensures that the current index set in this enumerator represents a valid
    ''' index within the range of the string being enumerated.
    ''' </summary>
    ''' <exception cref="InvalidOperationException">If the index set in this
    ''' enumerator is before the start or after the end of its contained string.
    ''' </exception>
    ''' <exception cref="OutOfRangeException">If the index does not indicate that
    ''' the enumerator is before the start or after the end of the string, but it is
    ''' outside the allowed range of the string. This should not happen in normal
    ''' usage of the class.</exception>
    ''' <exception cref="ObjectDisposedException">If this object has been disposed.
    ''' </exception>
    Private Sub EnsureState()
        EnsureDisposedState()
        If mIndex = BeforeStart Then Throw New InvalidOperationException(
         My.Resources.StringEnumerator_EnumeratorIsBeforeStartOfString)
        If mIndex = AfterEnd Then Throw New InvalidOperationException(
         My.Resources.StringEnumerator_EnumeratorIsAfterEndOfString)
        If mIndex < 0 OrElse mIndex >= mStr.Length Then _
         Throw New OutOfRangeException(My.Resources.StringEnumerator_InvalidIndex & mIndex)
    End Sub

    ''' <summary>
    ''' Ensures that this enumerator has not been disposed.
    ''' </summary>
    ''' <exception cref="ObjectDisposedException">If this object has been disposed
    ''' </exception>
    Private Sub EnsureDisposedState()
        If mStr Is Nothing Then Throw New ObjectDisposedException("LexEnumerator",
         My.Resources.StringEnumerator_ThisEnumeratorHasBeenDisposed)
    End Sub

    ''' <summary>
    ''' Dispoes of this enumerator, making it invalid for further use
    ''' </summary>

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Private mDisposed As Boolean

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not mDisposed Then
            If disposing Then
                mStr = Nothing
            End If
        End If
        mDisposed = True
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub


    ''' <summary>
    ''' Resets this enumerator, setting it back to pointing before the start of the
    ''' string that it is enumerating.
    ''' </summary>
    Public Sub Reset() Implements IEnumerator.Reset
        EnsureDisposedState()
        mIndex = BeforeStart
    End Sub

End Class
