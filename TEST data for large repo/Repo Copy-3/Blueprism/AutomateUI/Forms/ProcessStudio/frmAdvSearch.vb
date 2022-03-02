Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.AutomateAppCore
Imports BluePrism.Scheduling.Calendar
Imports LocaleTools
Imports BluePrism.AutomateAppCore.Utility

''' Project  : Automate
''' Class    : frmAdvSearch
''' 
'''
''' <summary>
''' The advanced process search form.
''' </summary>
Friend Class frmAdvSearch
    Inherits AutomateControls.Forms.HelpButtonForm

#Region " Windows Form Designer generated code "


    Public Sub New(ByVal ParentProcessViewingForm As IProcessViewingForm, ByVal ParentSearchControl As DiagramSearchToolstrip)
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        InitialiseTreeView()
        Me.SetProcessStudioParent(ParentProcessViewingForm)
        Me.mobjParentProcessSearchControl = ParentSearchControl
        AddHandler mobjParentProcessSearchControl.SearchFeedBack, AddressOf HandleSearchFeedback
        AddHandler mobjParentProcessSearchControl.NewSearch, AddressOf HandleNewSearch
        cboAdvSearch.Text = mobjParentProcessSearchControl.SearchText
        If Len(cboAdvSearch.Text) > 0 Then
            PopulateTreeView()
        End If

        Me.PopulateSearchTypes()
    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents btnFind As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents chkMatchCase As System.Windows.Forms.CheckBox
    Friend WithEvents chkWholeWord As System.Windows.Forms.CheckBox
    Friend WithEvents chkSearchUp As System.Windows.Forms.CheckBox
    Friend WithEvents lblFind As System.Windows.Forms.Label
    Friend WithEvents stsFind As System.Windows.Forms.StatusBar
    Friend WithEvents stsFindPanel1 As System.Windows.Forms.StatusBarPanel
    Friend WithEvents lstSearch As System.Windows.Forms.ListView
    Friend WithEvents ColumnPage As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnName As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnDescription As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnType As System.Windows.Forms.ColumnHeader
    Friend WithEvents TrVwStages As System.Windows.Forms.TreeView
    Friend WithEvents cboAdvSearch As System.Windows.Forms.ComboBox
    Friend WithEvents cmbUse As AutomateControls.ComboBoxes.MonoComboBox
    Friend WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
    Friend WithEvents chkSearchElements As System.Windows.Forms.CheckBox
    Friend WithEvents tcOptions As System.Windows.Forms.TabControl
    Friend WithEvents tpFindText As System.Windows.Forms.TabPage
    Friend WithEvents tpDependencyView As System.Windows.Forms.TabPage
    Friend WithEvents SplitContainer2 As System.Windows.Forms.SplitContainer
    Friend WithEvents tvReferences As System.Windows.Forms.TreeView
    Friend WithEvents TreeImages As System.Windows.Forms.ImageList
    Friend WithEvents StageImages As System.Windows.Forms.ImageList
    Friend WithEvents dgvStages As System.Windows.Forms.DataGridView
    Friend WithEvents img As System.Windows.Forms.DataGridViewImageColumn
    Friend WithEvents Page As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Stage As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents chkUse As System.Windows.Forms.CheckBox
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmAdvSearch))
        Me.StageImages = New System.Windows.Forms.ImageList(Me.components)
        Me.TreeImages = New System.Windows.Forms.ImageList(Me.components)
        Me.tcOptions = New System.Windows.Forms.TabControl()
        Me.tpFindText = New System.Windows.Forms.TabPage()
        Me.SplitContainer1 = New System.Windows.Forms.SplitContainer()
        Me.TrVwStages = New System.Windows.Forms.TreeView()
        Me.lstSearch = New System.Windows.Forms.ListView()
        Me.ColumnPage = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnName = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnDescription = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnType = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.chkSearchElements = New System.Windows.Forms.CheckBox()
        Me.btnFind = New AutomateControls.Buttons.StandardStyledButton()
        Me.cmbUse = New AutomateControls.ComboBoxes.MonoComboBox()
        Me.chkMatchCase = New System.Windows.Forms.CheckBox()
        Me.chkUse = New System.Windows.Forms.CheckBox()
        Me.chkWholeWord = New System.Windows.Forms.CheckBox()
        Me.chkSearchUp = New System.Windows.Forms.CheckBox()
        Me.lblFind = New System.Windows.Forms.Label()
        Me.cboAdvSearch = New System.Windows.Forms.ComboBox()
        Me.tpDependencyView = New System.Windows.Forms.TabPage()
        Me.SplitContainer2 = New System.Windows.Forms.SplitContainer()
        Me.tvReferences = New System.Windows.Forms.TreeView()
        Me.dgvStages = New System.Windows.Forms.DataGridView()
        Me.img = New System.Windows.Forms.DataGridViewImageColumn()
        Me.Page = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Stage = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.stsFind = New System.Windows.Forms.StatusBar()
        Me.stsFindPanel1 = New System.Windows.Forms.StatusBarPanel()
        Me.tcOptions.SuspendLayout()
        Me.tpFindText.SuspendLayout()
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer1.Panel1.SuspendLayout()
        Me.SplitContainer1.Panel2.SuspendLayout()
        Me.SplitContainer1.SuspendLayout()
        Me.tpDependencyView.SuspendLayout()
        CType(Me.SplitContainer2, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SplitContainer2.Panel1.SuspendLayout()
        Me.SplitContainer2.Panel2.SuspendLayout()
        Me.SplitContainer2.SuspendLayout()
        CType(Me.dgvStages, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.stsFindPanel1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'StageImages
        '
        Me.StageImages.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit
        resources.ApplyResources(Me.StageImages, "StageImages")
        Me.StageImages.TransparentColor = System.Drawing.Color.Transparent
        '
        'TreeImages
        '
        Me.TreeImages.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit
        resources.ApplyResources(Me.TreeImages, "TreeImages")
        Me.TreeImages.TransparentColor = System.Drawing.Color.Transparent
        '
        'tcOptions
        '
        resources.ApplyResources(Me.tcOptions, "tcOptions")
        Me.tcOptions.Controls.Add(Me.tpFindText)
        Me.tcOptions.Controls.Add(Me.tpDependencyView)
        Me.tcOptions.Name = "tcOptions"
        Me.tcOptions.SelectedIndex = 0
        '
        'tpFindText
        '
        Me.tpFindText.Controls.Add(Me.SplitContainer1)
        Me.tpFindText.Controls.Add(Me.chkSearchElements)
        Me.tpFindText.Controls.Add(Me.btnFind)
        Me.tpFindText.Controls.Add(Me.cmbUse)
        Me.tpFindText.Controls.Add(Me.chkMatchCase)
        Me.tpFindText.Controls.Add(Me.chkUse)
        Me.tpFindText.Controls.Add(Me.chkWholeWord)
        Me.tpFindText.Controls.Add(Me.chkSearchUp)
        Me.tpFindText.Controls.Add(Me.lblFind)
        Me.tpFindText.Controls.Add(Me.cboAdvSearch)
        resources.ApplyResources(Me.tpFindText, "tpFindText")
        Me.tpFindText.Name = "tpFindText"
        Me.tpFindText.UseVisualStyleBackColor = True
        '
        'SplitContainer1
        '
        resources.ApplyResources(Me.SplitContainer1, "SplitContainer1")
        Me.SplitContainer1.Name = "SplitContainer1"
        '
        'SplitContainer1.Panel1
        '
        Me.SplitContainer1.Panel1.Controls.Add(Me.TrVwStages)
        '
        'SplitContainer1.Panel2
        '
        Me.SplitContainer1.Panel2.Controls.Add(Me.lstSearch)
        '
        'TrVwStages
        '
        resources.ApplyResources(Me.TrVwStages, "TrVwStages")
        Me.TrVwStages.Name = "TrVwStages"
        Me.TrVwStages.TabStop = False
        '
        'lstSearch
        '
        Me.lstSearch.AllowColumnReorder = True
        Me.lstSearch.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnPage, Me.ColumnName, Me.ColumnDescription, Me.ColumnType})
        resources.ApplyResources(Me.lstSearch, "lstSearch")
        Me.lstSearch.FullRowSelect = True
        Me.lstSearch.MultiSelect = False
        Me.lstSearch.Name = "lstSearch"
        Me.lstSearch.TabStop = False
        Me.lstSearch.UseCompatibleStateImageBehavior = False
        Me.lstSearch.View = System.Windows.Forms.View.Details
        '
        'ColumnPage
        '
        resources.ApplyResources(Me.ColumnPage, "ColumnPage")
        '
        'ColumnName
        '
        resources.ApplyResources(Me.ColumnName, "ColumnName")
        '
        'ColumnDescription
        '
        resources.ApplyResources(Me.ColumnDescription, "ColumnDescription")
        '
        'ColumnType
        '
        resources.ApplyResources(Me.ColumnType, "ColumnType")
        '
        'chkSearchElements
        '
        resources.ApplyResources(Me.chkSearchElements, "chkSearchElements")
        Me.chkSearchElements.Name = "chkSearchElements"
        '
        'btnFind
        '
        resources.ApplyResources(Me.btnFind, "btnFind")
        Me.btnFind.Name = "btnFind"
        '
        'cmbUse
        '
        resources.ApplyResources(Me.cmbUse, "cmbUse")
        Me.cmbUse.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.cmbUse.DisabledItemColour = System.Drawing.Color.LightGray
        Me.cmbUse.DropDownBackColor = System.Drawing.SystemColors.ControlLightLight
        Me.cmbUse.DropDownForeColor = System.Drawing.Color.Black
        Me.cmbUse.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbUse.DropDownWidth = 144
        Me.cmbUse.Name = "cmbUse"
        '
        'chkMatchCase
        '
        resources.ApplyResources(Me.chkMatchCase, "chkMatchCase")
        Me.chkMatchCase.Name = "chkMatchCase"
        '
        'chkUse
        '
        resources.ApplyResources(Me.chkUse, "chkUse")
        Me.chkUse.Name = "chkUse"
        '
        'chkWholeWord
        '
        resources.ApplyResources(Me.chkWholeWord, "chkWholeWord")
        Me.chkWholeWord.Name = "chkWholeWord"
        '
        'chkSearchUp
        '
        resources.ApplyResources(Me.chkSearchUp, "chkSearchUp")
        Me.chkSearchUp.Name = "chkSearchUp"
        '
        'lblFind
        '
        resources.ApplyResources(Me.lblFind, "lblFind")
        Me.lblFind.Name = "lblFind"
        '
        'cboAdvSearch
        '
        resources.ApplyResources(Me.cboAdvSearch, "cboAdvSearch")
        Me.cboAdvSearch.Name = "cboAdvSearch"
        '
        'tpDependencyView
        '
        Me.tpDependencyView.Controls.Add(Me.SplitContainer2)
        resources.ApplyResources(Me.tpDependencyView, "tpDependencyView")
        Me.tpDependencyView.Name = "tpDependencyView"
        Me.tpDependencyView.UseVisualStyleBackColor = True
        '
        'SplitContainer2
        '
        resources.ApplyResources(Me.SplitContainer2, "SplitContainer2")
        Me.SplitContainer2.Name = "SplitContainer2"
        '
        'SplitContainer2.Panel1
        '
        Me.SplitContainer2.Panel1.Controls.Add(Me.tvReferences)
        '
        'SplitContainer2.Panel2
        '
        Me.SplitContainer2.Panel2.Controls.Add(Me.dgvStages)
        '
        'tvReferences
        '
        resources.ApplyResources(Me.tvReferences, "tvReferences")
        Me.tvReferences.HideSelection = False
        Me.tvReferences.ImageList = Me.TreeImages
        Me.tvReferences.Name = "tvReferences"
        '
        'dgvStages
        '
        Me.dgvStages.AllowUserToAddRows = False
        Me.dgvStages.AllowUserToDeleteRows = False
        Me.dgvStages.AllowUserToResizeRows = False
        Me.dgvStages.BackgroundColor = System.Drawing.Color.White
        Me.dgvStages.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None
        Me.dgvStages.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvStages.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.img, Me.Page, Me.Stage})
        resources.ApplyResources(Me.dgvStages, "dgvStages")
        Me.dgvStages.MultiSelect = False
        Me.dgvStages.Name = "dgvStages"
        Me.dgvStages.ReadOnly = True
        Me.dgvStages.RowHeadersVisible = False
        Me.dgvStages.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        '
        'img
        '
        Me.img.Frozen = True
        resources.ApplyResources(Me.img, "img")
        Me.img.Name = "img"
        Me.img.ReadOnly = True
        '
        'Page
        '
        resources.ApplyResources(Me.Page, "Page")
        Me.Page.Name = "Page"
        Me.Page.ReadOnly = True
        '
        'Stage
        '
        Me.Stage.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.Stage, "Stage")
        Me.Stage.Name = "Stage"
        Me.Stage.ReadOnly = True
        '
        'stsFind
        '
        resources.ApplyResources(Me.stsFind, "stsFind")
        Me.stsFind.Name = "stsFind"
        Me.stsFind.Panels.AddRange(New System.Windows.Forms.StatusBarPanel() {Me.stsFindPanel1})
        Me.stsFind.SizingGrip = False
        '
        'stsFindPanel1
        '
        resources.ApplyResources(Me.stsFindPanel1, "stsFindPanel1")
        Me.stsFindPanel1.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring
        '
        'frmAdvSearch
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.tcOptions)
        Me.Controls.Add(Me.stsFind)
        Me.HelpButton = True
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmAdvSearch"
        Me.tcOptions.ResumeLayout(False)
        Me.tpFindText.ResumeLayout(False)
        Me.SplitContainer1.Panel1.ResumeLayout(False)
        Me.SplitContainer1.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer1.ResumeLayout(False)
        Me.tpDependencyView.ResumeLayout(False)
        Me.SplitContainer2.Panel1.ResumeLayout(False)
        Me.SplitContainer2.Panel2.ResumeLayout(False)
        CType(Me.SplitContainer2, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SplitContainer2.ResumeLayout(False)
        CType(Me.dgvStages, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.stsFindPanel1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

#End Region

#Region "Member Variables"


    ''' <summary>
    ''' The form owning this advanced search form, displaying the process being
    ''' searched.
    ''' </summary>
    Private mobjProcessViewingForm As IProcessViewingForm

    ''' <summary>
    ''' The process search control owning this form.
    ''' </summary>
    Private mobjParentProcessSearchControl As DiagramSearchToolstrip

    Private iStoreFoundNodes() As Integer   'array containing found page number + 1
    Private blnFoundNodes As Boolean
    Private blnFocusCode As Boolean = True  'if set then unhighlight stage in lostfocus code
#End Region

#Region "Sets"

    ''' <summary>
    ''' Sets the parent process form
    ''' </summary>
    ''' <param name="parent">The parent process form</param>
    Private Sub SetProcessStudioParent(ByVal parent As IProcessViewingForm)
        Me.mobjProcessViewingForm = parent
    End Sub

    'Select a specific dependency
    Public Sub SelectDependency(dep As clsProcessDependency)
        Dim depKey As String = GetKey(dep)
        BuildDependencyTree()
        Dim nodes() As TreeNode = tvReferences.Nodes.Find(depKey, True)
        If nodes.Length > 0 Then
            tvReferences.SelectedNode = nodes(0)
        End If
    End Sub

    'Switch to the Dependencies tab
    Public Sub ShowDependencyTab()
        Me.tcOptions.SelectedTab = tpDependencyView
    End Sub

    'Switch to the Find tab
    Public Sub ShowFindTab()
        Me.tcOptions.SelectedTab = tpFindText
    End Sub

#End Region

#Region "Form Events"
    Private Sub frmAdvSearch_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        PopulateTreeView()
        BuildDependencyTree()
        Me.cboAdvSearch.Focus()
    End Sub

    Private Sub frmAdvSearch_VisibleChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.VisibleChanged
        'in her as frmadvsearch is not closed fully until parent form is closed.
        If Me.Visible = True Then
            PopulateTreeView()
            BuildDependencyTree()
        End If
    End Sub

    Private Sub frmAdvSearch_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        'Not ideal but only want to close this advsearch form when the parent closes.
        'This will ensure that all the users preferences are kept when adv search is selected/closed.
        Me.Enabled = False
        Me.Visible = False
        'Clear existing data in treeview and listview
        TrVwStages.Nodes.Clear()
        lstSearch.Items.Clear()
        cboAdvSearch.Text = ""
        'Clear dependencies
        tvReferences.Nodes.Clear()
        dgvStages.Rows.Clear()
        CType(mobjProcessViewingForm, Form).Focus()
        Me.mobjParentProcessSearchControl.ClearLastFound()
        e.Cancel = True
    End Sub

#End Region

#Region "Control Events"

    Private Sub btnFind_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFind.Click
        If Len(cboAdvSearch.Text) > 0 Then
            blnFocusCode = False
            Find()
            blnFocusCode = True
        End If
    End Sub

    Private Sub cboAdvSearch_KeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles cboAdvSearch.KeyDown
        If (e.KeyCode = Keys.Enter Or e.KeyCode = Keys.F3) And Len(cboAdvSearch.Text) > 0 Then
            blnFocusCode = False
            Find()           'search
            e.Handled = True
            cboAdvSearch.Focus()
            blnFocusCode = True
        End If
    End Sub

    Private Sub HandleLostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles cboAdvSearch.LostFocus, MyBase.LostFocus
        If blnFocusCode = True Then
            Me.mobjParentProcessSearchControl.ClearLastFound()
        End If
    End Sub

    Private Sub TrVwStages_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles TrVwStages.DoubleClick
        If Not TrVwStages.SelectedNode() Is Nothing Then
            If TypeOf TrVwStages.SelectedNode.Tag Is clsProcessStage Then            'only do if stage
                Dim SelectedStage As clsProcessStage = CType(TrVwStages.SelectedNode.Tag, clsProcessStage)
                If Not SelectedStage Is Nothing Then
                    Me.mobjParentProcessSearchControl.ShowResult(SelectedStage)
                End If
            End If
        End If
    End Sub


    ''' <summary>
    ''' Recursively scans the node tree looking at the tag of each node to see
    ''' if it matches the object of interest. Returns the first treenode found
    ''' with a matching tag.
    ''' </summary>
    ''' <param name="node">The collection of tree nodes to look down.
    ''' </param>
    ''' <param name="tagtolookfor">The object of interest to be compared with the tage.
    ''' </param>
    ''' <returns>Returns the first treenode with a tag matching the supplied object.
    ''' </returns>
    Private Function RecursivelyFindTreenodeWithTag(ByVal node As TreeNode, ByVal TagToLookFor As Object) As TreeNode
        For Each n As TreeNode In node.Nodes
            If n.Tag Is TagToLookFor Then Return n
            Dim tempnode As TreeNode = RecursivelyFindTreenodeWithTag(n, TagToLookFor)
            If Not tempnode Is Nothing Then Return tempnode
        Next

        'tag not found in any node
        Return Nothing
    End Function


    ''' <summary>
    ''' Recursively scans the node tree looking at the tag of each node to see
    ''' if it matches the object of interest. Returns the first treenode found
    ''' with a matching tag.
    ''' </summary>
    ''' <param name="nodecollection">The collection of tree nodes to look down.
    ''' </param>
    ''' <param name="tagtolookfor">The object of interest to be compared with the tage.
    ''' </param>
    ''' <returns>Returns the first treenode with a tag matching the supplied object.
    ''' </returns>
    Private Function RecursivelyFindTreenodeWithTag(ByVal nodecollection As TreeNodeCollection, ByVal tagtolookfor As Object) As TreeNode
        For Each n As TreeNode In nodecollection
            Dim tempnode As TreeNode = Me.RecursivelyFindTreenodeWithTag(n, tagtolookfor)
            If Not tempnode Is Nothing Then Return tempnode
        Next
        Return Nothing
    End Function

    Private Sub lstSearch_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstSearch.DoubleClick
        If lstSearch.SelectedItems.Count > 0 Then
            Try
                If TypeOf lstSearch.SelectedItems(0).Tag Is clsProcessStage Then
                    Dim SelectedStage As clsProcessStage = CType(lstSearch.SelectedItems(0).Tag, clsProcessStage)
                    Me.mobjParentProcessSearchControl.ShowResult(SelectedStage)
                    Dim tempnode As TreeNode = Me.RecursivelyFindTreenodeWithTag(TrVwStages.Nodes, SelectedStage)
                    If Not tempnode Is Nothing Then TrVwStages.SelectedNode = tempnode
                    If Not TrVwStages.SelectedNode Is Nothing AndAlso TrVwStages.SelectedNode.BackColor.Equals(Color.Yellow) Then
                        Me.mobjParentProcessSearchControl.ShowResult(SelectedStage)
                    End If
                End If
            Catch
                'do nothing
            End Try
        End If
    End Sub

    Private Sub chkUse_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkUse.CheckedChanged
        If chkUse.Checked Then
            cmbUse.Enabled = True
        Else
            cmbUse.Enabled = False
        End If
    End Sub
#End Region

#Region "Members"

#Region "Populate Tree and List"

    ''' <summary>
    ''' First clears any nodes/items in the treeview and listview
    ''' Sets the root node of the treeview to the Process name
    ''' Calls AddSheetNodes which adds the sheet/page nodes and which in turn calls
    ''' another function AddStageNodes that adds nodes for each stage on the sheet
    ''' If there are any stages containing the search string they are expanded
    ''' </summary>
    Private Sub PopulateTreeView()
        Try
            Dim rootNode As TreeNode              'process node
            Dim index As Integer
            ReDim iStoreFoundNodes(1000)             'integer array containing index of sheets with found text
            blnFoundNodes = False             'boolean is true if text is found
            Dim iActivePage As Integer
            Dim iActivePageNode As Integer
            Dim iStoreFoundNode As Integer

            'first clear existing data in treeview and listview
            TrVwStages.Nodes.Clear()
            lstSearch.Items.Clear()

            'next create root (process) node
            rootNode = New TreeNode(Me.mobjParentProcessSearchControl.Process.Name)
            TrVwStages.Nodes.Add(rootNode)

            'next add nodes
            AddSheetNodes(iActivePage, iActivePageNode)
            TrVwStages.TabStop = False

            'istorefoundnodes holds all nodes that contain found stages - so expand

            If blnFoundNodes = True Then
                rootNode.Expand()
                For index = 0 To iStoreFoundNodes.Length - 1
                    iStoreFoundNode = iStoreFoundNodes(index) - 1
                    If iStoreFoundNode >= 0 And iStoreFoundNode < rootNode.Nodes.Count Then
                        rootNode.Nodes(iStoreFoundNode).Expand()
                    Else
                        If index > 0 Then
                            index = iStoreFoundNodes.Length
                        End If
                    End If
                Next
                If iActivePageNode <> -1 AndAlso iActivePage <> -1 Then
                    rootNode.Nodes(iActivePage).Nodes(iActivePageNode).EnsureVisible()
                End If
            Else
                TrVwStages.CollapseAll()
            End If
        Catch ex As Exception
            UserMessage.OK(ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Handles search feedback by displaying the
    ''' message on the status bar.
    ''' </summary>
    ''' <param name="Message">The message received as
    ''' feedback.</param>
    Private Sub HandleSearchFeedback(ByVal Message As String)
        Me.stsFind.Text = Message
    End Sub
    ''' <summary>
    ''' Handles event raised when new search
    ''' is performed from sibling controls (eg
    ''' the process studio search bar).
    ''' </summary>
    ''' <param name="SearchText">The text of the new
    ''' search.</param>
    ''' <remarks>Updates the local combo box with the
    ''' new search.</remarks>
    Private Sub HandleNewSearch(ByVal SearchText As String)
        Me.AddToCombo(SearchText)
    End Sub


    ''' <summary>
    ''' Called by the PopulateTree Function
    ''' This adds nodes to the root (process) node of the treeview for each sheet/page
    ''' First a node is added for the Main Page
    ''' Next a loop through the subsheets adds the other nodes.
    ''' After each new node is added the AddStageNodes function is called.
    ''' </summary>
    Private Sub AddSheetNodes(ByRef ActivePage As Integer, ByRef ActivePageNode As Integer)
        Try
            Dim PageNode As TreeNode
            Dim blnFoundText As Boolean
            Dim index As Integer
            Dim istore As Integer
            Dim iActiveNode As Integer = -1
            Dim iLastpage As Integer
            Dim iActivePage As Integer = -1
            Dim iActivePageNode As Integer = -1

            'next loop through getsubsheets datatable creating a node 
            'for each datarow
            For Each objSub As clsProcessSubSheet In Me.mobjParentProcessSearchControl.Process.SubSheets
                index = index + 1
                Dim sPageName As String = LTools.GetC(objSub.Name, "misc", "page")               'dr(1) is the name of the page
                PageNode = New TreeNode(sPageName)
                Dim gPageGuid As Guid = objSub.ID               'dr(0) is the guid for the page
                PageNode.Tag = gPageGuid
                iActiveNode = -1
                AddStageNodes(PageNode, gPageGuid, sPageName, blnFoundText, iActiveNode)
                If blnFoundText Then
                    iStoreFoundNodes(istore + 1) = index + 1
                    istore = istore + 1
                    blnFoundNodes = True
                End If
                iLastpage = TrVwStages.Nodes(0).Nodes.Add(PageNode)
                If iActiveNode <> -1 Then
                    iActivePage = iLastpage
                    iActivePageNode = iActiveNode
                End If
            Next
            If iActivePageNode <> -1 AndAlso iActivePage <> -1 Then
                TrVwStages.Nodes(0).Nodes(iActivePage).Nodes(iActivePageNode).EnsureVisible()
            End If
            ActivePage = iActivePage
            ActivePageNode = iActivePageNode
        Catch ex As Exception
            UserMessage.OK(ex.Message)
        End Try
    End Sub


    ''' <summary>
    ''' Called from the AddSheetNodes function.
    ''' This function adds nodes to the sheet/page nodes of the treeview for each 
    ''' stage.
    ''' It loops through all the stages in the process and adds a node for each
    ''' one found with the same sheetid as the sheedID parameter passesd to it.
    ''' If the search phrase is found in the stage it is highlighted yellow.
    ''' </summary>
    ''' <param name="parent">This is the sheet/page treenode</param>
    ''' <param name="sheetId">This is the sheet id for which the sheet the stages need to be added</param>
    ''' <param name="sheetName">This is the name of the sheet.  Passed to save getting it again</param>
    ''' <param name="blnFoundText">Set to true if any of the stage nodes added contained the
    ''' search string.  This is passed back up as the nodes will need to be expanded</param>
    Private Sub AddStageNodes(ByVal parent As TreeNode, ByVal sheetId As Guid, ByVal sheetName As String, ByRef blnFoundText As Boolean, ByRef iactivenode As Integer)
        Try
            Dim StageNode As TreeNode
            Dim count As Integer = 0
            blnFoundText = False
            Dim blnFoundActive As Boolean
            Dim ilastnode As Integer

            For Each objStage As clsProcessStage In mobjParentProcessSearchControl.Process.GetStages()
                If objStage.GetSubSheetID.Equals(sheetId) Then
                    StageNode = New TreeNode(LTools.GetC(objStage.GetName, "misc", "stage"))
                    StageNode.Tag = objStage
                    blnFoundActive = False
                    If Len(cboAdvSearch.Text) > 0 Then
                        'highlight if search string in name or desc
                        Dim Params As clsProcessSearcher.SearchParameters = Me.GetSearchParameters

                        Dim prefix As String = "element:"
                        If Params.SearchText.StartsWith(prefix) Then Params.SearchText = Params.SearchText.Substring(prefix.Length, Params.SearchText.Length - prefix.Length)
                        If Me.mobjParentProcessSearchControl.mProcessSearcher.SearchForParameters(objStage, Params) Then
                            StageNode.BackColor = System.Drawing.Color.Yellow
                            blnFoundText = True
                            PopulateListview(objStage, sheetName)
                        End If
                    End If
                    ilastnode = parent.Nodes.Add(StageNode)
                    If blnFoundActive = True Then iactivenode = ilastnode
                    count += 1
                End If
            Next
            If count > 0 Then
                parent.Text &= String.Format(My.Resources.frmAdvSearch_Count0, count.ToString)
            End If
        Catch ex As Exception
            UserMessage.OK(ex.Message)
        End Try
    End Sub


    ''' <summary>
    ''' Adds one line to the listview for the stage parameter passed to it.
    ''' If the stage is the current found stage then the new line is highlighted yellow.
    ''' </summary>
    ''' <param name="mobjStage">The stage to be added to the listview</param>
    ''' <param name="sheetName">The name of the stage to be added</param>
    Private Sub PopulateListview(ByVal mobjStage As clsProcessStage, ByVal sheetName As String)
        Try
            Dim x As New ListViewItem
            x = lstSearch.Items.Add(sheetName)
            With x
                .SubItems.Add(LTools.GetC(mobjStage.GetName(), "misc", "stage"))
                .SubItems.Add(mobjStage.GetNarrative())
                .SubItems.Add(clsStageTypeName.GetLocalizedFriendlyName(mobjStage.StageType.ToString()))
                .Tag = mobjStage
            End With
        Catch ex As Exception
            UserMessage.OK(ex.Message)
        End Try
    End Sub

#End Region



    ''' <summary>
    ''' Adds the last search phrase to cboAdvSearch if it is not already there.
    ''' If the search phrase is already in cboAdvSearch then it will move it to be 
    ''' first in the index.
    ''' If there are 20 items it will remove the oldest item before adding a new item.
    ''' It populates the search combobox of the parent form/control with the same details
    ''' </summary>
    ''' <param name="sString">This is the string to be added to cboAdvSearch</param>
    Private Sub AddToCombo(ByVal sString As String)
        Try
            If cboAdvSearch.FindStringExact(sString) <> -1 Then          'found
                cboAdvSearch.Items.Remove(sString)
            End If
            If cboAdvSearch.Items.Count = 20 Then
                cboAdvSearch.Items.RemoveAt(19)
            End If
            cboAdvSearch.Items.Insert(0, sString)
            cboAdvSearch.Text = sString

            If Not mobjProcessViewingForm Is Nothing Then
                mobjParentProcessSearchControl.AddToCombo(sString)
                'Me.mobjParentProcessSearchControl.cmbSearch.Items.Clear()
                'For index = 0 To cboAdvSearch.Items.Count - 1
                '   Me.mobjParentProcessSearchControl.cmbSearch.Items.Add(cboAdvSearch.Items(index))
                'Next
            End If
        Catch ex As Exception
            UserMessage.OK(ex.Message)
        End Try
    End Sub


    ''' <summary>
    ''' Gets the search parameters chosen by the user
    ''' </summary>
    ''' <returns>Returns a SearchParameters structure,
    ''' indicating which options have been chosen in the UI.</returns>
    Public Function GetSearchParameters() As clsProcessSearcher.SearchParameters
        Dim Params As New clsProcessSearcher.SearchParameters

        Params.SearchText = Me.cboAdvSearch.Text
        Params.NewSearch = cboAdvSearch.Text <> Me.mobjParentProcessSearchControl.mProcessSearcher.LastSearchString
        Params.ForwardSearch = Not chkSearchUp.Checked
        Params.MatchCase = Me.chkMatchCase.Checked
        Params.MatchWholeWord = Me.chkWholeWord.Checked
        Params.SearchElements = Me.chkSearchElements.Checked

        If (Not chkUse.Checked) OrElse cmbUse.Text = "" Then
            Params.SearchType = clsProcessSearcher.SearchParameters.SearchTypes.Normal
        Else
            Params.SearchType = CType(cmbUse.SelectedItem.Tag, clsProcessSearcher.SearchParameters.SearchTypes)
        End If

        Static LastParams As clsProcessSearcher.SearchParameters
        If LastParams.SearchType <> Params.SearchType Then Params.NewSearch = True
        If LastParams.ForwardSearch <> Params.ForwardSearch Then Params.NewSearch = True
        If LastParams.MatchWholeWord <> Params.MatchWholeWord Then Params.NewSearch = True
        If LastParams.MatchCase <> Params.MatchCase Then Params.NewSearch = True
        LastParams = Params

        Return Params
    End Function


    ''' <summary>
    ''' This function calls clsprocess.find which will search all stages in the 
    ''' current process for the search phrase.
    ''' It also calls the PopulateTreeView function which updates the tree and 
    ''' list views on this form.
    ''' </summary>
    Private Sub Find()
        Try
            stsFind.Text = ""
            Dim SearchParams As clsProcessSearcher.SearchParameters = Me.GetSearchParameters

            Me.mobjParentProcessSearchControl.Find(SearchParams)
            AddToCombo(cboAdvSearch.Text)

            cboAdvSearch.Focus()
            PopulateTreeView()
        Catch ex As Exception
            UserMessage.OK(ex.Message)
        End Try
    End Sub

#End Region



    ''' <summary>
    ''' Populates the combo box of search types
    ''' (eg regex, wildcards etc).
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub PopulateSearchTypes()
        Me.cmbUse.BeginUpdate()
        Me.cmbUse.Items.Clear()
        Me.cmbUse.Items.Add(New AutomateControls.ComboBoxes.MonoComboBoxItem(My.Resources.frmAdvSearch_RegularExpressions, clsProcessSearcher.SearchParameters.SearchTypes.Regex))
        Me.cmbUse.Items.Add(New AutomateControls.ComboBoxes.MonoComboBoxItem(My.Resources.frmAdvSearch_WildCards, clsProcessSearcher.SearchParameters.SearchTypes.WildCard))
        Me.cmbUse.EndUpdate()
    End Sub

#Region "Initialise dependency tree"

    Private Sub InitialiseTreeView()

        TreeImages.Images.Add("Root", BluePrism.Images.ToolImages.Button_Blue_Forward_16x16)
        TreeImages.Images.Add("ProcessIDDependency", BluePrism.Images.ComponentImages.Procedure_16x16)
        TreeImages.Images.Add("ProcessNameDependency", BluePrism.Images.ComponentImages.Class_16x16)
        TreeImages.Images.Add("ProcessParentDependency", BluePrism.Images.ComponentImages.Class_16x16)
        TreeImages.Images.Add("ProcessActionDependency", BluePrism.Images.ComponentImages.Method_16x16)
        TreeImages.Images.Add("ProcessDataItemDependency", BluePrism.Images.ComponentImages.Field_16x16)
        TreeImages.Images.Add("ProcessWebServiceDependency", BluePrism.Images.ComponentImages.Document_HTML_16x16)
        TreeImages.Images.Add("ProcessWebApiDependency", BluePrism.Images.ComponentImages.Document_HTML_Purple_16x16)
        TreeImages.Images.Add("ProcessQueueDependency", BluePrism.Images.ComponentImages.Custom_Queue_16x16)
        TreeImages.Images.Add("ProcessCredentialsDependency", BluePrism.Images.AuthImages.User_Blue_Password_16x16)
        TreeImages.Images.Add("ProcessPageDependency", BluePrism.Images.ToolImages.Code_Snippet_16x16)
        TreeImages.Images.Add("ProcessElementDependency", BluePrism.Images.ComponentImages.Record_16x16)
        TreeImages.Images.Add("ProcessEnvironmentVarDependency", BluePrism.Images.ComponentImages.Field_Protected_16x16)
        TreeImages.Images.Add("ProcessFontDependency", BluePrism.Images.ComponentImages.Control_Label_16x16)
        TreeImages.Images.Add("ProcessCalendarDependency", BluePrism.Images.ComponentImages.Calendar_16x16)
        TreeImages.Images.Add("ProcessSkillDependency", BluePrism.Images.ComponentImages.Skill_16x16)
        TreeImages.Images.Add("Unknown", BluePrism.Images.ToolImages.Warning_16x16)

        StageImages.Images.Add("ActionStage", BluePrism.Images.StageImages.ActionStage_22x16)
        StageImages.Images.Add("AlertStage", BluePrism.Images.StageImages.AlertStage_22x16)
        StageImages.Images.Add("CalculationStage", BluePrism.Images.StageImages.CalculationStage_22x16)
        StageImages.Images.Add("ChoiceStartStage", BluePrism.Images.StageImages.ChoiceStartStage_22x16)
        StageImages.Images.Add("CodeStage", BluePrism.Images.StageImages.CodeStage_22x16)
        StageImages.Images.Add("DataStage", BluePrism.Images.StageImages.DataStage_22x16)
        StageImages.Images.Add("DecisionStage", BluePrism.Images.StageImages.DecisionStage_22x16)
        StageImages.Images.Add("EndStage", BluePrism.Images.StageImages.EndStage_22x16)
        StageImages.Images.Add("ExceptionStage", BluePrism.Images.StageImages.ExceptionStage_22x16)
        StageImages.Images.Add("LoopStartStage", BluePrism.Images.StageImages.LoopStartStage_22x16)
        StageImages.Images.Add("MultipleCalculationStage", BluePrism.Images.StageImages.MultipleCalculationStage_22x16)
        StageImages.Images.Add("NavigateStage", BluePrism.Images.StageImages.NavigateStage_22x16)
        StageImages.Images.Add("ProcessStage", BluePrism.Images.StageImages.SubProcessRefStage_22x16)
        StageImages.Images.Add("ReadStage", BluePrism.Images.StageImages.ReadStage_22x16)
        StageImages.Images.Add("StartStage", BluePrism.Images.StageImages.StartStage_22x16)
        StageImages.Images.Add("SubSheetStage", BluePrism.Images.StageImages.SubSheetRefStage_22x16)
        StageImages.Images.Add("WaitStartStage", BluePrism.Images.StageImages.WaitStartStage_22x16)
        StageImages.Images.Add("WriteStage", BluePrism.Images.StageImages.WriteStage_22x16)
        StageImages.Images.Add("SkillStage", BluePrism.Images.StageImages.SkillStage_22x16)
        StageImages.ImageSize = New Size(22, 16)
    End Sub

#End Region

#Region "Dependency tree builder"

    Private Sub BuildDependencyTree()
        Dim proc As clsProcess = mobjParentProcessSearchControl.Process

        Dim objList As clsGroupBusinessObject = proc.GetBusinessObjects()

        tvReferences.Nodes.Clear()
        tvReferences.BeginUpdate()
        Dim deps As clsProcessDependencyList = proc.GetDependencies(True)

        Dim depList As IEnumerable(Of clsProcessDependency)
        Dim nodeList As New List(Of TreeNode)()
        Dim node As TreeNode

        'Add Internal References as root node
        node = CreateNode(proc.Name, Nothing, "Root")
        node.Nodes.AddRange(GetObjectOrProcess(proc, deps, proc.Name))
        tvReferences.Nodes.Add(node)
        node.Expand()

        'External References
        'Objects - If model sharing then add Parent as the first object
        nodeList.Clear()
        If proc.ParentObject IsNot Nothing Then
            Dim parentDep As clsProcessDependency = deps.Find(New clsProcessParentDependency(proc.ParentObject))
            Dim name As String = String.Format(My.Resources.frmAdvSearch_0Parent, proc.ParentObject)
            If objList.FindObjectReference(proc.ParentObject) Is Nothing Then
                name = My.Resources.frmAdvSearch_MISSING & name
            End If
            node = CreateNode(name, parentDep, parentDep.TypeName)
            node.Nodes.AddRange(GetObjectOrProcess(proc, deps, proc.ParentObject))
            nodeList.Add(node)
        End If

        'Other objects & actions
        depList = From o As clsProcessDependency In deps.GetDependencies("ProcessNameDependency")
                  Order By CType(o, clsProcessNameDependency).RefProcessName Select o
        For Each d As clsProcessNameDependency In depList
            'Ignore self-references & parent - they are catered for above
            If d.RefProcessName = proc.Name OrElse d.RefProcessName = proc.ParentObject Then Continue For

            Dim name As String = String.Empty
            Dim obj As clsBusinessObject = objList.FindObjectReference(d.RefProcessName)
            If obj IsNot Nothing Then
                name = obj.FriendlyName
            Else
                name = My.Resources.frmAdvSearch_MISSING & d.RefProcessName
            End If
            node = CreateNode(name, d, d.TypeName)
            node.Nodes.AddRange(GetObjectOrProcess(proc, deps, d.RefProcessName))
            nodeList.Add(node)
        Next
        If nodeList.Count > 0 Then tvReferences.Nodes.Add(
            CreateParentNode(My.Resources.frmAdvSearch_Objects, "ProcessNameDependency", nodeList, False))

        'Processes
        nodeList.Clear()
        For Each d As clsProcessIDDependency In deps.GetDependencies("ProcessIDDependency")
            Dim name As String = gSv.GetProcessNameByID(d.RefProcessID)
            If name = String.Empty Then name = My.Resources.frmAdvSearch_UnknownProcess
            nodeList.Add(CreateNode(name, d, d.TypeName))
        Next
        If nodeList.Count > 0 Then tvReferences.Nodes.Add(
            CreateParentNode(My.Resources.frmAdvSearch_Processes, "ProcessIDDependency", nodeList))

        'Web services
        nodeList.Clear()
        For Each d As clsProcessWebServiceDependency In deps.GetDependencies("ProcessWebServiceDependency")
            Dim name As String = d.RefServiceName
            If objList.FindObjectReference(d.RefServiceName) Is Nothing Then
                name = My.Resources.frmAdvSearch_MISSING & name
            End If
            nodeList.Add(CreateNode(name, d, d.TypeName))
        Next
        If nodeList.Count > 0 Then tvReferences.Nodes.Add(
            CreateParentNode(My.Resources.frmAdvSearch_WebServices, "ProcessWebServiceDependency", nodeList))

        'Web APIs
        nodeList.Clear()
        For Each d As clsProcessWebApiDependency In deps.GetDependencies("ProcessWebApiDependency")
            Dim name As String = d.RefApiName
            If objList.FindObjectReference(d.RefApiName) Is Nothing Then
                name = My.Resources.frmAdvSearch_MISSING & name
            End If
            nodeList.Add(CreateNode(name, d, d.TypeName))
        Next
        If nodeList.Count > 0 Then tvReferences.Nodes.Add(
            CreateParentNode(My.Resources.frmAdvSearch_WebAPIServices, "ProcessWebApiDependency", nodeList))

        'Environment variables
        nodeList.Clear()
        Dim vars = gSv.GetEnvironmentVariables().ToDictionary(Of String, clsEnvironmentVariable)(Function(x) x.Name, Function(x) x)
        For Each d As clsProcessEnvironmentVarDependency In deps.GetDependencies("ProcessEnvironmentVarDependency")
            Dim name As String = d.RefVariableName
            If Not vars.ContainsKey(d.RefVariableName) Then
                name = My.Resources.frmAdvSearch_MISSING & name
            End If
            nodeList.Add(CreateNode(name, d, d.TypeName))
        Next
        If nodeList.Count > 0 Then tvReferences.Nodes.Add(
            CreateParentNode(My.Resources.frmAdvSearch_EnvironmentVariables, "ProcessEnvironmentVarDependency", nodeList))

        'Fonts
        nodeList.Clear()
        Dim fonts As New List(Of String)(gSv.GetFontNames())
        For Each d As clsProcessFontDependency In deps.GetDependencies("ProcessFontDependency")
            Dim name As String = d.RefFontName
            If Not fonts.Contains(d.RefFontName) Then
                name = My.Resources.frmAdvSearch_MISSING & name
            End If
            nodeList.Add(CreateNode(name, d, d.TypeName))
        Next
        If nodeList.Count > 0 Then tvReferences.Nodes.Add(
            CreateParentNode(My.Resources.frmAdvSearch_Fonts, "ProcessFontDependency", nodeList))

        'Calendars
        nodeList.Clear()
        Dim cals As IDictionary(Of Integer, ScheduleCalendar) = gSv.GetAllCalendars()
        For Each d As clsProcessCalendarDependency In deps.GetDependencies("ProcessCalendarDependency")
            Dim name As String = d.RefCalendarName
            If name = String.Empty Then
                nodeList.Add(CreateNode(LTools.Format(My.Resources.frmAdvSearch_plural_dynamic_references, "COUNT", d.StageIDs.Count), d, My.Resources.frmAdvSearch_Unknown))
            Else
                If Not cals.Values.Cast(Of ScheduleCalendar).Any(Function(c) c.Name = d.RefCalendarName) Then
                    name = My.Resources.frmAdvSearch_MISSING & name
                End If
                nodeList.Add(CreateNode(name, d, d.TypeName))
            End If
        Next
        If nodeList.Count > 0 Then tvReferences.Nodes.Add(
            CreateParentNode(My.Resources.frmAdvSearch_Calendars, "ProcessCalendarDependency", nodeList))

        'Credentials
        nodeList.Clear()
        Dim creds As New List(Of clsCredential)(gSv.GetAllCredentialsInfo())
        For Each d As clsProcessCredentialsDependency In deps.GetDependencies("ProcessCredentialsDependency")
            Dim name As String = d.RefCredentialsName
            If name = String.Empty Then
                nodeList.Add(CreateNode(LTools.Format(My.Resources.frmAdvSearch_plural_dynamic_references, "COUNT", d.StageIDs.Count), d, My.Resources.frmAdvSearch_Unknown))
            Else
                If Not creds.Cast(Of clsCredential).Any(Function(c) c.Name = d.RefCredentialsName) Then
                    name = My.Resources.frmAdvSearch_MISSING & name
                End If
                nodeList.Add(CreateNode(name, d, d.TypeName))
            End If
        Next
        If nodeList.Count > 0 Then tvReferences.Nodes.Add(
            CreateParentNode(My.Resources.frmAdvSearch_Credentials, "ProcessCredentialsDependency", nodeList))

        'Work queues
        nodeList.Clear()
        Dim queues As New List(Of String)(gSv.WorkQueueGetAllQueueNames())
        For Each d As clsProcessQueueDependency In deps.GetDependencies("ProcessQueueDependency")
            Dim name As String = d.RefQueueName
            If name = String.Empty Then
                nodeList.Add(CreateNode(LTools.Format(My.Resources.frmAdvSearch_plural_dynamic_references, "COUNT", d.StageIDs.Count), d, My.Resources.frmAdvSearch_Unknown))
            Else
                If Not queues.Cast(Of String).Any(Function(q) q = d.RefQueueName) Then
                    name = My.Resources.frmAdvSearch_MISSING & name
                End If
                nodeList.Add(CreateNode(name, d, d.TypeName))
            End If
        Next
        If nodeList.Count > 0 Then tvReferences.Nodes.Add(
            CreateParentNode(My.Resources.frmAdvSearch_WorkQueues, "ProcessQueueDependency", nodeList))

        'Skills
        BuildSkillsNode(deps)

        tvReferences.EndUpdate()
    End Sub

    Private Sub BuildSkillsNode(dependencies As clsProcessDependencyList)
        Dim skills = mobjParentProcessSearchControl.Process.SkillsInUse

        Dim nodeList As New List(Of TreeNode)
        For Each d As clsProcessSkillDependency In dependencies.GetDependencies("ProcessSkillDependency")
            Dim skill As SkillDetails = skills.FirstOrDefault(Function(s) s.SkillId = d.RefSkillId.ToString())
            If skill IsNot Nothing Then
                nodeList.Add(CreateNode(skill.SkillName, d, d.TypeName))
            Else
                nodeList.Add(CreateNode(My.Resources.frmAdvSearch_Unknown, d, d.TypeName))
            End If
        Next
        If nodeList.Count > 0 Then tvReferences.Nodes.Add(
            CreateParentNode(My.Resources.frmAdvSearch_Skills, "ProcessSkillDependency", nodeList))
    End Sub

    Private Function GetObjectOrProcess(proc As clsProcess, deps As clsProcessDependencyList,
     objName As String) As TreeNode()
        Dim objNodes As New List(Of TreeNode)()

        'Add Model elements (if they belong to the passed object)
        Dim nodeList As New List(Of TreeNode)()
        If proc.ProcessType = DiagramType.Object Then
            For Each d As clsProcessElementDependency In deps.GetDependencies("ProcessElementDependency")
                If d.RefProcessName = objName Then
                    Dim el As clsApplicationElement = proc.ApplicationDefinition.FindElement(d.RefElementID)
                    If el IsNot Nothing Then _
                        nodeList.Add(CreateNode(el.Name, d, d.TypeName)) Else _
                        nodeList.Add(CreateNode(My.Resources.frmAdvSearch_UnknownElement, d, d.TypeName))
                End If
            Next
        End If
        If nodeList.Count > 0 Then objNodes.Add(
            CreateParentNode(My.Resources.frmAdvSearch_ModelElements, "ProcessElementDependency", nodeList))

        'Add Data items (for the object/process being edited)
        nodeList.Clear()
        If objName = proc.Name Then
            For Each d As clsProcessDataItemDependency In deps.GetDependencies("ProcessDataItemDependency")
                nodeList.Add(CreateNode(d.RefDataItemName, d, d.TypeName))
            Next
        End If
        If nodeList.Count > 0 Then objNodes.Add(
            CreateParentNode(My.Resources.frmAdvSearch_DataItems, "ProcessDataItemDependency", nodeList))

        'Add Pages (for the object/process being edited)
        nodeList.Clear()
        If objName = proc.Name Then
            For Each d As clsProcessPageDependency In deps.GetDependencies("ProcessPageDependency")
                nodeList.Add(CreateNode(proc.GetSubSheetName(d.RefPageID), d, d.TypeName))
            Next
        End If
        If nodeList.Count > 0 Then objNodes.Add(
            CreateParentNode(My.Resources.frmAdvSearch_Pages, "ProcessPageDependency", nodeList))

        'Add Actions (if they belong to the passed object)
        nodeList.Clear()
        For Each d As clsProcessActionDependency In deps.GetDependencies("ProcessActionDependency")
            If d.RefProcessName = objName Then nodeList.Add(CreateNode(d.RefActionName, d, d.TypeName))
        Next
        If nodeList.Count > 0 Then objNodes.Add(
            CreateParentNode(My.Resources.frmAdvSearch_Actions, "ProcessActionDependency", nodeList))

        Return objNodes.ToArray()
    End Function

    Private Function CreateNode(label As String, dep As clsProcessDependency, imgKey As String) As TreeNode
        Dim node As New TreeNode(label)
        If dep IsNot Nothing Then
            node.Tag = dep
            node.Name = GetKey(dep)
        End If
        node.ImageKey = imgKey
        node.SelectedImageKey = imgKey
        Return node
    End Function

    Private Function CreateParentNode(label As String, imgKey As String, nodes As List(Of TreeNode), _
     Optional sorted As Boolean = True) As TreeNode
        Dim parent As TreeNode = CreateNode(label, Nothing, imgKey)
        If sorted Then
            Dim orderedNodes = From node As TreeNode In nodes Order By node.Text
            parent.Nodes.AddRange(orderedNodes.ToArray())
        Else
            parent.Nodes.AddRange(nodes.ToArray())
        End If

        Return parent
    End Function

#End Region

#Region "Dependency tree event handlers"

    Private Sub tvReferences_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles tvReferences.AfterSelect
        dgvStages.Rows.Clear()
        Dim dep As clsProcessDependency = TryCast(e.Node.Tag, clsProcessDependency)
        If dep Is Nothing Then
            dgvStages.Enabled = False
            Exit Sub
        End If
        dgvStages.Enabled = True
        For Each stgID As Guid In dep.StageIDs
            Dim row As Integer
            Dim st As clsProcessStage = mobjParentProcessSearchControl.Process.GetStage(stgID)
            row = dgvStages.Rows.Add(StageImages.Images.Item(st.StageType.ToString() & "Stage"), LTools.GetC(st.GetSubSheetName(), "misc", "page"), st.Name)
            dgvStages.Rows(row).Tag = stgID
        Next
        If dgvStages.Rows.Count > 0 Then dgvStages.Rows(0).Selected = True
    End Sub

    Private Sub dgvStages_SelectionChanged(sender As Object, e As DataGridViewCellEventArgs) Handles dgvStages.CellDoubleClick
        Dim stgID As Guid = CType(dgvStages.Rows(e.RowIndex).Tag, Guid)
        Dim SelectedStage As clsProcessStage = mobjParentProcessSearchControl.Process.GetStage(stgID)
        Me.mobjParentProcessSearchControl.ShowResult(SelectedStage)
    End Sub

#End Region

    Private Function GetKey(dep As clsProcessDependency) As String
        Dim key As String = String.Empty

        key = dep.TypeName
        For Each val As KeyValuePair(Of String, Object) In dep.GetValues()
            key &= "." & val.Value.ToString()
        Next

        Return key
    End Function

#Region "Help implementation"
    ''' <summary>
    ''' Gets the help file name.
    ''' </summary>
    Public Overrides Function GetHelpFile() As String
        Return "helpProcessSearch.htm"
    End Function

    Public Overrides Sub OpenHelp()
        Try
            OpenHelpFile(Me, GetHelpFile())
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

#End Region

End Class
