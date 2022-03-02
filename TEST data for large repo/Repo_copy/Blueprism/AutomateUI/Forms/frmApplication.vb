Imports System.Globalization
Imports System.IO
Imports System.Runtime.Remoting.Channels
Imports System.Threading

Imports AutomateControls.Forms

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections

Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Config
Imports BluePrism.AMI
Imports BluePrism.Images
Imports BluePrism.AutomateAppCore.Utility
Imports AutomateControls
Imports BluePrism.ApplicationManager.WindowSpy

Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.Common.Security
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.AutomateAppCore.Logging
Imports System.Security.Cryptography
Imports Autofac
Imports AutomateControls.Textboxes
Imports Form = System.Windows.Forms.Form
Imports BluePrism.BPCoreLib.DependencyInjection
Imports AutomateControls.UIState
Imports LocaleTools
Imports BluePrism.Server.Domain.Models

''' Project  : Automate
''' Class    : frmApplication
''' 
''' <summary>
''' The top level Automate application form.
''' </summary>
Public Class frmApplication
    Inherits HelpButtonForm : Implements IEnvironmentColourManager, IRefreshable

#Region " Class-scope Declarations "

    ''' <summary>
    ''' The names of the task panel items created in the task panel by this form,
    ''' just so a typo isn't introduced by using literals throughout the code.
    ''' </summary>
    Private Class TaskNames
        Public Home As String = My.Resources.Home
        Public Studio As String = My.Resources.Studio
        Public Control As String = My.Resources.xControl
        Public Dashboard As String = My.Resources.Analytics
        Public Releases As String = My.Resources.Releases
        Public System As String = My.Resources.System
    End Class

    ''' <summary>
    ''' The set of types which everyone can use, regardless of their user roles /
    ''' permissions. This is just a premature optimisation, I guess. Really, these
    ''' types shouldn't implement IPermission, to indicate that they are not
    ''' permission-constrained... ctlWelcome is already like this.
    ''' </summary>
    Private Shared UniversalPages As ICollection(Of Type) =
     GetReadOnly.ICollection(Of Type)(New HashSet(Of Type)({
      GetType(ctlLogin),
      GetType(ctlSingleSignon),
      GetType(ctlWelcome),
      GetType(frmAbout),
      GetType(frmErrorMessage)
     }
    ))

    Public Shared KeepProcessAlertsRunning As Boolean? = Nothing

    ''' <summary>
    ''' Gets the current open application form, or null if there is no such form
    ''' open and belonging to this application.
    ''' </summary>
    ''' <returns>The application form currently running within this application.
    ''' </returns>
    Public Shared Function GetCurrent() As frmApplication
        For Each f As Form In Application.OpenForms
            Dim appForm = TryCast(f, frmApplication)
            If appForm IsNot Nothing Then Return appForm
        Next
        Return Nothing
    End Function

#End Region

#Region " Events "

    ''' <summary>
    ''' Event fired when an import has been completed from within this form.
    ''' </summary>
    Public Event ImportCompleted As EventHandler

    ''' <summary>
    ''' Event fired when the page held in the application is changing.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="args">The arguments detailing the page change</param>
    Public Event PageChanging(ByVal sender As Object, ByVal args As PageChangeEventArgs)

    ''' <summary>
    ''' Event fires when the connection that the application is using is changed.
    ''' The new connection can be retrieved from the <see cref="Options"/>
    ''' class.
    ''' </summary>
    ''' <param name="sender">The source of the connection change - the application
    ''' form which processed it.</param>
    ''' <param name="e">The args not detailing the connection change</param>
    ''' <seealso cref="Options.DBConnectionSetting"/>
    Public Event ConnectionChanged(ByVal sender As Object, ByVal e As DatabaseChangeEventArgs)

#End Region

#Region " Member Variables "

    ''' <summary>
    ''' This object is the pointer to the current visible page.
    ''' </summary>
    Private mCurrPage As Control

    Private WithEvents mReferenceViewer As frmReferenceViewer

    ' The worker object for handling the connection config form
    Private WithEvents mConnConfigWorker As BackgroundWorker

    Private mArchiver As clsArchiver

    Public AlertEngine As clsAlertEngine

    ''' <summary>
    ''' This can be one of "Sign out" or "Exit"
    ''' </summary>
    Private msSignOffOption As String

    ''' <summary>
    ''' Timer to keep the server status up to date
    ''' </summary>
    Private mServerStatusUpdater As Threading.Timer

    ''' <summary>
    ''' We need to keep track of which forms are open so that when we do shut down
    ''' we can query each form to see if shutdown is possible.
    ''' </summary>
    Private mOpenForms As List(Of Form)

    ' The active queue manager held by this application form
    Private mQueueManager As ActiveQueueManager

    ' The object which keeps track of connections to resources
    Private mConnectionManager As IResourceConnectionManager

    ' The store which can be used to access data on groups, trees and their contents
    Private mGroupStore As IGroupStore

    Private ReadOnly mRunnerFactory As New ResourceRunnerFactory

    Private mCanCreateProcessInDefaultGroup As Boolean
    Private mCanCreateObjectInDefaultGroup As Boolean

    Private mPageChanging As Boolean
    Private mLocale As String

    Private mStatusUpdateActive As Boolean
    Private ReadOnly mStatusLock As Object = New Object


#End Region

#Region " Auto-Properties "

    Public Property SelectedLocale As String
        Get
            Return mLocale
        End Get
        Set
            If Value <> mLocale Then
                mLocale = Value
                RefreshLocale(mLocale)
            End If
        End Set
    End Property

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Constructor.
    ''' </summary>
    Public Sub New()
        MyBase.New()

        Dim configOptions = Options.Instance
        If configOptions.CurrentLocale IsNot Nothing AndAlso configOptions.CurrentLocale <> configOptions.SystemLocale Then
            System.Threading.Thread.CurrentThread.CurrentUICulture = New Globalization.CultureInfo(configOptions.CurrentLocale)
            System.Threading.Thread.CurrentThread.CurrentCulture = New Globalization.CultureInfo(configOptions.CurrentLocale)
        End If

        'This call is required by the Windows Form Designer.
        InitializeComponent()
        'Add any initialization after the InitializeComponent() call

        UpdateGlobalAppProperties()

        mConnConfigWorker = New BackgroundWorker()

        Dim bounds As Rectangle = Screen.GetWorkingArea(Me)
        If bounds.Height < Me.Height Then
            Me.Height = bounds.Height
        End If

        mOpenForms = New List(Of Form)

        Me.lblStatus.Width = Me.Width - 50
        Me.lblStatus.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText

        ExportNewReleaseToolStripMenuItem.Enabled = Licensing.License.CanUse(LicenseUse.ReleaseManager)

        AddHandler User.OnCheckIfUserCanLogout, AddressOf CanCurrentUserLogOut

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The group store maintained by this application form.
    ''' This provides a mechanism with which information about trees and their
    ''' contents can be retrieved and updated.
    ''' </summary>
    Public ReadOnly Property GroupStore As IGroupStore
        Get
            Return mGroupStore
        End Get
    End Property

    ''' <summary>
    ''' The archiving object used to archive and restore session data.
    ''' </summary>
    Public Property Archiver() As clsArchiver
        Get
            Return mArchiver
        End Get
        Set(ByVal value As clsArchiver)
            mArchiver = value
        End Set
    End Property

    Public Property PreviousUsername As String = Nothing

    Public Property PreviousPassword As SafeString = Nothing

    ''' <summary>
    ''' Gets or sets the environment-specific back colour in use in this environment.
    ''' Only set to the database-held values after login.
    ''' </summary>
    ''' <remarks>Note that this only affects the UI owned directly by this form - ie.
    ''' setting the colour here will not update the database</remarks>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentBackColor As Color _
     Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return lblStatus.BackColor
        End Get
        Set(value As Color)
            statusBar.BackColor = value
            lblStatus.BackColor = value
            btnFile.BackColor = value
            btnSignout.BackColor = value
            Dim pg = TryCast(mCurrPage, IEnvironmentColourManager)
            If pg IsNot Nothing Then pg.EnvironmentBackColor = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the environment-specific back colour in use in this environment.
    ''' Only set to the database-held values after login.
    ''' </summary>
    ''' <remarks>Note that this only affects the UI owned directly by this form - ie.
    ''' setting the colour here will not update the database</remarks>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentForeColor As Color _
     Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return lblStatus.ForeColor
        End Get
        Set(value As Color)
            statusBar.ForeColor = value
            lblStatus.ForeColor = value
            btnFile.ForeColor = value
            btnSignout.ForeColor = value
            Dim pg = TryCast(mCurrPage, IEnvironmentColourManager)
            If pg IsNot Nothing Then pg.EnvironmentForeColor = value
        End Set
    End Property

    ''' <summary>
    ''' The connection manager, if there is one.
    ''' 
    ''' This object is only instantiated until after login, because the connections
    ''' it creates uses the user id from the shared clsUser class. At logout time,
    ''' it is destroyed again.
    ''' </summary>
    Public ReadOnly Property ConnectionManager() As IResourceConnectionManager
        Get
            Return mConnectionManager
        End Get
    End Property

    ''' <summary>
    ''' The active queue manager associated with this application form, or null if
    ''' there is none. Really, this is primarily here so that the queue management
    ''' information isn't lost when the queue panel is exited from in Control Room.
    ''' </summary>
    Public ReadOnly Property QueueManager As ActiveQueueManager
        Get
            Return mQueueManager
        End Get
    End Property

    ''' <summary>
    ''' The current user, logged into this environment and using this form, or null
    ''' if no user is logged in.
    ''' </summary>
    Public ReadOnly Property CurrentUser() As User
        Get
            If User.LoggedIn Then Return User.Current Else Return Nothing
        End Get
    End Property

    ''' <summary>
    ''' Returns true if the application form is in the process of changing tab.
    ''' </summary>
    Public ReadOnly Property IsChangingTab As Boolean
        Get
            Return mPageChanging
        End Get
    End Property

    ''' <summary>
    ''' True if the user has chosen to close the top level application form.
    ''' </summary>
    Public Property ApplicationFormClosing As Boolean = False

#End Region

#Region " Event Handlers "

    Private Sub HelpAboutMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles HelpAboutMenuItem.Click
        AboutVersion(Me)
    End Sub

    Private Sub HelpTopicMenuItem_Click() Handles HelpTopicMenuItem.Click
        Dim helpFile As String = DirectCast(mCurrPage, IHelp).GetHelpFile()
        If helpFile = "HELPMISSING" Then
            UserMessage.Show(My.Resources.HelpFileMissing)
        Else
            Try
                OpenHelpFile(Me, helpFile)
            Catch
                UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
            End Try
        End If
    End Sub

    Private Sub HelpOpenMenuItem_Click(sender As Object, e As EventArgs) Handles HelpOpenMenuItem.Click
        Try
            OpenHelpFileHome(Me)
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

    Private Sub AcknowledgementsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AcknowledgementsToolStripMenuItem.Click
        Try
            OpenAcknowledgements()
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

    Private Sub HelpAPIDocumentationMenuItem_Click(sender As Object, e As EventArgs) Handles HelpAPIDocumentationMenuItem.Click
        Dim objBusDef As New frmBusDef
        objBusDef.SetEnvironmentColoursFromAncestor(Me)
        objBusDef.ShowInTaskbar = False
        objBusDef.ShowDialog()
        objBusDef.Dispose()
    End Sub

    ''' <summary>
    ''' Handles the file menu opening, ensuring that it only opens if the source
    ''' control is the File button
    ''' </summary>
    Private Sub mnuFile_Opening(sender As Object, e As CancelEventArgs) Handles mnuFile.Opening
        Dim src As ContextMenuStrip = TryCast(sender, ContextMenuStrip)
        If src Is Nothing OrElse src.SourceControl IsNot btnFile Then e.Cancel = True
    End Sub

    Private Sub OpenProcess(procid As Guid, dep As clsProcessDependency) Handles mReferenceViewer.OpenProcess
        'Handle requests to view processes from the viewer form
        Dim viewer As frmProcess = frmProcess.GetInstance(procid)
        If viewer Is Nothing Then
            viewer = New frmProcess(ProcessViewMode.PreviewProcess, procid, "", "")
            StartForm(viewer)
        End If
        viewer.BringToFront()
        viewer.SelectDependency(dep)
    End Sub

    Private Sub op(sender As Object, e As FormClosedEventArgs) Handles mReferenceViewer.FormClosed
        'Clear down member variable when viewer form closes
        mReferenceViewer = Nothing
    End Sub

    Private Sub frmApplication_Closed(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Closed
        If mCurrPage IsNot Nothing Then
            panMain.Controls.Remove(mCurrPage)
            mCurrPage.Dispose()
        End If

        mCurrPage = Nothing
        ShutDownProcessEngine(False)

        Me.AlertNotifyIcon.Visible = False
        If Not Me.AlertEngine Is Nothing Then
            Me.AlertEngine.Dispose()
            Me.AlertEngine = Nothing
        End If

        If User.LoggedIn AndAlso Not KeepProcessAlertsRunning Then User.Logout()

    End Sub

    ''' <summary>
    ''' The event handler for the New item on the file menu.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub NewToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NewToolStripMenuItem.Click

        If mCanCreateObjectInDefaultGroup OrElse mCanCreateProcessInDefaultGroup Then
            'Switch to Studio tab (and ensure it has actually switched)
            ShowDesignStudio()
            Dim dv = TryCast(mCurrPage, ctlDevelopView)
            If dv Is Nothing Then Return

            'Request creation of object or process
            Dim type As Groups.GroupMemberType = Groups.GroupMemberType.None
            If mCanCreateObjectInDefaultGroup And Not mCanCreateProcessInDefaultGroup Then
                type = Groups.GroupMemberType.Object
            ElseIf mCanCreateProcessInDefaultGroup And Not mCanCreateObjectInDefaultGroup Then
                type = Groups.GroupMemberType.Process
            End If

            Dim item = dv.Create(type, Guid.Empty)
            If item Is Nothing Then Return

            RefreshView()

        Else
            ShowPermissionMessage()
        End If

    End Sub

    ''' <summary>
    ''' The event handler for the Open item on the file menu.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub OpenToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OpenToolStripMenuItem.Click

        Dim perms = Permission.ObjectStudio.ImpliedEditBusinessObject.Union(
            Permission.ProcessStudio.ImpliedEditProcess)
        If User.Current.HasPermission(perms.ToArray()) Then
            ShowDesignStudio()
        Else
            ShowPermissionMessage()
        End If

    End Sub

    Private Sub ExitToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitToolStripMenuItem.Click
        Close()
    End Sub

    Private Sub btnSignout_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSignout.Click
        Logout()
    End Sub

    Private Sub btnFile_Click() Handles btnFile.Click
        mnuFile.Show(btnFile, New Point(0, btnFile.Height))
    End Sub

    Private Sub tcModuleSwitcher_Selected(ByVal sender As Object, ByVal e As TabControlEventArgs) Handles tcModuleSwitcher.Selected
        If mPageChanging Then Return
        mPageChanging = True
        tcModuleSwitcher.Enabled = False
        tcModuleSwitcher.Update()
        Try
            If e.TabPage Is tpWelcome Then
                mTaskPanel.SetSelectedItem(My.Resources.Home)
                ShowPage(GetType(ctlWelcome))

            ElseIf e.TabPage Is tpDesignStudio Then
                If Licensing.License.IsLicensed Then
                    mTaskPanel.SetSelectedItem(My.Resources.Studio)
                    ShowPage(GetType(ctlDevelopView))
                End If

            ElseIf e.TabPage Is tpReleaseManager Then
                If Licensing.License.CanUse(LicenseUse.ReleaseManager) Then
                    mTaskPanel.SetSelectedItem(My.Resources.Releases)
                    ShowPage(GetType(ctlReleaseManager))
                End If

            ElseIf e.TabPage Is tpControlRoom Then
                If Licensing.License.IsLicensed Then
                    mTaskPanel.SetSelectedItem(My.Resources.xControl)
                    ShowPage(GetType(ctlControlRoom))
                Else
                    clsUserInterfaceUtils.ShowOperationDisallowedMessage()
                End If

            ElseIf e.TabPage Is tpReview Then
                If Licensing.License.IsLicensed Then
                    mTaskPanel.SetSelectedItem(My.Resources.Analytics)
                    ShowPage(GetType(ctlDashboard))
                End If

            ElseIf e.TabPage Is tpSystemManager Then
                mTaskPanel.SetSelectedItem(My.Resources.System)
                ShowPage(GetType(ctlSystemManager))

            End If
        Catch ex As Exception
            Throw
        Finally
            mPageChanging = False
            tcModuleSwitcher.Enabled = True
            tcModuleSwitcher.Update()
        End Try

    End Sub

    Private Sub tcModuleSwitcher_Selecting(sender As Object, e As TabControlCancelEventArgs) Handles tcModuleSwitcher.Selecting
        If e.TabPage Is tpDigitalExchange Then
            Dim bpVersion = Me.GetType().Assembly.GetName().Version.ToString().Split("."c)
            Process.Start($"https://digitalexchange.blueprism.com/dx/search?sortOrder=lastApprovalDate_desc&filterCategories=40&filterCategoryTags=172,179&BPAutomate={bpVersion(0)}-{bpVersion(1)}-{bpVersion(2)}")
            e.Cancel = True
            Return
        End If
        If e.TabPage Is tpMyProfile Then
            Dim disablingTabControl As DisablingTabControl = TryCast(sender, DisablingTabControl)
            If disablingTabControl Is Nothing Then Return
            Dim tpMyProfileHorizontalPosition = disablingTabControl.TabItemHorizontalPositions(e.TabPage)
            tpMyProfile.ContextMenuStrip.Show(tpMyProfile, tpMyProfileHorizontalPosition, 0)
            e.Cancel = True
            Return
        End If
        If Not CheckChildAllowsLeaving() Then e.Cancel = True
    End Sub


    Private Sub CanCurrentUserLogOut(sender As Object, args As UserEventArgs)
        If Not CheckChildAllowsLeaving() Then Exit Sub

        Dim logoutSuccessful As Boolean
        CloseForms(logoutSuccessful)

        If Not logoutSuccessful Then
            SetUserArgsAsCannotLogOutWhilstWorking(args)
            Exit Sub
        End If

        If mArchiver IsNot Nothing AndAlso mArchiver.IsBackgroundOperationSetToContinue() Then
            Dim archivingOperation As String = ""
            If mArchiver.Mode = clsArchiver.ArchiverMode.BackgroundArchiving Then archivingOperation = "archive"
            If mArchiver.Mode = clsArchiver.ArchiverMode.BackgroundRestoring Then archivingOperation = "restore"

            If UserMessage.YesNo(
                My.Resources.The0OperationIsStillInProgressAndIs1Complete2DoYouWantToCancelIt,
                archivingOperation, mArchiver.PercentageComplete, vbCrLf) = MsgBoxResult.Yes Then
                mArchiver.Cancel()
            Else
                SetUserArgsAsCannotLogOutWhilstWorking(args)
                Exit Sub
            End If
        End If

        If AlertEngine IsNot Nothing Then
            If AlertEngine.UserIsAlertSubscriber AndAlso
            UserMessage.YesNo(String.Format(
                My.Resources.TheProcessAlertsMonitorIsCurrentlyRunningForUser0DoYouWishToStopItAndContinueTheDatabaseConversion, User.CurrentName)
                ) = MsgBoxResult.No Then
                KeepProcessAlertsRunning = True
                args.LogoutDenied = True
                Exit Sub
            Else
                AlertNotifyIcon.Visible = False
                AlertEngine.Dispose()
                AlertEngine = Nothing
            End If
        End If

        If Not ShutDownProcessEngine(True) Then
            SetUserArgsAsCannotLogOutWhilstWorking(args)
            Exit Sub
        End If
    End Sub

    Private Sub SetUserArgsAsCannotLogOutWhilstWorking(ByRef args As UserEventArgs)
        args.LogoutDenied = True
        args.LogoutDeniedMessage = My.Resources.UnableToLogOutWhileWorkIsInProgress
    End Sub




#Region " Event Handlers "

    ''' <summary>
    ''' The event handler for file exit.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ExitMenuToolStripItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitToolStripMenuItem.Click
        Me.Close()
    End Sub

    Private Sub ImportProcessToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs) Handles ImportProcessObjectToolStripMenuItem.Click
        Dim res As DialogResult

        res = StartForm(New frmImportRelease(frmImportRelease.ImportType.ProcessObject), True)

        If res = DialogResult.OK Then OnImportCompleted(EventArgs.Empty)
    End Sub

    Private Sub ImportReleaseToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs) Handles ImportReleaseSkillToolStripMenuItem.Click
        Dim res As DialogResult = DialogResult.Cancel
        Dim hasPermissionToImportRelease = User.Current.HasPermission(Permission.ReleaseManager.ImportRelease)

        If Licensing.License.CanUse(LicenseUse.ReleaseManager) AndAlso hasPermissionToImportRelease Then
            res = StartForm(New frmImportRelease(), True)
        End If
        If res = DialogResult.OK Then OnImportCompleted(EventArgs.Empty)
    End Sub
    Private Sub frmApplication_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        If e.Alt And e.KeyCode = Keys.F Then
            btnFile_Click()
        End If
        If User.LoggedIn AndAlso e.Control AndAlso e.KeyCode = Keys.I AndAlso User.Current.HasPermission(Permission.ReleaseManager.ImportRelease) Then
            StartForm(New frmImportRelease(frmImportRelease.ImportType.Release), True)
        End If
        If e.KeyCode = Keys.F5 Then
            RefreshView()
        End If
    End Sub

    ''' <summary>
    ''' Refreshes the view.
    ''' </summary>
    Private Sub RefreshView() Implements IRefreshable.RefreshView
        Dim r = TryCast(mCurrPage, IRefreshable)
        If r IsNot Nothing Then
            r.RefreshView()
        End If
    End Sub

    Private Sub frmApplication_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        'Initialise AMI...
        Dim sErr As String = Nothing
        If Not clsAMI.Init(sErr) Then
            UserMessage.Show(String.Format(My.Resources.ApplicationManagerInitialisationError0, sErr))
        End If

        SetTitleAndIcon()

        gAuditingEnabled = True

        With mTaskPanel
            .Top = 1
            .Left = 0
            .Height = Me.ClientSize.Height - Me.lblStatus.Height
        End With

        'Initialise ui settings
        UpdateUserInterfaceForLoggedOut()

    End Sub

    ''' <summary>
    ''' Special handling for ctlLogin we wait for the main application form  to be
    ''' shown then we wait for a connection
    ''' </summary>
    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)

        WaitForInitConnection()
    End Sub

    Private Sub ProcessObjectToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs) Handles ExportProcessObjectToolStripMenuItem.Click
        Try
            Dim bPermissionForObjects, bPermissionForProcesses As Boolean

            If User.Current.HasPermission(Permission.ObjectStudio.ExportBusinessObject) Then
                bPermissionForObjects = True
            End If

            If User.Current.HasPermission(Permission.ProcessStudio.ExportProcess) Then
                bPermissionForProcesses = True
            End If

            If bPermissionForObjects And Not bPermissionForProcesses Then
                Me.StartForm(New frmProcessExport(frmWizard.WizardType.BusinessObject))
            ElseIf bPermissionForProcesses And Not bPermissionForObjects Then
                Me.StartForm(New frmProcessExport(frmWizard.WizardType.Process))
            Else
                Me.StartForm(New frmProcessExport(frmWizard.WizardType.Selection))
            End If

        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.UnableToCompleteExport0, ex.Message))
        End Try
    End Sub

    ''' <summary>
    ''' Handles the menu request to create a new release.
    ''' </summary>
    Private Sub ExportNewReleaseToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs) Handles ExportNewReleaseToolStripMenuItem.Click

        If Not Licensing.License.CanUse(LicenseUse.ReleaseManager) Then
            clsUserInterfaceUtils.ShowOperationDisallowedMessage()
            Return
        ElseIf Not User.Current.HasPermission(
         Permission.ReleaseManager.CreateRelease, Permission.ProcessStudio.ExportProcess, Permission.ObjectStudio.ExportBusinessObject) Then
            UserMessage.Show(My.Resources.YouDonTHaveThePermissionsToCreateANewRelease)
            Return
        End If
        ' Otherwise, we're good to go...
        StartForm(New frmCreateRelease(), True)
    End Sub

    Private Sub ConnectionsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ConnectionsToolStripMenuItem.Click
        ShowDBConfig()
    End Sub

    ''' <summary>
    ''' Handles the background task of opening and handling the connection config
    ''' form - using the automateconfig process
    ''' </summary>
    Private Sub HandleDoWork(ByVal sender As Object, ByVal e As DoWorkEventArgs) _
     Handles mConnConfigWorker.DoWork

        Using proc As New Process()
            With proc.StartInfo
                .FileName = ApplicationProperties.AutomateConfigPath
                .CreateNoWindow = True
            End With
            Try
                proc.Start()
                proc.WaitForExit()
                e.Result = proc.ExitCode
            Catch win32Ex As Win32Exception
                UserMessage.Show(My.Resources.YouRequireAdminPermissionToConfigureTheServer)
            Catch ex As Exception
                UserMessage.Show(My.Resources.Unknown)
            End Try
        End Using

    End Sub

    ''' <summary>
    ''' Handles the connection configuration form being closed - ie. the background
    ''' worker which is running it completing.
    ''' </summary>
    Private Sub HandleConfigClosed(ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs) _
     Handles mConnConfigWorker.RunWorkerCompleted

        ' Check the connections and perform any post-processing, reloading the
        ' connections from clsOptions if necessary
        WaitForInitConnection(CInt(e.Result) = 0)

        ' First thing - ensure that this form is re-enabled
        Enabled = True

        ' Deal with any errors which occurred in the background worker
        If e.Error IsNot Nothing Then UserMessage.Show(
         My.Resources.AnErrorOccurredConfiguringTheConnections, e.Error) : Return

        ' Finally, (try and) make sure we have focus again - Windows seems very
        ' haphazard about how it deals with focus when a process exits.
        Activate()

    End Sub

    ''' <summary>
    ''' Update the status bar every two minutes with the current server details this
    ''' also sets a timestamp in the BPAAliveResources table  to indicate that the
    ''' user is still alive, this is a heartbeat used for checking whether the user
    ''' is still logged in.
    ''' </summary>
    Private Sub UpdateServerStatus(state As Object)
        'If the user is not logged in then there is nothing to do.
        If Not User.LoggedIn Then Return

        Try
            mStatusUpdateActive = True
            Dim configOptions = Options.Instance
            If configOptions.CurrentLocale IsNot Nothing AndAlso configOptions.CurrentLocale <> configOptions.SystemLocale Then '<last-locale> in User.config
                Thread.CurrentThread.CurrentUICulture = New CultureInfo(configOptions.CurrentLocale)
                Thread.CurrentThread.CurrentCulture = New CultureInfo(configOptions.CurrentLocale)
            End If

            Dim connectedTo = gSv.GetConnectedTo()
            InvokeIfNotDisposed(Sub() UpdateStatusBarText(connectedTo))

            gSv.SetKeepAliveTimeStamp()
            If mWarningDisplaying Then
                mWarningDisplaying = False
                InvokeIfNotDisposed(Sub() ClearStatusBarText())
            End If
        Catch ex As UnavailableException
            InvokeIfNotDisposed(Sub() ShowTimeStampErrorMessage(My.Resources.DatabaseServerNotAvailable))
        Catch ex As Exception
            InvokeIfNotDisposed(Sub() ShowTimeStampErrorMessage(ex.Message))
        Finally
            SyncLock mStatusLock
                Monitor.Pulse(mStatusLock)
            End SyncLock
            mStatusUpdateActive = False
        End Try
    End Sub
    Private Sub InvokeIfNotDisposed(method As [Delegate])
        If IsHandleCreated AndAlso Not IsDisposed Then
            Try
                Invoke(method)
            Catch ex As ObjectDisposedException
                'catch a race confition, if the item has been dispoed, while method is executing. We don't care as we check beforehand anyway.
            End Try
        End If
    End Sub
    Private Sub frmApplication_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing

        ApplicationFormClosing = True

        If mArchiver IsNot Nothing AndAlso mArchiver.IsBackgroundOperationInProgress() Then
            Dim opName As String = mArchiver.GetOperationName()
            If Not mArchiver.IsBackgroundOperationSetToContinue() Then
                UserMessage.Show(String.Format(My.Resources.PleaseWaitAMomentWhileThe0OperationIsCancelling, opName))
                e.Cancel = True
                Return
            End If

            Dim res As MsgBoxResult =
             UserMessage.TwoButtonsWithCustomText(
             My.Resources.The0OperationIsStillInProgressAndIs1Complete2DoYouWantToCancelItOrWaitForItToFi, My.Resources.Cancel, My.Resources.Wait,
             opName, mArchiver.PercentageComplete, vbCrLf)

            Dim cancel As Boolean = (res = MsgBoxResult.Yes)
            If cancel AndAlso mArchiver.IsBackgroundOperationInProgress() Then

                AddHandler mArchiver.ArchiveCompleted, AddressOf Archiver_CompleteAndClose
                AddHandler mArchiver.RestoreCompleted, AddressOf Archiver_CompleteAndClose

                mArchiver.Cancel()
                e.Cancel = True

            End If
        End If

        If Not User.LoggedIn Then

            ' we are not logged (signed) in so just close the app
            gAuditingEnabled = False
            CheckContinueAlerts()
            Exit Sub
        End If

        Dim bCanLogOut As Boolean
        Dim sMessage As String = ""

        ' First check that the stubborn child control allows a change
        If Not CheckChildAllowsLeaving() Then
            e.Cancel = True
            Exit Sub
        End If

        CloseForms(bCanLogOut)

        If bCanLogOut Then

            e.Cancel = False
            If mConnectionManager IsNot Nothing Then
                mConnectionManager.Dispose()
                mConnectionManager = Nothing
            End If

            If mQueueManager IsNot Nothing Then
                mQueueManager.Dispose()
                mQueueManager = Nothing
            End If

            CheckContinueAlerts()

        Else
            e.Cancel = True
            sMessage = My.Resources.UnableToExitWhileWorkIsInProgress
        End If

    End Sub

    ''' <summary>
    ''' This sub opens the Fault Reporting help page.
    ''' </summary>
    Public Sub ReportIssue() Handles HelpRequestSupportMenuItem.Click
        Try
            OpenHelpFile(Me, "helpFaultReporting")
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

    ''' <summary>
    ''' Opens My Profile menu.
    ''' </summary>
    Private Sub mnuMyProfile_Opening(sender As Object, e As CancelEventArgs) Handles mnuMyProfile.Opening
        Dim src As ContextMenuStrip = TryCast(sender, ContextMenuStrip)
        If src Is Nothing OrElse src.SourceControl IsNot tpMyProfile Then e.Cancel = True
    End Sub

    ''' <summary>
    ''' Handles click event for opening the Change Password wizard.
    ''' </summary>
    Private Sub ChangePasswordToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChangePasswordToolStripMenuItem.Click
        Dim passwordResetForm As New frmUserPasswordReset()
        passwordResetForm.UserName = User.Current.Name()
        passwordResetForm.ShowInTaskbar = False
        passwordResetForm.ShowDialog()
    End Sub

    ''' <summary>
    ''' Handles click event for signing a user out.
    ''' </summary>
    Private Sub SignOutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SignOutToolStripMenuItem.Click
        Logout()
    End Sub

#End Region

#End Region

#Region " Internal Events / OnXXX Handlers "

    ''' <summary>
    ''' Raises the <see cref="ImportCompleted"/> event
    ''' </summary>
    Protected Overridable Sub OnImportCompleted(e As EventArgs)
        RaiseEvent ImportCompleted(Me, e)
    End Sub

    Protected Overrides Sub OnHelpButtonClicked(ByVal e As CancelEventArgs)
        Try
            mnuHelp.Show(Me, Me.ClientRectangle.Width - mnuHelp.Width - 20, 0)
        Finally
            e.Cancel = True
        End Try
    End Sub

    Protected Overrides Sub OnF1KeyPressed(ByVal e As KeyEventArgs)
        HelpTopicMenuItem_Click()
    End Sub

#End Region

#Region " Other Methods "

    ''' <summary>
    ''' Override the form title and icon based on any branding settings currently in
    ''' force.
    ''' </summary>
    Private Sub SetTitleAndIcon()
        Dim branding = New Branding(Options.Instance)
        Me.Text = branding.GetTitle()
        Dim ii As Icon = branding.GetIcon()
        If ii IsNot Nothing Then
            Me.Icon = ii
        End If
    End Sub

    ''' <summary>
    ''' Attempts to authenticate the current user by single sign-on. The single
    ''' sign-on holding screen is displayed, and the login attempt made
    ''' asynchronously. If single signon is not configured, an error message is
    ''' displayed to the user and nothing further occurs
    ''' </summary>
    Friend Sub PerformSingleSignOn(locale As String)
        Dim configOptions = Options.Instance
        If Not configOptions.DbConnectionSetting.IsComplete Then
            UserMessage.Show(My.Resources.ADatabaseConnectionSettingIsBlankPleaseEditTheConnectionSettings)
        End If

        Try
            ServerFactory.CurrentConnectionValid()
        Catch ex As Exception
            UserMessage.Err(My.Resources.CouldNotConnectToSSODatabase, ex)
            Return
        End Try

        If Not gSv.DatabaseType() = DatabaseType.SingleSignOn Then
            UserMessage.Err(My.Resources.Connection0IsNotASingleSignOnConnection,
             configOptions.CurrentConnectionName)
            Return
        End If

        ConnectionsToolStripMenuItem.Enabled = False
        mLocale = locale

        ShowPage(GetType(ctlSingleSignon))

        If configOptions.CurrentLocale <> configOptions.SystemLocale Then
            System.Threading.Thread.CurrentThread.CurrentUICulture = New Globalization.CultureInfo(locale)
            System.Threading.Thread.CurrentThread.CurrentCulture = New Globalization.CultureInfo(locale)
            configOptions.CurrentLocale = locale
        End If

    End Sub

    ''' <summary>
    ''' Performs the tasks required by this window after the connection config form
    ''' has closed. Optionally reloads all the connections from clsOptions.
    ''' </summary>
    ''' <param name="reload">True to force this window to reload the connection
    ''' config details from clsOptions; False to not do so.</param>
    ''' <param name="initConnection">True to (possibly re-) initialise the current
    ''' connection set in clsOptions. False if the connection settings currently
    ''' set should be left as is. This parameter is ignored if <paramref
    ''' name="reload"/> is set to true - in this case, the connection is always
    ''' re-initialised.</param>
    Private Function PerformPostConnectionConfigChange(
     reload As Boolean, initConnection As Boolean) As DatabaseChangeEventArgs

        Dim configOptions = Options.Instance
        'Possibly new database connections so let's check them
        'and report to status bar:
        If reload Then
            ' Reload the config file
            configOptions.Reload()
            ' Force an initialise-connection if we've reloaded
            initConnection = True
        End If

        If initConnection Then
            Dim setting As clsDBConnectionSetting = configOptions.DbConnectionSetting

            Dim databaseValid As Boolean = False
            Dim complete As Boolean = setting.IsComplete()
            Dim ex As Exception = Nothing
            Dim sErr As String = Nothing

            If complete Then
                If Not BackgroundClientInit(setting) Then Return Nothing
                Try
                    ServerFactory.CurrentConnectionValid()
                    databaseValid = True
                Catch exValid As Exception
                    ex = exValid
                    sErr = exValid.Message
                    databaseValid = False
                End Try
            End If

            ' Build up the change event args based on the current state
            If complete Then
                If databaseValid Then
                    Return New DatabaseChangeEventArgs(True, Nothing,
                     Nothing, My.Resources.Connection0ConnectedTo1, setting.ConnectionName, gSv.GetConnectedTo())
                Else
                    Return New DatabaseChangeEventArgs(False, ex,
                    sErr, My.Resources.CouldNotConnectTo0, setting.ConnectionName)
                End If
            Else
                Return New DatabaseChangeEventArgs(False, ex,
                 sErr, My.Resources.YouMustSpecifyAConnectionBefore0CanFunction,
                 ApplicationProperties.ApplicationName)
            End If
        End If

        ' We can only get here if no changes were made. We still indicate success to
        ' ensure various disabled parts of the UI get re-enabled.
        Return New DatabaseChangeEventArgs(True)
    End Function

    ''' <summary>
    ''' Initialises the client connection in the background allowing the
    ''' backgroundworker to cancel
    ''' </summary>
    ''' <param name="setting">The connection setting to use</param>
    ''' <returns>False if the background worker was cancelled</returns>
    Private Function BackgroundClientInit(setting As clsDBConnectionSetting) As Boolean
        Dim initCompleted = False
        Dim t As New Thread(
            Sub()
                ServerFactory.ClientInit(setting)
                initCompleted = True
            End Sub)
        t.IsBackground = True
        t.Start()

        While Not initCompleted
            Thread.Sleep(500)
            If mConnectionChangeWorker.CancellationPending Then _
                Return False
        End While

        If ServerFactory.ServerAvailable() AndAlso
            Not ServerFactory.ServerManager.Server.CheckSnapshotIsolationIsEnabledInDB() Then
            UserMessage.Show(My.Resources.SnapshotIsolationNotEnabledInDatabase)
            Return False
        End If

        ServerFactory.ServerManager.RegisterHandlers(AddressOf ConnectionPending, AddressOf ConnectionRestored)
        Return True
    End Function

    Private Shared Sub SaveEnvironmentData(certExpiryDateTime As Date?, Optional applicationServerPortNumber As Integer = 0, Optional applicationServerFullyQualifiedDomainName As String = Nothing)
        Try
            gSv.SaveEnvironmentData(New EnvironmentData(EnvironmentType.Client,
                                                        clsUtility.GetFQDN(),
                                                        Nothing,
                                                        Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString,
                                                        Nothing,
                                                        Nothing,
                                                        certExpiryDateTime), applicationServerPortNumber, applicationServerFullyQualifiedDomainName)
        Catch
            'Do nothing, we have probably failed to connect and that will be handled elsewhere
        End Try
    End Sub
    ''' <summary>
    ''' Handle the server manager ConnectionPending event
    ''' </summary>
    Private Sub ConnectionPending()
        ' Ensure we are are on the user interface thread by checking
        ' InvokeRequired() on the active form
        Dim active = Form.ActiveForm
        If active IsNot Nothing AndAlso Not active.InvokeRequired Then
            If Not mShowConnectionPending Then
                mShowConnectionPending = True
                Dim worker As New BackgroundWorker()
                worker.WorkerSupportsCancellation = True
                AddHandler worker.DoWork, Sub() gSv.Nop()

                Dim result = ProgressDialog.Show(active, worker, My.Resources.ConnectingToServer, My.Resources.AttemptingToReEstablishConnectionMax2Mins)
                If result.Cancelled Then
                    mShowConnectionPending = False
                    Throw New UnavailableException(My.Resources.TheServerWasUnavailableAndTheUserCancelledTheReconnect)
                End If

                'Connection will have been restored at this point
                Dim connectedTo = gSv.GetConnectedTo()
                UpdateStatusBarText(connectedTo)
            Else
                Application.DoEvents()
            End If
        Else ' when coming from a background thread
            Thread.Sleep(5000)
        End If
    End Sub

    ''' <summary>
    ''' Boolean to indicate that the connection pending dialog is being shown
    ''' </summary>
    Private mShowConnectionPending As Boolean = False

    ''' <summary>
    ''' Handle the server manager ConnectionRestored event
    ''' </summary>
    Private Sub ConnectionRestored()
        mShowConnectionPending = False
        mConnectionManager?.CheckInstructionalClientStatus()
    End Sub

    ''' <summary>
    ''' Shows the database configuration form
    ''' </summary>
    Public Sub ShowDBConfig()
        If ServerFactory.ServerAvailable() Then
            ServerFactory.Close()
        End If
        ' Check if we can currently write to the config file location - if we can,
        ' then just show the config form within this process
        If Options.Instance.HasWritePrivileges Then
            Try
                Dim res As DialogResult
                Using f As New ConnectionConfigForm()
                    f.ShowInTaskbar = False
                    res = f.ShowDialog()
                End Using
                WaitForInitConnection(res = DialogResult.OK)
            Catch ex As Exception
                UserMessage.Show(My.Resources.AnErrorOccurredConfiguringTheConnections, ex)
            End Try
        Else
            ' Otherwise, we must use the background worker to open the form via
            ' the automateconfig process.
            If mConnConfigWorker.IsBusy Then Throw New BluePrismException(
             My.Resources.ErrorTheConnectionOptionsConfigurationThreadIsBusy)
            Enabled = False ' it is re-enabled when the backgroundworker completes
            mConnConfigWorker.RunWorkerAsync()
        End If
    End Sub

    ''' <summary>
    ''' Raises the <see cref="ConnectionChanged"/> event and updates the UI to
    ''' deal with the new connection.
    ''' </summary>
    Protected Overridable Sub OnConnectionChanged(ByVal e As DatabaseChangeEventArgs)
        ' We can't allow the API Documentation if the DB connection isn't valid
        ' It does horrid things. Simply horrid. (Bug #4486)
        HelpAPIDocumentationMenuItem.Enabled = e.Success AndAlso User.Current IsNot Nothing
        GuidedTourToolStripMenuItem.Enabled = User.Current IsNot Nothing

        If e.ChangesMade Then
            Dim prefix = If(e.Success, "", My.Resources.WARNING)

            SetStatusBarText(prefix & e.ShortMessage)
            UpdateEnvironmentColors(e.Success)
        End If

        RaiseEvent ConnectionChanged(Me, e)
    End Sub

    ''' <summary>
    ''' Gets the current task panel control.
    ''' </summary>
    ''' <returns></returns>
    Public Function GetTaskPanel() As ctlTaskPanel
        Return mTaskPanel
    End Function

    ''' <summary>
    ''' Shows or hides the users menu.
    ''' </summary>
    Public Sub ShowUserMenu(ByVal sender As Object, ByVal e As EventArgs)
        mnuUsers.Show(Me.mTaskPanel, New Point(mTaskPanel.Width, btnFile.Height))
    End Sub

    ''' <summary>
    ''' Clears up the list of users.
    ''' </summary>
    Public Sub ClearUsers()
        mnuUsers.Items.Clear()
    End Sub

    ''' <summary>
    ''' Adds a user to the user menu.
    ''' </summary>
    ''' <param name="name">Name of the user to add</param>
    ''' <param name="img">Image icon for the user</param>
    ''' <param name="handler">Event handler when the user is clicked</param>
    Public Sub AddUser(ByVal name As String, ByVal img As Image, ByVal handler As EventHandler)
        mnuUsers.Items.Add(name, img, handler)
    End Sub

    ''' <summary>
    ''' Initialises this machine as a Resource PC
    ''' </summary>
    Public Sub InitProcessEngine()
        If mResourceRunnerComponents Is Nothing Then
            Dim options = New ResourcePCStartUpOptions() With {
                                               .IsAuto = True,
                                               .Username = GetUserName()}

            mResourceRunnerComponents = mRunnerFactory.Create(options)
            mResourceRunnerComponents.Runner.Init()
            If AlertEngine IsNot Nothing Then
                AlertEngine.StartHandlingStageAlerts()
            Else
                AlertEngine = New clsAlertEngine(Me, User.Current, ResourceMachine.GetName())
                AlertEngine.StartHandlingStageAlerts()
            End If
        End If
    End Sub

    ''' <summary>
    ''' Terminates this machine's Resource PC session and closes the Resource PC form.
    ''' The user is asked for confirmation before proceeding.
    ''' </summary>
    ''' <param name="PromptBeforeShuttingDown">Determines whether the user should
    ''' be prompted before the process engine is shut down. When set to true a
    ''' warning (informing the user that all running sessions will be stopped) will
    ''' appear. The user will be able to choose yes or no.</param>
    ''' <returns>
    ''' Returns true one of the following conditions is met:
    '''     i) The PromptBeforeShuttingDown parameter is set to false;
    '''    ii) the process engine is not already running;
    '''   iii) a. The PromptBeforeShuttingDown parameter is set to true;
    '''        b. the process engine is running;
    '''        c. the user consented to its termination.
    ''' Otherwise returns false.
    ''' 
    ''' Whenever false is returned, the process engine will not have been
    ''' affected.
    ''' </returns>
    Public Function ShutDownProcessEngine(Optional ByVal PromptBeforeShuttingDown As Boolean = True) As Boolean
        If mResourceRunnerComponents Is Nothing Then Return True

        If Not PromptBeforeShuttingDown OrElse UserMessage.YesNo(
         My.Resources.AnyRunningSessionsWillNowBeTerminatedAreYouSureYouWishToProceed) = MsgBoxResult.Yes Then
            mResourceRunnerComponents.Runner.Shutdown()
            mResourceRunnerComponents.Dispose()
            mResourceRunnerComponents = Nothing
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' Public get accessor for mgUserName
    ''' </summary>
    ''' <returns></returns>
    Public Function GetUserName() As String
        If User.LoggedIn Then Return User.Current.Name Else Return Nothing
    End Function

    Public Function StartForm(ByVal frm As Form) As DialogResult
        Return StartForm(frm, False)
    End Function

    Public Function StartForm(ByVal frm As Form, ByVal asDialog As Boolean) As DialogResult
        Return StartForm(frm, asDialog, False)
    End Function

    Public Function StartForm(ByVal frm As Form, ByVal asDialog As Boolean, showUniqueOnly As Boolean) As DialogResult
        If Not CheckUserRoles(frm) Then Return DialogResult.Abort

        Dim child As IChild = TryCast(frm, IChild)
        If child IsNot Nothing Then child.ParentAppForm = Me

        frm.Icon = Icon

        Dim frmColor As IEnvironmentColourManager = TryCast(frm, IEnvironmentColourManager)
        If frmColor IsNot Nothing Then
            frmColor.SetEnvironmentColours(Me)
        End If

        Try
            ' If showing a dialog, there's no need to add to open forms, or activate
            ' it (especially not after it's closed (?!)), or dispose of it - 
            ' ShowDialog() handles all of that, and it won't be open once it returns
            If asDialog Then
                frm.ShowInTaskbar = False
                Return frm.ShowDialog()
            End If

            Dim openForm = mOpenForms.Where(Function(f) f.Name = frm.Name).FirstOrDefault()
            If openForm IsNot Nothing AndAlso showUniqueOnly Then
                openForm.Close()
            End If

            ' Make sure we clean the form up when it's closed
            ' Some form implementations may do this automatically,
            ' but a redundant call can do no harm and missing it means locking up
            AddHandler frm.FormClosed, Sub() ClosedForm(frm)
            mOpenForms.Add(frm)
            frm.Show()
            frm.Activate()

            Return DialogResult.OK

        Catch e As Exception
            UserMessage.Show(
             My.Resources.ThereWasAnErrorWhilstAttemptingToStartTheFormPleaseContactBluePrismForTechnical &
             String.Format(My.Resources.frmApplication_TheErrorMessageWas0, e.Message), e, 1048586)
        End Try
    End Function

    ''' <summary>
    ''' Sets the status bar text to the given value
    ''' </summary>
    ''' <param name="sText">The string with optional format placeholders containing
    ''' the text to display on the status bar.</param>
    ''' <param name="args">The arguments to use in the placeholders of the supplied
    ''' text.</param>
    Private Sub SetStatusBarText(ByVal sText As String, ByVal ParamArray args() As Object)
        Me.lblStatus.Text = String.Format(sText, args)
        Me.lblStatus.Visible = True
    End Sub

    ''' <summary>
    ''' Clears the text on the status bar and hides it.
    ''' </summary>
    Private Sub ClearStatusBarText()
        Me.lblStatus.Text = String.Empty
        Me.lblStatus.Visible = False
    End Sub

    ''' <summary>
    ''' This function is called by StartForm and Showpage, and checks that the user
    ''' has permission to open a particular form or page.
    ''' </summary>
    ''' <param name="frm">The form to check permissions for.</param>
    ''' <returns>True if the user passes any permissions checks inherent in the
    ''' object. False if permission has been denied</returns>
    Private Function CheckUserRoles(ByVal frm As Object) As Boolean

        'pages that everyone can use:
        If UniversalPages.Contains(frm.GetType()) Then Return True

        ' Check if the user has permission to the given form
        Dim u = User.Current
        If Not u.HasPermission(TryCast(frm, IPermission)) Then
            ShowPermissionMessage()
            Return False
        End If

        'check that the user has not been deleted
        Return Not u.Deleted

    End Function

    Friend Shared Sub ShowPermissionMessage()
        ' If it is a permission-constrained form - check the user's permissions, and
        ' ensure that the user hasn't been deleted... while they're logged in (?!)
        UserMessage.Show(String.Format(
         My.Resources.AutomateUI_Controls.ctlDevelopView_CheckPermission_YouDoNotHavePermissionToAccess,
         ApplicationProperties.ApplicationName), 1048586)
    End Sub

    ''' <summary>
    ''' This function is the main callback function that all SDI child pages use to 
    ''' display sibling pages. The sibling pages use this callback function so that
    ''' the child controls are siblings of thier parents and not spawned by the child
    ''' controls themselves. The convention used in the child controls to activate
    ''' this callback is: mobjParent.ShowPage(GetType(ctlFooBar))
    ''' [CG - this whole mechanism needs reworking, as does the code. It doesn't actually
    '''       even work as designed, but that is a good thing because the accidental
    '''       implementation is less buggy than what was intended!]
    ''' </summary>
    ''' <param name="pgType">The type of control that is to be created and made the 
    ''' active page. This type must ultimately extend Control</param>
    Public Sub ShowPage(ByVal pgType As Type)

        ' If the link takes us to a new instance of the current page then we do not
        ' need to do anything.
        If mCurrPage IsNot Nothing AndAlso pgType Is mCurrPage.GetType() Then Return

        ' This may have already been checked before hand but worth checking again to be certain
        If Not CheckChildAllowsLeaving() Then Return

        ' Indicate that the page is changing... cancel the change if listeners 
        ' elect to cancel it.
        Dim evt As New PageChangeEventArgs(mCurrPage, pgType)
        Try
            RaiseEvent PageChanging(Me, evt)
        Catch ex As Exception
            UserMessage.Show(My.Resources.AnErrorOccurredWhileChangingThePage, ex)
            evt.Cancel = True
        End Try
        If evt.Cancel Then Exit Sub

        'Create the page of the given type
        Dim newPage As Control = CType(Activator.CreateInstance(pgType), Control)

        'Check we are allowed to view the new page
        If Not CheckUserRoles(newPage) Then
            newPage.Dispose()
            Exit Sub
        End If

        'Set the properties of the new page
        newPage.Dock = DockStyle.Fill
        Dim colourMgr As IEnvironmentColourManager =
            TryCast(newPage, IEnvironmentColourManager)
        If colourMgr IsNot Nothing Then
            colourMgr.SetEnvironmentColours(Me)
        End If

        panMain.SuspendLayout()
        ' Remove the existing page if one present, ensuring that any child control
        ' is informed of its newfound orphan status
        Dim oldPage As Control = mCurrPage
        If oldPage IsNot Nothing Then
            panMain.Controls.Remove(oldPage)
            oldPage.Dispose()
            CType(oldPage, IChild).ParentAppForm = Nothing
        End If
        CType(newPage, IChild).ParentAppForm = Me

        ' Add the new page to the form
        panMain.Controls.Add(newPage)
        panMain.ResumeLayout(True)

        mCurrPage = newPage
    End Sub

    ''' <summary>
    ''' This function returns the index of the given menu item
    ''' </summary>
    Private Function GetSubMenuItemIndex(ByVal menuItem As MenuItem) As Integer

        Dim objMenu As Menu
        Dim index As Integer

        objMenu = menuItem.Parent
        For index = 0 To objMenu.MenuItems.Count - 1
            If menuItem.Text = objMenu.MenuItems(index).Text Then
                Exit For
            End If
        Next
        GetSubMenuItemIndex = index
    End Function


    ''' <summary>
    ''' Checks if the child of this application allows leaving - on any level, ie.
    ''' leaving the child to show a different page, or leaving the application.
    ''' </summary>
    ''' <returns>True if the child either doesn't care or has allowed leaving,
    ''' False otherwise.</returns>
    Private Function CheckChildAllowsLeaving() As Boolean
        Dim stubbornChild As IStubbornChild = TryCast(mCurrPage, IStubbornChild)
        Return (stubbornChild Is Nothing OrElse stubbornChild.CanLeave())
    End Function

    ''' <summary>
    ''' Loops through all forms opened by this application and trys to close them.
    ''' If they are all closed successfully then bAllFormsClosed is returned as true
    ''' otherwise returned as false
    ''' </summary>
    ''' <param name="bAllFormsClosed"></param>
    Private Sub CloseForms(ByRef bAllFormsClosed As Boolean)
        Dim bLogoutSuccessful As Boolean = True
        Try
            While mOpenForms.Count > 0
                Dim objForm As Form = mOpenForms(0)

                ' Closing will trigger the removal of the form from mOpenForms.
                objForm.Close()

                If Not objForm.IsDisposed Then
                    bLogoutSuccessful = False
                    Exit While
                End If

                ' But just in case (Remove() does no harm if the form is not there)
                mOpenForms.Remove(objForm)

            End While
        Catch ex As Exception
            UserMessage.ShowExceptionMessage(ex)
            bLogoutSuccessful = False
        End Try
        bAllFormsClosed = bLogoutSuccessful
    End Sub

    ''' <summary>
    ''' Called when a child form is closed. Removes the form from the internal list.
    ''' </summary>
    ''' <param name="form">The form that has been closed</param>
    Public Sub ClosedForm(ByVal form As Form)
        For Each objForm As Form In mOpenForms
            If objForm Is form Then
                objForm.Dispose()
                mOpenForms.Remove(objForm)
                Exit For
            End If
        Next
    End Sub

    ''' <summary>
    ''' This function should be called to actually logout the current user. It closes
    ''' all the forms, shuts down various things and then calls
    ''' UpdateUserInterfaceForLoggedOut()
    ''' </summary>
    Public Sub Logout()

        If Not CheckChildAllowsLeaving() Then Exit Sub

        Dim logoutSuccessful As Boolean

        CloseForms(logoutSuccessful)

        If Not logoutSuccessful Then
            UserMessage.Show(My.Resources.UnableToLogOutWhileWorkIsInProgress)
            Exit Sub
        End If

        ' TODO: This Try/Catch block is a bit odd.
        ' 
        Try
            If mArchiver IsNot Nothing AndAlso mArchiver.IsBackgroundOperationSetToContinue() Then
                Dim opName As String = ""
                If mArchiver.Mode = clsArchiver.ArchiverMode.BackgroundArchiving Then opName = "archive"
                If mArchiver.Mode = clsArchiver.ArchiverMode.BackgroundRestoring Then opName = "restore"

                If UserMessage.YesNo(
                 My.Resources.The0OperationIsStillInProgressAndIs1Complete2DoYouWantToCancelIt,
                 opName, mArchiver.PercentageComplete, vbCrLf) = MsgBoxResult.Yes Then

                    mArchiver.Cancel()

                End If
            End If

            If Not AlertEngine Is Nothing Then
                If AlertEngine.UserIsAlertSubscriber AndAlso
                 UserMessage.TwoButtonsWithCustomText(String.Format(
                  My.Resources.TheProcessAlertsMonitorIsCurrentlyRunningForUser011ToStopTheAlertsMonitorAndSig, User.CurrentName, vbCrLf),
                  My.Resources.SignOut, My.Resources.StaySignedIn) = MsgBoxResult.No Then
                    'Keep things running.
                    KeepProcessAlertsRunning = True
                    Me.Close()
                    Exit Sub
                Else
                    AlertNotifyIcon.Visible = False
                    AlertEngine.Dispose()
                    AlertEngine = Nothing
                End If
            End If

            ShutDownProcessEngine(False)
            SaveUserLayoutCache()
            LogOutRequested()
        Catch
        End Try

    End Sub

    Public Sub LogOutRequested()
        User.Logout()
        ShutdownManagers()
        SetTitleAndIcon()
        UpdateUserInterfaceForLoggedOut()
        WaitForInitConnection()
    End Sub

    ''' <summary>
    ''' This function just updates the UI so that the user appears logged out.
    ''' You probably don't want to call this directly and instead call Logout()
    ''' </summary>
    Private Sub UpdateUserInterfaceForLoggedOut()

        'Disable some menus that cannot be used during login.
        NewToolStripMenuItem.Enabled = False
        OpenToolStripMenuItem.Enabled = False
        ImportToolStripMenuItem.Enabled = False
        ExportToolStripMenuItem.Enabled = False
        ConnectionsToolStripMenuItem.Enabled = True

        btnSignout.Enabled = False
        ClearStatusBarText()

        'hide the current page to get rid of flicky effect
        If mCurrPage IsNot Nothing AndAlso mCurrPage.GetType IsNot GetType(ctlLogin) Then mCurrPage.Hide()
        tcModuleSwitcher.Visible = False

        ShowPage(GetType(ctlLogin))
    End Sub

    Private Sub ShutdownManagers()
        If mConnectionManager IsNot Nothing Then
            mConnectionManager.Dispose()
            mConnectionManager = Nothing
        End If

        If mQueueManager IsNot Nothing Then
            mQueueManager.Dispose()
            mQueueManager = Nothing
        End If

        If mGroupStore IsNot Nothing Then
            mGroupStore.Dispose()
            mGroupStore = Nothing
        End If
    End Sub


    ''' <summary>
    ''' This is a callback function called from ctlWelcome
    ''' We set some menu items and buttons enabled after logging in.
    ''' </summary>
    Public Sub AttemptBluePrismLogin(ByRef txtUserName As StyledTextBox,
                                     ByRef txtPassword As ctlAutomateSecurePassword,
                                     locale As String)

        'Check for DB connectivity
        Try
            ServerFactory.CurrentConnectionValid()
        Catch ex As Exception
            UserMessage.ShowExceptionMessage(ex)
            Exit Sub
        End Try

        Try
            Dim resourceName = ResourceMachine.GetName()
            Dim userName = txtUserName.Text
            Dim result As Auth.LoginResult = User.Login(resourceName, userName,
                                                   txtPassword.SecurePassword, locale)

            Select Case result.Code
                Case LoginResultCode.PasswordExpired
                    Using passwordResetForm As New frmUserPasswordReset()
                        passwordResetForm.UserName = txtUserName.Text
                        passwordResetForm.SetEnvironmentColours(Me)
                        passwordResetForm.Expired = True
                        passwordResetForm.txtCurrentPassword.Enabled = False
                        passwordResetForm.CurrentPassword = txtPassword.SecurePassword
                        passwordResetForm.ShowInTaskbar = False
                        passwordResetForm.ShowDialog()
                        If Not passwordResetForm.Saved Then
                            User.Logout()
                            txtPassword.Clear()
                            Exit Sub
                        Else
                            Dim secondLogin = User.Login(resourceName, userName, passwordResetForm.NewPassword, locale)
                            If secondLogin.IsSuccess Then
                                LoadUserLayoutCache()
                            Else
                                UserMessage.Show(String.Format(My.Resources.frmApplication_LoginFailed0, result.Description))
                                txtPassword.Clear()
                                WaitForInitConnection(TimeSpan.FromSeconds(1))
                                Exit Sub
                            End If
                        End If
                    End Using
                Case LoginResultCode.Success
                    LoadUserLayoutCache()
                Case Else
                    UserMessage.Show(String.Format(My.Resources.frmApplication_LoginFailed0, result.Description))
                    txtPassword.Clear()
                    WaitForInitConnection(TimeSpan.FromSeconds(1))
                    Exit Sub
            End Select
        Catch ex As UnknownLoginException
            UserMessage.Err(ex, String.Format(My.Resources.frmApplication_LoginFailed0, ex.Message))
            txtPassword.Clear()
            WaitForInitConnection(TimeSpan.FromSeconds(1))
            Exit Sub
        End Try

        Try
            If Not PerformPostLoginActions(locale) Then
                LogOutRequested()
                txtPassword.Clear()
            End If
        Catch ex As Exception
            LogOutRequested()
            UserMessage.Err(ex, String.Format(My.Resources.frmApplication_LoginFailed0, ex.Message))
            txtPassword.Clear()
        End Try

    End Sub

    Public Sub LoadUserLayoutCache()
        UIStateManager.Instance.UIControlConfigs = gSv.GetUserUILayoutPreferences()
    End Sub

    Public Sub SaveUserLayoutCache()
        gSv.SetUserUILayoutPreferences(UIStateManager.Instance.UIControlConfigs)
    End Sub

    Public Sub AttemptLogin(locale As String, loginMethod As Func(Of String, String, Auth.LoginResult))
        Try
            ServerFactory.CurrentConnectionValid()
        Catch ex As Exception
            UserMessage.ShowExceptionMessage(ex)
            Exit Sub
        End Try

        Try
            Dim resourceName = ResourceMachine.GetName()
            Dim result = loginMethod(resourceName, locale)

            If result.Code = LoginResultCode.Success Then
                Dim successfullySetUpApplication = PerformPostLoginActions(locale)
                If Not successfullySetUpApplication Then User.Logout()
            Else
                UserMessage.Show(String.Format(My.Resources.frmApplication_LoginFailed0, result.Description))
                WaitForInitConnection(TimeSpan.FromSeconds(1))
            End If

        Catch ex As Exception
            UserMessage.Err(ex, String.Format(My.Resources.frmApplication_LoginFailed0, ex.Message))
            WaitForInitConnection(TimeSpan.FromSeconds(1))
            Exit Sub
        End Try

    End Sub

    Private Function DependenciesValid() As Boolean
        'Check state of dependency info before proceeding
        Dim proceed As Boolean = False
        While Not proceed
            Select Case gSv.GetDependenciesStatus()
                Case clsServer.DependencyStates.Invalid
                    'Dependencies invalid - allow user to refresh or logout
                    If UserMessage.TwoButtonsWithCustomText(My.Resources.UnableToCompleteLoginTheProcessDependencyInformationIsOutOfDateAndNeedsToBeRefr,
                     My.Resources.RefreshNow, My.Resources.Logout) = MsgBoxResult.No Then Return False

                    SetStatusBarText(My.Resources.RefreshingProcessDependencyInformation)
                    Me.Refresh()
                    gSv.RebuildDependencies()
                Case clsServer.DependencyStates.Building
                    'Dependencies are being refreshed - allow user to retry or logout
                    If UserMessage.TwoButtonsWithCustomText(My.Resources.UnableToCompleteLoginTheProcessDependencyInformationIsCurrentlyBeingRefreshed,
                     My.Resources.Retry, My.Resources.Logout) = MsgBoxResult.No Then Return False

                    SetStatusBarText(My.Resources.Retrying)
                    Me.Refresh()
                    Threading.Thread.Sleep(2000)
                Case clsServer.DependencyStates.Valid
                    'Ok to proceed
                    proceed = True
            End Select
        End While
        Return True
    End Function

    Private Sub CreateManagers(configOptions As Config.IOptions)
        mConnectionManager = DependencyResolver.Resolve(Of IResourceConnectionManager)(New TypedParameter(GetType(ConnectionType), configOptions.DbConnectionSetting.ConnectionType),
                                                                               New TypedParameter(GetType(Boolean), gSv.GetPref(PreferenceNames.SystemSettings.UseAppServerConnections, False)))
        ' Ensure on login that the Server refrence is refreshed in classes with singleinstance scope
        mGroupStore = DependencyResolver.Resolve(Of IGroupStore)
        mGroupStore.Server = gSv
        Dim mRobotAddressStore = DependencyResolver.Resolve(Of IRobotAddressStore)
        mRobotAddressStore.Server = gSv

        mQueueManager = New ActiveQueueManager(mConnectionManager, mGroupStore)
        If configOptions.StartProcessEngine Then InitProcessEngine()

    End Sub



    Public Function PerformPostLoginActions(Optional locale As String = Nothing) As Boolean
        If Not DependenciesValid() Then Return False

        Dim configOptions = Options.Instance

        'check if custom cert is being used and update if required
        If gSv.IsServer Then
            SaveEnvironmentData(configOptions.GetCertificateExpiryDateTime, configOptions.DbConnectionSetting.Port, gSv.GetServerFullyQualifiedDomainName)
        Else
            SaveEnvironmentData(configOptions.GetCertificateExpiryDateTime)
        End If

        configOptions.LastNativeUser = If(User.Current.AuthType = AuthMode.Native, User.CurrentName, String.Empty)

        Dim perms = gSv.HasUserGotCreatePermissionOnDefaultGroup(New List(Of GroupTreeType) From {GroupTreeType.Processes, GroupTreeType.Objects})
        Dim auth = License

        mCanCreateProcessInDefaultGroup = perms(GroupTreeType.Processes)
        mCanCreateObjectInDefaultGroup = perms(GroupTreeType.Objects)

        'Enable menus that the logged in user can access.
        NewToolStripMenuItem.Enabled = mCanCreateObjectInDefaultGroup OrElse mCanCreateProcessInDefaultGroup
        OpenToolStripMenuItem.Enabled = True
        With User.Current
            ImportToolStripMenuItem.Enabled = .HasPermission(Permission.ProcessStudio.ImportProcess, Permission.ObjectStudio.ImportBusinessObject) OrElse .HasPermission(Permission.ReleaseManager.ImportRelease)
            ImportProcessObjectToolStripMenuItem.Enabled = .HasPermission(Permission.ProcessStudio.ImportProcess, Permission.ObjectStudio.ImportBusinessObject)
            ImportReleaseSkillToolStripMenuItem.Enabled = auth.CanUse(LicenseUse.ReleaseManager) AndAlso .HasPermission(Permission.ReleaseManager.ImportRelease)
            ' Need to redo this whenever the export menu is re-enabled.
            ExportToolStripMenuItem.Enabled =
                .HasPermission(Permission.ObjectStudio.ExportBusinessObject) OrElse
                .HasPermission(Permission.ProcessStudio.ExportProcess)
            ExportNewReleaseToolStripMenuItem.Enabled = auth.CanUse(LicenseUse.ReleaseManager) AndAlso .HasPermission(Permission.ReleaseManager.CreateRelease)
            ExportProcessObjectToolStripMenuItem.Enabled = .HasPermission(Permission.ProcessStudio.ExportProcess)
        End With
        HelpAPIDocumentationMenuItem.Enabled = True
        GuidedTourToolStripMenuItem.Enabled = True
        ConnectionsToolStripMenuItem.Enabled = False

        UpdateEnvironmentColors(True)

        'Remove the users menu
        'Safe to perform even when the user menu is not populated
        ClearUsers()

        Dim keys = gSv.GetLicenseInfo
        Dim wizardResult As DialogResult = DialogResult.OK
        'if the system is unlicensed show the first run license wizard, but only if the user has access to the license part of system manager
        If User.Current.HasPermission(Permission.SystemManager.System.License) Then
            If Not keys.Any(Function(x) Not x.Expired) Then
                Using wizard As New FirstRunWizard()
                    wizard.ShowInTaskbar = False
                    wizardResult = wizard.ShowDialog()
                End Using
            ElseIf keys.Any(Function(x) Not x.Expired AndAlso x.ActivationStatus = LicenseActivationStatus.NotActivated) Then
                Using wizard As New FirstRunWizard(keys.FirstOrDefault(Function(x) Not x.Expired AndAlso x.ActivationStatus = LicenseActivationStatus.NotActivated))
                    wizard.ShowInTaskbar = False
                    wizardResult = wizard.ShowDialog()
                End Using
            End If
        End If

        'Show the welcome screen unless they cancelled out of the license activation wizard before completion
        If wizardResult = DialogResult.Cancel Then
            'Create the managers before creating ctlSystemManager because ctlSystemManager passes through the current form to child controls which may expect the managers to be populated
            CreateManagers(configOptions)
            SetupModuleSwitcher(tpSystemManager)
            ShowPage(GetType(ctlSystemManager))
        Else
            ValidateClientConnectionCount(configOptions)
            CreateManagers(configOptions)
            SetupModuleSwitcher(tpWelcome)
            ShowPage(GetType(ctlWelcome))
            'Create the managers before creating ctlSystemManager because ctlSystemManager passes through the current form to child controls which may expect the managers to be populated
        End If

        If User.Current.HasPermission("Subscribe to Process Alerts") AndAlso AlertEngine Is Nothing Then
            AlertEngine = New clsAlertEngine(Me, User.Current, ResourceMachine.GetName())
        End If

        btnSignout.Enabled = True

        If mServerStatusUpdater Is Nothing Then
            mServerStatusUpdater = New Threading.Timer(AddressOf UpdateServerStatus, Nothing, 0, 120000)
        Else
            Dim connectedTo = gSv.GetConnectedTo()
            UpdateServerStatus(connectedTo)
        End If

        SetTitleAndIcon()

        If CryptoConfig.AllowOnlyFipsAlgorithms Then
            If gSv.DBEncryptionSchemesAreFipsCompliant().Count > 0 Then
                Dim FIPSAlert As New PopupForm(My.Resources.FIPSPolicyEnabled,
                                                My.Resources.ThereAreEncryptionSchemesInYourDatabaseThatAreNotFIPSCompliant,
                                                My.Resources.btnOk)
                AddHandler FIPSAlert.OnBtnOKClick, AddressOf HandleOnBtnOKClick
                FIPSAlert.ShowInTaskbar = False
                FIPSAlert.ShowDialog()
            End If
        End If
        DisplayImportForm()

        CultureValidationCheck()
        'Validate if the server is about to expire.
        ValidateServerCertificates()
        'This code must go after the First run wizard call. the If statement will need uncommenting when the license types are fully coded.

        ShowWelcomeTour(False)

        Return True

    End Function

    Private Sub ValidateClientConnectionCount(configOptions As Config.IOptions)

        If configOptions.DbConnectionSetting.ConnectionType <> ConnectionType.Direct Then Return

        Dim lowThresholdDoNotShowAgain = gSv.GetPref(ResourceConnection.DontShowClientRobotConnectionCheckLow, False)
        Dim highThresholdDoNotShowAgain = gSv.GetPref(ResourceConnection.DontShowClientRobotConnectionCheckHigh, False)
        'if both ticked, do not show then we can exit here.
        If lowThresholdDoNotShowAgain AndAlso highThresholdDoNotShowAgain Then Return

        Dim resourceCount = gSv.GetResourceCount()
        Dim lowThreshold = gSv.GetIntPref(ResourceConnection.ResourceConnectionLowThreshold)
        Dim highThreshold = gSv.GetIntPref(ResourceConnection.ResourceConnectionHighThreshold)

        If Not lowThresholdDoNotShowAgain AndAlso resourceCount > lowThreshold AndAlso resourceCount < highThreshold Then
            ShowMessageDialog(lowThreshold)
        ElseIf Not highThresholdDoNotShowAgain AndAlso resourceCount >= highThreshold Then
            DisableAllRobotsMessageDialog()
        End If
    End Sub

    Private Sub ShowMessageDialog(lowThreshold As Integer)
        Dim msg As New YesNoCheckboxPopupForm(My.Resources.ConnectionLimit, String.Format(My.Resources.ConnectionLimitEnvironmentBetweenXandY, lowThreshold, Environment.NewLine), My.Resources.DontShowThisMessageAgain)

        msg.ShowDialog()
        gSv.SetUserPref(ResourceConnection.DisableConnection, msg.DialogResult = DialogResult.No)
        If msg.IsChecked Then gSv.SetUserPref(ResourceConnection.DontShowClientRobotConnectionCheckLow, True)
        msg.Close()
    End Sub

    Private Function DisableAllRobotsMessageDialog() As DialogResult
        Dim msg As New PopupCheckBoxCloseForm(My.Resources.ConnectionLimit, My.Resources.MaximumNumberOfResourcesForDirectConnectionReached, My.Resources.DontShowThisMessageAgain, My.Resources.DisableAllRobotsMessageDialogOk)
        msg.ShowDialog()
        gSv.SetUserPref(ResourceConnection.DisableConnection, True)
        If msg.IsChecked Then gSv.SetUserPref(ResourceConnection.DontShowClientRobotConnectionCheckHigh, True)
        Return msg.DialogResult
    End Function

    Private Sub ShowWelcomeTour(forceShow As Boolean)

        Dim showAtStartup As Boolean
        Try
            showAtStartup = gSv.GetBoolPref(PreferenceNames.UI.ShowTourAtStartup)
        Catch
            gSv.SetUserPref(PreferenceNames.UI.ShowTourAtStartup, (Licensing.License.LicenseType = LicenseTypes.Evaluation OrElse Licensing.License.LicenseType = LicenseTypes.Education))
            showAtStartup = Licensing.License.LicenseType = LicenseTypes.Evaluation OrElse Licensing.License.LicenseType = LicenseTypes.Education
        End Try
        If forceShow OrElse showAtStartup Then
            Using welcomeWizard = New WelcomeTourWizard(showAtStartup)
                welcomeWizard.StartPosition = FormStartPosition.CenterParent
                welcomeWizard.ShowInTaskbar = False
                welcomeWizard.ShowDialog(Me)
            End Using
        End If
    End Sub

    ''' <summary>
    ''' Updates the statusbar text with details of who is logged in, and what
    ''' server/database they are connected to
    ''' </summary>
    ''' <param name="connectedTo">The server/database</param>
    Private Sub UpdateStatusBarText(connectedTo As String)

        If Not User.LoggedIn Then Return

        SetStatusBarText(
            My.Resources.x0Current1GUser23Connection4ConnectedTo5,
            If(User.Current.LastSignedInAt <> BPUtil.DateMinValueUtc,
               String.Format(My.Resources.Previous0G, User.Current.LastSignedInAt.ToLocalTime()), ""),
           User.Current.SignedInAt.ToLocalTime(),
           User.Current.Name,
           If(User.IsLoggedInto(DatabaseType.SingleSignOn), My.Resources.SSO, ""),
           Options.Instance.DbConnectionSetting.ConnectionName,
           connectedTo)
    End Sub

    Private Sub UpdateEnvironmentColors(ByVal success As Boolean)
        Dim envBackColor = ColourScheme.Default.EnvironmentBackColor
        Dim envForeColor = ColourScheme.Default.EnvironmentForeColor
        If ServerFactory.ServerAvailable AndAlso success Then
            gSv.GetEnvironmentColors(envBackColor, envForeColor)
        End If
        EnvironmentBackColor = envBackColor
        EnvironmentForeColor = envForeColor
    End Sub

    Private Sub SetupModuleSwitcher(ByRef selectedTab As TabPage)
        If mCurrPage IsNot Nothing Then mCurrPage.Hide() 'Hide the current page to get rid of flicky effect

        tcModuleSwitcher.Visible = True
        tcModuleSwitcher.SelectedTab = selectedTab

        If gSv.GetPref(PreferenceNames.Env.HideDigitalExchangeTab, False) Then
            tcModuleSwitcher.Controls.Remove(tpDigitalExchange)
        End If

        If User.Current.AuthType <> AuthMode.Native Then
            tcModuleSwitcher.Controls.Remove(tpMyProfile)
        End If

        UpdateModuleSwitcher()
    End Sub

    Private Sub UpdateModuleSwitcher()

        Dim auth = Licensing.License

        ' Generally, any licence means options are enabled
        Dim enabled As Boolean = auth.IsLicensed

        With User.Current
            tpDesignStudio.Enabled = .HasPermission(Permission.ProcessStudio.GroupName, Permission.ObjectStudio.GroupName) AndAlso enabled
            tpControlRoom.Enabled = .HasPermission(Permission.Resources.ImpliedViewResource.Concat(
                                                   {Permission.ControlRoom.GroupName,
                                                   Permission.Scheduler.GroupName, Permission.SystemManager.DataGateways.ControlRoom}).ToList()) AndAlso enabled
            tpReleaseManager.Enabled = .HasPermission(Permission.ReleaseManager.GroupName) AndAlso auth.CanUse(LicenseUse.ReleaseManager)
            tpReview.Enabled = .HasPermission(Permission.Analytics.GroupName) AndAlso enabled
            tpSystemManager.Enabled = .HasPermission(Permission.Resources.ImpliedManageResources.Concat(
                                                     Permission.Skills.ImpliedViewSkill).Concat(
                                                     {Permission.SystemManager.GroupName}).ToList())

            ImportReleaseSkillToolStripMenuItem.Enabled = auth.CanUse(LicenseUse.ReleaseManager) AndAlso .HasPermission(Permission.ReleaseManager.ImportRelease)
            ExportNewReleaseToolStripMenuItem.Enabled = auth.CanUse(LicenseUse.ReleaseManager) AndAlso .HasPermission(Permission.ReleaseManager.CreateRelease)
            tcModuleSwitcher.Invalidate()
        End With

        With mTaskPanel
            .BeginUpdate()
            If .HasItem(My.Resources.Home) Then    'Panel already exists so just update enabled states
                .SetItemEnabled(My.Resources.Studio, tpDesignStudio.Enabled)
                .SetItemEnabled(My.Resources.xControl, tpControlRoom.Enabled)
                .SetItemEnabled(My.Resources.Analytics, tpReview.Enabled)
                .SetItemEnabled(My.Resources.Releases, tpReleaseManager.Enabled)
                .SetItemEnabled(My.Resources.System, tpSystemManager.Enabled)
            Else
                .Clear()
                .Add(My.Resources.Home,
                     ToolImages.Home_32x32,
                     ToolImages.Home_32x32_Hot,
                     ToolImages.Home_32x32_Disabled,
                     True,
                     Sub() ShowWelcome())

                .Add(My.Resources.Studio,
                     ToolImages.Design_32x32,
                     ToolImages.Design_32x32_Hot,
                     ToolImages.Design_32x32_Disabled,
                     tpDesignStudio.Enabled,
                     Sub() ShowDesignStudio())

                .Add(My.Resources.xControl,
                     ToolImages.Monitor_32x32,
                     ToolImages.Monitor_32x32_hot,
                     ToolImages.Monitor_32x32_disabled,
                     tpControlRoom.Enabled,
                     Sub() ShowControlRoom())

                .Add(My.Resources.Analytics,
                     ToolImages.Chart_32x32,
                     ToolImages.Chart_32x32_Hot,
                     ToolImages.Chart_32x32_Disabled,
                     tpReview.Enabled,
                     Sub() ShowReview())

                .Add(My.Resources.Releases,
                     ComponentImages.Item_32x32,
                     ComponentImages.Item_32x32_Hot,
                     ComponentImages.Item_32x32_Disabled,
                     tpReleaseManager.Enabled,
                     Sub() ShowReleaseManager())

                .Add(My.Resources.System,
                     ToolImages.Tools_32x32,
                     ToolImages.Tools_32x32_Hot,
                     ToolImages.Tools_32x32_Disabled,
                     tpSystemManager.Enabled,
                     Sub() ShowSystemManager())

                Dim selectedItem As String = CStr(IIf(tcModuleSwitcher.SelectedTab.Name = tpSystemManager.Name, My.Resources.System, My.Resources.Home))
                .SetSelectedItem(selectedItem)

            End If
            .EndUpdate()
        End With
    End Sub

    ''' <summary>
    ''' This function makes the taskpane appear.
    ''' </summary>
    Private Sub ShowPanel()
        mTaskPanel.Visible = True
        mTaskPanel.Width = 211
        mCurrPage.Left = mTaskPanel.Width
    End Sub

    ''' <summary>
    ''' This function makes the taskpane disappear.
    ''' </summary>
    Private Sub HidePanel()
        mTaskPanel.Visible = False
        mTaskPanel.Width = 0
        mCurrPage.Left = mTaskPanel.Width
    End Sub

    ''' <summary>
    ''' Shows the 'About Automate form.
    ''' </summary>
    Public Sub AboutVersion(Optional ByVal parent As Control = Nothing)
        Using about As New frmAbout()
            about.ShowInTaskbar = False
            about.ShowDialog(parent)
        End Using
    End Sub

    ''' <summary>
    ''' Handles the licence being changed by ensuring that the control room is
    ''' enabled or disabled appropriately.
    ''' </summary>
    Public Sub NotifyLicenceChange()

        SetTitleAndIcon()

        'Need to save the options in case branding changed...
        Try
            Options.Instance.Save()
        Catch
            'Do Nothing
        End Try

        UpdateModuleSwitcher()
    End Sub

    ''' <summary>
    ''' Boolean used to indicate that a warning message is displaying.
    ''' </summary>
    Private mWarningDisplaying As Boolean = False

    Private mResourceRunnerComponents As ResourceRunnerComponents

    ''' <summary>
    ''' Shows a particular error message relating to a problem setting
    ''' a timestamp in the database; updates the statusbar with a more terse
    ''' message.
    ''' </summary>
    ''' <param name="exceptionmessage">An exception message to be appended to the
    ''' standard message.</param>
    Public Sub ShowTimeStampErrorMessage(ByVal exceptionmessage As String)
        Me.SetStatusBarText(Now().ToString & My.Resources.WARNINGFailedToUpdateDatabaseTimestamp)
        mWarningDisplaying = True
    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub


    ''' <summary>
    ''' Checks if the user wants to continue running process alerts in the background
    ''' if it is currently running for a subscribed user.
    ''' </summary>
    Private Sub CheckContinueAlerts()
        If KeepProcessAlertsRunning.HasValue Then Exit Sub

        If AlertEngine IsNot Nothing AndAlso AlertEngine.UserIsAlertSubscriber _
         AndAlso UserMessage.TwoButtonsWithCustomText(String.Format(
          My.Resources.TheProcessAlertsMonitorIsCurrentlyRunningForUser011ToStopTheAlertsMonitorAndExi, User.CurrentName, vbCrLf),
          My.Resources.xExit, My.Resources.StaySignedIn) = MsgBoxResult.No Then
            KeepProcessAlertsRunning = True
        Else
            KeepProcessAlertsRunning = False
            AlertNotifyIcon.Visible = False
        End If

    End Sub

    Private Sub Archiver_CompleteAndClose(ByVal sender As Object, ByVal e As OperationCompletedEventArgs)

        Dim arch As clsArchiver = DirectCast(sender, clsArchiver)

        ' This handler has been added in frmApplication_Closing in order to wait for the archiver.
        ' The archiver has now cancelled its operation, and we can call Me.Close() to invoke
        ' frmApplication_Closing again, this time to tidy up as normal.
        RemoveHandler arch.ArchiveCompleted, AddressOf Archiver_CompleteAndClose
        RemoveHandler arch.RestoreCompleted, AddressOf Archiver_CompleteAndClose
        Me.Close()

    End Sub

    Private Sub CloseWindowEventHandler(ByVal sResult As String)
        msSignOffOption = sResult
    End Sub

    Friend Sub ShowWelcome()
        tcModuleSwitcher.SelectedTab = tpWelcome
    End Sub

    Friend Sub ShowControlRoom()
        tcModuleSwitcher.SelectedTab = tpControlRoom
    End Sub

    Friend Sub ShowReview()
        tcModuleSwitcher.SelectedTab = tpReview
    End Sub

    Friend Sub ShowDesignStudio()
        tcModuleSwitcher.SelectedTab = tpDesignStudio
    End Sub

    Friend Sub ShowSystemManager()
        tcModuleSwitcher.SelectedTab = tpSystemManager
    End Sub

    Friend Sub ShowReleaseManager()
        tcModuleSwitcher.SelectedTab = tpReleaseManager
    End Sub

#Region "References"

    Public Function ConfirmDeletion(ByRef deps As clsProcessDependencyList) As Boolean
        'Check references to items being deleted
        Dim frm As New frmDeleteDependencies(deps)
        If StartForm(frm, True) = DialogResult.OK Then
            deps = frm.Deletions
            Return deps.Dependencies.Count > 0
        End If
        Return False
    End Function

    ''' <summary>
    ''' Finds references for the given dependency.
    ''' </summary>
    ''' <param name="dep">The dependency to find references for. A null value will
    ''' return immediately without searching for anything</param>
    ''' <param name="self">Not a clue</param>
    Public Sub FindReferences(
     dep As clsProcessDependency, Optional self As clsProcess = Nothing)
        If dep Is Nothing Then Return
        'Start-up (or reset) reference viewer
        If mReferenceViewer Is Nothing Then
            mReferenceViewer = New frmReferenceViewer()
            StartForm(mReferenceViewer)
        End If
        mReferenceViewer.Process = self
        mReferenceViewer.Dependency = dep

        If TypeOf dep Is clsProcessCalendarDependency Then
            Dim calendarDep = DirectCast(dep, clsProcessCalendarDependency)
            mReferenceViewer.tBar.SubTitle = LTools.GetC(calendarDep.RefCalendarName, "holiday", "calendar")
        End If

        If mReferenceViewer.ReferencesFound Then
            mReferenceViewer.BringToFront()
        Else

            Dim message As String
            If String.IsNullOrEmpty(mReferenceViewer.tBar.SubTitle) Then
                message = My.Resources.TheElementCannotFoundWithinTheModel
            Else
                message = String.Format(My.Resources.NoReferencesTo01Found, dep.GetLocalizedFriendlyName(), mReferenceViewer.tBar.SubTitle)
            End If
            UserMessage.OK(message)
            mReferenceViewer.Close()
        End If
    End Sub

#End Region


    'Form overrides dispose to clean up the component list.
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If components IsNot Nothing Then components.Dispose()
            If mConnectionManager IsNot Nothing Then mConnectionManager.Dispose()
            If mQueueManager IsNot Nothing Then mQueueManager.Dispose()
            mServerStatusUpdater?.Change(Timeout.Infinite, Timeout.Infinite)
            mServerStatusUpdater?.Dispose()
            mConnectionManager = Nothing
            mQueueManager = Nothing
            mResourceRunnerComponents?.Dispose()
            ChannelServices.UnregisterChannel(ChannelServices.GetChannel($"automate-IC-server-{Process.GetCurrentProcess().Id}"))
        End If
        MyBase.Dispose(disposing)
    End Sub

#End Region

    ''' <summary>
    ''' Little class for passing details to the background worker
    ''' </summary>
    Private Class ConnectionChangeDetails
        Public Property Connection As String
        Public Property Reload As Boolean
        Public Property Init As Boolean
        Public Property Delay As TimeSpan
    End Class

    ''' <summary>
    '''  Waits for the current connection to initialise
    ''' </summary>
    Private Sub WaitForInitConnection()
        WaitForInitconnection(Nothing, False, True, TimeSpan.Zero)
    End Sub

    ''' <summary>
    ''' Waits for the current connection to initialise
    ''' </summary>
    ''' <param name="delay">An additional artifical delay</param>
    Private Sub WaitForInitConnection(delay As TimeSpan)
        WaitForInitconnection(Nothing, False, False, delay)
    End Sub

    ''' <summary>
    ''' Waits for the given connection to initialise
    ''' <param name="connection">The connection name to use, ignore if nothing is
    ''' passed</param>
    ''' </summary>
    Friend Sub WaitForInitConnection(connection As String)
        WaitForInitconnection(connection, False, True, TimeSpan.Zero)
    End Sub

    ''' <summary>
    ''' Waits for the current connection to initalise, reloads the
    ''' connection if required and initialises it
    ''' </summary>
    ''' <param name="reload">Whether to reload</param>
    Private Sub WaitForInitconnection(reload As Boolean)
        WaitForInitconnection(Nothing, reload, True, TimeSpan.Zero)
    End Sub

    ''' <summary>
    ''' Waits for the connection to initalise and reloads and inits the connection
    ''' if required
    ''' </summary>
    ''' <param name="connection">The connection name to use, ignore if nothing is
    ''' passed</param>
    ''' <param name="reload">Whether to reload</param>
    ''' <param name="init">Whether to init</param>
    ''' <param name="delay">An additional artifical delay</param>
    Private Sub WaitForInitconnection(connection As String, reload As Boolean, init As Boolean, delay As TimeSpan)

        Dim page = TryCast(mCurrPage, ctlLogin)
        If page IsNot Nothing Then
            page.LoginEnabled = False
        End If

        Dim details = New ConnectionChangeDetails With {
                .Connection = connection,
                .Reload = reload,
                .Init = init,
                .Delay = delay}

        ProgressDialog.Show(Me, mConnectionChangeWorker, details,
         My.Resources.PleaseWait, My.Resources.InitialisingConnection)


        If page IsNot Nothing Then 'Check again because we need to focus after the above
            page.FocusUsernameOrPassword()
        End If

    End Sub

    ''' <summary>
    ''' Handles the work required in changing the connection.
    ''' Primarily, this is just the initialisation of the connection which the
    ''' parent Application form handles in the background.
    ''' It fires a <see cref="frmApplication.ConnectionChanged"/> event after it
    ''' initialises the connection (on the UI thread), so the handler for that event
    ''' is where we update the UI with the new connection details.
    ''' </summary>
    Private Sub HandleBackgroundConnectionChange(
     sender As Object, e As DoWorkEventArgs) Handles mConnectionChangeWorker.DoWork

        Dim d = DirectCast(e.Argument, ConnectionChangeDetails)

        If d.Connection IsNot Nothing Then _
            Options.Instance.CurrentConnectionName = d.Connection

        Thread.Sleep(d.Delay)

        e.Result = PerformPostConnectionConfigChange(d.Reload, d.Init)
    End Sub

    ''' <summary>
    ''' Handles the connection change background work completed.
    ''' </summary>
    Private Sub HandleBackgroundConnectionCompleted(
     sender As Object, e As RunWorkerCompletedEventArgs) _
     Handles mConnectionChangeWorker.RunWorkerCompleted

        Dim args = TryCast(e.Result, DatabaseChangeEventArgs)

        ' PerformPostConnectionConfigChange can return nothing which
        ' indicates we do not want to raise connection changed events.
        If args IsNot Nothing Then OnConnectionChanged(args)
    End Sub

    ''' <summary>
    ''' Handles the connection change background work completed.
    ''' </summary>
    Shared Sub UpdateGlobalAppProperties()
        ApplicationProperties.ApplicationName = My.Resources.frmApplication_BluePrism
        ApplicationProperties.CompanyName = My.Resources.frmApplication_BluePrism
        ApplicationProperties.ApplicationModellerName = My.Resources.frmApplication_ApplicationModeller
    End Sub

    ''' <summary>
    ''' Refreshes the form with the new locale
    ''' <param name="newLocaleName">The locale to use, ignore if nothing is
    ''' passed</param>
    ''' </summary>
    Friend Sub RefreshLocale(newLocaleName As String)
        Dim newLocale As CultureInfo = Nothing
        Dim newLocaleFormat As CultureInfo = Nothing

        Dim configOptions = Options.Instance
        If newLocaleName = configOptions.SystemLocale Then
            newLocale = New CultureInfo(configOptions.SystemLocale)
            newLocaleFormat = New CultureInfo(configOptions.SystemLocaleFormat)
        Else
            newLocale = New CultureInfo(newLocaleName)
            newLocaleFormat = New CultureInfo(newLocaleName)
        End If

        Thread.CurrentThread.CurrentUICulture = newLocale
        Thread.CurrentThread.CurrentCulture = newLocaleFormat
        configOptions.CurrentLocale = newLocaleName

        UpdateGlobalAppProperties()
        RefreshLoginScreen()
        WaitForInitConnection()

        clsAMI.StaticBuildTypes()
        clsProcessDataTypes.RebuildTypeInfo()
        clsWindowSpy.InvalidateInfoCache()
    End Sub

    Private Sub RefreshLoginScreen()
        Dim loginStateBefore = If(TryCast(mCurrPage, ctlLogin)?.LoginEnabled, False)

        Dim prevLoginTabIndex As Integer = 0
        Dim prevLoginPage = TryCast(mCurrPage, ctlLogin)
        If prevLoginPage IsNot Nothing Then
            prevLoginTabIndex = prevLoginPage.SwitchPanel.SelectedIndex
        End If

        Dim currentSize = Size

        For Each c As Control In Me.Controls
            Me.Controls.Remove(c)
            c.Dispose()
        Next

        Controls.Clear()
        InitializeComponent()

        'Reset the current page value so the new Login control can be set in the main panel.
        mCurrPage = Nothing

        UpdateUserInterfaceForLoggedOut()
        SetTitleAndIcon()
        RefreshLoginScreenLayout(currentSize)
        RestorePreviousUsernameAndPassword()

        Dim page = TryCast(mCurrPage, ctlLogin)
        If page IsNot Nothing Then
            page.LoginEnabled = loginStateBefore
            page.SwitchPanel.SelectedIndex = prevLoginTabIndex
        End If
    End Sub

    ''' <summary>
    ''' This is not the ideal solution to force WinForms to refresh the layout.
    ''' However, the form doesn't appear to respond to changing the DockStyle of the inner controls,
    ''' or using the built in Resume/SuspendLayout() with PerformLayout() and Refresh() commands.
    ''' So currently changing the overall size of the form and then back to it's original state appears,
    ''' to be the only way to reset the controls to re-fill the form that works at this point.
    ''' If a better way is found then this method should be changed in the future.
    ''' </summary>
    ''' <param name="previousSize">The previous Width, Height values of the form before the locale is updated.</param>
    Private Sub RefreshLoginScreenLayout(previousSize As Size)
        If WindowState = FormWindowState.Maximized Then
            WindowState = FormWindowState.Normal
            WindowState = FormWindowState.Maximized
        ElseIf WindowState = FormWindowState.Normal Then
            WindowState = FormWindowState.Maximized
            WindowState = FormWindowState.Normal
            Size = previousSize
        End If
    End Sub

    Private Sub RestorePreviousUsernameAndPassword()
        If PreviousUsername Is Nothing OrElse PreviousPassword Is Nothing Then Return
        Dim loginControl = TryCast(mCurrPage, ctlLogin)

        loginControl?.SetUsernameAndPassword(PreviousUsername, PreviousPassword)

        PreviousUsername = Nothing
        PreviousPassword = Nothing
    End Sub

    Private Sub HandleOnBtnOKClick(sender As Object, e As EventArgs)
        Dim popup = CType(sender, PopupForm)
        RemoveHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOKClick
        popup.Close()
    End Sub

    Private Sub GuidedTourToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles GuidedTourToolStripMenuItem.Click
        ShowWelcomeTour(True)
    End Sub

    ''' <summary>
    ''' Checks the runtime resource current culutre and determines if it uses unicode and non-unicode logging is enabled.  
    ''' If this is the case, 
    ''' </summary>
    Private Sub CultureValidationCheck()
        If Not gSv.GetPref(PreferenceNames.UI.DontShowUnicodeResourceWarning, False) AndAlso
            Not gSv.UnicodeLoggingEnabled() AndAlso
            IsRuntimeResourceCurrentCultureUnicode() Then
            Dim popup = New PopupCheckBoxForm(My.Resources.WARNING,
                                              My.Resources.UnicodeLanguageShouldHaveUnicodeLogging,
                                              My.Resources.DontShowThisMessageAgain, My.Resources.btnOk)
            AddHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOKClickCultureCheck
            popup.ShowInTaskbar = False
            popup.ShowDialog()
        End If
    End Sub


    ''' <summary>
    ''' Validation check on the server certificates to see if they will expire within the next X days. 
    ''' </summary>
    Private Sub ValidateServerCertificates()

        Dim requiredDaysWindow = gSv.GetCertificateExpThreshold
        Dim environmentDataCheck = gSv.GetEnvironmentData().Where(Function(x) x.CertificateExpTime.HasValue AndAlso x.CertificateExpTime.Value < Date.UtcNow.AddDays(requiredDaysWindow)) _
                                                            .Select(Function(y) y.CertificateExpTime.Value) _
                                                            .OrderBy(Function(d) d).Distinct()

        If environmentDataCheck.Any() Then
            Dim message = IIf(environmentDataCheck.Count > 1,
                              String.Format(My.Resources.TheMultipleCertificateUsedToEncryptTheApplicationServerConfigurationIsDueToExpireIn, requiredDaysWindow, environmentDataCheck.First),
                              String.Format(My.Resources.TheCertificateUsedToEncryptTheApplicationServerConfigurationIsDueToExpireIn, requiredDaysWindow, environmentDataCheck.First))
            Dim popup = New PopupForm(My.Resources.WARNING,
                                      message.ToString,
                                      My.Resources.btnOk)
            AddHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOKClick
            popup.ShowInTaskbar = False
            popup.ShowDialog()
        End If
    End Sub

    Private Sub HandleOnBtnOKClickCultureCheck(sender As Object, e As EventArgs)
        Dim popup = CType(sender, PopupCheckBoxForm)
        RemoveHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOKClickCultureCheck
        If popup.IsChecked Then
            gSv.SetUserPref(PreferenceNames.UI.DontShowUnicodeResourceWarning, True)
        End If
        popup.Close()
    End Sub

    Private Function IsRuntimeResourceCurrentCultureUnicode() As Boolean
        Dim resources = gSv.GetResourcesCulture()
        Dim anyUnicode = resources.Any(Function(c) IsUnicode(c.CurrentCulture))
        Return anyUnicode
    End Function

    Private Function IsUnicode(input As String) As Boolean
        Try
            Return Internationalisation.Locales.IsUnicode(input)
        Catch
            NLog.LogManager.GetCurrentClassLogger().Error($"Failed to perform unicode language check on culture - {input}")
            Return False
        End Try
    End Function

    Friend Sub DisplayPopup(descriptionText As String)
        For Each frm As Form In Me.OwnedForms
            If frm.Modal Then Exit Sub
        Next

        Dim popUp = New PopupForm(My.Resources.PleaseLogInToImport_Title, descriptionText, My.Resources.OK) With {
                .StartPosition = FormStartPosition.CenterParent
                }
        AddHandler popUp.OnBtnOKClick, AddressOf HandleOnBtnOKClick
        popUp.BringToFront()
        popUp.ShowInTaskbar = False
        popUp.ShowDialog(gMainForm)
    End Sub

    Friend Sub StartImportThread()
        Dim newThread As Thread = New Thread(New ThreadStart(AddressOf DisplayImportForm))
        newThread.SetApartmentState(ApartmentState.STA)
        newThread.CurrentCulture = New CultureInfo(Options.Instance.CurrentLocale)
        newThread.CurrentUICulture = New CultureInfo(Options.Instance.CurrentLocale)
        newThread.Start()
    End Sub

    Friend Sub DisplayImportForm()
        Dim action As Action = Sub()
                                   Dim filePath As String = String.Empty
                                   While FilesToImport.FileQueue.TryDequeue(filePath)
                                       Dim fileExtension = Path.GetExtension(filePath).Replace(".", "")
                                       Dim importerType = frmImportRelease.ImportType.Release
                                       If fileExtension.Equals(clsProcess.ProcessFileExtension, StringComparison.InvariantCultureIgnoreCase) OrElse
                                          fileExtension.Equals(clsProcess.ObjectFileExtension, StringComparison.InvariantCultureIgnoreCase) Then importerType = frmImportRelease.ImportType.ProcessObject

                                       Dim import = New frmImportRelease(importerType)
                                       import.SetFileToImport(filePath)
                                       OnImportCompleted(EventArgs.Empty)
                                   End While
                               End Sub
        Parallel.Invoke(action)
        FilesToImport.FilesInBatch = 0
    End Sub

    Private Sub frmApplication_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If (User.LoggedIn) Then
            SaveUserLayoutCache()
            'Stop the timer so it can't execute again.
            mServerStatusUpdater?.Change(Timeout.Infinite, Timeout.Infinite)
            'now attempt to wait if the update status is currently active.
            If mStatusUpdateActive Then
                SyncLock mStatusLock
                    Dim statusTimeout As Integer = 5000
                    'wait on status lock, just timeout if it takes too long.
                    Monitor.Wait(mStatusLock, statusTimeout)
                End SyncLock
            End If

        End If
    End Sub
End Class
