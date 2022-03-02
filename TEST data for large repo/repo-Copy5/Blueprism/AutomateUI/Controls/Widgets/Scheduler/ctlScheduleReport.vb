Imports BluePrism.Scheduling
Imports BluePrism.AutomateAppCore
Imports BluePrism.Server.Domain.Models

Public Class ctlScheduleReport : Inherits ctlScheduleListPanel

    ''' <summary>
    ''' The text used for the link label for viewing a log.
    ''' </summary>
    Private VIEW_LOG As String = My.Resources.ViewLog

    ''' <summary>
    ''' The text used for the link label for viewing a schedule.
    ''' </summary>
    Private VIEW_SCHEDULE As String = My.Resources.ViewSchedule

    ''' <summary>
    ''' The list of relative date items which are used for a report panel.
    ''' </summary>
    Private Shared ReadOnly RelativeDateList As IList(Of RelativeDateItem) =
     New List(Of RelativeDateItem)(New RelativeDateItem() _
      {RelativeToday, RelativeYesterday}).AsReadOnly()

    ''' <summary>
    ''' Gets the list of relative date items which is held in this list
    ''' control. For a timetable panel, this is: None; Today; Yesterday.
    ''' </summary>
    ''' <returns>A non-null list of relative date items to display in this
    ''' schedule list panel.</returns>
    Protected Overrides Function GetRelativeDateItems() As IList(Of RelativeDateItem)
        Return RelativeDateList
    End Function

    ''' <summary>
    ''' The verb for the label which sits between the 'days' and the date to
    ''' which those days apply. ie.
    ''' In the statement "Show 5 days [label] Today", this should return the
    ''' text which goes into [label].
    ''' For a report this is "ending", ie. "Show 5 days ending Today"
    ''' </summary>
    Protected Overrides ReadOnly Property DaysVerbing() As String
        Get
            Return My.Resources.Ending
        End Get
    End Property

    ''' <summary>
    ''' The verb for the label which sits between "Show the" and the days distance
    ''' updown textbox.
    ''' </summary>
    Protected Overrides ReadOnly Property ShowVerbing() As String
        Get
            Return My.Resources.Last
        End Get
    End Property

    ''' <summary>
    ''' Populates this panel and its child controls with the data from the 
    ''' given list.
    ''' </summary>
    ''' <param name="list">The list to populate this list panel with.</param>
    Protected Overrides Sub InnerPopulate(ByVal list As ScheduleList)

        MyBase.InnerPopulate(list)

        Try
            Dim logs As ICollection(Of IScheduleInstance) = list.Store.GetListEntries(list)

            gridScheduleList.Rows.Clear()
            For Each inst As IScheduleInstance In logs
                Dim log = TryCast(inst, IScheduleLog)
                Dim i As Integer = gridScheduleList.Rows.Add(
                 inst.Status, inst.Schedule.Name,
                 inst.StartTime.ToLocalTime(), inst.EndTime.ToLocalTime(),
                 log.SchedulerName,
                 VIEW_LOG, VIEW_SCHEDULE)

                gridScheduleList.Rows(i).Tag = inst
                gridScheduleList.Rows(i).Cells(0).ToolTipText = GetToolTipText(inst.Status)
            Next

        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.Error0, ex.Message), ex)
        End Try
    End Sub

    Private Function GetToolTipText(status As ItemStatus) As String
        Select Case status
            Case ItemStatus.Completed
                Return My.Resources.ScheduleCompleted

            Case ItemStatus.Stopped
                Return My.Resources.ScheduleStopped

            Case ItemStatus.Failed
                Return My.Resources.ScheduleFailed

            Case ItemStatus.Pending
                Return My.Resources.SchedulePending

            Case ItemStatus.Deferred
                Return My.Resources.ScheduleDeferred

            Case ItemStatus.Running
                Return My.Resources.ScheduleRunning

            Case ItemStatus.Debugging
                Return My.Resources.ScheduleDebugging

            Case ItemStatus.Locked
                Return My.Resources.ScheduleLocked

            Case ItemStatus.Queried
                Return My.Resources.ScheduleQueried

            Case ItemStatus.PartExceptioned
                Return My.Resources.SchedulePartExceptioned

        End Select

        Return String.Empty
    End Function


    ''' <summary>
    ''' Handles a link being clicked on a schedule instance.
    ''' </summary>
    ''' <param name="inst">The instance that was clicked.</param>
    ''' <param name="linkText">The text of the link that was clicked.</param>
    Private Sub ScheduleLinkClicked(ByVal inst As IScheduleInstance, ByVal linkText As String) _
     Handles MyBase.LinkClick

        Select Case linkText

            Case VIEW_SCHEDULE
                Dim manager As ctlControlRoom =
                 clsUserInterfaceUtils.GetAncestor(Of ctlControlRoom)(Me)
                If manager IsNot Nothing Then manager.SelectSchedule(inst.Schedule)

            Case VIEW_LOG
                Try
                    frmScheduleLogViewer.ShowLog(inst, Me)
                Catch ex As Exception
                    UserMessage.Err(ex, String.Format(My.Resources.UnableToShowScheduleLogs0, ex.Message))
                End Try
        End Select

    End Sub

End Class
