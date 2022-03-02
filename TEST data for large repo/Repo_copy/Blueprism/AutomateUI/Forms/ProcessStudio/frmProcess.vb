Imports System.IO
Imports System.Linq

Imports AutomateControls
Imports AutomateUI.Classes.UserInterface

Imports BluePrism.AMI
Imports BluePrism.AMI.clsAMI

Imports BluePrism.Core.Expressions
Imports BluePrism.Core.Extensions

Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AutomateProcessCore.Processes

Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.modWin32
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Images
Imports System.Windows.Forms.Integration
Imports AutomateUI.Controls.Widgets.ProcessStudio.Skills.ViewModel
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.ApplicationManager.AMI
Imports BluePrism.Server.Domain.Models

''' Project  : Automate
''' Class    : frmProcess
''' 
''' <summary>
''' Form used to display and edit a process
''' </summary>
Partial Friend Class frmProcess
    Inherits frmForm
    Implements IPermission, IProcessViewingForm, IEnvironmentColourManager

#Region " Class Scope Declarations "

    ''' <summary>
    ''' Class to compare toolstrips by location. They actually perform the comparison
    ''' on the name of the toolstrip, getting the location from the owning frmProcess
    ''' </summary>
    Private Class ToolstripComparer : Implements IComparer(Of String)

        ' The owner of this comparer
        Private mOwner As frmProcess

        ''' <summary>
        ''' Creates a new comparer owned by the given form
        ''' </summary>
        ''' <param name="owner"></param>
        Public Sub New(ByVal owner As frmProcess)
            mOwner = owner
        End Sub

        ''' <summary>
        ''' Compares the two names of toolstrips by getting their location and
        ''' comparing them.
        ''' </summary>
        ''' <param name="a">The name of the first toolstrip to compare</param>
        ''' <param name="b">The name of the second toolstrip to compare</param>
        ''' <returns>1, 0 or -1 if the toolstrip named 'a' is "greater than" (ie.
        ''' located after) the toolstrip named 'b' in the owning process form.
        '''  </returns>
        Public Function Compare(ByVal a As String, ByVal b As String) As Integer _
         Implements IComparer(Of String).Compare

            Dim aloc As Point = mOwner.mToolStrips(a).Location
            Dim bloc As Point = mOwner.mToolStrips(b).Location

            If aloc.Y > bloc.Y Then Return 1
            If aloc.Y < bloc.Y Then Return -1
            If aloc.X > bloc.X Then Return 1
            If aloc.X < bloc.X Then Return -1
            Return 0

        End Function

    End Class

    ''' <summary>
    ''' The maximum number of undo or redo menu subitems.
    ''' </summary>
    Private Const MaximumUndoLevel As Integer = 50

    ''' <summary>
    ''' A List of the instances of frmProcess currently in use.
    ''' </summary>
    Private Shared mOpenProcessForms As New List(Of frmProcess)

    ''' <summary>
    ''' Gets the already-open instance showing the specified process id.
    ''' </summary>
    ''' <param name="processID">The ID of the process being viewed in the instance
    ''' sought.</param>
    ''' <returns>Returns the instance if found, otherwise Nothing.</returns>
    Public Shared Function GetInstance(ByVal processID As Guid) As frmProcess
        For Each f As frmProcess In mOpenProcessForms
            If f.GetProcessID() = processID Then Return f
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Gets all frmProcess instances maintained by this class.
    ''' </summary>
    ''' <returns>An IEnumerable over all instances of frmProcess currently known
    ''' about in this app domain</returns>
    Public Shared Function GetAllInstances() As IEnumerable(Of frmProcess)
        Return mOpenProcessForms
    End Function

    ''' <summary>
    ''' Gets the already-open instance showing the specified process.
    ''' id, mode, etc.
    ''' </summary>
    ''' <param name="processID">The ID of the process being viewed in the instance
    ''' sought.</param>
    ''' <param name="mode">The mode of the instance sought.</param>
    ''' <returns>Returns the instance if found, otherwise Nothing.</returns>
    Public Shared Function GetInstance(ByVal processID As Guid, ByVal mode As ProcessViewMode) As frmProcess
        For Each f As frmProcess In mOpenProcessForms
            If f.GetProcessID() = processID Then Return f
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Gets the instance which is displaying the supplied process instance.
    ''' </summary>
    ''' <param name="Process">The process instance, whose parent window is sought.</param>
    ''' <returns>Returns the instance if it is found, or Nothing otherwise.</returns>
    Public Shared Function GetInstance(ByVal process As clsProcess) As frmProcess
        For Each f As frmProcess In mOpenProcessForms
            If f.mProcessViewer.Process Is process Then Return f
        Next
        Return Nothing
    End Function

#End Region

#Region " Published Events "

    ''' <summary>
    ''' Event raised when the process open in this form is saved to the database.
    ''' </summary>
    Public Event Saved As ProcessEventHandler

#End Region

#Region " Properties "

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
            lblStatus.BackColor = value
            stsBar.BackColor = value
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
            lblStatus.ForeColor = value
        End Set
    End Property

    ''' <summary>
    ''' The process viewer embedded in this process form
    ''' </summary>
    Public ReadOnly Property ProcessViewer() As ctlProcessViewer
        Get
            Return mProcessViewer
        End Get
    End Property

    ''' <summary>
    ''' Gets the associated permission level.
    ''' </summary>
    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) Implements IPermission.RequiredPermissions
        Get
            ' Default to Process mode, but if the mode is set and is for objects,
            ' ensure that the permission reflects that
            Dim perms As New List(Of String)
            If mProcessViewer IsNot Nothing AndAlso
             mProcessViewer.ModeIsObjectStudio() Then
                perms.AddRange({Permission.ObjectStudio.CreateBusinessObject,
                            Permission.ObjectStudio.EditBusinessObject,
                            Permission.ObjectStudio.ExecuteBusinessObject,
                            Permission.ObjectStudio.ViewBusinessObject})
            Else
                perms.AddRange({Permission.ProcessStudio.CreateProcess,
                            Permission.ProcessStudio.EditProcess,
                            Permission.ProcessStudio.ExecuteProcess,
                            Permission.ProcessStudio.ViewProcess})
            End If

            ' We could go the whole hog and further check the viewer mode
            ' (Edit / Clone / etc), but this is more of a sanity check - more
            ' specific permission tests should have been done before we reach here
            Return Permission.ByName(perms)
        End Get
    End Property

    ''' <summary>
    ''' The default time (in milliseconds) that messages will be displayed on the 
    ''' status bar for. To use a one-off custom time, use the appropriate overload
    ''' of SetStatusBarText().
    ''' The initial value of this property is 3000ms (ie. 3 seconds)
    ''' </summary>
    Public Property DefaultStatusBarMessageDuration() As Integer _
     Implements IProcessViewingForm.DefaultStatusBarMessageDuration
        Get
            Return mStatusBarMessageDuration
        End Get
        Set(ByVal Value As Integer)
            mStatusBarMessageDuration = Value
        End Set
    End Property

#End Region

#Region " Members "

    ' The popup form for data item watches.
    Public WithEvents mDataWatchForm As frmDataItemWatches

    ' The validation results form that is open, or Nothing if there isn't one.
    Private WithEvents mValidateResultsForm As frmValidateResults

    ' The application modeller form used in this business object.
    Private WithEvents mAppModellerForm As frmIntegrationAssistant

    ' A slider bar to embed in the debug button's speed menu dropdown
    Private WithEvents mDebugSpeedSlider As New ctlDebugTrackbar()

    ' The form dealing with the process MI
    Private WithEvents mProcessMIForm As frmProcessMI

    ' The session ID that this process form is operating in
    Private mgSessionID As Guid

    ' The ID of the process on display in this form. Guid.Empty if not known or set
    Private mProcessID As Guid

    ' private member to store public property DefaultStatusBarMessageDuration().
    Private mStatusBarMessageDuration As Integer = 3000

    ' The name of the process
    Private mProcessName As String

    ' The XML of the process - may be null if an already built process is open
    Private mXML As String

    ' The description of the process
    Private mDescription As String

    ' The process currently open - may be null if not modelling a pre-built process
    Private mProcess As clsProcess

    ' Whether this form was maximised or not when full screen mode was entered
    Private mPreFullScreenMaximized As Boolean

    ' Flag indicating whether this form is in full screen mode or not
    Private mFullScreen As Boolean

    ' Flag set while the format controls are being set to ensure that their
    ' event handlers aren't triggered
    Private mSettingFormat As Boolean

    ' A collection of toolstrips to hide according to the edit mode
    Private mHiddenToolStrips As ICollection(Of ToolStrip)

    ' Maps the default display styles to their tool strip buttons.
    Private mToolStripItemStyles As IDictionary(Of ToolStripItem, ToolStripItemDisplayStyle)

    ' The toolstrips in this form, keyed on their name
    Private mToolStrips As IDictionary(Of String, ToolStrip)

    ' Token used to close loading form
    Private mLoadingFormToken As LoadingFormToken

    Private mSkillsToolbar As SkillsToolbar

    ''' <summary>
    ''' The timer responsible for closing the loading window
    ''' </summary>
    ''' <remarks>
    ''' The theory here is that as the UI thread is locked during loading, the timer will
    ''' be unable to run during that time. Once the UI thread is unblocked the timer will
    ''' tick and the form will close.
    ''' This is initialised here rather than in the designer as adding elements to the form
    ''' with the visual designer breaks the form.
    ''' </remarks>
    Private WithEvents mLoadingWindowCloseTimer As Timer

    'Flag indicating that the user has permission to edit this application model.
    Private mHasPermissionToEdit As Boolean = False

    Private const BrowserTimeout As String = "30"

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new process form, opening the given process into it.
    ''' </summary>
    ''' <param name="mode">The viewing mode</param>
    ''' <param name="proc">An existing clsProcess object to use, or Nothing for
    ''' the original behaviour of creating one. If a clsProcess is supplied, it is
    ''' considered to be owned by the caller, and this form will close when the process
    ''' completes or fails execution.</param>
    ''' <param name="processId">The ID of the process</param>
    Public Sub New(
     ByVal mode As ProcessViewMode, ByVal proc As clsProcess, ByVal processId As Guid)
        Me.New(mode, processId, proc.Name, proc.Description, proc, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new, empty, process form using the given process properties.
    ''' </summary>
    ''' <param name="mode">The viewing mode</param>
    ''' <param name="processId">The process id</param>
    ''' <param name="name">The process name</param>
    ''' <param name="description">The process description</param>
    Public Sub New(ByVal mode As ProcessViewMode,
     ByVal processId As Guid, ByVal name As String, ByVal description As String)
        Me.New(mode, processId, name, description, Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new process form based on a process described by the given XML.
    ''' </summary>
    ''' <param name="mode">The mode in which the form should be opened.
    ''' </param>
    ''' <param name="xml">The XML to use in the form.</param>
    ''' <param name="processId">The ID of the process to use</param>
    Public Sub New(ByVal mode As ProcessViewMode, ByVal xml As String, ByVal processId As Guid)
        Me.New(mode, processId, Nothing, Nothing, Nothing, xml)
    End Sub

    ''' <summary>
    ''' Common constructor - creates a new process form with the given properties.
    ''' </summary>
    ''' <param name="mode"></param>
    ''' <param name="processId"></param>
    ''' <param name="name"></param>
    ''' <param name="description"></param>
    ''' <param name="process"></param>
    Private Sub New(ByVal mode As ProcessViewMode, ByVal processId As Guid,
     ByVal name As String, ByVal description As String,
     ByVal process As clsProcess, ByVal xml As String)

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        Dim bounds As Rectangle = Screen.GetWorkingArea(Me)
        If bounds.Height < Me.Height Then
            Me.Height = bounds.Height
        End If

        Dim delta As Integer = 0
        If lExpression.Right < exprEdit.Left - lExpression.Margin.Left Then
            delta = exprEdit.Left - lExpression.Right
            exprEdit.Left -= delta
        ElseIf lExpression.Right > exprEdit.Left Then
            delta = lExpression.Right - exprEdit.Left
            exprEdit.Left += delta
            exprEdit.Width -= delta
        End If
        If lStoreIn.Right > objStoreInEdit.Left Then
            delta = lStoreIn.Right - objStoreInEdit.Left
            exprEdit.Width -= delta
        End If
        If delta <> 0 Then
            lStoreIn.Left = exprEdit.Right + exprEdit.Margin.Right + lStoreIn.Margin.Left
            objStoreInEdit.Left = lStoreIn.Right + lStoreIn.Margin.Right
            objStoreInEdit.Width = objStoreInEdit.Width
            delta = (pnlExpressionRow.Width - objStoreInEdit.Margin.Right) - objStoreInEdit.Right
            exprEdit.Width += delta
            lStoreIn.Left += delta
            objStoreInEdit.Left += delta
        End If

        ' Store the strips keyed on their name into a map maintained in this object
        mToolStrips = New Dictionary(Of String, ToolStrip)
        For Each strip As ToolStrip In New ToolStrip() _
         {toolstripStandard, toolstripFont, toolstripDebug, toolstripSearch}
            mToolStrips(strip.Name) = strip
        Next

        ' Store the default DisplayStyle of all tool strip items. Only do this
        mToolStripItemStyles = New Dictionary(Of ToolStripItem, ToolStripItemDisplayStyle)
        For Each strip As ToolStrip In mToolStrips.Values
            For Each item As ToolStripItem In strip.Items
                mToolStripItemStyles.Add(item, item.DisplayStyle)
            Next
        Next

        mHiddenToolStrips = New clsSet(Of ToolStrip)

        ' Store the menu item which affects the visibility of each of the toolstrips
        toolstripStandard.Tag = mnuToolsStandard
        toolstripFont.Tag = mnuToolsFont
        toolstripTools.Tag = mnuToolsTools
        toolstripDebug.Tag = mnuToolsDebug
        toolstripSearch.Tag = mnuToolsSearch

        ' And vice versa
        mnuToolsStandard.Tag = toolstripStandard
        mnuToolsFont.Tag = toolstripFont
        mnuToolsTools.Tag = toolstripTools
        mnuToolsDebug.Tag = toolstripDebug
        mnuToolsSearch.Tag = toolstripSearch

        mProcessViewer.ViewMode = mode
        mProcessID = processId
        mProcessName = name
        mDescription = description
        mProcess = process
        mXML = xml

        EnforceToolbarOrder()

        'New processid needs creating now for auto saves.
        If mProcessViewer.ModeIsCloning() Then
            mProcessViewer.mgNewProcessID = Guid.NewGuid()
        End If

        Dim modeIsObjectStudio = mProcessViewer.ModeIsObjectStudio
        'Hide integration assistant unless this is Object Studio...
        If Not modeIsObjectStudio Then
            btnMenuApplicationModeller.Visible = False
            mnuApplicationModeller.Visible = False
            mnuApplicationModeller.Enabled = False
            btnMenuDebugLaunchApp.Visible = False
            ToolStripSeparator1.Visible = False

            'Hide the element usage report as well...
            mnuElementUsage.Visible = False
            mnuReports.Visible = False

        End If

        'Determine is user has permissions on any folders for processes/Objects so as to be able to create therefore saveAs
        Dim store As IGroupStore = GetGroupStore()
        Dim objectsTree As IGroupTree = Nothing
        Dim creatrePermissions As ICollection(Of String) = New List(Of String)
        Select Case modeIsObjectStudio
            Case False
                creatrePermissions.Add(Permission.ProcessStudio.CreateProcess)
                objectsTree = store.GetTree(GroupTreeType.Processes, Nothing, Nothing, False, False, False)
                objectsTree = objectsTree.GetFilteredView(Function(t) t.MemberType = GroupMemberType.Group, Function(g) g.Permissions.HasPermission(User.Current, creatrePermissions), False)
            Case True
                creatrePermissions.Add(Permission.ObjectStudio.CreateBusinessObject)
                objectsTree = store.GetTree(GroupTreeType.Objects, Nothing, Nothing, False, False, False)
                objectsTree = objectsTree.GetFilteredView(Function(t) t.MemberType = GroupMemberType.Group, Function(g) g.Permissions.HasPermission(User.Current, creatrePermissions), False)
        End Select
        If objectsTree.Root.Count = 0 Then
            Me.mnuSaveAs.Enabled = False
        End If

        'Set Process Search mode to the same as process viewer
        Me.toolstripSearch.ModeIsObjectStudio = modeIsObjectStudio

        SetFileMenuItemText()

        Dim undoRenderer As New clsProcessUndoMenuRenderer(Me)
        mnuEdit.Owner.Renderer = undoRenderer
        btnUndo.Owner.Renderer = undoRenderer
        exprEdit.ProcessViewer = Me.ProcessViewer

        ' Remove ability to create multiple calculations in NHS Edition...
        ' Also don't allow export of packages / releases.
        If Not Licensing.License.CanUse(LicenseUse.MultipleCalculations) Then
            mnuTools.DropDownItems.Remove(mnuMultipleCalculation)
            toolstripTools.Items.Remove(btnMultipleCalculation)
        End If

        If Not Licensing.License.CanUse(LicenseUse.ProcessAlerts) Then
            mnuTools.DropDownItems.Remove(mnuAlert)
            toolstripTools.Items.Remove(btnAlert)
        End If

        If Not Licensing.License.CanUse(LicenseUse.ReleaseManager) Then
            miExportAdhocPackage.Enabled = False
            miExportRelease.Enabled = False
        End If

        mnuApplicationModeller.Text = "&" & ApplicationProperties.ApplicationModellerName
        btnMenuApplicationModeller.Text = ApplicationProperties.ApplicationModellerName
        btnMenuApplicationModeller.ToolTipText = String.Format(My.Resources.frmProcess_OpenThe0, ApplicationProperties.ApplicationModellerName)

        Dim viewPermission = If(modeIsObjectStudio,
            Permission.ObjectStudio.ImpliedViewBusinessObject,
            Permission.ProcessStudio.ImpliedViewProcess)

        Dim permissions = gSv.GetEffectiveMemberPermissionsForProcess(mProcessID)

        If permissions.IsRestricted AndAlso Not permissions.HasPermission(User.Current, viewPermission) Then
            Throw New BluePrismException(My.Resources.frmProcess_YouDoNotHavePermissionToViewThisProcess)
        End If

        Dim editPermission = If(modeIsObjectStudio,
            Permission.ObjectStudio.ImpliedEditBusinessObject,
            Permission.ProcessStudio.ImpliedEditProcess)

        mHasPermissionToEdit =
            permissions.HasPermission(User.Current, editPermission)

    End Sub

    Private Sub EnforceToolbarOrder()

        'Enforce the order of the toolbars as overflow can invert it
        toolstripStandard.Location = New Point(0, toolstripStandard.Location.Y)
        toolstripFont.Location = New Point(toolstripStandard.Width, toolstripFont.Location.Y)

        toolstripDebug.Location = New Point(0, toolstripDebug.Location.Y)
        toolstripSearch.Location = New Point(toolstripDebug.Width, toolstripSearch.Location.Y)
    End Sub
    
#End Region

#Region "Gets and Sets"

    Friend Overrides Property ParentAppForm As frmApplication
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            MyBase.ParentAppForm = value
            If value IsNot Nothing Then
                EnvironmentBackColor = value.EnvironmentBackColor
                EnvironmentForeColor = value.EnvironmentForeColor
            End If
        End Set
    End Property

    ''' <summary>
    ''' Enables process exporting.
    ''' </summary>
    ''' <param name="value">True to enable</param>
    Public Sub SetProcessAsExportable(ByVal value As Boolean)
        mnuExport.Enabled = value
    End Sub

    ''' <summary>
    ''' Enables/disables the menu option that allows the current page to be
    ''' exported.
    ''' </summary>
    ''' <param name="value">Set to true to enable the menu option; false
    ''' to disable it.</param>
    Public Sub SetPageAsExportable(ByVal value As Boolean)
        miExportThisPage.Enabled = value
    End Sub

    ''' <summary>
    ''' Enables start paramters.
    ''' </summary>
    ''' <param name="value">True to enable</param>
    Public Sub SetStartParamsEnabled(ByVal value As Boolean)
        mnuStartParams.Enabled = value
    End Sub

    ''' <summary>
    ''' Sets the notify icon text.
    ''' </summary>
    ''' <param name="s">The text</param>
    Public Sub SetNTFYDebugText(ByVal s As String)
        Me.ntfyDebug.Text = s.Mid(1, 63)
    End Sub

    ''' <summary>
    ''' Enables calculation zoom.
    ''' </summary>
    ''' <param name="value">True to enable</param>
    Public Sub SetCalcZoomEnabledChecked(ByVal value As Boolean)
        Me.mnuCalculationZoom.Checked = value
    End Sub

    ''' <summary>
    ''' Enables Cut, Copy and Paste menu items.
    ''' </summary>
    ''' <param name="value">True to enable the menu items, False to disable them.
    ''' </param>
    Private Sub SetEditMenuCutCopyDeleteEnabled(ByVal value As Boolean)
        Me.mnuEditCut.Enabled = value AndAlso mProcessViewer.ModeIsEditable
        Me.btnCut.Enabled = value AndAlso mProcessViewer.ModeIsEditable
        Me.mnuEditCopy.Enabled = value
        Me.btnCopy.Enabled = value
        Me.mnuEditDelete.Enabled = value AndAlso mProcessViewer.ModeIsEditable
    End Sub

    ''' <summary>
    ''' Enables Properties menu item.
    ''' </summary>
    ''' <param name="value">True to enable</param>
    Public Sub SetMenuPropertiesEnabled(ByVal value As Boolean)
        Me.mnuEditProperties.Enabled = value
    End Sub

    ''' <summary>
    ''' Enables and disables the undo/redo menu items on the edit menu.
    ''' </summary>
    ''' <param name="UndoEnabled">Set to true to enable the undo option.</param>
    ''' <param name="RedoEnabled">Set to true to enable the redo option.</param>
    Public Sub SetEditMenuUndoRedoEnabled(ByVal UndoEnabled As Boolean, ByVal RedoEnabled As Boolean)
        Me.mnuEditUndo.Enabled = UndoEnabled
        Me.mnuEditRedo.Enabled = RedoEnabled
        Me.btnUndo.Enabled = UndoEnabled
        Me.btnRedo.Enabled = RedoEnabled
    End Sub

    ''' <summary>
    ''' gets the process id.
    ''' </summary>
    Public Function GetProcessID() As Guid
        Return mProcessID
    End Function

    ''' <summary>
    ''' Indicates the state of Full Screen mode.
    ''' </summary>
    ''' <returns>True id in Full Screen mode</returns>
    Public Function InFullScreenMode() As Boolean
        Return mFullScreen
    End Function

    ''' <summary>
    ''' Launch the dependency viewer and focus on the passed dependency object.
    ''' </summary>
    ''' <param name="dep"></param>
    Public Sub SelectDependency(dep As clsProcessDependency) Handles mProcessViewer.SelectDependency
        Me.toolstripSearch.SelectDependency(dep)
    End Sub

#End Region

#Region "Debug Button Handlers"

    Private Sub btnStop_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuTaskTrayStop.Click, mnuDebugStop.Click, btnMenuDebugPause.Click
        Try
            mProcessViewer.CloseBreakpointInfo()
            mProcessViewer.DebugAction(ProcessRunAction.Pause, sender Is mnuTaskTrayStop)
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.frmProcess_ErrorStoppingDebugging0, ex.Message))
        End Try
    End Sub

    Private Sub evhDoStep(ByVal sender As Object, ByVal e As System.EventArgs) Handles mnuTaskTrayStep.Click, mnuDebugStep.Click, btnMenuDebugStep.Click
        Try
            If mProcessViewer.HasPropertyWindowsOpen Then
                Throw New InvalidStateException(my.resources.frmProcess_DebuggingIsNotAvailableWhilePropertiesDialogsAreOpen)
            End If
            mProcessViewer.CloseBreakpointInfo()
            mProcessViewer.DebugAction(ProcessRunAction.StepIn, sender Is mnuTaskTrayStep)
        Catch ex as InvalidStateException
            UserMessage.OK(ex.Message)
        Catch ex As Exception
            UserMessage.Show(My.Resources.frmProcess_UnexpectedErrorEx, ex)
        End Try
    End Sub

    Private Sub evhDoStepOver(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuTaskTrayStepOver.Click, mnuDebugStepOver.Click, btnMenuDebugStepOver.Click
        Try
            ' replace the string.format paramater with the text of the menu item or button whe it is implemented under us-8152
            If mProcessViewer.HasPropertyWindowsOpen Then
                Throw New InvalidStateException(my.resources.frmProcess_DebuggingIsNotAvailableWhilePropertiesDialogsAreOpen)
            End If
            mProcessViewer.CloseBreakpointInfo()
            mProcessViewer.DebugAction(ProcessRunAction.StepOver, sender Is mnuTaskTrayStepOver)
        Catch ex as InvalidStateException
            UserMessage.OK(ex.Message)
        Catch ex As Exception
            UserMessage.Show(My.Resources.frmProcess_UnexpectedErrorEx, ex)
        End Try
    End Sub

    Private Sub btnDebugStepOut_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuTaskTrayStepOut.Click, mnuDebugStepOut.Click, btnMenuDebugStepOut.Click
        Try
            If mProcessViewer.HasPropertyWindowsOpen Then
                Throw New InvalidStateException(my.resources.frmProcess_DebuggingIsNotAvailableWhilePropertiesDialogsAreOpen)
            End If
            mProcessViewer.CloseBreakpointInfo()
            mProcessViewer.DebugAction(ProcessRunAction.StepOut)
        Catch ex as InvalidStateException
            UserMessage.OK(ex.Message)
        Catch ex As Exception
            UserMessage.Show(My.Resources.frmProcess_UnexpectedErrorEx, ex)
        End Try
    End Sub


    Private Sub btnDebugGo_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles mnuTaskTrayGo.Click, mnuDebugGo.Click, btnMenuDebugGo.ButtonClick
        Try
            If mProcessViewer.HasPropertyWindowsOpen Then
                Throw New InvalidStateException(my.resources.frmProcess_DebuggingIsNotAvailableWhilePropertiesDialogsAreOpen)
            End If
            mProcessViewer.CloseBreakpointInfo()
            mProcessViewer.DebugAction(ProcessRunAction.Go, sender Is mnuTaskTrayGo)
        Catch ex as InvalidStateException
            UserMessage.OK(ex.Message)
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.frmProcess_ErrorDuringDebugging0, ex.Message))
        End Try
    End Sub

    Private Sub btnDebugRestart_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles mnuTaskTrayRestart.Click, mnuDebugRestart.Click, btnMenuDebugReset.Click
        Try
            mProcessViewer.CloseBreakpointInfo()
            mProcessViewer.DebugAction(ProcessRunAction.Reset, sender Is mnuTaskTrayRestart)
            If mTextLog IsNot Nothing Then mTextLog.Clear()
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.frmProcess_ErrorResettingDebugging0, ex.Message))
        End Try
    End Sub

    Private Sub btnMenuDebugLaunchApp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMenuDebugLaunchApp.Click
        Try
            With Me.ProcessViewer.Process.AMI
                If Not .ApplicationIsLaunched() Then
                    Dim Err As clsAMIMessage = Nothing
                    If Not .SetTargetApplication(ProcessViewer.Process.ApplicationDefinition.ApplicationInfo, Err) Then
                        UserMessage.Show(Err.Message, Err.HelpTopic)
                        Exit Sub
                    End If
                    Err = Nothing
                    
                    Dim myArgs As Dictionary(Of String,String) = Nothing
                    If .GetTargetAppInfo().ID = clsApplicationTypeInfo.BrowserLaunchId OrElse .GetTargetAppInfo().ID = clsApplicationTypeInfo.BrowserAttachId Then
                        myArgs = New Dictionary(Of String, String) From {
                                {"BrowserLaunchTimeout", BrowserTimeout}
                                }
                    End If

                    If Not .LaunchApplication(Err, myArgs) Then
                        UserMessage.Show(String.Format(My.Resources.frmProcess_FailedToLaunchApplication0, Err.Message), Err.HelpTopic)
                        Exit Sub
                    End If
                Else
                    Dim Err As clsAMIMessage = Nothing
                    If Not Me.ProcessViewer.Process.AMI.DetachApplication(Err) Then
                        UserMessage.Show(String.Format(My.Resources.frmProcess_FailedToDetachFromApplication0, Err.Message), Err.HelpTopic)
                    End If
                End If
            End With
        Catch ex As Exception
            UserMessage.Show(My.Resources.frmProcess_UnexpectedError, ex)
        Finally
            Me.UpdateLaunchButton()
        End Try
    End Sub

    Private Sub UpdateLaunchButton()
        If Me.ProcessViewer.ModeIsObjectStudio Then
            Dim DebugInProgress As Boolean = (Me.ProcessViewer.Process.RunState <> ProcessRunState.Off)
            Me.btnMenuDebugLaunchApp.Enabled = Not DebugInProgress

            If Me.ProcessViewer.Process.AMI.ApplicationIsLaunched Then
                Me.btnMenuDebugLaunchApp.Text = My.Resources.frmProcess_Detach
            Else
                Me.btnMenuDebugLaunchApp.Text = Me.ProcessViewer.Process.AMI.GetLaunchCommandUIString
            End If
        Else
            Me.btnMenuDebugLaunchApp.Visible = False
            Me.btnMenuDebugLaunchApp.Enabled = False
        End If
    End Sub


    '''the handler for this method is added manually in the load event
    '''
    Public Sub RunStateChanged(ByVal mode As ProcessRunState)
        UpdateDebugButtons()

    End Sub

#End Region

#Region "ToolBar Clicks"

    Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            If mProcessViewer.UserHasLock() Then
                mProcessViewer.DoValidateAndSave(ctlProcessViewer.DoValidateAction.Save)
            End If
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub


    Private Sub btnPrint_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPrint.Click
        mProcessViewer.PrintPreview()
    End Sub

    Private Sub btnCut_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCut.Click
        mProcessViewer.DoCut()
    End Sub

    Private Sub btnCopy_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCopy.Click
        mProcessViewer.DoCopy()
    End Sub

    Private Sub btnPaste_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPaste.Click
        mProcessViewer.DoPaste()
    End Sub

    Private Sub btnMenuFullScreen_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMenuFullScreen.Click
        If Me.mFullScreen Then
            Me.ExitFullScreenMode()
        Else
            Me.EnterFullScreenMode()
        End If

        CType(sender, ToolStripButton).Checked = Me.mFullScreen
    End Sub

#End Region

#Region "File Menu"

    Private Sub MnuChkUseSummaries_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuChkUseSummaries.Click
        Me.mnuChkUseSummaries.Checked = Not Me.mnuChkUseSummaries.Checked
        mProcessViewer.SetSummariesEnabled(mnuChkUseSummaries.Checked)

        'now remember this setting so that it persists next time the user
        'enters process studio
        Try
            gSv.SetUserEditSummariesPreference(mnuChkUseSummaries.Checked)
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.frmProcess_FailedToSetUserPreferenceUseEditSummariesToDatabase0, ex.Message), 1044)
        End Try
    End Sub

    Private Sub mnuFileSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFileSave.Click
        If mProcessViewer.UserHasLock() Then
            mProcessViewer.DoValidateAndSave(ctlProcessViewer.DoValidateAction.Save)
            UpdateDebugButtons()
        End If
    End Sub

    Private Sub mnuSaveAs_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuSaveAs.Click

        If mProcessViewer.DoSaveAs Then
            'Create a new back up object based on the 'saved as' process.
            If mProcessViewer.AutoSaver IsNot Nothing Then
                mProcessViewer.AutoSaver.Dispose()
            End If
            mProcessViewer.AutoSaver = New clsAutoSaver(mProcessViewer.Process, mProcessViewer.mgNewProcessID)

            mProcessID = mProcessViewer.mgNewProcessID
        End If

        UpdateDebugButtons()

    End Sub

    Private Sub mnuFileClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFileClose.Click
        Me.Close()
    End Sub

    Private Sub RemoveMenuSeparatorIfSingleItem()
        Dim indexOfLastItem = miExportRelease.DropDownItems.Count() - 1
        If TypeOf (miExportRelease.DropDownItems.Item(indexOfLastItem)) Is ToolStripSeparator Then
            miExportRelease.DropDownItems.RemoveAt(indexOfLastItem)
        End If
    End Sub

#Region "Auto-BackUp menu"

    Private Sub LoadBackUpMenu()
        mnuBackup1.Checked = False
        mnuBackup2.Checked = False
        mnuBackup3.Checked = False
        mnuBackup1.Checked = False
        mnuBackup4.Checked = False
        mnuBackup5.Checked = False
        mnuBackup10.Checked = False
        mnuBackup20.Checked = False
        mnuBackup25.Checked = False
        mnuBackup30.Checked = False
        mnuBackupNever.Checked = False
        Select Case gSv.AutoSaveReadInterval()
            Case 1
                mnuBackup1.Checked = True
            Case 2
                mnuBackup2.Checked = True
            Case 3
                mnuBackup3.Checked = True
            Case 4
                mnuBackup4.Checked = True
            Case 5
                mnuBackup5.Checked = True
            Case 10
                mnuBackup10.Checked = True
            Case 15
                mnuBackup15.Checked = True
            Case 20
                mnuBackup20.Checked = True
            Case 25
                mnuBackup25.Checked = True
            Case 30
                mnuBackup30.Checked = True
            Case Else
                mnuBackupNever.Checked = True
        End Select
    End Sub

    Private Sub ClickBackUpMenu(ByVal menuItem As ToolStripMenuItem, ByVal interval As Integer)
        Try
            If mProcessViewer.AutoSaver IsNot Nothing Then
                mProcessViewer.AutoSaver.Interval = interval
                mProcessViewer.BackupIntervalChanged()
            End If
            gSv.AutoSaveWriteInterval(interval)
            Me.SetMenuItemChecked(menuItem)
        Catch ex As Exception
            UserMessage.Show(My.Resources.frmProcess_FailedToChangeBackUpInterval, 1034)
        End Try
    End Sub

    Private Sub mnuBackUpNever_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
    Handles mnuBackup1.Click, mnuBackup2.Click, mnuBackup3.Click, mnuBackup4.Click, mnuBackup5.Click,
     mnuBackup10.Click, mnuBackup15.Click, mnuBackup20.Click, mnuBackup25.Click, mnuBackup30.Click, mnuBackupNever.Click

        Dim iInterval As Integer

        If sender Is mnuBackup1 Then
            iInterval = 1
        ElseIf sender Is mnuBackup2 Then
            iInterval = 2
        ElseIf sender Is mnuBackup3 Then
            iInterval = 3
        ElseIf sender Is mnuBackup4 Then
            iInterval = 4
        ElseIf sender Is mnuBackup5 Then
            iInterval = 5
        ElseIf sender Is mnuBackup10 Then
            iInterval = 10
        ElseIf sender Is mnuBackup15 Then
            iInterval = 15
        ElseIf sender Is mnuBackup20 Then
            iInterval = 20
        ElseIf sender Is mnuBackup25 Then
            iInterval = 25
        ElseIf sender Is mnuBackup30 Then
            iInterval = 30
        Else
            iInterval = 0
        End If

        ClickBackUpMenu(CType(sender, ToolStripMenuItem), iInterval)

    End Sub

    Private Sub mnuBackUp_Select(ByVal sender As Object, ByVal e As System.EventArgs) Handles mnuBackup.Click, mnuBackup.MouseEnter
        LoadBackUpMenu()
    End Sub

#End Region

    Private Sub mnuPrintPreview_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuPrintPreview.Click
        mProcessViewer.PrintPreview()
    End Sub

    Private Sub mnuPrintProcess_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuPrintProcess.Click
        mProcessViewer.PrintProcess()
    End Sub

    Private Sub mnuPrintOnOnePage_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuPrintOnSinglePage.Click
        mProcessViewer.PrintOnOnePage()
    End Sub

    Private Sub HandleExportProcess(ByVal sender As Object, ByVal e As EventArgs) Handles miExportThisProcess.Click

        Dim wizardType = GetWizardType()

        If Not CanExportProcess(wizardType) Then
            frmApplication.ShowPermissionMessage()
            Return
        End If

        Try
            Dim objExport = New frmProcessExport(mProcessViewer.ProcessID, mProcessViewer.Process, wizardType)
            mParent.StartForm(objExport)
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.frmProcess_UnableToCreateTheExportWizard01, vbCrLf, ex))
        End Try

    End Sub

    Private Sub HandleExportPage(ByVal sender As Object, ByVal e As EventArgs) Handles miExportThisPage.Click

        Dim wizardType = GetWizardType()

        If Not CanExportProcess(wizardType) Then
            frmApplication.ShowPermissionMessage()
            Return
        End If

        Dim xml = mProcessViewer.Process.GenerateXML(mProcessViewer.Process.GetActiveSubSheet)

        Dim objExport = New frmProcessPageExport(mProcessViewer.ProcessID, xml, wizardType)
        mParent.StartForm(objExport)

    End Sub

    Private Function GetWizardType() As frmWizard.WizardType
        Return If(
            mProcessViewer.ModeIsObjectStudio(),
            frmWizard.WizardType.BusinessObject,
            frmWizard.WizardType.Process)
    End Function

    Private Function CanExportProcess(wizardType As frmWizard.WizardType) As Boolean

        Dim processPermissions = gSv.GetEffectiveMemberPermissionsForProcess(mProcessID)

        Return processPermissions.HasPermission(User.Current, GetRequiredExportPermission(wizardType))
    End Function

    Private Shared Function GetRequiredExportPermission(wizardType As frmWizard.WizardType) As String
        Return If(
            wizardType = frmWizard.WizardType.Process,
            Permission.ProcessStudio.ExportProcess,
            Permission.ObjectStudio.ExportBusinessObject)
    End Function

    Private Sub HandleExportReleaseOpening(ByVal sender As Object, ByVal e As EventArgs) _
     Handles miExportRelease.DropDownOpening
        ' Why oh why doesn't ToolStripItem collection implement ICollection(Of ToolStripItem)
        Dim menuItemsToDelete As New List(Of ToolStripItem)
        For Each menuItem As ToolStripItem In miExportRelease.DropDownItems
            If menuItem IsNot miExportAdhocPackage AndAlso menuItem IsNot misepExportRelease Then
                ' Remove It
                menuItemsToDelete.Add(menuItem)
            End If
        Next
        For Each menuItem As ToolStripItem In menuItemsToDelete
            miExportRelease.DropDownItems.Remove(menuItem)
        Next
        ' Now add it up with the latest packages
        For Each pkg As clsPackage In gSv.GetPackagesWithProcess(ProcessViewer.ProcessID)
            Dim item As ToolStripItem = miExportRelease.DropDownItems.Add(pkg.Name,
             ImageLists.Components_16x16.Images(ImageLists.Keys.Component.Package),
             AddressOf HandleExportSpecificPackageClicked)
            item.Tag = pkg
        Next
        RemoveMenuSeparatorIfSingleItem()
    End Sub

    Private Sub HandleExportSpecificPackageClicked(ByVal sender As Object, ByVal e As EventArgs)
        Dim item As ToolStripItem = DirectCast(sender, ToolStripItem)
        Dim f As New frmCreateRelease(DirectCast(item.Tag, clsPackage))
        f.SetEnvironmentColours(Me)
        f.ShowInTaskbar = False
        f.ShowDialog()
    End Sub

    Private Sub HandleExportNewPackageClicked(ByVal sender As Object, ByVal e As EventArgs) Handles miExportAdhocPackage.Click
        Dim f As New frmCreateRelease(mProcessViewer.Process)
        f.SetEnvironmentColours(Me)
        f.ShowInTaskbar = False
        f.ShowDialog()
    End Sub

    Private Sub mnuExit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuExit.Click
        mParent.Close()
    End Sub

#End Region

#Region "Edit Menu"


    Private Sub mnuEditSelectAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuEditSelectAll.Click
        mProcessViewer.DoSelectAll()
    End Sub

    Private Sub mnuEditCut_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuEditCut.Click
        mProcessViewer.DoCut()
    End Sub

    Private Sub mnuEditCopy_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuEditCopy.Click
        mProcessViewer.DoCopy()
    End Sub

    Private Sub mnuEditPaste_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuEditPaste.Click
        mProcessViewer.DoPaste()
    End Sub

    Private Sub mnuEditDelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuEditDelete.Click
        'If the sender is nothing then we are recieving this event because of the delete key pressed
        'this was not then doing anything because Me.mnuEditDelete.Enabled = false which was causing bug# 2278
        If sender Is Nothing OrElse (Me.mnuEdit.Enabled And Me.mnuEditDelete.Enabled) Then
            mProcessViewer.DoDeleteSelection()
            mProcessViewer.ValidateOpenPropertiesWindows()
            mProcessViewer.InvalidateView()
        End If
    End Sub

    Private Sub mnuEditProperties_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuEditProperties.Click
        mProcessViewer.LaunchSelectedStageProperties()
        mProcessViewer.InvalidateView()
    End Sub

    Private Sub mnuEditUndo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Undo()
    End Sub

    Private Sub mnuEditRedo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Redo()
    End Sub

    Private Sub mnuEditAddPage_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuEditAddPage.Click
        mProcessViewer.CreateNewPage()
    End Sub

    Private Sub btnRefresh_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRefresh.Click
        Try
            mProcessViewer.CloseBreakpointInfo()
            mProcessViewer.DebugAction(ProcessRunAction.Reset)
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.frmProcess_ErrorResettingDebugging0, ex.Message))
        End Try
    End Sub

    Private Sub MenuEitDependencies_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuEditDependencies.Click
        Me.toolstripSearch.StartAdvanced(True)
    End Sub

    Private Sub MenuEditAdvancedSearch_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuEditAdvancedSearch.Click
        Me.toolstripSearch.StartAdvanced()
    End Sub

#End Region

#Region "View Menu"

    Private Sub HandleZoomClick(ByVal sender As Object, ByVal e As EventArgs) _
     Handles z400.Click, z200.Click, z150.Click, z100.Click, z75.Click, z50.Click, z25.Click
        Dim percent As String = CStr(DirectCast(sender, ToolStripMenuItem).Tag)
        UpdateZoomPercent(percent)
    End Sub

    Private Sub mnuDynCursor_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuDynCursor.Click
        If mnuDynCursor.Checked = True Then
            mnuDynCursor.Checked = False
            mProcessViewer.SetDynamicCursor(False)
            mProcessViewer.ChangePointer()
        Else
            mnuDynCursor.Checked = True
            mProcessViewer.SetDynamicCursor(True)
            mProcessViewer.ChangePointer()
        End If
    End Sub

    Private Sub mnuFullScreen_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFullScreen.Click
        If Me.mFullScreen Then
            Me.ExitFullScreenMode()
        Else
            Me.EnterFullScreenMode()
        End If
    End Sub


    Private Sub chksnap_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSnap.Click
        If chkSnap.Checked = True Then
            chkSnap.Checked = False
        Else
            chkSnap.Checked = True
        End If
        mProcessViewer.SnapToGrid = chkSnap.Checked
        mProcessViewer.InvalidateView()
    End Sub

    Private Sub chkGrid_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkGrid.Click
        If chkGrid.Checked = True Then
            chkGrid.Checked = False
        Else
            chkGrid.Checked = True
        End If
        mProcessViewer.ShowGridLines = chkGrid.Checked
        mProcessViewer.InvalidateView()
    End Sub

    Private Sub HandleToggleSavePositions(ByVal sender As Object, ByVal e As EventArgs) _
     Handles mnuToolsSavePositions.Click
        mnuToolsSavePositions.Checked = Not mnuToolsSavePositions.Checked
        gSv.SetUserPref("studio.tools.save-positions", mnuToolsSavePositions.Checked)
    End Sub

    Private Sub HandleToggleLockPositions(ByVal sender As Object, ByVal e As EventArgs) _
     Handles mnuToolsLockPositions.Click
        mnuToolsLockPositions.Checked = Not mnuToolsLockPositions.Checked
        gSv.SetUserPref("studio.tools.lock-positions", mnuToolsLockPositions.Checked)
        UpdateToolStripLocking()
    End Sub

#End Region

#Region "Debug Menu"

    ''' <summary>
    ''' Handles the mnuStartParams click event. Shows a form for editing the start
    ''' params.
    ''' </summary>
    Private Sub mnuStartParams_Click(
     ByVal sender As Object, ByVal e As EventArgs) Handles mnuStartParams.Click
        Using f As New frmStartParams()
            f.SetEnvironmentColours(mParent)
            Dim proc As clsProcess = mProcessViewer.Process
            f.btnStart.Visible = False

            Dim sess As New clsProcessSession() With {
                .ResourceName = Environment.MachineName,
                .SessionID = mgSessionID,
                .Process = proc,
                .ProcessID = proc.Id
            }
            f.Sessions = GetSingleton.ICollection(Of ISession)(sess)
            f.ShowInTaskbar = False
            f.ShowDialog()

            If f.DialogResult <> DialogResult.OK Then Return

            proc.SetInputParams(sess.Arguments)

            'Check that the process is not in debug already
            If proc.RunState <> ProcessRunState.Off Then
                UserMessage.Show(My.Resources.frmProcess_TheNewStartupParametersWillNotTakeEffectUntilYouRestartTheProcess, 1054)
            End If

        End Using
    End Sub

    Private Sub Validate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuDebugValidate.Click, btnValidate.Click
        ShowProcessValidation()
    End Sub

    Public Sub ShowProcessValidation()
        Try
            If mValidateResultsForm Is Nothing Then
                mValidateResultsForm = New frmValidateResults(mProcessViewer.Process)
                mValidateResultsForm.Show()
            Else
                mValidateResultsForm.Reactivate()
                mValidateResultsForm.Activate()
            End If
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.frmProcess_UnexpectedErrorExMsg0, ex.Message), ex)
        End Try
    End Sub

    ''' <summary>
    ''' The last stage to be highlighted, if any.
    ''' </summary>
    Private mLastHighlightedStage As clsProcessStage
    Private Sub mobjValidateResults_GotoStage(ByVal gID As Guid) Handles mValidateResultsForm.GotoStage
        ClearStageHighlighting()
        mLastHighlightedStage = Me.ProcessViewer.Process.GetStage(gID)
        If mLastHighlightedStage IsNot Nothing Then
            mLastHighlightedStage.DisplayMode = StageShowMode.Search_Highlight
            mProcessViewer.ShowStage(gID)
        End If
    End Sub

    ''' <summary>
    ''' Clears any stage highlighting.
    ''' </summary>
    Private Sub ClearStageHighlighting()
        If mLastHighlightedStage IsNot Nothing Then
            mLastHighlightedStage.DisplayMode = StageShowMode.Normal
            mProcessViewer.InvalidateView()
        End If
    End Sub
    Private Sub mobjValidateResults_ClearHighlighting() Handles mValidateResultsForm.ClearHighlighting
        Me.ClearStageHighlighting()
    End Sub



    Private Sub mobjValidateResults_Closed(ByVal obj As Object, ByVal e As System.EventArgs) Handles mValidateResultsForm.Closed
        mValidateResultsForm = Nothing
    End Sub


    Private Sub mnuCalculationZoom_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuCalculationZoom.Click
        mnuCalculationZoom.Checked = Not mnuCalculationZoom.Checked
        mProcessViewer.SetCalculationZoomEnabled(mnuCalculationZoom.Checked)
    End Sub

    Public Sub mnuClearAllBreakpoints_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuClearAllBreakpoints.Click
        Dim stages As List(Of clsProcessStage) = mProcessViewer.Process.GetStages()
        Dim iCount As Integer = 0
        For Each ps As clsProcessStage In stages
            If Not ps.BreakPoint Is Nothing Then
                ps.BreakPoint = Nothing
                iCount += 1
            End If
        Next
        If iCount > 0 Then
            SetStatusBarText(String.Format(My.Resources.frmProcess_Cleared0Breakpoints, iCount), 3000)
        Else
            SetStatusBarText(My.Resources.frmProcess_NoBreakpointsToClear, 3000)
        End If

        mProcessViewer.CloseBreakpointInfo()
    End Sub

    Public Sub mnuFocusDebug_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFocusDebugStage.Click
        mProcessViewer.FocusDebugStage()
    End Sub

#End Region

#Region "Help Menu"

    Private Sub mnuTopic_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuTopic.Click
        Try
            If ProcessViewer.ModeIsObjectStudio() Then
                OpenHelpFile(Me, "helpObjectStudio.htm")
            Else
                OpenHelpFile(Me, "frmProcess.htm")
            End If
        Catch ex As Exception
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

    Private Sub mnuOpenHelp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuOpenHelp.Click
        OpenHelpFileHome(Me)
    End Sub

    Private Sub mnuSearch_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuSearch.Click
        OpenHelpFileSearch(Me)
    End Sub

    Private Sub mnuRequest_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuRequest.Click
        mParent.ReportIssue()
    End Sub

    Private Sub mnuAPIHelp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuAPIHelp.Click
        Dim objBusDef As New frmBusDef
        objBusDef.SetEnvironmentColoursFromAncestor(Me)
        objBusDef.ShowInTaskbar = False
        objBusDef.ShowDialog()
        objBusDef.Dispose()
    End Sub

    Private Sub mnuAbout_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuAbout.Click
        mParent.AboutVersion()
    End Sub

#End Region

#Region "Form Events"

    Private Sub frmProcess_Minimized(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Minimized
        If Not Me.mAppModellerForm Is Nothing Then
            Me.mAppModellerForm.WindowState = FormWindowState.Minimized
        End If
    End Sub

    Private Sub frmProcess_Restored(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Restored
        If Not Me.mAppModellerForm Is Nothing Then
            Me.mAppModellerForm.WindowState = FormWindowState.Normal
        End If
    End Sub

    Protected Overrides Sub OnShown(ByVal e As EventArgs)
        MyBase.OnShown(e)
        UpdateToolStripPositioning()
    End Sub

    Protected Overrides Sub OnLoad(ByVal e As EventArgs)
        MyBase.OnLoad(e)

        Dim skillViewModel = New SkillModel()
        mSkillsToolbar.DataContext = skillViewModel
        SkillToolbarVisiblityToolStripMenuItem.Checked = skillViewModel.HasSkills()

        mLoadingFormToken = LoadingFormHelper.ShowForm()
        mLoadingWindowCloseTimer = New Timer(Me.components) With
            {.Enabled = False, .Interval = 1000}
        mLoadingWindowCloseTimer.Enabled = True

        Dim bStartOK, bTemp As Boolean

        mProcessViewer.SetParent(Me)
        mProcessViewer.SetSuperParent(mParent)

        If mXML = Nothing Then
            mProcessViewer.Startup(mProcessViewer.ViewMode, mProcessID, mProcessName, mDescription, bStartOK, mProcess)
        Else
            mProcessViewer.Startup(mProcessViewer.ViewMode, mXML, mProcessID, bStartOK)
        End If
        If Not bStartOK Then
            DialogResult = DialogResult.Abort
            Close()
            Exit Sub
        End If

        'Handle undo events
        AddHandler Me.ProcessViewer.Process.UndoPositionSaved, AddressOf Me.HandleUndoPositionSaved
        AddHandler mnuEditUndo.MouseEnter, AddressOf mnuEditUndo_MouseEnter
        AddHandler mnuEditRedo.MouseEnter, AddressOf mnuEditUndo_MouseEnter

        Me.toolstripSearch.SetProcessStudioParent(Me)
        Me.toolstripSearch.SetProcessViewingControl(mProcessViewer)
        Me.HideSearchControl(Me.toolstripSearch)

        Me.btnFontColour.CurrentColor = System.Drawing.Color.Black        'default to black

        AddHandler mProcessViewer.Process.RunStateChanged, AddressOf RunStateChanged
        Dim objProcess As clsProcess = mProcessViewer.Process

        If mProcessViewer.ModeIsObjectStudio _
            Then ntfyDebug.Icon = Icons.ObjectStudio _
            Else ntfyDebug.Icon = Icons.Prism

        Dim configOptions = Options.Instance

        Select Case mProcessViewer.ViewMode

            Case ProcessViewMode.EditProcess, ProcessViewMode.EditObject

                BuildTools()
                BuildDebug()
                BuildFormattingOptions()

                Me.ntfyDebug.Text = objProcess.Name.Mid(1, 63)

                If configOptions.EditSummariesAreCompulsory Then
                    mProcessViewer.SetSummariesEnabled(True)
                    Me.mnuChkUseSummaries.Checked = True
                    Me.mnuChkUseSummaries.Enabled = False
                Else
                    Try
                        gSv.GetUserEditSummariesPreference(bTemp)
                        Me.mnuChkUseSummaries.Checked() = bTemp
                        mProcessViewer.SetSummariesEnabled(bTemp)
                    Catch ex As Exception
                        UserMessage.Show(String.Format(My.Resources.frmProcess_FailedToReadUserPreferenceUseEditSummariesFromDatabase0, ex.Message))
                    End Try
                End If

            Case ProcessViewMode.CloneProcess, ProcessViewMode.CloneObject

                BuildTools()
                BuildDebug()
                BuildFormattingOptions()

                Me.ntfyDebug.Text = objProcess.Name.Mid(1, 63)

                If configOptions.EditSummariesAreCompulsory Then
                    mProcessViewer.SetSummariesEnabled(True)
                    Me.mnuChkUseSummaries.Checked = True
                    Me.mnuChkUseSummaries.Enabled = False
                Else
                    Try
                        gSv.GetUserEditSummariesPreference(bTemp)
                        Me.mnuChkUseSummaries.Checked() = bTemp
                        mProcessViewer.SetSummariesEnabled(bTemp)
                    Catch ex As Exception
                        UserMessage.Show(String.Format(My.Resources.frmProcess_FailedToReadUserPreferenceUseEditSummariesFromDatabase0, ex.Message), 1044)
                    End Try
                End If

            Case ProcessViewMode.PreviewProcess, ProcessViewMode.PreviewObject

                Me.mnuEditCopy.Enabled = False
                Me.mnuEditCut.Enabled = False
                Me.mnuEditPaste.Enabled = False
                Me.mnuEditProperties.Enabled = False
                Me.mnuEditAddPage.Enabled = False

                Me.mnuTools.Enabled = False
                Me.mnuFileSave.Enabled = False
                Me.mnuSaveAs.Enabled = False
                Me.mnuBackup.Enabled = False
                Me.mnuExport.Enabled = False
                Me.mnuDebug.Enabled = False
                ' Bug 4557 - you can still go/step using keyboard shortcuts
                Me.mnuDebugGo.Enabled = False
                Me.btnMenuDebugGo.Enabled = False
                Me.mnuDebugStep.Enabled = False
                Me.mnuDebugStepOut.Enabled = False
                Me.mnuDebugStepOver.Enabled = False

                Me.mnuEdit.Enabled = False
                Me.btnSave.Enabled = False
                Me.ntfyDebug.Visible = False
                Me.mnuBackup5.Enabled = False
                Me.mnuBackup10.Enabled = False
                Me.mnuBackup15.Enabled = False
                Me.mnuBackup20.Enabled = False
                Me.mnuBackup25.Enabled = False
                Me.mnuBackup30.Enabled = False
                Me.mnuChkUseSummaries.Enabled = False
                Me.mnuToolBoxes.Enabled = False
                Me.toolstripFont.Enabled = False

                For Each oItem As ToolStripItem In toolstripStandard.Items
                    If Not oItem Is btnPrint Then
                        oItem.Enabled = False
                    End If
                Next
                For Each oItem As ToolStripItem In toolstripTools.Items
                    oItem.Enabled = False
                Next
                For Each oItem As ToolStripItem In toolstripSearch.Items
                    oItem.Enabled = False
                Next
                mHiddenToolStrips.Add(toolstripDebug)
                mHiddenToolStrips.Add(toolstripTools)
                SkillsToolbarPanel.Enabled = False

            Case ProcessViewMode.DebugProcess, ProcessViewMode.DebugObject

                Me.Name = "frmDebugProcess"
                Me.mnuFileSave.Enabled = False
                Me.mnuSaveAs.Enabled = False
                Me.mnuExport.Enabled = False
                Me.btnSave.Enabled = False
                Me.mnuEdit.Enabled = False
                Me.toolstripFont.Enabled = False
                BuildDebug()
                mProcessViewer.SetupDebugSessionAndLogging()
                Me.ntfyDebug.Text = objProcess.Name.Mid(1, 63)

                For Each oItem As ToolStripItem In toolstripStandard.Items
                    If Not oItem Is btnPrint Then
                        oItem.Enabled = False
                    End If
                Next
                For Each oItem As ToolStripItem In toolstripTools.Items
                    oItem.Enabled = False
                Next
                For Each oItem As ToolStripItem In toolstripSearch.Items
                    oItem.Enabled = False
                Next
                SkillsToolbarPanel.Enabled = False

            Case ProcessViewMode.AdHocTestProcess, ProcessViewMode.AdHocTestObject

                Me.mnuEditCopy.Enabled = False
                Me.mnuEditCut.Enabled = False
                Me.mnuEditPaste.Enabled = False
                Me.mnuEditProperties.Enabled = False

                Me.Name = "frmAdHocTestProcess"
                Me.mnuFileSave.Enabled = False
                Me.mnuSaveAs.Enabled = False
                Me.mnuExport.Enabled = False
                Me.btnSave.Enabled = False
                Me.mnuEdit.Enabled = False
                BuildDebug()
                mProcessViewer.SetupDebugSessionAndLogging()
                Me.ntfyDebug.Text = objProcess.Name.Mid(1, 63)

                For Each oItem As ToolStripItem In toolstripStandard.Items
                    If Not oItem Is btnPrint Then
                        oItem.Enabled = False
                    End If
                Next

                mHiddenToolStrips.Add(toolstripTools)
                SkillsToolbarPanel.Enabled = False

        End Select

        mTextLog =
            New clsTextBoxLoggingEngine(ProcessViewer.Process, txtLogMessages)

        Me.TrackBar1.Value = 1000
        SetDebugTimerIntervalFromTrackbarValue()

        UpdateDebugButtons()

        mProcessViewer.ClearSelection()
        Me.SetEditMenuCutCopyDeleteEnabled(False)
        Me.SetMenuPropertiesEnabled(False)

        Me.toolstripSearch.Left = Me.toolstripDebug.Width

        If Me.ProcessViewer.ModeIsObjectStudio Then
            AddHandler Me.ProcessViewer.Process.AMI.ApplicationStatusChanged, AddressOf HandleApplicationStatusChanged

            'Add menu item for viewing AMI doc
            mnuEditAddPage.Text = My.Resources.frmProcess_AddNewAction
            Dim mnuAMIDoc As New ToolStripMenuItem(My.Resources.frmProcess_ViewAMIDocumentation, Nothing, AddressOf mnuAMIDoc_Click)
            Dim APIIndex As Integer = Me.HelpToolStripMenuItem.DropDownItems.IndexOf(Me.mnuAPIHelp)
            Me.HelpToolStripMenuItem.DropDownItems.Insert(1 + APIIndex, mnuAMIDoc)
        End If

        BuildProcessPanView()

        mProcessViewer.pbview.Focus()

        mOpenProcessForms.Add(Me)

        Dim wizardType = GetWizardType()

        If Not CanExportProcess(wizardType) Then
            mnuExport.Enabled = False
        End If

        DisableFeaturesAccordingToLicense()
    End Sub

    Private Sub mnuAMIDoc_Click(ByVal sender As Object, ByVal e As EventArgs)
        clsUserInterfaceUtils.ShowHTMLDocument(mProcessViewer.Process.AMI.GetDocumentation(DocumentFormats.HTML), My.Resources.frmProcess_AMIDocumentationVersion & clsAMI.Version.ToString)
    End Sub

    ''' <summary>
    ''' Disables UI features if the license does not permit them.
    ''' </summary>
    Private Sub DisableFeaturesAccordingToLicense()
        btnProcessMI.Enabled = Licensing.License.IsLicensed
        mnuToolsProcessMI.Enabled = Me.btnProcessMI.Enabled
    End Sub

    Private Sub HandleApplicationStatusChanged(ByVal App As clsApplicationTypeInfo, ByVal NewStatus As clsAMI.ApplicationStatus)
        If IsHandleCreated AndAlso Not IsDisposed Then _
         Invoke(New Action(AddressOf UpdateLaunchButton))
    End Sub

    Private Sub frmProcess_Closing(ByVal sender As Object, ByVal e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            Me.Cursor = Cursors.WaitCursor

            ' if any property windows are open we do not want to allow a close
            If mProcessViewer.HasPropertyWindowsOpen Then
                UserMessage.OK(My.Resources.frmProcess_UnableToCloseStudioWhilePropertiesDialogsAreOpen)
                e.Cancel = True
                Return
            End If

            ' If we're aborting, then the OnLoad() never completed - ie. the form was
            ' never displayed. There should be nothing to do - certainly we don't want
            ' to assume that the current state of anything belonging to this form is
            ' valid - ergo, we leave now
            If DialogResult = DialogResult.Abort Then Return

            If Not mAppModellerForm Is Nothing AndAlso mAppModellerForm.Visible Then
                UserMessage.Show(String.Format(My.Resources.frmProcess_PleaseCloseThe0BeforeClosingObjectStudio, ApplicationProperties.ApplicationModellerName))
                e.Cancel = True
                Exit Sub
            End If

            ' Make sure we know whether lock is still intact before attempting closedown
            Try
                mProcessViewer.UserHasLock(True)
            Catch ex As Exception
                UserMessage.Show(My.Resources.frmProcess_FailedToDetermineIfUserHasLock, ex)
                e.Cancel = True
                Exit Sub
            End Try

            If Not mProcessViewer.CloseDown() Then
                e.Cancel = True
                Exit Sub
            End If

            'Stop handling status changed events
            Dim objProcess As clsProcess = mProcessViewer.Process
            If Not objProcess Is Nothing AndAlso objProcess.ProcessType = DiagramType.Object AndAlso Not objProcess.AMI Is Nothing Then
                RemoveHandler objProcess.AMI.ApplicationStatusChanged, AddressOf HandleApplicationStatusChanged
                If Me.mAppModellerForm IsNot Nothing Then
                    Me.mAppModellerForm.HaltApplicationMonitoring()
                End If
            End If

            'Close our validation results form if we have one...
            If Not mValidateResultsForm Is Nothing Then
                mValidateResultsForm.Close()
            End If

            'Close any instance of frmCalculationZoom
            mProcessViewer.SetCalculationZoomEnabled(False)

            'Remove form from global list
            mOpenProcessForms.Remove(Me)

            'Remember user prefs
            SaveToolStripPositions()

            'If the user closes the form half way through debug, then set
            'the runmode to aborted
            If (mProcess IsNot Nothing) Then
                If mProcess.RunState <> ProcessRunState.Completed Then _
             mProcess.RunState = ProcessRunState.Aborted

                'If we are looking at a child process during debug then
                'the process is "owned" elsewhere. Hence when we dispose of the
                'process viewer, the process viewer should not take the
                'initiative to dispose of the process
                mProcessViewer.SuppressProcessDisposal = True
            End If

            mProcessViewer.Dispose()
        Finally
            Me.Cursor = Cursors.Default
        End Try

    End Sub

#End Region

#Region "User Input Events (mouse and keyboard)"

    ''' <summary>
    ''' Handles mouse scrolling in this form, passing it onto the process viewer if
    ''' it does not have focus (and thus has already handled the mousewheel event
    ''' itself)
    ''' </summary>
    Protected Overrides Sub OnMouseWheel(ByVal e As MouseEventArgs)
        If Not mProcessViewer.ContainsFocus Then mProcessViewer.DoMouseWheel(e)
    End Sub

#End Region

#Region "Timer Events"

    ''' <summary>
    ''' Handles the status bar message timer ticking over.
    ''' This just stops the timer and clears the status bar text.
    ''' </summary>
    Private Sub timStatusBarTimer_Tick(ByVal sender As Object, ByVal e As EventArgs) _
     Handles timStatusBarTimer.Tick
        timStatusBarTimer.Stop()
        ClearStatusBarText()
    End Sub

#End Region

#Region "Fonts and Visual styles"

    Private Sub cmbFont_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbFont.SelectedIndexChanged

        If mSettingFormat = False Then
            Dim objProcess As clsProcess = mProcessViewer.Process
            Dim colSelection As clsProcessSelectionContainer = objProcess.SelectionContainer
            Dim objStage As clsProcessStage = Nothing
            Dim aStages As New List(Of clsProcessStage)

            If colSelection.Count > 0 Then
                For Each Selection As clsProcessSelection In colSelection
                    If Selection.mtType = clsProcessSelection.SelectionType.Stage Then
                        objStage = objProcess.GetStage(Selection.mgStageID)
                        If Not objStage Is Nothing Then
                            Try
                                objStage.FontFamily = cmbFont.SelectedItem.ToString
                                aStages.Add(objStage)
                            Catch
                            End Try
                        End If
                    End If
                Next

                objProcess.SaveUndoPosition(clsUndo.ActionType.ChangeFontOf, aStages.ToArray())
                UpdateFormattingControls()
                mProcessViewer.InvalidateView()
            End If
        End If
    End Sub

    Private Sub cmbSize_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbSize.SelectedIndexChanged
        If mSettingFormat = False Then
            Dim objProcess As clsProcess = mProcessViewer.Process
            Dim colSelection As clsProcessSelectionContainer = objProcess.SelectionContainer
            Dim objStage As clsProcessStage = Nothing
            Dim aStages As New List(Of clsProcessStage)

            If colSelection.Count > 0 Then
                For Each Selection As clsProcessSelection In colSelection
                    If Selection.mtType = clsProcessSelection.SelectionType.Stage Then
                        objStage = objProcess.GetStage(Selection.mgStageID)
                        If Not objStage Is Nothing Then
                            objStage.FontSize = CSng(cmbSize.SelectedItem.ToString)
                            objStage.FontSizeUnit = GraphicsUnit.Pixel
                            aStages.Add(objStage)
                        End If
                    End If
                Next

                objProcess.SaveUndoPosition(clsUndo.ActionType.ChangeFontSizeOf, aStages.ToArray())
                UpdateFormattingControls()
                mProcessViewer.InvalidateView()
            End If
        End If
    End Sub

    Private Sub btnFontColour_ButtonClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnFontColour.ButtonClick
        Me.SetSelectionFontColour(Me.btnFontColour.CurrentColor)
    End Sub

    Private Sub btnFontColour_ColorChanged(ByVal sender As System.Object, ByVal e As clsColorButtonEventArgs) Handles btnFontColour.ColorChanged
        Me.SetSelectionFontColour(e.Color)
    End Sub

    Private Sub SetSelectionFontColour(ByVal Colour As Color)
        Dim objProcess As clsProcess = mProcessViewer.Process
        Dim colSelection As clsProcessSelectionContainer = objProcess.SelectionContainer
        Dim objStage As clsProcessStage = Nothing
        Dim aStages As New List(Of clsProcessStage)

        If colSelection.Count > 0 Then
            For Each selection As clsProcessSelection In colSelection
                If selection.mtType = clsProcessSelection.SelectionType.Stage Then
                    objStage = objProcess.GetStage(selection.mgStageID)
                    If Not objStage Is Nothing Then
                        objStage.Color = Colour
                        aStages.Add(objStage)
                    End If
                End If
            Next

            objProcess.SaveUndoPosition(clsUndo.ActionType.ChangeFontColourOf, aStages.ToArray())
            UpdateFormattingControls()
            mProcessViewer.InvalidateView()
        End If
        mProcessViewer.Process.DefaultStageFontColour = Colour
    End Sub

    ''' <summary>
    ''' Takes the selected stages and makes the appropriate change of pressing
    ''' a style button (eg bold, italic, underline). This can be summarised as
    ''' follows:
    ''' 
    ''' a)If a single stage is selected, it toggles the status of 
    ''' the bold/italic/whatever button was pressed.
    ''' 
    ''' b)If several stages are selected then it toggles the status
    ''' of the first stage (as in a)) and then it copies this setting
    ''' to all other stages in the selection.
    ''' </summary>
    ''' <param name="iFontStyle">The fontstyle to be changed (eg bold/italic)
    ''' </param>
    Private Sub ChangeFontStyleOnSelectedStages(ByVal iFontStyle As FontStyle)

        Dim oProcess As clsProcess = mProcessViewer.Process
        Dim colSelection As clsProcessSelectionContainer = oProcess.SelectionContainer
        Dim oProcessSelection As clsProcessSelection
        Dim oStage As clsProcessStage
        Dim bApplyStyleToOtherStages, bRemoveStyleFromOtherStages As Boolean
        Dim iIndex As Integer
        Dim aStages As New List(Of clsProcessStage)

        For Each oProcessSelection In colSelection

            If oProcessSelection.mtType = clsProcessSelection.SelectionType.Stage Then

                'Only need to bother with stages.
                oStage = oProcess.GetStage(oProcessSelection.mgStageID)
                If iIndex = 0 Then
                    'This is the first stage in the selection.
                    If (oStage.Font.Style And iFontStyle) = 0 Then
                        'The first stage does not have this style, so it will be toggled on.
                        'Apply the style to any other stages in the selection.
                        bApplyStyleToOtherStages = True
                    Else
                        'The first stage does have this style, so it will be toggled off. 
                        'Remove the style from any other stages in the selection.
                        bRemoveStyleFromOtherStages = True
                    End If
                    oStage.FontStyle = oStage.FontStyle Xor iFontStyle
                    aStages.Add(oStage)
                    iIndex = 1

                Else
                    'There is more than one stage in the selection.
                    If (oStage.Font.Style And iFontStyle) = 0 And bApplyStyleToOtherStages Then
                        'This stage does not have this style and it should be added to 
                        'match the first stage in the selection.
                        oStage.FontStyle = oStage.Font.Style Or iFontStyle
                        aStages.Add(oStage)

                    ElseIf (oStage.Font.Style And iFontStyle) > 0 And bRemoveStyleFromOtherStages Then
                        'This stage has this style and it should be removed to 
                        'match the first stage in the selection.
                        oStage.FontStyle = oStage.Font.Style Xor iFontStyle
                        aStages.Add(oStage)
                    End If

                End If

            End If

        Next

        If iIndex = 1 Then
            oProcess.SaveUndoPosition(clsUndo.ActionType.ChangeFontStyleOf, aStages.ToArray())
            UpdateFormattingControls()
            mProcessViewer.InvalidateView()
        End If

    End Sub


    ''' <summary>
    ''' Update the formatting controls to be the same as the selected
    ''' stage(s).  If stages have differing details then the font and size will
    ''' instead show blank and the bold, italic etc will show the setting for the
    ''' first stage in the selection (as this is the current setting used in
    ''' the update if the button is pressed.
    ''' </summary>
    Public Sub UpdateFormattingControls()

        Try
            Dim objProcess As clsProcess = mProcessViewer.Process
            Dim objStage As clsProcessStage
            Dim sSize As String = ""
            Dim sFontName As String = ""
            Dim bBold, bUnderline, bItalic As Boolean
            Dim iCount As Integer
            Dim colSelection As Color

            For Each objProcessSelection As clsProcessSelection In objProcess.SelectionContainer
                If objProcessSelection.mtType = clsProcessSelection.SelectionType.Stage Then
                    objStage = objProcess.GetStage(objProcessSelection.mgStageID)
                    If objStage IsNot Nothing Then
                        If iCount = 0 Then
                            'This is the first stage in the selection.
                            bBold = objStage.Font.Bold
                            bUnderline = objStage.Font.Underline
                            bItalic = objStage.Font.Italic
                            sSize = objStage.Font.Size.ToString
                            sFontName = objStage.Font.Name
                            colSelection = objStage.Color
                        Else
                            bBold = bBold And objStage.Font.Bold
                            bUnderline = bUnderline And objStage.Font.Underline
                            bItalic = bItalic And objStage.Font.Italic
                            If sSize <> objStage.Font.Size.ToString Then
                                sSize = ""
                            End If
                            If sFontName <> objStage.Font.Name Then
                                sFontName = ""
                            End If
                            If Not colSelection.Equals(objStage.Color) Then
                                colSelection = Color.Empty
                            End If
                        End If
                        iCount += 1
                    End If
                End If
            Next
            btnBold.Checked = bBold
            btnUnderline.Checked = bUnderline
            btnItalic.Checked = bItalic
            mSettingFormat = True
            cmbSize.Text = sSize
            cmbFont.Text = sFontName
            mSettingFormat = False
            btnFontColour.CurrentColor = colSelection

        Catch ex As Exception
            UserMessage.ShowExceptionMessage(ex)
        End Try
    End Sub

    Private Sub btnBold_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBold.Click
        Me.ChangeFontStyleOnSelectedStages(FontStyle.Bold)
    End Sub

    Private Sub btnItalic_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnItalic.Click
        Me.ChangeFontStyleOnSelectedStages(FontStyle.Italic)
    End Sub

    Private Sub btnUnderline_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUnderline.Click
        Me.ChangeFontStyleOnSelectedStages(FontStyle.Underline)
    End Sub


    Protected Overrides Function ProcessCmdKey(ByRef msg As System.Windows.Forms.Message, ByVal keyData As System.Windows.Forms.Keys) As Boolean
        Dim disallow As Boolean =
            objStoreInEdit.HasTextFocus OrElse
            exprEdit.HasTextFocus OrElse
            cmbSize.Focused OrElse
            cmbFont.Focused OrElse
            toolstripSearch.HasTextFocus

        If disallow Then
            'Return value of false indicates that the key is not
            'a command key. Thus the delete key is interpreted
            'correctly within the focused control (eg the search
            'bar, and does not cause deletion on diagram).
            Return False
        Else
            Return MyBase.ProcessCmdKey(msg, keyData)
        End If

    End Function

    ''' <summary>
    ''' Sets the supplied ToolStripMenuitem checked property to True, whilst
    ''' unchecking all of its siblings.
    ''' 
    ''' This assumes that the supplied ToolStripMenuItem has an owner, and that all
    ''' of its siblings are also ToolStripMenuItems. An Exception will be thrown if
    ''' these assumptions are not met.
    ''' </summary>
    ''' <param name="item">The ToolStripMenuItem to set checked.</param>
    Private Sub SetMenuItemChecked(ByVal item As ToolStripMenuItem)
        For Each t As ToolStripItem In item.Owner.Items
            CType(t, ToolStripMenuItem).Checked = False
        Next
        item.Checked = True
    End Sub

    Private Sub slider_Scroll(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mDebugSpeedSlider.Scroll
        Me.TrackBar1.Value = Me.mDebugSpeedSlider.Value
        Me.SetDebugTimerIntervalFromTrackbarValue()
    End Sub

    Private Sub mnuMenuDebugMedium_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.SetMenuItemChecked(CType(sender, ToolStripMenuItem))
        Me.TrackBar1.Value = Me.TrackBar1.Minimum + (Me.TrackBar1.Maximum - Me.TrackBar1.Minimum) \ 3
        Me.SetDebugTimerIntervalFromTrackbarValue()
    End Sub

    Private Sub mnuMenuDebugFast_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.SetMenuItemChecked(CType(sender, ToolStripMenuItem))
        Me.TrackBar1.Value = Me.TrackBar1.Minimum + 2 * (Me.TrackBar1.Maximum - Me.TrackBar1.Minimum) \ 3
        Me.SetDebugTimerIntervalFromTrackbarValue()
    End Sub

    Private Sub mnuMenuDebugFullSpeed_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.SetMenuItemChecked(CType(sender, ToolStripMenuItem))
        Me.TrackBar1.Value = Me.TrackBar1.Maximum
        Me.SetDebugTimerIntervalFromTrackbarValue()
    End Sub

    Private Sub mnuMenuDebugSlow_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.SetMenuItemChecked(CType(sender, ToolStripMenuItem))
        Me.TrackBar1.Value = Me.TrackBar1.Minimum
        Me.SetDebugTimerIntervalFromTrackbarValue()
    End Sub

#End Region

#Region "ntfyDebug"

    Private Sub ntfyDebug_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles ntfyDebug.DoubleClick
        Me.WindowState = FormWindowState.Normal
    End Sub

#End Region

#Region "Methods"

    ''' <summary>
    ''' Replaces menu items with the correct text depending on whether the mode is
    ''' process studio or object studio
    ''' </summary>
    Private Sub SetFileMenuItemText()
        Dim objectStudio = mProcessViewer.ModeIsObjectStudio()
        SetMenuItemText(objectStudio, FileToolStripMenuItem.DropDownItems)
    End Sub

    ''' <summary>
    ''' Recursively replaces menu items with the correct text.
    ''' </summary>
    ''' <param name="objectStudio">Whether the the mode is object studio</param>
    ''' <param name="items">The items to replace</param>
    Private Sub SetMenuItemText(objectStudio As Boolean, items As ToolStripItemCollection)
        For Each item As ToolStripItem In items
            Dim menu = TryCast(item, ToolStripMenuItem)
            If menu IsNot Nothing Then
                SetMenuItemText(objectStudio, menu.DropDownItems)
            End If
            SwitchModeText(objectStudio, item)
        Next
    End Sub

    ''' <summary>
    ''' Replaces menu items with the correct text.
    ''' </summary>
    ''' <param name="objectStudio">Whether the the mode is object studio</param>
    ''' <param name="item">The item to replace</param>
    Private Sub SwitchModeText(objectStudio As Boolean, item As ToolStripItem)
        If objectStudio Then
            item.Text = item.Text.Replace(My.Resources.frmProcess_Process, My.Resources.frmProcess_BusinessObject)
        Else
            item.Text = item.Text.Replace(My.Resources.frmProcess_BusinessObject, My.Resources.frmProcess_Process)
        End If
    End Sub


    Private Sub SetTool(ByVal sender As Object, ByVal e As EventArgs)
        Dim t As ctlProcessViewer.StudioTool
        t = CType(System.Enum.Parse(GetType(ctlProcessViewer.StudioTool), CType(sender, Control).Name), ctlProcessViewer.StudioTool)
        mProcessViewer.SetCurrentTool(t)
        mProcessViewer.ChangePointer()
    End Sub

#Region "Process Debug Methods"

#End Region

#Region "Task Panes"

    Private Sub BuildTools()
        'Build Tools

        Dim ToolTips As New Dictionary(Of ctlProcessViewer.StudioTool, String)
        ToolTips.Add(ctlProcessViewer.StudioTool.Pointer, My.Resources.frmProcess_SelectAndManipulateStages)
        ToolTips.Add(ctlProcessViewer.StudioTool.Pan, My.Resources.frmProcess_MoveTheFlowchartDiagramUsingDraggingMovements)
        ToolTips.Add(AutomateUI.ctlProcessViewer.StudioTool.Link, My.Resources.frmProcess_CreateLinksBetweenStages)
        ToolTips.Add(AutomateUI.ctlProcessViewer.StudioTool.Process, My.Resources.frmProcess_CallAnotherProcess)
        ToolTips.Add(AutomateUI.ctlProcessViewer.StudioTool.Page, My.Resources.frmProcess_ReferenceAnotherPageInThisProcess)
        ToolTips.Add(AutomateUI.ctlProcessViewer.StudioTool.Action, My.Resources.frmProcess_PerformAnActionAgainstAnotherSystem)
        ToolTips.Add(AutomateUI.ctlProcessViewer.StudioTool.Decision, My.Resources.frmProcess_DefineCriteriaForAYesNoDecision)
        ToolTips.Add(AutomateUI.ctlProcessViewer.StudioTool.Calculation, My.Resources.frmProcess_ManipulateDataUsingMathematicalFunctions)
        ToolTips.Add(AutomateUI.ctlProcessViewer.StudioTool.Data, My.Resources.frmProcess_DataContainer)
        ToolTips.Add(AutomateUI.ctlProcessViewer.StudioTool.Collection, My.Resources.frmProcess_ACollectionOfDataItems)
        ToolTips.Add(AutomateUI.ctlProcessViewer.StudioTool.Loop, My.Resources.frmProcess_PerformASeriesOfStepsForEachItemInACollection)
        ToolTips.Add(AutomateUI.ctlProcessViewer.StudioTool.Note, My.Resources.frmProcess_AFreeFormatTextAreaThatCanAlsoBeUsedToTakeANoteInTheLog)
        ToolTips.Add(AutomateUI.ctlProcessViewer.StudioTool.End, My.Resources.frmProcess_TerminatesTheCurrentProcess)
        ToolTips.Add(AutomateUI.ctlProcessViewer.StudioTool.Choice, My.Resources.frmProcess_AllowsAChoiceToBeMadeBasedOnASetOfExpressions)
        ToolTips.Add(AutomateUI.ctlProcessViewer.StudioTool.Anchor, My.Resources.frmProcess_AllowsLinksToBeRoutedAroundTheProcessDiagram)
        ToolTips.Add(AutomateUI.ctlProcessViewer.StudioTool.Zoom, My.Resources.frmProcess_LeftClickToZoomInRightClickToZoomOut)
        ToolTips.Add(AutomateUI.ctlProcessViewer.StudioTool.Code, My.Resources.frmProcess_InsertCodeToBeExecutedAtRuntimeOftenUsefulForExternalCOMObjects)
        ToolTips.Add(AutomateUI.ctlProcessViewer.StudioTool.Read, My.Resources.frmProcess_ReadInformationFromYourTargetApplicationIntoDataItems)
        ToolTips.Add(AutomateUI.ctlProcessViewer.StudioTool.Write, My.Resources.frmProcess_WriteInformationFromDataItemsIntoYourTargetApplication)
        ToolTips.Add(AutomateUI.ctlProcessViewer.StudioTool.Navigate, My.Resources.frmProcess_PerformNavigationalStepsWithinYourTargetApplication)
        ToolTips.Add(AutomateUI.ctlProcessViewer.StudioTool.Wait, My.Resources.frmProcess_WaitForAnEventToTakePlaceInYourTargetApplication)

        Dim ToolTips2 As New Dictionary(Of ctlProcessViewer.StudioTool, String)
        ToolTips2.Add(ctlProcessViewer.StudioTool.End, My.Resources.frmProcess_TerminatesTheCurrentAction)
        ToolTips2.Add(ctlProcessViewer.StudioTool.Page, My.Resources.frmProcess_ReferenceAnotherActionInThisObject)
        ToolTips2.Add(ctlProcessViewer.StudioTool.Anchor, My.Resources.frmProcess_AllowsLinksToBeRoutedAroundTheObjectDiagram)

        'Tags for the tools toolbars
        mnuPointer.Tag = AutomateUI.ctlProcessViewer.StudioTool.Pointer
        mnuLink.Tag = AutomateUI.ctlProcessViewer.StudioTool.Link
        mnuNote.Tag = AutomateUI.ctlProcessViewer.StudioTool.Note
        mnuAnchor.Tag = AutomateUI.ctlProcessViewer.StudioTool.Anchor
        mnuDataItem.Tag = AutomateUI.ctlProcessViewer.StudioTool.Data
        mnuCollection.Tag = AutomateUI.ctlProcessViewer.StudioTool.Collection
        mnuCalculation.Tag = AutomateUI.ctlProcessViewer.StudioTool.Calculation
        mnuMultipleCalculation.Tag = AutomateUI.ctlProcessViewer.StudioTool.MultipleCalculation
        mnuLoop.Tag = AutomateUI.ctlProcessViewer.StudioTool.Loop
        mnuDecision.Tag = AutomateUI.ctlProcessViewer.StudioTool.Decision
        mnuChoice.Tag = AutomateUI.ctlProcessViewer.StudioTool.Choice
        mnuRead.Tag = AutomateUI.ctlProcessViewer.StudioTool.Read
        mnuWrite.Tag = AutomateUI.ctlProcessViewer.StudioTool.Write
        mnuNavigate.Tag = AutomateUI.ctlProcessViewer.StudioTool.Navigate
        mnuWait.Tag = AutomateUI.ctlProcessViewer.StudioTool.Wait
        mnuAction.Tag = AutomateUI.ctlProcessViewer.StudioTool.Action
        mnuProcess.Tag = AutomateUI.ctlProcessViewer.StudioTool.Process
        mnuPage.Tag = AutomateUI.ctlProcessViewer.StudioTool.Page
        mnuCode.Tag = AutomateUI.ctlProcessViewer.StudioTool.Code
        mnuEnd.Tag = AutomateUI.ctlProcessViewer.StudioTool.End
        mnuAlert.Tag = AutomateUI.ctlProcessViewer.StudioTool.Alert
        mnuException.Tag = AutomateUI.ctlProcessViewer.StudioTool.Exception
        mnuRecover.Tag = AutomateUI.ctlProcessViewer.StudioTool.Recover
        mnuResume.Tag = AutomateUI.ctlProcessViewer.StudioTool.Resume
        mnuBlock.Tag = AutomateUI.ctlProcessViewer.StudioTool.Block
        btnPointer.Tag = AutomateUI.ctlProcessViewer.StudioTool.Pointer
        btnLink.Tag = AutomateUI.ctlProcessViewer.StudioTool.Link
        btnNote.Tag = AutomateUI.ctlProcessViewer.StudioTool.Note
        btnAnchor.Tag = AutomateUI.ctlProcessViewer.StudioTool.Anchor
        btnData.Tag = AutomateUI.ctlProcessViewer.StudioTool.Data
        btnCollection.Tag = AutomateUI.ctlProcessViewer.StudioTool.Collection
        btnCalculation.Tag = AutomateUI.ctlProcessViewer.StudioTool.Calculation
        btnMultipleCalculation.Tag = AutomateUI.ctlProcessViewer.StudioTool.MultipleCalculation
        btnLoop.Tag = AutomateUI.ctlProcessViewer.StudioTool.Loop
        btnDecision.Tag = AutomateUI.ctlProcessViewer.StudioTool.Decision
        btnChoice.Tag = AutomateUI.ctlProcessViewer.StudioTool.Choice
        btnRead.Tag = AutomateUI.ctlProcessViewer.StudioTool.Read
        btnWrite.Tag = AutomateUI.ctlProcessViewer.StudioTool.Write
        btnNavigate.Tag = AutomateUI.ctlProcessViewer.StudioTool.Navigate
        btnWait.Tag = AutomateUI.ctlProcessViewer.StudioTool.Wait
        btnAction.Tag = AutomateUI.ctlProcessViewer.StudioTool.Action
        btnProcess.Tag = AutomateUI.ctlProcessViewer.StudioTool.Process
        btnPage.Tag = AutomateUI.ctlProcessViewer.StudioTool.Page
        btnCode.Tag = AutomateUI.ctlProcessViewer.StudioTool.Code
        btnEnd.Tag = AutomateUI.ctlProcessViewer.StudioTool.End
        btnAlert.Tag = AutomateUI.ctlProcessViewer.StudioTool.Alert
        btnException.Tag = AutomateUI.ctlProcessViewer.StudioTool.Exception
        btnRecover.Tag = AutomateUI.ctlProcessViewer.StudioTool.Recover
        btnResume.Tag = AutomateUI.ctlProcessViewer.StudioTool.Resume
        btnBlock.Tag = AutomateUI.ctlProcessViewer.StudioTool.Block

        If mProcessViewer.ModeIsObjectStudio Then
            'object studio
            Me.mnuAlert.Visible = False
            Me.btnAlert.Visible = False
        Else
            'process studio
            Me.mnuCode.Visible = False
            Me.mnuRead.Visible = False
            Me.mnuWrite.Visible = False
            Me.mnuNavigate.Visible = False
            Me.mnuWait.Visible = False

            Me.btnCode.Visible = False
            Me.btnRead.Visible = False
            Me.btnWrite.Visible = False
            Me.btnNavigate.Visible = False
            Me.btnWait.Visible = False
        End If

    End Sub

    Private Sub BuildFormattingOptions()
        'Build Formatting Options
        Dim ifc As New System.Drawing.Text.InstalledFontCollection

        For Each f As FontFamily In ifc.Families
            cmbFont.Items.Add(f.Name)
        Next
        mSettingFormat = True
        If cmbFont.Items.Contains("Segoe UI") Then
            cmbFont.Text = "Segoe UI"
        Else
            cmbFont.Text = cmbFont.Items(0).ToString
        End If

        For i As Integer = 8 To 32
            cmbSize.Items.Add(i)
        Next
        cmbSize.Text = My.Resources.frmProcess_FontSize10
        mSettingFormat = False
    End Sub

    Private Sub BuildDebug()

        Me.mDebugSpeedSlider.Value = 1000
        Me.ttTips.SetToolTip(Me.mDebugSpeedSlider, My.Resources.frmProcess_AdjustsTheSpeedOfTheDebugProcess)

        Dim slider As New ToolStripControlHost(mDebugSpeedSlider)

        Me.btnMenuDebugGo.DropDownItems.Add(slider)
        Me.btnMenuDebugGo.DropDown.BackColor = System.Drawing.SystemColors.Menu

    End Sub

    Private mPanView As ctlProcessDiagramPanView

    Private Sub BuildProcessPanView()
        mPanView = New ctlProcessDiagramPanView(mProcessViewer)
        mPanView.Dock = DockStyle.Fill

        Dim PanviewHost As New ToolStripControlHost(mPanView)
        PanviewHost.Dock = DockStyle.Fill
        PanviewHost.Size = New Size(250, 250)

        Me.btnPanViewDropDown.DropDownItems.Add(PanviewHost)
    End Sub

#End Region


#Region "Communication with process viewer"


#Region "Edit Menu Updates"

    ''' <summary>
    ''' Enables or disables the edit menu editing options such as cut, copy paste.
    ''' </summary>
    ''' <param name="CutCopyDeleteEnabled">The value for the Cut, Copy, Delete
    ''' menu options. These can only be set together all at once. Set to true to
    ''' enable the menu items, false otherwise.</param>
    ''' <param name="PasteEnabled">The value for the Paste option. Set to true to
    ''' enable the paste option in the menu, false otherwise.</param>
    Public Sub SetEditMenuOptions(ByVal CutCopyDeleteEnabled As Boolean, ByVal PasteEnabled As Boolean)
        Me.SetEditMenuCutCopyDeleteEnabled(CutCopyDeleteEnabled)
        Me.mnuEditPaste.Enabled = PasteEnabled AndAlso mProcessViewer.ModeIsEditable()
        Me.btnPaste.Enabled = PasteEnabled AndAlso mProcessViewer.ModeIsEditable()
    End Sub

#End Region

#Region "View Menu Zoom Updating"

    ''' <summary>
    ''' Updates the zoom level.
    ''' </summary>
    ''' <param name="percent">the new level</param>
    Public Sub ZoomUpdate(ByVal percent As Integer) Implements IProcessViewingForm.ZoomUpdate

        'Ensure the zoom is not updated when the combobox has focus
        'Which means the user is potentially editing the zoom
        'but has not commited the changes.
        If mZoomToolstripCombo.Focused Then Return

        z400.Checked = (percent = 400)
        z200.Checked = (percent = 200)
        z150.Checked = (percent = 150)
        z100.Checked = (percent = 100)
        z75.Checked = (percent = 75)
        z50.Checked = (percent = 50)
        z25.Checked = (percent = 25)

        zDyn.Text = CStr(percent)
        mZoomToolstripCombo.Text = String.Format(My.Resources.frmProcess_ZoomPercent, percent)
    End Sub

#End Region

    ''' <summary>
    ''' Set the enabled status of all debug buttons and menu items according to
    ''' the current situation.
    ''' </summary>
    Public Sub UpdateDebugButtons()

        'Gather information we need to make the decisions...
        Dim objProcess As clsProcess = mProcessViewer.Process
        Dim runmode As ProcessRunState = objProcess.RunState

        'Set some local flags according to the current status...
        Dim bIsRunnable As Boolean, bIsStoppable As Boolean
        Dim bIsRestartable As Boolean
        Dim bIsAtStart As Boolean = (runmode = ProcessRunState.Off)

        If Not mProcessViewer.CanDebug Then
            bIsRunnable = False
            bIsStoppable = False
            bIsRestartable = False
        Else
            Select Case runmode
                Case ProcessRunState.Completed, ProcessRunState.Running, ProcessRunState.Stepping
                    bIsRunnable = False
                Case ProcessRunState.Paused
                    If objProcess.ChildWaiting Then
                        bIsRunnable = False
                    Else
                        bIsRunnable = True
                    End If
                Case Else
                    bIsRunnable = True
            End Select

            Select Case runmode
                Case ProcessRunState.Running
                    bIsStoppable = True
                    bIsRestartable = False
                Case Else
                    bIsStoppable = False
                    bIsRestartable = True
            End Select
        End If

        Me.UpdateLaunchButton()

        mnuTaskTrayGo.Enabled = bIsRunnable
        'note the play button should always be enabled.

        mnuTaskTrayStep.Enabled = bIsRunnable
        btnMenuDebugStep.Enabled = bIsRunnable

        mnuTaskTrayStepOver.Enabled = bIsRunnable
        btnMenuDebugStepOver.Enabled = bIsRunnable

        mnuDebugStepOut.Enabled = bIsRunnable
        btnMenuDebugStepOut.Enabled = bIsRunnable

        mnuTaskTrayStop.Enabled = bIsStoppable
        btnMenuDebugPause.Enabled = bIsStoppable

        mnuTaskTrayRestart.Enabled = bIsRestartable
        btnMenuDebugReset.Enabled = bIsRestartable

        mnuSaveAs.Enabled = ShouldSaveAsButtonBeEnabled(runmode)

    End Sub

    Private Function ShouldSaveAsButtonBeEnabled(ByVal runmode As ProcessRunState) As Boolean
        Return {
            ProcessRunState.Off,
            ProcessRunState.Aborted,
            ProcessRunState.Failed,
            ProcessRunState.Completed
        }.Contains(runmode)
    End Function

#End Region

#Region "Methods for Interfacing With ProcessViewer Control"

    ''' <summary>
    ''' Sets StatusBar text.
    ''' </summary>
    ''' <param name="sMessage">The text</param>
    Public Overloads Sub SetStatusBarText(ByVal sMessage As String) Implements IProcessViewingForm.SetStatusBarText
        SetStatusBarText(sMessage, DefaultStatusBarMessageDuration)
    End Sub

    ''' <summary>
    ''' Sets temporary StatusBar text.
    ''' </summary>
    ''' <param name="msg">The text</param>
    ''' <param name="duration">The duration in milliseconds</param>
    Public Overloads Sub SetStatusBarText(ByVal msg As String, ByVal duration As Integer) _
     Implements IProcessViewingForm.SetStatusBarText
        If InvokeRequired Then
            Invoke(New Action(Of String, Integer)(AddressOf SetStatusBarMessage),
             msg, duration)
        Else
            SetStatusBarMessage(msg, duration)
        End If
    End Sub

    ''' <summary>
    ''' Sets the status bar message to the given value for the specified duration
    ''' </summary>
    ''' <param name="msg">The message to set in the status bar</param>
    ''' <param name="duration">The number of milliseconds that the message should be
    ''' visible</param>
    Private Sub SetStatusBarMessage(ByVal msg As String, ByVal duration As Integer)
        'set the message on the status bar
        lblStatus.Text = msg
        lblStatus.Visible = True

        ' Stop the timer now, abandoning any currently running statuses
        timStatusBarTimer.Stop()

        ' 0 indicates "leave message there forever" (or at least until next message)
        ' We do not entertain negative values.
        If duration > 0 Then
            'set up the timer so that the message gets removed after the requested time.
            timStatusBarTimer.Interval = duration
            timStatusBarTimer.Start()
        End If

    End Sub
    ''' <summary>
    ''' Clears StatusBar text.
    ''' </summary>
    Public Sub ClearStatusBarText() Implements IProcessViewingForm.ClearStatusBarText
        Me.lblStatus.Text = String.Empty
        Me.lblStatus.Visible = False
    End Sub

    Public Sub HideSearchControl(ByVal SearchControl As DiagramSearchToolstrip) Implements IProcessViewingForm.HideSearchControl
        'Dave intentionally left this in.  Sorry.
    End Sub

#End Region

#End Region

#Region "FullScreen Mode"

    ''' <summary>
    ''' Exits full screen mode, Restores the form to its previous size, 
    ''' shows the windows taskbar 
    ''' </summary>
    Public Sub ExitFullScreenMode()
        mFullScreen = False

        SuspendLayout()

        MainMenuStrip = mnuMain

        toolstripStandard.Visible = True
        mProcessViewer.Visible = True
        stsBar.Visible = True

        btnMenuFullScreen.Checked = False
        mnuFullScreen.Checked = False
        mnuMain.Visible = True

        StartBar(True)

        FormBorderStyle = FormBorderStyle.Sizable
        If Not mPreFullScreenMaximized Then WindowState = FormWindowState.Normal

        ResumeLayout()

    End Sub

    ''' <summary>
    ''' Enters Fullscreenmode, maximises the vewing window, hides unnessecry controls
    ''' hides the windows taskbar
    ''' </summary>
    Public Sub EnterFullScreenMode()

        mFullScreen = True

        SuspendLayout()

        If WindowState = FormWindowState.Maximized Then
            mPreFullScreenMaximized = True
            WindowState = FormWindowState.Normal
        End If

        FormBorderStyle = FormBorderStyle.None
        WindowState = FormWindowState.Maximized

        Menu = Nothing
        stsBar.Visible = False
        btnMenuFullScreen.Checked = True
        mnuFullScreen.Checked = True
        mnuMain.Visible = False

        StartBar(False)

        ResumeLayout()

        ResizeSkillsToolbar()

    End Sub


    ''' <summary>
    ''' Function used to show or hide the windows start bar.
    ''' </summary>
    ''' <param name="bShow">True to show, False to Hide.</param>
    Private Sub StartBar(ByVal bShow As Boolean)

        Dim hwnd As IntPtr
        hwnd = FindWindow("Shell_traywnd", "")
        If bShow Then
            Call SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP.SWP_SHOWWINDOW)
        Else
            Call SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP.SWP_HIDeWINDOW)
        End If

    End Sub

#End Region

    Private Sub btnWatch_Clicked(ByVal sender As System.Object, ByVal e As System.EventArgs) _
     Handles btnWatch.Click

        If mDataWatchForm Is Nothing Then
            mDataWatchForm = New frmDataItemWatches(Me)
            mDataWatchForm.SetEnvironmentColours(Me)
            mDataWatchForm.SetProcess(mProcessViewer.Process)
        Else
            mDataWatchForm.ctlDataItemWatches.RefreshTree()
        End If
        mDataWatchForm.Show()


    End Sub

    Private Sub mDataWatchForm_VisibleChanged(ByVal sender As Object, ByVal e As System.EventArgs) _
     Handles mDataWatchForm.VisibleChanged

        mnuToolsDataItemWatch.Checked = Not mnuToolsDataItemWatch.Checked

    End Sub

    Private Sub btnProcessMI_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
     Handles btnProcessMI.Click
        If Licensing.License.IsLicensed Then
            If mProcessMIForm Is Nothing OrElse mProcessMIForm.IsDisposed Then
                mProcessMIForm = New frmProcessMI(Me, mProcessID)
                mProcessMIForm.SetEnvironmentColours(Me)
            End If
            mProcessMIForm.Show()
        Else
            clsUserInterfaceUtils.ShowOperationDisallowedMessage()
        End If
    End Sub

    Private Sub mProcessMIForm_VisibleChanged(ByVal sender As Object, ByVal e As System.EventArgs) _
     Handles mProcessMIForm.VisibleChanged

        mnuToolsProcessMI.Checked = Not mnuToolsProcessMI.Checked

    End Sub

    Private Sub TrackBar1_Scroll(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TrackBar1.Scroll
        SetDebugTimerIntervalFromTrackbarValue()
    End Sub

    Private Sub SetDebugTimerIntervalFromTrackbarValue()
        'trackbar runs as linear scale. We adjust this scale to
        'give more control at the faster end

        Try
            'max and min delays allowed in milliseconds between stages
            Const maxDelayBetweenDebugStages = 2000
            Const minDelayBetweenDebugStages = 1

            'get value of trackbar as linear value 0.0 ... 1.0
            Dim adjustedTrackbarValue As Double = (TrackBar1.Maximum - TrackBar1.Value) / (TrackBar1.Maximum - TrackBar1.Minimum)

            If adjustedTrackbarValue = 0.0D Then
                mProcessViewer.Timer1.Interval = minDelayBetweenDebugStages
                mProcessViewer.RunAtNearFullSpeed = True
            Else
                mProcessViewer.Timer1.Interval = CInt(maxDelayBetweenDebugStages * adjustedTrackbarValue + minDelayBetweenDebugStages)
                mProcessViewer.RunAtNearFullSpeed = False
            End If
        Catch
            UserMessage.Show(My.Resources.frmProcess_InternalErrorSettingDebugTimerInterval)
        End Try
    End Sub

    ''' <summary>
    ''' Event handler used to handle the show 'Process MI' menu option.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub mnuProcessMIShow_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuToolsProcessMI.Click

        If mProcessMIForm Is Nothing OrElse mProcessMIForm.IsDisposed Then
            mProcessMIForm = New frmProcessMI(Me, mProcessID)
            mProcessMIForm.SetEnvironmentColours(Me)
        End If

        If mProcessMIForm.Visible Then
            mProcessMIForm.Hide()
        Else
            mProcessMIForm.Show()
        End If

    End Sub

    ''' <summary>
    ''' Event handler used to handle the show data item watch menu option.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub mnuToolsDataItemWatch_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles mnuToolsDataItemWatch.Click

        If mDataWatchForm Is Nothing Then
            mDataWatchForm = New frmDataItemWatches(Me)
            mDataWatchForm.SetEnvironmentColours(Me)
            mDataWatchForm.SetProcess(mProcessViewer.Process)
        End If


        If mDataWatchForm.Visible Then
            mDataWatchForm.Hide()
        Else
            mDataWatchForm.Show()
        End If

    End Sub

    ''' <summary>
    ''' Handles the event raised by the integration assistant when the user commits
    ''' the changes made to the application definition.
    ''' </summary>
    ''' <param name="newAppDef">The latest version of the application definition.
    ''' </param>
    Private Sub HandleApplicationDefinitionUpdated(ByVal newAppDef As clsApplicationDefinition,
     ByVal parent As KeyValuePair(Of Guid, String)) Handles mAppModellerForm.ApplicationDefinitonUpdated
        'Set the new application definition into the process
        mProcessViewer.ParentObject = parent
        If parent.Value Is Nothing Then
            If mProcessViewer.Process.ParentObjRef IsNot Nothing Then
                mProcessViewer.Process.UnloadParent()
            End If
            mProcessViewer.Process.ApplicationDefinition = newAppDef
        Else
            'Update any open objects sharing this model (this object, the parent or other siblings)
            For Each frm As frmProcess In mOpenProcessForms
                If frm.mProcessID = parent.Key OrElse frm.mProcessViewer.ParentObject.Key = parent.Key Then
                    frm.mProcessViewer.LoadParentApplicationModel()
                End If
            Next
        End If

        Dim amiMessage As clsAMIMessage = Nothing
        If Not Me.mProcessViewer.Process.AMI.SetTargetApplication _
            (mProcessViewer.Process.ApplicationDefinition.ApplicationInfo, amiMessage) Then
            UserMessage.Show(amiMessage.Message)
        End If
    End Sub

    Private Sub FindReferences(dep As clsProcessDependency) Handles mAppModellerForm.FindReferences
        If mProcessViewer.Process.ParentObjRef IsNot Nothing Then
            ParentAppForm.FindReferences(dep, CType(mProcessViewer.Process.ParentObjRef, clsVBO).Process)
        Else
            ParentAppForm.FindReferences(dep, mProcessViewer.Process)
        End If
    End Sub

    ''' <summary>
    ''' Handles a 'Show Integration Assistant' click, either from the menu or from
    ''' the button in the toolbar.
    ''' </summary>
    Private Sub HandleShowIntegrationAssistant(ByVal sender As Object, ByVal e As EventArgs) _
     Handles mnuApplicationModeller.Click, btnMenuApplicationModeller.Click
        Try
            ShowIntegrationAssistant()

        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.frmProcess_Error0, ex.Message), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Shows the application model window
    ''' </summary>
    Private Sub ShowIntegrationAssistant()
        'Ensure we're in Object Studio
        If Not Me.ProcessViewer.ModeIsObjectStudio Then
            Throw New InvalidOperationException(
             String.Format(My.Resources.frmProcess_0CannotBeShownUnlessInObjectStudio, ApplicationProperties.ApplicationModellerName))
        End If

        'If there is no application definition already existing then we need to run the wizard
        If mProcessViewer.Process.ApplicationDefinition.ApplicationInfo Is Nothing Then

            Dim parentObjectName = mProcessViewer.ParentObject.Value
            If Not mHasPermissionToEdit Then
                If parentObjectName IsNot Nothing Then
                    UserMessage.Show(String.Format(My.Resources.AutomateUI_Controls.frmProcess_ShowIntegrationAssistant_SharedApplicationModelNotAvailable, parentObjectName))
                Else
                    UserMessage.Show(My.Resources.AutomateUI_Controls.frmProcess_ShowIntegrationAssistant_ObjectDoesNotHaveApplicationModel)
                End If
                Exit Sub
            End If
            'Handle situation where parent object is missing
            If parentObjectName IsNot Nothing Then
                If UserMessage.OkCancel(String.Format(My.Resources.AutomateUI_Controls.frmProcess_ShowIntegrationAssistant_SharedApplicationModelNotAvailableParent, mProcessViewer.ParentObject.Value)) = MsgBoxResult.Cancel Then
                    Exit Sub
                End If
            End If
            Dim f As New frmApplicationDefinitionCreate(mProcessViewer.Process.Name,
             New KeyValuePair(Of Guid, String)(Guid.Empty, Nothing))
            f.PrototypeName = mProcessViewer.Process.Name
            f.SetEnvironmentColours(Me)
            f.ShowInTaskbar = False
            f.ShowDialog()
            If f.DialogResult = System.Windows.Forms.DialogResult.OK Then
                mProcessViewer.ParentObject = f.ParentObject
                If f.ParentObject.Value = Nothing Then
                    If mProcessViewer.Process.ParentObjRef IsNot Nothing Then
                        mProcessViewer.Process.UnloadParent()
                    End If
                    mProcessViewer.Process.ApplicationDefinition = f.ApplicationDefinition
                Else
                    mProcessViewer.LoadParentApplicationModel()
                End If
            Else
                Exit Sub
            End If
            f.Dispose()
        End If

        'If there is now an application definition then show the assistant
        If mProcessViewer.Process.ApplicationDefinition.ApplicationInfo IsNot Nothing Then

            'Create modeller form (if it's not already open)
            If mAppModellerForm Is Nothing OrElse mAppModellerForm.IsDisposed Then
                mAppModellerForm = New frmIntegrationAssistant(mProcessViewer.Process.AMI,
                 mProcessViewer.Process, mProcessViewer.ParentObject)
                mAppModellerForm.ApplicationDefinition = mProcessViewer.Process.ApplicationDefinition
                mAppModellerForm.SetEnvironmentColours(Me)
            End If

            'Now make sure it is visible
            Me.mAppModellerForm.Show()
            Me.mAppModellerForm.BringToFront()
        End If
    End Sub

    Private Function GetFirstChildName(ByVal element As clsApplicationElement) As String
        Dim child = element.ChildMembers.OfType(Of clsApplicationMember).FirstOrDefault()
        If child IsNot Nothing Then
            Return child.Name
        End If
        Return String.Empty
    End Function

#Region "Tools Toolbar"

    ''' <summary>
    ''' Updated the gripstyle of all the toolstips to ensure they are
    ''' not moveable (locked) based on the users preference.
    ''' </summary>
    Private Sub UpdateToolStripLocking()
        If mnuToolsLockPositions.Checked Then
            toolstripStandard.GripStyle = ToolStripGripStyle.Hidden
            toolstripDebug.GripStyle = ToolStripGripStyle.Hidden
            toolstripSearch.GripStyle = ToolStripGripStyle.Hidden
            toolstripFont.GripStyle = ToolStripGripStyle.Hidden
        Else
            toolstripStandard.GripStyle = ToolStripGripStyle.Visible
            toolstripDebug.GripStyle = ToolStripGripStyle.Visible
            toolstripSearch.GripStyle = ToolStripGripStyle.Visible
            toolstripFont.GripStyle = ToolStripGripStyle.Visible
        End If
    End Sub

    ''' <summary>
    ''' Reads saved positions from the DB and applies them to  the relevant tool
    ''' strip. The top tool strip panel is assumed to be the default for all tool
    ''' strips. Any tool strip whose name is not saved in the DB is left in its
    ''' default position.
    ''' </summary>
    Private Sub UpdateToolStripPositioning()

        ' If this is in the designer, or the form has not been constructed yet,
        ' skip setting the positions
        If DesignMode Then Exit Sub

        ' Set the lock positions menu item, and update tool strip locking.
        Dim lock As Boolean = gSv.GetPref(PreferenceNames.StudioTools.LockPositions, True)
        mnuToolsLockPositions.Checked = lock
        UpdateToolStripLocking()

        ' First off, set the save positions flag (and menu item)
        Dim remember As Boolean = gSv.GetPref(PreferenceNames.StudioTools.SavePositions, False)
        mnuToolsSavePositions.Checked = remember

        ' If we're not set to remember toolstrip positions, leave at current values
        If Not remember Then Return

        Dim toolstripPositions As ICollection(Of clsUIElementPosition) = gSv.GetToolStripPositions(ProcessViewer.ModeIsObjectStudio)
        If toolstripPositions.Count <= 0 Then Return

        ' Avoid all layout until this is done
        toolstripCont.SuspendLayout()
        Try
            ' Remove all the toolstrips that we are handling positioning for (ie.
            ' all those registered in mToolstrips).

            ' We record them in location order in case any toolstrips are not saved
            ' in the database - we want to add them back to the toolstrip panel in
            ' optimal order to try and ensure the laying out of the panel is sensible
            Dim removed As _
             New SortedDictionary(Of String, Control)(New ToolstripComparer(Me))
            For Each pan As ToolStripPanel In WinUtil.FindControls(Of ToolStripPanel)(Me)
                Dim removeFromThisPanel As New List(Of Control)
                For Each ctl As Control In pan.Controls
                    If mToolStrips.ContainsKey(ctl.Name) Then
                        removed(ctl.Name) = ctl
                        removeFromThisPanel.Add(ctl)
                    End If
                Next
                For Each ctl As Control In removeFromThisPanel
                    pan.Controls.Remove(ctl)
                Next
            Next

            ' Get saved positions from DB and organise them by dock panel
            Dim map As New Dictionary(Of DockStyle, ICollection(Of clsUIElementPosition))
            For Each uiLocn As clsUIElementPosition In toolstripPositions
                Dim coll As ICollection(Of clsUIElementPosition) = Nothing
                If Not map.TryGetValue(uiLocn.Dock, coll) Then
                    coll = New List(Of clsUIElementPosition)
                    map(uiLocn.Dock) = coll
                End If
                coll.Add(uiLocn)
            Next

            ' Go through each dock panel in turn and initialise it with the
            ' toolstrip values
            For Each dock As DockStyle In map.Keys

                ' Get the containing tool strip panel
                Dim pan As ToolStripPanel = GetToolStripPanelFromDock(dock)

                ' Begin initialisation of the toolstrip panel
                pan.BeginInit()

                ' Go through each of the element positions saved, and join the strips
                ' onto the panel
                For Each uiPosn As clsUIElementPosition In map(dock)

                    ' Get the toolstrip referenced in the position
                    Dim strip As ToolStrip = Nothing

                    ' Probably should always be there, but if we change variable names,
                    ' this could break it.. so just skip in that case.
                    If Not mToolStrips.TryGetValue(uiPosn.Name, strip) Then Continue For

                    ' Deregister this strip from the 'removed' map - so know not to
                    ' re-add it later.
                    removed.Remove(strip.Name)

                    ' Join the strip to the panel at the specified location
                    pan.Join(strip, uiPosn.Location)

                    ' And set it visible if it should be visible
                    strip.Visible =
                     (uiPosn.Visible AndAlso Not mHiddenToolStrips.Contains(strip))

                    ' Ensure that the menuitem has the correct flag set in it.
                    DirectCast(strip.Tag, ToolStripMenuItem).Checked = strip.Visible

                Next

                ' End initialisation - allow the strips to be laid out
                pan.EndInit()

            Next

            ' After all saved toolbars are added, add any which have been missed out;
            ' ie. any of those still remaining in the 'removed' dictionary.
            For Each strip As ToolStrip In removed.Values
                toolstripCont.TopToolStripPanel.Join(strip, strip.Location)
            Next

        Catch ex As Exception
            UserMessage.Show(
             My.Resources.frmProcess_ErrorOccurredAttemptingToSetToolStripPositons, ex)

        Finally
            ' Ensure that laying out resumes (and is done immediately) once all the
            ' toolstrips are in position
            toolstripCont.ResumeLayout(True)

        End Try

    End Sub

    ''' <summary>
    ''' Event handler used to handle the 'tools' menu items.
    ''' </summary>
    Private Sub HandleToolsCheckedChanged(
     ByVal sender As Object, ByVal e As EventArgs) _
     Handles mnuToolsStandard.CheckedChanged, mnuToolsDebug.CheckedChanged,
      mnuToolsTools.CheckedChanged, mnuToolsSearch.CheckedChanged, mnuToolsFont.CheckedChanged

        Dim menuItem As ToolStripMenuItem = CType(sender, ToolStripMenuItem)
        If menuItem.Tag IsNot Nothing Then _
         DirectCast(menuItem.Tag, ToolStrip).Visible = menuItem.Checked

    End Sub

    ''' <summary>
    ''' Hide/show all tools event handler.
    ''' </summary>
    Private Sub HandleShowAllClick(ByVal sender As Object, ByVal e As EventArgs) _
     Handles mnuShowAllTools.Click, mnuHideAllTools.Click

        Dim show As Boolean = (sender Is mnuShowAllTools)
        mnuToolsStandard.Checked = show
        mnuToolsFont.Checked = show
        mnuToolsDebug.Checked = show
        mnuToolsTools.Checked = show
        mnuToolsSearch.Checked = show

    End Sub

    ''' <summary>
    ''' Gets the top, bottom, left or right tool strip panel.
    ''' </summary>
    ''' <param name="dock">The dock style</param>
    ''' <returns>The corresponding tool strip panel.</returns>
    Private Function GetToolStripPanelFromDock(ByVal dock As DockStyle) As ToolStripPanel
        For Each ctl As Control In toolstripCont.Controls
            Dim stripPanel As ToolStripPanel = TryCast(ctl, ToolStripPanel)
            If stripPanel IsNot Nothing AndAlso stripPanel.Dock = dock Then Return stripPanel
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Gets a position character based on a tool strip's parent panel
    ''' </summary>
    ''' <param name="strip">The tool strip</param>
    ''' <returns>The position character, either t, b, l, r or f</returns>
    ''' <remarks>Floating toolstrips have not been implemented yet</remarks>
    Private Function GetDockFromToolStrip(ByVal strip As ToolStrip) As DockStyle
        If strip.Parent Is Nothing Then Return DockStyle.None
        Return strip.Parent.Dock
    End Function

    ''' <summary>
    ''' Writes tool strip positions into the DB.
    ''' </summary>
    Private Sub SaveToolStripPositions()

        If DesignMode Then Return
        If mProcessViewer.ModeIsEditable AndAlso mnuToolsSavePositions.Checked Then
            Try
                Dim posns As New List(Of clsUIElementPosition)
                For Each strip As ToolStrip In mToolStrips.Values
                    posns.Add(New clsUIElementPosition(strip.Name, GetDockFromToolStrip(strip),
                     strip.Location, strip.Visible))
                Next
                gSv.SetToolStripPositions(posns, ProcessViewer.ModeIsObjectStudio)

            Catch ex As Exception
                UserMessage.Show(My.Resources.frmProcess_AnErrorOccurredSavingToolStripPositions, ex)

            End Try
        End If


    End Sub

    ''' <summary>
    ''' Handler to adjust tool strip item text alignment.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ToolsToolStrip_LayoutStyleChanged(ByVal sender As Object, ByVal e As EventArgs) _
      Handles toolstripStandard.LayoutStyleChanged,
      toolstripDebug.LayoutStyleChanged,
      toolstripSearch.LayoutStyleChanged

        Dim oToolStrip As ToolStrip = CType(sender, ToolStrip)

        If oToolStrip.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow Then
            For Each oItem As ToolStripItem In oToolStrip.Items
                If TypeOf oItem Is ToolStripButton Or TypeOf oItem Is ToolStripSplitButton Then
                    oItem.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
                    oItem.TextImageRelation = TextImageRelation.ImageBeforeText
                    oItem.TextAlign = ContentAlignment.MiddleRight
                    oItem.ImageAlign = ContentAlignment.MiddleLeft
                    oItem.AutoSize = True
                End If
            Next
        Else
            For Each oItem As ToolStripItem In oToolStrip.Items
                If TypeOf oItem Is ToolStripButton Or TypeOf oItem Is ToolStripSplitButton Then
                    If Not mToolStripItemStyles Is Nothing AndAlso mToolStripItemStyles(oItem) <> Nothing Then
                        oItem.DisplayStyle = CType(mToolStripItemStyles(oItem), ToolStripItemDisplayStyle)
                    End If
                    oItem.ImageAlign = ContentAlignment.MiddleCenter
                    'Trim any padding
                    oItem.Text = oItem.Text.Trim
                End If
            Next
        End If

    End Sub

    Private Sub tsToolBoxButtonStrip_ItemClicked(ByVal sender As Object, ByVal e As System.Windows.Forms.ToolStripItemClickedEventArgs) Handles toolstripTools.ItemClicked, mnuTools.DropDownItemClicked
        Try
            Dim tool As AutomateUI.ctlProcessViewer.StudioTool = CType(e.ClickedItem.Tag, AutomateUI.ctlProcessViewer.StudioTool)
            mProcessViewer.SetCurrentTool(tool)
            mProcessViewer.ChangePointer()
        Catch ex As Exception
            UserMessage.Show(My.Resources.frmProcess_UnexpectedErrorWhilstSettingTheCurrentToolPleaseReportThisProblemToBluePrism, ex)
        End Try
    End Sub

    Private Sub UpdateToolsMenus(ByVal CurrentTool As ctlProcessViewer.StudioTool)
        Dim index As Integer
        For index = 0 To toolstripTools.Items.Count - 1
            If (toolstripTools.Items.Item(index).Enabled) Then
                Dim tsToolStripButton As ToolStripButton = CType(toolstripTools.Items.Item(index), ToolStripButton)
                If (CType(tsToolStripButton.Tag, AutomateUI.ctlProcessViewer.StudioTool) = CurrentTool) Then
                    tsToolStripButton.Checked = True
                Else
                    tsToolStripButton.Checked = False
                End If
            End If
        Next

        For index = 0 To mnuTools.DropDownItems.Count - 1
            If (mnuTools.DropDownItems.Item(index).Enabled) Then
                Dim mnuToolsMenuItem As ToolStripMenuItem = TryCast(mnuTools.DropDownItems.Item(index), ToolStripMenuItem)
                If mnuToolsMenuItem IsNot Nothing Then
                    If (CType(mnuToolsMenuItem.Tag, AutomateUI.ctlProcessViewer.StudioTool) = CurrentTool) Then
                        mnuToolsMenuItem.Checked = True
                    Else
                        mnuToolsMenuItem.Checked = False
                    End If
                End If
            End If
        Next
    End Sub

    Private Sub HandleProcessViewerToolChanged(ByVal NewTool As ctlProcessViewer.StudioTool) Handles mProcessViewer.ToolChanged
        Me.UpdateToolsMenus(NewTool)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="Saved"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the save</param>
    Protected Overridable Sub OnSaved(e As ProcessEventArgs)
        RaiseEvent Saved(Me, e)
    End Sub

    ''' <summary>
    ''' Handles the process viewer indicating that its process has been saved.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub HandleProcessSaved(sender As Object, e As ProcessEventArgs) Handles mProcessViewer.Saved
        OnSaved(e)
    End Sub

#End Region

    ''' <summary>
    ''' Our instance of a breakpoint locator form, if any.
    ''' </summary>
    Private mBreakpointLocatorForm As frmBreakpointLocator

    ''' <summary>
    ''' Makes sure that there is a single instance of the breakpoint properties form.
    ''' </summary>
    Private Sub ShowBreakpointLocator()
        Try
            If mBreakpointLocatorForm Is Nothing OrElse mBreakpointLocatorForm.IsDisposed Then
                mBreakpointLocatorForm = New frmBreakpointLocator()
                mBreakpointLocatorForm.SetEnvironmentColours(Me)
                mBreakpointLocatorForm.SetProcess(mProcessViewer.Process)
                mBreakpointLocatorForm.SetParentProcessStudioForm(Me)
                Me.AddOwnedForm(mBreakpointLocatorForm)
            End If

            Me.mBreakpointLocatorForm.Show()
        Catch ex As Exception
            UserMessage.Show(My.Resources.frmProcess_UnexpectedErrorEx, ex)
        End Try
    End Sub

    Private Sub btnBreakpoint_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBreakpoint.Click
        Me.ShowBreakpointLocator()
    End Sub

#Region "Undo and Redo"

    ''' <summary>
    ''' Used to handle clsProcess.UndoPositionSaved events.
    ''' </summary>
    ''' <param name="s"></param>
    ''' <remarks></remarks>
    Private Sub HandleUndoPositionSaved(ByVal s As clsUndo)

        PopulateUndoMenuItems()
        PopulateRedoMenuItems()

    End Sub


    ''' <summary>
    ''' Populates the undo menu item.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub PopulateUndoMenuItems()

        Dim list As List(Of clsUndo)
        list = mProcessViewer.Process.GetUndoStates()

        Dim oToolStripMenuItem As ToolStripMenuItem
        Dim iCount As Integer

        mnuEditUndo.DropDownItems.Clear()
        btnUndo.DropDownItems.Clear()

        Select Case list.Count
            Case 0
                mnuEditUndo.Text = My.Resources.frmProcess_Undo
                mnuEditUndo.ToolTipText = My.Resources.frmProcess_Undo
                mnuEditUndo.Enabled = False
                btnUndo.Text = My.Resources.frmProcess_Undo
                btnUndo.ToolTipText = My.Resources.frmProcess_Undo
                btnUndo.Enabled = False
                RemoveHandler mnuEditUndo.Click, AddressOf mnuEditUndo_Click

            Case 1
                mnuEditUndo.ShortcutKeys = Keys.Control Or Keys.Z
                mnuEditUndo.Text = String.Format(My.Resources.frmProcess_Undo0, list(0).ToString().ToConditionalLowerNoun())
                mnuEditUndo.Enabled = True
                btnUndo.ToolTipText = mnuEditUndo.Text
                btnUndo.Enabled = True
                AddHandler mnuEditUndo.Click, AddressOf mnuEditUndo_Click

            Case Else
                mnuEditUndo.ShortcutKeys = Keys.None
                mnuEditUndo.Text = My.Resources.frmProcess_Undo
                mnuEditUndo.Enabled = True
                btnUndo.ToolTipText = mnuEditUndo.Text
                btnUndo.Enabled = True
                RemoveHandler mnuEditUndo.Click, AddressOf mnuEditUndo_Click

                iCount = Math.Min(list.Count, MaximumUndoLevel)

                For i As Integer = 0 To iCount - 1

                    'Make a sub item using the parent item details.
                    oToolStripMenuItem = New ToolStripMenuItem(list(i).ToString())
                    If list(i).Description <> "" Then
                        oToolStripMenuItem.ToolTipText = list(i).ToolTip()
                    End If
                    mnuEditUndo.DropDownItems.Add(oToolStripMenuItem)
                    AddHandler oToolStripMenuItem.Click, AddressOf mnuEditUndoItem_Click

                    oToolStripMenuItem = New ToolStripMenuItem(list(i).ToString)
                    If list(i).Description <> "" Then
                        oToolStripMenuItem.ToolTipText = list(i).ToolTip()
                    End If
                    btnUndo.DropDownItems.Add(oToolStripMenuItem)
                    AddHandler oToolStripMenuItem.Click, AddressOf mnuEditUndoItem_Click

                    If i = 0 Then
                        btnUndo.ToolTipText = String.Format(My.Resources.frmProcess_Undo0, list(i).ToString().ToConditionalLowerNoun())
                        oToolStripMenuItem.ShortcutKeys = Keys.Control Or Keys.Z
                    End If
                Next
        End Select

    End Sub


    ''' <summary>
    ''' Populates the redo menu items.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub PopulateRedoMenuItems()

        Dim list As List(Of clsUndo)
        list = mProcessViewer.Process.GetRedoStates()

        Dim oToolStripMenuItem As ToolStripMenuItem
        Dim iCount As Integer

        mnuEditRedo.DropDownItems.Clear()
        btnRedo.DropDownItems.Clear()

        Select Case list.Count
            Case 0
                mnuEditRedo.Text = My.Resources.frmProcess_Redo
                mnuEditRedo.ToolTipText = My.Resources.frmProcess_Redo
                mnuEditRedo.Enabled = False
                btnRedo.Text = My.Resources.frmProcess_Redo
                btnRedo.ToolTipText = My.Resources.frmProcess_Redo
                btnRedo.Enabled = False
                RemoveHandler mnuEditRedo.Click, AddressOf mnuEditRedo_Click

            Case 1
                mnuEditRedo.ShortcutKeys = Keys.Control Or Keys.Y
                mnuEditRedo.Text = String.Format(My.Resources.frmProcess_Redo0, list(0).ToString().ToConditionalLowerNoun())
                mnuEditRedo.Enabled = True
                btnRedo.ToolTipText = mnuEditRedo.Text
                btnRedo.Enabled = True
                AddHandler mnuEditRedo.Click, AddressOf mnuEditRedo_Click

            Case Else
                mnuEditRedo.ShortcutKeys = Keys.None
                mnuEditRedo.Text = My.Resources.frmProcess_Redo
                mnuEditRedo.Enabled = True
                btnRedo.ToolTipText = mnuEditRedo.Text
                btnRedo.Enabled = True
                RemoveHandler mnuEditRedo.Click, AddressOf mnuEditRedo_Click

                iCount = Math.Min(list.Count, MaximumUndoLevel)
                For i As Integer = 0 To iCount - 1

                    btnRedo.ToolTipText = String.Format(My.Resources.frmProcess_Redo0, list(i).ToString().ToConditionalLowerNoun())
                    'Make a sub item using the parent item details.
                    oToolStripMenuItem = New ToolStripMenuItem(list(i).ToString)
                    If list(i).Description <> "" Then
                        oToolStripMenuItem.ToolTipText = list(i).ToolTip()
                    End If
                    mnuEditRedo.DropDownItems.Add(oToolStripMenuItem)
                    AddHandler oToolStripMenuItem.Click, AddressOf mnuEditRedoItem_Click

                    oToolStripMenuItem = New ToolStripMenuItem(list(i).ToString())
                    If list(i).Description <> "" Then
                        oToolStripMenuItem.ToolTipText = list(i).ToolTip()
                    End If
                    btnRedo.DropDownItems.Add(oToolStripMenuItem)
                    AddHandler oToolStripMenuItem.Click, AddressOf mnuEditRedoItem_Click

                    If i = 0 Then
                        btnRedo.ToolTipText = String.Format(My.Resources.frmProcess_Redo0, list(i).ToString().ToConditionalLowerNoun())
                        oToolStripMenuItem.ShortcutKeys = Keys.Control Or Keys.Y
                    End If
                Next
        End Select


    End Sub

#End Region

#Region "mnuEditUndo_MouseEnter"

    ''' <summary>
    ''' Used to force a redraw.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub mnuEditUndo_MouseEnter(ByVal sender As System.Object, ByVal e As System.EventArgs)

        CType(sender, ToolStripItem).Invalidate()

    End Sub

#End Region

#Region "mnuEditUndoItem_Click"

    ''' <summary>
    ''' Undo menu subitem click handler.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub mnuEditUndoItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

        Dim SenderItem As ToolStripMenuItem = CType(sender, ToolStripMenuItem)

        Dim pi As System.Reflection.PropertyInfo
        If SenderItem Is mnuEditRedo Then
            pi = GetType(ToolStripMenuItem).GetProperty("DropDownItems", Reflection.BindingFlags.IgnoreCase Or Reflection.BindingFlags.Instance Or Reflection.BindingFlags.Public)
        Else
            pi = GetType(ToolStripSplitButton).GetProperty("DropDownItems", Reflection.BindingFlags.IgnoreCase Or Reflection.BindingFlags.Instance Or Reflection.BindingFlags.Public)
        End If
        If SenderItem.OwnerItem IsNot Nothing Then
            Dim SiblingItems As ToolStripItemCollection = CType(pi.GetValue(SenderItem.OwnerItem, Nothing), ToolStripItemCollection)

            Dim Position As Integer = 1 + SiblingItems.IndexOf(SenderItem)
            If mProcessViewer.Undo(Position) Then
                PopulateUndoMenuItems()
                PopulateRedoMenuItems()
                SetStatusBarText(My.Resources.frmProcess_UndidMultipleActions)
            End If
        Else
                'This is the top level menu item rather than a child item
                mProcessViewer.Undo()
        End If
    End Sub

#End Region

#Region "mnuEditRedoItem_Click"

    ''' <summary>
    ''' Redo menu subitem click handler.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub mnuEditRedoItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim SenderItem As ToolStripMenuItem = CType(sender, ToolStripMenuItem)

        Dim pi As System.Reflection.PropertyInfo
        If TypeOf SenderItem.OwnerItem Is ToolStripMenuItem Then
            pi = GetType(ToolStripMenuItem).GetProperty("DropDownItems", Reflection.BindingFlags.IgnoreCase Or Reflection.BindingFlags.Instance Or Reflection.BindingFlags.Public)
        Else
            pi = GetType(ToolStripSplitButton).GetProperty("DropDownItems", Reflection.BindingFlags.IgnoreCase Or Reflection.BindingFlags.Instance Or Reflection.BindingFlags.Public)
        End If
        If SenderItem.OwnerItem IsNot Nothing Then
            Dim SiblingItems As ToolStripItemCollection = CType(pi.GetValue(SenderItem.OwnerItem, Nothing), ToolStripItemCollection)
            Dim Position As Integer = 1 + SiblingItems.IndexOf(SenderItem)
            If mProcessViewer.Redo(Position) Then
                PopulateUndoMenuItems()
                PopulateRedoMenuItems()
                SetStatusBarText(My.Resources.frmProcess_RedidMultipleActions)
            End If
        Else
                'This is the top level menu item rather than a child item
                mProcessViewer.Redo()
        End If
    End Sub

#End Region

#Region "btnUndo_Click"

    ''' <summary>
    ''' Undo button click handler.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub btnUndo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
    Handles btnUndo.ButtonClick

        If mProcessViewer.Process.CanUndo Then
            Undo()
        End If

    End Sub

#End Region

#Region "btnRedo_Click"

    ''' <summary>
    ''' Redo button click handler.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub btnRedo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) _
    Handles btnRedo.ButtonClick

        If mProcessViewer.Process.CanRedo Then
            Redo()
        End If

    End Sub

#End Region


    ''' <summary>
    ''' Performs a single undo operation.
    ''' </summary>
    Private Sub Undo()

        If mProcessViewer.Undo() Then
            'only update  if something has changed.
            SetStatusBarText(String.Format(My.Resources.frmProcess_Undid0, mnuEditUndo.ToolTipText))
            PopulateUndoMenuItems()
            PopulateRedoMenuItems()
        End If
    End Sub


    ''' <summary>
    ''' Performs a single redo operation.
    ''' </summary>
    Private Sub Redo()

        If mProcessViewer.Redo() Then
            'only update  if something has changed.
            SetStatusBarText(String.Format(My.Resources.frmProcess_Redid0, mnuEditRedo.ToolTipText))
            PopulateUndoMenuItems()
            PopulateRedoMenuItems()
        End If
    End Sub



    ''' <summary>
    ''' Updates the expression edit row with the currently selected stage.
    ''' </summary>
    ''' <param name="objStage"></param>
    Public Sub UpdateExpressionEditRow(ByVal objStage As clsProcessStage)

        If objStage Is Nothing Then
            ClearAndDisableExpressionRow()
        Else
            Select Case objStage.StageType
                Case StageTypes.Calculation
                    Dim cal As clsCalculationStage = TryCast(objStage, clsCalculationStage)
                    Me.objStoreInEdit.Enabled = True
                    Me.objStoreInEdit.AutoCreateDefault = String.Format(My.Resources.frmProcess_ResultOf0, cal.GetName)
                    objStoreInEdit.Text = cal.StoreIn
                    exprEdit.Text = cal.Expression.LocalForm
                    Me.exprEdit.Stage = cal
                    Me.exprEdit.Enabled = True

                Case StageTypes.Decision
                    Dim dec As clsDecisionStage = TryCast(objStage, clsDecisionStage)
                    exprEdit.Text = dec.Expression.LocalForm
                    objStoreInEdit.Text = String.Empty
                    Me.exprEdit.Stage = dec
                    Me.exprEdit.Enabled = True
                    Me.objStoreInEdit.Enabled = False

                Case StageTypes.Alert
                    Dim alertStg As clsAlertStage = TryCast(objStage, clsAlertStage)
                    exprEdit.Text = alertStg.Expression.LocalForm
                    objStoreInEdit.Text = String.Empty
                    Me.exprEdit.Stage = alertStg
                    Me.exprEdit.Enabled = True
                    Me.objStoreInEdit.Enabled = False

                Case Else
                    ClearAndDisableExpressionRow()
            End Select

            If mProcessViewer.IsPropertyWindowOpen(objStage.Id) Then
                DisableExpressionRow()
            End If
        End If
    End Sub

    ''' <summary>
    ''' Clears and disables the expression row
    ''' </summary>
    Private Sub ClearAndDisableExpressionRow()
        Me.exprEdit.Enabled = False
        Me.objStoreInEdit.Enabled = False
        Me.exprEdit.Text = String.Empty
        Me.objStoreInEdit.Text = String.Empty
    End Sub

    Private Sub DisableExpressionRow()
        Me.exprEdit.Enabled = False
        Me.objStoreInEdit.Enabled = False
    End Sub


    ''' <summary>
    ''' Handles lost focus events of the expression row controls.
    ''' </summary>
    Private Sub ExpressionEditRow_LostFocus(ByVal sender As Object, ByVal e As EventArgs) Handles exprEdit.LostFocus, objStoreInEdit.LostFocus
        If Not exprEdit.Stage Is Nothing Then
            Select Case exprEdit.Stage.StageType
                Case StageTypes.Calculation
                    Dim cal As clsCalculationStage = TryCast(exprEdit.Stage, clsCalculationStage)
                    cal.StoreIn = objStoreInEdit.Text
                    cal.Expression = BPExpression.FromLocalised(exprEdit.Text)
                Case StageTypes.Decision
                    Dim dec As clsDecisionStage = TryCast(exprEdit.Stage, clsDecisionStage)
                    dec.Expression = BPExpression.FromLocalised(exprEdit.Text)
                Case StageTypes.Alert
                    Dim alert As clsAlertStage = TryCast(exprEdit.Stage, clsAlertStage)
                    alert.Expression = BPExpression.FromLocalised(exprEdit.Text)
                Case Else
                    'Do nothing
            End Select
        End If
    End Sub

    Private Sub objStoreInEdit_AutoCreateRequested(ByVal name As String) Handles objStoreInEdit.AutoCreateRequested
        'Info used the last time we auto-placed a stage
        Static lastAddedStg As clsProcessStage
        Static lastRelPosn As clsProcessStagePositioner.RelativePositions

        'We assume that the calc stage is selected in the process
        Dim proc As clsProcess = ProcessViewer.Process

        If proc.GetSelectionType() <> clsProcessSelection.SelectionType.Stage Then Return
        Try
            Dim stg As clsCalculationStage =
             CType(proc.GetStage(proc.SelectionContainer.PrimarySelection.mgStageID), clsCalculationStage)

            Dim dtype As DataType =
             clsProcessStagePositioner.DataTypeFromExpression(stg, stg.Expression)

            Dim item As clsDataStage = clsProcessStagePositioner.CreateDataItem(
             name, stg, dtype, lastAddedStg, lastRelPosn)

            If item IsNot Nothing Then stg.StoreIn = item.Name
        Catch
        End Try

    End Sub



    Private Sub TransparencyMenus_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuTransparencyNone.Click, mnuTransparencyLow.Click, mnuTransparencyMedium.Click, mnuTransparencyHigh.Click, mnuTransparencyVeryHigh.Click
        Dim Item As ToolStripMenuItem = CType(sender, ToolStripMenuItem)
        If IsNumeric(Item.Tag) Then
            Try
                Me.Opacity = CType(Item.Tag, Double)
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.frmProcess_UnexpectedErrorExMsg0, ex.Message), ex)
            End Try
        Else
            UserMessage.Show(My.Resources.frmProcess_InternalErrorMenuItemIsNotProperlyConfiguredPleaseNotifyBluePrismAboutThisProbl)
        End If
    End Sub

    Private Sub AlwaysOnTopToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AlwaysOnTopToolStripMenuItem.Click
        Dim MenuItem As ToolStripMenuItem = CType(sender, ToolStripMenuItem)
        MenuItem.Checked = Not MenuItem.Checked
        Me.TopMost = MenuItem.Checked
    End Sub


    Private Sub mnuEdit_DropDownOpened(ByVal sender As Object, ByVal e As System.EventArgs) Handles mnuEdit.DropDownOpened
        'Selectively disable some of the menu options as appropriate ...
        mnuEditUndo.Enabled = mProcessViewer.Process.CanUndo
        mnuEditRedo.Enabled = mProcessViewer.Process.CanRedo
        Select Case mProcessViewer.Process.GetSelectionType
            Case clsProcessSelection.SelectionType.None
                SetEditMenuCutCopyDeleteEnabled(False)
                SetMenuPropertiesEnabled(False)
            Case clsProcessSelection.SelectionType.Multiple, clsProcessSelection.SelectionType.Link
                SetEditMenuCutCopyDeleteEnabled(True)
                SetMenuPropertiesEnabled(False)
            Case Else
                SetEditMenuCutCopyDeleteEnabled(True)
                SetMenuPropertiesEnabled(True)
        End Select
        Me.mnuEditPaste.Enabled = mProcessViewer.Process.CanPaste AndAlso mProcessViewer.ModeIsEditable
        'Only enable Selected stages if some stages are selected
        If mProcessViewer.Process.SelectionContainer.Count > 0 Then
            SelectedStagesToolStripMenuItem.Enabled = True
        Else
            SelectedStagesToolStripMenuItem.Enabled = False
        End If
    End Sub


    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        Me.Cursor = Cursors.WaitCursor

        Try
            If Not mPanView Is Nothing Then
                mPanView.Dispose()
            End If

            If disposing Then

                CloseLoadingForm()
                mSkillsToolbar = Nothing

                If ProcessViewer IsNot Nothing AndAlso
             ProcessViewer.Process IsNot Nothing AndAlso
             ProcessViewer.Process.ProcessType = DiagramType.Object AndAlso
             ProcessViewer.Process.AMI IsNot Nothing Then
                    RemoveHandler ProcessViewer.Process.AMI.ApplicationStatusChanged,
                    AddressOf HandleApplicationStatusChanged
                End If

                If mTextLog IsNot Nothing Then mTextLog.Dispose() : mTextLog = Nothing

                If Not (components Is Nothing) Then
                    components.Dispose()
                End If
            End If
            MyBase.Dispose(disposing)

        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    ''' <summary>
    ''' Enables/disables logging on all or selected stages.
    ''' </summary>
    Private Sub LoggingMenuItems_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles EnableLoggingAll.Click, DisableLoggingAll.Click, EnableErrorLoggingAll.Click,
     EnableLoggingSelected.Click, DisableLoggingSelected.Click, EnableErrorLoggingSelected.Click
        Dim mode As LogInfo.InhibitModes
        Dim sel As Boolean

        Try
            If sender Is EnableLoggingSelected Then
                mode = LogInfo.InhibitModes.Never
                sel = True
            ElseIf sender Is EnableErrorLoggingSelected Then
                mode = LogInfo.InhibitModes.OnSuccess
                sel = True
            ElseIf sender Is DisableLoggingSelected Then
                mode = LogInfo.InhibitModes.Always
                sel = True
            ElseIf sender Is EnableLoggingAll Then
                mode = LogInfo.InhibitModes.Never
                sel = False
            ElseIf sender Is EnableErrorLoggingAll Then
                mode = LogInfo.InhibitModes.OnSuccess
                sel = False
            ElseIf sender Is DisableLoggingAll Then
                mode = LogInfo.InhibitModes.Always
                sel = False
            End If

            'If some chanes have been made, add latest state to the undo buffer
            If SetLoggingMode(mode, sel) > 0 Then
                Dim summary As String = CStr(IIf(sel, My.Resources.frmProcess_SelectedStages, My.Resources.frmProcess_AllStages))
                Dim desc As String = String.Format(My.Resources.frmProcess_AllStagesTo1, summary, CType(sender, ToolStripMenuItem).Text)
                ProcessViewer.Process.SaveUndoPosition(clsUndo.ActionType.ChangeLoggingOf, summary, desc)
            End If
        Catch ex As Exception
            UserMessage.Show(My.Resources.frmProcess_FailedToApplyLoggingMode)
        End Try
    End Sub

    ''' <summary>
    ''' Apply the specified logging mode to all (or a selection of) stages.
    ''' </summary>
    ''' <param name="mode">The logging mode to set</param>
    ''' <param name="useSelection">If True only selected stages are updated</param>
    Private Function SetLoggingMode(ByVal mode As LogInfo.InhibitModes,
     ByVal useSelection As Boolean) As Integer
        Dim proc As clsProcess = ProcessViewer.Process
        Dim stgCount As Integer = 0

        If useSelection Then
            For Each sel As clsProcessSelection In proc.SelectionContainer
                proc.GetStage(sel.mgStageID).LogInhibit = mode
                stgCount += 1
            Next
        Else
            For Each st As clsProcessStage In proc.GetStages()
                st.LogInhibit = mode
                stgCount += 1
            Next
        End If

        Return stgCount
    End Function

    ''' <summary>
    ''' Holds a reference to the textbox logging engine, or Nothing if we are not
    ''' using one.
    ''' </summary>
    Private mTextLog As clsTextBoxLoggingEngine


    ''' <summary>
    ''' Handles the click event of the logging toggle button.
    ''' </summary>
    Private Sub btnLogging_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnLogging.Click

        If mTextLog Is Nothing Then Exit Sub

        Const DefaultExpandedLogHeight As Integer = 150
        Const ReservedDiagramHeight As Integer = 48

        If Not mTextLog.Toggled() Then
            Dim dist As Integer = splitMain.Height - DefaultExpandedLogHeight
            ' Reserve some space for the diagram regardless of window size
            If dist <= ReservedDiagramHeight Then dist = ReservedDiagramHeight
            If dist >= ReservedDiagramHeight Then splitMain.SplitterDistance = dist

            btnLogging.Text = My.Resources.frmProcess_HideLogMessages
        Else
            splitMain.SplitterDistance = splitMain.Height 'This will not actually be set less than the minimum size policy.
            btnLogging.Text = My.Resources.frmProcess_ShowLogMessages
        End If

        mTextLog.Toggle()
    End Sub

    ''' <summary>
    ''' Handles the click event of the view process log button.
    ''' </summary>
    Private Sub btnShowSysManLogger_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnShowSysManLogger.Click
        If ProcessViewer.Process.Session Is Nothing Then
            UserMessage.Show(My.Resources.frmProcess_NoLogIsAvailableSinceThisProcessIsNotYetRunning)
            Exit Sub
        End If

        Try
            mParent.StartForm(New frmLogViewer(ProcessViewer.Process.Session.ID))
        Catch ex As PermissionException
            UserMessage.ShowPermissionMessage()
        End Try
    End Sub

    ''' <summary>
    ''' Handles the click evednt of the debug exceptions menu item.
    ''' </summary>
    Private Sub mnuDebugExceptions_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuDebugExceptions.CheckedChanged
        ProcessViewer.Process.BreakOnHandledException = mnuDebugExceptions.Checked
    End Sub

    Private Sub mProcessViewer_ValidationErrorCountUpdated(ByVal Count As Integer) Handles mProcessViewer.ValidationErrorCountUpdated
        Me.btnValidate.Text = String.Format(My.Resources.frmProcess_0Errors, Count)
    End Sub

    Private Sub mobjValidateResults_ValidationErrorCountUpdated(ByVal count As Integer) Handles mValidateResultsForm.ValidationErrorCountUpdated
        ProcessViewer.FireValidationErrorCountUpdated(count)
    End Sub

    Private Sub mnuElementUsage_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuElementUsage.Click

        If mProcessViewer.HasNeverEverBeenSaved Then
            UserMessage.Show(My.Resources.frmProcess_SaveBeforeAttemptingToGenerateThisReport)
            Return
        End If
        Dim f As New SaveFileDialog()
        f.AddExtension = True
        f.DefaultExt = "csv"
        f.Filter = My.Resources.frmProcess_CSVFilesCsvCsv
        If f.ShowDialog(Me) = System.Windows.Forms.DialogResult.OK Then
            Dim r As New clsElementUsageReporter()
            Try
                Dim args As New List(Of Object)
                args.Add(gSv.GetProcessNameByID(mProcessID))
                Using sw As New StreamWriter(f.FileName)
                    r.Generate(args, sw)
                End Using
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.frmProcess_FailedToGenerateTheReport0, ex.Message))
            End Try
        End If

    End Sub

    Private Sub zDyn_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles zDyn.KeyDown
        If e.KeyCode = Keys.Enter Then
            UpdateZoomPercent(zDyn.Text)
        End If
    End Sub

    Private Sub mZoomToolStrip_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles mZoomToolstripCombo.KeyDown
        If e.KeyCode = Keys.Enter Then
            UpdateZoomPercent(mZoomToolstripCombo.Text)
        End If
    End Sub

    Private Sub mZoomToolStrip_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles mZoomToolstripCombo.SelectedIndexChanged

        If Not IsHandleCreated Then Return

        UpdateZoomPercent(mZoomToolstripCombo.Text)
    End Sub

    Private Sub mZoomInToolstripButton_Click(sender As Object, e As EventArgs) Handles mZoomInToolstripButton.Click
        mProcessViewer.ZoomIn()
    End Sub

    Private Sub mZoomOutToolstripButton_Click(sender As Object, e As EventArgs) Handles mZoomOutToolstripButton.Click
        mProcessViewer.ZoomOut()
    End Sub

    Private Sub UpdateZoomPercent(ByVal txt As String)
        txt = RegularExpressions.Regex.Replace(txt, "(\s*%\s*)+", "")
        Dim percent As Integer
        If Integer.TryParse(txt, percent) Then mProcessViewer.DoZoom(percent)
    End Sub

    Private Sub Tool_MouseMove(sender As Object, e As MouseEventArgs) Handles _
        btnAction.MouseMove,
        btnAlert.MouseMove,
        btnAnchor.MouseMove,
        btnCalculation.MouseMove,
        btnChoice.MouseMove,
        btnCode.MouseMove,
        btnCollection.MouseMove,
        btnData.MouseMove,
        btnDecision.MouseMove,
        btnEnd.MouseMove,
        btnException.MouseMove,
        btnLoop.MouseMove,
        btnMultipleCalculation.MouseMove,
        btnNavigate.MouseMove,
        btnNote.MouseMove,
        btnPage.MouseMove,
        btnProcess.MouseMove,
        btnRead.MouseMove,
        btnRecover.MouseMove,
        btnResume.MouseMove,
        btnWait.MouseMove,
        btnWrite.MouseMove

        If Not e.Button = System.Windows.Forms.MouseButtons.Left Then Return

        Dim item As ToolStripItem = TryCast(sender, ToolStripItem)
        If item Is Nothing Then Return

        Dim stageContainer = New ctlProcessViewer.StageDropContainer() With {
            .Tool = CType(item.Tag, ctlProcessViewer.StudioTool)}
        item.DoDragDrop(stageContainer, DragDropEffects.Copy)
    End Sub

    ''' <summary>
    ''' Triggers closing of the loading form following initialisation of the form - see remarks
    ''' on mLoadingWindowCloseTimer field for further details
    ''' </summary>
    Private Sub mLoadingWindowCloseTimer_Tick(sender As Object, e As EventArgs) Handles mLoadingWindowCloseTimer.Tick
        CloseLoadingForm()
        mLoadingWindowCloseTimer.Enabled = False
    End Sub

    Private Sub CloseLoadingForm()
        If mLoadingFormToken IsNot Nothing Then
            mLoadingFormToken.Dispose()
            mLoadingFormToken = Nothing
        End If
    End Sub

    Private Sub SkillToolbarVisiblityToolStripMenuItem_CheckedChanged(sender As Object, e As EventArgs) Handles SkillToolbarVisiblityToolStripMenuItem.CheckedChanged
        SkillsToolbarPanel.Visible = SkillToolbarVisiblityToolStripMenuItem.Checked
        ResizeSkillsToolbar()
    End Sub

    Private Sub frmProcess_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SetupSkillsToolbar()
    End Sub

    ''' <summary>
    ''' This method is required as there are some issues surrounding <see cref="ElementHost"/>
    ''' with regards that the Dock property doesn't work correctly as the hosted WPF control doesn't
    ''' recieve any sizing information on initial creation in order to resize itself to the size of
    ''' its parent. Therefore we need to supply it at runtime in order to explictly set its height
    ''' property to the size of the parent control to ensure that it displays correctly in the UI.
    ''' We can then subsequently update the size of the toolbar manually whenever the form is resized.
    ''' </summary>
    Private Sub SetupSkillsToolbar()
        Dim elementHost = New ElementHost()
        mSkillsToolbar = New SkillsToolbar()

        elementHost.Dock = DockStyle.Fill
        elementHost.AutoSize = True
        elementHost.BackColorTransparent = True
        elementHost.Child = mSkillsToolbar

        SkillsToolbarPanel.Controls.Add(elementHost)

        ResizeSkillsToolbar()
    End Sub

    Private Sub ResizeSkillsToolbar()
        If mSkillsToolbar IsNot Nothing Then
            mSkillsToolbar.Height = SkillsToolbarPanel.Height
        End If
    End Sub

    Private Sub frmProcess_SizeChanged(sender As Object, e As EventArgs) Handles MyBase.SizeChanged
        ResizeSkillsToolbar()
    End Sub
End Class
