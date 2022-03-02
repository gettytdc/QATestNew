Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.BPCoreLib
Imports BluePrism.AutomateProcessCore
Imports AutomateControls.TreeList
Imports LocaleTools
Imports BluePrism.Core.Resources
Imports BluePrism.Server.Domain.Models

''' Project  : Automate
''' Class    : frmProcessMI
''' 
''' <summary>
''' A form used to display session log data as MI on the process
''' diagram.
''' </summary>
''' <remarks>
''' 
''' Possible enhancements:
''' Fetch log data grouped by session id as well as stage id. This could
''' enable a more efficient data search. Currently if you analyse 10
''' sessions then uncheck one session and press Analyse again, all the data
''' is queried again. Similarly, if you analyse 9 of the 10 sessions on the
''' list, then check the 10th session and press Analyse, all the data is 
''' re-read. The downside of grouping by session id is that the SQl maybe slower.
''' 
''' In the same vein, the session data held in memory as clsProcessMI could
''' be manipulated as the user changes params of this form. EG if the data
''' object holds last month's sessions and the user changes the dates to last
''' week, strip the unwanted data out of the clsProcessMI object rather than 
''' re-query. This could be more hassle than its worth though.
''' 
''' </remarks>
Friend Class frmProcessMI : Inherits AutomateControls.Forms.HelpButtonForm
    Implements IEnvironmentColourManager

    ''' <summary>
    ''' The parent process form
    ''' </summary>
    Private mProcessForm As frmProcess


    ''' <summary>
    ''' The MI data object
    ''' </summary>
    Private mProcessMI As clsProcessMI

    ''' <summary>
    ''' The DB object
    ''' </summary>
    Private mDBConnection As clsDBConnection

    ''' <summary>
    ''' The process viewer on the parent form
    ''' </summary>
    Private mProcessViewer As ctlProcessViewer

    ''' <summary>
    ''' The form's tooltip object
    ''' </summary>
    Private mToolTip As ToolTip

    ''' <summary>
    ''' The process ID
    ''' </summary>
    Private mProcessID As Guid

    ''' <summary>
    ''' The current subsheet ID
    ''' </summary>
    Private mViewSubSheet As Guid

    ''' <summary>
    ''' A flag to indicate that one or more sessions have been read from
    ''' the DB.
    ''' </summary>
    Private mbGotSessions As Boolean

    ''' <summary>
    ''' The list of stages IDs
    ''' </summary>
    Private maCheckedStageIDs As Guid()

    ''' <summary>
    ''' The selected stage types.
    ''' </summary>
    Private miStageType As StageTypes

    ''' <summary>
    ''' Used to display session data read from the DB as MI on the screen.
    ''' </summary>
    Private WithEvents moBackgroundWorker As BackgroundWorker

    ''' <summary>
    ''' A list of selected resources supplied to the data object.
    ''' </summary>
    Private mResourceNames As List(Of String)

    ''' <summary>
    ''' A flag to close the form after the run worker is cancelled.
    ''' </summary>
    Private mbCloseFormAfterCancellation As Boolean

    ''' <summary>
    ''' A flag to hide the form after the run worker is cancelled.
    ''' </summary>
    Private mbHideFormAfterCancellation As Boolean

    ' Friendly names
    Private ReadOnly aOldNodeText As String() = New String() {"Actions", "Alerts", "Calculations", "Decisions", "Ends", "Exceptions", "Multiple Calculations", "Processes", "Recovers", "Resumes", "Starts", "Sub Sheets", "Choice Starts", "Wait Starts"}
    Private ReadOnly aNewNodeText As String() = New String() {My.Resources.frmProcessMI_Actions, My.Resources.frmProcessMI_Alerts, My.Resources.frmProcessMI_Calculations, My.Resources.frmProcessMI_Decisions, My.Resources.frmProcessMI_Ends, My.Resources.frmProcessMI_Exceptions, My.Resources.frmProcessMI_MultipleCalculations, My.Resources.frmProcessMI_Processes, My.Resources.frmProcessMI_Recovers, My.Resources.frmProcessMI_Resumes, My.Resources.frmProcessMI_Starts, My.Resources.frmProcessMI_Pages, My.Resources.frmProcessMI_Choices, My.Resources.frmProcessMI_Waits}


    ''' <summary>
    ''' Constructor
    ''' </summary>
    ''' <param name="ProcessForm">The parent process form</param>
    ''' <param name="ProcessID">The process ID</param>
    Public Sub New(ByVal ProcessForm As frmProcess, ByVal ProcessID As Guid)
        MyBase.New()
        InitializeComponent()
        Text = String.Format(My.Resources.frmProcessMI_ProcessMIFor0, ProcessForm.ProcessViewer.Process.Name)
        mProcessForm = ProcessForm
        mProcessID = ProcessID
        mProcessViewer = mProcessForm.ProcessViewer
        mViewSubSheet = mProcessViewer.Process.GetActiveSubSheet()

        mProcessMI = New clsProcessMI(mProcessViewer.Process, aOldNodeText, aNewNodeText)

        moBackgroundWorker = New BackgroundWorker
        moBackgroundWorker.WorkerReportsProgress = True
        moBackgroundWorker.WorkerSupportsCancellation = True

        moResourceView.ShowLocalResourcesOnAllMachines = True
    End Sub





    ''' <summary>
    ''' UserControl overrides dispose to clean up the component list.
    ''' </summary>
    ''' <param name="disposing"></param>
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If

        mProcessMI.ClearData()
        mProcessViewer.InvalidateView()
        RemoveHandler mProcessViewer.pbview.Paint, AddressOf PictureBox_Paint

        MyBase.Dispose(disposing)
    End Sub





    ''' <summary>
    ''' Reads data for the selected sessions calling ReportProgress to
    ''' update the process diagram. 
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks>Data is read from the DB two sessions at a time to try to avoid 
    ''' reading huge amounts of data. This may need a rethink with massive log tables.
    ''' </remarks>
    Private Sub BackgroundWorker_DoWork(ByVal sender As Object, ByVal e As DoWorkEventArgs) _
       Handles moBackgroundWorker.DoWork

        Dim aSessionNumbers As Integer()
        Dim aSessionNumberSubSet As Integer()
        Dim iSubsetLength As Integer

        If moBackgroundWorker.CancellationPending Then
            e.Cancel = True
            Exit Sub
        End If

        'Divide the array of session IDs into subsets and read the statistics 
        'for each subset from the database
        aSessionNumbers = CType(e.Argument, Integer())
        iSubsetLength = 2
        For i As Integer = 0 To aSessionNumbers.Length - 1

            If i + iSubsetLength > aSessionNumbers.Length Then
                'Make smaller subset to reach the end of the aSessionNumbers.
                aSessionNumberSubSet = New Integer(aSessionNumbers.Length - i - 1) {}
                Array.Copy(aSessionNumbers, i, aSessionNumberSubSet, 0, aSessionNumbers.Length - i)
            Else
                'Take a subset from aSessionNumbers.
                aSessionNumberSubSet = New Integer(iSubsetLength - 1) {}
                Array.Copy(aSessionNumbers, i, aSessionNumberSubSet, 0, iSubsetLength)
            End If

            mProcessMI.ReadSessionData(aSessionNumberSubSet, miStageType, maCheckedStageIDs)

            If moBackgroundWorker.CancellationPending Then
                e.Cancel = True
                Exit For
            Else
                moBackgroundWorker.ReportProgress(CInt(100 * i / aSessionNumbers.Length))
                i += aSessionNumberSubSet.Length - 1
            End If

        Next

    End Sub


    ''' <summary>
    ''' Responds to calls to BackgroundWorker.ReportProgress by invalidating the
    ''' ctleProcessViewer and updating the progress bar.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub BackgroundWorker_ProgressChanged(ByVal sender As Object, ByVal e As ProgressChangedEventArgs) _
     Handles moBackgroundWorker.ProgressChanged

        mProcessViewer.InvalidateView()
        moProgressBar.Value = e.ProgressPercentage

    End Sub


    ''' <summary>
    ''' Signals the end of the background worker activity, a cancellation by the user or an error. 
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub BackgroundWorker_RunWorkerCompleted(ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs) _
     Handles moBackgroundWorker.RunWorkerCompleted

        Dim bCloseForm As Boolean
        Dim bHideForm As Boolean

        If Not e.Error Is Nothing Then
            DisplayError(lblMessage, My.Resources.frmProcessMI_AnErrorHasOccurred)
            mProcessMI.ClearData()
        ElseIf e.Cancelled Then
            DisplayInfo(lblMessage, My.Resources.frmProcessMI_Cancelled)
            mProcessMI.ClearData()

            bCloseForm = mbCloseFormAfterCancellation
            bHideForm = mbHideFormAfterCancellation

        Else
            If mProcessMI.DataExists Then
                DisplayInfo(lblMessage, My.Resources.frmProcessMI_Complete)
            Else
                DisplayInfo(lblMessage, My.Resources.frmProcessMI_NoDataFound)
            End If
            moProgressBar.Value = moProgressBar.Maximum
        End If

        If bCloseForm Then
            Close()
        Else

            EnableControls(True)
            btnCancel.Enabled = False
            btnClear.Enabled = True
            btnExport.Enabled = True
            If bHideForm Then
                Hide()
                mbHideFormAfterCancellation = False
            End If

        End If

    End Sub


    ''' <summary>
    ''' Queries the DB for sessions matching the form parameters and lists the results.
    ''' </summary>
    Private Sub GetSessions()

        Dim bInvalidParameter As Boolean
        Dim dFrom, dTo As Date
        Dim resourceIDs As HashSet(Of Guid) = Nothing
        Dim sMessage As String = ""
        Dim oDataTable As DataTable
        Dim oListItem As ListViewItem
        Dim oListSubItem As ListViewItem.ListViewSubItem

        Cursor = Cursors.WaitCursor

        If ctlFromDate.Value.IsNull Then
            bInvalidParameter = True
            sMessage = My.Resources.frmProcessMI_StartDateNotSet
        End If

        If Not bInvalidParameter AndAlso ctlToDate.Value.IsNull Then
            bInvalidParameter = True
            sMessage = My.Resources.frmProcessMI_EndDateNotSet
        End If

        If Not bInvalidParameter Then

            If Not chkDebugOnly.Checked Then
                If moResourceView.SelectedItems.Count = 0 Then
                    bInvalidParameter = True
                    sMessage = My.Resources.frmProcessMI_ResourcesNotSelected
                Else
                    resourceIDs = New HashSet(Of Guid)
                    mResourceNames = New List(Of String)
                    For Each item As TreeListViewItem In moResourceView.SelectedItems
                        Dim res As ResourceGroupMember = TryCast(item.Tag, ResourceGroupMember)
                        If res IsNot Nothing Then
                            resourceIDs.Add(res.IdAsGuid)
                            mResourceNames.Add(res.Name)
                        End If
                        Dim gp As IGroup = TryCast(item.Tag, IGroup)
                        If gp IsNot Nothing Then
                            gp.Scan(Of ResourceGroupMember)(
                                Sub(m)
                                    resourceIDs.Add(m.IdAsGuid)
                                    mResourceNames.Add(m.Name)
                                End Sub)
                        End If

                    Next
                End If
            Else
                Dim sDebugResourceName As String = ResourceMachine.GetName() & "_debug"
                resourceIDs = New HashSet(Of Guid)
                mResourceNames = New List(Of String)
                mResourceNames.Add(sDebugResourceName)
                Dim gDebugResourceID As Guid = gSv.GetResourceId(sDebugResourceName)
                resourceIDs.Add(gDebugResourceID)
            End If

        End If

        If bInvalidParameter Then
            DisplayError(lblFound, sMessage)
            EnableAnalysis(False)
        Else
            dFrom = CDate(ctlFromDate.Value)
            dTo = CDate(ctlToDate.Value)

            Try
                oDataTable = gSv.MIReadSessions(mProcessID, dFrom, dTo, resourceIDs, chkDebugOnly.Checked, mProcessViewer.ModeIsObjectStudio)
                moListView.Items.Clear()

                For Each oRow As DataRow In oDataTable.Rows

                    If chkDebugOnly.Checked Then
                        oListItem = New ListViewItem(ResourceMachine.GetName())
                    Else
                        oListItem = New ListViewItem(CStr(oRow("Resource")))
                    End If
                    oListItem.SubItems.Add(CStr(oRow("Process")))
                    oListItem.SubItems.Add(CStr(oRow("StartDateTime")))
                    Dim oEndDateTime As Object = oRow("EndDateTime")
                    If TypeOf oEndDateTime Is DBNull Then
                        oListItem.SubItems.Add("")
                    Else
                        oListItem.SubItems.Add(CStr(oEndDateTime))
                    End If

                    oListItem.Tag = CInt(oRow("SessionNumber"))
                    oListItem.SubItems(0).Tag = CType(oRow("SessionID"), Guid)

                    oListItem.UseItemStyleForSubItems = True

                    oListSubItem = New ListViewItem.ListViewSubItem
                    oListSubItem.Text =
                     CType(oRow("statusid"), SessionStatus).ToString()

                    Select Case oListSubItem.Text
                        Case My.Resources.frmProcessMI_Terminated
                            oListItem.ForeColor = Color.Black
                        Case My.Resources.frmProcessMI_Stopped
                            oListItem.ForeColor = Color.Red
                        Case My.Resources.frmProcessMI_Completed
                            oListItem.ForeColor = Color.Blue
                    End Select
                    oListItem.SubItems.Add(oListSubItem)
                    moListView.Items.Add(oListItem)

                Next

                DisplayInfo(lblFound, LTools.Format(My.Resources.frmProcessMI_plural_SessionsFound, "COUNT", moListView.Items.Count))

                EnableAnalysis(moListView.Items.Count > 0)

                mbGotSessions = True

            Catch ex As Exception
                DisplayError(lblFound, ex.Message)
                EnableAnalysis(False)
            End Try

        End If

        moProgressBar.Value = moProgressBar.Minimum

        Cursor = Cursors.Default

    End Sub





    Private Sub DisplayInfo(ByRef oLabel As Label, ByVal sInfo As String)
        oLabel.Text = sInfo
        oLabel.ForeColor = System.Drawing.SystemColors.ControlText
    End Sub





    Private Sub DisplayError(ByRef oLabel As Label, ByVal sError As String)
        oLabel.Text = sError
        oLabel.ForeColor = Color.Red
    End Sub





    ''' <summary>
    ''' Enables the Analyse button and related controls.
    ''' </summary>
    ''' <param name="bEnable"></param>
    Private Sub EnableAnalysis(ByVal bEnable As Boolean)

        If bEnable Then
            btnAnalyse.Enabled = True
            DisplayInfo(lblMessage, My.Resources.frmProcessMI_Ready)
        Else
            btnAnalyse.Enabled = False
            DisplayInfo(lblMessage, My.Resources.frmProcessMI_NotReady)
        End If

        btnCancel.Enabled = False

    End Sub







    ''' <summary>
    ''' Replaces the existing show method in order to perform license checks.
    ''' Shows the form.
    ''' </summary>
    Public Shadows Sub Show()
        If Not Licensing.License.IsLicensed Then
            clsUserInterfaceUtils.ShowOperationDisallowedMessage()
            Close()
        Else
            MyBase.Show()
        End If
    End Sub


    ''' <summary>
    ''' Sets up the form controls and does a search using default parameters.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Me_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        If DesignMode Then Exit Sub


        'Clear the message labels
        lblFound.Text = ""
        lblSelected.Text = ""

        'Apply default start and end dates
        ctlFromDate.Value = New clsProcessValue(DataType.datetime, Today.AddMonths(-1), True)
        ctlToDate.Value = New clsProcessValue(DataType.datetime, Date.Today.AddDays(1), True)

        'Apply sorting to the session list
        Dim oSorter As New clsListViewSorter(moListView)
        oSorter.ColumnDataTypes = New Type() {GetType(String), GetType(String), GetType(Date), GetType(Date), GetType(String)}
        oSorter.SortColumn = 2
        oSorter.Order = System.Windows.Forms.SortOrder.Descending
        moListView.ListViewItemSorter = oSorter

        'Populate the resource list
        gMainForm.ConnectionManager.GetLatestDBResourceInfo(ResourceAttribute.Retired)
        moResourceView.ParentAppForm = gMainForm
        moResourceView.RefreshView()

        Dim iStageTypes As StageTypes
        If mProcessID.Equals(Guid.Empty) Then
            iStageTypes = StageTypes.Action _
              Or StageTypes.Skill _
              Or StageTypes.Calculation _
              Or StageTypes.Decision _
              Or StageTypes.Process _
              Or StageTypes.SubSheet _
              Or StageTypes.ChoiceStart _
              Or StageTypes.Start _
              Or StageTypes.End _
              Or StageTypes.Alert _
              Or StageTypes.Code _
              Or StageTypes.Read _
              Or StageTypes.Write _
              Or StageTypes.Navigate _
              Or StageTypes.WaitStart
            iStageTypes = iStageTypes Or StageTypes.MultipleCalculation
            iStageTypes = iStageTypes _
            Or StageTypes.Exception _
            Or StageTypes.Resume _
            Or StageTypes.Recover
        Else
            iStageTypes = StageTypes.Action _
              Or StageTypes.Skill _
              Or StageTypes.Calculation _
              Or StageTypes.Decision _
              Or StageTypes.Process _
              Or StageTypes.SubSheet _
              Or StageTypes.ChoiceStart _
              Or StageTypes.Start _
              Or StageTypes.End _
              Or StageTypes.Alert
            iStageTypes = iStageTypes Or StageTypes.MultipleCalculation
            iStageTypes = iStageTypes _
            Or StageTypes.Exception _
            Or StageTypes.Resume _
            Or StageTypes.Recover
        End If


        'Populate the stage treeview
        moTreeView.SetProcess(mProcessViewer.Process)
        moTreeView.Populate(iStageTypes)
        moTreeView.Sort()
        moTreeView.Expand(mViewSubSheet, False)
        moTreeView.EnsureVisible(mViewSubSheet)

        'Load filter template
        LoadDefaultTemplate()

        'Modify unfriendly node text.

        For Each oTreeNode As TreeNode In moTreeView.Nodes
            RenameNode(oTreeNode, aOldNodeText, aNewNodeText)
            oTreeNode.Text = LTools.GetC(oTreeNode.Text, "misc", "page") ' top level of tree only
        Next

        EnableAnalysis(False)

        'The cancel button is enabled when analysis is in progress
        btnCancel.Enabled = False

        'The clear button is enabled when analysis is complete
        btnClear.Enabled = False
        btnExport.Enabled = False

        SetToolTips()

        moProgressBar.Maximum = 100

        'Set event handlers
        AddHandler mProcessForm.FormClosing, AddressOf Parent_FormClosing
        AddHandler mProcessViewer.pbview.Paint, AddressOf PictureBox_Paint
        AddHandler ctlFromDate.Changed, AddressOf ctlFromDate_Changed
        AddHandler ctlFromDate.txtDate.TextChanged, AddressOf ctlFromDate_Changed
        AddHandler ctlToDate.txtDate.TextChanged, AddressOf ctlToDate_Changed
        AddHandler moListView.ItemCheck, AddressOf ListView_Checked
        AddHandler moTreeView.AfterCheck, AddressOf TreeView_Checked
        AddHandler moResourceView.ItemSelectionChanged, AddressOf moResourceView_ListViewItemSelectionChanged

    End Sub


    Private Sub RenameNode(ByVal oTreeNode As TreeNode, ByVal aOldText As String(), ByVal aNewText As String())

        If aOldText.Length <> aNewText.Length Then
            Throw New InvalidArgumentException(My.Resources.frmProcessMI_UnableToPopulateProvessMIStageTypeTreeView)
        End If

        If Array.IndexOf(aOldText, oTreeNode.Text) > -1 Then
            oTreeNode.Text = aNewText(Array.IndexOf(aOldText, oTreeNode.Text))
        End If

        For Each oChild As TreeNode In oTreeNode.Nodes
            RenameNode(oChild, aOldText, aNewText)
        Next

    End Sub


    ''' <summary>
    ''' Closes this form when the parent frmProcess closes.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Parent_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs)

        'Remove the handler that hides the form instead of closing.
        RemoveHandler FormClosing, AddressOf Me_FormClosing

        If RunWorkerNeedsCancelling() Then
            'Set the flag to close when the cancellation is complete.
            mbCloseFormAfterCancellation = True
            DoCancel()
        Else
            'Close now.
            Close()
        End If

    End Sub


    ''' <summary>
    ''' Cancels any work in progress and hides the form rather than
    ''' closing it. The form will close when the parent frmProcess closes.
    ''' Cancels any work in progress and hides the form rather than
    ''' closing it. The form will close when the parent frmProcess closes.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Me_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing

        'Cancel the closure.
        e.Cancel = True

        If RunWorkerNeedsCancelling() Then

            If UserMessage.YesNo(My.Resources.frmProcessMI_AreYouSureYouWantToAbortTheAnalysis) = MsgBoxResult.Yes Then

                'Check again if the worker has completed while the user message 
                'has been showing.
                If RunWorkerNeedsCancelling() Then
                    'Set the flag to hide the form when the cancellation is complete.
                    mbHideFormAfterCancellation = True
                    DoCancel()
                Else
                    'Hide rather than close.
                    Hide()
                End If

            Else
                'Do nothing, the user has decided to keep the form open.
            End If

        Else
            'Hide rather than close.
            Hide()
        End If

    End Sub


    ''' <summary>
    ''' Applies tool tips to the form.
    ''' </summary>
    Private Sub SetToolTips()

        mToolTip = New ToolTip
        mToolTip.AutoPopDelay = 5000
        mToolTip.InitialDelay = 1000
        mToolTip.ReshowDelay = 500
        mToolTip.ShowAlways = True

        mToolTip.SetToolTip(ctlFromDate.txtDate, My.Resources.frmProcessMI_EnterAStartDateToSearchFrom)
        mToolTip.SetToolTip(ctlFromDate.btnDate, My.Resources.frmProcessMI_SelectAStartDateToSearchFrom)
        mToolTip.SetToolTip(ctlToDate.txtDate, My.Resources.frmProcessMI_EnterAnEndDateToSearchUpTo)
        mToolTip.SetToolTip(ctlToDate.btnDate, My.Resources.frmProcessMI_SelectAnEndDateToSearchUpTo)
        mToolTip.SetToolTip(moResourceView, My.Resources.frmProcessMI_SelectTheResourcesToSearchForSessions)
        mToolTip.SetToolTip(btnSearch, My.Resources.frmProcessMI_ClickToSearchForSessions)
        mToolTip.SetToolTip(moListView, My.Resources.frmProcessMI_TickTheSessionsToIncludeInTheAnalysis)
        mToolTip.SetToolTip(moTreeView, My.Resources.frmProcessMI_TickTheStagesToIncludeInTheAnalysis)
        mToolTip.SetToolTip(btnAnalyse, My.Resources.frmProcessMI_ClickToAnalyseTheSelectedSessionsAnsStages)
        mToolTip.SetToolTip(btnCancel, My.Resources.frmProcessMI_ClickToStopTheAnalysis)
        mToolTip.SetToolTip(btnClear, My.Resources.frmProcessMI_ClickToClearAllAnalysisInformation)
        mToolTip.SetToolTip(btnExport, My.Resources.frmProcessMI_ClickToSaveAnalysisInformation)

    End Sub


    ''' <summary>
    ''' Enables or disables the form controls.
    ''' </summary>
    ''' <param name="Enabled">The flag to enable or disable</param>
    Private Sub EnableControls(ByVal Enabled As Boolean)

        moResourceView.Enabled = Not chkDebugOnly.Checked
        ctlFromDate.Enabled = Enabled
        ctlToDate.Enabled = Enabled
        btnSearch.Enabled = Enabled
        moListView.Enabled = Enabled
        moTreeView.Enabled = Enabled

    End Sub


    ''' <summary>
    ''' Paint handler for the ctlProcessViewer picture box.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub PictureBox_Paint(ByVal sender As System.Object, ByVal e As System.Windows.Forms.PaintEventArgs)

        If mViewSubSheet.Equals(mProcessViewer.Process.GetActiveSubSheet) Then
            'Subsheet has not changed.
        Else
            'Subsheet has changed.
            mViewSubSheet = mProcessViewer.Process.GetActiveSubSheet()
        End If

        mProcessViewer.Renderer.UpdateProcessMI(e.Graphics, mProcessViewer.pbview.Width, mProcessViewer.pbview.Height, mProcessMI)

    End Sub


    ''' <summary>
    ''' Searches for sessions in the DB.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnSearch_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
       Handles btnSearch.Click

        If Not mbGotSessions Then
            GetSessions()
        End If

    End Sub


    ''' <summary>
    ''' Reads log data and displays the resulting MI on the process diagram.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnAnalyse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAnalyse.Click

        Dim bNothingToDraw As Boolean

        If moListView.Items.Count = 0 Then
            DisplayError(lblFound, My.Resources.frmProcessMI_NoSessionsAvailable)
            bNothingToDraw = True
        Else
            If moListView.CheckedItems.Count = 0 Then
                DisplayError(lblSelected, My.Resources.frmProcessMI_NoSessionsSelected)
                bNothingToDraw = True
            Else
                If Not mProcessMI.StagesExist Then
                    DisplayError(lblMessage, My.Resources.frmProcessMI_NoStagesAvailable)
                    bNothingToDraw = True
                Else
                    If Not moTreeView.SomeStagesAreChecked Then
                        DisplayError(lblMessage, My.Resources.frmProcessMI_NoStagesSelected)
                        bNothingToDraw = True
                    Else
                        miStageType = moTreeView.GetStageTypesOfCheckedStages
                        If moTreeView.AllStagesAreChecked Then
                            maCheckedStageIDs = Nothing
                        Else
                            maCheckedStageIDs = moTreeView.GetCheckedStageIDs
                        End If
                    End If

                End If
            End If
        End If

        If bNothingToDraw Then

            btnCancel.Enabled = False
            EnableControls(True)

        Else

            mProcessMI.ClearData()
            mProcessMI.ClearSearchParameters()
            mProcessMI.ClearSessions()

            mProcessMI.SetSearchParameters(mResourceNames, CDate(ctlFromDate.Value), CDate(ctlToDate.Value))
            For Each oItem As ListViewItem In moListView.CheckedItems
                If mProcessID.Equals(Guid.Empty) Then
                    mProcessMI.AddSession(oItem.SubItems(0).Text, oItem.SubItems(2).Text, oItem.SubItems(3).Text, oItem.SubItems(4).Text)
                Else
                    mProcessMI.AddSession(oItem.SubItems(0).Text, oItem.SubItems(1).Text, oItem.SubItems(2).Text, oItem.SubItems(3).Text)
                End If
            Next

            If Not mbGotSessions Then
                GetSessions()
            End If

            mProcessViewer.InvalidateView()

            moProgressBar.Value = 10
            DisplayInfo(lblMessage, My.Resources.frmProcessMI_Working)

            btnCancel.Enabled = True
            EnableControls(False)

            'Collect the session numbers from the checked list items into an array
            Dim aSessionNumbers As Integer() = New Integer() {}
            For i As Integer = 0 To moListView.Items.Count - 1
                If moListView.Items(i).Checked Then
                    ReDim Preserve aSessionNumbers(aSessionNumbers.Length)
                    aSessionNumbers(aSessionNumbers.Length - 1) = CInt(moListView.Items(i).Tag)
                End If
            Next

            moBackgroundWorker.RunWorkerAsync(aSessionNumbers)

        End If

    End Sub


    ''' <summary>
    ''' Erases the current MI data.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnClear_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClear.Click

        mProcessMI.ClearData()
        mProcessViewer.InvalidateView()
        btnClear.Enabled = False
        btnExport.Enabled = False

    End Sub


    ''' <summary>
    ''' Cancels the background worker.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        DoCancel()
    End Sub


    Private Function RunWorkerNeedsCancelling() As Boolean

        If Not moBackgroundWorker Is Nothing _
        AndAlso moBackgroundWorker.IsBusy _
        AndAlso Not moBackgroundWorker.CancellationPending Then
            Return True
        Else
            Return False
        End If

    End Function


    ''' <summary>
    ''' Cancels the background worker.
    ''' </summary>
    Private Sub DoCancel()

        If RunWorkerNeedsCancelling() Then
            DisplayInfo(lblMessage, My.Resources.frmProcessMI_Cancelling)
            moBackgroundWorker.CancelAsync()
        End If

    End Sub





    ''' <summary>
    ''' Exports the MI
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnExport_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnExport.Click

        Dim sText As String = ""
        Dim sDate As String

        sDate = Format(Now, "yyyyMMdd HHmmss")

        SaveFileDialog1.FileName = SaveFileDialog1.InitialDirectory _
        & mProcessViewer.Process.Name & " MI " & sDate

        If SaveFileDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            Try
                If SaveFileDialog1.FileName.EndsWith("xml") Then
                    sText = mProcessMI.GetXML
                Else
                    sText = mProcessMI.GetCSV
                End If
                System.IO.File.WriteAllText(SaveFileDialog1.FileName, sText, System.Text.Encoding.UTF8)
            Catch ex As Exception
                UserMessage.Show(My.Resources.frmProcessMI_AnErrorHasOccurredAndTheProcessMIDataCouldNotBeSaved & ex.Message)
            End Try
        End If

    End Sub





    ''' <summary>
    ''' Closes the form.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click

        Close()

    End Sub





    ''' <summary>
    ''' Displays the help page.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnHelp_Click(ByVal sender As System.Object, ByVal e As CancelEventArgs) Handles Me.HelpButtonClicked
        e.Cancel = True
        Try
            OpenHelpFile(Me, "frmProcessMI.htm")
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

    ''' <summary>
    ''' Handler for the start date control.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ctlFromDate_Changed(ByVal sender As System.Object, ByVal e As System.EventArgs)

        ParameterChanged()

    End Sub





    ''' <summary>
    ''' Handler for the end date control.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ctlToDate_Changed(ByVal sender As System.Object, ByVal e As System.EventArgs)

        ParameterChanged()

    End Sub





    ''' <summary>
    ''' Clears any session search results in preparation for another
    ''' search with the new parameters.
    ''' </summary>
    Private Sub ParameterChanged()

        moListView.Items.Clear()
        mbGotSessions = False
        EnableAnalysis(False)
        DisplayInfo(lblFound, "")
        DisplayInfo(lblSelected, "")
        mProcessMI.ClearData()

    End Sub





    ''' <summary>
    ''' Displays the number of checked sessions and disables the analyse
    ''' button if none are selected.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ListView_Checked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ItemCheckEventArgs)

        Dim iChecked As Integer

        If e.CurrentValue = CheckState.Checked Then
            iChecked = -1
        Else
            iChecked = 1
        End If

        For i As Integer = 0 To moListView.Items.Count - 1
            If moListView.Items(i).Checked Then
                iChecked += 1
            End If
        Next

        DisplayInfo(lblSelected, LTools.Format(My.Resources.frmProcessMI_plural_SessionsSelected, "COUNT", iChecked))

        EnableAnalysis(iChecked > 0)

    End Sub





    Private Sub TreeView_Checked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.TreeViewEventArgs)

        If moTreeView.SomeStagesAreChecked Then
            DisplayInfo(lblMessage, My.Resources.frmProcessMI_NotReady)
        End If

    End Sub






    Private Sub moResourceView_ListViewItemSelectionChanged(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ListViewItemSelectionChangedEventArgs)

        ParameterChanged()

    End Sub





    Private Sub btnCheck_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
     Handles btnCheckAll.Click, btnCheckNone.Click

        Dim bChecked As Boolean

        If sender Is btnCheckAll Then
            bChecked = True
        End If

        If sender Is btnCheckNone Then
            bChecked = False
        End If

        If moListView.Items.Count > 0 Then
            RemoveHandler moListView.ItemCheck, AddressOf ListView_Checked
            For i As Integer = 1 To moListView.Items.Count - 1
                moListView.Items(i).Checked = bChecked
            Next
            AddHandler moListView.ItemCheck, AddressOf ListView_Checked
            moListView.Items(0).Checked = bChecked
        End If

    End Sub





    Private Sub moListView_MouseClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) _
     Handles moListView.MouseClick

        If e.Button = System.Windows.Forms.MouseButtons.Right Then
            Dim oItem As ListViewItem = moListView.GetItemAt(e.X, e.Y)
            If oItem IsNot Nothing Then
                'Show a View Log context menu, with the session id in the menu item tag.
                Dim oContextMenu As New ContextMenu
                oContextMenu.MenuItems.Add(My.Resources.frmProcessMI_ViewLog, New System.EventHandler(AddressOf moListView_ContextMenu))
                oContextMenu.MenuItems(0).Tag = oItem.SubItems(0).Tag
                oContextMenu.Show(moListView, e.Location)
            End If

        End If

    End Sub

    Private Sub moListView_ContextMenu(ByVal sender As System.Object, ByVal e As System.EventArgs)

        'Use the session id in the menu item tag to open a log viewer.
        gMainForm.StartForm(New frmLogViewer(CType(CType(sender, MenuItem).Tag, Guid)))

    End Sub

    Private Sub chkDebugOnly_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDebugOnly.CheckedChanged
        If chkDebugOnly.Checked Then
            moResourceView.Enabled = False
            ctlFromDate.Value = New clsProcessValue(DataType.datetime, Today, True)
        Else
            moResourceView.Enabled = True
            ctlFromDate.Value = New clsProcessValue(DataType.datetime, Today.AddMonths(-1), True)
        End If
    End Sub

    ''' <summary>
    ''' Event handler for the save template button.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
        Try
            SaveTemplate()

            Dim TemplateNames As Generic.List(Of String) = Nothing, sErr As String = Nothing
            gSv.ProcessMITemplateNames(TemplateNames, mProcessID)

            Dim NewName As String = cmbTemplateSwitcher.Text
            If UserMessage.OkCancelWithComboBox(My.Resources.frmProcessMI_PleaseChooseANameForYourTemplateYouMayOverwriteAnExistingTemplateBySelectingIt, TemplateNames.ToArray, NewName, My.Resources.frmProcessMI_Save, My.Resources.frmProcessMI_Cancel) = MsgBoxResult.Ok Then
                'Do the save
                Try

                    Dim TemplateXML As String = mProcessMI.GetFilterXML
                    If TemplateNames.Contains(NewName) Then
                        gSv.ProcessMIUpdateTemplate(NewName, mProcessID, TemplateXML)
                    Else
                        gSv.ProcessMICreateTemplate(NewName, mProcessID, TemplateXML)
                    End If
                Catch mdex As MissingDataException
                    UserMessage.Show(String.Format(My.Resources.frmProcessMI_SaveFailed0, mdex.Message))
                Catch ex As Exception
                    UserMessage.Show(String.Format(My.Resources.clsServer_ErrorInteractingWithDatabase0, ex.Message))
                Finally
                    RepopulateTemplateList(NewName)
                End Try
            End If
        Catch Ex As Exception
            UserMessage.Show(String.Format(My.Resources.frmProcessMI_UnexpectedError0, Ex.Message), Ex)
        End Try
    End Sub

    ''' <summary>
    ''' Saves a template from the treeview into the XML
    ''' </summary>
    Private Sub SaveTemplate()
        For Each nPage As TreeNode In moTreeView.Nodes
            For Each nGroup As TreeNode In nPage.Nodes
                For Each nStage As TreeNode In nGroup.Nodes
                    Dim st As clsProcessStage = TryCast(nStage.Tag, clsProcessStage)
                    If st IsNot Nothing Then
                        Dim sStageID As String = st.GetStageID.ToString
                        Dim bChecked As Boolean = nStage.Checked
                        mProcessMI.SetInUse(sStageID, bChecked)
                    End If
                Next
            Next
        Next
    End Sub

    ''' <summary>
    ''' Loads the default template
    ''' </summary>
    Private Sub LoadDefaultTemplate()
        Dim sErr As String = Nothing
        Dim sName As String = Nothing
        Try
            gSv.ProcessMIGetDefaultTemplate(mProcessID, sName)

            Dim sTemplateXML As String = Nothing
            gSv.ProcessMIGetTemplate(sName, mProcessID, sTemplateXML)

            mProcessMI.SetFilterXML(sTemplateXML)

            LoadTemplate()

            RepopulateTemplateList(sName)

        Catch mdEx As MissingDataException
            UserMessage.Show(String.Format(My.Resources.frmProcessMI_UnexpectedError0, mdEx.Message), mdEx)

        Catch Ex As Exception
            UserMessage.Show(String.Format(My.Resources.clsServer_ErrorInteractingWithDatabase0, Ex.Message))
        End Try
    End Sub

    ''' <summary>
    ''' Loads a template from the xml to the treeview
    ''' </summary>
    Private Sub LoadTemplate()
        For Each nPage As TreeNode In moTreeView.Nodes
            For Each nGroup As TreeNode In nPage.Nodes
                For Each nStage As TreeNode In nGroup.Nodes
                    Dim st As clsProcessStage = TryCast(nStage.Tag, clsProcessStage)
                    If st IsNot Nothing Then
                        Dim sStageID As String = st.GetStageID.ToString
                        Dim bChecked As Boolean = mProcessMI.GetInUse(sStageID)
                        nStage.Checked = bChecked
                    End If
                Next
            Next
        Next
    End Sub

    ''' <summary>
    ''' Repopulates the template list drop down
    ''' </summary>
    ''' <param name="sName">The name of the list view item to select after populating, may be nothing</param>
    Private Sub RepopulateTemplateList(ByVal sName As String)
        Dim Filters As Generic.List(Of String) = Nothing, sErr As String = Nothing

        Try
            gSv.ProcessMITemplateNames(Filters, mProcessID)
            cmbTemplateSwitcher.Items.Clear()
            For Each filter As String In Filters
                cmbTemplateSwitcher.Items.Add(filter)
            Next
            cmbTemplateSwitcher.Text = sName
            cmbTemplateSwitcher.SelectedItem = sName
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.frmProcessMI_UnableToPopulateFilterList0, ex.Message))
        End Try

        btnDelete.Enabled = cmbTemplateSwitcher.SelectedIndex <> -1
    End Sub

    ''' <summary>
    ''' Event handler for the delete template button
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnDelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDelete.Click
        Try
            If cmbTemplateSwitcher.Text <> "" Then
                If UserMessage.YesNoCancel(String.Format(My.Resources.frmProcessMI_AreYouSureYouWishToDeleteTheStoredFilter0, cmbTemplateSwitcher.Text)) = MsgBoxResult.Yes Then
                    gSv.ProcessMIDeleteTemplate(cmbTemplateSwitcher.Text, mProcessID)
                End If
            Else
                UserMessage.Show(My.Resources.frmProcessMI_PleaseFirstSelectAFilterToDelete)
            End If
        Catch mdex As MissingDataException
            UserMessage.Show(String.Format(My.Resources.frmProcessMI_FailedToDeleteFilter0, mdEx))
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.clsServer_ErrorInteractingWithDatabase0, ex.Message))
        Finally
            RepopulateTemplateList(Nothing)
        End Try
    End Sub

    ''' <summary>
    ''' Event handler for the selected index changed event of the template picker.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub cmbTemplateSwitcher_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbTemplateSwitcher.SelectedIndexChanged
        Try
            Dim sErr As String = Nothing
            Dim sTemplateXML As String = Nothing
            gSv.ProcessMIGetTemplate(cmbTemplateSwitcher.Text, mProcessID, sTemplateXML)

            mProcessMI.SetFilterXML(sTemplateXML)

            LoadTemplate()

            btnDelete.Enabled = cmbTemplateSwitcher.SelectedIndex <> -1

        Catch mdEx As MissingDataException
            UserMessage.Show(String.Format(My.Resources.frmProcessMI_UnexpectedError0, mdEx.Message), mdEx)

        Catch Ex As Exception
            UserMessage.Show(String.Format(My.Resources.frmProcessMI_UnexpectedError0, Ex.Message), Ex)
        End Try
    End Sub

    ''' <summary>
    ''' Event handler for the set default template button.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnDefault_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDefault.Click
        Try
            gSv.ProcessMISetDefaultTemplate(mProcessID, cmbTemplateSwitcher.Text)
            UserMessage.Show(My.Resources.frmProcessMI_DefaultTemplateSuccessfullyChanged)
        Catch Ex As Exception
            UserMessage.Show(String.Format(My.Resources.frmProcessMI_DefaultTemplateNotSet0, Ex.Message))
        Finally
            RepopulateTemplateList(Nothing)
        End Try
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
