Imports AutomateControls
Imports AutomateUI.My.Resources
Imports BluePrism.AutomateAppCore.Auth
Imports LocaleTools

Public Class CtlUsersSummary(Of T As Class) : Implements IDisposable

    Public Event OnSelectedUsersChanged As UserSelectedEventHandler

    Private WithEvents SelectedUsers As SortableBindingList(Of T)
    Private ReadOnly mSelectedRoles As RoleSet
    Private ReadOnly mBinIcon As Bitmap = ToolImages.Bin_16x16

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
                mBinIcon.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Public Sub New(selectedUsers As SortableBindingList(Of T), selectedRoles As RoleSet, ParamArray bindingPropertyNames() As String)
        Me.SelectedUsers = selectedUsers
        mSelectedRoles = selectedRoles
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        lblChosenUsers.Text = Resources.ctlUsersSummary_ChosenUsers
        lblRoles.Text = Resources.ctlUsersSummary_ChosenRoles
        CreateColumns(bindingPropertyNames)
        LoadUserGrid()
        LoadRolesGrid()
    End Sub

    Private Sub CreateColumns(ParamArray bindingPropertyNames() As String)
        Dim index = 0
        For Each propertyName In bindingPropertyNames
            Dim column = CreateColumn(propertyName)
            gridUsers.Columns.Insert(index, column)
            index += 1
        Next
    End Sub

    Private Function CreateColumn(dataPropertyName As String) As DataGridViewTextBoxColumn
        Dim column = New DataGridViewTextBoxColumn With {
            .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            .DataPropertyName = dataPropertyName,
            .FillWeight = 100,
            .HeaderText = "",
            .Name = $"col{dataPropertyName}",
            .ReadOnly = True,
            .Resizable = DataGridViewTriState.[False]
        }
        Return column
    End Function

    Private Sub LoadUserGrid()
        SelectedUsers.AllowRemove = True
        Dim userBindingSource = New BindingSource() With {.DataSource = SelectedUsers, .AllowNew = False}
        gridUsers.AutoGenerateColumns = False
        gridUsers.DataSource = userBindingSource

        UpdateAndRepositionUserCount()
    End Sub

    Private Sub LoadRolesGrid()
        For Each role As Role In mSelectedRoles
            gridRoles.Rows.Add(LTools.GetC(role.Name, "roleperms", "role"))
        Next
    End Sub

    Private Sub NewUsers_ListChanged(sender As Object, e As ListChangedEventArgs) Handles SelectedUsers.ListChanged
        UpdateAndRepositionUserCount()
    End Sub

    Private Sub UpdateAndRepositionUserCount()
        lblUserCount.Text = String.Format(ctlUsersSummary_TotalNumberOfUsers0, SelectedUsers.Count())
        lblUserCount.Location = New Point(gridUsers.Location.X + (gridUsers.Width - lblUserCount.Width), lblUserCount.Location.Y)
    End Sub

    Private Sub gridUsers_SelectionChanged_PreventSelectionOfRows(sender As Object, e As EventArgs) Handles gridUsers.SelectionChanged
        gridUsers.ClearSelection()
    End Sub

    Private Sub gridRoles_SelectionChanged_PreventSelectionOfRows(sender As Object, e As EventArgs) Handles gridRoles.SelectionChanged
        gridRoles.ClearSelection()
    End Sub

    Private Sub CtlUsersSummary_Layout(sender As Object, e As LayoutEventArgs) Handles MyBase.Layout
        gridUsers.PerformLayout()
        gridRoles.PerformLayout()
    End Sub

    Private Sub gridUsers_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles gridUsers.CellContentClick
        If e.ColumnIndex <> colDelete.Index Then Return

        Dim deletedUser As T = SelectedUsers.ElementAt(e.RowIndex)
        SelectedUsers.RemoveAt(e.RowIndex)
        Dim userSelectedEventArgs As New clsUserSelectedEventArgs() With {.UsersSelected = SelectedUsers.Any(), .DeletedUser = deletedUser}
        RaiseEvent OnSelectedUsersChanged(Me, userSelectedEventArgs)
    End Sub

    Private Sub gridUsers_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles gridUsers.CellFormatting
        If e.ColumnIndex = colDelete.Index Then
            e.Value = mBinIcon
            gridUsers.Rows(e.RowIndex).Cells(e.ColumnIndex).ToolTipText = Resources.ctlUsersSummary_RemoveUserFromList
        End If
    End Sub
End Class
