Option Strict On

Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Reflection
Imports System.Text
Imports System.Threading
Imports BluePrism.AMI
Imports BluePrism.ApplicationManager.AMI
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.BPCoreLib
Imports BluePrism.StartUp

Public Class frmMain

    ''' <summary>
    ''' Our Target Application when we are connected to one, or Nothing when we are
    ''' not.
    ''' </summary>
    Friend WithEvents mAMI As clsAMI = Nothing

    ''' <summary>
    ''' Our Font Editor form, when it is open, or Nothing when it is not.
    ''' </summary>
    Private WithEvents mFontEditor As frmFontEditor = Nothing

    ''' <summary>
    ''' True when we are spying
    ''' </summary>
    Private mSpying As Boolean = False

    Public Sub New()

        'Check for command line arguement 'locale=xx-XX'
        For Each commandLineArg As String In My.Application.CommandLineArgs
            If (commandLineArg.StartsWith("locale=")) Then
                Dim sLocale As String = commandLineArg.Substring("locale=".Length)
                Try
                    Thread.CurrentThread.CurrentUICulture = New CultureInfo(sLocale)
                    Thread.CurrentThread.CurrentCulture = New CultureInfo(sLocale)
                Catch ex As CultureNotFoundException
                    MessageBox.Show(My.Resources.LocaleMissing)
                Catch ex2 As IndexOutOfRangeException
                    MessageBox.Show(My.Resources.NotAValidCultureName)
                End Try
            End If
        Next

        ContainerInitialiser.SetUpContainer()
        RegexTimeout.SetDefaultRegexTimeout()
        InitializeComponent()
        Application.EnableVisualStyles()
        Dim sErr As String = Nothing

        clsConfig.Init(sErr)

#If Not DEBUG Then
        DocumentGeneratorToolStripMenuItem.Visible = False
#End If
    End Sub

    Private Sub rdoLaunch_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        UpdateGreys()
    End Sub

    Private Sub rdoAttach_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        UpdateGreys()
    End Sub

    Private Sub rdoDesktop_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        UpdateGreys()
    End Sub


    Private Sub ExitToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitToolStripMenuItem.Click
        Me.Close()
    End Sub

    Private Sub AdvancedToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AdvancedToolStripMenuItem.Click
        Dim objAdvanced As New frmAdvanced
        objAdvanced.Show()
    End Sub

    Private Sub btnDiscover_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDiscover.Click
        Dim bTempTarget As Boolean = False
        Try
            mSpying = True
            SetStatusMessage(My.Resources.DiscoveryInProgress)
            UpdateGreys()

            Dim el As clsElementTypeInfo = Nothing
            Dim ids As List(Of clsIdentifierInfo) = Nothing
            Try
                If mAMI.Spy(el, ids) Then
                    SetStatusMessage(String.Format(My.Resources.DiscoverySuccessfulElementOfType0Identified, el.ID))
                    Dim txt As New StringBuilder()
                    txt.AppendLine(String.Format(My.Resources.ElementType01, el.Name, el.ID))
                    For Each id As clsIdentifierInfo In ids
                        txt.AppendLine(String.Format(My.Resources.Identifier01, id.FullyQualifiedName, id.Value))
                    Next
                    MessageBox.Show(txt.ToString(), My.Resources.DiscoveryResults)

                Else
                    SetStatusMessage(My.Resources.DiscoveryCancelled)
                End If
            Catch ex As Exception
                SetStatusMessage(My.Resources.DiscoveryFailed)
                MessageBox.Show(ex.ToString(), My.Resources.DiscoveryFailed_1)
            End Try

        Finally

            mSpying = False
            UpdateGreys()
            Activate()
        End Try
    End Sub


    Private Sub btnLaunch_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnLaunch.Click

        If cmbApplicationSubType.SelectedIndex = 0 Then Return

        mAMI = New clsAMI(New clsGlobalInfo)
        Dim err As clsAMIMessage = Nothing
        Dim appinfo As clsApplicationTypeInfo = CType(cmbApplicationSubType.SelectedItem, clsApplicationTypeInfo)
        If Not mAMI.SetTargetApplication(appinfo, err) Then
            ShowMessage(String.Format(My.Resources.FailedToSetUpApplication0, err.Message))
            mAMI = Nothing
            Return
        End If
        Dim args As New Dictionary(Of String, String)
        For i As Integer = 0 To DataGridView1.Rows.Count - 1
            Dim ap As clsApplicationParameter = CType(DataGridView1.Rows(i).Tag, clsApplicationParameter)
            If ap.ParameterType = clsApplicationParameter.ParameterTypes.List AndAlso ap.FriendlyValues IsNot Nothing Then
                Dim fv As String = CStr(DataGridView1(1, i).Value)
                args(ap.Name) = ap.Values(ap.FriendlyValues.IndexOf(fv))
            Else
                args(ap.Name) = CStr(DataGridView1(1, i).Value)
            End If
        Next
        If mAMI.LaunchApplication(err, args) Then
            SetStatusMessage(My.Resources.LaunchSuccessful)
        Else
            ShowMessage(String.Format(My.Resources.FailedToLaunch0, err.Message))
            mAMI = Nothing
        End If
        UpdateGreys()
    End Sub

    Private Sub ShowMessage(ByVal message As String)
        MsgBox(message, , Me.Text)
    End Sub


    ''' <summary>
    ''' Open the font editor (or activate it if already open)
    ''' </summary>
    Private Sub FontEditorToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles FontEditorToolStripMenuItem.Click
        If mFontEditor IsNot Nothing Then
            mFontEditor.Activate()
            Return
        End If
        mFontEditor = New frmFontEditor()
        mFontEditor.mParent = Me
        mFontEditor.Show(Me)
    End Sub

    ''' <summary>
    ''' Set our status message.
    ''' </summary>
    ''' <param name="txt">The new text to set</param>
    Private Sub SetStatusMessage(ByVal txt As String)
        ToolStripStatusLabel1.Text = txt
    End Sub

    ''' <summary>
    ''' Update what is greyed out and what is not...
    ''' </summary>
    Private Sub UpdateGreys()

        Dim running As Boolean = Not (mAMI Is Nothing) AndAlso Not mSpying

        btnDiscover.Enabled = running
        btnDisconnect.Enabled = running
        btnInspect.Enabled = running
        btnSnapshot.Enabled = running

        If mSpying Then

            GroupBox1.Enabled = False
            DataGridView1.BackColor = SystemColors.Control
            DataGridView1.ForeColor = SystemColors.GrayText
            lblStatus.Text = String.Format(My.Resources.DiscoveryIsInProgress0SelectAnElementOrCancelToContinue, vbCrLf)

        ElseIf mAMI Is Nothing Then

            GroupBox1.Enabled = True
            DataGridView1.BackColor = SystemColors.Window
            DataGridView1.ForeColor = SystemColors.ControlText
            lblStatus.Text = My.Resources.NotConnectedToAnApplication

        Else

            GroupBox1.Enabled = False
            DataGridView1.BackColor = SystemColors.Control
            DataGridView1.ForeColor = SystemColors.GrayText
            Try
                Dim p As Process = Process.GetProcessById(mAMI.TargetPID)
                lblStatus.Text = String.Format(My.Resources.ConnectedToProcess0, p.ProcessName)
            Catch ex As Exception
                lblStatus.Text = String.Format(My.Resources.ProcessConnectionFailed0, ex.Message)
            End Try

        End If

        cmbApplicationSubType.Enabled = cmbApplicationType.SelectedIndex <> 0

    End Sub

    Private Sub DocumentGeneratorToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DocumentGeneratorToolStripMenuItem.Click
        Dim f As New frmDocsGen()
        f.ShowDialog()
    End Sub

    Private Sub txtCommandLine_TextUpdate(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbApplicationType.TextUpdate
        UpdateGreys()
    End Sub

    Private Sub TargetAppStatusChanged(ByVal appinfo As clsApplicationTypeInfo, ByVal status As clsAMI.ApplicationStatus) Handles mAMI.ApplicationStatusChanged
        Select Case status
            Case clsAMI.ApplicationStatus.Launched
            Case clsAMI.ApplicationStatus.NotLaunched
                mAMI.Dispose()
                mAMI = Nothing
                SetStatusMessage(My.Resources.TargetApplicationDisconnected)
                UpdateGreys()
        End Select
    End Sub

    Private Sub btnDisconnect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDisconnect.Click
        Dim err As clsAMIMessage = Nothing
        mAMI.DetachApplication(err)
    End Sub

    Private Sub frmMain_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Dim ami As New clsAMI(New clsGlobalInfo)
        'Populate the application types...
        cmbApplicationType.Items.Add(My.Resources.xSelect)
        For Each ai As clsApplicationTypeInfo In ami.GetApplicationTypes()
            cmbApplicationType.Items.Add(ai)
        Next
        cmbApplicationType.SelectedIndex = 0

        UpdateGreys()

    End Sub

    Private Sub UpdateSubTypes()
        cmbApplicationSubType.Items.Clear()
        If cmbApplicationType.SelectedIndex = 0 Then
            cmbApplicationSubType.Items.Add(My.Resources.SelectTypeFirst)
        Else
            cmbApplicationSubType.Items.Add(My.Resources.xSelect)
            For Each ai As clsApplicationTypeInfo In CType(cmbApplicationType.SelectedItem, clsApplicationTypeInfo).SubTypes
                cmbApplicationSubType.Items.Add(ai)
            Next
        End If
        cmbApplicationSubType.SelectedIndex = 0
    End Sub

    Private Sub UpdateParameters()
        DataGridView1.Rows.Clear()
        If cmbApplicationSubType.SelectedIndex <> 0 Then
            Dim rnum As Integer = 0
            For Each ap As clsApplicationParameter In CType(cmbApplicationSubType.SelectedItem, clsApplicationTypeInfo).Parameters
                DataGridView1.Rows.Add(New Object() {ap.Name, ""})
                DataGridView1.Rows(rnum).Tag = ap
                Select Case ap.ParameterType
                    Case clsApplicationParameter.ParameterTypes.Boolean
                        Dim cbc As New DataGridViewCheckBoxCell()
                        cbc.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
                        cbc.Value = IIf(ap.Value = "True", True, False)
                        DataGridView1(1, rnum) = cbc
                    Case clsApplicationParameter.ParameterTypes.File
                        Dim cbb As New DataGridViewButtonCell()
                        cbb.UseColumnTextForButtonValue = True
                        cbb.FlatStyle = FlatStyle.Standard
                        cbb.Value = "..."
                        DataGridView1(2, rnum) = cbb
                        If ap.Value IsNot Nothing Then
                            DataGridView1(1, rnum).Value = ap.Value
                        End If
                    Case clsApplicationParameter.ParameterTypes.List
                        Dim cmbc As New DataGridViewComboBoxCell()
                        If ap.FriendlyValues IsNot Nothing Then
                            cmbc.Items.AddRange(ap.FriendlyValues.ToArray())
                            cmbc.Value = ap.FriendlyValues(0)
                        Else
                            cmbc.Items.AddRange(ap.Values.ToArray())
                            cmbc.Value = ap.Values(0)
                        End If
                        DataGridView1(1, rnum) = cmbc
                    Case clsApplicationParameter.ParameterTypes.Number
                        If ap.Value IsNot Nothing Then
                            DataGridView1(1, rnum).Value = ap.Value
                        End If
                    Case clsApplicationParameter.ParameterTypes.String
                        If ap.Value IsNot Nothing Then
                            DataGridView1(1, rnum).Value = ap.Value
                        End If
                End Select
                rnum += 1
            Next
        End If
    End Sub

    Private Sub DataGridView1_CellContentClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles DataGridView1.CellContentClick

        If e.ColumnIndex = 2 Then
            Dim ap As clsApplicationParameter = CType(DataGridView1.Rows(e.RowIndex).Tag, clsApplicationParameter)
            Dim ofd As New OpenFileDialog()
            ofd.Filter = ap.FileExtensionFilter
            ofd.CheckFileExists = True
            ofd.CheckPathExists = True
            ofd.Multiselect = False
            If ofd.ShowDialog() = Windows.Forms.DialogResult.OK Then
                DataGridView1(1, e.RowIndex).Value = ofd.FileName
            End If
        End If

    End Sub

    Private Sub cmbApplicationType_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbApplicationType.SelectedIndexChanged
        UpdateSubTypes()
        UpdateGreys()
    End Sub

    Private Sub cmbApplicationSubType_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbApplicationSubType.SelectedIndexChanged
        UpdateParameters()
        UpdateGreys()
    End Sub

    Private Sub FontEditClosed(ByVal sender As Object, ByVal e As EventArgs) Handles mFontEditor.Closed
        mFontEditor = Nothing
    End Sub

    Private Sub AboutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AboutToolStripMenuItem.Click
        Dim name As String = My.Resources.BluePrismDiscoveryTool
        Dim msg As String = String.Format(My.Resources.x0Version1, name, Assembly.GetExecutingAssembly.GetName.Version.ToString(3))
        MessageBox.Show(Me, msg, name)
    End Sub

    Private Sub SettingsCaptureToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SettingsCaptureToolStripMenuItem.Click
        Dim f As New frmSettingsProfile()
        f.ShowDialog(Me)
    End Sub

    Private Sub mnuAppManConfig_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuAppManConfig.Click
        Dim f As New frmAppManConfig()
        f.ShowDialog(Me)
    End Sub

    Private Sub TerminalTestToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TerminalTestToolStripMenuItem.Click
        Dim f As New frmTerminal()
        f.ShowDialog(Me)
    End Sub

    Private Sub btnInspect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnInspect.Click
        Try
            Dim elements As ICollection(Of clsAMI.clsElement) = mAMI.GetElementTree(mAMI.GetTargetAppInfo().ID)
            Dim f As New frmInspect()
            f.mElements = elements
            f.Show()
        Catch ex As Exception
            ShowMessage(String.Format(My.Resources.FailedToBrowse0, ex.Message))
        End Try
    End Sub

    Private Sub frmMain_FormClosed(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed
        If mAMI IsNot Nothing Then
            Dim err As clsAMIMessage = Nothing
            mAMI.DetachApplication(err)
            'Will get disposed by the above, via the event!?
        End If
    End Sub

    Private Sub btnSnapshot_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSnapshot.Click
        Dim f As New frmTakeSnapshot()
        f.mAMI = mAMI
        f.ShowDialog()
    End Sub

    Private Sub LoadSnapshotToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LoadSnapshotToolStripMenuItem.Click
        Try
            Dim ofd As New OpenFileDialog()
            ofd.CheckFileExists = True
            ofd.CheckPathExists = True
            ofd.Multiselect = False
            If ofd.ShowDialog() = Windows.Forms.DialogResult.OK Then
                Dim snapshot As String = File.ReadAllText(ofd.FileName)
                Dim elements As ICollection(Of clsAMI.clsElement)
                Dim ami As New clsAMI(New clsGlobalInfo)
                elements = ami.GetElementTree(Nothing, True, Nothing, snapshot)
                Dim f As New frmInspect()
                f.mElements = elements
                f.Show()
            End If
        Catch ex As Exception
            ShowMessage(String.Format(My.Resources.FailedToLoadSnapshot0, ex.Message))
        End Try
    End Sub

    Private Sub GenerateSupportInformationToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles GenerateSupportInformationToolStripMenuItem.Click
        Dim f As New frmGenerateSupportInfo()
        f.ShowDialog()
    End Sub
End Class
