
Imports AutomateControls
Imports AutomateControls.Forms
Imports AutomateControls.Diary
Imports AutomateUI.Controls.Navigation.DataGateways
Imports AutomateUI.My.Resources

Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Groups

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Diary
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Core.Extensions

Imports BluePrism.Scheduling
Imports BluePrism.Scheduling.Triggers

Imports BluePrism.Images
Imports Internationalisation
Imports LocaleTools
Imports Newtonsoft.Json


''' <summary>
''' This class represents the schedule manager control in the scheduler tab in
''' control room.
''' </summary>
Public Class ctlControlRoom
    Implements IPermission, IStubbornChild, IHelp, IEnvironmentColourManager, IRefreshable

#Region "All the IStuff implementation stuff"

    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName(Permission.Resources.ImpliedViewResource.Concat(
                                     {Permission.ControlRoom.GroupName, Permission.Scheduler.GroupName, Permission.SystemManager.DataGateways.ControlRoom}).ToList())
        End Get
    End Property

    Private WithEvents mParent As frmApplication

    Private mSavePromptTriggered As Boolean

    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    ''' <summary>
    ''' Checks if this schedule manager can leave or if the user desires it to remain
    ''' with changes intact.
    ''' </summary>
    ''' <returns>True if the schedule manager has no unsaved data or the user has
    ''' allowed it to leave. False otherwise.</returns>
    Public Function CanLeave() As Boolean Implements IStubbornChild.CanLeave

        Dim leaving As Boolean = True

        CommitChanges()

        If IsAnyNodeMarked() AndAlso Not mSavePromptTriggered Then

            Dim form As New YesNoCancelPopupForm(PopupForm_UnsavedChanges,
                                                 ctlControlRoom_YouHaveNotYetSavedYourChangesInTheSchedulerWouldYouLikeToSaveYourChangesBeforeL,
                                                 String.Empty)
            form.ShowInTaskbar = False
            Select Case form.ShowDialog()

                Case DialogResult.Cancel
                    leaving = False

                Case DialogResult.Yes
                    leaving = SaveAll()

                Case DialogResult.No
                    mSavePromptTriggered = True

            End Select
        End If
        ' If we're good to go, then go, otherwise ensure that the scheduler is
        ' selected in the tab pane
        If leaving Then
            SaveExpandedControlRoomTreeState()
            Return True
        End If

        Dim tp As TabPage = UIUtil.GetAncestor(Of TabPage)(Me)
        If tp IsNot Nothing Then
            DirectCast(tp.Parent, TabControl).SelectedTab = tp
        End If

        Return False

    End Function

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "frmControlRoom.htm"
    End Function

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
            Return lblAreaTitle.BackColor
        End Get
        Set(value As Color)
            lblAreaTitle.BackColor = value
            mMenuButton.BackColor = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the environment-specific back colour in use in this environment.
    ''' Only set to the database-held values after login.
    ''' </summary>
    ''' <remarks>Note that this only affects the UI owned directly by this form - ie.
    ''' setting the colour here will not update the database</remarks>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentForeColor As Color _
     Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return lblAreaTitle.ForeColor
        End Get
        Set(value As Color)
            lblAreaTitle.ForeColor = value
        End Set
    End Property

#End Region

#Region "SaverOfAll background worker class"

    ''' <summary>
    ''' Background worker which handles the saving of all data in the schedule
    ''' manager - this allows a progress dialog to display progress.
    ''' </summary>
    Private Class SaverOfAll
        Inherits BackgroundWorker

        ' The schedule manager which initiated this saver
        Private Outer As ctlControlRoom

        ''' <summary>
        ''' Creates a new SaverOfAll for the given schedule manager
        ''' </summary>
        ''' <param name="mgr">The manager, given</param>
        Public Sub New(ByVal mgr As ctlControlRoom)
            Me.WorkerReportsProgress = True
            Me.WorkerSupportsCancellation = False
            Me.Outer = mgr
        End Sub

        ''' <summary>
        ''' Performs the work required of the saver.
        ''' </summary>
        ''' <param name="args">The args detailing the event.</param>
        Protected Overrides Sub OnDoWork(ByVal args As DoWorkEventArgs)

            args.Result = True ' assume it all worked

            Dim count As Integer =
             Outer.mTimetableCategoryNode.Nodes.Count +
             Outer.mReportCategoryNode.Nodes.Count +
             Outer.mScheduleCategoryNode.Nodes.Count

            If count = 0 Then Return ' you're not catching me out that easily...

            ' What is each item as a percentage?
            Dim singleItem As Double = 100.0 / CDbl(count)

            ' Progress accumulator in double terms
            Dim progress As Double = 0

            ' Go through each category node and save any children therein
            For Each node As TreeNode In Outer.mTimetableCategoryNode.Nodes
                Dim tt As ScheduleList = TryCast(node.Tag, ScheduleList)
                If tt IsNot Nothing AndAlso tt.HasChanged() Then
                    Outer.SaveScheduleList(tt)
                End If
                progress += singleItem
                Me.ReportProgress(CInt(progress))
            Next

            For Each node As TreeNode In Outer.mReportCategoryNode.Nodes
                Dim rept As ScheduleList = TryCast(node.Tag, ScheduleList)
                If rept IsNot Nothing AndAlso rept.HasChanged() Then
                    Outer.SaveScheduleList(rept)
                End If
                progress += singleItem
                Me.ReportProgress(CInt(progress))
            Next


            For Each node As TreeNode In Outer.mScheduleCategoryNode.Nodes
                Dim sched As SessionRunnerSchedule = TryCast(node.Tag, SessionRunnerSchedule)
                If sched IsNot Nothing AndAlso sched.HasChanged() Then
                    If Not Outer.SaveSchedule(sched) Then args.Result = False ' didn't work
                End If
                progress += singleItem
                Me.ReportProgress(CInt(progress))
            Next

        End Sub

    End Class

#End Region

#Region " SessionView Class "

    ''' <summary>
    ''' Class which represents a session view in the environment list.
    ''' </summary>
    Friend Class SessionView
        Public Sub New(nm As String, val As String)
            Name = nm
            Value = val
        End Sub
        Public Property Name As String
        Public Property Value As String
    End Class

#End Region

#Region " Member Variables "

    ''' <summary>
    ''' The schedule store to use to access/update the data
    ''' </summary>
    Private mScheduleStore As DatabaseBackedScheduleStore

    ''' <summary>
    ''' Holds a reference to the schedule category node
    ''' </summary>
    Private mScheduleCategoryNode As TreeNode

    ''' <summary>
    ''' Holds a reference to the retired schedule category node
    ''' </summary>
    Private mRetiredCategoryNode As TreeNode

    ''' <summary>
    ''' Holds a reference to the timetable category node
    ''' </summary>
    Private mTimetableCategoryNode As TreeNode

    ''' <summary>
    ''' Holds a reference to the report category node
    ''' </summary>
    Private mReportCategoryNode As TreeNode

    ''' <summary>
    ''' The regular font to use in the treeview - lazily intialised by the
    ''' GetRegularFont() method.
    ''' </summary>
    Private mRegularFont As Font

    ''' <summary>
    ''' The foreground colour to use for expired schedules in the tree.
    ''' Lazily initialised by <see cref="GetExpiredNodeForeColor"/> when needed.
    ''' </summary>
    Private mExpiredColor As Nullable(Of Color)

    ''' <summary>The task which has last been copied by the user. This data is not
    ''' saved to the clipboard, thus it only has meaning inside the currently GUI.
    ''' Also note that, at the point that the task is here, it is <em>not</em>
    ''' actually cloned, ie. it is still reference-equal with the task that was
    ''' targeted when Cut or Copy was called.
    ''' </summary>
    Private mCopiedTask As ScheduledTask

    ' The group node for the session management functions
    Private mSessionNode As TreeNode

    ' The group node for the (non-active) work queues
    Private mQueueNode As TreeNode

    ' The group node for the active work queue
    Private mActiveQueueNode As TreeNode

    ' The group node for the schedule nodes
    Private mSchedulerNode As TreeNode

    Private mDataGatewaysNode As TreeNode

    Private mExpandingTreeNodes As Boolean = False

    Private mRetiredLoaded As Boolean = False

    Private mSchedulesLoaded As Boolean = False

    ' The session management panel, if that is currently what is being displayed
    Private WithEvents panSessionManagement As ctlSessionManagement

    Public Property DataGatewayProcessError As Boolean

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Constructs a new ScheduleManager
    ''' </summary>
    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()
        mControlRoomTree.ImageList = ImageLists.ControlRoom_16x16

        'we only want this item if direct connection
        If gSv.IsServer Then ToggleConnectionsToolStripMenuItem.Enabled = False
    End Sub

#End Region

#Region "Private Utility Methods"

    ''' <summary>
    ''' Checks if the user has permissions to perform <em>any</em> of the given
    ''' actions, alerting them if the do not.
    ''' </summary>
    ''' <param name="actions">The action names to check the currently logged in user
    ''' for.</param>
    ''' <returns>True if the user has permission to perform any of the specified
    ''' actions; False if they have no such permission and they have been so advised.
    ''' </returns>
    Private Function AlertIfNoPermission(ByVal ParamArray actions() As String) As Boolean

        If User.Current.HasPermission(Permission.ByName(actions)) Then Return True

        ' Otherwise the user doesn't have appopriate permissions.
        UserMessage.Show(
         String.Format(My.Resources.ctlControlRoom_YouDoNotHavePermissionToPerformThatActionIfYouBelieveThatThisIsIncorrectThenPle, ApplicationProperties.ApplicationName), 1048586)
        Return False

    End Function

    ''' <summary>
    ''' Gets the 'normal' forecolour for unexpired schedules in the schedule manager
    ''' tree.
    ''' </summary>
    ''' <returns>The colour to use for the forecolor of schedules in the tree which
    ''' have not expired (oor have been modified)</returns>
    Private Function GetNormalNodeForeColor() As Color
        Return mControlRoomTree.ForeColor
    End Function

    ''' <summary>
    ''' Gets the 'expired' forecolour for unexpired schedules in the schedule manager
    ''' tree.
    ''' </summary>
    ''' <returns>The colour to use for the forecolor of schedules in the tree which
    ''' have expired (and are not modified)</returns>
    Private Function GetExpiredNodeForeColor() As Color
        If mExpiredColor.HasValue Then Return mExpiredColor.Value

        Dim col As Color = GetNormalNodeForeColor()
        col = Color.FromArgb(col.A,
         LowlightComponent(col.R), LowlightComponent(col.G), LowlightComponent(col.B))
        mExpiredColor = col
        Return col

    End Function

    ''' <summary>
    ''' Returns a lowlighted version of the given colour component.
    ''' This is half the difference between the given value and 255 either added
    ''' or subtracted from the given value.
    ''' </summary>
    ''' <param name="val">The value to 'lowlight'</param>
    ''' <returns>The lowlighted component value.</returns>
    Private Function LowlightComponent(ByVal val As Integer) As Integer
        If val < 0 OrElse val > 255 Then Throw New ArgumentOutOfRangeException(NameOf(val))

        Dim modder As Integer = (255 - val) \ 2
        If val > 192 Then
            Return val - modder
        Else
            Return val + modder
        End If

    End Function

    ''' <summary>
    ''' Gets the appropriate category node for the given schedule list.
    ''' </summary>
    ''' <param name="list">The list for which the appropriate category node is
    ''' required.</param>
    ''' <returns>The category node within the schedule manager tree which displays
    ''' lists of the given type.</returns>
    Private Function GetCategoryNode(ByVal list As ScheduleList) As TreeNode
        If list Is Nothing Then Return Nothing
        If list.ListType = ScheduleListType.Report Then Return mReportCategoryNode
        Return mTimetableCategoryNode
    End Function

    ''' <summary>
    ''' Checks if the given node is either a node representing a retired schedule,
    ''' a task within a retired schedule or the retired category node itself.
    ''' </summary>
    ''' <param name="node">The node to check and see if it is in the retired category
    ''' or not</param>
    ''' <returns>True if the given node is the retired category node or any node
    ''' beneath it; false otherwise.</returns>
    Private Function IsNodeInRetiredCategory(ByVal node As TreeNode) As Boolean
        While node IsNot Nothing
            If node Is mRetiredCategoryNode Then Return True
            node = node.Parent
        End While
        Return False
    End Function

#End Region

#Region "Populating Data"

    ''' <summary>
    ''' Populates the session management subtree of the navigation tree in Control
    ''' </summary>
    Private Sub PopulateSessionManagementTree()
        Dim canViewSession = User.Current.HasPermission(Permission.Resources.ImpliedViewResource)

        ' Create a node for the sessions
        mSessionNode = New TreeNode(ResMan.GetString("ctlControlRoom_tv_session_management"))
        If canViewSession Then
            mControlRoomTree.Nodes.Add(mSessionNode)
        End If

        mSessionNode.Name = mSessionNode.Text
        mSessionNode.Tag = gSv.GetPref(PreferenceNames.Session.SessionViewDefault, "")
        SetNodeImage(mSessionNode, ImageLists.Keys.ControlRoom.SessionManagement)

        Dim sessionViews = gSv.GetPref(PreferenceNames.Session.SessionViewList, "")
        If String.IsNullOrEmpty(sessionViews) Then
            With mSessionNode.Nodes.Add(ResMan.GetString("ctlControlRoom_tv_session_today"))
                .Name = ResMan.GetString("ctlControlRoom_tv_session_today")
                .ImageKey = "session-filter"
                .SelectedImageKey = .ImageKey
            End With
        Else
            For Each viewName In sessionViews.Split(CChar(vbLf))
                With mSessionNode.Nodes.Add(viewName)
                    .Name = viewName
                    .ImageKey = "session-filter"
                    .SelectedImageKey = .ImageKey
                    .Tag = gSv.GetPref(PreferenceNames.Session.SessionViewPrefix & viewName, "")
                End With
            Next
        End If
    End Sub

    Private Sub SaveExpandedControlRoomTreeState()
        If User.LoggedIn Then
            Dim nodeList As List(Of String) = (From node In mControlRoomTree.ExpandedNodes Select node.FullPath).ToList()
            Dim jsonNodeList = JsonConvert.SerializeObject(nodeList)
            gSv.SetUserPref(PreferenceNames.TreeViewStates.ControlRoomTreeExpandedState, jsonNodeList.ToString)
        End If
    End Sub

    Private Sub LoadExpandedControlRoomState()
        Dim nodeListString = gSv.GetPref(PreferenceNames.TreeViewStates.ControlRoomTreeExpandedState, "")
        If String.IsNullOrWhiteSpace(nodeListString) Then
            Dim doNotExpandTheseNodes = New List(Of TreeNode)({
                                            mScheduleCategoryNode,
                                            mRetiredCategoryNode})

            mControlRoomTree.ExpandAll(doNotExpandTheseNodes)
            Return
        End If

        Dim nodeList = Newtonsoft.Json.JsonConvert.DeserializeObject(Of List(Of String))(nodeListString)
        mControlRoomTree.CollapseAll()

        ExpandControlRoomTreeNodes(mControlRoomTree.Nodes, nodeList)
        mExpandingTreeNodes = False
    End Sub

    Private Sub ExpandControlRoomTreeNodes(nodes As IEnumerable, nodeList As List(Of String))

        For Each node As TreeNode In nodes
            If nodeList.Contains(node?.FullPath) Then
                'if mScheduleCategory Node is part of the tree then we should check this. if it is not defined we can set to true.
                mExpandingTreeNodes = node?.Name <> mScheduleCategoryNode?.Name AndAlso node?.Name <> mRetiredCategoryNode?.Name
                node?.Expand()
            End If
            If node.Nodes.Count > 0 Then
                ExpandControlRoomTreeNodes(node.Nodes, nodeList)
            End If
        Next
    End Sub

    ''' <summary>
    ''' Populates the two (passive and active) queues management subtrees of the
    ''' navigation tree in Control
    ''' </summary>
    Private Sub PopulateQueueTrees()
        Dim canViewQueues = User.Current.HasPermission(
                Permission.ControlRoom.ManageQueuesFullAccess,
                Permission.ControlRoom.ManageQueuesReadOnly)


        If mQueueNode Is Nothing Then



            Dim groupStore As IGroupStore = GetGroupStore()
            Dim allQueues As IGroupTree = groupStore.GetTree(GroupTreeType.Queues, Nothing, Nothing, False, False, False)

            Dim passiveQueues = allQueues.GetFilteredView(QueueGroupMember.PassiveFilter)
            mQueueNode = New TreeNode(ResMan.GetString("ctlControlRoom_tv_queue_management"))
            mQueueNode.Name = mQueueNode.Text
            SetNodeImage(mQueueNode, ImageLists.Keys.ControlRoom.QueueManagement)
            mQueueNode.Tag = passiveQueues

            mActiveQueueNode = New TreeNode(ResMan.GetString("ctlControlRoom_tv_active_queues"))
            mActiveQueueNode.Name = mActiveQueueNode.Text
            SetNodeImage(mActiveQueueNode, ImageLists.Keys.ControlRoom.QueueManagement)


            If canViewQueues Then
                Dim controllableActiveQueues = gSv.GetControllableActiveQueueIds()
                mControlRoomTree.Nodes.Add(mQueueNode)
                FillQueueTree(passiveQueues.Root, mQueueNode)

                Dim activeQueues = allQueues.GetFilteredView(QueueGroupMember.ActiveAndControllableFilter(controllableActiveQueues))
                mControlRoomTree.Nodes.Add(mActiveQueueNode)
                FillQueueTree(activeQueues.Root, mActiveQueueNode)
                mActiveQueueNode.Tag = activeQueues
            End If

        Else
            If canViewQueues Then

                Dim passiveQueues = DirectCast(mQueueNode.Tag, IGroupTree)
                Dim activeQueues = DirectCast(mActiveQueueNode.Tag, IGroupTree)

                ' Both filtered views work from the same tree, so reloading one will, in
                ' effect, reload the other too
                passiveQueues.Reload()

                ' Also need to refresh the filter which contains a list of controllable active queues.
                Dim controllableActiveQueues = gSv.GetControllableActiveQueueIds()
                activeQueues = activeQueues.RawTree.GetFilteredView(QueueGroupMember.ActiveAndControllableFilter(controllableActiveQueues))
                mActiveQueueNode.Tag = activeQueues

                FillQueueTree(passiveQueues.Root, mQueueNode)
                FillQueueTree(activeQueues.Root, mActiveQueueNode)
            End If

        End If

        ' If an Active Queue node is selected, ensure it's detail panel is
        ' refreshed in case of changes to queue accessibility
        If mControlRoomTree.SelectedNode.GetRootNode() Is mActiveQueueNode Then
            ChangePanel(mControlRoomTree.SelectedNode)
        End If

    End Sub

    ''' <summary>
    ''' Populates the schedule subtree of the navigation tree in Control
    ''' </summary>
    Private Sub PopulateScheduleTree()

        Dim canViewSchedules = User.Current.HasPermission("Scheduler") AndAlso
            Licensing.License.CanUse(LicenseUse.Scheduler)

        mSchedulerNode = New TreeNode(ResMan.GetString("ctlControlRoom_tv_scheduler"))
        If canViewSchedules Then mControlRoomTree.Nodes.Add(mSchedulerNode)

        mSchedulerNode.Name = mSchedulerNode.Text
        SetNodeImage(mSchedulerNode, ImageLists.Keys.ControlRoom.Scheduler)

        ' The diary entry sources for (all schedules) timetables and reports.
        Dim ttSource As New ScheduleListEntrySource(
         New ScheduleList(ScheduleListType.Timetable, mScheduleStore))

        Dim reportSource As New ScheduleListEntrySource(
         New ScheduleList(ScheduleListType.Report, mScheduleStore))

        ' Create the report node - set the tag to be the diary entry source for reports,
        ' and add all the report lists found on the database
        mReportCategoryNode = mSchedulerNode.Nodes.Add(ResMan.GetString("ctlControlRoom_tv_reports"))
        mReportCategoryNode.Name = "Reports"
        SetNodeImage(mReportCategoryNode, ImageLists.Keys.ControlRoom.ScheduleReport)
        mReportCategoryNode.Tag = reportSource
        Dim configOptions = Options.Instance
        For Each report As ScheduleList In mScheduleStore.GetAllLists(ScheduleListType.Report)
            report.Name = LTools.Get(report.Name, "misc", configOptions.CurrentLocale)
            report.Description = LTools.Get(report.Description, "misc", configOptions.CurrentLocale)
            AddListNode(report, False)
        Next

        ' Create the timetable node - set the tag to be the diary entry source for timetables,
        ' and add all the timetable lists found on the database
        mTimetableCategoryNode = mSchedulerNode.Nodes.Add(ResMan.GetString("ctlControlRoom_tv_timetables"))
        mTimetableCategoryNode.Name = "Timetables"
        SetNodeImage(mTimetableCategoryNode, ImageLists.Keys.ControlRoom.ScheduleTimetable)
        mTimetableCategoryNode.Tag = ttSource

        For Each timetable As ScheduleList In mScheduleStore.GetAllLists(ScheduleListType.Timetable)
            timetable.Name = LTools.Get(timetable.Name, "misc", configOptions.CurrentLocale)
            timetable.Description = LTools.Get(timetable.Description, "misc", configOptions.CurrentLocale)
            AddListNode(timetable, False)
        Next


        ' Create the schedules node, set the tag to be the diary entry source combining
        ' reports for the past and timetables for the future into a single source
        ' Then add all (active) schedules to the node.
        mScheduleCategoryNode = mSchedulerNode.Nodes.Add(ResMan.GetString("ctlControlRoom_tv_schedules"))
        mScheduleCategoryNode.Name = "Schedules"
        SetNodeImage(mScheduleCategoryNode, ImageLists.Keys.ControlRoom.Schedule)
        mScheduleCategoryNode.Tag = New CompoundScheduleListEntrySource(reportSource, ttSource)
        Dim scheduleNode As TreeNode = mScheduleCategoryNode.Nodes.Add("")
        scheduleNode.Name = ""

        mRetiredCategoryNode = mSchedulerNode.Nodes.Add(ResMan.GetString("ctlControlRoom_tv_retired_schedules"))
        mRetiredCategoryNode.Name = "Retired Schedules"
        SetNodeImage(mRetiredCategoryNode, ImageLists.Keys.ControlRoom.RetiredSchedule)

        scheduleNode = mRetiredCategoryNode.Nodes.Add("")
        scheduleNode.Name = ""

    End Sub

    ''' <summary>
    ''' Populates the main tree of the Control Room panel
    ''' </summary>
    Private Sub PopulateTree()

        Try
            mControlRoomTree.BeginUpdate()
            PopulateSessionManagementTree()
            PopulateQueueTrees()
            PopulateScheduleTree()
            PopulateDataGatewaysTree()

            mExpandingTreeNodes = True

            mScheduleCategoryNode.Collapse()
            mRetiredCategoryNode.Collapse()

            mExpandingTreeNodes = False

            ' We start on the node which the user has permission to view (ie. that which
            ' is the first we find that has been added to the control room treeview)
            Dim selNode As TreeNode = Nothing
            Select Case True
                Case mSessionNode.TreeView IsNot Nothing : selNode = mSessionNode
                Case mQueueNode.TreeView IsNot Nothing : selNode = mQueueNode
                Case mActiveQueueNode.TreeView IsNot Nothing : selNode = mActiveQueueNode
                Case mSchedulerNode.TreeView IsNot Nothing : selNode = mSchedulerNode
            End Select
            If selNode IsNot Nothing Then mControlRoomTree.SelectedNode = selNode

        Finally
            ' Open up the root node
            LoadExpandedControlRoomState()
            mControlRoomTree.EndUpdate()

        End Try

    End Sub

    Private Sub PopulateDataGatewaysTree()
        If Not User.Current.HasPermission(Permission.SystemManager.DataGateways.ControlRoom) Then Return

        Dim treeNodeName = ResMan.GetString("ctlControlRoom_tv_data_gateways")
        mDataGatewaysNode = New TreeNode With
            {
                .Name = treeNodeName,
                .Text = treeNodeName
            }

        mControlRoomTree.Nodes.Add(mDataGatewaysNode)
        mDataGatewaysNode.Name = mDataGatewaysNode.Text

        If gSv.ErrorOnDataGatewayProcess() Then
            SetDataGatewayIconError()
        Else
            SetDataGatewayIconOk()
        End If
    End Sub

    Public Sub ToggleDataGatewaysIcon()
        If DataGatewayProcessError Then
            SetDataGatewayIconOk()
        Else
            SetDataGatewayIconError()
        End If
    End Sub

    Private Sub SetDataGatewayIconError()
        SetNodeImage(mDataGatewaysNode, ImageLists.Keys.ControlRoom.DataPipelinesError)
        DataGatewayProcessError = True
    End Sub

    Private Sub SetDataGatewayIconOk()
        SetNodeImage(mDataGatewaysNode, ImageLists.Keys.ControlRoom.DataPipelinesOk)
        DataGatewayProcessError = False
    End Sub

    ''' <summary>
    ''' Gets the node directly within the specified node collection, which represents
    ''' the given group member
    ''' </summary>
    ''' <param name="nodes">The nodes to search</param>
    ''' <param name="mem">The group member to look for</param>
    ''' <returns>The node which represents the given group member or null if there
    ''' was no such node.</returns>
    Private Function FindNode(nodes As TreeNodeCollection, mem As IGroupMember) _
     As TreeNode
        For Each n As TreeNode In nodes
            If Object.Equals(mem, n.Tag) Then Return n
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Recursively searches the given node collection for a treenode which matches
    ''' a given predicate.
    ''' </summary>
    ''' <param name="nodes">The node collection to recursively search.</param>
    ''' <param name="pred">The predicate which must be satisfied to return the node.
    ''' </param>
    ''' <returns>The first node encountered in a depth-first search which satisfies
    ''' <paramref name="pred"/></returns>
    Private Function SearchNode(
     nodes As TreeNodeCollection, pred As Predicate(Of TreeNode)) As TreeNode
        For Each n As TreeNode In nodes
            If pred(n) Then Return n
            Dim found As TreeNode = SearchNode(n.Nodes, pred)
            If found IsNot Nothing Then Return found
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Finds the insert point for a group member in a node collection. This method
    ''' assumes that the nodes are currently in order, and that each node contains an
    ''' instance of <see cref="IGroupMember"/> to compare against.
    ''' </summary>
    ''' <param name="nodes">The node collection to find the insert point in</param>
    ''' <param name="mem">The member which is to be inserted</param>
    ''' <returns>The index at which the member should be inserted</returns>
    Private Function FindInsertPoint(nodes As TreeNodeCollection, mem As IGroupMember) _
     As Integer
        For i As Integer = 0 To nodes.Count - 1
            Dim currMem As IGroupMember = TryCast(nodes(i).Tag, IGroupMember)
            If currMem Is Nothing Then Continue For
            If currMem.CompareTo(mem) > 0 Then Return i
        Next
        Return nodes.Count
    End Function

    ''' <summary>
    ''' Populates the queue subtree at the given node from the given root.
    ''' </summary>
    ''' <param name="gp">The group to scan to populate the tree node's contents with.
    ''' </param>
    ''' <param name="gpNode">The node representing the group under which the contents
    ''' of the group should be added.
    ''' </param>
    Private Sub FillQueueTree(gp As IGroup, gpNode As TreeNode)
        Dim validNodes As New HashSet(Of TreeNode)
        For Each mem As IGroupMember In gp
            ' Get the current node for this member, if one exists
            Dim child As TreeNode = FindNode(gpNode.Nodes, mem)

            ' See if this member is a group
            Dim memGp As IGroup = TryCast(mem, IGroup)

            ' If it is an empty group, don't bother showing it
            If memGp IsNot Nothing AndAlso
             Not memGp.ContainsInSubtree(GroupMemberType.Queue) Then
                ' If there is a node representing this member, remove it
                If child IsNot Nothing Then
                    ' Change the selection to the parent if it is currently selected
                    If child.IsSelected Then mControlRoomTree.SelectedNode = gpNode
                    child.Remove()
                End If
                ' Next member in the group
                Continue For
            End If

            ' If no node exists for this member, add one to the group node
            ' Whether a new or existing node, update with the latest details from
            ' the model.
            If child Is Nothing Then child =
                gpNode.Nodes.Insert(FindInsertPoint(gpNode.Nodes, mem), mem.Name)

            With child
                .Tag = mem
                .Text = mem.Name
                .Name = mem.Name
                SetNodeImage(child, mem.ImageKey)
                If mem.IsGroup Then FillQueueTree(DirectCast(mem, IGroup), child)
            End With

            ' We've dealt with this node - add it to our store of 'valid' nodes, so
            ' we can remove any which don't exist in the model any more when we're
            ' done
            validNodes.Add(child)
        Next

        ' Check all the nodes under the group node and remove any which do not exist
        ' in the model any more (ie. any which are not in the 'validNodes' set)
        gpNode.Nodes.OfType(Of TreeNode)() _
                    .Where(Function(x) Not validNodes.Contains(x)) _
                    .ToList _
                    .ForEach(Sub(y) gpNode.Nodes.Remove(y))

    End Sub

    ''' <summary>
    ''' Delegate for calling Populate on the ctlScheduleListPanel
    ''' </summary>
    ''' <param name="pan">The panel on which to call the populate method.</param>
    ''' <param name="list">The list with which populate() should be called.
    ''' </param>
    Private Delegate Sub ListPopulateInvoker(
     ByVal pan As ctlScheduleListPanel, ByVal list As ScheduleList)

    ''' <summary>
    ''' Populates the given list control with the specified list.
    ''' </summary>
    ''' <param name="pan">The panel to populate.</param>
    ''' <param name="list">The list with which to populate it.</param>
    Private Sub PopulateList(ByVal pan As ctlScheduleListPanel, ByVal list As ScheduleList)

        If pan Is Nothing Then Exit Sub

        If pan.InvokeRequired Then
            pan.Invoke(New ListPopulateInvoker(AddressOf PopulateList), pan, list)
        Else
            If list Is SelectedScheduleList Then pan.Populate(list)
        End If
    End Sub

#End Region

#Region "Finding nodes"

    ''' <summary>
    ''' Finds the treenode corresponding to the given schedule if it exists within
    ''' this schedule manager
    ''' </summary>
    ''' <param name="sched">The schedule to find the node for</param>
    ''' <returns>The node to which the given schedule is currently assigned, or null
    ''' if no such node was found.</returns>
    Private Function FindScheduleNode(ByVal sched As ISchedule) As TreeNode
        For Each n As TreeNode In mScheduleCategoryNode.Nodes
            If n.Tag Is sched Then Return n
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Finds the treenode corresponding to the given task if it exists within this
    ''' schedule manager
    ''' </summary>
    ''' <param name="task">The task to find the node for</param>
    ''' <returns>The node to which the given task is currently assigned, or null if
    ''' no such node was found.</returns>
    Private Function FindTaskNode(ByVal task As ScheduledTask) As TreeNode
        Dim schedNode As TreeNode = FindScheduleNode(task.Owner)
        If schedNode IsNot Nothing Then
            For Each n As TreeNode In schedNode.Nodes
                If n.Tag Is task Then Return n
            Next
        End If
        Return Nothing
    End Function

    ''' <summary>
    ''' Finds the treenode corresponding to the given list if it exists within this
    '''  schedule manager
    ''' </summary>
    ''' <param name="list">The list to find the node for</param>
    ''' <returns>The node to which the given list is currently assigned, or null if
    '''  no such node was found.</returns>
    Private Function FindListNode(ByVal list As ScheduleList) As TreeNode
        Dim root As TreeNode
        If list.ListType = ScheduleListType.Report Then
            root = mReportCategoryNode
        Else
            root = mTimetableCategoryNode
        End If
        For Each n As TreeNode In root.Nodes
            If n.Tag Is list Then
                Return n
            End If
        Next
        Return Nothing
    End Function

#End Region

#Region "Node Marking - showing data changes"

    ''' <summary>
    ''' Checks if the given node is marked as changed or not.
    ''' </summary>
    ''' <param name="node">The node to check to see if it has changed.</param>
    ''' <returns>True if the node is marked by this class, false otherwise.
    ''' </returns>
    Private Function IsNodeMarked(ByVal node As TreeNode) As Boolean
        Return (node IsNot Nothing AndAlso node.ToolTipText <> "")
    End Function

    ''' <summary>
    ''' Checks if any nodes are currently marked as changed
    ''' </summary>
    ''' <returns>True if any of the timetable, report or schedule nodes are marked
    ''' as changed; False otherwise.</returns>
    Private Function IsAnyNodeMarked() As Boolean
        Return IsAnyNodeMarked(mTimetableCategoryNode) OrElse
         IsAnyNodeMarked(mReportCategoryNode) OrElse
         IsAnyNodeMarked(mScheduleCategoryNode)
    End Function

    ''' <summary>
    ''' Checks if any nodes within the given node are currently marked as changed.
    ''' Note that this only covers the immediate children of the specified node -
    ''' it doesn't recurse throughout all its descendents.
    ''' </summary>
    ''' <param name="parent">The tree node whose children should be checked.</param>
    ''' <returns>True if any of the children of the given node are marked.</returns>
    Private Function IsAnyNodeMarked(ByVal parent As TreeNode) As Boolean
        If parent IsNot Nothing Then
            For Each node As TreeNode In parent.Nodes
                If IsNodeMarked(node) Then Return True
            Next
        End If

        Return False
    End Function

    ''' <summary>
    ''' Delegate method used to invoke node-alteration operations from a non-UI
    ''' thread
    ''' </summary>
    ''' <param name="node">The node to performs some operation on.</param>
    Private Delegate Sub HandleNode(ByVal node As TreeNode)

    ''' <summary>
    ''' Marks the given node as having changed data.
    ''' </summary>
    ''' <param name="node">The node to mark as being changed.</param>
    Private Sub MarkNode(ByVal node As TreeNode)
        If node IsNot Nothing AndAlso Not IsNodeMarked(node) Then
            If InvokeRequired Then
                node.TreeView.BeginInvoke(New HandleNode(AddressOf ReallyMarkNode), node)
            Else
                ReallyMarkNode(node)
            End If
        End If
    End Sub

    ''' <summary>
    ''' Actually does the work of marking the node - this allows the operation to
    ''' be invoked on the UI thread which created the treeview.
    ''' </summary>
    ''' <param name="node">The node to 'really mark'</param>
    Private Sub ReallyMarkNode(ByVal node As TreeNode)
        node.ToolTipText = My.Resources.ctlControlRoom_DataChanged
        ' The 'asterisk' method.
        node.Text = String.Format(My.Resources.ctlControlRoom_MarkNodeAsChanged0, node.Text)
        ' The 'italic' method
        'node.NodeFont = GetItalicFont()
    End Sub

    ''' <summary>
    ''' Clears the given node indicating that it no longer holds changed data.
    ''' </summary>
    ''' <param name="node">The node to clear</param>
    Private Sub ClearNode(ByVal node As TreeNode)
        If node IsNot Nothing And IsNodeMarked(node) Then
            If InvokeRequired Then
                node.TreeView.BeginInvoke(New HandleNode(AddressOf ReallyClearNode), node)
            Else
                ReallyClearNode(node)
            End If
        End If
    End Sub

    ''' <summary>
    ''' Actually does the work of clearing the node - this allows the operation to
    ''' be invoked on the UI thread which created the treeview.
    ''' </summary>
    ''' <param name="n">The node to 'really clear'</param>
    Private Sub ReallyClearNode(ByVal n As TreeNode)
        n.ToolTipText = ""
        ' The 'asterisk' method
        n.Text = n.Text.Substring(2)
        ' The 'italic' method
        'node.NodeFont = GetRegularFont()
    End Sub

    ''' <summary>
    ''' Marks the given schedule as having been changed.
    ''' </summary>
    ''' <param name="sched">The schedule whose data has changed.</param>
    Private Sub MarkScheduleChanged(ByVal sched As SessionRunnerSchedule)

        If sched Is Nothing Then Return

        sched.Mark()
        MarkNode(FindScheduleNode(sched))
        ' If there are changes, assume that the schedule has become current.
        ShowAsExpired(sched, False)

    End Sub

    ''' <summary>
    ''' Sets the expired state of the given schedule to the given value.
    ''' This would normally just be the value of the 
    ''' <see cref="SessionRunnerSchedule.Expired"/> property, but the caller may
    ''' wish to override that property (if, for example, the user has modified the
    ''' schedule).
    ''' </summary>
    ''' <param name="sched">The schedule to set to show as expired or otherwise 
    ''' within the tree.</param>
    ''' <param name="expired">True to indicate that the schedule should display as
    ''' expired within the tree; False to indicate that it should display as active.
    ''' </param>
    Private Sub ShowAsExpired(ByVal sched As SessionRunnerSchedule, ByVal expired As Boolean)

        If sched Is Nothing Then Return

        Dim node As TreeNode = FindScheduleNode(sched)
        If node Is Nothing Then Return

        Dim col As Color
        If expired Then col = GetExpiredNodeForeColor() Else col = GetNormalNodeForeColor()

        If node.ForeColor <> col Then
            node.ForeColor = col
            For Each n As TreeNode In node.Nodes
                n.ForeColor = col
            Next
        End If

    End Sub

    ''' <summary>
    ''' Marks the given list as having been changed.
    ''' </summary>
    ''' <param name="sender">The list in question</param>
    ''' <param name="args">The arguments detailing the change</param>
    Private Sub MarkListChanged(ByVal sender As Object, ByVal args As DataChangeEventArgs)
        MarkNode(FindListNode(TryCast(sender, ScheduleList)))
    End Sub

    ''' <summary>
    ''' Sets the text in the tree node which may be marked as having data changed.
    ''' </summary>
    ''' <param name="n">The node whose text should be changed</param>
    ''' <param name="text">The text to set in the node.</param>
    Private Sub SetMarkableNodeText(ByVal n As TreeNode, ByVal text As String)
        If IsNodeMarked(n) Then
            n.Text = String.Format(My.Resources.ctlControlRoom_MarkNodeAsChanged0, text)
        Else
            n.Text = text
        End If
        ' Either way, the name is set to the new text
        n.Name = text
    End Sub

    ''' <summary>
    ''' Clears the data changed state of the given schedule.
    ''' </summary>
    ''' <param name="sched">The schedule whose data changed status should be
    ''' cleared.</param>
    Private Sub ClearScheduleChanged(ByVal sched As SessionRunnerSchedule)
        If InvokeRequired Then Invoke(Sub() ClearScheduleChanged(sched)) : Return
        sched.ResetChanged()
        ClearNode(FindScheduleNode(sched))
        ShowAsExpired(sched, sched.Expired)
    End Sub

#End Region

#Region "Object creation"

    ''' <summary>
    ''' Tests to see if the node with a given name exists in the tree under the
    ''' given category. Note that this expects any objects with names held in the
    ''' tree under this category to be subclasses of 
    ''' <see cref="DescribedNamedObject"/>. This covers schedules, tasks and lists
    ''' since they all extend it.
    ''' </summary>
    ''' <param name="category">The category to search</param>
    ''' <param name="name">The name to search for</param>
    Private Function NameExistsInTree(ByVal category As TreeNode, ByVal name As String) As Boolean
        For Each n As TreeNode In category.Nodes
            Dim obj As DescribedNamedObject = TryCast(n.Tag, DescribedNamedObject)
            If obj IsNot Nothing AndAlso obj.Name = name Then Return True
        Next
        Return False
    End Function

    ''' <summary>
    ''' Checks if the given name already exists as a schedule within this control.
    ''' </summary>
    ''' <param name="name">The potential name of a schedule to check.</param>
    ''' <returns>True if the given schedule name already exists within this control.
    ''' </returns>
    Private Function NameExistsInNodes(
     ByVal name As String, ByVal ParamArray nodes() As TreeNode) As Boolean
        For Each n As TreeNode In nodes
            If NameExistsInTree(n, name) Then Return True
        Next
        Return False
    End Function

    ''' <summary>
    ''' Gets a unique name based on the given format string, unique within all of
    ''' the provided nodes.
    ''' If the format variable "{0}" is present, it will be populated with a 
    ''' blank first, then with an ascending integer starting with 1, until a unique
    ''' schedule name is found. If no variable is present, the formatting string
    ''' " ({0})" is appended for the first integer value (1).
    ''' </summary>
    ''' <param name="formatString">The format string to use for the schedule name.
    ''' </param>
    ''' <param name="nodes">The nodes within which the name must be unique.</param>
    ''' <returns>The first unique name found based on the given format string.
    ''' </returns>
    Private Function GetUniqueName(
     ByVal formatString As String, ByVal ParamArray nodes() As TreeNode) As String

        ' If no numeric placeholder is specified, see if the raw name exists
        If Not formatString.Contains("{0}") Then
            If Not NameExistsInNodes(formatString, nodes) Then Return formatString
            ' It's already there, append our numeric placeholder to the format
            ' string and continue on.
            formatString &= My.Resources.ctlControlRoom_GetUniqueName0
        End If

        Dim name As String = Nothing
        ' Count up from 1 - MAX, checking each time to see if the name exists.
        For i As Integer = 1 To Integer.MaxValue
            name = String.Format(formatString, i).Trim()
            If Not NameExistsInNodes(name, nodes) Then Return name
        Next

        Throw New OverflowException(String.Format(My.Resources.ctlControlRoom_RanOutOfPotentialNamesWith0, name))

    End Function

    ''' <summary>
    ''' Handles the creation of a new schedule.
    ''' </summary>
    Private Sub HandleCreateSchedule(ByVal sender As Object, ByVal e As EventArgs)
        CreateSchedule()
    End Sub

    ''' <summary>
    ''' Handles the creation of a new task.
    ''' </summary>
    Private Sub HandleCreateTask(ByVal sender As Object, ByVal e As EventArgs)
        CreateTask()
    End Sub

    ''' <summary>
    ''' Handles the deleting of the currently selected task.
    ''' </summary>
    Private Sub HandleDeleteTask(ByVal sender As Object, ByVal e As EventArgs)
        DeleteSelectedTask()
    End Sub

    ''' <summary>
    ''' Creates a new schedule
    ''' </summary>
    Private Sub CreateSchedule()

        If AlertIfNoPermission("Create Schedule") Then
            Dim sched As New SessionRunnerSchedule(mScheduleStore.Owner)

            sched.Name = GetUniqueName(My.Resources.ctlControlRoom_NewSchedule, mScheduleCategoryNode, mRetiredCategoryNode)

            Dim md As New TriggerMetaData()
            md.IsUserTrigger = True
            sched.SetTrigger(TriggerFactory.GetInstance().CreateTrigger(md))
            sched.Add(sched.NewTask(), True)

            ' If we haven't expanded this yet we better had now
            LoadSchedules()

            ' It will have created the task, but it will be hidden in the
            ' contracted tree node, expand it so the user can see the task
            Dim node As TreeNode = AddScheduleNode(sched, True)
            node.Expand()
            MarkNode(node)

        End If
    End Sub

    Private Sub CreateScheduleListAndAddToTree(newScheduleName As String,
                                               newScheduleType As ScheduleListType)
        If AlertIfNoPermission("Create Schedule") Then
            Dim newSchedule = New ScheduleList(mScheduleStore)
            newSchedule.ListType = newScheduleType
            newSchedule.Name = GetUniqueName(newScheduleName, GetCategoryNode(newSchedule))
            MarkNode(AddListNode(newSchedule, True))
        End If
    End Sub

    ''' <summary>
    ''' Adds the given list node to the appropriate node in the tree, autoselecting
    ''' it as specified.
    ''' </summary>
    ''' <param name="list">The list to add a node for. null values are ignored.
    ''' </param>
    ''' <param name="autoselect">True to automatically select the node, false to
    ''' leave it unselected.</param>
    ''' <returns>The node that was created for the given list, or null if no node was
    ''' created for any reason.... eg. if the given list was null.</returns>
    Private Function AddListNode(
     ByVal list As ScheduleList, ByVal autoselect As Boolean) As TreeNode

        If list Is Nothing Then Return Nothing

        Dim listNode As TreeNode = GetCategoryNode(list).Nodes.Add(list.Name)
        listNode.Name = list.Name
        SetNodeImage(listNode, listNode.Parent.ImageKey)
        listNode.Tag = list

        If autoselect Then mControlRoomTree.SelectedNode = listNode

        Return listNode

    End Function

    ''' <summary>
    ''' Adds the given schedule node and any task nodes for the tasks it contains,
    ''' autoselecting the new schedule node as specified.
    ''' If the provided schedule is expired, it will display the schedule name in
    ''' the <see cref="GetExpiredNodeForeColor">expired forecolour</see>.
    ''' </summary>
    ''' <param name="sched">The schedule to add a node for.</param>
    ''' <param name="autoselect">True to automatically select the node, false to
    ''' leave it unselected.</param>
    ''' <returns>The node that was created for the given schedule, or null if no
    ''' node was created for any reason.... can't think why that might be just now.
    ''' </returns>
    Private Function AddScheduleNode(
     ByVal sched As SessionRunnerSchedule, ByVal autoselect As Boolean) As TreeNode

        If sched Is Nothing Then Return Nothing

        Dim schedNode As TreeNode = mScheduleCategoryNode.Nodes.Add(sched.Name)
        schedNode.Name = sched.Name
        SetNodeImage(schedNode, ImageLists.Keys.ControlRoom.Schedule)
        schedNode.Tag = sched

        ' Assume that we want this to appear expired if the schedule is expired.
        If sched.Expired Then
            schedNode.ForeColor = GetExpiredNodeForeColor()
        End If

        ' Add all the task nodes too - don't autoselect
        For Each task As ScheduledTask In sched
            AddTaskNode(sched, task, False)
        Next

        If autoselect Then mControlRoomTree.SelectedNode = schedNode

        Return schedNode

    End Function

    ''' <summary>
    ''' Adds the given task node to the specified schedule, ensuring that the UI is
    ''' kept up to date with the new data changes. This will autoselect the given
    ''' task, based on the given <paramref name="autoselect"/> value.
    ''' </summary>
    ''' <param name="sched">The schedule to which the task should be added.</param>
    ''' <param name="task">The task to add to the schedule.</param>
    ''' <param name="autoselect">True to automatically select the task in the tree
    ''' after it is added, false otherwise.</param>
    ''' <returns>The node that was created, or null if no node was created.
    ''' </returns>
    ''' <remarks>Note that this does not change any data - it's purely for updating
    ''' the UI.</remarks>
    Private Function AddTaskNode(
     ByVal sched As SessionRunnerSchedule, ByVal task As ScheduledTask, ByVal autoselect As Boolean) _
     As TreeNode

        Dim schedNode As TreeNode = FindScheduleNode(sched)
        If schedNode Is Nothing Then Return Nothing

        Dim taskNode As TreeNode = schedNode.Nodes.Add(task.Name)
        taskNode.Name = task.Name
        SetNodeImage(taskNode, ImageLists.Keys.ControlRoom.SessionTask)
        taskNode.Tag = task
        taskNode.ForeColor = schedNode.ForeColor

        If autoselect Then mControlRoomTree.SelectedNode = taskNode

        Return taskNode

    End Function

    ''' <summary>
    ''' Creates a new task and adds it to the currently selected schedule
    ''' </summary>
    Private Sub CreateTask()
        CreateTask(True)
    End Sub

    ''' <summary>
    ''' Creates a new task and adds it to the currently selected schedule
    ''' </summary>
    ''' <param name="autoselect">True to have the newly created task node selected
    ''' automatically; false to leave the currently selected node as the selected one
    ''' </param>
    Private Sub CreateTask(ByVal autoselect As Boolean)
        If AlertIfNoPermission("Create Schedule") Then
            Dim sched As SessionRunnerSchedule = SelectedSchedule
            If sched Is Nothing Then
                Dim currTask As ScheduledTask = SelectedTask
                If currTask IsNot Nothing Then sched = currTask.Owner
            End If
            If sched Is Nothing OrElse sched.Retired Then Return
            Dim task As ScheduledTask = sched.NewTask()
            If sched.Add(task, sched.InitialTask Is Nothing) Then
                AddTaskNode(sched, task, autoselect)
            End If
        End If
    End Sub

    Private Sub SetNodeImage(n As TreeNode, imgKey As String, Optional selImgKey As String = Nothing)
        n.ImageKey = imgKey
        n.SelectedImageKey = CStr(If(selImgKey, imgKey))
    End Sub

#End Region

#Region "Object deletion / retirement"

    Private Sub StopSchedule()
        Dim schedule = SelectedSchedule
        If schedule Is Nothing Then Return

        mScheduleStore.StopRunningSchedule(schedule)
    End Sub

    ''' <summary>
    ''' Deletes an existing schedule
    ''' </summary>
    Private Sub DeleteSchedule()
        ' If the user does not have the correct permissions, do not permit them
        If Not AlertIfNoPermission("Delete Schedule") Then Exit Sub

        Dim schedule As SessionRunnerSchedule = SelectedSchedule

        ' No schedule selected - no work to do
        If schedule Is Nothing Then Exit Sub

        ' Ensure that the user wants to delete the schedule
        If gSv.GetPref(UI.ShowDeleteSchedulePopup, True) Then
            ' If 'Yes' is not selected, do nothing
            If HandleDeleteScheduleConfirmationDialog() <> DialogResult.Yes Then Exit Sub
        End If

        Try
            ' Delete it from the backing store
            mScheduleStore.DeleteSchedule(schedule)
            ' Clear the panel first so that we don't try and save the changes
            ClearDetailPanel(Of ctlSchedule)()
            ' Then remove the node from the tree
            mControlRoomTree.SelectedNode.Remove()

        Catch ex As Exception
            UserMessage.Show(ctlControlRoom_AnErrorOccurredWhileAttemptingToDeleteTheSchedule, ex)
        End Try
    End Sub

    ''' <summary>
    ''' Displays the Delete Schedule <see cref="YesNoCheckboxPopupForm"/>.
    ''' </summary>
    ''' <returns>The <see cref="DialogResult"/> from the <see cref="YesNoCheckboxPopupForm"/>.</returns>
    Private Function HandleDeleteScheduleConfirmationDialog() As DialogResult
        Dim msg As New YesNoCheckboxPopupForm(AreYouSure, ctlControlRoom_AreYouSureYouWantToDeleteThisSchedule,
                DontShowThisMessageAgain)

        AddHandler msg.OnYesButtonClick, AddressOf DisableDeleteSchedulePopupForm

        msg.ShowDialog()

        Return msg.DialogResult
    End Function

    ''' <summary>
    ''' Retires a schedule
    ''' </summary>
    Private Sub RetireSchedule()
        If AlertIfNoPermission("Retire Schedule") Then
            Dim schedule As SessionRunnerSchedule = SelectedSchedule
            If schedule Is Nothing Then Exit Sub
            Try
                If schedule.Id <> 0 Then mScheduleStore.RetireSchedule(schedule)
                Dim n As TreeNode = mControlRoomTree.SelectedNode
                n.Remove()
                mRetiredCategoryNode.Nodes.Add(n)
                mControlRoomTree.SelectedNode = n

            Catch ex As Exception
                UserMessage.Show(My.Resources.ctlControlRoom_AnErrorOccurredWhileAttemptingToRetireTheSchedule, ex)

            End Try
        End If
    End Sub

    ''' <summary>
    ''' Unretires a schedule
    ''' </summary>
    Private Sub UnretireSchedule()
        If AlertIfNoPermission("Retire Schedule") Then
            Dim schedule As SessionRunnerSchedule = SelectedSchedule
            If schedule Is Nothing Then Exit Sub
            Try
                If schedule.Id <> 0 Then mScheduleStore.UnretireSchedule(schedule)
                Dim n As TreeNode = mControlRoomTree.SelectedNode
                n.Remove()
                mScheduleCategoryNode.Nodes.Add(n)
                For Each task As TreeNode In n.Nodes
                    SetNodeImage(task, ImageLists.Keys.ControlRoom.SessionTask)
                Next
                mControlRoomTree.SelectedNode = n

            Catch ex As Exception
                UserMessage.Show(My.Resources.ctlControlRoom_AnErrorOccurredWhileAttemptingToUnretireTheSchedule, ex)

            End Try

        End If
    End Sub

    ''' <summary>
    ''' Deletes a task from the schedule
    ''' </summary>
    Private Sub DeleteSelectedTask()
        DeleteTask(SelectedTask)
    End Sub

    ''' <summary>
    ''' Deletes the given task from its schedule.
    ''' </summary>
    ''' <param name="task">The task to delete, if the user has the correct
    ''' permissions and the task is not null and can be deleted.</param>
    Private Sub DeleteTask(ByVal task As ScheduledTask)
        If task Is Nothing Then Return
        If AlertIfNoPermission("Delete Schedule") Then
            If mScheduleStore.CanDeleteTask(task) Then
                MarkScheduleChanged(task.Owner)
                task.Owner.Remove(task) ' remove from its schedule
                mControlRoomTree.SelectedNode.Remove()
            End If
        End If
    End Sub

    ''' <summary>
    ''' Deletes a schedule list
    ''' </summary>
    Private Sub DeleteScheduleList()
        If AlertIfNoPermission("Delete Schedule") Then
            Dim list As ScheduleList = SelectedScheduleList
            If list Is Nothing Then Exit Sub
            Try
                If list.ID <> 0 Then mScheduleStore.DeleteScheduleList(list)
                mControlRoomTree.SelectedNode.Remove()

            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.ctlControlRoom_AnErrorOccurredWhileAttemptingToDeleteThe0,
                 list.ListType.ToString().ToLower()), ex)

            End Try

        End If
    End Sub

#End Region

#Region "Selected <n> properties"

    ''' <summary>
    ''' Gets the tag from the currently selected node in the schedule manager tree,
    ''' if a node is currently selected, and if it has a tag.
    ''' </summary>
    ''' <returns>The tag from the currently selected node in the tree, or null if
    ''' there is no currently selected node, or the selected node has no tag data.
    ''' </returns>
    Private ReadOnly Property SelectedTag() As Object
        Get
            Dim n As TreeNode = mControlRoomTree.SelectedNode
            If n IsNot Nothing Then Return n.Tag Else Return Nothing
        End Get
    End Property

    ''' <summary>
    ''' Gets the currently selected session view.
    ''' </summary>
    Friend ReadOnly Property SelectedSessionView As SessionView
        Get
            Return TryCast(SelectedTag, SessionView)
        End Get
    End Property

    ''' <summary>
    ''' Gets the currently selected session view value, or null if no session view is
    ''' currently selected.
    ''' </summary>
    Public ReadOnly Property SelectedSessionViewValue() As String
        Get
            Return TryCast(SelectedTag, String)
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the currently selected queue as a group member object.
    ''' </summary>
    Public Property SelectedQueueIdent As Integer
        Get
            Dim qm = TryCast(SelectedTag, QueueGroupMember)
            Return If(qm Is Nothing, 0, qm.IdAsInteger)
        End Get
        Set(value As Integer)
            ' Setting to 0 doesn't make sense - what would we select instead?
            If value = 0 Then Return

            ' Search for the node which matches the value to set to, searching both
            ' trees to make sure we find it if it's there at all
            Dim seeker =
                Function(qn As TreeNode) As Boolean
                    Dim qm = TryCast(qn.Tag, QueueGroupMember)
                    Return (qm IsNot Nothing AndAlso
                            qm.IdAsInteger = value)
                End Function

            Dim n = SearchNode(mQueueNode.Nodes, seeker)
            If n Is Nothing Then n = SearchNode(mActiveQueueNode.Nodes, seeker)

            ' If we find it, select it; if not, do nothing
            If n IsNot Nothing Then
                n.EnsureVisible()
                mControlRoomTree.SelectedNode = n
            End If

        End Set
    End Property

    ''' <summary>
    ''' Gets the currently selected schedule
    ''' </summary>
    ''' <returns>The schedule whose node is currently selected in the tree view,
    ''' or Nothing if a schedule is not currently selected.</returns>
    Public ReadOnly Property SelectedSchedule() As SessionRunnerSchedule
        Get
            Return TryCast(SelectedTag, SessionRunnerSchedule)
        End Get
    End Property

    ''' <summary>
    ''' Gets the currently selected task, or null if no task is currently selected.
    ''' </summary>
    ''' <returns>The task currently selected in the tree view, or null if a task is
    ''' not selected.</returns>
    Public ReadOnly Property SelectedTask() As ScheduledTask
        Get
            Return TryCast(SelectedTag, ScheduledTask)
        End Get
    End Property

    ''' <summary>
    ''' Gets the currently selected schedule list
    ''' </summary>
    ''' <returns>The schedule list whose node is currently selected in the tree view,
    ''' or Nothing if a schedule list is not currently selected.</returns>
    Public ReadOnly Property SelectedScheduleList() As ScheduleList
        Get
            Return TryCast(SelectedTag, ScheduleList)
        End Get
    End Property

#End Region

#Region "Saving data"

    ''' <summary>
    ''' Saves all changed data represented in this schedule manager.
    ''' </summary>
    Private Function SaveAll() As Boolean
        Return CBool(ProgressDialog.Show(
         Me, New SaverOfAll(Me), My.Resources.ctlControlRoom_ScheduleManager, My.Resources.ctlControlRoom_SavingAllData).Result)
    End Function

    ''' <summary>
    ''' Saves all changed data represented in this schedule manager, ignoring the
    ''' result.
    ''' </summary>
    Private Sub PerformSaveAll()
        If AlertIfNoPermission("Edit Schedule", "Create Schedule", "Delete Schedule") Then
            SaveAll()
        End If
    End Sub

    ''' <summary>
    ''' Saves the schedule
    ''' </summary>
    Private Sub SaveSchedule()
        SaveSchedule(SelectedSchedule)
    End Sub

    ''' <summary>
    ''' Saves the given schedule
    ''' </summary>
    ''' <param name="schedule">The schedule to save</param>
    Public Function SaveSchedule(schedule As SessionRunnerSchedule) As Boolean

        If schedule Is Nothing Then Return True ' no-op if no schedule to save.

        Try
            ' make sure the data is up to date in the object model
            If Not CommitChanges() Then Return False

            Dim scheduleLoopsInfinitely = schedule.IsLoopingSchedule()

            If scheduleLoopsInfinitely Then
                Dim warningMessage As New YesNoPopupForm(AreYouSure,
                                                 ctlControlRoom_ScheduleInifiniteLoopYesNoWarning,
                                                 String.Empty) With {.ShowInTaskbar = False}

                If warningMessage.ShowDialog(gMainForm) = DialogResult.No Then
                    Return False
                End If
            End If

            ' Save on the DB
            schedule.Owner.Store.SaveSchedule(schedule)
            ' Reset both the node and the schedule data
            ClearScheduleChanged(schedule)
            Return True

        Catch e As Exception
            Dim showErr = Sub() UserMessage.Show(
             My.Resources.ctlControlRoom_TheFollowingErrorOccurredWhileTryingToSaveTheSchedule &
             vbCrLf & e.Message, e)
            If InvokeRequired Then Invoke(showErr) Else showErr()
            Return False

        End Try

    End Function

    ''' <summary>
    ''' Saves the ScheduleList
    ''' </summary>
    Private Sub SaveScheduleList()
        SaveScheduleList(SelectedScheduleList)
    End Sub

    ''' <summary>
    ''' Saves the given ScheduleList
    ''' </summary>
    ''' <param name="list">The ScheduleList to save</param>
    Public Sub SaveScheduleList(ByVal list As ScheduleList)
        Try
            mScheduleStore.SaveScheduleList(list)
            PopulateList(GetPanel(Of ctlScheduleListPanel)(), list)
            ClearNode(FindListNode(list))
        Catch nameException As BluePrism.Server.Domain.Models.NameAlreadyExistsException
            UserMessage.Show(nameException.Message)
        Catch ex As Exception
            UserMessage.Show(String.Format(AnErrorOccurredSaving0, list.Name), ex)
        End Try
    End Sub

    Public Sub DisableDeleteSchedulePopupForm(sender As Object, e As EventArgs)
        Dim popup = CType(sender, YesNoCheckboxPopupForm)
        RemoveHandler popup.OnYesButtonClick, AddressOf DisableDeleteSchedulePopupForm

        If popup.IsChecked Then gSv.SetUserPref(UI.ShowDeleteSchedulePopup, False)

        popup.Close()
    End Sub

#End Region

#Region "Navigating / Changing view"

    ''' <summary>
    ''' Creates a view of T and adds it to the given panel, if the panel is already
    ''' hosting a view of this type, then that will be returned instead of a new one.
    ''' </summary>
    ''' <typeparam name="T">The type of view to create</typeparam>
    ''' <param name="addToPanel">The panel to add the view to.</param>
    ''' <returns>The control that was added</returns>
    Private Function CreateView(Of T As {Control, New})(ByVal addToPanel As Panel) As T
        Dim control As T = Nothing
        If addToPanel.HasChildren Then control = TryCast(addToPanel.Controls(0), T)

        If control Is Nothing Then
            addToPanel.SuspendLayout()
            ' We don't want this to be disposed when the addToPanel contents are disposed
            addToPanel.Controls.Remove(mMenuButton)
            addToPanel.Controls.Remove(lblAreaTitle)

            ' Now, dispose of and remove all the addToPanel controls
            addToPanel.Controls.Clear(True)

            control = New T()
            Dim child As IChild = TryCast(control, IChild)
            If child IsNot Nothing Then
                child.ParentAppForm = mParent
            End If
            addToPanel.Controls.Add(control)
            control.Dock = DockStyle.Fill
            addToPanel.Controls.Add(mMenuButton)
            addToPanel.Controls.Add(lblAreaTitle)

            Dim modder As IScheduleModifier = TryCast(control, IScheduleModifier)
            If modder IsNot Nothing Then
                AddHandler modder.ScheduleDataChange, AddressOf MarkScheduleChanged
            End If
            addToPanel.ResumeLayout()
        End If
        Return control
    End Function

    ''' <summary>
    ''' Ensures that all currently uncommitted changes are commited when the user
    ''' navigates away from the current node
    ''' </summary>
    Private Sub HandleBeforeSelect(
     ByVal sender As Object, ByVal e As TreeViewCancelEventArgs) _
     Handles mControlRoomTree.BeforeSelect
        If Not CommitChanges() Then e.Cancel = True
    End Sub

    ''' <summary>
    ''' Handles clicking of the nodes in the scheduler treeview, and loads the 
    ''' appropriate page
    ''' </summary>
    Public Sub HandleAfterSelect(
     ByVal sender As Object, ByVal e As TreeViewEventArgs) _
     Handles mControlRoomTree.AfterSelect

        If e.Node Is Nothing Then Return

        Dim root As TreeNode = e.Node.GetRootNode()
        If root Is mSessionNode Then
            mControlRoomTree.ContextMenuStrip = mSessionTreeContextMenu
        ElseIf root Is mQueueNode OrElse root Is mActiveQueueNode Then
            mControlRoomTree.ContextMenuStrip = mQueueTreeContextMenu
        Else ' Must be scheduler related
            mControlRoomTree.ContextMenuStrip = mScheduleTreeContextMenu
        End If

        lblAreaTitle.Text = GetNodeTitle(e.Node)

        Dim ctl As Control = ChangePanel(e.Node)
        ' Bit of an ugly hack to force focus to shift to the new control after the
        ' select is complete... TreeView just nabs it back if you just set focus as
        ' normal - found on :-
        If root Is mSchedulerNode Then clsUserInterfaceUtils.SetFocusDelayed(ctl)

    End Sub

    Private ReadOnly Property DetailPanel As Panel
        Get
            Return mSplitter.Panel2
        End Get
    End Property

    Private ReadOnly Property DetailControl As Control
        Get
            Dim p = DetailPanel
            If p Is Nothing Then Return Nothing
            If p.Controls.Count = 0 Then Return Nothing
            Return p.Controls(0)
        End Get
    End Property

    ''' <summary>
    ''' Changes the panel to the correct one depending on the node specified
    ''' </summary>
    ''' <param name="node">The node by which to decide which panel to change to.
    ''' </param>
    ''' <returns>The control to which the panel was changed</returns>
    Private Function ChangePanel(ByVal node As TreeNode) As Control

        Dim panel As Panel = DetailPanel

        ' Clear the session management panel if we're changing - it doesn't remove
        ' the control from the hosting panel, but it ensures that if we're not
        ' remaining within session management, we're not listening for its events
        panSessionManagement = Nothing

        Dim schedule = TryCast(node.Tag, SessionRunnerSchedule)
        If schedule IsNot Nothing Then
            Dim schPanel As ctlSchedule = CreateView(Of ctlSchedule)(panel)
            schPanel.ReadOnly = IsNodeInRetiredCategory(node) OrElse
             Not User.Current.HasPermission(Permission.Scheduler.EditSchedule)
            AddHandler schedule.NameChanging, AddressOf ScheduleNameChanging
            schPanel.Schedule = schedule
            Return schPanel
        End If

        Dim list As ScheduleList = TryCast(node.Tag, ScheduleList)
        If list IsNot Nothing Then
            AddHandler list.Events.NameChanging, AddressOf ListNameChanging
            AddHandler list.DataChanged, AddressOf MarkListChanged

            Dim listPanel As ctlScheduleListPanel
            If node.Parent Is mReportCategoryNode Then
                listPanel = CreateView(Of ctlScheduleReport)(panel)
            ElseIf node.Parent Is mTimetableCategoryNode Then
                listPanel = CreateView(Of ctlScheduleTimetable)(panel)
            Else
                Throw New InvalidOperationException(My.Resources.ctlControlRoom_InvalidParentNodeForThisChild)
            End If
            listPanel.Populate(list)
            Return listPanel
        End If

        Dim ltask As ScheduledTask = TryCast(node.Tag, ScheduledTask)
        If ltask IsNot Nothing Then
            Dim taskPanel As ctlTask = CreateView(Of ctlTask)(panel)

            taskPanel.ReadOnly = IsNodeInRetiredCategory(node) OrElse
             Not User.Current.HasPermission(Permission.Scheduler.EditSchedule)

            taskPanel.Task = ltask
            AddHandler taskPanel.NameChanged, AddressOf TaskNameChanged
            Return taskPanel
        End If

        If node Is mSessionNode OrElse node.Parent Is mSessionNode Then
            panSessionManagement = CreateView(Of ctlSessionManagement)(panel)
            panSessionManagement.ViewStateEncoded = CStr(node.Tag)
            Return panSessionManagement
        End If

        If node.GetRootNode() Is mQueueNode Then
            Dim qpanel = CreateView(Of ctlWorkQueueManagement)(panel)
            ' We don't want to double-deal with a change; if the user's is changing
            ' the selected queue via the node, don't deal with it again when the
            ' change comes back from the queue panel.
            AddHandler qpanel.SelectedQueueChanged,
                Sub(sender, e) _
                    SelectedQueueIdent = e.QueueIdent

            Dim mem As QueueGroupMember = TryCast(node.Tag, QueueGroupMember)
            Dim gp As IGroup = TryCast(node.Tag, IGroup)
            Dim t As IGroupTree = TryCast(node.Tag, IGroupTree)

            ' If it's a tree (ie. root node selected) use the root as our group
            If t IsNot Nothing Then gp = t.Root

            If gp IsNot Nothing Then
                qpanel.Group = gp
                qpanel.SelectQueue(Nothing)

            ElseIf mem IsNot Nothing Then
                qpanel.Group = mem.Owner
                qpanel.SelectQueue(mem)

            End If
            Return qpanel
        End If

        If node.GetRootNode() Is mActiveQueueNode Then
            Dim pan = CreateView(Of ActiveQueuePanel)(panel)

            ' Get the group or member associated with this node
            Dim qm As QueueGroupMember = TryCast(node.Tag, QueueGroupMember)
            Dim gp As IGroup = TryCast(node.Tag, IGroup)
            Dim t As IGroupTree = TryCast(node.Tag, IGroupTree)

            ' If it's a tree (ie. root node selected) use the root as our group
            If t IsNot Nothing Then gp = t.Root

            If qm Is Nothing AndAlso gp Is Nothing Then Return pan
            If gp Is Nothing Then gp = qm.Owner

            pan.ActiveQueueIdents =
             gp.OfType(Of QueueGroupMember).Select(Function(m) m.IdAsInteger).ToList()

            If qm IsNot Nothing Then pan.SelectedQueueIdent = qm.IdAsInteger

            Return pan
        End If

        If node Is mDataGatewaysNode Then
            Dim panelToDisplay = CreateView(Of ctlDataGateways)(panel)
            panelToDisplay.ControlRoom = Me

            Return panelToDisplay
        End If

        ' Set up the diary view control in any of the overview controls.
        Dim ov As ctlSchedulerOverview = CreateView(Of ctlSchedulerOverview)(panel)
        Dim dv As WeekView = ov.DiaryView

        Dim src As IDiaryEntrySource = TryCast(node.Tag, IDiaryEntrySource)
        dv.Visible = (src IsNot Nothing)
        If src IsNot Nothing Then

            AddHandler dv.DiaryEntryClick, AddressOf HandleDiaryEntryClick
            dv.DateFormat = Resources.DateTimeFormat_ctlControlRoom_WeekViewDateFormat

            dv.Source = src
            If node Is mScheduleCategoryNode Then
                dv.StartDate = Today.AddDays(-3)
                ' For schedules, the source is a report up until now, and a timetable
                ' from now onwards - create the distinction by showing <now in one
                ' colour, and > now in a different one.
                dv.Highlighter = New ThresholdHighlighter(Now, Nothing, CellHighlighter.DefaultColour)

            ElseIf node Is mTimetableCategoryNode Then
                dv.StartDate = Today
                ' Just 'lowlight' the weekend, and highlight weekdays between 9 and 5 as 
                ' 'working hours'
                dv.Highlighter = New MergingHighlighter(
                 New DaysHighlighter(Color.FromArgb(&HFA, &HFA, &HFA), DayOfWeek.Saturday, DayOfWeek.Sunday),
                 New TimeRangeHighlighter(New TimeSpan(9, 0, 0), New TimeSpan(17, 0, 0))
                )

            ElseIf node Is mReportCategoryNode Then
                dv.StartDate = Today.AddDays(-6)
                ' For reports just 'lowlight' anything from 'now' onwards...
                dv.Highlighter = New ThresholdHighlighter(Now, Nothing, Color.FromArgb(&HF0, &HF0, &HF0))

            End If
            ' Ensure that all entries that fall in the date range are visible without
            ' having to resize the cells, and scroll to 09:00
            dv.Pack()
            dv.ScrollTimeIntoView(New TimeSpan(9, 0, 0))

        End If

        Return ov

    End Function

    Private Sub HandleDiaryEntryClick(ByVal sender As Object, ByVal e As DiaryEntryClickedEventArgs)
        Dim inst As IScheduleInstance = TryCast(e.FirstEntry, IScheduleInstance)
        If inst IsNot Nothing Then
            SelectSchedule(inst.Schedule)
        End If
    End Sub

    Private Sub HandleViewStateChange() Handles panSessionManagement.ViewStateChanged
        With mControlRoomTree.SelectedNode
            If .Tag Is Nothing Then .Tag = panSessionManagement.ViewStateEncoded
        End With
    End Sub

    ''' <summary>
    ''' Handles the user requesting the saving of the session view.
    ''' </summary>
    Private Sub HandleSaveSessViewClick() Handles panSessionManagement.SaveViewClick

        ' Check we have a selected node, if not, default to top "session" node
        Dim selectedNode = If(mControlRoomTree.SelectedNode, mSessionNode)
        Dim currentName = selectedNode.Name

        ' Force default name if none is found or the node is the root "session" node.
        Dim initialNameForSavePrompt As String
        If (selectedNode Is mSessionNode OrElse String.IsNullOrEmpty(currentName)) Then
            initialNameForSavePrompt = BPUtil.FindUnique(
                My.Resources.ctlControlRoom_SessionView0, AddressOf mSessionNode.Nodes.ContainsKey)
        Else
            initialNameForSavePrompt = currentName
        End If

        ' See what the user wants to call it
        Dim name = frmInputBox.GetText(
            My.Resources.ctlControlRoom_SaveSessionViewAs, My.Resources.ctlControlRoom_SaveSessionView, initialNameForSavePrompt, 255)

        ' Null indicates the user cancelled
        If String.IsNullOrEmpty(name) Then Return

        ' If they've changed the name to one which already exists, check that they
        ' are okay with overwriting it
        If name = currentName OrElse mSessionNode.Nodes.ContainsKey(name) Then
            Dim response = MessageBox.Show(
                String.Format(My.Resources.ctlControlRoom_OverwriteExistingSessionViewNamed0, name),
                My.Resources.ctlControlRoom_OverwriteSessionView,
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question)

            ' If it's not okay, end this whole charade right now
            If response <> DialogResult.OK Then Return
        End If

        ' If we're here, that means we have a name and the nod to save the view
        ' Get the view state that we're saving
        Dim viewState = panSessionManagement.ViewStateEncoded

        ' Get the node for this view
        Dim viewNode = mSessionNode.Nodes(name)

        ' If it's not there, create it
        If viewNode Is Nothing Then
            viewNode = mSessionNode.Nodes.Add(name)
            viewNode.Name = name
            SetNodeImage(viewNode, ImageLists.Keys.ControlRoom.SessionFilter)
            ' Update list of views in the database
            gSv.SetUserPref(PreferenceNames.Session.SessionViewList,
                            CollectionUtil.Join(SessionViews, vbLf))
        End If
        viewNode.Tag = viewState

        ' Update the view itself
        gSv.SetUserPref(PreferenceNames.Session.SessionViewPrefix & name,
                        panSessionManagement.ViewStateEncoded)

        mControlRoomTree.SelectedNode = viewNode

    End Sub

    ''' <summary>
    ''' Gets a collection of the session views held in this control
    ''' </summary>
    Private ReadOnly Property SessionViews As ICollection(Of String)
        Get
            Dim views As New List(Of String)()
            For Each n As TreeNode In mSessionNode.Nodes : views.Add(n.Name) : Next
            Return views
        End Get
    End Property

    ''' <summary>
    ''' Changes the title of the Schedule manager depending on the node selected
    ''' </summary>
    ''' <param name="node">The node by which to decide which title to change to.
    ''' </param>
    Private Function GetNodeTitle(ByVal node As TreeNode) As String

        Dim rootish As TreeNode = node.GetClosestMatch(
            Function(n)
                Return n.Parent Is Nothing OrElse
                    n Is mTimetableCategoryNode OrElse
                    n Is mReportCategoryNode
            End Function)

        Select Case True
            Case rootish Is mSchedulerNode
                Return My.Resources.ctlControlRoom_SchedulesConfigureSchedulesAndAssociatedTasks
            Case rootish Is mTimetableCategoryNode
                Return My.Resources.ctlControlRoom_TimetablesViewSchedulesWhichAreSetUpToRunInTheFuture
            Case rootish Is mReportCategoryNode
                Return My.Resources.ctlControlRoom_ReportsViewSchedulesAndTasksThatHaveRunInThePast
            Case rootish Is mSessionNode
                Return My.Resources.ctlControlRoom_SessionsControlCurrentlyRunningSessions
            Case rootish Is mQueueNode, rootish Is mActiveQueueNode
                Return My.Resources.ctlControlRoom_QueuesConfigureWorkQueues
            Case rootish Is mDataGatewaysNode
                Return My.Resources.ctlControlRoom_DataGateways
            Case Else
                Return My.Resources.ctlControlRoom_ControlSessionsQueuesAndCreateAndMaintainSchedules
        End Select

    End Function

    ''' <summary>
    ''' Commits the changes of the currently visible schedule
    ''' </summary>
    ''' <returns>True if the changes were successfully committed or there were no
    ''' changes to commit; False if a validation error occurred while attempting
    ''' to commit the changes.</returns>
    Private Function CommitChanges() As Boolean
        If InvokeRequired Then Return CBool(Invoke(Function() CommitChanges()))

        Dim schPanel As ctlSchedule = GetPanel(Of ctlSchedule)()
        If schPanel IsNot Nothing Then Return schPanel.CommitIntervalChanges()

        ' If this is currently the session node, we want to save the last selected
        ' values in it and save in database
        If panSessionManagement IsNot Nothing AndAlso User.LoggedIn AndAlso mControlRoomTree.SelectedNode Is mSessionNode Then
            mSessionNode.Tag = panSessionManagement.ViewStateEncoded
            gSv.SetUserPref(PreferenceNames.Session.SessionViewDefault, panSessionManagement.ViewStateEncoded)
        End If
        Return True
    End Function

    ''' <summary>
    ''' Gets the current detail panel of the expected type in the right pane of the
    ''' main split panel.
    ''' If there is no detail panel present, or the panel is not of the specified
    ''' type, null is returned.
    ''' </summary>
    ''' <typeparam name="T">The type of detail panel which is required by this call.
    ''' </typeparam>
    ''' <returns>The detail panel of the specified type which is currently displayed
    ''' within this control, or null if no detail panel is displayed or if the panel
    ''' displayed is of a different type to that specified.</returns>
    Private Function GetPanel(Of T As Control)() As T
        With mSplitter.Panel2
            If .HasChildren Then Return TryCast(.Controls(0), T)
        End With
        Return Nothing
    End Function

    ''' <summary>
    ''' Clears the detail panel of the specified type of control. If the detail panel
    ''' currently displayed is of a different type to that specified, then no action
    ''' will occur as a result of this method.
    ''' </summary>
    ''' <typeparam name="T">The type of detail panel to clear</typeparam>
    Private Sub ClearDetailPanel(Of T As Control)()
        Dim chk As T = GetPanel(Of T)()
        If chk IsNot Nothing Then mSplitter.Panel2.Controls.Clear()
    End Sub

    ''' <summary>
    ''' Selects a given schedule in the schedule treeview
    ''' </summary>
    ''' <param name="schedule">The schedule to select</param>
    Public Sub SelectSchedule(ByVal schedule As ISchedule)
        For Each n As TreeNode In mScheduleCategoryNode.Nodes
            If DirectCast(schedule, Object).Equals(n.Tag) Then
                mControlRoomTree.SelectedNode = n
            End If
        Next
    End Sub

#End Region

#Region "Context Menu"

    ''' <summary>
    ''' Enables or disabled context menu items depending on the node selected
    ''' </summary>
    Private Sub HandleSchedulerContextMenuOpening(
     ByVal sender As Object, ByVal e As CancelEventArgs) _
     Handles mScheduleTreeContextMenu.Opening

        ' Can't edit? Then there's no context menu for you, young man.
        If Not User.Current.HasPermission(Permission.Scheduler.EditSchedule) Then
            e.Cancel = True
            Return
        End If

        ' Get the node that the context menu is being fired on
        Dim n As TreeNode = mControlRoomTree.SelectedNode

        If IsNodeWithNoMenu(n.Name) Then
            e.Cancel = True
            Return
        End If

        ' Details about the node which are used to figure out the menu which should
        ' be displayed.
        Dim sched As SessionRunnerSchedule =
            TryCast(n.Tag, SessionRunnerSchedule)
        Dim task As ScheduledTask = TryCast(n.Tag, ScheduledTask)
        Dim taskIsDeletable As Boolean =
            (task IsNot Nothing AndAlso mScheduleStore.CanDeleteTask(task))
        Dim list As ScheduleList = TryCast(n.Tag, ScheduleList)

        ' Some permissions for this user.
        Dim u As User = User.Current
        Dim canCreate As Boolean = u.HasPermission(Permission.Scheduler.CreateSchedule)
        Dim canDelete As Boolean = u.HasPermission(Permission.Scheduler.DeleteSchedule)
        Dim canRetire As Boolean = u.HasPermission(Permission.Scheduler.RetireSchedule)

        ' And just some shortcut flags to make it easier to read
        Dim isSchedule As Boolean = (sched IsNot Nothing)
        Dim isTask As Boolean = (task IsNot Nothing)
        Dim isList As Boolean = (list IsNot Nothing)
        Dim isCategoryNode As Boolean = (n Is mScheduleCategoryNode OrElse
         n Is mReportCategoryNode OrElse n Is mTimetableCategoryNode)

        ' "New" is available for schedules and any category node.
        ctxMenuNew.Enabled = canCreate AndAlso (isSchedule OrElse isCategoryNode)

        ' "Save" only allowed on schedules and lists for data that has changed
        ctxMenuSave.Enabled = (isSchedule OrElse isList) AndAlso IsNodeMarked(n)

        ' You can only retire schedules.
        ctxMenuRetire.Enabled = (canRetire AndAlso isSchedule AndAlso Not IsNodeMarked(n))

        ' Delete is available for lists, schedules and tasks which are not referenced in logs
        ctxMenuDelete.Enabled = canDelete AndAlso (
         isSchedule OrElse isList OrElse (isTask AndAlso taskIsDeletable)
        )

        ' You can only clone schedules and lists... for now.
        ctxMenuClone.Enabled = (isSchedule OrElse isList)

        ' And copy/paste is only for tasks
        ctxMenuCopy.Enabled = (isTask)
        ' paste is for pasting tasks - but you paste *into* a schedule, ergo add to to schedule.
        ' it should only be enabled if a task has previously been cut / copied.
        ctxMenuPaste.Enabled = (isSchedule AndAlso mCopiedTask IsNot Nothing)

        ' Run Now is (obviously) only for schedules.
        ' In fact, it makes so little sense elsewhere, it shouldn't even be visible.
        ctxMenuEditRunSeparator.Visible = (isSchedule)
        ctxMenuRunNow.Visible = (isSchedule)
        ctxMenuStop.Visible = (isSchedule)

        ' Some further customisation of the context menu for specific cases.
        If isTask AndAlso Not taskIsDeletable Then
            ' This means that the task cannot be deleted - add a little tooltip
            ' explaining to the user why it's disabled
            ctxMenuDelete.ToolTipText =
             My.Resources.ctlControlRoom_ThisTaskCannotBeDeletedItIsReferencedInAScheduleLog
        Else
            ' Just make sure the tooltip doesn't hang around beyond its welcome
            ctxMenuDelete.ToolTipText = Nothing
        End If

        ' "Retire" changes to "Unretire" if the target is a retired schedule.
        If isSchedule AndAlso n.Parent Is mRetiredCategoryNode Then
            ctxMenuRetire.Text = My.Resources.ctlControlRoom_Unretire
        Else
            ctxMenuRetire.Text = My.Resources.ctlControlRoom_Retire
        End If

        ' Final thing - the "New" text changes depending on which of the categories /
        ' schedules that the user clicked on.
        If isSchedule Then : ctxMenuNew.Text = My.Resources.ctlControlRoom_NewTask
        ElseIf n Is mScheduleCategoryNode Then : ctxMenuNew.Text = My.Resources.ctlControlRoom_NewSchedule
        ElseIf n Is mReportCategoryNode Then : ctxMenuNew.Text = My.Resources.ctlControlRoom_NewReport
        ElseIf n Is mTimetableCategoryNode Then : ctxMenuNew.Text = My.Resources.ctlControlRoom_NewTimetable
        Else : ctxMenuNew.Text = My.Resources.ctlControlRoom_New
        End If

    End Sub

    Private Function IsNodeWithNoMenu(elementName As String) As Boolean
        Return elementName = ResMan.GetString("ctlControlRoom_tv_data_gateways") OrElse
               elementName = ResMan.GetString("ctlControlRoom_tv_scheduler")
    End Function

    ''' <summary>
    ''' Handles the session context menu opening, ensuring that it does not display
    ''' for the root 'session' node.
    ''' </summary>
    Private Sub HandleSessionContextMenuOpening(
     ByVal sender As Object, ByVal e As CancelEventArgs) _
     Handles mSessionTreeContextMenu.Opening
        ' If they're on the 'session' node, there is no context menu
        If mControlRoomTree.SelectedNode Is mSessionNode Then e.Cancel = True
        ' Otherwise, all context menu items are available
    End Sub

    ''' <summary>
    ''' Handles the new context menu item click event
    ''' </summary>
    Private Sub ctxMenuNew_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles ctxMenuNew.Click
        Dim n As TreeNode = mControlRoomTree.SelectedNode
        If n IsNot Nothing Then
            If n Is mScheduleCategoryNode Then
                CreateSchedule()
            End If
            If n Is mReportCategoryNode Then
                CreateScheduleListAndAddToTree(ctlControlRoom_Report0,
                                               ScheduleListType.Report)
            End If
            If n Is mTimetableCategoryNode Then
                CreateScheduleListAndAddToTree(ctlControlRoom_Timetable0,
                                               ScheduleListType.Timetable)
            End If
            If TryCast(n.Tag, SessionRunnerSchedule) IsNot Nothing Then
                CreateTask()
            End If
        End If
    End Sub

    ''' <summary>
    ''' Handles the Save menu item being clicked on the context menu.
    ''' </summary>
    Private Sub ctxMenuSave_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles ctxMenuSave.Click
        If SelectedSchedule IsNot Nothing Then
            SaveSchedule()
        ElseIf SelectedScheduleList IsNot Nothing Then
            SaveScheduleList()
        End If
    End Sub

    Private Sub ctxMenuStop_Click(ByVal sender As Object, ByVal e As EventArgs) _
        Handles ctxMenuStop.Click
        StopSchedule()
    End Sub

    ''' <summary>
    ''' Handles the delete context menu item click event
    ''' </summary>
    Private Sub ctxMenuDelete_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles ctxMenuDelete.Click
        If SelectedSchedule IsNot Nothing Then
            DeleteSchedule()
        ElseIf SelectedTask IsNot Nothing Then
            DeleteSelectedTask()
        ElseIf SelectedScheduleList IsNot Nothing Then
            DeleteScheduleList()
        End If
    End Sub

    Private Sub ctxMenuRetire_Click(ByVal sender As Object, ByVal e As EventArgs) Handles ctxMenuRetire.Click
        If SelectedSchedule IsNot Nothing Then
            Dim n As TreeNode = mControlRoomTree.SelectedNode
            If n.Parent Is mScheduleCategoryNode Then
                RetireSchedule()
            ElseIf n.Parent Is mRetiredCategoryNode Then
                UnretireSchedule()
            End If
        End If
    End Sub

    ''' <summary>
    ''' Handles the Run Now context menu item click.
    ''' </summary>
    Private Sub ctxMenuRunNow_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles ctxMenuRunNow.Click

        Dim sched As SessionRunnerSchedule = SelectedSchedule
        If sched Is Nothing Then Return
        If sched.HasChanged() Then
            Dim result As MsgBoxResult = UserMessage.OkCancel(
             My.Resources.ctlControlRoom_ThisScheduleHasUnsavedChangesItMustBeSavedBeforeItIsExecutedSaveNow)
            If result <> MsgBoxResult.Ok Then Return
        End If
        ' Okay - either user has agreed to save, or it contains no unsaved data.
        ' Either way add our little system trigger and set it to go.
        mScheduleStore.TriggerSchedule(sched)

    End Sub

    ''' <summary>
    ''' Handles toggling of expand/collapse for queue nodes
    ''' </summary>
    Private Sub ctxMenuExpandCollapseQueues(sender As Object, e As EventArgs) _
     Handles mnuExpandAll.Click, mnuCollapseAll.Click
        If sender Is mnuExpandAll Then
            mQueueNode.ExpandAll()
            mActiveQueueNode.ExpandAll()
        Else
            mQueueNode.Collapse(False)
            mActiveQueueNode.Collapse(False)
        End If
    End Sub

#End Region

#Region "Other event handlers"

    ''' <summary>
    ''' Lazy loads the schedules
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub mControlRoomTree_BeforeExpand(sender As Object, e As TreeViewCancelEventArgs) Handles mControlRoomTree.BeforeExpand
        If Not mExpandingTreeNodes AndAlso
           e.Node IsNot Nothing Then

            If e.Node.Name = mRetiredCategoryNode.Name Then
                LoadRetiredSchedules()
            ElseIf e.Node.Name = mScheduleCategoryNode.Name Then
                LoadSchedules()
            End If
        End If

    End Sub
    ''' <summary>
    ''' Initialises this control, populating the data as required.
    ''' </summary>
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        If Not DesignMode Then
            Try
                mScheduleStore = New DatabaseBackedScheduleStore(gSv)
                PopulateTree()
                AddAction(My.Resources.ctlControlRoom_ApplyChanges, AddressOf PerformSaveAll)
                If Not User.Current.HasPermission(Permission.Scheduler.EditSchedule) Then
                    SetActionEnabled(My.Resources.ctlControlRoom_ApplyChanges, False)
                End If
            Catch ex As Exception
                UserMessage.Err(ex,
                 My.Resources.ctlControlRoom_AnErrorOccurredInitialisingTheControlTree0, ex.Message)
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Handles the event which occurs immediately before a label edit, ensuring that
    ''' they only occur for session view nodes.
    ''' </summary>
    Private Sub HandleLabelBeforeEdit(sender As Object, e As NodeLabelEditEventArgs) _
     Handles mControlRoomTree.BeforeLabelEdit
        ' Right now, we only support this for session view nodes - they're easy to
        ' check for. Disallow anything that isn't a session view node.
        If e.Node.Parent IsNot mSessionNode Then e.CancelEdit = True
    End Sub

    ''' <summary>
    ''' Handles the event immediately after a label edit, updating the node's name
    ''' and saving the change to the database
    ''' </summary>
    Private Sub HandleAfterLabelEdit(sender As Object, e As NodeLabelEditEventArgs) _
     Handles mControlRoomTree.AfterLabelEdit
        Dim node = e.Node
        ' If nothing's changed then end this charade right now
        If node.Name = e.Label OrElse e.Label Is Nothing Then
            e.CancelEdit = True
            Return
        End If

        Try
            ' Otherwise, we want to delete the old view name
            gSv.DeleteUserPref(PreferenceNames.Session.SessionViewPrefix & node.Name)

            ' Change the name
            node.Name = e.Label

            ' Create this on the database
            ' Just check that we have a viewstate - it may be null (if the node is
            ' the auto-generated 'Today' node)
            Dim state As String = DirectCast(node.Tag, String)
            If state Is Nothing Then
                state = panSessionManagement.ViewStateEncoded
                ' Might as well set this in the tag at the same time so we can just
                ' use it later if necessary
                node.Tag = state
            End If
            gSv.SetUserPref(PreferenceNames.Session.SessionViewPrefix & node.Name, state)

            ' And update the list of views
            gSv.SetUserPref(PreferenceNames.Session.SessionViewList,
                                CollectionUtil.Join(SessionViews, vbLf))

        Catch ex As Exception
            UserMessage.Err(
                    ex, My.Resources.ctlControlRoom_AnErrorOccurredWhileSavingTheViews0, ex.Message)

        End Try
    End Sub

    ''' <summary>
    ''' Handles the schedule name changing event and updates the nodes text 
    ''' accordingly
    ''' </summary>
    ''' <param name="args"></param>
    Private Sub ScheduleNameChanging(ByVal args As ScheduleRenameEventArgs)
        Dim n As TreeNode = FindScheduleNode(args.SourceSchedule)
        If n IsNot Nothing Then SetMarkableNodeText(n, args.NewName)
    End Sub

    ''' <summary>
    ''' Handles the list name changing and updates the node's text accordingly
    ''' </summary>
    ''' <param name="args">The arguments detailing the name event.</param>
    Private Sub ListNameChanging(ByVal args As ListRenameEventArgs)
        Dim n As TreeNode = FindListNode(args.Sender)
        If n IsNot Nothing Then SetMarkableNodeText(n, args.NewName)
    End Sub

    ''' <summary>
    ''' Handles the task name changing event and updates the nodes text 
    ''' accordingly
    ''' </summary>
    ''' <param name="args"></param>
    Private Sub TaskNameChanged(ByVal args As ctlTask.TaskNameChangedEventArgs)
        Dim n As TreeNode = FindTaskNode(args.SourceTask)
        If n IsNot Nothing Then n.Name = args.NewName : n.Text = args.NewName
    End Sub

    ''' <summary>
    ''' Makes the treeview select a node on right click
    ''' </summary>
    Private Sub HandleMouseDown(ByVal sender As Object, ByVal e As MouseEventArgs) _
     Handles mControlRoomTree.MouseDown
        If e.Button = System.Windows.Forms.MouseButtons.Right Then
            Dim node As TreeNode
            node = mControlRoomTree.GetNodeAt(e.X, e.Y)
            If node Is Nothing Then Exit Sub
            mControlRoomTree.SelectedNode = node
        End If
    End Sub

    ''' <summary>
    ''' Handles the user's request to delete a session view.
    ''' </summary>
    Private Sub HandleDeleteSessionView() Handles ctxMenuDeleteSessionView.Click
        Dim n As TreeNode = mControlRoomTree.SelectedNode

        ' Double check with the user
        Dim resp = MessageBox.Show(String.Format(My.Resources.ctlControlRoom_DeleteTheSessionView0, n.Name),
                                   My.Resources.ctlControlRoom_Delete,
                                   MessageBoxButtons.OKCancel,
                                   MessageBoxIcon.Question)
        If resp <> DialogResult.OK Then Return

        ' First set the current viewstate into the session node (so we don't
        ' refresh the view from a delete operation)
        mSessionNode.Tag = n.Tag

        ' Then set the currently selected node to the session node so we have
        ' a known place we're going to (without changing the actual view state)
        mControlRoomTree.SelectedNode = mSessionNode

        ' Save the view name so we can delete it
        Dim viewName As String = n.Name

        ' Delete the node itself
        n.Remove()

        ' and reset the view list to what we currently have displayed
        gSv.SetUserPref(PreferenceNames.Session.SessionViewList,
                        CollectionUtil.Join(SessionViews, vbLf))

        ' and then, er, remove the session view that's been deleted.
        gSv.DeleteUserPref(PreferenceNames.Session.SessionViewPrefix & viewName)

    End Sub

    ''' <summary>
    ''' Handles the 'Rename Session View' context menu click.
    ''' </summary>
    Private Sub HandleRenameSessionView() Handles ctxMenuRenameSessionView.Click
        mControlRoomTree.SelectedNode.BeginEdit()
    End Sub

    Private Sub HandleFolderExpand(sender As Object, e As TreeViewEventArgs) Handles mControlRoomTree.AfterExpand
        Dim mem As IGroupMember = TryCast(e.Node.Tag, IGroupMember)
        If mem Is Nothing Then Return
        SetNodeImage(e.Node, mem.ImageKeyExpanded)
    End Sub

    Private Sub HandleFolderCollapse(sender As Object, e As TreeViewEventArgs) Handles mControlRoomTree.AfterCollapse
        Dim mem As IGroupMember = TryCast(e.Node.Tag, IGroupMember)
        If mem Is Nothing Then Return
        SetNodeImage(e.Node, mem.ImageKey)
    End Sub

    Private Sub RefreshToolStripMenuItem_Click(sender As Object, e As EventArgs) _
        Handles RefreshToolStripMenuItem.Click
        RefreshView()
    End Sub

#End Region

#Region "Action Handling"

    ''' <summary>
    ''' Utility structure to combine link labels with the delegates that
    ''' must be called when those link labels are actioned.
    ''' </summary>
    Private Structure Action
        Private mControl As Control
        Private mDelegate As PerformAction
        Public Sub New(ByVal lab As Control, ByVal deleg As PerformAction)
            mControl = lab
            mDelegate = deleg
        End Sub
        Public ReadOnly Property Control() As Control
            Get
                Return mControl
            End Get
        End Property
        Public ReadOnly Property ActionPerformer() As PerformAction
            Get
                Return mDelegate
            End Get
        End Property
    End Structure

    ''' <summary>
    ''' Dictionary of actions to their corresponding action links.
    ''' </summary>
    Private mActions As New Dictionary(Of String, Action)

    ''' <summary>
    ''' Delegate called when an action link has been clicked in the schedule
    ''' manager.
    ''' </summary>
    Public Delegate Sub PerformAction()

    ''' <summary>
    ''' Adds the given action to this schedule manager, identified by the given
    ''' text, and calling the given ActionPerformed delegate when the resultant
    ''' link is clicked.
    ''' </summary>
    ''' <param name="txt">The text defining the action, and the text of the link
    ''' that is created as a result of adding the action.</param>
    ''' <param name="deleg">The delegate to be called when the link is clicked
    ''' </param>
    Public Sub AddAction(ByVal txt As String, ByVal deleg As PerformAction)
        Dim btn As New AutomateControls.Buttons.StandardStyledButton()
        btn.Text = txt
        btn.Dock = DockStyle.Bottom

        AddHandler btn.Click, AddressOf ControlClicked
        mSplitter.Panel1.Controls.Add(btn)
        mActions(txt) = New Action(btn, deleg)
    End Sub

    ''' <summary>
    ''' Removes the action identified by the given name from the schedule manager
    ''' </summary>
    ''' <param name="txt">The text identifying the action to be removed.</param>
    Public Sub RemoveAction(ByVal txt As String)
        Dim link As Action = mActions(txt)
        mSplitter.Panel1.Controls.Remove(link.Control)
        RemoveHandler link.Control.Click, AddressOf ControlClicked
        mActions.Remove(txt)
    End Sub

    ''' <summary>
    ''' Sets the given action to be enabled or disabled as specified.
    ''' </summary>
    ''' <param name="txt">The text identifying the action to be enabled/disabled.
    ''' </param>
    ''' <param name="enabled">True to enable the action; False to disable it.
    ''' </param>
    Public Sub SetActionEnabled(ByVal txt As String, ByVal enabled As Boolean)
        Try
            Dim link As Action = mActions(txt)
            link.Control.Enabled = enabled
        Catch knfe As KeyNotFoundException ' Ignore
        End Try

    End Sub

    ''' <summary>
    ''' Handler for one of the action links being clicked
    ''' </summary>
    Private Sub ControlClicked(ByVal sender As Object, ByVal e As EventArgs)
        Dim txt As String = CType(sender, Control).Text
        mActions(txt).ActionPerformer().Invoke()
    End Sub

#End Region

#Region "Copying / Pasting / Cloning"

    ''' <summary>
    ''' Handles a task being copied in the GUI.
    ''' </summary>
    Private Sub HandleCopy(ByVal sender As Object, ByVal e As EventArgs) Handles ctxMenuCopy.Click
        CopySelectedTask()
    End Sub

    ''' <summary>
    ''' Stores the currently selected task awaiting a paste into a schedule.
    ''' </summary>
    ''' <remarks>This method originally took a boolean argument indicating whether
    ''' the task should be deleted after it was copied (a "Cut" command rather than
    ''' a "Copy" command), but the data has too many interdependencies to do this in
    ''' an intuitive manner. eg. if you "Cut" and "Paste" a task into the same
    ''' schedule, any tasks pointing to it on success or failure would no longer
    ''' point to it, and, if it was the schedule's initial task, it no longer would 
    ''' be after the paste operation. Better to let the user do this with 2 steps and
    ''' maintain the internal data consistency.
    ''' </remarks>
    Private Sub CopySelectedTask()
        mCopiedTask = SelectedTask
    End Sub

    ''' <summary>
    ''' Handles the pasting of a task into a schedule when the appropriate context
    ''' menu item is clicked.
    ''' </summary>
    Private Sub HandlePaste(ByVal src As Object, ByVal e As EventArgs) Handles ctxMenuPaste.Click

        ' Not copied anything? Then what are you doing here?
        If mCopiedTask Is Nothing Then Return

        Dim sched As SessionRunnerSchedule = SelectedSchedule
        If sched Is Nothing Then Return

        ' Copy the task data-wise
        Dim task As ScheduledTask = mCopiedTask.CopyTo(sched)
        ' reset the 'clipboard' to nada.
        mCopiedTask = Nothing

        ' Didn't copy... nothing else to do.
        If task Is Nothing Then Return

        ' Now handle the GUI, we need to add the new task to the schedule in the
        ' treeview, and select it.
        AddTaskNode(sched, task, True)

        ' make sure the target schedule knows that it's changed and updated the UI appropriately
        MarkScheduleChanged(sched)

    End Sub

    ''' <summary>
    ''' Handles the 'Clone' context menu item being clicked.
    ''' </summary>
    Private Sub HandleClone(ByVal src As Object, ByVal e As EventArgs) Handles ctxMenuClone.Click

        ' Only valid for schedules and lists.
        Dim sched As SessionRunnerSchedule = SelectedSchedule

        If sched IsNot Nothing Then
            CloneSchedule(sched)
        Else
            Dim list As ScheduleList = SelectedScheduleList
            If list IsNot Nothing Then CloneList(list)
        End If

    End Sub

    ''' <summary>
    ''' Clones the given schedule and adds a representation of the schedule and its
    ''' tasks to the schedule manager tree.
    ''' </summary>
    ''' <param name="sched">The schedule to clone. If this is null, it is ignored.
    ''' </param>
    Private Sub CloneSchedule(ByVal sched As SessionRunnerSchedule)
        If sched Is Nothing Then Return
        Dim clone As SessionRunnerSchedule = DirectCast(sched.Copy(), SessionRunnerSchedule)

        ' Make sure the name is unique.
        clone.Name = GetUniqueName(
         String.Format(My.Resources.ctlControlRoom_CopyOf0, sched.Name), mScheduleCategoryNode, mRetiredCategoryNode)

        ' The copy comes back unassigned, so make sure it's assigned to the
        ' appropriate scheduler at this stage.
        clone.Owner = sched.Owner

        ' And add the node to the tree
        MarkNode(AddScheduleNode(clone, True))

    End Sub

    ''' <summary>
    ''' Clones the given list and adds a representation of it to the schedule manager
    ''' tree.
    ''' </summary>
    ''' <param name="list">The list to clone. If this is null it is ignored.</param>
    Private Sub CloneList(ByVal list As ScheduleList)

        If list Is Nothing Then Return
        Dim clone As ScheduleList = list.Copy()

        ' We need to make sure that the name is unique for that type of list.
        clone.Name = GetUniqueName(String.Format(My.Resources.ctlControlRoom_CopyOf0, clone.Name), GetCategoryNode(clone))

        MarkNode(AddListNode(clone, True))

    End Sub

#End Region

    ''' <summary>
    ''' Refreshes the Control page, including the queue tree in the tree view, as
    ''' well as the detail control, if it is refreshable.
    ''' </summary>
    Public Sub RefreshView() Implements IRefreshable.RefreshView
        Try
            PopulateQueueTrees()
            Dim r = TryCast(DetailControl, IRefreshable)
            If r IsNot Nothing Then r.RefreshView()
        Catch ex As Exception
            UserMessage.Err(ex, String.Format(My.Resources.ctlControlRoom_ErrorRefreshingViewAtThisTime0, ex.Message))
            Return
        End Try
    End Sub

    Private Sub HandleImportCompleted(sender As Object, e As EventArgs) _
     Handles mParent.ImportCompleted

        BeginInvoke(New FunctionDelegate(AddressOf RefreshView))
    End Sub

    Private Delegate Sub FunctionDelegate()
    Private Sub LoadRetiredSchedules()

        If mRetiredLoaded Then Return

        mRetiredCategoryNode.Nodes.Clear()

        ' Retired schedules have no diary viewer on them, so add no source to this node.
        ' Just pick up all the retired schedules from the database and add them.
        For Each schedule As SessionRunnerSchedule In mScheduleStore.GetRetiredSchedules()
            Dim scheduleNode As TreeNode = mRetiredCategoryNode.Nodes.Add(schedule.Name)
            scheduleNode.Name = schedule.Name
            SetNodeImage(scheduleNode, ImageLists.Keys.ControlRoom.RetiredSchedule)
            scheduleNode.Tag = schedule
            For Each t As ScheduledTask In schedule
                With scheduleNode.Nodes.Add(t.Name)
                    .Name = t.Name
                    .ImageKey = "task-retired"
                    .SelectedImageKey = .ImageKey
                    .Tag = t
                End With
            Next
        Next

        mRetiredLoaded = True
    End Sub

    Private Sub LoadSchedules()

        If mSchedulesLoaded Then Return

        mScheduleCategoryNode.Nodes.Clear()

        For Each schedule As SessionRunnerSchedule In mScheduleStore.GetActiveSchedules()
            AddScheduleNode(schedule, False)
        Next

        mSchedulesLoaded = True
    End Sub

    Private Sub ctlControlRoom_Leave(sender As Object, e As EventArgs) Handles MyBase.Leave
        SaveExpandedControlRoomTreeState()
    End Sub

    Private Sub ToggleConnectionsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ToggleConnectionsToolStripMenuItem.Click
        panSessionManagement.ToggleDisplayResources()
    End Sub
End Class

