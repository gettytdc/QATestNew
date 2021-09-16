Imports System.Runtime.Serialization
Imports BluePrism.Images
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.AutomateAppCore.Auth

Namespace Groups

    <Serializable>
    <DataContract(Name:="ogm", [Namespace]:="bp")>
    Public Class ObjectGroupMember : Inherits ProcessBackedGroupMember

        ''' <summary>
        ''' Creates a new empty object group member
        ''' </summary>
        Public Sub New()
            MyBase.New()
        End Sub

        ''' <summary>
        ''' Creates a new object group member with supplied data
        ''' </summary>
        ''' <param name="prov">The provider of the data to initialise this group
        ''' member with; this expects: <list>
        ''' <item>id: Guid: The ID of the object</item>
        ''' <item>name: String: The name of the object</item>
        ''' <item>description: String: The description of the object</item>
        ''' <item>createddate: DateTime: The date/time that the object was created
        ''' </item>
        ''' <item>createdby: String: The username of the user that created the object
        ''' </item>
        ''' <item>lastmodifieddate: DateTime: The date/time that the object was last
        ''' modified</item>
        ''' <item>lastmodifiedby: String: The username of the user that last modified
        ''' the object.</item>
        ''' </list></param>
        Public Sub New(prov As IDataProvider)
            MyBase.New(prov)
        End Sub

        ''' <summary>
        ''' A filter which can be applied to a tree which allows all non-retired,
        ''' but shareable objects to pass, but no others
        ''' </summary>
        Public Shared ReadOnly Property ShareableAndNotRetired As Predicate(Of IGroupMember)
            Get
                Return _
                 Function(gm)
                     ' If it's an Object, allow any active and shareable Objects through
                     Dim ogm = TryCast(gm, ObjectGroupMember)
                     If ogm Is Nothing Then Return False
                     Return Not ogm.HasAnyAttribute(ProcessAttributes.Retired) AndAlso
                        ogm.IsShareable
                 End Function
            End Get
        End Property

        ''' <summary>
        ''' Gets the image key for a group
        ''' </summary>
        Public Overrides ReadOnly Property ImageKey As String
            Get

                If IsLocked Then
                    If Permissions.IsRestricted Then
                        Return ImageLists.Keys.Component.ObjectLocked
                    End If
                    Return ImageLists.Keys.Component.ObjectGlobalLocked
                End If

                Return If(Permissions.IsRestricted,
                    ImageLists.Keys.Component.Object,
                    ImageLists.Keys.Component.ObjectGlobal)
            End Get
        End Property

        ''' <summary>
        ''' Gets the process type for this object - namely
        ''' <see cref="DiagramType.[Object]"/>
        ''' </summary>
        Public Overrides ReadOnly Property ProcessType As DiagramType
            Get
                Return DiagramType.Object
            End Get
        End Property

        ''' <summary>
        ''' The type of group member that this object represents.
        ''' </summary>
        Public Overrides ReadOnly Property MemberType As GroupMemberType
            Get
                Return GroupMemberType.Object
            End Get
        End Property

        ''' <summary>
        ''' Gets a dependency with which references to this group member can be
        ''' searched for, or null if this group member cannot be searched for
        ''' dependencies.
        ''' </summary>
        Public Overrides ReadOnly Property Dependency As clsProcessDependency
            Get
                Dim nm As String = Name
                If nm = "" Then Return Nothing
                Return New clsProcessNameDependency(nm)
            End Get
        End Property

        Public Overrides Function HasViewPermission(user As IUser) As Boolean
            Return Permissions.HasPermission(user, Permission.ObjectStudio.AllObjectPermissionsAllowingTreeView)
        End Function
    End Class

End Namespace