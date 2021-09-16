
''' <summary>
''' Class to encapsulate a comparer which reverses the order of a specified comparer,
''' or the natural order according to the default for the type.
''' </summary>
''' <typeparam name="T">The type of element whose order is to be reversed</typeparam>
<Serializable>
Public Class clsReverseComparer(Of T) : Implements IComparer(Of T)

    ' The comparer which provides the 'forward' order
    Private mComparer As IComparer(Of T)

    ''' <summary>
    ''' Creates a new reverse comparer of the specified type using its natural order
    ''' according to <see cref="Comparer(Of T).Default"/>.
    ''' </summary>
    Public Sub New()
        Me.New(Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new reverse comparer of the specified type which provides the
    ''' reverse of the order specified by the given comparer.
    ''' </summary>
    ''' <param name="comp">The comparer which provides the 'forward' order for the
    ''' type being compared. If null, the default comparer for the type is used as
    ''' the 'forward' comparison.</param>
    Public Sub New(ByVal comp As IComparer(Of T))
        If comp Is Nothing Then comp = Comparer(Of T).Default
        mComparer = comp
    End Sub

    ''' <summary>
    ''' Compares the given elements, returning the opposite of the 'forward'
    ''' comparison call.
    ''' </summary>
    ''' <param name="x">The element to compare</param>
    ''' <param name="y">The element to compare to <paramref name="x"/></param>
    ''' <returns>-1, 0 or 1 if <paramref name="x"/> is less than, equal to or greater
    ''' than <paramref name="y"/> respectively, according to the comparer configured
    ''' in this object at construction time.</returns>
    Public Function Compare(ByVal x As T, ByVal y As T) As Integer _
     Implements IComparer(Of T).Compare
        Return mComparer.Compare(y, x)
    End Function

End Class
