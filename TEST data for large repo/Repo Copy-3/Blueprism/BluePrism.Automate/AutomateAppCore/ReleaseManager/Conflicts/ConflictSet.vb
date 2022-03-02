Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections

Public Class ConflictSet

    ' The errors from the last time this conflict set was resolved.
    ' If null, this indicates that the conflict set has never been resolved.
    Private mErrors As clsErrorLog

    ' The release that this conflict set relates to
    Private mRelease As clsRelease

    ' The conflicts mapped against the component that is conflicting
    Private mConflicts As IDictionary(Of PackageComponent, ICollection(Of Conflict))

    ''' <summary>
    ''' Creates a new conflict set for the given release.
    ''' </summary>
    ''' <param name="rel">The release that this conflict set is for.</param>
    Public Sub New(ByVal rel As clsRelease)
        mRelease = rel
    End Sub

    ''' <summary>
    ''' Checks if this conflict set has been resolved. This is considered to be true
    ''' if :- <list>
    ''' <item>There are no conflicts (duh) -or-</item>
    ''' <item>As a result of the last call to <see cref="Resolve"/>, all conflicts
    ''' are marked as resolved, and there are no outstanding errors </item></list>
    ''' Note that this will return <c>False</c> if there are conflicts and
    ''' <see cref="Resolve"/> has not yet been called on this conflict set.
    ''' </summary>
    Public ReadOnly Property IsResolved() As Boolean
        Get
            If CollectionUtil.IsNullOrEmpty(mConflicts) Then Return True
            For Each coll As ICollection(Of Conflict) In mConflicts.Values
                For Each c As Conflict In coll
                    If Not c.IsResolved Then Return False
                Next
            Next
            ' mErrors must exist (indicating Resolve() has been called) and the error
            ' count must be zero in order to be considered resolved
            Return (mErrors IsNot Nothing AndAlso mErrors.Count = 0)
        End Get
    End Property

    ''' <summary>
    ''' Checks if this conflict set actually contains any conflicts.
    ''' </summary>
    Public ReadOnly Property IsEmpty() As Boolean
        Get
            If mConflicts Is Nothing Then Return True
            For Each coll As ICollection(Of Conflict) In mConflicts.Values
                If coll.Count > 0 Then Return False
            Next
            Return True
        End Get
    End Property

    ''' <summary>
    ''' The conflicts held by this set mapped to the components that they are
    ''' conflicting for.
    ''' </summary>
    Public ReadOnly Property Conflicts() As IDictionary(Of PackageComponent, ICollection(Of Conflict))
        Get
            If mConflicts Is Nothing Then mConflicts = New clsOrderedDictionary(Of PackageComponent, ICollection(Of Conflict))
            Return mConflicts
        End Get
    End Property

    ''' <summary>
    ''' The conflicts held by this set in one large collection.
    ''' </summary>
    Public ReadOnly Property AllConflicts() As ICollection(Of Conflict)
        Get
            If mConflicts Is Nothing Then Return GetEmpty.ICollection(Of Conflict)()
            Dim all As New List(Of Conflict)
            For Each coll As ICollection(Of Conflict) In mConflicts.Values
                all.AddRange(coll)
            Next
            Return all
        End Get
    End Property

    ''' <summary>
    ''' Adds a conflict from the given definition against the given component.
    ''' </summary>
    ''' <param name="comp">The component on which the conflict has occurred.</param>
    ''' <param name="defn">The definition of the conflict which has occurred.</param>
    ''' <returns>The conflict object created from the given definition on the
    ''' specified component.</returns>
    Public Function Add(ByVal comp As PackageComponent, ByVal defn As ConflictDefinition) As Conflict
        Dim coll As ICollection(Of Conflict) = Nothing
        If Not Conflicts.TryGetValue(comp, coll) Then
            coll = New List(Of Conflict)
            Conflicts(comp) = coll
        End If
        Dim c As New Conflict(comp, defn)
        coll.Add(c)
        Return c
    End Function

    ''' <summary>
    ''' Adds all the conflicts from the given component to this conflict set.
    ''' </summary>
    ''' <param name="comp">The component from which to draw the conflicts.</param>
    Public Sub AddConflicts(ByVal comp As PackageComponent)
        Dim coll As ICollection(Of Conflict) = Nothing
        Dim isNew As Boolean = False
        If Not Conflicts.TryGetValue(comp, coll) Then
            coll = New List(Of Conflict)
            isNew = True
        End If
        For Each c As Conflict In comp.Conflicts
            coll.Add(c)
        Next
        If isNew AndAlso coll.Count > 0 Then Conflicts(comp) = coll
    End Sub

    ''' <summary>
    ''' Validates the given resolutions, returning the error log with all errors
    ''' detected by the components in this release.
    ''' </summary>
    ''' <param name="results">The conflict resolutions to validate</param>
    ''' <returns>A log of the errors found in the given resolutions - the tag in the
    ''' error is the package component affected, where appropriate (this may not be
    ''' set if the error was caused by multiple package components)</returns>
    Public Function Resolve(ByVal results As ICollection(Of ConflictResolution)) As clsErrorLog
        Dim errors As New clsErrorLog()
        For Each checker As Conflict.Resolver In mRelease.ConflictResolvers
            checker(mRelease, results, errors)
        Next
        mErrors = errors
        Return errors
    End Function

End Class
