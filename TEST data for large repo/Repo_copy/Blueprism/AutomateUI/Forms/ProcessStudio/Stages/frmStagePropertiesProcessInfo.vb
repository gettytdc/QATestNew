
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.BPCoreLib.Collections

''' Project  : Automate
''' Class    : frmStagePropertiesProcessInfo
''' 
''' <summary>
''' The ProcessInfo properties form.
''' </summary>
Friend Class frmStagePropertiesProcessInfo
    Inherits frmProperties

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
    Protected WithEvents TabControl1 As System.Windows.Forms.TabControl
    Friend WithEvents tcConditions As System.Windows.Forms.TabPage
    Protected WithEvents tcInfo As System.Windows.Forms.TabPage
    Friend WithEvents txtCreateDate As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents txtModifiedDate As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents txtModifiedBy As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents txtCreatedBy As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents mPrePostConds As ctlPreconditionsEndconditions
    Protected WithEvents gpLogging As System.Windows.Forms.GroupBox
    Protected WithEvents cbLoggingAbortOnError As System.Windows.Forms.CheckBox
    Protected WithEvents spinLoggingRetryPeriod As AutomateControls.StyledNumericUpDown
    Protected WithEvents spinLoggingAttempts As AutomateControls.StyledNumericUpDown
    Private WithEvents panLoggingOptions As System.Windows.Forms.Panel
    Friend WithEvents Label2 As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents chbxPublish As System.Windows.Forms.CheckBox
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStagePropertiesProcessInfo))
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.tcConditions = New System.Windows.Forms.TabPage()
        Me.mPrePostConds = New AutomateUI.ctlPreconditionsEndconditions()
        Me.tcInfo = New System.Windows.Forms.TabPage()
        Me.gpLogging = New System.Windows.Forms.GroupBox()
        Me.cbLoggingAbortOnError = New System.Windows.Forms.CheckBox()
        Me.panLoggingOptions = New System.Windows.Forms.Panel()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.spinLoggingRetryPeriod = New AutomateControls.StyledNumericUpDown()
        Me.spinLoggingAttempts = New AutomateControls.StyledNumericUpDown()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.txtCreateDate = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtModifiedDate = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtModifiedBy = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtCreatedBy = New AutomateControls.Textboxes.StyledTextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.chbxPublish = New System.Windows.Forms.CheckBox()
        Me.TabControl1.SuspendLayout()
        Me.tcConditions.SuspendLayout()
        Me.tcInfo.SuspendLayout()
        Me.gpLogging.SuspendLayout()
        Me.panLoggingOptions.SuspendLayout()
        CType(Me.spinLoggingRetryPeriod, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.spinLoggingAttempts, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'txtName
        '
        resources.ApplyResources(Me.txtName, "txtName")
        '
        'txtDescription
        '
        resources.ApplyResources(Me.txtDescription, "txtDescription")
        '
        'TabControl1
        '
        resources.ApplyResources(Me.TabControl1, "TabControl1")
        Me.TabControl1.Controls.Add(Me.tcConditions)
        Me.TabControl1.Controls.Add(Me.tcInfo)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        '
        'tcConditions
        '
        Me.tcConditions.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.tcConditions.Controls.Add(Me.mPrePostConds)
        resources.ApplyResources(Me.tcConditions, "tcConditions")
        Me.tcConditions.Name = "tcConditions"
        '
        'mPrePostConds
        '
        resources.ApplyResources(Me.mPrePostConds, "mPrePostConds")
        Me.mPrePostConds.Name = "mPrePostConds"
        Me.mPrePostConds.PostConditions = CType(resources.GetObject("mPrePostConds.PostConditions"), System.Collections.Generic.ICollection(Of String))
        Me.mPrePostConds.PreConditions = CType(resources.GetObject("mPrePostConds.PreConditions"), System.Collections.Generic.ICollection(Of String))
        Me.mPrePostConds.ReadOnly = False
        '
        'tcInfo
        '
        Me.tcInfo.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.tcInfo.Controls.Add(Me.gpLogging)
        Me.tcInfo.Controls.Add(Me.Label4)
        Me.tcInfo.Controls.Add(Me.Label7)
        Me.tcInfo.Controls.Add(Me.txtCreateDate)
        Me.tcInfo.Controls.Add(Me.txtModifiedDate)
        Me.tcInfo.Controls.Add(Me.txtModifiedBy)
        Me.tcInfo.Controls.Add(Me.txtCreatedBy)
        Me.tcInfo.Controls.Add(Me.Label5)
        Me.tcInfo.Controls.Add(Me.Label3)
        resources.ApplyResources(Me.tcInfo, "tcInfo")
        Me.tcInfo.Name = "tcInfo"
        '
        'gpLogging
        '
        Me.gpLogging.Controls.Add(Me.cbLoggingAbortOnError)
        Me.gpLogging.Controls.Add(Me.panLoggingOptions)
        resources.ApplyResources(Me.gpLogging, "gpLogging")
        Me.gpLogging.Name = "gpLogging"
        Me.gpLogging.TabStop = False
        '
        'cbLoggingAbortOnError
        '
        resources.ApplyResources(Me.cbLoggingAbortOnError, "cbLoggingAbortOnError")
        Me.cbLoggingAbortOnError.Name = "cbLoggingAbortOnError"
        Me.cbLoggingAbortOnError.UseVisualStyleBackColor = True
        '
        'panLoggingOptions
        '
        Me.panLoggingOptions.BackColor = System.Drawing.Color.Transparent
        Me.panLoggingOptions.Controls.Add(Me.Label2)
        Me.panLoggingOptions.Controls.Add(Me.Label1)
        Me.panLoggingOptions.Controls.Add(Me.spinLoggingRetryPeriod)
        Me.panLoggingOptions.Controls.Add(Me.spinLoggingAttempts)
        resources.ApplyResources(Me.panLoggingOptions, "panLoggingOptions")
        Me.panLoggingOptions.Name = "panLoggingOptions"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'spinLoggingRetryPeriod
        '
        resources.ApplyResources(Me.spinLoggingRetryPeriod, "spinLoggingRetryPeriod")
        Me.spinLoggingRetryPeriod.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.spinLoggingRetryPeriod.Name = "spinLoggingRetryPeriod"
        Me.spinLoggingRetryPeriod.Value = New Decimal(New Integer() {5, 0, 0, 0})
        '
        'spinLoggingAttempts
        '
        resources.ApplyResources(Me.spinLoggingAttempts, "spinLoggingAttempts")
        Me.spinLoggingAttempts.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.spinLoggingAttempts.Name = "spinLoggingAttempts"
        Me.spinLoggingAttempts.Value = New Decimal(New Integer() {5, 0, 0, 0})
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.Name = "Label4"
        '
        'Label7
        '
        resources.ApplyResources(Me.Label7, "Label7")
        Me.Label7.Name = "Label7"
        '
        'txtCreateDate
        '
        resources.ApplyResources(Me.txtCreateDate, "txtCreateDate")
        Me.txtCreateDate.Name = "txtCreateDate"
        Me.txtCreateDate.ReadOnly = True
        Me.txtCreateDate.TabStop = False
        '
        'txtModifiedDate
        '
        resources.ApplyResources(Me.txtModifiedDate, "txtModifiedDate")
        Me.txtModifiedDate.Name = "txtModifiedDate"
        Me.txtModifiedDate.ReadOnly = True
        Me.txtModifiedDate.TabStop = False
        '
        'txtModifiedBy
        '
        resources.ApplyResources(Me.txtModifiedBy, "txtModifiedBy")
        Me.txtModifiedBy.Name = "txtModifiedBy"
        Me.txtModifiedBy.ReadOnly = True
        Me.txtModifiedBy.TabStop = False
        '
        'txtCreatedBy
        '
        resources.ApplyResources(Me.txtCreatedBy, "txtCreatedBy")
        Me.txtCreatedBy.Name = "txtCreatedBy"
        Me.txtCreatedBy.ReadOnly = True
        Me.txtCreatedBy.TabStop = False
        '
        'Label5
        '
        resources.ApplyResources(Me.Label5, "Label5")
        Me.Label5.Name = "Label5"
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Name = "Label3"
        '
        'chbxPublish
        '
        resources.ApplyResources(Me.chbxPublish, "chbxPublish")
        Me.chbxPublish.Name = "chbxPublish"
        '
        'frmStagePropertiesProcessInfo
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.chbxPublish)
        Me.Controls.Add(Me.TabControl1)
        Me.Name = "frmStagePropertiesProcessInfo"
        Me.Controls.SetChildIndex(Me.mTitleBar, 0)
        Me.Controls.SetChildIndex(Me.txtName, 0)
        Me.Controls.SetChildIndex(Me.txtDescription, 0)
        Me.Controls.SetChildIndex(Me.TabControl1, 0)
        Me.Controls.SetChildIndex(Me.chbxPublish, 0)
        Me.TabControl1.ResumeLayout(False)
        Me.tcConditions.ResumeLayout(False)
        Me.tcInfo.ResumeLayout(False)
        Me.tcInfo.PerformLayout()
        Me.gpLogging.ResumeLayout(False)
        Me.gpLogging.PerformLayout()
        Me.panLoggingOptions.ResumeLayout(False)
        Me.panLoggingOptions.PerformLayout()
        CType(Me.spinLoggingRetryPeriod, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.spinLoggingAttempts, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

    ' Id of the process.
    Private mProcessID As Guid

    Private mName As String

    Public Sub New(ByVal gProcessID As Guid)
        'This call is required by the Windows Form Designer.
        InitializeComponent()

        mProcessStage = Nothing
        mProcessID = gProcessID

        Me.LogInhibitVisible = False
    End Sub

    Public Sub New()
        'design mode so we don't care what ID we give
        Me.New(Guid.Empty)
    End Sub

    ''' <summary>
    ''' Private member to store public property ProcessStage.
    ''' Replaces inherited member mobjProcessStage and is
    ''' strongly typed version copy of reference.
    ''' </summary>
    ''' <remarks></remarks>
    Protected mProcessInfoStage As Stages.clsProcessInfoStage

    ''' <summary>
    ''' Override of base class implementation so that we can collect
    '''  a strongly typed copy of stage reference.
    ''' </summary>
    Public Overrides Property ProcessStage() As BluePrism.AutomateProcessCore.clsProcessStage
        Get
            Return MyBase.ProcessStage
        End Get
        Set(ByVal value As BluePrism.AutomateProcessCore.clsProcessStage)
            Me.mProcessInfoStage = CType(value, Stages.clsProcessInfoStage)
            MyBase.ProcessStage = value
        End Set
    End Property


    Protected Overrides Sub PopulateStageData()
        MyBase.PopulateStageData()

        'base class puts wrong name and description so overwrite
        With mProcessStage.Process

            Me.txtName.Text = .Name
            Me.txtDescription.Text = .Description

            'now set other data
            txtCreatedBy.Text = .CreatedBy
            txtCreateDate.Text = CStr(.CreatedDate)
            txtModifiedBy.Text = .ModifiedBy
            txtModifiedDate.Text = CStr(.ModifiedDate)
            mPrePostConds.PreConditions = .Preconditions
            mPrePostConds.PostConditions = .Endpoint.Split(
             New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)

            cbLoggingAbortOnError.Checked = .AbortOnLogError
            spinLoggingAttempts.Value = .LoggingAttempts
            spinLoggingRetryPeriod.Value = .LoggingRetryPeriod
            UpdateLoggingOptionsState()

            mName = Me.txtName.Text
        End With
    End Sub


    ''' <summary>
    ''' Extends the validation performed in the base class.
    ''' </summary>
    ''' <returns>Returns true if validation successful, false otherwise.</returns>
    Protected Overrides Function ApplyChanges() As Boolean

        'nothing of use to us in the base class implementation
        'so no need at this time to call:
        'MyBase.ApplyChanges

        'update database with live attribute flag
        Dim OriginalLiveStatus As Boolean = (Me.mInitialProcessAttributes And ProcessAttributes.Published) > 0
        If OriginalLiveStatus <> Me.chbxPublish.Checked Then
            'update process object with attributes
            Me.mProcessStage.Process.Attributes = Me.mProcessStage.Process.Attributes Xor ProcessAttributes.Published
        End If

        If txtName.Text.Trim = "" Then
            UserMessage.Show(My.Resources.frmStagePropertiesProcessInfo_YourChosenNameIsABlankOnePleaseChooseAnother)
            Return False
        End If

        If MyBase.txtName.Text = mName Then
            SetData()
            Return True
        End If

        Dim ConflictingProcessExists As Boolean = Not gSv.IsProcessNameUnique(mProcessID, txtName.Text, False)
        Dim ConflictingObjectExists As Boolean = Not gSv.IsProcessNameUnique(mProcessID, txtName.Text, True)
        If Not (ConflictingProcessExists OrElse ConflictingObjectExists) Then
            SetData()
            Return True
        Else
            Dim Thing As String = Nothing
            If ConflictingProcessExists Then UserMessage.Show(My.Resources.frmStagePropertiesProcessInfo_TheNameYouHaveChosenIsAlreadyInUseByAnExistingProcessPleaseChooseAnotherName)
            If ConflictingObjectExists Then UserMessage.Show(My.Resources.frmStagePropertiesProcessInfo_TheNameYouHaveChosenIsAlreadyInUseByAnExistingBusinessObjectPleaseChooseAnother)
            Return False
        End If

        Return True
    End Function


    Private Sub SetData()
        With mProcessStage.Process
            .Name = txtName.Text
            .Description = txtDescription.Text
            .Preconditions = mPrePostConds.PreConditions
            .Endpoint = CollectionUtil.Join(mPrePostConds.PostConditions, vbCrLf)
            .AbortOnLogError = cbLoggingAbortOnError.Checked
            .LoggingAttempts = CInt(spinLoggingAttempts.Value)
            .LoggingRetryPeriod = CInt(spinLoggingRetryPeriod.Value)
        End With
    End Sub


    ''' <summary>
    ''' Adds and selects a condition statement. If the condition does not already
    ''' exists, it will be added. On exit from this method, the condition is
    ''' selected, whether it existed before or not.
    ''' </summary>
    ''' <param name="cond">The condition to add/select</param>
    Public Sub AddCondition(ByVal cond As String)
        With mPrePostConds.lstPreconditions
            If .Items.Contains(cond) Then
                .SelectedIndex = .Items.IndexOf(cond)
            Else
                .Items.Add(cond)
                .SelectedIndex = .Items.Count - 1
            End If
        End With
    End Sub

    ''' <summary>
    ''' Updates the enabled state of the logging options dependent on the current
    ''' checked state of the 'abort on error' checkbox
    ''' </summary>
    Private Sub UpdateLoggingOptionsState()
        panLoggingOptions.Enabled = cbLoggingAbortOnError.Checked
    End Sub

    ''' <summary>
    ''' Gets the name of the associated help file.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmStagePropertiesProcessInfo.htm"
    End Function

    ''' <summary>
    ''' Opens the help file whether online or offline.
    ''' </summary>
    Public Overrides Sub OpenHelp()
        Try
            OpenHelpFile(Me, GetHelpFile())
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

    ''' <summary>
    ''' The process attributes before the user started editing.
    ''' </summary>
    Private mInitialProcessAttributes As ProcessAttributes

    Private Sub frmStagePropertiesProcessInfo_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
        If Not DesignMode Then
            Me.mInitialProcessAttributes = Me.mProcessStage.Process.Attributes
            Me.chbxPublish.Checked = (mInitialProcessAttributes And ProcessAttributes.Published) > 0
        End If
    End Sub



    ''' <summary>
    ''' Handles the 'abort on error' checkbox changing its value
    ''' </summary>
    Private Sub HandleLoggingAbortOnErrorChanged(
     ByVal sender As Object, ByVal e As EventArgs) _
     Handles cbLoggingAbortOnError.CheckedChanged
        UpdateLoggingOptionsState()
    End Sub
End Class
