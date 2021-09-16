
Namespace Collections

    ''' Project  : BPCoreLib
    ''' Class    : clsWindowedEnumerator
    ''' <summary>
    ''' An enumerator which operates on a specified range of a wrapped enumerator
    ''' </summary>
    ''' <typeparam name="T">The type of element held by the enumerator - no 
    ''' restriction is applied to this type</typeparam>
    Public Class clsWindowedEnumerator(Of T)
        Implements IEnumerator(Of T)

        ' Constant used to indicate that the enumerator is before the
        ' start of its enumerated data.
        Private Const BEFORE_START As Integer = -1

        ' Constant used to indicate that the enumerator is after the
        ' end of its enumerated data.
        Private Const AFTER_END As Integer = -2

        ' The wrapped enumerator
        Private mIter As IEnumerator(Of T)

        ' The start index that this enumerator should work on
        Private mStart As Integer

        ' The maximum number of elements to enumerate over
        Private mMaxNumber As Integer

        ' The current index in this enumerator
        ' BEFORE_START indicates before the first element
        ' AFTER_END indicates after the last element
        Private mIndex As Integer

        ' The current object enumerated up to
        Private mCurrent As T

        ''' <summary>
        ''' Creates a new WindowedEnumerator over the given enumerator, starting
        ''' at the specified index and enumerating over the maximum number of
        ''' elements as dictated.
        ''' </summary>
        ''' <param name="iter">The enumerator to iterate over</param>
        ''' <param name="start">The index of the first element that should be
        ''' returned by this enumerator</param>
        ''' <param name="number">The maximum number of elements to iterate over
        ''' in this enumerator</param>
        ''' <exception cref="ArgumentNullException">If the given enumerator is null.
        ''' </exception>
        ''' <exception cref="ArgumentOutOfRangeException">If either of the start or
        ''' number parameters are negative</exception>
        Private Sub New( _
         ByVal iter As IEnumerator(Of T), ByVal start As Integer, ByVal number As Integer)

            If iter Is Nothing Then Throw New ArgumentNullException(NameOf(iter))
            If start < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(start),
                 My.Resources.clsWindowedEnumerator_NegativeValuesNotAllowedInTheWindowedEnumeratorValueGiven & start)
            End If
            If number < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(number),
                 My.Resources.clsWindowedEnumerator_NegativeValuesNotAllowedInTheWindowedEnumeratorValueGiven & number)
            End If

            mIter = iter
            mStart = start
            mMaxNumber = number

            mIndex = BEFORE_START

        End Sub

        ''' <summary>
        ''' Creates a new WindowedEnumerator over an enumerator provided by the 
        ''' given enumerable, starting at the specified index and enumerating over 
        ''' the maximum number of elements as dictated.
        ''' </summary>
        ''' <param name="enumerable">The enumerable to provide a windowed enumerator
        ''' over</param>
        ''' <param name="start">The index of the first element that should be
        ''' iterated over by this enumerator</param>
        ''' <param name="number">The maximum number if elements in the contained
        ''' enumerator to iterate over.</param>
        ''' <exception cref="ArgumentNullException">If the given enumerable is null,
        ''' or its GetEnumerator() method returns null.</exception>
        ''' <exception cref="ArgumentOutOfRangeException">If either of the start or
        ''' number parameters are negative</exception>
        Public Sub New(
         ByVal enumerable As IEnumerable(Of T), ByVal start As Integer, ByVal number As Integer)

            Me.New(GetEnumerator(enumerable), start, number)

        End Sub

        ''' <summary>
        ''' Function to get an enumerator for the given enumerable.. this just 
        ''' ensures that the ArgumentNullException is thrown for the case when the
        ''' given enumerable is nothing (as opposed to the stock
        ''' NullReferenceException which would be thrown otherwise)
        ''' </summary>
        ''' <param name="enumerable">The enumerable for which the enumerator is
        ''' required.</param>
        ''' <returns>The enumerator from the given enumerable.</returns>
        ''' <exception cref="ArgumentNullException">If the given enumerable is null.
        ''' </exception>
        Private Shared Function GetEnumerator(
         ByVal enumerable As IEnumerable(Of T)) As IEnumerator(Of T)

            If enumerable Is Nothing Then Throw New ArgumentNullException(NameOf(enumerable))
            Return enumerable.GetEnumerator()

        End Function

        ''' <summary>
        ''' Gets the current element in this enumerator
        ''' </summary>
        ''' <exception cref="InvalidOperationException">If this enumerator is before
        ''' the first element or after the last element.</exception>
        Public ReadOnly Property Current() As T Implements IEnumerator(Of T).Current
            Get
                If mIndex = BEFORE_START Then
                    Throw New InvalidOperationException(My.Resources.clsWindowedEnumerator_EnumeratorIsBeforeTheFirstElement)
                End If
                If mIndex = AFTER_END Then
                    Throw New InvalidOperationException(My.Resources.clsWindowedEnumerator_EnumeratorIsAfterTheLastElement)
                End If
                Return mCurrent
            End Get
        End Property

        ''' <summary>
        ''' Gets the current element in this enumerator
        ''' </summary>
        ''' <exception cref="InvalidOperationException">If this enumerator is before
        ''' the first element or after the last element.</exception>
        Private ReadOnly Property Current1() As Object Implements IEnumerator.Current
            Get
                Return Current
            End Get
        End Property

        ''' <summary>
        ''' Attempts to move this enumerator to the next element
        ''' </summary>
        ''' <returns>True if this enumerator was successfully moved to the next
        ''' element; False if there are no more elements to return</returns>
        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext

            ' Deal with none to return...
            If mMaxNumber = 0 Then mIndex = AFTER_END

            ' Check that we've not reached the 'end' of our window in the enumerator
            If mIndex = AFTER_END Then Return False

            ' If we're before the start - work index up to the start, and return that.
            If mIndex = BEFORE_START Then
                mIndex = -1
                While mIndex < mStart
                    mIndex += 1
                    If Not mIter.MoveNext() Then    ' index is beyond the count of the enumerator
                        mIndex = AFTER_END
                        Return False
                    End If
                End While
                ' We've reached the 'start' point of the iterator - set our current object.
                mCurrent = mIter.Current
                Return True
            End If
            ' Otherwise we're still within the scope of the window
            ' index points to the last element returned
            mIndex += 1 ' Point index to next element

            ' Check 1) if we've returned all that we should, or
            ' 2) if the enumerator has run out of elements to return
            ' In either case, set this enumerator to 'Done'
            If mIndex - mStart >= mMaxNumber OrElse Not mIter.MoveNext Then
                mIndex = AFTER_END
                mCurrent = Nothing
                Return False
            End If

            ' index points to the new object, as does the enumerator,
            ' so set it and return true to indicate there is a value there.
            mCurrent = mIter.Current
            Return True

        End Function

        ''' <summary>
        ''' Resets this enumerator so that it can be used again.
        ''' </summary>
        Public Sub Reset() Implements IEnumerator.Reset
            mIndex = BEFORE_START
            mIter.Reset()
            mCurrent = Nothing
        End Sub

        ''' <summary>
        ''' Disposes of this enumerator and its contained enumerator
        ''' </summary>

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

        Private mDisposed As Boolean

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not mDisposed Then
                If disposing Then
                    mIter.Dispose()
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
