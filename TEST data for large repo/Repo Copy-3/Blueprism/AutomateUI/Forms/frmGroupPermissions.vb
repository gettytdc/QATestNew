Imports AutomateControls
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.AutomateAppCore
Imports System.Windows.Forms.VisualStyles
Imports BluePrism.BPCoreLib
Imports BluePrism.Core.Utility
Imports LocaleTools
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.AutomateAppCore.Auth.Permission
Imports BluePrism.Server.Domain.Models

Public Class frmGroupPermissions
    Implements IEnvironmentColourManager, IHelp

    ''' <summary>
    ''' Private class to hold column names
    ''' </summary>
    Private Class ColumnNames
        Public Const colChecked As String = "colChecked"
        Public Const colCheckedRecordLevel As String = "colRecordLevelPermChecked"
        Public Const colPermission As String = "colPermission"
        Public Const colPermissionRecordLevel As String = "colRecordLevelPermission"
    End Class

#Region " Members and Properties "

    ' The ID of the group being configured
    Private mGroup As IGroup

    ' The previous role selected
    Private mLastRole As Role

    Private mAvailablePermissions As ICollection(Of GroupTreePermission)
    Private mGroupPermissions As IGroupPermissions
    Private mChangesMade As Boolean = False
    Private mInitialRestrictedState As PermissionState

    ''' <summary>
    ''' Whether the permissions are read-only
    ''' </summary>
    Private mArePermissionsReadOnly As Boolean = False

    ''' <summary>
    ''' The currently selected user role
    ''' </summary>
    Public ReadOnly Property SelectedRole() As Role
        Get
            If dgvRoles.SelectedRows.Count = 0 Then Return Nothing
            Return DirectCast(dgvRoles.SelectedRows(0).Tag, Role)
        End Get
    End Property

#End Region

#Region " Constructor "

    Public Sub New(grp As IGroup, Optional [readOnly] As Boolean = False)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        mGroup = grp

        ' Get the permissions for this group, inherited or direct or none.
        mAvailablePermissions = gSv.GetGroupAvailablePermissions(mGroup.TreeType)
        mGroupPermissions = DirectCast(gSv.GetEffectiveGroupPermissions(mGroup.IdAsGuid).Clone(), IGroupPermissions)
        mInitialRestrictedState = mGroupPermissions.State

        ' Decide what is and isn't enabled
        If mGroupPermissions.State = PermissionState.UnRestricted Then
            rbUnrestricted.Checked = True
            rbInherited.Enabled = False
            rbRestricted.Enabled = True
            rbUnrestricted.Enabled = True
            dgvGroupPermissions.ReadOnly = True
            dgvRecordPermissions.ReadOnly = True
            cbxSelectAllItemPerms.Enabled = False
            cbxSelectAllGroupPerms.Enabled = False
            mArePermissionsReadOnly = True
            btnDeselectAll.Enabled = False
            btnSelectAll.Enabled = False

        ElseIf mGroupPermissions.State = PermissionState.Restricted Then
            rbRestricted.Checked = True
            rbRestricted.Enabled = True
            rbUnrestricted.Enabled = True
            rbInherited.Enabled = False
            dgvGroupPermissions.ReadOnly = False
            dgvRecordPermissions.ReadOnly = False
            cbxSelectAllItemPerms.Enabled = True
            cbxSelectAllGroupPerms.Enabled = True
            mArePermissionsReadOnly = False
            btnDeselectAll.Enabled = True
            btnSelectAll.Enabled = True
        Else
            ' Inherited
            rbUnrestricted.Enabled = False
            rbRestricted.Enabled = False
            rbInherited.Enabled = False
            rbInherited.Checked = True

            Dim info As Group = gSv.GetGroup(mGroupPermissions.InheritedAncestorID)
            rbInherited.Text = String.Format(
                My.Resources.frmGroupPermissions_Resources.rbInherited_Text_Template, info.Name)
            btnOK.Enabled = False
            dgvGroupPermissions.ReadOnly = True
            dgvRecordPermissions.ReadOnly = True
            cbxSelectAllItemPerms.Enabled = False
            cbxSelectAllGroupPerms.Enabled = False
            mArePermissionsReadOnly = True
            btnDeselectAll.Enabled = False
            btnSelectAll.Enabled = False
        End If

        tbar.Title = String.Format(My.Resources.frmGroupPermissions_Resources.tbar_Title_Template, grp.Name)
        tbar.SubTitle = String.Format(My.Resources.frmGroupPermissions_Resources.tbar_SubTitle_Template, grp.ItemTotalCount,
            If(grp.RawMembers.Count = 1,
                TreeDefinitionAttribute.GetLocalizedFriendlyName(grp.TreeType.GetTreeDefinition.SingularName),
                TreeDefinitionAttribute.GetLocalizedFriendlyName(grp.TreeType.GetTreeDefinition.PluralName)))
        PopulateRoles()

        If [readOnly] OrElse Not grp.Permissions.HasPermission(User.Current, grp.TreeType.GetTreeDefinition.AccessRightsPermission) Then
            Text = My.Resources.frmGroupPermissions_Resources.frmGroupPermissions_Text_ReadOnly
            SetReadOnly()
            SwitchRole()
        Else
            Text = My.Resources.frmGroupPermissions_Resources.frmGroupPermissions_Text
        End If

    End Sub

#End Region

#Region " Methods "

    ''' <summary>
    ''' Set the form to read only
    ''' </summary>
    Private Sub SetReadOnly()
        rbInherited.Enabled = False
        rbRestricted.Enabled = False
        rbUnrestricted.Enabled = False
        dgvGroupPermissions.ReadOnly = True
        dgvRecordPermissions.ReadOnly = True
        cbxSelectAllItemPerms.Enabled = False
        cbxSelectAllGroupPerms.Enabled = False
        btnOK.Enabled = False
        btnDeselectAll.Enabled = False
        btnSelectAll.Enabled = False
        mArePermissionsReadOnly = True
    End Sub

    ''' <summary>
    ''' Display the user roles that have access to at least 1 of the
    ''' available permissions.
    ''' </summary>
    Private Sub PopulateRoles()
        For Each r In SystemRoleSet.SystemCurrent
            If r.ShouldBeDisplayed(mAvailablePermissions) Then
                Dim i = dgvRoles.Rows.Add(LTools.GetC(r.Name, "roleperms", "role"))
                dgvRoles.Rows(i).Tag = r
            End If
        Next
        SelectFirstRole()
    End Sub

    ''' <summary>
    ''' Position on the first user role in the grid
    ''' </summary>
    Private Sub SelectFirstRole()
        If dgvRoles.Rows.Count = 0 Then Exit Sub
        dgvRoles.Rows(0).Selected = True
        UpdateGroupVisibilityWarningLabel()
        UpdateSelectAllCheckboxes()
    End Sub

    ''' <summary>
    ''' Save the current role permissions to memory
    ''' </summary>
    Private Sub SaveCurrentRole()
        If mLastRole Is Nothing Then Exit Sub

        Dim glp = GetGroupLevelPermsForRole(mLastRole)
        glp.Clear()
        For Each r As DataGridViewRow In dgvGroupPermissions.Rows
            If CBool(r.Cells(ColumnNames.colChecked).Value) Then
                glp.Add(DirectCast(r.Tag, Permission))
            End If
        Next
        For Each r As DataGridViewRow In dgvRecordPermissions.Rows
            If CBool(r.Cells(ColumnNames.colCheckedRecordLevel).Value) Then
                glp.Add(DirectCast(r.Tag, Permission))
            End If
        Next


    End Sub

    ''' <summary>
    ''' Handle change of user role
    ''' </summary>
    Private Sub SwitchRole()
        Dim role = SelectedRole()
        If role Is Nothing Then Exit Sub

        SaveCurrentRole()
        mLastRole = role

        dgvGroupPermissions.Rows.Clear()
        dgvRecordPermissions.Rows.Clear()
        Dim effectivePermissions As ICollection(Of Permission)
        If mGroupPermissions.State <> PermissionState.UnRestricted Then
            effectivePermissions = GetGroupLevelPermsForRole(role).Permissions
        Else
            effectivePermissions = role.Permissions
        End If

        Dim userHasRole = User.Current.HasRole(role)
        Dim permission = mGroup.Tree.TreeType.GetTreeDefinition.AccessRightsPermission

        Dim GetCheckboxValue = Function(p As Permission) _
            If(
                role.Permissions.Contains(p),
                effectivePermissions.Contains(p),
                DirectCast(Nothing, Boolean?))

        cbxSelectAllItemPerms.Enabled = False
        cbxSelectAllGroupPerms.Enabled = False

        For Each groupPermission In mAvailablePermissions.Where(Function(x) x.PermissionType = GroupPermissionLevel.Group)

            Dim permissionValue = GetCheckboxValue(groupPermission.Perm)
            '#bg-4261 (relates to #bg-4039) We need to get this permission from the role and not the tree
            'if the value if false then set to nothing to stop the checkbox showing at all
            If groupPermission.Perm.Name = ObjectStudio.ImportBusinessObject OrElse
                    groupPermission.Perm.Name = ProcessStudio.ImportProcess Then
                permissionValue = If(role.Permissions.Contains(groupPermission.Perm), ctype(True, Boolean?), Nothing)
            End If

            Dim newIndex = dgvGroupPermissions.Rows.Add(permissionValue, LTools.GetC(groupPermission.Perm.Name, "roleperms", "perm"))
            Dim row = dgvGroupPermissions.Rows(newIndex)
            row.Tag = groupPermission.Perm

            If mArePermissionsReadOnly OrElse
                (userHasRole AndAlso (groupPermission.Perm.Equals(permission))) OrElse
                Not role.Permissions.Contains(groupPermission.Perm) Then

                SetRowDisabled(row, ColumnNames.colChecked, ColumnNames.colPermission)
            Else
                '#BG-4039: The following if statement is a temporary measeure to ensure the import process
                'and import business object permissions are not editable and should only be assignable at the system level
                'See ticket for further info
                If groupPermission.Perm.Name = ObjectStudio.ImportBusinessObject OrElse
                    groupPermission.Perm.Name = ProcessStudio.ImportProcess Then
                    SetRowDisabled(row, ColumnNames.colChecked, ColumnNames.colPermission)
                End If
                cbxSelectAllGroupPerms.Enabled = True
            End If
        Next

        For Each memberPermission In mAvailablePermissions.Where(Function(x) x.PermissionType = GroupPermissionLevel.Member)
            Dim value = GetCheckboxValue(memberPermission.Perm)

            Dim newIndex = dgvRecordPermissions.Rows.Add(value, LTools.GetC(memberPermission.Perm.Name, "roleperms", "perm"))
            Dim row = dgvRecordPermissions.Rows(newIndex)
            row.Tag = memberPermission.Perm
            If mArePermissionsReadOnly OrElse Not role.Permissions.Contains(memberPermission.Perm) Then
                SetRowDisabled(row, ColumnNames.colCheckedRecordLevel, ColumnNames.colPermissionRecordLevel)
            Else
                cbxSelectAllItemPerms.Enabled = True
            End If
        Next

        UpdateGroupVisibilityWarningLabel()
        UpdateSelectAllCheckboxes()
        DisplayUsers(role)
    End Sub

    ''' <summary>
    ''' Sets the row disabled.
    ''' </summary>
    ''' <param name="row">The row to set disabled</param>
    ''' <param name="checkboxColumn">The name of the column in the row containing the checkbox</param>
    ''' <param name="textColumn">The name of the column in the row containting the text</param>
    Private Sub SetRowDisabled(row As DataGridViewRow, checkboxColumn As String, textColumn As String)
        Dim chkCell = row.Cells(checkboxColumn)
        chkCell.ReadOnly = True
        row.Cells(textColumn).Style.ForeColor = SystemColors.GrayText
    End Sub

    ''' <summary>
    ''' Get the group level permissions for the specified role
    ''' </summary>
    ''' <param name="role">The role to check</param>
    ''' <returns></returns>
    Private Function GetGroupLevelPermsForRole(role As Role) As GroupLevelPermissions
        With mGroupPermissions
            Dim gp = .FirstOrDefault(Function(g) g.Id = role.Id)
            If gp Is Nothing Then
                gp = New GroupLevelPermissions(role.Id, role.Name)
                ' Only populate all the permissions for this group level permissions,
                ' if the group permissions doesn't exist, the inital restriction state on from load was 'unrestricted' and it is now restricted.
                ' Prevents being populated when they are empty intentionally.
                If mInitialRestrictedState = PermissionState.UnRestricted AndAlso rbRestricted.Checked Then
                    gp.AddAll(role.Permissions.Intersect(mAvailablePermissions.Select(Function(x) x.Perm)))
                End If
                .Add(gp)
            End If
            Return gp
        End With
    End Function

    ''' <summary>
    ''' Load and display Blue Prism Roles in the grid, and whether the user is a
    ''' member of the associated security group.
    ''' </summary>
    ''' <param name="role"> The role to display the users for</param>
    Private Sub DisplayUsers(role As Role)

        If User.IsLoggedInto(DatabaseType.SingleSignOn) Then
            pnlUsersInRole.Visible = False
            Return
        End If

        Dim users = gSv.GetActiveUsersInRole(role.Id)

        dgvUsers.Rows.Clear()

        For Each user As User In users
            If user.IsHidden Then Continue For

            'Create the user row and add to the grid
            Dim r As New DataGridViewRow
            r.CreateCells(dgvUsers, user.Name)
            dgvUsers.Rows.Add(r)
        Next

        dgvUsers.Sort(colUserName, ListSortDirection.Ascending)

        ' Clear row selection
        dgvUsers.SelectedRow = Nothing

    End Sub

    Private Function HasChangedInheritedSubGroupPermissions() As Boolean
        Return mGroupPermissions.State = PermissionState.Restricted AndAlso HasChangesBeenMade() AndAlso mGroup.SubgroupCount > 0
    End Function

    Private Function HasChangesBeenMade() As Boolean
        Return mChangesMade OrElse mInitialRestrictedState <> mGroupPermissions.State
    End Function

    Private Function HasRestrictionBeenAddedToGroup() As Boolean
        Return mInitialRestrictedState = PermissionState.UnRestricted AndAlso _
            mGroupPermissions.State = PermissionState.Restricted
    End Function

    Private Function HasRestrictedSubGroups As Boolean

        Dim subgroupIds = mGroup.Search(Function(m) m.IsGroup) _
                .Select(Function(g) g.IdAsGuid()) _
                .ToList()

        Dim restrictions = gSv.GetGroupPermissionStates(subgroupIds)
        Dim result = restrictions.Any(Function(g) g.Value = PermissionState.Restricted)
        Return result

    End Function

    Private Sub SetCheckStateForAllItemLevelPermissionsCheckboxes(checkState As Boolean)
        For Each row As DataGridViewRow In dgvRecordPermissions.Rows
            Dim cell = row.Cells(ColumnNames.colCheckedRecordLevel)
            Dim v = cell.EditedFormattedValue
            If Not cell.ReadOnly AndAlso CBool(cell.Value) <> checkState Then
                mChangesMade = True
                cell.Value = checkState
            End If
        Next
    End Sub

    Private Sub SetCheckStateForAllGroupLevelPermissionsCheckboxes(checkState As Boolean)
        For Each row As DataGridViewRow In dgvGroupPermissions.Rows
            Dim cell = row.Cells(ColumnNames.colChecked)
            Dim v = cell.EditedFormattedValue
            If Not cell.ReadOnly AndAlso CBool(cell.Value) <> checkState Then
                mChangesMade = True
                cell.Value = checkState
            End If
        Next
    End Sub

    Private Function AllItemPermissionsHaveValue(value As Boolean) As Boolean
        For Each row As DataGridViewRow In dgvRecordPermissions.Rows
            Dim cell = row.Cells(ColumnNames.colCheckedRecordLevel)
            If Not cell.ReadOnly Then
                If CBool(cell.EditedFormattedValue) <> value Then
                    Return False
                End If
            End If
        Next
        Return True
    End Function

    Private Function AllGroupPermissionsHaveValue(value As Boolean) As Boolean
        For Each row As DataGridViewRow In dgvGroupPermissions.Rows
            Dim cell = row.Cells(ColumnNames.colChecked)
            If Not cell.ReadOnly Then
                If CBool(cell.EditedFormattedValue) <> value Then
                    Return False
                End If
            End If
        Next
        Return True
    End Function

    ''' <summary>
    ''' Returns true if all of the group permissions for the current role are unchecked.
    ''' </summary>
    ''' <returns></returns>
    Private Function AreAllPermissionsUnchecked() As Boolean
        Return AllItemPermissionsHaveValue(False) AndAlso AllGroupPermissionsHaveValue(False)
    End Function

    ''' <summary>
    ''' Display / hide the warning label regarding hidden group if all permissions are unchecked.
    ''' </summary>
    Private Sub UpdateGroupVisibilityWarningLabel()
        lblGroupVisibilityWarning.Visible = rbRestricted.Checked AndAlso AreAllPermissionsUnchecked()
    End Sub

    ''' <summary>
    ''' Checks / Unchecks the 'Select All' checkboxes based on the 'Check' states of the permissions in the datagrid 
    ''' This will uncheck the appropriate 'Select All' checkbox if all the associated permission checkboxes are unchecked.
    ''' </summary>
    Private Sub UpdateSelectAllCheckboxes()
        RemoveHandler cbxSelectAllItemPerms.CheckedChanged, AddressOf HandleSelectAllItemsCheckBoxChanged
        cbxSelectAllItemPerms.Checked = AllItemPermissionsHaveValue(True)
        AddHandler cbxSelectAllItemPerms.CheckedChanged, AddressOf HandleSelectAllItemsCheckBoxChanged

        RemoveHandler cbxSelectAllGroupPerms.CheckedChanged, AddressOf HandleSelectAllGroupsCheckBoxChanged
        cbxSelectAllGroupPerms.Checked = AllGroupPermissionsHaveValue(True)
        AddHandler cbxSelectAllGroupPerms.CheckedChanged, AddressOf HandleSelectAllGroupsCheckBoxChanged
    End Sub

#End Region

#Region " Event Handlers "

    ''' <summary>
    ''' Handles the first time a permissions checkbox is changed.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub DataGridCheckboxChanged(sender As Object, e As EventArgs) _
        Handles dgvGroupPermissions.CurrentCellDirtyStateChanged, dgvRecordPermissions.CurrentCellDirtyStateChanged
        mChangesMade = True
    End Sub


    ''' <summary>
    ''' Handle switching betweem restricted and unrestricted
    ''' </summary>
    Private Sub HandleRestrictedChanged(sender As Object, e As EventArgs) _
     Handles rbRestricted.CheckedChanged, rbUnrestricted.CheckedChanged

        dgvGroupPermissions.ReadOnly = rbUnrestricted.Checked
        dgvRecordPermissions.ReadOnly = rbUnrestricted.Checked
        cbxSelectAllItemPerms.Enabled = rbRestricted.Checked
        cbxSelectAllGroupPerms.Enabled = rbRestricted.Checked
        btnDeselectAll.Enabled = rbRestricted.Checked
        btnSelectAll.Enabled = rbRestricted.Checked

        If (rbRestricted.Checked) Then
            mGroupPermissions.State = PermissionState.Restricted
            mArePermissionsReadOnly = False
        Else
            mGroupPermissions.State = PermissionState.UnRestricted
            mArePermissionsReadOnly = True
        End If

        ' If switching to unrestricted, restore effective permissions
        If rbUnrestricted.Checked Then
            mGroupPermissions.Clear()
            SelectFirstRole()
        Else
            For Each r As DataGridViewRow In dgvRoles.Rows
                GetGroupLevelPermsForRole(DirectCast(r.Tag, Role))
            Next
        End If

        SwitchRole()
    End Sub

    ''' <summary>
    ''' Handle selected user role changing
    ''' </summary>
    Private Sub HandleRoleSelected(sender As Object, e As EventArgs) _
     Handles dgvRoles.SelectionChanged
        SwitchRole()
    End Sub

    ''' <summary>
    ''' Override cell painting for permissions checkbox
    ''' </summary>
    Private Sub HandleCellPainting(sender As Object, e As DataGridViewCellPaintingEventArgs) _
     Handles dgvGroupPermissions.CellPainting, dgvRecordPermissions.CellPainting

        ' This code is duplicated in frmEffectivePermissions.vb. If you make changes
        ' here, consider making the same changes in that file.

        Dim dgv = CType(sender, DataGridView)
        If e.ColumnIndex = 0 AndAlso e.RowIndex >= 0 Then
            ' Hide checkbox for any permission not available to the role
            Dim chkCell = dgv.Rows(e.RowIndex).Cells(e.ColumnIndex)
            If chkCell.Value Is Nothing Then
                e.PaintBackground(e.ClipBounds, True)
                e.Handled = True
                Return
            End If

            ' Render the checkbox disabled if it is readonly.
            If chkCell.ReadOnly Then
                e.PaintBackground(e.ClipBounds, True)
                Dim state = If(CBool(chkCell.Value), CheckBoxState.CheckedDisabled, CheckBoxState.UncheckedDisabled)
                Dim gridViewCenter = dgv.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, False).Center()
                Dim checkboxCenter = CheckBoxRenderer.GetGlyphSize(e.Graphics, state).Center()
                Dim checkboxPosition = New Point(gridViewCenter.X - checkboxCenter.X, gridViewCenter.Y - checkboxCenter.Y)
                CheckBoxRenderer.DrawCheckBox(e.Graphics, checkboxPosition, state)
                e.Handled = True
            End If
        End If
    End Sub

    ''' <summary>
    ''' Handle OK button click
    ''' </summary>
    Private Sub HandleOKButtonClick(sender As Object, e As EventArgs) _
     Handles btnOK.Click

        If HasChangesBeenMade() Then
            If HasRestrictionBeenAddedToGroup() AndAlso HasRestrictedSubGroups() Then

                Dim result = MessageBox.Show(My.Resources.frmGroupPermissions_Resources.dlg_overwrite_subgroup_permissions, My.Resources.frmGroupPermissions_Resources.frmGroupPermissions_HandleOKButtonClick_Warning, MessageBoxButtons.OKCancel, MessageBoxIcon.Question)
                If result <> DialogResult.OK Then Return

            ElseIf HasChangedInheritedSubGroupPermissions() Then

                Dim result = MessageBox.Show(String.Format(
                    My.Resources.frmGroupPermissions_Resources.dlg_permissions_will_be_applied_to_subgroups, TreeDefinitionAttribute.GetLocalizedFriendlyName(mGroup.TreeType.GetTreeDefinition().PluralName)),
                    My.Resources.frmGroupPermissions_Resources.frmGroupPermissions_HandleOKButtonClick_Warning, MessageBoxButtons.OKCancel, MessageBoxIcon.Question)
                If result <> DialogResult.OK Then Return

            End If

            SaveCurrentRole()

            Try
                gSv.SetActualGroupPermissions(mGroup.IdAsGuid, mGroupPermissions)
            Catch ex As BluePrismException
                UserMessage.Err(ex.Message)
            End Try
        End If

        DialogResult = DialogResult.OK
        Close()
    End Sub

    ''' <summary>
    ''' Handle Cancel button click
    ''' </summary>
    Private Sub HandlCancelButtonClick(sender As Object, e As EventArgs) _
     Handles btnCancel.Click
        DialogResult = DialogResult.Cancel
        Close()
    End Sub


    ''' <summary>
    ''' Handles clicks in the item level permissions DataGridView cells.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub HandleItemPermsCheckboxMouseClick(sender As Object, e As EventArgs) _
        Handles dgvRecordPermissions.CellContentClick
        UpdateGroupVisibilityWarningLabel()
        UpdateSelectAllCheckboxes()
    End Sub

    ''' <summary>
    ''' Handles clicks in the group level permissions DataGridView cells.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub HandleGroupPermsCheckboxMouseClick(sender As Object, e As EventArgs) _
        Handles dgvGroupPermissions.CellContentClick
        UpdateGroupVisibilityWarningLabel()
        UpdateSelectAllCheckboxes()
    End Sub

    ''' <summary>
    ''' Handles the "Select All" item level permissions checkbox state changing.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub HandleSelectAllItemsCheckBoxChanged(sender As Object, e As EventArgs) _
        Handles cbxSelectAllItemPerms.CheckedChanged
        SetCheckStateForAllItemLevelPermissionsCheckboxes(cbxSelectAllItemPerms.Checked)
        UpdateGroupVisibilityWarningLabel()
    End Sub

    ''' <summary>
    ''' Handles the "Select All" group level permissions checkbox state changing.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub HandleSelectAllGroupsCheckBoxChanged(sender As Object, e As EventArgs) _
        Handles cbxSelectAllGroupPerms.CheckedChanged
        SetCheckStateForAllGroupLevelPermissionsCheckboxes(cbxSelectAllGroupPerms.Checked)
        UpdateGroupVisibilityWarningLabel()
    End Sub

    ''' <summary>
    ''' Ensures that the gridviews cannot have a selected row when in readonly mode.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub HandleSelectionChanged(sender As Object, e As EventArgs) _
        Handles dgvGroupPermissions.SelectionChanged, dgvRecordPermissions.SelectionChanged
        Dim gridView = TryCast(sender, DataGridView)
        If gridView IsNot Nothing AndAlso gridView.ReadOnly Then
            gridView.ClearSelection()
        End If
    End Sub

#End Region

#Region " IEnvironmentColourManager Implementation "

    ''' <summary>
    ''' Gets or sets the environment-specific back colour in use in this environment.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentBackColor As Color _
     Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return tbar.BackColor
        End Get
        Set(value As Color)
            tbar.BackColor = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the environment-specific back colour in use in this environment.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentForeColor As Color _
     Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return tbar.ForeColor
        End Get
        Set(value As Color)
            tbar.ForeColor = value
        End Set
    End Property


    ''' <summary>
    ''' Handles the 'Deselect All' button click.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnDeselectAll_Click(sender As Object, e As EventArgs) Handles btnDeselectAll.Click
        mChangesMade = True

        For Each gp In mGroupPermissions
            gp.Clear()
        Next

        SetCheckStateForAllItemLevelPermissionsCheckboxes(False)
        SetCheckStateForAllGroupLevelPermissionsCheckboxes(False)

        cbxSelectAllGroupPerms.Checked = False
        cbxSelectAllItemPerms.Checked = False
    End Sub

    ''' <summary>
    ''' Handles the 'Select All' button click.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnSelectAll_Click(sender As Object, e As EventArgs) Handles btnSelectAll.Click
        mChangesMade = True
        For Each row As DataGridViewRow In dgvRoles.Rows
            Dim role = CType(row.Tag, Role)
            Dim groupLevelPerms = mGroupPermissions.FirstOrDefault(Function(x) x.Id = role.Id)
            If groupLevelPerms Is Nothing Then
                groupLevelPerms = New GroupLevelPermissions(role.Id)
                mGroupPermissions.Add(groupLevelPerms)
            End If
            groupLevelPerms.Clear()
            groupLevelPerms.AddAll(role.Permissions.Intersect(mAvailablePermissions.Select(Function(x) x.Perm)))

        Next

        SetCheckStateForAllItemLevelPermissionsCheckboxes(True)
        SetCheckStateForAllGroupLevelPermissionsCheckboxes(True)

        cbxSelectAllGroupPerms.Checked = True
        cbxSelectAllItemPerms.Checked = True
    End Sub



#End Region

#Region " IHelp Implementation "

    Public Overrides Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "mte-getting-started.htm"
    End Function

    Public Overrides Sub OpenHelp()
        Try
            OpenHelpFile(Me, GetHelpFile())
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

#End Region

End Class
