Option Strict On

Imports System.IO
Imports System.IO.Compression
Imports System.Text.RegularExpressions
Imports System.Globalization
Imports AutomateControls
Imports AutomateControls.Trees

Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateProcessCore

Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.BPCoreLib.InputOutput
Imports BluePrism.BPCoreLib
Imports LocaleTools
Imports BluePrism.Server.Domain.Models

''' Project  : Automate
''' Class    : ctlArchivingInterface
''' 
''' <summary>
''' The manual archiving interface for System Manager.
''' </summary>
Friend Class ctlArchivingInterface

    Inherits System.Windows.Forms.UserControl
    Implements IHelp
    Implements IPermission
    Implements IChild

#Region " Class-scope Declarations "

    ''' <summary>
    ''' Minor wrapper around a treenode which provides a node to hold a session log,
    ''' setting the text and tag appropriately, and providing access to the session
    ''' object which created it.
    ''' </summary>
    Private Class SessionNode : Inherits TreeNode
        ' The session held by this node
        Private mSession As clsSessionLog

        ''' <summary>
        ''' Creates a new session node for the given log
        ''' </summary>
        ''' <param name="sess">The session for which a node is required.</param>
        Public Sub New(sess As clsSessionLog)
            mSession = sess
            Me.Tag = sess.SessionId
            Me.Text = String.Format("{0:t}", sess.StartDateTime)
        End Sub

        ''' <summary>
        ''' The session held within this node.
        ''' </summary>
        Public ReadOnly Property Session As clsSessionLog
            Get
                Return mSession
            End Get
        End Property
    End Class

#End Region

#Region " Windows Form Designer generated code "

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer
    Friend WithEvents btnRestore As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnArchive As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnViewDBLog As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents trvArchivedLogs As clsAutoCheckingTreeView
    Friend WithEvents trvDBLogs As clsAutoCheckingTreeView
    Friend WithEvents lblArchiveData As System.Windows.Forms.Label
    Friend WithEvents lblDatabaseData As System.Windows.Forms.Label
    Friend WithEvents btnViewArchiveLog As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents mProgressBar As System.Windows.Forms.ProgressBar
    Friend WithEvents splitMain As System.Windows.Forms.SplitContainer
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents btnDelete As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents bwDatabaseTreeLoader As System.ComponentModel.BackgroundWorker
    Private WithEvents bwDirTreeLoader As System.ComponentModel.BackgroundWorker
    Friend WithEvents btnSaveFolder As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnBrowse As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents txtArchivePath As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents lblPathLabel As Label
    Friend WithEvents ctlProcessDateToDate As ctlProcessDate
    Friend WithEvents ctlProcessDateFromDate As ctlProcessDate
    Friend WithEvents lblDateRange As Label
    Friend WithEvents lblEnd As Label
    Friend WithEvents lblStart As Label
    Friend WithEvents DateRangeValidationToolTip As ToolTip
    Friend WithEvents lblPercent As System.Windows.Forms.Label
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlArchivingInterface))
        Me.lblArchiveData = New System.Windows.Forms.Label()
        Me.btnRestore = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnArchive = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnViewDBLog = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnViewArchiveLog = New AutomateControls.Buttons.StandardStyledButton()
        Me.lblDatabaseData = New System.Windows.Forms.Label()
        Me.mProgressBar = New System.Windows.Forms.ProgressBar()
        Me.splitMain = New System.Windows.Forms.SplitContainer()
        Me.trvArchivedLogs = New AutomateUI.clsAutoCheckingTreeView()
        Me.btnDelete = New AutomateControls.Buttons.StandardStyledButton()
        Me.trvDBLogs = New AutomateUI.clsAutoCheckingTreeView()
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.lblPercent = New System.Windows.Forms.Label()
        Me.bwDatabaseTreeLoader = New System.ComponentModel.BackgroundWorker()
        Me.bwDirTreeLoader = New System.ComponentModel.BackgroundWorker()
        Me.btnSaveFolder = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnBrowse = New AutomateControls.Buttons.StandardStyledButton()
        Me.txtArchivePath = New AutomateControls.Textboxes.StyledTextBox()
        Me.lblPathLabel = New System.Windows.Forms.Label()
        Me.lblDateRange = New System.Windows.Forms.Label()
        Me.lblEnd = New System.Windows.Forms.Label()
        Me.lblStart = New System.Windows.Forms.Label()
        Me.DateRangeValidationToolTip = New System.Windows.Forms.ToolTip(Me.components)
        Me.ctlProcessDateToDate = New AutomateUI.ctlProcessDate()
        Me.ctlProcessDateFromDate = New AutomateUI.ctlProcessDate()
        CType(Me.splitMain, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitMain.Panel1.SuspendLayout()
        Me.splitMain.Panel2.SuspendLayout()
        Me.splitMain.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblArchiveData
        '
        resources.ApplyResources(Me.lblArchiveData, "lblArchiveData")
        Me.lblArchiveData.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblArchiveData.Name = "lblArchiveData"
        '
        'btnRestore
        '
        resources.ApplyResources(Me.btnRestore, "btnRestore")
        Me.btnRestore.Name = "btnRestore"
        '
        'btnArchive
        '
        resources.ApplyResources(Me.btnArchive, "btnArchive")
        Me.btnArchive.Name = "btnArchive"
        '
        'btnViewDBLog
        '
        resources.ApplyResources(Me.btnViewDBLog, "btnViewDBLog")
        Me.btnViewDBLog.Name = "btnViewDBLog"
        '
        'btnViewArchiveLog
        '
        resources.ApplyResources(Me.btnViewArchiveLog, "btnViewArchiveLog")
        Me.btnViewArchiveLog.Name = "btnViewArchiveLog"
        '
        'lblDatabaseData
        '
        resources.ApplyResources(Me.lblDatabaseData, "lblDatabaseData")
        Me.lblDatabaseData.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblDatabaseData.Name = "lblDatabaseData"
        '
        'mProgressBar
        '
        resources.ApplyResources(Me.mProgressBar, "mProgressBar")
        Me.mProgressBar.Name = "mProgressBar"
        Me.mProgressBar.Step = 1
        '
        'splitMain
        '
        resources.ApplyResources(Me.splitMain, "splitMain")
        Me.splitMain.Name = "splitMain"
        '
        'splitMain.Panel1
        '
        Me.splitMain.Panel1.Controls.Add(Me.trvArchivedLogs)
        Me.splitMain.Panel1.Controls.Add(Me.lblArchiveData)
        Me.splitMain.Panel1.Controls.Add(Me.btnRestore)
        Me.splitMain.Panel1.Controls.Add(Me.btnViewArchiveLog)
        '
        'splitMain.Panel2
        '
        Me.splitMain.Panel2.Controls.Add(Me.btnDelete)
        Me.splitMain.Panel2.Controls.Add(Me.trvDBLogs)
        Me.splitMain.Panel2.Controls.Add(Me.lblDatabaseData)
        Me.splitMain.Panel2.Controls.Add(Me.btnArchive)
        Me.splitMain.Panel2.Controls.Add(Me.btnViewDBLog)
        '
        'trvArchivedLogs
        '
        resources.ApplyResources(Me.trvArchivedLogs, "trvArchivedLogs")
        Me.trvArchivedLogs.CheckBoxes = True
        Me.trvArchivedLogs.HideSelection = False
        Me.trvArchivedLogs.Name = "trvArchivedLogs"
        Me.trvArchivedLogs.UseToolTips = False
        '
        'btnDelete
        '
        resources.ApplyResources(Me.btnDelete, "btnDelete")
        Me.btnDelete.Name = "btnDelete"
        Me.btnDelete.UseVisualStyleBackColor = True
        '
        'trvDBLogs
        '
        resources.ApplyResources(Me.trvDBLogs, "trvDBLogs")
        Me.trvDBLogs.CheckBoxes = True
        Me.trvDBLogs.HideSelection = False
        Me.trvDBLogs.Name = "trvDBLogs"
        Me.trvDBLogs.UseToolTips = False
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.ForeColor = System.Drawing.SystemColors.ControlText
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'lblPercent
        '
        resources.ApplyResources(Me.lblPercent, "lblPercent")
        Me.lblPercent.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblPercent.Name = "lblPercent"
        '
        'bwDatabaseTreeLoader
        '
        '
        'bwDirTreeLoader
        '
        '
        'btnSaveFolder
        '
        resources.ApplyResources(Me.btnSaveFolder, "btnSaveFolder")
        Me.btnSaveFolder.Name = "btnSaveFolder"
        '
        'btnBrowse
        '
        resources.ApplyResources(Me.btnBrowse, "btnBrowse")
        Me.btnBrowse.Name = "btnBrowse"
        '
        'txtArchivePath
        '
        resources.ApplyResources(Me.txtArchivePath, "txtArchivePath")
        Me.txtArchivePath.Name = "txtArchivePath"
        Me.txtArchivePath.ReadOnly = True
        '
        'lblPathLabel
        '
        resources.ApplyResources(Me.lblPathLabel, "lblPathLabel")
        Me.lblPathLabel.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblPathLabel.Name = "lblPathLabel"
        '
        'lblDateRange
        '
        resources.ApplyResources(Me.lblDateRange, "lblDateRange")
        Me.lblDateRange.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblDateRange.Name = "lblDateRange"
        '
        'lblEnd
        '
        resources.ApplyResources(Me.lblEnd, "lblEnd")
        Me.lblEnd.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblEnd.Name = "lblEnd"
        '
        'lblStart
        '
        resources.ApplyResources(Me.lblStart, "lblStart")
        Me.lblStart.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblStart.Name = "lblStart"
        '
        'ctlProcessDateToDate
        '
        Me.ctlProcessDateToDate.DateButtonVisible = True
        resources.ApplyResources(Me.ctlProcessDateToDate, "ctlProcessDateToDate")
        Me.ctlProcessDateToDate.MaxValue = Nothing
        Me.ctlProcessDateToDate.MinValue = Nothing
        Me.ctlProcessDateToDate.Name = "ctlProcessDateToDate"
        Me.ctlProcessDateToDate.ReadOnly = False
        Me.ctlProcessDateToDate.Tag = "204,20"
        Me.ctlProcessDateToDate.TimeButtonVisible = False
        '
        'ctlProcessDateFromDate
        '
        Me.ctlProcessDateFromDate.DateButtonVisible = True
        resources.ApplyResources(Me.ctlProcessDateFromDate, "ctlProcessDateFromDate")
        Me.ctlProcessDateFromDate.MaxValue = Nothing
        Me.ctlProcessDateFromDate.MinValue = Nothing
        Me.ctlProcessDateFromDate.Name = "ctlProcessDateFromDate"
        Me.ctlProcessDateFromDate.ReadOnly = False
        Me.ctlProcessDateFromDate.Tag = "204,20"
        Me.ctlProcessDateFromDate.TimeButtonVisible = False
        '
        'ctlArchivingInterface
        '
        Me.Controls.Add(Me.lblStart)
        Me.Controls.Add(Me.lblEnd)
        Me.Controls.Add(Me.lblDateRange)
        Me.Controls.Add(Me.ctlProcessDateToDate)
        Me.Controls.Add(Me.ctlProcessDateFromDate)
        Me.Controls.Add(Me.lblPathLabel)
        Me.Controls.Add(Me.txtArchivePath)
        Me.Controls.Add(Me.lblPercent)
        Me.Controls.Add(Me.btnBrowse)
        Me.Controls.Add(Me.btnSaveFolder)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.splitMain)
        Me.Controls.Add(Me.mProgressBar)
        Me.Name = "ctlArchivingInterface"
        resources.ApplyResources(Me, "$this")
        Me.splitMain.Panel1.ResumeLayout(False)
        Me.splitMain.Panel1.PerformLayout()
        Me.splitMain.Panel2.ResumeLayout(False)
        Me.splitMain.Panel2.PerformLayout()
        CType(Me.splitMain, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitMain.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

#Region " Member Variables "

    ''' <summary>
    ''' Regular expression which matches the format used to name a directory
    ''' representing a month in the archived logs
    ''' </summary>
    Private ReadOnly mMonthRegex As New Regex(
     "^(0[1-9]|1[012])\s" &
     "(" & CollectionUtil.Join(DateTimeFormatInfo.CurrentInfo.MonthNames, "|") & ")$")

    ''' <summary>
    ''' Regular expression which matches the format used to name a directory
    ''' representing a day in the archived logs
    ''' </summary>
    Private ReadOnly mDayRegex As New Regex(
     "^(0[1-9]|[12][0-9]|3[01])\s" &
     "(" & CollectionUtil.Join(DateTimeFormatInfo.CurrentInfo.DayNames, "|") & ")$")

    ''' <summary>
    ''' The archiving object used for transferring logs to and from file and db
    ''' </summary>
    Private WithEvents mArchiver As clsArchiver

    ''' <summary>
    ''' Used to remember the expanded nodes prior to repopulating 
    ''' the database tree.
    ''' </summary>
    Private mDatabaseTreeExpandedNodes As List(Of TreeNode)

    ''' <summary>
    ''' The database tree root node.
    ''' </summary>
    Private mDatabaseTreeRootNode As TreeNode

    ''' <summary>
    ''' Used to remember the expanded nodes prior to repopulating 
    ''' the archive tree.
    ''' </summary>
    Private mArchiveTreeExpandedNodes As List(Of TreeNode)

    ''' <summary>
    ''' The archive tree root node.
    ''' </summary>
    Private mArchiveTreeRootNode As TreeNode

    Private mLastNodeChecked As TreeNode

    Private mOldProcessFromDate As Date

    Private mOldProcessToDate As Date

    Private Const mShowTooltipForMiliseconds As Integer = 5000
#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new archiving interface control.
    ''' </summary>
    Public Sub New()
        InitializeComponent()
        ' These need to be here due to a bug in visual studio's designer
        ' See http://tinyurl.com/px5worv for many tales of woe in this area
        splitMain.Panel1MinSize = 160
        splitMain.Panel2MinSize = 160

        Me.trvDBLogs.ContextMenu = New ContextMenu({New MenuItem(My.Resources.ctlArchivingInterface_ClearSelection, Sub() OnClearSelectedClicked(Me, New EventArgs()))})
    End Sub

    Private Sub OnClearSelectedClicked(sender As Object, e As EventArgs)
        trvDBLogs.ClearSelection()
        EnableAllLevels()
        mLastNodeChecked = Nothing
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The currently selected session, or Guid.Empty if no session is selected
    ''' </summary>
    Public ReadOnly Property SelectedSession() As Guid
        Get
            Dim n As TreeNode = trvDBLogs.SelectedNode
            If n IsNot Nothing AndAlso TypeOf n.Tag Is Guid Then _
             Return DirectCast(n.Tag, Guid)
            Return Guid.Empty
        End Get
    End Property

    ''' <summary>
    ''' The collection of session IDs which have been checked in the user interface
    ''' </summary>
    Public ReadOnly Property CheckedSessions() As ICollection(Of Guid)
        Get
            Dim ids As New List(Of Guid)
            For Each n As TreeNode In New TreeNodeEnumerable(trvDBLogs)
                ' Checking for a GUID is not enough since resource and process IDs
                ' are also stored in the node tags, so we check that it's a leaf
                If n.Checked AndAlso TypeOf n.Tag Is Guid AndAlso n.Nodes.Count = 0 _
                 Then ids.Add(DirectCast(n.Tag, Guid))
            Next
            Return ids
        End Get
    End Property

    ''' <summary>
    ''' The collection of files which have been checked in the user interface
    ''' </summary>
    Public ReadOnly Property CheckedFiles() As ICollection(Of FileInfo)
        Get
            Dim files As New List(Of FileInfo)
            For Each n As TreeNode In New TreeNodeEnumerable(trvArchivedLogs)
                'add the node if it represents a file
                If n.Checked AndAlso TypeOf n.Tag Is FileInfo Then _
                 files.Add(DirectCast(n.Tag, FileInfo))
            Next
            Return files
        End Get
    End Property

    ''' <summary>
    ''' Gets whether any file has been checked in the user interface
    ''' </summary>
    Private ReadOnly Property IsAnyFileChecked() As Boolean
        Get
            For Each n As TreeNode In New TreeNodeEnumerable(trvArchivedLogs)
                If n.Checked AndAlso TypeOf n.Tag Is FileInfo Then Return True
            Next
            Return False
        End Get
    End Property

    ''' <summary>
    ''' Gets whether any session has been checked in the user interface
    ''' </summary>
    Private ReadOnly Property IsAnySessionChecked() As Boolean
        Get
            For Each n As TreeNode In New TreeNodeEnumerable(trvDBLogs)
                If n.Checked AndAlso TypeOf n.Tag Is Guid Then Return True
            Next
            Return False
        End Get
    End Property


    ''' <summary>
    ''' The permission level required to use this control.
    ''' </summary>
    ''' <remarks>this doesn't have to be implemented until System Manager is atomised
    ''' into smaller permissions. We simply put it here to fulfill the IPermission
    ''' interface</remarks>
    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.None
        End Get
    End Property

    ''' <summary>
    ''' Gets the archiver associated with this control.
    ''' If none is currently set in this control, one is created with the archive
    ''' path currently set in the text field.
    ''' Any archiver in this control is set into the parent form so it can keep
    ''' track of any archive / restore operations.
    ''' </summary>
    Public ReadOnly Property Archiver() As clsArchiver
        Get
            If mArchiver Is Nothing Then
                mArchiver = New clsArchiver(txtArchivePath.Text)
                mParent.Archiver = mArchiver
            End If
            Return mArchiver
        End Get
    End Property

    ''' <summary>
    ''' Checks if this archiver panel is aware of a background archive operation in
    ''' progress.
    ''' </summary>
    ''' <remarks>This will be false if the 'parent' (ie. The
    ''' <see cref="frmApplication"/> hosting this panel, not the parent window) is
    ''' not set, or if it has no archiver set within it.</remarks>
    Public ReadOnly Property IsBusy() As Boolean
        Get
            Return Archiver.IsBackgroundOperationInProgress()
        End Get
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Releases the unmanaged resources used by the Component and optionally 
    ''' releases the managed resources.
    ''' </summary>
    ''' <param name="disposing">true to release both managed and unmanaged resources; 
    ''' false to release only unmanaged resources.</param>
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)

        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If

        ' You wouldn't normally need to do this, but since mArchiver is WithEvents
        ' and it outlasts this instance (it's stored in the frmApplication in order
        ' to allow archiving to operate in the background after this control is
        ' left), then we need to ensure that this instance is no longer registered
        ' as a listener on its events - setting it to null achieves this.
        mArchiver = Nothing

        ' Try and remove our temp dir and any temp files therein,
        ' If it fails for any reason - ignore and move on.
        Try
            Dim dir As DirectoryInfo = GetTemporaryViewDirectory(False)
            If dir.Exists Then dir.Delete(True)
        Catch
        End Try

        MyBase.Dispose(disposing)
    End Sub

    Private mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
            If mParent IsNot Nothing Then
                Dim archiver As clsArchiver = mParent.Archiver
                If archiver IsNot Nothing Then
                    mArchiver = archiver
                    SetBusy(archiver.IsBackgroundOperationInProgress())
                End If
            End If
        End Set
    End Property

    ''' <summary>
    ''' Returns the help file associated with this control.
    ''' </summary>
    ''' <returns>Returns the help file associated with this control.</returns>
    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "HELPMISSING"
    End Function

    ''' <summary>
    ''' Handles the archiver's archive operation completing.
    ''' </summary>
    Private Sub HandleArchiveCompleted(ByVal sender As Object, ByVal e As OperationCompletedEventArgs) _
     Handles mArchiver.ArchiveCompleted

        SetBusy(False)

        PopulateDatabaseTree()
        PopulateArchiveTree()

        If e.Cancelled Then
            UserMessage.Show(My.Resources.ArchiveOperationCancelled)

        ElseIf e.Error IsNot Nothing Then

            Dim faee As FileAlreadyExistsException = TryCast(e.Error, FileAlreadyExistsException)
            If faee IsNot Nothing Then
                UserMessage.Show(
                 My.Resources.AFileForOneOfTheCheckedSessionsAlreadyExistsArchivingAbortedFileName & faee.File.FullName)

            Else
                UserMessage.Show(My.Resources.ArchiveOperationFailed, e.Error)

            End If

        Else
            UserMessage.Show(My.Resources.ArchiveOperationComplete)

        End If

    End Sub

    ''' <summary>
    ''' Handles the archiver's restore operation completing.
    ''' </summary>
    Private Sub HandleRestoreCompleted(ByVal sender As Object, ByVal e As OperationCompletedEventArgs) _
     Handles mArchiver.RestoreCompleted

        SetBusy(False)

        PopulateDatabaseTree()
        PopulateArchiveTree()

        If e.Cancelled Then
            UserMessage.Show(My.Resources.RestoreOperationCancelled)

        ElseIf e.Error IsNot Nothing Then

            Dim saee As SessionAlreadyExistsException =
             TryCast(e.Error, SessionAlreadyExistsException)
            Dim knfe As KeyNotFoundException = TryCast(e.Error, KeyNotFoundException)

            If saee IsNot Nothing Then
                UserMessage.Err(saee,
                 My.Resources.OneOfTheCheckedSessionsAlreadyExistsOnTheDatabaseRestoreOperationStopped0Sessio, vbCrLf, saee.SessionId)

            ElseIf knfe IsNot Nothing Then
                ' The inner exception is the SQL exception that was thrown, which
                ' should indicate which data is missing and causing the restore to
                ' fail. See clsServer.RestoreSessionLog(clsSessionLog)
                UserMessage.Err(knfe.InnerException,
                 My.Resources.OneOfTheCheckedSessionsCouldNotBeRestoredBecauseItRequiresOtherDataWhichIsNotAv)

            Else
                UserMessage.Err(e.Error,
                 My.Resources.RestoreOperationFailed01, vbCrLf, e.Error.Message)

            End If

        Else
            UserMessage.Show(My.Resources.RestoreOperationComplete)
        End If

    End Sub

    ''' <summary>
    ''' Handles the progress changing in the contained archiver.
    ''' </summary>
    Private Sub HandleProgressChanged(ByVal sender As Object, ByVal e As ProgressChangedEventArgs) _
     Handles mArchiver.ProgressChanged

        mProgressBar.Visible = True
        mProgressBar.Value = e.ProgressPercentage
        lblPercent.Text = e.ProgressPercentage & "%"

    End Sub

    ''' <summary>
    ''' Applies the new root node to the tree view.
    ''' </summary>
    Private Sub HandleDBWorkerCompleted(
     ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs) _
     Handles bwDatabaseTreeLoader.RunWorkerCompleted

        If e.Error IsNot Nothing Then
            trvDBLogs.Nodes.Clear()
            UserMessage.Err(e.Error,
             My.Resources.ThereWasAnErrorPopulatingTheDatabaseTreeviewTheErrorMessageWas0, e.Error.Message)
        Else
            If mDatabaseTreeRootNode IsNot Nothing Then
                trvDBLogs.BeginUpdate()
                trvDBLogs.Nodes.Clear()
                If mDatabaseTreeRootNode IsNot Nothing Then
                    trvDBLogs.Nodes.Add(mDatabaseTreeRootNode)
                End If
                ExpandNodes(trvDBLogs, mDatabaseTreeExpandedNodes)
                trvDBLogs.EndUpdate()
            End If
        End If
        trvDBLogs.Enabled = True

    End Sub

    ''' <summary>
    ''' Checks if the user wants to create the given directory or not.
    ''' </summary>
    ''' <param name="dir">The directory which is currently not present and needs to
    ''' be checked to see if the user wants to create it or not.</param>
    ''' <returns>True if the user says that they do want to create it; false
    ''' otherwise.</returns>
    Private Function UserWantsToCreateDirectory(ByVal dir As String) As Boolean

        Return (
         MessageBox.Show(String.Format(
          My.Resources.TheArchiveDirectory0DoesNotExistDoYouWantToCreateIt1NoteThatYouWillBeUnableToAr,
          dir, vbCrLf),
          My.Resources.DirectoryDoesNotExist,
          MessageBoxButtons.OKCancel, MessageBoxIcon.Question
        ) = DialogResult.OK)

    End Function

    ''' <summary>
    ''' Applies the new root node to the tree view.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub HandleDirWorkerCompleted(ByVal sender As Object,
     ByVal e As RunWorkerCompletedEventArgs) Handles bwDirTreeLoader.RunWorkerCompleted

        If e.Error IsNot Nothing Then

            If TypeOf e.Error Is DirectoryNotFoundException Then
                If UserWantsToCreateDirectory(txtArchivePath.Text) Then

                    Try
                        Directory.CreateDirectory(txtArchivePath.Text)
                        ' Repopulate the archive tree after this work has been completed.
                        BeginInvoke(New MethodInvoker(AddressOf PopulateArchiveTree))

                    Catch ex As Exception
                        UserMessage.Show(String.Format(
                         My.Resources.TheDirectory0CouldnTBeCreated1,
                         txtArchivePath.Text, ex.Message), ex)

                        trvArchivedLogs.Nodes.Clear()

                    End Try
                Else
                    ' If the user doesn't want to create the directory, just ensure
                    ' that the treeview is clear - no point in erroring; the user
                    ' will have already been told that the directory is not valid
                    ' when checking if they wanted to create it
                    trvArchivedLogs.Nodes.Clear()

                End If
            Else
                UserMessage.Show(My.Resources.ThereWasAnErrorPopulatingTheArchivedFilesTreeview, e.Error)
                trvArchivedLogs.Nodes.Clear()

            End If

        Else
            trvArchivedLogs.BeginUpdate()
            trvArchivedLogs.Nodes.Clear()
            If mArchiveTreeRootNode IsNot Nothing Then
                trvArchivedLogs.Nodes.Add(mArchiveTreeRootNode)
            End If
            ExpandNodes(trvArchivedLogs, mArchiveTreeExpandedNodes)
            trvArchivedLogs.EndUpdate()

        End If

        trvArchivedLogs.Enabled = True
        btnBrowse.Enabled = True

    End Sub

    ''' <summary>
    ''' Gets the node for the given value from the given parent node, creating it and
    ''' inserting it into the parent's node collection if necessary.
    ''' </summary>
    ''' <param name="parent">The parent node on which the value's node representation
    ''' should be found.</param>
    ''' <param name="value">The value for which the node representing it is required.
    ''' </param>
    ''' <returns>The treenode which corresponds to the given value.</returns>
    Private Function GetNodeFor(parent As TreeNode, value As IComparable) As TreeNode
        Return GetNodeFor(parent, value, value.ToString())
    End Function

    ''' <summary>
    ''' Gets the node for the given value from the given parent node, creating it and
    ''' inserting it into the parent's node collection if necessary.
    ''' </summary>
    ''' <param name="parent">The parent node on which the value's node representation
    ''' should be found.</param>
    ''' <param name="value">The value for which the node representing it is required.
    ''' </param>
    ''' <param name="display">The display text for the node, if it is created in this
    ''' call. Note that this will have no effect if the node already exists.</param>
    ''' <returns>The treenode which corresponds to the given value.</returns>
    Private Function GetNodeFor(
     parent As TreeNode, value As IComparable, display As String) As TreeNode
        ' Find the insert point for the node (assuming it's not already there).
        Dim index As Integer
        ' Go up to (and including) the count of nodes. If we want to append a node
        ' we need to ensure that 'i' points to one after the last node.
        For index = 0 To parent.Nodes.Count
            ' If we've run out of nodes, exit now and the new node will be appended
            ' to the end of the collection
            If index = parent.Nodes.Count Then Exit For

            Dim n As TreeNode = parent.Nodes(index)
            Dim compareResult As Integer =
                DirectCast(n.Tag, IComparable).CompareTo(value)

            ' If we've reached a node representing a later value, we want to insert
            ' the new node at this point, so exit now and insert it.
            If compareResult > 0 Then Exit For

            ' If we've actually found the value, return that, creating nothing new
            If compareResult = 0 Then Return n

            ' Otherwise, move to the next node and see if that matches/exceeds the
            ' value for which we require a node
        Next

        ' If we get here, then we must insert a new node at the current 'index',
        ' and return it.
        Dim newNode As New TreeNode() With {.Text = display, .Tag = value}
        parent.Nodes.Insert(index, newNode)
        Return newNode
    End Function

    ''' <summary>
    ''' Adds a session to the database treeview, creating nodes as necessary to hold
    ''' it.
    ''' </summary>
    ''' <param name="sess">The session to add to the database</param>
    Private Sub AddSessionToDb(sess As clsSessionLog)
        Dim node As TreeNode = mDatabaseTreeRootNode
        node = GetNodeFor(node, sess.StartDateTime.Year)
        node = GetNodeFor(node, sess.StartDateTime.Month,
                          String.Format("{0:MMMM}", sess.StartDateTime))
        node = GetNodeFor(node, sess.StartDateTime.Day,
                          String.Format(My.Resources.ctlArchivingInterface_DayName0DayOfMonth1, sess.StartDateTime))
        node = GetNodeFor(node, sess.ProcessName)
        node = GetNodeFor(node, sess.RunningResourceName)

        ' Now we need to create a node for this session, inserted in time order into
        ' the other nodes underneath 'node'.
        ' We work this much the same way that GetNodeFor does, except that a) we're
        ' using SessionNode instances and b) we don't want to use an existing node
        Dim i As Integer
        For i = 0 To node.Nodes.Count
            If i = node.Nodes.Count Then Exit For
            Dim n As SessionNode = TryCast(node.Nodes(i), SessionNode)
            If n Is Nothing Then Continue For
            If n.Session.StartTimeOfDay >= sess.StartTimeOfDay Then Exit For
        Next
        node.Nodes.Insert(i, New SessionNode(sess))
    End Sub

    ''' <summary>
    ''' Takes a datatable of sessions and populates the database 
    ''' treeview root node with the contained sessions.
    ''' </summary>
    Private Sub HandleDbWorkerWork(ByVal sender As Object,
     ByVal e As DoWorkEventArgs) Handles bwDatabaseTreeLoader.DoWork

        Dim dateFilter = TryCast(e.Argument, List(Of Object))
        Dim fromDate As Date = DirectCast(dateFilter(0), Date)
        Dim toDate As Date = DirectCast(dateFilter(1), Date)

        Dim sessions As ICollection(Of clsSessionLog) = gSv.GetSessionLogs(fromDate, toDate, Nothing)
        For Each session As clsSessionLog In sessions
            ' We're only interested in complete sessions
            If session.IsFinished Then AddSessionToDb(session)
        Next
    End Sub

    ''' <summary>
    ''' Handles a log file being walked by the directory walker, building up the
    ''' treenodes required to contain it in the file treeview.
    ''' </summary>
    Private Sub HandleLogFileWalked(
     ByVal sender As Object, ByVal e As DirectoryWalkerFileEventArgs)
        ' We want to create nodes for all elements in the directory beyond the
        ' root directory
        Dim root As DirectoryInfo = DirectCast(sender, DirectoryWalker).RootDir
        Dim f As FileInfo = e.Info

        ' Create a stack and push all directories into it starting from the directory
        ' which contains the log file being walked
        Dim dirs As New Stack(Of DirectoryInfo)
        Dim d As DirectoryInfo = f.Directory
        While d IsNot Nothing AndAlso d.FullName <> root.FullName
            dirs.Push(d)
            d = d.Parent
        End While

        ' We need to use nodes to represent each of the directories to the root
        ' So start from the root, and recreate the directory structure from there
        ' as nodes
        Dim n As TreeNode = mArchiveTreeRootNode
        For Each dir As DirectoryInfo In dirs
            n = GetOrCreateNode(n, dir)
        Next

        ' n now represents the directory in which the file is held, so create a
        ' node to represent the file in that node
        Dim fNode As New TreeNode(f.Name)
        fNode.Name = fNode.Text
        fNode.Tag = e.Info
        n.Nodes.Add(fNode)

    End Sub

    ''' <summary>
    ''' Handles errors which occur while walking the archive directory
    ''' </summary>
    Private Sub HandleLogFileWalkError(
     ByVal sender As Object, ByVal e As DirectoryWalkerDirErrorEventArgs)
        If TypeOf e.Exception Is System.UnauthorizedAccessException Then _
         e.Ignore = True
    End Sub

    ''' <summary>
    ''' Gets or creates the treenode representing the given directory within the
    ''' specified treenode.
    ''' </summary>
    ''' <param name="parent">The node under which the directory treenode should be.
    ''' </param>
    ''' <param name="dir">The directory whose representative treenode is required.
    ''' </param>
    ''' <returns>The TreeNode representing the given directory under the specified
    ''' parent treenode. This may be created within this method, or it may have
    ''' already existed before this method was called.</returns>
    Private Function GetOrCreateNode(
     ByVal parent As TreeNode, ByVal dir As DirectoryInfo) As TreeNode
        Dim n As TreeNode = CollectionUtil.First(parent.Nodes.Find(dir.Name, False))

        ' If the node does not exist, create it.
        If n Is Nothing Then

            'The month and day folder name formats are '04 April' 
            'and '03 Tuesday'. This makes the treeview hard to read 
            'so change the node text to 'April' and 'Tuesday 4th'
            Dim txt As String = dir.Name
            Dim m As Match = mMonthRegex.Match(txt)
            If m.Success Then
                ' Matches the month regex - just use the month name for the node
                txt = m.Groups(2).Value
            Else
                ' Not a month - test against the day regex, just in case
                m = mDayRegex.Match(txt)
                If m.Success Then
                    ' It's a representation of a day - replace '03 Tuesday' with
                    ' 'Tuesday 03' or such like.
                    Dim day As Integer = CInt(m.Groups(1).Value)
                    Dim resString As String = Regex.Replace(My.Resources.ctlArchivingInterface_DayName0DayOfMonth1, "[{}0:]", "") 'I18n apply culture order
                    txt = resString.Replace("dddd", m.Groups(2).Value).Replace("dd", day.ToString("D2"))
                End If
                ' Otherwise, it's neither month nor day - just leave it as is
            End If

            n = New TreeNode(txt)
            n.Name = dir.Name 'The name should match the dir name even if txt changes
            n.Tag = dir
            parent.Nodes.Add(n)
        End If
        Return n
    End Function

    ''' <summary>
    ''' Loads the archive treeview root node with logs found in the archive path.
    ''' </summary>
    Private Sub HandleDirWorkerWork(ByVal sender As Object,
     ByVal e As DoWorkEventArgs) Handles bwDirTreeLoader.DoWork

        Dim sArchivePath As String = mArchiveTreeRootNode.Text

        ' NB Visual Studio will break for an unhandled exception here. This is a
        ' known VS problem and not a bug in the code, so just press Continue.
        ' The exception is handled in the RunWorkerCompleted event handler.
        If Not Directory.Exists(sArchivePath) Then _
         Throw New DirectoryNotFoundException(String.Format(
         My.Resources.TheArchivePath0IsInvalid, sArchivePath))

        Dim rootDir As New DirectoryInfo(sArchivePath)

        mArchiveTreeRootNode.Tag = rootDir

        Dim walker As New DirectoryWalker(sArchivePath, "*.bpl*")
        AddHandler walker.FileWalked, AddressOf HandleLogFileWalked
        AddHandler walker.DirectoryError, AddressOf HandleLogFileWalkError
        walker.Walk()

    End Sub

    ''' <summary>
    ''' Sets a background worker going to populate the DB tree view.
    ''' </summary>
    Private Sub PopulateDatabaseTree()

        btnViewDBLog.Enabled = False
        btnArchive.Enabled = False
        btnDelete.Enabled = False
        trvDBLogs.Enabled = False

        mDatabaseTreeExpandedNodes = GetExpandedNodes(trvDBLogs)
        mDatabaseTreeRootNode =
         New TreeNode(ApplicationProperties.ApplicationName & " Database")

        If User.LoggedIn Then
            Dim dateFilter = New List(Of Object) From {
                ctlProcessDateFromDate.Value.GetDateValue,
                ctlProcessDateToDate.Value.GetDateValue.AddDays(1).AddSeconds(-1)
            }
            bwDatabaseTreeLoader.RunWorkerAsync(dateFilter)
        End If

    End Sub

    ''' <summary>
    ''' Sets a background worker going to populate the archive tree view.
    ''' </summary>
    Private Sub PopulateArchiveTree()
        If bwDirTreeLoader.IsBusy Then
            ' The tree is already populating. This shouldn't actually happen, since
            ' when we start populating, we disable all the UI elements that could
            ' trigger another.
            Exit Sub
        End If

        btnViewArchiveLog.Enabled = False
        btnRestore.Enabled = False
        trvArchivedLogs.Enabled = False
        btnBrowse.Enabled = False

        mArchiveTreeExpandedNodes = GetExpandedNodes(trvArchivedLogs)
        mArchiveTreeRootNode = New TreeNode(txtArchivePath.Text)

        bwDirTreeLoader.RunWorkerAsync()

    End Sub

    ''' <summary>
    ''' Gets a list of the last expanded node in each expanded branch of the tree.
    ''' </summary>
    ''' <param name="oTreeView"></param>
    ''' <returns></returns>
    Private Function GetExpandedNodes(ByVal oTreeView As TreeView) As List(Of TreeNode)

        Dim aExpandedNodes As New List(Of TreeNode)

        If oTreeView IsNot Nothing AndAlso Not oTreeView.IsDisposed Then
            For Each oChildNode As TreeNode In oTreeView.Nodes
                If oChildNode.IsExpanded Then
                    GetExpandedNodes(oChildNode, aExpandedNodes)
                End If
            Next
        End If
        Return aExpandedNodes

    End Function

    ''' <summary>
    ''' Builds a list of the last expanded node in each expanded branch of the tree.
    ''' </summary>
    ''' <param name="oParentNode"></param>
    ''' <param name="aExpandedNodes"></param>
    Private Sub GetExpandedNodes(ByVal oParentNode As TreeNode, ByRef aExpandedNodes As List(Of TreeNode))

        Dim bAddedParent As Boolean

        For Each oChildNode As TreeNode In oParentNode.Nodes

            If oChildNode.IsExpanded Then
                'Keep drilling down.
                GetExpandedNodes(oChildNode, aExpandedNodes)
            Else
                'The expansion does not go any further, so 
                'add the parent node to the list.
                bAddedParent = True
            End If

        Next

        If bAddedParent OrElse (oParentNode.Nodes.Count = 0 And oParentNode.IsExpanded) Then
            If Not aExpandedNodes.Contains(oParentNode) Then
                aExpandedNodes.Add(oParentNode)
            End If
        End If

    End Sub

    ''' <summary>
    ''' Takes a list of nodes, builds an 'expanded branch' from the ancestors of each
    ''' node, and tries to apply this expanded branch to the tree.
    ''' </summary>
    ''' <param name="oTreeView"></param>
    ''' <param name="aExpandedNodes"></param>
    ''' <remarks>NB This method relies on the arrangement of nodes in  years, months
    ''' and dates.</remarks>
    Private Sub ExpandNodes(ByVal oTreeView As TreeView, ByVal aExpandedNodes As List(Of TreeNode))

        If oTreeView Is Nothing OrElse aExpandedNodes Is Nothing Then
            Exit Sub
        End If

        Dim aExpandedNodeBranch As List(Of String)
        Dim bMatchFound As Boolean
        Dim aNodes As TreeNodeCollection = oTreeView.Nodes

        For Each oExpandedNode As TreeNode In aExpandedNodes

            'Build a list of ancestor names for the expanded node.
            aExpandedNodeBranch = New List(Of String)
            aExpandedNodeBranch.Add(oExpandedNode.Text)
            While oExpandedNode.Parent IsNot Nothing
                aExpandedNodeBranch.Insert(0, oExpandedNode.Parent.Text)
                oExpandedNode = oExpandedNode.Parent
            End While

            'Starting at the top of the tree, work through the ancestor 
            'branch looking for a match at each level.
            aNodes = oTreeView.Nodes
            For Each sExpandedNodeName As String In aExpandedNodeBranch

                bMatchFound = False
                For Each oNode As TreeNode In aNodes

                    If sExpandedNodeName = oNode.Text Then
                        'Found a match, so move to the next level.
                        bMatchFound = True
                        oNode.Expand()
                        aNodes = oNode.Nodes
                        Exit For
                    End If

                Next

                If aNodes Is Nothing OrElse aNodes.Count = 0 Then
                    'Reached a leaf node, so go to the next branch.
                    Exit For
                End If

                If Not bMatchFound Then
                    'No node matching the expanded name could be 
                    'found, so abandon this branch and try the next.
                    Exit For
                End If
            Next

        Next

    End Sub

    ''' <summary>
    ''' Handles the control being loaded by populating the tree views, and getting
    ''' the initial archive path from the options file.
    ''' </summary>
    Protected Overrides Sub OnLoad(ByVal e As EventArgs)

        MyBase.OnLoad(e)

        If Not DesignMode Then

            txtArchivePath.Text = Options.Instance.ArchivePath
            ctlProcessDateFromDate.Value = New clsProcessValue(DataType.date, Today.AddDays(-7), True)
            ctlProcessDateToDate.Value = New clsProcessValue(DataType.date, Today, True)

            mOldProcessFromDate = ctlProcessDateFromDate.Value.GetDateValue
            mOldProcessToDate = ctlProcessDateToDate.Value.GetDateValue

            ctlProcessDateToDate.MinValue = ctlProcessDateFromDate.Value.GetDateValue
            ctlProcessDateToDate.MaxValue = Date.Today
            ctlProcessDateFromDate.MaxValue = ctlProcessDateToDate.Value.GetDateValue

            PopulateDatabaseTree()
            PopulateArchiveTree()

            mProgressBar.Visible = False

        End If

    End Sub

    ''' <summary>
    ''' Handles a treeview item being checked.
    ''' This ensures that any disabled items are not checked.
    ''' </summary>
    Private Sub trvArchivedLogs_BeforeCheck(ByVal sender As Object, ByVal e As TreeViewCancelEventArgs) Handles trvArchivedLogs.BeforeCheck
        'here we disable the checkbox on the grey nodes.
        If e.Node.ForeColor.Equals(SystemColors.GrayText) Then e.Cancel = True
    End Sub

    Private Sub trvDBLogs_BeforeCheck(ByVal sender As Object, ByVal e As TreeViewCancelEventArgs) Handles trvDBLogs.BeforeCheck
        'here we disable the checkbox on the grey nodes.
        If e.Action = TreeViewAction.Unknown Then Return

        If e.Node.ForeColor.Equals(Color.LightSlateGray) Then _
            e.Cancel = True
    End Sub

    ''' <summary>
    ''' Handles an item being selected in the database sessions.
    ''' Just checks if the item is leaf node representing a database log and
    ''' enables / disables the 'View Log' button accordingly.
    ''' </summary>
    Private Sub trvDBLogs_AfterSelect(ByVal sender As Object, ByVal e As TreeViewEventArgs) Handles trvDBLogs.AfterSelect
        Dim n As TreeNode = e.Node
        Me.btnViewDBLog.Enabled = ((n.Nodes.Count = 0) AndAlso (TypeOf n.Tag Is Guid))
    End Sub

    ''' <summary>
    ''' Handles an item being selected in the file logs.
    ''' Checks if the item is a leaf node and enables / disables the 'View File'
    ''' button accordingly.
    ''' </summary>
    Private Sub trvArchiveLogs_AfterSelect(ByVal sender As Object, ByVal e As TreeViewEventArgs) Handles trvArchivedLogs.AfterSelect
        Me.btnViewArchiveLog.Enabled = (e.Node.Nodes.Count = 0)
    End Sub

    ''' <summary>
    ''' Handles an item being checked in the file logs treeview.
    ''' Just enables/disables the 'Restore' button depending on whether any files
    ''' are checked.
    ''' </summary>
    Private Sub trvArchivedLogs_AfterCheck(
     ByVal sender As Object, ByVal e As TreeViewEventArgs) _
     Handles trvArchivedLogs.AfterCheck
        'Don't act on events not raised by user actions
        If e.Action = TreeViewAction.Unknown Then Return

        btnRestore.Enabled = IsAnyFileChecked

    End Sub

    Private Sub EnableAllLevels()
        For Each node As TreeNode In trvDBLogs.Nodes
            EnableNodeAndChildren(node)
        Next
    End Sub

    Private Sub EnableNodeAndChildren(node As TreeNode)
        node.ForeColor = Color.Black
        For Each child As TreeNode In node.Nodes
            EnableNodeAndChildren(child)
        Next
    End Sub

    Private Sub DisableOtherLevels(clickedNode As TreeNode)
        For Each node As TreeNode In trvDBLogs.Nodes
            DisableNodeNotOnLevel(node, clickedNode.Level)
        Next
    End Sub

    Private Sub DisableNodeNotOnLevel(node As TreeNode, level As Integer)
        If node.Level <> level Then
            node.ForeColor = Color.LightSlateGray
        End If

        For Each child As TreeNode In node.Nodes
            DisableNodeNotOnLevel(child, level)
        Next
    End Sub

    ''' <summary>
    ''' Handles an item being checked in the file logs treeview.
    ''' Just enables/disables the 'Archive' and 'Delete' buttons depending on whether
    ''' any files are checked.
    ''' </summary>
    Private Sub trvDBLogs_AfterCheck(ByVal sender As Object,
     ByVal e As TreeViewEventArgs) Handles trvDBLogs.AfterCheck
        'Don't act on events not raised by user actions
        If e.Action = TreeViewAction.Unknown Then Return

        Dim anySessionsChecked As Boolean = IsAnySessionChecked
        btnArchive.Enabled = anySessionsChecked
        btnDelete.Enabled = anySessionsChecked

        If e.Node.Checked Then
            DisableOtherLevels(e.Node)
            mLastNodeChecked = e.Node
        ElseIf Not trvDBLogs.IsAnyLeafChecked Then
            EnableAllLevels()
            mLastNodeChecked = Nothing
        End If
    End Sub

    ''' <summary>
    ''' Handles the 'Browse' button being clicked by opening a Folder browser
    ''' dialog box to capture the archive folder.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnBrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBrowse.Click
        Dim ofd As New FolderBrowserDialog()
        If Directory.Exists(txtArchivePath.Text) Then
            ofd.SelectedPath = txtArchivePath.Text
        Else
            ofd.RootFolder = Environment.SpecialFolder.Desktop
        End If
        If ofd.ShowDialog() = DialogResult.OK Then
            txtArchivePath.Text = ofd.SelectedPath
            PopulateArchiveTree()
        End If
    End Sub

    ''' <summary>
    ''' Handles the 'Archive' button being clicked.
    ''' This goes through all checked sessions and begins an archive operation on the
    ''' held <see cref="clsArchiver"/> object.
    ''' </summary>
    Private Sub btnArchive_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnArchive.Click

        Dim p As String = txtArchivePath.Text
        If p = "" Then
            UserMessage.Err(
             My.Resources.ErrorYouMustProvideAnDirectoryInWhichTheArchivesAreToBeStored)
            Return

        ElseIf Not Directory.Exists(p) Then
            If UserWantsToCreateDirectory(p) Then
                Try
                    Directory.CreateDirectory(p)

                Catch ex As Exception
                    UserMessage.Err(ex,
                     My.Resources.AnErrorOccurredWhileAttemptingToCreateTheDirectory)
                    Return

                End Try

            Else
                UserMessage.Err(
                 My.Resources.NoSessionsCanBeArchivedWithoutADirectoryToArchiveThemToArchiveCancelled)
                Return

            End If
        End If

        ' By this point the directory is verified
        Dim sessions As ICollection(Of Guid) = CheckedSessions
        If sessions.Count = 0 Then
            UserMessage.Show(My.Resources.NoSessionsAreSelected)
            Return
        End If

        'replace the archiving object because the settings may have changed recently
        Try
            Archiver.ArchivePath = p

        Catch ex As InvalidOperationException
            UserMessage.Show(My.Resources.TheArchiverIsCurrentlyBusy)
            Return

        End Try

        SetBusy(True)
        BuildArchiveAudit(False)

        'Start archiving the logs.
        Dim sErr As String = ""
        If Not mArchiver.CreateArchive(sessions, False, sErr) Then
            UserMessage.Err(
             My.Resources.ErrorsOccuredDuringTheArchivingProcess0TheArchivingContinuedWithAsMuchWorkAsPos,
             vbCrLf, sErr)

            SetBusy(False)

        End If

    End Sub

    ''' <summary>
    ''' Checks for orphaned session logs - ie. session log entries which have been
    ''' marked for deletion, but not yet deleted. If any are found, the user is
    ''' prompted and, assuming they give the nod, they are deleted in a background
    ''' operation which occupies the control (ie. inhibits the user from performing
    ''' any other archive/delete operations).
    ''' </summary>
    Friend Sub CheckForOrphans()
        ' Can't do anything if we're already busy doing stuff
        If IsBusy Then Return

        ' Get the orphans; if there are none, there's nothing to do
        Dim orphans As ICollection(Of Guid) = gSv.GetOrphanedSessionIds()
        If orphans.Count = 0 Then Return

        ' Ask the user if we want to do anything with the orphans.
        Dim resp As DialogResult = MessageBox.Show(LTools.Format(My.Resources.ctlArchivingInterface_plural_ThereArePartiallyDeletedSessions, "COUNT", orphans.Count, "VBCRLF", vbCrLf),
         My.Resources.ClearPartiallyDeletedSessions,
         MessageBoxButtons.OKCancel, MessageBoxIcon.Question)

        ' Anything other than OK means they don't want to do the work - bail now
        If resp <> DialogResult.OK Then Return

        ' Right, let's get to work
        BeginDeleteSessions(orphans)

    End Sub

    ''' <summary>
    ''' Handles the delete button being clicked... just double checks with the user
    ''' first, and then deletes all the session logs that are currently selected.
    ''' </summary>
    Private Sub HandleDeleteClicked(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnDelete.Click
        If UserMessage.OkCancel(
         My.Resources.AreYouSureYouWantToDeleteTheseLogsOnceTheyAreDeletedTheyCannotBeRestored) = MsgBoxResult.Ok Then
            BeginDeleteSessions(CheckedSessions)
        End If
    End Sub

    ''' <summary>
    ''' Begins the deletion of a collection of sessions in the background, ensuring
    ''' that the user is unable to attempt any further operations, and handling any
    ''' errors which occur in that initialization.
    ''' </summary>
    ''' <param name="ids">The IDs of the sessions to delete.</param>
    Private Sub BeginDeleteSessions(ByVal ids As ICollection(Of Guid))
        Try
            SetBusy(True)
            BuildArchiveAudit(True)

            ' Start archiving the logs.
            Dim sErr As String = Nothing
            If Not Archiver.CreateArchive(ids, True, sErr) Then
                UserMessage.Err(
                 My.Resources.ErrorsOccuredDuringTheDeletingProcess0TheArchivingContinuedWithAsMuchWorkAsPoss, vbCrLf, sErr)
                SetBusy(False)
            End If

        Catch ex As Exception
            UserMessage.Show(
             My.Resources.AnErrorOccurredWhileAttemptingToDeleteTheSessions, ex)
            SetBusy(False)

        End Try

    End Sub

    ''' <summary>
    ''' Sets whether this control is busy or not, ensuring that no action
    ''' buttons can be pressed or navigated while there is a background
    ''' archive / restore operation in progress.
    ''' </summary>
    ''' <param name="busy">True to indicate that the archiving interface is
    ''' currently awaiting an archive / restore operation being completed.
    ''' False to indicate that the interface is available for configuration
    ''' and for executing archive / restore operations.</param>
    Private Sub SetBusy(ByVal busy As Boolean)

        btnCancel.Visible = busy

        btnRestore.Enabled = (Not busy AndAlso trvArchivedLogs.IsAnyLeafChecked)
        btnArchive.Enabled = (Not busy AndAlso trvDBLogs.IsAnyLeafChecked)
        btnDelete.Enabled = btnArchive.Enabled

        trvArchivedLogs.Enabled = Not busy
        trvDBLogs.Enabled = Not busy

        If busy Then
            btnCancel.Focus()
        Else
            mProgressBar.Value = 0
            mProgressBar.Visible = False
            lblPercent.Text = ""
            btnCancel.Visible = False
        End If

    End Sub

    ''' <summary>
    ''' Handles the 'Restore' button being clicked.
    ''' This goes through all checked files and begins a restore operation on the 
    ''' held <see cref="clsArchiver"/> object.
    ''' </summary>
    Private Sub btnRestore_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnRestore.Click

        Dim arcPath As String = txtArchivePath.Text
        If arcPath <> "" AndAlso Directory.Exists(arcPath) Then

            ' replace the archiver because the settings may have changed recently
            Try
                Archiver.ArchivePath = arcPath
            Catch ex As InvalidOperationException
                UserMessage.Show(My.Resources.TheArchiverIsCurrentlyBusy)
                Return
            End Try

            ' Disable any buttons which shouldn't be available once the archiver's
            ' on its way
            SetBusy(True)
            BuildRestoreAudit()

            'Restore the files.
            Dim sErr As String = ""
            If Not mArchiver.RestoreArchive(CheckedFiles, sErr) Then
                If InStr(sErr, "FK_BPASession_BPAProcess") > 0 Then
                    sErr = sErr & vbCrLf &
                     My.Resources.NoteArchiveFilesCanOnlyBeRestoredIfTheRelevantProcessesResourcesAndUsersAlready
                End If
                UserMessage.Err(
                 My.Resources.ErrorsOccurredDuringTheRestorationProcessTheRestorationContinuedWithAsMuchWorkA,
                 sErr, vbCrLf)

                SetBusy(False)

            End If

        Else
            UserMessage.Show(My.Resources.TheArchivingPathSeemsToBeInvalidPleaseSetAValidPathForTheArchivingFolderAndTryA)
        End If

    End Sub

    ''' <summary>
    ''' Handles the 'View Log' button being clicked by opening the currently
    ''' selected log in the database logs treeview.
    ''' </summary>
    Private Sub HandleViewDBLog(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnViewDBLog.Click

        Try
            Dim sessionId As Guid = SelectedSession
            If sessionId <> Guid.Empty Then _
            mParent.StartForm(New frmLogViewer(sessionId))

        Catch ex As PermissionException
            UserMessage.Err(My.Resources.UserDoesNotHavePermissionToViewTheLog)
        Catch ex As Exception
            UserMessage.Err(My.Resources.UnableToViewLog & ex.Message)
        End Try


    End Sub

    ''' <summary>
    ''' Gets the temporary directory used for viewing files, creating it if it 
    ''' doesn't already exist.
    ''' </summary>
    ''' <returns>The directory in which we store the temporary files for viewing
    ''' an archived log file.</returns>
    Private Function GetTemporaryViewDirectory() As DirectoryInfo
        Return GetTemporaryViewDirectory(True)
    End Function

    ''' <summary>
    ''' Gets the temporary directory used for viewing files, creating it as directed
    ''' if it doesn't already exist.
    ''' </summary>
    ''' <param name="create">True to create the directory if it doesn't already
    ''' exist - false to not do so.</param>
    ''' <returns>The directory in which we store the temporary files for viewing
    ''' an archived log file.</returns>
    Private Function GetTemporaryViewDirectory(ByVal create As Boolean) As DirectoryInfo
        Dim tempDir As DirectoryInfo = New DirectoryInfo(Path.Combine(
         Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
         "Blue Prism Limited\Automate\Temporary Log Files\"
        ))
        If Not tempDir.Exists Then tempDir.Create()
        Return tempDir
    End Function

    ''' <summary>
    ''' Handles the 'View File' button being clicked.
    ''' This copies the selected file to a <see cref="GetTemporaryViewDirectory">
    ''' temporary directory</see> and opens it in Internet Explorer.
    ''' </summary>
    Private Sub btnViewArchivedLog_Click(
     ByVal sender As Object, ByVal e As EventArgs) Handles btnViewArchiveLog.Click

        Dim selected As TreeNode = trvArchivedLogs.SelectedNode
        If selected Is Nothing OrElse selected.Nodes.Count > 0 Then
            UserMessage.Show(My.Resources.ASingleFileNotAFolderMustBeSelectedInOrderToViewIt)
            Return
        End If

        Dim file As FileInfo = TryCast(selected.Tag, FileInfo)
        If file Is Nothing OrElse Not file.Exists Then
            UserMessage.Show(My.Resources.ThereIsNoFileAssociatedWithTheSelectedElement)
            Return
        End If

        Try
            'read the xml from the file
            ' If file.Name.EndsWith(".gz") Then ' It needs to be uncompressed first
            'open the file in internet explorer
            Dim tempDir As String = Path.Combine(
             Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
             "Blue Prism Limited\Automate\Temporary Log Files\"
            )

            ' Get the filename to use : The file should be *.bpl or *.bpl.gz,
            ' Take that '*' and suffix it with '.xml' 
            Static rx As New Regex("^(.*)\.bpl(\.gz)?$")
            Dim m As Match = rx.Match(file.Name)
            If Not m.Success Then
                UserMessage.Show(
                 My.Resources.UnrecognisedFileTypeItShouldHaveAnExtensionOfBplOrBplGzFilenameWas & file.Name)
                Return
            End If

            Dim outFile As New FileInfo(Path.Combine(tempDir, m.Groups(1).ToString() & ".xml"))
            If Not outFile.Directory.Exists Then outFile.Directory.Create()

            ' We have a filename - copy the file into the temp dir and open it in IE.
            If m.Groups(2).Length = 0 Then ' m.Groups(2) == ".gz" if the file is compressed.
                file.CopyTo(outFile.FullName, True)

            Else ' Awkward - it's compressed, so we must uncompress it on the fly.
                Dim buf(32768 - 1) As Byte ' A buffer to use in between the two streams
                Using inStream As Stream = New GZipStream(file.OpenRead(), CompressionMode.Decompress)
                    Using outStream As Stream = New FileStream(outFile.FullName, FileMode.Create)
                        While True
                            Dim count As Integer = inStream.Read(buf, 0, buf.Length)
                            If count = 0 Then Exit While
                            outStream.Write(buf, 0, count)
                        End While
                        outStream.Flush()
                    End Using
                End Using

            End If

            Dim ie As Object = CreateObject("InternetExplorer.application")
            CallByName(ie, "Navigate", CallType.Method, New Object() {outFile.FullName})
            CallByName(ie, "visible", CallType.Let, New Object() {True})

        Catch ex As Exception
            UserMessage.Show(My.Resources.AnErrorOccurredWhileAttemptingToLoadTheFile, ex)

        End Try

    End Sub

    ''' <summary>
    ''' Handles the Cancel button being clicked, by requesting the archiver cancel
    ''' its current operation - it won't occur immediately.
    ''' </summary>
    Private Sub btnCancel_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnCancel.Click
        Archiver.Cancel()
    End Sub

    ''' <summary>
    ''' Handles the 'Save' button being clicked by checking the archive path and
    ''' saving it to the options file if it is valid.
    ''' </summary>
    Private Sub btnSaveFolder_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSaveFolder.Click
        Dim spath As String = txtArchivePath.Text
        If spath <> "" AndAlso Directory.Exists(spath) Then
            Dim configOptions = Options.Instance
            configOptions.ArchivePath = spath
            Try
                configOptions.Save()
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.UnableToSaveArchivingPath0, ex.Message))
            End Try

            gSv.AuditRecordSysConfigEvent(
                 SysConfEventCode.ModifyArchive,
                 My.Resources.NewSettingsAreModeManualArchiveFolder & spath)
        Else
            UserMessage.Show(My.Resources.YouMustEnterAValidPathBeforeSaving)
        End If
    End Sub

#End Region

#Region " Audit "

    Private Sub BuildArchiveAudit(deleteOnly As Boolean)

        Dim selectedLogs = New List(Of clsSessionLog)
        Dim unselectedLogs = New List(Of clsSessionLog)
        For Each n As TreeNode In New TreeNodeEnumerable(trvDBLogs)
            If TypeOf n.Tag IsNot Guid OrElse n.Nodes.Count <> 0 Then Continue For

            If n.Checked Then
                selectedLogs.Add(DirectCast(n, SessionNode).Session)
            Else
                unselectedLogs.Add(DirectCast(n, SessionNode).Session)
            End If
        Next

        Archiver.SetArchiveAudit(selectedLogs, unselectedLogs, (mLastNodeChecked.Level > 3), Nothing, deleteOnly)
    End Sub

    Private Sub BuildRestoreAudit()

        Dim selectedFiles = New List(Of FileInfo)
        Dim fileProcessMap = New Dictionary(Of String, String)
        Dim unselectedFiles = New List(Of FileInfo)
        For Each n As TreeNode In New TreeNodeEnumerable(trvArchivedLogs)
            If TypeOf n.Tag IsNot FileInfo OrElse n.Nodes.Count <> 0 Then Continue For

            If n.Checked Then
                Dim file = DirectCast(n.Tag, FileInfo)
                selectedFiles.Add(file)
                fileProcessMap(file.Name) = n.Parent.Parent.Text
            Else
                unselectedFiles.Add(DirectCast(n.Tag, FileInfo))
            End If
        Next

        Archiver.SetRestoreAudit(selectedFiles, unselectedFiles, fileProcessMap)
    End Sub
    Private Sub CtlProcessFromDate_Changed(sender As Object, e As EventArgs) Handles ctlProcessDateFromDate.Changed
        If ctlProcessDateFromDate.Value.GetDateValue <= ctlProcessDateToDate.Value.GetDateValue Then
            ctlProcessDateFromDate.MaxValue = ctlProcessDateToDate.Value.GetDateValue
            ctlProcessDateToDate.MinValue = ctlProcessDateFromDate.Value.GetDateValue
            mOldProcessFromDate = ctlProcessDateFromDate.Value.GetDateValue
            PopulateDatabaseTree()
        Else
            ctlProcessDateFromDate.Value = New clsProcessValue(DataType.date, mOldProcessFromDate)
            DateRangeValidationToolTip.Show(My.Resources.TheStartDateMustBeBeforeTheEndDate, ctlProcessDateFromDate, mShowTooltipForMiliseconds)
        End If
    End Sub

    Private Sub CtlProcessToDate_Changed(sender As Object, e As EventArgs) Handles ctlProcessDateToDate.Changed
        If ctlProcessDateFromDate.Value.GetDateValue <= ctlProcessDateToDate.Value.GetDateValue Then
            ctlProcessDateToDate.MinValue = ctlProcessDateFromDate.Value.GetDateValue
            ctlProcessDateFromDate.MaxValue = ctlProcessDateToDate.Value.GetDateValue
            mOldProcessToDate = ctlProcessDateToDate.Value.GetDateValue
            PopulateDatabaseTree()
        Else
            ctlProcessDateToDate.Value = New clsProcessValue(DataType.date, mOldProcessToDate)
            DateRangeValidationToolTip.Show(My.Resources.TheEndDateMustBeafterTheStartDate, ctlProcessDateToDate, mShowTooltipForMiliseconds)
        End If
    End Sub

#End Region

End Class
