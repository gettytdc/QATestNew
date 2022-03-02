Imports AutomateControls
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Groups

Public Class GroupDetailPanel : Implements IEnvironmentColourManager, IRefreshable

#Region " Published Events "

    ''' <summary>
    ''' Event fired when a refresh request is made by this panel
    ''' </summary>
    Public Event RefreshRequested As EventHandler

    ''' <summary>
    ''' Event fired when a new item request is made by this panel
    ''' </summary>
    Public Event GroupMemberCreateRequested As CreateGroupMemberEventHandler

    ''' <summary>
    ''' Event fired when a 'delete' request is made within this panel
    ''' </summary>
    Public Event GroupMemberDeleteRequested As GroupMemberEventHandler

    ''' <summary>
    ''' Event fired when an 'unlock' request is made within this panel
    ''' </summary>
    Public Event GroupMemberUnlockRequested As GroupMemberEventHandler

    ''' <summary>
    ''' Event fired when the contents of this group have changed.
    ''' </summary>
    Public Event GroupContentsChanged As GroupEventHandler

    ''' <summary>
    ''' Event fired when a group member is activated within this control
    ''' </summary>
    Public Event GroupMemberActivated As GroupMemberEventHandler

    ''' <summary>
    ''' Event fired when a group member is previewed within this control
    ''' </summary>
    Public Event GroupMemberPreview As GroupMemberEventHandler

    Public Event GroupMemberContextMenuOpening As GroupMemberContexMenuOpeningEventHandler
    ''' <summary>
    ''' Event fired when a group members are compared within this control
    ''' </summary>
    Public Event GroupMemberCompareRequested As GroupMultipleMemberEventHandler

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new group detail panel
    ''' </summary>
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets or sets the group being displayed in this panel
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property DisplayedGroup As IGroup
        Get
            Return gvContents.IGroup
        End Get
        Set(value As IGroup)
            gvContents.IGroup = value
            UpdateCreateEnabled()
        End Set
    End Property

    Public Property DefaultGroup As IGroup
        Get
            Return gvContents.DefaultGroup
        End Get
        Set
            gvContents.DefaultGroup = Value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the environment back colour for this panel
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentBackColor As Color _
     Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return lblHeader.BackColor
        End Get
        Set(value As Color)
            lblHeader.BackColor = value
            mMenuButton.BackColor = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the environment fore colour for this panel
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentForeColor As Color _
     Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return lblHeader.ForeColor
        End Get
        Set(value As Color)
            lblHeader.ForeColor = value
        End Set
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Raises the <see cref="EventArgs"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnRefreshRequested(e As EventArgs)
        RaiseEvent RefreshRequested(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="CreateGroupMemberEventArgs"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnGroupMemberCreateRequested(e As CreateGroupMemberEventArgs)
        RaiseEvent GroupMemberCreateRequested(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="GroupMemberDeleteRequested"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnGroupMemberDeleteRequested(e As GroupMemberEventArgs)
        RaiseEvent GroupMemberDeleteRequested(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="GroupMemberUnlockRequested"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnGroupMemberUnlockRequested(e As GroupMemberEventArgs)
        RaiseEvent GroupMemberUnlockRequested(Me, e)
    End Sub


    ''' <summary>
    ''' Raises the <see cref="GroupMemberCompareRequested"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnGroupMemberCompareRequested(e As GroupMultipleMemberEventArgs)
        RaiseEvent GroupMemberCompareRequested(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="GroupContentsChanged"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event; the
    ''' <see cref="GroupMemberEventArgs.Target"/> property contains the group whose
    ''' contents have changed.</param>
    Protected Overridable Sub OnGroupContentsChanged(e As GroupEventArgs)
        RaiseEvent GroupContentsChanged(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="GroupMemberActivated"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnGroupMemberActivated(e As GroupMemberEventArgs)
        RaiseEvent GroupMemberActivated(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="GroupMemberPreview"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnGroupMemberPreview(e As GroupMemberEventArgs)
        RaiseEvent GroupMemberPreview(Me, e)
    End Sub



    Private Sub HandleGroupMemberContextMenuOpening(sender As Object, e As GroupMemberContexMenuOpeningEventArgs) _
        Handles gvContents.GroupMemberContextMenuOpening
        RaiseEvent GroupMemberContextMenuOpening(Me, e)
    End Sub

    ''' <summary>
    ''' Handles the <see cref="GroupContentsDataGridView.GroupMemberActivated"/>
    ''' event, just chaining it to the <see cref="GroupMemberActivated"/> event which
    ''' is fired from this control.
    ''' </summary>
    Private Sub HandleGroupMemberActivated(
     sender As Object, e As GroupMemberEventArgs) _
     Handles gvContents.GroupMemberActivated
        OnGroupMemberActivated(e)
    End Sub

    ''' <summary>
    ''' Handles the <see cref="GroupContentsDataGridView.GroupMemberPreview"/>
    ''' event, just chaining it to the <see cref="GroupMemberPreview"/> event which
    ''' is fired from this control.
    ''' </summary>
    Private Sub HandleGroupMemberPreview(
     sender As Object, e As GroupMemberEventArgs) _
     Handles gvContents.GroupMemberPreview
        OnGroupMemberPreview(e)
    End Sub



    ''' <summary>
    ''' Handles the <see cref="GroupContentsDataGridView.GroupMemberUnlockRequested"/>
    ''' event, just chaining it to the <see cref="GroupMemberUnlockRequested"/> event
    ''' </summary>
    Private Sub HandleGroupMemberDeleteRequest(
     sender As Object, e As GroupMemberEventArgs) _
     Handles gvContents.GroupMemberDeleteRequested
        OnGroupMemberDeleteRequested(e)
    End Sub

    ''' <summary>
    ''' Handles the <see cref="GroupContentsDataGridView.GroupMemberDeleteRequested"/>
    ''' event, just chaining it to the <see cref="GroupMemberDeleteRequested"/> event
    ''' </summary>
    Private Sub HandleGroupMemberUnlockRequest(
     sender As Object, e As GroupMemberEventArgs) _
     Handles gvContents.GroupMemberUnlockRequested
        OnGroupMemberUnlockRequested(e)
    End Sub

    ''' <summary>
    ''' Handles the <see cref="GroupContentsDataGridView.GroupContentsChanged"/>
    ''' event, just chaining it to the <see cref="GroupContentsChanged"/> event
    ''' </summary>
    Private Sub HandleGroupContentsChanged(
     sender As Object, e As GroupEventArgs) _
     Handles gvContents.GroupContentsChanged
        OnGroupContentsChanged(e)
    End Sub

    Private Sub HandleGroupMemberCompareRequested(
     sender As Object, e As GroupMultipleMemberEventArgs) _
     Handles gvContents.GroupMemberCompareRequested
        OnGroupMemberCompareRequested(e)
    End Sub

#End Region

    ''' <summary>
    ''' Updates the enabled state of the create context menu item according to the
    ''' permissions of the current user
    ''' </summary>
    Private Sub UpdateCreateEnabled()
        Dim treeDef = DisplayedGroup.TreeType.GetTreeDefinition()

        ' If displayed group is the root group and the tree has a default group, check permissions on the default group instead.
        ' (since that is where the created item will end up)
        If DisplayedGroup.IsRoot AndAlso DisplayedGroup.Tree.HasDefaultGroup Then
            If DisplayedGroup.Tree.CanAccessDefaultGroup Then
                ' Has a default group the can see - check permissions on that
                CreateToolStripMenuItem.Enabled = DisplayedGroup.Tree.DefaultGroup.Permissions.HasPermission(User.Current, treeDef.CreateItemPermission)
            Else
                'Has a default group but this user can not access it.
                CreateToolStripMenuItem.Enabled = False
            End If
        Else
            ' This is not the root node, check group permissions as normal
            CreateToolStripMenuItem.Enabled = DisplayedGroup.Permissions.HasPermission(User.Current, treeDef.CreateItemPermission)
        End If
    End Sub

    ''' <summary>
    ''' Handles the create tool strip menu item click
    ''' </summary>
    Private Sub CreateToolStripMenuItem_Click(sender As Object, e As EventArgs) _
        Handles CreateToolStripMenuItem.Click
        Dim typeName As String = ""
        Try

            Dim gp = DisplayedGroup
            If gp Is Nothing Then Return

            ' If current group is the root node and this tree has a default group
            ' then create in the default group instead
            If gp.IsRoot AndAlso gp.Tree.HasDefaultGroup Then
                gp = gp.Tree.DefaultGroup
            End If

            Dim tp = gp.SupportedTypes.First(Function(t) t <> GroupMemberType.Group)

            If tp = GroupMemberType.None Then Return

            typeName = tp.GetLocalizedFriendlyName(True)

            Dim arg As New CreateGroupMemberEventArgs(gp, tp)
            OnGroupMemberCreateRequested(arg)

            ' And update the list
            OnRefreshRequested(e)

        Catch ex As Exception
            UserMessage.Err(
                ex, My.Resources.GroupDetailPanel_AnErrorOccurredCreatingANew0, typeName)

        End Try
    End Sub

    ''' <summary>
    ''' Updates the view.
    ''' </summary>
    Public Sub UpdateView() Implements IRefreshable.RefreshView
        gvContents.UpdateView()
    End Sub

    ''' <summary>
    ''' Handles the refresh toolstrip menu item click
    ''' </summary>
    Private Sub RefreshToolStripMenuItem_Click(sender As Object, e As EventArgs) _
        Handles RefreshToolStripMenuItem.Click
        OnRefreshRequested(e)
    End Sub

    Private Sub gvContents_KeyDown(sender As Object, e As KeyEventArgs) Handles gvContents.KeyDown
        Select Case e.KeyCode
            Case Keys.F10
                If e.Modifiers = Keys.Shift Then
                    gvContents.ContextMenuStrip.Show(gvContents.PointToScreen(Point.Empty))
                End If
            Case Keys.Enter
                For Each mem In gvContents.SelectedMembers
                    OnGroupMemberActivated(New GroupMemberEventArgs(mem))
                Next
                e.SuppressKeyPress = True
        End Select
    End Sub
End Class
