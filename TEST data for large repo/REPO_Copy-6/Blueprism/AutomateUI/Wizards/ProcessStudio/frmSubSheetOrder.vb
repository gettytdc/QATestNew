Imports BluePrism.AutomateProcessCore

''' Project  : Automate
''' Class    : frmSubSheetOrder
''' 
''' <summary>
''' A form for managing process pages.
''' </summary>
Friend Class frmSubSheetOrder : Inherits frmForm

#Region " Windows Form Designer generated code "


    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents btnUp As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnDown As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnOK As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnCopy As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnCut As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
    Friend WithEvents btnDelete As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents lvSubSheets As AutomateControls.DetailListView
    Friend WithEvents ColumnHeader1 As System.Windows.Forms.ColumnHeader
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSubSheetOrder))
        Me.btnUp = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnDown = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnOK = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnCopy = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnCut = New AutomateControls.Buttons.StandardStyledButton()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.btnDelete = New AutomateControls.Buttons.StandardStyledButton()
        Me.lvSubSheets = New AutomateControls.DetailListView()
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.SuspendLayout()
        '
        'btnUp
        '
        resources.ApplyResources(Me.btnUp, "btnUp")
        Me.btnUp.Cursor = System.Windows.Forms.Cursors.Default
        Me.btnUp.Image = Global.AutomateUI.My.Resources.ToolImages.Nudge_Up_16x16
        Me.btnUp.Name = "btnUp"
        Me.ToolTip1.SetToolTip(Me.btnUp, resources.GetString("btnUp.ToolTip"))
        '
        'btnDown
        '
        resources.ApplyResources(Me.btnDown, "btnDown")
        Me.btnDown.Image = Global.AutomateUI.My.Resources.ToolImages.Nudge_Down_16x16
        Me.btnDown.Name = "btnDown"
        Me.ToolTip1.SetToolTip(Me.btnDown, resources.GetString("btnDown.ToolTip"))
        '
        'btnOK
        '
        resources.ApplyResources(Me.btnOK, "btnOK")
        Me.btnOK.Name = "btnOK"
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.Name = "btnCancel"
        '
        'btnCopy
        '
        resources.ApplyResources(Me.btnCopy, "btnCopy")
        Me.btnCopy.Image = Global.AutomateUI.My.Resources.ToolImages.Copy_16x16
        Me.btnCopy.Name = "btnCopy"
        Me.ToolTip1.SetToolTip(Me.btnCopy, resources.GetString("btnCopy.ToolTip"))
        '
        'btnCut
        '
        resources.ApplyResources(Me.btnCut, "btnCut")
        Me.btnCut.Image = Global.AutomateUI.My.Resources.ToolImages.Cut_16x16
        Me.btnCut.Name = "btnCut"
        Me.ToolTip1.SetToolTip(Me.btnCut, resources.GetString("btnCut.ToolTip"))
        '
        'btnDelete
        '
        resources.ApplyResources(Me.btnDelete, "btnDelete")
        Me.btnDelete.Image = Global.AutomateUI.My.Resources.ToolImages.Delete_Red_16x16
        Me.btnDelete.Name = "btnDelete"
        Me.ToolTip1.SetToolTip(Me.btnDelete, resources.GetString("btnDelete.ToolTip"))
        '
        'lvSubSheets
        '
        resources.ApplyResources(Me.lvSubSheets, "lvSubSheets")
        Me.lvSubSheets.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1})
        Me.lvSubSheets.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None
        Me.lvSubSheets.LabelEdit = True
        Me.lvSubSheets.Name = "lvSubSheets"
        Me.lvSubSheets.UseCompatibleStateImageBehavior = False
        '
        'ColumnHeader1
        '
        resources.ApplyResources(Me.ColumnHeader1, "ColumnHeader1")
        '
        'frmSubSheetOrder
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.lvSubSheets)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnDelete)
        Me.Controls.Add(Me.btnCut)
        Me.Controls.Add(Me.btnCopy)
        Me.Controls.Add(Me.btnDown)
        Me.Controls.Add(Me.btnUp)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmSubSheetOrder"
        Me.ResumeLayout(False)

    End Sub

#End Region

#Region " Member Variables and Properties "

    'a reference to the parent process
    Private mProcess As clsProcess

    Private mProcessViewer As ctlProcessViewer

    ' The subsheet IDs that we are operating on. All subsheets are represented here,
    ' even if they are not displayed in the UI.
    Private mSubSheets As IList(Of KeyValuePair(Of Guid, String))

    ' The offset within mSubSheets that the UI displays from, skipping the 'Main'/
    ' 'Initialise' and 'Clean Up' pages.
    Private mIndexOffset As Integer

    ' Provide access to modified subsheet list
    Public ReadOnly Property SubSheets() As IList(Of KeyValuePair(Of Guid, String))
        Get
            Return mSubSheets
        End Get
    End Property

#End Region

#Region " Constructors and Destructors "

    ''' <summary>
    ''' Creates a new form to change the sheet order in a process
    ''' </summary>
    ''' <param name="process">The process whose sheets orders are to be changed.
    ''' </param>
    Public Sub New(ByVal process As clsProcess, processViewer As ctlProcessViewer)

        MyBase.New()
        InitializeComponent()

        mProcess = process
        mProcessViewer = processViewer

        'Collect all the subsheet ID's...
        mSubSheets = New List(Of KeyValuePair(Of Guid, String))
        For Each sheet As clsProcessSubSheet In mProcess.SubSheets
            mSubSheets.Add(New KeyValuePair(Of Guid, String)(sheet.ID, sheet.Name))
        Next

        'Populate the list box...
        PopulateList(mSubSheets)
        SetButtons()

    End Sub

    ''' <summary>
    ''' Disposes of this form
    ''' </summary>
    ''' <param name="disposing">True if disposing explicitly.</param>
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

#End Region

#Region " Methods "

    ''' <summary>
    ''' Populates a listbox with the names of the subsheets
    ''' </summary>
    ''' <param name="subSheetIds">The collection of subsheet IDs.</param>
    Private Sub PopulateList(ByVal subSheetIds As IList(Of KeyValuePair(Of Guid, String)))
        Dim index As Integer = 0
        For Each sheet As KeyValuePair(Of Guid, String) In subSheetIds
            Dim sh As clsProcessSubSheet = mProcess.GetSubSheetByID(sheet.Key)
            If sh.IsNormal Then
                Dim s As New ListViewItem(sh.Name)
                s.Tag = sh.ID
                lvSubSheets.Items.Add(s)
            Else
                mIndexOffset += 1
                ' We assume that any non-orderable sheets are at the beginning of the
                ' subsheet list. If that's the case then mIndexOffset should always
                ' point to one beyond the current index. This assumption failed when
                ' you could insert a page between the "Initialise" and "Clean Up"
                ' pages - see bug 5364
                Debug.Assert(index + 1 = mIndexOffset, _
                 "The base pages for this process are in the wrong order")
            End If
            index += 1
        Next
    End Sub

    ''' <summary>
    ''' Moves listbox items up or down the list.
    ''' </summary>
    ''' <param name="up">A flag indicating if the move is up or down</param>
    ''' <returns>True if a move has been performed. When a move cannot be made 
    ''' (for example the last item cannot go down any further) False is returned.
    ''' </returns>
    Private Function MoveListViewItem(ByVal up As Boolean) As Boolean

        'do nothing unless an item has been selected
        If lvSubSheets.SelectedItems.Count = 0 Then Return False

        'decide if the intended move can be done
        Dim selected(lvSubSheets.SelectedItems.Count - 1) As Integer
        lvSubSheets.SelectedIndices.CopyTo(selected, 0)

        If up Then
            If selected(0) = 0 Then Return False

            lvSubSheets.SelectedItems.Clear()
            For i As Integer = 0 To selected.Length - 1
                Dim itemIndex As Integer = selected(i)
                Dim newItem As ListViewItem = CType(lvSubSheets.Items(itemIndex).Clone(), ListViewItem)

                lvSubSheets.Items.RemoveAt(itemIndex)
                lvSubSheets.Items.Insert(itemIndex - 1, newItem)

                mSubSheets.RemoveAt(itemIndex + mIndexOffset)
                mSubSheets.Insert(itemIndex + mIndexOffset - 1, New KeyValuePair(Of Guid, String)(CType(newItem.Tag, Guid), newItem.Text))

                lvSubSheets.SelectedIndices.Add(itemIndex - 1)
            Next
        Else
            If selected(selected.Length - 1) = lvSubSheets.Items.Count - 1 Then Return False

            lvSubSheets.SelectedItems.Clear()
            For i As Integer = selected.Length - 1 To 0 Step -1
                Dim itemIndex As Integer = selected(i)
                Dim newItem As ListViewItem = CType(lvSubSheets.Items(itemIndex).Clone(), ListViewItem)

                lvSubSheets.Items.RemoveAt(itemIndex)
                lvSubSheets.Items.Insert(itemIndex + 1, newItem)

                mSubSheets.RemoveAt(itemIndex + mIndexOffset)
                mSubSheets.Insert(itemIndex + mIndexOffset + 1, New KeyValuePair(Of Guid, String)(CType(newItem.Tag, Guid), newItem.Text))

                lvSubSheets.SelectedIndices.Add(itemIndex + 1)
            Next
        End If

        Return True
    End Function

    ''' <summary>
    '''  Remove selected sheets from the list
    ''' </summary>
    Private Sub DeleteSheets()

        For i As Integer = lvSubSheets.SelectedIndices.Count - 1 To 0 Step -1
            Dim itemIndex As Integer = lvSubSheets.SelectedIndices(i)


            Dim sheetGuid = mSubSheets(itemIndex + mIndexOffset).Key
            Dim sheetName = mSubSheets(itemIndex + mIndexOffset).Value
            Dim sheet = mProcess.GetSubSheetByID(sheetGuid)

            If sheet IsNot Nothing AndAlso mProcessViewer.GetOpenPropertiesWindowsOnSubSheet(sheet).Any Then
                If UserMessage.OK(String.Format(My.Resources.ThePageCannotBeDeletedWhilePropertyDialogsAreOpen, sheetName)) = MsgBoxResult.Ok Then
                    Return
                End If
            End If

            lvSubSheets.Items.RemoveAt(itemIndex)
            mSubSheets.RemoveAt(itemIndex + mIndexOffset)
        Next

        lvSubSheets.SelectedItems.Clear()
    End Sub

    ''' <summary>
    ''' Set button availability according to selected sheets
    ''' </summary>
    Private Sub SetButtons()
        ' Initialise all to disabled
        btnUp.Enabled = False
        btnDown.Enabled = False
        btnCopy.Enabled = False
        btnCut.Enabled = False
        btnDelete.Enabled = False

        ' If sheets selected enable copy/cut/delete
        If lvSubSheets.SelectedItems.Count > 0 Then
            btnCopy.Enabled = True
            btnCut.Enabled = True
            btnDelete.Enabled = True
            ' If top sheet not selected then enable move up
            If lvSubSheets.SelectedIndices(0) > 0 Then
                btnUp.Enabled = True
            End If
            ' If bottom sheet not selected then enable move down
            If lvSubSheets.SelectedIndices(lvSubSheets.SelectedIndices.Count - 1) < lvSubSheets.Items.Count - 1 Then
                btnDown.Enabled = True
            End If
        End If
    End Sub

#End Region

#Region " Event Handlers "

    ''' <summary>
    ''' Handles the 'up' arrow button being clicked
    ''' </summary>
    Private Sub btnUp_Click( _
     ByVal sender As Object, ByVal e As EventArgs) Handles btnUp.Click
        MoveListViewItem(True)
    End Sub

    ''' <summary>
    ''' Handles the 'down' arrow button being clicked
    ''' </summary>
    Private Sub btnDown_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnDown.Click
        MoveListViewItem(False)
    End Sub

    ''' <summary>
    ''' Handles the 'Copy' button being clicked
    ''' </summary>
    Private Sub btnCopy_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnCopy.Click
        ' Ignore if nothing selected
        If lvSubSheets.SelectedItems.Count = 0 Then Exit Sub

        ' Get IDs of selected sheets
        Dim sheetIDs As New List(Of Guid)
        For Each item As ListViewItem In lvSubSheets.SelectedItems
            sheetIDs.Add(CType(item.Tag, Guid))
        Next

        ' Generate XML & copy to clipboard
        Clipboard.SetDataObject(mProcess.GeneratePageSelectionXML(sheetIDs))
    End Sub

    ''' <summary>
    ''' Handles the 'Cut' button being clicked
    ''' </summary>
    Private Sub btnCut_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnCut.Click
        ' Ignore if nothing selected
        If lvSubSheets.SelectedItems.Count = 0 Then Exit Sub

        ' Get IDs of selected sheets
        Dim sheetIDs As New List(Of Guid)
        For Each item As ListViewItem In lvSubSheets.SelectedItems
            sheetIDs.Add(CType(item.Tag, Guid))
        Next

        ' Generate XML & copy to clipboard
        Clipboard.SetDataObject(mProcess.GeneratePageSelectionXML(sheetIDs))

        ' Remove selected sheets
        DeleteSheets()
    End Sub

    ''' <summary>
    ''' Handles the 'Delete' button being clicked.
    ''' </summary>
    Private Sub btnDelete_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnDelete.Click
        ' Remove selected sheets
        DeleteSheets()
    End Sub

    ''' <summary>
    ''' Handles the 'Cancel' button being clicked.
    ''' </summary>
    Private Sub btnCancel_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnCancel.Click
        DialogResult = DialogResult.Cancel
        Close()
    End Sub

    ''' <summary>
    ''' Handles the 'OK' button being clicked
    ''' </summary>
    Private Sub btnOK_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnOK.Click
        ' Check for any deleted sheets
        Dim deleted As Integer = mProcess.SubSheets.Count - (lvSubSheets.Items.Count + mIndexOffset)
        If deleted > 0 Then
            If UserMessage.YesNo(String.Format(My.Resources.frmSubSheetOrder_AreYouSureYouWantToDelete0Pages, deleted)) = MsgBoxResult.No Then
                Return
            End If
        End If
        DialogResult = DialogResult.OK
        Close()
    End Sub

    ''' <summary>
    ''' Handles a subsheet name being edited
    ''' </summary>
    Private Sub lvSubSheets_AfterLabelEdit(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LabelEditEventArgs) _
     Handles lvSubSheets.AfterLabelEdit
        ' Ignore if unchanged
        If e.Label Is Nothing Then
            Return
        End If

        ' Check sheet name is unique
        For Each kvp As KeyValuePair(Of Guid, String) In mSubSheets
            If e.Label = kvp.Value Then
                e.CancelEdit = True
                UserMessage.Show(My.Resources.frmSubSheetOrder_APageWithThatNameAlreadyExistsPleaseChooseAnother)
                Return
            End If
        Next

        ' Create new item containing orginal sheet ID and new name
        Dim item As New KeyValuePair(Of Guid, String)(mSubSheets(e.Item + mIndexOffset).Key, e.Label)

        ' Remove old item & insert new
        mSubSheets.RemoveAt(e.Item + mIndexOffset)
        mSubSheets.Insert(e.Item + mIndexOffset, item)
    End Sub

    ''' <summary>
    ''' Handles resize of listview (stretch column to full width)
    ''' </summary>
    Private Sub lvSubSheets_Resize(ByVal sender As System.Object, ByVal e As System.EventArgs) _
     Handles lvSubSheets.Resize
        If lvSubSheets.Columns.Count > 0 Then _
            lvSubSheets.Columns.Item(0).Width = lvSubSheets.Width - 5
    End Sub

    ''' <summary>
    ''' Handles changes to selected items (set button availability)
    ''' </summary>
    Private Sub lvSubSheets_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) _
     Handles lvSubSheets.SelectedIndexChanged
        SetButtons()
    End Sub

#End Region

End Class
