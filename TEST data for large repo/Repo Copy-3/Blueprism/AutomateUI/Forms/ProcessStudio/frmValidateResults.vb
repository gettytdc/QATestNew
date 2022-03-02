Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.AutomateAppCore
Imports BluePrism.BPCoreLib.Collections
Imports AutomateControls
Imports LocaleTools
Imports BluePrism.AutomateProcessCore.ProcessLoading
Imports BluePrism.AutomateAppCore.Utility

Friend Class frmValidateResults
    Inherits frmForm
    Implements IHelp

    Public Event ValidationErrorCountUpdated(ByVal count As Integer)

#Region " Windows Form Designer generated code "

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
    Friend WithEvents btnClose As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnRepair As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnCheckAgain As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnGoto As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents chkBusinessObjects As System.Windows.Forms.CheckBox
    Friend WithEvents btnRelatedHelp As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnHelp As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents gridResults As System.Windows.Forms.DataGridView
    Friend WithEvents colGridPage As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colGridStageName As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colGridType As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colGridAction As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colGridDescription As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colGridRepairable As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents gridResultsContextMenu As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents copyMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents txtStatus As AutomateControls.Textboxes.StyledTextBox
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmValidateResults))
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.btnClose = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnRepair = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnCheckAgain = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnGoto = New AutomateControls.Buttons.StandardStyledButton()
        Me.chkBusinessObjects = New System.Windows.Forms.CheckBox()
        Me.txtStatus = New AutomateControls.Textboxes.StyledTextBox()
        Me.btnRelatedHelp = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnHelp = New AutomateControls.Buttons.StandardStyledButton()
        Me.gridResults = New System.Windows.Forms.DataGridView()
        Me.colGridPage = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colGridStageName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colGridType = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colGridAction = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colGridDescription = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colGridRepairable = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.gridResultsContextMenu = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.copyMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        CType(Me.gridResults, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.gridResultsContextMenu.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnClose
        '
        resources.ApplyResources(Me.btnClose, "btnClose")
        Me.btnClose.Name = "btnClose"
        '
        'btnRepair
        '
        resources.ApplyResources(Me.btnRepair, "btnRepair")
        Me.btnRepair.Name = "btnRepair"
        '
        'btnCheckAgain
        '
        resources.ApplyResources(Me.btnCheckAgain, "btnCheckAgain")
        Me.btnCheckAgain.Name = "btnCheckAgain"
        '
        'btnGoto
        '
        resources.ApplyResources(Me.btnGoto, "btnGoto")
        Me.btnGoto.Name = "btnGoto"
        '
        'chkBusinessObjects
        '
        resources.ApplyResources(Me.chkBusinessObjects, "chkBusinessObjects")
        Me.chkBusinessObjects.Name = "chkBusinessObjects"
        '
        'txtStatus
        '
        resources.ApplyResources(Me.txtStatus, "txtStatus")
        Me.txtStatus.Name = "txtStatus"
        Me.txtStatus.ReadOnly = True
        '
        'btnRelatedHelp
        '
        resources.ApplyResources(Me.btnRelatedHelp, "btnRelatedHelp")
        Me.btnRelatedHelp.Name = "btnRelatedHelp"
        '
        'btnHelp
        '
        resources.ApplyResources(Me.btnHelp, "btnHelp")
        Me.btnHelp.Name = "btnHelp"
        '
        'gridResults
        '
        Me.gridResults.AllowUserToAddRows = False
        Me.gridResults.AllowUserToResizeRows = False
        resources.ApplyResources(Me.gridResults, "gridResults")
        Me.gridResults.BackgroundColor = System.Drawing.SystemColors.Window
        Me.gridResults.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.gridResults.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None
        Me.gridResults.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None
        Me.gridResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.gridResults.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colGridPage, Me.colGridStageName, Me.colGridType, Me.colGridAction, Me.colGridDescription, Me.colGridRepairable})
        Me.gridResults.ContextMenuStrip = Me.gridResultsContextMenu
        DataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft
        DataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle1.Font = New System.Drawing.Font("Segoe UI", 8.25!)
        DataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle1.Padding = New System.Windows.Forms.Padding(2, 0, 0, 0)
        DataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        DataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.[False]
        Me.gridResults.DefaultCellStyle = DataGridViewCellStyle1
        Me.gridResults.MultiSelect = False
        Me.gridResults.Name = "gridResults"
        Me.gridResults.ReadOnly = True
        Me.gridResults.RowHeadersVisible = False
        Me.gridResults.RowTemplate.Height = 16
        Me.gridResults.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        '
        'colGridPage
        '
        Me.colGridPage.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        resources.ApplyResources(Me.colGridPage, "colGridPage")
        Me.colGridPage.Name = "colGridPage"
        Me.colGridPage.ReadOnly = True
        '
        'colGridStageName
        '
        Me.colGridStageName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None
        resources.ApplyResources(Me.colGridStageName, "colGridStageName")
        Me.colGridStageName.Name = "colGridStageName"
        Me.colGridStageName.ReadOnly = True
        '
        'colGridType
        '
        Me.colGridType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        resources.ApplyResources(Me.colGridType, "colGridType")
        Me.colGridType.Name = "colGridType"
        Me.colGridType.ReadOnly = True
        '
        'colGridAction
        '
        Me.colGridAction.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells
        resources.ApplyResources(Me.colGridAction, "colGridAction")
        Me.colGridAction.Name = "colGridAction"
        Me.colGridAction.ReadOnly = True
        '
        'colGridDescription
        '
        Me.colGridDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.colGridDescription, "colGridDescription")
        Me.colGridDescription.Name = "colGridDescription"
        Me.colGridDescription.ReadOnly = True
        '
        'colGridRepairable
        '
        Me.colGridRepairable.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
        resources.ApplyResources(Me.colGridRepairable, "colGridRepairable")
        Me.colGridRepairable.Name = "colGridRepairable"
        Me.colGridRepairable.ReadOnly = True
        '
        'gridResultsContextMenu
        '
        Me.gridResultsContextMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.copyMenuItem})
        Me.gridResultsContextMenu.Name = "ContextMenuStrip1"
        resources.ApplyResources(Me.gridResultsContextMenu, "gridResultsContextMenu")
        '
        'copyMenuItem
        '
        Me.copyMenuItem.Name = "copyMenuItem"
        resources.ApplyResources(Me.copyMenuItem, "copyMenuItem")
        '
        'frmValidateResults
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.gridResults)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btnRelatedHelp)
        Me.Controls.Add(Me.txtStatus)
        Me.Controls.Add(Me.chkBusinessObjects)
        Me.Controls.Add(Me.btnGoto)
        Me.Controls.Add(Me.btnCheckAgain)
        Me.Controls.Add(Me.btnRepair)
        Me.Controls.Add(Me.btnClose)
        Me.Name = "frmValidateResults"
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show
        CType(Me.gridResults, System.ComponentModel.ISupportInitialize).EndInit()
        Me.gridResultsContextMenu.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

#Region " Constructors "

    Public Sub New(ByVal proc As clsProcess)
        Me.New(proc, Nothing, ValidateProcessResult.SourceTypes.Normal)
    End Sub

    Public Sub New(ByVal proc As clsProcess, ByVal stage As clsProcessStage, ByVal errorSource As ValidateProcessResult.SourceTypes)
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        mProcess = proc
        mStage = stage
        mErrorSource = errorSource
        mUseFilter = (errorSource = ValidateProcessResult.SourceTypes.Code)

    End Sub

#End Region

#Region " Events "

    ''' <summary>
    ''' Event raised when the form wants the calling process view to jump to another
    ''' stage.
    ''' </summary>
    ''' <param name="gID">The stage to go to</param>
    Public Event GotoStage(ByVal gID As Guid)

    ''' <summary>
    ''' Event raised to indicate that any highlighting of
    ''' stages.
    ''' </summary>
    ''' <remarks>Such stages may have been highlighted when the user has clicked goto
    ''' stage, for example.</remarks>
    Public Event ClearHighlighting()

#End Region

#Region " Member Variables "

    ' Flag indicating if a validation operation is currently in progress
    Private mValidating As Boolean = False

    ''' <summary>
    ''' The process we are working with. Set by the constructor, and never changes.
    ''' </summary>
    Private mProcess As clsProcess

    ''' <summary>
    ''' The stage in the process to filter results by.
    ''' </summary>
    ''' <remarks></remarks>
    Private mStage As clsProcessStage

    Private mUseFilter As Boolean

    Private mErrorSource As ValidateProcessResult.SourceTypes

    ''' <summary>
    ''' The results of the last validation we did.
    ''' </summary>
    Private mResults As ICollection(Of ValidateProcessResult)

    Private mControlLoadComplete As Boolean = False

#End Region

    ''' <summary>
    ''' The (first) currently selected validation check, if any is selected, or null
    ''' if no check is selected.
    ''' </summary>
    Private ReadOnly Property SelectedResult() As ValidateProcessResult
        Get
            If gridResults.SelectedRows.Count > 0 Then _
             Return DirectCast(gridResults.SelectedRows(0).Tag, ValidateProcessResult)
            Return Nothing
        End Get
    End Property


    Private Sub frmValidateResults_FormClosed(ByVal sender As Object, ByVal e As FormClosingEventArgs) Handles Me.FormClosing
        RaiseEvent ClearHighlighting()
    End Sub

    Private Sub frmValidateResults_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load

        chkBusinessObjects.Checked = True
        If mProcess.ProcessType = DiagramType.Object Then
            Text = String.Format(My.Resources.frmValidateResults_BusinessObjectValidation0, mProcess.Name)
        Else
            Text = String.Format(My.Resources.frmValidateResults_ProcessValidation0, mProcess.Name)
        End If

        mControlLoadComplete = True

        DoValidation(False)

    End Sub

    ''' <summary>
    ''' Refreshes the validation list, by getting a fresh list of errors from the
    ''' process, and repopulating the list.
    ''' </summary>
    ''' <param name="bAttemptRepair">Determines whether the process should be
    ''' instructed to self-correct, during the examination process.</param>
    Private Sub DoValidation(ByVal bAttemptRepair As Boolean)
        Me.Cursor = Cursors.WaitCursor
        If Not mControlLoadComplete Then Return

        clsAPC.ProcessLoader.CacheBehaviour = CacheRefreshBehaviour.NeverCheckForUpdates

        Dim allSettings As IDictionary(Of Integer, IDictionary(Of Integer, Integer)) =
         gSv.GetValidationAllActionSettings()
        Dim actions As IDictionary(Of Integer, String) = gSv.GetValidationActions()

        Dim rules = gSv.GetValidationInfo()
        Dim validationInfo = rules.ToDictionary(Of Integer, clsValidationInfo)(Function(y) y.CheckID, Function(z) z)
        Dim exTypes As New HashSet(Of String)(StringComparer.CurrentCultureIgnoreCase)
        Dim types As IDictionary(Of Integer, String) = gSv.GetValidationTypes()

        ' if we are performing exception type validation we need to pass in the
        ' list of existing exception types
        Try
            If validationInfo(142).Enabled Then
                Dim exceptionTypes = gSv.GetExceptionTypes()
                exTypes.UnionWith(exceptionTypes)
            End If
        Catch ex As Exception
            UserMessage.Err(ex.Message)
        End Try

        mResults = clsValidationInfo.FilteredValidateProcess(
         mProcess, validationInfo, bAttemptRepair, Not chkBusinessObjects.Checked, exTypes
         )

        Dim total As Integer = 0
        Dim counts As New clsGeneratorDictionary(Of clsValidationInfo.Types, Integer)
        Dim numCanRepair As Integer = 0

        mValidating = True
        Try
            gridResults.Rows.Clear()
            For Each res As ValidateProcessResult In mResults

                If mUseFilter Then
                    If mStage IsNot Nothing AndAlso mStage.Id <> res.StageId Then Continue For
                    If mErrorSource <> res.ErrorSource Then Continue For
                End If

                'Get the validation info for this error...
                Dim info As clsValidationInfo = validationInfo(res.CheckID)

                'Don't add if the 'Ignore' action has been specified.
                Dim actionid As Integer = allSettings(info.CatID)(info.TypeID)
                If actionid = clsValidationInfo.Actions.Ignore Then Continue For
                Dim action As String = actions(actionid)

                ' At this point we're committed to including the check.
                counts(info.TypeID) += 1
                total += 1

                If res.Repairable Then numCanRepair += 1

                Dim page As String = res.PageName
                If page = "" Then page = My.Resources.frmValidateResults_NA

                Dim stageName As String = res.StageName
                If stageName = "" Then stageName = My.Resources.frmValidateResults_NA

                'Add row...
                Dim rowInd As Integer = gridResults.Rows.Add(LTools.GetC(page, "misc", "page"), LTools.GetC(stageName, "misc", "stage"), types(info.TypeID),
                 action, res.FormatMessage(info.Message), IIf(res.Repairable, My.Resources.frmValidateResults_Yes, My.Resources.frmValidateResults_No))
                gridResults.Rows(rowInd).Tag = res

            Next

            If mResults.Count = 0 Then
                txtStatus.Text = My.Resources.frmValidateResults_NoResults

            Else
                txtStatus.Text = LTools.Format(My.Resources.frmValidateResults_plural_ResultsCanBeAutomaticallyRepaired, "TOTAL", total, "NUM", numCanRepair)
            End If

            RaiseEvent ValidationErrorCountUpdated(counts(clsValidationInfo.Types.Error))

        Finally
            Me.Cursor = Cursors.Default
            mValidating = False
        End Try
        UpdateGridUI()

        clsAPC.ProcessLoader.CacheBehaviour = CacheRefreshBehaviour.CheckForUpdatesEveryTime
    End Sub

    ''' <summary>
    ''' Refreshes the UI, by enabling/disabling buttons, etc, as appropriate to the
    ''' application state.
    ''' </summary>
    Private Sub UpdateGridUI()

        If mValidating Then Return ' Ignore updates if validating.

        btnGoto.Enabled = False
        btnRelatedHelp.Enabled = False

        Dim res As ValidateProcessResult = SelectedResult
        If res IsNot Nothing Then
            If res.StageId <> Guid.Empty Then btnGoto.Enabled = Not mUseFilter
            If res.HelpReference <> "" Then btnRelatedHelp.Enabled = True
        End If

        Dim bCanRepair As Boolean = False
        For Each r As ValidateProcessResult In mResults
            If r.Repairable Then
                bCanRepair = True
                Exit For
            End If
        Next
        btnRepair.Enabled = bCanRepair
    End Sub

    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        Close()
    End Sub

    Private Sub btnCheckAgain_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCheckAgain.Click
        DoValidation(False)
    End Sub

    Private Sub btnRepair_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRepair.Click
        'Fix the errors...
        DoValidation(True)
        'Validate again to get a list without the errors (hopefully!)...
        DoValidation(False)
    End Sub

    Private Sub gridResults_SelectionChanged(ByVal sender As Object, ByVal e As EventArgs) Handles gridResults.SelectionChanged
        UpdateGridUI()
    End Sub

    Private Sub btnGoto_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnGoto.Click
        GotoGridSelection()
    End Sub

    Private Sub gridResults_DoubleClick(ByVal sender As Object, ByVal e As EventArgs) Handles gridResults.DoubleClick
        GotoGridSelection()
    End Sub


    ''' <summary>
    ''' Selects the clicked row in the validation results grid - this is so right-clicking to open the context menu also changes the selected row.
    '''
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub HandleResultsCellMouseDown(ByVal sender As System.Object, ByVal e As DataGridViewCellMouseEventArgs) Handles gridResults.CellMouseDown

        If e.ColumnIndex >= 0 And e.RowIndex >= 0 Then
            gridResults.CurrentCell = gridResults.Rows(e.RowIndex).Cells(e.ColumnIndex)
        End If

    End Sub

    ''' <summary>
    ''' Copies the selected validation result to the clipboard 
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub HandleGridResultsCopyMenuItemClick(ByVal sender As Object, ByVal e As EventArgs) Handles copyMenuItem.Click

        Dim clipboardText As New StringBuilder()

        For Each row As DataGridViewRow In gridResults.SelectedRows
            With row
                clipboardText.AppendFormat(
                My.Resources.frmValidateResults_Page10Stage20Type30Action40Description50Repairable600,
                Environment.NewLine,
                .Cells("colGridPage").Value,
                .Cells("colGridStageName").Value,
                .Cells("colGridType").Value,
                .Cells("colGridAction").Value,
                .Cells("colGridDescription").Value,
                .Cells("colGridRepairable").Value
                )

            End With
        Next

        Clipboard.SetText(clipboardText.ToString())

    End Sub

    ''' <summary>
    ''' Causes the process diagram to be navigated to show the stage concerned with
    ''' the current selection.
    ''' </summary>
    Private Sub GotoGridSelection()
        Dim res As ValidateProcessResult = SelectedResult
        If res IsNot Nothing Then RaiseEvent GotoStage(res.StageId)
    End Sub

    ''' <summary>
    ''' Called if the form is already open, but the caller wants to refresh it.
    ''' </summary>
    Public Sub Reactivate()
        DoValidation(False)
    End Sub

    Private Sub chkBusinessObjects_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkBusinessObjects.CheckedChanged
        DoValidation(False)
    End Sub

    Private Sub btnHelp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnHelp.Click
        Try
            OpenHelpFile(Me, GetHelpFile())
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub



    ''' <summary>
    ''' Gets the appropriate help file.
    ''' </summary>
    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "frmValidateResults.htm"
    End Function

    Private Sub btnRelatedHelp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRelatedHelp.Click
        Dim res As ValidateProcessResult = SelectedResult
        If res IsNot Nothing Then
            Dim topic As Integer
            If Integer.TryParse(res.HelpReference, topic) Then
                HelpLauncher.ShowTopicNumber(Me, topic)
            Else
                Try
                    OpenHelpFile(Me, res.HelpReference)
                Catch
                    UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
                End Try
            End If
        End If
    End Sub

End Class
