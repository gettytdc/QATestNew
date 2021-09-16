Imports AutomateControls
Imports BluePrism.AMI
Imports BluePrism.ApplicationManager.AMI
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.AutomateProcessCore

Friend Class frmDiagnostics
    Implements IHelp


    Private mAMI As clsAMI
    Private mAppDef As clsApplicationDefinition
    Private Const MarginTop As Integer = 20
    Private Const MarginLeft As Integer = 45
    Private Const MarginBottom As Integer = 20
    Private Const Spacing As Integer = 125

    Public Sub New()
        Me.New(Nothing, Nothing)
    End Sub

    Public Sub New(ByVal ami As clsAMI, ByVal appDef As clsApplicationDefinition)
        InitializeComponent()
        MinimizeBox = False
        MaximizeBox = False
        mAMI = ami
        mAppDef = appDef


        Dim allowedDiagnosticActions As List(Of clsActionTypeInfo) = clsAMI.GetAllowedDiagnosticActions(mAppDef.ApplicationInfo)
        Dim iOffset As Integer = 0

        If allowedDiagnosticActions.Count = 0 Then
            Dim lblTitle As New Label
            lblTitle.AutoSize = True
            lblTitle.Text = My.Resources.None
            lblTitle.Left = MarginLeft
            lblTitle.Top = MarginTop
            lblTitle.Font = New System.Drawing.Font("Segoe UI", 12.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            lblTitle.ForeColor = System.Drawing.Color.SteelBlue
            Me.Controls.Add(lblTitle)

            Dim lblSummary As New Label
            lblSummary.AutoSize = True
            lblSummary.Text = My.Resources.ThereAreNoDiagnosticActionsAvailableForThisApplicationType
            lblSummary.Left = MarginTop
            lblSummary.Top = lblTitle.Bottom + 5
            Me.Controls.Add(lblSummary)
            iOffset = 200 'So that the form is sized correctly
        End If

        For Each action As clsActionTypeInfo In allowedDiagnosticActions
            Dim lblTitle As New Label
            lblTitle.AutoSize = True
            lblTitle.Text = action.Name
            lblTitle.Left = MarginLeft
            lblTitle.Top = iOffset + MarginTop
            lblTitle.Font = New System.Drawing.Font("Segoe UI", 12.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            lblTitle.ForeColor = System.Drawing.Color.SteelBlue

            Dim chkSummary As New CheckBox
            chkSummary.AutoSize = True
            chkSummary.Text = action.HelpText
            chkSummary.Left = MarginLeft
            chkSummary.Top = lblTitle.Bottom + 5
            chkSummary.Tag = action

            For Each a As clsActionTypeInfo In mAppDef.DiagnosticActions
                If a.ID = action.ID Then
                    chkSummary.Checked = True
                    Exit For
                End If
            Next

            Dim btnTakeNow As New AutomateControls.Buttons.StandardStyledButton
            btnTakeNow.Text = My.Resources.frmDiagnostics_TakeNow
            btnTakeNow.Left = MarginLeft
            btnTakeNow.Size = btnCancel.Size
            btnTakeNow.Top = chkSummary.Bottom + 5
            btnTakeNow.Tag = action
            AddHandler btnTakeNow.Click, AddressOf TakeNow

            iOffset += Spacing
            Me.Controls.AddRange(New Control() {lblTitle, chkSummary, btnTakeNow})
            btnTakeNow.BackColor = Button.DefaultBackColor
        Next

        Me.ClientSize = New Size(435, iOffset + MarginTop + MarginBottom)

        btnHelp.Top = iOffset + MarginTop - btnHelp.Height
        btnCancel.Top = iOffset + MarginTop - btnCancel.Height
        btnOK.Top = iOffset + MarginTop - btnOK.Height

        btnCancel.BackColor = Button.DefaultBackColor
        btnOK.BackColor = Button.DefaultBackColor

    End Sub

    ''' <summary>
    ''' Takes a diagnostic action immediately.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub TakeNow(ByVal sender As Object, ByVal e As EventArgs)
        Me.Cursor = Cursors.WaitCursor
        Dim objControl As Control = TryCast(sender, Control)
        If Not objControl Is Nothing Then
            Dim objAction As clsActionTypeInfo = TryCast(objControl.Tag, clsActionTypeInfo)
            If Not objAction Is Nothing Then
                Dim sResult As String = Nothing
                Dim objErr As clsAMIMessage = Nothing
                If Not mAMI.DoDiagnosticAction(objAction.ID, sResult, objErr) Then
                    UserMessage.Show(objErr.Message)
                Else
                    UserMessage.Show(sResult)
                End If
            End If
        End If
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub btnOK_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnOK.Click
        mAppDef.DiagnosticActions.Clear()

        For Each c As Control In Me.Controls
            Dim ch As CheckBox = TryCast(c, CheckBox)
            If Not ch Is Nothing AndAlso ch.Checked Then
                Dim a As clsActionTypeInfo = TryCast(ch.Tag, clsActionTypeInfo)
                If Not a Is Nothing Then
                    mAppDef.DiagnosticActions.Add(a)
                End If
            End If
        Next

        Me.Close()
    End Sub

    Private Sub btnHelp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnHelp.Click
        Try
            OpenHelpFile(Me, GetHelpFile)
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "helpApplicationModellerSnapshot.htm"
    End Function
End Class
