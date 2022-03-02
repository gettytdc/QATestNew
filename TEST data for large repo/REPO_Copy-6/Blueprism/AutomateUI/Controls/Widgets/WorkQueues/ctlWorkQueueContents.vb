Imports ListViewSubItem = System.Windows.Forms.ListViewItem.ListViewSubItem

Imports AutomateControls
Imports BluePrism.AutomateAppCore.Utility

Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports WorkQueueEventCode = BluePrism.AutomateAppCore.WorkQueueEventCode

Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Images
Imports BluePrism.BPCoreLib
Imports BluePrism.Server.Domain.Models
Imports System.Text.RegularExpressions
Imports System.Data.SqlClient

''' Project  : Automate
''' Class    : ctlWorkQueueContents
''' 
''' <summary>
''' A control that displays the contents of a work queue.
''' </summary>
Public Class ctlWorkQueueContents : Implements IHelp

    ''' <summary>
    ''' The column names used in the listview showing the queue contents.
    ''' </summary>
    Private Class ColumnNames
        Public Const Icon As String = "Icon"
        Public Const Position As String = "Position"
        Public Const ItemKey As String = "Item Key"
        Public Const Priority As String = "Priority"
        Public Const Status As String = "Status"
        Public Const Tags As String = "Tags"
        Public Const Resource As String = "Resource"
        Public Const Attempt As String = "Attempt"
        Public Const Created As String = "Created"
        Public Const LastUpdated As String = "Last Updated"
        Public Const NextReview As String = "Next Review"
        Public Const Completed As String = "Completed"
        Public Const TotalWorkTime As String = "Total Work Time"
        Public Const ExceptionDate As String = "Exception Date"
        Public Const ExceptionReason As String = "Exception Reason"
    End Class

    Private Class DropdownLists
        Public Shared ReadOnly DateAndTimeDropdown() As String = {
                         "All", "15 Minutes", "30 Minutes", "1 Hour", "2 Hours", "4 Hours", "8 Hours", "12 Hours",
                                "18 Hours", "24 Hours", "Today", "3 Days", "7 Days", "31 Days"
                        }
        Public Shared ReadOnly IntergerDropdown() As String = {
                                "All", ">1", "<=5"
                            }
        Public Shared ReadOnly OtherDropdown() As String = {
                         "All"
                        }
    End Class

#Region "Sorting handling"

    ''' <summary>
    ''' Object which handles the sorting for the work queue contents list view
    ''' </summary>
    ''' <remarks></remarks>
    Private mSortHandler As ListSortHandler

    ''' <summary>
    ''' Class to represent the list sort handler. The main task for this class is to 
    ''' maintain the state of the current sort details - namely the column which is
    ''' being sorted on, and the direction of the sort.
    ''' It provides accessor methods to the image required to represent the current
    ''' sort order and handles the column click event in the work queue content's
    ''' list view in order to keep track of the current sort state.
    ''' </summary>
    ''' <remarks></remarks>
    Private Class ListSortHandler

        ''' <summary>
        ''' Flag indicating whether the current sort column is ascending or not
        ''' True indicates an ascending order; False a descending order.
        ''' Default is descending
        ''' </summary>
        Private mOrder As SortOrder

        ''' <summary>
        ''' The column currently used to order the listview.
        ''' By default, this is the "Position" column
        ''' </summary>
        ''' <remarks></remarks>
        Private mOrderColumnKey As String

        ''' <summary>
        ''' The keys of the columns mapped against their corresponding sort column
        ''' </summary>
        ''' <remarks></remarks>
        Private Shared ReadOnly SortColumns As _
         Dictionary(Of String, QueueSortColumn) = GenerateSortColumns()

        ''' <summary>
        ''' Generates the sort columns which maps the keys of the columns against
        ''' their corresponding work queue content filter sort column values
        ''' </summary>
        ''' <returns>A dictionary of string (column key) against SortColumn</returns>
        Private Shared Function GenerateSortColumns() As _
         Dictionary(Of String, QueueSortColumn)

            Dim map As New Dictionary(Of String, QueueSortColumn)
            map(ColumnNames.Icon) = QueueSortColumn.ByState
            map(ColumnNames.Position) = QueueSortColumn.ByPosition
            map(ColumnNames.ItemKey) = QueueSortColumn.ByItemKey
            map(ColumnNames.Priority) = QueueSortColumn.ByPriority
            map(ColumnNames.Status) = QueueSortColumn.ByStatus
            ' Sorting on tags is meaningless (since they are in arbitrary order),
            ' so default to last updated
            map(ColumnNames.Tags) = QueueSortColumn.ByLastUpdatedDate
            map(ColumnNames.Resource) = QueueSortColumn.ByResource
            map(ColumnNames.Attempt) = QueueSortColumn.ByAttempt
            map(ColumnNames.Created) = QueueSortColumn.ByLoadedDate
            map(ColumnNames.LastUpdated) = QueueSortColumn.ByLastUpdatedDate
            map(ColumnNames.NextReview) = QueueSortColumn.ByNextReviewDate
            map(ColumnNames.Completed) = QueueSortColumn.ByCompleted
            map(ColumnNames.TotalWorkTime) = QueueSortColumn.ByWorkTime
            map(ColumnNames.ExceptionDate) = QueueSortColumn.ByExceptionDate
            map(ColumnNames.ExceptionReason) = QueueSortColumn.ByExceptionReason
            Return map
        End Function


        ''' <summary>
        ''' The parent work queue contents control that this object is sorting
        ''' </summary>
        Private mOwner As ctlWorkQueueContents

        ''' <summary>
        ''' Creates a new sort handler for the given control
        ''' </summary>
        ''' <param name="parent">The control which this object will be handling
        ''' the sorting for.</param>
        Public Sub New(ByVal parent As ctlWorkQueueContents)
            mOwner = parent
            parent.lstQueueContents.SmallImageList = clsSortImages.Instance.GetImageList()
            AddHandler parent.lstQueueContents.ColumnClick, AddressOf SortListView
            PressSortColumn("Last Updated") 'My.Resources.ListSortHandler_LastUpdated
        End Sub

        ''' <summary>
        ''' Handles the column click event for the listview that we're helping to sort.
        ''' This maintains the state of the current sort column and order, and forces
        ''' a refresh on the parent control 
        ''' </summary>
        ''' <param name="sender">The source of this event</param>
        ''' <param name="evt">The details of the event.</param>
        Public Sub SortListView(ByVal sender As Object, ByVal evt As ColumnClickEventArgs)
            PressSortColumn(mOwner.lstQueueContents.Columns(evt.Column).Name)
        End Sub

        ''' <summary>
        ''' Emulates the pressing of the work queue contents column identified by the given 
        ''' key. This will check which column is currently 'active' - if the column pressed
        ''' is the same, it will toggle the order of sorting.
        ''' If the column is different it will transfer the order to that column (using the
        ''' same direction of ordering).
        ''' It then forces a refresh in order to pick up the data from the database (since
        ''' it's the database where the actual sorting is done).
        ''' </summary>
        ''' <param name="key">The key identifying the column that's been 'pressed'</param>
        Public Sub PressSortColumn(ByVal key As String)

            Dim col As ColumnHeader = mOwner.lstQueueContents.Columns(key)
            If (col Is Nothing) Then Return
            If key = ColumnNames.Tags Then Return ' Tags cannot be sorted....

            If key = mOrderColumnKey Then
                If mOrder = SortOrder.Ascending OrElse mOrder = SortOrder.Unspecified Then
                    mOrder = SortOrder.Descending
                Else
                    mOrder = SortOrder.Ascending
                End If
            Else
                Dim oldcol As ColumnHeader = mOwner.lstQueueContents.Columns(mOrderColumnKey)
                If oldcol IsNot Nothing Then oldcol.ImageKey = Nothing
                mOrderColumnKey = key
                ' Leave the sort order as it is... unless it's nothing at all,
                ' in which case default it to descending
                If mOrder = SortOrder.Unspecified Then mOrder = SortOrder.Descending

            End If

            col.ImageIndex = clsSortImages.Instance.GetImageIndex(CType(mOrder, Windows.Forms.SortOrder))
            If mOwner.IsHandleCreated Then mOwner.RefreshList()
        End Sub


        ''' <summary>
        ''' Checks if this sorter is currently set to sort on the given column header
        ''' </summary>
        ''' <param name="colHeader">The column to check if we are sorting on</param>
        ''' <returns>True if this object is set to sort on the given column;
        ''' False otherwise.</returns>
        Public Function IsSortingOn(ByVal colHeader As ColumnHeader) As Boolean
            Return (colHeader.Name = mOrderColumnKey)
        End Function

        ''' <summary>
        ''' Checks if this sorter is currently sorting on the column represented
        ''' by the given key
        ''' </summary>
        ''' <param name="columnKey">The key to check if we're sorting on</param>
        ''' <returns>True if this sorter is sorting on the given key;
        ''' False otherwise</returns>
        Public Function IsSortingOn(ByVal columnKey As String) As Boolean
            Return (columnKey = mOrderColumnKey)
        End Function

        ''' <summary>
        ''' Gets the image which represents the current sort order
        ''' </summary>
        ''' <returns>The image representing the current sort order.</returns>
        Public Function GetImage() As Image
            Return clsSortImages.Instance.GetImage(CType(mOrder, Windows.Forms.SortOrder))
        End Function

        ''' <summary>
        ''' Applies the ordering which is currently in effect in this work queue into the
        ''' given filter.
        ''' </summary>
        Public Sub ApplyOrdering(ByVal filter As WorkQueueFilter)

            filter.SortColumn = SortColumns(mOrderColumnKey)
            If (mOrder = SortOrder.Ascending) Then
                filter.SortOrder = QueueSortOrder.Ascending
            Else
                filter.SortOrder = QueueSortOrder.Descending
            End If

        End Sub

    End Class

#End Region


    ''' <summary>
    ''' The header for the "position" column. Not displayed by default
    ''' </summary>
    Private mPositionColumn As ColumnHeader

    Private mPendingGetContents As Boolean

    Private mParent As frmApplication

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        If Not DesignMode Then
            'Prepare the listview image list

            'Prepare filters
            BuildList()
            BuildDropDownFilters()
            PopulateDropDownFilters()
            RefreshListviewSize()

            ' Add the sort handler
            mSortHandler = New ListSortHandler(Me)
        End If
    End Sub

    ''' <summary>
    ''' Configures the listview's headers, etc.
    ''' </summary>
    Private Sub BuildList()
        Try
            lstQueueContents.OwnerDraw = True

            Const dateAlignment As HorizontalAlignment = HorizontalAlignment.Right
            Const numberAlignment As HorizontalAlignment = HorizontalAlignment.Right
            Const textAlignment As HorizontalAlignment = HorizontalAlignment.Left
            Const timespanAlignment As HorizontalAlignment = HorizontalAlignment.Right

            lstQueueContents.BeginUpdate()
            With lstQueueContents.Columns
                .Add(ColumnNames.Icon, "", 40, HorizontalAlignment.Right, -1)
                mPositionColumn = .Add(ColumnNames.Position, My.Resources.ctlWorkQueueContents_Position, 75, numberAlignment, -1)
                .Add(ColumnNames.ItemKey, My.Resources.ctlWorkQueueContents_ItemKey, 120, textAlignment, -1)
                .Add(ColumnNames.Priority, My.Resources.ctlWorkQueueContents_Priority, 75, numberAlignment, -1)
                .Add(ColumnNames.Status, My.Resources.ctlWorkQueueContents_Status, 70, textAlignment, -1)
                .Add(ColumnNames.Tags, My.Resources.ctlWorkQueueContents_Tags, 120, textAlignment, -1)
                .Add(ColumnNames.Resource, My.Resources.ctlWorkQueueContents_Resource, 100, textAlignment, -1)
                .Add(ColumnNames.Attempt, My.Resources.ctlWorkQueueContents_Attempt, 80, numberAlignment, -1)
                .Add(ColumnNames.Created, My.Resources.ctlWorkQueueContents_Created, 120, dateAlignment, -1)
                .Add(ColumnNames.LastUpdated, My.Resources.ctlWorkQueueContents_LastUpdated, 140, dateAlignment, -1)
                .Add(ColumnNames.NextReview, My.Resources.ctlWorkQueueContents_NextReview, 140, dateAlignment, -1)
                .Add(ColumnNames.Completed, My.Resources.ctlWorkQueueContents_Completed, 120, dateAlignment, -1)
                .Add(ColumnNames.TotalWorkTime, My.Resources.ctlWorkQueueContents_TotalWorkTime, 140, timespanAlignment, -1)
                .Add(ColumnNames.ExceptionDate, My.Resources.ctlWorkQueueContents_ExceptionDate, 140, dateAlignment, -1)
                .Add(ColumnNames.ExceptionReason, My.Resources.ctlWorkQueueContents_ExceptionReason, 240, textAlignment, -1)
            End With

        Finally
            RefreshListviewSize()
            lstQueueContents.EndUpdate()
        End Try
    End Sub


#Region "Listview Painting" 'We draw our own listview items because, the right-align feature does not seem to work

    Private Sub lvResult_DrawItem(ByVal sender As Object, ByVal e As DrawListViewItemEventArgs) Handles lstQueueContents.DrawItem
        e.DrawBackground()
        If e.Item.Selected Then
            e.Graphics.FillRectangle(New SolidBrush(SystemColors.Highlight), e.Bounds)
        End If
        e.DrawFocusRectangle()
    End Sub


    ' Handle DrawSubItem event
    Private Sub lvResult_DrawSubItem(ByVal sender As Object, ByVal e As DrawListViewSubItemEventArgs) Handles lstQueueContents.DrawSubItem

        Dim C As Color
        If e.Item.Selected Then
            C = SystemColors.HighlightText
            e.Graphics.FillRectangle(New SolidBrush(SystemColors.Highlight), e.Bounds)
        Else
            C = SystemColors.WindowText
            e.DrawBackground()
        End If

        Dim flags As TextFormatFlags
        Dim sf As New StringFormat
        Dim HorizInflation As Integer
        Select Case e.Header.TextAlign
            Case HorizontalAlignment.Center
                sf.Alignment = StringAlignment.Center
                flags = TextFormatFlags.HorizontalCenter
            Case HorizontalAlignment.Right
                sf.Alignment = StringAlignment.Far
                flags = TextFormatFlags.Right
                HorizInflation = -3
            Case Else
                sf.Alignment = StringAlignment.Near
                flags = TextFormatFlags.Left
                HorizInflation = 3
        End Select
        Dim Bounds As Rectangle = e.Bounds
        Bounds.Offset(HorizInflation, 0)
        Bounds.Inflate(-2, 0)

        e.Graphics.DrawString(e.SubItem.Text, lstQueueContents.Font, New SolidBrush(C), Bounds, sf)

        If e.Header.Name = ColumnNames.Icon AndAlso e.Item.ImageKey <> "" Then
            Dim I As Image = GetImage(e.Item.ImageKey)
            Dim VertOffset As Integer = Math.Max(0, (ListviewItemHeight - I.Height) \ 2)
            Dim Left As Integer
            Select Case e.Header.TextAlign
                Case HorizontalAlignment.Left
                    Left = 2
                Case HorizontalAlignment.Right
                    Left = e.Header.Width - I.Width - 2
                Case HorizontalAlignment.Center
                    Left = (e.Header.Width - I.Width) \ 2
            End Select

            e.Graphics.DrawImage(I, New Rectangle(Point.Add(e.Bounds.Location, New Size(Left, 0)), I.Size))
        End If
    End Sub

    Private Sub lvResult_DrawColumnHeader(ByVal sender As Object, ByVal e As DrawListViewColumnHeaderEventArgs) Handles lstQueueContents.DrawColumnHeader


        If e.Header.TextAlign = HorizontalAlignment.Right Then
            'Draw background
            Dim HotTracked As Boolean = e.Bounds.Contains(lstQueueContents.PointToClient(System.Windows.Forms.Cursor.Position))
            AutomateControls.ControlPaintStyle.DrawListViewHeader(e.Graphics, e.Bounds, HotTracked)

            Const reservedimagewidth As Integer = 10 + 6 + 2 'left margin +  image width + right margin
            Dim sf As New StringFormat(StringFormatFlags.NoWrap)
            sf.Alignment = StringAlignment.Far
            sf.Trimming = StringTrimming.EllipsisCharacter

            Dim S As Size = e.Graphics.MeasureString(e.Header.Text, lstQueueContents.Font).ToSize
            Dim Offset As Integer = Math.Max(0, (e.Bounds.Height - S.Height) \ 2)
            e.Graphics.DrawString(e.Header.Text, lstQueueContents.Font, New SolidBrush(SystemColors.WindowText), New RectangleF(e.Bounds.Left + reservedimagewidth, e.Bounds.Top + Offset, e.Bounds.Width - 4 - reservedimagewidth, e.Bounds.Height), sf)

            If mSortHandler.IsSortingOn(e.Header) Then
                Dim I As Image = mSortHandler.GetImage()
                If I IsNot Nothing Then
                    Dim ImageOffset As Integer = Math.Max(0, (e.Bounds.Height - I.Height) \ 2)
                    e.Graphics.DrawImage(I, e.Bounds.Left + 10, ImageOffset, I.Width, I.Height)
                End If
            End If
        Else
            e.DrawDefault = True
        End If
    End Sub

#End Region



    ''' <summary>
    ''' The ID of the queue whose contents are being browsed. This is populated by a
    ''' call to RefreshList
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property QueueId() As Guid
        Get
            Return mQueueId
        End Get
        Set(value As Guid)
            If mQueueId = value Then Return
            mQueueId = value
            PopulateList(True)
        End Set
    End Property
    Private mQueueId As Guid

    ''' <summary>
    ''' Gets whether an operation to retrieve the work queue contents is currently
    ''' progressing
    ''' </summary>
    Private ReadOnly Property IsGettingContents As Boolean
        Get
            Return bwGetContents.IsBusy
        End Get
    End Property

    ''' <summary>
    ''' Refreshes the list, according to its current configuration.
    ''' </summary>
    Public Sub RefreshList()

        If IsGettingContents Then Return
        Try
            Cursor = Cursors.WaitCursor
            PopulateList(False)
        Catch ex As Exception
            UserMessage.Err(ex, My.Resources.ctlWorkQueueContents_UnexpectedError0, ex.Message)
        Finally
            Cursor = Cursors.Default
        End Try
    End Sub

    ''' <summary>
    ''' The panel shown to the user when busy populating the list.
    ''' </summary>
    Dim BusyPanel As Panel

    ''' <summary>
    ''' The label on the busypanel giving information to the user.
    ''' </summary>
    Dim BusyLabel As Label


    ''' <summary>
    ''' The progressbar on the busypanel.
    ''' </summary>
    Dim BusyProgressBar As ProgressBar

    ''' <summary>
    ''' Shows a feedback panel to the user, consisting of some text and a progress bar.
    ''' </summary>
    Private Sub ShowBusyNotification()
        If BusyPanel Is Nothing Then
            BusyPanel = New Panel
            BusyPanel.BackColor = Color.Transparent
            BusyPanel.Size = New Size(400, 44)
            AddHandler BusyPanel.Paint, AddressOf PaintBusyPanel

            BusyLabel = New Label
            BusyLabel.TextAlign = ContentAlignment.TopCenter
            BusyLabel.Width = BusyPanel.Width - 16
            BusyLabel.Text = My.Resources.ctlWorkQueueContents_FetchingItemsFromDatabase
            BusyLabel.Left = (BusyPanel.Width - BusyLabel.Width) \ 2
            BusyLabel.Top = 1
            BusyLabel.BackColor = Color.Transparent
            BusyLabel.ForeColor = Color.Black
            BusyPanel.Controls.Add(BusyLabel)

            BusyProgressBar = New ProgressBar
            BusyProgressBar.Style = ProgressBarStyle.Marquee
            BusyProgressBar.Width = BusyPanel.Width - 16
            BusyProgressBar.Left = 8
            BusyProgressBar.Height = 12
            BusyProgressBar.Top = (BusyPanel.Height - BusyProgressBar.Height) \ 2
            BusyPanel.Controls.Add(BusyProgressBar)
            BusyProgressBar.BringToFront()
        End If

        Enabled = False

        BusyLabel.Text = My.Resources.ctlWorkQueueContents_FetchingItemsFromDatabase
        BusyProgressBar.Style = ProgressBarStyle.Marquee

        BusyPanel.Left = (Width - BusyPanel.Width) \ 2
        BusyPanel.Top = (Height - BusyPanel.Height) \ 2
        Controls.Add(BusyPanel)
        BusyPanel.BringToFront()

        mBusyPanelBeginTime = DateTime.Now
    End Sub

    ''' <summary>
    ''' The time at which the busy panel was first shown.
    ''' </summary>
    Private mBusyPanelBeginTime As DateTime

    ''' <summary>
    ''' Hides the user feedback message and progress bar, if it is visible.
    ''' </summary>
    Private Sub HideBusyNotification()
        Controls.Remove(BusyPanel)
        Enabled = True
    End Sub

    ''' <summary>
    ''' The background colour used when painting the busy panel.
    ''' </summary>
    Private mBusyPanelBackColour As Color = Color.LightGray

    ''' <summary>
    ''' The brush used to paint the busy panel background.
    ''' </summary>
    Private mBusyPanelBackgroundBrush As New SolidBrush(mBusyPanelBackColour)

    Private Sub PaintBusyPanel(ByVal Sender As Object, ByVal e As PaintEventArgs)
        e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
        e.Graphics.Clear(SystemColors.ButtonFace)

        Dim halfheight As Integer = BusyPanel.Height \ 2
        e.Graphics.FillEllipse(mBusyPanelBackgroundBrush, New Rectangle(Point.Empty, New Size(BusyPanel.Height, BusyPanel.Height)))
        e.Graphics.FillEllipse(mBusyPanelBackgroundBrush, New Rectangle(New Point(BusyPanel.Width - BusyPanel.Height), New Size(BusyPanel.Height, BusyPanel.Height)))
        e.Graphics.FillRectangle(mBusyPanelBackgroundBrush, New Rectangle(New Point(halfheight, 0), New Size(BusyPanel.Width - BusyPanel.Height, BusyPanel.Height)))
    End Sub

    Private mLastQueueId As Guid

    Public Class QueueContentsPayload
        Public Property Id As Guid
        Public Property Filter As WorkQueueFilter
        Public Property Items As IDictionary(Of Long, clsWorkQueueItem)
        Public Property TotalItemCount As Integer
    End Class

    ''' <summary>
    ''' Populates the list with the contents of the specified queue, using the latest
    ''' information from the database.
    ''' </summary>
    ''' <param name="resetToFirstPage">If true, the query will be treated as a fresh
    ''' one, fetching results from the first page; otherwise the existing page
    ''' position will be respected.</param>
    Private Sub PopulateList(ByVal resetToFirstPage As Boolean)
        If Not IsHandleCreated Then mPendingGetContents = True : Return

        If mQueueId = Guid.Empty Then Return
        ' We don't want to deal with getting contents twice for a single pending call
        mPendingGetContents = False
        Try
            Cursor = Cursors.WaitCursor
            mbIgnoreRowsPerPageEvents = True
            Dim sErr As String = Nothing

            Dim filter As WorkQueueFilter = Nothing
            ' Using a dictionary as a hash set to allow quick access to the keys
            Dim selectedItems As Dictionary(Of Long, Object) = Nothing
            If mQueueId = Guid.Empty Then
                mQueueId = Guid.Empty
                mRowsPerPage.TotalRows = 0
                mWorkQueueItems = Nothing
                mLastResults = Nothing
                mLastFilter = Nothing
                lstQueueContents.Items.Clear()
                Return
            End If

            If mQueueId <> mLastQueueId Then
                Dim FilterID As Guid, FilterName As String = Nothing

                If Not gSv.WorkQueueGetDefaultFilter(mQueueId, FilterID, FilterName) Then
                    UserMessage.Show(
                        My.Resources.ctlWorkQueueContents_FailedToRetrieveTheDefaultFilterForThisQueueABlankFilterWillBeUsedInstead & vbCrLf & vbCrLf &
                        String.Format(My.Resources.ctlWorkQueueContents_ErrorMessageWas0, My.Resources.clsServer_NoMatchingQueueFound))
                Else
                    Dim currentFilter As String = CStr(cmbFilterSwitcher.SelectedItem)
                    If Not String.IsNullOrEmpty(FilterName) Then
                        cmbFilterSwitcher.SelectedItem = FilterName
                    Else
                        cmbFilterSwitcher.SelectedItem = Nothing
                        RestoreDefaultFilters()
                    End If
                    If CStr(cmbFilterSwitcher.SelectedItem) <> currentFilter Then
                        'Changing the selected item will cause a cmbFilterSwitcher_SelectedIndexChanged
                        'event to fire which in turn will call PopulateUIWithFilter so we can exit here
                        'because PopulateUIWithFilter causes another refresh
                        Exit Sub
                    End If
                End If

            End If

            mLastQueueId = mQueueId
            filter = GetQueryFilter(resetToFirstPage)

            If resetToFirstPage Then mRowsPerPage.CurrentPage = 1

            ShowBusyNotification()

            ' If already refreshing another queues contents, cancel it. Once the background worker completes it
            ' will run again to get this queue contents
            If bwGetContents.IsBusy Then
                mRepopulateContents = True
                bwGetContents.CancelAsync()
                Return
            End If

            bwGetContents.RunWorkerAsync(
                New QueueContentsPayload() With {.Id = mQueueId, .Filter = filter}
            )
        Catch ex As Exception
            UserMessage.Err(ex, My.Resources.ctlWorkQueueContents_ErrorUpdatingContents0, ex.Message)
            mbIgnoreRowsPerPageEvents = False
            Cursor = Cursors.Default
            HideBusyNotification()
        End Try

    End Sub

    ''' <summary>
    ''' Fetches filtered results from the database and stores results in the
    ''' mFilteredContentsSuccess, mFilteredContentsError, mFilteredContentsRowCount
    ''' and mFilteredContentsResults members.
    ''' </summary>
    Private Sub HandleGetContentsDoWork(
     sender As Object, e As DoWorkEventArgs) Handles bwGetContents.DoWork
        Dim payload = DirectCast(e.Argument, QueueContentsPayload)

        ' Output vars for the call
        Dim total As Integer = 0
        Dim results As ICollection(Of clsWorkQueueItem) = Nothing

        ' Get the contents from the database - populate the output vars
        Try
            gSv.WorkQueuesGetQueueFilteredContents(
                payload.Id, payload.Filter, total, results)
        Catch ex As Exception
            Throw New OperationFailedException(ex.Message)
        End Try

        ' We've done the hard bit - see if we have a pending cancellation
        If bwGetContents.CancellationPending Then e.Cancel = True : Return

        ' Update the payload with the return data
        payload.TotalItemCount = total
        ' Map the returned items against their ident in a new map
        payload.Items = results.ToDictionary(Function(q) q.Ident)

        ' And set the payload into the result
        e.Result = payload

    End Sub

    ''' <summary>
    ''' Handles the completion of the background worker responsible for getting the
    ''' contents of the selected queue
    ''' </summary>
    Private Sub HandleGetContentsCompleted(
     sender As Object, e As RunWorkerCompletedEventArgs) _
     Handles bwGetContents.RunWorkerCompleted
        Try

            Dim ex As Exception = e.Error
            If ex IsNot Nothing Then
                UserMessage.Err(
                    ex, My.Resources.ctlWorkQueueContents_AnErrorOccurredUpdatingTheContents0, ex.Message)
                Return
            End If

            ' If the selected queue has changed while the background worker was getting the last payload
            ' we will need to discard this payload and get the contents of the currently selected queue.
            If mRepopulateContents Then
                mRepopulateContents = False
                PopulateList(True)
                Return
            End If

            If e.Cancelled Then
                Return
            End If

            ' Ensure that any selected items are re-selected
            Dim selectedIdents As New HashSet(Of Long)()
            For Each item As ListViewItem In lstQueueContents.SelectedItems
                selectedIdents.Add(CLng(item.Tag))
            Next

            With DirectCast(e.Result, QueueContentsPayload)
                mWorkQueueItems = .Items
                mLastFilter = .Filter
                mRowsPerPage.TotalRows = .TotalItemCount
            End With

            'cache these results, in case needed for export
            mLastResults = mWorkQueueItems

            ' Part of the fix for #4393 - the current page may be 1 beyond the last page if
            ' all the items on the last page were deleted.
            If mRowsPerPage.CurrentPage > mRowsPerPage.TotalPages Then
                mRowsPerPage.CurrentPage = mRowsPerPage.TotalPages
            End If

            If mWorkQueueItems IsNot Nothing Then
                UpdateList(mWorkQueueItems.Values)
            End If

            ' If we have stored any selected items...
            If selectedIdents IsNot Nothing AndAlso selectedIdents.Count > 0 Then
                ' Go through the list and re-select them.
                For Each Item As ListViewItem In lstQueueContents.Items
                    If selectedIdents.Contains(DirectCast(Item.Tag, Long)) Then
                        Item.Selected = True
                    End If
                Next
            End If
        Finally
            mbIgnoreRowsPerPageEvents = False
            Cursor = Cursors.Default
            HideBusyNotification()
        End Try

    End Sub

    ''' <summary>
    ''' The work queue items keyed on identity.
    ''' </summary>
    Private mWorkQueueItems As IDictionary(Of Long, clsWorkQueueItem)

    ''' <summary>
    ''' Builds a query filter, based on the current UI config, including the currently
    ''' selected search filters in the combo boxes.
    ''' </summary>
    ''' <param name="resetToFirstPage">When true, the filter will be set such that
    ''' the query will fetch results from the first page onwards; otherwise the 
    ''' existing page reference will be respected.</param>
    ''' <returns>Returns a filter object.</returns>
    Private Function GetQueryFilter(ByVal resetToFirstPage As Boolean) As WorkQueueFilter
        Dim fp As New clsWorkQueueFilterBuilder
        For Each c As Control In pnlListviewContainer.Controls
            Dim cmb As ComboBox = TryCast(c, ComboBox)
            If cmb IsNot Nothing Then
                Dim isCustomSearch = String.IsNullOrEmpty(CStr(cmb.SelectedValue))
                Dim text As String = CStr(IIf(isCustomSearch, cmb.Text, cmb.SelectedValue))
                If cmb Is mImageDropDownFilter Then
                    Dim img As Image = TryCast(mImageDropDownFilter.SelectedItem, Image)
                    If img IsNot Nothing Then
                        text = CType(img.Tag, String)
                    Else
                        text = ""
                    End If
                End If
                Dim filterOK As Boolean
                If (text Is Nothing) Then
                    filterOK = False
                Else
                    filterOK = fp.ApplyConstraint(cmb.Name, text)
                End If
                If Not filterOK Then
                    cmb.BackColor = Color.LightPink
                Else
                    cmb.BackColor = Color.White
                    If cmb.SelectedItem Is Nothing AndAlso cmb.Text <> "" Then
                        If Not cmb.Items.Contains(cmb.Text) Then
                            Dim newRange As New DataTable
                            newRange.Columns.Add("Text", GetType(String))
                            newRange.Columns.Add("Value", GetType(String))
                            newRange.Rows.Add(cmb.Text, "")
                            Select Case clsWorkQueueFilterBuilder.GetColumnType(cmb.Name)
                                Case clsWorkQueueFilterBuilder.ColumnTypes.DateAndTimeOfPastEvent,
                                     clsWorkQueueFilterBuilder.ColumnTypes.DateAndTimeOfFutureEvent,
                                     clsWorkQueueFilterBuilder.ColumnTypes.DateAndTimeOfPossibleEvent
                                    newRange.Merge(AddL10nRange(DropdownLists.DateAndTimeDropdown, "Last"))
                                Case clsWorkQueueFilterBuilder.ColumnTypes.IntegerColumn
                                    newRange.Merge(AddL10nRange(DropdownLists.IntergerDropdown, ""))
                                Case Else
                                    newRange.Merge(AddL10nRange(DropdownLists.OtherDropdown, ""))
                            End Select
                            cmb.DataSource = newRange
                        End If
                    End If
                End If
            End If
        Next

        Dim filter As WorkQueueFilter = fp.Filter
        mSortHandler.ApplyOrdering(filter)
        filter.MaxRows = mRowsPerPage.RowsPerPage

        If resetToFirstPage Then
            filter.StartIndex = 0
        Else
            filter.StartIndex = (mRowsPerPage.CurrentPage - 1) * mRowsPerPage.RowsPerPage
        End If

        Return filter
    End Function

    ''' <summary>
    ''' The results last populated into the user interface.
    ''' This corresponds to the search performed using mLastFilter as the filter
    ''' </summary>
    Private mLastResults As IDictionary(Of Long, clsWorkQueueItem)

    ''' <summary>
    ''' The filter used in the last query to fetch data. This should be created
    ''' destroyed, etc at the same time as mLastResults.
    ''' </summary>
    Private mLastFilter As WorkQueueFilter

    ''' <summary>
    ''' Creates a new list view subitem
    ''' </summary>
    ''' <param name="nm">The name to ascribe to the subitem</param>
    ''' <param name="txt">The text to set in the subitem</param>
    ''' <returns>A populated listview subitem</returns>
    Private Function NewSubItem(
     ByVal nm As String, ByVal txt As String) As ListViewSubItem
        Return New ListViewSubItem() With {.Name = nm, .Text = txt}
    End Function

    ''' <summary>
    ''' Updates the list in the UI with the supplied data. Any existing data
    ''' is cleared first.
    ''' </summary>
    ''' <param name="items">The work queue items to be populated.</param>
    Private Sub UpdateList(ByVal items As ICollection(Of clsWorkQueueItem))
        lstQueueContents.BeginUpdate()
        Try
            StopPositionFetcher()
            lstQueueContents.Items.Clear()

            AddPositionColumn()

            If items IsNot Nothing AndAlso items.Count > 0 Then
                BusyLabel.Text = My.Resources.ctlWorkQueueContents_GeneratingListItems
                BusyLabel.Refresh()
                BusyLabel.Left = (BusyPanel.Width - BusyLabel.Width) \ 2
                BusyProgressBar.Style = ProgressBarStyle.Blocks
                BusyProgressBar.Minimum = 0
                BusyProgressBar.Maximum = items.Count
                BusyProgressBar.Value = 0

                Dim listViewItems As New ListView.ListViewItemCollection(lstQueueContents)
                For Each item As clsWorkQueueItem In items

                    Dim siList As New List(Of ListViewItem.ListViewSubItem)
                    Dim si As ListViewItem.ListViewSubItem = NewSubItem(ColumnNames.Icon, "")
                    si.Tag = item.ID
                    siList.Add(si)

                    ' siList.Add(NewSubItem(item.ID.ToString(), ""))    ' The item itself
                    siList.Add(NewSubItem(ColumnNames.Position, item.PositionDisplay))
                    siList.Add(NewSubItem(ColumnNames.ItemKey, item.KeyValue))
                    siList.Add(NewSubItem(ColumnNames.Priority, item.Priority.ToString()))
                    siList.Add(NewSubItem(ColumnNames.Status, item.Status))
                    siList.Add(NewSubItem(ColumnNames.Tags, item.TagString))
                    siList.Add(NewSubItem(ColumnNames.Resource, item.Resource))
                    siList.Add(NewSubItem(ColumnNames.Attempt, item.Attempt.ToString()))
                    siList.Add(NewSubItem(ColumnNames.Created, item.LoadedDisplay))
                    siList.Add(NewSubItem(ColumnNames.LastUpdated, item.LastUpdatedDisplay))
                    siList.Add(NewSubItem(ColumnNames.NextReview, item.DeferredDisplay))
                    siList.Add(NewSubItem(ColumnNames.Completed, item.CompletedDateDisplay))
                    siList.Add(NewSubItem(ColumnNames.TotalWorkTime, item.WorkTimeDisplay))
                    siList.Add(NewSubItem(ColumnNames.ExceptionDate, item.ExceptionDateDisplay))
                    siList.Add(NewSubItem(ColumnNames.ExceptionReason, item.ExceptionReason))

                    Dim lvItem As New ListViewItem(siList.ToArray(), item.CurrentStateImageKey)
                    lvItem.Tag = item.Ident
                    lvItem.Text = ""

                    listViewItems.Add(lvItem)
                    BusyProgressBar.Value += 1
                Next

            End If

            ToggleLinkShowPosition(lstQueueContents.Items.Count > 0)

            If PositionColumnAvailable() _
             Then StartPositionFetcher() _
             Else RemovePositionColumn()

            RefreshListviewSize()

        Finally
            lstQueueContents.EndUpdate()

        End Try
    End Sub

    Private Sub AddPositionColumn()
        'We add this temporarily whilst adding data, then remove it if necessary
        If Not lstQueueContents.Columns.Contains(mPositionColumn) Then
            mPositionColumn.Width = 75

            If lstQueueContents.Columns.Count = 0 Then
                lstQueueContents.Columns.Insert(0, mPositionColumn)
            Else
                lstQueueContents.Columns.Insert(1, mPositionColumn)
            End If

        End If 'We add this temporarily whilst adding data, then remove it if necessary

    End Sub



    ''' <summary>
    ''' Hides the column named "Position", and updates the listview accordingly.
    ''' </summary>
    Private Sub RemovePositionColumn()
        Try
            lstQueueContents.BeginUpdate()

            lstQueueContents.Columns.Remove(mPositionColumn)
            For Each item As ListViewItem In lstQueueContents.Items
                Dim subitem As ListViewSubItem = item.SubItems(ColumnNames.Position)
                If subitem IsNot Nothing Then item.SubItems.Remove(subitem)
            Next

            RefreshListviewSize()
        Finally
            lstQueueContents.EndUpdate()
        End Try
    End Sub

    ''' <summary>
    ''' Shows the column named "Position", and updates the listview accordingly.
    ''' </summary>
    Private Sub ShowPositionColumn()
        Try
            lstQueueContents.BeginUpdate()

            lstQueueContents.Columns.Insert(1, mPositionColumn)
            For Each item As ListViewItem In lstQueueContents.Items
                Dim subitem As ListViewSubItem = item.SubItems(ColumnNames.Position)
                If subitem Is Nothing Then
                    subitem = New ListViewSubItem(item, "")
                    item.SubItems.Insert(1, subitem)
                    subitem.Name = ColumnNames.Position
                End If

                If item.ImageKey = "Ellipsis" Then
                    subitem.Text = "?"
                Else
                    subitem.Text = ""
                End If
            Next
            mPositionColumn.Width = 75

            RefreshListviewSize()
        Finally
            lstQueueContents.EndUpdate()
        End Try
    End Sub

    ''' <summary>
    ''' When true, events from the rows per page control will be ignored.
    ''' See the objRowsPerPage_Changed function.
    ''' </summary>
    Private mbIgnoreRowsPerPageEvents As Boolean = False


    ''' <summary>
    ''' Event handler for the rows per page control.
    ''' </summary>
    Private Sub HandleRowsPerPageChanged() Handles mRowsPerPage.ConfigChanged
        If Not mbIgnoreRowsPerPageEvents Then
            Try
                Cursor = Cursors.WaitCursor
                PopulateList(True)
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.ctlWorkQueueContents_InternalError0, ex.Message), ex)
            Finally
                Cursor = Cursors.Default
            End Try
        End If
    End Sub


    ''' <summary>
    ''' Event handler for the rows per page control.
    ''' </summary>
    Private Sub objRowsPerPage_PageChanged() Handles mRowsPerPage.PageChanged
        If Not mbIgnoreRowsPerPageEvents Then
            Try
                Cursor = Cursors.WaitCursor
                RefreshList()
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.ctlWorkQueueContents_InternalError0, ex.Message), ex)
            Finally
                Cursor = Cursors.Default
            End Try
        End If
    End Sub

    ''' <summary>
    ''' The height of a listview column header
    ''' </summary>
    Const ColumnHeaderHeight As Integer = 21

    ''' <summary>
    ''' The height of a listview row in details mode
    ''' </summary>
    Const ListviewItemHeight As Integer = 14


    ''' <summary>
    ''' Sets the width of the listview to the length of its columns, and the height
    ''' to the full height of its rows.
    ''' This gets rid of the built-in listview scrollbars, and forces the containing
    ''' panel to have scrollbars instead.
    ''' </summary>
    Private Sub RefreshListviewSize()
        'Get the total column width, make the listview slightly wider.
        'This gets rid of the horiz scrollbar
        Dim iTotalColumnWidths As Integer
        For i As Integer = 0 To lstQueueContents.Columns.Count - 1
            iTotalColumnWidths += lstQueueContents.Columns(i).Width
        Next
        lstQueueContents.Width = iTotalColumnWidths + SystemInformation.VerticalScrollBarWidth + 5
        lstQueueContents.Height = Math.Max((lstQueueContents.Items.Count + 1) * ListviewItemHeight + ColumnHeaderHeight, pnlListviewContainer.Height - ComboBoxHeight)

        'Align each combo box to the associated column header
        Dim offset As Integer
        offset = lstQueueContents.Left
        For Each c As ColumnHeader In lstQueueContents.Columns
            Dim d As ComboBox = TryCast(pnlListviewContainer.Controls(c.Name), ComboBox)
            If d IsNot Nothing Then
                d.Left = offset
                d.Width = c.Width - 1
                offset += c.Width
            End If
        Next
    End Sub

    ''' <summary>
    ''' Handles the drawing of the images combo box.
    ''' </summary>
    Private Sub DrawImageComboItem(ByVal sender As Object, ByVal ea As DrawItemEventArgs)
        ea.DrawBackground()
        ea.DrawFocusRectangle()

        Try
            Dim combo As ComboBox = CType(sender, ComboBox)
            Dim itemValue As Object = combo.Items(ea.Index)
            Dim image As Image = TryCast(itemValue, Image)
            If image IsNot Nothing Then
                Dim offset As Integer = Math.Max(0, (ea.Bounds.Height - image.Height) \ 2)
                ea.Graphics.DrawImage(image, ea.Bounds.Left + offset, ea.Bounds.Top + offset, image.Width, image.Height)
            Else
                If TypeOf itemValue Is String Then
                    Dim StringSize As Size = ea.Graphics.MeasureString(CType(itemValue, String), combo.Font).ToSize
                    Dim offset As Integer = Math.Max(0, (ea.Bounds.Height - StringSize.Height) \ 2)
                    ea.Graphics.DrawString(CType(itemValue, String), combo.Font, New SolidBrush(ea.ForeColor), ea.Bounds.Left + offset, ea.Bounds.Top + offset)
                End If
            End If
        Catch ex As Exception
            ea.Graphics.FillRectangle(Brushes.Red, ea.Bounds)
        End Try
    End Sub



    ''' <summary>
    ''' Refreshes the Dropdown, called by the event handler of listviews columnheader
    ''' resize event.
    ''' </summary>
    Private Sub RefreshDropDowns()
        If Not (lstQueueContents Is Nothing OrElse lstQueueContents.Columns.Count = 0) Then
            RefreshListviewSize()
        End If
    End Sub


    Private Sub lstQueueContents_ColumnReordered(ByVal sender As Object, ByVal e As ColumnReorderedEventArgs) Handles lstQueueContents.ColumnReordered
        Try
            RefreshDropDowns()
        Catch ex As Exception
            'do nothing
        End Try
    End Sub

    Private Sub lstQueueContents_ColumnResized(ByVal sender As Object, ByVal e As ColumnWidthChangedEventArgs) Handles lstQueueContents.ColumnWidthChanged
        Try
            RefreshDropDowns()
        Catch ex As Exception
            'do nothing
        End Try
    End Sub

    ''' <summary>
    ''' The desired height of the filter combo boxes.
    ''' </summary>
    Const ComboBoxHeight As Integer = 22

    ''' <summary>
    ''' The image filter receives special treatment, so we require a reference
    ''' at all times.
    ''' </summary>
    Private mImageDropDownFilter As ComboBox

    ''' <summary>
    ''' The combo box filter placed above the "position" column, which is
    ''' sometimes present and sometimes not.
    ''' </summary>
    Private mPositionDropDownFilter As ComboBox

    ''' <summary>
    ''' We create the dropdown filters here; they are populated later on
    ''' </summary>
    Private Sub BuildDropDownFilters()
        lstQueueContents.Top += ComboBoxHeight
        lstQueueContents.Height -= ComboBoxHeight

        Dim offset As Integer = lstQueueContents.Left
        For Each c As ColumnHeader In lstQueueContents.Columns
            Dim d As New System.Windows.Forms.ComboBox

            Select Case c.Name
                Case "Icon"
                    d.DrawMode = DrawMode.OwnerDrawVariable
                    AddHandler d.DrawItem, AddressOf DrawImageComboItem
                    d.DropDownStyle = ComboBoxStyle.DropDownList
                    mImageDropDownFilter = d
                Case "Position"
                    d.DropDownStyle = ComboBoxStyle.DropDown
                    mPositionDropDownFilter = d
                    mPositionDropDownFilter.DropDownStyle = ComboBoxStyle.DropDownList 'we can't filter on this, because it is not fetched from database as part of main query.
                Case "Exception Reason"
                    d.DropDownStyle = ComboBoxStyle.DropDownList 'we don't allow filtering on this column
                Case Else
                    d.DropDownStyle = ComboBoxStyle.DropDown
            End Select

            d.Anchor = AnchorStyles.Top Or AnchorStyles.Left
            d.Left = offset
            d.Width = c.Width - 1
            d.Height = ComboBoxHeight
            d.Top = lstQueueContents.Top - ComboBoxHeight
            d.Name = c.Name
            pnlListviewContainer.Controls.Add(d)
            d.BringToFront()
            offset += c.Width
        Next
    End Sub

    Private Sub SetDefaultFilterValue(ByVal Combo As ComboBox)
        'All combos have default "all" for now
        Combo.Text = My.Resources.ctlWorkQueueContents_All
    End Sub

    ''' <summary>
    ''' Populates the drop-down filters with some sensible defaults, and some
    ''' suggested alternatives, to coax the user into experimenting with their
    ''' own values.
    ''' </summary>
    Private Sub PopulateDropDownFilters()
        For Each objControl As Control In pnlListviewContainer.Controls
            If TypeOf objControl Is ComboBox Then
                Dim c As ComboBox = CType(objControl, ComboBox)
                Select Case clsWorkQueueFilterBuilder.GetColumnType(c.Name)
                    Case clsWorkQueueFilterBuilder.ColumnTypes.DateAndTimeOfPastEvent
                        c.DisplayMember = "Text"
                        c.ValueMember = "Value"
                        c.DataSource = AddL10nRange(DropdownLists.DateAndTimeDropdown, "Last")
                        SetDefaultFilterValue(c)
                    Case clsWorkQueueFilterBuilder.ColumnTypes.DateAndTimeOfFutureEvent
                        c.DisplayMember = "Text"
                        c.ValueMember = "Value"
                        c.DataSource = AddL10nRange(DropdownLists.DateAndTimeDropdown, "Next")
                        SetDefaultFilterValue(c)
                    Case clsWorkQueueFilterBuilder.ColumnTypes.DateAndTimeOfPossibleEvent
                        c.DisplayMember = "Text"
                        c.ValueMember = "Value"
                        c.DataSource = AddL10nRange(DropdownLists.DateAndTimeDropdown, "Last")
                        SetDefaultFilterValue(c)
                    Case clsWorkQueueFilterBuilder.ColumnTypes.IntegerColumn
                        c.DisplayMember = "Text"
                        c.ValueMember = "Value"
                        If c IsNot mPositionDropDownFilter Then
                            c.DataSource = AddL10nRange(DropdownLists.IntergerDropdown, "")
                        Else
                            c.DataSource = AddL10nRange(DropdownLists.OtherDropdown, "")
                        End If
                        SetDefaultFilterValue(c)
                    Case clsWorkQueueFilterBuilder.ColumnTypes.ImageColumn
                        c.Items.Clear()
                        c.Items.Add(My.Resources.ctlWorkQueueContents_All)
                        c.DropDownStyle = ComboBoxStyle.DropDownList
                        For Each key As String In ImageKeys
                            c.Items.Add(GetImage(key))
                        Next
                        SetDefaultFilterValue(c)
                    Case Else
                        c.DisplayMember = "Text"
                        c.ValueMember = "Value"
                        c.DataSource = AddL10nRange(DropdownLists.OtherDropdown, "")
                        SetDefaultFilterValue(c)
                End Select

                AddHandler c.SelectedIndexChanged, AddressOf FilterIndexChanged
                AddHandler c.KeyDown, AddressOf FilterKeyDown
            End If
        Next
    End Sub

    ''' <summary>
    ''' Creates localized DataTable for Filter ComboBoxes. 
    ''' </summary>
    ''' <returns>Localized DataTable</returns>
    ''' <param name="range">ComboBox AddRange strings</param>
    Private Function AddL10nRange(ByVal range As String(), ByVal prefix As String) As DataTable
        Dim tb As New DataTable
        tb.Columns.Add("Text", GetType(String))
        tb.Columns.Add("Value", GetType(String))
        For Each value As String In range
            Dim resxKey As String = "ctlWorkQueueContents_" & If(value.Equals("All") OrElse value.Equals("Today"), "", prefix) & Regex.Replace(value, "\s*", "")
            value = CStr(IIf(value.Equals("All") OrElse value.Equals("Today"), value, CStr(IIf(prefix.Equals(""), value, prefix & " " & value))))
            Dim localizedText As String = My.Resources.ResourceManager.GetString(resxKey, My.Resources.Culture)
            If (localizedText Is Nothing) Then
                localizedText = value
            End If
            tb.Rows.Add(localizedText, value)
        Next
        Return tb
    End Function

    ''' <summary>
    ''' A list of the different image keys available
    ''' </summary>
    Private Shared ImageKeys As String() = {"Tick", "Padlock", "Person", "Ellipsis"}

    ''' <summary>
    ''' Retrieves the image associated with the supplied key.
    ''' </summary>
    ''' <param name="imageKey">The key of the image desired.</param>
    ''' <returns>Returns the image associated with the supplied key.</returns>
    Private Function GetImage(ByVal imageKey As String) As Image
        Dim img As Image = Nothing
        Select Case imageKey
            Case "Tick"
                img = ToolImages.Tick_16x16
            Case "Padlock"
                img = ToolImages.Lock_16x16
            Case "Person"
                img = ToolImages.Flag_Purple_16x16
            Case "Ellipsis"
                img = ToolImages.Custom_Pending_16x16
            Case Else
                Throw New ArgumentException(My.Resources.ctlWorkQueueContents_InvalidImageKey)
        End Select
        img.Tag = imageKey
        Return img
    End Function

    ''' <summary>
    ''' When true, event handlers for index changed on filter combos will be
    ''' ignored.
    ''' </summary>
    Private mIgnoreFilterComboChanges As Boolean = False
    Private mRepopulateContents As Boolean

    ''' <summary>
    ''' SelectedIndexChanged Event Handler for the Filters
    ''' </summary>
    Private Sub FilterIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        If Not mIgnoreFilterComboChanges Then
            If Not CType(sender, ComboBox).DroppedDown Then
                PopulateList(True)
            End If
        End If
    End Sub

    ''' <summary>
    ''' KeyDown Event Handler for the Filters
    ''' </summary>
    Private Sub FilterKeyDown(ByVal sender As Object, ByVal e As KeyEventArgs)
        If e.KeyCode = Keys.Enter Then PopulateList(True)
    End Sub

    Protected Overrides Sub OnLoad(ByVal e As EventArgs)
        MyBase.OnLoad(e)
        If Not DesignMode Then
            mParent = TryCast(GetAncestorOfType(Me, GetType(frmApplication)), frmApplication)

            RepopulateFilterList("")

            'The events here trigger the population of the list
            mRowsPerPage.MaxRows = 10000
            mRowsPerPage.RowsPerPage = 100

            If mPendingGetContents Then
                mPendingGetContents = False
                PopulateList(True)
            End If
        End If
    End Sub

#Region "Context Menu Stuff"

    ''' <summary>
    ''' Gets the work queue item object which corresponds to the given listview item.
    ''' </summary>
    ''' <param name="viewItem">The list view item for which the work queue item is
    ''' required.</param>
    ''' <returns>The work queue item corresponding to the specified list view item.
    ''' </returns>
    Private Function CorrespondingQueueItem(ByVal viewItem As ListViewItem) As clsWorkQueueItem
        Return mWorkQueueItems(CType(viewItem.Tag, Long))
    End Function

    ''' <summary>
    ''' Structure to hold the state of the selected items in the work queue contents
    ''' list
    ''' </summary>
    Private Structure SelectedItemsState

        ''' <summary>
        ''' Flag which is set if any of the selected items are pending (ie. unlocked
        ''' and unworked)
        ''' </summary>
        Public AnyPending As Boolean

        ''' <summary>
        ''' Flag which is set if any of the selected items are locked.
        ''' </summary>
        Public AnyLocked As Boolean

        ''' <summary>
        ''' Flag which is set if any of the selected items have been completed.
        ''' </summary>
        Public AnyCompleted As Boolean

        ''' <summary>
        ''' Flag which is set if any of the selected items have exceptioned.
        ''' </summary>
        Public AnyExceptioned As Boolean

        ''' <summary>
        ''' Flag which is set if any of the flags in this structure are set, 
        ''' indicating that any items have been selected.
        ''' </summary>
        Public ReadOnly Property AnySelected() As Boolean
            Get
                Return AnyPending OrElse AnyLocked OrElse AnyCompleted OrElse AnyExceptioned
            End Get
        End Property

        ''' <summary>
        ''' Flag which is set if any of the selected items have finished, either
        ''' having completed successfully or exceptioned.
        ''' </summary>
        Public ReadOnly Property AnyFinished() As Boolean
            Get
                Return AnyCompleted OrElse AnyExceptioned
            End Get
        End Property

        ''' <summary>
        ''' Flag which is set if all selected items (of which there is at least one)
        ''' are in a pending (and unlocked) state.
        ''' </summary>
        Public ReadOnly Property AllPending() As Boolean
            Get
                Return AnyPending AndAlso Not AnyLocked AndAlso Not AnyFinished
            End Get
        End Property

        ''' <summary>
        ''' Flag which is set if all selected items (of which there is at least one)
        ''' are in a locked state. 
        ''' </summary>
        Public ReadOnly Property AllLocked() As Boolean
            Get
                Return AnyLocked AndAlso Not AnyPending AndAlso Not AnyFinished
            End Get
        End Property

        ''' <summary>
        ''' Flag which is set if all selected items (of which there is at least one)
        ''' are finished, having either completed successfully or exceptioned
        ''' </summary>
        Public ReadOnly Property AllFinished() As Boolean
            Get
                Return AnyFinished AndAlso Not AnyPending AndAlso Not AnyLocked
            End Get
        End Property

        ''' <summary>
        ''' Flag which is set if all selected items (of which there is at least one)
        ''' have completed
        ''' </summary>
        Public ReadOnly Property AllCompleted() As Boolean
            Get
                Return AnyCompleted AndAlso Not AnyPending AndAlso Not AnyLocked AndAlso Not AnyExceptioned
            End Get
        End Property

        ''' <summary>
        ''' Flag which is set if all selected items (of which there is at least one)
        ''' have exceptioned
        ''' </summary>
        Public ReadOnly Property AllExceptioned() As Boolean
            Get
                Return AnyExceptioned AndAlso Not AnyPending AndAlso Not AnyLocked AndAlso Not AnyCompleted
            End Get
        End Property

    End Structure

    ''' <summary>
    ''' Builds a context menu appropriate to the current view, and associates
    ''' it with the listview.
    ''' </summary>
    Private Sub BuildContextMenu()
        Dim cm As New ContextMenuStrip
        Dim item As ToolStripItem
        ' The focused (single) item - differs from the selected items (though it
        ' should be contained by them) - there for options which only act on a 
        ' single item (View Log; Copy Key)
        Dim focusedItem As ListViewItem = lstQueueContents.FocusedItem
        Dim focusedWorkQueueItem As clsWorkQueueItem = Nothing
        If focusedItem IsNot Nothing Then focusedWorkQueueItem = CorrespondingQueueItem(focusedItem)

        Dim state As New SelectedItemsState()
        For Each qitem As clsWorkQueueItem In GetSelectedItems()
            Select Case qitem.CurrentState
                Case clsWorkQueueItem.State.Pending, clsWorkQueueItem.State.Deferred
                    state.AnyPending = True
                Case clsWorkQueueItem.State.Locked
                    state.AnyLocked = True
                Case clsWorkQueueItem.State.Completed
                    state.AnyCompleted = True
                Case clsWorkQueueItem.State.Exceptioned
                    state.AnyExceptioned = True
            End Select
        Next

        ' Quick list of the context items and the shortcut letters (in letter order):
        ' A : Select All
        ' C : Copy Item Key
        ' D : Delete
        ' E : Export
        ' F : Refresh View
        ' H : Help
        ' L : View Log
        ' M : Mark with Exception
        ' N : Change Deferral Date
        ' R : Force Retry
        ' S : Edit Status
        ' U : Unlock
        ' V : View All Retries

        'Add refresh item
        item = cm.Items.Add(My.Resources.ctlWorkQueueContents_ReFreshView, ToolImages.Refresh_16x16, AddressOf OnRefreshClicked)

        'Add select all item
        item = cm.Items.Add(My.Resources.ctlWorkQueueContents_SelectAll, Nothing, AddressOf OnSelectAllClicked)

        cm.Items.Add(New ToolStripSeparator)

        Dim hasPermission = User.Current.HasPermission("Full Access to Queue Management")
        'Add unlock item
        item = cm.Items.Add(My.Resources.ctlWorkQueueContents_UnlockSelectedCaseS, ToolImages.Unlock_16x16, AddressOf OnUnlockClicked)
        item.Enabled = state.AllLocked AndAlso hasPermission

        'Add exception item - only available for pending cases
        item = cm.Items.Add(My.Resources.ctlWorkQueueContents_MarkSelectedCaseSWithException, AuthImages.User_Blue_16x16, AddressOf OnExceptionClicked)
        item.Enabled = state.AllPending AndAlso hasPermission

        'Add delete item - available for worked cases only
        item = cm.Items.Add(My.Resources.ctlWorkQueueContents_DeleteWorkedCaseSFromSelection, ToolImages.Delete_Red_16x16, AddressOf OnClearClicked)
        item.Enabled = state.AnyFinished AndAlso hasPermission

        item = cm.Items.Add(My.Resources.ctlWorkQueueContents_ForceRetry, ToolImages.Redo_16x16, AddressOf OnForceRetryClicked)
        item.Enabled = state.AllExceptioned AndAlso hasPermission

        cm.Items.Add(New ToolStripSeparator)

        If state.AnySelected Then

            ' you can only change the deferral date on pending items.
            If state.AllPending Then
                item = cm.Items.Add(My.Resources.ctlWorkQueueContents_ChaNgeDeferralDate, Nothing, AddressOf OnChangeDeferralDateClicked)
                item.Enabled = hasPermission
            End If

            ' Since status has no effect on workflow, you can change status on any 
            ' work queue item.
            item = cm.Items.Add(My.Resources.ctlWorkQueueContents_EditStatus, Nothing, AddressOf OnEditStatusClicked)
            item.Enabled = hasPermission

            cm.Items.Add(New ToolStripSeparator)

        End If

        If focusedWorkQueueItem IsNot Nothing Then

            cm.Items.Add(My.Resources.ctlWorkQueueContents_ViewAllAttemptsOfThisItem, Nothing, AddressOf OnViewAllAttempts)
            If Not String.IsNullOrEmpty(focusedWorkQueueItem.KeyValue) Then
                cm.Items.Add(My.Resources.ctlWorkQueueContents_CopyItemKeyToClipboard, Nothing, AddressOf OnCopyKeyClicked)
            End If

        End If


        'Add export item
        item = cm.Items.Add(My.Resources.ctlWorkQueueContents_ExportCurrentViewAsReport, Nothing, AddressOf OnExportClicked)
        If mLastResults Is Nothing Then item.Enabled = False

        item = cm.Items.Add(My.Resources.ctlWorkQueueContents_ViewLog, Nothing, AddressOf OnViewLogClicked)
        If Not state.AnySelected Then item.Enabled = False
        If Not User.Current.HasPermission("Audit - Process Logs") Then
            item.Enabled = False
            item.ToolTipText =
             My.Resources.ctlWorkQueueContents_YouDoNotHaveTheAppropriatePermissionsToViewTheLogsForThisSession
        End If

        cm.Items.Add(New ToolStripSeparator)

        'Add help item
        item = cm.Items.Add(My.Resources.ctlWorkQueueContents_Help, ToolImages.Help_16x16, AddressOf OnHelpClicked)

        ContextMenuStrip = cm
    End Sub

    Private Sub lstQueueContents_Mouseleave(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstQueueContents.MouseLeave
        'Sometimes we get a false alarm, especially on first column
        Dim ListviewScreenBounds As Rectangle = lstQueueContents.RectangleToScreen(lstQueueContents.Bounds)
        If Not ListviewScreenBounds.Contains(System.Windows.Forms.Cursor.Position) Then
            mTooltip.Hide(lstQueueContents)
        End If
    End Sub


    Private Sub lstQueueContents_MouseHover(ByVal sender As Object, ByVal e As MouseEventArgs) Handles lstQueueContents.MouseMove
        Try
            Dim tooltipSet As Boolean
            Dim localPoint As Point = lstQueueContents.PointToClient(System.Windows.Forms.Cursor.Position)
            Dim info As ListViewHitTestInfo = lstQueueContents.HitTest(localPoint)
            Dim item As ListViewItem = info.Item
            Dim subItem As ListViewSubItem = info.SubItem
            If item IsNot Nothing AndAlso subItem IsNot Nothing Then

                Dim msg As String = Nothing
                Dim queueItem As clsWorkQueueItem = CorrespondingQueueItem(item)
                Dim colName As String = subItem.Name

                Select Case colName

                    Case ColumnNames.Icon
                        ' If info.SubItem Is info.Item.SubItems(0) Then
                        Select Case info.Item.ImageKey
                            Case "Padlock"
                                msg = String.Format(
                                 My.Resources.ctlWorkQueueContents_LockedThisCaseWasLockedOn0, queueItem.LockedDisplay)
                            Case "Tick"
                                msg = String.Format(
                                 My.Resources.ctlWorkQueueContents_CompletedThisCaseWasSuccessfullyCompletedOn0,
                                 queueItem.CompletedDateDisplay)
                            Case "Person"
                                msg = String.Format(
                                 My.Resources.ctlWorkQueueContents_ExceptionThisCaseMustBeProcessedManually2ItWasMarkedWithAnExceptionOn0WithTheMe,
                                 queueItem.ExceptionDateDisplay, queueItem.ExceptionReason, vbCrLf)
                            Case "Ellipsis"
                                msg = My.Resources.ctlWorkQueueContents_PendingThisCaseIsYetToBeWorked
                        End Select

                    Case ColumnNames.Tags
                        Dim sb As New StringBuilder()
                        Dim sep As String = Nothing
                        For Each tag As String In queueItem.Tags
                            If sep Is Nothing Then sep = vbCrLf Else sb.Append(sep)
                            sb.Append(tag)
                        Next
                        If sb.Length > 0 Then msg = sb.ToString()

                End Select

                If msg IsNot Nothing Then
                    tooltipSet = True
                    Dim textDifferent As Boolean = msg <> mTooltip.GetToolTip(lstQueueContents)
                    Static LastToolTipLocation As Point

                    Dim bounds As Rectangle = subItem.Bounds
                    ' The first subitem represents the whole item, so (obviously) its bounds
                    ' encompass the whole item. This is apparently on purpose (mad. quite mad)
                    ' So we have hoops to jump through if that's the case.
                    If bounds = item.Bounds Then
                        ' It's only the width that's affected, and it's always the first
                        ' column, so we can use this safely.
                        bounds.Width = lstQueueContents.Columns(0).Width
                    End If
                    Dim locn As Point = New Point(
                     bounds.Left + bounds.Width + 2, bounds.Top + 2)

                    If textDifferent OrElse locn <> LastToolTipLocation Then
                        LastToolTipLocation = locn
                        mTooltip.Show(msg, lstQueueContents, locn)
                    End If
                End If

            End If

            If Not tooltipSet Then
                mTooltip.Hide(lstQueueContents)
            End If
        Catch

        End Try
    End Sub

    ''' <summary>
    ''' Retrieves the tooltip to be displayed for the named column.
    ''' </summary>
    ''' <param name="columnName">The name of the column of interest.</param>
    ''' <returns>Returns the text suitable for display in a tooltip
    ''' for the named column, which may be empty but never null.</returns>
    Private Function GetTooltipForColumn(ByVal columnName As String) As String
        Select Case columnName
            Case "Position"
                Return My.Resources.ctlWorkQueueContents_TheOrderInWhichPendingCasesAreToBeWorked
            Case "Exception"
                Return My.Resources.ctlWorkQueueContents_TheDateAndTimeAtWhichAnExceptionLastOccurredForThisCaseIfAny
            Case "Attempt"
                Return My.Resources.ctlWorkQueueContents_TheAttemptNumberRepresentedByThisEntry
            Case "Status"
                Return My.Resources.ctlWorkQueueContents_TheStatusOfTheCaseSProgressAsDefinedInTheRelevantProcessDiagram
            Case "Tags"
                Return My.Resources.ctlWorkQueueContents_TheTagsWhichAreCurrentlyAppliedToThisCase
            Case "Created"
                Return My.Resources.ctlWorkQueueContents_TheDateAndTimeAtWhichThisCaseWasCreatedAndLoadedIntoTheWorkQueue
            Case "Last Updated"
                Return My.Resources.ctlWorkQueueContents_TheDateAndTimeAtWhichThisCaseWasLastUpdatedMeaningOneOfCreatedLockedExceptionSe
            Case "Next Review"
                Return My.Resources.ctlWorkQueueContents_TheEarliestDateAndTimeAtWhichThisCaseWillNextBePickedFromTheQueueForWorkingOrBl
            Case "Exception Reason"
                Return My.Resources.ctlWorkQueueContents_TheReasonGivenByTheProcessWhichSetAnExceptionIfAny
            Case "Item Key"
                Return My.Resources.ctlWorkQueueContents_TheKeyValueForThisItem
            Case "Completed"
                Return My.Resources.ctlWorkQueueContents_TheDateAndTimeAtWhichThisCaseWasCompletedIfAppropriate
            Case "Total Work Time"
                Return My.Resources.ctlWorkQueueContents_TheTotalTimeSpentWorkingOnThisCaseAggregatedAcrossAllAttemptsUpToAndIncludingTh
            Case Else
                Return ""
        End Select
    End Function


    Private Sub lstQueueContents_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles lstQueueContents.MouseUp
        If e.Button = System.Windows.Forms.MouseButtons.Right Then
            Try
                If ContextMenuStrip Is Nothing Then
                    BuildContextMenu()
                    ContextMenuStrip.Show(lstQueueContents, e.Location)
                    ContextMenuStrip = Nothing
                End If
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.ctlWorkQueueContents_UnexpectedError0, ex.Message), ex)
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Event handler for the context menu "refresh" option
    ''' </summary>
    Private Sub OnRefreshClicked(ByVal sender As Object, ByVal e As EventArgs)
        RefreshList()
    End Sub

    ''' <summary>
    ''' Event handler for the context menu "select all" option
    ''' </summary>
    Private Sub OnSelectAllClicked(ByVal sender As Object, ByVal e As EventArgs)
        Try
            For Each i As ListViewItem In lstQueueContents.Items
                i.Selected = True
            Next
            lstQueueContents.Select()
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlWorkQueueContents_UnexpectedError0, ex.Message), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Event handler for the context menu "unlock" option
    ''' </summary>
    Private Sub OnUnlockClicked(ByVal sender As Object, ByVal e As EventArgs)
        If User.Current.HasPermission(Permission.ControlRoom.ManageQueuesFullAccess) Then
            Try
                Dim sErr As String = Nothing
                Dim selectedItems As ICollection(Of clsWorkQueueItem) = GetSelectedItems()
                For Each workQueueItem As clsWorkQueueItem In selectedItems
                    Try
                        gSv.WorkQueueUnlockItem(workQueueItem, mQueueId)
                    Catch ex As Exception
                        UserMessage.Show(
                         String.Format(My.Resources.ctlWorkQueueContents_FailedToUnlockSelectedItemsDueToADatabaseError0, ex.Message), ex)

                    End Try
                Next
                RefreshList()
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.ctlWorkQueueContents_UnexpectedError0, ex.Message), ex)
            End Try
        Else
            ShowPermissionMessage()
        End If
    End Sub

    ''' <summary>
    ''' Gets the ancestor control of the required type from the given control
    ''' </summary>
    ''' <param name="ctrl">The control whose parent is required</param>
    ''' <param name="t">The type of control required</param>
    ''' <returns>The first control in the ancestry tree which matches the given
    ''' type.</returns>
    Private Function GetAncestorOfType(ByVal ctrl As Control, ByVal t As Type) As Control

        If ctrl Is Nothing OrElse ctrl.GetType().IsAssignableFrom(t) Then
            Return ctrl
        End If
        Return GetAncestorOfType(ctrl.Parent, t)

    End Function

    ''' <summary>
    ''' Refreshes the work queue list which is displaying the list of work queues.
    ''' There is no direct link, so this traverses the ancestor components for the
    ''' control room and uses the instance it finds on there.
    ''' If this component has not been added to a control room component, this no-ops
    ''' </summary>
    Private Sub RefreshQueueList()
        ' VS's handling of multiple lines in VB.net is a horror.
        Dim ctrlRoom As ctlWorkQueueManagement =
         TryCast(GetAncestorOfType(Parent, GetType(ctlWorkQueueManagement)), ctlWorkQueueManagement)

        If ctrlRoom IsNot Nothing Then
            ctrlRoom.mWorkQueueList.RefreshList()
        End If
    End Sub

    ''' <summary>
    ''' Event handler for the context menu "clear" option
    ''' </summary>
    Private Sub OnClearClicked(ByVal sender As Object, ByVal e As EventArgs)
        If User.Current.HasPermission(Permission.ControlRoom.ManageQueuesFullAccess) Then
            Try
                Dim selectedItems As IList(Of clsWorkQueueItem) = GetSelectedItems()
                gSv.WorkQueueClearWorked(mQueueId, selectedItems, "", True)
                RefreshQueueList()
                RefreshList()
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.ctlWorkQueueContents_UnexpectedError0, ex.Message), ex)
            End Try
        Else
            ShowPermissionMessage()
        End If
    End Sub

    ''' <summary>
    ''' Handles the 'force retry' option being clicked.
    ''' </summary>
    Private Sub OnForceRetryClicked(ByVal sender As Object, ByVal e As EventArgs)
        If User.Current.HasPermission(Permission.ControlRoom.ManageQueuesFullAccess) Then
            Try
                Dim selectedItems As IList(Of clsWorkQueueItem) = GetSelectedItems()
                Dim invalidStates As ICollection(Of String) =
                    gSv.WorkQueueForceRetry(selectedItems, mQueueId, True)
                If invalidStates.Count > 0 Then
                    UserMessage.Show(My.Resources.ctlWorkQueueContents_TheFollowingWorkItemsCouldNotBeProcessedForTheGivenReasons & vbCrLf & CollectionUtil.Join(invalidStates, vbCrLf))
                Else
                    RefreshQueueList()
                    RefreshList()
                End If
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.ctlWorkQueueContents_UnexpectedError0, ex.Message), ex)
            End Try
        Else
            ShowPermissionMessage()
        End If

    End Sub

    ''' <summary>
    ''' Event handler for the context menu "Mark Exception" option
    ''' </summary>
    Private Sub OnExceptionClicked(ByVal sender As Object, ByVal e As EventArgs)
        If User.Current.HasPermission(Permission.ControlRoom.ManageQueuesFullAccess) Then
            Try
                Dim resp As frmSavePrompt.Response = frmSavePrompt.ShowPrompt(mParent,
                New OkCancelSavePromptConfig(
                  formText:=My.Resources.ctlWorkQueueContents_ExceptionSetter,
                  headingText:=My.Resources.ctlWorkQueueContents_MarkCasesWithException,
                  subheadingText:=My.Resources.ctlWorkQueueContents_TheSelectedCasesWillBePreventedFromBeingWorked,
                  promptText:=My.Resources.ctlWorkQueueContents_PleaseProvideAnExceptionReasonForTheSelectedItems))

                If resp.Result = DialogResult.OK Then
                    Dim sErr As String = Nothing
                    Dim selectedItems As IList(Of clsWorkQueueItem) = GetSelectedItems()
                    Dim retryCount As Integer = 0
                    Try
                        gSv.WorkQueueMarkException(
                            Guid.Empty, selectedItems, resp.Text, False,
                            False, retryCount, mQueueId, True)
                    Catch ex As Exception
                        UserMessage.Show(String.Format(My.Resources.ctlWorkQueueContents_DatabaseError10, ex.Message), ex)
                    End Try
                    RefreshQueueList()
                    RefreshList()
                End If
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.ctlWorkQueueContents_UnexpectedError0, ex.Message), ex)
            End Try
        Else
            ShowPermissionMessage()
        End If
    End Sub

    ''' <summary>
    ''' Handles the 'change deferral date' action on the context menu
    ''' Currently unconstrained, it will change any and all items that
    ''' are currently selected.
    ''' </summary>
    Private Sub OnChangeDeferralDateClicked(ByVal sender As Object, ByVal e As EventArgs)
        If User.Current.HasPermission(Permission.ControlRoom.ManageQueuesFullAccess) Then
            Try
                Dim selectedItems As IList(Of clsWorkQueueItem) = GetSelectedItems()

                Dim dtChooser As New frmDateTimeChooser(
                 DateTime.Now.AddMinutes(1),
                 frmDateTimeChooser.ValidationRule.AfterNow)
                dtChooser.ShowInTaskbar = False
                ' If user didn't cancel, pick out the date/time and attempt to set it.
                If dtChooser.ShowDialog() = DialogResult.OK Then
                    Try
                        gSv.WorkQueueDefer(selectedItems, dtChooser.ChosenDateTime.ToUniversalTime(), QueueId, True)

                    Catch iwqise As InvalidWorkItemStateException
                        UserMessage.Show(iwqise.Message)

                    Catch ex As Exception
                        UserMessage.Show(My.Resources.ctlWorkQueueContents_FailedToDeferWorkQueueItems & ex.Message, ex)
                    End Try
                End If

                RefreshQueueList()
                RefreshList()
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.ctlWorkQueueContents_UnexpectedError0, ex.Message), ex)
            End Try
        Else
            ShowPermissionMessage()
        End If
    End Sub

    ''' <summary>
    ''' Handler for the 'Edit Status' context menu option being clicked.
    ''' This accepts a new status and sets it into the selected items on the database.
    ''' It then refreshes the list so that the new status should be displayed.
    ''' </summary>
    Private Sub OnEditStatusClicked(ByVal sender As Object, ByVal e As EventArgs)

        Dim sel As IList(Of clsWorkQueueItem) = GetSelectedItems()
        If sel.Count = 0 Then Return ' Nothing selected? Go hang.

        ' If there's only one item selected, use its status as the init value
        Dim defaultStatus As String = ""
        If sel.Count = 1 Then defaultStatus = sel(0).Status

        ' Capture the new status.
        Dim status As String = frmInputBox.GetText(
         My.Resources.ctlWorkQueueContents_PleaseProvideTheNewStatus, My.Resources.ctlWorkQueueContents_WorkQueueItemStatus, defaultStatus)

        If status IsNot Nothing Then ' Nothing = Cancelled.
            ' Okay, then update the status 
            Try
                gSv.WorkQueueUpdateStatus(sel, status)
                RefreshQueueList()
                RefreshList()

            Catch aoore As ArgumentOutOfRangeException
                UserMessage.Show(My.Resources.ctlWorkQueueContents_ErrorTheGivenStatusIsTooLong, aoore)

            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.ctlWorkQueueContents_DatabaseError20, ex.Message), ex)

            End Try
        End If

    End Sub

    ''' <summary>
    ''' Shows a message to the user, explaining that they have been denied permission
    ''' to perform an action.
    ''' </summary>
    Private Sub ShowPermissionMessage()
        UserMessage.Show(String.Format(My.Resources.ctlWorkQueueContents_YouDoNotHavePermissionToPerformThatActionIfYouBelieveThatThisIsIncorrectThenPle, ApplicationProperties.ApplicationName), 1048586)
    End Sub

    ''' <summary>
    ''' Event handler for the context menu "help" option
    ''' </summary>
    Private Sub OnHelpClicked(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Cursor = Cursors.WaitCursor
            Try
                OpenHelpFile(Me, GetHelpFile)
            Catch
                UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
            End Try
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlWorkQueueContents_UnexpectedError0, ex.Message), ex)
        Finally
            Cursor = Cursors.Default
        End Try
    End Sub

    ''' <summary>
    ''' Event handler for the context menu "View all retry attempts" option
    ''' </summary>
    Private Sub OnViewAllAttempts(ByVal sender As Object, ByVal e As EventArgs)
        Dim instanceList As New ctlWorkQueueInstanceList(
         CorrespondingQueueItem(lstQueueContents.FocusedItem).ID)
        Dim frm As New WorkQueueItemAttemptViewer(instanceList)
        frm.Show()
    End Sub

    ''' <summary>
    ''' Handler for the 'Copy Key Item to clipboard' menu item selection event
    ''' </summary>
    Private Sub OnCopyKeyClicked(ByVal sender As Object, ByVal e As EventArgs)

        Dim text As String = CorrespondingQueueItem(lstQueueContents.FocusedItem).KeyValue
        Clipboard.SetText(text)
        UserMessage.ShowFloating(Me, Nothing, My.Resources.ctlWorkQueueContents_ItemKeySaved,
        String.Format(My.Resources.ctlWorkQueueContents_Key0SavedToTheClipboard, text), New Point(50, 50), 1500)

    End Sub

    ''' <summary>
    ''' Event handler for the context menu "export" option
    ''' </summary>
    Private Sub OnExportClicked(ByVal sender As Object, ByVal e As EventArgs)
        If mLastResults Is Nothing Then
            UserMessage.Show(My.Resources.ctlWorkQueueContents_NoQueuesToCreateReportFor)
            Return
        End If

        Dim f As frmWorkQueueViewExport = Nothing
        Try
            f = New frmWorkQueueViewExport
            f.QueueID = mQueueId
            f.WorkQueueItems = mLastResults.Values
            f.QueryFilter = mLastFilter
            f.SetEnvironmentColoursFromAncestor(mParent)
            f.ShowDialog()
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlWorkQueueContents_UnexpectedError0, ex.Message), ex)
        Finally
            If f IsNot Nothing Then f.Dispose()
            Cursor = Cursors.Default
        End Try
    End Sub

    ''' <summary>
    ''' Event handler for the context menu "view log" option
    ''' </summary>
    Private Sub OnViewLogClicked(ByVal sender As Object, ByVal e As EventArgs)

        ' 1) Get the session ID which worked this item
        ' 2) Find out if the session still exists on the database (there's no
        '    referential integrity enforced by the database for this one)
        '    [Note: Done for us in the GetValidSessionId() method]
        ' 3) Get the nearest time before the action time of the item.
        '    [Note: Again, done at the database level - the seq no is returned]

        ' Okay, parte the first - get the session ID.
        Dim item As ListViewItem = lstQueueContents.FocusedItem
        Dim seqNo As Integer = -1
        Try
            Dim sessionId As Guid =
             gSv.WorkQueueGetValidSessionId(CType(item.Tag, Long), seqNo)

            If sessionId <> Guid.Empty Then
                ' it still exists, so create the log viewer.
                ' The sequence number from the database should match up to that
                ' defined in the log viewer form.
                Dim logView As New frmLogViewer(sessionId, seqNo)
                If mParent IsNot Nothing Then mParent.StartForm(logView)

            Else
                UserMessage.Show(String.Format(
                 My.Resources.ctlWorkQueueContents_ThereIsNoSessionLogAssociatedWithTheItemWithKey0,
                 item.SubItems(ColumnNames.ItemKey).Text))

            End If

        Catch ex As Exception
            UserMessage.Show(My.Resources.ctlWorkQueueContents_AnErrorOccurredWhileTryingToGetTheLogForThisItem, ex)

        End Try

    End Sub

    ''' <summary>
    ''' Gets the IDs of the items selected in the UI
    ''' </summary>
    ''' <returns>Returns a (possibly empty, but not null) list
    ''' of IDs.</returns>
    Private Function GetSelectedItems() As IList(Of clsWorkQueueItem)
        Dim items As New List(Of clsWorkQueueItem)
        For Each item As ListViewItem In lstQueueContents.SelectedItems
            items.Add(CorrespondingQueueItem(item))
        Next
        Return items
    End Function

#End Region



    Private Sub pnlListviewContainer_MouseMove(ByVal sender As Object, ByVal e As MouseEventArgs) Handles pnlListviewContainer.MouseMove
        If e.Y > lstQueueContents.Top And e.Y < lstQueueContents.Top + ColumnHeaderHeight Then
            Dim CurrentX As Integer = e.X
            Dim ColumnLeft As Integer = 0
            Dim i As Integer = 0
            ' fix for #4464 - bound-check i before getting the width of the indexed column
            While i < lstQueueContents.Columns.Count AndAlso
             CurrentX > lstQueueContents.Columns(i).Width

                CurrentX -= lstQueueContents.Columns(i).Width
                ColumnLeft += lstQueueContents.Columns(i).Width
                i += 1

            End While

            If i < lstQueueContents.Columns.Count Then
                lstQueueContents.Invalidate(New Rectangle(ColumnLeft, 0, lstQueueContents.Columns(i).Width, ColumnHeaderHeight))
            End If
        End If

    End Sub

    Private Sub pnlListviewContainer_SizeChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles pnlListviewContainer.SizeChanged
        RefreshListviewSize()
        Dim Combo As ComboBox = TryCast(pnlListviewContainer.Controls("Created"), ComboBox)
        If Combo IsNot Nothing Then
            lstQueueContents.Top = Combo.Bottom
        End If
    End Sub

    Private Sub lstQueueContents_SizeChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstQueueContents.SizeChanged
        Dim i As Integer = lstQueueContents.Width
    End Sub

    ''' <summary>
    ''' Gets the name of the help file associated with this control.
    ''' </summary>
    ''' <returns>Returns the name of the file containing help.</returns>
    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "control-queues.html"
    End Function

    ''' <summary>
    ''' Restores the default value in each filter.
    ''' </summary>
    Private Sub RestoreDefaultFilters()
        For Each c As Control In pnlListviewContainer.Controls
            If TypeOf c Is ComboBox Then
                SetDefaultFilterValue(CType(c, ComboBox))
            End If
        Next
    End Sub

    Private Sub lnkClearFilters_LinkClicked(ByVal sender As Object, ByVal e As LinkLabelLinkClickedEventArgs) Handles lnkClearFilters.LinkClicked
        Try
            RestoreDefaultFilters()
            cmbFilterSwitcher.SelectedItem = Nothing
            RefreshList()
        Catch Ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlWorkQueueContents_UnexpectedError0, Ex.Message), Ex)
        End Try
    End Sub

    Private Function PositionColumnAvailable() As Boolean
        Return llShowPosition.Text = My.Resources.ctlWorkQueueContents_RemovePositionsColumn
    End Function

    Private Sub llShowPosition_LinkClicked(ByVal sender As Object, ByVal e As LinkLabelLinkClickedEventArgs) Handles llShowPosition.LinkClicked
        Try
            If PositionColumnAvailable() Then
                llShowPosition.Text = My.Resources.ctlWorkQueueContents_ShowPositionsInQueue
                RemovePositionColumn()
                StopPositionFetcher()
            Else
                llShowPosition.Text = My.Resources.ctlWorkQueueContents_RemovePositionsColumn
                StopPositionFetcher()

                Try
                    lstQueueContents.BeginUpdate()
                    ShowPositionColumn()
                Finally
                    lstQueueContents.EndUpdate()
                End Try

                StartPositionFetcher()
            End If
        Catch Ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlWorkQueueContents_UnexpectedError0, Ex.Message), Ex)
        End Try
    End Sub

    ''' <summary>
    ''' Starts the thread which fetches the position of each item in the queue.
    ''' </summary>
    Private Sub StartPositionFetcher()
        If Not bwGetPositions.IsBusy Then bwGetPositions.RunWorkerAsync(mLastResults)
    End Sub

    ''' <summary>
    ''' Stops the thread which fetches the position of each item in the queue.
    ''' </summary>
    Private Sub StopPositionFetcher()
        If bwGetPositions.IsBusy Then bwGetPositions.CancelAsync()
    End Sub

    ''' <summary>
    ''' Fetches the positions for each item in the listview, one by one.
    ''' </summary>
    Private Sub HandleGetPositionsDoWork(sender As Object, e As DoWorkEventArgs) Handles bwGetPositions.DoWork
        ' This can happen if no queues are selected - ignore it. Just no-op
        Dim items = TryCast(e.Argument, IDictionary(Of Long, clsWorkQueueItem))
        If items Is Nothing OrElse items.Count = 0 Then Return

        Try
            gSv.WorkQueueGetItemPositions(mQueueId, items)
        Catch ex As Exception
            UserMessage.Show(ex.Message)
            Return
        End Try

        If bwGetPositions.CancellationPending Then e.Cancel = True : Return

        e.Result = items
    End Sub

    ''' <summary>
    ''' Updates the UI after the positions have been retrieved
    ''' </summary>
    Private Sub HandleGetPositionsCompleted(
     sender As Object, e As RunWorkerCompletedEventArgs) _
     Handles bwGetPositions.RunWorkerCompleted
        ' If there was an error we need to report it
        Dim ex As Exception = e.Error
        If ex IsNot Nothing Then
            UserMessage.Err(ex,
             My.Resources.ctlWorkQueueContents_AnErrorOccurredRetrievingTheQueuePositions0, ex.Message)
            Return
        End If

        ' If it was cancelled, there is nothing to do
        If e.Cancelled Then Return

        mLastResults = DirectCast(e.Result, IDictionary(Of Long, clsWorkQueueItem))

        UpdateItemPosition(mLastResults)

    End Sub

    ''' <summary>
    ''' Updates the position column of the supplied item, with the specified value.
    ''' </summary>
    ''' <param name="items">The dictionary mapping the queue position against the
    ''' queue item's key </param>
    Private Sub UpdateItemPosition(ByVal items As IDictionary(Of Long, clsWorkQueueItem))
        ' Go through each item in the list
        For Each item As ListViewItem In lstQueueContents.Items
            ' Get its ID and check if it's there in our items to update the posn on
            Dim id As Long = CType(item.Tag, Long)
            If Not items.ContainsKey(id) Then Continue For

            ' Get the position subitem and set its value to the queue item's position
            Dim subitem As ListViewSubItem = item.SubItems(ColumnNames.Position)
            If subitem IsNot Nothing Then
                Dim posn As Integer = items(id).Position
                subitem.Text = If(posn > 0, CStr(posn), "")
            End If

        Next
    End Sub

    Private Sub lstQueueContents_MouseWheel(ByVal sender As Object, ByVal e As MouseEventArgs) Handles lstQueueContents.MouseWheel
        pnlListviewContainer.DoMouseWheel(e)
    End Sub

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            Try
                bwGetPositions.CancelAsync()
                bwGetContents.CancelAsync()
                If disposing AndAlso components IsNot Nothing Then
                    components.Dispose()
                End If
            Catch ' Ignore errors in disposing
            End Try
        End If
        MyBase.Dispose(disposing)
    End Sub


    Private Sub cmbFilterSwitcher_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmbFilterSwitcher.SelectedIndexChanged
        Dim Filter As WorkQueueUIFilter
        Try
            If cmbFilterSwitcher.Text <> "" Then
                Cursor = Cursors.WaitCursor
                Filter = WorkQueueUIFilter.FromName(cmbFilterSwitcher.Text)
                PopulateUIWithFilter(Filter)
            End If
        Catch Ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlWorkQueueContents_UnableToLoadFilterSettings0, Ex.Message))
        Finally
            Cursor = Cursors.Default
            btnDeleteFilter.Enabled = cmbFilterSwitcher.SelectedIndex <> -1
        End Try
    End Sub

    ''' <summary>
    ''' Populates the UI with the supplied filter settings and refreshes the
    ''' current view.
    ''' </summary>
    ''' <param name="Filter">The filter to be populated.</param>
    Private Sub PopulateUIWithFilter(ByVal filter As WorkQueueUIFilter)
        mIgnoreFilterComboChanges = True
        Try
            If filter.IconKey <> "" Then
                For Each o As Object In mImageDropDownFilter.Items
                    Dim I As Image = TryCast(o, Image)
                    If I IsNot Nothing Then
                        If CType(I.Tag, String) = filter.IconKey Then
                            mImageDropDownFilter.SelectedItem = I
                            Exit For
                        End If
                    End If
                Next
            Else
                SetDefaultFilterValue(mImageDropDownFilter)
            End If

            With pnlListviewContainer
                .Controls(ColumnNames.ItemKey).Text = filter.ItemKeyFilter
                .Controls(ColumnNames.Priority).Text = filter.PriorityFilter
                .Controls(ColumnNames.Status).Text = filter.StatusFilter
                .Controls(ColumnNames.Tags).Text = filter.TagsFilter
                .Controls(ColumnNames.Resource).Text = filter.ResourceFilter
                .Controls(ColumnNames.Attempt).Text = filter.AttemptFilter
                .Controls(ColumnNames.Created).Text = filter.CreatedFilter
                .Controls(ColumnNames.LastUpdated).Text = filter.LastUpdatedFilter
                .Controls(ColumnNames.NextReview).Text = filter.NextReviewFilter
                .Controls(ColumnNames.Completed).Text = filter.CompletedFilter
                .Controls(ColumnNames.TotalWorkTime).Text = filter.TotalWorkTimeFilter
                .Controls(ColumnNames.ExceptionDate).Text = filter.ExceptionDateFilter
                .Controls(ColumnNames.ExceptionReason).Text = filter.ExceptionReasonFilter
            End With

            'this change causes UI refresh
            mRowsPerPage.RowsPerPage = filter.MaxRows
        Finally
            mIgnoreFilterComboChanges = False
            PopulateList(True)
        End Try
    End Sub

    ''' <summary>
    ''' Creates a UI filter corresponding to the current config in the UI
    ''' </summary>
    Private Function GetUIFilter() As WorkQueueUIFilter
        Dim filter As New WorkQueueUIFilter

        Dim image As Image = TryCast(mImageDropDownFilter.SelectedItem, Image)
        If image IsNot Nothing Then
            filter.IconKey = CType(image.Tag, String)
        Else
            filter.IconKey = ""
        End If
        filter.ItemKeyFilter = pnlListviewContainer.Controls(ColumnNames.ItemKey).Text
        filter.PriorityFilter = pnlListviewContainer.Controls(ColumnNames.Priority).Text
        filter.StatusFilter = pnlListviewContainer.Controls(ColumnNames.Status).Text
        filter.TagsFilter = pnlListviewContainer.Controls(ColumnNames.Tags).Text
        filter.ResourceFilter = pnlListviewContainer.Controls(ColumnNames.Resource).Text
        filter.AttemptFilter = pnlListviewContainer.Controls(ColumnNames.Attempt).Text
        filter.CreatedFilter = pnlListviewContainer.Controls(ColumnNames.Created).Text
        filter.LastUpdatedFilter = pnlListviewContainer.Controls(ColumnNames.LastUpdated).Text
        filter.NextReviewFilter = pnlListviewContainer.Controls(ColumnNames.NextReview).Text
        filter.CompletedFilter = pnlListviewContainer.Controls(ColumnNames.Completed).Text
        filter.TotalWorkTimeFilter = pnlListviewContainer.Controls(ColumnNames.TotalWorkTime).Text
        filter.ExceptionDateFilter = pnlListviewContainer.Controls(ColumnNames.ExceptionDate).Text
        filter.ExceptionReasonFilter = pnlListviewContainer.Controls(ColumnNames.ExceptionReason).Text

        filter.MaxRows = mRowsPerPage.RowsPerPage
        Return filter
    End Function



    Private Sub btnDeleteFilter_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDeleteFilter.Click
        Try
            If cmbFilterSwitcher.Text <> "" Then
                If UserMessage.YesNoCancel(String.Format(My.Resources.ctlWorkQueueContents_AreYouSureYouWishToDeleteTheStoredFilter0, cmbFilterSwitcher.Text)) = MsgBoxResult.Yes Then
                    gSv.WorkQueueDeleteFilter(cmbFilterSwitcher.Text)
                End If
            Else
                UserMessage.Show(My.Resources.ctlWorkQueueContents_PleaseFirstSelectAFilterToDelete)
            End If
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlWorkQueueContents_FailedToDeleteFilter0, ex.Message), ex)
        Finally
            RepopulateFilterList(Nothing)
        End Try
    End Sub

    Private Sub btnSaveFilter_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSaveFilter.Click

        Dim filterNames As List(Of String)

        Try
            filterNames = gSv.WorkQueueGetFilterNames()
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.clsServer_ErrorInteractingWithDatabase0, ex.Message), ex)
            Exit Sub
        End Try

        Dim newName As String = cmbFilterSwitcher.Text

        If UserMessage.OkCancelWithComboBox(My.Resources.ctlWorkQueueContents_PleaseChooseANameForYourSavedFilterYouMayOverwriteAnExistingFilterBySelectingIt, filterNames.ToArray, newName, My.Resources.ctlWorkQueueContents_Save, My.Resources.ctlWorkQueueContents_Cancel) = MsgBoxResult.Ok Then
            Try
                Dim filterXML As String = GetUIFilter().ToXML()

                If filterNames.Contains(newName) Then
                    Dim recordsAffected = gSv.WorkQueueUpdateFilter(newName, filterXML)
                    If recordsAffected = 0 Then UserMessage.Show(String.Format(My.Resources.ctlWorkQueueContents_SaveFailed0, My.Resources.clsServer_CommandCompletedSuccesfullyButNoItemsWereUpdated))
                Else
                    Try
                        gSv.WorkQueueCreateFilter(newName, filterXML, Guid.Empty)
                    Catch sqlEx As SqlException
                        Dim errorMessage = String.Format(My.Resources.clsServer_CannotCreateFilterWithName0BecauseAFilterWithThisNameAlreadyExists, newName)
                        UserMessage.Show(String.Format(My.Resources.ctlWorkQueueContents_SaveFailed0, errorMessage))
                    End Try
                End If
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.ctlWorkQueueContents_UnexpectedError0, ex.Message), ex)
            Finally
                RepopulateFilterList(newName)
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Populates the filter list with the latest values from the database.
    ''' </summary>
    ''' <param name="ItemToSelect">The item to be selected from the list,
    ''' if any.</param>
    Private Sub RepopulateFilterList(ByVal ItemToSelect As String)

        Try
            Dim filters = gSv.WorkQueueGetFilterNames()
            cmbFilterSwitcher.Items.Clear()

            For Each filter As String In filters
                cmbFilterSwitcher.Items.Add(filter)
            Next
            cmbFilterSwitcher.SelectedItem = ItemToSelect
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlWorkQueueContents_UnableToPopulateFilterList0, ex.Message))
        End Try

        btnDeleteFilter.Enabled = cmbFilterSwitcher.SelectedIndex <> -1

    End Sub

    Private Sub BtnSetDefault_Click(sender As System.Object, e As System.EventArgs) Handles btnSetDefault.Click
        Try
            If mQueueId <> Guid.Empty Then
                Dim filterName = cmbFilterSwitcher.Text
                If Not gSv.WorkQueueSetDefaultFilter(mQueueId, filterName) Then
                    Dim errorMessage = String.Format(My.Resources.clsServer_NoFilterFoundWithTheName0, filterName)
                    UserMessage.Show(String.Format(My.Resources.ctlWorkQueueContents_DefaultFilterNotSet0, errorMessage))
                Else
                    UserMessage.Show(My.Resources.ctlWorkQueueContents_DefaultFilterSuccessfullyChanged)
                End If
            Else
                UserMessage.Show(My.Resources.ctlWorkQueueContents_CannotSetDefaultFilterNoQueueSelected)
            End If
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlWorkQueueContents_UnexpectedError0, ex.Message), ex)
        End Try
    End Sub

    Public Sub ToggleLinkShowPosition(ByVal value As Boolean)
        llShowPosition.Enabled = value
    End Sub
End Class
