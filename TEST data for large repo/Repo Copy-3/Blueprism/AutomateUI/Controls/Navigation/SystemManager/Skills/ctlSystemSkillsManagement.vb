Imports AutomateControls
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateProcessCore
Imports BluePrism.Skills

Public Class ctlSystemSkillsManagement
    Implements IPermission, IHelp, IStubbornChild

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)

        Try
            PopulateDataGrid(gSv.GetSkills())
            SetOptions()
        Catch ex As Exception
            UserMessage.Err(String.Format(My.Resources.ctlSystemSkillsManagement_UnableToRetrieveDataFromServer0, ex.Message))
        End Try

    End Sub

    Private Sub SetOptions()
        llDelete.Enabled = (dgvSkills.Rows.Count > 0 AndAlso
                                User.Current.HasPermission(Permission.Skills.ManageSkill))
        llReferences.Enabled = (dgvSkills.Rows.Count > 0)
    End Sub

    Private Sub PopulateDataGrid(skills As IEnumerable(Of Skill))
        dgvSkills.Rows.Clear()

        For Each skill In skills

            Dim webSkillVersion = TryCast(skill.LatestVersion, WebSkillVersion)
            Dim row = New DataGridViewRow()
            Dim index = dgvSkills.Rows.Add(skill.LatestVersion.Name,
                               SkillCategoryExtensions.GetDescription(skill.LatestVersion.Category),
                               skill.Provider,
                               skill.LatestVersion.VersionNumber,
                               skill.PreviousVersions.Count,
                               If(webSkillVersion?.WebApiName, ""),
                               skill.Enabled)

            dgvSkills.Rows(index).Tag = skill

            If Not User.Current.HasPermission(Permission.Skills.ManageSkill) Then
                dgvSkills.Rows(index).Cells(colEnabled.Index).ReadOnly = True
            End If

            ' If web api is disabled set tooltip and make enabled checkbox unchecked and readonly
            If webSkillVersion IsNot Nothing AndAlso Not webSkillVersion.WebApiEnabled Then
                dgvSkills.Rows(index).Cells(colWebAPI.Index).ToolTipText = My.Resources.ctlSystemSkillsManagement_WebApiNotEnabled

                ' Remove CellValueChanged handler so it doesn't fire when updating checkbox value
                RemoveHandler dgvSkills.CellValueChanged, AddressOf HandleEnabledCheckboxClicked
                dgvSkills.Rows(index).Cells(colEnabled.Index).Value = False
                AddHandler dgvSkills.CellValueChanged, AddressOf HandleEnabledCheckboxClicked

                dgvSkills.Rows(index).Cells(colEnabled.Index).ReadOnly = True
            End If

            If skill.PreviousVersions.Count = 0 Then
                dgvSkills.Rows(index).Cells(colInstalledVersions.Index) = New DataGridViewTextBoxCell() With {.Value = "0"}
                dgvSkills.Rows(index).Cells(colInstalledVersions.Index).ReadOnly = True
            End If
        Next


    End Sub

    ' Handle the 'Previous Versions' column link being clicked.
    Private Sub HandleCellClicked(sender As Object, e As DataGridViewCellEventArgs) Handles dgvSkills.CellClick
        If e.ColumnIndex = colInstalledVersions.Index AndAlso e.RowIndex <> -1 Then
            If TypeOf dgvSkills.Rows(e.RowIndex).Cells(e.ColumnIndex) Is DataGridViewLinkCell Then
                Dim skill = CType(dgvSkills.Rows(e.RowIndex).Tag, Skill)
                Dim versionForm = New SkillVersionsForm(skill)
                versionForm.Show(Me)
            End If
        End If

    End Sub

    ' Handle the 'Enabled' checkbox being clicked.
    Private Sub HandleEnabledCheckboxClicked(sender As Object, e As DataGridViewCellEventArgs) Handles dgvSkills.CellValueChanged
        If e.ColumnIndex = colEnabled.Index AndAlso e.RowIndex <> -1 Then
            Dim skill = CType(dgvSkills.Rows(e.RowIndex).Tag, Skill)
            Dim enabled = CType(dgvSkills.Rows(e.RowIndex).Cells(e.ColumnIndex).Value, Boolean)

            Try
                gSv.UpdateSkillEnabled(skill.Id, enabled)
            Catch ex As Exception
                UserMessage.Err(String.Format(My.Resources.ctlSystemSkillsManagement_ErrorWhenUpdatingSkill0, ex.Message))
            End Try


        End If
    End Sub

    ' Fixes issue with checkbox not firing CellValueChanged event until checkbox loses focus.
    ' https://stackoverflow.com/questions/11843488/how-to-detect-datagridview-checkbox-event-change
    Private Sub HandleCellMouseUp(sender As Object, e As DataGridViewCellMouseEventArgs) Handles dgvSkills.CellMouseUp
        If e.ColumnIndex = colEnabled.Index AndAlso e.RowIndex <> -1 Then
            dgvSkills.EndEdit()
        End If
    End Sub

    ' Draw the warning icon next the web api name, and draw the disabled checkbox if the web api is disabled.
    Private Sub HandleCellPainting(sender As Object, e As DataGridViewCellPaintingEventArgs) Handles dgvSkills.CellPainting

        ' Draw the web api warning icon next to th name if the web api is disabled.
        If e.ColumnIndex = colWebAPI.Index AndAlso e.RowIndex <> -1 Then

            Dim skill = CType(dgvSkills.Rows(e.RowIndex).Tag, Skill)

            Dim webSkillVersion = TryCast(skill.LatestVersion, WebSkillVersion)
            If webSkillVersion Is Nothing Then Return

            If webSkillVersion.WebApiEnabled Then Return

            Dim bmp = BluePrism.Images.ToolImages.Warning_16x16
            e.PaintBackground(e.CellBounds, False)

            Dim imgDestination = New Rectangle(e.CellBounds.Left, e.CellBounds.Top + CInt(e.CellBounds.Height / 2 - 8), 16, 16)
            Dim imgSource = New Rectangle(0, 0, 16, 16)
            e.Graphics.DrawImage(bmp, imgDestination, imgSource, GraphicsUnit.Pixel)
            e.Handled = True

            TextRenderer.DrawText(e.Graphics, e.Value.ToString(),
                                      e.CellStyle.Font, New Rectangle(e.CellBounds.Left + 16, e.CellBounds.Top, e.CellBounds.Width - 16, e.CellBounds.Height),
                                      e.CellStyle.ForeColor, TextFormatFlags.VerticalCenter)
        End If

        ' If checkbox is readonly, draw it greyed-out.
        ' https://social.msdn.microsoft.com/Forums/en-US/bcaf5432-42f3-4551-9e62-36aca59f10d3/styling-the-checkbox-in-the-datagridviewcheckbox-column?forum=winformsdatacontrols
        If e.ColumnIndex = colEnabled.Index AndAlso e.RowIndex <> -1 Then
            If dgvSkills.Rows(e.RowIndex).Cells(e.ColumnIndex).ReadOnly Then
                e.PaintBackground(e.CellBounds, False)
                Dim checkBoxLocation = e.CellBounds.Location
                checkBoxLocation.Offset(CInt(e.CellBounds.Width / 2 - 8), CInt(e.CellBounds.Height / 2 - 8))
                If CBool(dgvSkills.Rows(e.RowIndex).Cells(e.ColumnIndex).Value) = False Then
                    CheckBoxRenderer.DrawCheckBox(e.Graphics, checkBoxLocation, VisualStyles.CheckBoxState.UncheckedDisabled)
                Else
                    CheckBoxRenderer.DrawCheckBox(e.Graphics, checkBoxLocation, VisualStyles.CheckBoxState.CheckedDisabled)
                End If

                e.Handled = True

            End If
        End If

    End Sub

    Private Sub HandleFindReferences(sender As Object, e As EventArgs) Handles llReferences.Click
        Dim skill = CType(dgvSkills.SelectedRow.Tag, Skill)
        ParentAppForm.FindReferences(New clsProcessSkillDependency(skill.Id, skill.LatestVersion.Name))
    End Sub

    Private Sub HandleDelete(sender As Object, e As EventArgs) Handles llDelete.Click
        Dim skill = CType(dgvSkills.SelectedRow.Tag, Skill)
        Dim dependencyList = New clsProcessDependencyList()
        dependencyList.Add(New clsProcessSkillDependency(skill.Id, skill.LatestVersion.Name))
        If Not ParentAppForm.ConfirmDeletion(dependencyList) Then Return

        Try
            gSv.DeleteSkill(skill.Id, skill.LatestVersion.Name)
            dgvSkills.Rows.RemoveAt(dgvSkills.SelectedRow.Index)
            SetOptions()
        Catch ex As Exception
            UserMessage.Err(String.Format(My.Resources.ctlSystemSkillsManagement_ErrorDeletingSkill0, ex.Message))
        End Try
    End Sub

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "ctlSystemSkillsManagement.htm"
    End Function

    Public Function CanLeave() As Boolean Implements IStubbornChild.CanLeave
        Return True
    End Function

    Public ReadOnly Property RequiredPermissions As ICollection(Of Permission) Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName(Permission.Skills.ViewSkill, Permission.Skills.ManageSkill)
        End Get
    End Property

    Public Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
End Class
