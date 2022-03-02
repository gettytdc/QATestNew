Imports AutomateControls
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateAppCore.Utility

Friend Class frmBreakpointLocator : Inherits frmForm
    Implements IHelp, IEnvironmentColourManager

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

        Me.chkPage.Checked = True
        AddHandler chkPage.CheckedChanged, AddressOf chkPage_CheckedChanged
        Me.objBluebar.Title = My.Resources.frmBreakpointLocator_FindAndManageBreakpointsInYourProcess

        Me.trvwStages.ImageList = Me.imgBreakpointIcons
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
    Friend WithEvents trvwStages As System.Windows.Forms.TreeView
    Friend WithEvents rtbExpression As ctlExpressionRichTextBox
    Friend WithEvents btnClearAll As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnGotoStage As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnClearBreakpoint As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents pnlHeader As AutomateUI.clsPanel
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents chkPage As System.Windows.Forms.CheckBox
    Friend WithEvents imgBreakpointIcons As System.Windows.Forms.ImageList
    Friend WithEvents lblExpression As System.Windows.Forms.Label
    Friend WithEvents lbBreakpoints As System.Windows.Forms.Label
    Friend WithEvents btnClose As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnHelp As AutomateControls.Buttons.StandardStyledButton
    Protected WithEvents objBluebar As AutomateControls.TitleBar
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmBreakpointLocator))
        Me.trvwStages = New System.Windows.Forms.TreeView()
        Me.lblExpression = New System.Windows.Forms.Label()
        Me.rtbExpression = New AutomateUI.ctlExpressionRichTextBox()
        Me.lbBreakpoints = New System.Windows.Forms.Label()
        Me.btnGotoStage = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnClearAll = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnClearBreakpoint = New AutomateControls.Buttons.StandardStyledButton()
        Me.pnlHeader = New AutomateUI.clsPanel()
        Me.chkPage = New System.Windows.Forms.CheckBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.objBluebar = New AutomateControls.TitleBar()
        Me.imgBreakpointIcons = New System.Windows.Forms.ImageList(Me.components)
        Me.btnClose = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnHelp = New AutomateControls.Buttons.StandardStyledButton()
        Me.pnlHeader.SuspendLayout()
        Me.SuspendLayout()
        '
        'trvwStages
        '
        resources.ApplyResources(Me.trvwStages, "trvwStages")
        Me.trvwStages.HideSelection = False
        Me.trvwStages.Name = "trvwStages"
        '
        'lblExpression
        '
        resources.ApplyResources(Me.lblExpression, "lblExpression")
        Me.lblExpression.Name = "lblExpression"
        '
        'rtbExpression
        '
        Me.rtbExpression.AllowDrop = True
        resources.ApplyResources(Me.rtbExpression, "rtbExpression")
        Me.rtbExpression.DetectUrls = False
        Me.rtbExpression.HideSelection = False
        Me.rtbExpression.HighlightingEnabled = True
        Me.rtbExpression.Name = "rtbExpression"
        Me.rtbExpression.PasswordChar = ChrW(0)
        Me.rtbExpression.ReadOnly = True
        '
        'lbBreakpoints
        '
        resources.ApplyResources(Me.lbBreakpoints, "lbBreakpoints")
        Me.lbBreakpoints.Name = "lbBreakpoints"
        '
        'btnGotoStage
        '
        resources.ApplyResources(Me.btnGotoStage, "btnGotoStage")
        Me.btnGotoStage.Name = "btnGotoStage"
        '
        'btnClearAll
        '
        resources.ApplyResources(Me.btnClearAll, "btnClearAll")
        Me.btnClearAll.Name = "btnClearAll"
        '
        'btnClearBreakpoint
        '
        resources.ApplyResources(Me.btnClearBreakpoint, "btnClearBreakpoint")
        Me.btnClearBreakpoint.Name = "btnClearBreakpoint"
        '
        'pnlHeader
        '
        Me.pnlHeader.BorderColor = System.Drawing.SystemColors.InactiveCaption
        Me.pnlHeader.BorderStyle = AutomateUI.clsPanel.BorderMode.[On]
        Me.pnlHeader.BorderWidth = 1
        Me.pnlHeader.Controls.Add(Me.chkPage)
        Me.pnlHeader.Controls.Add(Me.Label1)
        resources.ApplyResources(Me.pnlHeader, "pnlHeader")
        Me.pnlHeader.Name = "pnlHeader"
        '
        'chkPage
        '
        resources.ApplyResources(Me.chkPage, "chkPage")
        Me.chkPage.Name = "chkPage"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'objBluebar
        '
        resources.ApplyResources(Me.objBluebar, "objBluebar")
        Me.objBluebar.Name = "objBluebar"
        '
        'imgBreakpointIcons
        '
        Me.imgBreakpointIcons.ImageStream = CType(resources.GetObject("imgBreakpointIcons.ImageStream"), System.Windows.Forms.ImageListStreamer)
        Me.imgBreakpointIcons.TransparentColor = System.Drawing.Color.Transparent
        Me.imgBreakpointIcons.Images.SetKeyName(0, "")
        Me.imgBreakpointIcons.Images.SetKeyName(1, "")
        Me.imgBreakpointIcons.Images.SetKeyName(2, "")
        '
        'btnClose
        '
        resources.ApplyResources(Me.btnClose, "btnClose")
        Me.btnClose.Name = "btnClose"
        '
        'btnHelp
        '
        resources.ApplyResources(Me.btnHelp, "btnHelp")
        Me.btnHelp.Name = "btnHelp"
        '
        'frmBreakpointLocator
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.btnClose)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.pnlHeader)
        Me.Controls.Add(Me.rtbExpression)
        Me.Controls.Add(Me.btnClearBreakpoint)
        Me.Controls.Add(Me.btnClearAll)
        Me.Controls.Add(Me.btnGotoStage)
        Me.Controls.Add(Me.trvwStages)
        Me.Controls.Add(Me.lbBreakpoints)
        Me.Controls.Add(Me.lblExpression)
        Me.Controls.Add(Me.objBluebar)
        Me.Name = "frmBreakpointLocator"
        Me.pnlHeader.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private mobjParentProcessStudioForm As frmProcess

    Private Const IMG_CONDITIONAL As Integer = 0
    Private Const IMG_NONCONDITIONAL As Integer = 1
    Private Const IMG_BLANK As Integer = 3


    Public Sub SetParentProcessStudioForm(ByVal ProcessStudioInstance As frmProcess)
        Me.mobjParentProcessStudioForm = ProcessStudioInstance
    End Sub

    Private mProcess As clsProcess


#Region "Private methods"

    ''' <summary>
    ''' Sets the process owning this breakpoint locator and causes
    ''' breakpoint list to be populated.
    ''' </summary>
    ''' <param name="Process">Process.</param>
    Public Sub SetProcess(ByVal Process As clsProcess)
        Me.mProcess = Process
        Me.RepopulateBreakpoints()
    End Sub


    Private Sub RepopulateBreakpoints()
        If Me.chkPage.Checked Then
            PopulateBreakpointTreeviewBySubsheet(mProcess.GetStagesWithBreakpoint())
        Else
            PopulateBreakpointTreeviewByName(mProcess.GetStagesWithBreakpoint())
        End If
    End Sub


    Private Sub PopulateBreakpointTreeviewByName(ByVal Stages As List(Of clsProcessStage))
        Debug.Assert(Not Stages Is Nothing)
        Debug.Assert(Not Me.mProcess Is Nothing)

        'since we only call this locally with the filter StageHasBreakpoint()
        'we can assume that all stages returned do have breakpoints.

        Try
            'if a node is selected remember its name
            Dim SelectedNodeName As String = ""
            If Not Me.trvwStages.SelectedNode Is Nothing Then
                SelectedNodeName = Me.trvwStages.SelectedNode.Text
            End If

            'clear current nodes
            Me.trvwStages.BeginUpdate()
            Me.trvwStages.Nodes.Clear()

            'sort the list of stages
            Dim a As New List(Of clsProcessStage)
            For Each ss As clsProcessStage In Stages
                a.Add(ss)

                'add list to treeview
                Dim tn As TreeNode
                tn = New TreeNode(ss.GetName)
                tn.Tag = ss.GetStageID
                Me.trvwStages.Nodes.Add(tn)
                If tn.Text = SelectedNodeName Then Me.trvwStages.SelectedNode = tn

                'set the appropriate image based on whether breakpoint is conditional or not
                If (ss.BreakPoint.BreakPointType And clsProcessBreakpoint.BreakEvents.WhenConditionMet) > 0 Then
                    tn.ImageIndex = IMG_CONDITIONAL
                    tn.SelectedImageIndex = IMG_CONDITIONAL
                Else
                    tn.ImageIndex = IMG_NONCONDITIONAL
                    tn.SelectedImageIndex = IMG_NONCONDITIONAL
                End If
            Next

        Catch ex As Exception
            UserMessage.Show(My.Resources.frmBreakpointLocator_InternalErrorPopulatingListOfBreakpointStages)
        Finally
            Me.trvwStages.EndUpdate()
        End Try
    End Sub


    Private Sub PopulateBreakpointTreeviewBySubsheet(ByVal Stages As List(Of clsProcessStage))
        Debug.Assert(Not Stages Is Nothing)
        Debug.Assert(Not Me.mProcess Is Nothing)

        'since we only call this locally with the filter StageHasBreakpoint()
        'we can assume that all stages returned do have breakpoints.

        Dim htSubsheetNodesBySheetID As New Hashtable

        Try
            'if a node is selected remember its name
            Dim SelectedNodeName As String = ""
            If Not Me.trvwStages.SelectedNode Is Nothing Then
                SelectedNodeName = Me.trvwStages.SelectedNode.Text
            End If

            'clear current layout
            Me.trvwStages.BeginUpdate()
            Me.trvwStages.Nodes.Clear()

            'loop round stages adding to sheet subnodes
            Dim SheetName As String
            Dim SheetNode As TreeNode
            Dim StageNode As TreeNode
            For Each s As clsProcessStage In Stages
                'get the node of the subsheet the stage belongs to
                SheetName = mProcess.GetSubSheetName(s.GetSubSheetID)
                SheetNode = CType(htSubsheetNodesBySheetID(SheetName), TreeNode)
                If SheetNode Is Nothing Then
                    SheetNode = Me.trvwStages.Nodes.Add(SheetName)
                    htSubsheetNodesBySheetID.Add(SheetName, SheetNode)
                End If
                SheetNode.ImageIndex = IMG_BLANK
                SheetNode.SelectedImageIndex = IMG_BLANK
                'add the stage node to the sheet node
                StageNode = New TreeNode(s.GetName)
                StageNode.Tag = s.GetStageID
                StageNode.NodeFont = New Font(Me.trvwStages.Font, Me.trvwStages.Font.Style And CType((Integer.MaxValue Xor FontStyle.Bold), FontStyle))
                SheetNode.Nodes.Add(StageNode)
                If StageNode.Text = SelectedNodeName Then Me.trvwStages.SelectedNode = StageNode
                'set the appropriate image based on whether breakpoint is conditional or not
                If (s.BreakPoint.BreakPointType And clsProcessBreakpoint.BreakEvents.WhenConditionMet) > 0 Then
                    StageNode.ImageIndex = IMG_CONDITIONAL
                    StageNode.SelectedImageIndex = IMG_CONDITIONAL
                Else
                    StageNode.ImageIndex = IMG_NONCONDITIONAL
                    StageNode.SelectedImageIndex = IMG_NONCONDITIONAL
                End If
            Next

        Catch ex As Exception
            UserMessage.Show(My.Resources.frmBreakpointLocator_InternalErrorPopulatingListOfBreakpointStages)
        Finally
            Me.trvwStages.EndUpdate()
        End Try

    End Sub

    ''' <summary>
    ''' Determines if the supplied stage has a breakpoint.
    ''' </summary>
    ''' <param name="Stage">Stage to test</param>
    ''' <returns>True if has breakpoint; false otherwise.</returns>
    Private Function StageHasBreakpoint(ByVal Stage As clsProcessStage) As Boolean
        Return Stage.HasBreakPoint
    End Function

#End Region


#Region "Events"

    Private Sub trvwStages_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles trvwStages.DoubleClick, btnGotoStage.Click
        Try
            If Not trvwStages.SelectedNode() Is Nothing Then
                If Not trvwStages.SelectedNode.Tag Is Nothing Then              'only do if stage node
                    Dim SelectedStage As clsProcessStage = Me.mProcess.GetStage(CType(trvwStages.SelectedNode.Tag, Guid))
                    Me.mobjParentProcessStudioForm.ProcessViewer.ShowStage(SelectedStage)
                    Me.mobjParentProcessStudioForm.ProcessViewer.InvalidateView()
                End If
            Else
                UserMessage.Show(My.Resources.frmBreakpointLocator_PleaseFirstSelectAStageFromTheTreeviewList)
            End If
        Catch ex As Exception
            UserMessage.Show(My.Resources.frmBreakpointLocator_InternalError)
        End Try
    End Sub


#Region "Button Clicks"

    Private Sub btnClearBreakpoint_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnClearBreakpoint.Click
        Try
            If Not trvwStages.SelectedNode() Is Nothing Then
                If Not trvwStages.SelectedNode.Tag Is Nothing Then              'only do if stage node
                    Dim SelectedStage As clsProcessStage = Me.mProcess.GetStage(CType(trvwStages.SelectedNode.Tag, Guid))
                    If Not SelectedStage Is Nothing Then
                        SelectedStage.BreakPoint = Nothing
                        Me.trvwStages.Nodes.Remove(Me.trvwStages.SelectedNode)
                        Me.mobjParentProcessStudioForm.ProcessViewer.InvalidateView()
                        Me.RepopulateBreakpoints()
                    End If
                End If
            Else
                UserMessage.Show(My.Resources.frmBreakpointLocator_PleaseFirstSelectAStageFromTheTreeviewList)
            End If
        Catch ex As Exception
            UserMessage.Show(My.Resources.frmBreakpointLocator_InternalError)
        End Try
    End Sub

    Private Sub btnClearAll_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnClearAll.Click
        Try
            Me.mobjParentProcessStudioForm.mnuClearAllBreakpoints_Click(Nothing, Nothing)
            Me.mobjParentProcessStudioForm.ProcessViewer.InvalidateView()
            Me.RepopulateBreakpoints()
        Catch ex As Exception
            UserMessage.Show(My.Resources.frmBreakpointLocator_InternalError)
        End Try
    End Sub

#End Region

    Private Sub chkPage_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Try
            RepopulateBreakpoints()
        Catch ex As Exception
            UserMessage.Show(My.Resources.frmBreakpointLocator_InternalError)
        End Try
    End Sub

#End Region

    Private Sub trvwStages_AfterSelect(ByVal sender As Object, ByVal e As System.Windows.Forms.TreeViewEventArgs) Handles trvwStages.AfterSelect
        Try
            'put the breakpoint condition into the expression box, if one exists
            Dim tn As TreeNode = Me.trvwStages.SelectedNode
            If Not tn Is Nothing Then
                Dim stageid As Guid = CType(tn.Tag, Guid)
                If Not stageid.Equals(Guid.Empty) Then
                    Dim stage As clsProcessStage = Me.mProcess.GetStage(stageid)
                    If Not stage Is Nothing Then
                        If stage.HasBreakPoint Then
                            Me.rtbExpression.Text = clsExpression.NormalToLocal(stage.BreakPoint.Condition)
                            Exit Sub
                        End If
                    End If
                End If
            End If

            'default fallback option
            Me.rtbExpression.Text = ""
        Catch ex As Exception
            UserMessage.Show(My.Resources.frmBreakpointLocator_InternalError)
        End Try
    End Sub

    ''' <summary>
    ''' Gets help file
    ''' </summary>
    ''' <returns></returns>
    Public Function GetHelpfile() As String Implements IHelp.GetHelpFile
        Return "frmBreakpointLocator.htm"
    End Function


    Private Sub btnHelp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnHelp.Click
        Try
            OpenHelpFile(Me, GetHelpfile())
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        DialogResult = System.Windows.Forms.DialogResult.Cancel
        Close()
    End Sub

    Public Property EnvironmentBackColor As Color Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return objBluebar.BackColor
        End Get
        Set(value As Color)
            objBluebar.BackColor = value
        End Set
    End Property

    Public Property EnvironmentForeColor As Color Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return objBluebar.TitleColor
        End Get
        Set(value As Color)
            objBluebar.TitleColor = value
        End Set
    End Property
End Class
