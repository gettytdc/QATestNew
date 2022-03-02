Imports BluePrism.AutomateProcessCore
Imports AutomateControls
Imports BluePrism.AutomateAppCore.Utility

''' Project  : Automate
''' Class    : frmBreakpointProperties
''' 
''' <summary>
''' Properties form of a stage's breakpoint. Allows condition to be set etc.
''' </summary>
Friend Class frmBreakpointProperties
    Inherits Forms.HelpButtonForm
    Implements IChild, IEnvironmentColourManager


#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        Me.objBluebar.Title = My.Resources.frmBreakpointProperties_SetTheConditionsUnderWhichThisBreakpointWillHaltDebugging
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

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents chkChangeValue As System.Windows.Forms.CheckBox
    Friend WithEvents chkAccessed As System.Windows.Forms.CheckBox
    Friend WithEvents chkChangedWithCondition As System.Windows.Forms.CheckBox
    Friend WithEvents lblPauseWhen As System.Windows.Forms.Label
    Friend WithEvents lblConditionset As System.Windows.Forms.LinkLabel
    Friend WithEvents rtbExpression As ctlExpressionRichTextBox
    Friend WithEvents objBluebar As AutomateControls.TitleBar
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnOK As AutomateControls.Buttons.StandardStyledButton
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmBreakpointProperties))
        Me.chkChangeValue = New System.Windows.Forms.CheckBox()
        Me.chkAccessed = New System.Windows.Forms.CheckBox()
        Me.lblPauseWhen = New System.Windows.Forms.Label()
        Me.chkChangedWithCondition = New System.Windows.Forms.CheckBox()
        Me.lblConditionset = New System.Windows.Forms.LinkLabel()
        Me.rtbExpression = New AutomateUI.ctlExpressionRichTextBox()
        Me.objBluebar = New AutomateControls.TitleBar()
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnOK = New AutomateControls.Buttons.StandardStyledButton()
        Me.SuspendLayout()
        '
        'chkChangeValue
        '
        resources.ApplyResources(Me.chkChangeValue, "chkChangeValue")
        Me.chkChangeValue.Name = "chkChangeValue"
        '
        'chkAccessed
        '
        resources.ApplyResources(Me.chkAccessed, "chkAccessed")
        Me.chkAccessed.Name = "chkAccessed"
        '
        'lblPauseWhen
        '
        resources.ApplyResources(Me.lblPauseWhen, "lblPauseWhen")
        Me.lblPauseWhen.Name = "lblPauseWhen"
        '
        'chkChangedWithCondition
        '
        resources.ApplyResources(Me.chkChangedWithCondition, "chkChangedWithCondition")
        Me.chkChangedWithCondition.Name = "chkChangedWithCondition"
        '
        'lblConditionset
        '
        resources.ApplyResources(Me.lblConditionset, "lblConditionset")
        Me.lblConditionset.Name = "lblConditionset"
        Me.lblConditionset.TabStop = True
        '
        'rtbExpression
        '
        Me.rtbExpression.AllowDrop = True
        resources.ApplyResources(Me.rtbExpression, "rtbExpression")
        Me.rtbExpression.DetectUrls = False
        Me.rtbExpression.HideSelection = False
        Me.rtbExpression.HighlightingEnabled = True
        Me.rtbExpression.Name = "rtbExpression"
        Me.rtbExpression.PasswordChar = ChrW(0)
        Me.rtbExpression.ReadOnly = True
        '
        'objBluebar
        '
        resources.ApplyResources(Me.objBluebar, "objBluebar")
        Me.objBluebar.Name = "objBluebar"
        Me.objBluebar.Title = "Set the conditions under which this breakpoint will halt debugging"
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancel.Name = "btnCancel"
        '
        'btnOK
        '
        resources.ApplyResources(Me.btnOK, "btnOK")
        Me.btnOK.Name = "btnOK"
        '
        'frmBreakpointProperties
        '
        Me.AcceptButton = Me.btnOK
        resources.ApplyResources(Me, "$this")
        Me.CancelButton = Me.btnCancel
        Me.Controls.Add(Me.objBluebar)
        Me.Controls.Add(Me.rtbExpression)
        Me.Controls.Add(Me.lblConditionset)
        Me.Controls.Add(Me.chkChangedWithCondition)
        Me.Controls.Add(Me.lblPauseWhen)
        Me.Controls.Add(Me.chkAccessed)
        Me.Controls.Add(Me.chkChangeValue)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.btnCancel)
        Me.HelpButton = True
        Me.Name = "frmBreakpointProperties"
        Me.ResumeLayout(False)

    End Sub

#End Region

    ''' <summary>
    ''' Working throw-away Breakpoint object only commited to stage if "ok"
    ''' is clicked.
    ''' </summary>
    Private mobjProcessBreakpoint As clsProcessBreakpoint

    ''' <summary>
    ''' The stage to which this breakpoint relates
    ''' </summary>
    Private mobjProcessStage As clsProcessStage

    ''' <summary>
    ''' The expression choosing form that will pop up when the user 
    ''' clicks the set expression link.
    ''' </summary>
    Private WithEvents mExpressionChooserForm As frmExpressionChooser

    ''' <summary>
    ''' Sets the stage to which this form relates. This method should be called 
    ''' immediately after the constructor in order that the form's controls
    ''' can be populated.
    ''' </summary>
    ''' <param name="objStage">The stage.</param>
    Public Sub SetStage(ByVal objStage As clsProcessStage)
        Debug.Assert(Not objStage Is Nothing)

        Me.mobjProcessStage = objStage

        If Me.mobjProcessStage.BreakPoint Is Nothing Then
            Me.mobjProcessBreakpoint = New clsProcessBreakpoint(Me.mobjProcessStage)
        Else
            Me.mobjProcessBreakpoint = CType(Me.mobjProcessStage.BreakPoint.Clone, clsProcessBreakpoint)
        End If

        Me.PopulateControls()
    End Sub

    ''' <summary>
    ''' A process viewer used to launch stage properties.
    ''' </summary>
    ''' <remarks>May be null, but if null then no stage properties can be viewed.</remarks>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Friend Property ProcessViewer() As ctlProcessViewer
        Get
            Return mProcessViewer
        End Get
        Set(ByVal value As ctlProcessViewer)
            mProcessViewer = value
        End Set
    End Property
    Private mProcessViewer As ctlProcessViewer

    ''' <summary>
    ''' Populates the controls on the form with the information contained in
    ''' the breakpoint object. This object must not be null when this method
    ''' is called.
    ''' </summary>
    Private Sub PopulateControls()
        Debug.Assert(Not Me.mobjProcessBreakpoint Is Nothing)

        Me.chkChangedWithCondition.Checked = (Me.mobjProcessBreakpoint.BreakPointType And clsProcessBreakpoint.BreakEvents.WhenConditionMet) > 0
        Me.chkChangeValue.Checked = Me.chkChangedWithCondition.Checked OrElse (Me.mobjProcessBreakpoint.BreakPointType And clsProcessBreakpoint.BreakEvents.WhenDataValueChanged) > 0
        Me.chkAccessed.Checked = (Me.mobjProcessBreakpoint.BreakPointType And clsProcessBreakpoint.BreakEvents.WhenDataValueRead) > 0
        Me.rtbExpression.Text = clsExpression.NormalToLocal(Me.mobjProcessBreakpoint.Condition)
        Me.rtbExpression.ColourText()

        Me.chkChangedWithCondition.Enabled = Me.chkChangeValue.Checked
        Me.lblConditionset.Enabled = Me.chkChangedWithCondition.Enabled
        Me.rtbExpression.Enabled = Me.chkChangedWithCondition.Enabled
    End Sub


    Private Sub chkChangedWithCondition_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkChangedWithCondition.CheckedChanged
        Me.lblConditionset.Enabled = Me.chkChangedWithCondition.Checked
        Me.rtbExpression.Enabled = Me.lblConditionset.Enabled
    End Sub

    Private Sub lblConditionset_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles lblConditionset.LinkClicked
        Me.mExpressionChooserForm = New frmExpressionChooser
        Me.mExpressionChooserForm.SetEnvironmentColours(Me)
        Me.mExpressionChooserForm.ShowInTaskbar = False
        Me.mExpressionChooserForm.Stage = Me.mobjProcessStage
        Me.mExpressionChooserForm.Expression = clsExpression.NormalToLocal(Me.mobjProcessBreakpoint.Condition)

        Me.mExpressionChooserForm.mExpressionBuilder.Validator = AddressOf Me.mExpressionChooserForm.mExpressionBuilder.IsValidDecision
        Me.mExpressionChooserForm.mExpressionBuilder.Tester = AddressOf Me.mExpressionChooserForm.mExpressionBuilder.TestDecision
        Me.mExpressionChooserForm.mExpressionBuilder.StoreInVisible = False
        Me.mExpressionChooserForm.mExpressionBuilder.ProcessViewer = Me.ProcessViewer

        Me.mExpressionChooserForm.ShowDialog()
        If Not Me.mExpressionChooserForm.DialogResult = System.Windows.Forms.DialogResult.Cancel Then
            Me.mobjProcessBreakpoint.Condition = Me.mExpressionChooserForm.Expression
            Me.rtbExpression.Text = clsExpression.NormalToLocal(Me.mobjProcessBreakpoint.Condition)
        End If
        Me.mExpressionChooserForm.Dispose()
    End Sub

    Private Sub chkChangeValue_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkChangeValue.CheckedChanged
        If Not Me.chkChangeValue.Checked Then
            Me.chkChangedWithCondition.Checked = False
            Me.lblConditionset.Enabled = False
        End If
        Me.chkChangedWithCondition.Enabled = Me.chkChangeValue.Checked
    End Sub


    Public Overrides Function GetHelpFile() As String
        Return "frmBreakpointProperties.htm"
    End Function

    Public Overrides Sub OpenHelp()
        Try
            OpenHelpFile(Me, GetHelpFile())
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

    Private mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    Private Sub btnOk_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnOK.Click
        Debug.Assert(Not Me.mobjProcessStage Is Nothing)

        'copy config from user interface to object model
        Me.mobjProcessBreakpoint.BreakPointType = clsProcessBreakpoint.BreakEvents.None
        If Me.chkChangeValue.Checked Then
            If Me.chkChangedWithCondition.Checked Then
                Me.mobjProcessBreakpoint.BreakPointType = Me.mobjProcessBreakpoint.BreakPointType Or clsProcessBreakpoint.BreakEvents.WhenConditionMet
            Else
                Me.mobjProcessBreakpoint.BreakPointType = Me.mobjProcessBreakpoint.BreakPointType Or clsProcessBreakpoint.BreakEvents.WhenDataValueChanged
            End If
        End If
        If Me.chkAccessed.Checked Then
            Me.mobjProcessBreakpoint.BreakPointType = Me.mobjProcessBreakpoint.BreakPointType Or clsProcessBreakpoint.BreakEvents.WhenDataValueRead
        End If

        'if not testing conditions then no need to keep old conditions
        If Not (Me.mobjProcessBreakpoint.BreakPointType And clsProcessBreakpoint.BreakEvents.WhenConditionMet) > 0 Then
            Me.mobjProcessBreakpoint.Condition = ""
        End If

        'commit changes to client stage and exit
        Me.mobjProcessStage.BreakPoint = Me.mobjProcessBreakpoint

        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Public Property EnvironmentBackColor As Color Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return objBluebar.BackColor
        End Get
        Set(value As Color)
            objBluebar.BackColor = value
        End Set
    End Property

    Public Property EnvironmentForeColor As Color Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return objBluebar.TitleColor
        End Get
        Set(value As Color)
            objBluebar.TitleColor = value
        End Set
    End Property
End Class
