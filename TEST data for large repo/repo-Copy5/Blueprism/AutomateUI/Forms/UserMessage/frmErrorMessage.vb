
Imports AutomateControls
Imports BluePrism.AutomateAppCore.Utility


#Region "frmErrorMessage"

''' Project  : Automate
''' Class    : frmErrorMessage
''' 
''' <summary>
''' A form to display user messages.
''' </summary>
Friend Class frmErrorMessage
    Inherits Forms.HelpButtonForm
    Implements IEnvironmentColourManager

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
    Friend WithEvents btnOK1 As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents txtMessage As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents pnlError As System.Windows.Forms.Panel
    Friend WithEvents pnlOk As System.Windows.Forms.Panel
    Friend WithEvents btnYes1 As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents lblOK As System.Windows.Forms.Label
    Friend WithEvents pnlYesNo As System.Windows.Forms.Panel
    Friend WithEvents lblYesNo As System.Windows.Forms.Label
    Friend WithEvents btnNo1 As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents pnlYesNoCancel As System.Windows.Forms.Panel
    Friend WithEvents lblYesNoCancel As System.Windows.Forms.Label
    Friend WithEvents btnCancel2 As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnYes2 As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnNo2 As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents pnlOKCancel As System.Windows.Forms.Panel
    Friend WithEvents lblOKCancel As System.Windows.Forms.Label
    Friend WithEvents btnOK3 As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnOK2 As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnCancel3 As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents pnlbuttons As System.Windows.Forms.Panel
    Friend WithEvents pnlYesNoCheckBox As System.Windows.Forms.Panel
    Friend WithEvents lblYesNoCheckBox As System.Windows.Forms.Label
    Friend WithEvents chkbxYesNoCheckbox As System.Windows.Forms.CheckBox
    Friend WithEvents btnYesNoCheckBoxYes As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnYesNoCheckboxNo As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents lblYesNoCheckboxCaption As System.Windows.Forms.Label
    Friend WithEvents pnlyesnocheckboxbuttons As System.Windows.Forms.Panel
    Friend WithEvents pnlyesnocancelwithLinkLabel As System.Windows.Forms.Panel
    Friend WithEvents lblYesNoLinkMessage As System.Windows.Forms.Label
    Friend WithEvents Panel2 As System.Windows.Forms.Panel
    Friend WithEvents llYesNoLink As System.Windows.Forms.LinkLabel
    Friend WithEvents btnYesNoCancelLinkCancel As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnYesNoCancelLinkNo As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnYesNoCancelLinkYes As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents objBlueBar As AutomateControls.TitleBar
    Friend WithEvents Panel3 As System.Windows.Forms.Panel
    Friend WithEvents pnlBlueBarMessage As System.Windows.Forms.Panel
    Friend WithEvents txtBlueBarMessage As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents lblBlueBarMessageComment As System.Windows.Forms.Label
    Friend WithEvents btnCopyToClipboard As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
    Friend WithEvents PnlOkCancelWithComboBox As System.Windows.Forms.Panel
    Friend WithEvents cmbOKCancelWithComboBoxChoice As System.Windows.Forms.ComboBox
    Friend WithEvents btnOKCancelWithComboCancel As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents lblOKCancelWithComboBoxPrompt As System.Windows.Forms.Label
    Friend WithEvents btnOKCancelWithComboOK As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnBlueBarMessageOK As AutomateControls.Buttons.StandardStyledButton
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmErrorMessage))
        Me.btnOK1 = New AutomateControls.Buttons.StandardStyledButton()
        Me.txtMessage = New AutomateControls.Textboxes.StyledTextBox()
        Me.pnlError = New System.Windows.Forms.Panel()
        Me.btnCopyToClipboard = New AutomateControls.Buttons.StandardStyledButton()
        Me.pnlOk = New System.Windows.Forms.Panel()
        Me.lblOK = New System.Windows.Forms.Label()
        Me.btnOK2 = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnYes1 = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnNo1 = New AutomateControls.Buttons.StandardStyledButton()
        Me.pnlYesNo = New System.Windows.Forms.Panel()
        Me.lblYesNo = New System.Windows.Forms.Label()
        Me.pnlYesNoCancel = New System.Windows.Forms.Panel()
        Me.lblYesNoCancel = New System.Windows.Forms.Label()
        Me.pnlbuttons = New System.Windows.Forms.Panel()
        Me.btnCancel2 = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnNo2 = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnYes2 = New AutomateControls.Buttons.StandardStyledButton()
        Me.pnlOKCancel = New System.Windows.Forms.Panel()
        Me.btnCancel3 = New AutomateControls.Buttons.StandardStyledButton()
        Me.lblOKCancel = New System.Windows.Forms.Label()
        Me.btnOK3 = New AutomateControls.Buttons.StandardStyledButton()
        Me.pnlYesNoCheckBox = New System.Windows.Forms.Panel()
        Me.lblYesNoCheckboxCaption = New System.Windows.Forms.Label()
        Me.chkbxYesNoCheckbox = New System.Windows.Forms.CheckBox()
        Me.lblYesNoCheckBox = New System.Windows.Forms.Label()
        Me.pnlyesnocheckboxbuttons = New System.Windows.Forms.Panel()
        Me.btnYesNoCheckboxNo = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnYesNoCheckBoxYes = New AutomateControls.Buttons.StandardStyledButton()
        Me.pnlyesnocancelwithLinkLabel = New System.Windows.Forms.Panel()
        Me.llYesNoLink = New System.Windows.Forms.LinkLabel()
        Me.lblYesNoLinkMessage = New System.Windows.Forms.Label()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.btnYesNoCancelLinkCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnYesNoCancelLinkNo = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnYesNoCancelLinkYes = New AutomateControls.Buttons.StandardStyledButton()
        Me.pnlBlueBarMessage = New System.Windows.Forms.Panel()
        Me.lblBlueBarMessageComment = New System.Windows.Forms.Label()
        Me.objBlueBar = New AutomateControls.TitleBar()
        Me.Panel3 = New System.Windows.Forms.Panel()
        Me.txtBlueBarMessage = New AutomateControls.Textboxes.StyledTextBox()
        Me.btnBlueBarMessageOK = New AutomateControls.Buttons.StandardStyledButton()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.PnlOkCancelWithComboBox = New System.Windows.Forms.Panel()
        Me.cmbOKCancelWithComboBoxChoice = New System.Windows.Forms.ComboBox()
        Me.btnOKCancelWithComboCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.lblOKCancelWithComboBoxPrompt = New System.Windows.Forms.Label()
        Me.btnOKCancelWithComboOK = New AutomateControls.Buttons.StandardStyledButton()
        Me.pnlError.SuspendLayout()
        Me.pnlOk.SuspendLayout()
        Me.pnlYesNo.SuspendLayout()
        Me.pnlYesNoCancel.SuspendLayout()
        Me.pnlbuttons.SuspendLayout()
        Me.pnlOKCancel.SuspendLayout()
        Me.pnlYesNoCheckBox.SuspendLayout()
        Me.pnlyesnocheckboxbuttons.SuspendLayout()
        Me.pnlyesnocancelwithLinkLabel.SuspendLayout()
        Me.Panel2.SuspendLayout()
        Me.pnlBlueBarMessage.SuspendLayout()
        Me.Panel3.SuspendLayout()
        Me.PnlOkCancelWithComboBox.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnOK1
        '
        resources.ApplyResources(Me.btnOK1, "btnOK1")
        Me.btnOK1.Name = "btnOK1"
        '
        'txtMessage
        '
        resources.ApplyResources(Me.txtMessage, "txtMessage")
        Me.txtMessage.BackColor = System.Drawing.Color.White
        Me.txtMessage.Name = "txtMessage"
        Me.txtMessage.ReadOnly = True
        Me.txtMessage.TabStop = False
        '
        'pnlError
        '
        resources.ApplyResources(Me.pnlError, "pnlError")
        Me.pnlError.Controls.Add(Me.btnCopyToClipboard)
        Me.pnlError.Controls.Add(Me.txtMessage)
        Me.pnlError.Controls.Add(Me.btnOK1)
        Me.pnlError.Name = "pnlError"
        '
        'btnCopyToClipboard
        '
        resources.ApplyResources(Me.btnCopyToClipboard, "btnCopyToClipboard")
        Me.btnCopyToClipboard.Image = Global.AutomateUI.My.Resources.ToolImages.Copy_16x16
        Me.btnCopyToClipboard.Name = "btnCopyToClipboard"
        Me.ToolTip1.SetToolTip(Me.btnCopyToClipboard, resources.GetString("btnCopyToClipboard.ToolTip"))
        Me.btnCopyToClipboard.UseVisualStyleBackColor = True
        '
        'pnlOk
        '
        resources.ApplyResources(Me.pnlOk, "pnlOk")
        Me.pnlOk.Controls.Add(Me.lblOK)
        Me.pnlOk.Controls.Add(Me.btnOK2)
        Me.pnlOk.Name = "pnlOk"
        '
        'lblOK
        '
        resources.ApplyResources(Me.lblOK, "lblOK")
        Me.lblOK.Name = "lblOK"
        Me.lblOK.UseMnemonic = False
        '
        'btnOK2
        '
        resources.ApplyResources(Me.btnOK2, "btnOK2")
        Me.btnOK2.Name = "btnOK2"
        '
        'btnYes1
        '
        resources.ApplyResources(Me.btnYes1, "btnYes1")
        Me.btnYes1.Name = "btnYes1"
        '
        'btnNo1
        '
        resources.ApplyResources(Me.btnNo1, "btnNo1")
        Me.btnNo1.Name = "btnNo1"
        '
        'pnlYesNo
        '
        resources.ApplyResources(Me.pnlYesNo, "pnlYesNo")
        Me.pnlYesNo.Controls.Add(Me.lblYesNo)
        Me.pnlYesNo.Controls.Add(Me.btnYes1)
        Me.pnlYesNo.Controls.Add(Me.btnNo1)
        Me.pnlYesNo.Name = "pnlYesNo"
        '
        'lblYesNo
        '
        resources.ApplyResources(Me.lblYesNo, "lblYesNo")
        Me.lblYesNo.Name = "lblYesNo"
        '
        'pnlYesNoCancel
        '
        resources.ApplyResources(Me.pnlYesNoCancel, "pnlYesNoCancel")
        Me.pnlYesNoCancel.Controls.Add(Me.lblYesNoCancel)
        Me.pnlYesNoCancel.Controls.Add(Me.pnlbuttons)
        Me.pnlYesNoCancel.Name = "pnlYesNoCancel"
        '
        'lblYesNoCancel
        '
        resources.ApplyResources(Me.lblYesNoCancel, "lblYesNoCancel")
        Me.lblYesNoCancel.Name = "lblYesNoCancel"
        Me.lblYesNoCancel.UseMnemonic = False
        '
        'pnlbuttons
        '
        resources.ApplyResources(Me.pnlbuttons, "pnlbuttons")
        Me.pnlbuttons.Controls.Add(Me.btnCancel2)
        Me.pnlbuttons.Controls.Add(Me.btnNo2)
        Me.pnlbuttons.Controls.Add(Me.btnYes2)
        Me.pnlbuttons.Name = "pnlbuttons"
        '
        'btnCancel2
        '
        resources.ApplyResources(Me.btnCancel2, "btnCancel2")
        Me.btnCancel2.Name = "btnCancel2"
        '
        'btnNo2
        '
        resources.ApplyResources(Me.btnNo2, "btnNo2")
        Me.btnNo2.Name = "btnNo2"
        '
        'btnYes2
        '
        resources.ApplyResources(Me.btnYes2, "btnYes2")
        Me.btnYes2.Name = "btnYes2"
        '
        'pnlOKCancel
        '
        resources.ApplyResources(Me.pnlOKCancel, "pnlOKCancel")
        Me.pnlOKCancel.Controls.Add(Me.btnCancel3)
        Me.pnlOKCancel.Controls.Add(Me.btnOK3)
        Me.pnlOKCancel.Controls.Add(Me.lblOKCancel)
        Me.pnlOKCancel.Name = "pnlOKCancel"
        '
        'btnCancel3
        '
        resources.ApplyResources(Me.btnCancel3, "btnCancel3")
        Me.btnCancel3.Name = "btnCancel3"
        '
        'lblOKCancel
        '
        resources.ApplyResources(Me.lblOKCancel, "lblOKCancel")
        Me.lblOKCancel.Name = "lblOKCancel"
        Me.lblOKCancel.UseMnemonic = False
        '
        'btnOK3
        '
        resources.ApplyResources(Me.btnOK3, "btnOK3")
        Me.btnOK3.Name = "btnOK3"
        '
        'pnlYesNoCheckBox
        '
        resources.ApplyResources(Me.pnlYesNoCheckBox, "pnlYesNoCheckBox")
        Me.pnlYesNoCheckBox.Controls.Add(Me.lblYesNoCheckboxCaption)
        Me.pnlYesNoCheckBox.Controls.Add(Me.chkbxYesNoCheckbox)
        Me.pnlYesNoCheckBox.Controls.Add(Me.lblYesNoCheckBox)
        Me.pnlYesNoCheckBox.Controls.Add(Me.pnlyesnocheckboxbuttons)
        Me.pnlYesNoCheckBox.Name = "pnlYesNoCheckBox"
        '
        'lblYesNoCheckboxCaption
        '
        resources.ApplyResources(Me.lblYesNoCheckboxCaption, "lblYesNoCheckboxCaption")
        Me.lblYesNoCheckboxCaption.Name = "lblYesNoCheckboxCaption"
        '
        'chkbxYesNoCheckbox
        '
        resources.ApplyResources(Me.chkbxYesNoCheckbox, "chkbxYesNoCheckbox")
        Me.chkbxYesNoCheckbox.Name = "chkbxYesNoCheckbox"
        '
        'lblYesNoCheckBox
        '
        resources.ApplyResources(Me.lblYesNoCheckBox, "lblYesNoCheckBox")
        Me.lblYesNoCheckBox.Name = "lblYesNoCheckBox"
        Me.lblYesNoCheckBox.UseMnemonic = False
        '
        'pnlyesnocheckboxbuttons
        '
        Me.pnlyesnocheckboxbuttons.Controls.Add(Me.btnYesNoCheckboxNo)
        Me.pnlyesnocheckboxbuttons.Controls.Add(Me.btnYesNoCheckBoxYes)
        resources.ApplyResources(Me.pnlyesnocheckboxbuttons, "pnlyesnocheckboxbuttons")
        Me.pnlyesnocheckboxbuttons.Name = "pnlyesnocheckboxbuttons"
        '
        'btnYesNoCheckboxNo
        '
        resources.ApplyResources(Me.btnYesNoCheckboxNo, "btnYesNoCheckboxNo")
        Me.btnYesNoCheckboxNo.Name = "btnYesNoCheckboxNo"
        '
        'btnYesNoCheckBoxYes
        '
        resources.ApplyResources(Me.btnYesNoCheckBoxYes, "btnYesNoCheckBoxYes")
        Me.btnYesNoCheckBoxYes.Name = "btnYesNoCheckBoxYes"
        '
        'pnlyesnocancelwithLinkLabel
        '
        resources.ApplyResources(Me.pnlyesnocancelwithLinkLabel, "pnlyesnocancelwithLinkLabel")
        Me.pnlyesnocancelwithLinkLabel.Controls.Add(Me.llYesNoLink)
        Me.pnlyesnocancelwithLinkLabel.Controls.Add(Me.lblYesNoLinkMessage)
        Me.pnlyesnocancelwithLinkLabel.Controls.Add(Me.Panel2)
        Me.pnlyesnocancelwithLinkLabel.Name = "pnlyesnocancelwithLinkLabel"
        '
        'llYesNoLink
        '
        resources.ApplyResources(Me.llYesNoLink, "llYesNoLink")
        Me.llYesNoLink.Name = "llYesNoLink"
        Me.llYesNoLink.TabStop = True
        '
        'lblYesNoLinkMessage
        '
        resources.ApplyResources(Me.lblYesNoLinkMessage, "lblYesNoLinkMessage")
        Me.lblYesNoLinkMessage.Name = "lblYesNoLinkMessage"
        Me.lblYesNoLinkMessage.UseMnemonic = False
        '
        'Panel2
        '
        resources.ApplyResources(Me.Panel2, "Panel2")
        Me.Panel2.Controls.Add(Me.btnYesNoCancelLinkCancel)
        Me.Panel2.Controls.Add(Me.btnYesNoCancelLinkNo)
        Me.Panel2.Controls.Add(Me.btnYesNoCancelLinkYes)
        Me.Panel2.Name = "Panel2"
        '
        'btnYesNoCancelLinkCancel
        '
        resources.ApplyResources(Me.btnYesNoCancelLinkCancel, "btnYesNoCancelLinkCancel")
        Me.btnYesNoCancelLinkCancel.Name = "btnYesNoCancelLinkCancel"
        '
        'btnYesNoCancelLinkNo
        '
        resources.ApplyResources(Me.btnYesNoCancelLinkNo, "btnYesNoCancelLinkNo")
        Me.btnYesNoCancelLinkNo.Name = "btnYesNoCancelLinkNo"
        '
        'btnYesNoCancelLinkYes
        '
        resources.ApplyResources(Me.btnYesNoCancelLinkYes, "btnYesNoCancelLinkYes")
        Me.btnYesNoCancelLinkYes.Name = "btnYesNoCancelLinkYes"
        '
        'pnlBlueBarMessage
        '
        resources.ApplyResources(Me.pnlBlueBarMessage, "pnlBlueBarMessage")
        Me.pnlBlueBarMessage.Controls.Add(Me.lblBlueBarMessageComment)
        Me.pnlBlueBarMessage.Controls.Add(Me.objBlueBar)
        Me.pnlBlueBarMessage.Controls.Add(Me.Panel3)
        Me.pnlBlueBarMessage.Name = "pnlBlueBarMessage"
        '
        'lblBlueBarMessageComment
        '
        resources.ApplyResources(Me.lblBlueBarMessageComment, "lblBlueBarMessageComment")
        Me.lblBlueBarMessageComment.Name = "lblBlueBarMessageComment"
        Me.lblBlueBarMessageComment.UseMnemonic = False
        '
        'objBlueBar
        '
        resources.ApplyResources(Me.objBlueBar, "objBlueBar")
        Me.objBlueBar.Name = "objBlueBar"
        '
        'Panel3
        '
        resources.ApplyResources(Me.Panel3, "Panel3")
        Me.Panel3.Controls.Add(Me.txtBlueBarMessage)
        Me.Panel3.Controls.Add(Me.btnBlueBarMessageOK)
        Me.Panel3.Name = "Panel3"
        '
        'txtBlueBarMessage
        '
        resources.ApplyResources(Me.txtBlueBarMessage, "txtBlueBarMessage")
        Me.txtBlueBarMessage.BackColor = System.Drawing.Color.White
        Me.txtBlueBarMessage.Name = "txtBlueBarMessage"
        Me.txtBlueBarMessage.ReadOnly = True
        Me.txtBlueBarMessage.TabStop = False
        '
        'btnBlueBarMessageOK
        '
        resources.ApplyResources(Me.btnBlueBarMessageOK, "btnBlueBarMessageOK")
        Me.btnBlueBarMessageOK.Name = "btnBlueBarMessageOK"
        '
        'PnlOkCancelWithComboBox
        '
        resources.ApplyResources(Me.PnlOkCancelWithComboBox, "PnlOkCancelWithComboBox")
        Me.PnlOkCancelWithComboBox.Controls.Add(Me.cmbOKCancelWithComboBoxChoice)
        Me.PnlOkCancelWithComboBox.Controls.Add(Me.btnOKCancelWithComboCancel)
        Me.PnlOkCancelWithComboBox.Controls.Add(Me.lblOKCancelWithComboBoxPrompt)
        Me.PnlOkCancelWithComboBox.Controls.Add(Me.btnOKCancelWithComboOK)
        Me.PnlOkCancelWithComboBox.Name = "PnlOkCancelWithComboBox"
        '
        'cmbOKCancelWithComboBoxChoice
        '
        Me.cmbOKCancelWithComboBoxChoice.FormattingEnabled = True
        resources.ApplyResources(Me.cmbOKCancelWithComboBoxChoice, "cmbOKCancelWithComboBoxChoice")
        Me.cmbOKCancelWithComboBoxChoice.Name = "cmbOKCancelWithComboBoxChoice"
        '
        'btnOKCancelWithComboCancel
        '
        resources.ApplyResources(Me.btnOKCancelWithComboCancel, "btnOKCancelWithComboCancel")
        Me.btnOKCancelWithComboCancel.Name = "btnOKCancelWithComboCancel"
        '
        'lblOKCancelWithComboBoxPrompt
        '
        resources.ApplyResources(Me.lblOKCancelWithComboBoxPrompt, "lblOKCancelWithComboBoxPrompt")
        Me.lblOKCancelWithComboBoxPrompt.Name = "lblOKCancelWithComboBoxPrompt"
        Me.lblOKCancelWithComboBoxPrompt.UseMnemonic = False
        '
        'btnOKCancelWithComboOK
        '
        resources.ApplyResources(Me.btnOKCancelWithComboOK, "btnOKCancelWithComboOK")
        Me.btnOKCancelWithComboOK.Name = "btnOKCancelWithComboOK"
        '
        'frmErrorMessage
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.PnlOkCancelWithComboBox)
        Me.Controls.Add(Me.pnlBlueBarMessage)
        Me.Controls.Add(Me.pnlYesNoCheckBox)
        Me.Controls.Add(Me.pnlOKCancel)
        Me.Controls.Add(Me.pnlOk)
        Me.Controls.Add(Me.pnlYesNo)
        Me.Controls.Add(Me.pnlError)
        Me.Controls.Add(Me.pnlYesNoCancel)
        Me.Controls.Add(Me.pnlyesnocancelwithLinkLabel)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmErrorMessage"
        Me.pnlError.ResumeLayout(False)
        Me.pnlError.PerformLayout()
        Me.pnlOk.ResumeLayout(False)
        Me.pnlYesNo.ResumeLayout(False)
        Me.pnlYesNo.PerformLayout()
        Me.pnlYesNoCancel.ResumeLayout(False)
        Me.pnlbuttons.ResumeLayout(False)
        Me.pnlOKCancel.ResumeLayout(False)
        Me.pnlOKCancel.PerformLayout()
        Me.pnlYesNoCheckBox.ResumeLayout(False)
        Me.pnlyesnocheckboxbuttons.ResumeLayout(False)
        Me.pnlyesnocancelwithLinkLabel.ResumeLayout(False)
        Me.Panel2.ResumeLayout(False)
        Me.pnlBlueBarMessage.ResumeLayout(False)
        Me.Panel3.ResumeLayout(False)
        Me.Panel3.PerformLayout()
        Me.PnlOkCancelWithComboBox.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region


    ''' <summary>
    ''' The types of message to display.
    ''' </summary>
    Public Enum MessageType
        OkError
        OkMessage
        BlueBarOkMessage
        OkCancelMessage
        OkCancelWithComboBoxMessage
        YesNoMessage
        YesNoCancelMessage
        YesNoCheckBoxMessage
        YesNoCancelLinkLabelMessage
    End Enum

    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="sPrompt">The message prompt</param>
    ''' <param name="type">The type of message</param>
    ''' <param name="yesnocheckboxprompt">Optional checkbox text</param>
    ''' <param name="Comment">Optional additional comment</param>
    Public Sub New(ByVal sPrompt As String, Optional ByVal type As MessageType = MessageType.OkError, Optional ByVal yesnocheckboxprompt As String = "", Optional ByVal Comment As String = "")

        MyBase.New()
        InitializeComponent()

        Select Case type
            Case MessageType.OkError
                ShowPanel(pnlError)
                txtMessage.Text = sPrompt
                Me.HelpButton = True
                Me.AcceptButton = btnOK1

            Case MessageType.OkMessage
                ShowPanel(pnlOk)
                lblOK.Text = sPrompt
                Me.AcceptButton = btnOK2

            Case MessageType.OkCancelMessage
                ShowPanel(pnlOKCancel)
                lblOKCancel.Text = sPrompt
                Me.AcceptButton = btnOK3

            Case MessageType.OkCancelWithComboBoxMessage
                ShowPanel(Me.PnlOkCancelWithComboBox)
                Me.lblOKCancelWithComboBoxPrompt.Text = sPrompt
                Me.AcceptButton = Me.btnOKCancelWithComboOK

            Case MessageType.YesNoMessage
                ShowPanel(pnlYesNo)
                lblYesNo.Text = sPrompt
                Me.AcceptButton = btnYes1

            Case MessageType.YesNoCancelMessage
                ShowPanel(pnlYesNoCancel)
                lblYesNoCancel.Text = sPrompt
                Me.AcceptButton = btnYes2

            Case MessageType.YesNoCheckBoxMessage
                ShowPanel(pnlYesNoCheckBox)
                lblYesNoCheckBox.Text = sPrompt
                lblYesNoCheckboxCaption.Text = yesnocheckboxprompt
                Me.AcceptButton = btnYesNoCheckBoxYes

            Case MessageType.YesNoCancelLinkLabelMessage
                ShowPanel(Me.pnlyesnocancelwithLinkLabel)
                Me.lblYesNoLinkMessage.Text = sPrompt
                Me.AcceptButton = Me.btnYesNoCancelLinkYes

            Case MessageType.BlueBarOkMessage
                ShowPanel(Me.pnlBlueBarMessage)
                Me.lblBlueBarMessageComment.Text = Comment
                Me.txtBlueBarMessage.Text = sPrompt
                Me.HelpButton = True
                Me.AcceptButton = Me.btnBlueBarMessageOK

        End Select

    End Sub

    ''' <summary>
    ''' Private member to store public property HelpTopic()
    ''' </summary>
    Private miHelpTopic As Integer
    ''' <summary>
    ''' The help topic to be shown if the user clicks the help button.
    ''' Set to 0 or less to hide the button, or if you have no help
    ''' topic to supply.
    ''' </summary>
    ''' <remarks>The help topic is used in preference to the
    ''' HelpPage. In the absence of a help topic, the HelpPage
    ''' reference is used instead.</remarks>
    Public Property HelpTopicNumber() As Integer
        Get
            Return miHelpTopic
        End Get
        Set(ByVal value As Integer)
            miHelpTopic = value
            Me.HelpButton = (value > 0) OrElse (Not String.IsNullOrEmpty(Me.HelpPage))
        End Set
    End Property

    ''' <summary>
    ''' Private member to store public property HelpPage()
    ''' </summary>
    Private msHelpPage As String
    ''' <summary>
    ''' A string reference to an html page. Eg "frmIntegrationAssistant.htm"
    ''' </summary>
    ''' <value></value>
    Public Property HelpPage() As String
        Get
            Return msHelpPage
        End Get
        Set(ByVal value As String)
            msHelpPage = value
            Me.HelpButton = (Not String.IsNullOrEmpty(value))
        End Set
    End Property


    Private Sub ShowPanel(ByVal panel As Panel)
        Dim child As Control
        For Each child In Me.Controls
            If child.GetType.Name = "Panel" Then
                child.Visible = False
            End If
        Next
        panel.Visible = True
        panel.BringToFront()
        panel.Location = New Point(0, 0)
        Me.Width = panel.Width + 20
        Me.Height = panel.Height + 40
    End Sub

#Region "Events"

    Protected Overrides Sub OnHelpButtonClicked(ByVal e As System.ComponentModel.CancelEventArgs)
        Try
            If HelpTopicNumber > 0 Then
                HelpLauncher.ShowTopicNumber(Me, HelpTopicNumber)
            Else
                Try
                    OpenHelpFile(Me, HelpPage)
                Catch
                    UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
                End Try
            End If
        Finally
            e.Cancel = True
        End Try
    End Sub

    Public Event ClickButton(ByVal result As MsgBoxResult)

    Private Sub btnOk2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOK1.Click, btnOK2.Click, btnOK3.Click, btnBlueBarMessageOK.Click, btnOKCancelWithComboOK.Click
        RaiseEvent ClickButton(MsgBoxResult.Ok)
        Me.Close()
    End Sub

    Private Sub btnYes1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnYes1.Click, btnYes2.Click, btnYesNoCheckBoxYes.Click, btnYesNoCancelLinkYes.Click
        RaiseEvent ClickButton(MsgBoxResult.Yes)
        Me.Close()
    End Sub

    Private Sub btnNo1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnNo1.Click, btnNo2.Click, btnYesNoCheckboxNo.Click, btnYesNoCancelLinkNo.Click
        RaiseEvent ClickButton(MsgBoxResult.No)
        Me.Close()
    End Sub

    Private Sub btnCancel2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel2.Click, btnCancel3.Click, btnYesNoCancelLinkCancel.Click, btnOKCancelWithComboCancel.Click
        RaiseEvent ClickButton(MsgBoxResult.Cancel)
        Me.Close()
    End Sub



#End Region

    Private Sub llYesNoLink_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles llYesNoLink.MouseUp
        RaiseEvent ClickButton(MsgBoxResult.Cancel)
        Me.Close()
    End Sub

    Private Sub btnCopyToClipboard_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnCopyToClipboard.Click
        Try
            Dim Message As String = Me.txtMessage.Text

            Dim DetailBox As Control = Me.Controls(UserMessage.ExceptionDetailBoxName)
            If (DetailBox IsNot Nothing) AndAlso DetailBox.Visible Then
                Message &= vbCrLf & vbCrLf & vbCrLf & vbCrLf & DetailBox.Text
            End If

            Clipboard.SetText(Message)
        Catch
            UserMessage.ShowFloating(CType(sender, Control), ToolTipIcon.Error, My.Resources.UserMessageTitleError, My.Resources.FailedToCopyTextToTheClipboard, Point.Empty, 2000)
        End Try
    End Sub

    Public Property EnvironmentBackColor As Color Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return objBlueBar.BackColor
        End Get
        Set(value As Color)
            objBlueBar.BackColor = value
        End Set
    End Property

    Public Property EnvironmentForeColor As Color Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return objBlueBar.TitleColor
        End Get
        Set(value As Color)
            objBlueBar.TitleColor = value
        End Set
    End Property
End Class

#End Region