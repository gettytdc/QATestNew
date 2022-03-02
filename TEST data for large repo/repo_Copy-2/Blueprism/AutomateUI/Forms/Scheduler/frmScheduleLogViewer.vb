Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.Scheduling

Friend Class frmScheduleLogViewer
    Implements IEnvironmentColourManager

    Private mInst As IScheduleInstance

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
            Return mBlueIconBar.BackColor
        End Get
        Set(value As Color)
            mBlueIconBar.BackColor = value
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
            Return mBlueIconBar.TitleColor
        End Get
        Set(value As Color)
            mBlueIconBar.TitleColor = value
            mBlueIconBar.SubtitleColor = value
        End Set
    End Property

    ''' <summary>
    ''' Shows the log described by the given schedule instance.
    ''' </summary>
    ''' <param name="inst">The schedule instance to show in a log viewer form.
    ''' </param>
    ''' <param name="parent">The parent control (to get theme from)</param>
    Public Shared Sub ShowLog(inst As IScheduleInstance, parent As Control)
        Dim frm As New frmScheduleLogViewer()
        frm.Populate(inst)
        frm.SetEnvironmentColoursFromAncestor(parent)
        frm.CenterToParent()
        frm.ShowInTaskbar = False
        frm.ShowDialog()
    End Sub

    ''' <summary>
    ''' Populates this log viewer with the given log entries.
    ''' </summary>
    ''' <param name="inst">The schedule instance to show in a log viewer form.
    ''' </param>
    Public Sub Populate(ByVal inst As IScheduleInstance)
        mInst = inst
        Dim log As IScheduleLog = DirectCast(inst, IScheduleLog)

        mBlueIconBar.Title = String.Format(
         My.Resources.ScheduleLog0G1,
         inst.InstanceTime,
         If(log.SchedulerName <> "", vbCrLf & String.Format(My.Resources.ExecutedBy0, log.SchedulerName), "")
        )

        For Each task As TaskCompoundLogEntry In inst.CompoundLogEntries

            Dim i As Integer = gridLogEntries.Rows.Add(task.Name, "",
             task.StartDate.ToLocalTime(), task.EndDate.ToLocalTime(), "", task.TerminationReason)
            gridLogEntries.Rows(i).DefaultCellStyle.BackColor = Color.LightBlue
            gridLogEntries.Rows(i).Tag = task

            For Each datedMsg As KeyValuePair(Of Date, String) In task.SessionCreationFailedMessages
                i = gridLogEntries.Rows.Add("", "",
                  datedMsg.Key.ToLocalTime(), datedMsg.Key.ToLocalTime(), "",
                  datedMsg.Value)
            Next



            For Each session As SessionCompoundLogEntry In task.SortedSessions
                Dim isSessionLogArchived = session.SessionID = Guid.Empty


                Dim indexAdded = gridLogEntries.Rows.Add(session.Name, session.ResourceName,
                 session.StartDate.ToLocalTime(), session.EndDate.ToLocalTime(),
                 IIf(isSessionLogArchived,
                    My.Resources.ArchivedSessionLog,
                    My.Resources.ViewLog),
                 session.TerminationReason)

                ApplyViewLogStatus(session, indexAdded, isSessionLogArchived)
            Next
        Next
    End Sub

    Private Sub ApplyViewLogStatus(session As SessionCompoundLogEntry, currentIndex As Integer, isSessionLogArchived As Boolean)
        Dim viewLogColumnIndex As Integer = 4

        If (isSessionLogArchived) Then
            gridLogEntries.Rows(currentIndex).Cells(viewLogColumnIndex) = New DataGridViewTextBoxCell With {
                .ToolTipText = My.Resources.ArchivedSessionLogTooltip
            }
            gridLogEntries.Rows(currentIndex).Cells(viewLogColumnIndex).Value = My.Resources.ArchivedSessionLog
            gridLogEntries.Rows(currentIndex).Cells(viewLogColumnIndex).ReadOnly = True
        Else
            gridLogEntries.Rows(currentIndex).Tag = session
        End If

    End Sub

    ''' <summary>
    ''' Handles the content being clicked in the log viewer to enable the
    ''' session log to be viewed.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub GridContentClicked(ByVal sender As Object, ByVal e As DataGridViewCellEventArgs) _
     Handles gridLogEntries.CellContentClick
        If e.RowIndex = -1 Then Return

        Select Case e.ColumnIndex
            Case 4
                Dim ent As SessionCompoundLogEntry =
                 TryCast(gridLogEntries.Rows(e.RowIndex).Tag, SessionCompoundLogEntry)
                If ent IsNot Nothing Then
                    If gSv.UserHasAccessToSession(ent.SessionID) Then
                        Dim f As New frmLogViewer(ent.SessionID)
                        f.ShowInTaskbar = False
                        f.ShowDialog()
                    Else
                        UserMessage.Show(My.Resources.YouDoNotHavePermissionToViewTheSessionLogsForThisProcessOrResource)
                    End If
                End If
        End Select
    End Sub

    Private Sub ExportEntireLogToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExportEntireLogToolStripMenuItem.Click
        Dim f As New frmScheduleLogExport()
        f.Populate(mInst)
        f.ShowInTaskbar = False
        f.ShowDialog()
    End Sub

    Private Sub CloseToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CloseToolStripMenuItem.Click
        Close()
    End Sub
End Class
