Imports BluePrism.BPCoreLib
Imports BluePrism.Server.Domain.Models

Friend Class ctlListView
    Inherits AutomateControls.UserControlWithBorder

#Region "Windows Form Designer Generated Code"


    ''' <summary>
    ''' The default constructor
    ''' </summary>
    Public Sub New()
        MyBase.New()

        'Required by Forms Designer
        Me.InitializeComponent()

        Me.HidePaginationControls()

        'User Code here
        Me.mRows = New clsCollectionWithEvents(Of clsListRow)
        Me.mHeader.OwningListView = Me
        Me.Canvas.BringToFront()

        AddHandler Me.mHeader.ColumnResizing, AddressOf Me.HandleColumnResizing
        AddHandler Me.mHeader.ColumnResized, AddressOf Me.HandleColumnResized

        Me.DoubleBuffered = True
        Me.pnlScrollPanel.IsDoubleBuffered = True
        Me.pnlScrollPanel.MouseScrollIncrement = ctlListView.DefaultRowHeight

        Me.AllowDrop = True
    End Sub

    ''' <summary>
    ''' A scrollable panel nested inside this control that
    ''' contains the individual rows.
    ''' </summary>
    ''' <remarks></remarks>
    Protected WithEvents pnlScrollPanel As AutomateControls.MonoPanel

    ''' <summary>
    ''' A UI control displaying the column headers. Placed above
    ''' the pnlRowView panel.
    ''' </summary>
    ''' <remarks></remarks>
    Private WithEvents mHeader As AutomateUI.ctlListHeader
    Private components As IContainer

    ''' <summary>
    ''' A panel onto which we paint all of the rows. We dump this inside
    ''' the scroll panel which then automatically takes care of scrolling.
    ''' </summary>
    Protected WithEvents Canvas As System.Windows.Forms.PictureBox


    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlListView))
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.pnlScrollPanel = New AutomateControls.MonoPanel()
        Me.Canvas = New System.Windows.Forms.PictureBox()
        Me.mHeader = New AutomateUI.ctlListHeader()
        Me.pnlPaginationControls = New System.Windows.Forms.Panel()
        Me.lblRowsPerPage = New System.Windows.Forms.Label()
        Me.txtRowsPerPage = New AutomateControls.Textboxes.StyledTextBox()
        Me.lblPageNum = New System.Windows.Forms.Label()
        Me.btnLast = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnNext = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnPrevious = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnFirst = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.Panel1.SuspendLayout()
        Me.Panel2.SuspendLayout()
        Me.pnlScrollPanel.SuspendLayout()
        CType(Me.Canvas, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlPaginationControls.SuspendLayout()
        Me.SuspendLayout()
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.Panel2)
        Me.Panel1.Controls.Add(Me.pnlPaginationControls)
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.Name = "Panel1"
        '
        'Panel2
        '
        Me.Panel2.Controls.Add(Me.pnlScrollPanel)
        Me.Panel2.Controls.Add(Me.mHeader)
        resources.ApplyResources(Me.Panel2, "Panel2")
        Me.Panel2.Name = "Panel2"
        '
        'pnlScrollPanel
        '
        resources.ApplyResources(Me.pnlScrollPanel, "pnlScrollPanel")
        Me.pnlScrollPanel.Controls.Add(Me.Canvas)
        Me.pnlScrollPanel.DockPadding.All = 0
        Me.pnlScrollPanel.DockPadding.Bottom = 0
        Me.pnlScrollPanel.DockPadding.Left = 0
        Me.pnlScrollPanel.DockPadding.Right = 0
        Me.pnlScrollPanel.DockPadding.Top = 0
        Me.pnlScrollPanel.ForeColor = System.Drawing.Color.Black
        Me.pnlScrollPanel.IsDoubleBuffered = False
        Me.pnlScrollPanel.MouseScrollIncrement = 0
        Me.pnlScrollPanel.Name = "pnlScrollPanel"
        '
        'Canvas
        '
        resources.ApplyResources(Me.Canvas, "Canvas")
        Me.Canvas.Name = "Canvas"
        Me.Canvas.TabStop = False
        '
        'mHeader
        '
        resources.ApplyResources(Me.mHeader, "mHeader")
        Me.mHeader.FillColumn = Nothing
        Me.mHeader.FillColumnIndex = -1
        Me.mHeader.LastColumnAutoSize = False
        Me.mHeader.MinimumColumnWidth = 200
        Me.mHeader.Name = "mHeader"
        Me.mHeader.OwningListView = Nothing
        Me.mHeader.ScrollOffset = 0
        Me.mHeader.TabStop = False
        '
        'pnlPaginationControls
        '
        Me.pnlPaginationControls.BackColor = System.Drawing.SystemColors.Control
        Me.pnlPaginationControls.Controls.Add(Me.lblRowsPerPage)
        Me.pnlPaginationControls.Controls.Add(Me.txtRowsPerPage)
        Me.pnlPaginationControls.Controls.Add(Me.lblPageNum)
        Me.pnlPaginationControls.Controls.Add(Me.btnLast)
        Me.pnlPaginationControls.Controls.Add(Me.btnNext)
        Me.pnlPaginationControls.Controls.Add(Me.btnPrevious)
        Me.pnlPaginationControls.Controls.Add(Me.btnFirst)
        resources.ApplyResources(Me.pnlPaginationControls, "pnlPaginationControls")
        Me.pnlPaginationControls.Name = "pnlPaginationControls"
        '
        'lblRowsPerPage
        '
        resources.ApplyResources(Me.lblRowsPerPage, "lblRowsPerPage")
        Me.lblRowsPerPage.Name = "lblRowsPerPage"
        '
        'txtRowsPerPage
        '
        resources.ApplyResources(Me.txtRowsPerPage, "txtRowsPerPage")
        Me.txtRowsPerPage.Name = "txtRowsPerPage"
        '
        'lblPageNum
        '
        resources.ApplyResources(Me.lblPageNum, "lblPageNum")
        Me.lblPageNum.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblPageNum.Name = "lblPageNum"
        '
        'btnLast
        '
        resources.ApplyResources(Me.btnLast, "btnLast")
        Me.btnLast.Name = "btnLast"
        Me.btnLast.UseVisualStyleBackColor = False
        '
        'btnNext
        '
        resources.ApplyResources(Me.btnNext, "btnNext")
        Me.btnNext.Name = "btnNext"
        Me.btnNext.UseVisualStyleBackColor = False
        '
        'btnPrevious
        '
        resources.ApplyResources(Me.btnPrevious, "btnPrevious")
        Me.btnPrevious.Name = "btnPrevious"
        Me.btnPrevious.UseVisualStyleBackColor = False
        '
        'btnFirst
        '
        resources.ApplyResources(Me.btnFirst, "btnFirst")
        Me.btnFirst.Name = "btnFirst"
        Me.btnFirst.UseVisualStyleBackColor = False
        '
        'ctlListView
        '
        Me.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.Controls.Add(Me.Panel1)
        Me.FrameBorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        resources.ApplyResources(Me, "$this")
        Me.Name = "ctlListView"
        Me.Panel1.ResumeLayout(False)
        Me.Panel2.ResumeLayout(False)
        Me.pnlScrollPanel.ResumeLayout(False)
        CType(Me.Canvas, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlPaginationControls.ResumeLayout(False)
        Me.pnlPaginationControls.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

#End Region


#Region "Members"

    ''' <summary>
    ''' Holds the rows of the listview.
    ''' </summary>
    Private WithEvents mRows As clsCollectionWithEvents(Of clsListRow)

    ''' <summary>
    ''' Stores the zero-based index of the row which is currently
    ''' selected for editing. A negative value indicates that no 
    ''' row is selected for editing.
    ''' </summary>
    Private WithEvents mCurrentEditableRow As clsListRow

    ''' <summary>
    ''' The editable row used for changing values. Only one row is editable
    ''' at a time, so we just show/hide/move the single row from place to 
    ''' place as appropriate
    ''' </summary>
    Private mEditRowControl As ctlEditableListRow


    ''' <summary>
    ''' Defines interface for creating edit control.
    ''' </summary>
    ''' <param name="RowToBeEdited">A reference to the row to be edited.
    ''' Clients should use this to populate their edit control with the
    ''' appropriate data.</param>
    ''' <returns>Returns an editable row with the correct number of columns,
    ''' matching the appropriate data types in the columns.</returns>
    Public Delegate Function CreateEditRow(ByVal RowToBeEdited As clsListRow) As ctlEditableListRow

    ''' <summary>
    ''' Private member to store public property Readonly
    ''' </summary>
    ''' <remarks></remarks>
    Private mbReadonly As Boolean = False
    ''' <summary>
    ''' Indicates whether the listview is readonly. If so then the data cannot
    ''' be modified.
    ''' </summary>
    Public Property [Readonly]() As Boolean
        Get
            Return Me.mbReadonly
        End Get
        Set(ByVal value As Boolean)
            Me.mbReadonly = value
            If Me.mbReadonly Then
                Me.EndEditing()
            End If
        End Set
    End Property

#End Region


#Region "Event Declarations"

    ''' <summary>
    ''' Event raised when the row currently selected for editing is
    ''' changed.
    ''' </summary>
    ''' <param name="sender">The sender of the event.</param>
    ''' <param name="e">The args detailing the old and new rows from the change.
    ''' </param>
    Public Event EditableRowChanged(
        ByVal sender As Object, ByVal e As ListRowChangedEventArgs)

    ''' <summary>
    ''' An event that is raised every time a column header is clicked.
    ''' </summary>
    Public Event ColumnClicked As ColumnClickEventHandler

    ''' <summary>
    ''' Event fired when an activatable process value control is activated
    ''' </summary>
    ''' <param name="sender">The sender of the event - ie. this listview.</param>
    ''' <param name="name">The name of the activated value.</param>
    ''' <param name="valueControl">The process value control that was activated.
    ''' </param>
    Public Event ValueControlActivated(
        ByVal sender As ctlListView, ByVal name As String,
        ByVal valueControl As IActivatableProcessValue)

#End Region


#Region "Properties"

    ''' <summary>
    ''' The minimum width to apply to columns. Columns larger
    ''' than this will be enlarged.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The minimum column width allowed.</returns>
    Public Property MinimumColumnWidth() As Integer
        Get
            Return mHeader.MinimumColumnWidth
        End Get
        Set(ByVal value As Integer)
            mHeader.MinimumColumnWidth = value
        End Set
    End Property

    ''' <summary>
    ''' The columns of the listview.
    ''' </summary>
    ''' <value>.</value>
    Public ReadOnly Property Columns() As clsColumnCollection
        Get
            Return mHeader.Columns
        End Get
    End Property

    ''' <summary>
    ''' Determines if the last column should automatically
    ''' be sized to fill (and not exceed in width) the 
    ''' space available left by the other columns.
    ''' </summary>
    ''' <value></value>
    ''' <returns>True or false.</returns>
    Public Property LastColumnAutoSize() As Boolean
        Get
            Return mHeader.LastColumnAutoSize
        End Get
        Set(ByVal Value As Boolean)
            mHeader.LastColumnAutoSize = Value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the fill column to use in this list view.
    ''' Note that if this is set to null, it has the same effect as setting no
    ''' fill column.
    ''' </summary>
    ''' <exception cref="ArgumentException">If an attempt is made to set the fill
    ''' column to a column which does not belong to this list view.</exception>
    Public Property FillColumn() As ctlListColumn
        Get
            Return mHeader.FillColumn
        End Get
        Set(ByVal value As ctlListColumn)
            mHeader.FillColumn = value
        End Set
    End Property


    ''' <summary>
    ''' Private member to store public property HighlightedRowOutline()
    ''' </summary>
    Private mHighlightedRowOutline As Color = AutomateControls.ColourScheme.Default.ListViewSelectedRowOutline
    ''' <summary>
    ''' The colour used to outline the selected row. Goes hand in hand with
    ''' HighlightedRowBackColour and has same relationship with child
    ''' rows.
    ''' </summary>
    ''' <value></value>
    Public Property HighlightedRowOutline() As Color
        Get
            Return mHighlightedRowOutline
        End Get
        Set(ByVal value As Color)
            mHighlightedRowOutline = value
        End Set
    End Property

    Private mHighlightedForeColor As Color = Color.FromKnownColor(KnownColor.HighlightText)
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents Panel2 As System.Windows.Forms.Panel
    Friend WithEvents pnlPaginationControls As System.Windows.Forms.Panel
    Friend WithEvents lblRowsPerPage As System.Windows.Forms.Label
    Friend WithEvents txtRowsPerPage As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents lblPageNum As System.Windows.Forms.Label
    Friend WithEvents btnLast As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnNext As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnPrevious As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnFirst As AutomateControls.Buttons.StandardStyledButton

    Public Property HighlightedForeColor() As Color
        Get
            Return mHighlightedForeColor
        End Get
        Set(ByVal value As Color)
            mHighlightedForeColor = value
        End Set
    End Property

    ''' <summary>
    ''' Private member to store public property HighlightedRowBackColour()
    ''' </summary>
    Private mHighlightedRowBackColour As Color = AutomateControls.ColourScheme.Default.ListViewSelectedRowBackground
    ''' <summary>
    ''' The colour used to highlight the selected row. Each row can 
    ''' be set separately using the HighlightedBackColour on the row
    ''' in question.
    ''' 
    ''' When not set, the row is highlighted using the setting here.
    ''' </summary>
    ''' <value></value>
    Public Property HighlightedRowBackColour() As Color
        Get
            Return mHighlightedRowBackColour
        End Get
        Set(ByVal value As Color)
            mHighlightedRowBackColour = value
        End Set
    End Property

    ''' <summary>
    ''' Commits the changes to the currently editable row, but leaves it in an
    ''' editing state.
    ''' </summary>
    Friend Sub CommitEditableRow()
        If mCurrentEditableRow IsNot Nothing AndAlso mEditRowControl IsNot Nothing Then
            mCurrentEditableRow.EndEdit(mEditRowControl)
        End If
    End Sub

    ''' <summary>
    ''' Provides access to the current editable row
    ''' </summary>
    Public Property CurrentEditableRow() As clsListRow
        Get
            Return Me.mCurrentEditableRow
        End Get
        Set(ByVal Value As clsListRow)
            If Not Value Is Me.mCurrentEditableRow Then

                If Me.Readonly Then
                    Value = Nothing
                End If

                'End editing the current row, if needs be
                If Me.mCurrentEditableRow IsNot Nothing Then
                    If Me.mEditRowControl IsNot Nothing Then
                        Me.mCurrentEditableRow.EndEdit(Me.mEditRowControl)
                        RemoveHandler Me.mEditRowControl.ListRowKeyDown, AddressOf OnListRowKeyDown
                        RemoveHandler Me.mEditRowControl.EditControlKeyPreview, AddressOf OnEditControlKeyPreview
                        Me.Canvas.Controls.Remove(mEditRowControl)
                        mEditRowControl.Dispose()
                        mEditRowControl = Nothing
                    End If
                End If

                'Remember old value and set the new row
                Dim OldRow As clsListRow = Me.mCurrentEditableRow
                Me.mCurrentEditableRow = Value

                'Start editing the new row
                If mCurrentEditableRow IsNot Nothing Then

                    'Set correct page first
                    Me.miCurrentPageIndex = Me.mCurrentEditableRow.Index \ miRowsPerPage
                    If miCurrentPageIndex > 1 Then
                        Me.ShowPaginationControls()
                    End If
                    Me.UpdatePaginationDisplay()

                    'populate edit control 
                    Me.mEditRowControl = Me.mCurrentEditableRow.CreateEditRow
                    Me.mEditRowControl.OwningListView = Me
                    AddHandler Me.mEditRowControl.ListRowKeyDown, AddressOf OnListRowKeyDown
                    AddHandler Me.mEditRowControl.EditControlKeyPreview, AddressOf OnEditControlKeyPreview
                    mCurrentEditableRow.BeginEdit(Me.mEditRowControl)
                    Me.mEditRowControl.IsSelected = True

                    'and move to correct position
                    Me.PositionEditRow()
                    Me.Canvas.Controls.Add(mEditRowControl)
                    mEditRowControl.BringToFront()
                    Me.mCurrentEditableRow.EnsureVisible()

                End If
                RaiseEvent EditableRowChanged(Me,
                    New ListRowChangedEventArgs(OldRow, mCurrentEditableRow))
                UpdateView()
            End If
        End Set
    End Property

    ''' <summary>
    ''' The editable listrow control used to edit the current row,
    ''' if any.
    ''' </summary>
    Public ReadOnly Property EditRowControl() As ctlEditableListRow
        Get
            Return mEditRowControl
        End Get
    End Property

    ''' <summary>
    ''' Positions the edit row against the row it represents. This must
    ''' be done after clicking a new row, sorting the current row order,
    ''' resizing this control, etc.
    ''' </summary>
    Private Sub PositionEditRow()
        If Me.mCurrentEditableRow IsNot Nothing Then
            mEditRowControl.Bounds = GetRowBounds(mCurrentEditableRow)
            mEditRowControl.Left += 1
            mEditRowControl.Top += 1
            SetEditableColumnWidths(mEditRowControl)
        End If
    End Sub

    ''' <summary>
    ''' Gets the bounds of the supplied row.
    ''' </summary>
    ''' <param name="Row">The row of interest.</param>
    ''' <returns>Gets the bounds of the row, in coordinates relative to the canvas.</returns>
    Private Function GetRowBounds(ByVal Row As clsListRow) As Rectangle
        Dim RowIndex As Integer = mRows.IndexOf(Row) Mod miRowsPerPage
        Return New Rectangle(0, RowIndex * RowHeight, mHeader.TotalOfColumnWidths, RowHeight)
    End Function


    Private Sub SetEditableColumnWidths(ByVal EditRow As ctlEditableListRow)
        If Me.Columns.Count = EditRow.Items.Count Then
            Dim TotalWidth As Integer = 0
            For i As Integer = 0 To Me.Columns.Count - 1
                EditRow.Items(i).Left = TotalWidth
                EditRow.Items(i).Width = Me.Columns(i).Width - 1
                TotalWidth += Me.Columns(i).Width

                For Each item As ctlEditableListItem In EditRow.Items
                    item.Height = EditRow.Height - 1
                Next
            Next
        Else
            Debug.Assert(False)
        End If
    End Sub

    ''' <summary>
    ''' Provides access to the rows in the listview
    ''' </summary>
    ''' <value></value>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property Rows() As clsCollectionWithEvents(Of clsListRow)
        Get
            Return mRows
        End Get
    End Property

    ''' <summary>
    ''' Gets the last listrow in this listview, or nothing if this view currently
    ''' has no rows.
    ''' </summary>
    Friend ReadOnly Property LastRow() As clsListRow
        Get
            If mRows.Count > 0 Then Return mRows(mRows.Count - 1)
            Return Nothing
        End Get
    End Property

    ''' <summary>
    ''' Private member to store public property ListViewItemSorter
    ''' </summary>
    Private mSorter As clsAutomateListViewItemSorter

    ''' <summary>
    ''' iComparer object used for sorting listview items
    ''' </summary>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Sorter() As clsAutomateListViewItemSorter
        Get
            Return mSorter
        End Get
        Set(ByVal Value As clsAutomateListViewItemSorter)
            ' Deregister and remove any existing sorter
            If mSorter IsNot Nothing AndAlso mSorter IsNot Value Then mSorter.Dispose()
            mSorter = Value
        End Set
    End Property

    ''' <summary>
    ''' Property to indicate if this listview is sortable or not.
    ''' Setting this to true will create an instance of
    ''' <see cref="clsAutomateListViewItemSorter"/> and assign it to this list
    ''' view, false will remove any currently registered sorter.
    ''' The sorter object can be set or retrieved manually using the
    ''' <see cref="Sorter"/> property.
    ''' </summary>
    Public Property Sortable() As Boolean
        Get
            Return (mSorter IsNot Nothing)
        End Get
        Set(ByVal value As Boolean)
            ' If we're setting it to not sortable, dispose of the existing
            ' sorter (to disassociate it from this listview) and reset the
            ' sorter variable to null.
            If Not value Then
                If mSorter IsNot Nothing Then mSorter.Dispose()
                mSorter = Nothing

            ElseIf mSorter Is Nothing Then
                ' otherwise, set the new sorter into the variable.
                mSorter = New clsAutomateListViewItemSorter(Me)

            End If
        End Set
    End Property



    ''' <summary>
    ''' The available size of the rows area, minus the
    ''' scrollbars (if they are present).
    ''' </summary>
    Public ReadOnly Property EffectiveSize() As Size
        Get
            Return Me.pnlScrollPanel.DisplayRectangle.Size
        End Get
    End Property

#End Region


    ''' <summary>
    ''' Updates the vertical scroll increment on the 
    ''' panel containing the rows.
    ''' </summary>
    ''' <remarks>Sets the value of the scroll increment
    ''' to be one row.</remarks>
    Private Sub UpdateMouseScrollIncrement()
        If Me.Rows.Count > 0 Then
            Me.pnlScrollPanel.MouseScrollIncrement = RowHeight
        End If
    End Sub

    ' Friend Sub FireValueControlActivated(sender as IActivatableProcessValue)

    ''' <summary>
    ''' Updates the listview in response to a new row being added.
    ''' </summary>
    ''' <param name="NewRow">The newly added row.</param>
    Private Sub HandleRowAdded(ByVal NewRow As clsListRow) Handles mRows.ItemAdded
        Me.HandleRowInserted(Me.Rows.Count, NewRow)
        UpdateMouseScrollIncrement()
    End Sub


    ''' <summary>
    ''' Handles the insertion of new rows.
    ''' </summary>
    ''' <param name="index">The index at which the new item
    ''' was inserted.</param>
    ''' <param name="NewRow">The newly inserted row.</param>
    Private Sub HandleRowInserted(ByVal index As Integer, ByVal NewRow As clsListRow) Handles mRows.ItemInserted
        Me.VerifyRowIntegrity(NewRow)
        Me.UpdateView()
    End Sub

    ''' <summary>
    ''' Ensures that the supplied row is suitable for this listview,
    ''' and associates it with this listview if so.
    ''' </summary>
    ''' <param name="NewRow">The row under consideration.</param>
    Private Sub VerifyRowIntegrity(ByVal NewRow As clsListRow)
        If mHeader Is Nothing Then
            Throw New InvalidOperationException(My.Resources.ctlListView_TheListViewDoesnTHaveAHeader)
        End If
        If NewRow.Items.Count <> Me.mHeader.Columns.Count Then
            Throw New InvalidArgumentException(My.Resources.ctlListView_TheNumberOfColumnsDefinedInTheRowDoesNotMatchTheNumberOfColumnsDefinedInTheHead)
        End If

        If Not NewRow.Owner Is Nothing Then
            If NewRow.Owner IsNot Me Then
                Throw New InvalidArgumentException(My.Resources.ctlListView_TheListRowBeingAddedIsAlreadyContainedInAnotherListviewControl)
            End If
        Else
            NewRow.Owner = Me
        End If
    End Sub

    ''' <summary>
    ''' Removes the supplied row from the listview
    ''' </summary>
    ''' <param name="removedRow">The row to be removed. Must
    ''' be a member of this listview.</param>
    Private Sub HandleRowRemoved(ByVal removedRow As clsListRow, ByVal oldIndex As Integer) Handles mRows.ItemRemoved
        If removedRow Is Nothing Then Return

        'Do we need to find a new selected row?
        If removedRow Is mCurrentEditableRow Then
            While oldIndex >= Rows.Count
                oldIndex -= 1
            End While
            If oldIndex >= 0 Then
                CurrentEditableRow = Rows(oldIndex)
            Else
                CurrentEditableRow = Nothing
            End If
        End If

        removedRow.Owner = Nothing

        UpdateView()
    End Sub

    ''' <summary>
    ''' Removes all the rows from the listview
    ''' </summary>
    Private Sub HandleRowsCleared(ByVal OldRows As List(Of clsListRow)) Handles mRows.CollectionCleared
        Dim index As Integer = 0
        For Each r As clsListRow In OldRows
            Me.HandleRowRemoved(r, index)
            index += 1
        Next

        Me.UpdateView()
    End Sub

    ''' <summary>
    ''' Handles the addition of several rows to the listview.
    ''' </summary>
    ''' <param name="NewRows">The newly added rows.</param>
    Private Sub HandleRowRangeAdded(ByVal NewRows As List(Of clsListRow)) Handles mRows.ItemRangeAdded
        For Each r As clsListRow In NewRows
            Me.VerifyRowIntegrity(r)
        Next

        Me.UpdateView()
    End Sub

    ''' <summary>
    ''' Ensures that the values are committed when this list view is validated
    ''' away from.
    ''' </summary>
    ''' <param name="e"></param>
    Protected Overrides Sub OnValidated(ByVal e As EventArgs)
        If mCurrentEditableRow IsNot Nothing Then
            mCurrentEditableRow.EndEdit(mEditRowControl)
        End If
    End Sub

    ''' <summary>
    ''' Updates the positions of all the listview elements.
    ''' </summary>
    Public Sub UpdateView()
        Dim NumPages As Integer = CInt(Math.Ceiling(mRows.Count / Me.miRowsPerPage))
        Me.miCurrentPageIndex = Math.Min(Me.miCurrentPageIndex, NumPages - 1)
        Me.miCurrentPageIndex = Math.Max(0, miCurrentPageIndex)
        Me.UpdatePaginationDisplay()

        If mRows.Count > Me.miRowsPerPage Then
            Me.ShowPaginationControls()
        Else
            Me.HidePaginationControls()
        End If

        'If we re-adjust from long page to short page for
        'example, then the canvas may no longer be visible
        Me.SetCanvasSize()
        Me.pnlScrollPanel.ScrollControlIntoView(Me.Canvas)
        Me.Canvas.Invalidate()
    End Sub



    ''' <summary>
    ''' Scrolls the supplied row into view.
    ''' </summary>
    ''' <param name="Row">The row which should be made
    ''' visible.</param>
    ''' <remarks>
    ''' Make sure you call UpdateView first, orelse
    ''' this method will have no effect.
    ''' 
    ''' Has no effect if the supplied row is
    ''' null, or if the row is not already a member of
    ''' this listview (use Rows.Add() first).</remarks>
    Public Sub ScrollToRow(ByVal Row As clsListRow)
        Me.SetCanvasSize()
        If (Row IsNot Nothing) AndAlso Me.mRows.Contains(Row) Then
            Dim Bounds As Rectangle = Me.GetRowBounds(Row)
            Bounds.Offset(Me.pnlScrollPanel.AutoScrollPosition)
            Me.pnlScrollPanel.ScrollRectIntoView(Bounds)
        End If
    End Sub

    ''' <summary>
    ''' Resizes the columns in the listview when the header has been updated.
    ''' </summary>
    ''' <param name="sender"></param>
    Protected Overridable Sub OnResizeColumns(ByVal sender As ctlListHeader)
        If Me.mEditRowControl IsNot Nothing Then
            Me.SetEditableColumnWidths(Me.mEditRowControl)
        End If

        Me.Canvas.Invalidate()
    End Sub


    ''' <summary>
    ''' Moves the selected row up, if it is not null, and 
    ''' if it is not already the first row. It
    ''' will be necessary to call UpdateView afterwards
    ''' in order
    ''' </summary>
    ''' <returns>Returns true if the row is successfully
    ''' moved down</returns>
    ''' <remarks></remarks>
    Public Function MoveEditableRowUp() As Boolean
        Dim R As clsListRow = Me.CurrentEditableRow
        If Not R Is Nothing Then
            Dim OldIndex As Integer = Me.Rows.IndexOf(R)
            If Not OldIndex = 0 Then
                Me.Rows.Remove(R)
                Me.Rows.Insert(OldIndex - 1, R)
                Me.CurrentEditableRow = R
                Return True
            End If
        End If

        Return False
    End Function

    ''' <summary>
    ''' Moves the selected row down, if it is not null, and 
    ''' if it is not already the last row. It
    ''' will be necessary to call UpdateView afterwards
    ''' in order
    ''' </summary>
    ''' <returns>Returns true if the row is successfully
    ''' moved down</returns>
    ''' <remarks></remarks>
    Public Function MoveEditableRowdown() As Boolean
        Dim R As clsListRow = Me.CurrentEditableRow
        If Not R Is Nothing Then
            Dim OldIndex As Integer = Me.Rows.IndexOf(R)
            If Not OldIndex = Me.Rows.Count - 1 Then
                Me.Rows.Remove(R)
                Me.Rows.Insert(OldIndex + 1, R)
                Me.CurrentEditableRow = R
                Return True
            End If
        End If

        Return False
    End Function


    ''' <summary>
    ''' Sorts the rows, using the supplied iComparer.
    ''' </summary>
    ''' <param name="Comparer">The iComparer to use when sorting the rows.</param>
    Public Sub SortRows(ByVal Comparer As System.Collections.Generic.IComparer(Of clsListRow))
        Me.mRows.Sort(Comparer)
        Me.PositionEditRow()
        Me.UpdateView()
        Me.mHeader.RePaint()
    End Sub


    Private Sub pnlRowView_HScroll(ByVal sender As Object, ByVal e As System.Windows.Forms.ScrollEventArgs) Handles pnlScrollPanel.HorizontalScroll
        Me.mHeader.ScrollOffset = e.NewValue
    End Sub

    Private Sub ctlListView_EnabledChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.EnabledChanged
        If Me.Enabled Then
            Me.pnlScrollPanel.BackColor = SystemColors.ControlLightLight
            Me.Canvas.BackColor = SystemColors.ControlLightLight
        Else
            Me.pnlScrollPanel.BackColor = Color.DarkGray
            Me.Canvas.BackColor = Color.DarkGray
        End If
    End Sub

    ''' <summary>
    ''' Gets the backcolour of the canvas according to the listview's
    ''' state (eg enabled, readonly, etc)
    ''' </summary>
    Private Function GetCanvasBackColour() As Color
        If Me.Enabled Then
            If Me.Readonly Then
                Return Color.Ivory
            Else
                Return Color.White
            End If
        Else
            Return Color.DarkGray
        End If
    End Function

    Private Sub HandleColumnClicked(ByVal sender As Object, ByVal e As ColumnClickEventArgs) Handles mHeader.ColumnClicked
        Me.EndEditing()
        RaiseEvent ColumnClicked(sender, e)
    End Sub


    ''' <summary>
    ''' Event handler for when resizing finishes on a column.
    ''' </summary>
    ''' <param name="Column">The column whose right hand edge has been adjusted.</param>
    Private Sub HandleColumnResized(ByVal Column As ctlListColumn)
        'Clear any remaining resizing line
        If Not mLastDrawnLineSource.Equals(Point.Empty) Then
            ControlPaint.DrawReversibleLine(mLastDrawnLineSource, mLastDrawnLineDest, Me.BackColor)
            mLastDrawnLineSource = Point.Empty
            mLastDrawnLineDest = Point.Empty
        End If

        'Update the row widths to the new column widths
        Me.OnResizeColumns(Me.mHeader)
    End Sub


    ''' <summary>
    ''' When resizing we draw a line to show the user
    ''' where the new column boundary will go.
    ''' 
    ''' This is the source of the line.
    ''' </summary>
    Private mLastDrawnLineSource As Point
    ''' <summary>
    ''' When resizing we draw a line to show the user
    ''' where the new column boundary will go.
    ''' 
    ''' This is the destination of the line.
    ''' </summary>
    Private mLastDrawnLineDest As Point


    ''' <summary>
    ''' Handles the resizing of a column.
    ''' </summary>
    ''' <param name="Column">The column whose right hand edge is being resized.</param>
    Private Sub HandleColumnResizing(ByVal Column As ctlListColumn)
        Me.EndEditing()
        Me.DrawResizingLine(Column)
    End Sub

    ''' <summary>
    ''' Draws a vertical line down the control in line
    ''' with the right hand edge of the supplied column.
    ''' </summary>
    ''' <param name="Column">The column next to which the 
    ''' line should be drawn.</param>
    ''' <remarks>Primarily useful when resizing the column,
    ''' we can show where its edge will be.</remarks>
    Private Sub DrawResizingLine(ByVal Column As ctlListColumn)
        'Erase previous line
        If Not mLastDrawnLineSource.Equals(Point.Empty) Then
            ControlPaint.DrawReversibleLine(mLastDrawnLineSource, mLastDrawnLineDest, Me.BackColor)
        End If

        'Draw new line
        Dim SourcePoint As Point = Me.PointToScreen(New Point(Column.Right - Column.OwningHeader.ScrollOffset, 0))
        Dim DestPoint As Point = Me.PointToScreen(New Point(Column.Right - Column.OwningHeader.ScrollOffset, Me.Height))
        ControlPaint.DrawReversibleLine(SourcePoint, DestPoint, Me.BackColor)

        'Remember details of last line drawn so that we can erase it later
        Me.mLastDrawnLineSource = SourcePoint
        Me.mLastDrawnLineDest = DestPoint
    End Sub

    ''' <summary>
    ''' Subroutine to handle PreviewkeyDownevents passed up through item and rows for the editcontrols.  
    ''' This was the only way it has been able to trap the keydown events for these controls, probably
    ''' because they have controls within the control (so we have about 5 levels of usercontrol/control)
    ''' </summary>
    ''' <param name="Sender">This will be the ctllistrow that passed this event up</param>
    ''' <param name="e">This will be PreviewKeyDownEventArgs</param>
    ''' <remarks></remarks>
    Private Sub OnEditControlKeyPreview(ByVal Sender As Object, ByVal e As PreviewKeyDownEventArgs)
        If Not Sender Is Nothing Then
            ManageCursorKeys(Sender, e.KeyCode)
        End If
    End Sub

    ''' <summary>
    ''' Handles KeyDownEvents sent all the way from the ctllistitem (and its embedded control 
    ''' if there is one)
    ''' </summary>
    ''' <param name="Sender">This will be the ctllistrow that passed this event up</param>
    ''' <param name="e">This will be KeyEventArgs</param>
    ''' <remarks></remarks>
    Private Sub OnListRowKeyDown(ByVal Sender As Object, ByVal e As KeyEventArgs)
        If Not Sender Is Nothing Then
            ManageCursorKeys(Sender, e.KeyCode)
        End If
    End Sub

    ''' <summary>
    ''' This code will do the cursor up and down with that lovely(?) blue highlighting moving with it.
    ''' </summary>
    ''' <param name="sender">This will be the ctllistrow that passed the key event up</param>
    ''' <param name="key">this will be the key that was pressed (we will only act on up and down)</param>
    ''' <remarks></remarks>
    Private Sub ManageCursorKeys(ByVal sender As Object, ByVal key As Keys)
        If Me.CurrentEditableRow IsNot Nothing Then
            'Remember the selected row
            Dim OriginalSelectedRow As clsListRow = Me.CurrentEditableRow

            'Find out index of focused column
            Dim FocusedColumnIndex As Integer
            If Me.mEditRowControl IsNot Nothing Then
                FocusedColumnIndex = Me.mEditRowControl.GetFocusedColumnIndex
            End If

            'Select the next row up/down as appropriate
            Dim NewIndex As Integer = Me.CurrentEditableRow.Index
            Select Case key
                Case Keys.Up
                    NewIndex = Math.Max(NewIndex - 1, miRowsPerPage * miCurrentPageIndex)
                Case Keys.Down
                    NewIndex = Math.Min(Math.Min(NewIndex + 1, (miCurrentPageIndex + 1) * miRowsPerPage - 1), Me.Rows.Count - 1)
                Case Else
                    Exit Sub
            End Select
            Me.CurrentEditableRow = Me.Rows(NewIndex)

            If Me.CurrentEditableRow IsNot OriginalSelectedRow Then
                Me.mEditRowControl.SetFocusedColumnIndex(FocusedColumnIndex)
            End If
        End If
    End Sub

    ''' <summary>
    ''' The zero-based index of the row under the mousedown event
    ''' </summary>
    Private MouseDownRowIndex As Integer


    ''' <summary>
    ''' The zero-based index of the column under the mousedown event
    ''' </summary>
    Private MouseDownColumnIndex As Integer

    ''' <summary>
    ''' Resolves a point on the canvas as a row/column.
    ''' </summary>
    ''' <param name="P">The point on the canvas of interest. This value should
    ''' already have taken the scroll offset into account.</param>
    ''' <param name="LocalHitPoint">The coordinates of the mouse hit relative
    ''' to the row in which the hit takes place</param>
    ''' <returns>Returns a point containing the index of the row
    ''' and the index of the column clicked, in the x,y fields (respectively).</returns>
    Private Function GetRowColumnIndex(ByVal P As Point, ByRef LocalHitPoint As Point) As Point
        LocalHitPoint.Y = P.Y Mod RowHeight
        LocalHitPoint.X = P.X

        Dim RowIndex As Integer = P.Y \ RowHeight + (miCurrentPageIndex * miRowsPerPage)
        Dim ColumnIndex As Integer = -1
        Dim CumulativeWidth As Integer = 0
        For i As Integer = 0 To mHeader.Columns.Count - 1
            Dim Column As ctlListColumn = mHeader.Columns(i)
            If P.X < CumulativeWidth + Column.Width Then
                ColumnIndex = i
                Exit For
            Else
                CumulativeWidth += Column.Width
            End If
        Next

        Return New Point(ColumnIndex, RowIndex)
    End Function


    Private Sub Canvas_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Canvas.MouseDown
        If Not Me.Readonly Then
            Dim RelativePoint As Point
            Dim CellClicked As Point = Me.GetRowColumnIndex(e.Location, RelativePoint)

            'Inform row that it was clicked
            If CellClicked.Y > -1 AndAlso CellClicked.Y < Me.Rows.Count Then
                Me.Rows(CellClicked.Y).OnMouseDown(RelativePoint)
            End If

            Me.MouseDownRowIndex = CellClicked.Y
            Me.MouseDownColumnIndex = CellClicked.X
        End If
    End Sub


    Private Sub Canvas_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Canvas.MouseUp
        If Not Me.Readonly Then
            Dim RelativePoint As Point
            Dim CellClicked As Point = Me.GetRowColumnIndex(e.Location, RelativePoint)

            'Inform row that it was clicked
            If CellClicked.Y > -1 AndAlso CellClicked.Y < Me.Rows.Count Then
                Me.Rows(CellClicked.Y).OnMouseUp(RelativePoint)
            End If

            'If mouseup/mousedown on same row then that row becomes current
            If MouseDownRowIndex = CellClicked.Y Then

                'Select the row in question
                If MouseDownRowIndex > -1 AndAlso MouseDownRowIndex < mRows.Count Then
                    Me.CurrentEditableRow = Me.mRows(MouseDownRowIndex)
                End If

                'Setting the above property starts editing the row, where appropriate.
                'Need to focus the appropriate column within that row
                Me.FocusEditColumn(CellClicked.X)
            End If
        End If
    End Sub

    ''' <summary>
    ''' Gives keyboard focus to the specified column in the current editable
    ''' row, if such a row exists.
    ''' </summary>
    ''' <param name="ColumnIndex">The zero-based index of the column to
    ''' be focused.</param>
    Public Sub FocusEditColumn(ByVal ColumnIndex As Integer)
        If mEditRowControl IsNot Nothing Then

            'Focus, or otherwise show the chosen column
            If ColumnIndex > -1 AndAlso ColumnIndex < Me.mEditRowControl.Items.Count Then
                Dim ctl As Control = Me.mEditRowControl.Items(ColumnIndex).NestedControl
                If ctl IsNot Nothing AndAlso Not ctl.IsDisposed Then
                    ctl.Focus()
                    ctl.Select()
                    Select Case True
                        Case TypeOf ctl Is ctlExpressionEdit
                            CType(ctl, ctlExpressionEdit).SelectAll()

                        Case TypeOf ctl Is ctlStoreInEdit
                            CType(ctl, ctlStoreInEdit).SelectAll()

                        Case TypeOf ctl Is AutomateControls.ComboBoxes.MonoComboBox
                            CType(ctl, AutomateControls.ComboBoxes.MonoComboBox).DroppedDown = True

                        Case TypeOf ctl Is ComboBox
                            CType(ctl, ComboBox).DroppedDown = True

                    End Select
                End If
            End If
        End If
    End Sub

    ''' <summary>
    ''' Gives keyboard focus to the element of the first column in the first row
    ''' </summary>
    Public Sub FocusFirstGridItem()
        If Rows.Count <= 0 Then Return
        CurrentEditableRow = Rows(0)
        If mEditRowControl Is Nothing Then Return
        Dim ctl As Control = Me.mEditRowControl.Items(0)?.NestedControl
        If ctl IsNot Nothing AndAlso Not ctl.IsDisposed Then
            ctl.Focus()
            ctl.Select()
            Select Case True
                Case TypeOf ctl Is ctlExpressionEdit
                    CType(ctl, ctlExpressionEdit).SelectAll()

                Case TypeOf ctl Is ctlStoreInEdit
                    CType(ctl, ctlStoreInEdit).SelectAll()

                Case TypeOf ctl Is AutomateControls.ComboBoxes.MonoComboBox
                    CType(ctl, AutomateControls.ComboBoxes.MonoComboBox).DroppedDown = True

                Case TypeOf ctl Is ComboBox
                    CType(ctl, ComboBox).DroppedDown = True
            End Select
        End If
    End Sub

    ''' <summary>
    ''' Ends any editing which may be underway; does nothing otherwise.
    ''' </summary>
    ''' <remarks>Any "half-edited" values in the UI will be committed.</remarks>
    Public Sub EndEditing()
        Me.CurrentEditableRow = Nothing
    End Sub

    ''' <summary>
    ''' Adjusts the size of the canvas, according to the number of rows
    ''' present, and the size of the viewport.
    ''' </summary>
    Private Sub SetCanvasSize()
        Dim NumRowsOnCurrentPage As Integer = Math.Min(Me.miRowsPerPage, Me.Rows.Count - (Me.miRowsPerPage * miCurrentPageIndex))
        Me.Canvas.ClientSize = New Size(Math.Max(Me.pnlScrollPanel.DisplayRectangle.Width - SystemInformation.VerticalScrollBarWidth - 1, mHeader.TotalOfColumnWidths), 1 + Math.Max(Me.pnlScrollPanel.DisplayRectangle.Height - SystemInformation.HorizontalScrollBarHeight - 1, NumRowsOnCurrentPage * RowHeight))
    End Sub

    Private Sub pnlRowView_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Canvas.Paint
        Me.SetCanvasSize()
        Dim FirstVisibleRowIndex As Integer = Math.Abs(Me.pnlScrollPanel.AutoScrollPosition.Y) \ RowHeight
        FirstVisibleRowIndex += (miCurrentPageIndex * miRowsPerPage)
        Dim NumVisibleRows As Integer = Math.Min(Me.miRowsPerPage, CInt(Math.Ceiling(Me.pnlScrollPanel.Height / RowHeight)))
        Dim LastVisibleRowIndex As Integer = Math.Min(mRows.Count - 1, FirstVisibleRowIndex + NumVisibleRows - 1)

        e.Graphics.Clear(Me.GetCanvasBackColour)

        'Loop through the rows, drawing each one. Start at the first
        'visible item and stop after the last visible item
        Dim Top As Integer = (FirstVisibleRowIndex - (miCurrentPageIndex * miRowsPerPage)) * RowHeight
        Dim CurrentIndex As Integer = FirstVisibleRowIndex
        Dim TotalWidth As Integer = mHeader.TotalOfColumnWidths
        While CurrentIndex <= LastVisibleRowIndex
            mRows(CurrentIndex).Draw(e.Graphics, Top)
            CurrentIndex += 1
            Top += RowHeight
            e.Graphics.DrawLine(Pens.Silver, 0, Top, TotalWidth, Top)
        End While

        'Draw vertical lines between each column
        Dim CumulativeWidth As Integer
        FirstVisibleRowIndex = FirstVisibleRowIndex Mod Me.miRowsPerPage
        LastVisibleRowIndex = LastVisibleRowIndex Mod Me.miRowsPerPage
        For Each col As ctlListColumn In Me.mHeader.Columns
            CumulativeWidth += col.Width
            e.Graphics.DrawLine(Pens.Silver, CumulativeWidth, FirstVisibleRowIndex * RowHeight, CumulativeWidth, (LastVisibleRowIndex + 1) * RowHeight)
        Next
    End Sub

    ''' <summary>
    ''' Gets the index of the first visible row in the current view, on the 
    ''' current page, if such a row exists.
    ''' </summary>
    ''' <returns>Gets the index of the first visible row, if such a row
    ''' exists. Returns -1 otherwise.</returns>
    Private Function GetFirstVisibleRowIndex() As Integer
        If Me.Rows.Count > 0 Then
            Dim FirstVisibleRowIndex As Integer = Math.Abs(Me.pnlScrollPanel.AutoScrollPosition.Y) \ RowHeight
            FirstVisibleRowIndex += (miCurrentPageIndex * miRowsPerPage)
            Return FirstVisibleRowIndex
        Else
            Return -1
        End If
    End Function


    ''' <summary>
    ''' Magic number for now. Maybe in the future we will make
    ''' this dynamic based on the preferred height of each cell's
    ''' contents.
    ''' </summary>
    Public Const DefaultRowHeight As Integer = 26

    Public Property RowHeight() As Integer
        Get
            Return mRowHeight
        End Get
        Set(ByVal value As Integer)
            mRowHeight = value
        End Set
    End Property
    Private mRowHeight As Integer = DefaultRowHeight

    Private Sub pnlRowView_VScrollChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles pnlScrollPanel.VScrollChanged
        Me.mHeader.UpdateColumns()
    End Sub

    Private Sub ctlListView_SizeChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.SizeChanged
        Me.mHeader.Width = Me.Width
        Me.PositionEditRow()
    End Sub

    ''' <summary>
    ''' Override the dispose method to ensure all rows are disposed of cleanly
    ''' </summary>
    ''' <param name="disposing"></param>
    ''' <remarks></remarks>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If mRows IsNot Nothing Then
                For Each r As clsListRow In mRows
                    r.Dispose()
                Next
                mRows = Nothing
            End If

            If Me.mEditRowControl IsNot Nothing Then
                Me.mEditRowControl.Dispose()
                Me.mEditRowControl = Nothing
            End If
        End If

        MyBase.Dispose(disposing)
    End Sub

    Private Sub Canvas_Scroll(ByVal sender As Object, ByVal e As System.Windows.Forms.ScrollEventArgs) Handles pnlScrollPanel.VerticalScroll, pnlScrollPanel.HorizontalScroll
        Me.Canvas.Invalidate()
    End Sub

    ''' <summary>
    ''' Shows the pagination controls, if they are not already visible
    ''' </summary>
    Private Sub ShowPaginationControls()
        If Not Me.pnlPaginationControls.Visible Then
            MyBase.SuspendLayout()
            Me.pnlScrollPanel.Height -= Me.pnlPaginationControls.Height
            Me.pnlPaginationControls.Top = Me.pnlScrollPanel.Bottom + 1
            Me.pnlPaginationControls.Visible = True
            Me.UpdatePaginationDisplay()
            MyBase.ResumeLayout(True)
        End If
    End Sub

    ''' <summary>
    ''' Hides the pagination control, if they are visible.
    ''' </summary>
    Private Sub HidePaginationControls()
        If Me.pnlPaginationControls.Visible Then
            MyBase.SuspendLayout()
            Me.pnlScrollPanel.Height += Me.pnlPaginationControls.Height
            Me.pnlPaginationControls.Visible = False
            Me.miCurrentPageIndex = 0
            MyBase.ResumeLayout(True)
        End If
    End Sub

    ''' <summary>
    ''' The number of rows to be viewed on each page
    ''' </summary>
    Private miRowsPerPage As Integer = 1000

    ''' <summary>
    ''' The zero-based index of the page currently being viewed
    ''' </summary>
    Private miCurrentPageIndex As Integer

    Private Sub btnFirst_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFirst.Click
        Me.EndEditing()
        miCurrentPageIndex = 0
        Me.pnlScrollPanel.AutoScrollPosition = Point.Empty
        Me.UpdatePaginationDisplay()
        Me.UpdateView()
    End Sub

    Private Sub btnPrevious_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPrevious.Click
        Me.EndEditing()
        If miCurrentPageIndex > 0 Then
            miCurrentPageIndex -= 1
            Me.pnlScrollPanel.AutoScrollPosition = Point.Empty
            Me.UpdatePaginationDisplay()
            Me.UpdateView()
        End If
    End Sub

    Private Sub btnNext_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnNext.Click
        Me.EndEditing()
        Dim NumPages As Integer = CInt(Math.Ceiling(mRows.Count / miRowsPerPage))
        If miCurrentPageIndex < NumPages - 1 Then
            miCurrentPageIndex += 1
            Me.pnlScrollPanel.AutoScrollPosition = Point.Empty
            Me.UpdatePaginationDisplay()
            Me.UpdateView()
        End If
    End Sub

    Private Sub btnLast_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnLast.Click
        Me.EndEditing()
        Dim NumPages As Integer = CInt(Math.Ceiling(mRows.Count / miRowsPerPage))
        If miCurrentPageIndex < NumPages - 1 Then
            miCurrentPageIndex = NumPages - 1
            Me.pnlScrollPanel.AutoScrollPosition = Point.Empty
            Me.UpdatePaginationDisplay()
            Me.UpdateView()
        End If
    End Sub

    ''' <summary>
    ''' Updates the 'enabled' status of each pagination buttons.
    ''' </summary>
    Private Sub UpdatePaginationButtons()
        Dim NumPages As Integer = CInt(Math.Ceiling(mRows.Count / miRowsPerPage))
        Me.btnFirst.Enabled = miCurrentPageIndex > 0
        Me.btnPrevious.Enabled = Me.btnFirst.Enabled

        Me.btnNext.Enabled = miCurrentPageIndex < NumPages - 1
        Me.btnLast.Enabled = Me.btnNext.Enabled
    End Sub

    ''' <summary>
    ''' Updates all features of the pagination display.
    ''' </summary>
    Private Sub UpdatePaginationDisplay()
        Dim NumPages As Integer = CInt(Math.Ceiling(mRows.Count / miRowsPerPage))
        Me.lblPageNum.Text = String.Format(My.Resources.ctlListView_Page0Of1, (miCurrentPageIndex + 1).ToString, NumPages.ToString)

        Dim Offset As Integer = (lblPageNum.Right + 8) - Me.btnNext.Left
        Me.btnNext.Left += Offset
        Me.btnLast.Left += Offset

        Me.txtRowsPerPage.Text = Me.miRowsPerPage.ToString
        Me.UpdatePaginationButtons()
    End Sub

    Private Sub txtRowsPerPage_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtRowsPerPage.KeyDown
        If e.KeyCode = Keys.Enter Then
            ApplyRowsPerPage()
        End If
    End Sub


    ''' <summary>
    ''' Reads the value in the rows per page box, and applies it, if it is
    ''' valid.
    ''' </summary>
    Private Sub ApplyRowsPerPage()
        Const MessageDuration As Integer = 3000 'Duration of tooltip messages, in milliseconds
        Dim Value As Integer
        If Integer.TryParse(Me.txtRowsPerPage.Text, Value) Then
            If Value > 0 Then
                Const MaxValue As Integer = 1000
                If Value <= MaxValue Then
                    If miRowsPerPage <> Value Then

                        'Make sure that after adjusting the rows per page we will
                        'still be looking at the row of interest
                        Dim RowOfInterest As clsListRow = Me.CurrentEditableRow
                        If RowOfInterest Is Nothing Then
                            RowOfInterest = Me.GetRowFromIndex(Me.GetFirstVisibleRowIndex)
                        End If
                        If RowOfInterest IsNot Nothing Then
                            Me.miCurrentPageIndex = RowOfInterest.Index \ Value
                        End If
                        Me.miRowsPerPage = Value
                        If RowOfInterest IsNot Nothing Then
                            Me.PositionEditRow()
                            RowOfInterest.EnsureVisible()
                        End If

                        Me.UpdatePaginationDisplay()
                        Me.UpdateView()
                    End If
                Else
                    UserMessage.ShowFloating(Me.txtRowsPerPage, ToolTipIcon.Info, "Limit exceeded", String.Format(My.Resources.ctlListView_TheMaximumAcceptableValueIs0, MaxValue.ToString), New Point(Me.txtRowsPerPage.Size), MessageDuration, True)
                End If
            Else
                UserMessage.ShowFloating(Me.txtRowsPerPage, ToolTipIcon.Error, My.Resources.ctlListView_InvalidValue, My.Resources.ctlListView_YouMustChooseAValueGreaterThanZeroRowsPerPage, New Point(Me.txtRowsPerPage.Size), MessageDuration, True)
            End If
        Else
            UserMessage.ShowFloating(Me.txtRowsPerPage, ToolTipIcon.Info, My.Resources.ctlListView_NotANumber, My.Resources.ctlListView_TheSuppliedValueWasNotRecognisedAsAValidNumberPleaseTryAgain, New Point(Me.txtRowsPerPage.Size), MessageDuration, True)
        End If
    End Sub

    Private Function GetRowFromIndex(ByVal Index As Integer) As clsListRow
        If Index > -1 AndAlso Index < Me.Rows.Count Then
            Return Me.Rows(Index)
        Else
            Return Nothing
        End If
    End Function

    ''' <summary>
    ''' Handles an edit control being activated on the current editable row.
    ''' This is usually the result of a collection being activated.
    ''' </summary>
    ''' <param name="sender">The process value control which has been activated.
    ''' </param>
    ''' <param name="e">The args detailing the event.</param>
    Private Sub HandleEditControlActivated(ByVal sender As clsListRow, ByVal val As IActivatableProcessValue, ByVal e As EventArgs) _
        Handles mCurrentEditableRow.EditControlActivated

        Dim index As Integer = -1
        For i As Integer = 0 To mEditRowControl.Items.Count - 1
            If mEditRowControl.Items(i).NestedControl Is val Then index = i
        Next
        If index = -1 Then Return
        RaiseEvent ValueControlActivated(Me, mHeader.Columns(index).Text, val)
    End Sub

    Protected Overrides Sub OnDragOver(ByVal drgevent As System.Windows.Forms.DragEventArgs)
        If Not Me.Readonly Then
            Dim CanvasPoint As Point = Me.Canvas.PointToClient(New Point(drgevent.X, drgevent.Y))
            Dim LocalPoint As Point
            Dim CellIndex As Point = Me.GetRowColumnIndex(CanvasPoint, LocalPoint)

            Dim TargetRow As clsListRow = Me.GetRowFromIndex(CellIndex.Y)
            If TargetRow IsNot Nothing Then
                Dim RowBounds As Rectangle = Me.GetRowBounds(TargetRow)
                TargetRow.OnDragOver(drgevent, LocalPoint, CellIndex.X)
            End If

            If Me.LastCellDraggedOver <> CellIndex Then
                Me.UnhighlightLastDraggedCell()
            End If
            LastCellDraggedOver = CellIndex

            MyBase.OnDragOver(drgevent)
        End If
    End Sub

    Private LastCellDraggedOver As Point

    Protected Overrides Sub OnDragDrop(ByVal e As DragEventArgs)
        If Not Me.Readonly Then
            Dim CanvasPoint As Point = Me.Canvas.PointToClient(New Point(e.X, e.Y))
            Dim LocalPoint As Point
            Dim CellIndex As Point = Me.GetRowColumnIndex(CanvasPoint, LocalPoint)

            Dim TargetRow As clsListRow = Me.GetRowFromIndex(CellIndex.Y)
            If TargetRow IsNot Nothing Then
                Dim RowBounds As Rectangle = Me.GetRowBounds(TargetRow)
                TargetRow.OnDragDrop(e, LocalPoint, CellIndex.X)
            End If

            MyBase.OnDragDrop(e)
        End If
    End Sub

    Protected Overrides Sub OnDragLeave(ByVal e As System.EventArgs)
        If Not Me.Readonly Then
            Me.UnhighlightLastDraggedCell()

            MyBase.OnDragLeave(e)
        End If
    End Sub

    Private Sub UnhighlightLastDraggedCell()
        If LastCellDraggedOver <> New Point(-1, -1) Then
            Dim LastRow As clsListRow = Me.GetRowFromIndex(LastCellDraggedOver.Y)
            If LastRow IsNot Nothing Then
                LastRow.OnDragLeave(LastCellDraggedOver.X)
            End If
            LastCellDraggedOver = New Point(-1, -1)
        End If
    End Sub

    Public Sub InvalidateRow(ByVal Row As clsListRow)
        If Me.Rows.Contains(Row) Then
            Dim RowBounds As Rectangle = Me.GetRowBounds(Row)
            Me.Canvas.Invalidate(RowBounds)
        End If
    End Sub

End Class
