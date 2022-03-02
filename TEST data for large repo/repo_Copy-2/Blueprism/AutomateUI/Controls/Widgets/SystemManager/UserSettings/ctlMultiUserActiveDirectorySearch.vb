Imports AutomateControls
Imports AutomateControls.DataGridViews
Imports AutomateUI.My.Resources
Imports BluePrism.Core.ActiveDirectory.UserQuery

Friend Class ctlMultiUserActiveDirectorySearch : Inherits ActiveDirectoryUserSearchBase
    Private ReadOnly mSelectedUsersSidsAndUpns As SortableBindingList(Of NewActiveDirectoryUserViewModel)
    Private mExceededMaxUsers As Boolean = False
    Private ReadOnly mTempReadOnlyRows As List(Of String) = New List(Of String)

    Private Const MaxNumberOfSelectableUsers As Integer = 100

    Public Sub New(ByRef selectedUsers As SortableBindingList(Of NewActiveDirectoryUserViewModel))
        InitializeComponent()
        mSelectedUsersSidsAndUpns = selectedUsers
    End Sub

    Protected Overrides Sub OnChangeSelectADUser(sender As Object, e As DataGridViewCellEventArgs)
        Dim sid = dgvActiveDirectoryUsers.Rows(e.RowIndex).Cells("colSid").Value.ToString()
        Dim upn = dgvActiveDirectoryUsers.Rows(e.RowIndex).Cells("colUserPrincipalName").Value.ToString()

        dgvActiveDirectoryUsers.CommitEdit(DataGridViewDataErrorContexts.Commit)
        Dim adUser = mSelectedUsersSidsAndUpns.FirstOrDefault(Function(x) x.Sid = sid)
        If adUser IsNot Nothing
            mSelectedUsersSidsAndUpns.Remove(adUser)
        Else
            mSelectedUsersSidsAndUpns.Add(New NewActiveDirectoryUserViewModel(sid, upn))
        End If

        AllowOrPreventSelectingMoreUsersBasedOnMaxNumberSelectable(False)
        NumberOfSelectedUsersChanged()
    End Sub

    Protected Overrides Sub OnSearch(greyOutIfMapped As Boolean)
        MyBase.OnSearch(True)
        NumberOfSelectedUsersChanged()
    End Sub

    Protected Overrides Sub RecheckUserIfSelected(adUser As ActiveDirectoryUser, rowIndex As Integer)
        If mSelectedUsersSidsAndUpns.Select(Function(x) x.Sid).Contains(adUser.Sid) Then
            dgvActiveDirectoryUsers.Rows(rowIndex).Cells("colSelectUser").Value = True
        End If
    End Sub

    Private Sub NumberOfSelectedUsersChanged()
        MyBase.SelectedUserChanged(mSelectedUsersSidsAndUpns.Count > 0)
        UpdateTotalNumberOfSelectedUsersLabel()
    End Sub

    Private Sub UpdateTotalNumberOfSelectedUsersLabel()
        If dgvActiveDirectoryUsers.Rows.Count() > 0 Then
            lblTotalNumberOfSelectedUsers.Text _
                = String.Format(ActiveDirectoryUserSearch_Resources.UserSearcherSelectedUserCount0, mSelectedUsersSidsAndUpns.Count())
        End If
    End Sub

    Public Sub UpdateGridFromListOfSelectedUsers()
        mLoading = True

        For Each row As DataGridViewRow In dgvActiveDirectoryUsers.Rows
            Dim upn As String = dgvActiveDirectoryUsers.Item("colUserPrincipalName", row.Index).ToString
            If Not UpnIsInvalid(upn) Then
                Dim sid As String = dgvActiveDirectoryUsers.Item("colSid", row.Index).ToString
                Dim userIsSelected = mSelectedUsersSidsAndUpns.Select(Function(x) x.Sid).Contains(sid)
                dgvActiveDirectoryUsers.Rows(row.Index).Cells("colSelectUser").Value = userIsSelected
            End If
        Next

        If Not mPagination Is Nothing Then
            ChangeUserPage(mPagination.CurrentPageNumber)
        End If

        NumberOfSelectedUsersChanged()

        mLoading = False
    End Sub

    Protected Overrides Sub AllowOrPreventSelectingMoreUsersBasedOnMaxNumberSelectable(viewChanged As Boolean)
        If (Not mExceededMaxUsers OrElse viewChanged) AndAlso mSelectedUsersSidsAndUpns.Count >= MaxNumberOfSelectableUsers Then
            MakeUnselectedUserRowsReadOnly()
            mExceededMaxUsers = True
        ElseIf mExceededMaxUsers AndAlso mSelectedUsersSidsAndUpns.Count < MaxNumberOfSelectableUsers Then
            MakeUnselectedUserRowsSelectable()
            mExceededMaxUsers = False
        End If
    End Sub

    Private Sub MakeUnselectedUserRowsReadOnly()
        For Each row As DataGridViewRow In dgvActiveDirectoryUsers.Rows
            If Not CType(row.Cells("colSelectUser").Value, Boolean) AndAlso Not row.ReadOnly Then
                row.ReadOnly = True
                row.Cells("colSelectUser") = New DataGridViewDisableCheckBoxCell()

                mTempReadOnlyRows.Add(row.Cells("colSid").Value.ToString())
            End If
        Next
    End Sub

    Private Sub MakeUnselectedUserRowsSelectable()
        For Each row As DataGridViewRow In dgvActiveDirectoryUsers.Rows
            If mTempReadOnlyRows.Contains(row.Cells("colSid").Value.ToString()) Then
                row.ReadOnly = False
                row.Cells("colSelectUser") = New DataGridViewCheckBoxCell()
            End If
        Next

        mTempReadOnlyRows.Clear()
    End Sub
End Class
