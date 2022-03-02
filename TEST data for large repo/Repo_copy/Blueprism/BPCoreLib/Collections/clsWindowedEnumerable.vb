Namespace Collections

    ''' Project  : BPCoreLib
    ''' Class    : clsWindowedEnumerable
    ''' <summary>
    ''' An enumerable which provides a view over an enumerable with constraints,
    ''' allowing a subset of that collection to be traversed in an enumerator
    ''' </summary>
    ''' <typeparam name="T">The type of the enumerable</typeparam>
    <Serializable, DebuggerDisplay("Count: {Count}")> _
    Public Class clsWindowedEnumerable(Of T) : Implements IEnumerable(Of T)

        ''' <summary>
        ''' The enumerable to provide a window onto 
        ''' </summary>
        Private mEnum As IEnumerable(Of T)

        ''' <summary>
        ''' The index of the first element to form part of the view.
        ''' </summary>
        Private mStart As Integer

        ''' <summary>
        ''' The maximum number of elements to return within this view.
        ''' </summary>
        Private mMaxNumber As Integer

        ''' <summary>
        ''' Creates a new windowed enumerable wrapping the given enumerable,
        ''' and indicating the bounds of the window onto that enumerable.
        ''' </summary>
        ''' <param name="enumerable">The enumerable which this object is
        ''' providing a view over.</param>
        ''' <param name="start">The index of the first element which forms
        ''' part of this view.</param>
        ''' <param name="maxNumber">The maximum number of elements to be
        ''' allowed within this view.</param>
        ''' <exception cref="ArgumentNullException">If the given enumerable is null
        ''' </exception>
        ''' <exception cref="ArgumentOutOfRangeException">If : <list>
        ''' <item><paramref name="start"/> is negative -or-</item>
        ''' <item><paramref name="maxNumber"/> is negative -or-</item>
        ''' <item>The sum of the <paramref name="start"/> and
        ''' <paramref name="maxNumber"/> parameters is beyond Integer.MaxValue</item>
        ''' </list>
        ''' </exception>
        Public Sub New( _
         ByVal enumerable As IEnumerable(Of T), ByVal start As Integer, ByVal maxNumber As Integer)

            If enumerable Is Nothing Then Throw New ArgumentNullException(NameOf(enumerable))
            If start < 0 Then
                Throw New ArgumentOutOfRangeException(
                 NameOf(start), String.Format(My.Resources.clsWindowedEnumerable_NegativeValuesAreNotAllowed0, start))
            End If
            If maxNumber < 0 Then
                Throw New ArgumentOutOfRangeException(
                 NameOf(maxNumber), String.Format(My.Resources.clsWindowedEnumerable_NegativeValuesAreNotAllowed0, maxNumber))
            End If
            If CLng(start) + CLng(maxNumber) > CLng(Integer.MaxValue) Then
                Throw New ArgumentOutOfRangeException(
                  String.Format(My.Resources.clsWindowedEnumerable_StartMaxNumberSummedTogetherGoBeyondTheMaximumAllowedValueStart0MaxNumber1, start, maxNumber))
            End If

            mEnum = enumerable
            mStart = start
            mMaxNumber = maxNumber

        End Sub

        ''' <summary>
        ''' Gets an enumerator over the view of the wrapped enumerable.
        ''' </summary>
        ''' <returns>An enumerator which enumerates only the elements that
        ''' fall within the bounds specified within this window.</returns>
        Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of T) Implements System.Collections.Generic.IEnumerable(Of T).GetEnumerator
            Return New clsWindowedEnumerator(Of T)(mEnum, mStart, mMaxNumber)
        End Function

        ''' <summary>
        ''' Gets an enumerator over the view of the wrapped enumerable.
        ''' </summary>
        ''' <returns>An enumerator which enumerates only the elements that
        ''' fall within the bounds specified within this window.</returns>
        Private Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function

    End Class

End Namespace
