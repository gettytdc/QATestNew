Imports BluePrism.AutomateAppCore
Imports BluePrism.BPCoreLib

Friend Class frmAuditLogViewer
    Inherits frmLogViewer

    ''' <summary>
    ''' The start of the displayed date range.
    ''' </summary>
    Private ReadOnly mFromDate As Date

    ''' <summary>
    ''' The end of the displayed date range.
    ''' </summary>
    Private ReadOnly mToDate As Date

    ''' <summary>
    ''' The search text in which to filter logs within the viewer.
    ''' </summary>
    Private ReadOnly mSearchText As String

    ''' <summary>
    ''' Constructor
    ''' </summary>
    ''' <param name="fromDate">The start date</param>
    ''' <param name="toDate">The end date</param>
    Public Sub New(fromDate As Date, toDate As Date, searchText As String)
        MyBase.New()
        InitializeComponent()

        'There will only be (for now) one page for audit 
        'logs so hide the controls not required.
        SinglePageMode = True

        mFromDate = fromDate.ToUniversalTime()
        mToDate = toDate.ToUniversalTime()
        mSearchText = searchText

        'Handle mouse clicks on the data grid
        RemoveHandler gridLog.MouseClick, AddressOf DataGridView_Click

        'Reset the cell reference
        mFindPosn = Nowhere

        'Start handling the cell formatting.
        AddHandler gridLog.CellFormatting, AddressOf HandleCellFormatting

        'Resize the datatable array to the number of log pages
        'and get the first page of data. Note there will only
        'be (for now) one page for audit logs.
        ReDim mPages(0)
        PopulateDataGridView()

        Text = My.Resources.AuditLogViewer

        If Not String.IsNullOrWhiteSpace(mSearchText) Then
            titleBar.Title = String.Format(My.Resources.AuditLog_Date_SearchTerm, Format(mFromDate.ToLocalTime(), "d"), mSearchText)
            titleBar.WrapTitle = True
        Else
            titleBar.Title = String.Format(My.Resources.AuditLog0, Format(mFromDate.ToLocalTime(), "d"))
        End If

        btnColour.CurrentColor = Color.Red
        Name = $"{Name}_{fromDate.Ticks}_{toDate.Ticks}_{searchText}"
    End Sub

    ''' <summary>
    ''' Handles a Find() operation being completed. If <paramref name="matchCell"/>
    ''' is equal to <see cref="Nowhere"/>, that indicates that there was no match
    ''' for the search.
    ''' </summary>
    ''' <param name="matchCell">The cell at which a match was found. If this is equal
    ''' to <see cref="Nowhere"/>, no match was found in the log.</param>
    Protected Overrides Sub HandleFound(matchCell As Point)
        If matchCell = Nowhere Then
            UserMessage.OK(My.Resources.TheTerm0CouldNotBeFound, mFindNextSearch.Text)
        Else
            ' There is a match on the current page, so highlight it.
            ScrollToCell(matchCell)
            mFindPosn = matchCell
            gridLog.Invalidate()
        End If
    End Sub

    ''' <summary>
    ''' Overrides the close checking and cleanup in the parent class.
    ''' </summary>
    Protected Overrides Function CanClose() As Boolean
        Return True
    End Function

    ''' <summary>
    ''' Fills the data grid view.
    ''' </summary>
    Protected Overrides Sub PopulateDataGridView()
        Cursor = Cursors.WaitCursor
        EnableControls(False)

        Try
            Dim auditLog = gSv.GetAuditLogsByDateRange(mFromDate, mToDate)

            If Not String.IsNullOrEmpty(mSearchText) Then
                mPages(0) = auditLog.Rows.OfType(Of DataRow).Where(Function(x) x.Item("Narrative").ToString().Contains(mSearchText, StringComparison.CurrentCultureIgnoreCase) OrElse
                    x.Item("Comments").ToString().Contains(mSearchText, StringComparison.CurrentCultureIgnoreCase)).CopyToDataTable
            Else
                mPages(0) = auditLog
            End If

            mPages(0).Columns(0).ColumnName = My.Resources.TimeLocal
            mPages(0).Columns(1).ColumnName = My.Resources.frmAuditLogViewer_Narrative
            mPages(0).Columns(2).ColumnName = My.Resources.frmAuditLogViewer_Comments
            gridLog.DataSource = mPages(0)
            gridLog.Refresh()
            gridLog.Columns(0).HeaderText = My.Resources.TimeLocal
            gridLog.Columns(1).HeaderText = My.Resources.frmAuditLogViewer_Narrative
            gridLog.Columns(2).HeaderText = My.Resources.frmAuditLogViewer_Comments
        Catch ex As Exception
            UserMessage.Err(ex, My.Resources.FailedToGetAuditLogDataFromDatabase0, ex.Message)
        End Try

        EnableControls(True)
        Cursor = Cursors.Default
    End Sub

    ''' <summary>
    ''' Used to disable controls when the form is fetching data.
    ''' </summary>
    ''' <param name="enable">True for enable</param>
    Protected Overrides Sub EnableControls(enable As Boolean)
        ' No effect for the audit log viewer
    End Sub

    ''' <summary>
    ''' Formats the cells displayed on screen.
    ''' </summary>
    Protected Overrides Sub HandleCellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs)
        If DesignMode Then Exit Sub

        'Colour the 'find all' cells.
        If (e.Value IsNot Nothing) AndAlso (Not (TypeOf e.Value Is DBNull)) Then
            For Each searchItem As SearchItem In mFindAllSearches.Values
                If searchItem.SearchPattern.Match(CStr(e.Value)).Success Then
                    e.CellStyle.BackColor = searchItem.HighlightColour
                End If
            Next
        End If

        'Colour the 'find next' cell. We do this after the
        '"find all" searches because this one should appear on top
        If mFindPosn.Equals(New Point(e.ColumnIndex, e.RowIndex)) Then
            e.CellStyle.BackColor = btnColour.CurrentColor
            Exit Sub
        End If

        If e.ColumnIndex = 0 Then
            e.Value = CType(e.Value, Date).ToLocalTime()
            e.CellStyle.Format = "T"
        End If
    End Sub

    ''' <summary>
    ''' Applies column size modes
    ''' </summary>
    ''' <remarks>I'm not convinced this is necessary now that it's not handling
    ''' data binding inside an event.</remarks>
    Protected Overrides Sub UpdateDataView()
        'This method is empty.
    End Sub

    ''' <summary>
    ''' Resizes data grid to match window size
    ''' </summary>
    Protected Overrides Sub OnSizeChanged(e As EventArgs)
        'Call base implementation
        MyBase.OnSizeChanged(e)

        'Exit if data grid is empty or has no columns
        If gridLog Is Nothing OrElse gridLog.Columns.Count = 0 Then Exit Sub

        'Set each column to the Fill AutoSizeMode
        For Each column As DataGridViewColumn In gridLog.Columns
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        Next
    End Sub

    ''' <summary>
    ''' Repopulates the data grid.
    ''' </summary>
    Protected Overrides Sub DoRefresh()
        ReDim mPages(0)
        PopulateDataGridView()
    End Sub

    ''' <summary>
    ''' Exports all the log data.
    ''' </summary>
    Protected Overrides Sub DoCompleteExport()
        Dim auditLogExportForm = New frmAuditLogExport() With
        {
            .DefaultFileName = My.Resources.BPAAuditLog,
            .Text = .Text.Replace(My.Resources.Log, My.Resources.AuditLog),
            .Title = .Title.Replace(My.Resources.Log, My.Resources.AuditLog)
        }
        auditLogExportForm.Populate(mPages, My.Resources.AuditLog0GTo1G, mFromDate, mToDate)
        ParentAppForm.StartForm(auditLogExportForm)
    End Sub

End Class
