Imports AutomateUI.My.Resources
Imports BluePrism.AutomateAppCore.Auth

Friend Class CtlSingleUserActiveDirectorySearch : Inherits ActiveDirectoryUserSearchBase

    Private mTickBoxEventFrozen As Boolean = False

    Private mUserSelected As Boolean = False

    Private mSelectedUserSID As String = String.Empty
    Public ReadOnly Property SelectedUserSid As String
        Get
            Return mSelectedUserSID
        End Get
    End Property

    Private mSelectedUserUserPrincipalName As String = String.Empty
    Public ReadOnly Property SelectedUserUserPrincipalName As String
        Get
            Return mSelectedUserUserPrincipalName
        End Get
    End Property

    Public Sub New()

        InitializeComponent()
        btnCancel.Text = ActiveDirectoryUserSearch_Resources.UserSearcherButtonCancel
        btnAdd.Text = ActiveDirectoryUserSearch_Resources.UserSearcherButtonOK
        btnAdd.Enabled = False
    End Sub

    Public Sub FreezeUserDetails(user As User)
        Me.User = user
        cmbFilter.SelectedItem = ActiveDirectoryUserSearch_Resources.UserSearcherComboBoxSIDValue
        cmbFilter.Enabled = False

        tbSearchFilter.Text = user.ExternalId
        tbSearchFilter.Enabled = False
    End Sub

    Protected Overrides Sub OnSearch(greyOutIfMapped As Boolean)

        MyBase.OnSearch(False)

        btnAdd.Enabled = False
        mUserSelected = False
        mSelectedUserUserPrincipalName = String.Empty
        mSelectedUserSID = String.Empty
        mPageLinkList(0).Enabled = False

 
    End Sub

    Protected Overrides Sub OnChangeSelectADUser(sender As Object, e As DataGridViewCellEventArgs)

        If Not mTickBoxEventFrozen Then
            dgvActiveDirectoryUsers.CommitEdit(DataGridViewDataErrorContexts.Commit)

            If mUserSelected Then ClearAnySelectedFlag()

            Dim row As DataGridViewRow = dgvActiveDirectoryUsers.Rows(e.RowIndex)
            If Convert.ToBoolean(row.Cells("colSelectUser").Value) Then
                mSelectedUserSID = dgvActiveDirectoryUsers.Rows(e.RowIndex).Cells("colSid").Value.ToString()
                mSelectedUserUserPrincipalName = dgvActiveDirectoryUsers.Rows(e.RowIndex).Cells("colUserPrincipalName").Value.ToString()
                mUserSelected = True
            Else
                mSelectedUserSID = String.Empty
                mSelectedUserUserPrincipalName = String.Empty
                mUserSelected = False
            End If

            btnAdd.Enabled = mUserSelected
        End If

    End Sub


    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click

        mSelectedUserUserPrincipalName = String.Empty
        mSelectedUserSID = String.Empty

        Me.ParentForm.DialogResult = DialogResult.Cancel

    End Sub

    Private Sub btnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click

        If Not mUserSelected Then
            Me.ParentForm.DialogResult = DialogResult.Cancel
        Else
            Me.ParentForm.DialogResult = DialogResult.OK
        End If

    End Sub

    Private Sub ClearAnySelectedFlag()

        For Each row As DataGridViewRow In dgvActiveDirectoryUsers.Rows
            If row.Cells("colSid").Value.ToString = mSelectedUserSID Then
                row.Cells("colSelectUser").Value = False
            End If
        Next

    End Sub

End Class
