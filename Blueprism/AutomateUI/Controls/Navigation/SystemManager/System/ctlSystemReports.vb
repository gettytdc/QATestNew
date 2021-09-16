Imports System.IO
Imports AutomateControls
Imports AutomateControls.Forms
Imports AutomateUI.My.Resources
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib

Public Class ctlSystemReports : Implements IStubbornChild, IPermission, IHelp

    Private _refreshAt As New Date()
    Private _daily, _monthly As Integer
    Private _miEnabled, _miAuto As Boolean

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        btnUnlockMI.Text = GroupContentsDataGridView_Unlock
        dtpRefreshAt.CustomFormat = Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.LongTimePattern

        ' Add any initialization after the InitializeComponent() call.
        PopulateMIConfig()
        PopulateReports()
    End Sub

    Private Sub PopulateMIConfig()

        Dim lastRefreshed As New Date()

        rbManual.Checked = True
        gSv.GetMIConfig(_miEnabled, _miAuto, _refreshAt, lastRefreshed, _daily, _monthly)

        cbEnabled.Checked = _miEnabled
        rbAuto.Checked = _miAuto
        numDaysToKeep.Value = _daily
        numMonthsToKeep.Value = _monthly

        If _refreshAt = Date.MinValue Then
            _refreshAt = Date.Today.AddHours(1)
        End If

        dtpRefreshAt.Value = _refreshAt

        If lastRefreshed = Date.MinValue Then
            lblLastRefreshed.Text = String.Empty
        Else
            lblLastRefreshed.Text = String.Format(ctlSystemReports_LastRefreshedOn0, lastRefreshed.Date)
        End If

        ShowHideMIControls()
        InitMIUnlock()
        btnApply.Enabled = False
    End Sub

    Private Sub InitMIUnlock()
        Dim timeLocked = gSv.MIGetTimeLocked()

        If timeLocked > TimeSpan.FromHours(1) Then
            lblLockedHintText.Enabled = True
            btnUnlockMI.Enabled = True
            lblLockedHintText.Text = String.Format(ctlSystemReports_TheMIStatisticsDataRefreshHasBeenLockedFor0Hours, timeLocked.TotalHours)
        Else
            lblLockedHintText.Enabled = False
            btnUnlockMI.Enabled = False
            lblLockedHintText.Text = ctlSystemReports_TheMIStatisticsDataRefreshIsNotCurrentlyLocked
        End If
    End Sub

    Private Sub PopulateReports()

        With dgReports.Rows
            .Clear()
            Dim reps As ICollection(Of clsReporter) = clsReporter.GetAll()
            For Each r As clsReporter In reps
                If r.GetArguments().Count = 0 Then
                    Dim rnum As Integer = .Add(r.Name, r.Description)
                    dgReports.Rows(rnum).Tag = r
                End If
            Next
        End With
        UpdateReportButtons()
    End Sub

    Private Sub UpdateReportButtons()
        btnGenerateReport.Enabled = dgReports.SelectedRows.Count > 0
    End Sub

    Private Sub dgReports_SelectionChanged(ByVal sender As Object, ByVal e As EventArgs) Handles dgReports.SelectionChanged
        UpdateReportButtons()
    End Sub

    Private ReadOnly Property EnableApplyButton As Boolean
        Get
            Return Not cbEnabled.Checked = _miEnabled OrElse Not rbAuto.Checked = _miAuto _
                OrElse Not numDaysToKeep.Value = _daily OrElse Not numMonthsToKeep.Value = _monthly _
                OrElse Not dtpRefreshAt.Value = _refreshAt
        End Get
    End Property

    Private Sub btnGenerateReport_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnGenerateReport.Click
        If dgReports.SelectedRows.Count <> 1 Then Return
        Dim report As clsReporter = CType(dgReports.SelectedRows(0).Tag, clsReporter)
        Dim xargs As List(Of clsReporter.ArgumentInfo) = report.GetArguments()
        If xargs.Count > 0 Then
            UserMessage.Show(ctlSystemReports_CanTHandleReportsWithArgumentsHereYet)
            Return
        End If
        'Empty list, since we're not handling them, but the actual entered
        'arguments would go in here!
        Dim args As New List(Of Object)

        Dim f As New SaveFileDialog()
        f.AddExtension = True
        If report.OutputFormat = clsReporter.OutputFormats.CSV Then
            f.DefaultExt = "csv"
            f.Filter = ctlSystemReports_CSVFilesCsvCsv
        Else
            f.DefaultExt = "txt"
            f.Filter = ctlSystemReports_TextFilesTxtTxt
        End If
        If f.ShowDialog(Me) = DialogResult.OK Then
            Cursor = Cursors.WaitCursor
            Try
                Using sw As New StreamWriter(New FileStream(f.FileName, FileMode.Create), Encoding.UTF8)
                    Using spinner = New LoadingSpinner(New Action(Sub() report.Generate(args, sw)))
                        spinner.ShowDialog()
                    End Using
                End Using
                Dim popUp = New PopupForm(ReportGeneratedTitle,
                                          String.Format(ReportGeneratedMessage, f.FileName),
                                          btnOk)
                AddHandler popUp.OnBtnOKClick, AddressOf HandleOnBtnOKClick
                popUp.ShowDialog()
            Catch ex As Exception
                UserMessage.Show(String.Format(ctlSystemReports_FailedToGenerateTheReport0, ex.Message) & vbCrLf & vbCrLf & ctlSystemReports_Detail & vbCrLf & ex.StackTrace)
            Finally
                Cursor = Cursors.Default
            End Try
        End If
    End Sub

    Private Sub ShowHideMIControls()
        Dim show As Boolean = cbEnabled.Checked
        rbAuto.Enabled = show
        rbManual.Enabled = show

        If show AndAlso Not License.CanUse(LicenseUse.BPServer) Then
            rbAuto.Enabled = False
            rbManual.Enabled = False
        End If
        dtpRefreshAt.Enabled = show
        If rbManual.Checked Then
            dtpRefreshAt.Enabled = False
        End If

        numDaysToKeep.Enabled = show
        numMonthsToKeep.Enabled = show

        For Each c As Control In gpReporting.Controls
            If c.GetType() = GetType(Label) Then c.Enabled = show
        Next

        For Each control As Control In DailyStatisticsFlowLayoutPanel.Controls
            If control.GetType() = GetType(Label) Then control.Enabled = show
        Next
    End Sub

    Private Sub cbEnabled_CheckedChanged(sender As Object, e As EventArgs) _
     Handles cbEnabled.CheckedChanged, rbManual.CheckedChanged, rbAuto.CheckedChanged
        ShowHideMIControls()
        btnApply.Enabled = EnableApplyButton()
    End Sub

    Private Sub btnApply_Click(sender As Object, e As EventArgs) Handles btnApply.Click

        _miEnabled = cbEnabled.Checked
        _miAuto = rbAuto.Checked
        _refreshAt = dtpRefreshAt.Value
        _daily = CInt(numDaysToKeep.Value)
        _monthly = CInt(numMonthsToKeep.Value)

        gSv.SetMIConfig(_miEnabled, _miAuto, _refreshAt, _daily, _monthly)

        btnApply.Enabled = False
    End Sub

    Private Sub HandleOnBtnOKClick(sender As Object, e As EventArgs)
        Dim popUp = CType(sender, PopupForm)
        RemoveHandler popUp.OnBtnOKClick, AddressOf HandleOnBtnOKClick
        popUp.Close()
    End Sub

    Private mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    Public ReadOnly Property RequiredPermissions As ICollection(Of Permission) Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName("System - Reporting")
        End Get
    End Property

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "helpSystemManagerReports.htm"
    End Function

    Private Sub btnUnlockMI_Click(sender As Object, e As EventArgs) Handles btnUnlockMI.Click
        gSv.ResetMIRefreshLock()
        InitMIUnlock()
    End Sub

    Public Function CanLeave() As Boolean Implements IStubbornChild.CanLeave
        Dim leaving = True
        If User.LoggedIn AndAlso btnApply.Enabled Then
            Dim form As New YesNoCancelPopupForm(PopupForm_UnsavedChanges,
                                                 ctlControlRoom_YouHaveNotYetSavedYourChangesInTheSchedulerWouldYouLikeToSaveYourChangesBeforeL,
                                                 String.Empty)
            btnApply.Enabled = False
            Select Case form.ShowDialog()

                Case DialogResult.Cancel
                    leaving = False
                    btnApply.Enabled = True

                Case DialogResult.Yes
                    gSv.SetMIConfig(cbEnabled.Checked, rbAuto.Checked, dtpRefreshAt.Value,
                        CInt(numDaysToKeep.Value), CInt(numMonthsToKeep.Value))

            End Select
        End If

        Return leaving
    End Function

    Private Sub dtpRefreshAt_ValueChanged(sender As Object, e As EventArgs) Handles _
            dtpRefreshAt.ValueChanged, numDaysToKeep.ValueChanged, numMonthsToKeep.ValueChanged
        btnApply.Enabled = EnableApplyButton()
    End Sub

    Private Sub numDaysOrMonthsToKeep_KeyPress(sender As Object, e As KeyPressEventArgs) Handles numDaysToKeep.KeyPress, numMonthsToKeep.KeyPress
        If e.KeyChar = "."c OrElse e.KeyChar = ","c Then
            e.Handled = True
        End If

    End Sub

    Private Sub numDaysToKeep_KeyUp(sender As Object, e As KeyEventArgs) Handles numDaysToKeep.KeyUp
        CheckValueExceedsMaximum(numDaysToKeep)
        btnApply.Enabled = EnableApplyButton()
    End Sub

    Private Sub numMonthsToKeep_KeyUp(sender As Object, e As KeyEventArgs) Handles numMonthsToKeep.KeyUp
        CheckValueExceedsMaximum(numMonthsToKeep)
        btnApply.Enabled = EnableApplyButton()
    End Sub

    Private Sub CheckValueExceedsMaximum(ctl As NumericUpDown)

        Dim currentTextValue As Decimal

        If Decimal.TryParse(ctl.Text, currentTextValue) Then
            If currentTextValue > ctl.Maximum Then
                ctl.Text = ctl.Maximum.ToString()
            End If
        End If

    End Sub

End Class
