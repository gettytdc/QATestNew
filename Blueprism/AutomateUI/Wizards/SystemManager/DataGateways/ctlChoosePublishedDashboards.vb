Imports AutomateControls.Wizard
Imports BluePrism.AutomateAppCore
Imports BluePrism.DataPipeline.DataPipelineOutput
Imports AutomateUI.Wizards.SystemManager.DataGateways.Helpers

Public Class ctlChoosePublishedDashboards
    Inherits WizardPanel
    Implements IDataGatewaysWizardPanel

    Private ReadOnly mConfig As DataPipelineOutputConfig
    Private ReadOnly mDashboardList As List(Of Dashboard)
    Private mSelectedDashboards As New Dictionary(Of String, Boolean)

#Region "Constructor"
    Public Sub New(dataPipelineConfigOutput As DataPipelineOutputConfig, dashboardList As List(Of Dashboard))
        InitializeComponent()

        mConfig = dataPipelineConfigOutput
        mDashboardList = dashboardList
    End Sub
#End Region

#Region "Event handlers"
    Private Sub chkListPublishedDashboard_SelectedIndexChanged(sender As Object, e As EventArgs) Handles chkListPublishedDashboard.SelectedIndexChanged, chkListPublishedDashboard.SelectedValueChanged
        Dim dashboardNameList = chkListPublishedDashboard.Items.Cast(Of String).ToList()
        For each dashboardName In dashboardNameList
            mSelectedDashboards(dashboardName) = chkListPublishedDashboard.CheckedItems.Contains(dashboardName)
        Next
        IsInputCorrect()
    End Sub

    Private Sub btnDeselectAll_LinkClicked(sender As Object, e As EventArgs) Handles btnDeselectAll.LinkClicked
        SetAllDashboardsDataStatus(False)
        SetAllDashboardsCheckedState(CheckState.Unchecked)
        IsInputCorrect()
    End Sub

    Private Sub btnSelectAll_LinkClicked(sender As Object, e As EventArgs) Handles btnSelectAll.LinkClicked
        SetAllDashboardsDataStatus(True)
        SetAllDashboardsCheckedState(CheckState.Checked)
        IsInputCorrect()
    End Sub

    Private Sub SearchBox_TextChanged(sender As Object, e As EventArgs) Handles SearchBox.TextChanged
        If chkListPublishedDashboard.Items.Count > 0
            chkListPublishedDashboard.Items.Clear()
        End If

        For Each dashboard in mSelectedDashboards
            If dashboard.Key.Contains(SearchBox.Text) Then
                chkListPublishedDashboard.Items.Add(dashboard.Key)
                Dim index = chkListPublishedDashboard.Items.IndexOf(dashboard.Key)
                If index >= 0 Then chkListPublishedDashboard.SetItemChecked(index, dashboard.Value)
            End If
        Next
    End Sub

    Private Sub SearchBox_KeyDown(sender As Object, e As KeyEventArgs) Handles SearchBox.KeyDown
        If e.KeyCode = Keys.Enter Then e.SuppressKeyPress = True
    End Sub
#End Region

#Region "Open and close overrides"
    Public Sub OnOpen() Implements IDataGatewaysWizardPanel.OnOpen
        RefreshLabels()
        ClearHistoricData()
        LoadDashboardsIntoCheckList()

        If mConfig.SelectedDashboards IsNot Nothing AndAlso mConfig.SelectedDashboards.Count > 0 Then
            For Each CheckedDashboard In mConfig.SelectedDashboards
                Dim index = chkListPublishedDashboard.Items.IndexOf(CheckedDashboard)
                If index >= 0 Then chkListPublishedDashboard.SetItemChecked(index, True)
                mSelectedDashboards(CheckedDashboard) = True
            Next
            For Each dashboardName In mConfig.SelectedDashboards
                If Not mDashboardList.Exists(function(x) x.Name = dashboardName) Then _
                    mSelectedDashboards.Remove(dashboardName)
            Next
        End If

        IsInputCorrect()
        SearchBox.SelectionIndent = 19

        If mSelectedDashboards.Any(Function(X) X.Value) Then
            NavigateNext = True
            UpdateNavigate()
        End If
    End Sub

    Public Sub Closing() Implements IDataGatewaysWizardPanel.Closing
        Dim checkedDashboards As New List(Of String)
        For Each item In mSelectedDashboards
            If item.Value = True Then checkedDashboards.Add(item.Key)
        Next
        mConfig.SelectedDashboards = checkedDashboards
    End Sub
#End Region

#Region "Private methods"
    Private Sub LoadDashboardsIntoCheckList()
        For Each dashboard in mDashboardList
            If Not mSelectedDashboards.ContainsKey(dashboard.Name) AndAlso
             dashboard.Type = DashboardTypes.Published Then
                chkListPublishedDashboard.Items.Add(dashboard.Name)
                mSelectedDashboards.Add(dashboard.Name, False)
            End If
        Next
    End Sub

    Private Sub RefreshLabels()
        lblConfigName.Text = mConfig.Name
        lblSelectDashboardMessage.Text = String.Format(My.Resources.ChooseThePublishedDashboardsToBeSentTo0, GetLocalizedFriendlyNameToLower(mConfig.OutputType.Id))
    End Sub

    Private Sub SetAllDashboardsDataStatus(status As Boolean)
        Dim dashboardNameList = chkListPublishedDashboard.Items.Cast(Of String).ToList()
        For each dashboardName In dashboardNameList
            mSelectedDashboards(dashboardName) = status
        Next
    End Sub

    Private Sub SetAllDashboardsCheckedState(checkedState As CheckState)
        For i As Integer = 0 To chkListPublishedDashboard.Items.count - 1
            chkListPublishedDashboard.SetItemCheckState(i, checkedState)
        Next
    End Sub

    Private Sub IsInputCorrect()
        If mSelectedDashboards.Any(Function(X) X.Value) Then
            NavigateNext = True
        Else
            NavigateNext = False
        End If
        UpdateNavigate()
    End Sub

    Private Sub ClearHistoricData()
        If chkListPublishedDashboard.Items.Count > 0 Then _
            chkListPublishedDashboard.Items.Clear()
        If mSelectedDashboards.Count > 0 Then _
            mSelectedDashboards.Clear()
    End Sub
#End Region

#Region "Windows designer code"
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlChoosePublishedDashboards))
        Me.chkListPublishedDashboard = New System.Windows.Forms.CheckedListBox()
        Me.lblOutputName = New System.Windows.Forms.Label()
        Me.lblConfigName = New System.Windows.Forms.Label()
        Me.lblSelectDashboardMessage = New System.Windows.Forms.Label()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.btnDeselectAll = New AutomateControls.BulletedLinkLabel()
        Me.btnSelectAll = New AutomateControls.BulletedLinkLabel()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.SearchBox = New AutomateUI.ctlRichTextBox()
        Me.lblSearch = New System.Windows.Forms.Label()
        Me.Panel1.SuspendLayout
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'chkListPublishedDashboard
        '
        Me.chkListPublishedDashboard.BackColor = System.Drawing.Color.FromArgb(CType(CType(208, Byte), Integer), CType(CType(238, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.chkListPublishedDashboard.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.chkListPublishedDashboard.CheckOnClick = True
        resources.ApplyResources(Me.chkListPublishedDashboard, "chkListPublishedDashboard")
        Me.chkListPublishedDashboard.FormattingEnabled = True
        Me.chkListPublishedDashboard.Name = "chkListPublishedDashboard"
        '
        'lblOutputName
        '
        resources.ApplyResources(Me.lblOutputName, "lblOutputName")
        Me.lblOutputName.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblOutputName.Name = "lblOutputName"
        '
        'lblConfigName
        '
        resources.ApplyResources(Me.lblConfigName, "lblConfigName")
        Me.lblConfigName.ForeColor = System.Drawing.SystemColors.ControlDarkDark
        Me.lblConfigName.Name = "lblConfigName"
        '
        'lblSelectDashboardMessage
        '
        resources.ApplyResources(Me.lblSelectDashboardMessage, "lblSelectDashboardMessage")
        Me.lblSelectDashboardMessage.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblSelectDashboardMessage.Name = "lblSelectDashboardMessage"
        '
        'Panel1
        '
        Me.Panel1.BackColor = System.Drawing.Color.FromArgb(CType(CType(208, Byte), Integer), CType(CType(238, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.Panel1.Controls.Add(Me.btnDeselectAll)
        Me.Panel1.Controls.Add(Me.btnSelectAll)
        Me.Panel1.Controls.Add(Me.PictureBox1)
        Me.Panel1.Controls.Add(Me.SearchBox)
        Me.Panel1.Controls.Add(Me.lblSelectDashboardMessage)
        Me.Panel1.Controls.Add(Me.lblSearch)
        Me.Panel1.Controls.Add(Me.chkListPublishedDashboard)
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.Name = "Panel1"
        '
        'btnDeselectAll
        '
        resources.ApplyResources(Me.btnDeselectAll, "btnDeselectAll")
        Me.btnDeselectAll.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.btnDeselectAll.Name = "btnDeselectAll"
        Me.btnDeselectAll.TabStop = True
        Me.btnDeselectAll.UseCompatibleTextRendering = True
        '
        'btnSelectAll
        '
        resources.ApplyResources(Me.btnSelectAll, "btnSelectAll")
        Me.btnSelectAll.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.btnSelectAll.Name = "btnSelectAll"
        Me.btnSelectAll.TabStop = True
        Me.btnSelectAll.UseCompatibleTextRendering = True
        '
        'PictureBox1
        '
        Me.PictureBox1.BackColor = System.Drawing.Color.White
        resources.ApplyResources(Me.PictureBox1, "PictureBox1")
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.TabStop = False
        '
        'SearchBox
        '
        resources.ApplyResources(Me.SearchBox, "SearchBox")
        Me.SearchBox.Name = "SearchBox"
        '
        'lblSearch
        '
        resources.ApplyResources(Me.lblSearch, "lblSearch")
        Me.lblSearch.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblSearch.Name = "lblSearch"
        '
        'ctlChoosePublishedDashboards
        '
        Me.Controls.Add(Me.lblOutputName)
        Me.Controls.Add(Me.lblConfigName)
        Me.Controls.Add(Me.Panel1)
        Me.Name = "ctlChoosePublishedDashboards"
        Me.NavigatePrevious = True
        resources.ApplyResources(Me, "$this")
        Me.Title = Global.AutomateUI.My.Resources.Resources.ctlChooseDashboards_title
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents chkListPublishedDashboard As CheckedListBox
    Friend WithEvents lblOutputName As Label
    Friend WithEvents lblConfigName As Label
    Friend WithEvents lblSelectDashboardMessage As Label
    Friend WithEvents Panel1 As Panel

    Friend WithEvents lblSearch As Label
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents SearchBox As ctlRichTextBox
    Friend WithEvents btnSelectAll As AutomateControls.BulletedLinkLabel
    Friend WithEvents btnDeselectAll As AutomateControls.BulletedLinkLabel
#End Region
End Class
