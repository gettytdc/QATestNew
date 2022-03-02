
Imports AutomateControls.UIState.UIElements
Imports BluePrism.AutomateAppCore.Groups

Public Class ctlWorkQueueManagement
    Implements IRefreshable

    ''' <summary>
    ''' Event raised when a selection has changed in the work queue list.
    ''' </summary>
    ''' <remarks>
    ''' A selection is considered to be changed if a queue with a different ident to
    ''' the currently selected queue is chosen - if the list refreshes and the
    ''' current selection is reapplied after the refresh, no event is raised.
    ''' </remarks>
    <Browsable(True), Category("Behavior"), Description(
        "Event fired when the selected queue in the control has changed")>
    Public Event SelectedQueueChanged As QueueEventHandler

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

    End Sub


    ''' <summary>
    ''' Event handler for a change in the selected row in the list of queues.
    ''' </summary>
    Private Sub HandleSelectedQueueChanged( _
     ByVal sender As Object, ByVal e As QueueEventArgs) _
     Handles mWorkQueueList.SelectedQueueChanged
        OnSelectedQueueChanged(e)
    End Sub

    ''' <summary>
    ''' Sets the group whose contents (the queues at least) should be visible in this
    ''' control and its descendants.
    ''' </summary>
    Public WriteOnly Property Group As IGroup
        Set(value As IGroup)
            mWorkQueueList.Group = value
        End Set
    End Property

    ''' <summary>
    ''' Raises the <see cref="SelectedQueueChanged"/> event.
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnSelectedQueueChanged(e As QueueEventArgs)
        Try
            mWorkQueueContents.QueueId = e.QueueId
        Catch ex As Exception
            UserMessage.Err(ex, My.Resources.ctlWorkQueueManagement_ErrorSelectingQueue0, ex.Message)
        End Try
        RaiseEvent SelectedQueueChanged(Me, e)
    End Sub

    ''' <summary>
    ''' Selects the queue represented by the given number if it is currently held in
    ''' this control
    ''' </summary>
    ''' <param name="q"></param>
    Public Sub SelectQueue(ByVal q As QueueGroupMember)
        If q IsNot Nothing AndAlso mWorkQueueList.ContainsQueue(q.QueueGuid) Then
            mWorkQueueList.SelectedId = q.QueueGuid
        Else
            mWorkQueueList.SelectFirstQueue()
        End If
    End Sub

    ''' <summary>
    ''' Refreshes the work queue management control, updating both the list of queues
    ''' and the currently selected queue's contents.
    ''' </summary>
    Public Sub RefreshView() Implements IRefreshable.RefreshView
        mWorkQueueList.RefreshList()
        mWorkQueueContents.RefreshList()
    End Sub
End Class
