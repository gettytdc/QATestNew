Imports AutomateControls
Imports System.Windows.Forms.VisualStyles
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.TreeviewProcessing
Imports BluePrism.Core.Utility
Imports BluePrism.Images
Imports LocaleTools
Imports BluePrism.AutomateAppCore.Utility

Public Class frmEffectivePermissions
    Implements IEnvironmentColourManager, IHelp

    ''' <summary>
    ''' Private class to hold column names
    ''' </summary>
    Private Class ColumnNames
        Public Shared colChecked As String = "colChecked"
        Public Shared colCheckedRecordLevel As String = "colRecordLevelPermChecked"
        Public Shared colPermission As String = "colPermission"
        Public Shared colPermissionRecordLevel As String = "colRecordLevelPermission"
    End Class

    ''' <summary>
    ''' Private class to hold images
    ''' </summary>
    Private Class Images
        Public Shared restricted As Image = ImageLists.Components_16x16.Images(ImageLists.Keys.Component.ClosedGroup)
        Public Shared unrestricted As Image = ImageLists.Components_16x16.Images(ImageLists.Keys.Component.ClosedGlobalGroup)
    End Class

#Region " Members and Properties "

    ' The group member that we're viewing permissions for
    Private mGroupMember As IGroupMember

    ' The permissions available to this group member type
    Private mAvailablePermissions As ICollection(Of GroupTreePermission)

    ' The restricted permissions for this group member for each group that
    ' it is contained within
    Private mPermissions As ICollection(Of IGroupPermissions)

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

    Public Sub New(mem As IGroupMember)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        mGroupMember = mem
        mAvailablePermissions = gSv.GetGroupAvailablePermissions(mem.Tree.TreeType)

        ' Don't transmit the entire tree to the server.
        ' Added this code to blank the owner reference so that the 
        Dim myOwner As IGroup = mem.Owner
        mem.Owner = Nothing
        mPermissions = gSv.GetEffectiveGroupPermissionsForMember(mem)
        mem.Owner = myOwner

        Dim type = TreeDefinitionAttribute.GetLocalizedFriendlyName(mem.Tree.TreeType.GetTreeDefinition.SingularName)
        tbar.Title = String.Format(My.Resources.frmEffectivePermissions_Resources.tbar_Title_Template, type)
        tbar.SubTitle = mem.Name
        lblGroupInfo.Text = String.Format(My.Resources.frmEffectivePermissions_Resources.lblGroupInfo_Text_Template, type, mem.Name)
        PopulateRoles()

    End Sub

#End Region

#Region " Methods "

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
    End Sub

    ''' <summary>
    ''' Handle change of user role
    ''' </summary>
    Private Sub SwitchRole()
        Dim role = SelectedRole()
        If role Is Nothing Then Exit Sub

        dgvGroupInfo.Rows.Clear()
        Dim effectivePermissions As New List(Of Permission)()
        If mPermissions.Count = 0 Then
            ' Ungrouped item - just use role level permissions
            effectivePermissions.AddRange(role.Permissions.Intersect(mAvailablePermissions.Select(Function(x) x.Perm)))
        Else
            ' Grouped item
            For Each gp In mPermissions
                Dim group = mGroupMember.Tree.Root.FindById(gp.MemberId)
                Dim i = dgvGroupInfo.Rows.Add(If(gp.State = PermissionState.UnRestricted,
                                        Images.unrestricted, Images.restricted), group.RawMember.FullPath)
                dgvGroupInfo.Rows(i).Tag = group
                If gp.State = PermissionState.UnRestricted Then
                    dgvGroupInfo.Rows(i).Cells(0).ToolTipText = My.Resources.frmEffectivePermissions_Resources.Text_Unrestricted
                    effectivePermissions.AddRange(role.Permissions.Intersect(mAvailablePermissions.Select(Function(x) x.Perm)))
                Else
                    dgvGroupInfo.Rows(i).Cells(0).ToolTipText = My.Resources.frmEffectivePermissions_Resources.Text_Restricted
                    Dim glp = gp.FirstOrDefault(Function(g) g.Id = role.Id)
                    If glp IsNot Nothing Then effectivePermissions.AddRange(glp)
                End If
            Next
        End If

        dgvPermissions.Rows.Clear()
        For Each p In mAvailablePermissions.Where(Function(x) x.PermissionType = GroupPermissionLevel.Group)
            Dim i = dgvPermissions.Rows.Add(effectivePermissions.Contains(p.Perm), LTools.GetC(p.Perm.Name, "roleperms", "perm"))
            With dgvPermissions.Rows(i)
                .Tag = p
                .Cells(ColumnNames.colChecked).ReadOnly = True
                .Cells(ColumnNames.colPermission).Style.ForeColor = SystemColors.GrayText
                If Not role.Permissions.Contains(p.Perm) Then
                    .Cells(ColumnNames.colChecked).Value = Nothing
                End If
            End With
        Next
        dgvRecordPermissions.Rows.Clear()
        For Each p In mAvailablePermissions.Where(Function(x) x.PermissionType = GroupPermissionLevel.Member)
            Dim i = dgvRecordPermissions.Rows.Add(effectivePermissions.Contains(p.Perm), LTools.GetC(p.Perm.Name, "roleperms", "perm"))
            With dgvRecordPermissions.Rows(i)
                .Tag = p
                .Cells(ColumnNames.colCheckedRecordLevel).ReadOnly = True
                .Cells(ColumnNames.colPermissionRecordLevel).Style.ForeColor = SystemColors.GrayText
                If Not role.Permissions.Contains(p.Perm) Then
                    .Cells(ColumnNames.colCheckedRecordLevel).Value = Nothing
                End If
            End With
        Next

    End Sub

#End Region

#Region " Event Handlers "

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
     Handles dgvPermissions.CellPainting, dgvRecordPermissions.CellPainting

        ' This code is duplicated in frmGroupPermissions.vb. If you make changes
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
    ''' Handle click on view access rights link
    ''' </summary>
    Private Sub HandleGroupInfoCellContentClick(sender As Object, e As DataGridViewCellEventArgs) _
     Handles dgvGroupInfo.CellContentClick
        If e.ColumnIndex <> 2 Then Return
        'If link clicked then open manage access rights form
        Using f As New frmGroupPermissions(DirectCast(dgvGroupInfo.Rows(e.RowIndex).Tag, IGroup), True)
            f.SetEnvironmentColoursFromAncestor(Me)
            f.StartPosition = FormStartPosition.CenterParent
            f.ShowInTaskbar = False
            f.ShowDialog()
        End Using
    End Sub

    ''' <summary>
    ''' Ensures that the gridviews cannot have a selected row
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub HandleSelectionChanged(sender As Object, e As EventArgs) _
        Handles dgvPermissions.SelectionChanged, dgvRecordPermissions.SelectionChanged
        Dim gridView = TryCast(sender, DataGridView)
        If gridView IsNot Nothing Then
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
