
Imports System.Runtime.Serialization
Imports BluePrism.AutomateAppCore.clsServerPartialClasses
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Utilities.Functional

Namespace Auth

    ''' <summary>
    ''' Class representing an identifiable, named entity which maintains a group of
    ''' permissions.
    ''' </summary>
    <Serializable()>
    <DataContract([Namespace]:="bp", Name:="ph")>
    <KnownType(GetType(clsSet(Of Permission)))>
    Public MustInherit Class PermHolder
        Implements ICloneable, ICollection(Of Permission), IEquatable(Of PermHolder)

#Region " Member Variables "

        ' The database ID of this perm holder
        <DataMember>
        Private mId As Integer

        ' The name of this perm holder
        <DataMember>
        Private mName As String

        ' The permissions which are being held by this object
        <DataMember>
        Private mPerms As IBPSet(Of Permission)

        <DataMember>
        Private mRequiredFeature As Feature

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Creates a new perm holder using the specified values
        ''' </summary>
        ''' <param name="id">The ID of this role. Typically, this is the ID of the
        ''' holder on the database, though in some cases it may be a temporary
        ''' holding identifier, depending on the concrete class.
        ''' </param>
        ''' <param name="name">The name of the role</param>
        Protected Sub New(ByVal id As Integer, ByVal name As String, requiredFeature As Feature)
            mId = id
            mName = name
            mRequiredFeature = requiredFeature
        End Sub

        ''' <summary>
        ''' Creates a new permission group using data from the given provider.
        ''' </summary>
        ''' <param name="prov">The provider which offers the data required to create
        ''' a new permission group. This constructor expects the provider to offer
        ''' an integer <c>"id"</c> argument, and a string <c>"name"</c> argument.
        ''' </param>
        Protected Sub New(ByVal prov As IDataProvider)
            Me.New(
                prov.GetValue("id", 0),
                prov.GetString("name"),
                prov.GetValue("requiredFeature", "None").
                      Map(Function(x) If(String.IsNullOrEmpty(x), "None", x)).
                      Map(Function(x) DirectCast([Enum].Parse(GetType(Feature), x), Feature)))
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' The integer ID of this permission group as held on the database.
        ''' </summary>
        Public Property Id() As Integer
            Get
                Return mId
            End Get
            Set(ByVal value As Integer)
                If mId = value Then Return
                mId = value
            End Set
        End Property

        ''' <summary>
        ''' The name of this permission group.
        ''' </summary>
        Public Property Name() As String
            Get
                If mName Is Nothing Then Return "" Else Return mName
            End Get
            Set(ByVal value As String)
                mName = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the count of permissions in this holder
        ''' </summary>
        Public ReadOnly Property Count() As Integer _
         Implements ICollection(Of Permission).Count
            Get
                Return WritablePermissions.Count
            End Get
        End Property

        ''' <summary>
        ''' The permissions held in this group. Note that this collection is readonly
        ''' and will throw an <see cref="InvalidOperationException"/> if any attempt
        ''' is made to modify it.
        ''' </summary>
        Public ReadOnly Property Permissions() As ICollection(Of Permission)
            Get
                Return GetReadOnly.ICollection(WritablePermissions)
            End Get
        End Property

        Public ReadOnly Property RequiredFeature As Feature
            Get
                Return mRequiredFeature
            End Get
        End Property

        ''' <summary>
        ''' The permissions held in this group in a writable collection. This is
        ''' only used when initialising the permission groups.
        ''' </summary>
        Protected ReadOnly Property WritablePermissions() _
         As IBPSet(Of Permission)
            Get
                If mPerms Is Nothing Then mPerms = New clsSet(Of Permission)
                Return mPerms
            End Get
        End Property

        ''' <summary>
        ''' Flag indicating if this holder is readonly or not.
        ''' </summary>
        Private ReadOnly Property IsReadOnly() As Boolean _
         Implements ICollection(Of Permission).IsReadOnly
            Get
                Return False
            End Get
        End Property

#End Region

#Region " Methods "

        ''' <summary>
        ''' Adds a permission with the given name to this group. This will be a no-op
        ''' if no permission exists with the given name
        ''' </summary>
        ''' <param name="permName">The name of the permission to add</param>
        Public Sub Add(ByVal permName As String)
            Add(Permission.GetPermission(permName))
        End Sub

        ''' <summary>
        ''' Adds the given permission to this group. No-ops if the given permission
        ''' is null.
        ''' </summary>
        ''' <param name="perm">The permission to add. This method will have no effect
        ''' if <paramref name="perm"/> is null.</param>
        Public Sub Add(ByVal perm As Permission) _
         Implements ICollection(Of Permission).Add
            If perm IsNot Nothing Then WritablePermissions.Add(perm)
        End Sub

        ''' <summary>
        ''' Adds the given permissions to this holder.
        ''' </summary>
        ''' <param name="perms">The permissions to add to this holder.</param>
        Public Sub AddAll(ByVal perms As IEnumerable(Of Permission))
            For Each p As Permission In perms : Add(p) : Next
        End Sub

        ''' <summary>
        ''' Clears the permissions on this group
        ''' </summary>
        Public Sub Clear() Implements ICollection(Of Permission).Clear
            mPerms = Nothing
        End Sub

        ''' <summary>
        ''' Checks if this perm holder contains the given permission
        ''' </summary>
        ''' <param name="p">The permission to check for in this holder</param>
        ''' <returns>True if the given permission is held in this object; False
        ''' otherwise.</returns>
        Public Function Contains(ByVal p As Permission) As Boolean _
         Implements ICollection(Of Permission).Contains
            Return p IsNot Nothing AndAlso Permissions.Contains(p)
        End Function

        ''' <summary>
        ''' Removes the given permission from this holder, returning a flag
        ''' indicating if the permission was removed or not.
        ''' </summary>
        ''' <param name="p">The permission to remove</param>
        ''' <returns>True if the permission was found and removed</returns>
        Public Function Remove(ByVal p As Permission) As Boolean _
         Implements ICollection(Of Permission).Remove
            Return WritablePermissions.Remove(p)
        End Function

        ''' <summary>
        ''' Removes the permission with the given name from this holder, returning a
        ''' flag indicating if the permission was removed or not.
        ''' </summary>
        ''' <param name="permName">The name of the permission to remove</param>
        ''' <returns>True if the permission was found and removed from this holder;
        ''' False if it was not found.</returns>
        Public Function Remove(ByVal permName As String) As Boolean
            Return Remove(Permission.GetPermission(permName))
        End Function

        ''' <summary>
        ''' Removes all of the given permissions from this holder.
        ''' </summary>
        ''' <param name="perms">The permissions to remove</param>
        ''' <returns>True if this holder has changed as a result of this operation,
        ''' ie. if <em>any</em> of the given permissions were found in this holder
        ''' and subsequently removed.</returns>
        Public Function RemoveAll(ByVal perms As IEnumerable(Of Permission)) _
         As Boolean
            Dim removedAny As Boolean = False
            For Each p As Permission In perms
                If Remove(p) Then removedAny = True
            Next
            Return removedAny
        End Function

        ''' <summary>
        ''' Checks if this perm holder contains all of the permissions in the given
        ''' collection.
        ''' </summary>
        ''' <param name="perms">The collection of permissions to check</param>
        ''' <returns>True if the given permission collection is empty, or if all of
        ''' the permissions in it were found in this perm holder.</returns>
        Public Function ContainsAll(ByVal perms As ICollection(Of Permission)) _
         As Boolean
            For Each p As Permission In perms
                If Not Permissions.Contains(p) Then Return False
            Next
            Return True
        End Function

        ''' <summary>
        ''' Checks if this perm holder contains all of the permissions in the given
        ''' holder.
        ''' </summary>
        ''' <param name="holder">The permission holder to check</param>
        ''' <returns>True if the given permission collection is empty, or if all of
        ''' the permissions in it were found in this perm holder.</returns>
        Public Function ContainsAll(ByVal holder As PermHolder) As Boolean
            Return ContainsAll(holder.Permissions)
        End Function

        ''' <summary>
        ''' Checks if this perm holder contains any of the given permissions.
        ''' </summary>
        ''' <param name="perms">The permissions to check to see if any occur in this
        ''' permission holder.</param>
        ''' <returns>True if any of the given permissions occur in this holder.
        ''' </returns>
        Public Function ContainsAny(ByVal perms As ICollection(Of Permission)) _
         As Boolean
            For Each p As Permission In perms
                If Permissions.Contains(p) Then Return True
            Next
            Return False
        End Function

        ''' <summary>
        ''' Clones this permission holder object
        ''' </summary>
        ''' <returns>A deep clone of this object.</returns>
        Public Overridable Function Clone() As Object Implements ICloneable.Clone
            Dim copy As PermHolder = DirectCast(MemberwiseClone(), PermHolder)

            ' Copy across the perms if we have a collection there
            If mPerms IsNot Nothing Then
                ' Setting to null will cause WritablePermissions to autogenerate the
                ' appropriate collection to use when accessed
                copy.mPerms = Nothing

                ' Permissions are immutable and we can thus use the same reference
                copy.WritablePermissions.Union(mPerms)

            End If

            Return copy
        End Function

        ''' <summary>
        ''' Checks if the given object is equal to this perm holder.
        ''' </summary>
        ''' <param name="obj">The object to test for equality</param>
        ''' <returns>True if the given object is a non-null permholder with the same
        ''' ID, Name and permissions as this object; False otherwise.</returns>
        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            Dim ph As PermHolder = TryCast(obj, PermHolder)
            If ph Is Nothing Then Return False
            Me.Equals(ph)
        End Function


        Public Overloads Function Equals(other As PermHolder) As Boolean Implements IEquatable(Of PermHolder).Equals
            If other Is Nothing Then Return False
            If ReferenceEquals(Nothing, other) Then Return False
            If ReferenceEquals(Me, other) Then Return True
            Return mId = other.mId AndAlso String.Equals(mName, other.mName) _
                AndAlso CollectionUtil.AreEquivalent(mPerms, other.mPerms)
        End Function

        ''' <summary>
        ''' Gets an integer hash for this perm holder. This is purely a function of
        ''' the integer ID and name.
        ''' </summary>
        ''' <returns>An integer hash representing this object.</returns>
        Public Overrides Function GetHashCode() As Integer
            Return Id.GetHashCode() Xor Name.GetHashCode()
        End Function

        ''' <summary>
        ''' Gets a string representation of this perm holder.
        ''' </summary>
        ''' <returns>A string representation of this perm holder.</returns>
        Public Overrides Function ToString() As String
            Return String.Format("[PermHolder-Id:{0};Name:{1}]", Id, Name)
        End Function

        ''' <summary>
        ''' Copies the permissions in this holder to the given array.
        ''' </summary>
        ''' <param name="array">The array into which the permissions should be copied
        ''' </param>
        ''' <param name="arrayIndex">The index at which to begin the permissions.
        ''' </param>
        Public Sub CopyTo(ByVal array() As Permission, ByVal arrayIndex As Integer) _
         Implements ICollection(Of Permission).CopyTo
            Permissions.CopyTo(array, arrayIndex)
        End Sub

        ''' <summary>
        ''' Gets an enumerator over the permissions held in this role
        ''' </summary>
        ''' <returns>An initialised enumerator to loop over the permissions held in
        ''' this role.</returns>
        Public Function GetEnumerator() As IEnumerator(Of Permission) _
         Implements IEnumerable(Of Permission).GetEnumerator
            Return Permissions.GetEnumerator()
        End Function

        ''' <summary>
        ''' Gets an enumerator over the permissions held in this role
        ''' </summary>
        ''' <returns>An initialised enumerator to loop over the permissions held in
        ''' this role.</returns>
        Private Function GetNonGenericEnumerator() As IEnumerator _
         Implements IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function

#End Region
    End Class


End Namespace

