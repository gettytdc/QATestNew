Imports AutomateControls
Imports AutomateControls.Filters
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.clsLockFilter
Imports BluePrism.AutomateAppCore.clsLockInfo
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Images
Imports BluePrism.Server.Domain.Models

Public Class ctlWorkflowEnvironmentLocks
    Implements IHelp
    Implements IChild
    Implements IPermission

    ' The listview held by the filtered listview.
    Private WithEvents mLockingListView As ScrollHandlingListView

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        RowsPerPage.MaxRows = 10000
        RowsPerPage.RowsPerPage = 100

        ' Add any initialization after the InitializeComponent() call.
        ' Set the locking listview from the filtered listview
        mLockingListView = mFilteredLocks.FilteredView

        Dim stateDefn As New ImageFilterDefinition(FilterNames.Status, New FilterItem() {
         FilterItem.Empty,
         New FilterItem(LockState.Held.ToString(), LockState.Held, ToolImages.Lock_16x16),
         New FilterItem(LockState.Free.ToString(), LockState.Free, ToolImages.Unlock_16x16)
        })
        mFilteredLocks.AddFilter(FilterNames.Status, My.Resources.ctlWorkflowEnvironmentLocks_Status, 60, HorizontalAlignment.Right, stateDefn)
        mFilteredLocks.AddFilter(New StringFilterDefinition(FilterNames.Name), My.Resources.ctlWorkflowEnvironmentLocks_Name)
        mFilteredLocks.AddFilter(New StringFilterDefinition(FilterNames.Resource), My.Resources.ctlWorkflowEnvironmentLocks_Resource)
        mFilteredLocks.AddFilter(New StringFilterDefinition(FilterNames.Process), My.Resources.ctlWorkflowEnvironmentLocks_Process)
        AddPastDateFilterAndSetDefault()
        mFilteredLocks.AddFilter(FilterNames.LastComment, My.Resources.ctlWorkflowEnvironmentLocks_LastComment, 125,
                                 HorizontalAlignment.Left, New StringFilterDefinition(FilterNames.LastComment))

        ' Something needs to be selected before these can be chosen
        btnReleaseLocks.Enabled = False
        btnDeleteLocks.Enabled = False
        btnViewLogs.Enabled = False

        PopulateLocks()

        AddHandler RowsPerPage.ConfigChanged, AddressOf PopulateLocks
        AddHandler RowsPerPage.PageChanged, AddressOf PopulateLocks
    End Sub

#Region " Environment Locks "
    ''' <summary>
    ''' The collection of currently selected locks.
    ''' </summary>
    Private ReadOnly Property SelectedLocks() As ICollection(Of clsLockInfo)
        Get
            Dim l As New List(Of clsLockInfo)
            For Each item As ListViewItem In mLockingListView.SelectedItems
                l.Add(DirectCast(item.Tag, clsLockInfo))
            Next
            Return l
        End Get
    End Property

    ''' <summary>
    ''' The collection of lock names for currently selected locks.
    ''' </summary>
    Private ReadOnly Property SelectedLockNames() As ICollection(Of String)
        Get
            Dim s As New clsSet(Of String)
            For Each item As ListViewItem In mLockingListView.SelectedItems
                s.Add(DirectCast(item.Tag, clsLockInfo).Name)
            Next
            Return s
        End Get
    End Property

    ''' <summary>
    ''' Populates the control with up-to-date lock data from the database.
    ''' </summary>
    Private Sub PopulateLocks()
        ' Do this through the 'Update' method so that scroll positions are remembered
        ' and the flickering is reduced.
        mLockingListView.UpdateListView(AddressOf UpdateLocks)
    End Sub

    ''' <summary>
    ''' This method removes the blue highlight on the filter selection options above the lock table.
    ''' </summary>
    Private Sub RemoveHighlightingOnFilters()
        Dim filters = mFilteredLocks.CurrentFilters.GetEnumerator

        While filters.MoveNext
            filters.Current.Combo.SelectionLength = 0
        End While
    End Sub

    Private Sub AddPastDateFilterAndSetDefault()
        Dim pastDateFilterDefinition = New PastDateFilterDefinition(FilterNames.LockTime)

        mFilteredLocks.AddFilter(FilterNames.LockTime, My.Resources.ctlWorkflowEnvironmentLocks_LockTime, 125,
                                 HorizontalAlignment.Left, pastDateFilterDefinition)
        Dim dateFilter = mFilteredLocks.CurrentFilters.FirstOrDefault(Function(x) x.Name = FilterNames.LockTime)
        Dim todayFilterOption = dateFilter?.Definition.Items.FirstOrDefault(Function(x) x.FilterTerm = "Today")

        dateFilter.Combo.SelectedItem = todayFilterOption
    End Sub

    ''' <summary>
    ''' Updates the locks displayed in the given listview.
    ''' </summary>
    ''' <param name="lv">The listview on which the locks should be updated.</param>
    Private Sub UpdateLocks(ByVal lv As ListView)

        ' Save the names of the selected locks...
        Dim selectedLocks As ICollection(Of String) = Me.SelectedLockNames
        lv.Items.Clear()
        RowsPerPage.TotalRows = 0

        Try
            ' Go through all the locks and create listview items for each of them.
            Dim lf As New clsLockFilter(mFilteredLocks.CurrentFilters.FilterMap, RowsPerPage.RowsPerPage, RowsPerPage.CurrentPage)
            Dim serverResponse = gSv.SearchEnvLocks(lf)
            RowsPerPage.TotalRows = serverResponse.totalAmountOfRows

            For Each lock As clsLockInfo In serverResponse.filteredLocks
                Dim state As String = lock.State.ToString()
                Dim item As ListViewItem = lv.Items.Add("", state)
                item.Tag = lock
                With item.SubItems
                    .Add(lock.Name)
                    .Add(lock.ResourceName)
                    .Add(lock.ProcessName)
                    .Add(lock.LockTimeDisplay)
                    .Add(lock.Comment)
                End With
                ' If the lock with this name was previously selected, select it again
                If selectedLocks.Contains(lock.Name) Then item.Selected = True
            Next

            ' If we had a selection, and we now don't, ensure that this is handled
            ' as a selection change (which wouldn't be fired because there's nothing
            ' to select in the listview, ergo no change).
            If lv.Items.Count = 0 AndAlso selectedLocks.Count > 0 Then
                HandleLockSelectionChange(lv, EventArgs.Empty)
            End If

        Catch ex As Exception
            UserMessage.Show(My.Resources.ctlWorkflowEnvironmentLocks_ErrorWhenAttemptingToUpdateTheLockInformation, ex)

        End Try
    End Sub

    Private Async Sub UpdateLocksAsync()
        Dim selected = SelectedLockNames.AsEnumerable()
        Dim lv = mLockingListView
        Dim lockFilter As New clsLockFilter(mFilteredLocks.CurrentFilters.FilterMap, RowsPerPage.RowsPerPage, RowsPerPage.CurrentPage)

        Try
            Await Task.Run(Function() As (IEnumerable(Of ListViewItem), Integer)
                               Dim resp = gSv.SearchEnvLocks(lockFilter)
                               Dim newItems = GetLockListView(resp, selected)
                               Return (newItems, resp.totalAmountOfRows)
                           End Function).
                           ContinueWith(Sub(task)
                                            Invoke(Sub()
                                                       Dim res As (IEnumerable(Of ListViewItem), Integer) = task.Result
                                                       UpdateListView(lv, res.Item1, res.Item2)

                                                       If lv.Items.Count = 0 AndAlso SelectedLocks.Count > 0 Then
                                                           HandleLockSelectionChange(res.Item1, EventArgs.Empty)
                                                       End If
                                                   End Sub)
                                        End Sub)
        Catch ex As Exception
            UserMessage.Show(My.Resources.ctlWorkflowEnvironmentLocks_ErrorWhenAttemptingToUpdateTheLockInformation, ex)
        End Try
    End Sub

    Private Function GetLockListView(response As (filteredLocks As ICollection(Of clsLockInfo), totalAmountOfRows As Integer), selectedNames As IEnumerable(Of String)) As IEnumerable(Of ListViewItem)
        Dim items As New List(Of ListViewItem)

        For Each lock As clsLockInfo In response.filteredLocks
            Dim item = New ListViewItem With {
                .Tag = lock,
                .ImageKey = lock.State.ToString(),
                .Selected = selectedNames.Contains(lock.Name)
            }
            With item.SubItems
                .Add(lock.Name)
                .Add(lock.ResourceName)
                .Add(lock.ProcessName)
                .Add(lock.LockTimeDisplay)
                .Add(lock.Comment)
            End With
            items.Add(item)
        Next
        Return items
    End Function

    Private Sub UpdateListView(listView As ScrollHandlingListView, newItems As IEnumerable(Of ListViewItem), rows As Integer)
        Dim scrollPosition = listView.ScrollPosition ' save scroll position

        listView.BeginUpdate()

        RowsPerPage.TotalRows = rows
        listView.Items.Clear()
        listView.Items.AddRange(newItems.ToArray())
        listView.ScrollPosition = scrollPosition

        listView.EndUpdate()
    End Sub

#Region " Lock Event Handlers "

    ''' <summary>
    ''' Handle the context menu being requested on the locking listview.
    ''' </summary>
    Private Sub HandleMouseUp(ByVal sender As Object, ByVal e As MouseEventArgs) _
     Handles mLockingListView.MouseUp
        If e.Button = MouseButtons.Right Then
            Try
                Dim cm As New ContextMenuStrip()
                Dim relLocks As ToolStripItem =
                 cm.Items.Add(My.Resources.ctlWorkflowEnvironmentLocks_ReleaseLocks, btnReleaseLocks.Image, AddressOf HandleReleaseLocks)
                Dim delLocks As ToolStripItem =
                 cm.Items.Add(My.Resources.ctlWorkflowEnvironmentLocks_DeleteLocks, btnDeleteLocks.Image, AddressOf HandleDeleteLocks)
                cm.Items.Add(New ToolStripSeparator())
                Dim viewLogs As ToolStripItem =
                 cm.Items.Add(My.Resources.ctlWorkflowEnvironmentLocks_ViewLogs, btnViewLogs.Image, AddressOf HandleViewLogs)

                ' Assume all disabled
                relLocks.Enabled = False
                delLocks.Enabled = False
                viewLogs.Enabled = CanAccessSessionLog()

                ' Go through each lock and see what is available to do on each lock.
                For Each lock As clsLockInfo In Me.SelectedLocks
                    If lock.State = clsLockInfo.LockState.Held Then
                        relLocks.Enabled = True
                        ' If they are all enabled, no point in checking any further.
                        If delLocks.Enabled Then Exit For
                    Else
                        delLocks.Enabled = True
                    End If
                Next

                cm.Show(mLockingListView, e.Location)

            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.ctlWorkflowEnvironmentLocks_UnexpectedError0, ex.Message), ex)

            End Try
        End If

    End Sub

    ''' <summary>
    ''' Handles the selection changing in the locks listview.
    ''' This just ensures that the buttons are enabled or disabled as appropriate
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub HandleLockSelectionChange(ByVal sender As Object, ByVal e As EventArgs) _
     Handles mLockingListView.SelectedIndexChanged

        Dim locks As ICollection(Of clsLockInfo) = Me.SelectedLocks
        If locks.Count = 0 Then
            btnReleaseLocks.Enabled = False
            btnDeleteLocks.Enabled = False
            btnViewLogs.Enabled = False
        Else
            Dim anyHeld As Boolean = False
            Dim anyFree As Boolean = False
            ' Go through each lock and see what is available to do on each lock.
            For Each lock As clsLockInfo In Me.SelectedLocks
                If lock.State = clsLockInfo.LockState.Held Then
                    anyHeld = True
                Else
                    anyFree = True
                End If
            Next
            btnReleaseLocks.Enabled = anyHeld
            btnViewLogs.Enabled = CanAccessSessionLog()
            btnDeleteLocks.Enabled = anyFree
        End If

    End Sub

    ''' <summary>
    ''' Handles the 'Release Locks' action being initiated, either by clicking the
    ''' button or selecting the context menu item.
    ''' </summary>
    Private Sub HandleReleaseLocks(ByVal sender As Object, ByVal e As EventArgs) _
        Handles btnReleaseLocks.Click

        For Each lock As clsLockInfo In SelectedLocks
            If lock.Name.Equals(My.Resources.ctlWorkflowEnvironmentLocks_SendPublishedDashboards) And Not User.Current.IsSystemAdmin Then
                UserMessage.Show(My.Resources.ctlWorkflowEnvironmentLocks_OnlyAdministratorsCanReleaseDataGatewayLocks)
                Continue For
            End If
            Try
                gSv.ManualReleaseEnvLock(lock.Name, lock.Token, My.Resources.ctlWorkflowEnvironmentLocks_ReleasedInSystemManager)
            Catch ilte As IncorrectLockTokenException
                If UserMessage.YesNo(
                 My.Resources.ctlWorkflowEnvironmentLocks_TheLock0HasBeenReassignedSinceBeingDisplayed1ForceReleaseTheLockAnyway, lock.Name, vbCrLf) = MsgBoxResult.Yes Then

                    Try
                        gSv.ManualReleaseEnvLock(lock.Name, Nothing, My.Resources.ctlWorkflowEnvironmentLocks_ForceReleasedInSystemManager)
                    Catch innerNoSuchLock As NoSuchLockException
                        UserMessage.Err(innerNoSuchLock, My.Resources.ctlWorkflowEnvironmentLocks_TheLock0NoLongerExists, lock.Name)
                    End Try
                End If
            Catch noSuchLock As NoSuchLockException
                UserMessage.Err(noSuchLock, My.Resources.ctlWorkflowEnvironmentLocks_TheLock0NoLongerExists, lock.Name)
            End Try
        Next
        UpdateLocksAsync()
    End Sub




    ''' <summary>
    ''' Handles the 'Delete Locks' action being initiated, either by clicking the
    ''' button or selecting the context menu item.
    ''' </summary>
    Private Sub HandleDeleteLocks(ByVal sender As Object, ByVal e As EventArgs) _
        Handles btnDeleteLocks.Click
        If Me.SelectedLockNames.Contains(My.Resources.ctlWorkflowEnvironmentLocks_SendPublishedDashboards) And Not User.Current.IsSystemAdmin Then
            UserMessage.Show(My.Resources.ctlWorkflowEnvironmentLocks_OnlyAdministratorsCanDeleteDataGatewayLocks)
        Else
            Try
                gSv.DeleteLocks(Me.SelectedLockNames)
            Catch ex As Exception
                UserMessage.Show(My.Resources.ctlWorkflowEnvironmentLocks_ErrorWhileAttemptingToDeleteTheSelectedLocks, ex)
            End Try
        End If
        UpdateLocksAsync()
    End Sub

    ''' <summary>
    ''' Handles the 'View Logs' action being initiated, either by clicking the
    ''' button or selecting the context menu item.
    ''' </summary>
    Private Sub HandleViewLogs(ByVal sender As Object, ByVal e As EventArgs) _
        Handles btnViewLogs.Click
        Dim sessions As New clsSet(Of Guid)
        For Each lock As clsLockInfo In Me.SelectedLocks
            sessions.Add(lock.SessionId)
        Next
        ' Remove any empty sessions if they were added...
        sessions.Remove(Guid.Empty)
        For Each sessId As Guid In sessions
            mParent.StartForm(New frmLogViewer(sessId))
        Next
    End Sub

    ''' <summary>
    ''' Handles a filter being changed in the locks filtered listview
    ''' </summary>
    Private Sub HandleFilterChanged(ByVal source As FilterSet) _
        Handles mFilteredLocks.FilterApplied
        PopulateLocks()
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        UpdateLocksAsync()
    End Sub

#End Region

#End Region

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "helpSystemManagerLocking.htm"
    End Function

    Private mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    Public ReadOnly Property RequiredPermissions() As System.Collections.Generic.ICollection(Of BluePrism.AutomateAppCore.Auth.Permission) Implements BluePrism.AutomateAppCore.Auth.IPermission.RequiredPermissions
        Get
            Return Permission.ByName(Permission.SystemManager.Workflow.EnvironmentLocking)
        End Get
    End Property

    Private Function CanAccessSessionLog() As Boolean
        Return User.Current.HasPermission(Permission.SystemManager.Audit.BusinessObjectsLogs,
                                      Permission.SystemManager.Audit.ProcessLogs) AndAlso
                SelectedLocks.FirstOrDefault(Function(l) l.State = LockState.Held AndAlso
                                                l.CanViewSessionLog) IsNot Nothing
    End Function

    Private Sub ctlWorkflowEnvironmentLocks_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        RemoveHighlightingOnFilters()
    End Sub
End Class
