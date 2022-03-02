Imports System.Windows.Forms.Design
Imports ViewType = System.Windows.Forms.View
Imports AutomateControls

Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib.Collections

Imports BluePrism.AutomateAppCore
Imports BluePrism.Images

''' <summary>
''' List which loads and displays the history of a process from the audit records
''' </summary>
<Designer(GetType(ControlDesigner))>
Public Class ProcessHistoryListView : Inherits FlickerFreeListView : Implements IMode

#Region " Class-scope Declarations "

    ''' <summary>
    ''' The minimum width allowed to be set (programmatically) for the summary column
    ''' </summary>
    Private Const SummaryMinimumWidth As Integer = 50

    ''' <summary>
    ''' A lookup of audit codes to a display colour to represent them. If a code is
    ''' not recognised, it defaults to black
    ''' </summary>
    Private Shared ReadOnly mCodeColourLookup As IDictionary(Of String, Color) = _
     GenerateCodeColourLookup()

    ''' <summary>
    ''' Generates a mapping of display colours for particular audit codes.
    ''' </summary>
    ''' <returns>A map of the display colour to use mapped against the audit code
    ''' to which it applies. Broadly: deletions are red; modifications are blue;
    ''' creation,cloning,importing and inheritance is green, any unknowns will be
    ''' <see cref="Color.Empty"/> - ie. black.</returns>
    Private Shared Function GenerateCodeColourLookup() _
     As IDictionary(Of String, Color)
        Dim map As New clsGeneratorDictionary(Of String, Color)

        'creation, cloning, importing, inheritance
        For Each code As String In New String() { _
         "P001", "B001", "P003", "B003", "P006", "B006", "P008", "B008"}
            map(code) = Color.Green
        Next

        ' deletion
        For Each code As String In New String() {"P002", "B002"}
            map(code) = Color.DarkRed
        Next

        ' modification
        For Each code As String In New String() {"P004", "B004"}
            map(code) = Color.Blue
        Next

        Return GetReadOnly.IDictionary(map)
    End Function

    ''' <summary>
    ''' The edit types displayed by this process history list.
    ''' </summary>
    Private Shared mDisplayableEditTypes As ICollection(Of String) =
        GetReadOnly.ICollection(New clsSet(Of String) From {
        "P001", "P002", "P003", "P004", "P006", "P008",
        "B001", "B002", "B003", "B004", "B006", "B008"
    })

    ''' <summary>
    ''' Gets the edit type as a friendly name from the given audit code.
    ''' </summary>
    ''' <param name="s">The audit code for which the friendly name is required.
    ''' </param>
    ''' <returns>The user-friendly representation of the given audit code, or an
    ''' empty string if there is no such representation - this usually implies that
    ''' the audit code is not for user consumption and shouldn't be displayed in
    ''' the process history.</returns>
    Private Shared Function GetFriendlyEditType(ByVal s As String) As String
        Select Case s
            Case "P001", "B001" : Return My.Resources.ProcessHistoryListView_Creation
            Case "P002", "B002" : Return My.Resources.ProcessHistoryListView_Deletion
            Case "P003", "B003" : Return My.Resources.ProcessHistoryListView_CreationViaCloning
            Case "P004", "B004" : Return My.Resources.ProcessHistoryListView_Modification
            Case "P005", "B005" ' Unlock
            Case "P006", "B006" : Return My.Resources.ProcessHistoryListView_CreationViaImporting
            Case "P007", "B007" ' Export
            Case "P008", "B008" : Return My.Resources.ProcessHistoryListView_Inherited ' pre-v2
            Case "P009", "B009" ' Attributes changed
            Case "T008", "T009" ' Assigned to / removed from a schedule
            Case Else
                ' No message - if it's not supported, just don't show the history item.
        End Select
        Return ""
    End Function

    ''' <summary>
    ''' Specialised list view item which deals with process history events.
    ''' </summary>
    Private Class ProcessHistoryItem : Inherits ListViewItem
        Private mEventId As Integer
        Private mEditType As String
        Private mEventDate As Date
        Private mAvail As Boolean
        Private mUser As String
        Private mSummary As String

        ''' <summary>
        ''' Creates a new list view item representing a process history event
        ''' </summary>
        ''' <param name="prov">The data provider with the data for this history event
        ''' list view item. This expects the following data:<list>
        ''' <item>EventID: Integer: The ID of the audit event</item>
        ''' <item>sCode: String: The audit code of the event</item>
        ''' <item>eventdatetime: Date: The (UTC) date/time of the audit event, or
        ''' <see cref="DateTime.MinValue"/> if it has no date/time associated.</item>
        ''' <item>UserName: String: The username of the user who caused this audit
        ''' event to be created</item>
        ''' <item>EditSummary: String: The summary associated with this audit event.
        ''' </item>
        ''' <item>xmlavailable: Boolean: Whether the XML for this process is
        ''' available for this history event</item>
        ''' </list></param>
        Public Sub New(prov As IDataProvider)
            ' Populate the memvars
            mEventId = prov.GetValue("EventID", 0)
            mEditType = prov.GetString("sCode")
            mEventDate = prov.GetValue("eventdatetime", Date.MinValue)
            mAvail = prov.GetValue("xmlavailable", False)
            mUser = prov.GetString("UserName")
            mSummary = prov.GetString("EditSummary")

            ' Then build the item itself and its subitems from them
            Text = EventDateDisplay
            ForeColor = mCodeColourLookup(EditType)
            With SubItems
                .Add(EditTypeDisplay)
                .Add(UserName)
                .Add(Summary)
                .Add(If(Available, My.Resources.ProcessHistoryItem_Yes, My.Resources.ProcessHistoryItem_No))
            End With
        End Sub

        ''' <summary>
        ''' The event ID of the audit event represented by this list view item
        ''' </summary>
        Public ReadOnly Property EventId As Integer
            Get
                Return mEventId
            End Get
        End Property

        ''' <summary>
        ''' The event date (UTC) for this event, or <see cref="DateTime.MinValue"/>
        ''' if it has no date set.
        ''' </summary>
        Public ReadOnly Property EventDate As Date
            Get
                Return mEventDate
            End Get
        End Property

        ''' <summary>
        ''' The localised event date display for this event, or an empty string if it
        ''' has no date set.
        ''' </summary>
        Public ReadOnly Property EventDateDisplay As String
            Get
                If mEventDate = Date.MinValue Then Return ""
                Return mEventDate.ToLocalTime().ToString()
            End Get
        End Property

        ''' <summary>
        ''' The (raw) edit type of this audit event
        ''' </summary>
        Public ReadOnly Property EditType As String
            Get
                Return mEditType
            End Get
        End Property

        ''' <summary>
        ''' Gets the display version of the edit type for the audit event represented
        ''' by this list view item.
        ''' </summary>
        Public ReadOnly Property EditTypeDisplay As String
            Get
                Return GetFriendlyEditType(mEditType)
            End Get
        End Property

        ''' <summary>
        ''' Gets the availability of the process XML for this audit event.
        ''' </summary>
        Public ReadOnly Property Available As Boolean
            Get
                Return mAvail
            End Get
        End Property

        ''' <summary>
        ''' The username associated with this audit event
        ''' </summary>
        Public ReadOnly Property UserName As String
            Get
                Return mUser
            End Get
        End Property

        ''' <summary>
        ''' The audit summary for this audit event.
        ''' </summary>
        Public ReadOnly Property Summary As String
            Get
                Return mSummary
            End Get
        End Property

    End Class

#End Region

#Region " Member Variables "

    ' The ID of the process whose history is being displayed in this list
    Private mProcessId As Guid

    ' The column to show the event date/time
    Private WithEvents colDateTime As ColumnHeader

    ' The column to show the edit type
    Private WithEvents colEditType As ColumnHeader

    ' The column to show the user who made the change
    Private WithEvents colUser As ColumnHeader

    ' The column to show the summary of the change
    Private WithEvents colSummary As ColumnHeader

    ' The column to indicate if the XML is available (for comparisons)
    Private WithEvents colAvail As ColumnHeader

    ' The context menu for this list
    Private WithEvents mContextMenu As ContextMenuStrip

    ' Context menu item to view a historical entry
    Private WithEvents mViewEntry As ToolStripMenuItem

    ' Context menu item to compare two historical entries
    Private WithEvents mCompareEntries As ToolStripMenuItem

#End Region

#Region " Auto-properties "

    ''' <summary>
    ''' The mode in which this list is operating
    ''' </summary>
    Public Property Mode() As ProcessType Implements IMode.Mode

    ''' <summary>
    ''' Indicates whether or not the current user has permission to
    ''' view the selected Object/Process definition
    ''' </summary>
    Public Property CanViewDefinition() As Boolean = True

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new process history list control
    ''' </summary>
    Public Sub New()
        ' Some basic settings
        HideSelection = False
        FullRowSelect = True
        GridLines = True
        View = ViewType.Details

        ' Set up the columns
        colDateTime = New ColumnHeader() With {.Text = My.Resources.ProcessHistoryListView_DateAndTime, .Width = 135}
        colEditType = New ColumnHeader() With {.Text = My.Resources.ProcessHistoryListView_TypeOfEdit, .Width = 120}
        colUser = New ColumnHeader() With {.Text = My.Resources.ProcessHistoryListView_ByUser, .Width = 90}
        colSummary = New ColumnHeader() With {.Text = My.Resources.ProcessHistoryListView_EditSummary, .Width = 250}
        colAvail = New ColumnHeader() With {.Text = My.Resources.ProcessHistoryListView_Available, .Width = 85}

        Columns.AddRange({colDateTime, colEditType, colUser, colSummary, colAvail})
        FillColumn = colSummary.Index

        ' Create and set the sorter; we only need to specify the first column - the
        ' rest are strings which the sorter assumes and handles without being told
        ListViewItemSorter = New clsListViewSorter(Me) With {
            .ColumnDataTypes = {GetType(Date)},
            .SortColumn = 0,
            .Order = SortOrder.Descending
        }

        mViewEntry = New ToolStripMenuItem() With {
            .Text = My.Resources.ProcessHistoryListView_View,
            .Image = ToolImages.Window_View_16x16
        }

        mCompareEntries = New ToolStripMenuItem With {
            .Text = My.Resources.ProcessHistoryListView_Compare,
            .Image = ToolImages.Window_Tile_Vertical_16x16
        }

        mContextMenu = New ContextMenuStrip()
        mContextMenu.Items.AddRange({mViewEntry, mCompareEntries})

        ContextMenuStrip = mContextMenu

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The application form ultimately hosting this list or null if it is not hosted
    ''' on an instance of <see cref="frmApplication"/>
    ''' </summary>
    Private ReadOnly Property ParentAppForm As frmApplication
        Get
            Return TryCast(TopLevelControl, frmApplication)
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the process ID to display in this history list
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property ProcessId As Guid
        Get
            Return mProcessId
        End Get
        Set(value As Guid)
            If value = mProcessId Then Return
            mProcessId = value
            If Not DesignMode AndAlso IsHandleCreated Then RefreshView()
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets a value indicating whether the selected item in the control
    ''' remains highlighted when the control loses focus.
    ''' This is here just to hide the property from the visual designer - it should
    ''' always be the default value (False) in this control
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Overloads Property HideSelection As Boolean
        Get
            Return MyBase.HideSelection
        End Get
        Set(value As Boolean)
            MyBase.HideSelection = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets a value indicating whether clicking an item selects all its
    ''' subitems.
    ''' This is here just to hide the property from the visual designer - it should
    ''' always be the default value (False) in this control
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Overloads Property FullRowSelect As Boolean
        Get
            Return MyBase.FullRowSelect
        End Get
        Set(value As Boolean)
            MyBase.FullRowSelect = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets a value indicating whether grid lines appear between the rows
    ''' and columns containing the items and subitems in the control.
    ''' This is here just to hide the property from the visual designer - it should
    ''' always be the default value (False) in this control
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Overloads Property GridLines As Boolean
        Get
            Return MyBase.GridLines
        End Get
        Set(value As Boolean)
            MyBase.GridLines = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the viewtype to use in this listview.
    ''' This is here just to hide the property from the visual designer - it should
    ''' always be the default value (False) in this control
    ''' </summary>
    <Browsable(True), Category("Appearance"), DefaultValue(ViewType.Details),
     Description("The view type to use in the listview")>
    Public Overloads Property View As View
        Get
            Return MyBase.View
        End Get
        Set(value As View)
            MyBase.View = value
        End Set
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Checks if XML is available for the given list view item
    ''' </summary>
    ''' <param name="item">The item to check for process XML availability</param>
    ''' <returns>True if XML is available for the given item; False otherwise.
    ''' </returns>
    Private Function IsXmlAvailable(item As ListViewItem) As Boolean
        Dim phItem = TryCast(item, ProcessHistoryItem)
        Return (phItem IsNot Nothing AndAlso phItem.Available)
    End Function

    ''' <summary>
    ''' Ensures the menu items are enabled / disabled appropriately
    ''' </summary>
    Private Sub HandleContextMenuOpening() Handles mContextMenu.Opening
        Dim items = SelectedItems
        mViewEntry.Enabled = (items.Count = 1 AndAlso IsXmlAvailable(items(0)) AndAlso CanViewDefinition)
        mCompareEntries.Enabled = (items.Count = 2 AndAlso
                                   IsXmlAvailable(items(0)) AndAlso
                                   IsXmlAvailable(items(1)) AndAlso CanViewDefinition)
    End Sub

    ''' <summary>
    ''' Triggers the viewing of the currently selected history entry.
    ''' Any error are reported to the user and squashed by this method
    ''' </summary>
    Friend Sub ViewSelectedHistoryEntry()
        ' Check correct number selected (ie. precisely 1)
        If SelectedItems.Count = 0 Then UserMessage.Err(
            My.Resources.ProcessHistoryListView_PleaseChooseAnAvailableHistoryEntryToView) : Return
        If SelectedItems.Count > 1 Then UserMessage.Err(
            My.Resources.ProcessHistoryListView_PleaseChooseASingleAvailableHistoryEntryToView) : Return

        ' Make sure that the XML is available
        Dim item = TryCast(SelectedItems(0), ProcessHistoryItem)
        If item Is Nothing Then UserMessage.Err(
            My.Resources.ProcessHistoryListView_ErrorWhileReadingHistoryEntryNotAProcessHistoryItem) : Return
        If Not IsXmlAvailable(item) Then UserMessage.Err(
            My.Resources.ProcessHistoryListView_TheDiagramIsNotAvailableForThisHistoryEntry) : Return

        Try
            Dim viewMode = If(Mode.IsBusinessObject,
                              ProcessViewMode.PreviewObject,
                              ProcessViewMode.PreviewProcess)

            Dim openForm = frmProcess.GetInstance(ProcessId, viewMode)
            If openForm Is Nothing Then
                ParentAppForm.StartForm(New frmProcess(
                 viewMode, gSv.GetProcessHistoryXML(item.EventId, ProcessId), ProcessId))
            Else
                openForm.Activate()
            End If
        Catch ex As Exception
            UserMessage.Err(ex, My.Resources.ProcessHistoryListView_ErrorAttemptingToViewProcess0, ex.Message)
        End Try

    End Sub

    Friend Sub CompareSelectedHistoryEntries()
        ' Check correct number selected (ie. precisely 1)
        If SelectedItems.Count <> 2 Then UserMessage.Err(
            My.Resources.ProcessHistoryListView_PleaseChooseTwoAvailableHistoryEntriesToCompare) : Return

        ' Make sure that the XML is available
        Dim first = TryCast(SelectedItems(0), ProcessHistoryItem)
        If first Is Nothing Then UserMessage.Err(
         My.Resources.ProcessHistoryListView_ErrorComparingFirstEntryIsNotAProcessHistoryItem) : Return
        If Not IsXmlAvailable(first) Then UserMessage.Err(
         My.Resources.ProcessHistoryListView_TheDiagramIsNotAvailableForTheEntryDated0, first.Text) : Return

        Dim second = TryCast(SelectedItems(1), ProcessHistoryItem)
        If second Is Nothing Then UserMessage.Err(
         My.Resources.ProcessHistoryListView_ErrorComparingSecondEntryIsNotAProcessHistoryItem) : Return
        If Not IsXmlAvailable(second) Then UserMessage.Err(
         My.Resources.ProcessHistoryListView_TheDiagramIsNotAvailableForTheEntryDated0, first.Text) : Return

        Try
            ParentAppForm.StartForm(
             frmProcessComparison.FromHistory(first.EventId, second.EventId, first.EventDate, second.EventDate, ProcessId)
            )

        Catch ex As Exception
            UserMessage.Err(
                ex, My.Resources.ProcessHistoryListView_ErrorAttemptingToCompareProcesses0, ex.Message)

        End Try

    End Sub

    ''' <summary>
    ''' Handles the viewing of an entry from the process history.
    ''' </summary>
    Private Sub HandleViewEntryClick() Handles mViewEntry.Click
        ViewSelectedHistoryEntry()
    End Sub

    ''' <summary>
    ''' Handles the comparing of two entries from the process history.
    ''' </summary>
    Private Sub HandleCompareEntriesClick() Handles mCompareEntries.Click
        CompareSelectedHistoryEntries()
    End Sub

    ''' <summary>
    ''' Handles a, er, handle being created for this control.
    ''' Refreshes the view if a process is assigned and this list is not in design
    ''' mode.
    ''' </summary>
    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        If mProcessId <> Guid.Empty AndAlso Not DesignMode Then RefreshView()
    End Sub

    ''' <summary>
    ''' Clears the rows in this list
    ''' </summary>
    Public Sub ClearItems()
        Items.Clear()
    End Sub

    ''' <summary>
    ''' Refreshes this list with data from the database
    ''' </summary>
    Public Sub RefreshView()

        BeginUpdate()
        Try
            'add this info to the listview
            ClearItems()

            ' Load the process history for the selected process
            Using reader = gSv.GetProcessHistoryLog(ProcessId).CreateDataReader()
                Dim prov As New ReaderDataProvider(reader)
                While reader.Read()
                    If Not mDisplayableEditTypes.Contains(prov.GetString("sCode")) _
                     Then Continue While
                    Items.Add(New ProcessHistoryItem(prov))
                End While
            End Using

        Catch ex As Exception
            UserMessage.Err(ex, My.Resources.ProcessHistoryListView_Error & ex.Message)

        Finally
            EndUpdate()

        End Try
    End Sub

#End Region

End Class
