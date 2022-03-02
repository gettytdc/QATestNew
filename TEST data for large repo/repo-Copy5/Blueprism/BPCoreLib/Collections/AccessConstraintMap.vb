Imports System.Runtime.Serialization

Namespace Collections

    ''' <summary>
    ''' Mapping to represent the accessibility of a number of accessors to a
    ''' particular resource.
    ''' </summary>
    ''' <typeparam name="TResource">The type of the resource whose access is limited
    ''' to a set of allowed accessors.</typeparam>
    ''' <typeparam name="TAccessor">The type of the accessor which is provided access
    ''' to the resource</typeparam>
    <Serializable, DebuggerDisplay("Count: {Count}")>
    Public Class AccessConstraintMap(Of TResource, TAccessor)
        Inherits Dictionary(Of TResource, AccessSet(Of TAccessor))

        ' Flag indicating if a default value in an access set means 'allow all'
        Private mDefaultMeansAllowAll As Boolean

        ''' <summary>
        ''' Creates a new empty access constraint map where adding a default accessor
        ''' means that 'allow all' is set within the corresponding access set.
        ''' </summary>
        Public Sub New()
            Me.New(True)
        End Sub

        ''' <summary>
        ''' Creates a new empty access constraint map
        ''' </summary>
        ''' <param name="useDefaultValueAsAllowAll">True to indicate that adding a
        ''' default accessor value will alter the owning access set to allow all;
        ''' False to indicate that a default accessor value is treated just like any
        ''' other accessor value.</param>
        Public Sub New(ByVal useDefaultValueAsAllowAll As Boolean)
            mDefaultMeansAllowAll = useDefaultValueAsAllowAll
        End Sub

        ''' <summary>
        ''' Special constructor for Dictionary's ISerializable interface
        ''' </summary>
        Protected Sub New(info As SerializationInfo, context As StreamingContext)
            MyBase.New(info, context)
        End Sub

        ''' <summary>
        ''' Gets the access set for the given resource, creating it first if it
        ''' doesn't already exist within this map.
        ''' </summary>
        ''' <param name="res">The resource for which the access set is required.
        ''' </param>
        ''' <returns>The set registered with the given resource, which may have been
        ''' created as a result of this call if it did not already exist.</returns>
        Protected Function GetAccessSetFor(ByVal res As TResource) _
         As AccessSet(Of TAccessor)
            Dim s As AccessSet(Of TAccessor) = Nothing
            If TryGetValue(res, s) Then Return s
            s = New AccessSet(Of TAccessor)
            s.DefaultItemSetsAccessToAll = mDefaultMeansAllowAll
            Me(res) = s
            Return s
        End Function

        ''' <summary>
        ''' Adds an accessor to the given resource. If the given accessor is a
        ''' default value (null or default(T)), then this may be treated as meaning
        ''' 'allow all' on the corresponding set, depending on how this map was
        ''' constructed.
        ''' </summary>
        ''' <param name="res">The resource to which an additional accessor is being
        ''' added.</param>
        ''' <param name="acc">The accessor to add to the resource</param>
        ''' <returns>True if the corresponding access set was altered as a result of
        ''' this operation; False if no changes were made (ie. the accessor was
        ''' already in the set, or the set is configured to allow access to all)
        ''' </returns>
        Public Function AddAccessor(ByVal res As TResource, ByVal acc As TAccessor) _
         As Boolean
            Return GetAccessSetFor(res).Add(acc)
        End Function

        ''' <summary>
        ''' Removes an accessor from the given resource.
        ''' </summary>
        ''' <param name="res">The resource from which the accessor should be removed
        ''' </param>
        ''' <param name="acc">The accessor to remove from the resource</param>
        ''' <returns>True if the corresponding access set was altered as a result of
        ''' removing the accessor.</returns>
        Public Function RemoveAccessor(ByVal res As TResource, ByVal acc As TAccessor) _
         As Boolean
            Return GetAccessSetFor(res).Remove(acc)
        End Function

        ''' <summary>
        ''' Checks if an accessor has access to a specified resource.
        ''' </summary>
        ''' <param name="res">The resource to check access to</param>
        ''' <param name="acc">The accessor whose access is being requested.</param>
        ''' <returns>True if the given accessor has access; False otherwise.
        ''' </returns>
        Public Function HasAccess(ByVal res As TResource, ByVal acc As TAccessor) _
         As Boolean
            Dim s As AccessSet(Of TAccessor) = Nothing
            If Not TryGetValue(res, s) Then Return False
            Return s.HasAccess(acc)
        End Function

        ''' <summary>
        ''' Sets the access set corresponding to a specified resource to allow access
        ''' to all accessors.
        ''' </summary>
        ''' <param name="res">The resource for which access should be provided to all
        ''' </param>
        ''' <param name="allow">True to set the AccessSet corresponding to the given
        ''' resource to allow access to all accessors.</param>
        Public Sub SetAllowAll(ByVal res As TResource, ByVal allow As Boolean)
            GetAccessSetFor(res).AccessToAll = allow
        End Sub

    End Class

End Namespace