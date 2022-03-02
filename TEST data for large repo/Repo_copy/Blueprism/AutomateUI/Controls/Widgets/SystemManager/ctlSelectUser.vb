Imports System.DirectoryServices
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Images
Imports System.DirectoryServices.ActiveDirectory
Imports BluePrism.BPCoreLib
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.Core.ActiveDirectory
Imports BluePrism.Server.Domain.Models

''' <summary>
''' A control to display a list of users.
''' </summary>
Public Class ctlSelectUser : Inherits UserControl : Implements IMenuButtonHandler

#Region " Published Events "

    Public Event ADUserListRefreshed As EventHandler

    ''' <summary>
    ''' Raised when a Refresh event is requested by this control
    ''' </summary>
    Public Event RefreshRequested As EventHandler

    ''' <summary>
    ''' Raised when an 'Edit User' event is requested by this control
    ''' </summary>
    Public Event EditUserRequested As GroupMemberEventHandler

    ''' <summary>
    ''' Event raised when the user has indicated to this control that it wishes to
    ''' delete a user.
    ''' </summary>
    Public Event DeleteUserRequested As GroupMemberEventHandler

    ''' <summary>
    ''' Event raised when the user has indicated to this control that it wishes to
    ''' create a new user.
    ''' </summary>
    Public Event CreateUserRequested As CreateGroupMemberEventHandler

    ''' <summary>
    ''' Event raised when the user has indicated to this control that it wishes to
    ''' create a new external user.
    ''' </summary>
    Public Event CreateExternalUserRequested As CreateGroupMemberEventHandler

    Public Event CreateActiveDirectoryUserRequested As CreateGroupMemberEventHandler

    ''' <summary>
    ''' Raised when an 'Unlock User' event is requested by this control
    ''' </summary>
    Public Event UnlockUserRequested As GroupMemberEventHandler

#End Region

#Region " Windows Form Designer generated code "

    Public WithEvents ctxUserMaintenance As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents NewToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents EditToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DeleteToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents UnlockToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripSeparator2 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents ADRefreshToolstripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ADViewGroupMembershipToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents menuRefresh As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ProcessAlertsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem

    'UserControl overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer
    Private WithEvents lvUsers As AutomateControls.FlickerFreeListView
    Private WithEvents fullNamesWorker As System.ComponentModel.BackgroundWorker

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlSelectUser))
        Me.fullNamesWorker = New System.ComponentModel.BackgroundWorker()
        Me.lvUsers = New AutomateControls.FlickerFreeListView()
        Me.ctxUserMaintenance = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.NewToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.EditToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DeleteToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.UnlockToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ADRefreshToolstripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ADViewGroupMembershipToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator2 = New System.Windows.Forms.ToolStripSeparator()
        Me.ProcessAlertsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuRefresh = New System.Windows.Forms.ToolStripMenuItem()
        Me.ctxUserMaintenance.SuspendLayout()
        Me.SuspendLayout()
        '
        'fullNamesWorker
        '
        Me.fullNamesWorker.WorkerReportsProgress = True
        '
        'lvUsers
        '
        Me.lvUsers.AllowColumnReorder = True
        resources.ApplyResources(Me.lvUsers, "lvUsers")
        Me.lvUsers.ContextMenuStrip = Me.ctxUserMaintenance
        Me.lvUsers.FullRowSelect = True
        Me.lvUsers.HideSelection = False
        Me.lvUsers.MultiSelect = False
        Me.lvUsers.Name = "lvUsers"
        Me.lvUsers.UseCompatibleStateImageBehavior = False
        Me.lvUsers.View = System.Windows.Forms.View.Details
        '
        'ctxUserMaintenance
        '
        Me.ctxUserMaintenance.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.NewToolStripMenuItem, Me.EditToolStripMenuItem, Me.DeleteToolStripMenuItem, Me.UnlockToolStripMenuItem, Me.ADRefreshToolstripMenuItem, Me.ADViewGroupMembershipToolStripMenuItem, Me.ToolStripSeparator2, Me.ProcessAlertsToolStripMenuItem, Me.menuRefresh})
        Me.ctxUserMaintenance.Name = "ctxUserMaintenance"
        resources.ApplyResources(Me.ctxUserMaintenance, "ctxUserMaintenance")
        '
        'NewToolStripMenuItem
        '
        Me.NewToolStripMenuItem.Image = Global.AutomateUI.My.Resources.AuthImages.User_Blue_Add_16x16
        Me.NewToolStripMenuItem.Name = "NewToolStripMenuItem"
        resources.ApplyResources(Me.NewToolStripMenuItem, "NewToolStripMenuItem")
        '
        'EditToolStripMenuItem
        '
        resources.ApplyResources(Me.EditToolStripMenuItem, "EditToolStripMenuItem")
        Me.EditToolStripMenuItem.Image = Global.AutomateUI.My.Resources.AuthImages.User_Blue_Edit_16x16
        Me.EditToolStripMenuItem.Name = "EditToolStripMenuItem"
        '
        'DeleteToolStripMenuItem
        '
        resources.ApplyResources(Me.DeleteToolStripMenuItem, "DeleteToolStripMenuItem")
        Me.DeleteToolStripMenuItem.Image = Global.AutomateUI.My.Resources.ToolImages.Delete_Red_16x16
        Me.DeleteToolStripMenuItem.Name = "DeleteToolStripMenuItem"
        '
        'UnlockToolStripMenuItem
        '
        resources.ApplyResources(Me.UnlockToolStripMenuItem, "UnlockToolStripMenuItem")
        Me.UnlockToolStripMenuItem.Image = Global.AutomateUI.My.Resources.AuthImages.User_Blue_Unlock_16x16
        Me.UnlockToolStripMenuItem.Name = "UnlockToolStripMenuItem"
        '
        'ADRefreshToolstripMenuItem
        '
        Me.ADRefreshToolstripMenuItem.Name = "ADRefreshToolstripMenuItem"
        resources.ApplyResources(Me.ADRefreshToolstripMenuItem, "ADRefreshToolstripMenuItem")
        '
        'ADViewGroupMembershipToolStripMenuItem
        '
        Me.ADViewGroupMembershipToolStripMenuItem.Name = "ADViewGroupMembershipToolStripMenuItem"
        resources.ApplyResources(Me.ADViewGroupMembershipToolStripMenuItem, "ADViewGroupMembershipToolStripMenuItem")
        '
        'ToolStripSeparator2
        '
        Me.ToolStripSeparator2.Name = "ToolStripSeparator2"
        resources.ApplyResources(Me.ToolStripSeparator2, "ToolStripSeparator2")
        '
        'ProcessAlertsToolStripMenuItem
        '
        Me.ProcessAlertsToolStripMenuItem.Image = Global.AutomateUI.My.Resources.ToolImages.Alarm_16x16
        Me.ProcessAlertsToolStripMenuItem.Name = "ProcessAlertsToolStripMenuItem"
        resources.ApplyResources(Me.ProcessAlertsToolStripMenuItem, "ProcessAlertsToolStripMenuItem")
        '
        'menuRefresh
        '
        Me.menuRefresh.Image = Global.AutomateUI.My.Resources.ToolImages.Refresh_16x16
        Me.menuRefresh.Name = "menuRefresh"
        resources.ApplyResources(Me.menuRefresh, "menuRefresh")
        '
        'ctlSelectUser
        '
        Me.Controls.Add(Me.lvUsers)
        Me.Name = "ctlSelectUser"
        Me.ctxUserMaintenance.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

#End Region

#Region " Member Vars "

    ''' <summary>
    ''' The sorter responsible for performing the sort on the list view.
    ''' </summary>
    Private mSorter As clsListViewSorter

    Private mShouldShowAll As Boolean

    Private mSelectedGroup As IGroup

    Private mAllDisplayedUsersAreExternalOrDeleted As Boolean


#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new instance of the Select User control
    ''' </summary>
    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Show the correct context menu items for native auth or AD system
        Dim isDatabaseSingleSignOn = User.IsLoggedInto(DatabaseType.SingleSignOn)
        Dim allUsers As ICollection(Of User) = Nothing
        Dim logonOptions = gSv.GetLogonOptions(allUsers)

        DeleteToolStripMenuItem.Visible = Not isDatabaseSingleSignOn
        NewToolStripMenuItem.Visible = Not isDatabaseSingleSignOn
        EditToolStripMenuItem.Visible = Not isDatabaseSingleSignOn
        UnlockToolStripMenuItem.Visible = Not isDatabaseSingleSignOn

        ADRefreshToolstripMenuItem.Visible = isDatabaseSingleSignOn
        ADViewGroupMembershipToolStripMenuItem.Visible = isDatabaseSingleSignOn

        'Add any initialization after the InitializeComponent() call
        lvUsers.LargeImageList = ImageLists.Auth_32x32
        lvUsers.SmallImageList = ImageLists.Auth_16x16

        ProcessAlertsToolStripMenuItem.Enabled =
            Licensing.License.CanUse(LicenseUse.ProcessAlerts)

    End Sub

#End Region

#Region " Properties "
    ''' <summary>
    ''' Gets or sets whether all users should be displayed or not.
    ''' </summary>
    <Browsable(True), Category("Behavior"), DefaultValue(False), Description(
     "Whether this control should show all users, including deleted users")>
    Public Property ShouldShowAll As Boolean
        Get
            Return mShouldShowAll
        End Get
        Set(value As Boolean)
            mShouldShowAll = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the displayed group in this control
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property DisplayedGroup As IGroup
        Get
            Return mSelectedGroup
        End Get
        Set(value As IGroup)
            mSelectedGroup = value
            UpdateView()
        End Set
    End Property

    ''' <summary>
    ''' The menu strip to display in the menu button hosted by a parent of this
    ''' control.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property MenuStrip As ContextMenuStrip _
     Implements IMenuButtonHandler.MenuStrip
        Get
            Return ctxUserMaintenance
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the currently selected item in the users listview.
    ''' When setting, this will clear any current selection before setting the
    ''' selected item.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Protected Property SelectedItem() As ListViewItem
        Get
            Try
                If lvUsers.SelectedItems.Count = 0 Then Return Nothing
                Return lvUsers.SelectedItems(0)
            Catch aoore As ArgumentOutOfRangeException
                Return Nothing
            End Try
        End Get
        Set(ByVal value As ListViewItem)
            lvUsers.SelectedItems.Clear()
            If value IsNot Nothing Then value.Selected = True
        End Set
    End Property

    ''' <summary>
    ''' A User representing the first currently selected user, or null if no
    ''' user is selected.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property SelectedUser() As User
        Get
            Dim item As ListViewItem = SelectedItem
            If item Is Nothing Then Return Nothing
            Return TryCast(item.Tag, User)
        End Get
    End Property

    ''' <summary>
    ''' Gets the selected user as a user group member, or null if no user is selected
    ''' or the selected user could not be found in the displayed group.
    ''' </summary>
    Public ReadOnly Property SelectedUserMember As UserGroupMember
        Get
            Dim u = SelectedUser
            If u Is Nothing Then Return Nothing

            Dim g = DisplayedGroup
            If g Is Nothing Then Return Nothing

            Return TryCast(
                g.FirstOrDefault(Function(m) Not m.IsGroup AndAlso m.IdAsGuid = u.Id),
                UserGroupMember)

        End Get
    End Property

    ''' <summary>
    ''' Enable or disable multiple selections of users; Disabled by default.
    ''' </summary>
    <DefaultValue(False)>
    Public Property MultiSelect() As Boolean
        Get
            Return lvUsers.MultiSelect
        End Get
        Set(ByVal value As Boolean)
            lvUsers.MultiSelect = value
        End Set
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Raises the <see cref="RefreshRequested"/> event
    ''' </summary>
    Protected Overridable Sub OnRefreshRequested(e As EventArgs)
        RaiseEvent RefreshRequested(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="EditUserRequested"/> event
    ''' </summary>
    Protected Overridable Sub OnEditUserRequested(e As GroupMemberEventArgs)
        RaiseEvent EditUserRequested(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="UnlockUserRequested"/> event
    ''' </summary>
    Protected Overridable Sub OnUnlockUserRequested(e As GroupMemberEventArgs)
        RaiseEvent UnlockUserRequested(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="CreateUserRequested"/> event
    ''' </summary>
    Protected Overridable Sub OnCreateUserRequested(e As CreateGroupMemberEventArgs)
        RaiseEvent CreateUserRequested(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="OnCreateExternalUserRequested"/> event
    ''' </summary>
    Protected Overridable Sub OnCreateExternalUserRequested(e As CreateGroupMemberEventArgs)
        RaiseEvent CreateExternalUserRequested(Me, e)
    End Sub

    Protected Overridable Sub OnCreateActiveDirectoryUserRequested(e As CreateGroupMemberEventArgs)
        RaiseEvent CreateActiveDirectoryUserRequested(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="DeleteUserRequested"/> event.
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnDeleteUserRequested(e As GroupMemberEventArgs)
        RaiseEvent DeleteUserRequested(Me, e)
    End Sub

    ''' <summary>
    ''' Updates the view of the users in the group set in this control
    ''' </summary>
    Friend Sub UpdateView()

        mAllDisplayedUsersAreExternalOrDeleted = False

        If Not User.LoggedIn Then Exit Sub

        Dim group = DisplayedGroup
        Dim users As ICollection(Of User)
        If group Is Nothing Then
            users = New List(Of User)(gSv.GetAllUsers())
        Else
            users = New List(Of User)(group.Count)
            For Each m In group
                'Groups can contain groups as well as UserGroupMembers
                If TryCast(m, UserGroupMember) IsNot Nothing Then
                    Dim user As User = gSv.GetUser(DirectCast(m.Id, Guid))
                    users.Add(user)
                End If
            Next
        End If

        If Not ShouldShowAll Then
            Dim deletedUsers = New List(Of User)(users.Count)
            For Each u As User In users
                If u.Deleted Then deletedUsers.Add(u)
            Next
            For Each du As User In deletedUsers
                users.Remove(du)
            Next
        End If

        mAllDisplayedUsersAreExternalOrDeleted = users.All(Function(u) u.AuthType = BluePrism.Server.Domain.Models.AuthMode.External OrElse u.Deleted)

        'Preserve selected item
        Dim previouslySelectedUser = SelectedUser

        lvUsers.BeginUpdate()
        lvUsers.Items.Clear()
        lvUsers.Columns.Clear()

        If User.IsLoggedInto(DatabaseType.SingleSignOn) Then
            lvUsers.Columns.Add(My.Resources.ctlSelectUser_UPN, 300, HorizontalAlignment.Left)
            lvUsers.Columns.Add(My.Resources.ctlSelectUser_FullName, 300, HorizontalAlignment.Left)
            lvUsers.Columns.Add(My.Resources.ctlSelectUser_LastSignedIn, 300, HorizontalAlignment.Left)
        Else
            lvUsers.Columns.Add(My.Resources.ctlSelectUser_FullName, 300, HorizontalAlignment.Left)
            lvUsers.Columns.Add(My.Resources.ctlSelectUser_UserValidFrom, 300, HorizontalAlignment.Left)
            lvUsers.Columns.Add(My.Resources.ctlSelectUser_UserValidTo, 300, HorizontalAlignment.Left)
            lvUsers.Columns.Add(My.Resources.ctlSelectUser_PasswordExpiry, 300, HorizontalAlignment.Left)
            lvUsers.Columns.Add(My.Resources.ctlSelectUser_LastSignedIn, 300, HorizontalAlignment.Left)
        End If


        lvUsers.Columns.Add(My.Resources.ctlSelectUser_AccountType, 300, HorizontalAlignment.Left)

        For Each user In users
            If Not user.IsHidden Then
                Dim item As ListViewItem = lvUsers.Items.Add(user.Name, 0)

                If Not User.IsLoggedInto(DatabaseType.SingleSignOn) Then

                    item.SubItems.Add(user.CreatedOptionalDisplayDate)
                    item.SubItems.Add(user.ExpiryOptionalDisplayDate)
                    item.SubItems.Add(user.PasswordExpiryOptionalDisplayDate)
                    item.SubItems.Add(user.LastSignedInAtOptionalDisplayDate)

                    If user.Deleted Then
                        item.ImageKey = If(user.AuthType = AuthMode.AuthenticationServerServiceAccount,
                            ImageLists.Keys.Auth.ServiceAccount_Disabled,
                            ImageLists.Keys.Auth.User_Disabled)
                    ElseIf user.IsLocked Then
                        item.ImageKey = ImageLists.Keys.Auth.UserLocked
                    ElseIf user.AuthType = AuthMode.AuthenticationServerServiceAccount Then
                        item.ImageKey = ImageLists.Keys.Auth.ServiceAccount
                    Else
                        item.ImageKey = ImageLists.Keys.Auth.User
                    End If
                Else
                    'Placeholder for real name which is retrieved via LDAP on a separate thread
                    item.SubItems.Add("")
                    item.SubItems.Add(user.LastSignedInAtOptionalDisplayDate)

                    If user.Deleted Then
                        item.ImageKey = ImageLists.Keys.Auth.User_Disabled
                    Else
                        item.ImageKey = ImageLists.Keys.Auth.User
                    End If
                End If
                item.SubItems.Add(user.AuthType.ToLocalizedDisplayName)

                item.Tag = user

                If previouslySelectedUser IsNot Nothing AndAlso
                   user.Id = previouslySelectedUser.Id Then
                    item.Selected = True
                End If
            End If
        Next

        mSorter = New clsListViewSorter(lvUsers)
        If User.IsLoggedInto(DatabaseType.SingleSignOn) Then
            mSorter.ColumnDataTypes = New Type() {
                                                     GetType(String),
                                                     GetType(String),
                                                     GetType(Date)}
        Else
            mSorter.ColumnDataTypes = New Type() {
                                                     GetType(String),
                                                     GetType(Date),
                                                     GetType(Date),
                                                     GetType(Date),
                                                     GetType(Date)}
        End If

        mSorter.SortColumn = 0
        mSorter.Order = SortOrder.Ascending
        lvUsers.ListViewItemSorter = mSorter
        lvUsers.Sort()

        ' Single Sign-on names are fetched in separate thread and the UI is updated
        ' as it goes along.
        If User.IsLoggedInto(DatabaseType.SingleSignOn) Then
            If Not fullNamesWorker.IsBusy Then fullNamesWorker.RunWorkerAsync(
                lvUsers.Items.Cast(Of ListViewItem).Select(Function(i) i.Text).ToList()
                )
        End If

        ResizeIt()
        lvUsers.EndUpdate()

    End Sub

    ''' <summary>
    ''' Populates the user list view using <see cref="UpdateView"/> when
    ''' this control is loaded.
    ''' </summary>
    Protected Overrides Sub OnLoad(ByVal e As EventArgs)
        If Not DesignMode Then UpdateView()
    End Sub

    ''' <summary>
    ''' sigh
    ''' </summary>
    Private Sub ResizeIt()
        If lvUsers.View <> View.List Then
            Dim column As ColumnHeader
            For Each column In lvUsers.Columns
                column.Width = -2
            Next
        Else
            Dim column As ColumnHeader
            For Each column In lvUsers.Columns
                column.Width = 100
            Next

        End If
    End Sub

#End Region

#Region " Event Handlers "

    ''' <summary>
    ''' Handles the context menu opening, ensuring that the appropriate options are
    ''' available.
    ''' </summary>
    Private Sub HandleContextMenuOpening(
     sender As Object, e As EventArgs) Handles ctxUserMaintenance.Opening
        Dim databaseIsSingleSignOn = User.IsLoggedInto(DatabaseType.SingleSignOn)

        DeleteToolStripMenuItem.Enabled =
            Not databaseIsSingleSignOn AndAlso
            SelectedUser IsNot Nothing AndAlso
            Not SelectedUser.Deleted AndAlso
            Not SelectedUser.IsAuthenticationServerUserOrServiceAccount()

        Dim isSelected As Boolean = SelectedUser IsNot Nothing

        If Not databaseIsSingleSignOn Then
            EditToolStripMenuItem.Enabled = isSelected
            UnlockToolStripMenuItem.Enabled =
                isSelected AndAlso SelectedUser.IsLocked
            NewToolStripMenuItem.Enabled = Not isSelected
        Else
            ADRefreshToolstripMenuItem.Enabled = True
            ADViewGroupMembershipToolStripMenuItem.Enabled = isSelected
        End If

        ProcessAlertsToolStripMenuItem.Enabled =
            isSelected AndAlso
            Licensing.License.CanUse(LicenseUse.ProcessAlerts) AndAlso
            SelectedUser.HasPermission(Permission.ProcessAlerts.SubscribeToProcessAlerts) AndAlso
            Not SelectedUser.Deleted

    End Sub

    ''' <summary>
    ''' A silly pointless little fragment, necessary because we insist on using
    ''' listviews and then independently trying to get each of them to act like a
    ''' DataGridView does for free.
    ''' </summary>
    Private Sub lvUsers_Resize(ByVal sender As Object, ByVal e As EventArgs) _
     Handles lvUsers.Resize
        ResizeIt()
    End Sub

    ''' <summary>
    ''' Handles a user being activated in the users listview
    ''' </summary>
    Private Sub HandleUserActivated() Handles lvUsers.ItemActivate
        OnEditUserRequested(New GroupMemberEventArgs(SelectedUserMember))
    End Sub

    ''' <summary>
    ''' Handles a progress update from the background worker retrieving the full
    ''' names. The progress report data expected is a 2-element string array with a
    ''' a UPN and a display name at indexes 0 and 1, respectively.
    ''' </summary>
    Private Sub HandleProgressChanged(
                                      sender As Object, e As ProgressChangedEventArgs) _
        Handles fullNamesWorker.ProgressChanged
        Try
            Dim userNames = TryCast(e.UserState, String())
            Dim item = lvUsers.FindItemWithText(userNames(0))
            If item IsNot Nothing Then
                item.SubItems(1).Text = userNames(1)
                lvUsers.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.ColumnContent)
            End If

        Catch ex As Exception
            UserMessage.Show(
                String.Format(My.Resources.ctlSelectUser_UnexpectedErrorDuringListviewPopulation0, ex.Message), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Handles the work done by the background worker - ie. it retrieves the display
    ''' names for the upns given to it as an argument, using Active Directory.
    ''' </summary>
    Private Sub HandleDoWork(sender As Object, e As DoWorkEventArgs) _
        Handles fullNamesWorker.DoWork
        Dim upns = TryCast(e.Argument, ICollection(Of String))
        If CollectionUtil.IsNullOrEmpty(upns) Then Return

        Using gc = Forest.GetCurrentForest().FindGlobalCatalog()
            Using ds = gc.GetDirectorySearcher()
                Dim sb As New StringBuilder(2048)
                sb.Append("(&(objectCategory=user)(|")
                For Each upn As String In upns

                    sb.Append("(userPrincipalName=").
                        Append(LdapEscaper.EscapeSearchTerm(upn)).
                        Append(")")
                Next
                sb.Append("))")

                ds.Filter = sb.ToString()
                ds.PropertiesToLoad.Add("displayName")
                ds.PropertiesToLoad.Add("userPrincipalName")

                For Each sr As SearchResult In ds.FindAll()
                    Try
                        Dim upn = CStr(sr.Properties("userPrincipalName")(0))
                        Dim fullName = CStr(sr.Properties("displayName")(0))
                        fullNamesWorker.ReportProgress(0, {upn, fullName})
                    Catch ex As Exception
                        Debug.Print(My.Resources.ctlSelectUser_FailedToGetDisplayNames0, ex)
                    End Try
                Next
            End Using
        End Using
    End Sub

#Region "Context Menu Events"

    Private Sub NewToolStripMenuItem_Click(sender As Object, e As EventArgs) _
        Handles NewToolStripMenuItem.Click
        Dim group As IGroup = DisplayedGroup
        Dim groupMemberEventArgs As New CreateGroupMemberEventArgs(group, GroupMemberType.User)
        OnCreateUserRequested(groupMemberEventArgs)
        Dim newUser = TryCast(groupMemberEventArgs.CreatedItem, UserGroupMember)
        If newUser IsNot Nothing Then UpdateView()
    End Sub

    ''' <summary>
    ''' Handles the 'Edit' menu item being clicked
    ''' </summary>
    Private Sub EditToolStripMenuItem_Click() Handles EditToolStripMenuItem.Click
        OnEditUserRequested(New GroupMemberEventArgs(SelectedUserMember))
    End Sub

    Private Sub ProcessAlertsToolStripMenuItem_Click(sender As Object, e As EventArgs) _
        Handles ProcessAlertsToolStripMenuItem.Click
        Dim user = SelectedUser
        If user Is Nothing Then
            UserMessage.Show(My.Resources.ctlSelectUser_PleaseFirstSelectAUserToEdit)
            Exit Sub
        End If

        If frmAlertConfig.InstanceExists Then
            Dim alertForm As frmAlertConfig =
                    frmAlertConfig.GetInstance(user, frmAlertConfig.ViewMode.ProcessConfig)
            alertForm.Visible = True
            If alertForm.WindowState = FormWindowState.Minimized Then _
                alertForm.WindowState = FormWindowState.Normal
            alertForm.BringToFront()
        Else
            Dim alertForm As _
                    New frmAlertConfig(user, frmAlertConfig.ViewMode.ProcessConfig)
            alertForm.DisablePermissionChecking()
            ' Ensure that the form opens correctly - ie. that the user has the
            ' correct permissions to open it. If not, dispose of it immediately.
            If DirectCast(Me.ParentForm, frmApplication).StartForm(alertForm) = DialogResult.Abort Then _
                alertForm.Dispose()
        End If

    End Sub

    Private Sub UnlockToolStripMenuItem_Click(sender As Object, e As EventArgs) _
        Handles UnlockToolStripMenuItem.Click
        OnUnlockUserRequested(New GroupMemberEventArgs(SelectedUserMember))
    End Sub

    Private Sub DeleteToolStripMenuItem_Click(sender As Object, e As EventArgs) _
        Handles DeleteToolStripMenuItem.Click
        Dim u = SelectedUserMember
        If u IsNot Nothing Then OnDeleteUserRequested(New GroupMemberEventArgs(u))
    End Sub

    Private Sub RefreshADUserListMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs) _
        Handles ADRefreshToolstripMenuItem.Click
        Try
            Dim msg = RefreshADUserListMessageBuilder.Build(gSv.RefreshADUserList())
            RaiseEvent ADUserListRefreshed(Me, EventArgs.Empty)
            UpdateView()

            If msg IsNot Nothing Then UserMessage.Show(msg)

        Catch ex As Exception
            UserMessage.Err(
                ex, My.Resources.ctlSelectUser_ErrorSynchronizingActiveDirectoryUsers0, ex.Message)
        End Try
    End Sub


    Private Sub ViewUserGroupsMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs) _
        Handles ADViewGroupMembershipToolStripMenuItem.Click
        Dim user = SelectedUser
        If user Is Nothing Then
            UserMessage.Show(My.Resources.ctlSelectUser_PleaseFirstSelectAUser)
            Exit Sub
        End If

        Try
            Using f As New frmUserGroupMembership(user.Id, user.Name)
                f.SetEnvironmentColours(DirectCast(Me.ParentForm, frmApplication))
                f.ShowInTaskbar = False
                f.ShowDialog()
                RaiseEvent ADUserListRefreshed(Me, EventArgs.Empty)
            End Using
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlSelectUser_UnexpectedError0, ex.Message), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Handles the 'Refresh' button being clicked in the context menu.
    ''' </summary>
    Private Sub HandleRefreshClick(sender As Object, e As EventArgs) Handles menuRefresh.Click
        OnRefreshRequested(EventArgs.Empty)
    End Sub

#End Region

#End Region

End Class
