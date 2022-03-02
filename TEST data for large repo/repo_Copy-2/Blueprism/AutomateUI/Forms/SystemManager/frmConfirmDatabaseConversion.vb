Imports AutomateControls
Imports AutomateControls.Forms

Public Class frmConfirmDatabaseConversion : Inherits AutomateForm : Implements IChild, IEnvironmentColourManager

    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmConfirmDatabaseConversion))
        Me.titleBar = New AutomateControls.TitleBar()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.btnOK = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.lblRandomNumber = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.ConfirmationNumberTextbox = New AutomateUI.clsNumericOnlyTextBox()
        Me.SuspendLayout()
        '
        'titleBar
        '
        resources.ApplyResources(Me.titleBar, "titleBar")
        Me.titleBar.Name = "titleBar"
        Me.titleBar.SubtitleFont = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold)
        Me.titleBar.TitleFont = New System.Drawing.Font("Segoe UI", 12.0!)
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'btnOK
        '
        resources.ApplyResources(Me.btnOK, "btnOK")
        Me.btnOK.Name = "btnOK"
        Me.btnOK.UseVisualStyleBackColor = False
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = False
        '
        'lblRandomNumber
        '
        resources.ApplyResources(Me.lblRandomNumber, "lblRandomNumber")
        Me.lblRandomNumber.Name = "lblRandomNumber"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Name = "Label3"
        '
        'ConfirmationNumberTextbox
        '
        resources.ApplyResources(Me.ConfirmationNumberTextbox, "ConfirmationNumberTextbox")
        Me.ConfirmationNumberTextbox.Name = "ConfirmationNumberTextbox"
        '
        'frmConfirmDatabaseConversion
        '
        resources.ApplyResources(Me, "$this")
        Me.ControlBox = False
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.lblRandomNumber)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.ConfirmationNumberTextbox)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.titleBar)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmConfirmDatabaseConversion"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents titleBar As AutomateControls.TitleBar
    Friend WithEvents Label1 As Label
    Friend WithEvents ConfirmationNumberTextbox As clsNumericOnlyTextBox
    Friend WithEvents btnOK As AutomateControls.Buttons.StandardStyledButton
    Private components As IContainer
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents lblRandomNumber As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label3 As Label

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()
        ConfirmationCode = GetRandomNumber()
    End Sub


    Protected mParent As frmApplication
    Friend Overridable Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    Public Property EnvironmentBackColor As Color Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return titleBar.BackColor
        End Get
        Set(value As Color)
            titleBar.BackColor = value
        End Set
    End Property

    Public Property EnvironmentForeColor As Color Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return titleBar.TitleColor
        End Get
        Set(value As Color)
            titleBar.TitleColor = value
        End Set
    End Property

    Private Property mConfirmationCode As Integer
    Public Property ConfirmationCode As Integer
        Get
            Return mConfirmationCode
        End Get
        Set(value As Integer)
            mConfirmationCode = value
            lblRandomNumber.Text = mConfirmationCode.ToString
        End Set
    End Property

    Public Function GetParent() As frmApplication
        Return mParent
    End Function

    Private Function GetRandomNumber() As Integer
        Dim rnd = New Random()
        Return rnd.Next(1000, 9999)
    End Function

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        If ConfirmationNumberTextbox.Text = mConfirmationCode.ToString Then
            Me.DialogResult = DialogResult.OK
        Else
            UserMessage.OK(My.Resources.frmConfirmDatabaseConversion_PleaseEnterTheCorrectConfirmationNumber)
        End If
    End Sub
End Class
