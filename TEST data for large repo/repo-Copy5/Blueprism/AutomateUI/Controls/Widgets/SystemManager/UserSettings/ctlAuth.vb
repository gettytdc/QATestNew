Imports System.DirectoryServices.AccountManagement
Imports System.Security.Principal
Imports System.Windows.Forms.VisualStyles
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports AutomateControls
Imports BluePrism.AutomateAppCore.clsServerPartialClasses
Imports BluePrism.AutomateAppCore.Utility

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Config
Imports LocaleTools
Imports BluePrism.Server.Domain.Models

''' <summary>
''' Control to handle configuration of the authorisation settings in the environment;
''' roles and permissions for the user, and the configuration of roles in general
''' </summary>
Friend Class ctlAuth

    <Flags>
    Private Enum ColumnType
        None = 0
        SingleSignOn = 1
        NativeOrExternal = 2
    End Enum

    Private Class SearcherPayload
        Public Property Domain As String
        Public Property Name As String
        Public Property Role As Role
        Public Property Sid As SecurityIdentifier
        Public Property Group As GroupPrincipal
        Public Property Users As IEnumerable(Of UserPrincipal)
        Public Property BPUsers As IEnumerable(Of User)
        Public Property Server As String
        Public Property ConfigException As Exception
    End Class

    ''' <summary>
    ''' Enumeration of the checkbox images corresponding to permission group checked states
    ''' </summary>
    Private Enum CheckboxImages As Integer
        Unchecked = 0
        Checked = 1
        Indeterminate = 2
    End Enum

#Region " Member Variables "

    ' The system roles in the current environment
    Private WithEvents mSystemRoles As SystemRoleSet

    ' The user whose roles we are modifying
    Private mUser As User

    ' The current edit mode of this control
    Private mEditMode As AuthEditMode

    ' The roleset being modified/displayed by this control
    Private mRoles As RoleSet

    ' The collection of user groups retrieved from AD in the last search
    Private mGroups As ICollection(Of String)

    ' The error which occurred within the last AD search; null if no error occurred
    Private mDirSearchError As Exception

    ' The searcher used to perform AD searches within this control
    Private mSearcher As ADGroupSearcher

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new auth control in an unspecified mode.
    ''' </summary>
    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Before the trees are populated, disable the checkboxes on the permissions 
        ' treeview and set up the image list to allow for 3 check states for partial 
        ' permissions on groups
        tvPerms.CheckBoxes = False
        ' The order of these images must correspond to the values in the images enum
        AddCheckboxToImageList(CheckBoxState.UncheckedNormal, tvPerms.StateImageList)
        AddCheckboxToImageList(CheckBoxState.CheckedNormal, tvPerms.StateImageList)
        AddCheckboxToImageList(CheckBoxState.MixedNormal, tvPerms.StateImageList)

        ' Indicate which columns in the user grid should be visible for each of the
        ' auth modes. A column can be made available to both by 'OR'ing the ColumnType
        ' together into the tag - currently colFullName is the only 'both' column.
        For Each col In New DataGridViewColumn() {
            colFullName, colUserPrincipalName, colDistinguishedName
        }
            col.Tag = BPUtil.IfNull(col.Tag, ColumnType.None) Or ColumnType.SingleSignOn
        Next

        For Each col In New DataGridViewColumn() {
            colFullName, colLastLoggedIn, colValidFrom, colValidTo, colPasswordExpiry
        }
            col.Tag = BPUtil.IfNull(col.Tag, ColumnType.None) Or ColumnType.NativeOrExternal
        Next

        ' Add any initialization after the InitializeComponent() call.
        If Not UIUtil.IsInVisualStudio Then
            mSystemRoles = SystemRoleSet.SystemCurrent
            PopulateTreeViews()
            Try
                ActiveDirectoryAvailable = User.IsLoggedInto(DatabaseType.SingleSignOn)
            Catch ex As Exception
                UserMessage.Err(ex, My.Resources.ctlAuth_DatabaseErrorCouldNotDetermineActiveDirectoryStatusErrorMessageWas0, ex.Message)
                FindForm().Close()
            End Try
            llManageRoles.Enabled = User.Current.HasPermission("Security - User Roles")
        End If

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets or sets the edit mode that this user control is in. It may be used to
    ''' either manage system roles or to update the roles/permissions assigned to a
    ''' user. This method sets the mode required.
    ''' </summary>
    <Browsable(True), DefaultValue(AuthEditMode.Unset),
     Description("The edit mode for the Auth control")>
    Public Property EditMode() As AuthEditMode
        Get
            Return mEditMode
        End Get
        Set(ByVal value As AuthEditMode)
            If mEditMode = value Then Return
            mEditMode = value

            ' --- Manage Roles mode only ---
            panRoleButtons.Visible = (value = AuthEditMode.ManageRoles)
            tvRoles.LabelEdit = (value = AuthEditMode.ManageRoles)
            mnuDeleteRole.Enabled = (value = AuthEditMode.ManageRoles)
            mnuRenameRole.Enabled = (value = AuthEditMode.ManageRoles)
            gridMembers.Enabled = (value = AuthEditMode.ManageRoles)
            splitMain.Panel2Collapsed = (value <> AuthEditMode.ManageRoles)


            ' --- Manage User mode only ---
            tvRoles.CheckBoxes = (value = AuthEditMode.ManageUser)
            llManageRoles.Visible = (value = AuthEditMode.ManageUser)

            ' disallow editing the permissions tree if in user mode
            If value = AuthEditMode.ManageUser Then _
                RemoveHandler tvPerms.NodeMouseClick, AddressOf tvPerms_NodeMouseClick

            If value = AuthEditMode.ManageUser AndAlso mUser IsNot Nothing Then _
             SetRolesChecked(mUser.Roles, True)
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets whether the AD controls in this panel are available or not
    ''' </summary>
    <Browsable(True), DefaultValue(True)>
    Private Property ActiveDirectoryAvailable() As Boolean
        Get
            Return panAdGroup.Visible
        End Get
        Set(ByVal value As Boolean)
            panAdGroup.Visible = value
            ' Only show the columns which are set to be visible for the specified
            ' auth mode (set as an ColumnType value in the tag by the constructor)
            For Each col As DataGridViewColumn In gridMembers.Columns
                Dim type = BPUtil.IfNull(col.Tag, ColumnType.NativeOrExternal Or ColumnType.SingleSignOn)
                col.Visible = (value AndAlso type.HasFlag(ColumnType.SingleSignOn) OrElse
                    Not value AndAlso type.HasFlag(ColumnType.NativeOrExternal)
                )
            Next
        End Set
    End Property

    ''' <summary>
    ''' The user whose roles are being modified by this form. Null if there is no
    ''' specific user being modified (ie. the mode is something other than
    ''' <see cref="AuthEditMode.ManageUser"/>
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property User() As User
        Get
            Return mUser
        End Get
        Set(ByVal value As User)
            mUser = value
            If mUser IsNot Nothing Then SetRolesChecked(mUser.Roles, True)
        End Set
    End Property

    ''' <summary>
    ''' The user roles being used by this control. If not set elsewhere, this will
    ''' use a clone of the <see cref="SystemRoleSet.Current">current roleset</see>
    ''' loaded in this environment.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend ReadOnly Property Roles() As RoleSet
        Get
            If mRoles Is Nothing Then mRoles = mSystemRoles.ModifiableCopy()
            Return mRoles
        End Get
    End Property

    ''' <summary>
    ''' Gets the roles which are checked in this control
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property CheckedRoles() As RoleSet
        Get
            Dim rs As New RoleSet()
            For Each n As TreeNode In tvRoles.Nodes
                If n.Checked Then rs.Add(DirectCast(n.Tag, Role))
            Next
            Return rs
        End Get
    End Property


    ''' <summary>
    ''' Gets whether this control is set to manage roles or not.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property IsManagingRoles() As Boolean
        Get
            Return (EditMode = AuthEditMode.ManageRoles)
        End Get
    End Property

    ''' <summary>
    ''' Gets whether this control is set to manage users or not.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property IsManagingUser() As Boolean
        Get
            Return (EditMode = AuthEditMode.ManageUser)
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the selected role in this control
    ''' </summary>
    Private Property SelectedRole() As Role
        Get
            Dim n As TreeNode = tvRoles.SelectedNode
            If n Is Nothing Then Return Nothing Else Return TryCast(n.Tag, Role)
        End Get
        Set(ByVal value As Role)
            If value Is Nothing Then tvRoles.SelectedNode = Nothing : Return
            Dim roleId As Integer = value.Id
            For Each n As TreeNode In tvRoles.Nodes
                Dim r As Role = TryCast(n.Tag, Role)
                If r Is Nothing Then Continue For
                If value.Id = r.Id OrElse value.Equals(r) _
                 Then tvRoles.SelectedNode = n : Return
            Next
        End Set
    End Property

    ''' <summary>
    ''' A non-null collection of strings representing the groupnames currently held
    ''' in this control. This collection is populated by a background worker which
    ''' polls the Active Directory instance for available groups.
    ''' </summary>
    Private ReadOnly Property Groups() As ICollection(Of String)
        Get
            If mGroups Is Nothing Then Return GetEmpty.ICollection(Of String)()
            Return mGroups
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets whether the user is currently waiting to show a directory search
    ''' or not. Setting this will set the <see cref="Enabled"/> state of this control
    ''' as well as setting the <see cref="Cursor"/> appropriately.
    ''' It will also set the <see cref="timerDirSearchTimeout"/> to be enabled or
    ''' disabled appropriately, meaning that the timeout will be counting down
    ''' immediately after setting this property to true
    ''' </summary>
    Private Property WaitingToShowDirSearch() As Boolean
        Get
            ' We check all things, because there's a moment in the Tick handler when
            ' the timer is disabled
            Return (timerDirSearchTimeout.Enabled _
             OrElse (Not Enabled AndAlso Cursor = Cursors.WaitCursor))
        End Get
        Set(ByVal value As Boolean)
            timerDirSearchTimeout.Enabled = value
            Enabled = Not value
            If value Then Cursor = Cursors.WaitCursor Else Cursor = Cursors.Default
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the currently checked permissions in this control. Note that
    ''' any modifications made to the returned collection are <em>not</em> reflected
    ''' in this control
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property CheckedPermissions() As ICollection(Of Permission)
        Get
            Dim perms As New clsSet(Of Permission)
            For Each groupNode As TreeNode In tvPerms.Nodes
                For Each permNode As TreeNode In groupNode.Nodes
                    If permNode.Checked Then _
                        perms.Add(DirectCast(permNode.Tag, Permission))
                Next
            Next
            Return perms
        End Get
        Set(ByVal value As ICollection(Of Permission))
            SetCheckedRecursive(tvPerms.Nodes, False)

            If CollectionUtil.IsNullOrEmpty(value) Then Return

            For Each groupNode As TreeNode In tvPerms.Nodes
                Dim allChecked As Boolean = True
                Dim anyChecked As Boolean = False
                Dim pg As PermissionGroup = DirectCast(groupNode.Tag, PermissionGroup)
                For Each permNode As TreeNode In groupNode.Nodes
                    Dim p As Permission = DirectCast(permNode.Tag, Permission)
                    Dim contained As Boolean = value.Contains(p)
                    permNode.Checked = contained
                    With permNode
                        If .Checked Then
                            .StateImageIndex = CheckboxImages.Checked
                        Else
                            .StateImageIndex = CheckboxImages.Unchecked
                        End If
                    End With
                    allChecked = allChecked AndAlso contained
                    anyChecked = anyChecked OrElse contained
                Next
                With groupNode
                    If allChecked Then
                        .StateImageIndex = CheckboxImages.Checked
                        .Checked = allChecked
                    ElseIf anyChecked Then
                        .StateImageIndex = CheckboxImages.Indeterminate
                    Else
                        .StateImageIndex = CheckboxImages.Unchecked
                    End If
                End With
            Next

        End Set
    End Property

    ''' <summary>
    ''' Gets the searcher in this control, or null if this control is disposed.
    ''' </summary>
    ''' <value></value>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Private ReadOnly Property Searcher As ADGroupSearcher
        Get
            If mSearcher Is Nothing Then mSearcher = New ADGroupSearcher()
            Return mSearcher
        End Get
    End Property

#End Region

#Region " Event Handlers "

    ''' <summary>
    ''' Handles a request from the user to display the AD groups and select one from
    ''' the list available.
    ''' </summary>
    Private Sub HandleGroupLookup(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnGroupLookup.Click
        Dim r As Role = SelectedRole

        Using f As New ADGroupSelectorForm(
         gSv.GetActiveDirectoryDomain(),
         CType(txtGroupPath.Tag, SecurityIdentifier),
         True,
         If(r Is Nothing, "", r.Name)
        )
            f.SetEnvironmentColoursFromAncestor(ParentForm)
            f.ShowInTaskbar = False
            If f.ShowDialog() <> DialogResult.OK Then Return

            If f.SelectedGroup IsNot Nothing Then
                DoSearch(New SearcherPayload() With {
                    .Role = r,
                    .Name = f.SelectedGroupName,
                    .Sid = f.SelectedGroup
                })
            End If

        End Using
    End Sub

    ''' <summary>
    ''' Handles the context menu opening, ensuring that it is not shown if this
    ''' control is not in 'managing roles' mode.
    ''' </summary>
    Private Sub HandleContextMenuOpening(sender As Object, e As CancelEventArgs) _
     Handles ctxRoles.Opening
        If Not IsManagingRoles Then e.Cancel = True
    End Sub

    ''' <summary>
    ''' Handles the system roles being updated.
    ''' </summary>
    Private Sub HandleSystemRolesUpdated(
     ByVal sender As Object, ByVal e As EventArgs) Handles mSystemRoles.Updated
        ' Make sure we're not trying to mess with the treeview if we're disposed
        If IsDisposed Then Return

        ' We need to go through the roles we have displayed and update them
        ' accordingly
        mRoles = mSystemRoles.ModifiableCopy()

        ' The roles can be updated on a non-UI thread so make sure that we are on
        ' the correct thread to update the control
        If InvokeRequired _
         Then Invoke(Sub() RepopulateRoles()) _
         Else RepopulateRoles()
    End Sub


    ''' <summary>
    ''' Handles the clicking of a node on the permissions tree view. As we're using a 
    ''' StateImageIndex and indeterminate states for the group boxes we must simulate 
    ''' the checking of the checkboxes.
    ''' </summary>
    Private Sub tvPerms_NodeMouseClick(ByVal sender As Object, ByVal e As MouseEventArgs) _
            Handles tvPerms.NodeMouseClick
        Const offsetPerm As Integer = 34
        Const offsetGroup As Integer = 16
        Const checkBoxWidth As Integer = 16

        If e.X <= offsetGroup OrElse e.X > (offsetPerm + checkBoxWidth) Then Return

        Dim it As TreeNode = tvPerms.HitTest(e.Location).Node

        ' If the selected role is readonly, then ensure that we don't allow its
        ' modification.
        EditPermissions(Sub() CheckPermissionTreeNode(it))

    End Sub

    ''' <summary>
    ''' Handles the key up event on the permissions tree view to allow toggling of 
    ''' permissions using the space bar.
    ''' </summary>
    Private Sub HandleKeyUp(sender As Object, e As KeyEventArgs) Handles tvPerms.KeyUp
        If e.KeyCode = Keys.Space Then
            EditPermissions(Sub() CheckPermissionTreeNode(tvPerms.SelectedNode))
        End If
    End Sub

    ''' <summary>
    ''' Checks if the user has permission to edit the permissions using the 
    ''' action, else a warning message is displayed. 
    ''' </summary>
    ''' <param name="action">Action to perform if the user has the permission 
    ''' to do so.</param>
    Private Sub EditPermissions(action As Action)
        Dim role = SelectedRole
        If role IsNot Nothing AndAlso Not role.CanChangePermissions Then
            UserMessage.Err(My.Resources.ctlAuth_The0RoleMayNotBeModified, role.Name)
        Else
            action()
        End If
    End Sub

    ''' <summary>
    ''' Handles the evnt fired before a role is checked
    ''' </summary>
    Private Sub HandleRolesBeforeCheck(
     ByVal sender As Object, ByVal e As TreeViewCancelEventArgs) _
     Handles tvRoles.BeforeCheck
        If EditMode = AuthEditMode.ManageUser AndAlso e.Node.Checked Then
            Dim r As Role = TryCast(e.Node.Tag, Role)
            ' If there are not any users in SystemAdmin role other than this user, show a message to user and cancel the 'uncheck' operation.
            If r IsNot Nothing AndAlso r.SystemAdmin AndAlso Not gSv.GetActiveUsersInRole(r.Id).Any(Function(x) x.Id <> User.Id) Then
                UserMessage.Err(My.Resources.ctlAuth_0IsTheLastUserAssignedThe1RoleSoThereforeItCannotBeRemoved, User.Name, r.Name)
                e.Cancel = True
            End If

        End If
    End Sub

    ''' <summary>
    ''' Handles a checkbox being checked on the roles treeview
    ''' </summary>
    Private Sub HandleRolesAfterCheck(
     ByVal sender As Object, ByVal e As TreeViewEventArgs) Handles tvRoles.AfterCheck
        If EditMode <> AuthEditMode.ManageUser Then Return
        CheckedPermissions = CheckedRoles.EffectivePermissions
        Dim r As Role = TryCast(e.Node.Tag, Role)
        If e.Node.Checked Then mUser.Roles.Add(r) Else mUser.Roles.Remove(r)
    End Sub

    ''' <summary>
    ''' Handles the event fired just before a permission is checked
    ''' </summary>
    Private Sub HandlePermsBeforeCheck(
     ByVal sender As Object, ByVal e As TreeViewCancelEventArgs) _
     Handles tvPerms.BeforeCheck

        ' Ignore programmatic checking of the permissions treeview
        If e.Action = TreeViewAction.Unknown Then Return

        ' Can't change permissions directly on the user
        If IsManagingUser Then e.Cancel = True : Return

        ' If the selected role is readonly, then ensure that we don't allow its
        ' modification.
        EditPermissions(Sub() e.Cancel = True)
    End Sub

    ''' <summary>
    ''' Handles the 'New Role' button being clicked
    ''' </summary>
    Private Sub HandleNewRole(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnNewRole.Click

        Dim r As Role = Roles.NewRole()
        Dim n As TreeNode = tvRoles.Nodes.Add(r.Name)
        n.Tag = r

        'get the node name ready to edit:
        tvRoles.SelectedNode = n
        n.EnsureVisible()
        tvRoles.LabelEdit = True
        n.BeginEdit()

        'clear the actions tree for the new role:
        SetCheckedRecursive(tvPerms.Nodes, False)

    End Sub


    ''' <summary>
    ''' Handles the 'Clone Role' button being clicked
    ''' </summary>
    Private Sub HandleCloneRole(sender As Object, e As EventArgs) _
        Handles mmuCloneRole.Click

        Dim selectedName As String = LTools.GetC(SelectedRole.Name, "roleperms", "role")

        If Not (User.Current.HasPermission(Permission.SystemManager.Security.UserRoles)) Then
            UserMessage.Err(My.Resources.ctlAuth_YouArenTAuthorisedToCloneThe0Role, selectedName)
            Return
        End If

        ' As we're cloning a role, we need to make sure the cloned role name is unique
        Dim name As String = Nothing, num As Integer = 0
        Do
            num += 1
            name = String.Format(My.Resources.ctlAuth_01, selectedName, num)
        Loop While Roles.Contains(name)


        ' Create a new role with the unique name
        Dim roleCopy As New Role(name)

        ' Add the permissions of the role to be cloned
        roleCopy.AddAll(SelectedRole.Permissions)
        roleCopy.CopiedFromRoleID = SelectedRole.Id

        ' Add the role to role set
        Roles.Add(roleCopy)

        ' Update TreeNode and save new role.
        Dim n As TreeNode = tvRoles.Nodes.Add(roleCopy.Name)
        n.Tag = roleCopy

        'get the node name ready to edit:
        tvRoles.SelectedNode = n
        n.EnsureVisible()
        tvRoles.LabelEdit = True
        n.BeginEdit()

        'clear the actions tree for the new role:
        SetCheckedRecursive(tvPerms.Nodes, False)

        ' Update the role display to show permissions
        UpdateRoleDisplay(roleCopy)

    End Sub

    ''' <summary>
    ''' Handles the 'Delete Role' button being clicked
    ''' </summary>
    Private Sub HandleDeleteRole(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnDeleteRole.Click, mnuDeleteRole.Click
        ' Get the selected role
        Dim r As Role = SelectedRole

        ' If there is none selected, error and cancel
        If r Is Nothing Then _
         UserMessage.Err(My.Resources.ctlAuth_PleaseFirstSelectARoleToDelete) : Return

        ' If it's readonly, error and cancel
        If Not r.CanDelete Then _
         UserMessage.Err(My.Resources.ctlAuth_YouCannotDeleteThe0Role, r.Name) : Return

        ' Count the number of users with this role.
        ' Offer a warning to the user to back out without deleting the role,
        ' regardless of how many users there are who are assigned to it
        Dim cnt As Integer = gSv.CountActiveUsersWithRole(r)

        Dim msg As String = My.Resources.ctlAuth_ThisRoleIsNotCurrentlyAssignedToAnyActiveUsers

        If cnt > 0 Then msg = LTools.Format(My.Resources.ctlAuth_plural_role_currently_assigned_to, "COUNT", cnt)

        Dim res As MsgBoxResult = UserMessage.OkCancel(
         My.Resources.ctlAuth_0AreYouSureYouWantToDeleteIt, msg)

        ' If they cancelled... well, cancel the delete
        If res = MsgBoxResult.Cancel Then Return

        Roles.Remove(r.Name)

        tvRoles.Nodes.Remove(tvRoles.SelectedNode)

        tvRoles.Focus()
        ' This will never be null because we cannot delete the System Administrator
        ' node, thus a node will always be selected.
        tvRoles.SelectedNode.EnsureVisible()

    End Sub

    ''' <summary>
    ''' Handles the event fired before a role's label is edited
    ''' </summary>
    Private Sub HandleBeforeRoleLabelEdit(ByVal sender As Object,
     ByVal e As NodeLabelEditEventArgs) Handles tvRoles.BeforeLabelEdit
        Dim r As Role = TryCast(e.Node.Tag, Role)
        If Not r.CanRename Then
            e.CancelEdit = True
            UserMessage.Err(My.Resources.ctlAuth_YouCannotRenameThe0Role, r.Name)
        End If
    End Sub

    ''' <summary>
    ''' Handles the role label having been edited
    ''' </summary>
    Private Sub HandleAfterRoleLabelEdit(ByVal sender As Object,
     ByVal e As NodeLabelEditEventArgs) Handles tvRoles.AfterLabelEdit

        Debug.Print("Label: {0}; Text: {1}",
         If(e.Label, My.Resources.ctlAuth_Null), e.Node.Text)

        ' If the user has cancelled the edit, just ignore it
        If e.Label Is Nothing Then Return

        Try
            Dim r As Role = DirectCast(e.Node.Tag, Role)
            Roles.Rename(r.Name, e.Label)
            e.Node.EndEdit(False)

        Catch ex As Exception
            e.CancelEdit = True
            UserMessage.Err(ex, My.Resources.ctlAuth_Error0, ex.Message)
            e.Node.BeginEdit()

        End Try
    End Sub

    ''' <summary>
    ''' Handles a role being selected, ensuring that the permissions are set
    ''' accordingly
    ''' </summary>
    Private Sub HandleRolesAfterSelect(
     ByVal sender As Object, ByVal e As TreeViewEventArgs) _
     Handles tvRoles.AfterSelect
        ' Selecting a role only changes anything for editrole mode
        If IsManagingRoles Then UpdateRoleDisplay()
    End Sub

    ''' <summary>
    ''' Handles the mouseup event in the roles treeview
    ''' </summary>
    Private Sub HandleRolesMouseUp(
     ByVal sender As Object, ByVal e As MouseEventArgs) Handles tvRoles.MouseUp
        If e.Button <> MouseButtons.Right Then Return
        Dim n As TreeNode = tvRoles.GetNodeAt(e.X, e.Y)
        mnuDeleteRole.Enabled = (n IsNot Nothing)
        mnuRenameRole.Enabled = (n IsNot Nothing)
        If n IsNot Nothing Then tvRoles.SelectedNode = n
    End Sub

    ''' <summary>
    ''' Handles the 'Rename' button being clicked
    ''' </summary>
    Private Sub HandleRenameRole(ByVal sender As Object, ByVal e As EventArgs) _
     Handles mnuRenameRole.Click
        If tvRoles.SelectedNode Is Nothing Then Exit Sub
        tvRoles.SelectedNode.BeginEdit()
    End Sub

    ''' <summary>
    ''' Handles the 'Manage Roles' button being clicked.
    ''' </summary>
    Private Sub HandleManageRolesClick(
     ByVal sender As Object, ByVal e As LinkLabelLinkClickedEventArgs) _
     Handles llManageRoles.LinkClicked
        If e.Button <> MouseButtons.Left Then Return
        Dim f As IChild = TryCast(FindForm(), IChild)
        If f Is Nothing Then Return
        Dim appForm As frmApplication = f.ParentAppForm
        If appForm Is Nothing Then Return
        appForm.StartForm(New frmManageRoles(), True)
        ' We don't need to worry about updating the roles directly - the form updates
        ' the system roleset, and we are already listening for changes to the system
        ' roleset, so we will handle it in there, if the user changes anything.
    End Sub

    ''' <summary>
    ''' Handles the work being done in Active Directory.
    ''' This performs the search to get the group principal corresponding to the SID
    ''' or name in the <see cref="SearcherPayload"/> given as the argument.
    ''' </summary>
    Private Sub HandleSearcherDoWork(sender As Object, e As DoWorkEventArgs) _
     Handles workSearcher.DoWork
        Dim payload = DirectCast(e.Argument, SearcherPayload)
        If payload Is Nothing Then Throw New ArgumentNullException(NameOf(payload))

        ' If AD, we need to go through the groups registered in AD for our data
        If ActiveDirectoryAvailable Then

            payload.Domain = gSv.GetActiveDirectoryDomain()

            ' Nothing to do if we have no SID or Name to look up
            If payload.Sid Is Nothing AndAlso payload.Name = "" _
             Then e.Result = payload : Return

            workSearcher.ReportProgress(5, My.Resources.ctlAuth_GettingTheSIDOfTheGroup)
            If workSearcher.CancellationPending Then
                e.Cancel = True
                Return
            End If

            ' We need a SID to look up the group. If we have one, great; if not then
            ' the 'payload.Name' might be a group name (legacy data) or a string-encoded
            ' SID. If it's the former, we look it up in the registered domain to get the
            ' SID which represents the group.
            If payload.Name <> "" AndAlso payload.Sid Is Nothing Then
                Try
                    payload.Sid = New SecurityIdentifier(payload.Name)
                Catch ex As Exception
                    Dim gp As GroupPrincipal = Searcher.GetADGroupByName(
                     payload.Domain, Nothing, payload.Name)
                    If gp IsNot Nothing AndAlso gp.Sid.IsAccountSid() Then
                        payload.Sid = gp.Sid
                        payload.Group = gp
                    Else
                        payload.Sid = Nothing
                    End If
                End Try
            End If
            If workSearcher.CancellationPending Then
                e.Cancel = True
                Return
            End If

            ' We may have the group from the name above - if we don't, but we have a SID,
            ' use that to retrieve the group
            If payload.Group Is Nothing AndAlso payload.Sid IsNot Nothing Then
                workSearcher.ReportProgress(50, My.Resources.ctlAuth_SearchingForTheGroup)

                ' Now do the actual search based on the SID that we have
                payload.Group = Searcher.GetADGroupBySid(
                    payload.Domain, Nothing, payload.Sid)
            End If

            workSearcher.ReportProgress(75, My.Resources.ctlAuth_RetrievingTheUsersInTheGroup)

            If payload.Group IsNot Nothing Then
                Dim users As New List(Of UserPrincipal)
                Try
                    For Each u In clsActiveDirectory.GetValidGroupMembers(payload.Group)
                        users.Add(u)
                    Next
                Catch ex As ActiveDirectoryConfigException
                    payload.ConfigException = ex
                End Try

                payload.Users = users
                payload.Server = Searcher.ConnectedServer
            End If
        Else ' ie. If Not ActiveDirectoryAvailable
            ' If not AD, we get our user information from the database
            payload.BPUsers = gSv.GetActiveUsersInRole(payload.Role.Id)

        End If

        workSearcher.ReportProgress(100, My.Resources.ctlAuth_PopulatingTheMembershipList)

        e.Result = payload
    End Sub

    ''' <summary>
    ''' Handles the progress changing in the active directory search.
    ''' </summary>
    Private Sub HandleSearcherProgressChanged(
     sender As Object, e As ProgressChangedEventArgs) _
     Handles workSearcher.ProgressChanged
        ' Stub - not really used yet.
    End Sub

    ''' <summary>
    ''' Handles the Active Directory search completing, ensuring that the UI is
    ''' updated accordingly.
    ''' </summary>
    Private Sub HandleSearcherCompleted(
     sender As Object, e As RunWorkerCompletedEventArgs) _
     Handles workSearcher.RunWorkerCompleted
        Dim ex As Exception = e.Error
        If ex IsNot Nothing Then
            UserMessage.Err(ex,
             My.Resources.ctlAuth_AnErrorOccurredWhileSearchingForTheUsersAssignedToThisRole)
            Return
        End If
        If e.Cancelled Then Return

        Dim payload = DirectCast(e.Result, SearcherPayload)

        ' Check for BP Users first; otherwise check for AD data and use that
        If payload.BPUsers IsNot Nothing Then
            SetUserList(payload.BPUsers)

        ElseIf payload.Sid Is Nothing Then
            txtGroupPath.Tag = Nothing
            txtGroupName.Text = ""
            txtGroupPath.Text = ""

        ElseIf payload.Group Is Nothing Then
            UserMessage.Err(
             My.Resources.ctlAuth_TheGroup0CannotBeFoundItMayHaveBeenRemovedFromTheConfiguredDomain12PleaseContac,
             payload.Name, payload.Domain, vbCrLf)

            ' Leave the group as whatever it was assigned as previously in this case

        Else
            txtGroupPath.Tag = payload.Sid
            txtGroupName.Text = payload.Group.Name
            txtGroupPath.Text = payload.Group.DistinguishedName

            If payload.Role IsNot Nothing Then _
             payload.Role.ActiveDirectoryGroup = payload.Sid.Value

            If payload.Users IsNot Nothing Then SetUserList(payload.Users)

            If payload.Server = "" _
             Then lblFromServer.Text = "" _
             Else lblFromServer.Text = My.Resources.ctlAuth_QueryPerformedOn & payload.Server

        End If

        ToolTip.SetToolTip(txtGroupPath, txtGroupPath.Text)

        If payload.ConfigException IsNot Nothing Then
            UserMessage.Show(payload.ConfigException.Message)
        End If
    End Sub

#End Region

#Region " Other Methods "

    ''' <summary>
    ''' Instructs the background worker to perform its search based on the given
    ''' payload.
    ''' </summary>
    ''' <param name="payload">The payload describing the parameters for the search.
    ''' </param>
    Private Sub DoSearch(payload As SearcherPayload)
        gridMembers.Rows.Clear()
        If Not workSearcher.IsBusy Then workSearcher.RunWorkerAsync(payload)
    End Sub

    ''' <summary>
    ''' Disposes of this control.
    ''' </summary>
    ''' <param name="disposing">True to indicate that this disposal is being done
    ''' explicitly; False to indicate that it is being performed implicitly by the
    ''' garbage collector via the finalizer method.</param>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing Then
                If components IsNot Nothing Then components.Dispose()
                ' We want to ensure that we are no longer listening to system role
                ' changes, now that this is disposed.
                mSystemRoles = Nothing
                Dim s As ADGroupSearcher = mSearcher
                If s IsNot Nothing Then s.Dispose() : mSearcher = Nothing
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    ''' <summary>
    ''' Validates the current user roles configuration
    ''' </summary>
    ''' <exception cref="BluePrism.Server.Domain.Models.ActiveDirectoryConfigException">If any of the groups
    ''' configured in the roles within this control are not valid for some reason.
    ''' </exception>
    Public Sub ValidateRoles()
        Roles.ValidateADGroups()
    End Sub

    ''' <summary>
    ''' Adds a tree node with the given attributes to the specified collection.
    ''' </summary>
    ''' <param name="nodes">The collection to which the node should be added.</param>
    ''' <param name="label">The label of the tree node.</param>
    ''' <param name="tag">The tag to apply to the node.</param>
    ''' <returns>The treenode created from the given parameters.</returns>
    Private Function AddNode(ByVal nodes As TreeNodeCollection, _
     ByVal label As String, ByVal tag As Object) As TreeNode
        Dim n As New TreeNode(label)
        n.Tag = tag
        nodes.Add(n)
        Return n
    End Function

    ''' <summary>
    ''' Populates the treeviews with the roles/permissions available on the system.
    ''' </summary>
    Private Sub PopulateTreeViews()
        ' Make sure we're not trying to update a disposed UI
        If IsDisposed Then Return

        ' We've been here before...
        If tvRoles.Nodes.Count > 0 Then Return

        tvPerms.Sorted = True
        tvRoles.Sorted = True

        ' Populate the Roles Tree - nice and simple
        tvRoles.BeginUpdate()
        tvRoles.Nodes.Clear()

        For Each r As Role In Roles
            AddNode(tvRoles.Nodes, LTools.GetC(r.Name, "roleperms", "role"), r)
        Next
        tvRoles.EndUpdate()

        ' And now the perms tree; perms within groups
        tvPerms.BeginUpdate()
        tvPerms.Nodes.Clear()

        For Each pg As PermissionGroup In PermissionGroup.All
            Dim groupNode As TreeNode = AddNode(tvPerms.Nodes, LTools.GetC(pg.Name, "roleperms", "group"), pg)
            For Each p As Permission In pg.Permissions
                AddNode(groupNode.Nodes, LTools.GetC(p.Name, "roleperms", "perm"), p)
            Next
        Next
        tvPerms.EndUpdate()

        tvRoles.SelectedNode = tvRoles.Nodes(0)

    End Sub

    ''' <summary>
    ''' Repopulates the roles treeview using the current roleset set in this control
    ''' </summary>
    Private Sub RepopulateRoles()
        ' Make sure we're not trying to update a disposed UI
        If IsDisposed Then Return

        ' A list of nodes that will need to be deleted
        Dim nodesToDelete As New List(Of TreeNode)

        ' A copy of the roles we need to add. These are removed as we go through
        ' the treeview, meaning we're left with the ones that we need to add
        Dim newRoles As New RoleSet()
        For Each r As Role In Roles
            newRoles.Add(r)
        Next

        ' Go through the nodes and process them as much as we can
        For Each n As TreeNode In tvRoles.Nodes
            Dim currRole As Role = DirectCast(n.Tag, Role)
            Dim newRole As Role = newRoles(currRole.Name)
            If newRole Is Nothing Then
                ' No new role - ie. it's been deleted
                nodesToDelete.Add(n)
            Else
                ' It's still there - save the new role object into the node
                n.Tag = newRole
                ' Remove the processed roles from our local collection, so that
                ' we can identify the roles which need to be added.
                newRoles.Remove(newRole)
                ' If this node is selected and we're currently managing roles,
                ' ensure that the checked permissions are updated too
                If n.IsSelected AndAlso IsManagingRoles Then _
                 CheckedPermissions = newRole.Permissions
            End If
        Next

        ' So now, 'nodesToDelete' contains the nodes that we need to get rid of,
        ' and 'newRoles' contains the roles for which we have no node.
        ' Any changed roles have been updated with their new information

        ' First, we delete the obsolete nodes
        For Each n As TreeNode In nodesToDelete : tvRoles.Nodes.Remove(n) : Next

        ' Then, we add our new ones
        For Each r As Role In newRoles : AddNode(tvRoles.Nodes, r.Name, r) : Next

        ' If in editrole mode, we need to have a role selected; if we already have
        ' one selected, ensure that its data is up to date
        If IsManagingRoles Then
            If tvRoles.SelectedNode Is Nothing Then
                tvRoles.SelectedNode = tvRoles.Nodes(0)
            Else : UpdateRoleDisplay()
            End If
        Else
            ' In edit user mode,so ensure the data's correct for any roles that user has
            For Each r As Role In CheckedRoles
                UpdateRoleDisplay(r)
            Next
        End If

    End Sub

    ''' <summary>
    ''' Recursively ticks or unticks all nodes and subnodes in the specified node
    ''' collection.
    ''' </summary>
    ''' <param name="nodes">The collection whose nodes are to be changed.</param>
    ''' <param name="value">True to check all the nodes and descendant nodes in
    ''' <paramref name="nodes"/>; False to uncheck them.</param>
    Private Sub SetCheckedRecursive( _
     ByVal nodes As TreeNodeCollection, ByVal value As Boolean)
        ' Make sure we're not trying to update a disposed UI
        If IsDisposed Then Return

        For Each n As TreeNode In nodes
            n.Checked = value
            n.StateImageIndex = If(value, CheckboxImages.Checked, CheckboxImages.Unchecked)
            SetCheckedRecursive(n.Nodes, value)
        Next
    End Sub

    ''' <summary>
    ''' Checks or unchecks a set of roles in this control
    ''' </summary>
    ''' <param name="roles">The roles whose nodes should be checked/unchecked</param>
    ''' <param name="value">True to check the specified roles; False to uncheck them.
    ''' </param>
    Private Sub SetRolesChecked( _
     ByVal roles As ICollection(Of Role), ByVal value As Boolean)
        ' Make sure we're not trying to update a disposed UI
        If IsDisposed Then Return

        For Each n As TreeNode In tvRoles.Nodes
            If roles.Contains(DirectCast(n.Tag, Role)) Then n.Checked = value
        Next
    End Sub

    ''' <summary>
    ''' Sets the user list values to the Active Directory users found in the given
    ''' collection.
    ''' </summary>
    ''' <param name="users">The user principals from which to draw the users for
    ''' the user list displayed on this control</param>
    Private Sub SetUserList(users As IEnumerable(Of UserPrincipal))
        ' Make sure we're not trying to update a disposed UI
        If IsDisposed Then Return

        Dim sortedasc = users.OrderBy(Function(k) k.Name, StringComparer.CurrentCulture)

        gridMembers.Rows.Clear()
        If users Is Nothing Then Return
        For Each u As UserPrincipal In sortedasc
            With gridMembers.Rows(gridMembers.Rows.Add())
                .Cells(colFullName.Index).Value = u.DisplayName
                .Cells(colDistinguishedName.Index).Value = u.DistinguishedName
                .Cells(colUserPrincipalName.Index).Value = u.UserPrincipalName
                .Tag = u
            End With
        Next
    End Sub

    ''' <summary>
    ''' Sets the user list values to the Blue Prism users found in the given
    ''' collection.
    ''' </summary>
    ''' <param name="users">The user objects from which to draw the users for
    ''' the user list displayed on this control</param>
    Private Sub SetUserList(users As IEnumerable(Of User))
        ' Make sure we're not trying to update a disposed UI
        If IsDisposed Then Return

        gridMembers.Rows.Clear()
        If users Is Nothing Then Return

        Dim sortedasc = users.OrderBy(Function(k) k.Name, StringComparer.CurrentCulture)

        For Each u As User In sortedasc
            With gridMembers.Rows(gridMembers.Rows.Add())
                .Cells(colFullName.Index).Value = u.Name
                .Cells(colValidFrom.Index).Value = u.Created
                .Cells(colValidTo.Index).Value = u.Expiry
                .Cells(colLastLoggedIn.Index).Value = u.LastSignedInAt
                .Cells(colPasswordExpiry.Index).Value = u.PasswordExpiry
                .Tag = u
            End With
        Next

    End Sub

    ''' <summary>
    ''' Ensures that the displayed information correctly represents the passed in 
    ''' role.
    ''' </summary>
    ''' <param name="r">The role to update the disply for</param>
    Private Sub UpdateRoleDisplay(r As Role)
        ' Make sure we're not trying to update a disposed UI
        If IsDisposed Then Return

        If r Is Nothing Then Return

        CheckedPermissions = r.Permissions

        DoSearch(New SearcherPayload() With {
            .Role = r,
            .Name = r.ActiveDirectoryGroup
        })

        btnGroupLookup.Enabled = Not r.SystemAdmin
    End Sub

    ''' <summary>
    ''' Ensures that the displayed information correctly represents the currently
    ''' selected role.
    ''' </summary>
    Private Sub UpdateRoleDisplay()
        UpdateRoleDisplay(SelectedRole)
    End Sub

    ''' <summary>
    ''' Adds the specified style to the image list for the passed TreeView making it
    ''' available in the StateImageList.
    ''' </summary>
    ''' <param name="style">The image to display as a Visual Style</param>
    ''' <param name="imageList">The ImageList the image should be added to</param>
    Private Sub AddCheckboxToImageList(ByVal style As VisualStyles.CheckBoxState, _
                                       ByVal imageList As ImageList)
        Dim i As New Bitmap(16, 16)
        CheckBoxRenderer.DrawCheckBox(Graphics.FromImage(i), New Point(0, 0), style)
        imageList.Images.Add(i)
    End Sub

    ''' <summary>
    ''' Recursively sets the Checked state and the StateImageIndex of a permission 
    ''' node and all its child nodes. 
    ''' </summary>
    ''' <param name="permNode">The permission node to update</param>
    ''' <param name="checking">True to check the node and any descendant permission 
    ''' nodes; False to uncheck them.</param>
    Private Sub UpdatePermissionNodeRecursive(ByVal permNode As TreeNode, _
                                           ByVal checking As Boolean)
        Dim image As CheckboxImages = CheckboxImages.Unchecked
        If checking Then image = CheckboxImages.Checked
        permNode.Checked = checking
        permNode.StateImageIndex = image
        For Each n As TreeNode In permNode.Nodes
            n.StateImageIndex = image
            n.Checked = checking
            UpdatePermissionNodeRecursive(n, checking)
        Next
    End Sub

    ''' <summary>
    ''' Checks/unchecks the passed in node, setting the correct StateImage in the UI 
    ''' and updating the currently selected role with the changes to the permissions. 
    ''' If the  node is a  permission group node then any child nodes will be 
    ''' checked/unchecked as required. 
    ''' </summary>
    Private Sub CheckPermissionTreeNode(ByVal n As TreeNode)
        Dim perms As New List(Of Permission)
        ' If the node represents a permission, we check/uncheck it and add it to our 
        ' list of permissions
        Dim selPerm As Permission = TryCast(n.Tag, Permission)
        If selPerm IsNot Nothing Then
            perms.Add(selPerm)
            Select Case n.StateImageIndex
                Case CheckboxImages.Checked
                    UpdatePermissionNodeRecursive(n, False)
                Case CheckboxImages.Unchecked
                    UpdatePermissionNodeRecursive(n, True)
            End Select
        End If

        ' If the node represents a group, we check/uncheck it and all of its child 
        ' nodes, and add the corresponding permissions to our list of permissions
        Dim selGroup As PermissionGroup = TryCast(n.Tag, PermissionGroup)
        If selGroup IsNot Nothing Then
            perms.AddRange(selGroup.Permissions)
            Select Case n.StateImageIndex
                Case CheckboxImages.Checked
                    UpdatePermissionNodeRecursive(n, False)
                Case CheckboxImages.Indeterminate, CheckboxImages.Unchecked
                    UpdatePermissionNodeRecursive(n, True)
            End Select
        End If

        ' Now all the permission nodes have been 'checked' we go back and update the 
        ' permissions tree to set the Permission Group nodes as checked, unchecked
        ' or partially checked as appropriate.
        For Each gpNode As TreeNode In tvPerms.Nodes
            Dim allChecked As Boolean = True
            Dim anyChecked As Boolean = False
            For Each permNode As TreeNode In gpNode.Nodes
                allChecked = allChecked AndAlso permNode.Checked
                anyChecked = anyChecked OrElse permNode.Checked
            Next
            With gpNode
                If allChecked Then
                    .StateImageIndex = CheckboxImages.Checked
                    .Checked = allChecked
                ElseIf anyChecked Then
                    .StateImageIndex = CheckboxImages.Indeterminate
                Else
                    .StateImageIndex = CheckboxImages.Unchecked
                End If
            End With
        Next

        ' Finally update the role with the changes to the permissions
        With SelectedRole
            If n.Checked Then .AddAll(perms) Else .RemoveAll(perms)
        End With
    End Sub

#End Region

End Class
