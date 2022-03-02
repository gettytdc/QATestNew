Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Resources

''' Project  : Automate
''' Class    : ctlLogin
''' 
''' <summary>
''' The log in control.
''' </summary>
Friend Class ctlSingleSignon
    Inherits System.Windows.Forms.UserControl
    Implements IHelp
    Implements IPermission
    Implements IChild

    Private mSigninResult As LoginResult

    Public Sub New()
        MyBase.New()
        InitializeComponent()
    End Sub

    ''' <summary>
    ''' Handles this control being loaded.
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

    End Sub


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
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents pbLogo As System.Windows.Forms.PictureBox
    Friend WithEvents PictureBox2 As System.Windows.Forms.PictureBox
    Friend WithEvents PictureBox3 As System.Windows.Forms.PictureBox
    Friend WithEvents ProgressBar1 As System.Windows.Forms.ProgressBar
    Friend WithEvents lblCancelNotes As System.Windows.Forms.Label
    Friend WithEvents BackgroundWorker As System.ComponentModel.BackgroundWorker
    Friend WithEvents llCancelSignin As System.Windows.Forms.LinkLabel

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlSingleSignon))
        Me.Label6 = New System.Windows.Forms.Label()
        Me.pbLogo = New System.Windows.Forms.PictureBox()
        Me.PictureBox2 = New System.Windows.Forms.PictureBox()
        Me.PictureBox3 = New System.Windows.Forms.PictureBox()
        Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
        Me.lblCancelNotes = New System.Windows.Forms.Label()
        Me.llCancelSignin = New System.Windows.Forms.LinkLabel()
        Me.BackgroundWorker = New System.ComponentModel.BackgroundWorker()
        CType(Me.pbLogo, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox3, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'Label6
        '
        resources.ApplyResources(Me.Label6, "Label6")
        Me.Label6.BackColor = System.Drawing.Color.White
        Me.Label6.ForeColor = System.Drawing.SystemColors.ControlDarkDark
        Me.Label6.Name = "Label6"
        '
        'pbLogo
        '
        resources.ApplyResources(Me.pbLogo, "pbLogo")
        Me.pbLogo.Name = "pbLogo"
        Me.pbLogo.TabStop = False
        '
        'PictureBox2
        '
        resources.ApplyResources(Me.PictureBox2, "PictureBox2")
        Me.PictureBox2.Name = "PictureBox2"
        Me.PictureBox2.TabStop = False
        '
        'PictureBox3
        '
        resources.ApplyResources(Me.PictureBox3, "PictureBox3")
        Me.PictureBox3.Name = "PictureBox3"
        Me.PictureBox3.TabStop = False
        '
        'ProgressBar1
        '
        resources.ApplyResources(Me.ProgressBar1, "ProgressBar1")
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee
        '
        'lblCancelNotes
        '
        resources.ApplyResources(Me.lblCancelNotes, "lblCancelNotes")
        Me.lblCancelNotes.BackColor = System.Drawing.Color.White
        Me.lblCancelNotes.ForeColor = System.Drawing.SystemColors.ControlDarkDark
        Me.lblCancelNotes.Name = "lblCancelNotes"
        '
        'llCancelSignin
        '
        resources.ApplyResources(Me.llCancelSignin, "llCancelSignin")
        Me.llCancelSignin.BackColor = System.Drawing.Color.White
        Me.llCancelSignin.ForeColor = System.Drawing.Color.SteelBlue
        Me.llCancelSignin.LinkColor = System.Drawing.Color.SteelBlue
        Me.llCancelSignin.Name = "llCancelSignin"
        Me.llCancelSignin.TabStop = True
        '
        'BackgroundWorker
        '
        Me.BackgroundWorker.WorkerSupportsCancellation = True
        '
        'ctlSingleSignon
        '
        Me.BackColor = System.Drawing.Color.White
        Me.Controls.Add(Me.lblCancelNotes)
        Me.Controls.Add(Me.llCancelSignin)
        Me.Controls.Add(Me.ProgressBar1)
        Me.Controls.Add(Me.PictureBox3)
        Me.Controls.Add(Me.PictureBox2)
        Me.Controls.Add(Me.pbLogo)
        Me.Controls.Add(Me.Label6)
        Me.Name = "ctlSingleSignon"
        resources.ApplyResources(Me, "$this")
        CType(Me.pbLogo, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox3, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region


    Private mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(ByVal value As frmApplication)
            mParent = value
        End Set
    End Property

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "helpSingleSignon.htm"
    End Function

    Private Sub ctlSingleSignon_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        BackgroundWorker.RunWorkerAsync()
    End Sub

    Public ReadOnly Property RequiredPermissions As ICollection(Of Permission) _
        Implements BluePrism.AutomateAppCore.Auth.IPermission.RequiredPermissions
        Get
            Return Permission.None
        End Get
    End Property

    Private Sub BackgroundWorker_DoWork(ByVal sender As Object, ByVal e As DoWorkEventArgs) Handles BackgroundWorker.DoWork
        mSigninResult = User.Login(ResourceMachine.GetName(), mParent.SelectedLocale)
    End Sub

    ''' <summary>
    ''' Handles the background worker completing.
    ''' Any sort of error will cause the application to exit after being reported.
    ''' </summary>
    Private Sub BackgroundWorker_RunWorkerCompleted(
        ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs) _
        Handles BackgroundWorker.RunWorkerCompleted

        If e.Error IsNot Nothing Then
            UserMessage.Err(e.Error, GetSignonFailedUserMessage(e.Error.Message))
            mParent.Close()
        Else
            If Not mSigninResult.IsSuccess Then
                UserMessage.Show(GetSignonFailedUserMessage(mSigninResult.Description))
                mParent.Close()
            Else
                If Not mParent.PerformPostLoginActions(mParent.SelectedLocale) Then
                    mParent.Close()
                End If
            End If
        End If
    End Sub

    ''' <summary>
    ''' Return a message to display to the user when the signon fails.
    ''' </summary>
    Private Function GetSignonFailedUserMessage(reason As String) As String
        Return String.Format(My.Resources.ctlSingleSignon_SignonFailed0BluePrismWillNowExitContactYourNetworkAdministratorToResolveTheseC, reason)
    End Function

    Private Sub llCancelSignin_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles llCancelSignin.LinkClicked
        BackgroundWorker.CancelAsync()
        mParent.Close()
    End Sub
End Class
