
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports System.Threading
Imports LocaleTools

Friend Class frmLogSearch
    Implements IPermission, IChild

#Region " Inner Classes "

    Private Class SearchReport
        Private mSessNo As Integer
        Private mMatch As Boolean
        Public Sub New(ByVal sessNo As Integer, ByVal isMatch As Boolean)
            mSessNo = sessNo
            mMatch = isMatch
        End Sub
        Public ReadOnly Property SessionNo() As Integer
            Get
                Return mSessNo
            End Get
        End Property
        Public ReadOnly Property IsMatch() As Boolean
            Get
                Return mMatch
            End Get
        End Property
    End Class

    Private Class SearchPayload

        ' A lock used to ensure that there are no race conditions when retrieving
        ' the next lock to be searched.
        Private ReadOnly mNextLock As New Object()

        ' The term being searched for
        Private mSearchTerm As String

        ' The fields to search in the session log
        Private mFields As ICollection(Of String)

        ' The initial count of session logs to search
        Private mInitCount As Integer

        ' Matched logs - keyed on session number, mapping the sequence numbers of
        ' the entries which matched.
        Private mMatches As IDictionary(Of Integer, ICollection(Of Long))

        ' The logs which have been checked and did not match
        Private mNonMatches As ICollection(Of Integer)

        ' The logs left to search
        Private mLeftToSearch As Queue(Of Integer)

        ' The last report recorded on this payload
        Private mLastReport As SearchReport

        ''' <summary>
        ''' Creates a new search payload for the logs identified by the given session
        ''' numbers, and looking for the specified search term
        ''' </summary>
        ''' <param name="searchTerm">The term to search for</param>
        ''' <param name="logsToSearch">The session numbers of the logs to search
        ''' </param>
        Public Sub New( _
         ByVal searchTerm As String, ByVal logsToSearch As ICollection(Of Integer), _
         ByVal fieldsToSearch As ICollection(Of String))
            mSearchTerm = searchTerm
            mLeftToSearch = New Queue(Of Integer)(logsToSearch)
            mInitCount = mLeftToSearch.Count
            mFields = GetSynced.ICollection(New clsSet(Of String)(fieldsToSearch))

            mMatches = GetSynced.IDictionary(
             New Dictionary(Of Integer, ICollection(Of Long)))
            mNonMatches = GetSynced.ICollection(New clsSet(Of Integer))

        End Sub

        ''' <summary>
        ''' The collection of fields to search on the database.
        ''' </summary>
        Public ReadOnly Property Fields() As ICollection(Of String)
            Get
                Return mFields
            End Get
        End Property

        ''' <summary>
        ''' The search term used to create this search payload
        ''' </summary>
        Public ReadOnly Property SearchTerm() As String
            Get
                Return mSearchTerm
            End Get
        End Property

        ''' <summary>
        ''' A map of sequence numbers against the session numbers in which a match
        ''' was found for the search term
        ''' </summary>
        ''' <remarks>Currently, this returns a collection containing only the first
        ''' sequence number, but this may change - not least because the SQL query
        ''' doing the search does a full search and then discards the results, but
        ''' the full search is still applied - we might as well use the whole data.
        ''' </remarks>
        Public ReadOnly Property Matches() _
         As IDictionary(Of Integer, ICollection(Of Long))
            Get
                Return mMatches
            End Get
        End Property

        ''' <summary>
        ''' A collection of session numbers which have been searched and in which no
        ''' match was found
        ''' </summary>
        Public ReadOnly Property NonMatches() As ICollection(Of Integer)
            Get
                Return mNonMatches
            End Get
        End Property

        ''' <summary>
        ''' Gets the session number for the next log to be searched.
        ''' </summary>
        ''' <param name="sessionNo">If there is another session to search, then on
        ''' exit this parameter will be set to the session number.</param>
        ''' <returns>True if there is a session to be searched and its number is set
        ''' in the <paramref name="sessionNo"/> parameter; False if there are no more
        ''' sessions to be searched.</returns>
        Public Function NextLog(ByRef sessionNo As Integer) As Boolean
            SyncLock mNextLock
                If mLeftToSearch.Count = 0 Then Return False
                sessionNo = mLeftToSearch.Dequeue()
                Return True
            End SyncLock
        End Function

        ''' <summary>
        ''' The initial count of sessions to be searched.
        ''' </summary>
        Public ReadOnly Property InitialCount() As Integer
            Get
                Return mInitCount
            End Get
        End Property

        ''' <summary>
        ''' The total number of searched sessions.
        ''' </summary>
        Public ReadOnly Property TotalSearched() As Integer
            Get
                Return (mMatches.Count + mNonMatches.Count)
            End Get
        End Property

        ''' <summary>
        ''' Implementation of adding a match sequence number to this payload.
        ''' This actually does the work that AddMatchSequenceNo invokes within a
        ''' block synchronized on the matches dictionary lock.
        ''' </summary>
        ''' <param name="sessionNo">The session number to add to</param>
        ''' <param name="logId">The logId to add</param>
        Private Sub InnerAddMatchLogId(sessionNo As Integer, logId As Long)
            Dim seqs As ICollection(Of Long) = Nothing
            If Not mMatches.TryGetValue(sessionNo, seqs) Then
                seqs = New List(Of Long)
                Matches(sessionNo) = seqs
            End If
            seqs.Add(logId)
        End Sub

        ''' <summary>
        ''' Adds the sequence number for a match to this payload.
        ''' </summary>
        ''' <param name="sessionNo">The session number to add a sequence number to.
        ''' </param>
        ''' <param name="logId">The logId of a matched record.</param>
        Public Sub AddMatchLogId(sessionNo As Integer, logId As Long)
            ' Ensure that this is thread safe within the matches dictionary
            DirectCast(mMatches, SynchronizedDelegator).PerformThreadSafeOperation(
             AddressOf InnerAddMatchLogId, sessionNo, logId)
        End Sub

        ''' <summary>
        ''' Gets the percentage complete for this search
        ''' </summary>
        Public ReadOnly Property PercentDone() As Integer
            Get
                If mInitCount = 0 Then Return 0
                Return CInt(100.0 * CDbl(TotalSearched) / CDbl(mInitCount))
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Worker class for a single thread to search the session logs for a particular
    ''' search term
    ''' </summary>
    Private Class FinderWorkerThread

        ' The event being monitored by the caller, set when the worker is finished
        Private mEvent As ManualResetEvent

        ' The owning log search form
        Private mOwner As frmLogSearch

        ''' <summary>
        ''' Creates a new finder worker object, owned by the given form, and with the
        ''' specified event to set when the worker is finished.
        ''' </summary>
        ''' <param name="owner">The owning form, on which the background worker which
        ''' spawned this worker resides.</param>
        ''' <param name="evt">The reset event to set when this worker is finished.
        ''' </param>
        Public Sub New(ByVal owner As frmLogSearch, ByVal evt As ManualResetEvent)
            mEvent = evt
            mOwner = owner
        End Sub

        ''' <summary>
        ''' The worker method for the thread that this worker represents.
        ''' </summary>
        ''' <param name="obj">The search payload providing the work for this object
        ''' to do.</param>
        Public Sub DoWork(ByVal obj As Object)

            Dim payload As SearchPayload = DirectCast(obj, SearchPayload)

            Dim sw As New Stopwatch()
            sw.Start()

            Try

                Dim sessNo As Integer = 0
                Dim count As Integer = 0
                ' Work through the next session number served up by the payload
                While payload.NextLog(sessNo)

                    ' If a cancellation has been requested, quit now
                    If mOwner.mFinder.CancellationPending Then Return

                    Debug.Assert(sessNo > 0) ' It should never be 0 or -ve now.
                    count += 1

                    ' Search this session.
                    Dim logId =
                     gSv.SearchSession(sessNo, payload.Fields, payload.SearchTerm)

                    ' Add as a match or a non-match into the payload - if it's a
                    ' match, ensure that the logId of the record on which
                    ' the term was found is recorded too.
                    If logId >= 0 Then
                        payload.AddMatchLogId(sessNo, logId)
                    Else
                        payload.NonMatches.Add(sessNo)
                    End If

                    ' Update the payload and report progress
                    mOwner.Enqueue(New SearchReport(sessNo, logId <> -1))

                    ' Not sure if ReportProgress is thread-safe or not; I can find no
                    ' docs telling me one way or the other, so I'll play it safe.
                    SyncLock mOwner.mProgressReportLock
                        mOwner.mFinder.ReportProgress(payload.PercentDone, payload)
                    End SyncLock

                End While
                sw.Stop()
                Console.WriteLine(My.Resources.SearchOf0LogSTook1MsIe2S,
                 count, sw.ElapsedMilliseconds, sw.ElapsedMilliseconds / 1000)

            Catch ex As Exception
                Debug.Fail(
                 "Error occurred while searching sessions: " & vbCrLf & ex.ToString())

            Finally
                mEvent.Set()

            End Try

        End Sub

    End Class

#End Region

#Region " Member Variables / Constants "

    ''' <summary>
    ''' The maximum number of worker threads to spawn for the search
    ''' </summary>
    Private Const MaxWorkerThreads As Integer = 10

    ' Lock for reporting progress by the worker threads
    Private ReadOnly mProgressReportLock As New Object()

    ' Indicates that the form should be closed when the search finishes.
    Private mCloseAfterCancellation As Boolean

    ' The last executed search payload 
    Private mPayload As SearchPayload

    ' The table containing all the sessions to be searched
    Private mLogTable As DataTable

    ' The queue of search reports built
    Private mQueue As Queue(Of SearchReport)

    Private ReadOnly mQueueLock As New Object()

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new log search form over the given process table and showing the
    ''' specified columns
    ''' </summary>
    ''' <param name="logTable">The datatable containing the logs to search</param>
    ''' <param name="visibleColumns">The columns to display in the search form.
    ''' </param>
    Public Sub New(ByVal logTable As DataTable,
     ByVal visibleColumns As ICollection(Of Integer))
        InitializeComponent()
        mQueue = New Queue(Of SearchReport)
        gridSessions.AutoGenerateColumns = False

        mLogTable = logTable
        gridSessions.DataSource = logTable

        'Hide and size columns as in the parent data grid view.
        For Each c As DataGridViewColumn In gridSessions.Columns
            c.Visible = visibleColumns.Contains(c.Index)
        Next
    End Sub

#End Region

#Region " Event Handling Methods "

    ''' <summary>
    ''' Handles the load event for this form. This loads up the field preferences
    ''' from the database.
    ''' </summary>
    Protected Overrides Sub OnLoad(ByVal e As EventArgs)
        MyBase.OnLoad(e)
        If DesignMode Then Return
        ' Get the saved search fields from the database and set them in this form
        ' (if they exist on the database)
        Try
            Dim flds As String = gSv.GetPref(PreferenceNames.UI.LogSearchFields, "")
            If flds <> "" Then Fields = New clsSet(Of String)(flds.Split(","c))
        Catch ' Ignore any errors in the database retrieval - it's not that important
        End Try
    End Sub

    ''' <summary>
    ''' Handles the fields menu being closed by ensuring that the fields are saved
    ''' to the database.
    ''' </summary>
    Private Sub HandleFieldsMenuClosed(
     ByVal sender As Object, ByVal e As ToolStripDropDownClosedEventArgs) _
     Handles ctxMenuFields.Closed

        If e.CloseReason = ToolStripDropDownCloseReason.CloseCalled Then Return
        Try
            gSv.SetUserPref("sysman.logsearch.fields",
             CollectionUtil.Join(Fields, ","))
        Catch
        End Try

    End Sub

    ''' <summary>
    ''' Makes sure that the user can't deselect all fields, which wouldn't be much
    ''' use by effectively disabling the checking of the last checked field item
    ''' </summary>
    Private Sub HandleFieldCheckedChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles mnuFieldsStageName.CheckedChanged, mnuFieldsResult.CheckedChanged,
      mnuFieldsProcessName.CheckedChanged, mnuFieldsParams.CheckedChanged,
      mnuFieldsPageName.CheckedChanged, mnuFieldsObjectName.CheckedChanged,
      mnuFieldsActionName.CheckedChanged

        Dim anyChecked As Boolean = False
        For Each mi As ToolStripMenuItem In ctxMenuFields.Items
            If mi.Checked Then anyChecked = True : Exit For
        Next
        If Not anyChecked Then DirectCast(sender, ToolStripMenuItem).Checked = True

    End Sub

    ''' <summary>
    ''' Shades rows as logs are being searched. Also applies the session status font
    ''' colour.
    ''' </summary>
    Private Sub HandleCellFormatting(
     ByVal sender As Object, ByVal e As DataGridViewCellFormattingEventArgs) _
     Handles gridSessions.CellFormatting

        Dim col As DataGridViewColumn = gridSessions.Columns(e.ColumnIndex)

        If col Is colStatus AndAlso e.Value IsNot Nothing Then
            Select Case e.Value.ToString
                Case "Pending" : e.CellStyle.ForeColor = Color.Orange
                Case "Running" : e.CellStyle.ForeColor = Color.Green
                Case "Completed" : e.CellStyle.ForeColor = Color.Blue
                Case "Stopped" : e.CellStyle.ForeColor = Color.Red
                Case "Debugging" : e.CellStyle.ForeColor = Color.Violet
                Case Else : e.CellStyle.ForeColor = Color.Black
            End Select
        End If
        e.Value = LTools.Get(e.Value.ToString, "misc", Options.Instance.CurrentLocale, "status")

    End Sub

    ''' <summary>
    ''' Handles the search of the DB.
    ''' </summary>
    Private Sub HandleFinderDoWork(ByVal sender As Object, ByVal e As DoWorkEventArgs) _
     Handles mFinder.DoWork

        Dim payload As SearchPayload = DirectCast(e.Argument, SearchPayload)

        Dim sw As New Stopwatch()
        sw.Start()

        ' No more workers than MaxWorkerThreads - there's no point in creating more
        ' workers than there are logs to search either.
        Dim threadCount As Integer = Math.Min(MaxWorkerThreads, payload.InitialCount)

        ' The events which the workers set to indicate that they are finished.
        Dim events(threadCount - 1) As ManualResetEvent

        ' The workers, in a list
        Dim workers As New List(Of FinderWorkerThread)

        ' Create the required workers, assign a ManualResetEvent to each one and
        ' add them into the threadpool ready to do the work.
        For i As Integer = 0 To threadCount - 1
            events(i) = New ManualResetEvent(False)
            Dim w As New FinderWorkerThread(Me, events(i))
            workers.Add(w)
            ThreadPool.QueueUserWorkItem(AddressOf w.DoWork, payload)
        Next

        ' Wait for all workers to complete
        WaitHandle.WaitAll(events)

        sw.Stop()
        Console.WriteLine(My.Resources.FullSearchWith0WorkerSTook1MsIe2S,
         threadCount, sw.ElapsedMilliseconds, sw.ElapsedMilliseconds / 1000)

        ' Set the result and exit
        e.Result = payload

    End Sub


    ''' <summary>
    ''' Responds to calls to BackgroundWorker.ReportProgress by updating the progress
    ''' bar.
    ''' </summary>
    Private Sub HandleFinderProgressChanged(
     ByVal sender As Object, ByVal e As ProgressChangedEventArgs) _
     Handles mFinder.ProgressChanged

        Dim payload As SearchPayload = DirectCast(e.UserState, SearchPayload)

        mProgressBar.Value = e.ProgressPercentage
        ' Set the label, unless cancellation is pending in which case, leave the
        ' "Cancelling..." message up there.
        If Not mFinder.CancellationPending Then _
         SetProgressLabel(payload)


        Dim report As SearchReport = Dequeue()
        If report Is Nothing Then Return

        Dim row As DataRow = GetRow(report.SessionNo)
        ' If it's a match, import it into the datatable held by the gridview
        ' If not, er, then don't
        If row IsNot Nothing AndAlso report.IsMatch Then _
         DirectCast(gridSessions.DataSource, DataTable).ImportRow(row)

    End Sub

    ''' <summary>
    ''' Handles the end of the background worker activity, a cancellation by the user
    ''' or an error. 
    ''' </summary>
    Private Sub HandleFinderCompleted(
     ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs) _
     Handles mFinder.RunWorkerCompleted

        Dim ex As Exception = e.Error
        If ex IsNot Nothing Then
            UserMessage.Show(
             String.Format(My.Resources.AnErrorOccurredWhileSearchingTheLogs0, ex.Message, ex))

            ' if cancellation was triggered by user attempting
            ' to close the form, follow through with their request
        ElseIf e.Cancelled AndAlso mCloseAfterCancellation Then
            Close()
            Return

        End If

        ' Reset all the UI elements to their default state
        mProgressBar.Value = mProgressBar.Minimum
        mProgressBar.Visible = False
        btnCancel.Text = My.Resources.Clear
        btnSearch.Enabled = True
        btnFields.Enabled = True
        lblMessage.Text = ""
        lnkViewLog.Visible = True
        pbViewLog.Visible = True

    End Sub

    ''' <summary>
    ''' Delays closure until any search in progress has been cancelled.
    ''' </summary>
    Protected Overrides Sub OnClosing(ByVal e As CancelEventArgs)
        If FinderCanBeCancelled() Then
            'Cancel the closure.
            e.Cancel = True
            'Set the flag to close after the background worker has finished.
            mCloseAfterCancellation = True
            CancelFinder()
        End If
        MyBase.OnClosing(e)
    End Sub

    ''' <summary>
    ''' Cancels a search.
    ''' </summary>
    Private Sub btnCancel_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnCancel.Click

        'Compare button text with "Stop" or resource equivalent
        If btnCancel.Text = My.Resources.frmLogSearch.btnCancel_Text Then
            lblMessage.Text = My.Resources.CancellingSearchPleaseWait
            CancelFinder()
        Else
            'Reset the search results.
            mPayload = Nothing

            btnCancel.Text = My.Resources.frmLogSearch.btnCancel_Text
            btnCancel.Enabled = False

            ' Reset the sessions back to their original value.
            gridSessions.DataSource = mLogTable

        End If

    End Sub

    ''' <summary>
    ''' Starts a search.
    ''' </summary>
    Private Sub btnSearch_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnSearch.Click
        Search()
    End Sub

    ''' <summary>
    ''' Starts a search if ENTER or F3 is pressed.
    ''' </summary>
    Private Sub txtSearch_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) _
     Handles cmbSearch.KeyDown

        ' Only allow the search if the button is enabled...
        If btnSearch.Enabled _
         AndAlso (e.KeyCode = Keys.Enter OrElse e.KeyCode = Keys.F3) Then
            Search()
        End If

    End Sub

    ''' <summary>
    ''' Displays the log viewer, starting at the first matching value if necessary.
    ''' </summary>
    Private Sub ContextViewLog_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles mnuViewSelectedLogs.Click
        ShowLogViewer()
    End Sub

    ''' <summary>
    ''' Handles the mousedown event for the grid, ensuring that the row beneath the
    ''' mouse is selected.
    ''' </summary>
    Sub HandleGridViewMouseDown(ByVal sender As Object, ByVal e As MouseEventArgs) _
     Handles gridSessions.MouseDown
        Dim hti As DataGridView.HitTestInfo = gridSessions.HitTest(e.X, e.Y)
        If hti.RowIndex >= 0 Then _
         gridSessions.Rows(hti.RowIndex).Selected = True
    End Sub

    ''' <summary>
    ''' Shows the context menu.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Friend Sub DataGridView_Click(ByVal sender As Object, ByVal e As MouseEventArgs) _
     Handles gridSessions.MouseUp
        If e.Button = MouseButtons.Right Then _
         mContextMenu.Show(gridSessions, e.Location)
    End Sub

    ''' <summary>
    ''' Shows the log viewer for the doubleclicked session
    ''' </summary>
    Private Sub DataGridView_DoubleClick(ByVal sender As Object, ByVal e As EventArgs) _
     Handles gridSessions.DoubleClick
        ShowLogViewer()
    End Sub

    ''' <summary>
    ''' Handles the 'View Log(s)' link being clicked.
    ''' </summary>
    Private Sub lnkViewLog_LinkClicked(
     ByVal sender As Object, ByVal e As LinkLabelLinkClickedEventArgs) _
     Handles lnkViewLog.LinkClicked
        ShowLogViewer()
    End Sub

    ''' <summary>
    ''' Ensures that the 'View Log' link label is enabled if session is selected, and
    ''' disabled otherwise.
    ''' </summary>
    Private Sub HandleSelectionChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles gridSessions.SelectionChanged
        lnkViewLog.Enabled = (gridSessions.SelectedCells.Count > 0)
    End Sub

    ''' <summary>
    ''' Handles the fields menu being entered by the cursor. This ensures that
    ''' checking a field on the menu doesn't cause the menu to close automatically.
    ''' </summary>
    Private Sub HandleFieldsMenuEntered(ByVal sender As Object, ByVal e As EventArgs) Handles ctxMenuFields.MouseEnter
        ctxMenuFields.AutoClose = False
    End Sub

    ''' <summary>
    ''' Handles the fields menu being left by the cursor. This ensures that clicking
    ''' elsewhere will cause the menu to close as expected.
    ''' </summary>
    Private Sub HandleFieldsMenuLeft(ByVal sender As Object, ByVal e As EventArgs) Handles ctxMenuFields.MouseLeave
        ctxMenuFields.AutoClose = True
    End Sub

#End Region

#Region " Queue Management Methods "

    ''' <summary>
    ''' Adds the given search report to the queue monitored and handled by this form.
    ''' </summary>
    ''' <param name="rep">The report to enqueue</param>
    Private Sub Enqueue(ByVal rep As SearchReport)
        If rep Is Nothing Then Return
        SyncLock mQueueLock
            mQueue.Enqueue(rep)
        End SyncLock
    End Sub

    ''' <summary>
    ''' Dequeues the next search report from the queue and returns it.
    ''' </summary>
    ''' <returns>The next search report in the queue or null if there is nothing in
    ''' the queue to return</returns>
    Private Function Dequeue() As SearchReport
        SyncLock mQueueLock
            If mQueue.Count = 0 Then Return Nothing
            Return mQueue.Dequeue()
        End SyncLock
    End Function

    ''' <summary>
    ''' Clears the report queue
    ''' </summary>
    Private Sub ClearReports()
        SyncLock mQueueLock
            mQueue.Clear()
        End SyncLock
    End Sub

#End Region

#Region " Other Methods "

    ''' <summary>
    ''' Gets the data row from the logs table which corresponds to the given session
    ''' number
    ''' </summary>
    ''' <param name="sessNo">The session number for which the corresponding data row
    ''' is required</param>
    ''' <returns>The data row representing the given session or null if there is no
    ''' such row.</returns>
    Private Function GetRow(ByVal sessNo As Integer) As DataRow
        For Each row As DataRow In mLogTable.Rows
            If sessNo = CInt(row("sessionnumber")) Then Return row
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Disposes of this form, cleaning up any appropriate references
    ''' </summary>
    ''' <param name="explicit">True if being called explicitly, false if being called
    ''' implicitly on finalization by the garbage collector</param>
    Protected Overrides Sub Dispose(ByVal explicit As Boolean)
        'Form overrides dispose to clean up the component list.
        If explicit Then ' We only clean up managed code in here.
            If components IsNot Nothing Then components.Dispose()
        End If

        MyBase.Dispose(explicit)
    End Sub

    ''' <summary>
    ''' Start the async search.
    ''' </summary>
    Private Sub Search()

        If cmbSearch.Text = "" Then
            ' Show all the rows when the search is cleared
            For Each r As DataGridViewRow In gridSessions.Rows
                r.Visible = True
            Next
        Else

            If Not cmbSearch.Items.Contains(cmbSearch.Text) Then _
             cmbSearch.Items.Add(cmbSearch.Text)

            mProgressBar.Visible = True
            btnCancel.Enabled = True
            btnCancel.Text = My.Resources.frmLogSearch.btnCancel_Text
            btnSearch.Enabled = False
            btnFields.Enabled = False
            lnkViewLog.Visible = False
            pbViewLog.Visible = False

            lblMessage.Text = My.Resources.InitialisingSearch

            ' Set a clone of the logs table - ie. just the structure - into the
            ' datagridview (effectively 'hiding' all rows in one big go)
            gridSessions.DataSource = mLogTable.Clone()

            ' Add the sessions in the log table into the list of sessions to search
            Dim sessNos As New List(Of Integer)
            For Each row As DataRow In mLogTable.Rows
                sessNos.Add(CInt(row("sessionnumber")))
            Next

            mPayload = New SearchPayload(cmbSearch.Text, sessNos, Fields)
            SetProgressLabel(mPayload)
            mFinder.RunWorkerAsync(mPayload)

        End If

    End Sub

    ''' <summary>
    ''' Gets or sets the fields to search in the next search of the sessions.
    ''' Note that the strings that this property recognises and returns are the
    ''' database field names which are stored in the 'Tag's of the corresponding
    ''' menu items.
    ''' </summary>
    Protected Property Fields() As ICollection(Of String)
        Get
            Dim flds As New clsSet(Of String)
            For Each item As ToolStripMenuItem In ctxMenuFields.Items
                Dim fldName As String = TryCast(item.Tag, String)
                If fldName IsNot Nothing AndAlso item.Checked Then flds.Add(fldName)
            Next
            Return flds
        End Get
        Set(ByVal value As ICollection(Of String))
            ' If there's nothing in the collection, set all fields on as a default
            Dim checkAll As Boolean = CollectionUtil.IsNullOrEmpty(value)
            For Each item As ToolStripMenuItem In ctxMenuFields.Items
                If Not item.CheckOnClick Then Continue For
                item.Checked = checkAll OrElse value.Contains(CStr(item.Tag))
            Next
        End Set
    End Property

    ''' <summary>
    ''' Sets the progress label from the given payload.
    ''' </summary>
    ''' <param name="p">The payload containing the data for the progress label
    ''' </param>
    Private Sub SetProgressLabel(ByVal p As SearchPayload)
        lblMessage.Text = String.Format(
         My.Resources.Searched01Logs, p.TotalSearched, p.InitialCount)
    End Sub

    ''' <summary>
    ''' Cancels any search in progess.
    ''' </summary>
    Private Sub CancelFinder()
        If FinderCanBeCancelled() Then mFinder.CancelAsync()
    End Sub

    ''' <summary>
    ''' Indicates whether the finder can currently be cancelled or not
    ''' </summary>
    ''' <returns>True if the search can be cancelled, false if it is not running or
    ''' a pending cancellation request has already been sent.</returns>
    Private Function FinderCanBeCancelled() As Boolean
        Return mFinder.IsBusy AndAlso Not mFinder.CancellationPending
    End Function

    ''' <summary>
    ''' Gets the bound DataRow associated with the given grid view row.
    ''' </summary>
    ''' <param name="row">The row for which the data row is required.</param>
    ''' <returns>The underlying DataRow corresponding to the given row</returns>
    Private Function GetDataRow(ByVal row As DataGridViewRow) As DataRow
        Return DirectCast(row.DataBoundItem, DataRowView).Row
    End Function

    ''' <summary>
    ''' Gets the session ID associated with the given grid view row.
    ''' </summary>
    ''' <param name="row">The row for which the session ID is required.</param>
    ''' <returns>The session ID corresponding to the given row</returns>
    Private Function GetSessionId(ByVal row As DataGridViewRow) As Guid
        Return DirectCast(GetDataRow(row)("sessionid"), Guid)
    End Function

    ''' <summary>
    ''' Gets the session number associated with the given row
    ''' </summary>
    ''' <param name="row">The row for which the session number is required</param>
    ''' <returns>The session number associated with the given row</returns>
    Private Function GetSessionNo(ByVal row As DataGridViewRow) As Integer
        Return DirectCast(GetDataRow(row)("sessionnumber"), Integer)
    End Function

    ''' <summary>
    ''' Shows log viewer forms for the sessions in the selected rows.
    ''' </summary>
    Private Sub ShowLogViewer()

        Dim sessIds As New clsSet(Of Guid)
        Dim payload As SearchPayload = mPayload
        ' If there is no payload to retrieve sequence numbers from, just open the
        ' log viewers in the default place.
        If payload Is Nothing Then
            For Each r As DataGridViewRow In gridSessions.SelectedRows
                mParent.StartForm(New frmLogViewer(GetSessionId(r), -1, ""))
            Next
        Else
            ' Otherwise, ensure that the payload is stable by locking on it, and
            ' use its data to retrieve sequence numbers at which the logs should
            ' be opened.
            SyncLock payload
                For Each r As DataGridViewRow In gridSessions.SelectedRows
                    Dim sessNo As Integer = GetSessionNo(r)

                    ' Get the sequence number if there is one
                    Dim logId =
                     CollectionUtil.First(Of Long)(payload.Matches(sessNo))

                    mParent.StartForm(New frmLogViewer(GetSessionId(r), logId, cmbSearch.Text))
                Next
            End SyncLock
        End If

    End Sub

#End Region

#Region " IPermission implementation "

    ''' <summary>
    ''' Gets the permission level for the control.
    ''' </summary>
    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName( _
             "Audit - Process Logs", "Audit - Business Object Logs")
        End Get
    End Property

#End Region

    Private mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

End Class
