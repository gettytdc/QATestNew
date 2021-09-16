Imports System.Runtime.Serialization
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.Images
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.AutomateAppCore.Auth

Namespace Groups

    <Serializable, DataContract(Name:="pgm", [Namespace]:="bp"), KnownType(GetType(ProcessAttributes))>
    Public Class ProcessGroupMember : Inherits ProcessBackedGroupMember

#Region " Class-scope Declarations "

        ''' <summary>
        ''' A filter which can be applied to a tree which allows all published
        ''' group members to pass, but no others
        ''' </summary>
        Public Shared ReadOnly Property PublishedFilter As Predicate(Of IGroupMember)
            Get
                Return _
                 Function(gm)
                     ' If it's a process, allow any published processes through
                     Dim pgm = TryCast(gm, ProcessGroupMember)
                     Return (
                         pgm IsNot Nothing AndAlso
                         pgm.Attributes.HasFlag(ProcessAttributes.Published)
                     )
                 End Function
            End Get
        End Property

        ''' <summary>
        ''' A filter which can be applied to a tree which allows all non-retired
        ''' group members to pass.
        ''' </summary>
        Public Shared ReadOnly Property NotRetiredFilter As Predicate(Of IGroupMember)
            Get
                Return _
                 Function(gm)
                     ' If it's a process, allow any published processes through
                     ' except those which are retired also
                     Dim pgm = TryCast(gm, ProcessGroupMember)
                     Return (
                         pgm IsNot Nothing AndAlso
                         Not pgm.Attributes.HasFlag(ProcessAttributes.Retired)
                     )
                 End Function
            End Get
        End Property

        ''' <summary>
        ''' A filter which can be applied to a tree which allows all published,
        ''' non-retired group members to pass.
        ''' </summary>
        Public Shared ReadOnly Property PublishedAndNotRetiredFilter As Predicate(Of IGroupMember)
            Get
                Return _
                 Function(gm)
                     ' If it's a process, allow any published processes through
                     ' except those which are retired also
                     Dim pgm = TryCast(gm, ProcessGroupMember)
                     Return (
                         pgm IsNot Nothing AndAlso
                         pgm.Attributes.HasFlag(ProcessAttributes.Published) AndAlso
                         Not pgm.Attributes.HasFlag(ProcessAttributes.Retired)
                     )
                 End Function
            End Get
        End Property

#End Region

        ''' <summary>
        ''' Creates a new empty process group member
        ''' </summary>
        Public Sub New()
            MyBase.New()
        End Sub

        ''' <summary>
        ''' Creates a new process group member with supplied data
        ''' </summary>
        ''' <param name="prov">The provider of the data to initialise this group
        ''' member with; this expects: <list>
        ''' <item>id: Guid: The ID of the process</item>
        ''' <item>name: String: The name of the process</item>
        ''' <item>description: String: The description of the process</item>
        ''' <item>createddate: DateTime: The date/time that the process was created
        ''' </item>
        ''' <item>createdby: String: The username of the user that created the
        ''' process</item>
        ''' <item>lastmodifieddate: DateTime: The date/time that the process was last
        ''' modified</item>
        ''' <item>lastmodifiedby: String: The username of the user that last modified
        ''' the process.</item>
        ''' </list></param>
        Public Sub New(prov As IDataProvider)
            MyBase.New(prov)
        End Sub

        ''' <summary>
        ''' Gets the image key for a group
        ''' </summary>
        Public Overrides ReadOnly Property ImageKey As String
            Get

                If IsLocked Then
                    If Permissions.IsRestricted Then
                        Return ImageLists.Keys.Component.ProcessLocked
                    End If
                    Return ImageLists.Keys.Component.ProcessGlobalLocked
                End If

                Return If(Permissions.IsRestricted,
                    ImageLists.Keys.Component.Process,
                    ImageLists.Keys.Component.ProcessGlobal)
            End Get
        End Property

        ''' <summary>
        ''' Gets the process type for this process - namely
        ''' <see cref="DiagramType.Process"/>
        ''' </summary>
        Public Overrides ReadOnly Property ProcessType As DiagramType
            Get
                Return DiagramType.Process
            End Get
        End Property

        ''' <summary>
        ''' The type of group member that this object represents.
        ''' </summary>
        Public Overrides ReadOnly Property MemberType As GroupMemberType
            Get
                Return GroupMemberType.Process
            End Get
        End Property       

        ''' <summary>
        ''' Gets a dependency with which references to this group member can be
        ''' searched for, or null if this group member cannot be searched for
        ''' dependencies.
        ''' </summary>
        Public Overrides ReadOnly Property Dependency As clsProcessDependency
            Get
                If IdAsGuid() = Guid.Empty OrElse Name = "" Then Return Nothing
                Return New clsProcessIDDependency(IdAsGuid, Name)
            End Get
        End Property

        Public Overrides Function HasViewPermission(user As IUser) As Boolean
            Return Permissions.HasPermission(user, Permission.ProcessStudio.AllProcessPermissionsAllowingTreeView)
        End Function

    End Class
End Namespace
