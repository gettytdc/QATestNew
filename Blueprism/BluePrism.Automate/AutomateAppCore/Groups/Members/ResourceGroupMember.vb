Imports BluePrism.AutomateAppCore.TreeviewProcessing
Imports ResourceDBStatus =
    BluePrism.AutomateAppCore.Resources.ResourceMachine.ResourceDBStatus
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Images
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.AutomateAppCore.Resources.ResourceMachine
Imports BluePrism.AutomateAppCore.Auth
Imports System.Drawing
Imports System.Runtime.Serialization
Imports BluePrism.Core.Resources
Imports BluePrism.Server.Domain.Models

Namespace Groups

    ''' <summary>
    ''' Represents resources within a group
    ''' </summary>
    <Serializable()>
    <DataContract([Namespace]:="bp", [Name]:="rgm")>
    <KnownType(GetType(ResourceAttribute))>
    <KnownType(GetType(ResourceDBStatus))>
    Public Class ResourceGroupMember : Inherits GroupMember

#Region " Class-scope Declarations "

        ''' <summary>
        ''' A predicate which allows only those resources which are
        ''' <see cref="IsActive">active</see> - ie on which a session can be created
        ''' in Control (notwithstanding the run mode of any existing sessions on the
        ''' resource).
        ''' </summary>
        Public Shared ReadOnly Property Active As Predicate(Of IGroupMember)
            Get
                Return _
                    Function(m)
                        Dim rm = TryCast(m, ResourceGroupMember)
                        If rm Is Nothing Then Return False
                        Return rm.IsActive
                    End Function
            End Get
        End Property

        ''' <summary>
        ''' A filter which can be applied to a tree which allows all non-retired,
        ''' non-debug resource group members to pass, but no others
        ''' </summary>
        Public Shared ReadOnly Property NotRetiredAndNotDebug As Predicate(Of IGroupMember)
            Get
                Return _
                    Function(gm)
                        ' If it's a process, allow any published processes through
                        Dim rm = TryCast(gm, ResourceGroupMember)
                        If rm Is Nothing Then Return False
                        Return Not rm.HasAnyAttribute(
                            ResourceAttribute.Retired Or ResourceAttribute.Debug)
                    End Function
            End Get
        End Property

        ''' <summary>
        ''' A filter which can be applied to a tree which allows all non-retired,
        ''' non-debug resource group members to pass, but no others
        ''' </summary>
        Public Shared ReadOnly Property RetiredAndNotDebug As Predicate(Of IGroupMember)
            Get
                Return _
                    Function(gm)
                        ' If it's a process, allow any published processes through
                        Dim rm = TryCast(gm, ResourceGroupMember)
                        If rm Is Nothing Then Return False
                        Return rm.HasAttribute(ResourceAttribute.Retired) AndAlso
                           Not rm.HasAttribute(ResourceAttribute.Debug)
                    End Function
            End Get
        End Property

        ''' <summary>
        ''' A filter which can be applied to a tree which allows all resource groups
        ''' for which the current user has the Control Resource permission for to pass, but no others
        ''' </summary>
        Public Shared ReadOnly Property ControllableResourceGroup As Predicate(Of IGroupMember)
            Get
                Return Function(gm)
                           If TypeOf (gm) Is ResourcePool Then Return False
                           Dim gp = TryCast(gm, Group)
                           If gp Is Nothing Then Return False
                           Return gp.Permissions.HasPermission(User.Current, Permission.Resources.ControlResource)
                       End Function
            End Get
        End Property

        ''' <summary>
        ''' A filter which can be applied to a tree which allows all resources
        ''' for which the current user has the View Resource permission to pass, but no others
        ''' </summary>
        Public Shared ReadOnly Property ViewableResource As Predicate(Of IGroupMember)
            Get
                Return Function(gm)
                           Dim rm = TryCast(gm, ResourceGroupMember)
                           If rm Is Nothing Then Return False
                           Return rm.Permissions.HasPermission(User.Current, Permission.Resources.ImpliedViewResource)
                       End Function
            End Get
        End Property

        ''' <summary>
        ''' Inner class to hold the data names for the properties in this class
        ''' </summary>
        Private Class DataNames
            Public Const Attributes As String = "Attributes"
            Public Const ChildResourceCount As String = "ChildResourceCount"
            Public Const Status As String = "Status"
            Public Const Info As String = "Info"
            Public Const ConnectionState As String = "ConnectionState"
            Public Const IsPoolMember As String = "IsPoolMember"
            Public Const PoolMembers As String = "PoolMembers"
            Public Const LastError As String = "LastError"
            Public Const InfoColour As String = "InfoColour"
            Public Const Configuration As String = "Configuration"
        End Class

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Creates a new resource group member using data from a provider.
        ''' </summary>
        ''' <param name="prov">The provider of the data to initialise this group
        ''' member with - this expects: <list>
        ''' <item>id: Guid: The ID of the resource</item>
        ''' <item>name: String: The name of the resource</item>
        ''' <item>attributes: <see cref="ResourceAttribute"/>: The attributes of the
        ''' resource</item>
        ''' <item>status: <see cref="ResourceMachine.ResourceDBStatus"/>: The
        ''' status of this resource from the database.</item>
        ''' <item>ispoolmember: Boolean: Whether this resource represents a pool
        ''' member or not</item>
        ''' </list></param>
        Public Sub New(prov As IDataProvider)
            MyBase.New(prov)
            Attributes = prov.GetValue("attributes", ResourceAttribute.None)

            IsPoolMember = prov.GetValue("ispoolmember", False)

            If IsPoolMember Then
                Id = prov.GetGuid("Id")
                Name = prov.GetString("Name")
                Attributes = ResourceAttribute.None
                ChildResourceCount = 0
                LatestConnectionMessage = ""
                ConnectionState = ResourceConnectionState.Offline
                Status = ResourceStatus.Idle
            Else
                Dim dbstatus = prov.GetValue("statusid", ResourceDBStatus.Unknown)
                If dbstatus = ResourceDBStatus.Offline Then
                    Status = ResourceStatus.Offline
                ElseIf dbstatus = ResourceDBStatus.Pending Then
                    Status = ResourceStatus.Working
                ElseIf dbstatus = ResourceDBStatus.Ready Then
                    Status = ResourceStatus.Idle
                Else
                    Status = ResourceStatus.Offline
                End If
            End If
            Dim diagnostics = DirectCast(prov.GetValue("diagnostics", 0), clsAPC.Diags)
            Dim logToEventLog = prov.GetValue("logtoeventlog", True)

            Dim cfg As New CombinedConfig()
            cfg.SetLoggingStates(diagnostics, logToEventLog)
            Me.Configuration = cfg

            ConnectionState = Nothing
        End Sub

        ''' <summary>
        ''' Creates a new, empty process or object node
        ''' </summary>
        Public Sub New()
            Me.New(NullDataProvider.Instance)
        End Sub

        ''' <summary>
        ''' Creates a new resource group member from the data on the given resource
        ''' machine.
        ''' </summary>
        ''' <param name="r">The resource machine from which to draw the data.</param>
        Public Sub New(r As IResourceMachine)
            Me.New(NullDataProvider.Instance)
            MapUpdatedProperties(r)
        End Sub

        ''' <summary>
        ''' Creates a new resource group member, empty apart from the ID
        ''' </summary>
        ''' <param name="resourceId">The ID of the resource group member</param>
        Public Sub New(resourceId As Guid)
            Me.New(NullDataProvider.Instance)
            Id = resourceId
        End Sub

#End Region

#Region " Associated Data Properties "

        ''' <summary>
        ''' The attributes of the resource that this group member represents
        ''' </summary>
        <DataMember(Name:="a", EmitDefaultValue:=False)>
        Public Property Attributes As ResourceAttribute
            Get
                Return GetData(DataNames.Attributes, ResourceAttribute.None)
            End Get
            Set(value As ResourceAttribute)
                SetData(DataNames.Attributes, value)
            End Set
        End Property

        ''' <summary>
        ''' The number of child resources for this resource, if it represents a pool.
        ''' Note that this is specific to pool members, not to child members in a
        ''' group.
        ''' </summary>
        <DataMember(Name:="cr", EmitDefaultValue:=False)>
        Public Property ChildResourceCount As Integer
            Get
                Return GetData(DataNames.ChildResourceCount, 0)
            End Get
            Set(value As Integer)
                SetData(DataNames.ChildResourceCount, value)
            End Set
        End Property

        ''' <summary>
        ''' The status of this resource from the database
        ''' </summary>
        <DataMember(Name:="st", EmitDefaultValue:=False)>
        Public Property Status As ResourceStatus
            Get
                Return GetData(DataNames.Status, ResourceStatus.Offline)
            End Get
            Set(value As ResourceStatus)
                SetData(DataNames.Status, value)
            End Set
        End Property

        ''' <summary>
        ''' Returns a user friendly string representing the resource's status
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property StatusText As String
            Get
                Return ResourceInfo.GetResourceStatusFriendlyName(Status)
            End Get
        End Property

        ''' <summary>
        ''' The connection state to the resource represented by this group member.
        ''' This may not be set since it's not a database-retrieved value; it must
        ''' be added after the group member has been retrieved from the database,
        ''' using data gleaned from a resource connection to the actual machine.
        ''' </summary>
        <DataMember(Name:="cs", EmitDefaultValue:=False)>
        Public Property ConnectionState As Nullable(Of ResourceConnectionState)
            Get
                Return GetData(
                    DataNames.ConnectionState, CType(0, ResourceConnectionState))
            End Get
            Set(value As Nullable(Of ResourceConnectionState))
                SetData(DataNames.ConnectionState, value)
            End Set
        End Property

        ''' <summary>
        ''' Gets the last connection error message encountered on this Resource.
        ''' </summary>
        <DataMember(Name:="l", EmitDefaultValue:=False)>
        Public Property LatestConnectionMessage As String
            Get
                Return GetData(
                    DataNames.LastError, CType("", String))
            End Get
            Set(value As String)
                SetData(DataNames.LastError, value)
            End Set
        End Property

        ''' <summary>
        ''' Holds an informational message about the resource
        ''' </summary>
        <DataMember(Name:="in", EmitDefaultValue:=False)>
        Public Property Info As String
            Get
                Return GetData(
                    DataNames.Info, CType("", String))
            End Get
            Set(value As String)
                SetData(DataNames.Info, value)
            End Set
        End Property

        ''' <summary>
        ''' Holds a colour representing the status of the resource
        ''' </summary>
        <DataMember(Name:="ic", EmitDefaultValue:=False)>
        Public Property InfoColour As Integer
            Get
                Return GetData(DataNames.InfoColour, Color.Black).ToArgb
            End Get
            Set(value As Integer)
                SetData(DataNames.InfoColour, Color.FromArgb(value))
            End Set
        End Property

        ''' <summary>
        ''' Gets whether this resource represents a pool member or a full resource.
        ''' </summary>
        <DataMember(Name:="p", EmitDefaultValue:=False)>
        Public Property IsPoolMember As Boolean
            Get
                Return GetData(DataNames.IsPoolMember, False)
            End Get
            Private Set(value As Boolean)
                SetData(DataNames.IsPoolMember, value)
            End Set
        End Property

        <DataMember(Name:="c")>
        Public Property Configuration As CombinedConfig
            Get
                Return GetData(DataNames.Configuration, New CombinedConfig())
            End Get
            Set(value As CombinedConfig)
                SetData(DataNames.Configuration, value)
            End Set
        End Property

        ''' <summary>
        ''' The members of the pool that this group member represents, in a
        ''' modifiable List of members.
        ''' </summary>
        Private ReadOnly Property InnerPoolMembers As List(Of ResourceGroupMember)
            Get
                Dim mems As List(Of ResourceGroupMember) = Nothing
                mems = GetData(DataNames.PoolMembers, mems)
                If mems Is Nothing Then
                    mems = New List(Of ResourceGroupMember)
                    SetData(DataNames.PoolMembers, mems)
                End If
                Return mems
            End Get
        End Property

#End Region

#Region " Properties "

        ''' <summary>
        ''' The linking table between nodes of this type and groups. In this case,
        ''' the table is <c>BPAGroupResource</c>.
        ''' </summary>
        Public Overrides ReadOnly Property LinkTableName As String
            Get
                Return "BPAGroupResource"
            End Get
        End Property

        ''' <summary>
        ''' Gets whether the resource represented by this object is retired or not.
        ''' </summary>
        Public Overrides ReadOnly Property IsRetired As Boolean
            Get
                Return HasAttribute(ResourceAttribute.Retired)
            End Get
        End Property

        ''' <summary>
        ''' The image key to use, keyed to the imagelists:
        ''' <see cref="ImageLists.Components_16x16"/> and
        ''' <see cref="ImageLists.Components_32x32"/> in <c>BluePrism.Images</c>
        ''' </summary>
        Public Overrides ReadOnly Property ImageKey As String
            Get
                Select Case ConnectionState
                    Case ResourceConnectionState.Disconnected
                        If Status <> ResourceStatus.Private Then
                            Return ImageLists.Keys.Component.ResourceDisconnected
                        End If
                End Select


                Select Case Status
                    Case ResourceStatus.Idle
                        Return ImageLists.Keys.Component.ResourceActive
                    Case ResourceStatus.LoggedOut
                        Return ImageLists.Keys.Component.ResourceLoginAgent
                    Case ResourceStatus.Missing
                        Return ImageLists.Keys.Component.ResourceConnecting
                    Case ResourceStatus.Warning
                        Return ImageLists.Keys.Component.ResourceWarning
                    Case ResourceStatus.Offline
                        Return ImageLists.Keys.Component.ResourceInactive
                    Case ResourceStatus.Working
                        Return ImageLists.Keys.Component.ResourceBusy
                    Case ResourceStatus.Pool
                        Return ImageLists.Keys.Component.ResourcePool
                    Case ResourceStatus.Private
                        Return ImageLists.Keys.Component.ResourceInUse
                    Case Else
                        Return ImageLists.Keys.Component.ResourceInactive
                End Select
            End Get
        End Property

        ''' <summary>
        ''' The type of this group member, ie. <see cref="GroupMemberType.Resource"/>
        ''' </summary>
        Public Overrides ReadOnly Property MemberType As GroupMemberType
            Get
                Return GroupMemberType.Resource
            End Get
        End Property

        ''' <summary>
        ''' Get the appropriate display string for the current connection state.
        ''' </summary>
        Public ReadOnly Property ConnectionStateText() As String
            Get
                ' Can be null apparantly
                If ConnectionState Is Nothing Then Return ""

                ' Hide this for pool members
                If IsPoolMember Then Return ""

                ' Get the correct token for the correct state.
                Select Case ConnectionState
                    Case ResourceConnectionState.Connected
                        Return My.Resources.ResourceGroupMember_YesConnected
                    Case ResourceConnectionState.Sleep  'sleep is the same as connected for the gui
                        Return My.Resources.ResourceGroupMember_YesConnected
                    Case ResourceConnectionState.Connecting
                        Return My.Resources.ResourceGroupMember_Validating
                    Case ResourceConnectionState.InUse
                        Return My.Resources.ResourceGroupMember_NoPrivate
                    Case ResourceConnectionState.Offline, ResourceConnectionState.Unavailable, ResourceConnectionState.Hidden
                        Return My.Resources.ResourceGroupMember_No
                    Case ResourceConnectionState.Server
                        Return My.Resources.ResourceGroupMember_viaApplicationServer
                    Case ResourceConnectionState.Disconnected
                        Return My.Resources.ResourceGroupMember_Disconnected
                    Case Else
                        Return ""
                End Select
            End Get
        End Property

        Public Shared Function ConnectionStateToStateText(state As ResourceConnectionState) As String
            Select Case state
                Case ResourceConnectionState.Connected
                    Return BluePrism.AutomateAppCore.My.Resources.ResourceGroupMember_YesConnected
                Case ResourceConnectionState.Sleep  'sleep is the same as connected for the gui
                    Return My.Resources.ResourceGroupMember_YesConnected
                Case ResourceConnectionState.Connecting
                    Return My.Resources.ResourceGroupMember_Validating
                Case ResourceConnectionState.InUse
                    Return My.Resources.ResourceGroupMember_NoPrivate
                Case ResourceConnectionState.Offline, ResourceConnectionState.Unavailable
                    Return My.Resources.ResourceGroupMember_No
                Case ResourceConnectionState.Server
                    Return My.Resources.ResourceGroupMember_viaApplicationServer
                Case Else
                    Return ""
            End Select
        End Function

        ''' <summary>
        ''' The members of the pool that this group member represents, or an empty
        ''' collection if this group member does not represent a pool
        ''' </summary>
        Public ReadOnly Property PoolMembers As ICollection(Of ResourceGroupMember)
            Get
                If Not IsPool Then _
                 Return GetEmpty.ICollection(Of ResourceGroupMember)()

                Return GetReadOnly.ICollection(InnerPoolMembers)
            End Get
        End Property

        ''' <summary>
        ''' Gets whether this resource group member represents a pool or not.
        ''' </summary>
        Public Overrides ReadOnly Property IsPool As Boolean
            Get
                Return HasAttribute(ResourceAttribute.Pool)
            End Get
        End Property

        ''' <summary>
        ''' Gets the child count display value for this resource group member, if it
        ''' represents a pool, or an empty string if it does not.
        ''' </summary>
        Public ReadOnly Property ChildText As String
            Get
                If Not HasAttribute(ResourceAttribute.Pool) Then Return ""
                Return ChildResourceCount.ToString()
            End Get
        End Property

        ''' <summary>
        ''' Checks if a resource is 'active' - ie. if it would appear as a resource
        ''' which can have a session scheduled on it in Control.
        ''' </summary>
        ''' <remarks>Note that a resource can be 'active' even if it is not connected
        ''' to, not online or has exclusive sessions scheduled on it - this is purely
        ''' a test of whether the resource would show up in the Control list or not.
        ''' </remarks>
        Public ReadOnly Property IsActive As Boolean
            Get
                Return Not IsPoolMember AndAlso Not HasAnyAttribute(
                    ResourceAttribute.Retired Or ResourceAttribute.Debug)
            End Get
        End Property

        Public ReadOnly Property LogLevel As LogLevel
            Get
                If Me.Configuration.LoggingDefault <> CombinedConfig.CombinedState.Disabled Then
                    Return LogLevel.Normal
                End If
                If Me.Configuration.LoggingErrorsOnlyOverride <> CombinedConfig.CombinedState.Disabled Then
                    Return LogLevel.Errors
                End If
                If Me.Configuration.LoggingKeyOverride <> CombinedConfig.CombinedState.Disabled Then
                    Return LogLevel.Key
                End If
                If Me.Configuration.LoggingAllOverride <> CombinedConfig.CombinedState.Disabled Then
                    Return LogLevel.All
                End If

                Return LogLevel.Unknown
            End Get
        End Property

#End Region

#Region " Methods "

        ''' <summary>
        ''' Updates this group member with the data from the resource machine.
        ''' </summary>
        ''' <param name="mach">The resource machine to update this member from.
        ''' </param>
        Public Sub MapUpdatedProperties(mach As IResourceMachine)
            If mach IsNot Nothing Then
                Id = mach.Id
                Name = mach.Name
                Status = mach.DisplayStatus
                Attributes = mach.Attributes
                ChildResourceCount = mach.ChildResourceCount
                LatestConnectionMessage = mach.LastError
                Info = mach.Info
                InfoColour = mach.InfoColour.ToArgb
                IsPoolMember = mach.IsInPool
                ConnectionState = mach.ProvideConnectionState()
            End If
        End Sub

        ''' <summary>
        ''' Sets the pool members from the given collection of member resource IDs.
        ''' These IDs are expected to be available in the tree that this member
        ''' resides in.
        ''' </summary>
        ''' <param name="memberIds">The IDs to set as pool members for this resource.
        ''' </param>
        Public Sub SetPoolMembers(memberIds As ICollection(Of Guid))
            Dim members As ICollection(Of ResourceGroupMember) = InnerPoolMembers
            members.Clear()

            If Tree Is Nothing OrElse memberIds Is Nothing Then Return

            For Each resId As Guid In memberIds
                Dim rm = TryCast(Tree.RawTree.Root.FindById(resId), ResourceGroupMember)
                If rm Is Nothing Then Continue For
                members.Add(rm)
            Next
        End Sub

        ''' <summary>
        ''' Checks that the given attributes are set in this resource. If the value
        ''' contains multiple OR'ed attributes, this method will return true only if
        ''' all of the requested attributes are set in this group member.
        ''' </summary>
        ''' <param name="attr">The attribute value to check this resource group
        ''' member for.</param>
        ''' <returns>True if this resource group member has all the attributes given
        ''' in the argument.</returns>
        Public Function HasAttribute(attr As ResourceAttribute) As Boolean
            Return Attributes.HasFlag(attr)
        End Function

        ''' <summary>
        ''' Gets whether this member has any of the attributes OR'd into the given
        ''' enum value.
        ''' </summary>
        ''' <param name="attr">The attributes to check this member for</param>
        ''' <returns>True if this member has any of the attributes encoded into
        ''' <paramref name="attr"/> or if <paramref name="attr"/> is
        ''' <see cref="ProcessAttributes.None"/>; False otherwise.</returns>
        Public Function HasAnyAttribute(attr As ResourceAttribute) As Boolean
            Return Attributes.HasAnyFlag(attr)
        End Function

        ''' <summary>
        ''' Adds a resource attribute to the attributes in this member.
        ''' </summary>
        ''' <param name="attr">The attribute to set on in this group member.</param>
        Public Sub AddAttribute(attr As ResourceAttribute)
            Attributes = Attributes Or attr
        End Sub

        ''' <summary>
        ''' Removes a resource attribute from the attributes in this member.
        ''' </summary>
        ''' <param name="attr">The attribute to set off in this group member.</param>
        Public Sub RemoveAttribute(attr As ResourceAttribute)
            Attributes = Attributes And Not attr
        End Sub

        ''' <summary>
        ''' Creates an orphaned deep clone of this group member
        ''' </summary>
        ''' <returns>A clone of this group member with no owner. Note that if this
        ''' member is associated with other members (eg. it's a group), those
        ''' <em>will</em> be deep cloned in the returned member.</returns>
        ''' <remarks>The pool members (if any exist) in this group member are deep
        ''' cloned as well as any other group member data.</remarks>
        Public Overrides Function CloneOrphaned() As IGroupMember
            Dim rgm = TryCast(MyBase.CloneOrphaned(), ResourceGroupMember)
            ' Get the list of pool members from the assoc data. If there,
            ' replace it with a deep clone of the list
            Dim lst As List(Of ResourceGroupMember) = Nothing
            lst = Me.GetData(DataNames.PoolMembers, lst)
            If lst IsNot Nothing Then
                Dim newData As New List(Of ResourceGroupMember)(
                    lst.Select(Function(m) DirectCast(m.CloneOrphaned(), ResourceGroupMember)))
                rgm.SetData(DataNames.PoolMembers, newData)
            End If
            Return rgm
        End Function

        ''' <summary>
        ''' Set the current resource status to retired
        ''' </summary>
        Public Sub Retire()

            ' Check that the retire action can be performed
            If IsRetired Then
                Throw New InvalidStateException(String.Format(My.Resources.ResourceGroupMember_TheResource0IsAlreadyRetired, Name))
            ElseIf IsInGroup() Then
                Dim permissionLogic = New GroupPermissionLogic()
                If Not permissionLogic.ValidateMoveMember(Me, Owner, RootGroup, False, Function() True, User.Current) Then
                    Throw New PermissionException(
                        String.Format(My.Resources.ResourceGroupMember_YouDoNotHavePermissionToMoveTheResource0OutOfGroup1, Name, Owner.Name))
                End If
            End If

            ' Retire the resource
            gSv.RetireResource(IdAsGuid)

            ' Update the member
            AddAttribute(ResourceAttribute.Retired)
            If IsInGroup() Then Remove()
        End Sub

        ''' <summary>
        ''' Set the current resource status to unretired
        ''' </summary>
        Public Sub Unretire(moveToGroup As IGroup)

            ' Check that the unretire action can be performed
            If Not IsRetired Then
                Throw New InvalidStateException(String.Format(My.Resources.ResourceGroupMember_TheResource0IsNotRetired, Name))
            End If
            Dim permissionLogic = New GroupPermissionLogic()
            If Not permissionLogic.ValidateMoveMember(Me, Owner, moveToGroup, False, Function() True, User.Current) Then
                Throw New PermissionException(
                    String.Format(My.Resources.ResourceGroupMember_YouDoNotHavePermissionToMoveTheResource0IntoGroup1, Name, moveToGroup.Name))
            End If

            ' Unretire the resource
            gSv.UnretireResource(IdAsGuid)

            ' Update the member
            RemoveAttribute(ResourceAttribute.Retired)
            MoveTo(moveToGroup)
        End Sub

        ''' <summary>
        ''' Override to the has view permission for resources that checks for implied
        ''' view permission rather than any permission.
        ''' </summary>
        ''' <param name="user">the user in question</param>
        ''' <returns>true if the user has permission</returns>
        Public Overrides Function HasViewPermission(user As IUser) As Boolean
            Return Permissions.HasPermission(user, Permission.Resources.ImpliedViewResource)
        End Function

#End Region

    End Class

End Namespace
