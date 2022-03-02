Imports AutomateControls
Imports BluePrism.Images
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes

Public Class ProcessDetailPanel : Implements IEnvironmentColourManager, IRefreshable

    ''' <summary>
    ''' Event fired when a refresh request is made by this panel
    ''' </summary>
    Public Event RefreshRequested As EventHandler

    ''' <summary>
    ''' Event fired when a 'delete' request is made within this panel
    ''' </summary>
    Public Event GroupMemberDeleteRequested As GroupMemberEventHandler

    ''' <summary>
    ''' Event fired when a group member is activated within this control
    ''' </summary>
    Public Event GroupMemberActivated As GroupMemberEventHandler


    ''' <summary>
    ''' Event fired when a group member is previewed within this control
    ''' </summary>
    Public Event GroupMemberPreview As GroupMemberEventHandler



    ' The process/object being shown in this panel
    Private mProcessMember As ProcessBackedGroupMember

    ''' <summary>
    ''' Gets or sets the environment-specific back colour in use in this environment.
    ''' Only set to the database-held values after login.
    ''' </summary>
    ''' <remarks>Note that this only affects the UI owned directly by this form - ie.
    ''' setting the colour here will not update the database</remarks>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentBackColor As Color _
     Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return lblSubtitle.BackColor
        End Get
        Set(value As Color)
            lblSubtitle.BackColor = value
            panTitle.BackColor = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the environment-specific fore colour in use in this environment.
    ''' Only set to the database-held values after login.
    ''' </summary>
    ''' <remarks>Note that this only affects the UI owned directly by this form - ie.
    ''' setting the colour here will not update the database</remarks>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentForeColor As Color _
        Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return lblSubtitle.ForeColor
        End Get
        Set(value As Color)
            lblSubtitle.ForeColor = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the process-backed group member which this panel is showing the
    ''' detail of.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property ProcessMember As ProcessBackedGroupMember
        Get
            Return mProcessMember
        End Get
        Set(value As ProcessBackedGroupMember)
            mProcessMember = value
        End Set
    End Property

    ''' <summary>
    ''' Raises the <see cref="GroupMemberActivated"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnGroupMemberActivated(e As GroupMemberEventArgs)
        RaiseEvent GroupMemberActivated(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="GroupMemberDeleteRequested"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnGroupMemberDeleteRequested(e As GroupMemberEventArgs)
        RaiseEvent GroupMemberDeleteRequested(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="GroupMemberPreview"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnGroupMemberPreview(e As GroupMemberEventArgs)
        RaiseEvent GroupMemberPreview(Me, e)
    End Sub


    ''' <summary>
    ''' Handles a look at the references to the process-backed object being displayed
    ''' in this form
    ''' </summary>
    Private Sub FindReferencesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FindReferencesToolStripMenuItem.Click
        Dim p As frmApplication = TryCast(ParentForm, frmApplication)
        If p Is Nothing Then Return
        Dim dep As clsProcessDependency = mProcessMember.Dependency
        If dep Is Nothing Then Return
        p.FindReferences(dep)
    End Sub

    Private Sub RefreshToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RefreshToolStripMenuItem.Click
        RefreshView()
        'Refresh all the tree
        RaiseEvent RefreshRequested(Me, New EventArgs)
    End Sub

    Public Sub RefreshView() Implements IRefreshable.RefreshView
        If mProcessMember Is Nothing Then
            pbIcon.Image = Nothing
            lblProcessName.Text = String.Empty
            ProcessDescriptionTextBox.Text = String.Empty
            mHistoryList.ClearItems()
        Else
            pbIcon.Image = ImageLists.Components_32x32.Images(mProcessMember.ImageKey)
            lblProcessName.Text = mProcessMember.Name
            ProcessDescriptionTextBox.Text = mProcessMember.Description
            mHistoryList.Mode = mProcessMember.ProcessType
            mHistoryList.CanViewDefinition = mProcessMember.Permissions.HasPermission(
                        User.Current, If(mProcessMember.ProcessType = ProcessType.Process,
                            Permission.ProcessStudio.ImpliedViewProcess,
                            Permission.ObjectStudio.ImpliedViewBusinessObject))
            If mHistoryList.ProcessId <> mProcessMember.IdAsGuid Then
                mHistoryList.ProcessId = mProcessMember.IdAsGuid
            Else
                mHistoryList.RefreshView()
            End If
        End If
    End Sub

    ''' <summary>
    ''' Handles the edit context menu item
    ''' </summary>
    Private Sub OpenToolStripMenuItem_Click(sender As Object, e As EventArgs) _
        Handles OpenToolStripMenuItem.Click
        OnGroupMemberActivated(New GroupMemberEventArgs(mProcessMember))
    End Sub

    ''' <summary>
    ''' Handles the delete context menu item
    ''' </summary>
    Private Sub DeleteToolStripMenuItem_Click(sender As Object, e As EventArgs) _
        Handles DeleteToolStripMenuItem.Click
        OnGroupMemberDeleteRequested(New GroupMemberEventArgs(mProcessMember))
    End Sub

    ''' <summary>
    ''' Handles the view context menu item
    ''' </summary>
    Private Sub ViewToolStripMenuItem_Click(sender As Object, e As EventArgs) _
        Handles ViewToolStripMenuItem.Click
        OnGroupMemberPreview(New GroupMemberEventArgs(mProcessMember))
    End Sub



    ''' <summary>
    ''' Handles the opening of the menu button menu strip
    ''' </summary>
    Private Sub HandlesMenuButtonMenuOpening(sender As Object, e As CancelEventArgs) Handles mMenuButtonContextMenuStrip.Opening
        Dim treeDef = mProcessMember.Tree.TreeType.GetTreeDefinition()
        DeleteToolStripMenuItem.Enabled =
            mProcessMember.Permissions.HasPermission(User.Current, treeDef.DeleteItemPermission)
        ViewToolStripMenuItem.Enabled =
            mProcessMember.Permissions.HasPermission(User.Current, If(mProcessMember.ProcessType = DiagramType.Process,
                                    Permission.ProcessStudio.ImpliedViewProcess, Permission.ObjectStudio.ImpliedViewBusinessObject))
        OpenToolStripMenuItem.Enabled =
            mProcessMember.Permissions.HasPermission(User.Current, If(mProcessMember.ProcessType = DiagramType.Process,
                                    Permission.ProcessStudio.ImpliedEditProcess, Permission.ObjectStudio.ImpliedEditBusinessObject))
    End Sub

    Private Sub mHistoryList_KeyDown(sender As Object, e As KeyEventArgs) Handles mHistoryList.KeyDown

        If e.KeyCode = Keys.F10 AndAlso e.Modifiers = Keys.Shift Then
            mHistoryList.ContextMenuStrip.Show(mHistoryList.PointToScreen(Point.Empty))
        End If

    End Sub
End Class
