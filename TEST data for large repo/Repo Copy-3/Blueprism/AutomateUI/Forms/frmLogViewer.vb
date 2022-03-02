Imports System.Text.RegularExpressions
Imports System.Globalization
Imports System.Reflection
Imports System.Resources
Imports System.Xml
Imports AutomateControls
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Common.Security
Imports BluePrism.Core.Xml
Imports BluePrism.Utilities.Functional
Imports LocaleTools

''' Project  : Automate
''' Class    : AutomateUI.frmLogViewer
''' 
''' <summary>
''' Implements the session log viewer interface. The Audit Log viewer also inherits
''' the functionality defined here - see AutomateUI.frmAuditLogViewer.
''' </summary>
Friend Class frmLogViewer : Implements IPermission, IChild, IEnvironmentColourManager

#Region " Permission and Help "

    ''' <summary>
    ''' Gets help file string for this class.
    ''' </summary>
    ''' <returns>Help file.</returns>
    Public Overrides Function GetHelpFile() As String
        Return HelpFile
    End Function

    ''' <summary>
    ''' Opens online or offline help for this form.
    ''' </summary>
    Public Overrides Sub OpenHelp()
        Try
            OpenHelpFile(Me, GetHelpFile())
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub


    ''' <summary>
    ''' Gets the name of the associated help file.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName("System Manager")
        End Get
    End Property

#End Region

#Region " Constants "

    ''' <summary>
    ''' The page size default
    ''' </summary>
    Private Const DefaultRowsPerPage As Integer = 1000

    ''' <summary>
    ''' The help page
    ''' </summary>
    Private Const HelpFile As String = "frmLogsViewer.htm"

    ''' <summary>
    ''' A reference to row and column values -1,-1
    ''' </summary>
    Protected Shared ReadOnly Nowhere As New Point(-1, -1)

    ''' <summary>
    ''' Class to map column names to stongly typed identifiers
    ''' </summary>
    Public Class ColumnNames
        Public Shared LogNumber As String = "LogNumber"
        Public Shared Text As String = "Text"
        Public Shared Result As String = "Result"
        Public Shared StageName As String = "StageName"
        Public Shared ResourceStart As String = "Resource Start"
        Public Shared ResourceEnd As String = "Resource End"
        Public Shared StageType As String = "StageType"
        Public Shared Parameters As String = "Parameters"
        Public Shared StageId As String = "StageID"
        Public Shared Process As String = "Process"
        Public Shared [Object] As String = "Object"
        Public Shared Page As String = "Page"
        Public Shared ResultType As String = "ResultType"
        Public Shared Action As String = "Action"
        Public Shared BluePrismMemory As String = "Blue Prism Memory"
        Public Shared TargetAppID As String = "Target App ID"
        Public Shared TargetMemory As String = "Target Memory"

        ''' <summary>
        ''' System columns are hidden from view.
        ''' </summary>
        Public Class System
            Public Shared StageTypeValue As String = "StageTypeValue"
            Public Shared ResultTypeValue As String = "ResultTypeValue"
            Public Shared ParameterXml As String = "ParameterXml"
            Public Shared StartOffset As String = "StartOffset"
            Public Shared EndOffset As String = "EndOffset"
        End Class

    End Class


    ''' <summary>
    ''' System columns that will always be hidden
    ''' </summary>
    Private Shared ReadOnly SystemColumns As ICollection(Of String) =
     GetReadOnly.ICollectionFrom(
         ColumnNames.System.StageTypeValue,
         ColumnNames.System.ResultTypeValue,
         ColumnNames.System.ParameterXml,
         ColumnNames.System.StartOffset,
         ColumnNames.System.EndOffset)

    ''' <summary>
    ''' Columns that will be hidden by default
    ''' </summary>
    Private Shared ReadOnly DefaultHiddenColumns As ICollection(Of String) =
     GetReadOnly.ICollectionFrom(
        ColumnNames.LogNumber,
        ColumnNames.StageId,
        ColumnNames.Process,
        ColumnNames.Page,
        ColumnNames.Object,
        ColumnNames.Action,
        ColumnNames.ResultType,
        ColumnNames.ResourceEnd,
        ColumnNames.BluePrismMemory,
        ColumnNames.TargetAppID,
        ColumnNames.TargetMemory)

#End Region

#Region " Members "

    ''' <summary>
    ''' ResourceManager for custom resources belonging to control
    ''' </summary>
    Private Shared ReadOnly CustomResources As ResourceManager =
        My.Resources.frmLogViewer_Resources.ResourceManager

    ' The current view type
    Private mViewType As ViewType = ViewType.Grid

    ' The session number of the session being viewed
    Private mSessionNo As Integer

    ' The current (1-based) page being viewed
    Protected mPage As Integer

    ' The total number of rows in the session log
    Private mTotalRows As Integer

    ' The current page size
    Private mRowsPerPage As Integer

    ' Columns that the user has hidden
    Private mHiddenColumns As List(Of String)

    ' The session process name
    Private mProcessName As String

    ' The session start date
    Private mSessionDate As DateTimeOffset

    ' The parameter viewer
    Private WithEvents mLogParameterViewer As frmLogParameterViewer

    ' The size of the parameter viewer
    Private mLogParameterViewerSize As Size

    ' The row to scroll into view at form load time.
    Protected mInitialIndex As Integer

    ' The LogNumber field value in the selected row when the view type is changed.
    Private mSelectedLogNumber As Integer

    ' A flag to indicate that the current page field has been changed
    Protected mPageChangeRequired As Boolean

    ' The cached logs arrayed into data tables, each representing a single page
    Protected mPages As DataTable()

    ' The cell position of a 'found' cell
    Protected mFindPosn As Point

    ' The search item used for performing 'find next' operations.
    Protected mFindNextSearch As SearchItem

    ' A map of search items, keyed on entered search terms which the user has
    ' requested a 'Find All'
    Protected mFindAllSearches As New Dictionary(Of String, SearchItem)

    ' A reference to the point where the grid view context menu was activated
    Protected mClickLocn As Point

    ' Background Worker to search the logs (either cached or on the database)
    Protected WithEvents mFindWorker As BackgroundWorker

    ' A flag to indicate that the form is to be closed at when the background
    ' worker has finished.
    Protected mCloseAfterCancel As Boolean

    ' The list of columns to search in a log search effected within this form
    Protected mColumnsToSearch As List(Of String)

    ' The context menu
    Protected mContextMenu As ContextMenu

    ' The parent context menu item to all the individual column menu items.
    Private mColumnsContextMenu As MenuItem

    ' The menu item to show all the columns in the log.
    Private mShowAllContextMenu As MenuItem

    ' The grid mode context menu item, indicating that grid mode is selected
    Private mGridViewContextMenu As MenuItem

    ' The list mode context menu item, indicating that list mode is selected
    Private mListViewContextMenu As MenuItem

    ' The cache for the 'parameter details' tooltips, keyed on log sequence number.
    Private mTooltipCache As Dictionary(Of Integer, String)

    ' Flag indicating if this form is in single page mode
    Private mSinglePage As Boolean


#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new empty log viewer form
    ''' </summary>
    ''' <remarks>See descendant frmAuditLogViewer</remarks>
    Protected Sub New()
        Me.New(Guid.Empty, -1, "")
    End Sub

    ''' <summary>
    ''' Opens the log viewer at last page.
    ''' </summary>
    ''' <param name="sessId">The session ID</param>
    Public Sub New(ByVal sessId As Guid)
        Me.New(sessId, -1, "")
    End Sub

    ''' <summary>
    ''' Opens the log viewer at the given sequence number.
    ''' </summary>
    ''' <param name="sessId">The session ID</param>
    ''' <param name="logId">The logId at which the log should be opened.
    ''' -1 indicates that the log should be opened at the last entry.</param>
    Public Sub New(ByVal sessId As Guid, ByVal logId As Long)
        Me.New(sessId, logId, "")
    End Sub

    ''' <summary>
    ''' Opens the log viewer at at particular point in the logs
    ''' </summary>
    ''' <param name="sessId">The session ID</param>
    ''' <param name="logId">The logId to start viewing from, or -1
    ''' to start at the end of the last page.</param>
    ''' <param name="searchStr">The search term</param>
    Public Sub New(
     ByVal sessId As Guid, ByVal logId As Long, ByVal searchStr As String)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Make sure all tooltips have a 20 second delay.
        Dim fld As FieldInfo = GetType(DataGridView).GetField("toolTipControl",
         BindingFlags.Instance Or BindingFlags.Static Or BindingFlags.NonPublic)

        Dim ttCtl As Object = fld.GetValue(gridLog)
        Dim ttFld As FieldInfo = ttCtl.GetType.GetField("toolTip",
         BindingFlags.Instance Or BindingFlags.Static Or BindingFlags.NonPublic)

        'Create a tooltip now with default values as would be created
        'Lazily by the datagrid.
        Dim tt As New ToolTip()
        tt.ShowAlways = True
        tt.InitialDelay = 0
        tt.UseFading = False
        tt.UseAnimation = False

        'Set the AutoPopDelay to 20000 miliseconds instead of 0
        tt.AutoPopDelay = 20000

        'Apply the change
        ttFld.SetValue(ttCtl, tt)

        'Initialise the tooltip cache
        mTooltipCache = New Dictionary(Of Integer, String)

        If searchStr <> "" Then
            cmbFind.Text = searchStr
            mFindNextSearch = GetSearchTerm(cmbFind.Text)
        End If

        If sessId <> Guid.Empty Then DoSetup(sessId, logId)

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The number of pages in the log
    ''' </summary>
    <Browsable(False)>
    Protected ReadOnly Property TotalPages() As Integer
        Get
            If mTotalRows = 0 Then Return 1
            Return CInt(Math.Ceiling(mTotalRows / mRowsPerPage))
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the single page mode for this form. Single page mode will remove
    ''' all menu items and controls which offer page selection, search or export
    ''' functionality.
    ''' </summary>
    <Browsable(True), DefaultValue(False),
     Description("Removes navigation and search functionality from the form")>
    Public Property SinglePageMode() As Boolean
        Get
            Return mSinglePage
        End Get
        Set(ByVal value As Boolean)
            If value = mSinglePage Then Return
            mSinglePage = value

            ' Collapse the search pane and hide all of the search/navigate controls
            Dim vis As Boolean = Not mSinglePage

            splitPane.Panel1Collapsed = splitPane.Panel1Collapsed OrElse mSinglePage
            panSearchConstraints.Visible = vis
            panPageControl.Visible = vis
            lnkToggleSearch.Visible = vis

            mnuExportPage.Visible = vis
            mnuListModeSeparator.Visible = vis
            mnuListMode.Visible = vis
            mnuGridMode.Visible = vis
            mnuColumnsSeparator.Visible = vis
            mnuColumns.Visible = vis
            mnuShowAll.Visible = vis

        End Set
    End Property

    ''' <summary>
    ''' The underlying data columns to search, as opposed to the visible data columns
    ''' to search. This primarily ensures that the 'Parameters' columns is replaced
    ''' by the corresponding data column, ie. 'ParameterXml'.
    ''' </summary>
    Private ReadOnly Property DataColumnsToSearch() As ICollection(Of String)
        Get
            If CollectionUtil.IsNullOrEmpty(Of String)(mColumnsToSearch) Then _
             Return GetEmpty.ICollection(Of String)()
            Dim cols As New List(Of String)
            For Each c As String In mColumnsToSearch
                If c = "Parameters" Then cols.Add("ParameterXml") Else cols.Add(c)
            Next
            Return GetReadOnly.ICollection(cols)
        End Get
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Initialisation
    ''' </summary>
    ''' <param name="sessId">The session ID</param>
    ''' <param name="logId">The logId to start viewing from, or -1
    ''' to start at the end of the last page.</param>
    Private Sub DoSetup(sessId As Guid, logId As Long)

        mSessionNo = gSv.GetSessionNumber(sessId)

        'Get the size of the log and work out the number of pages required.
        mRowsPerPage = DefaultRowsPerPage
        mTotalRows = gSv.GetLogsCount(mSessionNo)

        'Work out which page and row to start on.
        If logId = -1 Then
            'Start at the bottom of the last page.
            mPage = TotalPages
            If TotalPages = 0 Then
                mPage = 0
                mInitialIndex = 0
            Else
                mInitialIndex = mTotalRows - ((mPage - 1) * mRowsPerPage) - 1
            End If
        Else
            Dim logNumber = gSv.LogIdToLogNumber(mSessionNo, logId)
            logNumber = Math.Min(logNumber, mTotalRows)
            If TotalPages = 0 Then
                mPage = 0
                mInitialIndex = 0
            Else
                mPage = CInt(Math.Ceiling(logNumber / mRowsPerPage))
                mInitialIndex = logNumber - ((mPage - 1) * mRowsPerPage)
            End If
        End If

        'Set the 'page x of y' text boxes
        txtCurrentPage.Text = CStr(mPage)
        txtTotalPages.Text = CStr(TotalPages)

        'Handle the current page changes.
        AddHandler txtCurrentPage.KeyDown, AddressOf txtCurrentPage_KeyDown
        AddHandler txtCurrentPage.LostFocus, AddressOf txtCurrentPage_LostFocus
        AddHandler txtCurrentPage.TextChanged, AddressOf txtCurrentPage_TextChanged

        'Set page navigation buttons
        If mTotalRows = 0 Then
            EnableControls(False)
        Else
            btnPrevPage.Enabled = mPage > 1
            btnFirstPage.Enabled = btnPrevPage.Enabled
            btnNextPage.Enabled = mPage < TotalPages
            btnLastPage.Enabled = btnNextPage.Enabled
        End If

        'Set the rows per page drop down
        cmbRowsPerPage.Text = mRowsPerPage.ToString
        If mTotalRows > 0 Then
            AddHandler cmbRowsPerPage.SelectedIndexChanged, AddressOf cmbRowsPerPage_SelectedIndexChanged
        End If

        'Reset the cell reference
        mFindPosn = Nowhere

        'Start with the search panel collapsed.
        splitPane.Panel1Collapsed = True

        'Set title and subtitle
        Text = My.Resources.SessionLogViewer

        Dim sessions = gSv.GetActualSessions(sessId)
        Dim sess = sessions.First

        mProcessName = sess.ProcessName
        mSessionDate = sess.SessionStart

        titleBar.Title = GetTitle(sess) & mProcessName.Truncate(80, My.Resources.Ellipsis) & vbCrLf _
         & My.Resources.SessionDate & mSessionDate.DateTime.ToString()

        'Set colour button
        btnColour.CurrentColor = Color.Red

        'Handle the view type drop down
        AddHandler cmbViewType.SelectedValueChanged, AddressOf cmbViewType_SelectedValueChanged

        'Handle the view type menus
        mnuListMode.Checked = mViewType = ViewType.List
        mnuGridMode.Checked = mViewType = ViewType.Grid
        AddHandler mnuListMode.Click, AddressOf mnuViewMode_Click
        AddHandler mnuGridMode.Click, AddressOf mnuViewMode_Click

        'Handle mouse clicks on the data grid
        AddHandler gridLog.MouseClick, AddressOf DataGridView_Click

        CreateContextMenu()

        'Create the background worker.
        mFindWorker = New BackgroundWorker()
        mFindWorker.WorkerReportsProgress = True
        mFindWorker.WorkerSupportsCancellation = True

        'Populate the data grid.
        If TotalPages > 0 Then
            DisposeOfPages()

            'Resize the datatable array to the number of log pages
            ReDim mPages(TotalPages - 1)

            'Get the first page of data.
            PopulateDataGridView()
        Else
            'This session has no logs
            lblNoLogs.Visible = True
            gridLog.Visible = False
        End If


    End Sub

    Private Shared Function GetTitle(session As ISession) As String
        Dim processType = Processes.DiagramType.Unset
        gSv.GetProcessInfo(session.ProcessID, Nothing, Nothing, Nothing, Nothing, processType)
        Return If(processType = Processes.DiagramType.Object, My.Resources.ctlProcessViewer_ObjectColin, My.Resources.ProcessColon)
    End Function

    ''' <summary>
    ''' Creates a context menu for the data grid
    ''' </summary>
    Private Sub CreateContextMenu()

        mContextMenu = New ContextMenu()
        Dim item As MenuItem

        mGridViewContextMenu = New MenuItem(My.Resources.GridView)
        mGridViewContextMenu.Checked = mViewType = ViewType.Grid
        AddHandler mGridViewContextMenu.Click, AddressOf ContextGrid_Click
        mContextMenu.MenuItems.Add(mGridViewContextMenu)

        mListViewContextMenu = New MenuItem(My.Resources.ListView)
        mListViewContextMenu.Checked = mViewType = ViewType.List
        AddHandler mListViewContextMenu.Click, AddressOf ContextList_Click
        mContextMenu.MenuItems.Add(mListViewContextMenu)

        mContextMenu.MenuItems.Add(New MenuItem(My.Resources.Hyphen))

        item = New MenuItem(My.Resources.CopySelectedCells)
        AddHandler item.Click, AddressOf ContextCopy_Click
        mContextMenu.MenuItems.Add(item)

        item = New MenuItem(My.Resources.CopyAll)
        AddHandler item.Click, AddressOf ContextCopyAll_Click
        mContextMenu.MenuItems.Add(item)

        mContextMenu.MenuItems.Add(New MenuItem(My.Resources.Hyphen))

        item = New MenuItem(My.Resources.HideColumn)
        AddHandler item.Click, AddressOf ContextHide_Click
        mContextMenu.MenuItems.Add(item)

        mShowAllContextMenu = New MenuItem(My.Resources.ShowAllColumns)
        AddHandler mShowAllContextMenu.Click, AddressOf ContextShowAll_Click
        mContextMenu.MenuItems.Add(mShowAllContextMenu)

        mColumnsContextMenu = New MenuItem(My.Resources.Columns)
        mContextMenu.MenuItems.Add(mColumnsContextMenu)

    End Sub

    ''' <summary>
    ''' Fills the data grid view with the current page of data. If the page has
    ''' already been visited the data will be in the array of data tables. Otherwise
    ''' go to the DB.
    ''' </summary>
    Protected Overridable Sub PopulateDataGridView()

        If DesignMode Then Exit Sub

        Dim logTable As DataTable = Nothing
        Dim sErr As String = ""
        Dim currRow As Integer
        Dim gotLogs As Boolean
        Dim hiddenCols As Integer

        Select Case mPage
            ' Should never be zero
            Case 0 : Exit Sub
            Case 1 : currRow = 1
            Case Else : currRow = CInt(mRowsPerPage * (mPage - 1) + 1)
        End Select

        EnableControls(False)

        'Get the datatable from the cache or from the DB if necessary.
        If mPage > 0 AndAlso mPage <= mPages.Length _
         AndAlso mPages(mPage - 1) IsNot Nothing Then
            'This page has been saved from a previous query.
            logTable = mPages(mPage - 1)
            gotLogs = True
        Else
            'Get this page from the DB
            If mViewType = ViewType.List Then
                Try
                    logTable = gSv.GetLogsAsText(mSessionNo, currRow, mRowsPerPage)
                    FormatLogsForTextMode(logTable)
                    gotLogs = True
                Catch
                End Try
            Else
                Try
                    logTable = gSv.GetLogs(mSessionNo, currRow, mRowsPerPage)
                    gotLogs = True
                Catch ex As Exception
                    sErr = ex.Message
                End Try

                If gotLogs Then
                    'If the choice of hidden columns is unknown then read from the DB.
                    If mHiddenColumns Is Nothing Then
                        mHiddenColumns = New List(Of String)
                        hiddenCols = gSv.GetLogViewerHiddenColumns()

                        If hiddenCols = -1 Then
                            'Use the default if there was nothing in the DB
                            mHiddenColumns.AddRange(DefaultHiddenColumns)
                        Else
                            'Look through the columns of this datatable
                            For c As Integer = 0 To logTable.Columns.Count - 1
                                If (CInt(2 ^ c) And hiddenCols) > 0 Then
                                    mHiddenColumns.Add(logTable.Columns(c).ColumnName)
                                End If
                            Next
                            'Parameters is not part of the datatable but is added 
                            'as an extra column to the datagrid after data binding.
                            If (CInt(2 ^ logTable.Columns.Count) And hiddenCols) > 0 Then
                                mHiddenColumns.Add("Parameters")
                            End If
                        End If
                    End If

                End If

            End If
        End If

        If gotLogs Then
            mFindPosn = Nowhere
            mPages(mPage - 1) = logTable
            gridLog.DataSource = logTable
            UpdateDataView()
            gridLog.Refresh()

        ElseIf sErr <> "" Then
            UserMessage.Show(String.Format(My.Resources.AnErrorOccurredWhileRetrievingTheLogs0, sErr))

        End If

        EnableControls(True)

    End Sub

    ''' <summary>
    ''' Adds a text row to the given data table at the specified row number
    ''' </summary>
    ''' <param name="dt">The table to which the row should be added</param>
    ''' <param name="logNo">The log number to set in the row</param>
    ''' <param name="txt">The text to set in the row</param>
    ''' <param name="rowNo">The index within the datatable that the row should be
    ''' inserted into</param>
    Private Sub AddTextRow(ByVal dt As DataTable,
     ByVal logNo As Integer, ByVal rowNo As Integer,
     ByVal txt As String, ByVal ParamArray args() As Object)
        Dim newRow As DataRow = dt.NewRow
        newRow("LogNumber") = logNo
        newRow("Text") = String.Format(txt, args)
        dt.Rows.InsertAt(newRow, rowNo)
    End Sub

    ''' <summary>
    ''' Formats log data for display in V2 list style
    ''' </summary>
    ''' <param name="dt">The logs</param>
    Private Sub FormatLogsForTextMode(ByVal dt As DataTable)

        For Each row As DataRow In dt.Select(
         "(Result <> '' AND Result IS NOT NULL) OR ParameterXml IS NOT NULL OR ObjectName IS NOT NULL")

            'If there are parameters, insert new rows into the data table.
            Dim objectName As String = BPUtil.IfNull(row("ObjectName"), "")
            If objectName = "" Then Continue For

            'If there are parameters, insert new rows into the data table.
            Dim paramXml As String = BPUtil.IfNull(row("ParameterXml"), "")
            If paramXml = "" Then Continue For

            Dim rowNo = dt.Rows.IndexOf(row)
            Dim logNo = CInt(row("LogNumber"))
            Dim offset = 0
            Dim xmlDoc As New ReadableXmlDocument(paramXml)

            For Each n As XmlElement In xmlDoc.DocumentElement
                Dim dirn = If(n.Name = "inputs", "input", "output")
                Dim argList As clsArgumentList =
                 clsArgumentList.XMLToArguments(n.OuterXml, dirn = "output")

                For Each arg As clsArgument In argList

                    Dim startDate = CType(row("Start"), DateTime)

                    ' Describe the argument - include the value if it's not a
                    ' collection argument (the collection is fully described later).
                    offset += 1
                    AddTextRow(dt, logNo, rowNo + offset,
                     My.Resources.x01Parameter234,
                     startDate, If(dirn = "input", My.Resources.frmLogViewer_Input, My.Resources.frmLogViewer_Output),
                     clsBusinessObjectAction.GetLocalizedFriendlyName(arg.Name, objectName, "Params"), arg.Value.DataTypeName,
                     IIf(arg.Value.DataType = DataType.collection, "", My.Resources.SpaceHyphenSpace & arg.Value.FormattedValue))

                    'If the parameter is a collection, work through the rows.
                    If arg.Value.DataType <> DataType.collection Then Continue For

                    ' A null collection means nothing to show
                    If arg.Value.IsNull Then Continue For

                    Dim rowIndex As Integer = 1
                    For Each collRow As clsCollectionRow In arg.Value.Collection.Rows
                        For Each fld As String In collRow.FieldNames
                            Dim val As clsProcessValue = collRow(fld)
                            offset += 1
                            AddTextRow(dt, logNo, rowNo + offset,
                             My.Resources.x0COLLECTIONRow1Field234,
                             row("Start"), rowIndex, fld,
                             val.DataTypeName, val.FormattedValue)
                        Next
                        rowIndex += 1
                    Next collRow
                Next arg
            Next n
        Next row

        ' Remove the columns no longer required for text view.
        For i As Integer = dt.Columns.Count - 1 To 0 Step -1
            Dim colName As String = dt.Columns(i).ColumnName
            If colName <> "Text" AndAlso colName <> "LogNumber" Then _
             dt.Columns.Remove(colName)
        Next

    End Sub

    ''' <summary>
    ''' Used to disable controls when the form is fetching data.
    ''' </summary>
    ''' <param name="enable">True for enable the controls if they were previously
    ''' enabled before being disabled by this method. False to disable and save the
    ''' state of the controls.</param>
    Protected Overridable Sub EnableControls(ByVal enable As Boolean)

        For Each c As Control In New Control() {
         btnFirstPage, btnPrevPage, btnNextPage, btnLastPage, cmbRowsPerPage}
            If enable Then
                ' Restore the saved enabled state
                c.Enabled = (c.Tag Is Nothing OrElse CBool(c.Tag))
                c.Tag = Nothing
            Else
                c.Tag = c.Enabled
                c.Enabled = False
            End If
        Next

    End Sub

#End Region

#Region " DataGridViewHandlers "

    ''' <summary>
    ''' Sets up the data grid once the data has been attached.
    ''' </summary>
    Protected Overridable Sub UpdateDataView()

        If DesignMode Then Exit Sub

        If Not mViewType = ViewType.List AndAlso Not gridLog.Columns.Contains("Parameters") Then
            'Add a new hyperlink column for the parameters.
            Dim linkCol As New DataGridViewLinkColumn()
            linkCol.LinkBehavior = LinkBehavior.AlwaysUnderline
            linkCol.LinkBehavior = LinkBehavior.SystemDefault
            linkCol.LinkColor = Color.Blue
            linkCol.Name = "Parameters"
            linkCol.HeaderText = My.Resources.Parameters
            linkCol.UseColumnTextForLinkValue = True
            gridLog.Columns.Insert(gridLog.Columns.Count, linkCol)
        End If

        If mViewType = ViewType.List Then
            'In list view hide the log number column.
            If gridLog.Columns.Contains("LogNumber") Then
                gridLog.Columns("LogNumber").Visible = False
            End If
        Else
            'Hide the system columns
            For Each sColumn As String In SystemColumns
                If gridLog.Columns.Contains(sColumn) Then
                    gridLog.Columns(sColumn).Visible = False
                End If
            Next

            'Hide the user's choice of columns
            If mHiddenColumns IsNot Nothing Then
                For Each sColumn As String In mHiddenColumns
                    If gridLog.Columns.Contains(sColumn) Then
                        gridLog.Columns(sColumn).Visible = False
                    End If
                Next
            End If
        End If

        'By default the visible columns will use autosize mode to fit 
        'within the available width of the grid. Autosizing will later 
        'be switched off so that when the user expands a column the 
        'datagrid will scroll. 
        For Each oDataGridViewColumn As DataGridViewColumn In gridLog.Columns
            oDataGridViewColumn.HeaderText = GetLocalizedFriendlyNameHeader(oDataGridViewColumn.Name) 'clsUtility.GetUnCamelled(oDataGridViewColumn.Name)
            ApplyAutoSizeColumnMode(oDataGridViewColumn)
        Next

        'Disable the column selection menu item when in text view.
        If mViewType = ViewType.List Then
            mnuColumns.Visible = False
        Else
            mnuColumns.Visible = True
            mnuShowAll.Enabled = mHiddenColumns.Count > 0
            mnuShowAll.Checked = mHiddenColumns.Count = 0

            'Create show/hide menu items for each column.
            Dim oMenuItem As MenuItem
            mnuColumns.MenuItems.Clear()
            For Each oDataGridViewColumn As DataGridViewColumn In gridLog.Columns
                If Not SystemColumns.Contains(oDataGridViewColumn.Name) Then
                    oMenuItem = mnuColumns.MenuItems.Add(oDataGridViewColumn.HeaderText, AddressOf mnuColumn_Click)
                    oMenuItem.Tag = oDataGridViewColumn.Name
                    oMenuItem.Name = oDataGridViewColumn.Name
                    oMenuItem.Checked = oDataGridViewColumn.Visible
                End If
            Next

            'Create show/hide context menu items for each column.
            mColumnsContextMenu.MenuItems.Clear()
            For Each oDataGridViewColumn As DataGridViewColumn In gridLog.Columns
                If Not SystemColumns.Contains(oDataGridViewColumn.Name) Then
                    oMenuItem = mColumnsContextMenu.MenuItems.Add(oDataGridViewColumn.HeaderText, AddressOf mnuColumn_Click)
                    oMenuItem.Tag = oDataGridViewColumn.Name
                    oMenuItem.Name = oDataGridViewColumn.Name
                    oMenuItem.Checked = oDataGridViewColumn.Visible
                End If
            Next

            'Not sure how, but when the link column is first added, 
            'it is positioned at the end of the DataGridView1.Columns 
            'collection. When you get here though, it has 'moved' up to zero.
            mnuColumns.MenuItems("Parameters").Index = mnuColumns.MenuItems.Count - 1
            mColumnsContextMenu.MenuItems("Parameters").Index = mColumnsContextMenu.MenuItems.Count - 1

        End If

        PopulateColumnsToSearchListBox()

        'If the datagrid has been populated as a result of a view type 
        'change, maintain the selection from the previous view type.
        If mSelectedLogNumber > 0 Then

            'Count the number of rows with a lower LogNumber than the 
            'previously selected log. This will give the index of the 
            'first row with the same LogNumber as before.
            Dim iRow As Integer = mPages(mPage - 1).Select("LogNumber<" & CStr(mSelectedLogNumber)).Length
            If iRow >= 0 AndAlso iRow < gridLog.RowCount Then
                ' Ensure that the selected row is in view.
                If Not gridLog.Rows(iRow).Displayed Then ScrollToRow(iRow)
                SelectFirstCell(iRow)
            End If

        End If

    End Sub

    ''' <summary>
    ''' Gets the localized friendly name for column according to the current culture.
    ''' </summary>
    ''' <param name="columnName">The column name string</param>
    ''' <returns>The localised column string for the current culture</returns>
    Public Shared Function GetLocalizedFriendlyNameHeader(columnName As String) As String
        Dim resxKey As String = "HeaderCell_" & Regex.Replace(columnName, "\s*", "")
        Dim localizedHeader As String = My.Resources.ResourceManager.GetString(resxKey)
        Return If(localizedHeader Is Nothing, clsUtility.GetUnCamelled(columnName), localizedHeader)
    End Function

    ''' <summary>
    ''' Populates the Columns To Search list box
    ''' using the names of the datagrid columns.
    ''' </summary>
    Private Sub PopulateColumnsToSearchListBox()

        'Sort the columns in display order. This shouldn't be
        'necessary but somehow the params column is getting to the
        'front of colums collection.
        Dim aColumns As New SortedList(Of Integer, DataGridViewColumn)
        Dim enumColumns As IEnumerator(Of KeyValuePair(Of Integer, DataGridViewColumn))

        For Each oDataGridViewColumn As DataGridViewColumn In gridLog.Columns
            If oDataGridViewColumn.Visible Then
                aColumns.Add(oDataGridViewColumn.DisplayIndex, oDataGridViewColumn)
            End If
        Next

        Dim lastSearchListBox As New List(Of String)
        Dim checkAll = (chklSearchColumns.Items.Count = 0)

        If checkAll = False Then
            Dim maxCount = chklSearchColumns.Items.Count - 1
            For item = 0 To maxCount
                If chklSearchColumns.GetItemChecked(item) Then
                    lastSearchListBox.Add(chklSearchColumns.Items(item).ToString())
                End If
            Next
        End If

        If lastSearchListBox.Count = 0 Then
            checkAll = True
        End If

        chklSearchColumns.Items.Clear()

        enumColumns = aColumns.GetEnumerator
        While enumColumns.MoveNext
            If checkAll OrElse lastSearchListBox.Contains(enumColumns.Current.Value.HeaderText) Then
                chklSearchColumns.Items.Add(enumColumns.Current.Value.HeaderText, True)
            Else
                chklSearchColumns.Items.Add(enumColumns.Current.Value.HeaderText, mViewType = ViewType.List)
            End If
        End While

        'Only enable the list in grid view.
        chklSearchColumns.Enabled = mViewType = ViewType.Grid

    End Sub

    ''' <summary>
    ''' Sets column size mode and fill weight.
    ''' </summary>
    ''' <param name="col">The column</param>
    Private Sub ApplyAutoSizeColumnMode(ByVal col As DataGridViewColumn)

        Select Case col.Name
            Case ColumnNames.Text, ColumnNames.Result
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                col.FillWeight = 5
            Case ColumnNames.StageName
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                col.FillWeight = 4
            Case ColumnNames.Object, ColumnNames.Action, ColumnNames.Page, ColumnNames.Process
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                col.FillWeight = 3
            Case ColumnNames.ResourceStart, ColumnNames.ResourceEnd, ColumnNames.StageType, ColumnNames.ResultType
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
                col.FillWeight = 2
            Case ColumnNames.Parameters
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader
                col.FillWeight = 1
            Case ColumnNames.BluePrismMemory, ColumnNames.TargetAppID, ColumnNames.TargetMemory
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                col.FillWeight = 2
            Case Else
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                col.FillWeight = 1
        End Select

    End Sub

    ''' <summary>
    ''' Formats the cells displayed on screen.
    ''' </summary>
    Protected Overridable Sub HandleCellFormatting(
     sender As Object, e As DataGridViewCellFormattingEventArgs) _
     Handles gridLog.CellFormatting

        If DesignMode Then Return
        If e.ColumnIndex = -1 OrElse e.RowIndex = -1 Then Return

        Dim cols = gridLog.Columns
        Dim col = cols(e.ColumnIndex)
        Dim row = gridLog.Rows(e.RowIndex)
        Dim cell = row.Cells(e.ColumnIndex)

        ' Colour the result type fields.
        If col.Name = ColumnNames.ResultType AndAlso cols.Contains(ColumnNames.System.ResultTypeValue) Then
            Dim dtype As DataType =
             BPUtil.IfNull(row.Cells(ColumnNames.System.ResultTypeValue).Value, DataType.unknown)

            If dtype <> DataType.unknown Then _
             e.CellStyle.ForeColor = DataItemColour.GetDataItemColor(dtype)
            e.Value = clsProcessDataTypes.GetFriendlyName(dtype)

            Return
        End If

        ' If we have a result, decode it into a process value and display it in its
        ' local format
        If col.Name = ColumnNames.Result AndAlso cols.Contains(ColumnNames.System.ResultTypeValue) _
         AndAlso Not IsDBNull(e.Value) Then
            Try
                Dim dtype As DataType =
                 BPUtil.IfNull(row.Cells(ColumnNames.System.ResultTypeValue).Value, DataType.unknown)
                Dim val As clsProcessValue
                If dtype = DataType.password Then
                    val = New SafeString(CStr(e.Value))
                Else
                    val = clsProcessValue.Decode(dtype, CStr(e.Value))
                End If

                Select Case dtype
                    Case DataType.date : e.Value = CDate(val).ToShortDateString()
                    Case DataType.time : e.Value = CDate(val).ToShortTimeString()
                    Case DataType.number : e.Value = CDec(val).ToString()
                    Case DataType.datetime
                        Dim dateValue = New DateTimeOffset(CDate(val))
                        SetDateTimeTooltipText(cell, dateValue)
                        e.Value = dateValue.DateTime
                    Case DataType.text
                        Dim errorString = CStr(e.Value)
                        If errorString.StartsWith("ERROR: ") Then
                            e.Value = errorString.Replace("ERROR: ", My.Resources.LogError)
                        End If
                End Select
            Catch
                'If any of this fails just display it as it appears in the database
            End Try
        End If

        'format start/end dates in local time if the offset is known.
        If col.Name = ColumnNames.ResourceStart AndAlso Not IsDBNull(e.Value) Then
            Dim startOffset = row.Cells(ColumnNames.System.StartOffset).Value
            FormatDateTimeOffset(e, cell, startOffset)
        End If

        If col.Name = ColumnNames.ResourceEnd AndAlso Not IsDBNull(e.Value) Then
            Dim endOffset = row.Cells(ColumnNames.System.EndOffset).Value
            FormatDateTimeOffset(e, cell, endOffset)
        End If

        Dim cellVal As String
        If col.Name = ColumnNames.Parameters Then
            ' Use the value in the hidden XML cell
            cellVal = BPUtil.IfNull(row.Cells(ColumnNames.System.ParameterXml).Value, "")

            ' Make a link for any parameters.
            If cellVal <> "" Then e.Value = My.Resources.View : SetParameterToolTip(cell, e)

        Else
            ' We want the string representation of the cell value, regardless of
            ' what type it is (we can't rely on the intrinsic value being a string)
            ' A null value is represented as an empty string
            cellVal = BPUtil.IfNull(Of Object)(e.Value, "").ToString()

        End If

        ' Localize the StageName name
        If col.Name = ColumnNames.StageName Then
            e.Value = LTools.GetC(cellVal, "misc", "stage")
        End If

        ' Localize the Page name
        If col.Name = ColumnNames.Page Then
            e.Value = LTools.GetC(cellVal, "misc", "page")
        End If

        ' Localize the StageType name
        If col.Name = ColumnNames.StageType Then
            e.Value = clsStageTypeName.GetLocalizedFriendlyName(cellVal)
        End If

        ' Localize the Action name for IBOs
        If col.Name = ColumnNames.Action AndAlso cols.Contains(ColumnNames.Object) Then
            Dim iboClass As String =
                 BPUtil.IfNull(row.Cells(ColumnNames.Object).Value, "").ToString()
            If iboClass <> "" AndAlso cellVal <> "" Then
                e.Value = clsBusinessObjectAction.GetLocalizedFriendlyName(cellVal, iboClass, "Action")
            End If
        End If

        ' Colour the 'find all' cells.
        If mColumnsToSearch.Contains(col.Name) AndAlso cellVal <> "" Then
            For Each srchItem As SearchItem In mFindAllSearches.Values
                If srchItem.SearchPattern.IsMatch(cellVal) Then
                    e.CellStyle.BackColor = srchItem.HighlightColour
                End If
            Next
        End If

        ' Colour the 'find next' cell. We do this after the
        ' "find all" searches because this one should appear on top
        If mFindPosn.Equals(New Point(e.ColumnIndex, e.RowIndex)) Then
            e.CellStyle.BackColor = btnColour.CurrentColor
        End If

    End Sub

    ''' <summary>
    ''' Formats the date time offset and sets the appropriate tooltip on the cell.
    ''' </summary>
    ''' <param name="cell">The cell on which to set the tooltip</param>
    ''' <param name="e">The DataGridView cell formatting arguments</param>
    ''' <param name="offset">The timezoneoffset to use if available</param>
    Private Sub FormatDateTimeOffset(e As DataGridViewCellFormattingEventArgs, cell As DataGridViewCell, offset As Object)
        Dim dateValue = CDate(e.Value)

        If IsDBNull(offset) Then
            e.Value = dateValue
        Else
            Dim dateTimeOffset As New DateTimeOffset(dateValue, TimeSpan.FromSeconds(CInt(offset)))
            SetDateTimeTooltipText(cell, dateTimeOffset)
            e.Value = dateTimeOffset.DateTime
        End If

        e.CellStyle.Format = "G"
    End Sub

    ''' <summary>
    ''' Sets the tooltip text in a standard format
    ''' </summary>
    ''' <param name="cell">The cell on which to set the tooltip</param>
    ''' <param name="dateTimeOffset">The datetime/offset to use</param>
    Friend Shared Sub SetDateTimeTooltipText(cell As DataGridViewCell, dateTimeOffset As DateTimeOffset)
        cell.ToolTipText = String.Format(My.Resources.UserLocal02UTCTime1, dateTimeOffset.ToLocalTime, dateTimeOffset.ToUniversalTime, vbCrLf)
    End Sub

    ''' <summary>
    ''' Builds a tooltip from the parameter XML.
    ''' </summary>
    Private Sub SetParameterToolTip(cell As DataGridViewCell, e As DataGridViewCellFormattingEventArgs)
        Try
            Dim row = gridLog.Rows(e.RowIndex)
            Dim seqnum = CInt(row.Cells("LogNumber").Value)
            Dim toolTip = String.Empty
            If mTooltipCache.ContainsKey(seqnum) Then
                toolTip = mTooltipCache(seqnum)
            Else

                Dim objectValue As String = row.Cells(ColumnNames.Object).Value.ToString
                Dim parameterValue = row.Cells(ColumnNames.System.ParameterXml).Value
                If IsDBNull(parameterValue) Then Return
                Dim parameterXML = CStr(parameterValue)

                Using xr As New XmlTextReader(New IO.StringReader(parameterXML))

                    Const maxLines = 6
                    Const maxLength = 40
                    Dim iLines As Integer = 0

                    While xr.Read
                        'We're only interested in input/output elements
                        If xr.NodeType <> XmlNodeType.Element OrElse
                         Not (xr.Name = "input" OrElse xr.Name = "output") Then
                            Continue While
                        End If


                        Dim line = If(xr.Name = "input", My.Resources.INcolon, My.Resources.OUTcolon)

                        Dim dataName = clsBusinessObjectAction.GetLocalizedFriendlyName(xr("name"), objectValue, "Params")
                        dataName = dataName.Truncate(maxLength, My.Resources.Ellipsis)

                        Dim dataType = xr("type")
                        line &= " " & dataName & My.Resources.SpaceLeftParen & dataType & My.Resources.RightParen

                        If dataType <> "collection" Then
                            Dim dataValue = xr("value")
                            dataValue = dataValue.Truncate(maxLength, My.Resources.Ellipsis)
                            line &= My.Resources.SpaceEqualsSpace & dataValue
                        End If

                        iLines += 1
                        If iLines < maxLines Then
                            toolTip &= line & vbCrLf
                        Else
                            toolTip &= My.Resources.More
                            Exit While
                        End If
                    End While
                    toolTip = toolTip.Trim

                End Using

                mTooltipCache.Add(seqnum, toolTip)
            End If

            cell.ToolTipText = toolTip
        Catch ex As Exception
            cell.ToolTipText = My.Resources.ClickToDisplayAListOfParameters
        End Try

    End Sub

    ''' <summary>
    ''' Opens the parameter viewing form.
    ''' </summary>
    Private Sub ParameterButton_CellContentClick(
     ByVal sender As Object, ByVal e As DataGridViewCellEventArgs) Handles gridLog.CellContentClick
        If DesignMode Then Exit Sub
        If e.ColumnIndex < 0 OrElse e.RowIndex < 0 Then Exit Sub

        If gridLog.Columns(e.ColumnIndex).Name <> "Parameters" Then Return

        Dim paramXml As String =
         BPUtil.IfNull(gridLog("ParameterXml", e.RowIndex).Value, "")
        If paramXml = "" Then Return
        Dim objectName As String =
         BPUtil.IfNull(gridLog("Object", e.RowIndex).Value, "")

        If mLogParameterViewer Is Nothing Then
            mLogParameterViewer = New frmLogParameterViewer()
            mLogParameterViewer.Owner = Me
            If mLogParameterViewerSize <> Size.Empty Then _
             mLogParameterViewer.Size = mLogParameterViewerSize
            Dim p As Point = Location
            p.X += (Width - mLogParameterViewer.Width) \ 2
            p.Y += (Height - mLogParameterViewer.Height) \ 2
            mLogParameterViewer.Location = p
            mLogParameterViewer.Show()
        Else
            mLogParameterViewer.BringToFront()
        End If
        mLogParameterViewer.SetData(paramXml, mFindAllSearches, btnColour.CurrentColor, objectName)

    End Sub

    ''' <summary>
    ''' Displays the context menu
    ''' </summary>
    Friend Sub DataGridView_Click(ByVal sender As Object, ByVal e As MouseEventArgs)

        If e.Button = MouseButtons.Right Then
            mClickLocn = e.Location
            If mViewType = ViewType.List Then
                mContextMenu.MenuItems(0).Visible = True
                mContextMenu.MenuItems(1).Visible = True
                mContextMenu.MenuItems(2).Visible = True
                mContextMenu.MenuItems(3).Visible = True
                mContextMenu.MenuItems(4).Visible = True
                mContextMenu.MenuItems(5).Visible = False
                mContextMenu.MenuItems(6).Visible = False
                mContextMenu.MenuItems(7).Visible = False
                mContextMenu.MenuItems(8).Visible = False
            Else
                mContextMenu.MenuItems(0).Visible = True
                mContextMenu.MenuItems(1).Visible = True
                mContextMenu.MenuItems(2).Visible = True
                mContextMenu.MenuItems(3).Visible = True
                mContextMenu.MenuItems(4).Visible = True
                mContextMenu.MenuItems(5).Visible = True
                mContextMenu.MenuItems(6).Visible = True
                mContextMenu.MenuItems(7).Visible = True
                mContextMenu.MenuItems(8).Visible = True
            End If
            mContextMenu.Show(gridLog, e.Location)
        End If

    End Sub

#End Region

#Region " Background Worker "

    ''' <summary>
    ''' Handles the async search of the cached pages and the DB.
    ''' </summary>
    Private Sub BackgroundWorker_DoWork(ByVal sender As Object, ByVal e As DoWorkEventArgs) _
      Handles mFindWorker.DoWork

        If mFindWorker.CancellationPending Then e.Cancel = True : Return

        Dim matchPgNo As Integer
        Dim count As Integer = 0

        ' Search from the next page and loop round to the current page again
        For iter As Integer = mPage To TotalPages + mPage - 1
            Dim index As Integer = iter Mod TotalPages
            count += 1

            If mPages(index) Is Nothing Then
                'Get the row number for this page.
                Dim currRow As Integer = (mRowsPerPage * index) + 1

                'This page isn't cached, so query the DB. 
                Dim tab As DataTable
                If mViewType = ViewType.List Then
                    tab = gSv.GetLogsAsText(mSessionNo, currRow, mRowsPerPage)
                    FormatLogsForTextMode(tab)
                Else
                    tab = gSv.GetLogs(mSessionNo, currRow, mRowsPerPage)
                End If
                mPages(index) = tab
            End If

            ' NB Populating a datatable with a subset of log rows and then
            ' searching through them may seem inefficient but it has been
            ' necessary to enable 'whole word' searching, something not 
            ' easily acheived in an SQL query. Fetching data a page at a time 
            ' also enables the search to be cancelled.
            If DataTableContainsNextMatch(mPages(index)) Then
                matchPgNo = index + 1
                Exit For
            End If

            If mFindWorker.CancellationPending Then
                e.Cancel = True
                Exit For
            Else
                mFindWorker.ReportProgress(100 * (count \ TotalPages))
            End If

        Next

        e.Result = matchPgNo

    End Sub

    ''' <summary>
    ''' Responds to calls to BackgroundWorker.ReportProgress by updating the progress bar.
    ''' </summary>
    Private Sub BackgroundWorker_ProgressChanged(ByVal sender As Object, ByVal e As ProgressChangedEventArgs) _
     Handles mFindWorker.ProgressChanged

        progressFind.Value = e.ProgressPercentage

    End Sub

    ''' <summary>
    ''' Handles the end of the background worker activity, a cancellation by the user or an error. 
    ''' </summary>
    Private Sub HandleFindWorkerCompleted(
     ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs) _
     Handles mFindWorker.RunWorkerCompleted

        If e.Error IsNot Nothing Then
            UserMessage.Err(e.Error,
             My.Resources.TheFollowingErrorHasOccurred001, vbCrLf, e.Error.Message)

        ElseIf e.Cancelled Then
            ' Check if this cancellation has been triggered by an attempt to close
            ' the form. If it has, the original close action would have been halted,
            ' so do the close now.
            If mCloseAfterCancel Then Close() : Return

        Else
            progressFind.Value = progressFind.Maximum

            Dim matchPage As Integer = CInt(e.Result)

            If matchPage = 0 Then
                UserMessage.OK(My.Resources.TheTerm0CouldNotBeFound,
                 mFindNextSearch.Text)

                ' Reset to the 'beginning' of the log so the user can search again
                mFindPosn = Nowhere
                gridLog.Invalidate()
            Else
                ' Change page.
                txtCurrentPage.Text = CStr(matchPage)
                ChangePage()
                ' Get the first match location on the page and scroll it into view.
                mFindPosn = GetDataGridMatchLocation(Nowhere)
                ScrollToCell(mFindPosn)
                gridLog.Invalidate()
            End If

        End If

        EnableControls(True)
        UpdateSearchUI(False)
        cmbFind.Focus()
        btnCancel.Enabled = False
        progressFind.Value = progressFind.Minimum
        progressFind.Visible = False

    End Sub

    ''' <summary>
    ''' Cancels the async search.
    ''' </summary>
    Private Sub CancelBackgroundWorker()
        If RunWorkerNeedsCancelling() Then mFindWorker.CancelAsync()
    End Sub

    ''' <summary>
    ''' Indicates whether the async search needs cancelling.
    ''' </summary>
    ''' <returns>True if the finder background worker is running and not currently
    ''' awaiting cancellation.</returns>
    Private Function RunWorkerNeedsCancelling() As Boolean
        Return (mFindWorker IsNot Nothing _
         AndAlso mFindWorker.IsBusy AndAlso Not mFindWorker.CancellationPending)
    End Function

#End Region

#Region " Find Control Handlers "

    ''' <summary>
    ''' Indicates whether a cached data table contains the next match.
    ''' </summary>
    ''' <param name="tab">The data table</param>
    ''' <returns>True if the next match exists</returns>
    Private Function DataTableContainsNextMatch(ByVal tab As DataTable) As Boolean

        If tab Is Nothing Then Return False

        Dim cols As ICollection(Of String) = DataColumnsToSearch
        If cols.Count = 0 Then Return False

        For r As Integer = 0 To tab.Rows.Count - 1
            For Each col As String In cols
                If mFindNextSearch.Matches(tab.Rows(r)(col)) Then Return True
            Next
        Next
        Return False

    End Function

    ''' <summary>
    ''' Get the reference to the next matching cell in the data grid.
    ''' </summary>
    ''' <param name="pStart">The reference to start search from</param>
    ''' <returns>The column and row of the match</returns>
    Protected Function GetDataGridMatchLocation(ByVal pStart As Point) As Point

        Dim oCellValue As Object = Nothing
        Dim iColumnIndex As Integer
        Dim iCell As Integer
        Dim iRow As Integer
        Dim oCell As DataGridViewCell
        Dim oRow As DataGridViewRow

        'Start at the next cell
        iCell = Math.Max(0, pStart.X + 1)
        iRow = Math.Max(0, pStart.Y)

        Dim colParameters As DataGridViewColumn = gridLog.Columns("Parameters")
        If colParameters IsNot Nothing AndAlso colParameters.Index = pStart.X Then
            iRow += 1
        End If

        If iCell >= gridLog.ColumnCount Then
            'Start at the first cell of the next row
            iCell = 0
            iRow += 1
        End If

        If mColumnsToSearch.Count = 0 Then
            Return Nowhere
        End If

        'Search the current page of the data grid
        For iRow = iRow To gridLog.Rows.Count - 1
            oRow = gridLog.Rows(iRow)

            For iCell = iCell To oRow.Cells.Count - 1
                oCell = oRow.Cells(iCell)

                If oCell Is Nothing Then
                    Return Nowhere
                End If

                If Not mColumnsToSearch.Contains(gridLog.Columns(oCell.ColumnIndex).Name) Then
                    'Don't search this column and move to the next one.
                    Continue For
                End If

                'Ignore invisible cells apart from the XML cell.
                If gridLog.Columns(oCell.ColumnIndex).Visible Then
                    iColumnIndex = oCell.ColumnIndex
                    'For parameters, search the hidden XML cell instead.
                    If colParameters IsNot Nothing AndAlso oCell.ColumnIndex = colParameters.Index Then
                        oCell = gridLog.Rows(oCell.RowIndex).Cells("ParameterXML")
                    End If
                Else
                    'Don't search this column and move to the next one.
                    Continue For
                End If

                If oCell Is Nothing _
                 OrElse oCell.Value Is Nothing _
                 OrElse TypeOf oCell.Value Is DBNull _
                 OrElse mFindNextSearch.SearchPattern.IsMatch(oCell.Value.ToString) = False Then
                    'The cell is invisible, empty or doesn't match, so don't highlight.
                Else
                    'This is a matching cell.
                    Return New Point(iColumnIndex, oCell.RowIndex)
                End If
            Next
            iCell = 0
        Next
        Return Nowhere

    End Function

    ''' <summary>
    ''' Scrolls the data grid so that the given cell is visible.
    ''' </summary>
    ''' <param name="pCell">The cell</param>
    Protected Sub ScrollToCell(ByVal pCell As Point)

        ScrollToColumn(pCell.X)
        ScrollToRow(pCell.Y)

    End Sub

    ''' <summary>
    ''' Scrolls to the given row.
    ''' </summary>
    ''' <param name="iRow">The row index</param>
    Protected Sub ScrollToRow(ByVal iRow As Integer)

        If iRow >= 0 AndAlso iRow < gridLog.Rows.Count _
         AndAlso Not gridLog.Rows(iRow).Displayed Then
            gridLog.FirstDisplayedScrollingRowIndex = iRow
        End If

    End Sub

    ''' <summary>
    ''' Scrolls to the given column.
    ''' </summary>
    ''' <param name="iColumn"></param>
    Protected Sub ScrollToColumn(ByVal iColumn As Integer)

        If iColumn >= 0 AndAlso iColumn < gridLog.Columns.Count _
         AndAlso Not gridLog.Columns(iColumn).Displayed Then
            'Select the first visible column available.
            For c As Integer = iColumn To gridLog.Columns.Count - 1
                If gridLog.Columns(c).Visible Then
                    gridLog.FirstDisplayedScrollingColumnIndex = c
                    Exit For
                End If
            Next
        End If

    End Sub

    ''' <summary>
    ''' Selects the left hand cell on the given row.
    ''' </summary>
    ''' <param name="rowIndex">The (zero-based) row number on which the cell should
    ''' be selected.</param>
    Private Sub SelectFirstCell(ByVal rowIndex As Integer)

        ' Really shouldn't happen; just ignore it if it does.
        If rowIndex >= gridLog.RowCount Then Return

        'Undo any existing selection.
        For Each c As DataGridViewCell In gridLog.SelectedCells
            c.Selected = False
        Next

        ' Get the first displayed column
        Dim col As DataGridViewColumn =
         gridLog.Columns.GetFirstColumn(DataGridViewElementStates.Displayed)
        ' If there is no displayed column (er, what?) then just quietly retire.
        If col Is Nothing Then Return

        Dim cell As DataGridViewCell = gridLog(col.Index, rowIndex)
        cell.Selected = True
        gridLog.CurrentCell = cell

    End Sub

    ''' <summary>
    ''' Searches for the next matching cell.
    ''' </summary>
    ''' <param name="findAll">True to highlight all cells matching the search
    ''' constraints. False to find the next match.</param>
    Private Sub Find(ByVal findAll As Boolean)

        'Ignore blank searches
        If cmbFind.Text = "" Then Return

        'Add the search text to the combo.
        If Not cmbFind.Items.Contains(cmbFind.Text) Then _
         cmbFind.Items.Insert(0, cmbFind.Text)

        'Find All or Find Next.
        If findAll Then
            If Not mFindAllSearches.ContainsKey(cmbFind.Text) Then
                mFindAllSearches.Add(cmbFind.Text, GetSearchTerm(cmbFind.Text))
            End If
        Else
            If mFindAllSearches.ContainsKey(cmbFind.Text) Then
                mFindAllSearches.Remove(cmbFind.Text)
            End If
        End If

        Dim item As SearchItem = GetSearchTerm(cmbFind.Text)
        If mFindNextSearch Is Nothing OrElse mFindNextSearch.Text <> item.Text Then
            'This is a new search, so reset and start from the top.
            mFindPosn = Nowhere
        End If
        mFindNextSearch = item

        Dim matchCell As Point
        If findAll Then
            'Look for any match on the current page.
            matchCell = GetDataGridMatchLocation(Nowhere)
        Else
            'Look on the current page for a match beyond the current 'found' position.
            matchCell = GetDataGridMatchLocation(mFindPosn)
        End If

        HandleFound(matchCell)

    End Sub

    ''' <summary>
    ''' Handles a Find() operation being completed. If <paramref name="matchCell"/>
    ''' is equal to <see cref="Nowhere"/>, that indicates that there was no match
    ''' for the search.
    ''' </summary>
    ''' <param name="matchCell">The cell at which a match was found. If this is equal
    ''' to <see cref="Nowhere"/>, no match was found in the log.</param>
    Protected Overridable Sub HandleFound(ByVal matchCell As Point)
        If matchCell = Nowhere Then
            ' There is no match on the current page, so look in the cached pages or
            ' the DB.
            If Not mFindWorker.IsBusy Then
                EnableControls(False)
                UpdateSearchUI(True)
                progressFind.Visible = True
                mFindWorker.RunWorkerAsync()
            Else
                UserMessage.Show(
                 My.Resources.ASearchIsCurrentlyUnderwayPleaseCancelIfYouWishToConductADifferentSearch)
            End If
        Else
            'There is a match on the current page, so highlight it.
            ScrollToCell(matchCell)
            mFindPosn = matchCell
            gridLog.Invalidate()
            UpdateSearchUI(False)
        End If
    End Sub

    ''' <summary>
    ''' Updates the UI according to whether or not a search is currenly in progress.
    ''' </summary>
    ''' <param name="searchInProgress">When true the UI will be updated to prevent
    ''' further searches until the current search is complete. Otherwise new searches
    ''' will be permitted.</param>
    Private Sub UpdateSearchUI(ByVal searchInProgress As Boolean)
        panSearchConstraints.Enabled = Not searchInProgress
        btnClear.Enabled = Not searchInProgress
        btnCancel.Enabled = searchInProgress
    End Sub

    ''' <summary>
    ''' Handles the 'Find Next' or 'Find All' buttons being pressed.
    ''' </summary>
    Private Sub HandleFindClick(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnFindNext.Click, btnFindAll.Click
        Try
            Find(sender Is btnFindAll)
        Catch ex As Exception
            UserMessage.Err(ex, My.Resources.UnexpectedErrorWhileSearching0, ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Executes a 'Find Next' in response to ENTER or F3
    ''' </summary>
    Private Sub cmbFind_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles cmbFind.KeyDown
        If e.KeyCode = Keys.Enter OrElse e.KeyCode = Keys.F3 _
         Then Find(False) _
         Else mFindNextSearch = Nothing
    End Sub

    ''' <summary>
    ''' Resets all searches and clears any highlighting.
    ''' </summary>
    Private Sub btnClear_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnClear.Click

        mFindPosn = Nowhere

        mFindNextSearch = Nothing
        cmbFind.Text = Nothing
        mFindAllSearches.Clear()
        gridLog.Invalidate()

    End Sub

    ''' <summary>
    ''' Cancels the search.
    ''' </summary>
    Private Sub btnCancel_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnCancel.Click
        CancelBackgroundWorker()
    End Sub

    ''' <summary>
    ''' Resets the current search object
    ''' </summary>
    Private Sub HandleConstraintsCheckedChanged(
     ByVal sender As Object, ByVal e As EventArgs) Handles _
      chkCase.CheckedChanged, chkWholeWord.CheckedChanged, chkWildcards.CheckedChanged
        mFindNextSearch = Nothing
    End Sub

    ''' <summary>
    ''' Gets a regex to use to find matching data.
    ''' </summary>
    Protected Function GetSearchTerm(ByVal txt As String) As SearchItem

        If txt = "" Then Return Nothing

        Dim options As RegexOptions = RegexOptions.Multiline
        Dim expr As String = ""

        If Not chkCase.Checked Then options = options Or RegexOptions.IgnoreCase

        ' Store the terms (with regex control characters duly escaped) that we'll be
        ' searching for into a list.
        Dim escapedTerms As New List(Of String)
        escapedTerms.Add(Regex.Escape(txt))

        ' Check to see if the user is searching for a date or a number. Ensure that
        ' all appropriate cultural variations are catered for in the search
        Dim dt As Date
        Dim dec As Decimal
        If Date.TryParse(txt, Nothing, DateTimeStyles.NoCurrentDateDefault, dt) Then
            ' We have a potential date - go through the possible encodes
            ' and add them to the search
            For Each enc As String In clsProcessValue.GetDateEncodes(dt)
                ' If we're already searching for it, there's no need
                ' to add it again. Let's not make work for ourselves
                If enc = txt Then Continue For
                ' Otherwise, add this variation to search for too
                escapedTerms.Add(Regex.Escape(enc))
            Next
        ElseIf Decimal.TryParse(txt, dec) Then
            Dim decStr As String = clsProcessValue.GetNumberEncode(dec)
            If decStr <> txt Then escapedTerms.Add(Regex.Escape(decStr))
        End If

        ' If we have one term to look for, just set that as the expression
        ' If we have multiple terms, group them and pipe separate them so that
        ' we search correctly
        If escapedTerms.Count = 1 _
         Then expr = escapedTerms(0) _
         Else expr = "(" & CollectionUtil.Join(escapedTerms, "|") & ")"

        If chkWildcards.Checked Then _
         expr = expr.Replace("\*", ".*").Replace("\?", ".").Replace("\#", "\d")

        If chkWholeWord.Checked Then expr = "\b" & expr & "\b"

        Dim srch As New SearchItem()
        srch.SearchPattern = New Regex(expr, options)
        srch.HighlightColour = btnColour.CurrentColor
        srch.Text = txt
        srch.MatchCase = chkCase.Checked
        srch.MatchWholeWord = chkWholeWord.Checked
        Return srch

    End Function

#End Region

#Region " Form Handlers "

    ''' <summary>
    ''' Actions to perform when the form is loaded
    ''' </summary>
    Protected Overrides Sub OnLoad(ByVal e As EventArgs)

        MyBase.OnLoad(e)
        If DesignMode Then Exit Sub

        ' Switch off auto-size mode, allowing the user to resize beyond the size
        ' of the window
        For Each c As DataGridViewColumn In gridLog.Columns
            c.AutoSizeMode = DataGridViewAutoSizeColumnMode.None
        Next

        'Scroll to a row if necessary.
        If mInitialIndex >= 0 AndAlso mInitialIndex < gridLog.RowCount Then
            gridLog.FirstDisplayedScrollingRowIndex = mInitialIndex
            SelectFirstCell(mInitialIndex)
        End If

        'Do an initial search if necessary.
        If cmbFind.Text <> "" Then
            mFindPosn = GetDataGridMatchLocation(Nowhere)
            ' If the search succeeded (ie. term was found and matching column is
            ' visible), scroll to that cell
            If mFindPosn <> Nowhere Then
                ScrollToCell(mFindPosn)
                gridLog.Refresh()
            End If
        End If
        cmbViewType.BindToLocalisedEnumItems(Of ViewType)(CustomResources, "ViewType_{0}")
        cmbViewType.SelectedValue = ViewType.Grid
    End Sub

    ''' <summary>
    ''' Cleans up anything which requires cleaning before the form can close, and
    ''' returns whether it is ready to close or not.
    ''' The only time this may try and inhibit the closing is if there is a search
    ''' background worker in progress - it will flag the worker to cancel its job,
    ''' at which point the form will be fully closed.
    ''' </summary>
    ''' <returns>True to allow closing of the form; False to inhibit closing
    ''' </returns>
    Protected Overridable Function CanClose() As Boolean

        If RunWorkerNeedsCancelling() Then
            'Set the flag to close after the background worker has finished.
            mCloseAfterCancel = True
            CancelBackgroundWorker()
            'Cancel the closure.
            Return False
        End If

        'Close the parameter viewer if necessary
        If mLogParameterViewer IsNot Nothing Then
            mLogParameterViewer.Close()
        End If

        If mViewType = ViewType.Grid Then

            'The log viewer has been in grid view
            mHiddenColumns = New List(Of String)
            Dim dtLogs As DataTable = CType(gridLog.DataSource, DataTable)
            Dim iHiddenColumns As Integer
            For Each c As DataGridViewColumn In gridLog.Columns
                If Not c.Visible Then
                    If dtLogs.Columns.Contains(c.Name) Then
                        mHiddenColumns.Add(c.Name)
                        iHiddenColumns += CInt(2 ^ dtLogs.Columns(c.Name).Ordinal)
                    ElseIf c.Name = "Parameters" Then
                        mHiddenColumns.Add(c.Name)
                        iHiddenColumns += CInt(2 ^ dtLogs.Columns.Count)
                    End If
                End If
            Next
            'Save the user's choice of visible columns.
            gSv.SetLogViewerHiddenColumns(iHiddenColumns)

        End If

        Return True

    End Function

    ''' <summary>
    ''' Closes the parameter viewer if necessary and writes the hidden column
    ''' selection to the DB.
    ''' </summary>
    Protected Sub Me_Closing(
     ByVal sender As Object, ByVal e As FormClosingEventArgs) Handles Me.FormClosing
        If CanClose() Then
            DisposeOfPages()
        Else
            e.Cancel = True
        End If
    End Sub

    ''' <summary>
    ''' Keeps the last known size of the parameter viewer for use next time.
    ''' </summary>
    Private Sub LogParameterViewer_Closing(ByVal sender As Object, ByVal e As FormClosingEventArgs) _
     Handles mLogParameterViewer.FormClosing
        mLogParameterViewerSize = mLogParameterViewer.Size
    End Sub

    ''' <summary>
    ''' Handles closure of the log parameter viewer form.
    ''' </summary>
    Private Sub LogParameterViewer_Closed(ByVal sender As Object, ByVal e As FormClosedEventArgs) _
     Handles mLogParameterViewer.FormClosed
        mLogParameterViewer.Dispose()
        mLogParameterViewer = Nothing
    End Sub


    ''' <summary>
    ''' Expands and collapses the left hand search panel.
    ''' </summary>
    Private Sub HandleToggleClick(
     ByVal sender As Object, ByVal e As LinkLabelLinkClickedEventArgs) _
     Handles lnkToggleSearch.LinkClicked
        lnkToggleSearch.Image.RotateFlip(RotateFlipType.RotateNoneFlipX)
        splitPane.Panel1Collapsed = Not splitPane.Panel1Collapsed
    End Sub

    ''' <summary>
    ''' Maintains a list of column names based on the checked items in the list.
    ''' </summary>
    Private Sub chklSearchColumns_ItemCheck(ByVal sender As Object, ByVal e As ItemCheckEventArgs) _
     Handles chklSearchColumns.ItemCheck

        Dim sHeaderText As String = chklSearchColumns.Items(e.Index).ToString
        Dim sColumnName As String = ""

        For Each oColumn As DataGridViewColumn In gridLog.Columns
            If oColumn.HeaderText = sHeaderText Then
                sColumnName = oColumn.Name
                Exit For
            End If
        Next

        'Collect all checked column names
        mColumnsToSearch = New List(Of String)
        For Each oItem As Object In chklSearchColumns.CheckedItems
            For Each oColumn As DataGridViewColumn In gridLog.Columns
                If oColumn.HeaderText = oItem.ToString Then
                    mColumnsToSearch.Add(oColumn.Name)
                    Exit For
                End If
            Next
        Next

        If e.NewValue = CheckState.Checked Then
            mColumnsToSearch.Add(sColumnName)
        Else
            mColumnsToSearch.Remove(sColumnName)
        End If

    End Sub

#End Region

#Region " Menu Handlers "

    ''' <summary>
    ''' Copies selected cells data
    ''' </summary>
    Private Sub ContextCopy_Click(ByVal sender As Object, ByVal e As EventArgs)
        CopyToClipBoard()
    End Sub

    ''' <summary>
    ''' Copies all cell data
    ''' </summary>
    Private Sub ContextCopyAll_Click(ByVal sender As Object, ByVal e As EventArgs)
        gridLog.SelectAll()
        CopyToClipBoard()
    End Sub

    ''' <summary>
    ''' Copies grid data to clipboard
    ''' </summary>
    Private Sub CopyToClipBoard()
        If gridLog.GetCellCount(DataGridViewElementStates.Selected) = 0 Then
            gridLog.SelectAll()
        End If
        Try
            Dim oDataObject As DataObject = gridLog.GetClipboardContent()
            If oDataObject IsNot Nothing Then
                Clipboard.SetDataObject(oDataObject)
            End If
        Catch ex As Runtime.InteropServices.ExternalException
        End Try
    End Sub

    ''' <summary>
    ''' Hides the column where the mouse was clicked
    ''' </summary>
    Private Sub ContextHide_Click(ByVal sender As Object, ByVal e As EventArgs)

        For i As Integer = 0 To gridLog.Columns.Count - 1
            If gridLog.Columns(i).Visible Then
                If gridLog.GetColumnDisplayRectangle(i, False).Contains(mClickLocn) Then
                    'Update menu items
                    For Each oMenuItem As MenuItem In mColumnsContextMenu.MenuItems
                        If gridLog.Columns(i).HeaderText = oMenuItem.Text Then
                            oMenuItem.PerformClick()
                            Exit Sub
                        End If
                    Next
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' Unhides all columns
    ''' </summary>
    Private Sub ContextShowAll_Click(ByVal sender As Object, ByVal e As EventArgs)
        mnuShowAll.PerformClick()
    End Sub

    ''' <summary>
    ''' Changes to grid view
    ''' </summary>
    Private Sub ContextGrid_Click(ByVal sender As Object, ByVal e As EventArgs)
        mnuGridMode.PerformClick()
    End Sub

    ''' <summary>
    ''' Changes to list view
    ''' </summary>
    Private Sub ContextList_Click(ByVal sender As Object, ByVal e As EventArgs)
        mnuListMode.PerformClick()
    End Sub

    ''' <summary>
    ''' Closes the log viewer.
    ''' </summary>
    Private Sub mnuClose_Click(ByVal sender As Object, ByVal e As EventArgs) Handles mnuClose.Click
        Close()
    End Sub

    ''' <summary>
    ''' Exports all the log data.
    ''' </summary>
    Private Sub mnuExportLog_Click(ByVal sender As Object, ByVal e As EventArgs) Handles mnuExportLog.Click
        DoCompleteExport()
    End Sub

    ''' <summary>
    ''' Exports all the log data.
    ''' </summary>
    Protected Overridable Sub DoCompleteExport()

        Dim dtCopies As DataTable()

        ReDim dtCopies(mPages.Length - 1)
        For i As Integer = 0 To mPages.Length - 1
            dtCopies(i) = GetCopyWithHiddenColumnsRemoved(mPages(i))
        Next

        Dim f As New frmSessionLogExport()
        f.SetEnvironmentColoursFromAncestor(Me)
        Dim textMode = mViewType = ViewType.List
        f.Populate(dtCopies, textMode, mRowsPerPage, mSessionNo, String.Format(My.Resources.CompleteSessionLogFor0, mProcessName))
        f.DefaultFileName = My.Resources.BPACompleteSessionLog
        f.Show()

    End Sub

    ''' <summary>
    ''' Exports the current page of data.
    ''' </summary>
    Private Sub mnuExportPage_Click(ByVal sender As Object, ByVal e As EventArgs) Handles mnuExportPage.Click

        Dim dtThisPage As DataTable = GetCopyWithHiddenColumnsRemoved(mPages(mPage - 1))
        Dim f As New frmSessionLogExport()
        f.SetEnvironmentColoursFromAncestor(Me)
        Dim textMode As Boolean = mViewType = ViewType.List
        f.Populate(New DataTable() {dtThisPage}, textMode, mRowsPerPage, mSessionNo, String.Format(My.Resources.PartialSessionLogFor0, mProcessName))
        f.DefaultFileName = My.Resources.BPAPartialSessionLog
        f.Show()

    End Sub

    ''' <summary>
    ''' Gets a copy of the data table of the current page.
    ''' </summary>
    Private Function GetCopyWithHiddenColumnsRemoved(ByVal tab As DataTable) As DataTable
        If tab Is Nothing Then Return Nothing

        Dim copy As DataTable = tab.Copy()

        For Each col As DataGridViewColumn In gridLog.Columns
            If Not copy.Columns.Contains(col.Name) Then Continue For
            Select Case col.Name
                Case ColumnNames.System.ParameterXml
                    If Not gridLog.Columns(ColumnNames.Parameters).Visible Then
                        copy.Columns.Remove(col.Name)
                    End If
                Case ColumnNames.System.StageTypeValue, ColumnNames.System.ResultTypeValue
                    copy.Columns.Remove(col.Name)
                Case Else
                    If Not gridLog.Columns(col.Name).Visible Then
                        copy.Columns.Remove(col.Name)
                    End If
            End Select
        Next
        Return copy

    End Function

    ''' <summary>
    ''' Clears any cached data and refreshes the current page from the DB
    ''' </summary>
    Private Sub mnuRefresh_Click(ByVal sender As Object, ByVal e As EventArgs) Handles mnuRefresh.Click
        DoRefresh()
    End Sub

    ''' <summary>
    ''' Clears any cached data and refreshes the current page from the DB
    ''' </summary>
    Protected Overridable Sub DoRefresh()

        mTotalRows = gSv.GetLogsCount(mSessionNo)

        If mTotalRows > 0 Then
            DisposeOfPages()

            ReDim mPages(TotalPages - 1)
            PopulateDataGridView()
        End If

    End Sub

    ''' <summary>
    ''' Hides or shows all columns
    ''' </summary>
    Private Sub mnuShowAll_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles mnuShowAll.Click

        Dim item As MenuItem = CType(sender, MenuItem)

        'Show all columns
        For Each col As DataGridViewColumn In gridLog.Columns
            If Not SystemColumns.Contains(col.Name) Then col.Visible = True
        Next

        'Check all menu items
        For Each item In mnuColumns.MenuItems : item.Checked = True : Next
        mnuShowAll.Enabled = False

        'Check all context menu items
        For Each item In mColumnsContextMenu.MenuItems : item.Checked = True : Next
        mShowAllContextMenu.Enabled = False

        PopulateColumnsToSearchListBox()

    End Sub

    ''' <summary>
    ''' Hides or shows the selected column.
    ''' </summary>
    Private Sub mnuColumn_Click(ByVal sender As Object, ByVal e As EventArgs)
        Dim item As MenuItem = CType(sender, MenuItem)
        Dim col As DataGridViewColumn = gridLog.Columns(CStr(item.Tag))
        Dim visible As New List(Of DataGridViewColumn)

        'Check if the last visible column is about to be hidden.
        For Each c As DataGridViewColumn In gridLog.Columns
            If c.Visible Then visible.Add(c)
        Next
        If item.Checked AndAlso visible.Count = 1 AndAlso visible(0) Is col Then
            UserMessage.OK(My.Resources.PleaseEnsureAtLeastOneColumnIsAlwaysVisible)
            Exit Sub
        End If

        '(Un?)Check the item
        item.Checked = Not item.Checked

        'Check the same menu item in the other menu
        If item.Parent Is mnuColumns Then
            mColumnsContextMenu.MenuItems(item.Name).Checked = item.Checked
            mColumnsContextMenu.MenuItems(item.Name).Enabled = item.Enabled
        ElseIf item.Parent Is mColumnsContextMenu Then
            mnuColumns.MenuItems(item.Name).Checked = item.Checked
            mnuColumns.MenuItems(item.Name).Enabled = item.Enabled
        End If

        ' Set the Show All menus accordingly.
        If item.Checked = False Then
            mnuShowAll.Checked = False
            mnuShowAll.Enabled = True
            mShowAllContextMenu.Checked = False
            mShowAllContextMenu.Enabled = True
        End If

        ' Size the column and set the visibility
        If item.Checked Then
            ' Use autosizing initialy and then switch to 'none' to allow the
            ' user to size the columns.
            col.Visible = True
            For Each c As DataGridViewColumn In gridLog.Columns
                If c.Visible Then ApplyAutoSizeColumnMode(c)
            Next
            For Each c As DataGridViewColumn In gridLog.Columns
                c.AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            Next
        Else
            col.Visible = False
        End If

        PopulateColumnsToSearchListBox()

    End Sub

    ''' <summary>
    ''' Swaps between List view and Grid view.
    ''' </summary>
    Private Sub mnuViewMode_Click(ByVal sender As Object, ByVal e As EventArgs)

        ' If what's requested is already set, ignore it
        If sender Is mnuListMode AndAlso mViewType = ViewType.List Then Return
        If sender Is mnuGridMode AndAlso mViewType = ViewType.Grid Then Return

        ' First thing to do so that any further calls return immediately from
        ' this method if called as a result of this method making changes to
        ' the state of the controls
        mViewType = If(sender Is mnuListMode, ViewType.List, ViewType.Grid)

        ' Get the log number from the selected row
        If gridLog.SelectedCells.Count = 0 Then
            mSelectedLogNumber = 1
        Else
            Dim rowInd As Integer = gridLog.SelectedCells(0).RowIndex
            mSelectedLogNumber = CInt(gridLog("LogNumber", rowInd).Value)
        End If

        If sender Is mnuListMode Then
            cmbViewType.SelectedValue = ViewType.List

            mnuListMode.Checked = True
            mnuGridMode.Checked = False

            mListViewContextMenu.Checked = True
            mGridViewContextMenu.Checked = False

            mnuColumns.Visible = False

            'Get the hidden columns used in the grid view
            mHiddenColumns = New List(Of String)
            For Each c As DataGridViewColumn In gridLog.Columns
                If Not c.Visible Then mHiddenColumns.Add(c.Name)
            Next

            'Close the parameter viewer if necessary
            If mLogParameterViewer IsNot Nothing Then mLogParameterViewer.Close()

        Else
            cmbViewType.SelectedValue = ViewType.Grid


            mnuListMode.Checked = False
            mnuGridMode.Checked = True

            mListViewContextMenu.Checked = False
            mGridViewContextMenu.Checked = True

            mnuColumns.Visible = True

        End If

        'Reset the datagridview and repopulate.
        gridLog.Columns.Clear()
        DisposeOfPages()

        ReDim mPages(TotalPages - 1)
        PopulateDataGridView()

    End Sub

    ''' <summary>
    ''' Swaps between List view and Grid view.
    ''' </summary>
    Private Sub cmbViewType_SelectedValueChanged(ByVal sender As Object, ByVal e As EventArgs)
        Dim viewType = cmbViewType.GetSelectedValueOrDefault(Of ViewType)

        If viewType = ViewType.List Then
            mnuListMode.PerformClick()
        ElseIf viewType = ViewType.Grid Then
            mnuGridMode.PerformClick()
        End If
        gridLog.Focus()

    End Sub


#End Region

#Region " Page Navigation Handlers "

    Private Sub btnPrevPage_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnPrevPage.Click
        If mPage > 1 Then
            If mLogParameterViewer IsNot Nothing Then
                mLogParameterViewer.Close()
            End If
            mPage -= 1
            PopulateDataGridView()
            btnPrevPage.Enabled = mPage > 1
            btnFirstPage.Enabled = btnPrevPage.Enabled
            btnNextPage.Enabled = mPage < TotalPages
            btnLastPage.Enabled = btnNextPage.Enabled
            txtCurrentPage.Text = mPage.ToString
            txtTotalPages.Text = TotalPages.ToString
        End If
    End Sub

    Private Sub btnNextPage_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnNextPage.Click
        btnNextPage_ClickDo()
    End Sub

    Private Sub btnNextPage_ClickDo()
        If mPage < TotalPages Then
            If mLogParameterViewer IsNot Nothing Then
                mLogParameterViewer.Close()
            End If
            mPage += 1
            PopulateDataGridView()
            btnNextPage.Enabled = mPage < TotalPages
            btnLastPage.Enabled = btnNextPage.Enabled
            btnPrevPage.Enabled = True
            btnFirstPage.Enabled = btnPrevPage.Enabled
            txtCurrentPage.Text = mPage.ToString
            txtTotalPages.Text = TotalPages.ToString
        End If
    End Sub

    Private Sub btnFirstPage_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnFirstPage.Click
        btnFirstPage_ClickDo()
    End Sub

    Private Sub btnFirstPage_ClickDo()
        If mLogParameterViewer IsNot Nothing Then
            mLogParameterViewer.Close()
        End If
        mPage = 1
        PopulateDataGridView()
        btnPrevPage.Enabled = False
        btnFirstPage.Enabled = btnPrevPage.Enabled
        btnNextPage.Enabled = mPage < TotalPages
        btnLastPage.Enabled = btnNextPage.Enabled
        txtCurrentPage.Text = mPage.ToString
        txtTotalPages.Text = TotalPages.ToString
    End Sub

    Private Sub btnLastPage_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnLastPage.Click
        If mLogParameterViewer IsNot Nothing Then
            mLogParameterViewer.Close()
        End If
        mPage = TotalPages
        PopulateDataGridView()
        btnNextPage.Enabled = False
        btnLastPage.Enabled = btnNextPage.Enabled
        btnPrevPage.Enabled = True
        btnFirstPage.Enabled = btnPrevPage.Enabled
        txtCurrentPage.Text = mPage.ToString
        txtTotalPages.Text = TotalPages.ToString
    End Sub

    Private Sub cmbRowsPerPage_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles cmbRowsPerPage.KeyDown

        If (e.KeyCode = Keys.Enter OrElse e.KeyCode = Keys.F3) _
         AndAlso IsNumeric(cmbRowsPerPage.Text) Then
            mRowsPerPage = CInt(cmbRowsPerPage.Text)
            DisposeOfPages()
            ReDim mPages(TotalPages - 1)
            btnFirstPage_ClickDo()
        End If

    End Sub

    Private Sub cmbRowsPerPage_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)

        If IsNumeric(cmbRowsPerPage.Text) Then
            mRowsPerPage = CInt(cmbRowsPerPage.Text)
            DisposeOfPages()
            ReDim mPages(TotalPages - 1)
            btnFirstPage_ClickDo()

        ElseIf cmbRowsPerPage.SelectedIndex = cmbRowsPerPage.Items.Count - 1 Then
            mRowsPerPage = mTotalRows
            DisposeOfPages()
            ReDim mPages(0)
            btnFirstPage_ClickDo()

        End If

    End Sub

    ''' <summary>
    ''' Sets a flag to indicate a page change is due.
    ''' </summary>
    Private Sub txtCurrentPage_TextChanged(ByVal sender As Object, ByVal e As EventArgs)
        mPageChangeRequired = True
    End Sub

    ''' <summary>
    ''' Changes the page when the current page field loses focus.
    ''' </summary>
    Private Sub txtCurrentPage_LostFocus(ByVal sender As Object, ByVal e As EventArgs)
        ChangePage()
    End Sub

    ''' <summary>
    ''' Changes the page when ENTER or TAB is pressed in the current page field.
    ''' </summary>
    Private Sub txtCurrentPage_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs)

        If (e.KeyCode = Keys.Enter Or e.KeyCode = Keys.Tab) Then
            ChangePage()
        End If

    End Sub

    ''' <summary>
    ''' Repopulates the vieweer when the current page field is changed
    ''' </summary>
    Private Sub ChangePage()

        If mPageChangeRequired And IsNumeric(txtCurrentPage.Text) Then

            mPageChangeRequired = False

            Dim iTargetPage As Integer = CInt(txtCurrentPage.Text)

            If iTargetPage < 1 Then
                iTargetPage = 1
                RemoveHandler txtCurrentPage.TextChanged, AddressOf txtCurrentPage_TextChanged
                txtCurrentPage.Text = iTargetPage.ToString
                AddHandler txtCurrentPage.TextChanged, AddressOf txtCurrentPage_TextChanged
            End If

            If iTargetPage > TotalPages Then
                iTargetPage = TotalPages
                RemoveHandler txtCurrentPage.TextChanged, AddressOf txtCurrentPage_TextChanged
                txtCurrentPage.Text = iTargetPage.ToString
                AddHandler txtCurrentPage.TextChanged, AddressOf txtCurrentPage_TextChanged
            End If

            If mPage <> iTargetPage Then

                mPage = iTargetPage

                btnPrevPage.Enabled = mPage > 1
                btnFirstPage.Enabled = btnPrevPage.Enabled
                btnNextPage.Enabled = mPage < TotalPages
                btnLastPage.Enabled = btnNextPage.Enabled

                PopulateDataGridView()

            End If

        End If

    End Sub

#End Region

    Private Sub DisposeOfPages()
        mPages?.ForEach(Sub(x) x?.Dispose()).Evaluate()
    End Sub

#Region " Class Search "

    ''' <summary>
    ''' Represents a user search, including their desired
    ''' search term, and highlighting colour.
    ''' </summary>
    Friend Class SearchItem
        ''' <summary>
        ''' A regex used to match the search term against
        ''' candidate strings.
        ''' </summary>
        Public SearchPattern As Regex
        ''' <summary>
        ''' The colour to be used when highlighting results.
        ''' </summary>
        Public HighlightColour As Color
        ''' <summary>
        ''' The text to seach for.
        ''' </summary>
        ''' <remarks></remarks>
        Public Text As String
        ''' <summary>
        ''' Indicate a case sensitive search.
        ''' </summary>
        ''' <remarks></remarks>
        Public MatchCase As Boolean
        ''' <summary>
        ''' Indicates a whole word search.
        ''' </summary>
        ''' <remarks></remarks>
        Public MatchWholeWord As Boolean

        ''' <summary>
        ''' Checks if this search item matches the given table at the given row and
        ''' column location
        ''' </summary>
        ''' <param name="tab">The table to check</param>
        ''' <param name="row">The row number to test</param>
        ''' <param name="col">The column number to test</param>
        ''' <returns>True if the value in the given data table at the specified
        ''' location matched the search pattern set in this item; False otherwise.
        ''' </returns>
        Public Function Matches(
         ByVal tab As DataTable, ByVal row As Integer, ByVal col As Integer) As Boolean
            Return Matches(tab.Rows(row)(col))
        End Function

        ''' <summary>
        ''' Checks if this search item matches the given value.
        ''' </summary>
        ''' <param name="cellValue">The value of the cell to test.</param>
        ''' <returns>True if the specified value matched the search pattern set in
        ''' this item; False otherwise. Note that a value of DBNull will always
        ''' return false.</returns>
        Public Function Matches(ByVal cellValue As Object) As Boolean
            If SearchPattern Is Nothing Then Return False
            Dim str As String = BPUtil.IfNull(Of Object)(cellValue, "").ToString()
            Return SearchPattern.IsMatch(str)
        End Function

    End Class

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

    Public Property EnvironmentBackColor As Color Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return titleBar.BackColor
        End Get
        Set(value As Color)
            titleBar.BackColor = value
        End Set
    End Property

    Public Property EnvironmentForeColor As Color Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return titleBar.TitleColor
        End Get
        Set(value As Color)
            titleBar.TitleColor = value
        End Set
    End Property

    ''' <summary>
    ''' The type of view used to display the list
    ''' </summary>
    Private Enum ViewType
        ''' <summary>
        ''' Results displayed as grid
        ''' </summary>
        Grid
        ''' <summary>
        ''' Results display as list
        ''' </summary>
        List
    End Enum
End Class
