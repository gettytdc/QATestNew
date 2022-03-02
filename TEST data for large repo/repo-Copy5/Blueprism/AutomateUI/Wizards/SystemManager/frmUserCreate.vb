Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.Server.Domain.Models

''' <summary>
''' A wizard to create a new user.
''' </summary>
Friend Class frmUserCreate : Inherits frmWizard : Implements IPermission

#Region " Windows Form Designer generated code "

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso Not (components Is Nothing) Then
            components.Dispose()
        End If

        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer
    Private WithEvents mExpiryControl As AutomateUI.ctlUserSettingsExpiry
    Private WithEvents pnlUser As Panel
    Private WithEvents pnlExpiryDates As System.Windows.Forms.Panel
    Private WithEvents pnlRolesAndPermissions As System.Windows.Forms.Panel
    Friend WithEvents pnlAuthType As Panel
    Friend WithEvents pnlUsersSummary As Panel
    Friend WithEvents pnlWaitSpinner As Panel
    Friend WithEvents picCreateUsersSpinner As PictureBox

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Private WithEvents mAuthControl As AutomateUI.ctlAuth
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmUserCreate))
        Dim lblCreatingUsers As System.Windows.Forms.Label
        Me.mExpiryControl = New AutomateUI.ctlUserSettingsExpiry()
        Me.mAuthControl = New AutomateUI.ctlAuth()
        Me.pnlUser = New System.Windows.Forms.Panel()
        Me.pnlExpiryDates = New System.Windows.Forms.Panel()
        Me.pnlAuthType = New System.Windows.Forms.Panel()
        Me.pnlRolesAndPermissions = New System.Windows.Forms.Panel()
        Me.pnlUsersSummary = New System.Windows.Forms.Panel()
        Me.pnlWaitSpinner = New System.Windows.Forms.Panel()
        Me.picCreateUsersSpinner = New System.Windows.Forms.PictureBox()
        lblCreatingUsers = New System.Windows.Forms.Label()
        Me.pnlExpiryDates.SuspendLayout()
        Me.pnlRolesAndPermissions.SuspendLayout()
        Me.pnlWaitSpinner.SuspendLayout()
        CType(Me.picCreateUsersSpinner, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        '
        'btnNext
        '
        resources.ApplyResources(Me.btnNext, "btnNext")
        '
        'btnBack
        '
        Me.btnBack.BackColor = System.Drawing.SystemColors.Control
        Me.btnBack.ForeColor = System.Drawing.SystemColors.ControlText
        resources.ApplyResources(Me.btnBack, "btnBack")
        '
        'objBluebar
        '
        resources.ApplyResources(Me.objBluebar, "objBluebar")
        '
        'lblCreatingUsers
        '
        resources.ApplyResources(lblCreatingUsers, "lblCreatingUsers")
        lblCreatingUsers.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        lblCreatingUsers.Name = "lblCreatingUsers"
        '
        'mExpiryControl
        '
        resources.ApplyResources(Me.mExpiryControl, "mExpiryControl")
        Me.mExpiryControl.Name = "mExpiryControl"
        Me.mExpiryControl.User = Nothing
        '
        'mAuthControl
        '
        resources.ApplyResources(Me.mAuthControl, "mAuthControl")
        Me.mAuthControl.EditMode = AutomateUI.AuthEditMode.ManageUser
        Me.mAuthControl.Name = "mAuthControl"
        '
        'pnlUser
        '
        resources.ApplyResources(Me.pnlUser, "pnlUser")
        Me.pnlUser.Name = "pnlUser"
        '
        'pnlExpiryDates
        '
        Me.pnlExpiryDates.Controls.Add(Me.mExpiryControl)
        resources.ApplyResources(Me.pnlExpiryDates, "pnlExpiryDates")
        Me.pnlExpiryDates.Name = "pnlExpiryDates"
        '
        'pnlAuthType
        '
        resources.ApplyResources(Me.pnlAuthType, "pnlAuthType")
        Me.pnlAuthType.Name = "pnlAuthType"
        '
        'pnlRolesAndPermissions
        '
        Me.pnlRolesAndPermissions.Controls.Add(Me.mAuthControl)
        resources.ApplyResources(Me.pnlRolesAndPermissions, "pnlRolesAndPermissions")
        Me.pnlRolesAndPermissions.Name = "pnlRolesAndPermissions"
        '
        'pnlUsersSummary
        '
        resources.ApplyResources(Me.pnlUsersSummary, "pnlUsersSummary")
        Me.pnlUsersSummary.Name = "pnlUsersSummary"
        '
        'pnlWaitSpinner
        '
        resources.ApplyResources(Me.pnlWaitSpinner, "pnlWaitSpinner")
        Me.pnlWaitSpinner.BackColor = System.Drawing.Color.Transparent
        Me.pnlWaitSpinner.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlWaitSpinner.Controls.Add(lblCreatingUsers)
        Me.pnlWaitSpinner.Controls.Add(Me.picCreateUsersSpinner)
        Me.pnlWaitSpinner.Name = "pnlWaitSpinner"
        '
        'picCreateUsersSpinner
        '
        resources.ApplyResources(Me.picCreateUsersSpinner, "picCreateUsersSpinner")
        Me.picCreateUsersSpinner.Image = Global.AutomateUI.My.Resources.Resources.preloader
        Me.picCreateUsersSpinner.Name = "picCreateUsersSpinner"
        Me.picCreateUsersSpinner.TabStop = False
        '
        'frmUserCreate
        '
        resources.ApplyResources(Me, "$this")
        Me.CancelButton = Nothing
        Me.Controls.Add(Me.pnlWaitSpinner)
        Me.Controls.Add(Me.pnlUsersSummary)
        Me.Controls.Add(Me.pnlAuthType)
        Me.Controls.Add(Me.pnlUser)
        Me.Controls.Add(Me.pnlExpiryDates)
        Me.Controls.Add(Me.pnlRolesAndPermissions)
        Me.Name = "frmUserCreate"
        Me.Title = "Assign a username and attributes to a new Blue Prism user"
        Me.Controls.SetChildIndex(Me.pnlRolesAndPermissions, 0)
        Me.Controls.SetChildIndex(Me.pnlExpiryDates, 0)
        Me.Controls.SetChildIndex(Me.pnlUser, 0)
        Me.Controls.SetChildIndex(Me.objBluebar, 0)
        Me.Controls.SetChildIndex(Me.btnBack, 0)
        Me.Controls.SetChildIndex(Me.btnNext, 0)
        Me.Controls.SetChildIndex(Me.btnCancel, 0)
        Me.Controls.SetChildIndex(Me.pnlAuthType, 0)
        Me.Controls.SetChildIndex(Me.pnlUsersSummary, 0)
        Me.Controls.SetChildIndex(Me.pnlWaitSpinner, 0)
        Me.pnlExpiryDates.ResumeLayout(False)
        Me.pnlRolesAndPermissions.ResumeLayout(False)
        Me.pnlWaitSpinner.ResumeLayout(False)
        Me.pnlWaitSpinner.PerformLayout()
        CType(Me.picCreateUsersSpinner, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

    Private mAuthType As UserAuthType
    Private mUser As User
    Private mUserDetails As UserDetailsControl
    Private mAdUsersSummary As CtlUsersSummary(Of NewActiveDirectoryUserViewModel)
    Private ReadOnly mSelectedUsersSidsAndUpns As New SortableBindingList(Of NewActiveDirectoryUserViewModel)
    Private mMultiUserActiveDirectorySearch As ctlMultiUserActiveDirectorySearch

    Public Sub New()
        Me.New(AuthMode.Native)
    End Sub

    Public Sub New(authMode As AuthMode)
        MyBase.New()
        InitializeComponent()
        SetUpEditControls(authMode)
        SetMaxStepsForUserType()
    End Sub

    Private Sub SetUpEditControls(authMode As AuthMode)

        If UIUtil.IsInVisualStudio Then Return

        Dim passwordRules As PasswordRules = New PasswordRules()
        Dim logonOptions As LogonOptions = New LogonOptions()
        gSv.GetSignonSettings(passwordRules, logonOptions)

        mAuthType = New UserAuthType(authMode,
            True,
            logonOptions.MappedActiveDirectoryAuthenticationEnabled) With {
                .Dock = DockStyle.Fill
        }

        pnlAuthType.Controls.Add(mAuthType)
    End Sub

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName("System Manager")
        End Get
    End Property
    Public Property Group As IGroup
    Protected Overrides Sub BackPage()
        MyBase.BackPage()
    End Sub
    Protected Overrides Sub UpdatePage()
        Dim currStep = GetStep()

        If mAuthType.AuthTypeCount = 1 Then
            ' skip the first page if only one auth type to choose from
            currStep += 1
        End If

        SetMaxStepsForUserType()

        Select Case currStep
            Case 0
                ShowPage(pnlAuthType, False)
                Title = My.Resources.frmUserCreate_SelectAuthType
                mAuthType.Focus()
            Case 1, 2, 3, 4
                If mAuthType.GetSelectedAuthType() = AuthMode.MappedActiveDirectory Then
                    ActiveDirectoryUserWizard(currStep)
                ElseIf mAuthType.GetSelectedAuthType() = AuthMode.Native Then
                    NativeUserWizard(currStep)
                End If
        End Select
    End Sub

    Private Sub SetMaxStepsForUserType()
        If mAuthType.GetSelectedAuthType() = AuthMode.MappedActiveDirectory Then
            SetMaxSteps(3)
        Else
            SetMaxSteps(If(mAuthType.AuthTypeCount = 1, 2, 3))
        End If
    End Sub

    Private Sub ActiveDirectoryUserWizard(currStep As Integer)
        Select Case currStep
            Case 1
                If IsNewUserOrChangeOfUserAuth(mAuthControl.User, mAuthType.GetSelectedAuthType()) Then
                    mAuthControl.User =
                        User.CreateEmptyMappedActiveDirectoryUser(Guid.NewGuid())
                End If
                Title = My.Resources.ctlSecurityOptions_AssignRolesAndPermissions
                ShowPage(pnlRolesAndPermissions, False)
                mAuthControl.Focus()
                btnNext.Enabled = True
            Case 2
                Title = My.Resources.frmUserCreate_SearchActiveDirectory
                CreateUserDetails()
                ShowPage(pnlUser, False)
                mUserDetails.Focus()
            Case 3
                Title = My.Resources.frmUserCreate_CreateActiveDirectoryUsers
                CreateActiveDirectoryUserSummary()
                ShowPage(pnlUsersSummary, False)
                btnNext.Text = My.Resources.frmUserCreate_CreateButton
            Case 4
                If mUserDetails.AllFieldsValid() Then
                    NextPage()
                Else
                    Rollback(1)
                End If
                CreateActiveDirectoryUsers()
        End Select
    End Sub

    Private Sub NativeUserWizard(currStep As Integer)
        Select Case currStep
            Case 1
                CreateUserDetails()
                ShowPage(pnlUser, False)
                mUserDetails.Focus()
            Case 2
                If Not mUserDetails.AllFieldsValid() Then
                    Rollback(1)
                    Return
                End If

                If mUser.AuthType = AuthMode.Native Then
                    ShowPage(pnlExpiryDates, False)
                    mExpiryControl.Focus()
                Else
                    NextPage()
                End If
            Case 3
                If mExpiryControl.AllFieldsValid() Then
                    ShowPage(pnlRolesAndPermissions, False)
                    mAuthControl.Focus()
                Else
                    Rollback(1)
                End If
            Case 4
                CreateNativeUser()
        End Select
    End Sub

    Private Sub CreateNativeUser()
        Try
            CreateNewNativeUser()
            UserMessage.Show(My.Resources.frmUserCreate_TheUserHasBeenSuccessfullyCreated)
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            UserMessage.Err(ex,
             My.Resources.frmUserCreate_TheUserCouldNotBeCreated0, ex.Message)
            Rollback(If(mAuthType.AuthTypeCount = 1, 3, 4))
            mUser = Nothing
            mAuthControl.User = Nothing
            UpdatePage()
        End Try
    End Sub

    Private Sub CreateActiveDirectoryUserSummary()
        mAdUsersSummary = New CtlUsersSummary(Of NewActiveDirectoryUserViewModel)(
            mSelectedUsersSidsAndUpns,
            mAuthControl.CheckedRoles,
            NameOf(NewActiveDirectoryUserViewModel.Upn))
        AddHandler mAdUsersSummary.OnSelectedUsersChanged, AddressOf OnUserSelected
        pnlUsersSummary.Controls.Clear()

        mAdUsersSummary.Dock = DockStyle.Fill
        pnlUsersSummary.Controls.Add(mAdUsersSummary)
    End Sub

    Private Sub CreateActiveDirectoryUsers()

        If mSelectedUsersSidsAndUpns.Count = 0 Then Exit Sub
        Dim users = New List(Of User)

        For Each entry As NewActiveDirectoryUserViewModel In mSelectedUsersSidsAndUpns
            Dim user As User = User.CreateEmptyMappedActiveDirectoryUser(Guid.NewGuid())
            user.Name = entry.Upn
            user.ExternalId = entry.Sid
            user.Roles.AddAll(mAuthControl.CheckedRoles)
            users.Add(user)
        Next

        Const batchSize As Integer = 10
        Dim numUsers = users.Count
        ShowHideCreatingUsersSpinner(True)

        Task.Factory.StartNew(Function() gSv.CreateNewMappedActiveDirectoryUsers(users, batchSize)) _
            .ContinueWith(Sub(task As Task(Of Integer))
                              BeginInvoke(Sub()
                                              If task.IsFaulted AndAlso task.Exception IsNot Nothing Then
                                                  CreateUsersTaskFailedHandler(task)
                                              ElseIf task.Result = numUsers Then
                                                  CreateNewMappedUsersPartialSuccessHandler()
                                              Else
                                                  CreateNewMappedUsersPartialSuccessHandler(task.Result, numUsers)
                                              End If
                                          End Sub)
                          End Sub)
    End Sub

    Private Sub CreateUsersTaskFailedHandler(task As Task)
        Dim firstException As Exception = Nothing
        task.Exception.Handle(Function(ex As Exception)
                                  If firstException Is Nothing Then
                                      firstException = ex
                                  End If
                                  Return True
                              End Function)
        If firstException IsNot Nothing AndAlso
           firstException.GetType() = GetType(NameAlreadyExistsException) Then
            UserMessage.Show(firstException.Message)
        Else
            UserMessage.Show(My.Resources.frmUserCreate_CreateUsersOperationFailed,
                             If(firstException, task.Exception))
        End If
        CloseCreatingUsersDialog()
    End Sub

    Private Sub CreateNewMappedUsersPartialSuccessHandler()
        UserMessage.Show(My.Resources.frmUserCreate_TheUsersWereSuccessfullyCreatedAndTheAuditLogUpdated)
        CloseCreatingUsersDialog()
    End Sub

    Private Sub CreateNewMappedUsersPartialSuccessHandler(usersCreated As Integer, totalUsers As Integer)
        UserMessage.Show(String.Format(My.Resources.frmUserCreate_0Of1UsersWereCreatedPleaseTryAgain, usersCreated, totalUsers))
        CloseCreatingUsersDialog()
    End Sub

    Private Sub CloseCreatingUsersDialog()
        ShowHideCreatingUsersSpinner(False)
        DisposeControls()
        DialogResult = DialogResult.OK
        Dispose()
    End Sub

    Private Sub ShowHideCreatingUsersSpinner(showSpinner As Boolean)
        pnlWaitSpinner.Enabled = showSpinner
        pnlWaitSpinner.Visible = showSpinner
        btnBack.Enabled = Not showSpinner
        btnCancel.Enabled = Not showSpinner
        btnNext.Enabled = Not showSpinner

        If showSpinner Then
            pnlWaitSpinner.BringToFront()
            AddHandler Closing, AddressOf frmUserCreate_FormClosing
        Else
            pnlWaitSpinner.SendToBack()
            RemoveHandler Closing, AddressOf frmUserCreate_FormClosing
        End If
    End Sub

    Private Sub frmUserCreate_FormClosing(sender As Object, e As CancelEventArgs)
        e.Cancel = True
    End Sub

    Private Sub CreateUserDetails()

        Dim authMode As AuthMode = mAuthType.GetSelectedAuthType()
        If IsNewUserOrChangeOfUserAuth(mUser, authMode) Then
            Dim userName As String = If(mUser Is Nothing, String.Empty, mUser.Name)
            Select Case authMode
                Case AuthMode.Native
                    Title = My.Resources.frmUserCreate_AssignUserNameAttributes
                    mUserDetails = New UserSettingsNameAndPassword()
                    mUser = New User(AuthMode.Native, Guid.NewGuid(), userName, Nothing)
                Case AuthMode.MappedActiveDirectory
                    Title = My.Resources.frmUserCreate_SearchActiveDirectory
                    mMultiUserActiveDirectorySearch = New ctlMultiUserActiveDirectorySearch(mSelectedUsersSidsAndUpns)
                    AddHandler mMultiUserActiveDirectorySearch.OnSelectedUserChanged, AddressOf OnUserSelected
                    mUserDetails = mMultiUserActiveDirectorySearch
                    mUser = User.CreateEmptyMappedActiveDirectoryUser(Guid.NewGuid())
                    mUser.Name = userName
                    btnNext.Enabled = False
                Case Else
                    Throw New ArgumentException($"{authMode} not supported")
            End Select

            ' clear any user we may have previously added
            For Each control As UserDetailsControl In pnlUser.Controls
                control.Dispose()
            Next
            pnlUser.Controls.Clear()

            mUserDetails.Dock = DockStyle.Fill
            pnlUser.Controls.Add(mUserDetails)

            mUserDetails.User = mUser
            mAuthControl.User = mUser
            mExpiryControl.User = mUser
        Else
            If authMode = AuthMode.MappedActiveDirectory Then
                DirectCast(mUserDetails, ctlMultiUserActiveDirectorySearch).UpdateGridFromListOfSelectedUsers()
            End If
        End If
    End Sub

    Private Sub OnUserSelected(sender As Object, args As clsUserSelectedEventArgs)
        btnNext.Enabled = args.UsersSelected
    End Sub

    Private Sub CreateNewNativeUser()
        mUser.Roles.AddAll(mAuthControl.CheckedRoles)
        Dim userNameAndPasswordControl = DirectCast(mUserDetails, UserSettingsNameAndPassword)
        gSv.CreateNewUser(mUser, userNameAndPasswordControl.NewPassword)

        If Group IsNot Nothing Then
            Dim userGroupMember = New UserGroupMember(mUser)
            gSv.AddToGroup(GroupTreeType.Users, CType(Group.Id, Guid), userGroupMember)
        End If
    End Sub

    Private Function IsNewUserOrChangeOfUserAuth(user As User, authMode As AuthMode) As Boolean
        If (user Is Nothing) _
        Or (user IsNot Nothing AndAlso user.AuthType <> authMode) Then
            Return True
        End If
        Return False
    End Function

    Public Overrides Function GetHelpFile() As String
        Return "frmUserCreate.htm"
    End Function

    Private Sub DisposeControls()
        mMultiUserActiveDirectorySearch?.Dispose()
        mAdUsersSummary?.Dispose()
    End Sub
End Class
