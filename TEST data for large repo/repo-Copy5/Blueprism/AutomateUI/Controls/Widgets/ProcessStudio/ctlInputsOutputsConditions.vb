
Imports BluePrism.AutomateProcessCore

''' Project  : Automate
''' Class    : ctlInputsOutputsConditions
''' 
''' <summary>
''' A class that displays tabs and listview controls for Inputs Outputs and
''' Conditions, used by all the properties forms.
''' </summary>
Friend Class ctlInputsOutputsConditions
    Inherits System.Windows.Forms.UserControl

#Region " Published Events "

    ''' <summary>
    ''' Event fired when a tab is selected in this control.
    ''' </summary>
    ''' <param name="sender">The sender of this event, ie. this control.</param>
    ''' <param name="e">The event args indicating the selected tab.</param>
    Public Event TabSelected(ByVal sender As Object, ByVal e As TabControlEventArgs)

    ''' <summary>
    ''' Event fired when a user requests a check of the code. Only makes sense if
    ''' <see cref="CodeVisible"/> is set to true.
    ''' </summary>
    Public Event CheckCode(ByVal sender As Object, ByVal e As EventArgs)

    ''' <summary>
    ''' Raises the <see cref="TabSelected"/> event.
    ''' </summary>
    ''' <param name="e">The event args indicating the selected tab.</param>
    Protected Overridable Sub OnTabSelected(ByVal e As TabControlEventArgs)
        RaiseEvent TabSelected(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="CheckCode"/> event
    ''' </summary>
    ''' <param name="e">The event args detailing the event</param>
    Protected Overridable Sub OnCheckCode(ByVal e As EventArgs)
        RaiseEvent CheckCode(Me, e)
    End Sub

#End Region

#Region " Member Variables "

    ''' <summary>
    ''' holds the stage that is having its imputs outputs or conditions modified.
    ''' </summary>
    Private mStage As clsProcessStage



#End Region

#Region " Constructors "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        Me.ctlPrecondsEndConds.[ReadOnly] = True

        CodeVisible = False

        tpInputs.Tag = CodeStageTabs.TabTypes.Inputs
        tpOutputs.Tag = CodeStageTabs.TabTypes.Outputs
        tpConditions.Tag = CodeStageTabs.TabTypes.Conditions
        tpCode.Tag = CodeStageTabs.TabTypes.Code
    End Sub

#End Region

#Region " Windows Form Designer generated code "

    'UserControl overrides dispose to clean up the component list.
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
    Friend WithEvents tcTabs As System.Windows.Forms.TabControl
    Private WithEvents tpInputs As System.Windows.Forms.TabPage
    Private WithEvents tpOutputs As System.Windows.Forms.TabPage
    Private WithEvents tpConditions As System.Windows.Forms.TabPage
    Friend WithEvents ctlPrecondsEndConds As ctlPreconditionsEndconditions
    Friend WithEvents OutputsList As AutomateUI.ctlParameterList
    Friend WithEvents pnlTitle2 As clsPanel
    Friend WithEvents InputsList As AutomateUI.ctlParameterList
    Private WithEvents tpCode As System.Windows.Forms.TabPage
    Private WithEvents btnCheckCode As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents mCodeEditor As AutomateUI.ctlCodeEditor
    Friend WithEvents Panel1 As clsPanel

    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlInputsOutputsConditions))
        Me.tcTabs = New System.Windows.Forms.TabControl()
        Me.tpInputs = New System.Windows.Forms.TabPage()
        Me.Panel1 = New AutomateUI.clsPanel()
        Me.InputsList = New AutomateUI.ctlParameterList()
        Me.tpOutputs = New System.Windows.Forms.TabPage()
        Me.pnlTitle2 = New AutomateUI.clsPanel()
        Me.OutputsList = New AutomateUI.ctlParameterList()
        Me.tpConditions = New System.Windows.Forms.TabPage()
        Me.ctlPrecondsEndConds = New AutomateUI.ctlPreconditionsEndconditions()
        Me.tpCode = New System.Windows.Forms.TabPage()
        Me.mCodeEditor = New AutomateUI.ctlCodeEditor()
        Me.btnCheckCode = New AutomateControls.Buttons.StandardStyledButton()
        Me.tcTabs.SuspendLayout()
        Me.tpInputs.SuspendLayout()
        Me.Panel1.SuspendLayout()
        Me.tpOutputs.SuspendLayout()
        Me.pnlTitle2.SuspendLayout()
        Me.tpConditions.SuspendLayout()
        Me.tpCode.SuspendLayout()
        Me.SuspendLayout()
        '
        'tcTabs
        '
        resources.ApplyResources(Me.tcTabs, "tcTabs")
        Me.tcTabs.Controls.Add(Me.tpInputs)
        Me.tcTabs.Controls.Add(Me.tpOutputs)
        Me.tcTabs.Controls.Add(Me.tpConditions)
        Me.tcTabs.Controls.Add(Me.tpCode)
        Me.tcTabs.Name = "tcTabs"
        Me.tcTabs.SelectedIndex = 0
        '
        'tpInputs
        '
        Me.tpInputs.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.tpInputs.Controls.Add(Me.Panel1)
        resources.ApplyResources(Me.tpInputs, "tpInputs")
        Me.tpInputs.Name = "tpInputs"
        '
        'Panel1
        '
        Me.Panel1.BorderColor = System.Drawing.Color.LightSteelBlue
        Me.Panel1.BorderStyle = AutomateUI.clsPanel.BorderMode.[On]
        Me.Panel1.BorderWidth = 1
        Me.Panel1.Controls.Add(Me.InputsList)
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.Name = "Panel1"
        '
        'InputsList
        '
        resources.ApplyResources(Me.InputsList, "InputsList")
        Me.InputsList.FullyEditable = True
        Me.InputsList.MapTypeToApply = BluePrism.AutomateProcessCore.MapType.None
        Me.InputsList.Name = "InputsList"
        Me.InputsList.ParameterDirection = BluePrism.AutomateProcessCore.ParamDirection.None
        Me.InputsList.Readonly = False
        Me.InputsList.Stage = Nothing
        Me.InputsList.SuppressedDataTypes = BluePrism.AutomateProcessCore.DataType.unknown
        Me.InputsList.Treeview = Nothing
        '
        'tpOutputs
        '
        Me.tpOutputs.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.tpOutputs.Controls.Add(Me.pnlTitle2)
        resources.ApplyResources(Me.tpOutputs, "tpOutputs")
        Me.tpOutputs.Name = "tpOutputs"
        '
        'pnlTitle2
        '
        Me.pnlTitle2.BorderColor = System.Drawing.Color.LightSteelBlue
        Me.pnlTitle2.BorderStyle = AutomateUI.clsPanel.BorderMode.[On]
        Me.pnlTitle2.BorderWidth = 1
        Me.pnlTitle2.Controls.Add(Me.OutputsList)
        resources.ApplyResources(Me.pnlTitle2, "pnlTitle2")
        Me.pnlTitle2.Name = "pnlTitle2"
        '
        'OutputsList
        '
        resources.ApplyResources(Me.OutputsList, "OutputsList")
        Me.OutputsList.FullyEditable = True
        Me.OutputsList.MapTypeToApply = BluePrism.AutomateProcessCore.MapType.None
        Me.OutputsList.Name = "OutputsList"
        Me.OutputsList.ParameterDirection = BluePrism.AutomateProcessCore.ParamDirection.None
        Me.OutputsList.Readonly = False
        Me.OutputsList.Stage = Nothing
        Me.OutputsList.SuppressedDataTypes = BluePrism.AutomateProcessCore.DataType.unknown
        Me.OutputsList.Treeview = Nothing
        '
        'tpConditions
        '
        Me.tpConditions.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.tpConditions.Controls.Add(Me.ctlPrecondsEndConds)
        resources.ApplyResources(Me.tpConditions, "tpConditions")
        Me.tpConditions.Name = "tpConditions"
        '
        'ctlPrecondsEndConds
        '
        resources.ApplyResources(Me.ctlPrecondsEndConds, "ctlPrecondsEndConds")
        Me.ctlPrecondsEndConds.Name = "ctlPrecondsEndConds"
        Me.ctlPrecondsEndConds.PostConditions = CType(resources.GetObject("ctlPrecondsEndConds.PostConditions"), System.Collections.Generic.ICollection(Of String))
        Me.ctlPrecondsEndConds.PreConditions = CType(resources.GetObject("ctlPrecondsEndConds.PreConditions"), System.Collections.Generic.ICollection(Of String))
        Me.ctlPrecondsEndConds.ReadOnly = False
        '
        'tpCode
        '
        Me.tpCode.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.tpCode.Controls.Add(Me.mCodeEditor)
        Me.tpCode.Controls.Add(Me.btnCheckCode)
        resources.ApplyResources(Me.tpCode, "tpCode")
        Me.tpCode.Name = "tpCode"
        Me.tpCode.UseVisualStyleBackColor = True
        '
        'mCodeEditor
        '
        resources.ApplyResources(Me.mCodeEditor, "mCodeEditor")
        Me.mCodeEditor.Code = ""
        Me.mCodeEditor.Name = "mCodeEditor"
        Me.mCodeEditor.ReadOnly = False
        '
        'btnCheckCode
        '
        resources.ApplyResources(Me.btnCheckCode, "btnCheckCode")
        Me.btnCheckCode.Name = "btnCheckCode"
        Me.btnCheckCode.UseVisualStyleBackColor = True
        '
        'ctlInputsOutputsConditions
        '
        Me.Controls.Add(Me.tcTabs)
        Me.Name = "ctlInputsOutputsConditions"
        resources.ApplyResources(Me, "$this")
        Me.tcTabs.ResumeLayout(False)
        Me.tpInputs.ResumeLayout(False)
        Me.Panel1.ResumeLayout(False)
        Me.tpOutputs.ResumeLayout(False)
        Me.pnlTitle2.ResumeLayout(False)
        Me.tpConditions.ResumeLayout(False)
        Me.tpCode.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
#End Region

#Region " Properties "

    Private Sub SetTabVisible(ByVal tp As TabPage, ByVal visible As Boolean)
        If visible _
         Then tcTabs.TabPages.Add(tp) _
         Else tcTabs.TabPages.Remove(tp)
    End Sub

    ''' <summary>
    ''' Shows or hides the inputs tab in this control. By default, it is shown
    ''' </summary>
    <Browsable(True), Category("Visible Tabs"), DefaultValue(True)> _
    Public Property InputsVisible() As Boolean
        Get
            Return tcTabs.TabPages.Contains(tpInputs)
        End Get
        Set(ByVal value As Boolean)
            SetTabVisible(tpInputs, value)
        End Set
    End Property

    ''' <summary>
    ''' Shows or hides the outputs tab in this control. By default, it is shown
    ''' </summary>
    <Browsable(True), Category("Visible Tabs"), DefaultValue(True)> _
    Public Property OutputsVisible() As Boolean
        Get
            Return tcTabs.TabPages.Contains(tpOutputs)
        End Get
        Set(ByVal value As Boolean)
            SetTabVisible(tpOutputs, value)
        End Set
    End Property

    ''' <summary>
    ''' Shows or hides the conditions tab in this control. By default, it is shown
    ''' </summary>
    <Browsable(True), Category("Visible Tabs"), DefaultValue(True)> _
    Public Property ConditionsVisible() As Boolean
        Get
            Return tcTabs.TabPages.Contains(tpConditions)
        End Get
        Set(ByVal value As Boolean)
            SetTabVisible(tpConditions, value)
        End Set
    End Property

    ''' <summary>
    ''' Shows or hides the code tab in this control. By default, it is not shown
    ''' </summary>
    <Browsable(True), Category("Visible Tabs"), DefaultValue(False)> _
    Public Property CodeVisible() As Boolean
        Get
            Return tcTabs.TabPages.Contains(tpCode)
        End Get
        Set(ByVal value As Boolean)
            SetTabVisible(tpCode, value)
        End Set
    End Property

    ''' <summary>
    ''' Gets a reference to the code editor in this control
    ''' </summary>
    <Browsable(False)> _
    Friend ReadOnly Property CodeEditor() As ctlCodeEditor
        Get
            Return mCodeEditor
        End Get
    End Property

    ''' <summary>
    ''' A mode setting that makes the control non-editable
    ''' </summary>
    <Browsable(True), Category("Behavior"), DefaultValue(False)> _
    Public Property [ReadOnly]() As Boolean
        Get
            Return (InputsList.Readonly AndAlso OutputsList.Readonly)
        End Get
        Set(ByVal value As Boolean)
            InputsList.Readonly = value
            OutputsList.Readonly = value
        End Set
    End Property

    ''' <summary>
    ''' Associates a data item treeview with this control
    ''' </summary>
    <Browsable(False)> _
    Public WriteOnly Property Treeview() As ctlDataItemTreeView
        Set(ByVal value As ctlDataItemTreeView)
            InputsList.Treeview = value
            OutputsList.Treeview = value
        End Set
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Handles a tab being selected in this control
    ''' </summary>
    Private Sub HandleTabSelected( _
     ByVal sender As Object, ByVal e As TabControlEventArgs) Handles tcTabs.Selected
        OnTabSelected(e)
    End Sub

    ''' <summary>
    ''' Handles a code check request from the user
    ''' </summary>
    Private Sub HandleCodeCheck(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnCheckCode.Click
        OnCheckCode(e)
    End Sub

    ''' <summary>
    ''' Gives the control a reference to business objects and a stage object.
    ''' </summary>
    ''' <param name="objStage">The paramenter object</param>
    ''' <param name="objBusinessObjects">The paramenter object</param>
    Public Sub SetStage(ByVal objStage As clsProcessStage, ByVal objProcessViewer As ctlProcessViewer, ByVal objBusinessObjects As clsGroupBusinessObject)
        mStage = objStage
        Me.InputsList.Stage = objStage
        Me.InputsList.ProcessViewer = objProcessViewer
        Me.OutputsList.Stage = objStage
        Me.OutputsList.ProcessViewer = objProcessViewer

        Me.InputsList.ParameterDirection = ParamDirection.In
        Me.OutputsList.ParameterDirection = ParamDirection.Out

        Select Case mStage.StageType
            Case StageTypes.Start
                Me.InputsList.MapTypeToApply = MapType.Stage
                Me.HideConditionsTab()
                Me.HideOutputsTab()
                Me.InputsList.ShowDescriptionColumn()
            Case StageTypes.End
                Me.HideInputsTab()
                Me.HideConditionsTab()
                Me.OutputsList.MapTypeToApply = MapType.Stage
                Me.OutputsList.ShowDescriptionColumn()
            Case StageTypes.Process, StageTypes.Action, StageTypes.Skill, StageTypes.SubSheet
                InputsList.FullyEditable = False
                OutputsList.FullyEditable = False
                Me.InputsList.MapTypeToApply = MapType.Expr
                Me.OutputsList.MapTypeToApply = MapType.Stage
            Case StageTypes.Code
                Me.HideConditionsTab()
                Me.InputsList.MapTypeToApply = MapType.Expr
                Me.OutputsList.MapTypeToApply = MapType.Stage
        End Select
    End Sub

    ''' <summary>
    ''' Hides the conditions tab
    ''' </summary>
    Public Sub HideConditionsTab()
        Me.tcTabs.TabPages.Remove(Me.tpConditions)
    End Sub

    ''' <summary>
    ''' Hides the inputs tab
    ''' </summary>
    Public Sub HideInputsTab()
        Me.tcTabs.TabPages.Remove(Me.tpInputs)
    End Sub

    ''' <summary>
    ''' Hides the outputs tab
    ''' </summary>
    Public Sub HideOutputsTab()
        Me.tcTabs.TabPages.Remove(Me.tpOutputs)
    End Sub

    ''' <summary>
    '''  Refreshes the inputs/outputs using the parameters from the given stage.
    ''' </summary>
    ''' <param name="stg"></param>
    Public Sub RefreshControls(ByVal stg As clsProcessStage)
        RefreshControls(stg.GetInputs(), stg.GetOutputs())
    End Sub

    ''' <summary>
    ''' Refreshes the inputs outputs list, using the list
    ''' of parameters supplied.
    ''' </summary>
    ''' <param name="Inputs">The inputs to display.</param>
    ''' <param name="Outputs">The outputs to display.</param>
    Public Sub RefreshControls(ByVal Inputs As List(Of clsProcessParameter), ByVal Outputs As List(Of clsProcessParameter))
        Me.InputsList.Populate(Inputs)
        Me.OutputsList.Populate(Outputs)
    End Sub

    ''' <summary>
    ''' Populates the pre and post condidtions.
    ''' </summary>
    ''' <param name="preConds">A collection of precondition strings</param>
    ''' <param name="postConds">A collection of postcondition strings</param>
    Public Sub UpdatePreconditionsAndPostconditions( _
     ByVal preConds As ICollection(Of String), ByVal postConds As ICollection(Of String))
        ctlPrecondsEndConds.PreConditions = preConds
        ctlPrecondsEndConds.PostConditions = postConds
    End Sub

    ''' <summary>
    ''' List of clsprocessinputparameters as represented in the inputs list.
    ''' </summary>
    ''' <param name="bRemoveUnNamed">A flag to remove any params with a blank name</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetInputParameters(Optional ByVal bRemoveUnNamed As Boolean = True) As List(Of clsProcessParameter)
        Return Me.InputsList.GetParameters(bRemoveUnNamed)
    End Function

    ''' <summary>
    ''' List of clsprocessinputparameters as represented in the outputs list.
    ''' </summary>
    ''' <param name="bRemoveUnNamed">A flag to remove any params with a blank name</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetOutputParameters(Optional ByVal bRemoveUnNamed As Boolean = True) As List(Of clsProcessParameter)
        Return Me.OutputsList.GetParameters(bRemoveUnNamed)
    End Function

#End Region

End Class
