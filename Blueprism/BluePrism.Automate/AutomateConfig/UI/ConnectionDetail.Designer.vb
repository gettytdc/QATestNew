<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ConnectionDetail
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim lblConnectionName As System.Windows.Forms.Label
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ConnectionDetail))
        Dim Label1 As System.Windows.Forms.Label
        Dim Label2 As System.Windows.Forms.Label
        Dim Label3 As System.Windows.Forms.Label
        Dim Label4 As System.Windows.Forms.Label
        Dim Label5 As System.Windows.Forms.Label
        Dim Label6 As System.Windows.Forms.Label
        Dim Label7 As System.Windows.Forms.Label
        Dim Label8 As System.Windows.Forms.Label
        Dim Panel2 As System.Windows.Forms.Panel
        Dim Label9 As System.Windows.Forms.Label
        Dim Label10 As System.Windows.Forms.Label
        Dim lblConnectionMode As System.Windows.Forms.Label
        Dim Label11 As System.Windows.Forms.Label
        Dim lblOrdered As System.Windows.Forms.Label
        Me.btnTest = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.panMain = New System.Windows.Forms.TableLayoutPanel()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.chkOrdered = New System.Windows.Forms.CheckBox()
        Me.CaptionedControl5 = New BluePrism.Config.CaptionedControl()
        Me.txtConnectionString = New AutomateControls.Textboxes.StyledTextBox()
        Me.CaptionControl1 = New BluePrism.Config.CaptionedControl()
        Me.txtConnectionName = New AutomateControls.Textboxes.StyledTextBox()
        Me.CaptionControl7 = New BluePrism.Config.CaptionedControl()
        Me.txtBpServer = New AutomateControls.Textboxes.StyledTextBox()
        Me.CaptionControl6 = New BluePrism.Config.CaptionedControl()
        Me.txtPassword = New AutomateControls.SecurePasswordTextBox()
        Me.CaptionControl5 = New BluePrism.Config.CaptionedControl()
        Me.txtUserId = New AutomateControls.Textboxes.StyledTextBox()
        Me.CaptionControl4 = New BluePrism.Config.CaptionedControl()
        Me.txtDbName = New AutomateControls.Textboxes.StyledTextBox()
        Me.CaptionControl3 = New BluePrism.Config.CaptionedControl()
        Me.txtDbServer = New AutomateControls.Textboxes.StyledTextBox()
        Me.CaptionControl2 = New BluePrism.Config.CaptionedControl()
        Me.cmbConnType = New System.Windows.Forms.ComboBox()
        Me.CaptionedControl2 = New BluePrism.Config.CaptionedControl()
        Me.cmbConnectionMode = New System.Windows.Forms.ComboBox()
        Me.CaptionedControl1 = New BluePrism.Config.CaptionedControl()
        Me.txtExtraParams = New AutomateControls.Textboxes.StyledTextBox()
        Me.Panel9 = New System.Windows.Forms.Panel()
        Me.numAGPort = New AutomateControls.StyledNumericUpDown()
        Me.cbMultiSubnetFailover = New System.Windows.Forms.CheckBox()
        Me.CaptionedControl3 = New BluePrism.Config.CaptionedControl()
        Me.numServerPort = New AutomateControls.StyledNumericUpDown()
        Me.CaptionedControl4 = New BluePrism.Config.CaptionedControl()
        Me.panCallback = New System.Windows.Forms.Panel()
        Me.cbDisableCallBack = New System.Windows.Forms.CheckBox()
        Me.numCallbackPort = New AutomateControls.StyledNumericUpDown()
        lblConnectionName = New System.Windows.Forms.Label()
        Label1 = New System.Windows.Forms.Label()
        Label2 = New System.Windows.Forms.Label()
        Label3 = New System.Windows.Forms.Label()
        Label4 = New System.Windows.Forms.Label()
        Label5 = New System.Windows.Forms.Label()
        Label6 = New System.Windows.Forms.Label()
        Label7 = New System.Windows.Forms.Label()
        Label8 = New System.Windows.Forms.Label()
        Panel2 = New System.Windows.Forms.Panel()
        Label9 = New System.Windows.Forms.Label()
        Label10 = New System.Windows.Forms.Label()
        lblConnectionMode = New System.Windows.Forms.Label()
        Label11 = New System.Windows.Forms.Label()
        lblOrdered = New System.Windows.Forms.Label()
        Panel2.SuspendLayout()
        Me.panMain.SuspendLayout()
        Me.Panel1.SuspendLayout()
        Me.CaptionedControl5.SuspendLayout()
        Me.CaptionControl1.SuspendLayout()
        Me.CaptionControl7.SuspendLayout()
        Me.CaptionControl6.SuspendLayout()
        Me.CaptionControl5.SuspendLayout()
        Me.CaptionControl4.SuspendLayout()
        Me.CaptionControl3.SuspendLayout()
        Me.CaptionControl2.SuspendLayout()
        Me.CaptionedControl2.SuspendLayout()
        Me.CaptionedControl1.SuspendLayout()
        Me.Panel9.SuspendLayout()
        CType(Me.numAGPort, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.CaptionedControl3.SuspendLayout()
        CType(Me.numServerPort, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.CaptionedControl4.SuspendLayout()
        Me.panCallback.SuspendLayout()
        CType(Me.numCallbackPort, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lblConnectionName
        '
        resources.ApplyResources(lblConnectionName, "lblConnectionName")
        lblConnectionName.Name = "lblConnectionName"
        '
        'Label1
        '
        resources.ApplyResources(Label1, "Label1")
        Label1.Name = "Label1"
        '
        'Label2
        '
        resources.ApplyResources(Label2, "Label2")
        Label2.Name = "Label2"
        '
        'Label3
        '
        resources.ApplyResources(Label3, "Label3")
        Label3.Name = "Label3"
        '
        'Label4
        '
        resources.ApplyResources(Label4, "Label4")
        Label4.Name = "Label4"
        '
        'Label5
        '
        resources.ApplyResources(Label5, "Label5")
        Label5.Name = "Label5"
        '
        'Label6
        '
        resources.ApplyResources(Label6, "Label6")
        Label6.Name = "Label6"
        '
        'Label7
        '
        resources.ApplyResources(Label7, "Label7")
        Label7.Name = "Label7"
        '
        'Label8
        '
        resources.ApplyResources(Label8, "Label8")
        Label8.Name = "Label8"
        '
        'Panel2
        '
        Panel2.Controls.Add(Me.btnTest)
        resources.ApplyResources(Panel2, "Panel2")
        Panel2.Name = "Panel2"
        '
        'btnTest
        '
        resources.ApplyResources(Me.btnTest, "btnTest")
        Me.btnTest.Name = "btnTest"
        Me.btnTest.UseVisualStyleBackColor = False
        '
        'Label9
        '
        resources.ApplyResources(Label9, "Label9")
        Label9.Name = "Label9"
        '
        'Label10
        '
        resources.ApplyResources(Label10, "Label10")
        Label10.Name = "Label10"
        '
        'lblConnectionMode
        '
        resources.ApplyResources(lblConnectionMode, "lblConnectionMode")
        lblConnectionMode.Name = "lblConnectionMode"
        '
        'Label11
        '
        resources.ApplyResources(Label11, "Label11")
        Label11.Name = "Label11"
        '
        'lblOrdered
        '
        resources.ApplyResources(lblOrdered, "lblOrdered")
        lblOrdered.Name = "lblOrdered"
        '
        'panMain
        '
        resources.ApplyResources(Me.panMain, "panMain")
        Me.panMain.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.panMain.Controls.Add(Me.Panel1, 1, 13)
        Me.panMain.Controls.Add(lblOrdered, 0, 13)
        Me.panMain.Controls.Add(Label11, 0, 12)
        Me.panMain.Controls.Add(Me.CaptionedControl5, 1, 12)
        Me.panMain.Controls.Add(Label8, 0, 9)
        Me.panMain.Controls.Add(Me.CaptionControl1, 1, 0)
        Me.panMain.Controls.Add(Me.CaptionControl7, 1, 6)
        Me.panMain.Controls.Add(Me.CaptionControl6, 1, 5)
        Me.panMain.Controls.Add(Label5, 0, 5)
        Me.panMain.Controls.Add(Me.CaptionControl5, 1, 4)
        Me.panMain.Controls.Add(Label4, 0, 4)
        Me.panMain.Controls.Add(Me.CaptionControl4, 1, 3)
        Me.panMain.Controls.Add(Label3, 0, 3)
        Me.panMain.Controls.Add(Me.CaptionControl3, 1, 2)
        Me.panMain.Controls.Add(Label2, 0, 2)
        Me.panMain.Controls.Add(Label1, 0, 1)
        Me.panMain.Controls.Add(lblConnectionName, 0, 0)
        Me.panMain.Controls.Add(Me.CaptionControl2, 1, 1)
        Me.panMain.Controls.Add(Label6, 0, 6)
        Me.panMain.Controls.Add(lblConnectionMode, 0, 7)
        Me.panMain.Controls.Add(Me.CaptionedControl2, 1, 7)
        Me.panMain.Controls.Add(Label7, 0, 8)
        Me.panMain.Controls.Add(Label10, 0, 11)
        Me.panMain.Controls.Add(Me.CaptionedControl1, 1, 11)
        Me.panMain.Controls.Add(Label9, 0, 10)
        Me.panMain.Controls.Add(Me.Panel9, 1, 10)
        Me.panMain.Controls.Add(Me.CaptionedControl3, 1, 8)
        Me.panMain.Controls.Add(Me.CaptionedControl4, 1, 9)
        Me.panMain.Name = "panMain"
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.chkOrdered)
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.Name = "Panel1"
        '
        'chkOrdered
        '
        resources.ApplyResources(Me.chkOrdered, "chkOrdered")
        Me.chkOrdered.Name = "chkOrdered"
        Me.chkOrdered.UseVisualStyleBackColor = True
        '
        'CaptionedControl5
        '
        Me.CaptionedControl5.Caption = "The complete SQL connection string to use"
        Me.CaptionedControl5.CaptionFont = New System.Drawing.Font("Segoe UI", 7.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        resources.ApplyResources(Me.CaptionedControl5, "CaptionedControl5")
        Me.CaptionedControl5.Controls.Add(Me.txtConnectionString, 0, 0)
        Me.CaptionedControl5.Name = "CaptionedControl5"
        '
        'txtConnectionString
        '
        Me.txtConnectionString.BorderColor = System.Drawing.Color.Empty
        resources.ApplyResources(Me.txtConnectionString, "txtConnectionString")
        Me.txtConnectionString.Name = "txtConnectionString"
        '
        'CaptionControl1
        '
        Me.CaptionControl1.Caption = "The name by which this connection will be remembered"
        Me.CaptionControl1.CaptionFont = New System.Drawing.Font("Segoe UI", 7.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        resources.ApplyResources(Me.CaptionControl1, "CaptionControl1")
        Me.CaptionControl1.Controls.Add(Me.txtConnectionName, 0, 0)
        Me.CaptionControl1.Name = "CaptionControl1"
        '
        'txtConnectionName
        '
        Me.txtConnectionName.BorderColor = System.Drawing.Color.Empty
        resources.ApplyResources(Me.txtConnectionName, "txtConnectionName")
        Me.txtConnectionName.Name = "txtConnectionName"
        '
        'CaptionControl7
        '
        Me.CaptionControl7.Caption = "The hostname of the Blue Prism Server"
        Me.CaptionControl7.CaptionFont = New System.Drawing.Font("Segoe UI", 7.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        resources.ApplyResources(Me.CaptionControl7, "CaptionControl7")
        Me.CaptionControl7.Controls.Add(Me.txtBpServer, 0, 0)
        Me.CaptionControl7.Name = "CaptionControl7"
        '
        'txtBpServer
        '
        Me.txtBpServer.BorderColor = System.Drawing.Color.Empty
        resources.ApplyResources(Me.txtBpServer, "txtBpServer")
        Me.txtBpServer.Name = "txtBpServer"
        '
        'CaptionControl6
        '
        Me.CaptionControl6.Caption = "The password of the user named above"
        Me.CaptionControl6.CaptionFont = New System.Drawing.Font("Segoe UI", 7.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        resources.ApplyResources(Me.CaptionControl6, "CaptionControl6")
        Me.CaptionControl6.Controls.Add(Me.txtPassword, 0, 0)
        Me.CaptionControl6.Name = "CaptionControl6"
        '
        'txtPassword
        '
        Me.txtPassword.BorderColor = System.Drawing.Color.Empty
        resources.ApplyResources(Me.txtPassword, "txtPassword")
        Me.txtPassword.Name = "txtPassword"
        '
        'CaptionControl5
        '
        Me.CaptionControl5.Caption = "The database user name to use"
        Me.CaptionControl5.CaptionFont = New System.Drawing.Font("Segoe UI", 7.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        resources.ApplyResources(Me.CaptionControl5, "CaptionControl5")
        Me.CaptionControl5.Controls.Add(Me.txtUserId, 0, 0)
        Me.CaptionControl5.Name = "CaptionControl5"
        '
        'txtUserId
        '
        Me.txtUserId.BorderColor = System.Drawing.Color.Empty
        resources.ApplyResources(Me.txtUserId, "txtUserId")
        Me.txtUserId.Name = "txtUserId"
        '
        'CaptionControl4
        '
        Me.CaptionControl4.Caption = "The name of the database to connect to"
        Me.CaptionControl4.CaptionFont = New System.Drawing.Font("Segoe UI", 7.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        resources.ApplyResources(Me.CaptionControl4, "CaptionControl4")
        Me.CaptionControl4.Controls.Add(Me.txtDbName, 0, 0)
        Me.CaptionControl4.Name = "CaptionControl4"
        '
        'txtDbName
        '
        Me.txtDbName.BorderColor = System.Drawing.Color.Empty
        resources.ApplyResources(Me.txtDbName, "txtDbName")
        Me.txtDbName.Name = "txtDbName"
        '
        'CaptionControl3
        '
        Me.CaptionControl3.Caption = "The hostname of the database server"
        Me.CaptionControl3.CaptionFont = New System.Drawing.Font("Segoe UI", 7.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        resources.ApplyResources(Me.CaptionControl3, "CaptionControl3")
        Me.CaptionControl3.Controls.Add(Me.txtDbServer, 0, 0)
        Me.CaptionControl3.Name = "CaptionControl3"
        '
        'txtDbServer
        '
        Me.txtDbServer.BorderColor = System.Drawing.Color.Empty
        resources.ApplyResources(Me.txtDbServer, "txtDbServer")
        Me.txtDbServer.Name = "txtDbServer"
        '
        'CaptionControl2
        '
        Me.CaptionControl2.Caption = "The type of connection to use"
        Me.CaptionControl2.CaptionFont = New System.Drawing.Font("Segoe UI", 7.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        resources.ApplyResources(Me.CaptionControl2, "CaptionControl2")
        Me.CaptionControl2.Controls.Add(Me.cmbConnType, 0, 0)
        Me.CaptionControl2.Name = "CaptionControl2"
        '
        'cmbConnType
        '
        resources.ApplyResources(Me.cmbConnType, "cmbConnType")
        Me.cmbConnType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbConnType.FormattingEnabled = True
        Me.cmbConnType.Name = "cmbConnType"
        '
        'CaptionedControl2
        '
        Me.CaptionedControl2.Caption = "This must match the mode configured on the Blue Prism Server(s)"
        Me.CaptionedControl2.CaptionFont = New System.Drawing.Font("Segoe UI", 7.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        resources.ApplyResources(Me.CaptionedControl2, "CaptionedControl2")
        Me.CaptionedControl2.Controls.Add(Me.cmbConnectionMode, 0, 0)
        Me.CaptionedControl2.Name = "CaptionedControl2"
        '
        'cmbConnectionMode
        '
        resources.ApplyResources(Me.cmbConnectionMode, "cmbConnectionMode")
        Me.cmbConnectionMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbConnectionMode.FormattingEnabled = True
        Me.cmbConnectionMode.Name = "cmbConnectionMode"
        '
        'CaptionedControl1
        '
        Me.CaptionedControl1.Caption = "Semi-colon separated parameters to add to the connection string"
        Me.CaptionedControl1.CaptionFont = New System.Drawing.Font("Segoe UI", 7.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        resources.ApplyResources(Me.CaptionedControl1, "CaptionedControl1")
        Me.CaptionedControl1.Controls.Add(Me.txtExtraParams, 0, 0)
        Me.CaptionedControl1.Name = "CaptionedControl1"
        '
        'txtExtraParams
        '
        Me.txtExtraParams.BorderColor = System.Drawing.Color.Empty
        resources.ApplyResources(Me.txtExtraParams, "txtExtraParams")
        Me.txtExtraParams.Name = "txtExtraParams"
        '
        'Panel9
        '
        Me.Panel9.Controls.Add(Me.numAGPort)
        Me.Panel9.Controls.Add(Me.cbMultiSubnetFailover)
        resources.ApplyResources(Me.Panel9, "Panel9")
        Me.Panel9.Name = "Panel9"
        '
        'numAGPort
        '
        resources.ApplyResources(Me.numAGPort, "numAGPort")
        Me.numAGPort.Maximum = New Decimal(New Integer() {99999, 0, 0, 0})
        Me.numAGPort.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.numAGPort.Name = "numAGPort"
        Me.numAGPort.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'cbMultiSubnetFailover
        '
        resources.ApplyResources(Me.cbMultiSubnetFailover, "cbMultiSubnetFailover")
        Me.cbMultiSubnetFailover.Name = "cbMultiSubnetFailover"
        Me.cbMultiSubnetFailover.UseVisualStyleBackColor = True
        '
        'CaptionedControl3
        '
        Me.CaptionedControl3.Caption = "This must match the listening port configured on the Blue Prism Server(s)"
        Me.CaptionedControl3.CaptionFont = New System.Drawing.Font("Segoe UI", 7.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        resources.ApplyResources(Me.CaptionedControl3, "CaptionedControl3")
        Me.CaptionedControl3.Controls.Add(Me.numServerPort, 0, 0)
        Me.CaptionedControl3.Name = "CaptionedControl3"
        '
        'numServerPort
        '
        resources.ApplyResources(Me.numServerPort, "numServerPort")
        Me.numServerPort.Maximum = New Decimal(New Integer() {99999, 0, 0, 0})
        Me.numServerPort.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.numServerPort.Name = "numServerPort"
        Me.numServerPort.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'CaptionedControl4
        '
        Me.CaptionedControl4.Caption = "The port on this device which receives callback communication"
        Me.CaptionedControl4.CaptionFont = New System.Drawing.Font("Segoe UI", 7.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        resources.ApplyResources(Me.CaptionedControl4, "CaptionedControl4")
        Me.CaptionedControl4.Controls.Add(Me.panCallback, 0, 0)
        Me.CaptionedControl4.Name = "CaptionedControl4"
        '
        'panCallback
        '
        Me.panCallback.Controls.Add(Me.cbDisableCallBack)
        Me.panCallback.Controls.Add(Me.numCallbackPort)
        resources.ApplyResources(Me.panCallback, "panCallback")
        Me.panCallback.Name = "panCallback"
        '
        'cbDisableCallBack
        '
        resources.ApplyResources(Me.cbDisableCallBack, "cbDisableCallBack")
        Me.cbDisableCallBack.Name = "cbDisableCallBack"
        Me.cbDisableCallBack.UseVisualStyleBackColor = True
        '
        'numCallbackPort
        '
        resources.ApplyResources(Me.numCallbackPort, "numCallbackPort")
        Me.numCallbackPort.Maximum = New Decimal(New Integer() {99999, 0, 0, 0})
        Me.numCallbackPort.Name = "numCallbackPort"
        '
        'ConnectionDetail
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.panMain)
        Me.Controls.Add(Panel2)
        Me.Name = "ConnectionDetail"
        resources.ApplyResources(Me, "$this")
        Panel2.ResumeLayout(False)
        Panel2.PerformLayout()
        Me.panMain.ResumeLayout(False)
        Me.panMain.PerformLayout()
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.CaptionedControl5.ResumeLayout(False)
        Me.CaptionedControl5.PerformLayout()
        Me.CaptionControl1.ResumeLayout(False)
        Me.CaptionControl1.PerformLayout()
        Me.CaptionControl7.ResumeLayout(False)
        Me.CaptionControl7.PerformLayout()
        Me.CaptionControl6.ResumeLayout(False)
        Me.CaptionControl6.PerformLayout()
        Me.CaptionControl5.ResumeLayout(False)
        Me.CaptionControl5.PerformLayout()
        Me.CaptionControl4.ResumeLayout(False)
        Me.CaptionControl4.PerformLayout()
        Me.CaptionControl3.ResumeLayout(False)
        Me.CaptionControl3.PerformLayout()
        Me.CaptionControl2.ResumeLayout(False)
        Me.CaptionedControl2.ResumeLayout(False)
        Me.CaptionedControl1.ResumeLayout(False)
        Me.CaptionedControl1.PerformLayout()
        Me.Panel9.ResumeLayout(False)
        Me.Panel9.PerformLayout()
        CType(Me.numAGPort, System.ComponentModel.ISupportInitialize).EndInit()
        Me.CaptionedControl3.ResumeLayout(False)
        CType(Me.numServerPort, System.ComponentModel.ISupportInitialize).EndInit()
        Me.CaptionedControl4.ResumeLayout(False)
        Me.panCallback.ResumeLayout(False)
        Me.panCallback.PerformLayout()
        CType(Me.numCallbackPort, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

End Sub
    Private WithEvents txtConnectionName As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents txtDbName As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents txtDbServer As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents txtUserId As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents txtBpServer As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents Panel9 As System.Windows.Forms.Panel
    Private WithEvents numServerPort As AutomateControls.StyledNumericUpDown
    Private WithEvents numCallbackPort As AutomateControls.StyledNumericUpDown
    Private WithEvents cmbConnType As System.Windows.Forms.ComboBox
    Private WithEvents btnTest As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents cbMultiSubnetFailover As System.Windows.Forms.CheckBox
    Private WithEvents numAGPort As AutomateControls.StyledNumericUpDown
    Private WithEvents txtExtraParams As AutomateControls.Textboxes.StyledTextBox
    Private WithEvents txtPassword As AutomateControls.SecurePasswordTextBox
    Private WithEvents CaptionedControl2 As BluePrism.Config.CaptionedControl
    Private WithEvents cmbConnectionMode As System.Windows.Forms.ComboBox
    Private WithEvents CaptionedControl3 As BluePrism.Config.CaptionedControl
    Private WithEvents CaptionedControl4 As BluePrism.Config.CaptionedControl
    Private WithEvents cbDisableCallBack As System.Windows.Forms.CheckBox
    Friend WithEvents panCallback As Panel
    Friend WithEvents CaptionControl1 As CaptionedControl
    Friend WithEvents CaptionControl7 As CaptionedControl
    Friend WithEvents CaptionControl6 As CaptionedControl
    Friend WithEvents CaptionControl5 As CaptionedControl
    Friend WithEvents CaptionControl4 As CaptionedControl
    Friend WithEvents CaptionControl3 As CaptionedControl
    Friend WithEvents CaptionControl2 As CaptionedControl
    Friend WithEvents CaptionedControl1 As CaptionedControl
    Friend WithEvents CaptionedControl5 As CaptionedControl
    Friend WithEvents panMain As TableLayoutPanel
    Private WithEvents txtConnectionString As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents Panel1 As Panel
    Friend WithEvents chkOrdered As CheckBox
End Class
