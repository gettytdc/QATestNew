
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateProcessCore.Processes
Imports AutomateControls.Filters
Imports FilterNames = BluePrism.AutomateAppCore.clsSessionLogFilter.FilterNames
Imports AutomateControls
Imports LocaleTools
Imports BluePrism.Server.Domain.Models

Public Class ctlLogList
    Implements IChild

#Region " Class-scope Declarations "

    ''' <summary>
    ''' The filter definitions mapped against their column header titles
    ''' </summary>
    Private Shared mFilterDefns As _
     IDictionary(Of String, IFilterDefinition) = InitFilterDefns()

    ''' <summary>
    ''' Initialises the filter definitions, creating one for each column
    ''' </summary>
    ''' <returns>An initialised map of filter definitions mapped against their column
    ''' header names.</returns>
    Private Shared Function InitFilterDefns() As IDictionary(Of String, IFilterDefinition)
        Dim map As New Dictionary(Of String, IFilterDefinition)
        map(FilterNames.SessionNo) = New IntegerFilterDefinition(FilterNames.SessionNo)
        map(FilterNames.StartDate) = New PastDateFilterDefinition(FilterNames.StartDate)
        map(FilterNames.EndDate) = New PastDateFilterDefinition(FilterNames.EndDate)
        map(FilterNames.Process) = New StringFilterDefinition(FilterNames.Process)
        map(FilterNames.Status) = New StringFilterDefinition(FilterNames.Status)
        map(FilterNames.SourceLocn) = New StringFilterDefinition(FilterNames.SourceLocn)
        map(FilterNames.TargetLocn) = New StringFilterDefinition(FilterNames.TargetLocn)
        map(FilterNames.WindowsUser) = New StringFilterDefinition(FilterNames.WindowsUser)
        Return GetReadOnly.IDictionary(map)
    End Function

    ''' <summary>
    ''' Gets the filter definition corresponding to the given column
    ''' </summary>
    ''' <param name="column">The name (ie. the header cell's text) of the column for
    ''' which the filter definition is required.</param>
    ''' <returns>The filter definition corresponding to the given column name.
    ''' </returns>
    ''' <exception cref="NoSuchElementException">If no filter definition exists for
    ''' the specified column</exception>
    Public Shared Function GetFilterDefn(ByVal column As String) As IFilterDefinition
        Dim defn As IFilterDefinition = Nothing
        If mFilterDefns.TryGetValue(column, defn) Then Return defn
        Throw New NoSuchElementException(
         My.Resources.NoFilterDefinitionFoundFor0, column)
    End Function

    ''' <summary>
    ''' Creates a new filter for the given column
    ''' </summary>
    ''' <param name="column">The name (ie. the header cell's text) of the column for
    ''' which a filter is required.</param>
    ''' <returns>A newly created filter from the filter definition held against the
    ''' specified column.</returns>
    ''' <exception cref="NoSuchElementException">If no filter definition exists for
    ''' the specified column</exception>
    Public Shared Function CreateFilter(ByVal column As String) As Filter
        Return New Filter(GetFilterDefn(column))
    End Function

#End Region

#Region " Member Variables "

    Private Const InitialMaxRows = 1000
    Private Const InitialRowsPerPage = 100
    ' Flag indicating that the filters are currently being cleared.
    Private mClearingFilters As Boolean

    ' The filters monitoring the combo boxes in place on this control
    Private mFilters As FilterSet

    ' The session filter object maintained by this list
    Private mSessionFilter As clsSessionLogFilter

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new empty LogList control
    ''' </summary>
    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        RowsPerPage.MaxRows = InitialMaxRows
        RowsPerPage.RowsPerPage = InitialRowsPerPage

        AddHandler RowsPerPage.ConfigChanged, AddressOf Populate
        AddHandler RowsPerPage.PageChanged, AddressOf Populate

        mFilterDefns = InitFilterDefns() 'Reload to refresh string resources
        Me.colStatus.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colStatus.FillWeight = 50.0!

        ' Strangely, this property is not available in the designer
        gridLogs.AutoGenerateColumns = False

        ' Add any initialization after the InitializeComponent() call.
        mFilters = New FilterSet()
        mSessionFilter = New clsSessionLogFilter()

        ' Create a filter for each of the columns and show/hide it depending on
        ' whether the column is currently visible or not.
        ' Add the combo box to the flow panel above the gridview.
        filterFlow.Controls.Clear()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlLogList))
        For Each col As DataGridViewColumn In gridLogs.Columns

            Dim fil As Filter = CreateFilter(resources.GetString(col.Name & ".HeaderText", Globalization.CultureInfo.GetCultureInfo("en")))
            mFilters.Add(fil)
            ' We want the filter always on hand from the column so that we can
            ' handle visibility changes and width changes easily
            col.Tag = fil

            ' Format the combo box so that it fits neatly in with the grid columns
            Dim combo As ComboBox = fil.Combo
            combo.Margin = Padding.Empty
            combo.Width = col.Width
            combo.Visible = col.Visible
            filterFlow.Controls.Add(combo)

            AddHandler fil.FilterChanging, AddressOf HandleFilterChanging

            Dim mnu As New ToolStripMenuItem(col.HeaderText)
            mnu.Tag = col
            mnu.CheckOnClick = True
            mnu.Checked = col.Visible
            ctxMenuColHeader.Items.Add(mnu)

            col.HeaderCell.ContextMenuStrip = ctxMenuColHeader
            col.HeaderCell.Tag = mnu

            ' At the end of this, the column's tag is the filter; the column
            ' header cell's tag is the menu item corresponding to this column
        Next

        ' Set the filters to their defaults.
        ClearFilters()

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The process type to be used for this log list. A value of
    ''' <see cref="DiagramType.Unset"/> will cause the list to contain both
    ''' process and object logs.
    ''' </summary>
    <Browsable(True),
     Description("The process type that this logs list will display"),
     DefaultValue(DiagramType.Unset)>
    Public Property ProcessType() As DiagramType
        Get
            Return mSessionFilter.ProcessType
        End Get
        Set(ByVal value As DiagramType)
            mSessionFilter.ProcessType = value
            Dim header As String
            Select Case value
                Case DiagramType.Process : header = My.Resources.ctlLogList_Process
                Case DiagramType.Object : header = My.Resources.ctlLogList_Object
                Case Else : header = My.Resources.ctlLogList_ProcessObject
            End Select
            colProcess.HeaderText = header
        End Set
    End Property

    Private mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the data row associated with the datagridviewrow at the specified index.
    ''' </summary>
    ''' <param name="displayedRowIndex">The index within the DataGridView for which
    ''' the corresponding DataRow is required.</param>
    ''' <returns>The DataRow associated with the given grid view row index, or null
    ''' if the index is out of the bounds of the gridview, or if the row has no
    ''' DataRow associated with it.</returns>
    Private Function GetDataRow(ByVal displayedRowIndex As Integer) As DataRow

        If displayedRowIndex < 0 _
         OrElse displayedRowIndex >= gridLogs.Rows.Count Then Return Nothing

        Dim row As DataGridViewRow = gridLogs.Rows(displayedRowIndex)
        Return DirectCast(row.DataBoundItem, DataRowView).Row

    End Function

    ''' <summary>
    ''' Gets the DataRows for the currently selected rows in the gridview.
    ''' The returned collection is read-only.
    ''' </summary>
    Private ReadOnly Property SelectedDataRows() As ICollection(Of DataRow)
        Get
            If gridLogs.Rows.GetRowCount(DataGridViewElementStates.Selected) = 0 Then _
             Return GetEmpty.ICollection(Of DataRow)()

            Dim rows As New List(Of DataRow)
            For Each row As DataGridViewRow In gridLogs.SelectedRows
                rows.Add(DirectCast(row.DataBoundItem, DataRowView).Row)
            Next
            Return GetReadOnly.ICollection(rows) ' just to be consistently read-only
        End Get
    End Property

    ''' <summary>
    ''' The collection of GUIDs representing the IDs of the currently selected
    ''' sessions. Note that the returned collection is read-only, and the set
    ''' collection is not stored by this control - the contents are taken and set
    ''' in the list and the collection is ignored from then on.
    ''' </summary>
    Private Property SelectedSessionIds() As ICollection(Of Guid)
        Get
            Dim ids As New clsSet(Of Guid)
            For Each row As DataRow In SelectedDataRows
                ids.Add(DirectCast(row("sessionid"), Guid))
            Next
            Return GetReadOnly.ICollection(ids)
        End Get
        Set(ByVal value As ICollection(Of Guid))
            gridLogs.ClearSelection()
            If CollectionUtil.IsNullOrEmpty(value) Then Return

            Dim dv As DataView = DirectCast(gridLogs.DataSource, DataView)
            For Each id As Guid In value
                Dim index As Integer = dv.Find(id)
                If index = -1 Then Continue For
                gridLogs.Rows.SharedRow(index).Selected = True
                ' Dim dvRow As DataRowView = dv(index)
            Next
        End Set
    End Property

    ''' <summary>
    ''' The logs currently displayed in this control in the form of a DataTable.
    ''' </summary>
    Friend Property LogsTable() As DataTable
        Get
            Return DirectCast(gridLogs.DataSource, DataTable)
        End Get
        Set(ByVal value As DataTable)
            gridLogs.DataSource = value
        End Set
    End Property

#End Region

#Region " Event Handler Methods "

    ''' <summary>
    ''' Handles this list being loaded. This just ensures that the combo box text is
    ''' not selected - if a combo box is resized, its text is selected, and the
    ''' Resize event isn't fired until the ComboBox is displayed, so it must be done
    ''' when the control is loaded.
    ''' </summary>
    Protected Overrides Sub OnLoad(ByVal e As EventArgs)
        MyBase.OnLoad(e)
        ' Some of the combos get selected for some reason when the control is
        ' laid out. Put a stop to that nonsense right now.
        For Each combo As ComboBox In filterFlow.Controls
            combo.SelectionLength = 0
        Next
    End Sub

    ''' <summary>
    ''' Handles a filter changing in the log filters.
    ''' </summary>
    Sub HandleFilterChanging(
     ByVal source As Filter, ByVal e As FilterChangingEventArgs)
        If Not mClearingFilters Then Populate()
    End Sub

    ''' <summary>
    ''' Handles the state of a column changing - specifically, it handles a column
    ''' being set visible or invisible, ensuring that the associated filter follows
    ''' suit.
    ''' </summary>
    Sub HandleColumnChanged(
     ByVal sender As Object, ByVal e As DataGridViewColumnStateChangedEventArgs) _
     Handles gridLogs.ColumnStateChanged
        If e.StateChanged = DataGridViewElementStates.Visible Then
            Dim combo As ComboBox = GetFilterCombo(e.Column)
            If combo IsNot Nothing Then combo.Visible = e.Column.Visible
        End If
    End Sub

    ''' <summary>
    ''' Handles a column width changing, ensuring that the associated filter combo
    ''' box is updated with the same width.
    ''' </summary>
    Sub HandleColumnWidthChanged(
     ByVal sender As Object, ByVal e As DataGridViewColumnEventArgs) _
     Handles gridLogs.ColumnWidthChanged
        Dim combo As ComboBox = GetFilterCombo(e.Column)
        If combo IsNot Nothing Then combo.Width = e.Column.Width
    End Sub

    ''' <summary>
    ''' Applies the session status font colour.
    ''' </summary>
    Sub HandleCellFormatting(
     ByVal sender As Object, ByVal e As DataGridViewCellFormattingEventArgs) _
     Handles gridLogs.CellFormatting

        If e.ColumnIndex = colStatus.Index AndAlso e.Value IsNot Nothing Then
            Dim col As Color
            Select Case e.Value.ToString
                Case "Pending" : col = Color.Orange
                Case "Running" : col = Color.Green
                Case "Completed" : col = Color.Blue
                Case "Stopped" : col = Color.Red
                Case "Debugging" : col = Color.Violet
                Case Else : col = Color.Black
            End Select
            e.CellStyle.ForeColor = col
            e.Value = LTools.Get(e.Value.ToString, "misc", Options.Instance.CurrentLocale, "status")
        End If

    End Sub

    ''' <summary>
    ''' Handles the 'View Log' link label being clicked.
    ''' </summary>
    Sub HandleViewLogLink(
     ByVal sender As Object, ByVal e As LinkLabelLinkClickedEventArgs) _
     Handles lnkViewLog.LinkClicked
        ViewLogs(SelectedSessionIds)
    End Sub

    ''' <summary>
    ''' Handles the 'View Selected' menu item being clicked
    ''' </summary>
    Sub HandleViewLogMenuItem(ByVal sender As Object, ByVal e As EventArgs) _
     Handles mnuViewSelected.Click
        ViewLogs(SelectedSessionIds)
    End Sub

    ''' <summary>
    ''' Handles a log being double clicked in the gridview
    ''' </summary>
    Sub HandleLogDoubleClick(
     ByVal sender As Object, ByVal e As DataGridViewCellMouseEventArgs) _
     Handles gridLogs.CellMouseDoubleClick

        ' Only deal with left doubleclicks
        If e.Button <> System.Windows.Forms.MouseButtons.Left Then Return

        ' Get the data row corresponding to the given row index
        Dim row As DataRow = GetDataRow(e.RowIndex)
        If row Is Nothing Then Return ' ignore out-of-bounds double-clicks

        ' Select that row, disregarding the currently selected rows
        gridLogs.ClearSelection()
        gridLogs.Rows(e.RowIndex).Selected = True

        ' Open a log viewer for the selected row
        ViewLogs(New Guid() {DirectCast(row("sessionid"), Guid)})

    End Sub

    ''' <summary>
    ''' Handles the mouse entering the header context menu - this ensures that it is
    ''' not autoclosed on columns being clicked.
    ''' </summary>
    Sub HandleHeaderContextMenuMouseEnter(
     ByVal sender As Object, ByVal e As EventArgs) _
     Handles ctxMenuColHeader.MouseEnter
        ctxMenuColHeader.AutoClose = False
    End Sub

    ''' <summary>
    ''' Handles the mousedown event on a grid view cell, ensuring that the row
    ''' underneath the mouse at the point of clicking is selected.
    ''' </summary>
    Sub HandleCellMouseDown(
     ByVal sender As Object, ByVal e As DataGridViewCellMouseEventArgs) _
     Handles gridLogs.CellMouseDown

        ' Header clicks? Don't care.
        If e.RowIndex < 0 Then Return

        ' Left clicks? Already handled reasonably well, so we still don't care.
        If e.Button = MouseButtons.Left Then Return

        Dim row As DataGridViewRow = gridLogs.Rows(e.RowIndex)

        If (Control.ModifierKeys And Keys.Control) <> 0 Then
            ' Right click and control forces the row under the cursor to be selected
            row.Selected = True

        Else
            ' If the shift key is not being held down, and the row is not already
            ' selected, set the current cell to the one underneath the cursor
            If (Control.ModifierKeys And Keys.Shift) = 0 AndAlso
             Not row.Selected Then gridLogs.CurrentCell = row.Cells(e.ColumnIndex)

        End If


    End Sub

    ''' <summary>
    ''' Handles the mouse leaving the header context menu - this ensures that it is
    ''' autoclosed once more after the mouse has moved on from the context menu.
    ''' </summary>
    Sub HandleHeaderContextMenuMouseLeave(
     ByVal sender As Object, ByVal e As EventArgs) _
     Handles ctxMenuColHeader.MouseLeave
        ctxMenuColHeader.AutoClose = True
    End Sub

    ''' <summary>
    ''' Handles the header context menu being closed. This applies the column
    ''' visibility as requested by the user.
    ''' </summary>
    Sub HandleHeaderContextMenuClosed(
     ByVal sender As Object, ByVal e As ToolStripDropDownClosedEventArgs) _
     Handles ctxMenuColHeader.Closed
        ' Flag to ensure that at least one column is visible
        Dim anyVisible As Boolean = False

        ' Go through all of the context menu items and show or hide their
        ' corresponding columns as appropriate
        For Each item As ToolStripMenuItem In ctxMenuColHeader.Items
            anyVisible = anyVisible OrElse item.Checked
            DirectCast(item.Tag, DataGridViewColumn).Visible = item.Checked
        Next

        ' If they decided to hide all columns (silly users), redisplay the start
        ' column and show a wrist-slap error message.
        If Not anyVisible Then
            colStart.Visible = True
            DirectCast(colStart.HeaderCell.Tag, ToolStripMenuItem).Checked = True
            MessageBox.Show(My.Resources.AtLeastOneColumnMustBeDisplayed, My.Resources.ctlLogList_Error,
             MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub

    ''' <summary>
    ''' Handles one of the search menu items being clicked
    ''' </summary>
    Sub HandleSearchMenuItem(ByVal sender As Object, ByVal e As EventArgs) _
     Handles mnuSearchSelected.Click, mnuSearchAll.Click,
      lnkSearchSelected.Click, lnkSearchAll.Click
        ShowSearchForm(sender Is mnuSearchAll OrElse sender Is lnkSearchAll)
    End Sub

    ''' <summary>
    ''' Handles the 'Reset Filters' link being clicked
    ''' </summary>
    Sub HandleResetFiltersLink(
     ByVal sender As Object, ByVal e As LinkLabelLinkClickedEventArgs) _
     Handles lnkResetFilters.LinkClicked
        ClearFilters()
        Populate()
    End Sub

    Private Sub HandleColumnHeaderMouseClick(sender As Object, e As DataGridViewCellMouseEventArgs) Handles gridLogs.ColumnHeaderMouseClick
        If gridLogs.Columns(e.ColumnIndex).Name = mSessionFilter.SortColumn Then
            mSessionFilter.SortDirection = If(mSessionFilter.SortDirection = ListSortDirection.Ascending, ListSortDirection.Descending, ListSortDirection.Ascending)
        Else
            mSessionFilter.SortDirection = ListSortDirection.Ascending
        End If

        mSessionFilter.SortColumn = gridLogs.Columns(e.ColumnIndex).Name
        RowsPerPage.CurrentPage = 1
        Populate()

    End Sub
#End Region

#Region " Other Methods "

    ''' <summary>
    ''' Gets the filter combo box associated with the given column
    ''' </summary>
    ''' <param name="col">The column for which the filter combo is required.</param>
    ''' <returns>The filter combo box assocaited with the given column, or null if
    ''' no such combo box could be found.</returns>
    Function GetFilterCombo(ByVal col As DataGridViewColumn) As ComboBox
        Dim fil As Filter = TryCast(col.Tag, Filter)
        If fil Is Nothing Then Return Nothing Else Return fil.Combo
    End Function

    ''' <summary>
    ''' Clears the filters setting them back to the default - ie. all empty except
    ''' for the start date which is set to a default value
    ''' </summary>
    Private Sub ClearFilters()
        mClearingFilters = True
        Try
            For Each fil As Filter In mFilters
                fil.Clear()
            Next
            SetDefaultStartDateFilter()
        Finally
            mClearingFilters = False
        End Try
    End Sub

    Private Sub SetDefaultStartDateFilter()
        Dim filter = mFilters(FilterNames.StartDate)
        Dim defaultTerm = PastDateFilterDefinition.Terms.Today
        Dim defaultItem = filter.Definition.Items.
                FirstOrDefault(Function(i) i.FilterTerm = defaultTerm)
        filter.Combo.SelectedItem = defaultItem
    End Sub

    ''' <summary>
    ''' Displays the session search form for a data grid view.
    ''' </summary>
    ''' <param name="showAll">True to show all rows</param>
    Private Sub ShowSearchForm(ByVal showAll As Boolean)

        ' --- Copy the relevant data into a new datatable ---

        ' Get the original table from the dataview in the gridview
        Dim origDt As DataTable = DirectCast(gridLogs.DataSource, DataTable)

        ' Clone it - this copies structure, not data.
        Dim dt As DataTable = origDt.Clone()

        ' Get the rows we're interested - this collection will be the rows from
        ' the original datatable.
        Dim rows As ICollection(Of DataRow)

        ' If we're being asked to show all, or there are no rows selected, use
        ' all the logs in the grid. Otherwise, only use those which are selected.
        If showAll OrElse
         gridLogs.Rows.GetRowCount(DataGridViewElementStates.Selected) = 0 Then
            rows = GetReadOnly.ICollection(origDt.Select())
        Else
            rows = SelectedDataRows
        End If

        ' For each relevant row, import it into our new sub-table
        For Each r As DataRow In rows
            dt.ImportRow(r)
        Next

        ' Now, identify the columns we want to be visible in the search form
        Dim visibleCols As New clsSet(Of Integer)
        For Each c As DataGridViewColumn In gridLogs.Columns
            If c.Visible Then visibleCols.Add(c.Index)
        Next

        Dim ls As New frmLogSearch(dt, visibleCols)
        ls.StartPosition = FormStartPosition.CenterParent
        mParent.StartForm(ls)

    End Sub

    ''' <summary>
    ''' Opens log viewers for the session logs with the given IDs.
    ''' </summary>
    ''' <param name="ids">The collection of IDs for which the session log viewers are
    ''' required.</param>
    Private Sub ViewLogs(ByVal ids As ICollection(Of Guid))
        For Each id As Guid In ids
            Dim viewer As New frmLogViewer(id)
            Dim parent = GetAncestor(Of frmApplication)()
            If parent IsNot Nothing Then
                parent.StartForm(viewer)
            End If
        Next
    End Sub

    ''' <summary>
    ''' Updates the logs table from the database, applying the current filters.
    ''' </summary>
    Friend Sub Populate()
        mSessionFilter.SetFrom(mFilters.FilterMap)
        mSessionFilter.RowsPerPage = RowsPerPage.RowsPerPage
        mSessionFilter.CurrentPage = RowsPerPage.CurrentPage

        LogsTable = gSv.GetSessionLogsTable(mSessionFilter)
        RowsPerPage.TotalRows = If(LogsTable.Rows.Count > 0, CType(LogsTable.Rows(0)(0), Integer), 0)
        SetSortIcon()
    End Sub



    Private Sub SetSortIcon()
        For Each column As DataGridViewColumn In gridLogs.Columns
            If column.Name = mSessionFilter.SortColumn Then
                column.HeaderCell.SortGlyphDirection = If(mSessionFilter.SortDirection = ListSortDirection.Ascending, SortOrder.Ascending, SortOrder.Descending)
            Else
                column.HeaderCell.SortGlyphDirection = SortOrder.None
            End If
        Next
    End Sub

#End Region

End Class
