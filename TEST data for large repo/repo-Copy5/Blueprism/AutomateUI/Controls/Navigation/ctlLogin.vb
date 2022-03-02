Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Images
Imports BluePrism.BPCoreLib
Imports System.Globalization
Imports BluePrism.Common.Security
Imports BluePrism.Config
Imports BluePrism.ExternalLoginBrowser
Imports BluePrism.ExternalLoginBrowser.Form
Imports BluePrism.ExternalLoginBrowser.Cookies
Imports AutomateControls
Imports AutomateControls.Textboxes

''' Project  : Automate
''' Class    : ctlLogin
''' 
''' <summary>
''' The login control.
''' </summary>
Friend Class ctlLogin
    Inherits UserControl
    Implements IHelp, IPermission, IChild

#Region " Member Variables "

    ' The map of users against their user names.
    Private mUsers As New SortedDictionary(Of String, User)

    ' The configured logon options
    Private mLogonOptions As LogonOptions

    Private mUseAuthenticationServerSignIn As Boolean

    ' The application form hosting this control

    Private WithEvents mBalloonTip As System.Windows.Forms.ToolTip
    Private WithEvents panDirect As System.Windows.Forms.Panel
    Private WithEvents panSSO As System.Windows.Forms.Panel
    Friend WithEvents cmbConnection As System.Windows.Forms.ComboBox
    Private WithEvents llConfigure As System.Windows.Forms.LinkLabel
    Private WithEvents llLocale As LinkLabel
    Friend WithEvents SwitchPanel As AutomateControls.SwitchPanel
    Friend WithEvents tpDirectLogin As System.Windows.Forms.TabPage
    Friend WithEvents tpSSOLogin As System.Windows.Forms.TabPage
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Private WithEvents llSSOConfigure As System.Windows.Forms.LinkLabel
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents btnSSOLogin As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents llRefresh As System.Windows.Forms.LinkLabel

    ' Flag indicating if this control has handled the Load event or not yet
    Private mLoaded As Boolean
    Private mExternalLoginFormOpen As Boolean
    Friend WithEvents gpBPLogin As GroupBox
    Private WithEvents txtUsername As StyledTextBox
    Friend WithEvents txtPassword As ctlAutomateSecurePassword
    Friend WithEvents btnDirectLogin As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents gpActiveDirectory As GroupBox
    Friend WithEvents btnSignInUsingActiveDirectory As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents PicExternalLoading As PictureBox
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents Panel2 As Panel
    Friend WithEvents tpAuthenticationServerSignin As TabPage
    Private WithEvents pnlAuthenticationServerSignin As Panel
    Friend WithEvents btnAuthenticationServerSignin As Buttons.StandardStyledButton
    Private WithEvents LinkLabel1 As LinkLabel
    Private Shared mPseudoLocalization As Boolean

#End Region

#Region " Constructors "

    Public Sub New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        If Application.RenderWithVisualStyles Then
            Me.Panel1.BackColor = Color.White
            Me.BackColor = Color.FromKnownColor(KnownColor.Control)
        Else
            Me.BackColor = Color.White
            Me.Panel1.BackColor = Color.FromKnownColor(KnownColor.Control)
        End If

        txtPassword.PasswordChar = BPUtil.PasswordChar

        AddHandler Options.Instance.Load, AddressOf HandleOptionsLoaded

    End Sub

#End Region

#Region " Windows Form Designer generated code "

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer
    Private WithEvents pbLogo As System.Windows.Forms.PictureBox

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim Label4 As System.Windows.Forms.Label
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlLogin))
        Dim LblAuthenticationServerConnecting As System.Windows.Forms.Label
        Dim llPassword As System.Windows.Forms.Label
        Dim llUsername As System.Windows.Forms.Label
        Me.pbLogo = New System.Windows.Forms.PictureBox()
        Me.mBalloonTip = New System.Windows.Forms.ToolTip(Me.components)
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.PicExternalLoading = New System.Windows.Forms.PictureBox()
        Me.llLocale = New System.Windows.Forms.LinkLabel()
        Me.llRefresh = New System.Windows.Forms.LinkLabel()
        Me.llConfigure = New System.Windows.Forms.LinkLabel()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.SwitchPanel = New AutomateControls.SwitchPanel()
        Me.tpDirectLogin = New System.Windows.Forms.TabPage()
        Me.panDirect = New System.Windows.Forms.Panel()
        Me.gpActiveDirectory = New System.Windows.Forms.GroupBox()
        Me.btnSignInUsingActiveDirectory = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.gpBPLogin = New System.Windows.Forms.GroupBox()
        Me.txtUsername = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtPassword = New AutomateUI.ctlAutomateSecurePassword()
        Me.btnDirectLogin = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.tpSSOLogin = New System.Windows.Forms.TabPage()
        Me.panSSO = New System.Windows.Forms.Panel()
        Me.btnSSOLogin = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.llSSOConfigure = New System.Windows.Forms.LinkLabel()
        Me.tpAuthenticationServerSignin = New System.Windows.Forms.TabPage()
        Me.pnlAuthenticationServerSignin = New System.Windows.Forms.Panel()
        Me.btnAuthenticationServerSignin = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.LinkLabel1 = New System.Windows.Forms.LinkLabel()
        Me.cmbConnection = New System.Windows.Forms.ComboBox()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Label4 = New System.Windows.Forms.Label()
        LblAuthenticationServerConnecting = New System.Windows.Forms.Label()
        llPassword = New System.Windows.Forms.Label()
        llUsername = New System.Windows.Forms.Label()
        CType(Me.pbLogo, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.Panel1.SuspendLayout()
        Me.Panel2.SuspendLayout()
        CType(Me.PicExternalLoading, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SwitchPanel.SuspendLayout()
        Me.tpDirectLogin.SuspendLayout()
        Me.panDirect.SuspendLayout()
        Me.gpActiveDirectory.SuspendLayout()
        Me.gpBPLogin.SuspendLayout()
        Me.tpSSOLogin.SuspendLayout()
        Me.panSSO.SuspendLayout()
        Me.tpAuthenticationServerSignin.SuspendLayout()
        Me.pnlAuthenticationServerSignin.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'Label4
        '
        resources.ApplyResources(Label4, "Label4")
        Label4.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Label4.Name = "Label4"
        '
        'LblAuthenticationServerConnecting
        '
        resources.ApplyResources(LblAuthenticationServerConnecting, "LblAuthenticationServerConnecting")
        LblAuthenticationServerConnecting.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        LblAuthenticationServerConnecting.Name = "LblAuthenticationServerConnecting"
        '
        'llPassword
        '
        resources.ApplyResources(llPassword, "llPassword")
        llPassword.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        llPassword.Name = "llPassword"
        '
        'llUsername
        '
        resources.ApplyResources(llUsername, "llUsername")
        llUsername.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        llUsername.Name = "llUsername"
        '
        'pbLogo
        '
        resources.ApplyResources(Me.pbLogo, "pbLogo")
        Me.pbLogo.Name = "pbLogo"
        Me.pbLogo.TabStop = False
        '
        'mBalloonTip
        '
        Me.mBalloonTip.AutoPopDelay = 0
        Me.mBalloonTip.InitialDelay = 30000
        Me.mBalloonTip.IsBalloon = True
        Me.mBalloonTip.ReshowDelay = 0
        Me.mBalloonTip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info
        '
        'TableLayoutPanel1
        '
        resources.ApplyResources(Me.TableLayoutPanel1, "TableLayoutPanel1")
        Me.TableLayoutPanel1.Controls.Add(Me.Panel1, 1, 1)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        '
        'Panel1
        '
        Me.Panel1.BackColor = System.Drawing.Color.White
        Me.Panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Panel1.Controls.Add(Me.Panel2)
        Me.Panel1.Controls.Add(Me.llLocale)
        Me.Panel1.Controls.Add(Me.llRefresh)
        Me.Panel1.Controls.Add(Me.llConfigure)
        Me.Panel1.Controls.Add(Me.Label1)
        Me.Panel1.Controls.Add(Me.SwitchPanel)
        Me.Panel1.Controls.Add(Me.cmbConnection)
        Me.Panel1.Controls.Add(Label4)
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.Name = "Panel1"
        '
        'Panel2
        '
        resources.ApplyResources(Me.Panel2, "Panel2")
        Me.Panel2.BackColor = System.Drawing.Color.Transparent
        Me.Panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Panel2.Controls.Add(LblAuthenticationServerConnecting)
        Me.Panel2.Controls.Add(Me.PicExternalLoading)
        Me.Panel2.Name = "Panel2"
        '
        'PicExternalLoading
        '
        resources.ApplyResources(Me.PicExternalLoading, "PicExternalLoading")
        Me.PicExternalLoading.Image = Global.AutomateUI.My.Resources.Resources.preloader
        Me.PicExternalLoading.Name = "PicExternalLoading"
        Me.PicExternalLoading.TabStop = False
        '
        'llLocale
        '
        resources.ApplyResources(Me.llLocale, "llLocale")
        Me.llLocale.BackColor = System.Drawing.Color.White
        Me.llLocale.ForeColor = System.Drawing.Color.FromArgb(CType(CType(11, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(183, Byte), Integer))
        Me.llLocale.LinkColor = System.Drawing.Color.SteelBlue
        Me.llLocale.Name = "llLocale"
        Me.llLocale.TabStop = True
        Me.llLocale.UseCompatibleTextRendering = True
        '
        'llRefresh
        '
        resources.ApplyResources(Me.llRefresh, "llRefresh")
        Me.llRefresh.ForeColor = System.Drawing.Color.FromArgb(CType(CType(11, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(183, Byte), Integer))
        Me.llRefresh.LinkColor = System.Drawing.Color.SteelBlue
        Me.llRefresh.Name = "llRefresh"
        Me.llRefresh.TabStop = True
        '
        'llConfigure
        '
        resources.ApplyResources(Me.llConfigure, "llConfigure")
        Me.llConfigure.ForeColor = System.Drawing.Color.FromArgb(CType(CType(11, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(183, Byte), Integer))
        Me.llConfigure.LinkColor = System.Drawing.Color.SteelBlue
        Me.llConfigure.Name = "llConfigure"
        Me.llConfigure.TabStop = True
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.Label1.Name = "Label1"
        '
        'SwitchPanel
        '
        resources.ApplyResources(Me.SwitchPanel, "SwitchPanel")
        Me.SwitchPanel.Controls.Add(Me.tpDirectLogin)
        Me.SwitchPanel.Controls.Add(Me.tpSSOLogin)
        Me.SwitchPanel.Controls.Add(Me.tpAuthenticationServerSignin)
        Me.SwitchPanel.DisableArrowKeys = False
        Me.SwitchPanel.Name = "SwitchPanel"
        Me.SwitchPanel.SelectedIndex = 0
        '
        'tpDirectLogin
        '
        Me.tpDirectLogin.Controls.Add(Me.panDirect)
        resources.ApplyResources(Me.tpDirectLogin, "tpDirectLogin")
        Me.tpDirectLogin.Name = "tpDirectLogin"
        Me.tpDirectLogin.UseVisualStyleBackColor = True
        '
        'panDirect
        '
        resources.ApplyResources(Me.panDirect, "panDirect")
        Me.panDirect.BackColor = System.Drawing.Color.Transparent
        Me.panDirect.Controls.Add(Me.gpActiveDirectory)
        Me.panDirect.Controls.Add(Me.gpBPLogin)
        Me.panDirect.Name = "panDirect"
        '
        'gpActiveDirectory
        '
        resources.ApplyResources(Me.gpActiveDirectory, "gpActiveDirectory")
        Me.gpActiveDirectory.Controls.Add(Me.btnSignInUsingActiveDirectory)
        Me.gpActiveDirectory.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.gpActiveDirectory.Name = "gpActiveDirectory"
        Me.gpActiveDirectory.TabStop = False
        '
        'btnSignInUsingActiveDirectory
        '
        resources.ApplyResources(Me.btnSignInUsingActiveDirectory, "btnSignInUsingActiveDirectory")
        Me.btnSignInUsingActiveDirectory.BackColor = System.Drawing.SystemColors.Window
        Me.btnSignInUsingActiveDirectory.ForeColor = System.Drawing.Color.FromArgb(CType(CType(11, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(183, Byte), Integer))
        Me.btnSignInUsingActiveDirectory.Name = "btnSignInUsingActiveDirectory"
        Me.btnSignInUsingActiveDirectory.UseVisualStyleBackColor = False
        '
        'gpBPLogin
        '
        resources.ApplyResources(Me.gpBPLogin, "gpBPLogin")
        Me.gpBPLogin.Controls.Add(Me.txtUsername)
        Me.gpBPLogin.Controls.Add(llPassword)
        Me.gpBPLogin.Controls.Add(llUsername)
        Me.gpBPLogin.Controls.Add(Me.txtPassword)
        Me.gpBPLogin.Controls.Add(Me.btnDirectLogin)
        Me.gpBPLogin.ForeColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.gpBPLogin.Name = "gpBPLogin"
        Me.gpBPLogin.TabStop = False
        '
        'txtUsername
        '
        resources.ApplyResources(Me.txtUsername, "txtUsername")
        Me.txtUsername.BorderColor = System.Drawing.Color.FromArgb(CType(CType(11, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(183, Byte), Integer))
        Me.txtUsername.Name = "txtUsername"
        '
        'txtPassword
        '
        resources.ApplyResources(Me.txtPassword, "txtPassword")
        Me.txtPassword.BorderColor = System.Drawing.Color.FromArgb(CType(CType(11, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(183, Byte), Integer))
        Me.txtPassword.Name = "txtPassword"
        '
        'btnDirectLogin
        '
        resources.ApplyResources(Me.btnDirectLogin, "btnDirectLogin")
        Me.btnDirectLogin.BackColor = System.Drawing.SystemColors.Window
        Me.btnDirectLogin.ForeColor = System.Drawing.Color.FromArgb(CType(CType(11, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(183, Byte), Integer))
        Me.btnDirectLogin.Name = "btnDirectLogin"
        Me.btnDirectLogin.UseVisualStyleBackColor = False
        '
        'tpSSOLogin
        '
        Me.tpSSOLogin.Controls.Add(Me.panSSO)
        resources.ApplyResources(Me.tpSSOLogin, "tpSSOLogin")
        Me.tpSSOLogin.Name = "tpSSOLogin"
        Me.tpSSOLogin.UseVisualStyleBackColor = True
        '
        'panSSO
        '
        Me.panSSO.Controls.Add(Me.btnSSOLogin)
        Me.panSSO.Controls.Add(Me.llSSOConfigure)
        resources.ApplyResources(Me.panSSO, "panSSO")
        Me.panSSO.Name = "panSSO"
        '
        'btnSSOLogin
        '
        resources.ApplyResources(Me.btnSSOLogin, "btnSSOLogin")
        Me.btnSSOLogin.BackColor = System.Drawing.Color.White
        Me.btnSSOLogin.Name = "btnSSOLogin"
        Me.btnSSOLogin.UseVisualStyleBackColor = False
        '
        'llSSOConfigure
        '
        resources.ApplyResources(Me.llSSOConfigure, "llSSOConfigure")
        Me.llSSOConfigure.ForeColor = System.Drawing.Color.SteelBlue
        Me.llSSOConfigure.LinkColor = System.Drawing.Color.SteelBlue
        Me.llSSOConfigure.Name = "llSSOConfigure"
        Me.llSSOConfigure.TabStop = True
        '
        'tpAuthenticationServerSignin
        '
        Me.tpAuthenticationServerSignin.Controls.Add(Me.pnlAuthenticationServerSignin)
        resources.ApplyResources(Me.tpAuthenticationServerSignin, "tpAuthenticationServerSignin")
        Me.tpAuthenticationServerSignin.Name = "tpAuthenticationServerSignin"
        Me.tpAuthenticationServerSignin.UseVisualStyleBackColor = True
        '
        'pnlAuthenticationServerSignin
        '
        Me.pnlAuthenticationServerSignin.Controls.Add(Me.btnAuthenticationServerSignin)
        Me.pnlAuthenticationServerSignin.Controls.Add(Me.LinkLabel1)
        resources.ApplyResources(Me.pnlAuthenticationServerSignin, "pnlAuthenticationServerSignin")
        Me.pnlAuthenticationServerSignin.Name = "pnlAuthenticationServerSignin"
        '
        'btnAuthenticationServerSignin
        '
        resources.ApplyResources(Me.btnAuthenticationServerSignin, "btnAuthenticationServerSignin")
        Me.btnAuthenticationServerSignin.BackColor = System.Drawing.SystemColors.Window
        Me.btnAuthenticationServerSignin.ForeColor = System.Drawing.Color.FromArgb(CType(CType(11, Byte), Integer), CType(CType(117, Byte), Integer), CType(CType(183, Byte), Integer))
        Me.btnAuthenticationServerSignin.Name = "btnAuthenticationServerSignin"
        Me.btnAuthenticationServerSignin.UseVisualStyleBackColor = False
        '
        'LinkLabel1
        '
        resources.ApplyResources(Me.LinkLabel1, "LinkLabel1")
        Me.LinkLabel1.ForeColor = System.Drawing.Color.SteelBlue
        Me.LinkLabel1.LinkColor = System.Drawing.Color.SteelBlue
        Me.LinkLabel1.Name = "LinkLabel1"
        Me.LinkLabel1.TabStop = True
        '
        'cmbConnection
        '
        resources.ApplyResources(Me.cmbConnection, "cmbConnection")
        Me.cmbConnection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbConnection.FormattingEnabled = True
        Me.cmbConnection.Name = "cmbConnection"
        '
        'PictureBox1
        '
        resources.ApplyResources(Me.PictureBox1, "PictureBox1")
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.TabStop = False
        '
        'ctlLogin
        '
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Controls.Add(Me.pbLogo)
        Me.Name = "ctlLogin"
        resources.ApplyResources(Me, "$this")
        CType(Me.pbLogo, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.Panel2.ResumeLayout(False)
        Me.Panel2.PerformLayout()
        CType(Me.PicExternalLoading, System.ComponentModel.ISupportInitialize).EndInit()
        Me.SwitchPanel.ResumeLayout(False)
        Me.tpDirectLogin.ResumeLayout(False)
        Me.tpDirectLogin.PerformLayout()
        Me.panDirect.ResumeLayout(False)
        Me.panDirect.PerformLayout()
        Me.gpActiveDirectory.ResumeLayout(False)
        Me.gpActiveDirectory.PerformLayout()
        Me.gpBPLogin.ResumeLayout(False)
        Me.gpBPLogin.PerformLayout()
        Me.tpSSOLogin.ResumeLayout(False)
        Me.panSSO.ResumeLayout(False)
        Me.panSSO.PerformLayout()
        Me.tpAuthenticationServerSignin.ResumeLayout(False)
        Me.pnlAuthenticationServerSignin.ResumeLayout(False)
        Me.pnlAuthenticationServerSignin.PerformLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The permission level for the control. This control is universal, so it
    ''' returns a full permission level
    ''' </summary>
    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.None
        End Get
    End Property

    Friend Shared Property PseudoLocalization() As Boolean
        Get
            Return mPseudoLocalization
        End Get
        Set(ByVal value As Boolean)
            mPseudoLocalization = value
        End Set
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Handles the options being loaded from the config file - this ensures
    ''' that the links displayed in the Connections task pane are updated with
    ''' the current values.
    ''' </summary>
    Private Sub HandleOptionsLoaded()
        ' Make sure this is invoked on the appropriate thread (asynchronously)
        If InvokeRequired Then
            BeginInvoke(New MethodInvoker(AddressOf UpdateDatabaseTasks))
        Else
            UpdateDatabaseTasks()
        End If
    End Sub

    ''' <summary>
    ''' Updates the task pane that this control maintains for the connections
    ''' configured on this machine
    ''' </summary>
    Friend Sub UpdateDatabaseTasks()
        If mParent Is Nothing Then Return

        With cmbConnection
            .BeginUpdate()
            .Items.Clear()
            ' Go through each connection setting and create a task for it
            ' Ensure that the current connection setting is emphasised.
            Dim configOptions = Options.Instance
            Dim currSetting As clsDBConnectionSetting = configOptions.DbConnectionSetting
            For Each setting As clsDBConnectionSetting In configOptions.Connections
                .Items.Add(setting.ConnectionName)
            Next
            .Text = currSetting.ConnectionName
            .EndUpdate()
        End With

    End Sub

    ''' <summary>
    ''' Handle the connections configuration panel being requested.
    ''' </summary>
    Private Sub HandleConfigureConnections(ByVal sender As Object, ByVal e As LinkLabelLinkClickedEventArgs) _
      Handles llConfigure.LinkClicked, llSSOConfigure.LinkClicked
        If mParent IsNot Nothing Then mParent.ShowDBConfig()
    End Sub

    Private Sub llLocale_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles llLocale.LinkClicked
        Using localeConfigForm As New SelectLanguageForm(ctlLogin.PseudoLocalization)
            localeConfigForm.ShowInTaskbar = False
            localeConfigForm.StartPosition = FormStartPosition.CenterParent
            If localeConfigForm.ShowDialog() = DialogResult.OK AndAlso localeConfigForm.NewLocale IsNot Nothing Then
                If localeConfigForm.NewLocale <> Options.Instance.CurrentLocale Then
                    ChangeLocale(localeConfigForm.NewLocale)
                End If
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Handles the connection being changed by clicking on the refresh button
    ''' </summary>
    Private Sub HandleConnectionClicked(ByVal sender As Object, ByVal e As EventArgs) Handles llRefresh.LinkClicked

        mParent.WaitForInitConnection(cmbConnection.Text)

    End Sub

    ''' <summary>
    ''' Property Used to make it simple to enable
    ''' or disable the login controls
    ''' </summary>
    Friend Property LoginEnabled As Boolean
        Get
            Return txtUsername.Enabled AndAlso
            txtPassword.Enabled AndAlso
            btnDirectLogin.Enabled AndAlso
            btnSSOLogin.Enabled AndAlso
            Not llRefresh.Visible AndAlso
            btnSignInUsingActiveDirectory.Enabled
        End Get
        Set(value As Boolean)
            txtUsername.Enabled = value
            txtPassword.Enabled = value
            btnDirectLogin.Enabled = value
            btnSSOLogin.Enabled = value
            llRefresh.Visible = Not value
            btnSignInUsingActiveDirectory.Enabled = value
        End Set
    End Property

    ''' <summary>
    ''' Handles a connection changed by picking an option from the connection drop down
    ''' </summary>
    Private Sub HandleConnectionChanged(ByVal sender As Object, ByVal e As EventArgs) Handles cmbConnection.SelectedValueChanged

        If Not IsHandleCreated Then Return

        ' Get the currently selected name - ie. the name of the currently selected
        ' connection
        Dim currName As String = Options.Instance.CurrentConnectionName

        ' Get the new name - ie. the text of the link which was clicked
        Dim newName As String = cmbConnection.Text

        ' If they match, ignore the click
        If currName = newName Then Return

        HandleConnectionClicked(sender, e)
    End Sub

    ''' <summary>
    ''' Handles the connection being changed and intialised by the parent Application
    ''' form.
    ''' </summary>
    ''' <param name="e">The event args defining the database change event. Whether
    ''' the connection initialisation was successful or not is defined here, along
    ''' with a message indicating to the user whether it was successful or not.
    ''' </param>
    Private Sub ParentHandleConnectionChanged(ByVal sender As Object, ByVal e As DatabaseChangeEventArgs)
        Try
            ' Enable / Disable the login controls based on the success of the connection
            LoginEnabled = e.Success

            With mParent.GetTaskPanel()
                .BeginUpdate()
                .Clear()
                .EndUpdate()
            End With

            ' There's no point in continuing from here if we don't have a successful
            ' connection to the database.
            If Not e.Success Then
                ' Build the error message for the user
                Dim msg As String = My.Resources.ctlLogin_Error & e.ShortMessage & vbCrLf & vbCrLf & e.LongMessage
                ' If still loading the login control, queue the message for after the
                ' control has loaded. Otherwise just display it.
                If Not mLoaded Then
                    BeginInvoke(New Action(Of String, Exception)(AddressOf UserMessage.Show), msg, e.Exception)
                Else
                    UserMessage.Show(msg, e.Exception)
                End If
                ' Either way, return now.. we can't get a user list from a dead connection
                Return
            End If

            ' Set the allow pasting property of the password text control, from the
            ' setting in the database
            txtPassword.AllowPasting = gSv.GetAllowPasswordPasting()

            ' Show or hide the appropriate panel for single-sign-on
            Dim allUsers As ICollection(Of User) = Nothing
            mLogonOptions = gSv.GetLogonOptions(allUsers)

            If mLogonOptions.ShowUserList Then
                mParent.GetTaskPanel().Add(My.Resources.ctlLogin_SignInPanel, AuthImages.Users_32x32, AuthImages.Users_32x32_Disabled, True, AddressOf mParent.ShowUserMenu)
            End If

            mUseAuthenticationServerSignIn = mLogonOptions.AuthenticationServerAuthenticationEnabled

            SetActiveTab(mUseAuthenticationServerSignIn)
            SetLoginPanelHeight(mLogonOptions.SingleSignon OrElse mUseAuthenticationServerSignIn)

            If mUseAuthenticationServerSignIn Then Return

            If Not mLogonOptions.SingleSignon Then

                ' Get the list of users and store them in memory for lookups when they type thier
                ' user name in we can check whether they will expire soon.
                mUsers.Clear()
                For Each user In allUsers
                    If Not user.IsHidden AndAlso Not user.Deleted Then
                        mUsers(user.Name) = user
                    End If
                Next

                ' If we need to show the list of users on the side bar we can add them now
                If mLogonOptions.ShowUserList Then
                    'Add users to User menu
                    mParent.ClearUsers()

                    If mUsers.Count <> 0 Then
                        Dim img As Image = AuthImages.User_Blue_16x16
                        For Each user As User In mUsers.Values
                            mParent.AddUser(user.Name, img, AddressOf UserClicked)
                        Next
                    End If
                End If

                'we try to get the logon options, but if we can't we don't want to alert the 
                'user that the database connection string maybe wrong until they try to log in
                Select Case mLogonOptions.AutoPopulate
                    Case AutoPopulateMode.None : txtUsername.Text = ""
                    Case AutoPopulateMode.SystemUser : txtUsername.Text = Environment.UserName
                    Case AutoPopulateMode.LastUser : txtUsername.Text = Options.Instance.LastNativeUser
                End Select

            End If

            DisplayMappedActiveDirectoryLoginButton()
            FocusUsernameOrPassword()
        Catch
        End Try
    End Sub

    Private Sub SetActiveTab(useAuthenticationServerSignin As Boolean)
        If useAuthenticationServerSignin Then
            SwitchPanel.SelectedTab = tpAuthenticationServerSignin
        ElseIf mLogonOptions.SingleSignon Then
            SwitchPanel.SelectedTab = tpSSOLogin
        Else
            SwitchPanel.SelectedTab = tpDirectLogin
        End If
    End Sub

    Private Sub SetLoginPanelHeight(useSmallerWindow As Boolean)
        TableLayoutPanel1.RowStyles(1).Height = If(useSmallerWindow, 209, 499)
    End Sub

    ''' <summary>
    ''' Delay focus the username or password control
    ''' </summary>
    Friend Sub FocusUsernameOrPassword()
        Try
            ' "Something" grabs the focus if you just set the focus.
            ' Previous workaround was a timer which set the focus after a second (ugh)
            ' This is better. It sets it immediately after all current queued UI operations
            ' have completed, meaning I won't get error messages saying "Bad user: 'min'"
            Dim focusCtl As Control
            If txtUsername.Text = "" Then focusCtl = txtUsername Else focusCtl = txtPassword
            clsUserInterfaceUtils.SetFocusDelayed(focusCtl)
        Catch
            'If we fail to set focus the user will have to click. Not a big deal
        End Try
    End Sub

    Private WithEvents mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
            UpdateDatabaseTasks()
            If mParent IsNot Nothing Then
                AddHandler mParent.ConnectionChanged, New frmApplication.ConnectionChangedEventHandler(AddressOf ParentHandleConnectionChanged)
            End If
        End Set
    End Property

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "frmLogin.htm"
    End Function

    ''' <summary>
    ''' The usename hyperlink click event handler.
    ''' </summary>
    Public Sub UserClicked(ByVal sender As Object, ByVal e As EventArgs)
        txtUsername.Text = DirectCast(sender, ToolStripItem).Text
        txtPassword.Clear()
        txtPassword.Focus()
    End Sub

    ''' <summary>
    ''' Handles this control being loaded.
    ''' This checks the status of the database.
    ''' </summary>
    Protected Overrides Sub OnLoad(ByVal e As EventArgs)
        MyBase.OnLoad(e)

        'Get the logo and position it nicely.
        pbLogo.Image = Branding.GetLargeLogo()
        If pbLogo.Image IsNot Nothing Then
            pbLogo.Size = pbLogo.Image.Size
            pbLogo.Location = New Point(
             Me.Width - pbLogo.Image.Width - Branding.LargeLogoMarginRight,
             Me.Height - pbLogo.Image.Height - Branding.LargeLogoMarginBottom)
        End If
        If Options.Instance.Unbranded Then
            Label1.Text = My.Resources.ctlLogin_SignIn
        End If
        mLoaded = True
        gpActiveDirectory.Hide()
    End Sub

    Private Sub Login()
        mBalloonTip.Hide(txtPassword)
        mParent.AttemptBluePrismLogin(txtUsername, txtPassword, CultureInfo.CurrentUICulture.Name)
    End Sub

    Private Sub txtPassword_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles txtPassword.KeyDown
        If e.KeyCode = Keys.Enter OrElse e.KeyCode = Keys.Tab Then Login()
    End Sub

    Private Sub txtUsername_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles txtUsername.KeyDown
        If e.KeyCode = Keys.Enter OrElse e.KeyCode = Keys.Tab Then txtPassword.Focus()
    End Sub

    ''' <summary>
    ''' Handles the password field being entered - this just shows the bubble tooltip
    ''' with an expiry warning for the current user if:-
    ''' a) a registered user has been entered in the username field
    ''' b) such expiry warnings are enabled for this connection, and
    ''' c) the user account/password expires within the configured window
    ''' </summary>
    Private Sub txtPassword_Enter(ByVal sender As Object, ByVal e As EventArgs) Handles txtPassword.Enter

        ' Get the current user
        Dim user As User = Nothing

        ' If there are no users defined, or no username has been entered, or
        ' the user entered doesn't match one of our registered users, just do nothing
        If mUsers.Count = 0 OrElse txtUsername.Text = "" OrElse
         Not mUsers.TryGetValue(txtUsername.Text, user) Then Return

        ' Get the warning message, and display it if there is one
        Dim warningMessage As String = GetExpiryMessage(user)
        If warningMessage <> "" Then
            mBalloonTip.ToolTipTitle = GetExpiryPrompt(user)
            ' Seems redundant (is), but necessary to workaround a 'bubble doesn't
            ' point to the attached control' issue with bubble tooltips
            ' http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=98281
            ' After much playing about, this method seemed pretty solid (ie. always
            ' displayed it pointing the right way) and the least ugly (ie. least flicker)
            ' It's a combination of a couple of the workarounds from the above link
            mBalloonTip.Active = False
            mBalloonTip.SetToolTip(txtPassword, warningMessage)
            mBalloonTip.Active = True
            mBalloonTip.Show(warningMessage, txtPassword, CType(txtPassword.Size, Point))

        End If

    End Sub

    ''' <summary>
    ''' The expiry message for this user.
    ''' </summary>
    Public Function GetExpiryMessage(u As User) As String
        ' Build up a format string.
        ' The Format() call is done at the end, but the arguments are :-
        ' 0: account expiry date
        ' 1: password expiry date
        ' 2: newline
        Dim formatStr As String = ""

        ' Account expiry warning first.
        If u.AccountExpiresSoon Then
            If u.Expiry <= Today Then
                formatStr =
                 My.Resources.ctlLogin_WarningYourAccountExpiredOn0D22PleaseAskTheSystemManagerToReactivateThisAccount
            Else
                formatStr =
                 My.Resources.ctlLogin_ReminderYourAccountWillExpireOn0D22PleaseContactYourBluePrismAdministratorToExt
            End If
        End If

        ' Password expiry warning follows
        If u.PasswordExpiresSoon Then
            ' If there are warnings / reminders on both account and password,
            ' we show both separated by a couple of newlines
            If formatStr <> "" Then formatStr &= "{2}{2}"

            If u.PasswordExpiry < Today Then
                formatStr &=
                 My.Resources.ctlLogin_WarningYourPasswordExpiredOn1D2AndWillNeedToBeChangedWhenYouNextLogIn
            ElseIf u.PasswordExpiry > Today Then
                formatStr &=
                 My.Resources.ctlLogin_WarningYourPasswordWillExpireOn1D2AndWillNeedToBeChangedWhenYouNextLogInAfterTh
            Else ' ie. password expires today
                formatStr &=
                 My.Resources.ctlLogin_ReminderYourPasswordExpiresToday2AndWillNeedToBeChangedWhenYouNextLogIn
            End If
        End If
        If formatStr = "" Then Return "" ' Shortcut avoiding redundant method call
        Return String.Format(formatStr, u.Expiry, u.PasswordExpiry, vbCrLf)
    End Function

    ''' <summary>
    ''' The prompt for this user regarding account / password expiry - a short
    ''' prompt indicating that there is a reminder to view.
    ''' </summary>
    Public Function GetExpiryPrompt(u As User) As String
        Dim pwWarning As Boolean = u.PasswordExpiresSoon
        Dim accWarning As Boolean = u.AccountExpiresSoon
        If pwWarning AndAlso accWarning Then Return My.Resources.ctlLogin_AccountAndPasswordExpiryReminder
        If accWarning Then Return My.Resources.ctlLogin_AccountExpiryReminder
        If pwWarning Then Return My.Resources.ctlLogin_PasswordExpiryReminder
        Return ""
    End Function

    ''' <summary>
    ''' Handles the disposing of the expiry warning for this control if the login
    ''' control is resized, the form is moved or the password field loses focus.
    ''' </summary>
    Private Sub HandleHideWarning(ByVal sender As Object, ByVal e As EventArgs) _
        Handles MyBase.Resize, mParent.Move, txtPassword.Leave, mParent.Deactivate

        mBalloonTip.Hide(txtPassword)
    End Sub

    ''' <summary>
    ''' Disposes of this control
    ''' </summary>
    ''' <param name="explicit">True if called explicitly via a Dispose() call, False
    ''' if called at object destruction time</param>
    Protected Overrides Sub Dispose(ByVal explicit As Boolean)
        If explicit Then
            If components IsNot Nothing Then components.Dispose()
            RemoveHandler Options.Instance.Load, AddressOf HandleOptionsLoaded
            If mParent IsNot Nothing Then
                RemoveHandler mParent.ConnectionChanged, AddressOf ParentHandleConnectionChanged
            End If
        End If
        MyBase.Dispose(explicit)
    End Sub

    Private Sub DisplayMappedActiveDirectoryLoginButton()
        If Not mLogonOptions.SingleSignon _
            AndAlso mLogonOptions.MappedActiveDirectoryAuthenticationEnabled _
            AndAlso gSv.CurrentWindowsUserIsMappedToABluePrismUser() Then
            gpActiveDirectory.Show()
        Else
            gpActiveDirectory.Hide()
        End If
    End Sub

#End Region

    Private Sub btnDirectLogin_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDirectLogin.Click
        If txtUsername.Text = "" Then
            UserMessage.Show(My.Resources.ctlLogin_YouMustEnterAUsername)
            txtUsername.Focus()
            Exit Sub
        End If
        Login()
    End Sub

    Private Sub btnSignInUsingActiveDirectory_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSignInUsingActiveDirectory.Click

        Dim loginMethod As Func(Of String, String, Auth.LoginResult) =
            Function(resource, locale) User.LoginWithMappedActiveDirectoryUser(resource, locale)

        mParent.AttemptLogin(CultureInfo.CurrentUICulture.Name, loginMethod)
    End Sub

    Private Async Sub SignInUsingExternalLoginBrowser(authenticationServerUrl As String)
        Try
            DisplayConnectingToExternalProvider(True)
            BrowserFormFactory.InitializeChromiumEmbeddedFramework()
            Dim browser = New BrowserStartup(authenticationServerUrl)
            Dim externalAuthenticationResult = Await browser.LoginWithDisplayVisible()

            Dim cookieManager As ICookieManager = New CefCookieManager()
            Await cookieManager.DeleteIdentityCookieFromBrowser()

            If Not externalAuthenticationResult.IsError Then
                Dim loginMethod As Func(Of String, String, LoginResult) =
                        Function(resource, locale)
                            Return User.LoginWithAccessToken(resource,
                                                             externalAuthenticationResult.AccessToken,
                                                             locale)
                        End Function
                mParent.AttemptLogin(CultureInfo.CurrentUICulture.Name, loginMethod)
            Else
                Select Case externalAuthenticationResult.[Error]
                    Case IdentityModel.OidcClient.Browser.BrowserResultType.UnknownError.ToString()
                        UserMessage.Err(My.Resources.ctlExternalLogin_UnexpectedBrowserError)
                    Case IdentityModel.OidcClient.Browser.BrowserResultType.HttpError.ToString()
                        UserMessage.Err(My.Resources.ctlExternalLogin_LoginFailed)
                    Case IdentityModel.OidcClient.Browser.BrowserResultType.Timeout.ToString()
                        UserMessage.Err(My.Resources.ctlExternalLogin_ExternalProviderTimeout)
                End Select

            End If
        Catch ex As Exception
            UserMessage.Err(My.Resources.ctlExternalLogin_UnexpectedBrowserError)
        Finally
            DisplayConnectingToExternalProvider(False)
        End Try
    End Sub

    Private Sub btnSSOLogin_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSSOLogin.Click
        mParent.PerformSingleSignOn(CultureInfo.CurrentUICulture.Name)
    End Sub

    Private Sub ChangeLocale(locale As String)
        If ParentAppForm IsNot Nothing AndAlso locale IsNot Nothing Then
            ParentAppForm.PreviousUsername = txtUsername.Text
            ParentAppForm.PreviousPassword = txtPassword.SecurePassword
            ParentAppForm.SelectedLocale = locale
        End If
    End Sub

    Public Sub SetUsernameAndPassword(username As String, password As SafeString)
        txtUsername.Text = username
        txtPassword.SecurePassword = password
    End Sub

    Private Sub DisplayConnectingToExternalProvider(ByVal state As Boolean)
        txtUsername.Enabled = Not state
        txtPassword.Enabled = Not state
        btnDirectLogin.Enabled = Not state
        btnSignInUsingActiveDirectory.Enabled = Not state
        cmbConnection.Enabled = Not state

        llRefresh.Enabled = Not state
        llConfigure.Enabled = Not state
        llLocale.Enabled = Not state
        Panel2.Enabled = state
        Panel2.Visible = state

        If state Then
            Dim controlIndex = Panel2.Controls.IndexOfKey("LblAuthenticationServerConnecting")
            If controlIndex >= 0 Then
                Dim connectLabel = CType(Panel2.Controls(controlIndex), Label)
                connectLabel.Text = My.Resources.ctlLogin_ConnectingToTheAuthenticationServer
            End If
            Panel2.BringToFront()
        Else
            Panel2.SendToBack()
        End If
    End Sub

    Private Sub BtnAuthenticationServerSignin_Click(sender As Object, e As EventArgs) Handles btnAuthenticationServerSignin.Click
        If mUseAuthenticationServerSignIn Then
            SignInUsingExternalLoginBrowser(mLogonOptions.AuthenticationServerUrl)
        End If
    End Sub

End Class
