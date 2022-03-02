Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.Images
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Utility

Public Class ctlWorkQueueList : Implements IHelp

    ''' <summary>
    ''' Event raised when a selection has changed in the work queue list.
    ''' </summary>
    ''' <remarks>
    ''' A selection is considered to be changed if a queue with a different ident to
    ''' the currently selected queue is chosen - if the list refreshes and the
    ''' current selection is reapplied after the refresh, no event is raised.
    ''' </remarks>
    <Browsable(True), Category("Behavior"), Description(
        "Event fired when the selected queue in the list has changed")>
    Public Event SelectedQueueChanged As QueueEventHandler

    ''' <summary>
    ''' The ListView sorting object
    ''' </summary>
    Private mSorter As clsListViewSorter

    Private mGroup As IGroup
    ' Private mFilter As ICollection(Of Integer) = Nothing

    Private mLastSelectedIdent As Integer = 0

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        BuildList()
    End Sub

    ''' <summary>
    ''' Handles a selected index changing in this list. Only chains it on if the
    ''' selected queue differs from the last selected queue.
    ''' </summary>
    Private Sub HandleListSelectedIndexChanged(sender As Object, e As EventArgs) _
     Handles lstQueues.SelectedIndexChanged
        Dim q = SelectedQueue
        Dim ident As Integer = If(q Is Nothing, 0, q.Ident)
        If ident = mLastSelectedIdent Then Return
        mLastSelectedIdent = ident
        OnSelectedQueueChanged(New QueueEventArgs(q))
    End Sub

    ''' <summary>
    ''' Raises the <see cref="SelectedQueueChanged"/> event.
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnSelectedQueueChanged(e As QueueEventArgs)
        RaiseEvent SelectedQueueChanged(Me, e)
    End Sub

    ''' <summary>
    ''' Configures the listview's headers, etc.
    ''' </summary>
    Private Sub BuildList()
        Dim imgList As New ImageList()
        imgList.Images.Add("Paused", MediaImages.mm_Pause_16x16)
        imgList.Images.Add("Running", MediaImages.mm_Play_16x16)
        lstQueues.SmallImageList = imgList

        mSorter = New clsListViewSorter(Me.lstQueues)
        mSorter.ColumnDataTypes = New Type() { _
            GetType(String), _
            GetType(String), _
            GetType(Integer), _
            GetType(Integer), _
            GetType(Integer), _
            GetType(Integer), _
            GetType(Integer), _
            GetType(TimeSpan), _
            GetType(TimeSpan)}

        mSorter.SortColumn = 1
        mSorter.Order = SortOrder.Ascending
        lstQueues.ListViewItemSorter = mSorter
    End Sub

    ''' <summary>
    ''' Repopulates the list of queues, using the latest information from the database.
    ''' </summary>
    Public Sub RefreshList()
        Dim sel As clsWorkQueue = SelectedQueue
        Try
            lstQueues.BeginUpdate()
            lstQueues.Items.Clear()
            Dim active As Integer = 0
            Dim count As Integer = 0
            Dim mFilter = If(mGroup Is Nothing,
             Nothing,
             mGroup.
                OfType(Of QueueGroupMember).
                Select(Function(m) m.IdAsInteger).
                ToList()
            )
            For Each queue As clsWorkQueue In gSv.WorkQueueGetQueuesFiltered(mFilter)
                count += 1
                Dim item As New ListViewItem(queue.Name)
                item.Tag = queue
                item.ImageKey = queue.RunningLabel
                With item.SubItems
                    .Add(queue.RunningLabel)
                    .Add(queue.Completed.ToString())
                    .Add(queue.Pending.ToString())
                    .Add(queue.Exceptioned.ToString())
                    .Add(queue.TotalAttempts.ToString())
                    .Add(FormatTimespan(queue.AverageWorkedTime))
                    .Add(FormatTimespan(queue.TotalWorkTime))
                End With
                lstQueues.Items.Add(item)
                If sel IsNot Nothing AndAlso sel.Ident = queue.Ident Then
                    item.Selected = True
                End If
                If queue.IsRunning Then active += 1
            Next
            lblQueues.Text = String.Format(
                My.Resources.ctlWorkQueueList_0QueuesRunning1Paused2, count, active, count - active)

        Catch ex As Exception
            UserMessage.Err(ex,
             My.Resources.ctlWorkQueueList_AnErrorOccurredWhileRefreshingTheQueueList, ex)

        Finally
            'Clearing the items clears the SelectedQueue, so re-set it here
            If sel IsNot Nothing Then
                SelectedQueue = sel
            End If
            lstQueues.EndUpdate()
        End Try

    End Sub

    ''' <summary>
    ''' Formats a timespan for display in the UI.
    ''' </summary>
    ''' <param name="Value">The timespan to be displayed.</param>
    ''' <returns>Returns a string representing the supplied timespan, suitable for
    ''' display in the UI.</returns>
    Private Function FormatTimespan(ByVal Value As TimeSpan) As String
        Dim sb As New System.Text.StringBuilder()
        If Value.Days > 0 Then sb.AppendFormat("{0}.", Value.Days)
        If Value.Hours > 0 Then sb.AppendFormat("{0:00}:", Value.Hours)
        sb.AppendFormat("{0:00}:{1:00}.{2:000}", Value.Minutes, Value.Seconds, Value.Milliseconds)
        Return sb.ToString()
    End Function

#Region "Context Menu Stuff"

    ''' <summary>
    ''' Prepares creates a context menu for this control.
    ''' </summary>
    Private Sub BuildContextMenu()
        Dim cm As New ContextMenuStrip
        Dim item As ToolStripItem

        'Add refresh item
        item = cm.Items.Add(My.Resources.ctlWorkQueueList_ReFreshView, ToolImages.Refresh_16x16, AddressOf OnRefreshClicked)

        'Add select all item
        item = cm.Items.Add(My.Resources.ctlWorkQueueList_SelectAll, Nothing, AddressOf OnSelectAllClicked)

        cm.Items.Add(New ToolStripSeparator)

        Dim canControlQueues = User.Current.HasPermission("Full Access to Queue Management")

        'Add pause item
        Dim canPause As Boolean = canControlQueues AndAlso SelectedQueues.Any(Function(q) q.IsRunning)
        item = cm.Items.Add(My.Resources.ctlWorkQueueList_PauseSelectedQueueS, MediaImages.mm_Pause_16x16, AddressOf OnPauseClicked)
        item.Enabled = canPause

        'Add resume item
        Dim canResume As Boolean = canControlQueues AndAlso SelectedQueues.Any(Function(q) Not q.IsRunning)
        item = cm.Items.Add(My.Resources.ctlWorkQueueList_ResumeSelectedQueueS, MediaImages.mm_Play_16x16, AddressOf OnResumeClicked)
        item.Enabled = canResume

        cm.Items.Add(New ToolStripSeparator)

        'Add help all item
        item = cm.Items.Add(My.Resources.ctlWorkQueueList_Help, ToolImages.Help_16x16, AddressOf OnHelpClicked)

        Me.ContextMenuStrip = cm
    End Sub

    Private Sub lstQueues_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles lstQueues.MouseUp
        If e.Button = System.Windows.Forms.MouseButtons.Right Then
            Try
                If Me.ContextMenuStrip Is Nothing Then
                    BuildContextMenu()
                    Me.ContextMenuStrip.Show(lstQueues, e.Location)
                    Me.ContextMenuStrip = Nothing
                End If
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.ctlWorkQueueList_UnexpectedError0, ex.Message), ex)
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Event handler for the context menu "select all" option
    ''' </summary>
    Private Sub OnSelectAllClicked(ByVal sender As Object, ByVal e As EventArgs)
        Try
            For Each i As ListViewItem In lstQueues.Items
                i.Selected = True
            Next
            lstQueues.Select()
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlWorkQueueList_UnexpectedError0, ex.Message), ex)
        End Try
    End Sub


    ''' <summary>
    ''' Updates the paused/active status of those queues selected in the UI.
    ''' </summary>
    ''' <param name="Running">When true, the queues will be resumed;
    ''' when false, the queues will be paused.</param>
    Private Sub SetRunningStatusOfSelectedQueues(ByVal running As Boolean)
        If lstQueues.SelectedItems.Count = 0 Then Return
        Try
            Dim ids = SelectedQueues.Select(Function(q) q.Id).ToList
            gSv.SetQueueRunningStatus(ids, running)
            RefreshList()
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlWorkQueueList_ErrorSettingQueueStatus0, ex.Message), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Event handler for the context menu "resume" option
    ''' </summary>
    Private Sub OnResumeClicked(ByVal sender As Object, ByVal e As EventArgs)
        Try
            SetRunningStatusOfSelectedQueues(True)
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlWorkQueueList_UnexpectedError0, ex.Message), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Event handler for the context menu "pause" option
    ''' </summary>
    Private Sub OnPauseClicked(ByVal sender As Object, ByVal e As EventArgs)
        Try
            SetRunningStatusOfSelectedQueues(False)
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlWorkQueueList_UnexpectedError0, ex.Message), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Event handler for the context menu "refresh" option
    ''' </summary>
    Private Sub OnRefreshClicked(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.RefreshList()
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlWorkQueueList_UnexpectedError0, ex.Message), ex)
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    ''' <summary>
    ''' Event handler for the context menu "help" option
    ''' </summary>
    Private Sub OnHelpClicked(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Me.Cursor = Cursors.WaitCursor
            Try
                OpenHelpFile(Me, GetHelpFile())
            Catch
                UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
            End Try
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlWorkQueueList_UnexpectedError0, ex.Message), ex)
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

#End Region

    ''' <summary>
    ''' Gets or sets the currently selected queue - the focused queue, if there is 
    ''' one, the first of the selected queues otherwise - or null if no queue is 
    ''' selected.
    ''' </summary>
    Public Property SelectedQueue As clsWorkQueue
        Get
            Dim item = lstQueues.FocusedItem
            If item Is Nothing Then _
             item = lstQueues.SelectedItems.Cast(Of ListViewItem).FirstOrDefault()
            If item Is Nothing Then Return Nothing
            Return TryCast(item.Tag, clsWorkQueue)
        End Get
        Set(value As clsWorkQueue)
            If value Is Nothing Then
                SetSelectedQueue(Function(q As clsWorkQueue) False)
            Else
                SetSelectedQueue(Function(q As clsWorkQueue) q.Equals(value))
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets all selected queues based on items selected in the list
    ''' </summary>
    Private ReadOnly Property SelectedQueues As IEnumerable(Of clsWorkQueue)
        Get
            Return From selectedItem In lstQueues.SelectedItems.OfType(Of ListViewItem)()
                Let queue = TryCast(selectedItem.Tag, clsWorkQueue)
                Where queue IsNot Nothing
                Select queue
        End Get
    End Property

    ''' <summary>
    ''' Sets the selected queue to the first one in this list which passes a
    ''' specified predicate
    ''' </summary>
    ''' <param name="pred">The predicate which determines which queue should be set
    ''' as the selected queue</param>
    Private Sub SetSelectedQueue(pred As Predicate(Of clsWorkQueue))
        ' Flag to indicate if we have a selection set
        Dim isQueueSelected = False
        For Each item As ListViewItem In lstQueues.Items
            Dim q As clsWorkQueue = TryCast(item.Tag, clsWorkQueue)
            If q IsNot Nothing Then
                ' We only want to select one, so as soon as we find one which
                ' satisfies the predicate set a flag to show we have a selected queue
                Dim sel = Not isQueueSelected AndAlso pred(q)
                isQueueSelected = isQueueSelected OrElse sel

                Try
                    item.Selected = sel
                    item.Focused = sel
                Catch ex As NullReferenceException
                    ' BG-7028. Null Reference exception occurs if a queue has been cloned
                    ' which is being removed as part of the aformentioned ticket. The 
                    ' result is that selection jumps to the top-most instance of this queue
                End Try
            End If
        Next
    End Sub

    ''' <summary>
    ''' Checks if this control contains a queue with the given ID.
    ''' </summary>
    ''' <param name="queueId">The ID of the queue to search for</param>
    ''' <returns>True if this list contains an entry for a queue with the specified
    ''' ID; False otherwise.</returns>
    Public Function ContainsQueue(queueId As Guid) As Boolean
        For Each item As ListViewItem In lstQueues.Items
            Dim queue As clsWorkQueue = TryCast(item.Tag, clsWorkQueue)
            If queue IsNot Nothing AndAlso queue.Id = queueId Then Return True
        Next
        Return False
    End Function

    ''' <summary>
    ''' Gets or sets the selected ID in this work queue list
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectedId As Guid
        Get
            Dim wq = SelectedQueue
            Return If(wq Is Nothing, Guid.Empty, wq.Id)
        End Get
        Set(value As Guid)
            SetSelectedQueue(Function(q) q.Id = value)
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the selected ID in this work queue list
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectedIdent As Integer
        Get
            Dim wq = SelectedQueue
            Return If(wq Is Nothing, 0, wq.Ident)
        End Get
        Set(value As Integer)
            SetSelectedQueue(Function(q) q.Ident = value)
        End Set
    End Property

    ''' <summary>
    ''' Sets the group whose queues should be visible in this list, excluding all
    ''' others. A value of null will clear the filtering and show all groups.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public WriteOnly Property Group As IGroup
        Set(value As IGroup)
            ' If we're setting to the same group - no action
            If mGroup IsNot Nothing AndAlso value IsNot Nothing AndAlso
             mGroup.RawGroup Is value.RawGroup Then Return
            mGroup = value
            RefreshList()
        End Set
    End Property

    ''' <summary>
    ''' Selects the first queue in this list.
    ''' </summary>
    Friend Sub SelectFirstQueue()
        ' Has effect of setting the selected queue to being the first one encountered
        SetSelectedQueue(Function(q) True)
    End Sub

    ''' <summary>
    ''' Gets the name of the help file associated with this control.
    ''' </summary>
    ''' <returns>Returns the name of the file containing help.</returns>
    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "control-queues.html"
    End Function

End Class
