Imports AutomateControls
Imports AutomateControls.DataGridViews
Imports AutomateUI.My.Resources
Imports BluePrism.ActiveDirectoryUserSearcher.Pagination
Imports BluePrism.ActiveDirectoryUserSearcher.Services
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib.DependencyInjection
Imports BluePrism.Core.ActiveDirectory
Imports BluePrism.Core.ActiveDirectory.DirectoryServices
Imports BluePrism.Core.ActiveDirectory.UserQuery

Friend Class ActiveDirectoryUserSearchBase : Inherits UserDetailsControl : Implements IDisposable
    Public Event OnSelectedUserChanged As UserSelectedEventHandler
    Private Const MaxUpnLength = 128

    Protected Sub SelectedUserChanged(usersSelected As Boolean)
        Dim userSelectedEventArgs As New clsUserSelectedEventArgs() With {.UsersSelected = usersSelected}
        RaiseEvent OnSelectedUserChanged(Me, userSelectedEventArgs)
    End Sub

    Private mUser As User
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Overrides Property User() As User
        Get
            Return mUser
        End Get
        Set(ByVal value As User)
            mUser = value
        End Set
    End Property

    Protected mSearching As Boolean = False
    Protected mLoading As Boolean = False

    Protected Const UsersPerPage As Integer = 30
    Protected Const MaximumPaginationLabels = 5

    Private ReadOnly mUserFilterTypes As New Dictionary(Of Integer, UserFilterType)
    Private mCredentials As DirectorySearcherCredentials

    Protected ReadOnly mPageLinkList(MaximumPaginationLabels) As LinkLabel
    Private ReadOnly mWarningIcon As Bitmap = ToolImages.Warning_16x16

    Protected mPagination As IPagination
    Friend WithEvents colUserPrincipalName As DataGridViewTextBoxColumn
    Friend WithEvents colDistinguishedName As DataGridViewTextBoxColumn
    Friend WithEvents colSelectUser As DataGridViewCheckBoxColumn
    Friend WithEvents colSid As DataGridViewTextBoxColumn
    Private mActiveDirectorySearchCredentialsForm As frmActiveDirectorySearchCredentials



    Sub New()
        MyBase.New()

        ' This call is required by the designer.
        InitializeComponent()
        pbWarningIcon.Image = mWarningIcon

        mActiveDirectorySearchCredentialsForm = New frmActiveDirectorySearchCredentials()

        lblSearchRoot.Text = ActiveDirectoryUserSearch_Resources.UserSearcherSearchRootHeader
        lblApplyFilter.Text = ActiveDirectoryUserSearch_Resources.UserSearcherApplyFilterHeader

        btnCredentials.Text = ActiveDirectoryUserSearch_Resources.UserSearcherButtonUseCredentials

        SetSearchingAsTextToCurrentUser()

        btnSearch.Text = ActiveDirectoryUserSearch_Resources.UserSearcherButtonSearch

        lblNoResultsFound.Text = ActiveDirectoryUserSearch_Resources.UserSearcherNoAdResultsReturned

        cmbFilter.Items.Add(ActiveDirectoryUserSearch_Resources.UserSearcherComboBoxCnValue)
        cmbFilter.Items.Add(ActiveDirectoryUserSearch_Resources.UserSearcherComboBoxSIDValue)
        cmbFilter.Items.Add(ActiveDirectoryUserSearch_Resources.UserSearcherComboBoxUpnValue)
        cmbFilter.SelectedIndex = 0

        dgvActiveDirectoryUsers.Columns("colUserPrincipalName").HeaderText = ActiveDirectoryUserSearch_Resources.UserSearcherUpnDataGridColumn
        dgvActiveDirectoryUsers.Columns("colDistinguishedName").HeaderText = ActiveDirectoryUserSearch_Resources.UserSearcherDistinguishedNameDataGridColumn
        dgvActiveDirectoryUsers.Columns("colSelectUser").HeaderText = ActiveDirectoryUserSearch_Resources.UserSearcherSelectUserDataGridColumn

        mPageLinkList = {llPage1, llPage2, llPage3, llPage4, llPage5}

        lblInfo.Text = String.Empty

        mUserFilterTypes.Add(0, UserFilterType.Cn)
        mUserFilterTypes.Add(1, UserFilterType.Sid)
        mUserFilterTypes.Add(2, UserFilterType.UserPrincipalName)

    End Sub

    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso Not (components Is Nothing) Then
                components.Dispose()
                mWarningIcon.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub


#Region "Event handlers"

#If Not DEBUG Then
    Private Sub frmActiveDirectoryUserSearch_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        
        tbSearchRoot.Text = DependencyResolver.FromScope(
            Function(searchService As IActiveDirectoryUserSearchService)
                Return searchService.GetDistinguishedNameOfCurrentForest()
            End Function)

    End Sub
#End If

    Protected Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click
        OnSearch(True)
        AllowOrPreventSelectingMoreUsersBasedOnMaxNumberSelectable(True)
    End Sub

    Protected Overridable Sub OnSearch(greyOutIfMapped As Boolean)
        Dim firstUserOnPage = 0
        Dim usersFound As Integer = SearchForUsers(firstUserOnPage, greyOutIfMapped)

        If usersFound > 0 Then

            mPagination = New Pagination(UsersPerPage, MaximumPaginationLabels, usersFound) With {
                .CurrentPageNumber = 1
            }

            UpdateUserCount(usersFound)
            UpdatePageLinks()

        End If
    End Sub


    Private Sub dgvActiveDirectoryUsers_CurrentCellDirtyStateChanged(sender As Object, e As EventArgs) Handles dgvActiveDirectoryUsers.CurrentCellDirtyStateChanged

        If dgvActiveDirectoryUsers.IsCurrentCellDirty Then
            dgvActiveDirectoryUsers.CommitEdit(DataGridViewDataErrorContexts.Commit)
        End If

    End Sub

    Private Sub btnCredentials_Click(sender As Object, e As EventArgs) Handles btnCredentials.Click

        Dim parent = TryCast(ParentForm, IEnvironmentColourManager)
        If parent IsNot Nothing Then
            mActiveDirectorySearchCredentialsForm.EnvironmentBackColor = parent.EnvironmentBackColor
            mActiveDirectorySearchCredentialsForm.EnvironmentForeColor = parent.EnvironmentForeColor
        End If

        mCredentials = mActiveDirectorySearchCredentialsForm.ShowInDialog(Me)
        Me.ParentForm.DialogResult = DialogResult.None

        If mCredentials Is Nothing Then
            SetSearchingAsTextToCurrentUser()
        Else
            lblSearchingAs.Text _
                = String.Format(ActiveDirectoryUserSearch_Resources.UserSearcherSearchingAs0, mCredentials.Username)

        End If

        SetQueryErrorMessageVisibility(False)

    End Sub

    Private Sub llLeftArrow_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles llLeftArrow.LinkClicked

        If mPagination.CanGoBackwards Then
            ChangeUserPage(mPagination.CurrentPageNumber - 1)
        End If

    End Sub

    Private Sub llRightArrow_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles llRightArrow.LinkClicked

        If mPagination.CanGoForwards Then
            ChangeUserPage(mPagination.CurrentPageNumber + 1)
        End If

    End Sub

    Private Sub llPage_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles llPage1.LinkClicked, llPage5.LinkClicked, llPage4.LinkClicked, llPage3.LinkClicked, llPage2.LinkClicked

        ChangeUserPage(Convert.ToInt32(CType(sender, LinkLabel).Text))

    End Sub

    Private Sub dgvActiveDirectoryUsers_CellValueChanged(sender As Object, e As DataGridViewCellEventArgs) Handles dgvActiveDirectoryUsers.CellValueChanged
        If Not mSearching AndAlso Not mLoading _
            AndAlso e.RowIndex >= 0 _
            AndAlso dgvActiveDirectoryUsers.Columns(e.ColumnIndex).Name = "colSelectUser" Then
            OnChangeSelectADUser(sender, e)

        End If
    End Sub

    Protected Overridable Sub OnChangeSelectADUser(sender As Object, e As DataGridViewCellEventArgs)
    End Sub

    Private Sub tbSearchRoot_TextChanged(sender As Object, e As EventArgs) Handles tbSearchRoot.TextChanged
        SetQueryErrorMessageVisibility(False)
    End Sub

#End Region

#Region "Private Methods"

    Protected Function SearchForUsers(firstUserOnPage As Integer, greyOutIfMapped As Boolean) As Integer
        mSearching = True
        dgvActiveDirectoryUsers.Rows.Clear()

        Dim populateGrid = True

        Dim queryResult = DependencyResolver.FromScope(
            Function(searchService As IActiveDirectoryUserSearchService)
                Return searchService.FindActiveDirectoryUsers(tbSearchRoot.Text,
                                                              mUserFilterTypes(cmbFilter.SelectedIndex),
                                                              tbSearchFilter.Text,
                                                              mCredentials, firstUserOnPage, UsersPerPage)
            End Function)
        Dim activeDirectoryQueryError = GetErrorTextFromQueryResult(queryResult)
        If Not String.IsNullOrEmpty(activeDirectoryQueryError) Then
            lblQueryError.Text = activeDirectoryQueryError
            SetQueryErrorMessageVisibility(True)
            HidePagination()

            populateGrid = False
        Else
            SetQueryErrorMessageVisibility(False)
        End If

        If queryResult.TotalUsers = 0 Then
            HidePagination()
            lblNoResultsFound.Visible = True
            lblInfo.Visible = False
            populateGrid = False
        Else
            lblInfo.Visible = True
            lblNoResultsFound.Visible = False
        End If

        If Not populateGrid Then Return 0

        For Each adUser In queryResult.RequestedPage
            Dim newRowIndex = dgvActiveDirectoryUsers.Rows.Add()

            dgvActiveDirectoryUsers.Rows(newRowIndex).Cells("colUserPrincipalName").Value = adUser.UserPrincipalName
            dgvActiveDirectoryUsers.Rows(newRowIndex).Cells("colDistinguishedName").Value = adUser.DistinguishedName
            dgvActiveDirectoryUsers.Rows(newRowIndex).Cells("colSid").Value = adUser.Sid

            If adUser.AlreadyMapped AndAlso greyOutIfMapped Then ShowGreyedOutRow(newRowIndex)

            ShowDisabledRowWithWarningIfUpnIsInvalid(adUser, newRowIndex)

            RecheckUserIfSelected(adUser, newRowIndex)
        Next

        dgvActiveDirectoryUsers.SelectedRow = Nothing
        dgvActiveDirectoryUsers.AutoResizeColumns()
        mSearching = False

        Return queryResult.TotalUsers

    End Function

    Private Sub ShowDisabledRowWithWarningIfUpnIsInvalid(adUser As ActiveDirectoryUser, newRowIndex As Integer)
        If UpnIsInvalid(adUser.UserPrincipalName) Then
            With dgvActiveDirectoryUsers.Rows(newRowIndex)
                .Cells("colSelectUser") = New DataGridViewCheckBoxWithImageCell(mWarningIcon)
                .Cells("colSelectUser").Value = False
                .DefaultCellStyle.ForeColor = Color.Gray
                .ReadOnly = True
                .Cells("colSelectUser").ToolTipText = ADUserSearch_UsersWithNoUpnOrUpnOver128CharactersCannotBeMapped
            End With
        End If
    End Sub

    Private Sub ShowGreyedOutRow(rowIndex As Integer)
        With dgvActiveDirectoryUsers.Rows(rowIndex)
            .Cells("colSelectUser") = New DataGridViewDisableCheckBoxCell()
            .DefaultCellStyle.ForeColor = Color.Gray
            .ReadOnly = True
            .Cells("colSelectUser").Value = False
        End With
    End Sub

    Protected Function UpnIsInvalid(upn As String) As Boolean
        Return String.IsNullOrEmpty(upn) OrElse upn.Length > MaxUpnLength
    End Function

    Protected Overridable Sub RecheckUserIfSelected(adUser As ActiveDirectoryUser, rowIndex As Integer)

    End Sub

    Private Sub SetQueryErrorMessageVisibility(visible As Boolean)
        pbWarningIcon.Visible = visible
        lblQueryError.Visible = visible
    End Sub

    Private Function GetErrorTextFromQueryResult(result As PaginatedUserQueryResult) As String

        Dim status As String = String.Empty

        Select Case result.Status
            Case QueryStatus.InvalidQuery
                status = ActiveDirectoryUserSearch_Resources.UserSearcherInvalidAdQuery

            Case QueryStatus.InvalidCredentials
                status = ActiveDirectoryUserSearch_Resources.UserSearcherInvalidCredentials
        End Select

        Return status

    End Function

    Protected Sub ChangeUserPage(nextPage As Integer)

        mPagination.CurrentPageNumber = nextPage
        Dim firstUserOnPage As Integer = 0

        If nextPage > 1 Then firstUserOnPage = UsersPerPage * (nextPage - 1)

        Dim usersFound As Integer = SearchForUsers(firstUserOnPage, True)

        If usersFound > 0 Then
            UpdateUserCount(usersFound)
            UpdatePageLinks()
        End If
        AllowOrPreventSelectingMoreUsersBasedOnMaxNumberSelectable(True)

    End Sub

    Protected Overridable Sub AllowOrPreventSelectingMoreUsersBasedOnMaxNumberSelectable(viewChanged As Boolean)
    End Sub

    Protected Sub UpdateUserCount(numberOfUsersFound As Integer)

        If numberOfUsersFound > 1 Then
            lblInfo.Text = $"{numberOfUsersFound} {ActiveDirectoryUserSearch_Resources.UserSearcherSearchResultsPlural}"
        Else
            lblInfo.Text = $"{numberOfUsersFound} {ActiveDirectoryUserSearch_Resources.UserSearcherSearchResultsSingular}"
        End If

    End Sub

    Protected Sub UpdatePageLinks()

        llLeftArrow.Visible = True
        llLeftArrow.Enabled = mPagination.CanGoBackwards

        For i = 0 To MaximumPaginationLabels - 1

            If mPagination.VisiblePageNumbers.Count > i Then

                mPageLinkList(i).Enabled = True
                mPageLinkList(i).Visible = True
                mPageLinkList(i).Text = mPagination.VisiblePageNumbers(i).ToString()

                If mPageLinkList(i).Text = mPagination.CurrentPageNumber.ToString() Then
                    mPageLinkList(i).LinkColor = SystemColors.Highlight
                    mPageLinkList(i).Font = New Font(mPageLinkList(i).Font, FontStyle.Bold)
                Else
                    mPageLinkList(i).LinkColor = SystemColors.ControlText
                    mPageLinkList(i).Font = New Font(mPageLinkList(i).Font, FontStyle.Regular)
                End If

            Else

                mPageLinkList(i).Enabled = False
                mPageLinkList(i).Visible = False

            End If
        Next

        llRightArrow.Visible = True
        llRightArrow.Enabled = mPagination.CanGoForwards

    End Sub

    Private Sub HidePagination()

        For i = 0 To MaximumPaginationLabels - 1
            mPageLinkList(i).Enabled = False
            mPageLinkList(i).Visible = False
        Next

        llLeftArrow.Enabled = False
        llLeftArrow.Visible = False
        llRightArrow.Enabled = False
        llRightArrow.Visible = False

    End Sub

    Private Sub SetSearchingAsTextToCurrentUser()
        lblSearchingAs.Text _
                   = String.Format(ActiveDirectoryUserSearch_Resources.UserSearcherSearchingAs0, ActiveDirectoryUserSearch_Resources.UserSearcherSearchingAsCurrentUser)
    End Sub
#End Region

#Region "Overrides"
    Public Overrides Function AllFieldsValid() As Boolean
    End Function
#End Region

    Private WithEvents pnlSearch As Panel
    Protected WithEvents cmbFilter As AutomateControls.ReadOnlyComboBox
    Private WithEvents tbSearchRoot As AutomateControls.Textboxes.StyledTextBox
    Protected WithEvents btnSearch As AutomateControls.Buttons.StandardStyledButton
    Private components As IContainer
    Protected WithEvents tbSearchFilter As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents btnCredentials As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents lblApplyFilter As Label
    Protected WithEvents dgvActiveDirectoryUsers As AutomateControls.DataGridViews.DragAndDroppableDataGridView
    Friend WithEvents FlowLayoutPanel1 As FlowLayoutPanel
    Friend WithEvents llLeftArrow As LinkLabel
    Friend WithEvents llPage1 As LinkLabel
    Friend WithEvents llPage2 As LinkLabel
    Friend WithEvents llPage3 As LinkLabel
    Friend WithEvents llPage4 As LinkLabel
    Friend WithEvents llPage5 As LinkLabel
    Friend WithEvents llRightArrow As LinkLabel
    Friend WithEvents lblInfo As Label
    Private WithEvents lblSearchRoot As Label
    Friend WithEvents lblSearchingAs As Label
    Friend WithEvents lblQueryError As Label
    Private WithEvents pbWarningIcon As PictureBox
    Friend WithEvents lblNoResultsFound As Label

    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim ColumnHeaderDataGridViewCellStyle As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.pnlSearch = New System.Windows.Forms.Panel()
        Me.pbWarningIcon = New System.Windows.Forms.PictureBox()
        Me.lblQueryError = New System.Windows.Forms.Label()
        Me.lblSearchingAs = New System.Windows.Forms.Label()
        Me.lblApplyFilter = New System.Windows.Forms.Label()
        Me.lblSearchRoot = New System.Windows.Forms.Label()
        Me.FlowLayoutPanel1 = New System.Windows.Forms.FlowLayoutPanel()
        Me.llLeftArrow = New System.Windows.Forms.LinkLabel()
        Me.llPage1 = New System.Windows.Forms.LinkLabel()
        Me.llPage2 = New System.Windows.Forms.LinkLabel()
        Me.llPage3 = New System.Windows.Forms.LinkLabel()
        Me.llPage4 = New System.Windows.Forms.LinkLabel()
        Me.llPage5 = New System.Windows.Forms.LinkLabel()
        Me.llRightArrow = New System.Windows.Forms.LinkLabel()
        Me.lblInfo = New System.Windows.Forms.Label()
        Me.lblNoResultsFound = New System.Windows.Forms.Label()
        Me.dgvActiveDirectoryUsers = New AutomateControls.DataGridViews.DragAndDroppableDataGridView()
        Me.cmbFilter = New AutomateControls.ReadOnlyComboBox()
        Me.tbSearchRoot = New AutomateControls.Textboxes.StyledTextBox()
        Me.btnSearch = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.tbSearchFilter = New AutomateControls.Textboxes.StyledTextBox()
        Me.btnCredentials = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.colUserPrincipalName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colDistinguishedName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colSelectUser = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.colSid = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.pnlSearch.SuspendLayout()
        CType(Me.pbWarningIcon, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.FlowLayoutPanel1.SuspendLayout()
        CType(Me.dgvActiveDirectoryUsers, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'pnlSearch
        '
        Me.pnlSearch.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.pnlSearch.Controls.Add(Me.pbWarningIcon)
        Me.pnlSearch.Controls.Add(Me.lblQueryError)
        Me.pnlSearch.Controls.Add(Me.lblSearchingAs)
        Me.pnlSearch.Controls.Add(Me.cmbFilter)
        Me.pnlSearch.Controls.Add(Me.tbSearchRoot)
        Me.pnlSearch.Controls.Add(Me.btnSearch)
        Me.pnlSearch.Controls.Add(Me.tbSearchFilter)
        Me.pnlSearch.Controls.Add(Me.btnCredentials)
        Me.pnlSearch.Controls.Add(Me.lblApplyFilter)
        Me.pnlSearch.Controls.Add(Me.lblSearchRoot)
        Me.pnlSearch.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlSearch.Location = New System.Drawing.Point(0, 0)
        Me.pnlSearch.Name = "pnlSearch"
        Me.pnlSearch.Size = New System.Drawing.Size(724, 152)
        Me.pnlSearch.TabIndex = 3
        '
        'pbWarningIcon
        '
        Me.pbWarningIcon.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.pbWarningIcon.Location = New System.Drawing.Point(16, 80)
        Me.pbWarningIcon.Margin = New System.Windows.Forms.Padding(0)
        Me.pbWarningIcon.Name = "pbWarningIcon"
        Me.pbWarningIcon.Size = New System.Drawing.Size(16, 16)
        Me.pbWarningIcon.TabIndex = 43
        Me.pbWarningIcon.TabStop = False
        Me.pbWarningIcon.Visible = False
        '
        'lblQueryError
        '
        Me.lblQueryError.AutoSize = True
        Me.lblQueryError.Location = New System.Drawing.Point(45, 80)
        Me.lblQueryError.Name = "lblQueryError"
        Me.lblQueryError.Size = New System.Drawing.Size(65, 13)
        Me.lblQueryError.TabIndex = 5
        Me.lblQueryError.Text = "invalidQuery"
        Me.lblQueryError.Visible = False
        '
        'lblSearchingAs
        '
        Me.lblSearchingAs.AutoSize = True
        Me.lblSearchingAs.Location = New System.Drawing.Point(13, 61)
        Me.lblSearchingAs.Name = "lblSearchingAs"
        Me.lblSearchingAs.Size = New System.Drawing.Size(134, 13)
        Me.lblSearchingAs.TabIndex = 4
        Me.lblSearchingAs.Text = "Searching as: Current User"
        '
        'lblApplyFilter
        '
        Me.lblApplyFilter.AutoSize = True
        Me.lblApplyFilter.Location = New System.Drawing.Point(13, 101)
        Me.lblApplyFilter.Name = "lblApplyFilter"
        Me.lblApplyFilter.Size = New System.Drawing.Size(55, 13)
        Me.lblApplyFilter.TabIndex = 3
        Me.lblApplyFilter.Text = "Apply filter"
        '
        'lblSearchRoot
        '
        Me.lblSearchRoot.AutoSize = True
        Me.lblSearchRoot.Location = New System.Drawing.Point(13, 12)
        Me.lblSearchRoot.Name = "lblSearchRoot"
        Me.lblSearchRoot.Size = New System.Drawing.Size(62, 13)
        Me.lblSearchRoot.TabIndex = 2
        Me.lblSearchRoot.Text = "Search root"
        '
        'FlowLayoutPanel1
        '
        Me.FlowLayoutPanel1.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.FlowLayoutPanel1.Controls.Add(Me.llLeftArrow)
        Me.FlowLayoutPanel1.Controls.Add(Me.llPage1)
        Me.FlowLayoutPanel1.Controls.Add(Me.llPage2)
        Me.FlowLayoutPanel1.Controls.Add(Me.llPage3)
        Me.FlowLayoutPanel1.Controls.Add(Me.llPage4)
        Me.FlowLayoutPanel1.Controls.Add(Me.llPage5)
        Me.FlowLayoutPanel1.Controls.Add(Me.llRightArrow)
        Me.FlowLayoutPanel1.Location = New System.Drawing.Point(16, 398)
        Me.FlowLayoutPanel1.Name = "FlowLayoutPanel1"
        Me.FlowLayoutPanel1.Size = New System.Drawing.Size(186, 24)
        Me.FlowLayoutPanel1.TabIndex = 18
        '
        'llLeftArrow
        '
        Me.llLeftArrow.ActiveLinkColor = System.Drawing.Color.Red
        Me.llLeftArrow.Anchor = System.Windows.Forms.AnchorStyles.Bottom
        Me.llLeftArrow.AutoSize = True
        Me.llLeftArrow.Enabled = False
        Me.llLeftArrow.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.llLeftArrow.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline
        Me.llLeftArrow.LinkColor = System.Drawing.SystemColors.Highlight
        Me.llLeftArrow.Location = New System.Drawing.Point(3, 0)
        Me.llLeftArrow.Name = "llLeftArrow"
        Me.llLeftArrow.Padding = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.llLeftArrow.Size = New System.Drawing.Size(20, 16)
        Me.llLeftArrow.TabIndex = 14
        Me.llLeftArrow.TabStop = True
        Me.llLeftArrow.Text = "<"
        Me.llLeftArrow.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.llLeftArrow.Visible = False
        '
        'llPage1
        '
        Me.llPage1.ActiveLinkColor = System.Drawing.Color.Red
        Me.llPage1.AutoSize = True
        Me.llPage1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.llPage1.Enabled = False
        Me.llPage1.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline
        Me.llPage1.LinkColor = System.Drawing.SystemColors.ControlText
        Me.llPage1.Location = New System.Drawing.Point(29, 0)
        Me.llPage1.Name = "llPage1"
        Me.llPage1.Padding = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.llPage1.Size = New System.Drawing.Size(17, 16)
        Me.llPage1.TabIndex = 9
        Me.llPage1.TabStop = True
        Me.llPage1.Text = "1"
        Me.llPage1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.llPage1.Visible = False
        '
        'llPage2
        '
        Me.llPage2.ActiveLinkColor = System.Drawing.Color.Red
        Me.llPage2.AutoSize = True
        Me.llPage2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.llPage2.Enabled = False
        Me.llPage2.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline
        Me.llPage2.LinkColor = System.Drawing.SystemColors.ControlText
        Me.llPage2.Location = New System.Drawing.Point(52, 0)
        Me.llPage2.Name = "llPage2"
        Me.llPage2.Padding = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.llPage2.Size = New System.Drawing.Size(17, 16)
        Me.llPage2.TabIndex = 10
        Me.llPage2.TabStop = True
        Me.llPage2.Text = "2"
        Me.llPage2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.llPage2.Visible = False
        '
        'llPage3
        '
        Me.llPage3.ActiveLinkColor = System.Drawing.Color.Red
        Me.llPage3.AutoSize = True
        Me.llPage3.Dock = System.Windows.Forms.DockStyle.Fill
        Me.llPage3.Enabled = False
        Me.llPage3.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline
        Me.llPage3.LinkColor = System.Drawing.SystemColors.ControlText
        Me.llPage3.Location = New System.Drawing.Point(75, 0)
        Me.llPage3.Name = "llPage3"
        Me.llPage3.Padding = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.llPage3.Size = New System.Drawing.Size(17, 16)
        Me.llPage3.TabIndex = 11
        Me.llPage3.TabStop = True
        Me.llPage3.Text = "3"
        Me.llPage3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.llPage3.Visible = False
        '
        'llPage4
        '
        Me.llPage4.ActiveLinkColor = System.Drawing.Color.Red
        Me.llPage4.AutoSize = True
        Me.llPage4.Dock = System.Windows.Forms.DockStyle.Fill
        Me.llPage4.Enabled = False
        Me.llPage4.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline
        Me.llPage4.LinkColor = System.Drawing.SystemColors.ControlText
        Me.llPage4.Location = New System.Drawing.Point(98, 0)
        Me.llPage4.Name = "llPage4"
        Me.llPage4.Padding = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.llPage4.Size = New System.Drawing.Size(17, 16)
        Me.llPage4.TabIndex = 12
        Me.llPage4.TabStop = True
        Me.llPage4.Text = "4"
        Me.llPage4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.llPage4.Visible = False
        '
        'llPage5
        '
        Me.llPage5.ActiveLinkColor = System.Drawing.Color.Red
        Me.llPage5.AutoSize = True
        Me.llPage5.Dock = System.Windows.Forms.DockStyle.Fill
        Me.llPage5.Enabled = False
        Me.llPage5.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline
        Me.llPage5.LinkColor = System.Drawing.SystemColors.ControlText
        Me.llPage5.Location = New System.Drawing.Point(121, 0)
        Me.llPage5.Name = "llPage5"
        Me.llPage5.Padding = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.llPage5.Size = New System.Drawing.Size(17, 16)
        Me.llPage5.TabIndex = 13
        Me.llPage5.TabStop = True
        Me.llPage5.Text = "5"
        Me.llPage5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.llPage5.Visible = False
        '
        'llRightArrow
        '
        Me.llRightArrow.ActiveLinkColor = System.Drawing.Color.Red
        Me.llRightArrow.Anchor = System.Windows.Forms.AnchorStyles.Bottom
        Me.llRightArrow.AutoSize = True
        Me.llRightArrow.Enabled = False
        Me.llRightArrow.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.llRightArrow.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline
        Me.llRightArrow.LinkColor = System.Drawing.SystemColors.Highlight
        Me.llRightArrow.Location = New System.Drawing.Point(144, 0)
        Me.llRightArrow.Name = "llRightArrow"
        Me.llRightArrow.Padding = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.llRightArrow.Size = New System.Drawing.Size(20, 16)
        Me.llRightArrow.TabIndex = 15
        Me.llRightArrow.TabStop = True
        Me.llRightArrow.Text = ">"
        Me.llRightArrow.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.llRightArrow.Visible = False
        '
        'lblInfo
        '
        Me.lblInfo.Anchor = System.Windows.Forms.AnchorStyles.Bottom
        Me.lblInfo.AutoSize = True
        Me.lblInfo.Location = New System.Drawing.Point(310, 398)
        Me.lblInfo.Name = "lblInfo"
        Me.lblInfo.Size = New System.Drawing.Size(55, 13)
        Me.lblInfo.TabIndex = 16
        Me.lblInfo.Text = "status text"
        Me.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblNoResultsFound
        '
        Me.lblNoResultsFound.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblNoResultsFound.AutoSize = True
        Me.lblNoResultsFound.Location = New System.Drawing.Point(13, 398)
        Me.lblNoResultsFound.Name = "lblNoResultsFound"
        Me.lblNoResultsFound.Size = New System.Drawing.Size(39, 13)
        Me.lblNoResultsFound.TabIndex = 19
        Me.lblNoResultsFound.Text = "Label1"
        Me.lblNoResultsFound.Visible = False
        '
        'dgvActiveDirectoryUsers
        '
        Me.dgvActiveDirectoryUsers.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgvActiveDirectoryUsers.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.ColumnHeader
        ColumnHeaderDataGridViewCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        ColumnHeaderDataGridViewCellStyle.BackColor = System.Drawing.SystemColors.Control
        ColumnHeaderDataGridViewCellStyle.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        ColumnHeaderDataGridViewCellStyle.ForeColor = System.Drawing.SystemColors.WindowText
        ColumnHeaderDataGridViewCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight
        ColumnHeaderDataGridViewCellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        ColumnHeaderDataGridViewCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.dgvActiveDirectoryUsers.ColumnHeadersDefaultCellStyle = ColumnHeaderDataGridViewCellStyle
        Me.dgvActiveDirectoryUsers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvActiveDirectoryUsers.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colUserPrincipalName, Me.colDistinguishedName, Me.colSelectUser, Me.colSid})
        Me.dgvActiveDirectoryUsers.Location = New System.Drawing.Point(16, 158)
        Me.dgvActiveDirectoryUsers.MultiSelect = False
        Me.dgvActiveDirectoryUsers.Name = "dgvActiveDirectoryUsers"
        Me.dgvActiveDirectoryUsers.Size = New System.Drawing.Size(687, 234)
        Me.dgvActiveDirectoryUsers.SkipReadOnlyRows = True
        Me.dgvActiveDirectoryUsers.TabIndex = 4
        Me.dgvActiveDirectoryUsers.TabStop = False
        '
        'cmbFilter
        '
        Me.cmbFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbFilter.FormattingEnabled = True
        Me.cmbFilter.Location = New System.Drawing.Point(16, 121)
        Me.cmbFilter.Name = "cmbFilter"
        Me.cmbFilter.ReadOnly = False
        Me.cmbFilter.Size = New System.Drawing.Size(57, 21)
        Me.cmbFilter.TabIndex = 1
        '
        'tbSearchRoot
        '
        Me.tbSearchRoot.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.tbSearchRoot.BorderColor = System.Drawing.Color.Empty
        Me.tbSearchRoot.Location = New System.Drawing.Point(16, 32)
        Me.tbSearchRoot.Name = "tbSearchRoot"
        Me.tbSearchRoot.Size = New System.Drawing.Size(687, 20)
        Me.tbSearchRoot.TabIndex = 3
        Me.tbSearchRoot.TabStop = False
        Me.tbSearchRoot.Text = "DC="
        '
        'btnSearch
        '
        Me.btnSearch.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnSearch.Location = New System.Drawing.Point(620, 120)
        Me.btnSearch.Name = "btnSearch"
        Me.btnSearch.Size = New System.Drawing.Size(85, 23)
        Me.btnSearch.TabIndex = 3
        Me.btnSearch.Text = "Search"
        Me.btnSearch.UseVisualStyleBackColor = False
        '
        'tbSearchFilter
        '
        Me.tbSearchFilter.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.tbSearchFilter.BorderColor = System.Drawing.Color.Empty
        Me.tbSearchFilter.Location = New System.Drawing.Point(79, 121)
        Me.tbSearchFilter.Name = "tbSearchFilter"
        Me.tbSearchFilter.Size = New System.Drawing.Size(536, 20)
        Me.tbSearchFilter.TabIndex = 2
        '
        'btnCredentials
        '
        Me.btnCredentials.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnCredentials.AutoSize = True
        Me.btnCredentials.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCredentials.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnCredentials.Location = New System.Drawing.Point(495, 58)
        Me.btnCredentials.Name = "btnCredentials"
        Me.btnCredentials.Size = New System.Drawing.Size(209, 23)
        Me.btnCredentials.TabIndex = 0
        Me.btnCredentials.Text = "Search with other credentials"
        Me.btnCredentials.UseVisualStyleBackColor = False
        '
        'colUserPrincipalName
        '
        Me.colUserPrincipalName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader
        Me.colUserPrincipalName.HeaderText = "Active Directory name (UPN)"
        Me.colUserPrincipalName.MinimumWidth = 230
        Me.colUserPrincipalName.Name = "colUserPrincipalName"
        Me.colUserPrincipalName.ReadOnly = True
        Me.colUserPrincipalName.Width = 230
        '
        'colDistinguishedName
        '
        Me.colDistinguishedName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colDistinguishedName.FillWeight = 70.0!
        Me.colDistinguishedName.HeaderText = "Distinguished Name"
        Me.colDistinguishedName.MinimumWidth = 250
        Me.colDistinguishedName.Name = "colDistinguishedName"
        Me.colDistinguishedName.ReadOnly = True
        Me.colDistinguishedName.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        '
        'colSelectUser
        '
        Me.colSelectUser.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells
        Me.colSelectUser.HeaderText = "Select User"
        Me.colSelectUser.MinimumWidth = 70
        Me.colSelectUser.Name = "colSelectUser"
        Me.colSelectUser.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.colSelectUser.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        Me.colSelectUser.Width = 87
        '
        'colSid
        '
        Me.colSid.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader
        Me.colSid.HeaderText = "SID"
        Me.colSid.Name = "colSid"
        Me.colSid.ReadOnly = True
        Me.colSid.Visible = False
        Me.colSid.Width = 50
        '
        'ActiveDirectoryUserSearchBase
        '
        Me.Controls.Add(Me.lblNoResultsFound)
        Me.Controls.Add(Me.FlowLayoutPanel1)
        Me.Controls.Add(Me.dgvActiveDirectoryUsers)
        Me.Controls.Add(Me.pnlSearch)
        Me.Controls.Add(Me.lblInfo)
        Me.MinimumSize = New System.Drawing.Size(400, 350)
        Me.Name = "ActiveDirectoryUserSearchBase"
        Me.Size = New System.Drawing.Size(724, 427)
        Me.pnlSearch.ResumeLayout(False)
        Me.pnlSearch.PerformLayout()
        CType(Me.pbWarningIcon, System.ComponentModel.ISupportInitialize).EndInit()
        Me.FlowLayoutPanel1.ResumeLayout(False)
        Me.FlowLayoutPanel1.PerformLayout()
        CType(Me.dgvActiveDirectoryUsers, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout

End Sub


End Class
