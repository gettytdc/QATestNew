Imports AutomateControls
Imports BluePrism.AutomateProcessCore
Imports AutomateControls.Forms

Imports BluePrism.AutomateAppCore

''' Project  : Automate
''' Class    : frmProperties
''' 
''' <summary>
''' A properties form super-class.
''' </summary>
Friend Class frmProperties
    Inherits HelpButtonForm
    Implements IEnvironmentColourManager

#Region " Windows Form Designer generated code "

    ''' <summary>
    ''' Constructor.
    ''' </summary>
    Public Sub New()
        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        Me.SetStyle(ControlStyles.ResizeRedraw, True)

        'Setup logging/warning option dropdown lists
        Dim logItems As New List(Of Mode)(
            {New Mode(My.Resources.frmProperties_Enabled, LogInfo.InhibitModes.Never),
            New Mode(My.Resources.frmProperties_Disabled, LogInfo.InhibitModes.Always),
            New Mode(My.Resources.frmProperties_ErrorsOnly, LogInfo.InhibitModes.OnSuccess)})
        cmbLogging.DisplayMember = "ItemText"
        cmbLogging.ValueMember = "ItemValue"
        cmbLogging.DataSource = logItems

        Dim warnItems As New List(Of Mode)(
            {New Mode(My.Resources.frmProperties_SystemDefault, WarningOption.SystemDefault),
            New Mode(My.Resources.frmProperties_Overridden, WarningOption.Overridden)})
        cmbWarningOption.DisplayMember = "Itemtext"
        cmbWarningOption.ValueMember = "ItemValue"
        cmbWarningOption.DataSource = warnItems

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
    Friend WithEvents btnOk As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Protected WithEvents txtName As AutomateControls.Textboxes.StyledTextBox
    Protected WithEvents txtDescription As AutomateControls.Textboxes.StyledTextBox

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Private WithEvents panBottomRow As System.Windows.Forms.Panel
    Friend WithEvents cmbLogging As System.Windows.Forms.ComboBox
    Friend WithEvents lblLogging As System.Windows.Forms.Label
    Friend WithEvents lblWarningThreshold As System.Windows.Forms.Label
    Friend WithEvents nudWarningThreshold As AutomateControls.StyledNumericUpDown
    Friend WithEvents cmbWarningOption As System.Windows.Forms.ComboBox
    Friend WithEvents lblWarningOption As System.Windows.Forms.Label
    Friend WithEvents chkLogParametersInhibit As System.Windows.Forms.CheckBox
    Friend WithEvents lblWarningInfo As System.Windows.Forms.Label
    Protected Friend WithEvents mTitleBar As AutomateControls.TitleBar
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmProperties))
        Me.btnOk = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.txtName = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtDescription = New AutomateControls.Textboxes.StyledTextBox()
        Me.mTitleBar = New AutomateControls.TitleBar()
        Me.panBottomRow = New System.Windows.Forms.Panel()
        Me.lblWarningInfo = New System.Windows.Forms.Label()
        Me.chkLogParametersInhibit = New System.Windows.Forms.CheckBox()
        Me.lblWarningThreshold = New System.Windows.Forms.Label()
        Me.nudWarningThreshold = New AutomateControls.StyledNumericUpDown()
        Me.cmbWarningOption = New System.Windows.Forms.ComboBox()
        Me.lblWarningOption = New System.Windows.Forms.Label()
        Me.cmbLogging = New System.Windows.Forms.ComboBox()
        Me.lblLogging = New System.Windows.Forms.Label()
        Me.panBottomRow.SuspendLayout
        CType(Me.nudWarningThreshold,System.ComponentModel.ISupportInitialize).BeginInit
        Me.SuspendLayout
        '
        'btnOk
        '
        resources.ApplyResources(Me.btnOk, "btnOk")
        Me.btnOk.Name = "btnOk"
        Me.btnOk.UseVisualStyleBackColor = false
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = false
        '
        'txtName
        '
        resources.ApplyResources(Me.txtName, "txtName")
        Me.txtName.BorderColor = System.Drawing.Color.Empty
        Me.txtName.Name = "txtName"
        '
        'txtDescription
        '
        Me.txtDescription.AcceptsReturn = true
        resources.ApplyResources(Me.txtDescription, "txtDescription")
        Me.txtDescription.BorderColor = System.Drawing.Color.Empty
        Me.txtDescription.Name = "txtDescription"
        '
        'mTitleBar
        '
        resources.ApplyResources(Me.mTitleBar, "mTitleBar")
        Me.mTitleBar.Name = "mTitleBar"
        Me.mTitleBar.SubtitleFont = New System.Drawing.Font("Segoe UI", 8.25!)
        Me.mTitleBar.SubtitlePosition = New System.Drawing.Point(10, 35)
        Me.mTitleBar.TabStop = false
        Me.mTitleBar.TitleFont = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
        '
        'panBottomRow
        '
        Me.panBottomRow.Controls.Add(Me.lblWarningInfo)
        Me.panBottomRow.Controls.Add(Me.chkLogParametersInhibit)
        Me.panBottomRow.Controls.Add(Me.lblWarningThreshold)
        Me.panBottomRow.Controls.Add(Me.nudWarningThreshold)
        Me.panBottomRow.Controls.Add(Me.cmbWarningOption)
        Me.panBottomRow.Controls.Add(Me.lblWarningOption)
        Me.panBottomRow.Controls.Add(Me.cmbLogging)
        Me.panBottomRow.Controls.Add(Me.lblLogging)
        Me.panBottomRow.Controls.Add(Me.btnCancel)
        Me.panBottomRow.Controls.Add(Me.btnOk)
        resources.ApplyResources(Me.panBottomRow, "panBottomRow")
        Me.panBottomRow.Name = "panBottomRow"
        '
        'lblWarningInfo
        '
        resources.ApplyResources(Me.lblWarningInfo, "lblWarningInfo")
        Me.lblWarningInfo.Name = "lblWarningInfo"
        '
        'chkLogParametersInhibit
        '
        resources.ApplyResources(Me.chkLogParametersInhibit, "chkLogParametersInhibit")
        Me.chkLogParametersInhibit.Name = "chkLogParametersInhibit"
        '
        'lblWarningThreshold
        '
        resources.ApplyResources(Me.lblWarningThreshold, "lblWarningThreshold")
        Me.lblWarningThreshold.Name = "lblWarningThreshold"
        '
        'nudWarningThreshold
        '
        resources.ApplyResources(Me.nudWarningThreshold, "nudWarningThreshold")
        Me.nudWarningThreshold.Name = "nudWarningThreshold"
        '
        'cmbWarningOption
        '
        resources.ApplyResources(Me.cmbWarningOption, "cmbWarningOption")
        Me.cmbWarningOption.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbWarningOption.FormattingEnabled = true
        Me.cmbWarningOption.Items.AddRange(New Object() {resources.GetString("cmbWarningOption.Items"), resources.GetString("cmbWarningOption.Items1")})
        Me.cmbWarningOption.Name = "cmbWarningOption"
        '
        'lblWarningOption
        '
        resources.ApplyResources(Me.lblWarningOption, "lblWarningOption")
        Me.lblWarningOption.Name = "lblWarningOption"
        '
        'cmbLogging
        '
        resources.ApplyResources(Me.cmbLogging, "cmbLogging")
        Me.cmbLogging.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbLogging.FormattingEnabled = true
        Me.cmbLogging.Name = "cmbLogging"
        '
        'lblLogging
        '
        resources.ApplyResources(Me.lblLogging, "lblLogging")
        Me.lblLogging.Name = "lblLogging"
        '
        'frmProperties
        '
        Me.AcceptButton = Me.btnOk
        Me.CancelButton = Me.btnCancel
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.panBottomRow)
        Me.Controls.Add(Me.txtDescription)
        Me.Controls.Add(Me.txtName)
        Me.Controls.Add(Me.mTitleBar)
        Me.HelpButton = true
        Me.Name = "frmProperties"
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide
        Me.panBottomRow.ResumeLayout(false)
        Me.panBottomRow.PerformLayout
        CType(Me.nudWarningThreshold,System.ComponentModel.ISupportInitialize).EndInit
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub

#End Region

#Region " Member variables "

    ''' <summary>
    ''' Private member to store public property ProcessStage()
    ''' </summary>
    Protected mProcessStage As clsProcessStage

    Protected mObjectRefs As clsGroupBusinessObject


    Protected mParentForm As Form

    Private mEditable As Boolean

#End Region

    Private Enum WarningOption
        SystemDefault
        Overridden
    End Enum

#Region "Properties"

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
            Return mTitleBar.BackColor
        End Get
        Set(value As Color)
            mTitleBar.BackColor = value
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
            Return mTitleBar.TitleColor
        End Get
        Set(value As Color)
            mTitleBar.TitleColor = value
            mTitleBar.SubtitleColor = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the visible state of the 'inhibit logging' checkbox.
    ''' </summary>
    Friend Property LogInhibitVisible() As Boolean
        Get
            Return cmbLogging.Visible
        End Get
        Set(ByVal value As Boolean)
            lblLogging.Visible = value
            cmbLogging.Visible = value
            lblWarningOption.Visible = value
            cmbWarningOption.Visible = value
            lblWarningThreshold.Visible = value
            lblWarningInfo.Visible = value
            nudWarningThreshold.Visible = value
        End Set
    End Property

    ''' <summary>
    ''' The process stage being modified in this stage properties form.
    ''' 
    ''' When using a properties form, a clone of the stage to be modified
    ''' should be made and passed in here. Afterwards this property should
    ''' be read back again.
    ''' 
    ''' Whilst at first glance this may seem unnecessary
    ''' it is indeed important, since the properties form may change the
    ''' object reference to point to another process stage object.
    ''' </summary>
    ''' <value>.</value>
    Public Overridable Property ProcessStage() As clsProcessStage
        Get
            Return mProcessStage
        End Get
        Set(ByVal value As clsProcessStage)
            mProcessStage = value
            mObjectRefs = value.Process.GetBusinessObjects()
            PopulateStageData()
        End Set
    End Property


    ''' <summary>
    ''' The process being modified in this stage properties form.
    ''' </summary>
    Protected ReadOnly Property Process() As clsProcess
        Get
            Return ProcessStage.Process
        End Get
    End Property

    ''' <summary>
    ''' Indicates if the form is editable.
    ''' </summary>
    ''' <value>True if the form is editable</value>
    Public Property IsEditable() As Boolean
        Get
            Return mEditable
        End Get
        Set(ByVal Value As Boolean)
            mEditable = Value
            If Not mEditable Then
                DisableChildControls(Me)
                btnCancel.Enabled = True
                btnCancel.Text = "Close"
                btnOk.Hide()
            End If
        End Set
    End Property

    ''' <summary>
    ''' Disables all child controls recursively in the supplied control.
    ''' Controls will either be made readonly, or 'switched off' using the
    ''' 'enabled' property (or similar for custom user controls).
    ''' </summary>
    ''' <param name="ctl">The control whose children are to be disabled.
    ''' All child controls will be operated on recursively.</param>
    Private Sub DisableChildControls(ByVal ctl As Control)
        For Each c As Control In ctl.Controls
            Debug.WriteLine(c.Name & " " & c.GetType.Name)
            Select Case True
                Case TypeOf c Is TabControl
                    DisableChildControls(c)
                Case TypeOf c Is TabPage
                    DisableChildControls(c)
                Case TypeOf c Is ctlParameterList
                    Dim i As Integer = 1
                Case TypeOf c Is TextBox
                    CType(c, TextBox).ReadOnly = True
                Case TypeOf c Is RichTextBox
                    CType(c, RichTextBox).ReadOnly = True
                Case TypeOf c Is ScintillaNET.Scintilla
                    CType(c, ScintillaNET.Scintilla).IsReadOnly = True
                Case TypeOf c Is ctlInputsOutputsConditions
                    CType(c, ctlInputsOutputsConditions).ReadOnly = True
                Case TypeOf c Is ctlPreconditionsEndconditions
                    CType(c, ctlPreconditionsEndconditions).ReadOnly = True
                Case TypeOf c Is ScrollBar
                    c.Enabled = True
                Case TypeOf c Is ctlParameterList
                    CType(c, ctlParameterList).Readonly = True
                Case TypeOf c Is ctlListView
                    CType(c, ctlListView).Readonly = True
                Case TypeOf c Is AutomateControls.MonoScrollableControl
                    c.Enabled = True
                Case TypeOf c Is AutomateUI.ctlExpressionEdit
                    c.Enabled = False
                Case Else
                    If c.Controls Is Nothing OrElse c.Controls.Count = 0 Then
                        c.Enabled = False
                    End If
                    DisableChildControls(c)
            End Select
        Next
    End Sub

    ''' <summary>
    ''' Private member to store public property ProcessViewer()
    ''' </summary>
    Private mProcessViewer As ctlProcessViewer
    ''' <summary>
    ''' The process viewer associated with this form,
    ''' if any.
    ''' </summary>
    Public Property ProcessViewer() As ctlProcessViewer
        Get
            Return mProcessViewer
        End Get
        Set(ByVal value As ctlProcessViewer)
            mProcessViewer = value
        End Set
    End Property

#End Region

#Region "SetParentForm"

    ''' <summary>
    ''' Sets the parent form.
    ''' </summary>
    ''' <param name="objParent">The parent form</param>
    Public Sub SetParentForm(ByVal objParent As Form)
        mParentForm = objParent
    End Sub

#End Region

#Region "Apply Changes"

    ''' <summary>
    ''' Applies changes to the stage object and validates the state of the current
    ''' stage. Returns false if the changes were not successfully validated.
    ''' </summary>
    ''' <returns>Returns true unless validation fails or an error occurs.</returns>
    Protected Overridable Function ApplyChanges() As Boolean
        If mProcessStage IsNot Nothing Then

            'things common to all stages:
            GetLoggingOptions()
            mProcessStage.Name = GetName()
            mProcessStage.Narrative = Me.txtDescription.Text

            'Validate the name
            If txtName.Text = "" Then
                UserMessage.Show(My.Resources.frmProperties_YouMustEnterANameForThisStage)
                Return False
            End If

            'check for duplicate names of input parameters
            Dim Params As List(Of clsProcessParameter) = mProcessStage.GetInputs
            If ListContainsDuplicateParameters(Params) Then
                UserMessage.Show(My.Resources.frmProperties_TheInputParametersForThisStageDoNotAllHaveUniqueNamesPleaseAmendTheParameterNam)
                Return False
            End If

            'And now output parameters
            Params = mProcessStage.GetOutputs
            If ListContainsDuplicateParameters(Params) Then
                UserMessage.Show(My.Resources.frmProperties_TheOutputParametersForThisStageDoNotAllHaveUniqueNamesPleaseAmendTheParameterNa)
                Return False
            End If

            Return True
        Else
            UserMessage.Show(My.Resources.frmProperties_InternalErrorBadConfiguration) '"Internal Error: bad configuration."
            Return False
        End If

        Return True
    End Function

    ''' <summary>
    ''' Gets the name field value.
    ''' </summary>
    ''' <returns>The value</returns>
    Protected Overridable Function GetName() As String
        Return Me.txtName.Text
    End Function

    ''' <summary>
    ''' Determines if the supplied list of parameters contains two (or more)
    ''' parameters with the same name.
    ''' </summary>
    ''' <param name="ParameterList">The list of parameters.</param>
    ''' <returns>True if a duplicate pair exists, False otherwise.</returns>
    Private Function ListContainsDuplicateParameters(ByVal ParameterList As List(Of clsProcessParameter)) As Boolean
        Dim namesList As New Generic.Dictionary(Of String, String)
        For Each p As clsProcessParameter In ParameterList
            Dim name As String = p.Name
            If Not namesList.ContainsKey(name) Then
                namesList.Add(name, name)
            Else
                Return True
            End If
        Next

        Return False
    End Function

#End Region

    ''' <summary>
    ''' Prepares for object studio mode, perhaps hiding and adding features
    ''' </summary>
    Public Overridable Sub PrepareForObjectStudio()
        'do nothing
    End Sub

#Region " Protected Methods "

    ''' <summary>
    ''' Performs any population of the user interface based on the stage
    ''' in the member variable mobjprocessstage. Naturally, this
    ''' object reference must not be null.
    ''' </summary>
    Protected Overridable Sub PopulateStageData()
        'Fill in all the fields...
        txtName.Text = mProcessStage.GetName()
        txtDescription.Text = mProcessStage.GetNarrative()
        SetLoggingOptions()

        If Me.ProcessViewer IsNot Nothing Then
            ' If a data item treeview is in this form, set the process viewer
            ' from this form into it
            Dim tv As ctlDataItemTreeView = FindChildControlOfType(Of ctlDataItemTreeView)()
            If tv IsNot Nothing Then tv.ProcessViewer = Me.ProcessViewer
        End If

    End Sub

    Protected Sub UpdateStageLogging()
        SetLoggingOptions()
    End Sub

    ''' <summary>
    ''' Finds the first child control of the required type
    ''' </summary>
    ''' <typeparam name="T">The type of control required.</typeparam>
    ''' <returns>The control of the required type if it exists and is a child of
    ''' this form. Null otherwise.</returns>
    Protected Function FindChildControlOfType(Of T As Control)() As T
        For Each ctl As Control In Me.Controls
            If TypeOf ctl Is T Then Return DirectCast(ctl, T)
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Function to check whether the current value should be visible / editable
    ''' for this stage.
    ''' </summary>
    ''' <returns>True if the current value is valid with the current state of
    ''' the stage's process; False if it is invalid and should not be visible or
    ''' editable.</returns>
    ''' <remarks>Obviously, this only really makes sense for properties dialogs
    ''' which are displaying some kind of data item stage (either for a data item
    ''' or a collection)</remarks>
    Protected Overridable Function ShouldShowCurrentValue() As Boolean
        Dim p As clsProcess = mProcessStage.Process
        Dim runStage As clsProcessStage = p.RunStage
        ' Current value should be shown if the process is currently running and
        ' the next stage is not a start stage (which will overwrite the current
        ' value with the initial value).
        ' A null next stage usually indicates that the debugger has gone beyond
        ' the end stage (objects only affected by this) - see bug 5199, comment 3
        Return (p.IsRunning() _
         AndAlso (runStage Is Nothing OrElse runStage.StageType <> StageTypes.Start))

    End Function

#End Region

#Region "btnCancel_Click"
    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

#End Region

    Private Sub frmProperties_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load
        If mProcessStage IsNot Nothing Then
            If lblWarningInfo.Bottom > panBottomRow.Height Then
                lblWarningInfo.Top -= CInt(Math.Round(lblWarningInfo.Top / 5))
            End If
            AddHandler cmbLogging.SelectedValueChanged, AddressOf HandleLoggingOptionsChanged
            AddHandler cmbWarningOption.SelectedValueChanged, AddressOf HandleLoggingOptionsChanged
        End If
    End Sub

    Private Sub btnOk_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnOk.Click
        If Me.ApplyChanges() Then
            Me.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.Close()
        End If
    End Sub

    ''' <summary>
    ''' Initialise the logging option for the current stage
    ''' </summary>
    Private Sub SetLoggingOptions()
        cmbLogging.SelectedValue = mProcessStage.LogInhibit
        If mProcessStage.OverrideDefaultWarningThreshold Then
            cmbWarningOption.SelectedValue = WarningOption.Overridden
            nudWarningThreshold.Value = mProcessStage.WarningThreshold
        Else
            cmbWarningOption.SelectedValue = WarningOption.SystemDefault
        End If
        loggingOptionsChanged()
    End Sub

    ''' <summary>
    ''' Set the configured stage logging options into the current stage
    ''' </summary>
    Private Sub GetLoggingOptions()
        mProcessStage.LogInhibit = CType(cmbLogging.SelectedValue, LogInfo.InhibitModes)
        Dim warning = CType(cmbWarningOption.SelectedValue, WarningOption)
        If CType(cmbWarningOption.SelectedValue, WarningOption) = WarningOption.SystemDefault Then
            mProcessStage.WarningThreshold = mProcessStage.DefaultWarningThreshold
        Else
            mProcessStage.WarningThreshold = CInt(nudWarningThreshold.Value)
        End If
    End Sub

    ''' <summary>
    ''' Update display following change to the stage logging options
    ''' </summary>
    Private Sub loggingOptionsChanged()

        SetupLogParameterCheckbox()

        If CType(cmbWarningOption.SelectedValue, WarningOption) = WarningOption.SystemDefault Then
            nudWarningThreshold.Value = CDec(gSv.GetStageWarningThreshold()) / 60
            lblWarningThreshold.Enabled = False
            lblWarningInfo.Enabled = False
            nudWarningThreshold.Enabled = False
        Else
            lblWarningThreshold.Enabled = True
            lblWarningInfo.Enabled = True
            nudWarningThreshold.Enabled = True
        End If
    End Sub

    ''' <summary>
    ''' Set the state and availability of the log parameters check box based on the 
    ''' logging level of the stage
    ''' </summary>
    ''' <param name="isEditable">Can the logging parameter check box be edited? This
    ''' will set the Enabled flag of the check box and will be ignored if the logging
    ''' level of the stage does not allow the check box to be changed</param>
    Protected Friend Sub SetupLogParameterCheckbox(Optional isEditable As Boolean? = Nothing)
        If CType(cmbLogging.SelectedValue, LogInfo.InhibitModes) = LogInfo.InhibitModes.Always Then
            chkLogParametersInhibit.Checked = False
            chkLogParametersInhibit.Enabled = False
        Else
            chkLogParametersInhibit.Enabled = CBool(IIf(isEditable.HasValue, isEditable, True))
        End If
    End Sub

    ''' <summary>
    ''' Handles the stage logging options changing (once form loaded)
    ''' </summary>
    Private Sub HandleLoggingOptionsChanged(sender As Object, e As EventArgs)
        loggingOptionsChanged()
    End Sub

#Region "Mode dropdownlist class"

    Private Class Mode

        Public Property ItemText() As String

        Public Property ItemValue() As Object

        Public Sub New(text As String, value As Object)
            ItemText = text
            ItemValue = value
        End Sub

        Public Overrides Function ToString() As String
            Return ItemText
        End Function
    End Class

#End Region

End Class

