
Imports BluePrism.BPCoreLib
Imports BluePrism.Core.Configuration
Imports BluePrism.Core.Utility
''' Project  : Automate
''' Class    : frmAbout
''' 
''' <summary>
''' A form to display the 'About' details of AutomateUI.
''' </summary>
Friend Class frmAbout
    Inherits frmForm

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        lblCopyright.Text = String.Format(lblCopyright.Text, Date.Now.Year)
    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents LNetversion As System.Windows.Forms.Label
    Friend WithEvents lVersion As System.Windows.Forms.Label
    Friend WithEvents lblAppMan As System.Windows.Forms.Label
    Friend WithEvents lblAppManVersion As System.Windows.Forms.Label
    Friend WithEvents lblCopyright As System.Windows.Forms.Label
    Friend WithEvents lblTrademarks As Label

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmAbout))
        Me.lblTrademarks = New System.Windows.Forms.Label()
        Me.lblCopyright = New System.Windows.Forms.Label()
        Me.lblAppManVersion = New System.Windows.Forms.Label()
        Me.lblAppMan = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.LNetversion = New System.Windows.Forms.Label()
        Me.lVersion = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'lblTrademarks
        '
        resources.ApplyResources(Me.lblTrademarks, "lblTrademarks")
        Me.lblTrademarks.BackColor = System.Drawing.Color.Transparent
        Me.lblTrademarks.ForeColor = System.Drawing.Color.White
        Me.lblTrademarks.Name = "lblTrademarks"
        '
        'lblCopyright
        '
        resources.ApplyResources(Me.lblCopyright, "lblCopyright")
        Me.lblCopyright.BackColor = System.Drawing.Color.Transparent
        Me.lblCopyright.ForeColor = System.Drawing.Color.FromArgb(CType(CType(115, Byte), Integer), CType(CType(184, Byte), Integer), CType(CType(230, Byte), Integer))
        Me.lblCopyright.Name = "lblCopyright"
        '
        'lblAppManVersion
        '
        Me.lblAppManVersion.BackColor = System.Drawing.Color.Transparent
        resources.ApplyResources(Me.lblAppManVersion, "lblAppManVersion")
        Me.lblAppManVersion.ForeColor = System.Drawing.Color.White
        Me.lblAppManVersion.Name = "lblAppManVersion"
        '
        'lblAppMan
        '
        Me.lblAppMan.BackColor = System.Drawing.Color.Transparent
        resources.ApplyResources(Me.lblAppMan, "lblAppMan")
        Me.lblAppMan.ForeColor = System.Drawing.Color.White
        Me.lblAppMan.Name = "lblAppMan"
        '
        'Label5
        '
        Me.Label5.BackColor = System.Drawing.Color.Transparent
        resources.ApplyResources(Me.Label5, "Label5")
        Me.Label5.ForeColor = System.Drawing.Color.White
        Me.Label5.Name = "Label5"
        '
        'LNetversion
        '
        Me.LNetversion.BackColor = System.Drawing.Color.Transparent
        resources.ApplyResources(Me.LNetversion, "LNetversion")
        Me.LNetversion.ForeColor = System.Drawing.Color.White
        Me.LNetversion.Name = "LNetversion"
        '
        'lVersion
        '
        Me.lVersion.BackColor = System.Drawing.Color.Transparent
        resources.ApplyResources(Me.lVersion, "lVersion")
        Me.lVersion.ForeColor = System.Drawing.Color.White
        Me.lVersion.Name = "lVersion"
        '
        'frmAbout
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.lblTrademarks)
        Me.Controls.Add(Me.lblCopyright)
        Me.Controls.Add(Me.lblAppManVersion)
        Me.Controls.Add(Me.lblAppMan)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.LNetversion)
        Me.Controls.Add(Me.lVersion)
        Me.DoubleBuffered = True
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmAbout"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

    'If you're confused about why there's no permission property here
    'then see frmapplication.checkuserroles() - this form is on a whitelist

    ''' <summary>
    ''' Handles the forms load event and puts the version number in the version label
    ''' </summary>
    ''' <param name="sender">The object that send the event</param>
    ''' <param name="e">The eventhandler for the event</param>
    Private Sub frmAbout_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        'Display different image for unbranded installations
        If IO.File.Exists("unbranded") Then
            Me.BackgroundImage = LogoImages.aboutn
        Else
            Me.BackgroundImage = LogoImages.about
        End If

        'This isn't supposed to be translated, please don't externalise.
        lblTrademarks.Text = Chr(34) & "Blue Prism" & Chr(34) & ", the " & Chr(34) & "Blue Prism" & Chr(34) & " logo and Prism device are either trademarks or registered trademarks of Blue Prism Limited." & vbCrLf & "All Rights Reserved."


        lVersion.Text = Me.GetBluePrismVersion()
        LNetversion.Text = String.Format($"{BPUtil.GetDotNetVersion()} {If(SecurityPolicy.EnforceFIPSCompliance, "(FIPS)", "")}")

        'Set AMI version, if an installation of AMI exists
        Dim AN As System.Reflection.Assembly = System.Reflection.Assembly.Load("AMI")
        If Not ((AN Is Nothing) OrElse (AN.GetName.Version Is Nothing)) Then
            lblAppManVersion.Text = AN.GetName.Version.ToString
        Else
            lblAppMan.Visible = False
            lblAppManVersion.Visible = False
        End If

    End Sub

#Region "Event Handlers"
    Private Sub lVersion_DoubleClick(sender As Object, e As EventArgs) Handles lVersion.DoubleClick
        Clipboard.SetText(lVersion.Text)
    End Sub
#End Region

End Class
