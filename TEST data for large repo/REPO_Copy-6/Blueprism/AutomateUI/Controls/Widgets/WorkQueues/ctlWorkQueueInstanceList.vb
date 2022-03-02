
Imports BluePrism.AutomateAppCore
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Images

''' Project  : Automate
''' Class    : ctlWorkQueueInstanceList
''' 
''' <summary>
''' A control for displaying a read-only and low-functionality list of work queue
''' items. It was primarily created to display a list of retry instances for a 
''' particular item, but, in reality it will show all the items passed into it in
''' the relevant constructor.
''' </summary>
Public Class ctlWorkQueueInstanceList

    ''' <summary>
    ''' Gets all the work queue items which correspond to the given ID.
    ''' </summary>
    ''' <param name="identifier">The identifier of the work queue item for which all
    ''' retry instances are required. If the given identifier is a GUID, this will
    ''' assume it is the Item ID. If it is a Long, it will assume that it is the 
    ''' item instance identity. Anything else will cause an empty to be
    ''' returned.</param>
    ''' <returns>A collection of all work queue item instances which share the Item
    ''' ID of the item specified by the given identifier.</returns>
    Private Shared Function GetAllItemsWithID(ByVal identifier As Object) _
     As ICollection(Of clsWorkQueueItem)

        Dim id As Guid = Nothing
        Dim ident As Long = Nothing

        If TypeOf identifier Is Guid Then
            id = DirectCast(identifier, Guid)
        ElseIf TypeOf identifier Is Long Then
            ident = DirectCast(identifier, Long)
        Else
            Return New clsWorkQueueItem() {}
        End If

        Dim items As ICollection(Of clsWorkQueueItem) = Nothing
        Dim sErr As String = Nothing

        Try
            gSv.WorkQueueGetAllRetryInstances(id, ident, items)
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlWorkQueueInstanceList_ErrorWhileRetrievingAllRetries0, ex.Message))
            Return Nothing
        End Try

        Return items

    End Function

#Region "Constructors"

    ''' <summary>
    ''' Creates a new WorkQueueInstanceList which shows all the instances
    ''' associated with the given Item ID.
    ''' </summary>
    ''' <param name="itemId">The Item ID for which all instances are required.
    ''' </param>
    Public Sub New(ByVal itemId As Guid)
        Me.New(GetAllItemsWithID(itemId))
    End Sub

    ''' <summary>
    ''' Creates a new WorkQueueInstanceList which shows all the instances which share
    ''' the item ID with the instance specified by the given identity.
    ''' </summary>
    ''' <param name="itemIdent">The identity of the instance for which all other
    ''' instances of the same item are required.
    ''' </param>
    Public Sub New(ByVal itemIdent As Long)
        Me.New(GetAllItemsWithID(itemIdent))
    End Sub

    ''' <summary>
    ''' Creates a WorkQueueInstanceList which displays the given list of work queue
    ''' item objects.
    ''' </summary>
    ''' <param name="items">The fully populated list of work queue items to display
    ''' </param>
    ''' <exception cref="ArgumentNullException">If the given collection is null.
    ''' </exception>
    Public Sub New(ByVal items As ICollection(Of clsWorkQueueItem))

        If items Is Nothing Then
            Throw New ArgumentNullException(NameOf(items), My.Resources.ctlWorkQueueInstanceList_CannotDisplayANullListOfWorkQueueItems)
        End If

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' State, Item Key, Status, Tags, Attempt, Created, Last Updated, Next Review, 
        ' Completed, Total Work Time, Exception Date, Exception Reason

        Dim cols As New clsOrderedDictionary(Of String, DataGridViewColumn)
        cols("State") = New DataGridViewImageColumn()
        cols("Item Key") = New DataGridViewColumn(New DataGridViewTextBoxCell())
        cols("Status") = New DataGridViewColumn(New DataGridViewTextBoxCell())
        cols("Tags") = New DataGridViewColumn(New DataGridViewTextBoxCell())
        cols("Resource") = New DataGridViewColumn(New DataGridViewTextBoxCell())
        cols("Attempt") = New DataGridViewColumn(New DataGridViewTextBoxCell())
        cols("Last Updated") = New DataGridViewColumn(New DataGridViewTextBoxCell())
        cols("Next Review") = New DataGridViewColumn(New DataGridViewTextBoxCell())
        cols("Completed") = New DataGridViewColumn(New DataGridViewTextBoxCell())
        cols("Total Work Time") = New DataGridViewColumn(New DataGridViewTextBoxCell())
        cols("Exception Date") = New DataGridViewColumn(New DataGridViewTextBoxCell())
        cols("Exception Reason") = New DataGridViewColumn(New DataGridViewTextBoxCell())

        For Each entry As KeyValuePair(Of String, DataGridViewColumn) In cols
            entry.Value.Name = entry.Key.Replace(" ", "")
            entry.Value.HeaderText = My.Resources.ResourceManager.GetString("ctlWorkQueueInstanceList_" & entry.Value.Name, My.Resources.Culture) 'entry.Key
        Next

        ' AddRange, for some reason *always* fails suggesting that the CellStyle
        ' is null for one of the columns... I've no idea why.
        Dim mGridFont As Font = New Font("Segoe UI", 8.25F)
        Dim xMargin = 5
        For Each col As DataGridViewColumn In cols.Values
            mGrid.Columns.Add(col)
            Dim c = mGrid.Columns(mGrid.ColumnCount - 1)
            c.MinimumWidth = TextRenderer.MeasureText(col.HeaderText, mGridFont).Width + xMargin * 2
        Next

        Dim lastCol As DataGridViewColumn = mGrid.Columns(mGrid.ColumnCount - 1)
        ' Make the last column auto-size to fill available space
        lastCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        ' But give it a minwidth, or it's unusable
        lastCol.MinimumWidth = 100

        ' Now add all the data...
        For Each item As clsWorkQueueItem In items
            Dim bmp As Bitmap
            Select Case item.CurrentState
                Case clsWorkQueueItem.State.Completed
                    bmp = ToolImages.Tick_16x16
                Case clsWorkQueueItem.State.Exceptioned
                    bmp = ToolImages.Flag_Purple_16x16
                Case clsWorkQueueItem.State.Locked
                    bmp = ToolImages.Lock_16x16
                Case clsWorkQueueItem.State.Pending, clsWorkQueueItem.State.Deferred
                    bmp = ToolImages.Custom_Pending_16x16
                Case Else
                    bmp = Nothing
            End Select

            ' Note - Attempt is incremented because it's zero based...
            Dim index As Integer = mGrid.Rows.Add( _
              bmp, _
              item.KeyValue, _
              item.Status, _
              item.TagString, _
              item.Resource, _
              item.Attempt, _
              item.LastUpdatedDisplay, _
              item.DeferredDisplay, _
              item.CompletedDateDisplay, _
              item.WorkTimeDisplay, _
              item.ExceptionDateDisplay, _
              item.ExceptionReason)
            'Dim row As DataGridViewRow = mGrid.Rows(index)
        Next
        mGrid.AllowUserToAddRows = False
        mGrid.Padding = New Padding(0, 0, 0, Convert.ToInt32(mGrid.RowTemplate.Height / items.Count))

        Me.PerformLayout()

    End Sub

#End Region

    ''' <summary>
    ''' Override the preferred size, to return the preferred size of the grid.
    ''' </summary>
    ''' <param name="proposedSize">The, er, "custom-sized area for a control".
    ''' </param>
    ''' <returns>The preferred size of this control. In this case, it is just
    ''' the preferred size of its sole child control.</returns>
    ''' <remarks>
    ''' Apparently, 'PreferredSize' of a container cannot look at its child
    ''' controls and calculate a preferred size for itself - it's up to the
    ''' code that's using it to do it (?!!). That's encapsulation, folks.
    ''' </remarks>
    Public Overrides Function GetPreferredSize(ByVal proposedSize As Size) As Size
        Return mGrid.GetPreferredSize(proposedSize)
    End Function

End Class
