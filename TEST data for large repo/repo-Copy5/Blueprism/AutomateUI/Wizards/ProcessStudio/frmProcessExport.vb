Imports System.IO

Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.AutomateAppCore.clsServer
Imports BluePrism.Core.Compression

''' Project  : Automate
''' Class    : frmProcessExport
''' 
''' <summary>
''' A wizard for selecting a process to export.
''' </summary>
Friend Class frmProcessExport
    Inherits frmWizard
    Implements IPermission

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
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents pnlFile As System.Windows.Forms.Panel
    Friend WithEvents mProcessLocator As ProcessBackedMemberTreeListView
    Friend WithEvents pnlList As System.Windows.Forms.Panel
    Friend WithEvents pnlWizardType As System.Windows.Forms.Panel
    Friend WithEvents lblQuestion As System.Windows.Forms.Label
    Friend WithEvents mProcessTypeChooser As AutomateUI.ctlProcessOrObject
    Friend WithEvents lblWarning As System.Windows.Forms.Label
    Friend WithEvents pbWarning As System.Windows.Forms.PictureBox
    Friend WithEvents mPathFinder As ctlPathFinder
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmProcessExport))
        Dim TreeListViewItemCollectionComparer1 As AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer = New AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer()
        Me.pnlList = New System.Windows.Forms.Panel()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.mProcessLocator = New AutomateUI.ProcessBackedMemberTreeListView()
        Me.pnlWizardType = New System.Windows.Forms.Panel()
        Me.mProcessTypeChooser = New AutomateUI.ctlProcessOrObject()
        Me.lblQuestion = New System.Windows.Forms.Label()
        Me.pnlFile = New System.Windows.Forms.Panel()
        Me.lblWarning = New System.Windows.Forms.Label()
        Me.pbWarning = New System.Windows.Forms.PictureBox()
        Me.mPathFinder = New AutomateUI.ctlPathFinder()
        Me.pnlList.SuspendLayout()
        Me.pnlWizardType.SuspendLayout()
        Me.pnlFile.SuspendLayout()
        CType(Me.pbWarning, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        '
        'btnNext
        '
        resources.ApplyResources(Me.btnNext, "btnNext")
        '
        'btnBack
        '
        resources.ApplyResources(Me.btnBack, "btnBack")
        '
        'objBluebar
        '
        resources.ApplyResources(Me.objBluebar, "objBluebar")
        '
        'pnlList
        '
        Me.pnlList.Controls.Add(Me.Label4)
        Me.pnlList.Controls.Add(Me.mProcessLocator)
        resources.ApplyResources(Me.pnlList, "pnlList")
        Me.pnlList.Name = "pnlList"
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.Name = "Label4"
        '
        'mProcessLocator
        '
        resources.ApplyResources(Me.mProcessLocator, "mProcessLocator")
        Me.mProcessLocator.BackColor = System.Drawing.Color.White
        Me.mProcessLocator.CausesValidation = False
        TreeListViewItemCollectionComparer1.Column = 0
        TreeListViewItemCollectionComparer1.SortOrder = System.Windows.Forms.SortOrder.Ascending
        Me.mProcessLocator.Comparer = TreeListViewItemCollectionComparer1
        Me.mProcessLocator.FocusedItem = Nothing
        Me.mProcessLocator.MultiLevelSelect = True
        Me.mProcessLocator.Name = "mProcessLocator"
        Me.mProcessLocator.ShowExposedWebServiceName = False
        Me.mProcessLocator.ShowDocumentLiteralFlag = False
        Me.mProcessLocator.UseLegacyNamespaceFlag = False
        Me.mProcessLocator.UseCompatibleStateImageBehavior = False
        '
        'pnlWizardType
        '
        Me.pnlWizardType.Controls.Add(Me.mProcessTypeChooser)
        Me.pnlWizardType.Controls.Add(Me.lblQuestion)
        resources.ApplyResources(Me.pnlWizardType, "pnlWizardType")
        Me.pnlWizardType.Name = "pnlWizardType"
        '
        'mProcessTypeChooser
        '
        resources.ApplyResources(Me.mProcessTypeChooser, "mProcessTypeChooser")
        Me.mProcessTypeChooser.Name = "mProcessTypeChooser"
        '
        'lblQuestion
        '
        resources.ApplyResources(Me.lblQuestion, "lblQuestion")
        Me.lblQuestion.Name = "lblQuestion"
        '
        'pnlFile
        '
        Me.pnlFile.Controls.Add(Me.lblWarning)
        Me.pnlFile.Controls.Add(Me.pbWarning)
        Me.pnlFile.Controls.Add(Me.mPathFinder)
        resources.ApplyResources(Me.pnlFile, "pnlFile")
        Me.pnlFile.Name = "pnlFile"
        '
        'lblWarning
        '
        resources.ApplyResources(Me.lblWarning, "lblWarning")
        Me.lblWarning.Name = "lblWarning"
        '
        'pbWarning
        '
        Me.pbWarning.Image = Global.AutomateUI.My.Resources.ToolImages.Warning_16x16
        resources.ApplyResources(Me.pbWarning, "pbWarning")
        Me.pbWarning.Name = "pbWarning"
        Me.pbWarning.TabStop = False
        '
        'mPathFinder
        '
        resources.ApplyResources(Me.mPathFinder, "mPathFinder")
        Me.mPathFinder.Filter = Nothing
        Me.mPathFinder.InitialDirectory = Nothing
        Me.mPathFinder.Mode = AutomateUI.ctlPathFinder.PathModes.Save
        Me.mPathFinder.Name = "mPathFinder"
        Me.mPathFinder.SuggestedFilename = Nothing
        '
        'frmProcessExport
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.pnlFile)
        Me.Controls.Add(Me.pnlList)
        Me.Controls.Add(Me.pnlWizardType)
        Me.Name = "frmProcessExport"
        Me.Title = "The process or business object will be copied into a file"
        Me.Controls.SetChildIndex(Me.btnBack, 0)
        Me.Controls.SetChildIndex(Me.btnNext, 0)
        Me.Controls.SetChildIndex(Me.pnlWizardType, 0)
        Me.Controls.SetChildIndex(Me.pnlList, 0)
        Me.Controls.SetChildIndex(Me.btnCancel, 0)
        Me.Controls.SetChildIndex(Me.pnlFile, 0)
        Me.Controls.SetChildIndex(Me.objBluebar, 0)
        Me.pnlList.ResumeLayout(False)
        Me.pnlWizardType.ResumeLayout(False)
        Me.pnlWizardType.PerformLayout()
        Me.pnlFile.ResumeLayout(False)
        CType(Me.pbWarning, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

#End Region

#Region "Members"

    ''' <summary>
    ''' Enumeration of the pages which map onto the current step in the wizard.
    ''' </summary>
    Private Enum Pages As Integer
        SelectionPage = 0
        ChooseProcessPage = 1
        ChooseOutputFilePage = 2
        FinalPage = 3
    End Enum

    ' The process id either selected (if called from main window)
    ' or passed if called from process studio
    Private mProcessId As Guid

    ''' <summary>
    ''' The initial type that this wizard was initialised with.
    ''' </summary>
    Private mInitialMode As WizardType

    ''' <summary>
    ''' Gets the associated permission level.
    ''' </summary>
    ''' <value>The permission level</value>
    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) Implements IPermission.RequiredPermissions
        Get
            Select Case mWizardType
                Case WizardType.BusinessObject
                    Return Permission.ByName(Permission.ObjectStudio.ExportBusinessObject)
                Case WizardType.Process
                    Return Permission.ByName(Permission.ProcessStudio.ExportProcess)
                Case WizardType.Selection
                    Return Permission.ByName(Permission.ObjectStudio.ExportBusinessObject,
                                             Permission.ProcessStudio.ExportProcess)
            End Select
            Return Permission.None
        End Get
    End Property

    ''' <summary>
    ''' The current selected process name.
    ''' </summary>
    Private ReadOnly Property SelectedProcessName() As String
        Get
            ' The current process name is in the XML - anything else could be out
            ' of date, so that has to win in any contest of priorities.
            Dim name As String = clsProcess.ExtractProcessName(mXML)
            If name <> "" Then Return name
            ' OK - it's not in the process, try the process locator
            Try
                Return mProcessLocator.SelectedMembers.First.Name
            Catch
                ' Right. Well, er, not a lot we can do then.
                If mWizardType = WizardType.BusinessObject Then Return My.Resources.frmProcessExport_NoBusinessObjectSelected
                Return My.Resources.frmProcessExport_NoProcessSelected
            End Try
        End Get
    End Property
    Private mXML As String

    Private mProcessAttributes As ProcessAttributes

#End Region

#Region "New"

    Public Sub New(ByVal wizType As WizardType)
        Me.New(Nothing, "", wizType)
    End Sub

    Public Sub New(ByVal procId As Guid, ByVal process As clsProcess, ByVal wizType As WizardType)
        Me.New(procId, "", wizType)
    End Sub

    Public Sub New(ByVal procId As Guid, ByVal procXml As String, ByVal wizType As WizardType)
        MyBase.New(wizType)
        mInitialMode = wizType

        Me.InitializeComponent()

        ResetTitleBar()

        mProcessLocator.MultiSelect = False
        'mProcessLocator.Enabled = False
        'mProcessLocator.IncludePreviewContextMenuItem = False
        mProcessId = procId
        mXML = procXml

        If procId <> Nothing Then
            SetStep(Pages.ChooseOutputFilePage)
            SetMaxSteps(1)
        ElseIf wizType = WizardType.Selection Then
            SetStep(Pages.SelectionPage)
            SetMaxSteps(2)
        Else
            SetStep(Pages.ChooseProcessPage)
            SetMaxSteps(1)
        End If

    End Sub

#End Region

#Region "UpdatePage"

    ''' <summary>
    ''' Moves the wizard along to the next step.
    ''' </summary>
    Protected Overrides Sub UpdatePage()

        Select Case MyBase.GetStep

            Case Pages.SelectionPage
                SetupSelectionPage()

            Case Pages.ChooseProcessPage
                SetupChooseProcessPage()

            Case Pages.ChooseOutputFilePage
                SetupChooseOutputFilePage()

            Case Pages.FinalPage
                SetupFinalPage()
        End Select

    End Sub

    Private Sub SetupSelectionPage()
        ' This page should only be reachable if the selection is to be
        ' chosen - either initially or by pressing the back button.
        ' Ensure the wizard type is reset accordingly.
        mWizardType = WizardType.Selection

        UpdatePanelTextToProcessOrObject(WizardType.Selection, pnlWizardType)
        ResetTitleBar()

        ShowPage(pnlWizardType)
    End Sub

    Private Sub SetupChooseProcessPage()
        If mWizardType = WizardType.Selection Then
            mWizardType = mProcessTypeChooser.ChosenType
            ' If they've still not chosen (surely can't happen) default to Process
            If mWizardType = WizardType.Selection Then mWizardType = WizardType.Process
        End If
        ' If we're choosing the process, ensure that the XML is reset
        ' so we don't accidentally export the wrong process (see bug #5347)
        mProcessId = Nothing
        mXML = Nothing

        UpdatePanelTextToProcessOrObject(mWizardType, pnlList)

        Select Case mWizardType
            Case WizardType.Process
                mProcessLocator.TreeType = GroupTreeType.Processes
            Case WizardType.BusinessObject
                mProcessLocator.TreeType = GroupTreeType.Objects
            Case Else
                mProcessLocator.TreeType = GroupTreeType.None
        End Select
        mProcessLocator.Filter = Function(mem) Not mem.IsRetired

        ShowPage(pnlList)
    End Sub

    Private Function CanExportProcess() As Boolean
        Dim processPermissions = gSv.GetEffectiveMemberPermissionsForProcess(mProcessId)

        Return processPermissions.HasPermission(User.Current, GetRequiredExportPermission())
    End Function

    Private Function GetRequiredExportPermission() As String
        Return If(
            mWizardType = WizardType.Process,
            Permission.ProcessStudio.ExportProcess,
            Permission.ObjectStudio.ExportBusinessObject)
    End Function

    Private Sub SetupChooseOutputFilePage()
        ' Get the process ID (if not passed in).
        If mProcessId = Nothing Then
            ' Check something is selected...
            mProcessId = mProcessLocator.ProcessId
            If mProcessId = Nothing Then
                ShowMessage(My.Resources.frmProcessExport_No0WasSelected)
                Rollback()
                Exit Sub
            End If
            ' ...and it isn't a group
            If (mProcessLocator.GetSelectedMembers(Of ProcessBackedGroupMember).Count <= 0) Then
                ShowMessage(My.Resources.frmProcessExport_CannotExportAGroup)
                Rollback()
                Exit Sub
            End If
        End If

        If Not CanExportProcess() Then
            frmApplication.ShowPermissionMessage()
            Rollback()
            Exit Sub
        End If
        ' Get the XML if it's not already set.
        If mXML = "" Then
            Try
                Dim processDetail = gSv.GetProcessXMLAndAssociatedData(mProcessId)
                mXML = CStr(IIf(processDetail.Zipped, GZipCompression.Decompress(processDetail.Xml), processDetail.Xml))
                mProcessAttributes = processDetail.Attributes
            Catch ex As Exception
                UserMessage.Show(My.Resources.frmProcessExport_FailedToRetrieveProcessDefinition, ex)
                Rollback()
                Exit Sub
            End Try

            If String.IsNullOrEmpty(mXML) Then
                ' Well, something went wrong....
                UserMessage.Show(My.Resources.frmProcessExport_FailedToRetrieveXmlFromDatabaseEmptyValueReturned)
                Rollback()
                Exit Sub
            End If
        End If

        UpdatePanelTextToProcessOrObject(mWizardType, pnlFile)
        ShowPage(pnlFile)

        Try
            Dim objectType = IIf(mWizardType = WizardType.BusinessObject, My.Resources.frmProcessExport_Object, My.Resources.frmProcessExport_Process)
            Dim extension As String = GetExtension()

            mPathFinder.SuggestedFilename = String.Format(My.Resources.frmProcessExport_BPA012,
                                                          objectType,
                                                      Me.SelectedProcessName,
                                                          extension)
        Catch ex As Exception
            UserMessage.Show(My.Resources.frmProcessExport_FailedToExportProcessInvalid, ex)
            Rollback()
            Exit Sub
        End Try
        
        mPathFinder.Filter = clsFileSystem.GetFileFilterString(GetFileExtension)
        mPathFinder.Focus()
        ShowWarning()
    End Sub

    Private Function GetExtension() As String
        Select Case True
            Case mWizardType = WizardType.Process
                Return clsProcess.ProcessFileExtension
            Case mWizardType = WizardType.BusinessObject
                Return clsProcess.ObjectFileExtension
            Case Else
                Return "xml"
        End Select
    End Function

    Private Function GetFileExtension() As clsFileSystem.FileExtensions

        Select Case True
            Case mWizardType = WizardType.Process
                Return clsFileSystem.FileExtensions.bpprocess
            Case mWizardType = WizardType.BusinessObject
                Return clsFileSystem.FileExtensions.bpobject
            Case Else
                Return clsFileSystem.FileExtensions.XML
        End Select
    End Function

    Private Sub SetupFinalPage()
        Dim errorDescription As String = Nothing

        If mPathFinder.txtFile.Text = "" Then
            UserMessage.Show(My.Resources.frmProcessExport_YouMustEnterAValidFileName)
            Rollback()
            Exit Sub
        End If

        If File.Exists(mPathFinder.txtFile.Text) Then
            If UserMessage.YesNo(
                My.Resources.frmProcessExport_AFileWithThisNameAlreadyExistsInTheSpecifiedLocationTheExistingFileWillBeReplac
                ) <> MsgBoxResult.Yes Then
                Rollback()
                Exit Sub
            End If
        End If

        If clsProcess.ExportXMLToFile(mXML, mProcessId, mProcessAttributes, mPathFinder.txtFile.Text, errorDescription) Then

            Try
                'Record event in Audit Log...
                If mWizardType = WizardType.BusinessObject Then
                    gSv.AuditRecordBusinessObjectEvent(BusinessObjectEventCode.ExportBusinessObject, mProcessId, String.Format(My.Resources.frmProcessExport_0TheTargetFileWas1, msAuditComments, mPathFinder.txtFile.Text), Nothing, Nothing)
                Else
                    gSv.AuditRecordProcessEvent(ProcessEventCode.ExportProcess, mProcessId, String.Format(My.Resources.frmProcessExport_0TheTargetFileWas1, msAuditComments, mPathFinder.txtFile.Text), Nothing, Nothing)
                End If
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.ctlProcessViewer_WarningAuditRecordingFailedWithError0, ex.Message))  
            End Try

            Close()
        Else
            UserMessage.Show(errorDescription)
            Rollback()
        End If
    End Sub

    Private Sub ShowMessage(msg As String)
        Dim type As String
        Select Case mWizardType
            Case WizardType.Process
                type = My.Resources.frmProcessExport_ProcessLC
            Case WizardType.BusinessObject
                type = My.Resources.frmProcessExport_BusinessObject
            Case Else
                type = My.Resources.frmProcessExport_Unknown
        End Select
        UserMessage.Show(String.Format(msg, type))
    End Sub

    Private Sub ShowWarning()
        pbWarning.Visible = False
        lblWarning.Visible = False
        If mWizardType <> WizardType.BusinessObject Then Exit Sub

        Dim parent As String = gSv.GetParentReference(mProcessId)
        If parent = String.Empty Then Exit Sub

        lblWarning.Text = String.Format(My.Resources.frmProcessExport_ThisObjectDependsOnTheApplicationModelOf0AndMayNotFunctionCorrectlyInEnvironmen,
         parent)
        lblWarning.Visible = True
        pbWarning.Visible = True
    End Sub

    Private Sub ResetTitleBar()
        Me.Text = My.Resources.frmProcessExport_ExportProcessOrBusinessObject
        Me.Title = My.Resources.frmProcessExport_TheProcessOrBusinessObjectWillBeCopiedIntoAFile
    End Sub
#End Region


#Region "GetHelpFile"
    ''' <summary>
    ''' Gets the name of the associated help file.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmProcessExport.htm"
    End Function

#End Region

#Region "ProcessLocateControl1_KeyDown"

    Private Sub ProcessLocateControl1_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles MyBase.KeyDown
        Select Case e.KeyCode
            Case Keys.Enter
                If MyBase.GetStep = 0 Then Me.NextPage() 'ie if we are looking at the list of processes
        End Select
    End Sub

#End Region

    ''' <summary>
    ''' Private member to store public property AuditComments()
    ''' </summary>
    Private msAuditComments As String
    ''' <summary>
    ''' Comments that will be added to the audit log
    ''' </summary>
    ''' <value></value>
    Public Property AuditComments() As String
        Get
            Return msAuditComments
        End Get
        Set(ByVal value As String)
            msAuditComments = value
        End Set
    End Property

    Private Sub CtlProcessLocate1_ProcessDoubleClick(sender As Object, e As GroupMemberEventArgs) _
        Handles mProcessLocator.GroupItemActivated
        NextPage()
    End Sub

End Class

#Region "frmProcessPageExport"

Friend Class frmProcessPageExport
    Inherits frmProcessExport

    Public Sub New(ByVal gProcessID As Guid, ByVal sXML As String, ByVal iWizardType As WizardType)
        MyBase.New(gProcessID, sXML, iWizardType)
    End Sub

    Public Sub New(ByVal iWizardType As WizardType)
        MyBase.New(iWizardType)
    End Sub

    Private Sub frmProcessExport_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Me.Title = Me.Title.Replace(My.Resources.frmProcessExport_ProcessLC.ToLower, My.Resources.frmProcessExport_ProcessPage.ToLower)
    End Sub

End Class

#End Region
