
Imports System.Runtime.Serialization
Imports BluePrism.AutomateAppCore.clsServerPartialClasses
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Server.Domain.Models

Namespace Auth

    ''' <summary>
    ''' Class representing a group of permissions from the database
    ''' </summary>
    <Serializable()>
    <DataContract([Namespace]:="bp")>
    Public Class PermissionGroup : Inherits PermHolder

#Region " Class-scope declarations "

        ' The statically maintained mapping of groups to their names
        Private Shared mGroups As IDictionary(Of String, PermissionGroup)

        ''' <summary>
        ''' Initialises the permission groups, creating a mapping of groups against
        ''' their names. After this, the groups will contain their IDs and Names
        ''' only; ie. none will contain any group members.
        ''' </summary>
        ''' <param name="prov">The provider which will be providing the initial data
        ''' for the groups. This is expected to provide an integer <c>"id"</c> field
        ''' and a string <c>"name"</c> field.</param>
        Friend Shared Sub Init(ByVal prov As IMultipleDataProvider)
            Dim map As New Dictionary(Of String, PermissionGroup)
            While prov.MoveNext()
                Dim pg As New PermissionGroup(prov)
                map(pg.Name) = pg
            End While
            mGroups = GetReadOnly.IDictionary(GetSynced.IDictionary(map))
        End Sub

        ''' <summary>
        ''' Loads a map of all the permission groups found in a data provider, using
        ''' a pre-initialised map of permissions to populate the group with.
        ''' </summary>
        ''' <param name="prov">The multi-data provider which will provide the group
        ''' and member data. Each row of the provider is expected to provide the "id"
        ''' and "name" of the permission group as required by the constructor,
        ''' and an integer "permid" value representing a single permission contained
        ''' in the referenced group.</param>
        ''' <param name="perms">The pre-populated dictionary of permissions keyed
        ''' against their permission IDs.</param>
        ''' <returns>A dictionary containing the groups created from the provided
        ''' data.</returns>
        Friend Shared Function Load(ByVal prov As IMultipleDataProvider, _
         ByVal perms As IDictionary(Of Integer, Permission)) _
         As IDictionary(Of Integer, PermissionGroup)
            Dim map As New Dictionary(Of Integer, PermissionGroup)
            While prov.MoveNext()
                Dim groupId As Integer = prov.GetValue("id", 0)
                Dim group As PermissionGroup = Nothing
                If Not map.TryGetValue(groupId, group) Then
                    group = New PermissionGroup(prov)
                    map(groupId) = group
                End If
                group.Add(perms(prov.GetValue("permid", 0)))
            End While
            Return map
        End Function

        ''' <summary>
        ''' Populates the static permission groups from a dictionary of groups
        ''' </summary>
        ''' <param name="groups">The permission groups from where the data is drawn
        ''' to populate this class.</param>
        Friend Shared Sub Populate( _
         ByVal groups As IDictionary(Of Integer, PermissionGroup))
            Dim map As New Dictionary(Of String, PermissionGroup)
            For Each pg As PermissionGroup In groups.Values
                map(pg.Name) = pg
            Next
            mGroups = GetReadOnly.IDictionary(GetSynced.IDictionary(map))
        End Sub

        ''' <summary>
        ''' Gets the groups set in this permission group class or an empty dictionary
        ''' if this class has not been initialised.
        ''' </summary>
        Private Shared ReadOnly Property Groups() _
         As IDictionary(Of String, PermissionGroup)
            Get
                If mGroups Is Nothing Then _
                 mGroups = New Dictionary(Of String, PermissionGroup)
                Return mGroups
            End Get
        End Property

        ''' <summary>
        ''' Gets the permission group with the given name.
        ''' </summary>
        ''' <param name="name">The name of the required permission group</param>
        ''' <returns>The group associated with the given name</returns>
        Public Shared Function GetGroup(ByVal name As String) As PermissionGroup
            If Not IsInitialised Then Throw New NotInitialisedException( _
             "PermissionGroup class has not been initialised ")
            Dim pg As PermissionGroup = Nothing
            If mGroups.TryGetValue(name, pg) Then Return pg
            Return Nothing
        End Function

        ''' <summary>
        ''' Gets whether this class has been initialised or not
        ''' </summary>
        Public Shared ReadOnly Property IsInitialised() As Boolean
            Get
                Return (mGroups IsNot Nothing AndAlso mGroups.Count > 0)
            End Get
        End Property

        ''' <summary>
        ''' Gets all of the permission groups loaded into this class.
        ''' </summary>
        Public Shared ReadOnly Property All() As ICollection(Of PermissionGroup)
            Get
                Return Groups.Values
            End Get
        End Property

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Creates a new permission group using data from the given provider.
        ''' </summary>
        ''' <param name="prov">The provider which offers the data required to create
        ''' a new permission group. This constructor expects the provider to offer
        ''' an integer <c>"id"</c> argument, and a string <c>"name"</c> argument.
        ''' </param>
        Private Sub New(ByVal prov As IDataProvider)
            MyBase.New(prov)
        End Sub


        ''' <summary>
        ''' Do not use - for data contact unit testing only.
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="name"></param>
        ''' <remarks></remarks>
        Friend Sub New(id As Integer, name As String)
            MyBase.New(id, name, Feature.None)
        End Sub
#End Region

    End Class


End Namespace

