Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth.Permission
Imports BluePrism.BPCoreLib
Imports BluePrism.Images
Imports BluePrism.Server.Domain.Models
Imports NLog

''' <summary>
''' A control displaying the Auotmate welcome screen.
''' </summary>
Friend Class ctlWelcome
    Inherits UserControl
    Implements IHelp, IChild, IEnvironmentColourManager

    ''' <summary>
    ''' Creates a new, empty welcome control
    ''' </summary>
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
             Me.Width - pbLogo.Width - Branding.LargeLogoMarginRight,
             Me.Height - pbLogo.Height - Branding.LargeLogoMarginBottom)
        End If

        LoadHomePage()
        lblAreaTitle.Text = gSv.GetPref(PreferenceNames.Env.EnvironmentName, My.Resources.Home)

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

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents lSystemManager As System.Windows.Forms.Label
    Friend WithEvents lblLicenseInfo As System.Windows.Forms.Label
    Friend WithEvents pbLogo As System.Windows.Forms.PictureBox
    Friend WithEvents ElementHost1 As System.Windows.Forms.Integration.ElementHost
    Friend WithEvents tRefresh As System.Windows.Forms.Timer
    Friend tileView As AutomateUI.ctlTileView
    Friend WithEvents lblAreaTitle As System.Windows.Forms.Label
    Friend WithEvents pbLicenceImage As System.Windows.Forms.PictureBox
    Private WithEvents lblLicenceWarning As System.Windows.Forms.Label
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlWelcome))
        Me.lblLicenseInfo = New System.Windows.Forms.Label()
        Me.pbLogo = New System.Windows.Forms.PictureBox()
        Me.lblLicenceWarning = New System.Windows.Forms.Label()
        Me.ElementHost1 = New System.Windows.Forms.Integration.ElementHost()
        Me.tileView = New AutomateUI.ctlTileView()
        Me.tRefresh = New System.Windows.Forms.Timer(Me.components)
        Me.lblAreaTitle = New System.Windows.Forms.Label()
        Me.pbLicenceImage = New System.Windows.Forms.PictureBox()
        CType(Me.pbLogo, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.pbLicenceImage, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lblLicenseInfo
        '
        resources.ApplyResources(Me.lblLicenseInfo, "lblLicenseInfo")
        Me.lblLicenseInfo.ForeColor = System.Drawing.SystemColors.ControlDarkDark
        Me.lblLicenseInfo.Name = "lblLicenseInfo"
        '
        'pbLogo
        '
        resources.ApplyResources(Me.pbLogo, "pbLogo")
        Me.pbLogo.Name = "pbLogo"
        Me.pbLogo.TabStop = False
        '
        'lblLicenceWarning
        '
        resources.ApplyResources(Me.lblLicenceWarning, "lblLicenceWarning")
        Me.lblLicenceWarning.CausesValidation = False
        Me.lblLicenceWarning.ForeColor = System.Drawing.Color.DarkRed
        Me.lblLicenceWarning.Name = "lblLicenceWarning"
        Me.lblLicenceWarning.UseMnemonic = False
        '
        'ElementHost1
        '
        resources.ApplyResources(Me.ElementHost1, "ElementHost1")
        Me.ElementHost1.Name = "ElementHost1"
        Me.ElementHost1.Child = Me.tileView
        '
        'tRefresh
        '
        Me.tRefresh.Interval = 60000
        '
        'lblAreaTitle
        '
        resources.ApplyResources(Me.lblAreaTitle, "lblAreaTitle")
        Me.lblAreaTitle.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(114, Byte), Integer), CType(CType(198, Byte), Integer))
        Me.lblAreaTitle.ForeColor = System.Drawing.Color.White
        Me.lblAreaTitle.Name = "lblAreaTitle"
        '
        'pbLicenceImage
        '
        resources.ApplyResources(Me.pbLicenceImage, "pbLicenceImage")
        Me.pbLicenceImage.Name = "pbLicenceImage"
        Me.pbLicenceImage.TabStop = False
        '
        'ctlWelcome
        '
        Me.Controls.Add(Me.pbLicenceImage)
        Me.Controls.Add(Me.lblAreaTitle)
        Me.Controls.Add(Me.ElementHost1)
        Me.Controls.Add(Me.lblLicenceWarning)
        Me.Controls.Add(Me.pbLogo)
        Me.Controls.Add(Me.lblLicenseInfo)
        Me.Name = "ctlWelcome"
        resources.ApplyResources(Me, "$this")
        CType(Me.pbLogo, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.pbLicenceImage, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            ' Special case for ctlWelcome - it exists after the control is removed (and
            ' thus after the parent is set to null) and the reference is needed for the
            ' task pane to be able to function. Thus, SetParent(Nothing) should be
            ' ignored for this control
            If value IsNot Nothing Then mParent = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the control's help file name.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "frmWelcome.htm"
    End Function

    'Dashboard tiles
    Private mTiles As List(Of DashboardTile)

    Private Sub LoadHomePage()
        Dim loadGraphsAsync As Boolean = True
        Try
            loadGraphsAsync = gSv.GetPref(PreferenceNames.UI.HomePageLoadGraphsAsync, True)
        Catch ex As Exception
            LogManager.GetCurrentClassLogger().Warn(ex, "Unable to get homepage.loadgraphsasync pref from server")
        End Try

        If loadGraphsAsync Then
            Task.Run(Sub() DrawGraphs())
        Else
            DrawGraphs()
        End If
    End Sub

    Private Sub DrawGraphs()
        'If user can set their own welcome page content then retrieve their preferred
        'dashboard, otherwise the global default dashbourd will be shown (guid.empty)
        Dim defaultDashboardID As Guid = Guid.Empty
        If Auth.User.Current.HasPermission(Analytics.ViewDashboards) OrElse
Auth.User.Current.HasPermission(Analytics.DesignPersonalDashboards) OrElse
Auth.User.Current.HasPermission(Analytics.DesignGlobalDashboards) Then
            defaultDashboardID = gSv.GetPref(PreferenceNames.UI.DefaultDashboard, Guid.Empty)
        End If

        Try
            mTiles = gSv.GetDashboardTiles(defaultDashboardID)
        Catch nse As NoSuchElementException
            mTiles = gSv.GetDashboardTiles(Guid.Empty)
        Catch ex As Exception
            Invoke(Sub() UserMessage.Show(String.Format(My.Resources.ctlWelcome_FailedToLoadDashboard0, ex.Message), ex))
        End Try

        If mTiles Is Nothing Then
            Return
        End If

        Dim tileList As New Dictionary(Of Guid, Size)
        For Each tile As DashboardTile In mTiles
            tileList.Add(tile.Tile.ID, tile.Size)
        Next
        Invoke(Sub() tileView.InitForView(tileList))

        Dim now As Date = Date.Now
        For Each tile As DashboardTile In mTiles
            Invoke(Sub() RefreshTile(tile, now))
        Next
        Invoke(Sub() tRefresh.Start())
    End Sub

    Private Sub RefreshTile(tile As DashboardTile, now As Date)
        Dim chart As New ChartTile(tile.Tile)
        tileView.RefreshTile(tile.Tile.ID, chart.Build(tile.Size))
        tile.LastRefreshed = now
    End Sub

    Private Sub tRefresh_Tick(sender As Object, e As EventArgs) Handles tRefresh.Tick
        Dim now As Date = Date.Now
        Dim tileList As New Dictionary(Of Guid, Size)
        For Each tile As DashboardTile In mTiles
            If tile.Tile.RefreshInterval > 0 Then
                If tile.LastRefreshed = Date.MinValue OrElse
                 tile.LastRefreshed.AddSeconds(tile.Tile.RefreshInterval) <= now Then
                    RefreshTile(tile, now)
                End If
            End If
        Next
    End Sub

    ''' <summary>
    ''' Handles the welcome control being loaded. Primarily handles the licence
    ''' information being written to the screen.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ctlWelcome_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
        Dim auth = Licensing.License

        'Set current license 
        If Not String.IsNullOrWhiteSpace(auth.LicenseOwner) Then
            lblLicenseInfo.Text = String.Format(My.Resources.ctlWelcome_LicensedTo0, auth.LicenseOwner)
        End If

        'Display any license info/warning messages
        Dim warning As Boolean
        lblLicenceWarning.Text = auth.GetLicenseChangeMessage(warning)
        If lblLicenceWarning.Text = String.Empty And Not auth.IsLicensed Then
            lblLicenseInfo.Text = My.Resources.ctlWelcome_UnlicensedPleaseEnterALicenseKey
            Return
        ElseIf lblLicenceWarning.Text = String.Empty Then
            Return
        ElseIf warning Then
            lblLicenceWarning.ForeColor = Color.DarkRed
            pbLicenceImage.Image = ToolImages.Warning_16x16
        Else
            lblLicenceWarning.ForeColor = Color.DarkBlue
            pbLicenceImage.Image = ToolImages.Information_16x16
        End If

    End Sub

#Region "IEnvironmentColourManager implementation"

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentBackColor As Color _
     Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return lblAreaTitle.BackColor
        End Get
        Set(value As Color)
            lblAreaTitle.BackColor = value
        End Set
    End Property

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentForeColor As Color _
     Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return lblAreaTitle.ForeColor
        End Get
        Set(value As Color)
            lblAreaTitle.ForeColor = value
        End Set
    End Property

#End Region

End Class

