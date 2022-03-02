Imports AutomateControls
Imports BluePrism.BPCoreLib
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Images
Imports Internationalisation
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.clsServerPartialClasses
Imports AutomateControls.UIState.UIElements
Imports BluePrism.Server.Domain.Models

''' Project  : Automate
''' Class    : ctlSystemManager
'''
''' <summary>
''' The System Manager control.
''' </summary>
Friend Class ctlSystemManager
    Inherits System.Windows.Forms.UserControl
    Implements IHelp, IPermission, IStubbornChild, IEnvironmentColourManager

#Region " Windows Form Designer generated code "

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            For Each c As Control In mCachedPages.Values
                c.Dispose()
            Next
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
    Friend WithEvents llDeleteLog As System.Windows.Forms.LinkLabel
    Friend WithEvents ChBxStartProcessEngine As System.Windows.Forms.CheckBox
    Friend WithEvents splitPanel As AutomateControls.SplitContainers.HighlightingSplitContainer
    Friend WithEvents lblSystemArea As System.Windows.Forms.Label
    Friend WithEvents pnlTitle As System.Windows.Forms.Panel
    Friend WithEvents lblTitle As System.Windows.Forms.Label
    Public WithEvents mMenuButton As AutomateControls.MenuButton
    Friend WithEvents tvNavigationTree As System.Windows.Forms.TreeView
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlSystemManager))
        Me.llDeleteLog = New System.Windows.Forms.LinkLabel()
        Me.splitPanel = New AutomateControls.SplitContainers.HighlightingSplitContainer()
        Me.tvNavigationTree = New System.Windows.Forms.TreeView()
        Me.pnlTitle = New System.Windows.Forms.Panel()
        Me.lblTitle = New System.Windows.Forms.Label()
        Me.mMenuButton = New AutomateControls.MenuButton()
        Me.lblSystemArea = New System.Windows.Forms.Label()
        CType(Me.splitPanel, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitPanel.Panel1.SuspendLayout()
        Me.splitPanel.Panel2.SuspendLayout()
        Me.splitPanel.SuspendLayout()
        Me.pnlTitle.SuspendLayout()
        Me.SuspendLayout()
        '
        'llDeleteLog
        '
        resources.ApplyResources(Me.llDeleteLog, "llDeleteLog")
        Me.llDeleteLog.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.llDeleteLog.Name = "llDeleteLog"
        Me.llDeleteLog.TabStop = True
        '
        'splitPanel
        '
        Me.splitPanel.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        resources.ApplyResources(Me.splitPanel, "splitPanel")
        Me.splitPanel.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.splitPanel.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.splitPanel.GripVisible = False
        Me.splitPanel.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.splitPanel.MouseLeaveColor = System.Drawing.Color.White
        Me.splitPanel.Name = "splitPanel"
        '
        'splitPanel.Panel1
        '
        Me.splitPanel.Panel1.Controls.Add(Me.tvNavigationTree)
        Me.splitPanel.Panel1.Controls.Add(Me.pnlTitle)
        '
        'splitPanel.Panel2
        '
        Me.splitPanel.Panel2.Controls.Add(Me.mMenuButton)
        Me.splitPanel.Panel2.Controls.Add(Me.lblSystemArea)
        Me.splitPanel.SplitLineMode = AutomateControls.GrippableSplitLineMode.[Single]
        Me.splitPanel.TabStop = False
        Me.splitPanel.TextColor = System.Drawing.Color.Black
        '
        'tvNavigationTree
        '
        Me.tvNavigationTree.BorderStyle = System.Windows.Forms.BorderStyle.None
        resources.ApplyResources(Me.tvNavigationTree, "tvNavigationTree")
        Me.tvNavigationTree.HideSelection = False
        Me.tvNavigationTree.Name = "tvNavigationTree"
        '
        'pnlTitle
        '
        Me.pnlTitle.Controls.Add(Me.lblTitle)
        resources.ApplyResources(Me.pnlTitle, "pnlTitle")
        Me.pnlTitle.Name = "pnlTitle"
        '
        'lblTitle
        '
        Me.lblTitle.BackColor = System.Drawing.Color.DimGray
        resources.ApplyResources(Me.lblTitle, "lblTitle")
        Me.lblTitle.ForeColor = System.Drawing.Color.White
        Me.lblTitle.Name = "lblTitle"
        '
        'mMenuButton
        '
        resources.ApplyResources(Me.mMenuButton, "mMenuButton")
        Me.mMenuButton.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(114, Byte), Integer), CType(CType(198, Byte), Integer))
        Me.mMenuButton.Name = "mMenuButton"
        '
        'lblSystemArea
        '
        Me.lblSystemArea.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(114, Byte), Integer), CType(CType(198, Byte), Integer))
        resources.ApplyResources(Me.lblSystemArea, "lblSystemArea")
        Me.lblSystemArea.ForeColor = System.Drawing.Color.White
        Me.lblSystemArea.Name = "lblSystemArea"
        '
        'ctlSystemManager
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.Color.White
        Me.Controls.Add(Me.splitPanel)
        Me.Name = "ctlSystemManager"
        resources.ApplyResources(Me, "$this")
        Me.splitPanel.Panel1.ResumeLayout(False)
        Me.splitPanel.Panel2.ResumeLayout(False)
        CType(Me.splitPanel, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitPanel.ResumeLayout(False)
        Me.pnlTitle.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

#End Region

#Region " Class-scope Declarations "

    Private Class PageInfo
        Public Property PageType As Type
        Public Property PageTitle As String
        Public Property Permission As String()
        Public Property ProcessType As ProcessType
        Public Property Licensed As Boolean
    End Class

#End Region

#Region " Variables "

    ' The current page displayed in System Manager
    Private mCurrentPage As Control

    ' A cache of the pages held in this instance of System Manager
    Private mCachedPages As Dictionary(Of Type, Control)

    ' The Application form hosting this control
    Private mParent As frmApplication

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets or sets the Application form which acts as parent to this control
    ''' </summary>
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the permission level for the control.
    ''' </summary>
    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName(Permission.Resources.ImpliedManageResources.Concat(
                                     Permission.Skills.ImpliedViewSkill).Concat(
                                     {Permission.SystemManager.GroupName}).ToList())
        End Get
    End Property

    ''' <summary>
    ''' The context menu strip to use for the menu button.
    ''' If null, this will not display a menu button
    ''' </summary>
    <Browsable(True), Category("Behavior"), Description(
        "The context menu to use for the menu button. A null value will hide " &
        "the menu button")>
    Public Property MenuButtonContextMenuStrip As ContextMenuStrip
        Get
            Return mMenuButton.ContextMenuStrip
        End Get
        Set(value As ContextMenuStrip)
            mMenuButton.ContextMenuStrip = value
        End Set
    End Property

    'public Property RefreshButton as 

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
            Return lblSystemArea.BackColor
        End Get
        Set(value As Color)
            lblSystemArea.BackColor = value
            mMenuButton.BackColor = value
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
            Return lblSystemArea.ForeColor
        End Get
        Set(value As Color)
            lblSystemArea.ForeColor = value
        End Set
    End Property

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new System Manager control
    ''' </summary>
    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()
        PopulateTreeView()
        tvNavigationTree.ImageList = ImageLists.Sysman_16x16

        mCachedPages = New Dictionary(Of Type, Control)

        SetupPageInfo(Nothing, tvNavigationTree.Nodes, IsLicensed)

    End Sub

#End Region

#Region " Methods "

    ''' <summary>
    ''' Populates the system manger navigation treeview using strings from resource files.
    ''' </summary>
    Private Sub PopulateTreeView()
        Dim nProcessExposure = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_exposure")) With {.Name = "nProcessExposure"}
        Dim nProcessManagement = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_management")) With {.Name = "nProcessManagement"}
        Dim nProcessHistory = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_history")) With {.Name = "nProcessHistory"}
        Dim nProcessExceptionTypes = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_exception_types")) With {.Name = "nProcessExceptionTypes"}
        Dim nProcessEnvironmentVariables = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_env_variables")) With {.Name = "nProcessEnvironmentVariables"}
        Dim nProcesses = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_processes"), New TreeNode() {nProcessExposure, nProcessManagement, nProcessHistory, nProcessExceptionTypes, nProcessEnvironmentVariables}) With {.Name = "nProcesses", .ImageKey = "processes", .SelectedImageKey = "processes"}
        Dim nObjectExposure = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_exposure")) With {.Name = "nObjectExposure"}
        Dim nObjectManagement = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_management")) With {.Name = "nObjectManagement"}
        Dim nObjectHistory = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_history")) With {.Name = "nObjectHistory"}
        Dim nObjectExternal = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_external")) With {.Name = "nObjectExternal"}
        Dim nSoapWebServices = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_soap_web_services")) With {.Name = "nSoapWebServices"}
        Dim nSystemWebConnectionSettings = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_web_connection_settings")) With {.Name = "nSystemWebConnectionSettings"}
        Dim nWebApiServices = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_webapi_services"), New TreeNode() {nSystemWebConnectionSettings}) With {.Name = "nWebApiServices"}
        Dim nObjectExceptionTypes = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_exception_types")) With {.Name = "nObjectExceptionTypes"}
        Dim nObjectEnvironmentVariables = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_env_variables")) With {.Name = "nObjectEnvironmentVariables"}
        Dim nObjects = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_objects"), New TreeNode() {nObjectExposure, nObjectManagement, nObjectHistory, nObjectExternal, nSoapWebServices, nWebApiServices, nObjectExceptionTypes, nObjectEnvironmentVariables}) With {.Name = "nObjects", .ImageKey = "objects", .SelectedImageKey = "objects"}
        Dim nResourcePools = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_pools")) With {.Name = "nResourcePools"}
        Dim nResourceManagement = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_management")) With {.Name = "nResourceManagement"}
        Dim nResources = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_resources"), New TreeNode() {nResourcePools, nResourceManagement}) With {.Name = "nResources", .ImageKey = "resources", .SelectedImageKey = "resources"}
        Dim nWorkflowQueues = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_work_queues")) With {.Name = "nWorkflowWorkQueues"}
        Dim nWorkflowEnvironmentLocks = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_env_locks")) With {.Name = "nWorkFlowEnvironmentLocks"}
        Dim nWorkFlow = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_workflow"), New TreeNode() {nWorkflowQueues, nWorkflowEnvironmentLocks}) With {.Name = "nWorkflow", .ImageKey = "workflow", .SelectedImageKey = "workflow"}
        Dim documentProcessingQueuesNode = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_documentprocessingqueues")) With {.Name = "documentProcessingQueuesNode"}
        Dim documentProcessingNode = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_documentprocessing"), New TreeNode() {documentProcessingQueuesNode}) With {.Name = "documentProcessingNode", .ImageKey = "documentProcessing", .SelectedImageKey = "documentProcessing"}
        Dim nSecurityUsers = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_users")) With {.Name = "nSecurityUsers"}
        Dim nSecurittyUserRoles = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_user_roles")) With {.Name = "nSecurityUserRoles"}
        Dim nSecurityOptions = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_sign_on_settings")) With {.Name = "nSecurityOptions"}
        Dim nSecurityCredentials = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_credentials")) With {.Name = "nSecurityCredentials"}
        Dim nSecurityEncryption = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_encryption_schemes")) With {.Name = "nSecurityEncryption"}
        Dim nSecurity = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_security"), New TreeNode() {nSecurityUsers, nSecurittyUserRoles, nSecurityOptions, nSecurityCredentials, nSecurityEncryption}) With {.Name = "nSecurity", .ImageKey = "security", .SelectedImageKey = "security"}
        Dim nAuditProcessLogs = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_process_logs")) With {.Name = "nAuditProcessLogs"}
        Dim nAuditObjectLogs = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_object_logs")) With {.Name = "nAuditObjectLogs"}
        Dim nAuditLogs = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_audit_logs")) With {.Name = "nAuditLogs"}
        Dim nAuditStatistics = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_statistics")) With {.Name = "nAuditStatistics"}
        Dim nAuditAlerts = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_alerts")) With {.Name = "nAuditAlerts"}
        Dim nAuditDesignControl = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_design_control")) With {.Name = "nAuditDesignControl"}
        Dim nAudit = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_audit"), New TreeNode() {nAuditProcessLogs, nAuditObjectLogs, nAuditLogs, nAuditStatistics, nAuditAlerts, nAuditDesignControl}) With {.Name = "nAudit", .ImageKey = "audit", .SelectedImageKey = "audit"}

        Dim nDataGatewaySettings = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_dg_settings")) With {.Name = "nDataGatewaySettings"}
        Dim nDataGatewayConfiguration = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_configuration")) With {.Name = "nDataGatewayConfiguration"}
        Dim nDataGateways = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_data_gateways"), New TreeNode() {nDataGatewaySettings, nDataGatewayConfiguration}) With {.Name = "nDataGateways", .ImageKey = "dataPipelines", .SelectedImageKey = "dataPipelines"}

        Dim nSystemSettings = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_settings")) With {.Name = "nSystemSettings"}
        Dim nSystemLicense = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_license")) With {.Name = "nSystemLicense"}
        Dim nSystemArchiving = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_archiving")) With {.Name = "nSystemArchiving"}
        Dim nSystemScheduler = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_scheduler")) With {.Name = "nSystemScheduler"}
        Dim nSystemCalendar = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_calendar")) With {.Name = "nSystemCalendar"}
        Dim nSystemFonts = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_fonts")) With {.Name = "nSystemFonts"}
        Dim nWorkQueueSnapshots = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_work_queue_snapshots")) With {.Name = "nWorkQueueSnapshots"}
        Dim nSystemReports = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_reporting"), New TreeNode() {nWorkQueueSnapshots}) With {.Name = "nSystemReports"}
        Dim nSkillsManagement = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_skills_management")) With {.Name = "nSkillsManagement"}
        Dim nSkills = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_skills"), New TreeNode() {nSkillsManagement}) With {.Name = "nSkills", .ImageKey = "skills", .SelectedImageKey = "skills"}
        Dim nSystem = New TreeNode(ResMan.GetString("ctlsystemmanager_tv_system"), New TreeNode() {nSystemSettings, nSystemLicense, nSystemArchiving, nSystemScheduler, nSystemCalendar, nSystemFonts, nSystemReports}) With {.Name = "nSystem", .ImageKey = "system", .SelectedImageKey = "system"}

        tvNavigationTree.Nodes.AddRange(New TreeNode() {nProcesses, nObjects, nSkills, nResources, nDataGateways, nWorkFlow, documentProcessingNode, nSecurity, nAudit, nSystem})

        If Not gSv.GetFeatureEnabled(Feature.DocumentProcessing) Then
            documentProcessingNode.Remove()
        End If

    End Sub


    ''' <summary>
    ''' Checks if this application page can be safely left or not. This checks the
    ''' currently selected sysman page to see if it can safely leave.
    ''' </summary>
    ''' <returns>True if the currently selected page does not require the user to
    ''' remain on it, False otherwise.</returns>
    Public Function CanLeave() As Boolean Implements IStubbornChild.CanLeave
        Dim stubborn As IStubbornChild = TryCast(mCurrentPage, IStubbornChild)
        Return (stubborn Is Nothing OrElse stubborn.CanLeave())
    End Function

    Private Function SetEnabledByPermission() As PageInfo
        Dim firstPageWithPermission As PageInfo = Nothing
        For Each node As TreeNode In tvNavigationTree.Nodes
            Dim childHasPermission As Boolean = False

            For Each subnode As TreeNode In node.Nodes
                UpdateAccordingToPermissions(subnode, firstPageWithPermission, childHasPermission)
            Next

            If Not childHasPermission Then
                node.ForeColor = Color.Gray
            Else
                node.ForeColor = Color.Black
                node.ExpandAll()
            End If
        Next
        Return firstPageWithPermission
    End Function

    Private Sub UpdateAccordingToPermissions(node As TreeNode, ByRef firstPageWithPermission As PageInfo, ByRef hasPermission As Boolean)
        Dim info = TryCast(node.Tag, PageInfo)
        If info IsNot Nothing AndAlso info.Permission IsNot Nothing Then
            If User.Current.HasPermission(info.Permission) AndAlso info.Licensed Then
                If firstPageWithPermission Is Nothing Then
                    firstPageWithPermission = info
                End If
                hasPermission = True
                node.ForeColor = Color.Black
            Else
                node.ForeColor = Color.Gray
            End If
        End If

        For Each child As TreeNode In node.Nodes
            UpdateAccordingToPermissions(child, firstPageWithPermission, hasPermission)
        Next
    End Sub

    Private ReadOnly Property IsLicensed As Boolean
        Get
            Dim auth = Licensing.License
            Return auth.IsLicensed
        End Get
    End Property

    Public Sub NotifyLicenceChange()
        SetupPageInfo(Nothing, tvNavigationTree.Nodes, IsLicensed)
        SetEnabledByPermission()
    End Sub

    Private Sub SetupPageInfo(parentName As String, nodes As TreeNodeCollection, licensed As Boolean)
        For Each node As TreeNode In nodes
            Dim childName = If(parentName Is Nothing, node.Nodes(0).Text, node.Text)
            Dim title = String.Format(My.Resources.ctlSystemManager_Title01, If(parentName, node.Text), childName)

            node.Tag = SetupPageInfo(node.Name, title, licensed)
            SetupPageInfo(node.Text, node.Nodes, licensed)
        Next
    End Sub


    Private Function SetupPageInfo(ByVal name As String, ByVal title As String, licensed As Boolean) As PageInfo
        Dim info As New PageInfo
        info.PageTitle = title
        info.Licensed = licensed
        Select Case name
            Case "nProcesses", "nProcessExposure"
                info.PageType = GetType(ctlProcessExposure)
                info.Permission = {Permission.SystemManager.Processes.Exposure}
                info.ProcessType = ProcessType.Process
            Case "nProcessManagement"
                info.PageType = GetType(ProcessRetirementControl)
                info.Permission = {Permission.SystemManager.Processes.Management}
                info.ProcessType = ProcessType.Process
            Case "nProcessHistory"
                info.PageType = GetType(ctlProcessHistory)
                info.Permission = {Permission.SystemManager.Processes.History}
                info.ProcessType = ProcessType.Process
            Case "nProcessExceptionTypes"
                info.PageType = GetType(ctlExceptionTypes)
                info.Permission = {Permission.SystemManager.Processes.ExceptionTypes}
                info.ProcessType = ProcessType.Process
            Case "nProcessEnvironmentVariables"
                info.PageType = GetType(ctlEnvironmentVariables)
                info.Permission = {Permission.SystemManager.Processes.ConfigureEnvironmentVariables}
                info.ProcessType = ProcessType.Process
            Case "nObjects", "nObjectExposure"
                info.PageType = GetType(ctlProcessExposure)
                info.Permission = {Permission.SystemManager.BusinessObjects.Exposure}
                info.ProcessType = ProcessType.BusinessObject
            Case "nObjectManagement"
                info.PageType = GetType(ProcessRetirementControl)
                info.Permission = {Permission.SystemManager.BusinessObjects.Management}
                info.ProcessType = ProcessType.BusinessObject
            Case "nObjectHistory"
                info.PageType = GetType(ctlProcessHistory)
                info.Permission = {Permission.SystemManager.BusinessObjects.History}
                info.ProcessType = ProcessType.BusinessObject
            Case "nObjectExternal"
                info.PageType = GetType(ctlExternalBusinessObjects)
                info.Permission = {Permission.SystemManager.BusinessObjects.External}
            Case "nSoapWebServices"
                info.PageType = GetType(ctlWebServices)
                info.Permission = {Permission.SystemManager.BusinessObjects.WebServicesSoap}
            Case "nWebApiServices"
                info.PageType = GetType(ctlWebApiServices)
                info.Permission = {Permission.SystemManager.BusinessObjects.WebServicesWebApi}
            Case "nObjectExceptionTypes"
                info.PageType = GetType(ctlExceptionTypes)
                info.Permission = {Permission.SystemManager.BusinessObjects.ExceptionTypes}
                info.ProcessType = ProcessType.BusinessObject
            Case "nObjectEnvironmentVariables"
                info.PageType = GetType(ctlEnvironmentVariables)
                info.Permission = {Permission.SystemManager.BusinessObjects.ViewEnvironmentVariables}
                info.ProcessType = ProcessType.BusinessObject
            Case "nResources", "nResourcePools"
                info.PageType = GetType(ctlResourcePools)
                info.Permission = {Permission.SystemManager.Resources.Pools}
            Case "nResourceManagement"
                info.PageType = GetType(ctlResourceManagement)
                info.Permission = Permission.Resources.ImpliedManageResources
            Case "nWorkflow", "nWorkflowWorkQueues"
                info.PageType = GetType(ctlWorkflowWorkQueues)
                info.Permission = {Permission.SystemManager.Workflow.WorkQueueConfiguration}
            Case "nWorkFlowEnvironmentLocks"
                info.PageType = GetType(ctlWorkflowEnvironmentLocks)
                info.Permission = {Permission.SystemManager.Workflow.EnvironmentLocking}
            Case "nSecurity", "nSecurityUsers"
                info.PageType = GetType(CtlSecurityUsers)
                info.Permission = {Permission.SystemManager.Security.Users}
            Case "nSecurityUserRoles"
                info.PageType = GetType(ctlSecurityUserRoles)
                info.Permission = {Permission.SystemManager.Security.UserRoles}
            Case "nSecurityOptions"
                If User.IsLoggedInto(DatabaseType.SingleSignOn) Then
                    info.PageType = GetType(ctlSystemSingleSignon)
                Else
                    info.PageType = GetType(ctlSecurityOptions)
                End If
                info.Permission = {Permission.SystemManager.Security.SignOnSettings}
            Case "nSecurityCredentials"
                info.PageType = GetType(ctlSecurityCredentials)
                info.Permission = {Permission.SystemManager.Security.Credentials}
            Case "nSecurityEncryption"
                info.PageType = GetType(ctlSecurityEncryptionSchemes)
                info.Permission = {Permission.SystemManager.Security.ViewEncryptionSchemes}
            Case "nAudit", "nAuditProcessLogs"
                info.PageType = GetType(ctlAuditProcessLogs)
                info.Permission = {Permission.SystemManager.Audit.ProcessLogs}
            Case "nAuditObjectLogs"
                info.PageType = GetType(ctlAuditObjectLogs)
                info.Permission = {Permission.SystemManager.Audit.BusinessObjectsLogs}
            Case "nAuditLogs"
                info.PageType = GetType(ctlAuditLogs)
                info.Permission = {Permission.SystemManager.Audit.AuditLogs}
            Case "nAuditStatistics"
                info.PageType = GetType(ctlAuditStatistics)
                info.Permission = {Permission.SystemManager.Audit.Statistics}
            Case "nAuditAlerts"
                info.PageType = GetType(ctlAuditAlerts)
                info.Permission = {Permission.SystemManager.Audit.Alerts}
            Case "nAuditDesignControl"
                info.PageType = GetType(ctlAuditDesignControl)
                info.Permission = {Permission.SystemManager.Audit.ConfigureDesignControls, Permission.SystemManager.Audit.ViewDesignControls}
            Case "nSystem", "nSystemSettings"
                info.PageType = GetType(ctlSystemSettings)
                info.Permission = {Permission.SystemManager.System.Settings}
            Case "nSystemWebConnectionSettings"
                info.PageType = GetType(ctlSystemWebConnectionSettings)
                info.Permission = {Permission.SystemManager.BusinessObjects.WebConnectionSettings}
            Case "nSystemLicense"
                info.PageType = GetType(ctlSystemLicense)
                info.Permission = {Permission.SystemManager.System.License}
                info.Licensed = True 'Everyone is 'licensed' to use the license entry page
            Case "nSystemArchiving"
                info.PageType = GetType(ctlSystemArchiving)
                info.Permission = {Permission.SystemManager.System.Archiving}
            Case "nSystemScheduler"
                info.PageType = GetType(ctlSystemScheduler)
                info.Permission = {Permission.SystemManager.System.Scheduler}
            Case "nSystemCalendar"
                info.PageType = GetType(ctlSystemCalendar)
                info.Permission = {Permission.SystemManager.System.Calendars}
            Case "nSystemFonts"
                info.PageType = GetType(ctlSystemFonts)
                info.Permission = {Permission.SystemManager.System.Fonts}
            Case "nWorkQueueSnapshots"
                info.PageType = GetType(ctlWorkQueueSnapshots)
                info.Permission = {Permission.SystemManager.System.Reporting}
            Case "nSystemReports"
                info.PageType = GetType(ctlSystemReports)
                info.Permission = {Permission.SystemManager.System.Reporting}
            Case "nSkills", "nSkillsManagement"
                info.PageType = GetType(ctlSystemSkillsManagement)
                info.Permission = {Permission.Skills.ViewSkill, Permission.Skills.ManageSkill}
            Case "documentProcessingNode", "documentProcessingQueuesNode"
                info.PageType = GetType(ctlSystemDocumentProcessingQueueManagement)
                info.Permission = {Permission.DocumentProcessing.Configure}
            Case "nDataGateways", "nDataGatewaySettings"
                info.PageType = GetType(ctlDataPipelineSettings)
                info.Permission = Permission.SystemManager.DataGateways.ImpliedConfiguration
            Case "nDataGatewayConfiguration"
                info.PageType = GetType(ctlDataGatewayConfigurationContainer)
                info.Permission = Permission.SystemManager.DataGateways.ImpliedConfiguration
            Case Else
                Throw New InvalidArgumentException("Unknown system manager area")
        End Select
        Return info
    End Function

    ''' <summary>
    ''' Gets the control's help file name.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Dim page As IHelp = TryCast(mCurrentPage, IHelp)
        If page IsNot Nothing Then
            Return page.GetHelpFile()
        End If
        Return "frmSystemManager.htm"
    End Function

    Private Sub ShowPage(ByVal pageType As Type, ByVal mode As ProcessType, ByVal title As String)

        ' Check first that we're not selecting the same thing again - this can happen
        ' more so than before because the heading node now selects the detail for its
        ' first node
        If mCurrentPage IsNot Nothing AndAlso mCurrentPage.GetType() Is pageType Then
            Dim pageMode As IMode = TryCast(mCurrentPage, IMode)
            If pageMode IsNot Nothing AndAlso pageMode.Mode = mode Then Return
        End If

        Dim page As Control
        If mCachedPages.ContainsKey(pageType) Then
            page = mCachedPages(pageType)
        Else
            page = CType(Activator.CreateInstance(pageType), Control)
            ' We don't want to cache any 'stubborn' child pages, not least because the data
            ' will get stuck if the user cancels their changes.
            If Not TypeOf page Is IStubbornChild Then
                mCachedPages.Add(pageType, page)
            End If
        End If

        Dim m As IMode = TryCast(page, IMode)
        If m IsNot Nothing Then
            m.Mode = mode
        End If

        Dim perm As IPermission = TryCast(page, IPermission)
        If perm IsNot Nothing AndAlso
         Not User.Current.HasPermission(perm.RequiredPermissions) Then
            ShowPermissionMessage()
            Exit Sub
        End If

        Me.SuspendLayout()
        lblSystemArea.Text = title

        If mCurrentPage IsNot Nothing Then
            splitPanel.Panel2.Controls.Remove(mCurrentPage)

            If TypeOf mCurrentPage Is IStubbornChild Then
                mCurrentPage.Dispose()
            End If
        End If

        Dim child As IChild = TryCast(page, IChild)
        If child IsNot Nothing Then child.ParentAppForm = mParent
        page.Dock = DockStyle.Fill
        clsFont.SetFont(page)
        splitPanel.Panel2.Controls.Add(page)
        splitPanel.Panel2.Controls.SetChildIndex(page, 0) 'Needed for dock ordering.


        Me.ResumeLayout(True)

        mCurrentPage = page
        ConfigureMenus()

        ' maintain tab focus on the treecontrol
        TabIndex = tvNavigationTree.TabIndex
        splitPanel.Focus()
        splitPanel.ActiveControl = mCurrentPage

    End Sub

    Private Sub ShowPermissionMessage()
        UserMessage.Show(My.Resources.ctlSystemManager_YouDoNotHavePermissionToAccessThisPartOfTheSystemIfYouBelieveThatThisIsIncorrec)
    End Sub

    ''' <summary>
    ''' Hot swap between the menu Burger or Refresh or Invisible.
    ''' Burger Menu takes priority over Refresh
    ''' </summary>
    Private Sub ConfigureMenus()
        Dim btnHandler = TryCast(mCurrentPage, IMenuButtonHandler)

        MenuButtonContextMenuStrip = btnHandler?.MenuStrip
        RemoveHandler mMenuButton.Click, AddressOf RefreshButton_Click

        If MenuButtonContextMenuStrip IsNot Nothing Then
            mMenuButton.Visible = True
            mMenuButton.Image = ToolImages.Menu_Button_16x16
        ElseIf TypeOf mCurrentPage Is IRefreshable Then
            mMenuButton.Visible = True
            mMenuButton.Image = ToolImages.Refresh_24x24

            AddHandler mMenuButton.Click, AddressOf RefreshButton_Click
        Else
            mMenuButton.Visible = False
        End If
    End Sub

#End Region

#Region " Event Handlers "

    Private Sub ctlSystemManager_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim info As PageInfo = SetEnabledByPermission()
        If info IsNot Nothing Then
            ShowPage(info.PageType, info.ProcessType, info.PageTitle)
        End If
    End Sub

    ''' <summary>
    ''' Handles the point before a selection is made in the treeview, ensuring that
    ''' it is safe to leave the current selected panel.
    ''' </summary>
    Private Sub HandleBeforeSelect(sender As Object, e As TreeViewCancelEventArgs) _
        Handles tvNavigationTree.BeforeSelect

        If mParent.IsChangingTab Then
            e.Cancel = True
            Return
        End If

        e.Cancel = Not CanLeave()
    End Sub

    Private Sub tvNavigationTree_AfterSelect(ByVal sender As Object, ByVal e As TreeViewEventArgs) _
        Handles tvNavigationTree.AfterSelect
        Dim info As PageInfo = TryCast(e.Node.Tag, PageInfo)
        If info IsNot Nothing Then
            If Not User.Current.HasPermission(info.Permission) OrElse Not info.Licensed Then
                ShowPermissionMessage()
                Exit Sub
            Else
                ShowPage(info.PageType, info.ProcessType, info.PageTitle)
            End If
        End If
    End Sub

    Private Sub RefreshButton_Click(sender As Object, e As EventArgs)
        TryCast(mCurrentPage, IRefreshable)?.RefreshView()
    End Sub
#End Region

End Class
