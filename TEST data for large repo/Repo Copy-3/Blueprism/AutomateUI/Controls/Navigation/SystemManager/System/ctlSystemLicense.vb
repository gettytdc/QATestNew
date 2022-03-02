Imports System.IO
Imports BluePrism.BPCoreLib
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports LocaleTools

Public Class ctlSystemLicense
    Implements IChild
    Implements IPermission

#Region " Member Variables "
    Private mProcessesText As String
    Private mSessionsText As String
    Private mResourcesText As String
    Private mAlertsText As String
    Private mSelectedLicense As KeyInfo
#End Region

    ''' <summary>
    ''' Handles the loading of this control, initialising the display text variables 
    ''' with any translated text required later.
    ''' </summary>
    Protected Overrides Sub OnLoad(e As EventArgs)
        ' Store the initial text values so the numbers can be added on after translation
        mProcessesText = lblProcesses.Text
        mSessionsText = lblSessions.Text
        mResourcesText = lblResources.Text
        mAlertsText = lblAlerts.Text

        MyBase.OnLoad(e)

    End Sub

    ''' <summary>
    ''' Populates the UI with the current license info
    ''' </summary>
    Private Sub DisplayLicenses()
        Dim firstKey = License.FirstKey
        Dim licenseType = LTools.GetC(License.LicenseType.ToString(), "misc", "license")
        If firstKey.Expired Then
            licenseType = LTools.GetC(LicenseTypes.None.ToString(), "misc", "license")
        End If
        txtLicenseType.Text = licenseType 

        If mParent IsNot Nothing Then mParent.NotifyLicenceChange()

        Dim sysMan = clsUserInterfaceUtils.GetAncestor(Of ctlSystemManager)(Me)
        If sysMan IsNot Nothing Then
            sysMan.NotifyLicenceChange()
        End If

        With Licensing.License
            lblProcesses.Text = String.Format(
                "{0} {1}", mProcessesText, .NumPublishedProcessesLabel)
            lblSessions.Text = String.Format(
                "{0} {1}", mSessionsText, .NumConcurrentSessionsLabel)
            lblResources.Text = String.Format(
                "{0} {1}", mResourcesText, .NumResourcePCsLabel)
            lblAlerts.Text = String.Format(
                "{0} {1}", mAlertsText, .NumProcessAlertsPCsLabel)
        End With

        dgvLicenses.Columns("colStartDate").ValueType = GetType(Date)
        dgvLicenses.Columns("colExpiryDate").ValueType = GetType(Date)

        dgvLicenses.Rows.Clear()
        Dim keys = gSv.GetLicenseInfo()
        For Each lic As KeyInfo In keys
            If Not chkShowExpiredLicenses.Checked AndAlso lic.Expired Then Continue For
            dgvLicenses.Rows.Add(BuildLicenseRow(lic))
        Next
        dgvLicenses.Sort(dgvLicenses.Columns("colExpiryDate"), ListSortDirection.Ascending)

    End Sub

    ''' <summary>
    ''' Return a grid row to represent the passed license KeyInfo.
    ''' </summary>
    ''' <param name="lic">The license details</param>
    ''' <returns>The DataGridViewRow to display</returns>
    Private Function BuildLicenseRow(lic As KeyInfo) As DataGridViewRow
        Dim r As New DataGridViewRow()
        r.Tag = lic
        r.CreateCells(dgvLicenses,
                lic.StatusLabel,
                lic.ActivationStatusLabel,
                lic.LicenseOwner,
                lic.StartDate,
                lic.ExpiryDate,
                lic.NumPublishedProcessesLabel,
                lic.NumConcurrentSessionsLabel,
                lic.NumResourcePCsLabel,
                lic.NumProcessAlertsPCsLabel,
                If(lic.Standalone, My.Resources.ctlSystemLicense_Yes, My.Resources.ctlSystemLicense_No),
                If(lic.Decipher, My.Resources.ctlSystemLicense_Yes, My.Resources.ctlSystemLicense_No),
                My.Resources.ctlSystemLicense_Remove)
        If lic.Expired Then
            r.DefaultCellStyle.ForeColor = Color.Red
            r.DefaultCellStyle.SelectionForeColor = Color.Red
        ElseIf lic.Effective Then
            r.DefaultCellStyle.ForeColor = Color.Green
            r.DefaultCellStyle.SelectionForeColor = Color.Green
            If lic.ExpiresSoon Then _
                r.DefaultCellStyle.Font = New Font(dgvLicenses.Font, FontStyle.Bold)
        Else
            r.DefaultCellStyle.ForeColor = Color.Blue
            r.DefaultCellStyle.SelectionForeColor = Color.Blue
        End If

        Return r
    End Function

    ''' <summary>
    ''' Add new license from file
    ''' </summary>
    Private Sub AddLicense()
        Try
            Dim fo As New OpenFileDialog()
            fo.Filter = My.Resources.ctlSystemLicense_BluePrismLicenseLicLic
            fo.FilterIndex = 1
            If fo.ShowDialog() = DialogResult.OK Then
                Dim newkey As String = File.ReadAllText(fo.FileName())
                Dim keyToAdd = New KeyInfo(newkey, DateTime.UtcNow, User.CurrentId)

                Try
                    Dim keys = gSv.AddLicenseKey(keyToAdd, fo.FileName())
                    Licensing.SetLicenseKeys(keys)
                Catch
                    Throw
                End Try

                UserMessage.ShowFloating(Me, ToolTipIcon.Info, My.Resources.ctlSystemLicense_Success,
                 My.Resources.ctlSystemLicense_LicenseKeySuccessfullyUpdated, Point.Empty)
            End If
        Catch ex As Exception
            UserMessage.Err(ex, My.Resources.ctlSystemLicense_OperationFailed0, ex.Message)
        End Try
        DisplayLicenses()

    End Sub

    ''' <summary>
    ''' Remove the specified license from the database
    ''' </summary>
    ''' <param name="lic">The license to remove</param>
    Private Sub RemoveLicense(lic As KeyInfo)
        If UserMessage.YesNo(If(lic.Effective,
            My.Resources.ctlSystemLicense_ThisIsAnActiveLicenseAreYouSureYouWantToRemoveIt,
            My.Resources.ctlSystemLicense_AreYouSureYouWantToRemoveThisLicense)) <> MsgBoxResult.Yes Then Exit Sub

        Try
            Dim keys = gSv.RemoveLicenseKey(lic)
            Licensing.SetLicenseKeys(keys)

            UserMessage.ShowFloating(Me, ToolTipIcon.Info, My.Resources.ctlSystemLicense_Success,
             My.Resources.ctlSystemLicense_LicenseKeySuccessfullyRemoved, Point.Empty)
        Catch ex As Exception
            UserMessage.Err(ex, My.Resources.ctlSystemLicense_OperationFailed0, ex.Message)
        End Try
        DisplayLicenses()

    End Sub

#Region " Event Handlers "

    ''' <summary>
    ''' Handles request to add a new license
    ''' </summary>
    Private Sub HandleNewLicenseRequest(sender As Object, e As LinkLabelLinkClickedEventArgs) _
      Handles llNewLicense.LinkClicked
        AddLicense()
    End Sub

    ''' <summary>
    ''' Handles initial form load
    ''' </summary>
    Private Sub HandleFormLoad(sender As Object, e As EventArgs) _
      Handles Me.Load
        DisplayLicenses()
    End Sub

    ''' <summary>
    ''' Handles toggling of 'show expired' checkbox
    ''' </summary>
    Private Sub HandleToggleExpired(sender As Object, e As EventArgs) _
      Handles chkShowExpiredLicenses.CheckedChanged
        DisplayLicenses()
    End Sub

    Private Sub HandleCellContentClick(sender As Object, e As DataGridViewCellEventArgs) _
      Handles dgvLicenses.CellContentClick
        If e.RowIndex >= 0 Then
            If e.ColumnIndex = dgvLicenses.Columns("colActivationStatus").Index Then
                Dim licenseToActivate = TryCast(dgvLicenses.Rows(e.RowIndex).Tag, KeyInfo)
                If licenseToActivate.ActivationStatus = LicenseActivationStatus.NotActivated AndAlso Not licenseToActivate.Expired Then
                    Using activationForm As New FirstRunWizard(licenseToActivate)
                        activationForm.ShowDialog()
                        DisplayLicenses()
                    End Using
                ElseIf licenseToActivate.ActivationStatus = LicenseActivationStatus.Activated Then
                    Using licenseHistory As New LicenseActivationHistory(licenseToActivate)
                        licenseHistory.StartPosition = FormStartPosition.CenterParent
                        licenseHistory.ShowDialog()
                    End Using
                End If
            ElseIf e.ColumnIndex = dgvLicenses.Columns("colRemove").Index Then
                RemoveLicense(CType(dgvLicenses.Rows(e.RowIndex).Tag, KeyInfo))
            End If
        End If
    End Sub

    Private Sub dgvLicenses_MouseClick(sender As Object, e As MouseEventArgs) Handles dgvLicenses.MouseClick
        If e.Button = MouseButtons.Right Then

            Dim rowClicked = dgvLicenses.HitTest(e.Location.X, e.Location.Y).RowIndex
            If rowClicked > -1 Then
                Dim licenseClicked = TryCast(dgvLicenses.Rows(rowClicked).Tag, KeyInfo)
                If licenseClicked?.ActivationStatus = LicenseActivationStatus.NotActivated Then
                    mSelectedLicense = licenseClicked
                    Dim historyMenuItem = New ToolStripMenuItem
                    historyMenuItem.Text = My.Resources.LicenseActivationShowHistoryText
                    historyMenuItem.Enabled = True
                    historyMenuItem.Visible = True
                    ContextMenuStrip1.Items.Clear()
                    ContextMenuStrip1.Items.Add(historyMenuItem)
                    ContextMenuStrip1.Show(dgvLicenses, e.Location)

                End If
            End If
        End If

    End Sub

    Private Sub ContextMenuStrip1_ItemClicked(sender As Object, e As ToolStripItemClickedEventArgs) Handles ContextMenuStrip1.ItemClicked
        If e.ClickedItem.Text = My.Resources.LicenseActivationShowHistoryText Then
            Using licenseHistory As New LicenseActivationHistory(mSelectedLicense)
                licenseHistory.StartPosition = FormStartPosition.CenterParent
                licenseHistory.ShowDialog()
            End Using
        End If
    End Sub




#End Region

#Region " IChild & IPermission implementations "

    Private mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) _
      Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName("System - License")
        End Get
    End Property


    Private Sub HandleVisibleChanged(sender As Object, e As EventArgs) Handles MyBase.VisibleChanged

        Dim systemLicenseForm As ctlSystemLicense = CType(sender, ctlSystemLicense)
        If systemLicenseForm.Visible Then
            clsLicenseQueries.RefreshLicense()
        End If

    End Sub

    Private Sub dgvLicenses_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles dgvLicenses.CellFormatting
        Dim keyInfo = CType(dgvLicenses.Rows(e.RowIndex).Tag, KeyInfo)
        Select Case keyInfo.ActivationStatus
            Case LicenseActivationStatus.NotApplicable
                FormatNotApplicableExpiredCells(dgvLicenses.Rows(e.RowIndex).Cells("colActivationStatus"))
        End Select
        If keyInfo.ActivationStatus = LicenseActivationStatus.NotActivated AndAlso keyInfo.Expired Then
            FormatNotApplicableExpiredCells(dgvLicenses.Rows(e.RowIndex).Cells("colActivationStatus"))
        End If
    End Sub

    Private Shared Sub FormatNotApplicableExpiredCells(cell As DataGridViewCell)
        Dim linkCell = DirectCast(cell, DataGridViewLinkCell)
        linkCell.ReadOnly = True
        linkCell.LinkBehavior = LinkBehavior.NeverUnderline
        linkCell.TrackVisitedState = False
    End Sub

#End Region


End Class
