Imports System.Globalization
Imports AutomateControls
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib

Public Class ctlAuditLogs
    Implements IChild
    Implements IPermission
    Implements IHelp

    Private mSorted As Boolean = False
    Private mAuditLogs As DataTable = Nothing
    Private mFilteredAuditLogs As IEnumerable(Of DataRow)
    Private mCurrentComboIndex As Integer = 0

    Public Sub New()
        
        InitializeComponent()
        InitializeFilter()
        PopulateAuditSessions()
        AddHandler gridAudit.DataBindingComplete, AddressOf HandleAuditGridDataBindingComplete
        ToggleShowHideButton(True)
        cboSort.SelectedIndex = 0

    End Sub

    ''' <summary>
    ''' Formats the data grid view once the data is in place.
    ''' </summary>
    Private Sub HandleAuditGridDataBindingComplete(
     ByVal sender As Object, ByVal e As DataGridViewBindingCompleteEventArgs)

        ' Stop handling data binding and start handling cell formatting.
        RemoveHandler gridAudit.DataBindingComplete,
         AddressOf HandleAuditGridDataBindingComplete
        AddHandler gridAudit.CellFormatting, AddressOf HandleAuditGridCellFormatting

        gridAudit.SuspendLayout()
        gridAudit.Columns(0).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        gridAudit.Columns(0).SortMode = DataGridViewColumnSortMode.Programmatic
        gridAudit.ResumeLayout()

    End Sub

    ''' <summary>
    ''' Handles a mouse click in the audit gridview
    ''' </summary>
    Private Sub GridAudit_MouseClick(
     ByVal sender As Object, ByVal e As MouseEventArgs) _
     Handles gridAudit.MouseClick
        If e.Button = MouseButtons.Right Then _
         ctxMenuAudit.Show(gridAudit, e.Location)
    End Sub


    ''' <summary>
    ''' Handles a 'View Log' request for an audit log
    ''' </summary>
    Private Sub GridAudit_DoubleClick(
     ByVal sender As Object, ByVal e As EventArgs) _
         Handles gridAudit.DoubleClick,
         ViewSelectedLogsToolStripMenuItem.Click

        ShowAuditViewForm()
    End Sub

    ''' <summary>
    ''' Sort the audit data grid by date
    ''' </summary>
    Private Sub SortGridByDate() Handles cboSort.SelectedIndexChanged

        If mCurrentComboIndex = cboSort.SelectedIndex Then
            Return
        End If

        If Not mSorted Then
            SetGridAscending()
            mSorted = True
        Else
            SetGridDataSource()
            mSorted = False
        End If

        mCurrentComboIndex = cboSort.SelectedIndex

    End Sub

    ''' <summary>
    ''' Displays a log viewer form for each selected date in a data grid view.
    ''' </summary>
    Private Sub ShowAuditViewForm()

        If gridAudit.Rows.GetRowCount(DataGridViewElementStates.Selected) = 0 Then Return

        Dim datesShown As New clsSet(Of Date)
        For Each c As DataGridViewCell In gridAudit.SelectedCells
            Dim startTime = CType(c.Value, Date)
            If datesShown.Add(startTime) Then
                ParentAppForm.StartForm(New frmAuditLogViewer(startTime, startTime.AddDays(1), txtSearch.Text), False, True)
            End If
        Next

    End Sub

    ''' <summary>
    ''' Reads audit data from the DB and applies it to the audit data grid view.
    ''' </summary>
    Private Sub PopulateAuditSessions()

        Try
            mAuditLogs = gSv.GetAuditLogsByDateRange(dpFrom.Value, dpTo.Value.AddDays(1))
            mFilteredAuditLogs = mAuditLogs.Rows.OfType(Of DataRow)
            SetGridDataSource()
            gridAudit.Columns(0).HeaderText = My.Resources.ctlAuditLogs_Date
        Catch ex As Exception
            UserMessage.Err(ex, My.Resources.FailedToGetAuditLogDataFromDatabase0, ex.Message)
        End Try

    End Sub

    ''' <summary>
    ''' Applies the session status font colour.
    ''' </summary>
    Private Sub HandleAuditGridCellFormatting(
     ByVal sender As Object, ByVal e As DataGridViewCellFormattingEventArgs)
        e.CellStyle.Format = "d"
        e.CellStyle.ForeColor = Color.Black
    End Sub

    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm

    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName("Audit - Audit Logs")
        End Get
    End Property

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "frmAuditLogViewer.htm"
    End Function

    Private Sub SearchButton_Click() Handles SearchButton.Click

        Try
            mAuditLogs = gSv.GetAuditLogsByDateRange(dpFrom.Value, dpTo.Value.AddDays(1))
            mFilteredAuditLogs = FilterAuditLogsBySearchText()

            If mSorted Then
                SetGridAscending()
            Else
                SetGridDataSource()
            End If
            GenerateSearchCopyText()
        Catch ex As Exception
            UserMessage.Err(ex, My.Resources.FailedToGetAuditLogDataFromDatabase0, ex.Message)
        End Try

    End Sub

    Private Sub SetGridDataSource()

        gridAudit.DataSource = mFilteredAuditLogs.
            Select(Function(x) x.Item(0)).
            Select(Function(x) CType(x, Date)).
            Select(Function(x) x.ToLocalTime()).
            Select(Function(x) x.Date).
            Distinct().
            Select(Function(x) New With {.Date = x}).
            OrderByDescending(Function(x) x.Date).
            ToList()

    End Sub

    Private Sub SetGridAscending()

        gridAudit.DataSource = mFilteredAuditLogs.
            Select(Function(x) x.Item(0)).
            Select(Function(x) CType(x, Date)).
            Select(Function(x) x.ToLocalTime()).
            Select(Function(x) x.Date).
            Distinct().
            Select(Function(x) New With {.Date = x}).
            ToList()

    End Sub

    Private Function FilterAuditLogsBySearchText() As IEnumerable(Of DataRow)

        Dim filteredAuditLogs = mAuditLogs.Rows.OfType(Of DataRow)
        Dim searchText = txtSearch.Text.Trim()

        If Not String.IsNullOrEmpty(searchText) Then
            filteredAuditLogs = filteredAuditLogs.Where(Function(x) x.Item("Narrative").ToString().Contains(searchText, StringComparison.CurrentCultureIgnoreCase) OrElse
                x.Item("Comments").ToString().Contains(searchText, StringComparison.CurrentCultureIgnoreCase))
        End If

        Return filteredAuditLogs

    End Function

    Private Sub ResetFilter() Handles ResetButton.Click

        Try
            dpFrom.Value = Date.Now.AddDays(-7)
            dpTo.Value = Date.Now
            txtSearch.Text = Nothing
            mAuditLogs = gSv.GetAuditLogsByDateRange(dpFrom.Value, dpTo.Value.AddDays(1))
            mFilteredAuditLogs = mAuditLogs.Rows.OfType(Of DataRow)
            If cboSort.SelectedIndex = 0 Then
                SetGridDataSource()
            Else
                cboSort.SelectedIndex = 0
            End If
            GenerateSearchCopyText()
        Catch ex As Exception
            UserMessage.Err(ex, My.Resources.FailedToGetAuditLogDataFromDatabase0, ex.Message)
        End Try

    End Sub

    Private Sub InitializeFilter()
        Dim currentCulture = New CultureInfo(CultureInfo.CurrentCulture.Name)
        dpFrom.Culture = currentCulture
        dpTo.Culture = currentCulture

        dpFrom.Value = Date.Now.AddDays(-7)
        GenerateSearchCopyText()
    End Sub

    Private Sub ShowHideButton_Click(sender As Object, e As EventArgs) Handles ShowHideButton.Click
        ToggleShowHideButton()
    End Sub

    Private Sub ToggleShowHideButton(Optional forceShow As Boolean = False)
        Const Padding = 16
        Const VisibleLocation = 200

        If Not SearchPanel.Visible OrElse forceShow Then
            SearchPanel.Show()
            ShowHideButton.Top = VisibleLocation
            ShowHideButton.Text = My.Resources.ctlAuditLogs_SearchButtonText_Hide
        Else
            SearchPanel.Hide()
            ShowHideButton.Top = Padding
            ShowHideButton.Text = My.Resources.ctlAuditLogs_SearchButtonText_Show
        End If

        gridAudit.Top = ShowHideButton.Top + ShowHideButton.Height + Padding
        gridAudit.Height = Height - gridAudit.Top - Padding
        cboSort.Top = gridAudit.Top
        lblSearchCopy.Top = ShowHideButton.Top
    End Sub

    Private Sub GenerateSearchCopyText()
        Dim dateFrom = dpFrom.Value.ToString(dpFrom.FormatProvider.ShortDatePattern)
        Dim dateTo = dpTo.Value.ToString(dpTo.FormatProvider.ShortDatePattern)

        If dateFrom.Equals(dateTo) AndAlso String.IsNullOrEmpty(txtSearch.Text) Then
            lblSearchCopy.Text = String.Format(My.Resources.ctlAuditLogs_SearchCopyLabel_OneDate, dateFrom)
        ElseIf dateFrom.Equals(dateTo) AndAlso Not String.IsNullOrEmpty(txtSearch.Text) Then
            lblSearchCopy.Text = String.Format(My.Resources.ctlAuditLogs_SearchCopyLabel_OneDate_WithSearchTerm, dateFrom, txtSearch.Text)
        ElseIf Not dateFrom.Equals(dateTo) AndAlso String.IsNullOrEmpty(txtSearch.Text) Then
            lblSearchCopy.Text = String.Format(My.Resources.ctlAuditLogs_SearchCopyLabel_TwoDates, dateFrom, dateTo)
        Else
            lblSearchCopy.Text = String.Format(My.Resources.ctlAuditLogs_SearchCopyLabel_TwoDates_WithSearchTerm, dateFrom, dateTo, txtSearch.Text)
        End If
    End Sub

    Private Sub OnWindowResize() Handles Me.Resize
        Const RightPadding = 150
        lblSearchCopy.Width = Width - RightPadding
    End Sub

End Class
