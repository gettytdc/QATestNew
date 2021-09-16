Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Resources.ResourceMachine
Imports BluePrism.Images
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.BPCoreLib
Imports BluePrism.Server.Domain.Models

Public Class ctlResourceManagement : Implements IStubbornChild, IPermission, IHelp, IRefreshable

#Region " Member Variables "

    Private mParent As frmApplication

    Private mActiveResources As IGroupTree
    Private mRetiredResources As IGroupTree

#End Region

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        lstResourcesRetired.SmallImageList = ImageLists.Components_16x16
        lstResourcesRetired.LargeImageList = ImageLists.Components_32x32

        With mResourceGroupTree.ExtraContextMenuItems
            .Add(Me.menuLoggingLevel)
            .Add(Me.menuEventLogEnabled)
            .Add(Me.ResetFQDNToolStripMenuItem)
            .Add(Me.menuSortResources)
        End With
        mResourceGroupTree.ManageAccessRightsEnabled = True
        mResourceGroupTree.ValidateMemberMovements = True

        'ensure "active resource panel" labels are fully displayed if possible, resizing panel if necessary
        Dim hintLocation = New Point(lResourcesHint.Location.X, lResourcesHint.Location.Y)
        hintLocation.X = lResources.Width
        lResourcesHint.Location = hintLocation
        Dim activeLabelsWidth = lResources.Width + lResourcesHint.Width
        Dim availableWidth = (mSplit.Width - mSplit.SplitterWidth) / 2
        If availableWidth < activeLabelsWidth Then
            Dim allLabelsWidth = activeLabelsWidth + mSplit.SplitterWidth + Label1.Width
            If allLabelsWidth <= mSplit.Width Then
                mSplit.SplitterDistance = activeLabelsWidth
            End If
        End If
    End Sub

    ''' <summary>
    ''' Handles the load event for the control.
    ''' </summary>
    ''' <param name="e">The event arguments</param>
    Protected Overrides Sub OnLoad(ByVal e As EventArgs)
        MyBase.OnLoad(e)

        With GetGroupStore().GetTree(GroupTreeType.Resources, Nothing, Nothing, True, True, True)
            mActiveResources = .GetFilteredView(ResourceGroupMember.NotRetiredAndNotDebug, Nothing, True)
            mRetiredResources = .GetFilteredView(ResourceGroupMember.RetiredAndNotDebug)
        End With

        mResourceGroupTree.AddTree(mActiveResources)
        RefreshResources()
    End Sub

    Private Sub RefreshResources()
        mActiveResources.Reload()

        lstResourcesRetired.BeginUpdate()
        lstResourcesRetired.Clear()
        For Each r As ResourceGroupMember In mRetiredResources.Root.FlattenedContents(Of Collections.clsSortedSet(Of IGroupMember))(False)
            With lstResourcesRetired.Items.Add(r.Name)
                .Tag = r
                .ImageKey = r.ImageKey
            End With
        Next
        lstResourcesRetired.EndUpdate()
        mResourceGroupTree.UpdateView()
    End Sub

    ''' <summary>
    ''' Instruct treeview control to refresh
    ''' </summary>
    Private Sub RefreshViewHandle() Handles mResourceGroupTree.RefreshView
        mResourceGroupTree.ClearGroupCache()
        mResourceGroupTree.UpdateView(True)
        RefreshResources()
    End Sub


    ''' <summary>
    ''' Handler for resourcesdropped event.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub lstResourcesRetired_DragDrop(ByVal sender As Object, ByVal e As DragEventArgs) Handles lstResourcesRetired.DragDrop
        Dim resources = e.Data.GetData(Of ICollection(Of IGroupMember))()
        If resources Is Nothing Then Return
        Try
            ProcessDroppedContents(resources, Nothing, True)
            mResourceGroupTree.Refresh()
            mResourceGroupTree.RefreshTreeFilter()
        Catch ex As Exception
            UserMessage.Err(ex,
             My.Resources.ctlResourceManagement_AnErrorOccurredWhileTryingToUnretireTheSelectedItems0,
             ex.Message)
        End Try

    End Sub

    Private Sub mResourceGroupTree_GroupMemberDropped(sender As Object, e As GroupMemberDropEventArgs) Handles mResourceGroupTree.GroupMemberDropped
        Try
            ProcessDroppedContents(e.Contents, e.Target, False)
            mResourceGroupTree.Refresh()
            mResourceGroupTree.RefreshTreeFilter()
        Catch ex As Exception
            UserMessage.Err(ex,
             My.Resources.ctlResourceManagement_AnErrorOccurredWhileTryingToUnretireTheSelectedItems0,
             ex.Message)
        End Try
    End Sub


    Private Sub lstResourcesRetired_DragOver(ByVal sender As Object, ByVal e As DragEventArgs) Handles lstResourcesRetired.DragOver
        Try
            If e.Data.GetDataPresent(GetType(ICollection(Of IGroupMember))) Then
                e.Effect = DragDropEffects.Move
            End If
        Catch
            e.Effect = DragDropEffects.None
        End Try
    End Sub

    Private Sub lstResourcesRetired_ItemDrag(ByVal sender As Object, ByVal e As ItemDragEventArgs) Handles lstResourcesRetired.ItemDrag
        If lstResourcesRetired.SelectedItems.Count > 0 Then
            Dim resources As New List(Of IGroupMember)
            Dim resourcesToRemove As New List(Of ListViewItem)
            For Each it As ListViewItem In lstResourcesRetired.SelectedItems
                Dim res As IGroupMember = TryCast(it.Tag, IGroupMember)
                If res IsNot Nothing Then
                    resources.Add(res)
                    resourcesToRemove.Add(it)
                End If
            Next

            lstResourcesRetired.DoDragDrop(Of ICollection(Of IGroupMember))(resources, DragDropEffects.Move)
            '    For Each res As ListViewItem In resourcesToRemove
            '        res.Remove()
            '    Next
            'End If
        End If
    End Sub

    ''' <summary>
    ''' The currently selected <em>active</em> resource ID
    ''' </summary>
    Private ReadOnly Property SelectedActiveResourceId() As Guid
        Get
            Dim res As ResourceGroupMember = TryCast(mResourceGroupTree.SelectedMember, ResourceGroupMember)
            If res IsNot Nothing Then Return res.IdAsGuid
            Return Nothing
        End Get
    End Property

    ''' <summary>
    ''' Signifies that the event handlers should be skipped because the code is
    ''' updating the checkboxes, and not the user.
    ''' </summary>
    Private mUpdating As Boolean

    ''' <summary>
    ''' Constructs a context menu for configuring resources
    ''' </summary>
    Private Sub ResourcesContextMenu_Opening(ByVal sender As Object, ByVal e As GroupMemberCancelEventArgs) Handles mResourceGroupTree.ContextMenuOpening

        If mResourceGroupTree.SelectedMember Is Nothing Then Return
        Dim canConfigure = mResourceGroupTree.SelectedMember.Permissions.HasPermission(
                            User.Current, Permission.Resources.ConfigureResource)

        If TypeOf e.Target Is IGroup Then
            Dim group As IGroup = TryCast(e.Target, IGroup)
            menuLoggingLevel.Visible = False
            menuEventLogEnabled.Visible = False
            ResetFQDNToolStripMenuItem.Visible = False
            menuSortResources.Visible = True
            If group?.RawGroup?.Any() Then
                menuSortResources.Enabled = True
                SortByLogLevelAscending.Checked = False
                SortByLogLevelDescending.Checked = False
                SortByNameAscending.Checked = False
                SortByNameDescending.Checked = False
                If group.SortFieldName = SortColumnNames.Name.ToString() Then
                    If group.SortOrder = SortOrder.Ascending Then
                        SortByNameAscending.Checked = True
                    ElseIf group.SortOrder = SortOrder.Descending Then
                        SortByNameDescending.Checked = True
                    End If
                ElseIf group.SortFieldName = SortColumnNames.LogLevel.ToString() Then
                    If group.SortOrder = SortOrder.Ascending Then
                        SortByLogLevelAscending.Checked = True
                    ElseIf group.SortOrder = SortOrder.Descending Then
                        SortByLogLevelDescending.Checked = True
                    End If
                End If
            Else
                menuSortResources.Enabled = False
            End If

            Return
        Else
            menuLoggingLevel.Visible = True
            menuEventLogEnabled.Visible = True
            menuSortResources.Visible = False
            menuLoggingLevel.Enabled = canConfigure AndAlso mResourceGroupTree.SelectedNodes.Count = 1
            menuEventLogEnabled.Enabled = canConfigure AndAlso mResourceGroupTree.SelectedNodes.Count = 1
        End If

        Try
            mUpdating = True
            Dim cfg As CombinedConfig = gSv.GetResourceDiagnosticsCombined({SelectedActiveResourceId})

            DefaultToolStripMenuItem.CheckState = CType(cfg.LoggingDefault, CheckState)
            AllStagesToolStripMenuItem.CheckState = CType(cfg.LoggingAllOverride, CheckState)
            KeyStagesToolStripMenuItem.CheckState = CType(cfg.LoggingKeyOverride, CheckState)
            ErrorsOnlyToolStripMenuItem.CheckState = CType(cfg.LoggingErrorsOnlyOverride, CheckState)
            LogMemoryUsageToolStripMenuItem.CheckState = CType(cfg.LoggingMemory, CheckState)
            IncludeMemoryCleanupToolStripMenuItem.CheckState = CType(cfg.LoggingForceGC, CheckState)
            LogWebServiceCommunicationToolStripMenuItem.CheckState = CType(cfg.LoggingWebServices, CheckState)

            ' If no resources have memory usage logged, disable the 'memory cleanup' item
            IncludeMemoryCleanupToolStripMenuItem.Enabled =
             (cfg.LoggingMemory <> CombinedConfig.CombinedState.Disabled)

            menuEventLogEnabled.CheckState = CType(cfg.LoggingToEventLog, CheckState)

            If gSv.GetResourceRegistrationMode() = ResourceRegistrationMode.MachineFQDN Then
                ResetFQDNToolStripMenuItem.Enabled = canConfigure AndAlso gSv.ResourcesHaveFQDN({SelectedActiveResourceId})
                ResetFQDNToolStripMenuItem.Visible = True
            Else
                ResetFQDNToolStripMenuItem.Enabled = False
                ResetFQDNToolStripMenuItem.Visible = False
            End If

        Catch ex As Exception
            UserMessage.Show(My.Resources.ctlResourceManagement_AnErrorOccurredRetrievingTheResourceStatuses, ex)

        Finally
            mUpdating = False
        End Try

    End Sub

    ''' <summary>
    ''' Handle the click event explicitly so that we can change the behaviour from the
    ''' default which is to uncheck interterminate items.
    ''' </summary>
    Private Sub Clicked(ByVal sender As Object, ByVal e As EventArgs) _
        Handles DefaultToolStripMenuItem.Click,
                AllStagesToolStripMenuItem.Click,
                KeyStagesToolStripMenuItem.Click,
                ErrorsOnlyToolStripMenuItem.Click,
                LogMemoryUsageToolStripMenuItem.Click,
                IncludeMemoryCleanupToolStripMenuItem.Click,
                LogWebServiceCommunicationToolStripMenuItem.Click

        Dim item As ToolStripMenuItem = TryCast(sender, ToolStripMenuItem)
        If item IsNot Nothing Then
            Select Case item.CheckState
                Case CheckState.Unchecked : item.CheckState = CheckState.Checked
                Case CheckState.Indeterminate : item.CheckState = CheckState.Checked
                Case Else : item.CheckState = CheckState.Unchecked
            End Select
        End If

    End Sub

    Private Sub SortClicked(ByVal sender As Object, ByVal e As EventArgs) _
        Handles SortByNameAscending.Click,
                SortByNameDescending.Click,
                SortByLogLevelAscending.Click,
                SortByLogLevelDescending.Click

        Dim item As ToolStripMenuItem = TryCast(sender, ToolStripMenuItem)
        If item IsNot Nothing Then
            For Each r As ToolStripMenuItem In item.Owner.Items
                If r.Equals(item) AndAlso r.Checked = False Then
                    r.Checked = True
                Else
                    r.Checked = False
                End If
            Next
        End If

        Dim sortField As String = Nothing
        Dim sortOrder As SortOrder = SortOrder.None
        If item Is SortByNameAscending AndAlso item.Checked Then
            sortField = SortColumnNames.Name.ToString()
            sortOrder = SortOrder.Ascending
        ElseIf item Is SortByNameDescending AndAlso item.Checked Then
            sortField = SortColumnNames.Name.ToString()
            sortOrder = SortOrder.Descending
        ElseIf item Is SortByLogLevelAscending AndAlso item.Checked Then
            sortField = SortColumnNames.LogLevel.ToString()
            sortOrder = SortOrder.Ascending
        ElseIf item Is SortByLogLevelDescending AndAlso item.Checked Then
            sortField = SortColumnNames.LogLevel.ToString()
            sortOrder = SortOrder.Descending
        End If

        Dim group As FilteringGroup = TryCast(mResourceGroupTree.SelectedMembersGroup, FilteringGroup)
        If group IsNot Nothing Then
            Me.SortAndExpand(mActiveResources.RawTree.Root, group.IdAsGuid, sortField, sortOrder)
            mResourceGroupTree.Clear()
            mResourceGroupTree.AddTree(mActiveResources)
            mResourceGroupTree.UpdateView()
        End If
    End Sub

    Private Sub SortAndExpand(group As IGroup, guid As Guid, sortField As String, sortOrder As SortOrder)
        If group IsNot Nothing Then
            Dim node = mResourceGroupTree.GetNodeFor(guid)
            If node IsNot Nothing Then
                group.Expanded = TryCast(node.Tag, IGroup).Expanded
            End If
            If group.IdAsGuid = guid Then
                group.SortFieldName = sortField
                group.SortOrder = sortOrder
            End If
            For Each childGroup As Object In group.RawGroup
                Dim res As IGroup = TryCast(childGroup, IGroup)
                If res IsNot Nothing Then
                    SortAndExpand(res, guid, sortField, sortOrder)
                End If
            Next
        End If
    End Sub

    ''' <summary>
    ''' Update the checkstates according to the users selection
    ''' </summary>
    Private Sub CheckStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
       Handles DefaultToolStripMenuItem.CheckStateChanged,
                AllStagesToolStripMenuItem.CheckStateChanged,
                KeyStagesToolStripMenuItem.CheckStateChanged,
                ErrorsOnlyToolStripMenuItem.CheckStateChanged,
                LogMemoryUsageToolStripMenuItem.CheckStateChanged,
                IncludeMemoryCleanupToolStripMenuItem.CheckStateChanged,
                LogWebServiceCommunicationToolStripMenuItem.CheckStateChanged

        If mUpdating Then Exit Sub

        Try
            Dim logLevel As Integer = 4
            Dim logMemory As Integer = 2
            Dim logForceGC As Integer = 2
            Dim logWebServices As Integer = 2
            If sender Is DefaultToolStripMenuItem Then
                logLevel = 0
            ElseIf sender Is KeyStagesToolStripMenuItem Then
                logLevel = 1
            ElseIf sender Is AllStagesToolStripMenuItem Then
                logLevel = 2
            ElseIf sender Is ErrorsOnlyToolStripMenuItem Then
                logLevel = 3
            ElseIf sender Is LogMemoryUsageToolStripMenuItem Then
                logMemory = LogMemoryUsageToolStripMenuItem.CheckState
            ElseIf sender Is IncludeMemoryCleanupToolStripMenuItem Then
                logForceGC = IncludeMemoryCleanupToolStripMenuItem.CheckState
            ElseIf sender Is LogWebServiceCommunicationToolStripMenuItem Then
                logWebServices = LogWebServiceCommunicationToolStripMenuItem.CheckState()
            End If

            gSv.SetResourceDiagnosticsCombined(SelectedActiveResourceId, logLevel, logMemory, logForceGC, logWebServices)
            Dim resource As ResourceGroupMember = TryCast(mResourceGroupTree.SelectedMember, ResourceGroupMember)
            If resource IsNot Nothing Then
                resource.Configuration = gSv.GetResourceDiagnosticsCombined({SelectedActiveResourceId})
                Dim selectedNode = mResourceGroupTree.GetNodeFor(SelectedActiveResourceId)
                mResourceGroupTree.Refresh()
            End If

        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlResourceManagement_Error0, ex.Message))
        End Try

    End Sub

    ''' <summary>
    ''' Handles the changing of the check state for the "Log to Event Log" item
    ''' </summary>
    Private Sub HandleEventLogCheckStateChanged(ByVal sender As Object, ByVal e As EventArgs) _
        Handles menuEventLogEnabled.CheckStateChanged

        If mUpdating Then Return

        Dim state As CheckState = menuEventLogEnabled.CheckState

        ' If it's indeterminate, then leave it as it currently is
        If state = CheckState.Indeterminate Then Return

        ' Otherwise, enable / disable it as appropriate
        Try
            gSv.SetResourceEventLogging(SelectedActiveResourceId, (state = CheckState.Checked))
        Catch ex As Exception
            UserMessage.Show(
             My.Resources.ctlResourceManagement_AnErrorOccurredWhileAttemptingToSaveTheEventLoggingChange, ex)
        End Try

    End Sub


    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName(Permission.Resources.ImpliedManageResources)
        End Get
    End Property

    Private Sub ResetFQDNToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ResetFQDNToolStripMenuItem.Click
        Try
            gSv.ResetResourceFQDN(SelectedActiveResourceId)
            UserMessage.Show(My.Resources.ctlResourceManagement_TheFQDNHasBeenResetForTheSelectedResourcesConsiderRestartingThemIfTheyAreCurren)
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlResourceManagement_ErrorResettingTheFQDNForThisResource0, ex.Message))
        End Try
    End Sub

    ''' <summary>
    ''' Processes the dropping of some group members onto a target, either causing a
    ''' retirement or an unretirement of a resource.
    ''' </summary>
    ''' <param name="contents">The group members which are being dragged</param>
    ''' <param name="target">The target onto which the members are being dropped.
    ''' </param>
    ''' <param name="retiring">True if the action should cause the retiring of the
    ''' contents; False if it should cause the 'unretiring' of the contents.</param>
    Private Sub ProcessDroppedContents(
     contents As ICollection(Of IGroupMember),
     target As IGroupMember,
     retiring As Boolean)
        Dim resources As IEnumerable(Of ResourceGroupMember) = GetResources(contents)
        
        ' If we're retiring the contents and any of them are retired, *or* we're
        ' unretiring the contents and any are not retired, exit now.
        If resources.Any(Function(mem) Not (retiring Xor mem.IsRetired)) Then Return

        Dim gp As IGroup = TryCast(target, IGroup)
        If Not retiring Then
            ' Get the group to add it to
            If gp Is Nothing Then gp = target.Owner
            If gp Is Nothing Then Return

            ' If trying to place into root node when the tree has a default group - disallow this.
            If gp.IsRoot AndAlso gp.Tree.HasDefaultGroup Then
                Return
            End If
        End If

        ' Go through all the dragged resources and retire / unretire them
        Try
            For Each mem In resources
                If retiring Then mem.Retire() Else mem.Unretire(gp)
            Next
        Catch ex As PermissionException
            UserMessage.OK(ex.Message)
        End Try
        RefreshResources()
    End Sub

    Private Function GetResources(contents As IEnumerable(Of IGroupMember)) As IEnumerable(Of ResourceGroupMember)

        Dim resources = contents.OfType(Of ResourceGroupMember)

        ' Only one group can be selected, so we are only interested in the first
        Dim group = contents.OfType(Of IGroup).FirstOrDefault

        ' We only process groups when there no resources in the collection
        If resources.Count = 0 AndAlso group IsNot Nothing Then
            resources = GetResourcesForGroup(group)
        End If
        Return resources
    End Function

    Private Function GetResourcesForGroup(group As IGroup) As IEnumerable(Of ResourceGroupMember)
        Dim resourceList = New List(Of ResourceGroupMember)
        For Each subGroup In group.OfType(Of IGroup)
            resourceList.AddRange(GetResourcesForGroup(subGroup))
        Next
        resourceList.AddRange((From member In group Select member).OfType(Of ResourceGroupMember)().ToList())
        Return resourceList
    End Function


    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "helpSystemManagerResourceManagement.htm"
    End Function

    Public Sub RefreshView() Implements IRefreshable.RefreshView
        RefreshViewHandle()
    End Sub

    Public Function CanLeave() As Boolean Implements IStubbornChild.CanLeave
        Return True
    End Function

    Private Enum SortColumnNames
        Name
        LogLevel
    End Enum

    Private Sub ResourceGroupTree_NodeDoubleClick(sender As Object, e As MouseEventArgs) Handles mResourceGroupTree.NodeDoubleClick
        If TypeOf mResourceGroupTree.SelectedMember IsNot ResourceGroupMember OrElse
                TypeOf mResourceGroupTree.SelectedMember.Owner?.RawGroup Is ResourcePool Then
            Return
        End If
        Try
            ProcessDroppedContents({mResourceGroupTree.SelectedMember}, Nothing, True)
        Catch ex As Exception
            UserMessage.Err(ex,
             My.Resources.ctlResourceManagement_AnErrorOccurredWhileTryingToUnretireTheSelectedItems0,
             ex.Message)
        End Try
    End Sub
End Class
